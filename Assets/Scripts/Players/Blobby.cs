using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blobby : Player
{
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
        terminalVelocity = new float[] { -1.5208f, -1.5208f, -1.5208f, -1.5208f };
        jumpFloatiness = new float[] { 4, 4, 4, 4, 4, 4, 4, 4 };
        weaponCooldowns = new float[] { 0.095f, 0.35f, 0.185f, 0.0475f, 0.175f, 0.0925f };
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
        for (int i = Random.Range(1, 4); i > 0; i--)
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
