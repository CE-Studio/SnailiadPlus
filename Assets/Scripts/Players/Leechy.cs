using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leechy : Player
{
    private bool shouldBackfire = false;

    // This function is called the moment the script is loaded. I use it to initialize a lot of variables and such
    public override void OnEnable()
    {
        base.OnEnable();

        defaultGravityDir = Dirs.Floor;
        canJump = new int[][] { new int[] { -1 } };
        canSwapGravity = new int[][] { new int[] { -1 } };
        retainGravityOnAirborne = new int[][] { new int[] { 8 } };
        canGravityJumpOpposite = new int[][] { new int[] { 8 } };
        canGravityJumpAdjacent = new int[][] { new int[] { 8 } };
        canGravityShock = new int[][] { new int[] { -1 } };
        shellable = new int[][] { new int[] { -2 } };
        hopWhileMoving = new int[][] { new int[] { -2 } };
        hopPower = 0;
        canRoundInnerCorners = new int[][] { new int[] { -1 } };
        canRoundOuterCorners = new int[][] { new int[] { -1 } };
        canRoundOppositeOuterCorners = new int[][] { new int[] { 8 } };
        stickToWallsWhenHurt = new int[][] { new int[] { 8 } };
        runSpeed = new float[] { 9f, 9f, 9f, 12.6316f };
        jumpPower = new float[] { 26.5f, 26.5f, 26.5f, 26.5f, 31.125f, 31.125f, 31.125f, 31.125f };
        gravity = new float[] { 1.125f, 1.125f, 1.125f, 1.125f };
        terminalVelocity = new float[] { -0.5208f, -0.5208f, -0.5208f, -0.5208f };
        jumpFloatiness = new float[] { 4, 4, 4, 4, 4, 4, 4, 4 };
        weaponCooldowns = new float[] { 0.54f, 0.405f, 0.046f, 0.0414f, 0.23f, 0.207f, 0.13f, 0.117f, 0f, 0f };
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
        gravShockChargeTime = 0.625f;
        gravShockChargeMult = 0.5f;
        gravShockSpeed = 40.0f;
        gravShockSteering = 4f;
        damageMultiplier = 1.75f;
        healthGainFromParry = 0;

        int[] tempData = PlayState.GetAnim("Player_Leechy_data").frames;
        animData = new bool[tempData.Length];
        for (int i = 0; i < tempData.Length; i++)
            animData[i] = tempData[i] == 1;

        PlayState.currentProfile.character = "Leechy";

        anim.ClearList();
        string[] animDirections = new string[] { "floor_right", "floor_left", "ceiling_right", "ceiling_left", "wallR_down", "wallR_up", "wallL_down", "wallL_up" };
        string[] animStates = new string[] { "idle", "move", "air", "shock" };
        for (int i = 0; i <= 3; i++)
        {
            for (int j = 0; j < animDirections.Length; j++)
            {
                for (int k = 0; k < animStates.Length; k++)
                {
                    anim.Add("Player_Leechy" + i + "_" + animDirections[j] + "_" + animStates[k]);
                }
            }
            anim.Add("Player_Leechy" + i + "_die");
        }

        PlayState.SetCamFocus(camFocus);
    }

    public override void IdleAnim()
    {
        //idleParticles.Add(PlayState.RequestParticle(new Vector2(transform.position.x + 0.75f, transform.position.y), "zzz"));
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
        string currentState = "Player_Leechy" + PlayState.globalFunctions.shellStateBuffer + "_";
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

    public override Bullet Shoot(bool isShock = false)
    {
        if ((fireCooldown == 0 && armed && gravShockState == 0) || isShock)
        {
            Vector2 inputDir = new(Control.AxisX(), Control.AxisY());
            Vector2 aimDir = Control.Aim();
            int type = selectedWeapon + (PlayState.CheckForItem("Devastator") ? 3 : 0);
            int dir = 0;
            if (isShock)
            {
                type = PlayState.CheckForItem("Full-Metal Snail") ? 8 : 7;
                dir = gravityDir switch
                {
                    Dirs.Floor => 6,
                    Dirs.WallL => 3,
                    Dirs.WallR => 4,
                    Dirs.Ceiling => 1,
                    _ => 6
                };
            }
            else
            {
                string dirStr = inputDir.x + "" + inputDir.y;
                if (aimDir != Vector2.zero)
                    dirStr = aimDir.x + "" + aimDir.y;
                switch (dirStr)
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
            }

            Bullet thisBullet = PlayState.globalFunctions.playerBulletPool.transform.GetChild(bulletID).GetComponent<Bullet>();
            if (!thisBullet.isActive)
            {
                thisBullet.Shoot(type, dir, applyRapidFireMultiplier);
                if (!isShock)
                {
                    bool applyRapid = PlayState.CheckForItem("Rapid Fire") || (PlayState.CheckForItem("Devastator") && PlayState.stackWeaponMods);
                    int fireRateIndex = type - 1 - (type > 3 ? 3 : 0) + (applyRapid ? 3 : 0);
                    fireCooldown = weaponCooldowns[fireRateIndex];
                }
                PlayState.PlaySound(type switch
                {
                    1 => "ShotPeashooter",
                    2 => "ShotBoomerang",
                    3 => "ShotRainbow",
                    4 => "ShotPeashooterDev",
                    5 => "ShotBoomerangDev",
                    6 => "ShotRainbowDev",
                    7 => "ShockLaunch",
                    8 => "ShockLaunch",
                    _ => "ShotRainbow"
                });
                if (PlayState.isInBossRush)
                {
                    switch (type)
                    {
                        case 1: case 4: PlayState.activeRushData.peasFired++; break;
                        case 2: case 5: PlayState.activeRushData.boomsFired++; break;
                        case 3: case 6: PlayState.activeRushData.wavesFired++; break;
                        case 7: case 8: PlayState.activeRushData.shocksFired++; break;
                    }
                }
            }
            bulletID = (bulletID + 1) % PlayState.globalFunctions.playerBulletPool.transform.childCount;

            if (PlayState.CheckForItem("Rapid Fire"))
            {
                if (shouldBackfire && !isShock)
                {
                    Bullet otherBullet = PlayState.globalFunctions.playerBulletPool.transform.GetChild(bulletID).GetComponent<Bullet>();
                    if (!otherBullet.isActive)
                    {
                        int backDir = dir switch
                        {
                            0 => 7,
                            1 => 6,
                            2 => 5,
                            3 => 4,
                            4 => 3,
                            5 => 2,
                            6 => 1,
                            _ => 0
                        };
                        otherBullet.Shoot(type, backDir, applyRapidFireMultiplier);
                        bulletID = (bulletID + 1) % PlayState.globalFunctions.playerBulletPool.transform.childCount;
                    }
                }
                shouldBackfire = !shouldBackfire;
            }

            return thisBullet;
        }
        return null;
    }
}
