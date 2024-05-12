using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blobby : Player
{
    private const float WALL_SLIDE_ACCEL = 0.125f;
    private const float WALL_SLIDE_MAX = 4.5f;
    private const float WALL_JUMP_VEL = 12.5f;
    private const int NON_ANGEL_JUMP_COUNT = 1;
    private const int ANGEL_JUMP_COUNT = 3;

    private float wallJumpTempVel = 0;
    private int jumpsLeft;

    // This function is called the moment the script is loaded. I use it to initialize a lot of variables and such
    public override void OnEnable()
    {
        base.OnEnable();

        defaultGravityDir = Dirs.Floor;
        canJump = new int[][] { new int[] { -1 } };
        canSwapGravity = new int[][] { new int[] { 4 } };
        retainGravityOnAirborne = new int[][] { new int[] { -2 } };
        canGravityJumpOpposite = new int[][] { new int[] { -2 } };
        canGravityJumpAdjacent = new int[][] { new int[] { -2 } };
        canGravityShock = new int[][] { new int[] { -1 } };
        shellable = new int[][] { new int[] { 5 } };
        hopWhileMoving = new int[][] { new int[] { -1 } };
        hopPower = 22.5f;
        canRoundInnerCorners = new int[][] { new int[] { -2 } };
        canRoundOuterCorners = new int[][] { new int[] { -2 } };
        canRoundOppositeOuterCorners = new int[][] { new int[] { -2 } };
        stickToWallsWhenHurt = new int[][] { new int[] { 4 } };
        runSpeed = new float[] { 8.3333f, 8.3333f, 8.3333f, 10.5f };
        jumpPower = new float[] { 29.5f, 29.5f, 29.5f, 29.5f, 29.5f, 29.5f, 29.5f, 29.5f };
        gravity = new float[] { 1.5f, 1.5f, 1.5f, 1.5f };
        terminalVelocity = new float[] { -0.7208f, -0.7208f, -0.7208f, -0.7208f };
        jumpFloatiness = new float[] { 4, 4, 4, 4, 4, 4, 4, 4 };
        weaponCooldowns = new float[] { 0.54f, 0.405f, 0.095f, 0.0475f, 0.35f, 0.175f, 0.185f, 0.0925f, 0f, 0f };
        applyRapidFireMultiplier = true;
        idleTimer = 30;
        hitboxSize_normal = new Vector2(0.96f, 0.96f);
        hitboxSize_shell = new Vector2(0.96f, 0.96f);
        hitboxOffset_normal = Vector2.zero;
        hitboxOffset_shell = Vector2.zero;
        unshellAdjust = 0f;
        shellTurnaroundAdjust = 0f;
        coyoteTime = 0.0625f;
        jumpBuffer = 0.125f;
        gravShockChargeTime = 0.75f;
        gravShockChargeMult = 0.5f;
        gravShockSpeed = 40.0f;
        gravShockSteering = 2.75f;
        damageMultiplier = 1f;
        healthGainFromParry = 4;

        int[] tempData = PlayState.GetAnim("Player_Blobby_data").frames;
        animData = new bool[tempData.Length];
        for (int i = 0; i < tempData.Length; i++)
            animData[i] = tempData[i] == 1;

        PlayState.currentProfile.character = "Blobby";

        anim.ClearList();
        for (int i = 0; i <= 3; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                foreach (string lrAnim in new string[] { "floor_{0}_idle", "floor_{0}_hop", "floor_{0}_jump", "floor_{0}_wallJump",
                    "floor_{0}_ceilJump", "floor_{0}_shell", "shock_{0}", })
                {
                    anim.Add("Player_Blobby" + i + (j == 0 ? "A" : "B") + "_" + string.Format(lrAnim, "left"));
                    anim.Add("Player_Blobby" + i + (j == 0 ? "A" : "B") + "_" + string.Format(lrAnim, "right"));
                }
                anim.Add("Player_Blobby" + i + (j == 0 ? "A" : "B") + "_wallL_settle");
                anim.Add("Player_Blobby" + i + (j == 0 ? "A" : "B") + "_wallL_slide");
                anim.Add("Player_Blobby" + i + (j == 0 ? "A" : "B") + "_wallR_settle");
                anim.Add("Player_Blobby" + i + (j == 0 ? "A" : "B") + "_wallR_slide");
                anim.Add("Player_Blobby" + i + (j == 0 ? "A" : "B") + "_ceiling_settle");
                anim.Add("Player_Blobby" + i + (j == 0 ? "A" : "B") + "_ceiling_slideL");
                anim.Add("Player_Blobby" + i + (j == 0 ? "A" : "B") + "_ceiling_slideR");
                anim.Add("Player_Blobby" + i + (j == 0 ? "A" : "B") + "_die");
            }
        }
        PlayState.SetCamFocus(camFocus);
    }

    public override void IdleAnim()
    {
        if (!shelled)
            ToggleShell();
        idleParticles.Add(PlayState.RequestParticle(new Vector2(transform.position.x + 0.75f, transform.position.y), "zzz"));
    }

    // LateUpdate() is called after everything else a frame needs has been handled. Here, it's used for animations
    public override void LateUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        /*\
         *   ANIMATION DATA VALUES
         * 0 - Update animation on move
         * 1 - Update animation on turnaround
         * 2 - Update animation when off ground
         * 3 - Flip X on ground
         * 4 - Flip X on ceiling
         * 5 - Flip X on right wall
        \*/
        string currentState = "Player_Blobby" + PlayState.globalFunctions.shellStateBuffer + (PlayState.CheckForItem(5) ? "B" : "A") + "_";
        if (inDeathCutscene)
        {
            anim.Play(currentState + "die");
            return;
        }

        sprite.flipX = false;
        sprite.flipY = false;

        if (gravShockState > 0)
        {
            sprite.flipX = facingLeft && animData[5];
            currentState += "shock_" + (facingLeft ? "left" : "right");
        }
        else
        {
            switch (gravityDir)
            {
                default:
                case Dirs.Floor:
                    sprite.flipX = facingLeft && animData[3];
                    currentState += "floor_" + (facingLeft ? "left_" : "right_");
                    if (shelled)
                        currentState += "shell";
                    else if (lastGravity == Dirs.Ceiling)
                        currentState += "ceilJump";
                    else if (lastGravity != Dirs.Floor)
                        currentState += "wallJump";
                    else if (!grounded && !ungroundedViaHop)
                        currentState += "jump";
                    else if (!grounded && ungroundedViaHop)
                        currentState += "hop";
                    else
                        currentState += "idle";
                    break;
                case Dirs.WallL:
                    currentState += "wallL_";
                    if (velocity.y != 0)
                        currentState += "slide";
                    else
                        currentState += "settle";
                    break;
                case Dirs.WallR:
                    sprite.flipX = !facingLeft && animData[5];
                    currentState += "wallR_";
                    if (velocity.y != 0)
                        currentState += "slide";
                    else
                        currentState += "settle";
                    break;
                case Dirs.Ceiling:
                    sprite.flipX = facingLeft && animData[4];
                    currentState += "ceiling_";
                    if (velocity.x < 0)
                        currentState += "slideL";
                    else if (velocity.x > 0)
                        currentState += "slideR";
                    else
                        currentState += "settle";
                    break;
            }
        }

        if (currentState != anim.currentAnimName)
            anim.Play(currentState);

        sprite.flipX = forceFaceH != 1 && (forceFaceH == -1 || sprite.flipX);
        sprite.flipY = forceFaceV == 1 || (forceFaceV != -1 && sprite.flipY);
    }
    public override void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game || PlayState.noclipMode)
            return;

        // To start things off, we mark our current position as the last position we took. Same with our hitbox size
        // Among other things, this is used to test for ground when we're airborne
        lastPosition = new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y);
        lastSize = box.size;
        lastGravity = gravityDir;
        groundedLastFrame = grounded;
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
        // We increment the Gravity Shock timer in case it happens to be active
        if (gravShockState < 0)
            gravShockState++;
        gravShockTimer = gravShockState > 0 ? gravShockTimer + Time.fixedDeltaTime : 0;
        // We update our home direction assuming gravity keep behavior is set to any state change
        if (PlayState.generalData.gravKeepType != 1)
            homeGravity = defaultGravityDir;
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
            if (velocity.x == Mathf.Infinity || velocity.x == -Mathf.Infinity)
                velocity.x = 0;
            if (velocity.y == Mathf.Infinity || velocity.y == -Mathf.Infinity)
                velocity.y = 0;
            velocity.x += wallJumpTempVel;
            transform.position += (Vector3)velocity;
            if (wallJumpTempVel != 0)
                wallJumpTempVel = grounded ? 0 : Mathf.Lerp(wallJumpTempVel, 0, 4 * Time.fixedDeltaTime);

            if (Control.ShootPress() && PlayState.generalData.shootMode)
                toggleModeActive = !toggleModeActive;
            else if (!PlayState.generalData.shootMode)
                toggleModeActive = false;
            if ((Control.ShootHold() || Control.StrafeHold() || toggleModeActive || Control.Aim() != Vector2.zero) && !PlayState.paralyzed)
            {
                if (shelled)
                    ToggleShell();
                Shoot();
            }

            if (gravShockState != 2)
                EjectFromCollisions();

            // Hey, do we happen to be stuck falling on a corner here?
            if (lastPosition == (Vector2)transform.position && !grounded && !groundedLastFrame && (gravityDir == lastGravity))
            {
                transform.position += PlayState.FRAC_64 * gravityDir switch
                {
                    Dirs.Floor => new Vector3(1, Control.AxisX(), 0),
                    Dirs.WallL => new Vector3(Control.AxisY(), 1, 0),
                    Dirs.WallR => new Vector3(Control.AxisY(), -1, 0),
                    _ => new Vector3(-1, Control.AxisX(), 0)
                };
            }

            //Vector2 halfBox = box.size * 0.5f;
            //Debug.DrawLine(new(transform.position.x - halfBox.x, transform.position.y - halfBox.y, 0),
            //    new(transform.position.x + halfBox.x, transform.position.y + halfBox.y, 0), Color.red, 3f);
            //Debug.DrawLine(new(transform.position.x - halfBox.x, transform.position.y + halfBox.y, 0),
            //    new(transform.position.x + halfBox.x, transform.position.y - halfBox.y, 0), Color.red, 3f);

            // I don't know why I have to do this, but nothing else I did worked.
            if (gravityDir == Dirs.Ceiling && Control.AxisX() == 0 && transform.position.x != lastPosition.x)
                transform.position = new Vector2(lastPosition.x, transform.position.y);
        }

        // Down here, we handle general Gravity Shock stuff
        if (gravShockState > 0)
        {
            if (gravShockCharge != null)
                gravShockCharge.transform.position = transform.position;
            if (gravShockBody != null)
                gravShockBody.transform.position = transform.position;
            if (gravShockBullet != null)
                gravShockBullet.transform.position = transform.position;
        }

        // Lastly, after all of that, we update the camera's focus point around the player
        camFocus.position = (Vector2)transform.position + camFocusOffset;
        Vector2 camBoundsX = new(
            PlayState.camCenter.x - PlayState.camBoundaryBuffers.x + PlayState.camTempBuffersX.x - 12.5f,
            PlayState.camCenter.x + PlayState.camBoundaryBuffers.x - PlayState.camTempBuffersX.y + 12.5f);
        Vector2 camBoundsY = new(
            PlayState.camCenter.y - PlayState.camBoundaryBuffers.y + PlayState.camTempBuffersY.x - 7.5f,
            PlayState.camCenter.y + PlayState.camBoundaryBuffers.y - PlayState.camTempBuffersY.y + 7.5f);
        if (transform.position.x > camBoundsX.x && transform.position.x < camBoundsX.y && transform.position.y > camBoundsY.x && transform.position.y < camBoundsY.y)
            camFocus.position = new(
                Mathf.Clamp(camFocus.position.x, camBoundsX.x, camBoundsX.y),
                Mathf.Clamp(camFocus.position.y, camBoundsY.x, camBoundsY.y));
    }

    public override void CaseDown()
    {
        // Blobby has unique cases for most of their gravity states
        // The floor state is largely the same, with the only difference being the extra jump requirements

        // We start by zeroing our relatively vertical velocity if we happen to be on the ground. Just in case
        if (grounded)
            velocity.y = 0;
        // We also set this variable that will toggle the horizontal movement check. The corner-rounding check will turn this off to ensure
        // Snaily remains attached to the wall they turn onto, considering the vertical check is run before the horizontal check
        bool performHorizCheck = true;

        // Before we do any movement checks, let's check to see if Gravity Shock happens to be active
        if (gravShockState > 0)
        {
            // State 1 means we're in the inital pullback before Gravity Shock launches
            if (gravShockState == 1)
            {
                float riseSpeed = 0.5f * Time.fixedDeltaTime;
                if (GetDistance(Dirs.Ceiling) < Mathf.Abs(riseSpeed))
                    transform.position = new Vector2(transform.position.x, transform.position.y + lastDistance - PlayState.FRAC_32);
                else
                    transform.position = new Vector2(transform.position.x, transform.position.y + riseSpeed);
                // FIRE!!
                if (gravShockTimer > gravShockChargeTime * (PlayState.CheckForItem("Rapid Fire") ? gravShockChargeMult : 1))
                {
                    gravShockState = 2;
                    if (gravShockCharge != null)
                        gravShockCharge.ResetParticle();
                    gravShockCharge = null;
                    gravShockBody = PlayState.RequestParticle(transform.position, "shockcharmain", new float[]
                    {
                        PlayState.currentProfile.character switch { "Snaily" => 0, "Sluggy" => 1, "Upside" => 2, "Leggy" => 3, "Blobby" => 4, "Leechy" => 5, _ => 0 },
                        PlayState.CheckForItem(9) ? 1 : 0,
                        (int)gravityDir
                    });
                    gravShockBullet = Shoot(true);
                    PlayState.globalFunctions.ScreenShake(new List<float> { 0.25f, 0f }, new List<float> { 0.25f });
                    PlayState.RequestParticle(transform.position, "shocklaunch", new float[] { 0 });
                    camFocusOffset = new Vector2(0, -4);
                    sprite.enabled = false;
                }
            }
            // State 2 means we've successfully fired off
            else
            {
                float fallSpeed = -gravShockSpeed * Time.fixedDeltaTime;
                // This block checks if we've hit the floor and reverts us to normal if we have
                GetDistance(Dirs.Floor, out List<Collider2D> cols, out List<string> colNames, Mathf.Abs(fallSpeed));
                bool hitDoor = false;
                for (int i = 0; i < cols.Count; i++)
                {
                    if (colNames[i].Contains("Breakable Block"))
                        cols[i].GetComponent<BreakableBlock>().OnTriggerStay2D(cols[i]);
                    if (colNames[i].Contains("Door"))
                    {
                        cols[i].GetComponent<Door>().OnTriggerEnter2D(gravShockBullet.box);
                        if (cols[i].GetComponent<Door>().locked)
                            hitDoor = true;
                    }
                }
                if ((lastDistance < Mathf.Abs(fallSpeed) && (colNames.Contains("Ground") || colNames.Contains("Platform") || hitDoor)) || !gravShockBullet.isActive)
                {
                    if (!gravShockBullet.isActive)
                    {
                        gravShockBullet = null;
                        HitFor(0);
                    }
                    else
                    {
                        transform.position = new Vector2(transform.position.x, transform.position.y - lastDistance + PlayState.FRAC_32);
                        PlayState.globalFunctions.ScreenShake(new List<float> { 0.65f, 0f }, new List<float> { 0.75f }, 90f, 5f);
                        PlayState.PlaySound("Stomp");
                        if (gravShockBullet != null)
                        {
                            gravShockBullet.Despawn(true);
                            gravShockBullet = null;
                        }
                        SpawnShockWaves();
                    }
                    gravShockBody.ResetParticle();
                    gravShockState = 0;
                    camFocusOffset = Vector2.zero;
                    sprite.enabled = true;
                }
                else
                    transform.position = new Vector2(transform.position.x, transform.position.y + fallSpeed);
                float steering = Control.AxisX() * Time.deltaTime * gravShockSteering;
                if (GetDistance(steering < 0 ? Dirs.WallL : Dirs.WallR) < Mathf.Abs(steering))
                    transform.position = new Vector2(transform.position.x + ((lastDistance - PlayState.FRAC_32) * (steering < 0 ? -1 : 1)), transform.position.y);
                else
                    transform.position = new Vector2(transform.position.x + steering, transform.position.y);
            }
            return;
        }

        // Now that that's over...
        // First, we perform relatively vertical checks. Jumping and falling.
        if (!grounded)
        {
            if ((gravityDir != defaultGravityDir && !CheckAbility(retainGravityOnAirborne)) || CheckForHomeDirRequirements())
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
                    if (Control.UpHold() && CheckAbility(canSwapGravity) && CanChangeGravWhileStunned())
                    {
                        gravityDir = Dirs.Ceiling;
                        SwapDir(Dirs.Ceiling);
                        UpdateHitbox();
                        grounded = true;
                        ungroundedViaHop = false;
                        holdingShell = true;
                        wallJumpTempVel = 0;
                        jumpsLeft = (PlayState.CheckForItem(8) || (PlayState.CheckForItem(9) && PlayState.stackShells)) ? ANGEL_JUMP_COUNT : NON_ANGEL_JUMP_COUNT;
                        AddCollision(lastCollision);
                    }
                }
                // Is the player falling? Let's check for floors this time
                if (velocity.y < 0 && GetDistance(Dirs.Floor) < Mathf.Abs(velocity.y))
                {
                    velocity.y = -lastDistance + PlayState.FRAC_128;
                    grounded = true;
                    ungroundedViaHop = false;
                    jumpsLeft = (PlayState.CheckForItem(8) || (PlayState.CheckForItem(9) && PlayState.stackShells)) ? ANGEL_JUMP_COUNT : NON_ANGEL_JUMP_COUNT;
                    AddCollision(lastCollision);
                }
            }
        }
        else
        {
            // Are we suddenly in the air (considered when Snaily is at least one pixel above the nearest surface) when we weren't last frame?
            if (GetDistance(Dirs.Floor, true) > (box.size.y * 0.5f) + PlayState.FRAC_16)
            {
                bool fall = true;
                // Is the player holding down and forward? If so, let's see if there are any corners to round
                if (GetCornerDistance() <= (box.size.x * 0.75f) && CheckAbility(canRoundOuterCorners) && Control.DownHold() &&
                    (facingLeft ? Control.LeftHold() : Control.RightHold()) && CanChangeGravWhileStunned())
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
                        UpdateHitbox();
                        velocity = new((GetDistance(facingLeft ? Dirs.WallL : Dirs.WallR) * (facingLeft ? -1 : 1)) +
                            (PlayState.FRAC_128 * Mathf.Sign(Control.AxisX())), -PlayState.FRAC_16);
                        performHorizCheck = false;
                        AddCollision(lastCollision);
                        // Round that corner, you glorious little snail, you
                        fall = false;
                    }
                }
                // FALL
                if (fall)
                {
                    grounded = false;
                    if (!CheckAbility(retainGravityOnAirborne) && lastGravity != defaultGravityDir)
                        coyoteTimeCounter = coyoteTime;
                }
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
        if (Control.AxisX() != 0 && !Control.StrafeHold() && performHorizCheck)
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
                againstWallFlag = true;
                velocity.x = (lastDistance - PlayState.FRAC_128) * Mathf.Sign(Control.AxisX());
                wallJumpTempVel = 0;
                AddCollision(lastCollision);
                // Does the player happen to be trying to climb a wall?
                if (GetDistance(Dirs.Floor, true) + GetDistance(Dirs.Ceiling, true) > box.size.y + PlayState.FRAC_8 &&
                    CanChangeGravWhileStunned() && CheckAbility(canSwapGravity))
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
                        UpdateHitbox();
                        grounded = true;
                        jumpsLeft = (PlayState.CheckForItem(8) || (PlayState.CheckForItem(9) && PlayState.stackShells)) ? ANGEL_JUMP_COUNT : NON_ANGEL_JUMP_COUNT;
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
                    velocity.y = hopPower * jumpMod * Time.fixedDeltaTime;
                    lastPointBeforeHop = transform.position.y;
                }
            }
        }
        else if (Control.AxisX() == 0 || Control.StrafeHold())
            velocity.x = 0;

        // Now, let's see if we can jump
        if (CheckAbility(canJump) && Control.JumpHold() && (grounded || (coyoteTimeCounter < coyoteTime) || (ungroundedViaHop && (transform.position.y > lastPointBeforeHop))
            || (jumpsLeft > 0 && (grounded || ungroundedViaHop || !holdingJump))) && (!holdingJump || (jumpBufferCounter < jumpBuffer && velocity.y < 0))
            && GetDistance(Dirs.Ceiling) > 0.95f)
        {
            if (!(!grounded && !ungroundedViaHop && Control.DownHold() && Control.AxisX() == 0 && PlayState.CheckForItem(10)))
            {
                if (shelled)
                    ToggleShell();
                if ((PlayState.CheckForItem(8) || (PlayState.CheckForItem(9) && PlayState.stackShells)) && !grounded && !ungroundedViaHop)
                {
                    PlayState.RequestParticle(transform.position, "AngelJumpEffect");
                    PlayState.PlaySound("AngelJump");
                }
                else
                    PlayState.PlaySound("Jump");
                grounded = false;
                ungroundedViaHop = false;
                holdingJump = true;
                jumpsLeft--;
                if (gravityDir != defaultGravityDir)
                {
                    if (CheckAbility(retainGravityOnAirborne) && !CheckForHomeDirRequirements())
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
            }
        }
        // How about gravity jumping?
        int swapType = PlayState.generalData.gravSwapType;
        float maxSecs = Control.MAX_DOUBLE_TAP_SECONDS;
        if ((Control.JumpHold() || swapType == 2) && (!holdingJump || swapType > 0) && !grounded)
        {
            // Jumping in the same direction you're falling (and triggering Gravity Shock)
            if (CheckAbility(canGravityShock) && Control.AxisX() == 0 && PlayState.CheckForItem(10) && (
                (swapType == 0 && Control.DownHold()) ||
                (swapType == 1 && Control.DownHold() && !holdingShell) ||
                (swapType == 2 && Control.DownPress() && Control.secondsSinceLastDirTap[(int)Dirs.Floor] < maxSecs)))
            {
                gravShockState = 1;
                gravShockCharge = PlayState.RequestParticle(transform.position, "shockcharge");
                PlayState.PlaySound("ShockCharge");
                velocity = Vector2.zero;
                coyoteTimeCounter = coyoteTime;
            }
            // Jumping in the opposite direction
            else if (CheckAbility(canGravityJumpOpposite) && ((CheckAbility(canGravityJumpAdjacent) && (
                (swapType == 0 && Control.UpHold()) ||
                (swapType == 1 && Control.UpHold() && !holdingShell) ||
                (swapType == 2 && Control.UpPress() && Control.secondsSinceLastDirTap[(int)Dirs.Ceiling] < maxSecs)
                )) || (!CheckAbility(canGravityJumpAdjacent) && (
                swapType < 2 || (swapType == 2 && Control.UpPress() && Control.secondsSinceLastDirTap[(int)Dirs.Ceiling] < maxSecs)
                ))))
            {
                gravityDir = Dirs.Ceiling;
                if (PlayState.generalData.gravKeepType == 1)
                    homeGravity = Dirs.Ceiling;
                SwapDir(Dirs.Ceiling);
                UpdateHitbox();
                holdingShell = true;
                coyoteTimeCounter = coyoteTime;
                Control.secondsSinceLastDirTap[(int)Dirs.Ceiling] = maxSecs;
            }
            // Jumping to the left or right
            else if (CheckAbility(canGravityJumpAdjacent) && (
                (swapType == 0 && (Control.LeftHold() || Control.RightHold())) ||
                (swapType == 1 && (Control.LeftHold() || Control.RightHold()) && !holdingShell) ||
                (swapType == 2 && ((Control.LeftPress() && Control.secondsSinceLastDirTap[(int)Dirs.WallL] < maxSecs) ||
                    (Control.RightPress() && Control.secondsSinceLastDirTap[(int)Dirs.WallR] < maxSecs)))
                ))
            {
                Dirs newDir = Control.RightHold() ? Dirs.WallR : Dirs.WallL;
                gravityDir = newDir;
                if (PlayState.generalData.gravKeepType == 1)
                    homeGravity = newDir;
                SwapDir(newDir);
                SwitchSurfaceAxis();
                UpdateHitbox();
                holdingShell = true;
                coyoteTimeCounter = coyoteTime;
                Control.secondsSinceLastDirTap[(int)Dirs.WallL] = maxSecs;
                Control.secondsSinceLastDirTap[(int)Dirs.WallR] = maxSecs;
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
            !toggleModeActive &&
            !holdingShell && CheckAbility(shellable))
        {
            ToggleShell();
            holdingShell = true;
        }
        else if (!holdingShell && Control.DownHold())
            holdingShell = true;
        if (holdingShell && gravityDir == Dirs.Floor && !Control.DownHold())
            holdingShell = false;
    }

    public override void CaseLeft()
    {
        // Blobby has unique cases for most of their gravity states
        // The left state is a simple "stick to the wall, slide down, jump up" type deal

        velocity.x = 0;
        // Firstly, we'll handle going down
        if (Control.DownHold())
            velocity.y = Mathf.Clamp(velocity.y - WALL_SLIDE_ACCEL * Time.fixedDeltaTime, -WALL_SLIDE_MAX * Time.fixedDeltaTime, 0);
        else
            velocity.y = 0;
        // Have we hit the ground?
        if (Mathf.Abs(velocity.y) > GetDistance(Dirs.Floor))
        {
            gravityDir = Dirs.Floor;
            SwapDir(Dirs.WallR);
            holdingShell = true;
        }
        // We have not. This lets us jump
        else if (Control.JumpHold() && !holdingJump && GetDistance(Dirs.Ceiling) > 0.25f)
        {
            gravityDir = Dirs.Floor;
            SwapDir(Dirs.WallR);
            grounded = false;
            ungroundedViaHop = false;
            coyoteTimeCounter = coyoteTime;
            jumpsLeft--;
            velocity.y = jumpPower[readIDJump] * jumpMod * Time.fixedDeltaTime;
            wallJumpTempVel = WALL_JUMP_VEL * speedMod * Time.fixedDeltaTime;
            PlayState.PlaySound("Jump");
        }
        // Do we happen to be ungrounded by this event?
        if (GetDistance(Dirs.WallL) > 0.25f)
        {
            gravityDir = Dirs.Floor;
            SwapDir(Dirs.WallR);
            holdingShell = true;
            grounded = false;
            ungroundedViaHop = false;
            jumpsLeft--;
        }
        holdingJump = Control.JumpHold();
    }

    public override void CaseRight()
    {
        // Blobby has unique cases for most of their gravity states
        // The right state is a simple "stick to the wall, slide down, jump up" type deal

        velocity.x = 0;
        // Firstly, we'll handle going down
        if (Control.DownHold())
            velocity.y = Mathf.Clamp(velocity.y - WALL_SLIDE_ACCEL * Time.fixedDeltaTime, -WALL_SLIDE_MAX * Time.fixedDeltaTime, 0);
        else
            velocity.y = 0;
        // Have we hit the ground?
        if (Mathf.Abs(velocity.y) > GetDistance(Dirs.Floor))
        {
            gravityDir = Dirs.Floor;
            SwapDir(Dirs.WallL);
            holdingShell = true;
        }
        // We have not. This lets us jump
        else if (Control.JumpHold() && !holdingJump && GetDistance(Dirs.Ceiling) > 0.25f)
        {
            gravityDir = Dirs.Floor;
            SwapDir(Dirs.WallL);
            grounded = false;
            ungroundedViaHop = false;
            coyoteTimeCounter = coyoteTime;
            jumpsLeft--;
            velocity.y = jumpPower[readIDJump] * jumpMod * Time.fixedDeltaTime;
            wallJumpTempVel = -WALL_JUMP_VEL * speedMod * Time.fixedDeltaTime;
            PlayState.PlaySound("Jump");
        }
        // Do we happen to be ungrounded by this event?
        if (GetDistance(Dirs.WallR) > 0.25f)
        {
            gravityDir = Dirs.Floor;
            SwapDir(Dirs.WallL);
            holdingShell = true;
            grounded = false;
            ungroundedViaHop = false;
            jumpsLeft--;
        }
        holdingJump = Control.JumpHold();
    }

    public override void CaseUp()
    {
        // Blobby has unique cases for most of their gravity states
        // The up state is a simple "stick to the ceiling, slide left and right, jump down at partial max fall speed" type deal

        velocity.y = 0;
        // Firstly, we'll handle going horizontally
        if (Control.AxisX() != 0)
        {
            if ((velocity.x > 0 && Control.AxisX() == -1) || (velocity.x < 0 && Control.AxisX() == 1))
                velocity.x = 0;
            velocity.x = Mathf.Clamp(velocity.x + (WALL_SLIDE_ACCEL * Control.AxisX()) * Time.fixedDeltaTime,
                -WALL_SLIDE_MAX * Time.fixedDeltaTime, WALL_SLIDE_MAX * Time.fixedDeltaTime);
        }
        else
            velocity.x = 0;
        // Have we hit a wall?
        if ((velocity.x < 0 && Mathf.Abs(velocity.x) > GetDistance(Dirs.WallL)) || (velocity.x > 0 && Mathf.Abs(velocity.x) > GetDistance(Dirs.WallR)))
            velocity.x = (lastDistance - PlayState.FRAC_128) * Control.AxisX();
        // Are we trying to jump?
        if (Control.JumpHold() && !holdingJump && GetDistance(Dirs.Floor) > 0.25f)
        {
            gravityDir = Dirs.Floor;
            grounded = false;
            ungroundedViaHop = false;
            coyoteTimeCounter = coyoteTime;
            jumpsLeft--;
            velocity.y = terminalVelocity[readIDJump % 4] * 0.25f;
            PlayState.PlaySound("Jump");
        }
        // Do we happen to be ungrounded by this event?
        if (GetDistance(Dirs.Ceiling) > 0.25f)
        {
            gravityDir = Dirs.Floor;
            holdingShell = true;
            grounded = false;
            ungroundedViaHop = false;
            jumpsLeft--;
        }
        holdingJump = Control.JumpHold();
    }

    public override IEnumerator DieAndRespawn()
    {
        if (shelled)
            ToggleShell();
        health = 0;
        transform.parent = null;
        PlayState.globalFunctions.UpdateHearts();
        inDeathCutscene = true;
        box.enabled = false;
        if (gravShockState > 0)
        {
            gravShockState = 0;
            if (gravShockBullet != null)
                gravShockBullet.Despawn();
            gravShockBullet = null;
            if (gravShockCharge != null)
                gravShockCharge.ResetParticle();
            gravShockCharge = null;
        }
        PlayState.paralyzed = true;
        PlayState.PlaySound("Death");
        PlayState.areaOfDeath = PlayState.currentArea;
        for (int i = 4; i > 0; i--)
            PlayState.RequestParticle(new Vector2(Random.Range(transform.position.x - 0.5f, transform.position.x + 0.5f),
                Random.Range(transform.position.y - 0.5f, transform.position.y + 0.5f)), "explosion", new float[] { 2 });
        float timer = 0;
        bool hasStartedTransition = false;
        while (((timer < 1.6f && PlayState.quickDeathTransition) || (timer < 2 && !PlayState.quickDeathTransition))
            && !PlayState.resetInducingFadeActive && PlayState.gameState == PlayState.GameState.game)
        {
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
            if (timer > 1 && !hasStartedTransition)
            {
                hasStartedTransition = true;
                PlayState.ScreenFlash("Death Transition");
            }
        }
        yield return new WaitForEndOfFrame();
        if (!PlayState.resetInducingFadeActive && PlayState.gameState == PlayState.GameState.game)
        {
            if (PlayState.positionOfLastRoom == PlayState.positionOfLastSave)
            {
                Transform deathLocation = PlayState.roomTriggerParent.transform.GetChild((int)PlayState.positionOfLastRoom.x).GetChild((int)PlayState.positionOfLastRoom.y);
                deathLocation.GetComponent<Collider2D>().enabled = true;
                deathLocation.GetComponent<RoomTrigger>().active = true;
                deathLocation.GetComponent<RoomTrigger>().DespawnEverything();
            }
            if (PlayState.inBossFight)
                PlayState.ToggleBossfightState(false, 0, true);
            transform.position = PlayState.currentProfile.saveCoords;
            inDeathCutscene = false;
            box.enabled = true;
            PlayState.paralyzed = false;
            health = maxHealth;
            ResetState();
            PlayState.globalFunctions.UpdateHearts();
            yield return new WaitForEndOfFrame();
            PlayState.ScreenFlash("Room Transition");
        }
    }
}
