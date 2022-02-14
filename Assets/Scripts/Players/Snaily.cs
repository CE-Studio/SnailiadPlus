using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snaily : MonoBehaviour
{
    public const float RUNSPEED_NORMAL = 8;
    public const float JUMPPOWER_NORMAL = 26;
    public const float GRAVITY = 1.35f;
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
    private int gravityDir = DIR_FLOOR;
    public bool grounded = false;
    public bool shelled = false;
    private float speedMod = 1;
    private float jumpMod = 1;
    private float gravityMod = 1;
    private bool facingLeft = false;
    private bool facingDown = false;
    public bool holdingJump = false;
    private bool holdingShell = false;
    private bool axisFlag = false;
    private bool againstWallFlag = false;
    private float fireCooldown = 0;
    private int bulletID = 0;

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
    public AudioClip fireBoomerang;
    public AudioClip fireRainbow;

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
        fireBoomerang = (AudioClip)Resources.Load("Sounds/Sfx/ShotBoomerang");
        fireRainbow = (AudioClip)Resources.Load("Sounds/Sfx/ShotRainbow");

        // Weapon cooldowns; first three are without Rapid Fire, last three are with
        WEAPON_COOLDOWNS[0] = 0.085f;
        WEAPON_COOLDOWNS[1] = 0.3f;
        WEAPON_COOLDOWNS[2] = 0.17f;
        WEAPON_COOLDOWNS[3] = 0.0425f;
        WEAPON_COOLDOWNS[4] = 0.15f;
        WEAPON_COOLDOWNS[5] = 0.085f;

        PlayState.currentCharacter = "Snaily";
        player.playerScriptSnaily = this;
    }

    // This function is called once per frame
    void Update()
    {
        
    }

    // This function is called once every 0.02 seconds (50 time a second) regardless of framerate. Unity requires all physics calculations to be
    // run in this function, so it's where I put movement code as it utilizes boxcasts
    void FixedUpdate()
    {
        if (PlayState.gameState == "Game")
        {
            // To start things off, we mark our current position as the last position we took. Same with our hitbox size
            // Among other things, this is used to test for ground when we're airborne
            lastPosition = new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y);
            lastSize = box.size;
            // We also update all our boxcasts, both for the corner and in case they're misaligned with our current gravity state
            UpdateBoxcasts();
            // Next, we decrease the fire cooldown
            fireCooldown = Mathf.Clamp(fireCooldown - Time.fixedDeltaTime, 0, Mathf.Infinity);
            // Then, we check to see if Snaily has been stunned and this script has not been made aware
            if (player.stunned && gravityDir != DIR_FLOOR && grounded && !PlayState.CheckForItem("Gravity Snail"))
            {
                switch (gravityDir)
                {
                    case DIR_WALL_LEFT:
                    case DIR_WALL_RIGHT:
                        SwapDir(DIR_FLOOR);
                        SwitchSurfaceAxis();
                        transform.position = new Vector2(transform.position.x + ((box.size.x - box.size.y) * 0.5f * (facingLeft ? 1 : -1)), transform.position.y);
                        gravityDir = DIR_FLOOR;
                        grounded = false;
                        break;
                    case DIR_CEILING:
                        grounded = false;
                        gravityDir = DIR_FLOOR;
                        SwapDir(DIR_FLOOR);
                        break;
                }
                return;
            }
            // We reset the flag marking if Snaily is airborne and shoving their face into a wall
            againstWallFlag = false;
            // Finally, we update the parent Player script with our current gravity and directions
            player.gravityDir = gravityDir;
            player.facingLeft = facingLeft;
            player.facingDown = facingDown;

            // Next, we run different blocks of movement code based on our gravity state. They're largely the same, but are kept separate
            // so that things can stay different between them if needed, like Snaily falling off walls and ceilings without Gravity Snail
            if (!player.inDeathCutscene)
            {
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
                            if (Control.AxisX() != 0 && !Control.StrafeHold() && !PlayState.paralyzed)
                            {
                                if (shelled)
                                {
                                    if (Control.AxisX() == (facingLeft ? 1 : -1))
                                        transform.position = new Vector2(transform.position.x + (0.1667f * (facingLeft ? 1 : -1)), transform.position.y);
                                    if (grounded)
                                        ToggleShell();
                                    float distance = Vector2.Distance(boxL.point, new Vector2(transform.position.x, boxL.point.y));
                                    if (distance < box.size.x * 0.5f)
                                    {
                                        transform.position = new Vector2(transform.position.x + ((box.size.x * 0.675f) - distance) *
                                            (boxL.point.x < transform.position.x ? 1 : -1), transform.position.y);
                                        UpdateBoxcasts();
                                    }
                                }
                                SwapDir(Control.RightHold() ? DIR_WALL_RIGHT : DIR_WALL_LEFT);
                                float runSpeedValue = RUNSPEED_NORMAL * speedMod * Time.fixedDeltaTime;
                                if ((facingLeft ? boxL : boxR).distance < runSpeedValue)
                                {
                                    againstWallFlag = true;
                                    velocity.x = facingLeft ? -runSpeedValue + (runSpeedValue - boxL.distance) + 0.0078125f :
                                        runSpeedValue - (runSpeedValue - boxR.distance) - 0.0078125f;
                                    // In case the player happens to be holding the relative up/down button while the character runs face-first into a wall,
                                    // we check to see if climbing is possible in either direction and switch the character's gravity state
                                    if ((boxD.distance + boxU.distance) >= 1)
                                    {
                                        if (!player.stunned)
                                        {
                                            if (Control.UpHold() || (Control.DownHold() && !grounded))
                                            {
                                                transform.position = new Vector2(transform.position.x + velocity.x, transform.position.y);
                                                float boxCorrection = (box.size.y - box.size.x) * 0.5f;
                                                float ceilDis = boxU.distance - boxCorrection;
                                                float floorDis = boxD.distance - boxCorrection;
                                                SwitchSurfaceAxis();
                                                UpdateBoxcasts();
                                                float adjustment = 0;
                                                if (grounded)
                                                    adjustment = boxCorrection;
                                                else
                                                {
                                                    if (ceilDis < floorDis && ceilDis < box.size.y * 0.5f)
                                                        adjustment = -(ceilDis - (box.size.y * 0.5f));
                                                    else if (floorDis < ceilDis && floorDis < box.size.y * 0.5f)
                                                        adjustment = floorDis - (box.size.y * 0.5f);
                                                }
                                                transform.position = new Vector2(
                                                    transform.position.x + (facingLeft ? boxCorrection : -boxCorrection),
                                                    transform.position.y - adjustment
                                                    );
                                                SwapDir(Control.UpHold() ? DIR_CEILING : DIR_FLOOR);
                                                gravityDir = facingLeft ? DIR_WALL_LEFT : DIR_WALL_RIGHT;
                                                grounded = true;
                                                return;
                                            }
                                        }
                                    }
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
                                if (!againstWallFlag)
                                {
                                    transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
                                }
                                else
                                {
                                    // This entire block here covers the specific case of slipping into a one-tall tunnel in a wall while midair
                                    for (int i = 0; i < 8; i++)
                                    {
                                        transform.position = new Vector2(transform.position.x, transform.position.y + (velocity.y * 0.125f));
                                        RaycastHit2D tunnelCheckUpper = Physics2D.Raycast(
                                            new Vector2(transform.position.x, transform.position.y + 0.375f),
                                            facingLeft ? Vector2.left : Vector2.right,
                                            Mathf.Infinity,
                                            playerCollide,
                                            Mathf.Infinity,
                                            Mathf.Infinity
                                            );
                                        RaycastHit2D tunnelCheckLower = Physics2D.Raycast(
                                            new Vector2(transform.position.x, transform.position.y - 0.375f),
                                            facingLeft ? Vector2.left : Vector2.right,
                                            Mathf.Infinity,
                                            playerCollide,
                                            Mathf.Infinity,
                                            Mathf.Infinity
                                            );
                                        if (tunnelCheckUpper.distance >= 1.5f && tunnelCheckLower.distance >= 1.5f)
                                        {
                                            transform.position = new Vector2(
                                                transform.position.x + ((facingLeft ? -RUNSPEED_NORMAL : RUNSPEED_NORMAL) * speedMod * Time.fixedDeltaTime),
                                                Mathf.Floor(transform.position.y) + 0.5f);
                                            i = 8;
                                        }
                                    }
                                }
                                UpdateBoxcasts();
                                if (pokedCeiling)
                                {
                                    velocity.y = 0;
                                    if (Control.UpHold())
                                    {
                                        gravityDir = DIR_CEILING;
                                        SwapDir(DIR_CEILING);
                                        grounded = true;
                                        holdingShell = true;
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                if (boxD.distance > 0.0125f)
                                {
                                    if (boxCorner.distance <= 0.0125f)
                                    {
                                        if (Control.DownHold() && Control.AxisX() == (facingLeft ? -1 : 1) && !player.stunned)
                                        {
                                            SwapDir(facingLeft ? DIR_WALL_RIGHT : DIR_WALL_LEFT);
                                            SwitchSurfaceAxis();
                                            UpdateBoxcasts();
                                            RaycastHit2D wallTester = Physics2D.Raycast(
                                                new Vector2(transform.position.x + (facingLeft ? -box.size.x * 0.5f : box.size.x * 0.5f), transform.position.y - 0.75f),
                                                facingLeft ? Vector2.left : Vector2.right,
                                                Mathf.Infinity,
                                                playerCollide,
                                                Mathf.Infinity,
                                                Mathf.Infinity
                                                );
                                            transform.position = new Vector2(
                                                transform.position.x + (facingLeft ? -wallTester.distance : wallTester.distance),
                                                transform.position.y
                                                );
                                            gravityDir = facingLeft ? DIR_WALL_LEFT : DIR_WALL_RIGHT;
                                            return;
                                        }
                                    }
                                    else
                                        grounded = false;
                                }
                            }

                            // Now, let's see if we can jump
                            if (boxD.distance == 0)
                            {
                                transform.position = new Vector2(transform.position.x, transform.position.y + 0.01f);
                                UpdateBoxcasts();
                            }
                            if (Control.JumpHold() && grounded && !holdingJump && boxU.distance > 0.95f && !PlayState.paralyzed)
                            {
                                if (shelled)
                                    ToggleShell();
                                grounded = false;
                                velocity.y = JUMPPOWER_NORMAL * jumpMod * Time.deltaTime;
                                sfx.PlayOneShot(jump);
                            }
                            if (Control.JumpHold() && !holdingJump)
                                holdingJump = true;
                            else if (!Control.JumpHold() && holdingJump)
                                holdingJump = false;

                            // Finally, we check to see if we can shell
                            if (Control.DownHold() &&
                                Control.AxisX() == 0 &&
                                !Control.JumpHold() &&
                                !Control.ShootHold() &&
                                !Control.StrafeHold() &&
                                !holdingShell && !PlayState.paralyzed)
                            {
                                if (!shelled)
                                    ToggleShell();
                                else
                                {
                                    if (boxL.distance < 0.3375f && boxR.distance < 0.3375f)
                                        break;
                                    if (boxL.distance > 0.3375f && boxR.distance < 0.3375f)
                                        transform.position = new Vector2(transform.position.x - (0.675f - boxR.distance - (facingLeft ? 0.25f : 0)),
                                            transform.position.y);
                                    else if (boxL.distance < 0.3375f && boxR.distance > 0.3375f)
                                        transform.position = new Vector2(transform.position.x + (0.675f - boxL.distance - (facingLeft ? 0 : 0.25f)),
                                            transform.position.y);
                                    ToggleShell();
                                }
                                holdingShell = true;
                            }
                            else if (!holdingShell && Control.DownHold())
                                holdingShell = true;
                            if (holdingShell && !Control.DownHold())
                                holdingShell = false;
                        }
                        break;
                    case DIR_WALL_LEFT:
                        // This if block's purpose is so that you can click that minus button on the left and hide it from view, just so that you don't have
                        // to scroll quite as much if you don't want to. Cleanup, basically
                        if (true)
                        {
                            // We start by zeroing our relative vertical velocity if we're grounded, and our relative horizontal velocity no matter what
                            velocity.y = 0;
                            if (grounded)
                                velocity.x = 0;

                            // From here, we perform relatively horizontal movement checks to move, stop if we hit a wall, and allow for climbing
                            if (Control.AxisY() != 0 && !Control.StrafeHold() && !PlayState.paralyzed)
                            {
                                if (shelled)
                                {
                                    if (Control.AxisX() == (facingDown ? 1 : -1))
                                        transform.position = new Vector2(transform.position.x, transform.position.y + (0.1667f * (facingDown ? 1 : -1)));
                                    if (grounded)
                                        ToggleShell();
                                    float distance = Vector2.Distance(boxD.point, new Vector2(boxD.point.x, transform.position.y));
                                    if (distance < box.size.y * 0.5f)
                                    {
                                        transform.position = new Vector2(transform.position.x, transform.position.y + ((box.size.y * 0.675f) - distance) *
                                            (boxD.point.y < transform.position.y ? 1 : -1));
                                        UpdateBoxcasts();
                                    }
                                }
                                SwapDir(Control.UpHold() ? DIR_CEILING : DIR_FLOOR);
                                float runSpeedValue = RUNSPEED_NORMAL * speedMod * Time.fixedDeltaTime;
                                if ((facingDown ? boxD : boxU).distance < runSpeedValue)
                                {
                                    againstWallFlag = true;
                                    velocity.y = facingDown ? -runSpeedValue + (runSpeedValue - boxD.distance) + 0.0078125f :
                                        runSpeedValue - (runSpeedValue - boxU.distance) - 0.0078125f;
                                    // In case the player happens to be holding the relative up/down button while the character runs face-first into a wall,
                                    // we check to see if climbing is possible in either direction and switch the character's gravity state
                                    if ((boxL.distance + boxR.distance) >= 1)
                                    {
                                        if (!player.stunned)
                                        {
                                            if (Control.RightHold() || (Control.LeftHold() && !grounded))
                                            {
                                                transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
                                                float boxCorrection = (box.size.x - box.size.y) * 0.5f;
                                                float ceilDis = boxR.distance - boxCorrection;
                                                float floorDis = boxL.distance - boxCorrection;
                                                SwitchSurfaceAxis();
                                                UpdateBoxcasts();
                                                float adjustment = 0;
                                                if (grounded)
                                                    adjustment = boxCorrection;
                                                else
                                                {
                                                    if (ceilDis < floorDis && ceilDis < box.size.y * 0.5f)
                                                        adjustment = -(ceilDis - (box.size.y * 0.5f));
                                                    else if (floorDis < ceilDis && floorDis < box.size.y * 0.5f)
                                                        adjustment = floorDis - (box.size.y * 0.5f);
                                                }
                                                transform.position = new Vector2(
                                                    transform.position.x - adjustment,
                                                    transform.position.y + (facingDown ? boxCorrection : -boxCorrection)
                                                    );
                                                SwapDir((Control.RightHold()) ? DIR_WALL_RIGHT : DIR_WALL_LEFT);
                                                gravityDir = facingDown ? DIR_FLOOR : DIR_CEILING;
                                                grounded = true;
                                                return;
                                            }
                                        }
                                    }
                                }
                                else
                                    velocity.y = facingDown ? -runSpeedValue : runSpeedValue;
                                transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
                                UpdateBoxcasts();
                            }

                            // Now, we perform relatively vertical checks. This mainly involves jumping and falling
                            if (!grounded)
                            {
                                if (!PlayState.CheckForItem("Gravity Snail"))
                                {
                                    transform.position = new Vector2(transform.position.x + 0.0625f + (box.size.y - box.size.x) * 0.5f, transform.position.y);
                                    SwapDir(DIR_FLOOR);
                                    SwitchSurfaceAxis();
                                    gravityDir = DIR_FLOOR;
                                    if (Control.DownHold())
                                        holdingShell = true;
                                }
                                else
                                {
                                    bool pokedCeiling = false;
                                    velocity.x = Mathf.Clamp(velocity.x - GRAVITY * gravityMod * Time.fixedDeltaTime * ((!holdingJump && velocity.x > 0) ? FALLSPEED_MOD : 1), TERMINAL_VELOCITY, Mathf.Infinity);
                                    if (boxL.distance != 0 && boxR.distance != 0)
                                    {
                                        if (boxL.distance < -velocity.x && Mathf.Sign(velocity.x) == -1)
                                        {
                                            velocity.x = -boxL.distance;
                                            grounded = true;
                                        }
                                        else if (boxR.distance < velocity.x && Mathf.Sign(velocity.x) == 1)
                                        {
                                            velocity.x = boxR.distance;
                                            pokedCeiling = true;
                                        }
                                    }
                                    if (!againstWallFlag)
                                    {
                                        transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
                                    }
                                    else
                                    {
                                        // This entire block here covers the specific case of slipping into a one-tall tunnel in a wall while midair
                                        for (int i = 0; i < 8; i++)
                                        {
                                            transform.position = new Vector2(transform.position.x + (velocity.x * 0.125f), transform.position.y);
                                            RaycastHit2D tunnelCheckUpper = Physics2D.Raycast(
                                                new Vector2(transform.position.x + 0.375f, transform.position.y),
                                                facingDown ? Vector2.down : Vector2.up,
                                                Mathf.Infinity,
                                                playerCollide,
                                                Mathf.Infinity,
                                                Mathf.Infinity
                                                );
                                            RaycastHit2D tunnelCheckLower = Physics2D.Raycast(
                                                new Vector2(transform.position.x - 0.375f, transform.position.y),
                                                facingDown ? Vector2.down : Vector2.up,
                                                Mathf.Infinity,
                                                playerCollide,
                                                Mathf.Infinity,
                                                Mathf.Infinity
                                                );
                                            if (tunnelCheckUpper.distance >= 1.5f && tunnelCheckLower.distance >= 1.5f)
                                            {
                                                transform.position = new Vector2(
                                                    Mathf.Floor(transform.position.x) + 0.5f,
                                                    transform.position.y + ((facingDown ? -RUNSPEED_NORMAL : RUNSPEED_NORMAL) * speedMod * Time.fixedDeltaTime));
                                                i = 8;
                                            }
                                        }
                                    }
                                    UpdateBoxcasts();
                                    if (pokedCeiling)
                                    {
                                        velocity.x = 0;
                                        if (Control.RightHold())
                                        {
                                            gravityDir = DIR_WALL_RIGHT;
                                            SwapDir(DIR_WALL_RIGHT);
                                            grounded = true;
                                            holdingShell = true;
                                            return;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (boxL.distance > 0.0125f)
                                {
                                    if (boxCorner.distance <= 0.0125f)
                                    {
                                        if (!PlayState.CheckForItem("Gravity Snail"))
                                        {
                                            SwapDir(DIR_FLOOR);
                                            SwitchSurfaceAxis();
                                            gravityDir = DIR_FLOOR;
                                            if (Control.LeftHold() || Control.DownHold())
                                                holdingShell = true;
                                            transform.position = new Vector2(transform.position.x + 0.0625f, transform.position.y);
                                            UpdateBoxcasts();
                                            if (boxL.distance == 0)
                                                transform.position = new Vector2(transform.position.x - 0.3125f, transform.position.y);
                                            return;
                                        }
                                        else if (Control.LeftHold() && Control.AxisY() == (facingDown ? -1 : 1) && !player.stunned)
                                        {
                                            SwapDir(facingDown ? DIR_CEILING : DIR_FLOOR);
                                            SwitchSurfaceAxis();
                                            UpdateBoxcasts();
                                            RaycastHit2D wallTester = Physics2D.Raycast(
                                                new Vector2(transform.position.x - 0.75f, transform.position.y + (facingDown ? -box.size.y * 0.5f : box.size.y * 0.5f)),
                                                facingDown ? Vector2.down : Vector2.up,
                                                Mathf.Infinity,
                                                playerCollide,
                                                Mathf.Infinity,
                                                Mathf.Infinity
                                                );
                                            transform.position = new Vector2(
                                                transform.position.x,
                                                transform.position.y + (facingDown ? -wallTester.distance : wallTester.distance)
                                                );
                                            gravityDir = facingDown ? DIR_FLOOR : DIR_CEILING;
                                            return;
                                        }
                                    }
                                    else
                                        grounded = false;
                                }
                            }

                            // Now, let's see if we can jump
                            if (boxL.distance == 0)
                            {
                                transform.position = new Vector2(transform.position.x + 0.01f, transform.position.y);
                                UpdateBoxcasts();
                            }
                            if (Control.JumpHold() && grounded && !holdingJump && boxR.distance > 0.95f && !PlayState.paralyzed)
                            {
                                if (shelled)
                                    ToggleShell();
                                grounded = false;
                                if (PlayState.CheckForItem("Gravity Snail"))
                                    velocity.x = JUMPPOWER_NORMAL * jumpMod * Time.deltaTime;
                                else
                                {
                                    transform.position = new Vector2(transform.position.x + 0.0625f + (box.size.y - box.size.x) * 0.5f, transform.position.y);
                                    SwapDir(DIR_FLOOR);
                                    SwitchSurfaceAxis();
                                    gravityDir = DIR_FLOOR;
                                    if (Control.DownHold())
                                        holdingShell = true;
                                }
                                sfx.PlayOneShot(jump);
                            }
                            if (Control.JumpHold() && !holdingJump)
                                holdingJump = true;
                            else if (!Control.JumpHold() && holdingJump)
                                holdingJump = false;

                            // Finally, we check to see if we can shell
                            if (Control.AxisY() == 0 &&
                                Control.LeftHold() &&
                                !Control.JumpHold() &&
                                !Control.ShootHold() &&
                                !Control.StrafeHold() &&
                                !holdingShell && !PlayState.paralyzed)
                            {
                                if (!shelled)
                                    ToggleShell();
                                else
                                {
                                    if (boxD.distance < 0.3375f && boxU.distance < 0.3375f)
                                        break;
                                    if (boxD.distance > 0.3375f && boxU.distance < 0.3375f)
                                        transform.position = new Vector2(transform.position.x, transform.position.y -
                                            (0.675f - boxU.distance - (facingLeft ? 0.25f : 0)));
                                    else if (boxD.distance < 0.3375f && boxU.distance > 0.3375f)
                                        transform.position = new Vector2(transform.position.x, transform.position.y +
                                            (0.675f - boxD.distance - (facingLeft ? 0 : 0.25f)));
                                    ToggleShell();
                                }
                                holdingShell = true;
                            }
                            else if (!holdingShell && Control.LeftHold())
                                holdingShell = true;
                            if (holdingShell && !Control.LeftHold())
                                holdingShell = false;
                        }
                        break;
                    case DIR_WALL_RIGHT:
                        // This if block's purpose is so that you can click that minus button on the left and hide it from view, just so that you don't have
                        // to scroll quite as much if you don't want to. Cleanup, basically
                        if (true)
                        {
                            // We start by zeroing our relative vertical velocity if we're grounded, and our relative horizontal velocity no matter what
                            velocity.y = 0;
                            if (grounded)
                                velocity.x = 0;

                            // From here, we perform relatively horizontal movement checks to move, stop if we hit a wall, and allow for climbing
                            if (Control.AxisY() != 0 && !Control.StrafeHold() && !PlayState.paralyzed)
                            {
                                if (shelled)
                                {
                                    if (Control.AxisX() == (facingDown ? 1 : -1))
                                        transform.position = new Vector2(transform.position.x, transform.position.y + (0.1667f * (facingDown ? 1 : -1)));
                                    if (grounded)
                                        ToggleShell();
                                    float distance = Vector2.Distance(boxD.point, new Vector2(boxD.point.x, transform.position.y));
                                    if (distance < box.size.y * 0.5f)
                                    {
                                        transform.position = new Vector2(transform.position.x, transform.position.y + ((box.size.y * 0.675f) - distance) *
                                            (boxD.point.y < transform.position.y ? 1 : -1));
                                        UpdateBoxcasts();
                                    }
                                }
                                SwapDir(Control.UpHold() ? DIR_CEILING : DIR_FLOOR);
                                float runSpeedValue = RUNSPEED_NORMAL * speedMod * Time.fixedDeltaTime;
                                if ((facingDown ? boxD : boxU).distance < runSpeedValue)
                                {
                                    againstWallFlag = true;
                                    velocity.y = facingDown ? -runSpeedValue + (runSpeedValue - boxD.distance) + 0.0078125f :
                                        runSpeedValue - (runSpeedValue - boxU.distance) - 0.0078125f;
                                    // In case the player happens to be holding the relative up/down button while the character runs face-first into a wall,
                                    // we check to see if climbing is possible in either direction and switch the character's gravity state
                                    if ((boxL.distance + boxR.distance) >= 1)
                                    {
                                        if (!player.stunned)
                                        {
                                            if (Control.LeftHold() || (Control.RightHold() && !grounded))
                                            {
                                                transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
                                                float boxCorrection = (box.size.x - box.size.y) * 0.5f;
                                                float ceilDis = boxL.distance - boxCorrection;
                                                float floorDis = boxR.distance - boxCorrection;
                                                SwitchSurfaceAxis();
                                                UpdateBoxcasts();
                                                float adjustment = 0;
                                                if (grounded)
                                                    adjustment = -boxCorrection;
                                                else
                                                {
                                                    if (ceilDis < floorDis && ceilDis < box.size.x * 0.5f)
                                                        adjustment = ceilDis - (box.size.x * 0.5f);
                                                    else if (floorDis < ceilDis && floorDis < box.size.x * 0.5f)
                                                        adjustment = -(floorDis - (box.size.x * 0.5f));
                                                }
                                                transform.position = new Vector2(
                                                    transform.position.x - adjustment,
                                                    transform.position.y + (facingDown ? boxCorrection : -boxCorrection)
                                                    );
                                                SwapDir(Control.RightHold() ? DIR_WALL_RIGHT : DIR_WALL_LEFT);
                                                gravityDir = facingDown ? DIR_FLOOR : DIR_CEILING;
                                                grounded = true;
                                                return;
                                            }
                                        }
                                    }
                                }
                                else
                                    velocity.y = facingDown ? -runSpeedValue : runSpeedValue;
                                transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
                                UpdateBoxcasts();
                            }

                            // Now, we perform relatively vertical checks. This mainly involves jumping and falling
                            if (!grounded)
                            {
                                if (!PlayState.CheckForItem("Gravity Snail"))
                                {
                                    transform.position = new Vector2(transform.position.x - 0.0625f - (box.size.y - box.size.x) * 0.5f, transform.position.y);
                                    SwapDir(DIR_FLOOR);
                                    SwitchSurfaceAxis();
                                    gravityDir = DIR_FLOOR;
                                    if (Control.DownHold())
                                        holdingShell = true;
                                }
                                else
                                {
                                    bool pokedCeiling = false;
                                    velocity.x = Mathf.Clamp(velocity.x + GRAVITY * gravityMod * Time.fixedDeltaTime * ((!holdingJump && velocity.x < 0) ? FALLSPEED_MOD : 1), -Mathf.Infinity, -TERMINAL_VELOCITY);
                                    if (boxL.distance != 0 && boxR.distance != 0)
                                    {
                                        if (boxL.distance < -velocity.x && Mathf.Sign(velocity.x) == -1)
                                        {
                                            velocity.x = -boxL.distance;
                                            pokedCeiling = true;
                                        }
                                        else if (boxR.distance < velocity.x && Mathf.Sign(velocity.x) == 1)
                                        {
                                            velocity.x = boxR.distance;
                                            grounded = true;
                                        }
                                    }
                                    if (!againstWallFlag)
                                    {
                                        transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
                                    }
                                    else
                                    {
                                        // This entire block here covers the specific case of slipping into a one-tall tunnel in a wall while midair
                                        for (int i = 0; i < 8; i++)
                                        {
                                            transform.position = new Vector2(transform.position.x + (velocity.x * 0.125f), transform.position.y);
                                            RaycastHit2D tunnelCheckUpper = Physics2D.Raycast(
                                                new Vector2(transform.position.x + 0.375f, transform.position.y),
                                                facingDown ? Vector2.down : Vector2.up,
                                                Mathf.Infinity,
                                                playerCollide,
                                                Mathf.Infinity,
                                                Mathf.Infinity
                                                );
                                            RaycastHit2D tunnelCheckLower = Physics2D.Raycast(
                                                new Vector2(transform.position.x - 0.375f, transform.position.y),
                                                facingDown ? Vector2.down : Vector2.up,
                                                Mathf.Infinity,
                                                playerCollide,
                                                Mathf.Infinity,
                                                Mathf.Infinity
                                                );
                                            if (tunnelCheckUpper.distance >= 1.5f && tunnelCheckLower.distance >= 1.5f)
                                            {
                                                transform.position = new Vector2(
                                                    Mathf.Floor(transform.position.x) + 0.5f,
                                                    transform.position.y + ((facingDown ? -RUNSPEED_NORMAL : RUNSPEED_NORMAL) * speedMod * Time.fixedDeltaTime));
                                                i = 8;
                                            }
                                        }
                                    }
                                    UpdateBoxcasts();
                                    if (pokedCeiling)
                                    {
                                        velocity.x = 0;
                                        if (Control.LeftHold())
                                        {
                                            gravityDir = DIR_WALL_LEFT;
                                            SwapDir(DIR_WALL_LEFT);
                                            grounded = true;
                                            holdingShell = true;
                                            return;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (boxR.distance > 0.0125f)
                                {
                                    if (!PlayState.CheckForItem("Gravity Snail"))
                                    {
                                        SwapDir(DIR_FLOOR);
                                        SwitchSurfaceAxis();
                                        gravityDir = DIR_FLOOR;
                                        if (Control.RightHold() || Control.DownHold())
                                            holdingShell = true;
                                        transform.position = new Vector2(transform.position.x - 0.0625f, transform.position.y);
                                        UpdateBoxcasts();
                                        if (boxL.distance == 0)
                                            transform.position = new Vector2(transform.position.x + 0.3125f, transform.position.y);
                                        return;
                                    }
                                    else if (boxCorner.distance <= 0.0125f)
                                    {
                                        if (Control.RightHold() && Control.AxisY() == (facingDown ? -1 : 1) && !player.stunned)
                                        {
                                            SwapDir(facingDown ? DIR_CEILING : DIR_FLOOR);
                                            SwitchSurfaceAxis();
                                            UpdateBoxcasts();
                                            RaycastHit2D wallTester = Physics2D.Raycast(
                                                new Vector2(transform.position.x + 0.75f, transform.position.y + (facingDown ? -box.size.y * 0.5f : box.size.y * 0.5f)),
                                                facingDown ? Vector2.down : Vector2.up,
                                                Mathf.Infinity,
                                                playerCollide,
                                                Mathf.Infinity,
                                                Mathf.Infinity
                                                );
                                            transform.position = new Vector2(
                                                transform.position.x,
                                                transform.position.y + (facingDown ? -wallTester.distance : wallTester.distance)
                                                );
                                            gravityDir = facingDown ? DIR_FLOOR : DIR_CEILING;
                                            return;
                                        }
                                    }
                                    else
                                        grounded = false;
                                }
                            }

                            // Now, let's see if we can jump
                            if (boxR.distance == 0)
                            {
                                transform.position = new Vector2(transform.position.x - 0.01f, transform.position.y);
                                UpdateBoxcasts();
                            }
                            if (Control.JumpHold() && grounded && !holdingJump && boxL.distance > 0.95f && !PlayState.paralyzed)
                            {
                                if (shelled)
                                    ToggleShell();
                                grounded = false;
                                if (PlayState.CheckForItem("Gravity Snail"))
                                    velocity.x = -JUMPPOWER_NORMAL * jumpMod * Time.deltaTime;
                                else
                                {
                                    transform.position = new Vector2(transform.position.x - 0.0625f - (box.size.y - box.size.x) * 0.5f, transform.position.y);
                                    SwapDir(DIR_FLOOR);
                                    SwitchSurfaceAxis();
                                    gravityDir = DIR_FLOOR;
                                    if (Control.DownHold())
                                        holdingShell = true;
                                }
                                sfx.PlayOneShot(jump);
                            }
                            if (Control.JumpHold() && !holdingJump)
                                holdingJump = true;
                            else if (!Control.JumpHold() && holdingJump)
                                holdingJump = false;

                            // Finally, we check to see if we can shell
                            if (Control.AxisY() == 0 &&
                                Control.RightHold() &&
                                !Control.JumpHold() &&
                                !Control.ShootHold() &&
                                !Control.StrafeHold() &&
                                !holdingShell && !PlayState.paralyzed)
                            {
                                if (!shelled)
                                    ToggleShell();
                                else
                                {
                                    if (boxD.distance < 0.3375f && boxU.distance < 0.3375f)
                                        break;
                                    if (boxD.distance > 0.3375f && boxU.distance < 0.3375f)
                                        transform.position = new Vector2(transform.position.x, transform.position.y -
                                            (0.675f - boxU.distance - (facingLeft ? 0.25f : 0)));
                                    else if (boxD.distance < 0.3375f && boxU.distance > 0.3375f)
                                        transform.position = new Vector2(transform.position.x, transform.position.y +
                                            (0.675f - boxD.distance - (facingLeft ? 0 : 0.25f)));
                                    ToggleShell();
                                }
                                holdingShell = true;
                            }
                            else if (!holdingShell && Control.RightHold())
                                holdingShell = true;
                            if (holdingShell && !Control.RightHold())
                                holdingShell = false;
                        }
                        break;
                    case DIR_CEILING:
                        // This if block's purpose is so that you can click that minus button on the left and hide it from view, just so that you don't have
                        // to scroll quite as much if you don't want to. Cleanup, basically
                        if (true)
                        {
                            // We start by zeroing our relative vertical velocity if we're grounded, and our relative horizontal velocity no matter what
                            velocity.x = 0;
                            if (grounded)
                                velocity.y = 0;

                            // From here, we perform relatively horizontal movement checks to move, stop if we hit a wall, and allow for climbing
                            if (Control.AxisX() != 0 && !Control.StrafeHold() && !PlayState.paralyzed)
                            {
                                if (shelled)
                                {
                                    if (Control.AxisX() == (facingLeft ? 1 : -1))
                                        transform.position = new Vector2(transform.position.x + (0.1667f * (facingLeft ? 1 : -1)), transform.position.y);
                                    if (grounded)
                                        ToggleShell();
                                    float distance = Vector2.Distance(boxL.point, new Vector2(transform.position.x, boxL.point.y));
                                    if (distance < box.size.x * 0.5f)
                                    {
                                        transform.position = new Vector2(transform.position.x + ((box.size.x * 0.675f) - distance) *
                                            (boxL.point.x < transform.position.x ? 1 : -1), transform.position.y);
                                        UpdateBoxcasts();
                                    }
                                }
                                SwapDir(Control.RightHold() ? DIR_WALL_RIGHT : DIR_WALL_LEFT);
                                float runSpeedValue = RUNSPEED_NORMAL * speedMod * Time.fixedDeltaTime;
                                if ((facingLeft ? boxL : boxR).distance < runSpeedValue)
                                {
                                    againstWallFlag = true;
                                    velocity.x = facingLeft ? -runSpeedValue + (runSpeedValue - boxL.distance) + 0.0078125f :
                                        runSpeedValue - (runSpeedValue - boxR.distance) - 0.0078125f;
                                    // In case the player happens to be holding the relative up/down button while the character runs face-first into a wall,
                                    // we check to see if climbing is possible in either direction and switch the character's gravity state
                                    if ((boxD.distance + boxU.distance) >= 1)
                                    {
                                        if (!player.stunned)
                                        {
                                            if (Control.DownHold() || (Control.UpHold() && !grounded))
                                            {
                                                transform.position = new Vector2(transform.position.x + velocity.x, transform.position.y);
                                                float boxCorrection = (box.size.y - box.size.x) * 0.5f;
                                                float ceilDis = boxD.distance - boxCorrection;
                                                float floorDis = boxU.distance - boxCorrection;
                                                SwitchSurfaceAxis();
                                                UpdateBoxcasts();
                                                float adjustment = 0;
                                                if (grounded)
                                                    adjustment = -boxCorrection;
                                                else
                                                {
                                                    if (ceilDis < floorDis && ceilDis < box.size.y * 0.5f)
                                                        adjustment = ceilDis - (box.size.y * 0.5f);
                                                    else if (floorDis < ceilDis && floorDis < box.size.y * 0.5f)
                                                        adjustment = -(floorDis - (box.size.y * 0.5f));
                                                }
                                                transform.position = new Vector2(
                                                    transform.position.x + (facingLeft ? boxCorrection : -boxCorrection),
                                                    transform.position.y - adjustment
                                                    );
                                                SwapDir(Control.UpHold() ? DIR_CEILING : DIR_FLOOR);
                                                gravityDir = facingLeft ? DIR_WALL_LEFT : DIR_WALL_RIGHT;
                                                grounded = true;
                                                return;
                                            }
                                        }
                                    }
                                }
                                else
                                    velocity.x = facingLeft ? -runSpeedValue : runSpeedValue;
                                transform.position = new Vector2(transform.position.x + velocity.x, transform.position.y);
                                UpdateBoxcasts();
                            }

                            // Now, we perform relatively vertical checks. This mainly involves jumping and falling
                            if (!grounded)
                            {
                                if (!PlayState.CheckForItem("Gravity Snail"))
                                {
                                    SwapDir(DIR_FLOOR);
                                    gravityDir = DIR_FLOOR;
                                    if (Control.DownHold())
                                        holdingShell = true;
                                }
                                else
                                {
                                    bool pokedCeiling = false;
                                    velocity.y = Mathf.Clamp(velocity.y + GRAVITY * gravityMod * Time.fixedDeltaTime * ((!holdingJump && velocity.y < 0) ? FALLSPEED_MOD : 1), -Mathf.Infinity, -TERMINAL_VELOCITY);
                                    if (boxD.distance != 0 && boxU.distance != 0)
                                    {
                                        if (boxD.distance < -velocity.y && Mathf.Sign(velocity.y) == -1)
                                        {
                                            velocity.y = -boxD.distance;
                                            pokedCeiling = true;
                                        }
                                        else if (boxU.distance < velocity.y && Mathf.Sign(velocity.y) == 1)
                                        {
                                            velocity.y = boxU.distance;
                                            grounded = true;
                                        }
                                    }
                                    if (!againstWallFlag)
                                    {
                                        transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
                                    }
                                    else
                                    {
                                        // This entire block here covers the specific case of slipping into a one-tall tunnel in a wall while midair
                                        for (int i = 0; i < 8; i++)
                                        {
                                            transform.position = new Vector2(transform.position.x, transform.position.y + (velocity.y * 0.125f));
                                            RaycastHit2D tunnelCheckUpper = Physics2D.Raycast(
                                                new Vector2(transform.position.x, transform.position.y + 0.375f),
                                                facingLeft ? Vector2.left : Vector2.right,
                                                Mathf.Infinity,
                                                playerCollide,
                                                Mathf.Infinity,
                                                Mathf.Infinity
                                                );
                                            RaycastHit2D tunnelCheckLower = Physics2D.Raycast(
                                                new Vector2(transform.position.x, transform.position.y - 0.375f),
                                                facingLeft ? Vector2.left : Vector2.right,
                                                Mathf.Infinity,
                                                playerCollide,
                                                Mathf.Infinity,
                                                Mathf.Infinity
                                                );
                                            if (tunnelCheckUpper.distance >= 1.5f && tunnelCheckLower.distance >= 1.5f)
                                            {
                                                transform.position = new Vector2(
                                                    transform.position.x + ((facingLeft ? -RUNSPEED_NORMAL : RUNSPEED_NORMAL) * speedMod * Time.fixedDeltaTime),
                                                    Mathf.Floor(transform.position.y) + 0.5f);
                                                i = 8;
                                            }
                                        }
                                    }
                                    UpdateBoxcasts();
                                    if (pokedCeiling)
                                    {
                                        velocity.y = 0;
                                        if (Control.DownHold())
                                        {
                                            gravityDir = DIR_FLOOR;
                                            SwapDir(DIR_FLOOR);
                                            grounded = true;
                                            holdingShell = true;
                                            return;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (boxU.distance > 0.0125f)
                                {
                                    if (boxCorner.distance <= 0.0125f)
                                    {
                                        if (!PlayState.CheckForItem("Gravity Snail"))
                                        {
                                            SwapDir(DIR_FLOOR);
                                            gravityDir = DIR_FLOOR;
                                            if (Control.DownHold())
                                                holdingShell = true;
                                        }
                                        else if (Control.UpHold() && Control.AxisX() == (facingLeft ? -1 : 1) && !player.stunned)
                                        {
                                            SwapDir(facingLeft ? DIR_WALL_RIGHT : DIR_WALL_LEFT);
                                            SwitchSurfaceAxis();
                                            UpdateBoxcasts();
                                            RaycastHit2D wallTester = Physics2D.Raycast(
                                                new Vector2(transform.position.x + (facingLeft ? -box.size.x * 0.5f : box.size.x * 0.5f), transform.position.y + 0.75f),
                                                facingLeft ? Vector2.left : Vector2.right,
                                                Mathf.Infinity,
                                                playerCollide,
                                                Mathf.Infinity,
                                                Mathf.Infinity
                                                );
                                            transform.position = new Vector2(
                                                transform.position.x + (facingLeft ? -wallTester.distance : wallTester.distance),
                                                transform.position.y
                                                );
                                            gravityDir = facingLeft ? DIR_WALL_LEFT : DIR_WALL_RIGHT;
                                            return;
                                        }
                                    }
                                    else
                                        grounded = false;
                                }
                            }

                            // Now, let's see if we can jump
                            if (boxU.distance == 0)
                            {
                                transform.position = new Vector2(transform.position.x, transform.position.y - 0.01f);
                                UpdateBoxcasts();
                            }
                            if (Control.JumpHold() && grounded && !holdingJump && boxD.distance > 0.95f && !PlayState.paralyzed)
                            {
                                if (shelled)
                                    ToggleShell();
                                grounded = false;
                                if (PlayState.CheckForItem("Gravity Snail"))
                                    velocity.y = -JUMPPOWER_NORMAL * jumpMod * Time.deltaTime;
                                else
                                {
                                    SwapDir(DIR_FLOOR);
                                    gravityDir = DIR_FLOOR;
                                }
                                sfx.PlayOneShot(jump);
                            }
                            if (Control.JumpHold() && !holdingJump)
                                holdingJump = true;
                            else if (!Control.JumpHold() && holdingJump)
                                holdingJump = false;

                            // Finally, we check to see if we can shell
                            if (Control.UpHold() &&
                                Control.AxisX() == 0 &&
                                !Control.JumpHold() &&
                                !Control.ShootHold() &&
                                !Control.StrafeHold() &&
                                !holdingShell && !PlayState.paralyzed)
                            {
                                if (!shelled)
                                    ToggleShell();
                                else
                                {
                                    if (boxL.distance < 0.3375f && boxR.distance < 0.3375f)
                                        break;
                                    if (boxL.distance > 0.3375f && boxR.distance < 0.3375f)
                                        transform.position = new Vector2(transform.position.x - (0.675f - boxR.distance - (facingLeft ? 0.25f : 0)),
                                            transform.position.y);
                                    else if (boxL.distance < 0.3375f && boxR.distance > 0.3375f)
                                        transform.position = new Vector2(transform.position.x + (0.675f - boxL.distance - (facingLeft ? 0 : 0.25f)),
                                            transform.position.y);
                                    ToggleShell();
                                }
                                holdingShell = true;
                            }
                            else if (!holdingShell && Control.UpHold())
                                holdingShell = true;
                            if (holdingShell && !Control.UpHold())
                                holdingShell = false;
                        }
                        break;
                }

                if ((Control.ShootHold() || Control.StrafeHold()) && !PlayState.paralyzed)
                {
                    if (shelled)
                        ToggleShell();
                    Shoot();
                }
            }
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

        Vector2 cornerTestDir;
        if (GetDirName() == "CEILING")
            cornerTestDir = Vector2.up;
        else if (GetDirName() == "LEFT WALL")
            cornerTestDir = Vector2.left;
        else if (GetDirName() == "RIGHT WALL")
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

    // This function is called to reorient the player character in any way necessary
    // Note: this only accounts for four directions in either the ground/ceiling state or the wall state, never both. A call to
    // SwitchSurfaceAxis() is necessary for that
    private void SwapDir(int dirToFace)
    {
        switch (dirToFace)
        {
            case DIR_FLOOR:
                facingDown = true;
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
                facingDown = false;
                sprite.flipY = true;
                break;
        }
    }

    // This function is used to swap the player character between the ground/ceiling state and the wall state and vice versa
    private void SwitchSurfaceAxis()
    {
        if (!axisFlag)
            PlayAnim("wall");
        else
            PlayAnim("floor");
        axisFlag = !axisFlag;
        box.size = new Vector2(box.size.y, box.size.x);
    }

    // This function is called whenever a shelled character asks to enter/exit their shell
    public void ToggleShell()
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
                if (facingDown)
                    box.offset = new Vector2(0, HITBOX_SHELL_OFFSET);
                else
                    box.offset = new Vector2(0, -HITBOX_SHELL_OFFSET);
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
        UpdateBoxcasts();
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
    private string GetDirName()
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

    // This function handles activation of projectiles when the player presses either shoot button
    private void Shoot()
    {
        if (fireCooldown == 0 && player.selectedWeapon != 0)
        {
            Vector2 inputDir = new Vector2(Control.AxisX(), Control.AxisY());
            int type = player.selectedWeapon + (PlayState.CheckForItem("Devastator") ? 3 : 0);
            int dir = 0;
            switch (inputDir.x + "" + inputDir.y)
            {
                case "-11":
                    dir = 0;
                    break;
                case "01":
                    dir = 1;
                    break;
                case "11":
                    dir = 2;
                    break;
                case "-10":
                    dir = 3;
                    break;
                case "10":
                    dir = 4;
                    break;
                case "-1-1":
                    dir = 5;
                    break;
                case "0-1":
                    dir = 6;
                    break;
                case "1-1":
                    dir = 7;
                    break;
                case "00":
                    dir = -1;
                    break;
            }

            if (type == 1)
            {
                if (gravityDir == DIR_FLOOR && (dir == 5 || dir == 6 || dir == 7))
                    dir = facingLeft ? 3 : 4;
                else if (gravityDir == DIR_WALL_LEFT && (dir == 0 || dir == 3 || dir == 5))
                    dir = facingDown ? 6 : 1;
                else if (gravityDir == DIR_WALL_RIGHT && (dir == 2 || dir == 4 || dir == 7))
                    dir = facingDown ? 6 : 1;
                else if (gravityDir == DIR_CEILING && (dir == 0 || dir == 1 || dir == 2))
                    dir = facingLeft ? 3 : 4;
            }
            if (dir == -1)
            {
                if (gravityDir == DIR_FLOOR && dir == -1)
                    dir = facingLeft ? 3 : 4;
                else if (gravityDir == DIR_WALL_LEFT && dir == -1)
                    dir = facingDown ? 6 : 1;
                else if (gravityDir == DIR_WALL_RIGHT && dir == -1)
                    dir = facingDown ? 6 : 1;
                else if (gravityDir == DIR_CEILING && dir == -1)
                    dir = facingLeft ? 3 : 4;
            }
            if (!player.bulletPool.transform.GetChild(bulletID).GetComponent<Bullet>().isActive)
            {
                player.bulletPool.transform.GetChild(bulletID).GetComponent<Bullet>().Shoot(type, dir);
                bulletID++;
                if (bulletID >= player.bulletPool.transform.childCount)
                    bulletID = 0;
                fireCooldown = WEAPON_COOLDOWNS[type - 1];
                if (type == 6)
                    sfx.PlayOneShot(fireRainbow);
                else if (type == 5)
                    sfx.PlayOneShot(fireRainbow);
                else if (type == 4)
                    sfx.PlayOneShot(fireRainbow);
                else if (type == 3)
                    sfx.PlayOneShot(fireRainbow);
                else if (type == 2)
                    sfx.PlayOneShot(fireBoomerang);
                else
                    sfx.PlayOneShot(fireRainbow);
            }
        }
    }
}
