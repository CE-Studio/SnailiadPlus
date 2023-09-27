using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snaily : Player
{
    // This function is called the moment the script is loaded. I use it to initialize a lot of variables and such
    public override void Start()
    {
        base.Start();

        defaultGravityDir = Dirs.Floor;
        canJump = new int[][] { new int[] { -1 } };
        canSwapGravity = new int[][] { new int[] { -1 } };
        retainGravityOnAirborne = new int[][] { new int[] { 8 } };
        canGravityJumpOpposite = new int[][] { new int[] { 8 } };
        canGravityJumpAdjacent = new int[][] { new int[] { 8 } };
        shellable = new int[][] { new int[] { -1 } };
        hopWhileMoving = new int[][] { new int[] { -2 } };
        hopPower = 0;
        canRoundInnerCorners = new int[][] { new int[] { -1 } };
        canRoundOuterCorners = new int[][] { new int[] { -1 } };
        canRoundOppositeOuterCorners = new int[][] { new int[] { 8 } };
        stickToWallsWhenHurt = new int[][] { new int[] { 8 } };
        runSpeed = new float[] { 8.6667f, 8.6667f, 8.6667f, 11 };
        jumpPower = new float[] { 26.5f, 26.5f, 26.5f, 26.5f, 31.125f, 31.125f, 31.125f, 31.125f };
        gravity = new float[] { 1.5f, 1.5f, 1.5f, 1.5f };
        terminalVelocity = new float[] { -0.5208f, -0.5208f, -0.5208f, -0.5208f };
        jumpFloatiness = new float[] { 4, 4, 4, 4, 4, 4, 4, 4 };
        weaponCooldowns = new float[] { 0.085f, 0.3f, 0.17f, 0.0425f, 0.15f, 0.085f };
        applyRapidFireMultiplier = 1;
        idleTimer = 30;
        hitboxSize_normal = new Vector2(1.467508f, 0.96f);
        hitboxSize_shell = new Vector2(0.75f, 0.96f);
        hitboxOffset_normal = Vector2.zero;
        hitboxOffset_shell = new Vector2(-0.186518f, 0);
        unshellAdjust = 0.4f;
        shellTurnaroundAdjust = 0.1667f;
        coyoteTime = 0.0625f;
        jumpBuffer = 0.125f;

        int[] tempData = PlayState.GetAnim("Player_Snaily_data").frames;
        animData = new bool[tempData.Length];
        for (int i = 0; i < tempData.Length; i++)
            animData[i] = tempData[i] == 1;

        PlayState.currentProfile.character = "Snaily";

        string[] animDirections = new string[] { "floor_right", "floor_left", "ceiling_right", "ceiling_left", "wallR_down", "wallR_up", "wallL_down", "wallL_up" };
        string[] animStates = new string[] { "idle", "move", "shell", "air" };
        for (int i = 0; i <= 3; i++)
        {
            for (int j = 0; j < animDirections.Length; j++)
            {
                for (int k = 0; k < animStates.Length; k++)
                {
                    anim.Add("Player_Snaily" + i + "_" + animDirections[j] + "_" + animStates[k]);
                }
            }
            anim.Add("Player_Snaily" + i + "_die");
        }
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
        string currentState = "Player_Snaily" + PlayState.globalFunctions.shellStateBuffer + "_";
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
    
        if (shelled)
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
}
