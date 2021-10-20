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

    private RaycastHit2D boxHoriz;
    private RaycastHit2D boxVert;

    public BoxCollider2D box;
    public SpriteRenderer sprite;
    public Animator anim;
    public AudioSource sfx;

    public AudioClip shell;
    public AudioClip jump;

    public LayerMask playerCollide;

    public Player player;

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

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        // Firstly, let's ensure Snaily's collider's offset is oriented properly
        if (facingLeft)
            box.offset = new Vector2(Mathf.Abs(box.offset.x), box.offset.y);
        else
            box.offset = new Vector2(-Mathf.Abs(box.offset.x), box.offset.y);

        // Set up boxcasts
        boxHoriz = Physics2D.BoxCast(
            new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y),
            new Vector2(box.size.x - 0.025f, box.size.y - 0.025f),
            0,
            Vector2.right,
            boxDistances.x,
            playerCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );
        boxVert = Physics2D.BoxCast(
            new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y),
            new Vector2(box.size.x - 0.025f, box.size.y - 0.025f),
            0,
            Vector2.up,
            boxDistances.y,
            playerCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );

        // Before we move, let's just make sure all the keys are how they should be
        if (holdingJump && Input.GetAxisRaw("Jump") == 0)
            holdingJump = false;

        // Decide which direction we're currently falling
        switch (gravityDir)
        {
                                                                                 ////////////////
                                                                                 ////  DOWN  ////
                                                                                 ////////////////
            case DIR_FLOOR:
                // Firstly, we ensure we're oriented correctly
                SwapDir(DIR_FLOOR);

                // Now, we make sure our collision-finding boxcasts are facing the right way
                if (grounded)
                {
                    velocity.y = 0;
                    UpdateBoxcasts(boxDistances.x, -1);
                }
                else
                {
                    // If we happen to be falling, let's increase our downward velocity by some increment
                    // However we should slow it if jump is held as long as we're still going up
                    if (!holdingJump && velocity.y > 0)
                        velocity.y = Mathf.Clamp(velocity.y - GRAVITY * gravityMod * Time.fixedDeltaTime * FALLSPEED_MOD, TERMINAL_VELOCITY, Mathf.Infinity);
                    else
                        velocity.y = Mathf.Clamp(velocity.y - GRAVITY * gravityMod * Time.fixedDeltaTime, TERMINAL_VELOCITY, Mathf.Infinity);
                    UpdateBoxcasts(boxDistances.x, velocity.y);
                    //Debug.Log("Now it's " + velocity.y);
                }

                // Firstly, let's see if Snaily wants to toggle being in their shell
                // We'll start by un-shelling Snaily if they're shelled and decide to move, jump, or shoot
                if (shelled && ((Input.GetAxisRaw("Horizontal") != 0 && grounded) ||
                    (Input.GetAxisRaw("Jump") != 0 && grounded)))
                {
                    ToggleShell();
                }
                // Now we'll shell/unshell based on the button press
                if (Input.GetAxisRaw("Vertical") == -1 && Input.GetAxisRaw("Horizontal") == 0 && !holdingShell)
                {
                    ToggleShell();
                }
                if (!holdingShell && Input.GetAxisRaw("Vertical") == -1)
                {
                    holdingShell = true;
                }
                if (holdingShell && Input.GetAxisRaw("Vertical") != -1)
                {
                    holdingShell = false;
                }

                // Let's run left/right move checks next
                if (Input.GetAxisRaw("Horizontal") != 0)
                {
                    UpdateBoxcasts(Input.GetAxisRaw("Horizontal") * RUNSPEED_NORMAL * speedMod * Time.fixedDeltaTime, boxDistances.y);
                    // Let's make sure Snaily is facing where they're going!
                    if (Input.GetAxisRaw("Horizontal") > 0)
                        SwapDir(DIR_WALL_RIGHT);
                    else
                        SwapDir(DIR_WALL_LEFT);
                    // If the boxcast hit something...
                    if (boxHoriz.collider != null)
                    {
                        RaycastHit2D boxWall = Physics2D.Raycast(
                            new Vector2(transform.position.x, boxHoriz.point.y),
                            Vector2.right * Mathf.Sign(Input.GetAxisRaw("Horizontal")),
                            Mathf.Infinity,
                            playerCollide,
                            Mathf.Infinity,
                            Mathf.Infinity
                            );
                        // We check if the distance is 0 on the offchance the point of collision originates inside a ceiling
                        // If we didn't do this, Snaily might jitter around a bit upon hitting one
                        if (boxWall.distance != 0)
                        {
                            bool queryRight = false;
                            if (boxDistances.x > 0)
                            {
                                velocity.x = boxWall.distance - (box.size.x * 0.5f) - box.offset.x;
                                queryRight = true;
                            }
                            else
                            {
                                velocity.x = -boxWall.distance + (box.size.x * 0.5f) - box.offset.x;
                            }

                            RaycastHit2D boxClimbTest = Physics2D.Raycast(
                                    new Vector2(transform.position.x, transform.position.y),
                                    Vector2.up,
                                    Mathf.Infinity,
                                    playerCollide,
                                    Mathf.Infinity,
                                    Mathf.Infinity
                                    );
                            if ((Input.GetAxisRaw("Vertical") == 1 || (Input.GetAxisRaw("Vertical") == -1 && !grounded)) && boxClimbTest.distance > 1.5f)
                            {
                                bool queryUp = (Input.GetAxisRaw("Vertical") == 1);

                                gravityDir = queryRight ? DIR_WALL_RIGHT : DIR_WALL_LEFT;
                                SwitchSurfaceAxis();
                                SwapDir(queryRight ? DIR_WALL_RIGHT : DIR_WALL_LEFT);
                                SwapDir(queryUp ? DIR_CEILING : DIR_FLOOR);
                                PlayAnim("wall");

                                boxClimbTest = Physics2D.Raycast(
                                    new Vector2(transform.position.x, transform.position.y),
                                    Vector2.down,
                                    Mathf.Infinity,
                                    playerCollide,
                                    Mathf.Infinity,
                                    Mathf.Infinity
                                    );
                                float vertMod = 0;
                                if (boxClimbTest.distance < box.size.y * 0.5f)
                                    vertMod = boxClimbTest.distance - (box.size.y * 0.5f);
                                transform.position = new Vector2(transform.position.x, transform.position.y + vertMod);

                                velocity.x = queryRight ? boxWall.distance - (box.size.x * 0.5f) - box.offset.x : -boxWall.distance + (box.size.x * 0.5f) - box.offset.x;
                                transform.position = new Vector2(transform.position.x + velocity.x, transform.position.y);
                                velocity.x = 0;
                            }
                        }
                    }
                    // If the boxcast didn't hit anything...
                    else
                        velocity.x = boxDistances.x;
                }
                else
                    velocity.x = 0;

                // Here we check to see if Snaily's run off the edge of a platform
                if (grounded && boxVert.collider == null)
                {
                    grounded = false;
                    velocity.y = 0;
                    UpdateBoxcasts(boxDistances.x, -GRAVITY * gravityMod * Time.fixedDeltaTime);
                }
                // If we happen to be in the air...
                if (!grounded)
                {
                    // If the boxcast hit something...
                    if (boxVert.collider != null)
                    {
                        RaycastHit2D boxCeiling = Physics2D.Raycast(
                            new Vector2(boxVert.point.x, transform.position.y),
                            Vector2.up,
                            Mathf.Infinity,
                            playerCollide,
                            Mathf.Infinity,
                            Mathf.Infinity
                            );
                        // If we hit a ceiling, stop
                        // This 0.5f here is the distance to check for ceiling collision. In Snaily's case, it's half a unit
                        // If your character is more than one unit tall, don't forget to change this!
                        if (velocity.y > 0 && boxCeiling.distance < 0.5f)
                        {
                            transform.position = new Vector2(transform.position.x, transform.position.y + boxCeiling.distance - (box.size.y * 0.5f));
                            if (Input.GetAxisRaw("Vertical") == 1)
                            {
                                grounded = true;
                                if (shelled)
                                    ToggleShell();
                                gravityDir = DIR_CEILING;
                                SwapDir(DIR_CEILING);
                                return;
                            }
                            velocity.y = -0.0125f;
                            UpdateBoxcasts(boxDistances.x, velocity.y);
                        }
                        // If we hit a floor, mark ourselves as grounded and stop moving vertically at all
                        // Right after we get the exact distance from the ground and move just the right amount to align ourself with it
                        else if (velocity.y <= 0)
                        {
                            RaycastHit2D boxLand = Physics2D.Raycast(
                                new Vector2(boxVert.point.x, transform.position.y),
                                -Vector2.up,
                                Mathf.Infinity,
                                playerCollide,
                                Mathf.Infinity,
                                Mathf.Infinity
                                );
                            if (boxLand.point.y < transform.position.y)
                            {
                                velocity.y = -boxLand.distance + (box.size.y * 0.5f) + 0.03124f;
                                grounded = true;
                            }
                        }
                    }
                    // If the boxcast didn't hit anything...
                    else
                        // ...just move the max velocity we can
                        velocity.y = boxVert.distance;
                }
                // If we happen to be on the ground...
                else
                {
                    // Let's allow jumping!
                    // First, check to see if the button is even down
                    if (Input.GetAxisRaw("Jump") == 1 && !holdingJump)
                    {
                        holdingJump = true;
                        // Now, let's see if we have the space to
                        RaycastHit2D boxJump = Physics2D.BoxCast(
                            new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y),
                            new Vector2(box.size.x, box.size.y),
                            0,
                            Vector2.up,
                            Mathf.Infinity,
                            playerCollide,
                            Mathf.Infinity,
                            Mathf.Infinity
                            );

                        if (Vector2.Distance(transform.position, boxJump.point) > 0.75f)
                        {
                            // Looks like we're clear!
                            velocity.y = JUMPPOWER_NORMAL * Time.fixedDeltaTime;
                            sfx.PlayOneShot(jump);
                            grounded = false;
                        }
                    }
                }

                // Let's clamp our velocity values just to make sure we don't shoot across the map
                Vector2 finalVel = new Vector2(
                    Mathf.Clamp(velocity.x, -RUNSPEED_NORMAL * speedMod * Time.fixedDeltaTime, RUNSPEED_NORMAL * speedMod * Time.fixedDeltaTime),
                    Mathf.Clamp(velocity.y, TERMINAL_VELOCITY, Mathf.Infinity)
                    );
                transform.position = new Vector2(transform.position.x + finalVel.x, transform.position.y + finalVel.y);
                break;
                                                                                 ////////////////
                                                                                 ////  LEFT  ////
                                                                                 ////////////////
            case DIR_WALL_LEFT:
                break;
                                                                                 /////////////////
                                                                                 ////  RIGHT  ////
                                                                                 /////////////////
            case DIR_WALL_RIGHT:
                break;
                                                                                 ////////////////
                                                                                 ////   UP   ////
                                                                                 ////////////////
            case DIR_CEILING:
                break;
        }
    }

    private void UpdateBoxcasts(float x, float y)
    {
        boxDistances = new Vector2(x, y);
        boxHoriz.distance = x;
        boxVert.distance = y;
    }

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

    private void SwitchSurfaceAxis()
    {
        if (gravityDir == DIR_WALL_LEFT || gravityDir == DIR_WALL_RIGHT)
            PlayAnim("floor");
        else
            PlayAnim("idle");
        box.size = new Vector2(box.size.y, box.size.x);
    }

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
}
