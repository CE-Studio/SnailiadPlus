using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snaily : MonoBehaviour
{
    public const float RUNSPEED_NORMAL = 8;
    public const float JUMPPOWER_NORMAL = 24;
    public const float GRAVITY = 1.25f;
    public const float TERMINAL_VELOCITY = -0.66f;
    public const float HITBOX_X = 1.467508f;
    public const float HITBOX_Y = 0.96f;
    public const float HITBOX_SHELL_X = 0.75f;
    public const float HITBOX_SHELL_Y = 0.96f;
    public const float HITBOX_SHELL_OFFSET = 0.186518f;
    public const int DIR_FLOOR = 0;
    public const int DIR_WALL_LEFT = 1;
    public const int DIR_WALL_RIGHT = 2;
    public const int DIR_CEILING = 3;
    public const float FALLSPEED_MOD = 2.5f;
    
    public float[] WEAPON_COOLDOWNS = new float[6];

    private Vector2 velocity = new Vector2(0, 0);
    private Vector2 boxDistances = new Vector2(0, 0);
    private int gravityDir = DIR_FLOOR;
    private bool grounded = false;
    private bool shelled = false;
    private float speedMod = 1;
    private float jumpMod = 1;
    private float gravityMod = 1;
    private bool facingLeft = false;
    private bool facingUp = false;
    private int relativeDown = DIR_FLOOR;
    private int relativeLeft = DIR_WALL_LEFT;
    private int relativeRight = DIR_WALL_RIGHT;
    private int relativeUp = DIR_CEILING;
    private bool holdingJump = false;
    private bool holdingShell = false;
    private bool justSwapped = false;

    private RaycastHit2D boxL;
    private RaycastHit2D boxR;
    private RaycastHit2D boxU;
    private RaycastHit2D boxD;
    private RaycastHit2D boxCorner;
    private Vector2 lastPosition;
    private Vector2 lastSize;

    public BoxCollider2D box;
    public SpriteRenderer sprite;
    public Animator anim;
    public AudioSource sfx;

    public AudioClip shell;
    public AudioClip jump;

    public LayerMask playerCollide;

    public Player player;

    // This function is called the moment the script is loaded. I use it to initialize a lot of variables and such
    void Start()
    {
        box = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        sfx = GetComponent<AudioSource>();
        player = GetComponent<Player>();
        playerCollide = LayerMask.GetMask("PlayerCollide");

        shell = (AudioClip)Resources.Load("Sounds/Sfx/Shell");
        jump = (AudioClip)Resources.Load("Sounds/Sfx/Jump");

        // Weapon cooldowns; first three are without Rapid Fire, last three are with
        WEAPON_COOLDOWNS[0] = 0.085f;
        WEAPON_COOLDOWNS[1] = 0.3f;
        WEAPON_COOLDOWNS[2] = 0.17f;
        WEAPON_COOLDOWNS[3] = 0.0425f;
        WEAPON_COOLDOWNS[4] = 0.15f;
        WEAPON_COOLDOWNS[5] = 0.085f;
    }

    // This function is called once per frame
    void Update()
    {
        
    }

    // This function is called once every 0.02 seconds (50 time a second) regardless of framerate. Unity requires all physics calculations to be
    // run in this function, so it's where I put movement code as it utilizes boxcasts
    void FixedUpdate()
    {
        // To start things off, we mark our current position as the last position we took. Same with our hitbox size
        // Among other things, this is used to test for ground when we're airborne
        lastPosition = new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y);
        lastSize = box.size;
        // We also update all our boxcasts, both for the corner and in case they're misaligned with our current gravity state
        UpdateBoxcasts();

        // Next, we run different blocks of movement code based on our gravity state. They're largely the same, but are kept separate
        // so that things can stay different between them if needed, like Snaily falling off walls and ceilings without Gravity Snail
        switch (gravityDir)
        {
            case DIR_FLOOR:
                // This if block's purpose is so that you can click that minus button on the left and hide it from view, just so that you don't have
                // to scroll quite as much if you don't want to. Cleanup, basically
                if (true)
                {
                    // We start by zeroing our relative vertical velocity if we're grounded, and our relative horizontal velocity no matter what
                    velocity.x = 0;
                    if (grounded)
                        velocity.y = 0;

                    // From here, we perform relatively horizontal movement checks to move, stop if we hit a wall, and allow for climbing
                    if (Input.GetAxisRaw("Horizontal") != 0)
                    {
                        SwapDir((Input.GetAxisRaw("Horizontal") == 1) ? DIR_WALL_RIGHT : DIR_WALL_LEFT);
                        if (shelled && grounded)
                            ToggleShell();
                        float runSpeedValue = RUNSPEED_NORMAL * speedMod * Time.fixedDeltaTime;
                        if ((facingLeft ? boxL : boxR).distance < runSpeedValue)
                        {
                            velocity.x = facingLeft ? -runSpeedValue + (runSpeedValue - boxL.distance) : runSpeedValue - (runSpeedValue - boxR.distance);
                        }
                        else
                            velocity.x = facingLeft ? -runSpeedValue : runSpeedValue;
                        transform.position = new Vector2(transform.position.x + velocity.x, transform.position.y);
                        UpdateBoxcasts();
                    }

                    // Now, we perform relatively vertical checks. This mainly involves jumping and falling
                    if (!grounded)
                    {
                        bool pokedCeiling = false;
                        velocity.y = Mathf.Clamp(velocity.y - GRAVITY * gravityMod * Time.fixedDeltaTime * ((!holdingJump && velocity.y > 0) ? FALLSPEED_MOD : 1), TERMINAL_VELOCITY, Mathf.Infinity);
                        if (boxD.distance != 0 && boxU.distance != 0)
                        {
                            if (boxD.distance < -velocity.y && Mathf.Sign(velocity.y) == -1)
                            {
                                velocity.y = -boxD.distance;
                                grounded = true;
                            }
                            else if (boxU.distance < velocity.y && Mathf.Sign(velocity.y) == 1)
                            {
                                velocity.y = boxU.distance;
                                pokedCeiling = true;
                            }
                        }
                        transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
                        UpdateBoxcasts();
                        if (pokedCeiling)
                            velocity.y = 0;
                    }
                    else
                    {
                        if (boxD.distance > 0.0125f)
                        {
                            grounded = false;
                        }
                    }

                    // Now, let's see if we can jump
                    if (Input.GetAxisRaw("Jump") == 1 && grounded && !holdingJump && boxU.distance > 0.95f)
                    {
                        grounded = false;
                        velocity.y = JUMPPOWER_NORMAL * jumpMod * Time.deltaTime;
                        sfx.PlayOneShot(jump);
                    }
                    if (Input.GetAxisRaw("Jump") == 1 && !holdingJump)
                        holdingJump = true;
                    else if (Input.GetAxisRaw("Jump") != 1 && holdingJump)
                        holdingJump = false;
                }
                break;
        }
    }

    // This function is used to reset all five boxcasts the player character uses for ground checks. It's called once per
    // FixedUpdate() call automatically plus any additional resets needed, for instance, after a gravity change
    private void UpdateBoxcasts()
    {
        boxL = Physics2D.BoxCast(
            new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y),
            new Vector2(box.size.x, box.size.y - 0.015625f),
            0,
            Vector2.left,
            Mathf.Infinity,
            playerCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );
        boxR = Physics2D.BoxCast(
            new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y),
            new Vector2(box.size.x, box.size.y - 0.015625f),
            0,
            Vector2.right,
            Mathf.Infinity,
            playerCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );
        boxU = Physics2D.BoxCast(
            new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y),
            new Vector2(box.size.x - 0.015625f, box.size.y),
            0,
            Vector2.up,
            Mathf.Infinity,
            playerCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );
        boxD = Physics2D.BoxCast(
            new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y),
            new Vector2(box.size.x - 0.015625f, box.size.y),
            0,
            Vector2.down,
            Mathf.Infinity,
            playerCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );

        Vector2 cornerTestDir = Vector2.zero;
        if (getDirName() == "CEILING")
            cornerTestDir = Vector2.up;
        else if (getDirName() == "WALL LEFT")
            cornerTestDir = Vector2.left;
        else if (getDirName() == "WALL RIGHT")
            cornerTestDir = Vector2.right;
        else
            cornerTestDir = Vector2.down;
        boxCorner = Physics2D.BoxCast(
            lastPosition,
            lastSize,
            0,
            cornerTestDir,
            Mathf.Infinity,
            playerCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );
    }

    // This function is callede to reorient the player character in any way necessary
    // Note: this only accounts for four directions in either the ground/ceiling state or the wall state, never both. A call to
    // SwitchSurfaceAxis() is necessary for that
    private void SwapDir(int dirToFace)
    {
        switch (dirToFace)
        {
            case DIR_FLOOR:
                facingUp = false;
                sprite.flipY = false;
                break;
            case DIR_WALL_LEFT:
                facingLeft = true;
                sprite.flipX = true;
                break;
            case DIR_WALL_RIGHT:
                facingLeft = false;
                sprite.flipX = false;
                break;
            case DIR_CEILING:
                facingUp = true;
                sprite.flipY = true;
                break;
        }
    }

    // This function is used to swap the player character between the ground/ceiling state and the wall state and vice versa
    private void SwitchSurfaceAxis()
    {
        if (gravityDir == DIR_WALL_LEFT || gravityDir == DIR_WALL_RIGHT)
            PlayAnim("wall");
        else
            PlayAnim("floor");
        box.size = new Vector2(box.size.y, box.size.x);
    }

    // This function is called whenever a shelled character asks to enter/exit their shell
    private void ToggleShell()
    {
        if (shelled)
        {
            box.offset = Vector2.zero;
            if (gravityDir == DIR_WALL_LEFT || gravityDir == DIR_WALL_RIGHT)
                box.size = new Vector2(HITBOX_Y, HITBOX_X);
            else
                box.size = new Vector2(HITBOX_X, HITBOX_Y);
            PlayAnim("idle");
        }
        else
        {
            if (gravityDir == DIR_WALL_LEFT || gravityDir == DIR_WALL_RIGHT)
            {
                if (facingUp)
                    box.offset = new Vector2(0, -HITBOX_SHELL_OFFSET);
                else
                    box.offset = new Vector2(0, HITBOX_SHELL_OFFSET);
                box.size = new Vector2(HITBOX_SHELL_Y, HITBOX_SHELL_X);
            }
            else
            {
                if (facingLeft)
                    box.offset = new Vector2(HITBOX_SHELL_OFFSET, 0);
                else
                    box.offset = new Vector2(-HITBOX_SHELL_OFFSET, 0);
                box.size = new Vector2(HITBOX_SHELL_X, HITBOX_SHELL_Y);
            }
            sfx.PlayOneShot(shell);
            PlayAnim("shell");
        }
        shelled = !shelled;
    }

    // This function acts as an animation manager, converting a string into an animation name
    private void PlayAnim(string action)
    {
        string animName = "";
        animName += "Normal ";
        switch (action)
        {
            case "wall":
                animName += "wall ";
                if (shelled)
                    animName += "shell";
                else
                    animName += "idle";
                break;
            case "floor":
                animName += "floor ";
                if (shelled)
                    animName += "shell";
                else
                    animName += "idle";
                break;
            case "shell":
                if (gravityDir == DIR_WALL_LEFT || gravityDir == DIR_WALL_RIGHT)
                    animName += "wall ";
                else
                    animName += "floor ";
                animName += "shell";
                break;
            case "idle":
                if (gravityDir == DIR_WALL_LEFT || gravityDir == DIR_WALL_RIGHT)
                    animName += "wall ";
                else
                    animName += "floor ";
                animName += "idle";
                break;
            case "die":
                animName += "die";
                break;
            default:
                return;
        }
        anim.Play(animName, 0, 0);
    }

    // This function returns a string version of the current gravity direction, formatted in a different manner to the variable names
    private string getDirName()
    {
        string name = "";
        switch (gravityDir)
        {
            case 0:
                name = "FLOOR";
                break;
            case 1:
                name = "LEFT WALL";
                break;
            case 2:
                name = "RIGHT WALL";
                break;
            case 3:
                name = "CEILING";
                break;
        }
        return name;
    }
}
