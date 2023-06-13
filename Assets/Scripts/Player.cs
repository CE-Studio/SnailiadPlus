using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, ICutsceneObject {
    #region vars
    public enum Dirs { Floor, WallL, WallR, Ceiling };
    public Dirs gravityDir = Dirs.Floor;

    public const int MAX_DIST_CASTS = 4;
    public const int THIN_TUNNEL_ENTRANCE_STEPS = 16;
    public const float DIST_CAST_EDGE_BUFFER = 0;
    public int currentSurface = 0;
    public bool facingLeft = false;
    public bool facingDown = false;
    public int selectedWeapon = 0;
    public bool armed;
    public int health = 12;
    public int maxHealth = 12;
    public bool stunned = false;
    public bool inDeathCutscene = false;
    public bool underwater = false;
    public Vector2 velocity = Vector2.zero;
    public bool grounded;
    public bool shelled;
    public bool ungroundedViaHop;
    public bool groundedOnPlatform;
    public float lastDistance;

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

    public Vector2 lastPosition;
    public Vector2 lastSize;
    public List<Collider2D> collisions;
    public Collider2D lastCollision;

    public LayerMask playerCollide;

    // Movement control vars
    // Any var tagged with "I" (as in "item") follows this scheme: -1 = always, -2 = never, any item ID = item-bound
    // Item scheme variables can contain multiple values, denoting an assortment of items that can fulfill a given check
    // Example: setting hopWhileMoving to { { 4, 7 }, 8 } will make Snaily hop along the ground if they find either (High Jump AND Ice Snail) OR Gravity Snail
    public Dirs defaultGravityDir; // -------------------------------- Determines the default direction gravity pulls the player
    public int[][] canJump; // ------------------------------------- I Determines if the player can jump
    public int[][] canSwapGravity; // ------------------------------ I Determines if the player can change their gravity state
    public int[][] retainGravityOnAirborne; // --------------------- I Determines whether or not player keeps their current gravity when in the air
    public int[][] canGravityJumpOpposite; // ---------------------- I Determines if the player can change their gravity mid-air to the opposite direction
    public int[][] canGravityJumpAdjacent; // ---------------------- I Determines if the player can change their gravity mid-air relatively left or relatively right
    public int[][] shellable; // ----------------------------------- I Determines if the player can retract into a shell
    public int[][] hopWhileMoving; // ------------------------------ I Determines if the player bounces along the ground when they move
    public float hopPower; // ---------------------------------------- The power of a walking bounce
    public int[][] canRoundInnerCorners; // ------------------------ I Determines if the player can round inside corners
    public int[][] canRoundOuterCorners; // ------------------------ I Determines if the player can round outside corners
    public int[][] canRoundOppositeOuterCorners; // ---------------- I Determines if the player can round outside corners opposite the default gravity
    public int[][] stickToWallsWhenHurt; // ------------------------ I Determines if the player returns to their default gravity when hit by an enemy, bullet, or hazard or not
    public float[] runSpeed; // -------------------------------------- Contains the speed at which the player moves with each shell upgrade
    public float[] jumpPower; // ------------------------------------- Contains the player's jump power with each shell upgrade. The second half of the array assumes High Jump
    public float[] gravity; // --------------------------------------- Contains the gravity scale with each shell upgrade
    public float[] terminalVelocity; // ------------------------------ Contains the player's terminal velocity with each shell upgrade
    public float[] jumpFloatiness; // -------------------------------- Contains how floaty the player's jump is when the jump button is held with each shell upgrade + High Jump
    public float[] weaponCooldowns; // ------------------------------- Contains the cooldown in seconds of each weapon. The second half of the array assumes Rapid Fire
    public int applyRapidFireMultiplier; // -------------------------- Determines if collecting Rapid Fire affects bullet velocity
    public float idleTimer; // --------------------------------------- Determines how long the player must remain idle before playing an idle animation
    public List<Particle> idleParticles; // -------------------------- Contains every particle used in the player's idle animation so that they can be despawned easily
    public Vector2 hitboxSize_normal; // ----------------------------- The size of the player's hitbox
    public Vector2 hitboxSize_shell; // ------------------------------ The size of the player's hitbox while in their shell
    public Vector2 hitboxOffset_normal; // --------------------------- The offset of the player's hitbox
    public Vector2 hitboxOffset_shell; // ---------------------------- The offset of the player's hitbox while in their shell
    public float unshellAdjust; // ----------------------------------- The amount the player's position is adjusted by when unshelling near a wall
    public float shellTurnaroundAdjust; // --------------------------- The amount the player's position is adjusted when turning around in the air while shelled
    public float coyoteTime; // -------------------------------------- How long after leaving the ground via falling the player is still able to jump for
    public float jumpBuffer; // -------------------------------------- How long after pressing the jump button the player will continue to try to jump, in case of an early press
    #endregion vars

    #region cutscene
    public void cutRegister() {
        CutsceneManager.declare("PLAYER", new CutsceneManager.Unit("dict", new Dictionary<string, CutsceneManager.Unit> {
            {"setFreeze", new CutsceneManager.Unit(setFreeze, 2, "none")},
            {"impulse", new CutsceneManager.Unit(impulse, 3, "none")},
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
        Vector2 vel = new Vector2((float)inputs[0], (float)inputs[1]);
        int condition = (int)inputs[2];
        bool success = false;
        if (condition == 0) {
            velocity += vel;
            success = true;
        } else if (condition == 1) {
            if (grounded) {
                velocity += vel;
                success = true;
            }
        }
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
                    idleParticles[0].transform.position = new Vector2(transform.position.x + 0.75f + ((gravityDir == Dirs.Floor || gravityDir == Dirs.Ceiling) && facingLeft ? 0.25f : 0),
                        transform.position.y + ((gravityDir == Dirs.WallL || gravityDir == Dirs.WallR) && facingDown ? 0.25f : 0));
            }
        }
    }

    public virtual void IdleAnim()
    {
        
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
        // We ensure we're not clipping inside any ground
        EjectFromCollisions();
        // Next, we decrease the fire cooldown
        fireCooldown = Mathf.Clamp(fireCooldown - Time.fixedDeltaTime, 0, Mathf.Infinity);
        // Then, we reset the flag marking if Snaily is airborne and shoving their face into a wall
        againstWallFlag = false;
        // We increment the jump buffer and coyote time values if necessary
        if (Control.JumpHold())
            jumpBufferCounter += Time.fixedDeltaTime;
        else
            jumpBufferCounter = 0;
        if (!grounded)
            coyoteTimeCounter += Time.fixedDeltaTime;
        else
            coyoteTimeCounter = 0;
        // And finally, we clear the collision list
        collisions.Clear();
    
        // Next, we run different blocks of movement code based on our gravity state. They're largely the same, but are kept separate
        // so that things can stay different between them if needed, like Blobby's entire wall-grabbing gimmick
        if (!inDeathCutscene)
        {
            readIDSpeed = PlayState.CheckForItem(9) ? 3 : (PlayState.CheckForItem(8) ? 2 : (PlayState.CheckForItem(7) ? 1 : 0));
            readIDJump = readIDSpeed + (PlayState.CheckForItem(4) ? 4 : 0);
    
            switch (gravityDir)
            {
                case Dirs.Floor:
                    CaseDown();
                    break;
                case Dirs.WallL:
                    CaseLeft();
                    break;
                case Dirs.WallR:
                    CaseRight();
                    break;
                case Dirs.Ceiling:
                    CaseUp();
                    break;
            }
            transform.position += (Vector3)velocity;
            if (!grounded && transform.position == (Vector3)lastPosition)
                transform.position += PlayState.FRAC_128 * gravityDir switch { Dirs.Floor => Vector3.down, Dirs.WallL => Vector3.left, Dirs.WallR => Vector3.right, _ => Vector3.up };

            if ((Control.ShootHold() || Control.StrafeHold()) && !PlayState.paralyzed)
            {
                if (shelled)
                    ToggleShell();
                Shoot();
            }

            EjectFromCollisions();
        }
    }

    public virtual void CaseDown()
    {
        // We start by zeroing our relatively vertical velocity if we happen to be on the ground. Just in case
        if (grounded)
            velocity.y = 0;
        // We also set this variable that will toggle the horizontal movement check. The corner-rounding check will turn this off to ensure
        // Snaily remains attached to the wall they turn onto, considering the vertical check is run before the horizontal check
        bool performHorizCheck = true;

        // First, we perform relatively vertical checks. Jumping and falling.
        if (!grounded)
        {
            if (gravityDir != defaultGravityDir && !CheckAbility(retainGravityOnAirborne))
            {
                CorrectGravity(false);
                if (defaultGravityDir != Dirs.Ceiling)
                    EjectFromCollisions(Dirs.Floor);
            }
            else
            {
                // Vertical velocity is decreased by the gravity scale every physics update. If the jump button is down during the first half of the jump arc,
                // the player's fall is slowed, granting additional height for as long as the button is down
                velocity.y -= gravity[readIDSpeed] * gravityMod * Time.fixedDeltaTime;
                if (velocity.y > 0 && !holdingJump)
                    velocity.y = PlayState.Integrate(velocity.y, 0, jumpFloatiness[readIDSpeed + (PlayState.CheckForItem(4) ? 4 : 0)], Time.fixedDeltaTime);
                velocity.y = Mathf.Clamp(velocity.y, terminalVelocity[readIDSpeed], Mathf.Infinity);

                // Real quick, in case we're running our face into a wall, let's check to see if there are any tunnels for us to slip into
                if ((Control.LeftHold() && GetDistance(Dirs.WallL) < PlayState.FRAC_64) || (Control.RightHold() && GetDistance(Dirs.WallR) < PlayState.FRAC_64))
                    TestForTunnel();

                // Is the player rising? Let's check for ceilings
                if (velocity.y > 0 && GetDistance(Dirs.Ceiling) < Mathf.Abs(velocity.y))
                {
                    velocity.y = lastDistance - PlayState.FRAC_128;
                    // Can the player grab the ceiling?
                    if (Control.UpHold() && CheckAbility(canSwapGravity) && !stunned)
                    {
                        gravityDir = Dirs.Ceiling;
                        SwapDir(Dirs.Ceiling);
                        grounded = true;
                        ungroundedViaHop = false;
                        holdingShell = true;
                        AddCollision(lastCollision);
                    }
                }
                // Is the player falling? Let's check for floors this time
                if (velocity.y < 0 && GetDistance(Dirs.Floor) < Mathf.Abs(velocity.y))
                {
                    velocity.y = -lastDistance + PlayState.FRAC_128;
                    grounded = true;
                    ungroundedViaHop = false;
                    AddCollision(lastCollision);
                }
            }
        }
        else
        {
            // Are we suddenly in the air (considered when Snaily is at least one pixel above the nearest surface) when we weren't last frame?
            if (GetDistance(Dirs.Floor, true) > (box.size.y * 0.5f) + PlayState.FRAC_16)
            {
                // Is the player holding down and forward? If so, let's see if there are any corners to round
                if (GetCornerDistance() <= (box.size.x * 0.75f) && CheckAbility(canRoundOuterCorners) && Control.DownHold() &&
                    (facingLeft ? Control.LeftHold() : Control.RightHold()) && !stunned)
                {
                    // Can we even round these corners at all? This check assumes our default gravity state means this corner is considered a ceiling corner
                    if (!CheckAbility(canRoundOppositeOuterCorners) && ((defaultGravityDir == Dirs.WallL && Control.AxisX() == -1) ||
                        (defaultGravityDir == Dirs.WallR && Control.AxisX() == 1) || defaultGravityDir == Dirs.Ceiling))
                    {
                        CorrectGravity(true);
                        if (defaultGravityDir switch { Dirs.WallL => Control.LeftHold(), Dirs.WallR => Control.RightHold(), _ => Control.UpHold() })
                            holdingShell = true;
                    }
                    // Getting here means we can round this corner! We need to reorient ourselves and ensure we're actually the right distance from the wall
                    else
                    {
                        gravityDir = facingLeft ? Dirs.WallR : Dirs.WallL;
                        SwapDir(gravityDir);
                        SwitchSurfaceAxis();
                        velocity = new((GetDistance(facingLeft ? Dirs.WallL : Dirs.WallR) * (facingLeft ? -1 : 1)) +
                            (PlayState.FRAC_128 * Mathf.Sign(Control.AxisX())), -PlayState.FRAC_16);
                        performHorizCheck = false;
                        AddCollision(lastCollision);
                        // Round that corner, you glorious little snail, you
                    }
                }
                // FALL
                else
                    grounded = false;
            }
            else
            {
                // We're still safe on the ground. Here we're just logging the ground as a collision
                // While we're here, just in case we happen to be on a platform that's moving up, let's make sure we haven't accidentally clipped inside of it
                if (GetDistance(Dirs.Floor, true) < (box.size.y * 0.5f))
                    transform.position += ((box.size.y * 0.5f) - lastDistance + PlayState.FRAC_128) * Vector3.up;
                AddCollision(lastCollision);
            }
        }

        // Now, we perform horizontal checks for moving back and forth
        if (Control.AxisX() != 0 && !Control.StrafeHold() && !PlayState.paralyzed && performHorizCheck)
        {
            if (shelled)
            {
                if (Control.AxisX() == (facingLeft ? 1 : -1) && !grounded)
                    transform.position += new Vector3(shellTurnaroundAdjust * (facingLeft ? 1 : -1), 0, 0);
                if (grounded)
                    ToggleShell();
            }
            SwapDir(Control.RightHold() ? Dirs.WallR : Dirs.WallL);
            float runSpeedValue = runSpeed[readIDSpeed] * speedMod * Time.fixedDeltaTime;
            // Are we currently running our face into a wall?
            if (GetDistance(facingLeft ? Dirs.WallL : Dirs.WallR) < runSpeedValue)
            {
                velocity.x = (lastDistance - PlayState.FRAC_128) * Mathf.Sign(Control.AxisX());
                AddCollision(lastCollision);
                // Does the player happen to be trying to climb a wall?
                if (GetDistance(Dirs.Floor, true) + GetDistance(Dirs.Ceiling, true) > box.size.y + PlayState.FRAC_8 && !stunned && CheckAbility(canSwapGravity))
                {
                    if ((Control.UpHold() && !grounded) ||
                        (Control.UpHold() && grounded && CheckAbility(canRoundInnerCorners)) ||
                        (Control.DownHold() && !grounded))
                    {
                        if (shelled)
                            ToggleShell();
                        SwitchSurfaceAxis();
                        if (GetDistance(Dirs.Floor, true) < (box.size.y * 0.5f))
                            transform.position += new Vector3(0, (box.size.y * 0.5f) - lastDistance + PlayState.FRAC_128, 0);
                        if (GetDistance(Dirs.Ceiling, true) < (box.size.y * 0.5f))
                            transform.position += new Vector3(0, -((box.size.y * 0.5f) - lastDistance) - PlayState.FRAC_128, 0);
                        velocity = new((GetDistance(facingLeft ? Dirs.WallL : Dirs.WallR) - PlayState.FRAC_128) * (facingLeft ? -1 : 1), 0);
                        if (Control.UpHold())
                            SwapDir(Dirs.Ceiling);
                        gravityDir = facingLeft ? Dirs.WallL : Dirs.WallR;
                        grounded = true;
                    }
                }
            }
            // No, we're not
            else
            {
                velocity.x = runSpeedValue * Mathf.Sign(Control.AxisX());
                if (grounded && CheckAbility(hopWhileMoving))
                {
                    grounded = false;
                    ungroundedViaHop = true;
                    velocity.y = hopPower;
                    lastPointBeforeHop = transform.position.y;
                }
            }
        }
        else if ((Control.AxisX() == 0 && !PlayState.paralyzed) || Control.StrafeHold())
            velocity.x = 0;

        // Now, let's see if we can jump
        if (CheckAbility(canJump) && Control.JumpHold() && (grounded || (coyoteTimeCounter < coyoteTime) || (ungroundedViaHop && (transform.position.y > lastPointBeforeHop)))
            && (!holdingJump || (jumpBufferCounter < jumpBuffer && velocity.y < 0)) && GetDistance(Dirs.Ceiling) > 0.95f && !PlayState.paralyzed)
        {
            if (shelled)
                ToggleShell();
            grounded = false;
            holdingJump = true;
            if (gravityDir != defaultGravityDir)
            {
                if (CheckAbility(retainGravityOnAirborne))
                    velocity.y = jumpPower[readIDJump] * jumpMod * Time.fixedDeltaTime;
                else
                {
                    CorrectGravity(false);
                    if (defaultGravityDir != Dirs.Ceiling)
                        EjectFromCollisions(Dirs.Floor);
                    jumpBufferCounter = jumpBuffer;
                }
            }
            else
                velocity.y = jumpPower[readIDJump] * jumpMod * Time.fixedDeltaTime;
            PlayState.PlaySound("Jump");
        }
        // How about gravity jumping?
        if (Control.JumpHold() && !holdingJump && !grounded && CheckAbility(canSwapGravity))
        {
            if (CheckAbility(canGravityJumpOpposite) && ((Control.UpHold() && CheckAbility(canGravityJumpAdjacent)) || !CheckAbility(canGravityJumpAdjacent)))
            {
                gravityDir = Dirs.Ceiling;
                SwapDir(Dirs.Ceiling);
                holdingShell = true;
            }
            if (CheckAbility(canGravityJumpAdjacent) && Control.AxisX() != 0)
            {
                Dirs newDir = Control.RightHold() ? Dirs.WallR : Dirs.WallL;
                gravityDir = newDir;
                SwapDir(newDir);
                SwitchSurfaceAxis();
                holdingShell = true;
            }
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
        if (holdingShell && gravityDir == Dirs.Floor && !Control.DownHold())
            holdingShell = false;
    }

    public virtual void CaseLeft()
    {
        // We start by zeroing our relatively vertical velocity if we happen to be on the ground. Just in case
        if (grounded)
            velocity.x = 0;
        // We also set this variable that will toggle the horizontal movement check. The corner-rounding check will turn this off to ensure
        // Snaily remains attached to the wall they turn onto, considering the vertical check is run before the horizontal check
        bool performHorizCheck = true;

        // First, we perform relatively vertical checks. Jumping and falling.
        if (!grounded)
        {
            if (gravityDir != defaultGravityDir && !CheckAbility(retainGravityOnAirborne))
            {
                CorrectGravity(false);
                if (defaultGravityDir != Dirs.WallR)
                    EjectFromCollisions(Dirs.WallL);
            }
            else
            {
                // Vertical velocity is decreased by the gravity scale every physics update. If the jump button is down during the first half of the jump arc,
                // the player's fall is slowed, granting additional height for as long as the button is down
                velocity.x -= gravity[readIDSpeed] * gravityMod * Time.fixedDeltaTime;
                if (velocity.x > 0 && !holdingJump)
                    velocity.x = PlayState.Integrate(velocity.x, 0, jumpFloatiness[readIDSpeed + (PlayState.CheckForItem(4) ? 4 : 0)], Time.fixedDeltaTime);
                velocity.x = Mathf.Clamp(velocity.x, terminalVelocity[readIDSpeed], Mathf.Infinity);

                // Real quick, in case we're running our face into a wall, let's check to see if there are any tunnels for us to slip into
                if ((Control.DownHold() && GetDistance(Dirs.Floor) < PlayState.FRAC_64) || (Control.UpHold() && GetDistance(Dirs.Ceiling) < PlayState.FRAC_64))
                    TestForTunnel();

                // Is the player rising? Let's check for ceilings
                if (velocity.x > 0 && GetDistance(Dirs.WallR) < Mathf.Abs(velocity.x))
                {
                    velocity.x = lastDistance - PlayState.FRAC_128;
                    // Can the player grab the ceiling?
                    if (Control.RightHold() && CheckAbility(canSwapGravity) && !stunned)
                    {
                        gravityDir = Dirs.WallR;
                        SwapDir(Dirs.WallR);
                        grounded = true;
                        ungroundedViaHop = false;
                        holdingShell = true;
                        AddCollision(lastCollision);
                    }
                }
                // Is the player falling? Let's check for floors this time
                if (velocity.x < 0 && GetDistance(Dirs.WallL) < Mathf.Abs(velocity.x))
                {
                    velocity.x = -lastDistance + PlayState.FRAC_128;
                    grounded = true;
                    ungroundedViaHop = false;
                    AddCollision(lastCollision);
                }
            }
        }
        else
        {
            // Are we suddenly in the air (considered when Snaily is at least one pixel above the nearest surface) when we weren't last frame?
            if (GetDistance(Dirs.WallL, true) > (box.size.x * 0.5f) + PlayState.FRAC_16)
            {
                // Is the player holding down and forward? If so, let's see if there are any corners to round
                if (GetCornerDistance() <= (box.size.y * 0.75f) && CheckAbility(canRoundOuterCorners) && Control.LeftHold() &&
                    (facingDown ? Control.DownHold() : Control.UpHold()) && !stunned)
                {
                    // Can we even round these corners at all? This check assumes our default gravity state means this corner is considered a ceiling corner
                    if (!CheckAbility(canRoundOppositeOuterCorners) && ((defaultGravityDir == Dirs.Floor && Control.AxisY() == -1) ||
                        (defaultGravityDir == Dirs.Ceiling && Control.AxisY() == 1) || defaultGravityDir == Dirs.WallR))
                    {
                        CorrectGravity(true);
                        if (defaultGravityDir switch { Dirs.Floor => Control.DownHold(), Dirs.Ceiling => Control.UpHold(), _ => Control.RightHold() })
                            holdingShell = true;
                    }
                    // Getting here means we can round this corner! We need to reorient ourselves and ensure we're actually the right distance from the wall
                    else
                    {
                        gravityDir = facingDown ? Dirs.Ceiling : Dirs.Floor;
                        SwapDir(gravityDir);
                        SwitchSurfaceAxis();
                        velocity = new(-PlayState.FRAC_16, (GetDistance(facingDown ? Dirs.Floor : Dirs.Ceiling) * (facingDown ? -1 : 1)) +
                            (PlayState.FRAC_128 * Mathf.Sign(Control.AxisY())));
                        performHorizCheck = false;
                        AddCollision(lastCollision);
                        // Round that corner, you glorious little snail, you
                    }
                }
                // FALL
                else
                    grounded = false;
            }
            else
            {
                // We're still safe on the ground. Here we're just logging the ground as a collision
                // While we're here, just in case we happen to be on a platform that's moving up, let's make sure we haven't accidentally clipped inside of it
                if (GetDistance(Dirs.WallL, true) < (box.size.x * 0.5f))
                    transform.position += ((box.size.x * 0.5f) - lastDistance + PlayState.FRAC_128) * Vector3.right;
                AddCollision(lastCollision);
            }
        }

        // Now, we perform horizontal checks for moving back and forth
        if (Control.AxisY() != 0 && !Control.StrafeHold() && !PlayState.paralyzed && performHorizCheck)
        {
            if (shelled)
            {
                if (Control.AxisY() == (facingDown ? 1 : -1) && !grounded)
                    transform.position += new Vector3(0, shellTurnaroundAdjust * (facingDown ? 1 : -1), 0);
                if (grounded)
                    ToggleShell();
            }
            SwapDir(Control.UpHold() ? Dirs.Ceiling : Dirs.Floor);
            float runSpeedValue = runSpeed[readIDSpeed] * speedMod * Time.fixedDeltaTime;
            // Are we currently running our face into a wall?
            if (GetDistance(facingDown ? Dirs.Floor : Dirs.Ceiling) < runSpeedValue)
            {
                velocity.y = (lastDistance - PlayState.FRAC_128) * Mathf.Sign(Control.AxisY());
                AddCollision(lastCollision);
                // Does the player happen to be trying to climb a wall?
                if (GetDistance(Dirs.WallL, true) + GetDistance(Dirs.WallR, true) > box.size.x + PlayState.FRAC_8 && !stunned && CheckAbility(canSwapGravity))
                {
                    if ((Control.RightHold() && !grounded) ||
                        (Control.RightHold() && grounded && CheckAbility(canRoundInnerCorners)) ||
                        (Control.LeftHold() && !grounded))
                    {
                        if (shelled)
                            ToggleShell();
                        SwitchSurfaceAxis();
                        if (GetDistance(Dirs.WallL, true) < (box.size.x * 0.5f))
                            transform.position += new Vector3((box.size.x * 0.5f) - lastDistance + PlayState.FRAC_128, 0, 0);
                        if (GetDistance(Dirs.WallR, true) < (box.size.x * 0.5f))
                            transform.position += new Vector3(-((box.size.x * 0.5f) - lastDistance) - PlayState.FRAC_128, 0, 0);
                        velocity = new(0, (GetDistance(facingDown ? Dirs.Floor : Dirs.Ceiling) - PlayState.FRAC_128) * (facingDown ? -1 : 1));
                        if (Control.RightHold())
                            SwapDir(Dirs.WallR);
                        gravityDir = facingDown ? Dirs.Floor : Dirs.Ceiling;
                        grounded = true;
                    }
                }
            }
            // No, we're not
            else
            {
                velocity.y = runSpeedValue * Mathf.Sign(Control.AxisY());
                if (grounded && CheckAbility(hopWhileMoving))
                {
                    grounded = false;
                    ungroundedViaHop = true;
                    velocity.x = hopPower;
                    lastPointBeforeHop = transform.position.x;
                }
            }
        }
        else if ((Control.AxisY() == 0 && !PlayState.paralyzed) || Control.StrafeHold())
            velocity.y = 0;

        // Now, let's see if we can jump
        if (CheckAbility(canJump) && Control.JumpHold() && (grounded || (coyoteTimeCounter < coyoteTime) || (ungroundedViaHop && (transform.position.x > lastPointBeforeHop)))
            && (!holdingJump || (jumpBufferCounter < jumpBuffer && velocity.x < 0)) && GetDistance(Dirs.WallR) > 0.95f && !PlayState.paralyzed)
        {
            if (shelled)
                ToggleShell();
            grounded = false;
            holdingJump = true;
            if (gravityDir != defaultGravityDir)
            {
                if (CheckAbility(retainGravityOnAirborne))
                    velocity.x = jumpPower[readIDJump] * jumpMod * Time.fixedDeltaTime;
                else
                {
                    CorrectGravity(false);
                    if (defaultGravityDir != Dirs.WallR)
                        EjectFromCollisions(Dirs.WallL);
                    jumpBufferCounter = jumpBuffer;
                }
            }
            else
                velocity.x = jumpPower[readIDJump] * jumpMod * Time.fixedDeltaTime;
            PlayState.PlaySound("Jump");
        }
        // How about gravity jumping?
        if (Control.JumpHold() && !holdingJump && !grounded && CheckAbility(canSwapGravity))
        {
            if (CheckAbility(canGravityJumpOpposite) && ((Control.RightHold() && CheckAbility(canGravityJumpAdjacent)) || !CheckAbility(canGravityJumpAdjacent)))
            {
                gravityDir = Dirs.WallR;
                SwapDir(Dirs.WallR);
                holdingShell = true;
            }
            if (CheckAbility(canGravityJumpAdjacent) && Control.AxisY() != 0)
            {
                Dirs newDir = Control.UpHold() ? Dirs.Ceiling : Dirs.Floor;
                gravityDir = newDir;
                SwapDir(newDir);
                SwitchSurfaceAxis();
                holdingShell = true;
            }
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
        if (holdingShell && gravityDir == Dirs.WallL && !Control.LeftHold())
            holdingShell = false;
    }

    public virtual void CaseRight()
    {
        // We start by zeroing our relatively vertical velocity if we happen to be on the ground. Just in case
        if (grounded)
            velocity.x = 0;
        // We also set this variable that will toggle the horizontal movement check. The corner-rounding check will turn this off to ensure
        // Snaily remains attached to the wall they turn onto, considering the vertical check is run before the horizontal check
        bool performHorizCheck = true;

        // First, we perform relatively vertical checks. Jumping and falling.
        if (!grounded)
        {
            if (gravityDir != defaultGravityDir && !CheckAbility(retainGravityOnAirborne))
            {
                CorrectGravity(false);
                if (defaultGravityDir != Dirs.WallL)
                    EjectFromCollisions(Dirs.WallR);
            }
            else
            {
                // Vertical velocity is decreased by the gravity scale every physics update. If the jump button is down during the first half of the jump arc,
                // the player's fall is slowed, granting additional height for as long as the button is down
                velocity.x += gravity[readIDSpeed] * gravityMod * Time.fixedDeltaTime;
                if (velocity.x < 0 && !holdingJump)
                    velocity.x = PlayState.Integrate(velocity.x, 0, jumpFloatiness[readIDSpeed + (PlayState.CheckForItem(4) ? 4 : 0)], Time.fixedDeltaTime);
                velocity.x = Mathf.Clamp(velocity.x, -Mathf.Infinity, -terminalVelocity[readIDSpeed]);

                // Real quick, in case we're running our face into a wall, let's check to see if there are any tunnels for us to slip into
                if ((Control.DownHold() && GetDistance(Dirs.Floor) < PlayState.FRAC_64) || (Control.UpHold() && GetDistance(Dirs.Ceiling) < PlayState.FRAC_64))
                    TestForTunnel();

                // Is the player rising? Let's check for ceilings
                if (velocity.x < 0 && GetDistance(Dirs.WallL) < Mathf.Abs(velocity.x))
                {
                    velocity.x = -lastDistance + PlayState.FRAC_128;
                    // Can the player grab the ceiling?
                    if (Control.LeftHold() && CheckAbility(canSwapGravity) && !stunned)
                    {
                        gravityDir = Dirs.WallL;
                        SwapDir(Dirs.WallL);
                        grounded = true;
                        ungroundedViaHop = false;
                        holdingShell = true;
                        AddCollision(lastCollision);
                    }
                }
                // Is the player falling? Let's check for floors this time
                if (velocity.x > 0 && GetDistance(Dirs.WallR) < Mathf.Abs(velocity.x))
                {
                    velocity.x = lastDistance - PlayState.FRAC_128;
                    grounded = true;
                    ungroundedViaHop = false;
                    AddCollision(lastCollision);
                }
            }
        }
        else
        {
            // Are we suddenly in the air (considered when Snaily is at least one pixel above the nearest surface) when we weren't last frame?
            if (GetDistance(Dirs.WallR, true) > (box.size.x * 0.5f) + PlayState.FRAC_16)
            {
                // Is the player holding down and forward? If so, let's see if there are any corners to round
                if (GetCornerDistance() <= (box.size.y * 0.75f) && CheckAbility(canRoundOuterCorners) && Control.RightHold() &&
                    (facingDown ? Control.DownHold() : Control.UpHold()) && !stunned)
                {
                    // Can we even round these corners at all? This check assumes our default gravity state means this corner is considered a ceiling corner
                    if (!CheckAbility(canRoundOppositeOuterCorners) && ((defaultGravityDir == Dirs.Floor && Control.AxisY() == -1) ||
                        (defaultGravityDir == Dirs.Ceiling && Control.AxisY() == 1) || defaultGravityDir == Dirs.WallL))
                    {
                        CorrectGravity(true);
                        if (defaultGravityDir switch { Dirs.Floor => Control.DownHold(), Dirs.Ceiling => Control.UpHold(), _ => Control.LeftHold() })
                            holdingShell = true;
                    }
                    // Getting here means we can round this corner! We need to reorient ourselves and ensure we're actually the right distance from the wall
                    else
                    {
                        gravityDir = facingDown ? Dirs.Ceiling : Dirs.Floor;
                        SwapDir(gravityDir);
                        SwitchSurfaceAxis();
                        velocity = new(PlayState.FRAC_16, (GetDistance(facingDown ? Dirs.Floor : Dirs.Ceiling) * (facingDown ? -1 : 1)) +
                            (PlayState.FRAC_128 * Mathf.Sign(Control.AxisY())));
                        performHorizCheck = false;
                        AddCollision(lastCollision);
                        // Round that corner, you glorious little snail, you
                    }
                }
                // FALL
                else
                    grounded = false;
            }
            else
            {
                // We're still safe on the ground. Here we're just logging the ground as a collision
                // While we're here, just in case we happen to be on a platform that's moving up, let's make sure we haven't accidentally clipped inside of it
                if (GetDistance(Dirs.WallR, true) < (box.size.x * 0.5f))
                    transform.position += ((box.size.x * 0.5f) - lastDistance + PlayState.FRAC_128) * Vector3.left;
                AddCollision(lastCollision);
            }
        }

        // Now, we perform horizontal checks for moving back and forth
        if (Control.AxisY() != 0 && !Control.StrafeHold() && !PlayState.paralyzed && performHorizCheck)
        {
            if (shelled)
            {
                if (Control.AxisY() == (facingDown ? 1 : -1) && !grounded)
                    transform.position += new Vector3(0, shellTurnaroundAdjust * (facingDown ? 1 : -1), 0);
                if (grounded)
                    ToggleShell();
            }
            SwapDir(Control.UpHold() ? Dirs.Ceiling : Dirs.Floor);
            float runSpeedValue = runSpeed[readIDSpeed] * speedMod * Time.fixedDeltaTime;
            // Are we currently running our face into a wall?
            if (GetDistance(facingDown ? Dirs.Floor : Dirs.Ceiling) < runSpeedValue)
            {
                velocity.y = (lastDistance - PlayState.FRAC_128) * Mathf.Sign(Control.AxisY());
                AddCollision(lastCollision);
                // Does the player happen to be trying to climb a wall?
                if (GetDistance(Dirs.WallL, true) + GetDistance(Dirs.WallR, true) > box.size.x + PlayState.FRAC_8 && !stunned && CheckAbility(canSwapGravity))
                {
                    if ((Control.LeftHold() && !grounded) ||
                        (Control.LeftHold() && grounded && CheckAbility(canRoundInnerCorners)) ||
                        (Control.RightHold() && !grounded))
                    {
                        if (shelled)
                            ToggleShell();
                        SwitchSurfaceAxis();
                        if (GetDistance(Dirs.WallL, true) < (box.size.x * 0.5f))
                            transform.position += new Vector3((box.size.x * 0.5f) - lastDistance + PlayState.FRAC_128, 0, 0);
                        if (GetDistance(Dirs.WallR, true) < (box.size.x * 0.5f))
                            transform.position += new Vector3(-((box.size.x * 0.5f) - lastDistance) - PlayState.FRAC_128, 0, 0);
                        velocity = new(0, (GetDistance(facingDown ? Dirs.Floor : Dirs.Ceiling) - PlayState.FRAC_128) * (facingDown ? -1 : 1));
                        if (Control.LeftHold())
                            SwapDir(Dirs.WallL);
                        gravityDir = facingDown ? Dirs.Floor : Dirs.Ceiling;
                        grounded = true;
                    }
                }
            }
            // No, we're not
            else
            {
                velocity.y = runSpeedValue * Mathf.Sign(Control.AxisY());
                if (grounded && CheckAbility(hopWhileMoving))
                {
                    grounded = false;
                    ungroundedViaHop = true;
                    velocity.x = -hopPower;
                    lastPointBeforeHop = transform.position.x;
                }
            }
        }
        else if ((Control.AxisY() == 0 && !PlayState.paralyzed) || Control.StrafeHold())
            velocity.y = 0;

        // Now, let's see if we can jump
        if (CheckAbility(canJump) && Control.JumpHold() && (grounded || (coyoteTimeCounter < coyoteTime) || (ungroundedViaHop && (transform.position.x < lastPointBeforeHop)))
            && (!holdingJump || (jumpBufferCounter < jumpBuffer && velocity.x > 0)) && GetDistance(Dirs.WallL) > 0.95f && !PlayState.paralyzed)
        {
            if (shelled)
                ToggleShell();
            grounded = false;
            holdingJump = true;
            if (gravityDir != defaultGravityDir)
            {
                if (CheckAbility(retainGravityOnAirborne))
                    velocity.x = -jumpPower[readIDJump] * jumpMod * Time.fixedDeltaTime;
                else
                {
                    CorrectGravity(false);
                    if (defaultGravityDir != Dirs.WallL)
                        EjectFromCollisions(Dirs.WallR);
                    jumpBufferCounter = jumpBuffer;
                }
            }
            else
                velocity.x = -jumpPower[readIDJump] * jumpMod * Time.fixedDeltaTime;
            PlayState.PlaySound("Jump");
        }
        // How about gravity jumping?
        if (Control.JumpHold() && !holdingJump && !grounded && CheckAbility(canSwapGravity))
        {
            if (CheckAbility(canGravityJumpOpposite) && ((Control.LeftHold() && CheckAbility(canGravityJumpAdjacent)) || !CheckAbility(canGravityJumpAdjacent)))
            {
                gravityDir = Dirs.WallL;
                SwapDir(Dirs.WallL);
                holdingShell = true;
            }
            if (CheckAbility(canGravityJumpAdjacent) && Control.AxisY() != 0)
            {
                Dirs newDir = Control.UpHold() ? Dirs.Ceiling : Dirs.Floor;
                gravityDir = newDir;
                SwapDir(newDir);
                SwitchSurfaceAxis();
                holdingShell = true;
            }
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
        if (holdingShell && gravityDir == Dirs.WallR && !Control.RightHold())
            holdingShell = false;
    }

    public virtual void CaseUp()
    {
        // We start by zeroing our relatively vertical velocity if we happen to be on the ground. Just in case
        if (grounded)
            velocity.y = 0;
        // We also set this variable that will toggle the horizontal movement check. The corner-rounding check will turn this off to ensure
        // Snaily remains attached to the wall they turn onto, considering the vertical check is run before the horizontal check
        bool performHorizCheck = true;

        // First, we perform relatively vertical checks. Jumping and falling.
        if (!grounded)
        {
            if (gravityDir != defaultGravityDir && !CheckAbility(retainGravityOnAirborne))
            {
                CorrectGravity(false);
                if (defaultGravityDir != Dirs.Floor)
                    EjectFromCollisions(Dirs.Ceiling);
            }
            else
            {
                // Vertical velocity is decreased by the gravity scale every physics update. If the jump button is down during the first half of the jump arc,
                // the player's fall is slowed, granting additional height for as long as the button is down
                velocity.y += gravity[readIDSpeed] * gravityMod * Time.fixedDeltaTime;
                if (velocity.y < 0 && !holdingJump)
                    velocity.y = PlayState.Integrate(velocity.y, 0, jumpFloatiness[readIDSpeed + (PlayState.CheckForItem(4) ? 4 : 0)], Time.fixedDeltaTime);
                velocity.y = Mathf.Clamp(velocity.y, -Mathf.Infinity, -terminalVelocity[readIDSpeed]);

                // Real quick, in case we're running our face into a wall, let's check to see if there are any tunnels for us to slip into
                if ((Control.LeftHold() && GetDistance(Dirs.WallL) < PlayState.FRAC_64) || (Control.RightHold() && GetDistance(Dirs.WallR) < PlayState.FRAC_64))
                    TestForTunnel();

                // Is the player rising? Let's check for ceilings
                if (velocity.y < 0 && GetDistance(Dirs.Floor) < Mathf.Abs(velocity.y))
                {
                    velocity.y = -lastDistance + PlayState.FRAC_128;
                    // Can the player grab the ceiling?
                    if (Control.DownHold() && CheckAbility(canSwapGravity) && !stunned)
                    {
                        gravityDir = Dirs.Floor;
                        SwapDir(Dirs.Floor);
                        grounded = true;
                        ungroundedViaHop = false;
                        holdingShell = true;
                        AddCollision(lastCollision);
                    }
                }
                // Is the player falling? Let's check for floors this time
                if (velocity.y > 0 && GetDistance(Dirs.Ceiling) < Mathf.Abs(velocity.y))
                {
                    velocity.y = lastDistance - PlayState.FRAC_128;
                    grounded = true;
                    ungroundedViaHop = false;
                    AddCollision(lastCollision);
                }
            }
        }
        else
        {
            // Are we suddenly in the air (considered when Snaily is at least one pixel above the nearest surface) when we weren't last frame?
            if (GetDistance(Dirs.Ceiling, true) > (box.size.y * 0.5f) + PlayState.FRAC_16)
            {
                // Is the player holding down and forward? If so, let's see if there are any corners to round
                if (GetCornerDistance() <= (box.size.x * 0.75f) && CheckAbility(canRoundOuterCorners) && Control.UpHold() &&
                    (facingLeft ? Control.LeftHold() : Control.RightHold()) && !stunned)
                {
                    // Can we even round these corners at all? This check assumes our default gravity state means this corner is considered a ceiling corner
                    if (!CheckAbility(canRoundOppositeOuterCorners) && ((defaultGravityDir == Dirs.WallL && Control.AxisX() == -1) ||
                        (defaultGravityDir == Dirs.WallR && Control.AxisX() == 1) || defaultGravityDir == Dirs.Floor))
                    {
                        CorrectGravity(true);
                        if (defaultGravityDir switch { Dirs.WallL => Control.LeftHold(), Dirs.WallR => Control.RightHold(), _ => Control.DownHold() })
                            holdingShell = true;
                    }
                    // Getting here means we can round this corner! We need to reorient ourselves and ensure we're actually the right distance from the wall
                    else
                    {
                        gravityDir = facingLeft ? Dirs.WallR : Dirs.WallL;
                        SwapDir(gravityDir);
                        SwitchSurfaceAxis();
                        velocity = new((GetDistance(facingLeft ? Dirs.WallL : Dirs.WallR) * (facingLeft ? -1 : 1)) +
                            (PlayState.FRAC_128 * Mathf.Sign(Control.AxisX())), PlayState.FRAC_16);
                        performHorizCheck = false;
                        AddCollision(lastCollision);
                        // Round that corner, you glorious little snail, you
                    }
                }
                // FALL
                else
                    grounded = false;
            }
            else
            {
                // We're still safe on the ground. Here we're just logging the ground as a collision
                // While we're here, just in case we happen to be on a platform that's moving up, let's make sure we haven't accidentally clipped inside of it
                if (GetDistance(Dirs.Ceiling, true) < (box.size.y * 0.5f))
                    transform.position += ((box.size.y * 0.5f) - lastDistance + PlayState.FRAC_128) * Vector3.down;
                AddCollision(lastCollision);
            }
        }

        // Now, we perform horizontal checks for moving back and forth
        if (Control.AxisX() != 0 && !Control.StrafeHold() && !PlayState.paralyzed && performHorizCheck)
        {
            if (shelled)
            {
                if (Control.AxisX() == (facingLeft ? 1 : -1) && !grounded)
                    transform.position += new Vector3(shellTurnaroundAdjust * (facingLeft ? 1 : -1), 0, 0);
                if (grounded)
                    ToggleShell();
            }
            SwapDir(Control.RightHold() ? Dirs.WallR : Dirs.WallL);
            float runSpeedValue = runSpeed[readIDSpeed] * speedMod * Time.fixedDeltaTime;
            // Are we currently running our face into a wall?
            if (GetDistance(facingLeft ? Dirs.WallL : Dirs.WallR) < runSpeedValue)
            {
                velocity.x = (lastDistance - PlayState.FRAC_128) * Mathf.Sign(Control.AxisX());
                AddCollision(lastCollision);
                // Does the player happen to be trying to climb a wall?
                if (GetDistance(Dirs.Floor, true) + GetDistance(Dirs.Ceiling, true) > box.size.y + PlayState.FRAC_8 && !stunned && CheckAbility(canSwapGravity))
                {
                    if ((Control.DownHold() && !grounded) ||
                        (Control.DownHold() && grounded && CheckAbility(canRoundInnerCorners)) ||
                        (Control.UpHold() && !grounded))
                    {
                        if (shelled)
                            ToggleShell();
                        SwitchSurfaceAxis();
                        if (GetDistance(Dirs.Floor, true) < (box.size.y * 0.5f))
                            transform.position += new Vector3(0, (box.size.y * 0.5f) - lastDistance + PlayState.FRAC_128, 0);
                        if (GetDistance(Dirs.Ceiling, true) < (box.size.y * 0.5f))
                            transform.position += new Vector3(0, -((box.size.y * 0.5f) - lastDistance) - PlayState.FRAC_128, 0);
                        velocity = new((GetDistance(facingLeft ? Dirs.WallL : Dirs.WallR) - PlayState.FRAC_128) * (facingLeft ? -1 : 1), 0);
                        if (Control.DownHold())
                            SwapDir(Dirs.Floor);
                        gravityDir = facingLeft ? Dirs.WallL : Dirs.WallR;
                        grounded = true;
                    }
                }
            }
            // No, we're not
            else
            {
                velocity.x = runSpeedValue * Mathf.Sign(Control.AxisX());
                if (grounded && CheckAbility(hopWhileMoving))
                {
                    grounded = false;
                    ungroundedViaHop = true;
                    velocity.y = -hopPower;
                    lastPointBeforeHop = transform.position.y;
                }
            }
        }
        else if ((Control.AxisX() == 0 && !PlayState.paralyzed) || Control.StrafeHold())
            velocity.x = 0;

        // Now, let's see if we can jump
        if (CheckAbility(canJump) && Control.JumpHold() && (grounded || (coyoteTimeCounter < coyoteTime) || (ungroundedViaHop && (transform.position.y < lastPointBeforeHop)))
            && (!holdingJump || (jumpBufferCounter < jumpBuffer && velocity.y > 0)) && GetDistance(Dirs.Floor) > 0.95f && !PlayState.paralyzed)
        {
            if (shelled)
                ToggleShell();
            grounded = false;
            holdingJump = true;
            if (gravityDir != defaultGravityDir)
            {
                if (CheckAbility(retainGravityOnAirborne))
                    velocity.y = -jumpPower[readIDJump] * jumpMod * Time.fixedDeltaTime;
                else
                {
                    CorrectGravity(false);
                    if (defaultGravityDir != Dirs.Floor)
                        EjectFromCollisions(Dirs.Ceiling);
                    jumpBufferCounter = jumpBuffer;
                }
            }
            else
                velocity.y = -jumpPower[readIDJump] * jumpMod * Time.fixedDeltaTime;
            PlayState.PlaySound("Jump");
        }
        // How about gravity jumping?
        if (Control.JumpHold() && !holdingJump && !grounded && CheckAbility(canSwapGravity))
        {
            if (CheckAbility(canGravityJumpOpposite) && ((Control.DownHold() && CheckAbility(canGravityJumpAdjacent)) || !CheckAbility(canGravityJumpAdjacent)))
            {
                gravityDir = Dirs.Floor;
                SwapDir(Dirs.Floor);
                holdingShell = true;
            }
            if (CheckAbility(canGravityJumpAdjacent) && Control.AxisX() != 0)
            {
                Dirs newDir = Control.RightHold() ? Dirs.WallR : Dirs.WallL;
                gravityDir = newDir;
                SwapDir(newDir);
                SwitchSurfaceAxis();
                holdingShell = true;
            }
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
        if (holdingShell && gravityDir == Dirs.Ceiling && !Control.UpHold())
            holdingShell = false;
    }

    private bool CheckAbility(int[][] ability)
    {
        bool fullCheck = false;
        for (int i = 0; i < ability.Length; i++)
        {
            bool thisCheck = true;
            for (int j = 0; j < ability[i].Length; j++)
            {
                if (thisCheck)
                {
                    switch (ability[i][j])
                    {
                        case -1:
                            break;
                        case -2:
                            thisCheck = false;
                            break;
                        default:
                            thisCheck = PlayState.currentProfile.items[ability[i][j]] == 1;
                            break;
                    }
                }
            }
            fullCheck = thisCheck;
            if (fullCheck)
                i = ability.Length;
        }
        return fullCheck;
    }

    public Dirs GetDirOpposite(Dirs direction)
    {
        return direction switch
        {
            Dirs.WallL => Dirs.WallR,
            Dirs.WallR => Dirs.WallL,
            Dirs.Ceiling => Dirs.Floor,
            _ => Dirs.Ceiling
        };
    }

    public Dirs GetDirAdjacentLeft(Dirs direction)
    {
        return direction switch
        {
            Dirs.WallL => Dirs.Ceiling,
            Dirs.WallR => Dirs.Floor,
            Dirs.Ceiling => Dirs.WallR,
            _ => Dirs.WallL
        };
    }

    public Dirs GetDirAdjacentRight(Dirs direction)
    {
        return direction switch
        {
            Dirs.WallL => Dirs.Floor,
            Dirs.WallR => Dirs.Ceiling,
            Dirs.Ceiling => Dirs.WallL,
            _ => Dirs.WallR
        };
    }

    public void CorrectGravity(bool eject, bool zeroVel = true)
    {
        bool swapAxis = defaultGravityDir == GetDirAdjacentLeft(gravityDir) || defaultGravityDir == GetDirAdjacentRight(gravityDir);
        if (swapAxis)
            SwitchSurfaceAxis();
        if (eject)
            EjectFromCollisions();
        SwapDir(defaultGravityDir);
        gravityDir = defaultGravityDir;
        if (defaultGravityDir switch { Dirs.WallL => Control.LeftHold(), Dirs.WallR => Control.RightHold(),
            Dirs.Ceiling => Control.UpHold(), _ => Control.DownHold() })
            holdingShell = true;
        if (zeroVel)
        {
            if (gravityDir == Dirs.WallL || gravityDir == Dirs.WallR)
                velocity.x = 0;
            else
                velocity.y = 0;
        }
        jumpBufferCounter = jumpBuffer;
    }

    public void TestForTunnel()
    {
        bool foundTunnel = false;
        Vector2 a = transform.position;
        Vector2 b = (Vector2)transform.position + (velocity * ((gravityDir == Dirs.WallL || gravityDir == Dirs.WallR) ? Vector2.right : Vector2.up));
        Dirs dir;
        if (gravityDir == Dirs.WallL || gravityDir == Dirs.WallR)
            dir = facingDown ? Dirs.Floor : Dirs.Ceiling;
        else
            dir = facingLeft ? Dirs.WallL : Dirs.WallR;

        if (GetDistance(dir) > PlayState.FRAC_16)
            return;
        transform.position = b;
        if (GetDistance(dir) > PlayState.FRAC_16)
        {
            transform.position = a;
            return;
        }

        for (int i = 0; i <= THIN_TUNNEL_ENTRANCE_STEPS; i++)
        {
            transform.position = Vector2.Lerp(a, b, (float)i / (float)THIN_TUNNEL_ENTRANCE_STEPS);
            if (GetDistance(dir) > PlayState.FRAC_16)
            {
                i = THIN_TUNNEL_ENTRANCE_STEPS;
                foundTunnel = true;
                if (gravityDir == Dirs.WallL || gravityDir == Dirs.WallR)
                    transform.position += PlayState.FRAC_64 * (facingDown ? -1 : 1) * Vector3.up;
                else
                    transform.position += PlayState.FRAC_64 * (facingLeft ? -1 : 1) * Vector3.right;
            }
        }

        if (!foundTunnel)
            transform.position = a;
    }

    #endregion Movement

    #region Player utilities

    public float GetDistance(Dirs dir)
    {
        return GetDistance(dir, false, MAX_DIST_CASTS);
    }
    public float GetDistance(Dirs dir, int casts = MAX_DIST_CASTS)
    {
        return GetDistance(dir, false, casts);
    }
    public float GetDistance(Dirs dir, bool fromCenter = false, int casts = MAX_DIST_CASTS)
    {
        float shortestDis = Mathf.Infinity;
        lastCollision = null;
        Vector2 a = (Vector2)transform.position - (box.size * 0.5f) - new Vector2(DIST_CAST_EDGE_BUFFER, DIST_CAST_EDGE_BUFFER);
        Vector2 b = (Vector2)transform.position + (box.size * 0.5f) + new Vector2(DIST_CAST_EDGE_BUFFER, DIST_CAST_EDGE_BUFFER);
        Vector2 origin;
        RaycastHit2D hit;
        for (int i = 0; i < casts; i++)
        {
            float t = (float)i / (float)(casts - 1);
            switch (dir)
            {
                default:
                case Dirs.Floor:
                    if (fromCenter)
                        origin = Vector2.Lerp(new Vector2(a.x, transform.position.y), new Vector2(b.x, transform.position.y), t);
                    else
                        origin = Vector2.Lerp(a, new Vector2(b.x, a.y), t);
                    hit = Physics2D.Raycast(origin, Vector2.down, Mathf.Infinity, playerCollide);
                    break;
                case Dirs.WallL:
                    if (fromCenter)
                        origin = Vector2.Lerp(new Vector2(transform.position.x, a.y), new Vector2(transform.position.x, b.y), t);
                    else
                        origin = Vector2.Lerp(a, new Vector2(a.x, b.y), t);
                    hit = Physics2D.Raycast(origin, Vector2.left, Mathf.Infinity, playerCollide);
                    break;
                case Dirs.WallR:
                    if (fromCenter)
                        origin = Vector2.Lerp(new Vector2(transform.position.x, a.y), new Vector2(transform.position.x, b.y), t);
                    else
                        origin = Vector2.Lerp(new Vector2(b.x, a.y), b, t);
                    hit = Physics2D.Raycast(origin, Vector2.right, Mathf.Infinity, playerCollide);
                    break;
                case Dirs.Ceiling:
                    if (fromCenter)
                        origin = Vector2.Lerp(new Vector2(a.x, transform.position.y), new Vector2(b.x, transform.position.y), t);
                    else
                        origin = Vector2.Lerp(new Vector2(a.x, b.y), b, t);
                    hit = Physics2D.Raycast(origin, Vector2.up, Mathf.Infinity, playerCollide);
                    break;
            }
            if (hit.collider != null && !PlayState.IsPointPlayerCollidable(origin))
            {
                if (shortestDis > hit.distance)
                {
                    shortestDis = hit.distance;
                    lastCollision = hit.collider;
                }
                Debug.DrawLine(origin, hit.point, Color.white, 0);
            }
        }
        lastDistance = shortestDis;
        return shortestDis;
    }

    public float GetCornerDistance()
    {
        Vector2 testPosAdjustDir = gravityDir switch
        {
            Dirs.WallL => Vector2.left,
            Dirs.WallR => Vector2.right,
            Dirs.Ceiling => Vector2.up,
            _ => Vector2.down
        };
        float testPosAdjustDis = (gravityDir == Dirs.WallL || gravityDir == Dirs.WallR) ? box.size.x * 0.5f + 0.25f : box.size.y * 0.5f + 0.25f;
        Vector2 testCastDir = (gravityDir == Dirs.WallL || gravityDir == Dirs.WallR) ? (facingDown ? Vector2.up : Vector2.down) : (facingLeft ? Vector2.right : Vector2.left);
        RaycastHit2D cornerTest = Physics2D.Raycast((Vector2)transform.position + (testPosAdjustDir * testPosAdjustDis), testCastDir, Mathf.Infinity, playerCollide);
        if (cornerTest.collider == null)
            return Mathf.Infinity;
        return cornerTest.distance;
    }

    // This function is called to reorient the player character in any way necessary
    // Note: this only accounts for four directions in either the ground/ceiling state or the wall state, never both. A call to
    // SwitchSurfaceAxis() is necessary for that
    public void SwapDir(Dirs dirToFace)
    {
        switch (dirToFace)
        {
            case Dirs.Floor:
                facingDown = true;
                break;
            case Dirs.WallL:
                facingLeft = true;
                break;
            case Dirs.WallR:
                facingLeft = false;
                break;
            case Dirs.Ceiling:
                facingDown = false;
                break;
        }
    }

    // This function is used to swap the player character between the ground/ceiling state and the wall state and vice versa
    public void SwitchSurfaceAxis()
    {
        axisFlag = !axisFlag;
        box.size = new Vector2(box.size.y, box.size.x);
        box.offset = new Vector2(Mathf.Abs(box.offset.y) * (facingLeft ? 1 : -1), Mathf.Abs(box.offset.x) * (facingDown ? 1 : -1));
    }

    // This function is called whenever a shelled character asks to enter/exit their shell
    public virtual void ToggleShell()
    {
        if (stunned && !shelled)
            return;
        float[] disVars = new float[] { GetDistance(Dirs.Floor), GetDistance(Dirs.WallL), GetDistance(Dirs.WallR), GetDistance(Dirs.Ceiling) };
        if (shelled)
        {
            if (gravityDir == Dirs.WallL || gravityDir == Dirs.WallR)
            {
                if (disVars[(int)Dirs.Floor] < unshellAdjust && disVars[(int)Dirs.Ceiling] < unshellAdjust)
                    return;
                if (disVars[(int)Dirs.Floor] > unshellAdjust && disVars[(int)Dirs.Ceiling] < unshellAdjust)
                    transform.position = new Vector2(transform.position.x,
                        transform.position.y - (0.675f - disVars[(int)Dirs.Ceiling] - (facingDown ? 0.25f : 0)));
                else if (disVars[(int)Dirs.Floor] < unshellAdjust && disVars[(int)Dirs.Ceiling] > unshellAdjust)
                    transform.position = new Vector2(transform.position.x,
                        transform.position.y + (0.675f - disVars[(int)Dirs.Floor] - (facingDown ? 0 : 0.25f)));

                box.offset = new Vector2(hitboxOffset_normal.y * (facingLeft ? -1 : 1), hitboxOffset_normal.x * (facingDown ? -1 : 1));
                box.size = new Vector2(hitboxSize_normal.y, hitboxSize_normal.x);
            }
            else
            {
                if (disVars[(int)Dirs.WallL] < unshellAdjust && disVars[(int)Dirs.WallR] < unshellAdjust)
                    return;
                if (disVars[(int)Dirs.WallL] > unshellAdjust && disVars[(int)Dirs.WallR] < unshellAdjust)
                    transform.position = new Vector2(transform.position.x - (0.675f - disVars[(int)Dirs.WallR] - (facingLeft ? 0.25f : 0)),
                        transform.position.y);
                else if (disVars[(int)Dirs.WallL] < unshellAdjust && disVars[(int)Dirs.WallR] > unshellAdjust)
                    transform.position = new Vector2(transform.position.x + (0.675f - disVars[(int)Dirs.WallL] - (facingLeft ? 0 : 0.25f)),
                        transform.position.y);

                box.offset = new Vector2(hitboxOffset_normal.x * (facingLeft ? -1 : 1), hitboxOffset_normal.y * (facingDown ? -1 : 1));
                box.size = hitboxSize_normal;
            }
        }
        else
        {
            if (gravityDir == Dirs.WallL || gravityDir == Dirs.WallR)
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
        EjectFromCollisions();
    }

    // This function handles activation of projectiles when the player presses either shoot button
    public virtual void Shoot()
    {
        if (fireCooldown == 0 && armed && !PlayState.paralyzed)
        {
            Vector2 inputDir = new(Control.AxisX(), Control.AxisY());
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
                if (gravityDir == Dirs.Floor && (dir == 5 || dir == 6 || dir == 7))
                    dir = facingLeft ? 3 : 4;
                else if (gravityDir == Dirs.WallL && (dir == 0 || dir == 3 || dir == 5))
                    dir = facingDown ? 6 : 1;
                else if (gravityDir == Dirs.WallR && (dir == 2 || dir == 4 || dir == 7))
                    dir = facingDown ? 6 : 1;
                else if (gravityDir == Dirs.Ceiling && (dir == 0 || dir == 1 || dir == 2))
                    dir = facingLeft ? 3 : 4;
            }
            if (dir == -1)
            {
                if (gravityDir == Dirs.Floor && dir == -1)
                    dir = facingLeft ? 3 : 4;
                else if (gravityDir == Dirs.WallL && dir == -1)
                    dir = facingDown ? 6 : 1;
                else if (gravityDir == Dirs.WallR && dir == -1)
                    dir = facingDown ? 6 : 1;
                else if (gravityDir == Dirs.Ceiling && dir == -1)
                    dir = facingLeft ? 3 : 4;
            }
            if (!PlayState.globalFunctions.playerBulletPool.transform.GetChild(bulletID).GetComponent<Bullet>().isActive)
            {
                PlayState.globalFunctions.playerBulletPool.transform.GetChild(bulletID).GetComponent<Bullet>().Shoot(type, dir, applyRapidFireMultiplier == 1);
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
        if ((stunned && damage > 0) || inDeathCutscene || (PlayState.paralyzed && !PlayState.overrideParalysisInvulnerability))
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
        if (shelled)
            ToggleShell();
        stunned = true;
        if (!CheckAbility(stickToWallsWhenHurt))
            CorrectGravity(true, false);
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
        transform.parent = null;
        PlayState.globalFunctions.UpdateHearts();
        inDeathCutscene = true;
        box.enabled = false;
        PlayState.paralyzed = true;
        PlayState.PlaySound("Death");
        float timer = 0;
        bool hasStartedTransition = false;
        Vector3 fallDir = new(0.125f, 0.35f, 0);
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

    public void AddCollision(Collider2D collision)
    {
        if (!collisions.Contains(collision))
            collisions.Add(collision);
    }

    public void EjectFromCollisions()
    {
        if (inDeathCutscene)
            return;

        while (PlayState.IsPointPlayerCollidable(transform.position))
        {
            transform.position += gravityDir switch {
                Dirs.WallL => Vector3.right,
                Dirs.WallR => Vector3.left,
                Dirs.Ceiling => Vector3.down,
                _ => Vector3.up
            };
        }
        Vector2 tweakDis = Vector2.zero;
        if (GetDistance(Dirs.Floor, true) < box.size.y * 0.5f)
            tweakDis.y = (box.size.y * 0.5f) - lastDistance + PlayState.FRAC_128;
        if (GetDistance(Dirs.WallL, true) < box.size.x * 0.5f)
            tweakDis.x = (box.size.x * 0.5f) - lastDistance + PlayState.FRAC_128;
        if (GetDistance(Dirs.WallR, true) < box.size.x * 0.5f)
            tweakDis.x = (-box.size.x * 0.5f) + lastDistance - PlayState.FRAC_128;
        if (GetDistance(Dirs.Ceiling, true) < box.size.y * 0.5f)
            tweakDis.y = (-box.size.y * 0.5f) + lastDistance - PlayState.FRAC_128;
        transform.position += (Vector3)tweakDis;
    }
    public void EjectFromCollisions(Dirs from)
    {
        while (PlayState.IsPointPlayerCollidable(transform.position))
        {
            transform.position += from switch
            {
                Dirs.WallL => Vector3.right,
                Dirs.WallR => Vector3.left,
                Dirs.Ceiling => Vector3.down,
                _ => Vector3.up
            };
        }
        Vector2 tweakDis = Vector2.zero;
        if (from == Dirs.Floor && GetDistance(Dirs.Floor, true) < box.size.y * 0.5f)
            tweakDis.y = (box.size.y * 0.5f) - lastDistance + PlayState.FRAC_128;
        if (from == Dirs.WallL && GetDistance(Dirs.WallL, true) < box.size.x * 0.5f)
            tweakDis.x = (box.size.x * 0.5f) - lastDistance + PlayState.FRAC_128;
        if (from == Dirs.WallR && GetDistance(Dirs.WallR, true) < box.size.x * 0.5f)
            tweakDis.x = (-box.size.x * 0.5f) + lastDistance - PlayState.FRAC_128;
        if (from == Dirs.Ceiling && GetDistance(Dirs.Ceiling, true) < box.size.y * 0.5f)
            tweakDis.y = (-box.size.y * 0.5f) + lastDistance - PlayState.FRAC_128;
        transform.position += (Vector3)tweakDis;
    }

    public void ZeroWalkVelocity()
    {
        if (gravityDir == Dirs.WallL || gravityDir == Dirs.WallR)
            velocity.y = 0;
        else
            velocity.x = 0;
    }

    #endregion Player utilities
}
