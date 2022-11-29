using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, ICutsceneObject {
    #region vars
    public const int DIR_FLOOR = 0;
    public const int DIR_WALL_LEFT = 1;
    public const int DIR_WALL_RIGHT = 2;
    public const int DIR_CEILING = 3;
    public int currentSurface = 0;
    public bool facingLeft = false;
    public bool facingDown = false;
    public int selectedWeapon = 0;
    public bool armed;
    public int health = 12;
    public int maxHealth = 12;
    public bool stunned = false;
    public bool inDeathCutscene = false;
    public int gravityDir = 0;
    public bool underwater = false;
    public Vector2 velocity = Vector2.zero;
    public bool grounded;
    public bool shelled;
    public bool ungroundedViaHop;

    public float speedMod = 1;
    public float jumpMod = 1;
    public float gravityMod = 1;
    public bool holdingJump = false;
    public bool holdingShell = false;
    public bool axisFlag = false;
    public bool againstWallFlag = false;
    public float fireCooldown = 0;
    public int bulletID = 0;
    public float sleepTimer = 30f;
    public bool isSleeping = false;
    public int readIDSpeed = 0;
    public int readIDJump = 0;
    public float jumpBufferCounter = 0;
    public float coyoteTimeCounter = 0;
    public float lastPointBeforeHop = 0;
    public bool[] animData;

    public AnimationModule anim;
    public SpriteRenderer sprite;
    public BoxCollider2D box;
    public Rigidbody2D rb;

    public RaycastHit2D boxL;
    public RaycastHit2D boxR;
    public RaycastHit2D boxU;
    public RaycastHit2D boxD;
    public RaycastHit2D boxCorner;
    public Vector2 lastPosition;
    public Vector2 lastSize;

    public LayerMask playerCollide;

    //public Snaily playerScriptSnaily;

    // Movement control vars
    // Any var tagged with "I" (as in "item") follows this scheme: -1 = always, -2 = never, any item ID = item-bound
    public int defaultGravityDir = DIR_FLOOR; // --------------------- Determines the default direction gravity pulls the player
    public int[] canJump = new int[] { -1 }; // -------------------- I Determines if the player can jump
    public int[] canSwapGravity = new int[] { -1 }; // ------------- I Determines if the player can change their gravity state
    public int[] retainGravityOnAirborne = new int[] { 8 }; // ----- I Determines whether or not player keeps their current gravity when in the air
    public int[] canGravityJumpOpposite = new int[] { 8 }; // ------ I Determines if the player can change their gravity mid-air to the opposite direction
    public int[] canGravityJumpAdjacent = new int[] { 8 }; // ------ I Determines if the player can change their gravity mid-air relatively left or relatively right
    public int[] shellable = new int[] { -1 }; // ------------------ I Determines if the player can retract into a shell. Item system
    public int[] hopWhileMoving = new int[] { -2 }; // ------------- I Determines if the player bounces along the ground when they move
    public float hopPower; // ---------------------------------------- The power of a walking bounce
    public int[] canRoundInnerCorners = new int[] { -1 }; // ------- I Determines if the player can round inside corners
    public int[] canRoundOuterCorners = new int[] { -1 }; // ------- I Determines if the player can round outside corners
    public int[] canRoundOppositeOuterCorners = new int[] { -1 }; // I Determines if the player can round outside corners opposite the default gravity
    public float[] runSpeed = new float[4]; // ----------------------- Contains the speed at which the player moves with each shell upgrade
    public float[] jumpPower = new float[8]; // ---------------------- Contains the player's jump power with each shell upgrade. The second half of the array assumes High Jump
    public float[] gravity = new float[4]; // ------------------------ Contains the gravity scale with each shell upgrade
    public float[] terminalVelocity = new float[4]; // --------------- Contains the player's terminal velocity with each shell upgrade
    public float[] weaponCooldowns = new float[6]; // ---------------- Contains the cooldown in seconds of each weapon. The second half of the array assumes Rapid Fire
    public float idleTimer; // --------------------------------------- Determines how long the player must remain idle before playing an idle animation
    public List<Particle> idleParticles; // -------------------------- Contains every particle used in the player's idle animation so that they can be despawned easily
    public Vector2 hitboxSize_normal; // ----------------------------- The size of the player's hitbox
    public Vector2 hitboxSize_shell; // ------------------------------ The size of the player's hitbox while in their shell
    public Vector2 hitboxOffset_normal; // --------------------------- The offset of the player's hitbox
    public Vector2 hitboxOffset_shell; // ---------------------------- The offset of the player's hitbox while in their shell
    public float unshellAdjust ; // ---------------------------------- The amount the player's position is adjusted by when unshelling near a wall
    public float coyoteTime; // -------------------------------------- How long after leaving the ground via falling the player is still able to jump for
    public float jumpBuffer; // -------------------------------------- How long after pressing the jump button the player will continue to try to jump, in case of an early press
    #endregion vars

    #region cutscene
    public void cutRegister() {
        CutsceneManager.declare("PLAYER", new CutsceneManager.Unit("dict", new Dictionary<string, CutsceneManager.Unit> {
            {"setFreeze", new CutsceneManager.Unit(setFreeze, 2, "none")},
        }));
    }

    public void cutStart() {

    }

    public void cutEnd() {

    }

    public object[] setFreeze(object[] inputs) {
        bool mode = (bool)inputs[0];
        int condition = (int)inputs[1];
        bool success = false;
        if (condition == 0) {
            PlayState.paralyzed = mode;
            success = true;
        } else if (condition == 1) {
            if (grounded) {
                PlayState.paralyzed = mode;
                success = true;
            }
        }
        return new object[1] { success };
    }

    public object[] impulse(object[] inputs) {
        Vector3 vel = new Vector3((float)inputs[0], (float)inputs[1], (float)inputs[2]);
        int condition = (int)inputs[3];
        bool success = false;
        return new object[1] { success };
    }
    #endregion cutscene

    // Start() is called at the very beginning of the script's lifetime. It's used to initialize certain variables and states for components to be in.
    public virtual void Start()
    {
        // All this does is set Snaily's components to simpler variables that can be more easily called
        anim = GetComponent<AnimationModule>();
        sprite = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        playerCollide = LayerMask.GetMask("PlayerCollide");

        PlayState.globalFunctions.RenderNewHearts();
        PlayState.globalFunctions.UpdateHearts();

        PlayState.globalFunctions.RunDebugKeys();

        PlayState.globalFunctions.UpdateMusic(-1, -1, 3);
    }

    // Update(), called less frequently (every drawn frame), actually gets most of the inputs and converts them to what they should be given any current surface state
    void Update()
    {
        if (PlayState.gameState == PlayState.GameState.game)
        {
            rb.WakeUp();

            // Making sure we have weapons
            int[] weaponIDs = new int[] { 0, 1, 2, 11, 12 };
            armed = false;
            foreach (int weapon in weaponIDs)
                if (PlayState.CheckForItem(weapon))
                    armed = true;

            // Noclip!!!
            if (PlayState.noclipMode)
            {
                if (Control.ShootHold())
                {
                    if (Control.UpPress())
                        transform.position = new Vector2(transform.position.x, transform.position.y + 16);
                    if (Control.DownPress())
                        transform.position = new Vector2(transform.position.x, transform.position.y - 16);
                    if (Control.LeftPress())
                        transform.position = new Vector2(transform.position.x - 26, transform.position.y);
                    if (Control.RightPress())
                        transform.position = new Vector2(transform.position.x + 26, transform.position.y);
                }
                else
                    transform.position = new Vector2(transform.position.x + (10 * Control.AxisX() * (Control.JumpHold() ? 2.5f : 1) * Time.deltaTime),
                        transform.position.y + (10 * Control.AxisY() * (Control.JumpHold() ? 2.5f : 1) * Time.deltaTime));
                box.enabled = false;
            }
            else if (!inDeathCutscene)
                box.enabled = true;

            // Marking the "has jumped" flag for Snail NPC 01's dialogue
            if (Control.JumpHold())
                PlayState.hasJumped = true;

            // Weapon swapping
            if (Control.Weapon1() && PlayState.CheckForItem(0))
                PlayState.globalFunctions.ChangeActiveWeapon(0);
            if (Control.Weapon2() && (PlayState.CheckForItem(1) || PlayState.CheckForItem(11)))
                PlayState.globalFunctions.ChangeActiveWeapon(1);
            if (Control.Weapon3() && (PlayState.CheckForItem(2) || PlayState.CheckForItem(12)))
                PlayState.globalFunctions.ChangeActiveWeapon(2);

            // Sleep code! Don't do anything for thirty seconds and Snaily takes a nap!
            if (PlayState.gameState == PlayState.GameState.game)
            {
                if (Control.AxisX() != 0 || Control.AxisY() != 0 || Control.JumpHold() || Control.ShootHold() || Control.StrafeHold() || Control.SpeakHold() || stunned)
                {
                    sleepTimer = idleTimer;
                    isSleeping = false;
                    foreach (Particle particle in idleParticles)
                        particle.ResetParticle();
                    idleParticles.Clear();
                }
                else
                {
                    sleepTimer = Mathf.Clamp(sleepTimer - Time.deltaTime, 0, idleTimer);
                    if (sleepTimer == 0 && !isSleeping)
                    {
                        IdleAnim();
                        isSleeping = true;
                    }
                }
                if (idleParticles.Count > 0)
                    idleParticles[0].transform.position = new Vector2(transform.position.x + 0.75f + ((gravityDir == DIR_FLOOR || gravityDir == DIR_CEILING) && facingLeft ? 0.25f : 0),
                        transform.position.y + ((gravityDir == DIR_WALL_LEFT || gravityDir == DIR_WALL_RIGHT) && facingDown ? 0.25f : 0));
            }
        }
    }

    public virtual void IdleAnim()
    {
        if (!shelled)
            ToggleShell();
        idleParticles.Add(PlayState.RequestParticle(new Vector2(transform.position.x + 0.75f, transform.position.y), "zzz"));
    }

    // LateUpdate() is called after everything else a frame needs has been handled. Here, it's used for animations
    public virtual void LateUpdate()
    {
        
    }

    #region Movement

    // This function is called once every 0.02 seconds (50 time a second) regardless of framerate. Unity requires all physics calculations to be
    // run in this function, so it's where I put movement code as it utilizes boxcasts
    public virtual void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game || PlayState.noclipMode)
            return;
    
        // To start things off, we mark our current position as the last position we took. Same with our hitbox size
        // Among other things, this is used to test for ground when we're airborne
        lastPosition = new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y);
        lastSize = box.size;
        // We also update all our boxcasts, both for the corner and in case they're misaligned with our current gravity state
        UpdateBoxcasts();
        // Next, we decrease the fire cooldown
        fireCooldown = Mathf.Clamp(fireCooldown - Time.fixedDeltaTime, 0, Mathf.Infinity);
        // Then, we reset the flag marking if Snaily is airborne and shoving their face into a wall
        againstWallFlag = false;
        // Finally, we increment the jump buffer and coyote time values if necessary
        if (Control.JumpHold())
            jumpBufferCounter += Time.fixedDeltaTime;
        else
            jumpBufferCounter = 0;
        if (!grounded)
            coyoteTimeCounter += Time.fixedDeltaTime;
        else
            coyoteTimeCounter = 0;
    
        // Next, we run different blocks of movement code based on our gravity state. They're largely the same, but are kept separate
        // so that things can stay different between them if needed, like Snaily falling off walls and ceilings without Gravity Snail
        if (!inDeathCutscene)
        {
            readIDSpeed = PlayState.CheckForItem(9) ? 3 : (PlayState.CheckForItem(8) ? 2 : (PlayState.CheckForItem(7) ? 1 : 0));
            readIDJump = readIDSpeed + (PlayState.CheckForItem(4) ? 4 : 0);
    
            switch (gravityDir)
            {
                case DIR_FLOOR:
                    CaseDown();
                    break;
                case DIR_WALL_LEFT:
                    CaseLeft();
                    break;
                case DIR_WALL_RIGHT:
                    CaseRight();
                    break;
                case DIR_CEILING:
                    CaseUp();
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

    public virtual void CaseDown()
    {
        //// We start by zeroing our relative vertical velocity if we're grounded, and our relative horizontal velocity no matter what
        //velocity.x = 0;
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
            float runSpeedValue = runSpeed[readIDSpeed] * speedMod * Time.fixedDeltaTime;
            if ((facingLeft ? boxL : boxR).distance < runSpeedValue)
            {
                againstWallFlag = true;
                velocity.x = facingLeft ? -runSpeedValue + (runSpeedValue - boxL.distance) + 0.0078125f :
                    runSpeedValue - (runSpeedValue - boxR.distance) - 0.0078125f;
                // In case the player happens to be holding the relative up/down button while the character runs face-first into a wall,
                // we check to see if climbing is possible in either direction and switch the character's gravity state
                if ((boxD.distance + boxU.distance) >= 1)
                {
                    if (!stunned)
                    {
                        if (((Control.UpHold() && !grounded) ||
                            (Control.UpHold() && grounded && CheckAbility(canRoundInnerCorners)) ||
                            (Control.DownHold() && !grounded))
                            && CheckAbility(canSwapGravity))
                        {
                            if (shelled)
                                ToggleShell();

                            transform.position = new Vector2(transform.position.x + velocity.x, transform.position.y);
                            float boxCorrection = (box.size.y - box.size.x) * 0.5f;
                            float ceilDis = boxU.distance - boxCorrection;
                            float floorDis = boxD.distance - boxCorrection;
                            SwitchSurfaceAxis();
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
                                transform.position.y + adjustment
                                );
                            SwapDir(Control.UpHold() ? DIR_CEILING : DIR_FLOOR);
                            gravityDir = facingLeft ? DIR_WALL_LEFT : DIR_WALL_RIGHT;
                            grounded = true;
                            transform.position = new Vector2(Mathf.Round(transform.position.x * 2) * 0.5f + (facingLeft ? -0.01f : 0.01f), transform.position.y);
                            if (box.size.x * 0.5f - 0.5f > 0)
                                transform.position += new Vector3((box.size.x * 0.5f - 0.5f) * (facingLeft ? 1 : -1), 0, 0);
                            UpdateBoxcasts();
                            return;
                        }
                    }
                }
            }
            else
            {
                velocity.x = facingLeft ? -runSpeedValue : runSpeedValue;
                if (CheckAbility(hopWhileMoving))
                {
                    grounded = false;
                    ungroundedViaHop = true;
                    velocity.y = hopPower;
                    lastPointBeforeHop = transform.position.y;
                }
            }
            transform.position = new Vector2(transform.position.x + velocity.x, transform.position.y);
            UpdateBoxcasts();
        }

        // Now, we perform relatively vertical checks. This mainly involves jumping and falling
        if (!grounded)
        {
            if (gravityDir != defaultGravityDir && !CheckAbility(retainGravityOnAirborne))
            {
                switch (defaultGravityDir)
                {
                    case DIR_WALL_LEFT:
                        transform.position = new Vector2(transform.position.x, transform.position.y + 0.0625f + (box.size.x - box.size.y) * 0.5f);
                        SwapDir(DIR_WALL_LEFT);
                        SwitchSurfaceAxis();
                        gravityDir = DIR_WALL_LEFT;
                        if (Control.LeftHold())
                            holdingShell = true;
                        break;
                    case DIR_WALL_RIGHT:
                        transform.position = new Vector2(transform.position.x, transform.position.y + 0.0625f + (box.size.x - box.size.y) * 0.5f);
                        SwapDir(DIR_WALL_LEFT);
                        SwitchSurfaceAxis();
                        gravityDir = DIR_WALL_LEFT;
                        if (Control.RightHold())
                            holdingShell = true;
                        break;
                    case DIR_CEILING:
                        SwapDir(DIR_CEILING);
                        gravityDir = DIR_CEILING;
                        if (Control.UpHold())
                            holdingShell = true;
                        break;
                }
            }
            else
            {
                bool pokedCeiling = false;
                velocity.y -= gravity[readIDSpeed] * gravityMod * Time.fixedDeltaTime;
                if (velocity.y > 0 && !holdingJump)
                    velocity.y = PlayState.Integrate(velocity.y, 0, 4, Time.fixedDeltaTime);
                velocity.y = Mathf.Clamp(velocity.y, terminalVelocity[readIDSpeed], Mathf.Infinity);
                if (boxD.distance != 0 && boxU.distance != 0)
                {
                    if (boxU.distance < velocity.y && Mathf.Sign(velocity.y) == 1)
                    {
                        velocity.y = boxU.distance;
                        pokedCeiling = true;
                    }
                    else if (boxD.distance < -velocity.y && Mathf.Sign(velocity.y) == -1)
                    {
                        velocity.y = -boxD.distance;
                        grounded = true;
                        ungroundedViaHop = false;
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
                                transform.position.x + ((facingLeft ? -runSpeed[readIDSpeed] : runSpeed[readIDSpeed]) * speedMod * Time.fixedDeltaTime),
                                Mathf.Floor(transform.position.y) + 0.5f);
                            i = 8;
                        }
                    }
                }
                UpdateBoxcasts();
                if (pokedCeiling)
                {
                    velocity.y = 0;
                    if (Control.UpHold() && CheckAbility(canSwapGravity))
                    {
                        gravityDir = DIR_CEILING;
                        SwapDir(DIR_CEILING);
                        grounded = true;
                        ungroundedViaHop = false;
                        holdingShell = true;
                        return;
                    }
                }
                // Gravity jumping
                if (Control.JumpHold() && !holdingJump && CheckAbility(canSwapGravity))
                {
                    if (CheckAbility(canGravityJumpOpposite) && ((Control.UpHold() && CheckAbility(canGravityJumpAdjacent)) || (!CheckAbility(canGravityJumpAdjacent))))
                    {
                        gravityDir = DIR_CEILING;
                        SwapDir(DIR_CEILING);
                        holdingShell = true;
                    }
                    else if (Control.AxisX() != 0 && CheckAbility(canGravityJumpAdjacent))
                    {
                        int newDir = Control.AxisX() == 1 ? DIR_WALL_RIGHT : DIR_WALL_LEFT;
                        gravityDir = newDir;
                        SwapDir(newDir);
                        SwitchSurfaceAxis();
                        holdingShell = true;
                    }
                }
            }
        }
        else
        {
            // Suddenly in the air when we weren't last frame
            if (boxD.distance > 0.0125f)
            {
                // Round an outside corner
                if (boxCorner.distance <= 0.0125f && CheckAbility(canRoundOuterCorners))
                {
                    // Can't round corners? Fall.
                    if (!CheckAbility(canRoundOppositeOuterCorners) && defaultGravityDir == DIR_CEILING)
                    {
                        SwapDir(DIR_CEILING);
                        gravityDir = DIR_CEILING;
                        if (Control.UpHold())
                            holdingShell = true;
                    }
                    // CAN round corners? Round that corner, you glorious little snail, you
                    else if (Control.DownHold() && Control.AxisX() == (facingLeft ? -1 : 1) && !stunned)
                    {
                        SwapDir(facingLeft ? DIR_WALL_RIGHT : DIR_WALL_LEFT);
                        SwitchSurfaceAxis();
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
                // FALL
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
        if (CheckAbility(canJump) && Control.JumpHold() && (grounded || (coyoteTimeCounter < coyoteTime) || (ungroundedViaHop && (transform.position.y > lastPointBeforeHop)))
            && (!holdingJump || (jumpBufferCounter < jumpBuffer && velocity.y < 0)) && boxU.distance > 0.95f && !PlayState.paralyzed)
        {
            if (shelled)
                ToggleShell();
            grounded = false;
            if (gravityDir != defaultGravityDir)
            {
                if (CheckAbility(retainGravityOnAirborne))
                    velocity.y = jumpPower[readIDJump] * jumpMod * Time.deltaTime;
                else
                {
                    switch (defaultGravityDir)
                    {
                        case DIR_WALL_LEFT:
                            transform.position = new Vector2(transform.position.x, transform.position.y + 0.0625f + (box.size.x - box.size.y) * 0.5f);
                            SwapDir(DIR_WALL_LEFT);
                            SwitchSurfaceAxis();
                            gravityDir = DIR_WALL_LEFT;
                            if (Control.LeftHold())
                                holdingShell = true;
                            break;
                        case DIR_WALL_RIGHT:
                            transform.position = new Vector2(transform.position.x, transform.position.y + 0.0625f + (box.size.x - box.size.y) * 0.5f);
                            SwapDir(DIR_WALL_LEFT);
                            SwitchSurfaceAxis();
                            gravityDir = DIR_WALL_LEFT;
                            if (Control.RightHold())
                                holdingShell = true;
                            break;
                        case DIR_FLOOR:
                            SwapDir(DIR_FLOOR);
                            gravityDir = DIR_FLOOR;
                            if (Control.DownHold())
                                holdingShell = true;
                            break;
                    }
                }
            }
            else
                velocity.y = jumpPower[readIDJump] * jumpMod * Time.deltaTime;
            PlayState.PlaySound("Jump");
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
            !holdingShell && !PlayState.paralyzed && CheckAbility(shellable))
        {
            ToggleShell();
            holdingShell = true;
        }
        else if (!holdingShell && Control.DownHold())
            holdingShell = true;
        if (holdingShell && !Control.DownHold())
            holdingShell = false;
        if (PlayState.IsTileSolid(transform.position))
            transform.position = new Vector2(transform.position.x, transform.position.y + 1);
    }

    public virtual void CaseLeft()
    {
        //// We start by zeroing our relative vertical velocity if we're grounded, and our relative horizontal velocity no matter what
        //velocity.y = 0;
        if (grounded)
            velocity.x = 0;

        // From here, we perform relatively horizontal movement checks to move, stop if we hit a wall, and allow for climbing
        if (Control.AxisY() != 0 && !Control.StrafeHold() && !PlayState.paralyzed)
        {
            if (shelled)
            {
                if (Control.AxisY() == (facingDown ? 1 : -1))
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
            float runSpeedValue = runSpeed[readIDSpeed] * speedMod * Time.fixedDeltaTime;
            if ((facingDown ? boxD : boxU).distance < runSpeedValue)
            {
                againstWallFlag = true;
                velocity.y = facingDown ? -runSpeedValue + (runSpeedValue - boxD.distance) + 0.0078125f :
                    runSpeedValue - (runSpeedValue - boxU.distance) - 0.0078125f;
                // In case the player happens to be holding the relative up/down button while the character runs face-first into a wall,
                // we check to see if climbing is possible in either direction and switch the character's gravity state
                if ((boxL.distance + boxR.distance) >= 1)
                {
                    if (!stunned)
                    {
                        if (((Control.RightHold() && !grounded) ||
                            (Control.RightHold() && grounded && CheckAbility(canRoundInnerCorners)) ||
                            (Control.LeftHold() && !grounded))
                            && CheckAbility(canSwapGravity))
                        {
                            if (shelled)
                                ToggleShell();

                            transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
                            float boxCorrection = (box.size.x - box.size.y) * 0.5f;
                            float ceilDis = boxR.distance - boxCorrection;
                            float floorDis = boxL.distance - boxCorrection;
                            SwitchSurfaceAxis();
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
                                transform.position.x + adjustment,
                                transform.position.y + (facingDown ? boxCorrection : -boxCorrection)
                                );
                            SwapDir(Control.RightHold() ? DIR_WALL_RIGHT : DIR_WALL_LEFT);
                            gravityDir = facingDown ? DIR_FLOOR : DIR_CEILING;
                            grounded = true;
                            transform.position = new Vector2(transform.position.x, Mathf.Round(transform.position.y * 2) * 0.5f + (facingDown ? -0.01f : 0.01f));
                            if (box.size.y * 0.5f - 0.5f > 0)
                                transform.position += new Vector3(0, (box.size.y * 0.5f - 0.5f) * (facingDown ? 1 : -1), 0);
                            UpdateBoxcasts();
                            return;
                        }
                    }
                }
            }
            else
            {
                velocity.y = facingDown ? -runSpeedValue : runSpeedValue;
                if (CheckAbility(hopWhileMoving))
                {
                    grounded = false;
                    ungroundedViaHop = true;
                    velocity.x = hopPower;
                    lastPointBeforeHop = transform.position.x;
                }
            }
            transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
            UpdateBoxcasts();
        }

        // Now, we perform relatively vertical checks. This mainly involves jumping and falling
        if (!grounded)
        {
            if (gravityDir != defaultGravityDir && !CheckAbility(retainGravityOnAirborne))
            {
                switch (defaultGravityDir)
                {
                    case DIR_FLOOR:
                        transform.position = new Vector2(transform.position.x + 0.0625f + (box.size.x - box.size.y) * 0.5f, transform.position.y);
                        SwapDir(DIR_FLOOR);
                        SwitchSurfaceAxis();
                        gravityDir = DIR_FLOOR;
                        if (Control.DownHold())
                            holdingShell = true;
                        break;
                    case DIR_CEILING:
                        transform.position = new Vector2(transform.position.x + 0.0625f + (box.size.x - box.size.y) * 0.5f, transform.position.y);
                        SwapDir(DIR_CEILING);
                        SwitchSurfaceAxis();
                        gravityDir = DIR_CEILING;
                        if (Control.UpHold())
                            holdingShell = true;
                        break;
                    case DIR_WALL_RIGHT:
                        SwapDir(DIR_WALL_RIGHT);
                        gravityDir = DIR_WALL_RIGHT;
                        if (Control.RightHold())
                            holdingShell = true;
                        break;
                }
            }
            else
            {
                bool pokedCeiling = false;
                velocity.x -= gravity[readIDSpeed] * gravityMod * Time.fixedDeltaTime;
                if (velocity.x > 0 && !holdingJump)
                    velocity.x = PlayState.Integrate(velocity.x, 0, 4, Time.fixedDeltaTime);
                velocity.x = Mathf.Clamp(velocity.x, terminalVelocity[readIDSpeed], Mathf.Infinity);
                if (boxL.distance != 0 && boxR.distance != 0)
                {
                    if (boxR.distance < velocity.x && Mathf.Sign(velocity.x) == 1)
                    {
                        velocity.x = boxR.distance;
                        pokedCeiling = true;
                    }
                    else if (boxL.distance < -velocity.x && Mathf.Sign(velocity.x) == -1)
                    {
                        velocity.x = -boxL.distance;
                        grounded = true;
                        ungroundedViaHop = false;
                    }
                }
                if (!againstWallFlag)
                {
                    transform.position = new Vector2(transform.position.x + velocity.x, transform.position.y);
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
                                transform.position.y + ((facingDown ? -runSpeed[readIDSpeed] : runSpeed[readIDSpeed]) * speedMod * Time.fixedDeltaTime));
                            i = 8;
                        }
                    }
                }
                UpdateBoxcasts();
                if (pokedCeiling)
                {
                    velocity.x = 0;
                    if (Control.RightHold() && CheckAbility(canSwapGravity))
                    {
                        gravityDir = DIR_WALL_RIGHT;
                        SwapDir(DIR_WALL_RIGHT);
                        grounded = true;
                        ungroundedViaHop = false;
                        holdingShell = true;
                        return;
                    }
                }
                // Gravity jumping
                if (Control.JumpHold() && !holdingJump && CheckAbility(canSwapGravity))
                {
                    if (CheckAbility(canGravityJumpOpposite) && ((Control.RightHold() && CheckAbility(canGravityJumpAdjacent)) || (!CheckAbility(canGravityJumpAdjacent))))
                    {
                        gravityDir = DIR_WALL_RIGHT;
                        SwapDir(DIR_WALL_RIGHT);
                        holdingShell = true;
                    }
                    else if (Control.AxisY() != 0 && CheckAbility(canGravityJumpAdjacent))
                    {
                        int newDir = Control.AxisY() == 1 ? DIR_CEILING : DIR_FLOOR;
                        gravityDir = newDir;
                        SwapDir(newDir);
                        SwitchSurfaceAxis();
                        holdingShell = true;
                    }
                }
            }
        }
        else
        {
            // Suddenly in the air when we weren't last frame
            if (boxL.distance > 0.0125f)
            {
                // Round an outside corner
                if (boxCorner.distance <= 0.0125f && CheckAbility(canRoundOuterCorners))
                {
                    // Can't round corners? Fall.
                    if (!CheckAbility(canRoundOppositeOuterCorners) && defaultGravityDir == DIR_WALL_RIGHT)
                    {
                        SwapDir(DIR_WALL_RIGHT);
                        gravityDir = DIR_WALL_RIGHT;
                        if (Control.RightHold())
                            holdingShell = true;
                    }
                    // CAN round corners? Round that corner, you glorious little snail, you
                    else if (Control.LeftHold() && Control.AxisY() == (facingDown ? -1 : 1) && !stunned)
                    {
                        SwapDir(facingDown ? DIR_CEILING : DIR_FLOOR);
                        SwitchSurfaceAxis();
                        RaycastHit2D wallTester = Physics2D.Raycast(
                            new Vector2(transform.position.x - 0.75f, transform.position.y + (facingDown ? -box.size.x * 0.5f : box.size.x * 0.5f)),
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
                // FALL
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
        if (CheckAbility(canJump) && Control.JumpHold() && (grounded || coyoteTimeCounter < coyoteTime || (ungroundedViaHop && transform.position.x > lastPointBeforeHop))
            && (!holdingJump || (jumpBufferCounter < jumpBuffer && velocity.x < 0)) && boxR.distance > 0.95f && !PlayState.paralyzed)
        {
            if (shelled)
                ToggleShell();
            grounded = false;
            if (gravityDir != defaultGravityDir)
            {
                if (CheckAbility(retainGravityOnAirborne))
                    velocity.x = jumpPower[readIDJump] * jumpMod * Time.deltaTime;
                else
                {
                    switch (defaultGravityDir)
                    {
                        case DIR_FLOOR:
                            transform.position = new Vector2(transform.position.x + 0.0625f + (box.size.x - box.size.y) * 0.5f, transform.position.y);
                            SwapDir(DIR_FLOOR);
                            SwitchSurfaceAxis();
                            gravityDir = DIR_FLOOR;
                            if (Control.DownHold())
                                holdingShell = true;
                            break;
                        case DIR_CEILING:
                            transform.position = new Vector2(transform.position.x + 0.0625f + (box.size.x - box.size.y) * 0.5f, transform.position.y);
                            SwapDir(DIR_CEILING);
                            SwitchSurfaceAxis();
                            gravityDir = DIR_CEILING;
                            if (Control.UpHold())
                                holdingShell = true;
                            break;
                        case DIR_WALL_RIGHT:
                            SwapDir(DIR_WALL_RIGHT);
                            gravityDir = DIR_WALL_RIGHT;
                            if (Control.RightHold())
                                holdingShell = true;
                            break;
                    }
                }
            }
            else
                velocity.x = jumpPower[readIDJump] * jumpMod * Time.deltaTime;
            PlayState.PlaySound("Jump");
        }
        if (Control.JumpHold() && !holdingJump)
            holdingJump = true;
        else if (!Control.JumpHold() && holdingJump)
            holdingJump = false;

        // Finally, we check to see if we can shell
        if (Control.LeftHold() &&
            Control.AxisY() == 0 &&
            !Control.JumpHold() &&
            !Control.ShootHold() &&
            !Control.StrafeHold() &&
            !holdingShell && !PlayState.paralyzed && CheckAbility(shellable))
        {
            ToggleShell();
            holdingShell = true;
        }
        else if (!holdingShell && Control.LeftHold())
            holdingShell = true;
        if (holdingShell && !Control.LeftHold())
            holdingShell = false;
        if (PlayState.IsTileSolid(transform.position))
            transform.position = new Vector2(transform.position.x + 1, transform.position.y);
    }

    public virtual void CaseRight()
    {
        //// We start by zeroing our relative vertical velocity if we're grounded, and our relative horizontal velocity no matter what
        //velocity.y = 0;
        if (grounded)
            velocity.x = 0;

        // From here, we perform relatively horizontal movement checks to move, stop if we hit a wall, and allow for climbing
        if (Control.AxisY() != 0 && !Control.StrafeHold() && !PlayState.paralyzed)
        {
            if (shelled)
            {
                if (Control.AxisY() == (facingDown ? 1 : -1))
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
            float runSpeedValue = runSpeed[readIDSpeed] * speedMod * Time.fixedDeltaTime;
            if ((facingDown ? boxD : boxU).distance < runSpeedValue)
            {
                againstWallFlag = true;
                velocity.y = facingDown ? -runSpeedValue + (runSpeedValue - boxD.distance) + 0.0078125f :
                    runSpeedValue - (runSpeedValue - boxU.distance) - 0.0078125f;
                // In case the player happens to be holding the relative up/down button while the character runs face-first into a wall,
                // we check to see if climbing is possible in either direction and switch the character's gravity state
                if ((boxL.distance + boxR.distance) >= 1)
                {
                    if (!stunned)
                    {
                        if (((Control.LeftHold() && !grounded) ||
                            (Control.LeftHold() && grounded && CheckAbility(canRoundInnerCorners)) ||
                            (Control.RightHold() && !grounded))
                            && CheckAbility(canSwapGravity))
                        {
                            if (shelled)
                                ToggleShell();

                            transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
                            float boxCorrection = (box.size.x - box.size.y) * 0.5f;
                            float ceilDis = boxL.distance - boxCorrection;
                            float floorDis = boxR.distance - boxCorrection;
                            SwitchSurfaceAxis();
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
                                transform.position.x - adjustment,
                                transform.position.y + (facingDown ? boxCorrection : -boxCorrection)
                                );
                            SwapDir(Control.RightHold() ? DIR_WALL_RIGHT : DIR_WALL_LEFT);
                            gravityDir = facingDown ? DIR_FLOOR : DIR_CEILING;
                            grounded = true;
                            transform.position = new Vector2(transform.position.x, Mathf.Round(transform.position.y * 2) * 0.5f + (facingDown ? -0.01f : 0.01f));
                            if (box.size.y * 0.5f - 0.5f > 0)
                                transform.position += new Vector3(0, (box.size.y * 0.5f - 0.5f) * (facingDown ? 1 : -1), 0);
                            UpdateBoxcasts();
                            return;
                        }
                    }
                }
            }
            else
            {
                velocity.y = facingDown ? -runSpeedValue : runSpeedValue;
                if (CheckAbility(hopWhileMoving))
                {
                    grounded = false;
                    ungroundedViaHop = true;
                    velocity.x = -hopPower;
                    lastPointBeforeHop = transform.position.x;
                }
            }
            transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
            UpdateBoxcasts();
        }

        // Now, we perform relatively vertical checks. This mainly involves jumping and falling
        if (!grounded)
        {
            if (gravityDir != defaultGravityDir && !CheckAbility(retainGravityOnAirborne))
            {
                switch (defaultGravityDir)
                {
                    case DIR_FLOOR:
                        transform.position = new Vector2(transform.position.x - 0.0625f - (box.size.x - box.size.y) * 0.5f, transform.position.y);
                        SwapDir(DIR_FLOOR);
                        SwitchSurfaceAxis();
                        gravityDir = DIR_FLOOR;
                        if (Control.DownHold())
                            holdingShell = true;
                        break;
                    case DIR_CEILING:
                        transform.position = new Vector2(transform.position.x - 0.0625f - (box.size.x - box.size.y) * 0.5f, transform.position.y);
                        SwapDir(DIR_CEILING);
                        SwitchSurfaceAxis();
                        gravityDir = DIR_CEILING;
                        if (Control.UpHold())
                            holdingShell = true;
                        break;
                    case DIR_WALL_LEFT:
                        SwapDir(DIR_WALL_LEFT);
                        gravityDir = DIR_WALL_LEFT;
                        if (Control.LeftHold())
                            holdingShell = true;
                        break;
                }
            }
            else
            {
                bool pokedCeiling = false;
                velocity.x += gravity[readIDSpeed] * gravityMod * Time.fixedDeltaTime;
                if (velocity.x < 0 && !holdingJump)
                    velocity.x = PlayState.Integrate(velocity.x, 0, 4, Time.fixedDeltaTime);
                velocity.x = Mathf.Clamp(velocity.x, -Mathf.Infinity, -terminalVelocity[readIDSpeed]);
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
                        ungroundedViaHop = false;
                    }
                }
                if (!againstWallFlag)
                {
                    transform.position = new Vector2(transform.position.x + velocity.x, transform.position.y);
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
                                transform.position.y + ((facingDown ? -runSpeed[readIDSpeed] : runSpeed[readIDSpeed]) * speedMod * Time.fixedDeltaTime));
                            i = 8;
                        }
                    }
                }
                UpdateBoxcasts();
                if (pokedCeiling)
                {
                    velocity.x = 0;
                    if (Control.LeftHold() && CheckAbility(canSwapGravity))
                    {
                        gravityDir = DIR_WALL_LEFT;
                        SwapDir(DIR_WALL_LEFT);
                        grounded = true;
                        ungroundedViaHop = false;
                        holdingShell = true;
                        return;
                    }
                }
                // Gravity jumping
                if (Control.JumpHold() && !holdingJump && CheckAbility(canSwapGravity))
                {
                    if (CheckAbility(canGravityJumpOpposite) && ((Control.LeftHold() && CheckAbility(canGravityJumpAdjacent)) || (!CheckAbility(canGravityJumpAdjacent))))
                    {
                        gravityDir = DIR_WALL_LEFT;
                        SwapDir(DIR_WALL_LEFT);
                        holdingShell = true;
                    }
                    else if (Control.AxisY() != 0 && CheckAbility(canGravityJumpAdjacent))
                    {
                        int newDir = Control.AxisY() == 1 ? DIR_CEILING : DIR_FLOOR;
                        gravityDir = newDir;
                        SwapDir(newDir);
                        SwitchSurfaceAxis();
                        holdingShell = true;
                    }
                }
            }
        }
        else
        {
            // Suddenly in the air when we weren't last frame
            if (boxR.distance > 0.0125f)
            {
                // Round an outside corner
                if (boxCorner.distance <= 0.0125f && CheckAbility(canRoundOuterCorners))
                {
                    // Can't round corners? Fall.
                    if (!CheckAbility(canRoundOppositeOuterCorners) && defaultGravityDir == DIR_WALL_LEFT)
                    {
                        SwapDir(DIR_WALL_LEFT);
                        gravityDir = DIR_WALL_LEFT;
                        if (Control.LeftHold())
                            holdingShell = true;
                    }
                    // CAN round corners? Round that corner, you glorious little snail, you
                    else if (Control.RightHold() && Control.AxisY() == (facingDown ? -1 : 1) && !stunned)
                    {
                        SwapDir(facingDown ? DIR_CEILING : DIR_FLOOR);
                        SwitchSurfaceAxis();
                        RaycastHit2D wallTester = Physics2D.Raycast(
                            new Vector2(transform.position.x + 0.75f, transform.position.y + (facingDown ? -box.size.x * 0.5f : box.size.x * 0.5f)),
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
                // FALL
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
        if (CheckAbility(canJump) && Control.JumpHold() && (grounded || coyoteTimeCounter < coyoteTime || (ungroundedViaHop && transform.position.x < lastPointBeforeHop))
            && (!holdingJump || (jumpBufferCounter < jumpBuffer && velocity.x > 0)) && boxL.distance > 0.95f && !PlayState.paralyzed)
        {
            if (shelled)
                ToggleShell();
            grounded = false;
            if (gravityDir != defaultGravityDir)
            {
                if (CheckAbility(retainGravityOnAirborne))
                    velocity.x = -jumpPower[readIDJump] * jumpMod * Time.deltaTime;
                else
                {
                    switch (defaultGravityDir)
                    {
                        case DIR_FLOOR:
                            transform.position = new Vector2(transform.position.x - 0.0625f - (box.size.x - box.size.y) * 0.5f, transform.position.y);
                            SwapDir(DIR_FLOOR);
                            SwitchSurfaceAxis();
                            gravityDir = DIR_FLOOR;
                            if (Control.DownHold())
                                holdingShell = true;
                            break;
                        case DIR_CEILING:
                            transform.position = new Vector2(transform.position.x - 0.0625f - (box.size.x - box.size.y) * 0.5f, transform.position.y);
                            SwapDir(DIR_CEILING);
                            SwitchSurfaceAxis();
                            gravityDir = DIR_CEILING;
                            if (Control.UpHold())
                                holdingShell = true;
                            break;
                        case DIR_WALL_LEFT:
                            SwapDir(DIR_WALL_LEFT);
                            gravityDir = DIR_WALL_LEFT;
                            if (Control.LeftHold())
                                holdingShell = true;
                            break;
                    }
                }
            }
            else
                velocity.x = -jumpPower[readIDJump] * jumpMod * Time.deltaTime;
            PlayState.PlaySound("Jump");
        }
        if (Control.JumpHold() && !holdingJump)
            holdingJump = true;
        else if (!Control.JumpHold() && holdingJump)
            holdingJump = false;

        // Finally, we check to see if we can shell
        if (Control.RightHold() &&
            Control.AxisY() == 0 &&
            !Control.JumpHold() &&
            !Control.ShootHold() &&
            !Control.StrafeHold() &&
            !holdingShell && !PlayState.paralyzed && CheckAbility(shellable))
        {
            ToggleShell();
            holdingShell = true;
        }
        else if (!holdingShell && Control.RightHold())
            holdingShell = true;
        if (holdingShell && !Control.RightHold())
            holdingShell = false;
        if (PlayState.IsTileSolid(transform.position))
            transform.position = new Vector2(transform.position.x - 1, transform.position.y);
    }

    public virtual void CaseUp()
    {
        //// We start by zeroing our relative vertical velocity if we're grounded, and our relative horizontal velocity no matter what
        //velocity.x = 0;
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
            float runSpeedValue = runSpeed[readIDSpeed] * speedMod * Time.fixedDeltaTime;
            if ((facingLeft ? boxL : boxR).distance < runSpeedValue)
            {
                againstWallFlag = true;
                velocity.x = facingLeft ? -runSpeedValue + (runSpeedValue - boxL.distance) + 0.0078125f :
                    runSpeedValue - (runSpeedValue - boxR.distance) - 0.0078125f;
                // In case the player happens to be holding the relative up/down button while the character runs face-first into a wall,
                // we check to see if climbing is possible in either direction and switch the character's gravity state
                if ((boxD.distance + boxU.distance) >= 1)
                {
                    if (!stunned)
                    {
                        if (((Control.DownHold() && !grounded) ||
                            (Control.DownHold() && grounded && CheckAbility(canRoundInnerCorners)) ||
                            (Control.UpHold() && !grounded))
                            && CheckAbility(canSwapGravity))
                        {
                            if (shelled)
                                ToggleShell();

                            transform.position = new Vector2(transform.position.x + velocity.x, transform.position.y);
                            float boxCorrection = (box.size.y - box.size.x) * 0.5f;
                            float ceilDis = boxD.distance - boxCorrection;
                            float floorDis = boxU.distance - boxCorrection;
                            SwitchSurfaceAxis();
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
                            transform.position = new Vector2(Mathf.Round(transform.position.x * 2) * 0.5f + (facingLeft ? -0.01f : 0.01f), transform.position.y);
                            if (box.size.x * 0.5f - 0.5f > 0)
                                transform.position += new Vector3((box.size.x * 0.5f - 0.5f) * (facingLeft ? 1 : -1), 0, 0);
                            UpdateBoxcasts();
                            return;
                        }
                    }
                }
            }
            else
            {
                velocity.x = facingLeft ? -runSpeedValue : runSpeedValue;
                if (CheckAbility(hopWhileMoving))
                {
                    grounded = false;
                    ungroundedViaHop = true;
                    velocity.y = -hopPower;
                    lastPointBeforeHop = transform.position.y;
                }
            }
            transform.position = new Vector2(transform.position.x + velocity.x, transform.position.y);
            UpdateBoxcasts();
        }

        // Now, we perform relatively vertical checks. This mainly involves jumping and falling
        if (!grounded)
        {
            if (gravityDir != defaultGravityDir && !CheckAbility(retainGravityOnAirborne))
            {
                switch (defaultGravityDir)
                {
                    case DIR_WALL_LEFT:
                        transform.position = new Vector2(transform.position.x, transform.position.y - 0.0625f - (box.size.x - box.size.y) * 0.5f);
                        SwapDir(DIR_WALL_LEFT);
                        SwitchSurfaceAxis();
                        gravityDir = DIR_WALL_LEFT;
                        if (Control.LeftHold())
                            holdingShell = true;
                        break;
                    case DIR_WALL_RIGHT:
                        transform.position = new Vector2(transform.position.x, transform.position.y - 0.0625f - (box.size.x - box.size.y) * 0.5f);
                        SwapDir(DIR_WALL_LEFT);
                        SwitchSurfaceAxis();
                        gravityDir = DIR_WALL_LEFT;
                        if (Control.RightHold())
                            holdingShell = true;
                        break;
                    case DIR_FLOOR:
                        SwapDir(DIR_FLOOR);
                        gravityDir = DIR_FLOOR;
                        if (Control.DownHold())
                            holdingShell = true;
                        break;
                }
            }
            else
            {
                bool pokedCeiling = false;
                velocity.y += gravity[readIDSpeed] * gravityMod * Time.fixedDeltaTime;
                if (velocity.y < 0 && !holdingJump)
                    velocity.y = PlayState.Integrate(velocity.y, 0, 4, Time.fixedDeltaTime);
                velocity.y = Mathf.Clamp(velocity.y, -Mathf.Infinity, -terminalVelocity[readIDSpeed]);
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
                        ungroundedViaHop = false;
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
                                transform.position.x + ((facingLeft ? -runSpeed[readIDSpeed] : runSpeed[readIDSpeed]) * speedMod * Time.fixedDeltaTime),
                                Mathf.Floor(transform.position.y) + 0.5f);
                            i = 8;
                        }
                    }
                }
                UpdateBoxcasts();
                if (pokedCeiling)
                {
                    velocity.y = 0;
                    if (Control.DownHold() && CheckAbility(canSwapGravity))
                    {
                        gravityDir = DIR_FLOOR;
                        SwapDir(DIR_FLOOR);
                        grounded = true;
                        ungroundedViaHop = false;
                        holdingShell = true;
                        return;
                    }
                }
                // Gravity jumping
                if (Control.JumpHold() && !holdingJump && CheckAbility(canSwapGravity))
                {
                    if (CheckAbility(canGravityJumpOpposite) && ((Control.DownHold() && CheckAbility(canGravityJumpAdjacent)) || (!CheckAbility(canGravityJumpAdjacent))))
                    {
                        gravityDir = DIR_FLOOR;
                        SwapDir(DIR_FLOOR);
                        holdingShell = true;
                    }
                    else if (Control.AxisX() != 0 && CheckAbility(canGravityJumpAdjacent))
                    {
                        int newDir = Control.AxisX() == 1 ? DIR_WALL_RIGHT : DIR_WALL_LEFT;
                        gravityDir = newDir;
                        SwapDir(newDir);
                        SwitchSurfaceAxis();
                        holdingShell = true;
                    }
                }
            }
        }
        else
        {
            // Suddenly in the air when we weren't last frame
            if (boxU.distance > 0.0125f)
            {
                // Round an outside corner
                if (boxCorner.distance <= 0.0125f && CheckAbility(canRoundOuterCorners))
                {
                    // Can't round corners? Fall.
                    if (!CheckAbility(canRoundOppositeOuterCorners) && defaultGravityDir == DIR_FLOOR)
                    {
                        SwapDir(DIR_FLOOR);
                        gravityDir = DIR_FLOOR;
                        if (Control.DownHold())
                            holdingShell = true;
                    }
                    // CAN round corners? Round that corner, you glorious little snail, you
                    else if (Control.UpHold() && Control.AxisX() == (facingLeft ? -1 : 1) && !stunned)
                    {
                        SwapDir(facingLeft ? DIR_WALL_RIGHT : DIR_WALL_LEFT);
                        SwitchSurfaceAxis();
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
                // FALL
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
        if (CheckAbility(canJump) && Control.JumpHold() && (grounded || coyoteTimeCounter < coyoteTime || (ungroundedViaHop && transform.position.y < lastPointBeforeHop))
            && (!holdingJump || (jumpBufferCounter < jumpBuffer && velocity.y > 0)) && boxD.distance > 0.95f && !PlayState.paralyzed)
        {
            if (shelled)
                ToggleShell();
            grounded = false;
            if (gravityDir != defaultGravityDir)
            {
                if (CheckAbility(retainGravityOnAirborne))
                    velocity.y = -jumpPower[readIDJump] * jumpMod * Time.deltaTime;
                else
                {
                    switch (defaultGravityDir)
                    {
                        case DIR_WALL_LEFT:
                            transform.position = new Vector2(transform.position.x, transform.position.y - 0.0625f - (box.size.x - box.size.y) * 0.5f);
                            SwapDir(DIR_WALL_LEFT);
                            SwitchSurfaceAxis();
                            gravityDir = DIR_WALL_LEFT;
                            if (Control.LeftHold())
                                holdingShell = true;
                            break;
                        case DIR_WALL_RIGHT:
                            transform.position = new Vector2(transform.position.x, transform.position.y - 0.0625f - (box.size.x - box.size.y) * 0.5f);
                            SwapDir(DIR_WALL_LEFT);
                            SwitchSurfaceAxis();
                            gravityDir = DIR_WALL_LEFT;
                            if (Control.RightHold())
                                holdingShell = true;
                            break;
                        case DIR_FLOOR:
                            SwapDir(DIR_FLOOR);
                            gravityDir = DIR_FLOOR;
                            if (Control.DownHold())
                                holdingShell = true;
                            break;
                    }
                }
            }
            else
                velocity.y = -jumpPower[readIDJump] * jumpMod * Time.deltaTime;
            PlayState.PlaySound("Jump");
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
            !holdingShell && !PlayState.paralyzed && CheckAbility(shellable))
        {
            ToggleShell();
            holdingShell = true;
        }
        else if (!holdingShell && Control.UpHold())
            holdingShell = true;
        if (holdingShell && !Control.UpHold())
            holdingShell = false;
        if (PlayState.IsTileSolid(transform.position))
            transform.position = new Vector2(transform.position.x, transform.position.y - 1);
    }

    private bool CheckAbility(int[] ability)
    {
        for (int i = 0; i < ability.Length; i++)
        {
            if (ability[i] == -1)
                return true;
            else if (ability[i] == -2)
                return false;
            else if (PlayState.itemCollection[ability[i]] == 1)
                return true;
        }
        return false;
    }

    private int GetOppositeDir(int direction)
    {
        return direction switch
        {
            DIR_WALL_LEFT => DIR_WALL_RIGHT,
            DIR_WALL_RIGHT => DIR_WALL_LEFT,
            DIR_CEILING => DIR_FLOOR,
            _ => DIR_CEILING,
        };
    }

    #endregion Movement

    #region Player utilities

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
        if (gravityDir == DIR_CEILING)
            cornerTestDir = Vector2.up;
        else if (gravityDir == DIR_WALL_LEFT)
            cornerTestDir = Vector2.left;
        else if (gravityDir == DIR_WALL_RIGHT)
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
                break;
            case DIR_WALL_LEFT:
                facingLeft = true;
                break;
            case DIR_WALL_RIGHT:
                facingLeft = false;
                break;
            case DIR_CEILING:
                facingDown = false;
                break;
        }
    }

    // This function is used to swap the player character between the ground/ceiling state and the wall state and vice versa
    private void SwitchSurfaceAxis()
    {
        axisFlag = !axisFlag;
        box.size = new Vector2(box.size.y, box.size.x);
        box.offset = new Vector2(Mathf.Abs(box.offset.y) * (facingLeft ? 1 : -1), Mathf.Abs(box.offset.x) * (facingDown ? 1 : -1));
        UpdateBoxcasts();
    }

    // This function is called whenever a shelled character asks to enter/exit their shell
    public virtual void ToggleShell()
    {
        if (stunned && !shelled)
            return;
        //if (shelled)
        //{
        //    box.offset = Vector2.zero;
        //    if (gravityDir == DIR_WALL_LEFT || gravityDir == DIR_WALL_RIGHT)
        //        box.size = new Vector2(hitboxSize_normal.y, hitboxSize_normal.x);
        //    else
        //        box.size = hitboxSize_normal;
        //}
        //else
        //{
        //    if (gravityDir == DIR_WALL_LEFT || gravityDir == DIR_WALL_RIGHT)
        //    {
        //        //if (facingDown)
        //        //    box.offset = new Vector2(hitboxOffset_shell.y, hitboxOffset_shell.x);
        //        //else
        //        //    box.offset = new Vector2(0, -hitboxOffset_shell.x);
        //        box.size = new Vector2(hitboxSize_shell.y, hitboxSize_shell.x);
        //    }
        //    else
        //    {
        //        if (facingLeft)
        //            box.offset = new Vector2(HITBOX_SHELL_OFFSET, 0);
        //        else
        //            box.offset = new Vector2(-HITBOX_SHELL_OFFSET, 0);
        //        box.size = new Vector2(HITBOX_SHELL_X, HITBOX_SHELL_Y);
        //    }
        //    PlayState.PlaySound("Shell");
        //}
        if (shelled)
        {
            if (gravityDir == DIR_WALL_LEFT || gravityDir == DIR_WALL_RIGHT)
            {
                if (boxD.distance < unshellAdjust && boxU.distance < unshellAdjust)
                    return;
                if (boxD.distance > unshellAdjust && boxU.distance < unshellAdjust)
                    transform.position = new Vector2(transform.position.x,
                        transform.position.y - (0.675f - boxU.distance - (facingDown ? 0.25f : 0)));
                else if (boxD.distance < unshellAdjust && boxU.distance > unshellAdjust)
                    transform.position = new Vector2(transform.position.x,
                        transform.position.y + (0.675f - boxD.distance - (facingDown ? 0 : 0.25f)));

                box.offset = new Vector2(hitboxOffset_normal.y * (facingLeft ? -1 : 1), hitboxOffset_normal.x * (facingDown ? -1 : 1));
                box.size = new Vector2(hitboxSize_normal.y, hitboxSize_normal.x);
            }
            else
            {
                if (boxL.distance < unshellAdjust && boxR.distance < unshellAdjust)
                    return;
                if (boxL.distance > unshellAdjust && boxR.distance < unshellAdjust)
                    transform.position = new Vector2(transform.position.x - (0.675f - boxR.distance - (facingLeft ? 0.25f : 0)),
                        transform.position.y);
                else if (boxL.distance < unshellAdjust && boxR.distance > unshellAdjust)
                    transform.position = new Vector2(transform.position.x + (0.675f - boxL.distance - (facingLeft ? 0 : 0.25f)),
                        transform.position.y);

                box.offset = new Vector2(hitboxOffset_normal.x * (facingLeft ? -1 : 1), hitboxOffset_normal.y * (facingDown ? -1 : 1));
                box.size = hitboxSize_normal;
            }
        }
        else
        {
            if (gravityDir == DIR_WALL_LEFT || gravityDir == DIR_WALL_RIGHT)
            {
                box.offset = new Vector2(hitboxOffset_shell.y * (facingLeft ? -1 : 1), hitboxOffset_shell.x * (facingDown ? -1 : 1));
                box.size = new Vector2(hitboxSize_shell.y, hitboxSize_shell.x);
            }
            else
            {
                box.offset = new Vector2(hitboxOffset_shell.x * (facingLeft ? -1 : 1), hitboxOffset_shell.y * (facingDown ? -1 : 1));
                box.size = hitboxSize_shell;
            }
            PlayState.PlaySound("Shell");
        }
        shelled = !shelled;
        UpdateBoxcasts();
    }

    // This function handles activation of projectiles when the player presses either shoot button
    public virtual void Shoot()
    {
        if (fireCooldown == 0 && armed && !PlayState.paralyzed)
        {
            Vector2 inputDir = new Vector2(Control.AxisX(), Control.AxisY());
            int type = selectedWeapon + (PlayState.CheckForItem("Devastator") ? 3 : 0);
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

            if (type == 1 && grounded)
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
            if (!PlayState.globalFunctions.playerBulletPool.transform.GetChild(bulletID).GetComponent<Bullet>().isActive)
            {
                PlayState.globalFunctions.playerBulletPool.transform.GetChild(bulletID).GetComponent<Bullet>().Shoot(type, dir);
                bulletID++;
                if (bulletID >= PlayState.globalFunctions.playerBulletPool.transform.childCount)
                    bulletID = 0;
                int fireRateIndex = type - 1 - (type > 3 ? 3 : 0) + (PlayState.CheckForItem("Rapid Fire") ? 3 : 0);
                fireCooldown = weaponCooldowns[fireRateIndex];
                PlayState.PlaySound(type switch { 2 => "ShotBoomerang", 3 => "ShotRainbow", 4 => "ShotRainbow", 5 => "ShotRainbow", 6 => "ShotRainbow", _ => "ShotPeashooter", });
            }
        }
    }

    public void HitFor(int damage)
    {
        if (stunned || inDeathCutscene)
            return;

        if (damage < 0)
        {
            health = Mathf.Clamp(health - damage, 0, maxHealth);
            PlayState.globalFunctions.UpdateHearts();
        }
        else
        {
            if (health - damage <= 0)
                StartCoroutine(nameof(DieAndRespawn));
            else
                StartCoroutine(StunTimer(damage));
        }
    }

    public IEnumerator StunTimer(int damage)
    {
        if (shelled && PlayState.CheckForItem("Shell Shield"))
            PlayState.PlaySound("Ping");
        else
        {
            health = Mathf.RoundToInt(Mathf.Clamp(health - damage, 0, Mathf.Infinity));
            PlayState.globalFunctions.UpdateHearts();
            PlayState.PlaySound("Hurt");
        }
        stunned = true;
        float timer = 0;
        while (timer < 1)
        {
            if (PlayState.gameState == PlayState.GameState.game)
            {
                sprite.enabled = !sprite.enabled;
                timer += Time.deltaTime;
            }
            else if (PlayState.gameState == PlayState.GameState.menu)
                timer = 1;
            yield return new WaitForEndOfFrame();
        }
        if (PlayState.gameState != PlayState.GameState.menu)
        {
            sprite.enabled = true;
            stunned = false;
        }
    }

    public virtual IEnumerator DieAndRespawn()
    {
        if (shelled)
            ToggleShell();
        health = 0;
        PlayState.globalFunctions.UpdateHearts();
        inDeathCutscene = true;
        box.enabled = false;
        PlayState.paralyzed = true;
        PlayState.PlaySound("Death");
        float timer = 0;
        bool hasStartedTransition = false;
        Vector3 fallDir = new Vector3(0.125f, 0.35f, 0);
        if (!facingLeft)
            fallDir = new Vector3(-0.125f, 0.35f, 0);
        while ((timer < 1.6f && PlayState.quickDeathTransition) || (timer < 2 && !PlayState.quickDeathTransition))
        {
            transform.position += fallDir;
            fallDir = new Vector3(fallDir.x, Mathf.Clamp(fallDir.y - 0.025f, -0.5f, Mathf.Infinity), 0);
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
            if (timer > 1 && !hasStartedTransition)
            {
                hasStartedTransition = true;
                PlayState.ScreenFlash("Death Transition");
            }
        }
        yield return new WaitForEndOfFrame();
        if (PlayState.positionOfLastRoom == PlayState.positionOfLastSave)
        {
            Transform deathLocation = PlayState.roomTriggerParent.transform.GetChild((int)PlayState.positionOfLastRoom.x).GetChild((int)PlayState.positionOfLastRoom.y);
            deathLocation.GetComponent<Collider2D>().enabled = true;
            deathLocation.GetComponent<RoomTrigger>().active = true;
            deathLocation.GetComponent<RoomTrigger>().DespawnEverything();
        }
        PlayState.ToggleBossfightState(false, 0, true);
        transform.position = PlayState.respawnCoords;
        inDeathCutscene = false;
        box.enabled = true;
        PlayState.paralyzed = false;
        health = maxHealth;
        PlayState.globalFunctions.UpdateHearts();
        yield return new WaitForEndOfFrame();
        PlayState.ScreenFlash("Room Transition");
    }

    public void RemoteJump(float jumpPower)
    {
        //switch (PlayState.currentCharacter)
        //{
        //    default:
        //    case "Snaily":
        //        playerScriptSnaily.RemoteJump(jumpPower);
        //        break;
        //    case "Sluggy":
        //        break;
        //    case "Upside":
        //        break;
        //    case "Leggy":
        //        break;
        //    case "Blobby":
        //        break;
        //    case "Leechy":
        //        break;
        //}
    }

    public void RemoteGravity(int direction)
    {
        //switch (PlayState.currentCharacter)
        //{
        //    default:
        //    case "Snaily":
        //        playerScriptSnaily.RemoteGravity(direction);
        //        break;
        //    case "Sluggy":
        //        break;
        //    case "Upside":
        //        break;
        //    case "Leggy":
        //        break;
        //    case "Blobby":
        //        break;
        //    case "Leechy":
        //        break;
        //}
    }

    #endregion Player utilities
}
