using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leggy : Player
{
    private bool hasSwappedOnce = false;

    // This function is called the moment the script is loaded. I use it to initialize a lot of variables and such
    public override void OnEnable()
    {
        base.OnEnable();

        defaultGravityDir = Dirs.Floor;
        canJump = new int[][] { new int[] { -1 } };
        canSwapGravity = new int[][] { new int[] { -1, -3 } };
        retainGravityOnAirborne = new int[][] { new int[] { -1 } };
        canGravityJumpOpposite = new int[][] { new int[] { -1 } };
        canGravityJumpAdjacent = new int[][] { new int[] { -2 } };
        canGravityShock = new int[][] { new int[] { -1 } };
        shellable = new int[][] { new int[] { -1 } };
        hopWhileMoving = new int[][] { new int[] { -2 } };
        hopPower = 0;
        canRoundInnerCorners = new int[][] { new int[] { -2 } };
        canRoundOuterCorners = new int[][] { new int[] { -2 } };
        canRoundOppositeOuterCorners = new int[][] { new int[] { -2 } };
        stickToWallsWhenHurt = new int[][] { new int[] { -1 } };
        runSpeed = new float[] { 7.8f, 7.8f, 7.8f, 10 };
        jumpPower = new float[] { 23.5f, 23.5f, 23.5f, 23.5f, 30f, 30f, 30f, 30f };
        gravity = new float[] { 1.5f, 1.5f, 1.5f, 1.5f };
        terminalVelocity = new float[] { -0.5208f, -0.5208f, -0.5208f, -0.5208f };
        jumpFloatiness = new float[] { 4, 4, 4, 4, 4, 4, 4, 4 };
        weaponCooldowns = new float[] { 0.54f, 0.405f, 0.085f, 0.0425f, 0.3f, 0.15f, 0.17f, 0.085f, 0f, 0f };
        applyRapidFireMultiplier = true;
        idleTimer = 30;
        hitboxSize_normal = new Vector2(1.467508f, 0.96f);
        hitboxSize_shell = new Vector2(0.75f, 0.96f);
        hitboxOffset_normal = Vector2.zero;
        hitboxOffset_shell = new Vector2(-0.186518f, 0);
        unshellAdjust = 0.4f;
        shellTurnaroundAdjust = 0.1667f;
        coyoteTime = 0.0625f;
        jumpBuffer = 0.125f;
        gravShockChargeTime = 0.95f;
        gravShockChargeMult = 0.55f;
        gravShockSpeed = 40.0f;
        gravShockSteering = 2.75f;
        damageMultiplier = 0.8f;
        healthGainFromParry = 4;

        int[] tempData = PlayState.GetAnim("Player_Snaily_data").frames;
        animData = new bool[tempData.Length];
        for (int i = 0; i < tempData.Length; i++)
            animData[i] = tempData[i] == 1;

        PlayState.currentProfile.character = "Snaily";

        anim.ClearList();
        string[] animDirections = new string[] { "floor_right", "floor_left", "ceiling_right", "ceiling_left", "wallR_down", "wallR_up", "wallL_down", "wallL_up" };
        string[] animStates = new string[] { "idle", "move", "shell", "air", "shock" };
        for (int i = 0; i <= 3; i++)
        {
            for (int j = 0; j < animDirections.Length; j++)
            {
                for (int k = 0; k < animStates.Length; k++)
                {
                    anim.Add("Player_Leggy" + i + "_" + animDirections[j] + "_" + animStates[k]);
                }
            }
            anim.Add("Player_Leggy" + i + "_die");
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
         * 5 - Flip X on left wall
         * 6 - Flip Y on right wall
         * 7 - Flip Y on left wall
         * 8 - Flip Y on ceiling 
        \*/
        string currentState = "Player_Leggy" + PlayState.globalFunctions.shellStateBuffer + "_";
        if (inDeathCutscene)
        {
            anim.Play(currentState + "die");
            return;
        }

        sprite.flipX = false;
        sprite.flipY = false;

        if (gravityDir == Dirs.WallL)
        {
            if (animData[5])
                sprite.flipX = true;
            if (animData[1])
                currentState += "wallR_";
            else
                currentState += "wallL_";

            if (!facingDown && animData[7])
                sprite.flipY = true;
            if (!facingDown && animData[1])
                currentState += "down_";
            else if (!facingDown)
                currentState += "up_";
            else
                currentState += "down_";
        }
        else if (gravityDir == Dirs.WallR)
        {
            currentState += "wallR_";
            if (!facingDown && animData[6])
                sprite.flipY = true;
            if (!facingDown && animData[1])
                currentState += "down_";
            else if (!facingDown)
                currentState += "up_";
            else
                currentState += "down_";
        }
        else if (gravityDir == Dirs.Ceiling)
        {
            if (animData[8])
                sprite.flipY = true;
            if (animData[1])
                currentState += "floor_";
            else
                currentState += "ceiling_";

            if (facingLeft && animData[4])
                sprite.flipX = true;
            if (facingLeft && animData[1])
                currentState += "right_";
            else if (facingLeft)
                currentState += "left_";
            else
                currentState += "right_";
        }
        else
        {
            currentState += "floor_";
            if (facingLeft && animData[3])
                sprite.flipX = true;
            if (facingLeft && animData[1])
                currentState += "right_";
            else if (facingLeft)
                currentState += "left_";
            else
                currentState += "right_";
        }

        if (gravShockState > 0)
            currentState += "shock";
        else if (shelled)
            currentState += "shell";
        else if (!grounded && animData[2])
            currentState += "air";
        else if ((((gravityDir == Dirs.WallL || gravityDir == Dirs.WallR) && Control.AxisY() != 0) ||
            ((gravityDir == Dirs.Floor || gravityDir == Dirs.Ceiling) && Control.AxisY() != 0)) && animData[0])
            currentState += "move";
        else
            currentState += "idle";

        if (currentState != anim.currentAnimName)
            anim.Play(currentState);

        sprite.flipX = forceFaceH != 1 && (forceFaceH == -1 || sprite.flipX);
        sprite.flipY = forceFaceV == 1 || (forceFaceV != -1 && sprite.flipY);
    }

    public override void CaseDown()
    {
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
                if (gravShockTimer > gravShockChargeTime * (PlayState.CheckForItem(PlayState.Items.RapidFire) ? gravShockChargeMult : 1))
                {
                    gravShockState = 2;
                    if (gravShockCharge != null)
                        gravShockCharge.ResetParticle();
                    gravShockCharge = null;
                    gravShockBody = PlayState.RequestParticle(transform.position, "shockcharmain", new float[]
                    {
                        PlayState.currentProfile.character switch { "Snaily" => 0, "Sluggy" => 1, "Upside" => 2, "Leggy" => 3, "Blobby" => 4, "Leechy" => 5, _ => 0 },
                        PlayState.CheckForItem(PlayState.Items.MetalShell) ? 1 : 0,
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
                {
                    velocity.y = PlayState.Integrate(velocity.y, 0,
                        jumpFloatiness[readIDSpeed + (PlayState.CheckForItem(PlayState.Items.HighJump) ? 4 : 0)], Time.fixedDeltaTime);
                }
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
                        hasSwappedOnce = false;
                        AddCollision(lastCollision);
                    }
                }
                // Is the player falling? Let's check for floors this time
                if (velocity.y < 0 && GetDistance(Dirs.Floor) < Mathf.Abs(velocity.y))
                {
                    velocity.y = -lastDistance + PlayState.FRAC_128;
                    grounded = true;
                    ungroundedViaHop = false;
                    hasSwappedOnce = false;
                    AddCollision(lastCollision);
                }
            }
        }
        else
        {
            // Are we suddenly in the air (considered when Snaily is at least one pixel above the nearest surface) when we weren't last frame?
            if (GetDistance(Dirs.Floor, true) > (box.size.y * 0.5f) + PlayState.FRAC_16)
            {
                //bool fall = true;
                //// Is the player holding down and forward? If so, let's see if there are any corners to round
                //if (GetCornerDistance() <= (box.size.x * 0.75f) && CheckAbility(canRoundOuterCorners) && Control.DownHold() &&
                //    (facingLeft ? Control.LeftHold() : Control.RightHold()) && CanChangeGravWhileStunned())
                //{
                //    // Can we even round these corners at all? This check assumes our default gravity state means this corner is considered a ceiling corner
                //    if (!CheckAbility(canRoundOppositeOuterCorners) && ((defaultGravityDir == Dirs.WallL && Control.AxisX() == -1) ||
                //        (defaultGravityDir == Dirs.WallR && Control.AxisX() == 1) || defaultGravityDir == Dirs.Ceiling))
                //    {
                //        CorrectGravity(true);
                //        if (defaultGravityDir switch { Dirs.WallL => Control.LeftHold(), Dirs.WallR => Control.RightHold(), _ => Control.UpHold() })
                //            holdingShell = true;
                //    }
                //    // Getting here means we can round this corner! We need to reorient ourselves and ensure we're actually the right distance from the wall
                //    else
                //    {
                //        gravityDir = facingLeft ? Dirs.WallR : Dirs.WallL;
                //        SwapDir(gravityDir);
                //        SwitchSurfaceAxis();
                //        UpdateHitbox();
                //        velocity = new((GetDistance(facingLeft ? Dirs.WallL : Dirs.WallR) * (facingLeft ? -1 : 1)) +
                //            (PlayState.FRAC_128 * Mathf.Sign(Control.AxisX())), -PlayState.FRAC_16);
                //        performHorizCheck = false;
                //        AddCollision(lastCollision);
                //        // Round that corner, you glorious little snail, you
                //        fall = false;
                //    }
                //}
                //// FALL
                //if (fall)
                //{
                    grounded = false;
                    if (!CheckAbility(retainGravityOnAirborne) && lastGravity != defaultGravityDir)
                        coyoteTimeCounter = coyoteTime;
                //}
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
                AddCollision(lastCollision);
                // Does the player happen to be trying to climb a wall?
                //if (GetDistance(Dirs.Floor, true) + GetDistance(Dirs.Ceiling, true) > box.size.y + PlayState.FRAC_8 &&
                //    CanChangeGravWhileStunned() && CheckAbility(canSwapGravity))
                //{
                //    if ((Control.UpHold() && !grounded) ||
                //        (Control.UpHold() && grounded && CheckAbility(canRoundInnerCorners)) ||
                //        (Control.DownHold() && !grounded))
                //    {
                //        if (shelled)
                //            ToggleShell();
                //        SwitchSurfaceAxis();
                //        if (GetDistance(Dirs.Floor, true) < (box.size.y * 0.5f))
                //            transform.position += new Vector3(0, (box.size.y * 0.5f) - lastDistance + PlayState.FRAC_128, 0);
                //        if (GetDistance(Dirs.Ceiling, true) < (box.size.y * 0.5f))
                //            transform.position += new Vector3(0, -((box.size.y * 0.5f) - lastDistance) - PlayState.FRAC_128, 0);
                //        velocity = new((GetDistance(facingLeft ? Dirs.WallL : Dirs.WallR) - PlayState.FRAC_128) * (facingLeft ? -1 : 1), 0);
                //        if (Control.UpHold())
                //            SwapDir(Dirs.Ceiling);
                //        gravityDir = facingLeft ? Dirs.WallL : Dirs.WallR;
                //        UpdateHitbox();
                //        grounded = true;
                //    }
                //}
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
        else if (Control.AxisX() == 0 || Control.StrafeHold())
            velocity.x = 0;

        // Now, let's see if we can jump
        if (CheckAbility(canJump) && Control.JumpHold() && (grounded || (coyoteTimeCounter < coyoteTime) || (ungroundedViaHop && (transform.position.y > lastPointBeforeHop)))
            && (!holdingJump || (jumpBufferCounter < jumpBuffer && velocity.y < 0)) && GetDistance(Dirs.Ceiling) > 0.95f)
        {
            if (shelled)
                ToggleShell();
            grounded = false;
            holdingJump = true;
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
            PlayState.PlaySound("Jump");
        }
        // How about gravity jumping?
        int swapType = PlayState.generalData.gravSwapType;
        float maxSecs = Control.MAX_DOUBLE_TAP_SECONDS;
        if ((Control.JumpHold() || swapType == 2) && (!holdingJump || swapType > 0) && !grounded && CheckAbility(canSwapGravity))
        {
            // Jumping in the same direction you're falling (and triggering Gravity Shock)
            if (CheckAbility(canGravityShock) && Control.AxisX() == 0 && PlayState.CheckForItem(PlayState.Items.GravShock) && (
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
                ))) && !hasSwappedOnce)
            {
                gravityDir = Dirs.Ceiling;
                if (PlayState.generalData.gravKeepType == 1)
                    homeGravity = Dirs.Ceiling;
                SwapDir(Dirs.Ceiling);
                UpdateHitbox();
                holdingShell = true;
                coyoteTimeCounter = coyoteTime;
                Control.secondsSinceLastDirTap[(int)Dirs.Ceiling] = maxSecs;
                if (!PlayState.CheckForItem(PlayState.Items.FlyShell))
                    hasSwappedOnce = true;
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

    public override void CaseUp()
    {
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
                if (GetDistance(Dirs.Floor) < Mathf.Abs(riseSpeed))
                    transform.position = new Vector2(transform.position.x, transform.position.y - lastDistance + PlayState.FRAC_32);
                else
                    transform.position = new Vector2(transform.position.x, transform.position.y - riseSpeed);
                // FIRE!!
                if (gravShockTimer > gravShockChargeTime * (PlayState.CheckForItem(PlayState.Items.RapidFire) ? gravShockChargeMult : 1))
                {
                    gravShockState = 2;
                    if (gravShockCharge != null)
                        gravShockCharge.ResetParticle();
                    gravShockCharge = null;
                    gravShockBody = PlayState.RequestParticle(transform.position, "shockcharmain", new float[]
                    {
                        PlayState.currentProfile.character switch { "Snaily" => 0, "Sluggy" => 1, "Upside" => 2, "Leggy" => 3, "Blobby" => 4, "Leechy" => 5, _ => 0 },
                        PlayState.CheckForItem(PlayState.Items.MetalShell) ? 1 : 0,
                        (int)gravityDir
                    });
                    gravShockBullet = Shoot(true);
                    PlayState.globalFunctions.ScreenShake(new List<float> { 0.25f, 0f }, new List<float> { 0.25f });
                    PlayState.RequestParticle(transform.position, "shocklaunch", new float[] { 3 });
                    camFocusOffset = new Vector2(0, 4);
                    sprite.enabled = false;
                }
            }
            // State 2 means we've successfully fired off
            else
            {
                float fallSpeed = gravShockSpeed * Time.fixedDeltaTime;
                // This block checks if we've hit the floor and reverts us to normal if we have
                GetDistance(Dirs.Ceiling, out List<Collider2D> cols, out List<string> colNames, Mathf.Abs(fallSpeed));
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
                        transform.position = new Vector2(transform.position.x, transform.position.y + lastDistance - PlayState.FRAC_32);
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
                if (defaultGravityDir != Dirs.Floor)
                    EjectFromCollisions(Dirs.Ceiling);
            }
            else
            {
                // Vertical velocity is decreased by the gravity scale every physics update. If the jump button is down during the first half of the jump arc,
                // the player's fall is slowed, granting additional height for as long as the button is down
                velocity.y += gravity[readIDSpeed] * gravityMod * Time.fixedDeltaTime;
                if (velocity.y < 0 && !holdingJump)
                {
                    velocity.y = PlayState.Integrate(velocity.y, 0,
                        jumpFloatiness[readIDSpeed + (PlayState.CheckForItem(PlayState.Items.HighJump) ? 4 : 0)], Time.fixedDeltaTime);
                }
                velocity.y = Mathf.Clamp(velocity.y, -Mathf.Infinity, -terminalVelocity[readIDSpeed]);

                // Real quick, in case we're running our face into a wall, let's check to see if there are any tunnels for us to slip into
                if ((Control.LeftHold() && GetDistance(Dirs.WallL) < PlayState.FRAC_64) || (Control.RightHold() && GetDistance(Dirs.WallR) < PlayState.FRAC_64))
                    TestForTunnel();

                // Is the player rising? Let's check for ceilings
                if (velocity.y < 0 && GetDistance(Dirs.Floor) < Mathf.Abs(velocity.y))
                {
                    velocity.y = -lastDistance + PlayState.FRAC_128;
                    // Can the player grab the ceiling?
                    if (Control.DownHold() && CheckAbility(canSwapGravity) && CanChangeGravWhileStunned())
                    {
                        gravityDir = Dirs.Floor;
                        SwapDir(Dirs.Floor);
                        UpdateHitbox();
                        grounded = true;
                        ungroundedViaHop = false;
                        holdingShell = true;
                        hasSwappedOnce = false;
                        AddCollision(lastCollision);
                    }
                }
                // Is the player falling? Let's check for floors this time
                if (velocity.y > 0 && GetDistance(Dirs.Ceiling) < Mathf.Abs(velocity.y))
                {
                    velocity.y = lastDistance - PlayState.FRAC_128;
                    grounded = true;
                    ungroundedViaHop = false;
                    hasSwappedOnce = false;
                    AddCollision(lastCollision);
                }
            }
        }
        else
        {
            // Are we suddenly in the air (considered when Snaily is at least one pixel above the nearest surface) when we weren't last frame?
            if (GetDistance(Dirs.Ceiling, true) > (box.size.y * 0.5f) + PlayState.FRAC_16)
            {
                //bool fall = true;
                //// Is the player holding down and forward? If so, let's see if there are any corners to round
                //if (GetCornerDistance() <= (box.size.x * 0.75f) && CheckAbility(canRoundOuterCorners) && Control.UpHold() &&
                //    (facingLeft ? Control.LeftHold() : Control.RightHold()) && CanChangeGravWhileStunned())
                //{
                //    // Can we even round these corners at all? This check assumes our default gravity state means this corner is considered a ceiling corner
                //    if (!CheckAbility(canRoundOppositeOuterCorners) && ((defaultGravityDir == Dirs.WallL && Control.AxisX() == -1) ||
                //        (defaultGravityDir == Dirs.WallR && Control.AxisX() == 1) || defaultGravityDir == Dirs.Floor))
                //    {
                //        CorrectGravity(true);
                //        if (defaultGravityDir switch { Dirs.WallL => Control.LeftHold(), Dirs.WallR => Control.RightHold(), _ => Control.DownHold() })
                //            holdingShell = true;
                //    }
                //    // Getting here means we can round this corner! We need to reorient ourselves and ensure we're actually the right distance from the wall
                //    else
                //    {
                //        gravityDir = facingLeft ? Dirs.WallR : Dirs.WallL;
                //        SwapDir(gravityDir);
                //        SwitchSurfaceAxis();
                //        UpdateHitbox();
                //        velocity = new((GetDistance(facingLeft ? Dirs.WallL : Dirs.WallR) * (facingLeft ? -1 : 1)) +
                //            (PlayState.FRAC_128 * Mathf.Sign(Control.AxisX())), PlayState.FRAC_16);
                //        performHorizCheck = false;
                //        AddCollision(lastCollision);
                //        // Round that corner, you glorious little snail, you
                //        fall = false;
                //    }
                //}
                //// FALL
                //if (fall)
                //{
                    grounded = false;
                    if (!CheckAbility(retainGravityOnAirborne) && lastGravity != defaultGravityDir)
                        coyoteTimeCounter = coyoteTime;
                //}
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
                velocity.x = (lastDistance - PlayState.FRAC_128) * Mathf.Sign(Control.AxisX());
                AddCollision(lastCollision);
                // Does the player happen to be trying to climb a wall?
                //if (GetDistance(Dirs.Floor, true) + GetDistance(Dirs.Ceiling, true) > box.size.y + PlayState.FRAC_8 &&
                //    CanChangeGravWhileStunned() && CheckAbility(canSwapGravity))
                //{
                //    if ((Control.DownHold() && !grounded) ||
                //        (Control.DownHold() && grounded && CheckAbility(canRoundInnerCorners)) ||
                //        (Control.UpHold() && !grounded))
                //    {
                //        if (shelled)
                //            ToggleShell();
                //        SwitchSurfaceAxis();
                //        if (GetDistance(Dirs.Floor, true) < (box.size.y * 0.5f))
                //            transform.position += new Vector3(0, (box.size.y * 0.5f) - lastDistance + PlayState.FRAC_128, 0);
                //        if (GetDistance(Dirs.Ceiling, true) < (box.size.y * 0.5f))
                //            transform.position += new Vector3(0, -((box.size.y * 0.5f) - lastDistance) - PlayState.FRAC_128, 0);
                //        velocity = new((GetDistance(facingLeft ? Dirs.WallL : Dirs.WallR) - PlayState.FRAC_128) * (facingLeft ? -1 : 1), 0);
                //        if (Control.DownHold())
                //            SwapDir(Dirs.Floor);
                //        gravityDir = facingLeft ? Dirs.WallL : Dirs.WallR;
                //        UpdateHitbox();
                //        grounded = true;
                //    }
                //}
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
        else if (Control.AxisX() == 0 || Control.StrafeHold())
            velocity.x = 0;

        // Now, let's see if we can jump
        if (CheckAbility(canJump) && Control.JumpHold() && (grounded || (coyoteTimeCounter < coyoteTime) || (ungroundedViaHop && (transform.position.y < lastPointBeforeHop)))
            && (!holdingJump || (jumpBufferCounter < jumpBuffer && velocity.y > 0)) && GetDistance(Dirs.Floor) > 0.95f)
        {
            if (shelled)
                ToggleShell();
            grounded = false;
            holdingJump = true;
            if (gravityDir != defaultGravityDir)
            {
                if (CheckAbility(retainGravityOnAirborne) && !CheckForHomeDirRequirements())
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
        int swapType = PlayState.generalData.gravSwapType;
        float maxSecs = Control.MAX_DOUBLE_TAP_SECONDS;
        if ((Control.JumpHold() || swapType == 2) && (!holdingJump || swapType > 0) && !grounded && CheckAbility(canSwapGravity))
        {
            // Jumping in the same direction you're falling (and triggering Gravity Shock)
            if (CheckAbility(canGravityShock) && Control.AxisX() == 0 && PlayState.CheckForItem(PlayState.Items.GravShock) && (
                (swapType == 0 && Control.UpHold()) ||
                (swapType == 1 && Control.UpHold() && !holdingShell) ||
                (swapType == 2 && Control.UpPress() && Control.secondsSinceLastDirTap[(int)Dirs.Ceiling] < maxSecs)))
            {
                gravShockState = 1;
                gravShockCharge = PlayState.RequestParticle(transform.position, "shockcharge");
                PlayState.PlaySound("ShockCharge");
                velocity = Vector2.zero;
                coyoteTimeCounter = coyoteTime;
            }
            // Jumping in the opposite direction
            else if (CheckAbility(canGravityJumpOpposite) && ((CheckAbility(canGravityJumpAdjacent) && (
                (swapType == 0 && Control.DownHold()) ||
                (swapType == 1 && Control.DownHold() && !holdingShell) ||
                (swapType == 2 && Control.DownPress() && Control.secondsSinceLastDirTap[(int)Dirs.Floor] < maxSecs)
                )) || (!CheckAbility(canGravityJumpAdjacent) && (
                swapType < 2 || (swapType == 2 && Control.DownPress() && Control.secondsSinceLastDirTap[(int)Dirs.Floor] < maxSecs)
                ))) && !hasSwappedOnce)
            {
                gravityDir = Dirs.Floor;
                if (PlayState.generalData.gravKeepType == 1)
                    homeGravity = Dirs.Floor;
                SwapDir(Dirs.Floor);
                UpdateHitbox();
                holdingShell = true;
                coyoteTimeCounter = coyoteTime;
                Control.secondsSinceLastDirTap[(int)Dirs.Floor] = maxSecs;
                if (!PlayState.CheckForItem(PlayState.Items.FlyShell))
                    hasSwappedOnce = true;
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
        if (Control.UpHold() &&
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
        else if (!holdingShell && Control.UpHold())
            holdingShell = true;
        if (holdingShell && gravityDir == Dirs.Ceiling && !Control.UpHold())
            holdingShell = false;
    }
}
