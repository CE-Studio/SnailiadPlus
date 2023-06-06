using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravTurtle1 : Enemy
{
    private readonly float[] jumpTimeouts = new float[]
    {
        2.5f, 2.3f, 3f, 2.1f, 2.7f, 2.6f, 2.9f, 2.1f, 2.3f, 3.1f, 3.3f, 2.9f,
        2.6f, 2.4f, 1.9f, 3.1f, 2.7f, 3.9f, 4.2f, 1.8f, 2.8f, 3.1f, 3.8f, 2.8f
    };
    private const float FLIP_TIMEOUT = 0.3f;
    private const float JUMP_POWER = 31.125f;
    private const float GRAVITY = 1.25f;
    private const float TURNAROUND_TIMEOUT = 1.8f;
    private const float WALL_CHECK_MOD = 2f;
    private const int CAST_COUNT = 3;
    private float WALKSPEED = 1.5f;

    private int jumpTimeoutIndex = 0;
    private float jumpTimeout = 0f;
    private float flipTimeout = 9999999f;
    private float turnaroundTimeout = TURNAROUND_TIMEOUT;
    private Vector2 velocity = Vector2.zero;
    private bool hasSwapped = false;
    private bool initializedFacing = false;

    public enum Dirs { floor, wallL, wallR, ceiling };
    private Dirs gravityDir;
    private bool facingLeft = false;
    private bool facingDown = true;
    private bool grounded = true;
    private float[] hitPoints = new float[] { 0, 0, 0, 0 };

    private enum AnimStates { walk, turn, jump, flipTo };
    private AnimStates animState = AnimStates.walk;

    private Vector2 boxSize;

    private int[] animData;
    /*\ 
     *    ANIMATION DATA VALUES
     *  0 - 
     *  1 - 
     *  2 - Flip sprite X on left wall
     *  3 - Flip sprite Y on ceiling
     *  4 - Frame to flip sprite X on floor turnaround
     *  5 - Frame to flip sprite Y on floor -> ceiling flip
     *  6 - Frame to flip sprite X on wallL turnaround
     *  7 - Frame to flip sprite Y on wallL -> wallR flip
     *  8 - Frame to flip sprite X on wallR turnaround
     *  9 - Frame to flip sprite Y on wallR -> wallL flip
     * 10 - Frame to flip sprite X on ceiling turnaround
     * 11 - Frame to flip sprite Y on ceiling -> floor flip
    \*/

    private BoxCollider2D box;

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(300, 2, 20, true);
        col.TryGetComponent(out box);

        boxSize = box.size;
        animData = PlayState.GetAnim("Enemy_gravturtle1_data").frames;

        if (PlayState.currentProfile.difficulty == 2)
            WALKSPEED *= 1.9f;

        jumpTimeoutIndex = Mathf.Abs(Mathf.RoundToInt(transform.position.x + transform.position.y)) % jumpTimeouts.Length;
        jumpTimeout = jumpTimeouts[jumpTimeoutIndex];

        string[] dirs = new string[] { "floor", "wallL", "wallR", "ceiling" };
        string[] states = new string[] { "walk", "turn", "jump", "flipTo" };
        for (int i = 0; i < dirs.Length; i++)
        {
            for (int j = 0; j < states.Length; j++)
                anim.Add("Enemy_gravturtle1_" + dirs[i] + "_" + states[j]);
        }
    }

    private void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (!initializedFacing)
        {
            SwapRelativeVertical(true);
            if (gravityDir == Dirs.wallL || gravityDir == Dirs.wallR)
            {
                if (PlayState.player.transform.position.y > transform.position.y)
                {
                    velocity.y = WALKSPEED * Time.fixedDeltaTime;
                    SwapFacing(true, true);
                }
                else
                    velocity.y = -WALKSPEED * Time.fixedDeltaTime;
            }
            else
            {
                if (PlayState.player.transform.position.x > transform.position.x)
                    velocity.x = WALKSPEED * Time.fixedDeltaTime;
                else
                {
                    velocity.x = -WALKSPEED * Time.fixedDeltaTime;
                    SwapFacing(true, true);
                }
            }
            initializedFacing = true;
        }

        if (PlayState.OnScreen(transform.position, box))
        {
            bool turn = false;

            jumpTimeout -= Time.fixedDeltaTime;
            if (jumpTimeout <= 0)
            {
                jumpTimeoutIndex = (++jumpTimeoutIndex) % jumpTimeouts.Length;
                jumpTimeout = jumpTimeouts[jumpTimeoutIndex];
                if (grounded)
                {
                    flipTimeout = FLIP_TIMEOUT;
                    velocity = gravityDir switch
                    {
                        Dirs.floor => new Vector2(velocity.x, JUMP_POWER * Time.fixedDeltaTime),
                        Dirs.wallL => new Vector2(JUMP_POWER * Time.fixedDeltaTime, velocity.y),
                        Dirs.wallR => new Vector2(-JUMP_POWER * Time.fixedDeltaTime, velocity.y),
                        _ => new Vector2(velocity.x, -JUMP_POWER * Time.fixedDeltaTime)
                    };
                    PlayState.PlaySound("Jump");
                    animState = AnimStates.jump;
                    grounded = false;
                }
                else
                    flipTimeout = 0;
            }
            flipTimeout -= Time.fixedDeltaTime;
            if (flipTimeout <= 0)
            {
                flipTimeout = 9999999f;
                animState = AnimStates.flipTo;
                SetGravity(gravityDir switch
                {
                    Dirs.floor => Dirs.ceiling,
                    Dirs.wallL => Dirs.wallR,
                    Dirs.wallR => Dirs.wallL,
                    _ => Dirs.floor
                });
            }
            turnaroundTimeout -= Time.fixedDeltaTime;
            if (turnaroundTimeout <= 0)
            {
                turnaroundTimeout = TURNAROUND_TIMEOUT;
                if (gravityDir == Dirs.wallL || gravityDir == Dirs.wallR)
                {
                    if ((facingDown && PlayState.player.transform.position.y < transform.position.y) ||
                        (!facingDown && PlayState.player.transform.position.y > transform.position.y))
                    {
                        turn = true;
                        if (grounded)
                            animState = AnimStates.turn;
                        velocity.y = -velocity.y;
                    }
                }
                else
                {
                    if ((facingLeft && PlayState.player.transform.position.x < transform.position.x) ||
                        (!facingLeft && PlayState.player.transform.position.x > transform.position.x))
                    {
                        turn = true;
                        if (grounded)
                            animState = AnimStates.turn;
                        velocity.x = -velocity.x;
                    }
                }
            }

            float disDown = GetDistance(Dirs.floor);
            float disLeft = GetDistance(Dirs.wallL);
            float disRight = GetDistance(Dirs.wallR);
            float disUp = GetDistance(Dirs.ceiling);
            switch (gravityDir)
            {
                case Dirs.floor:
                    if (facingLeft && disLeft < Mathf.Abs(velocity.x) * WALL_CHECK_MOD)
                    {
                        velocity.x = WALKSPEED * Time.fixedDeltaTime;
                        turn = true;
                    }
                    if (!facingLeft && disRight < Mathf.Abs(velocity.x) * WALL_CHECK_MOD)
                    {
                        velocity.x = -WALKSPEED * Time.fixedDeltaTime;
                        turn = true;
                    }
                    if (grounded && disDown > 0.25f)
                        grounded = false;
                    else if (!grounded)
                    {
                        velocity.y -= GRAVITY * Time.fixedDeltaTime;
                        if (disUp < Mathf.Abs(velocity.y) && velocity.y > 0)
                        {
                            transform.position = new Vector2(transform.position.x, hitPoints[3] - (box.size.y * 0.5f) - PlayState.FRAC_128);
                            velocity.y = 0;
                            flipTimeout = 0;
                        }
                        else if (disDown < Mathf.Abs(velocity.y) && velocity.y < 0)
                        {
                            transform.position = new Vector2(transform.position.x, hitPoints[0] + (box.size.y * 0.5f) + PlayState.FRAC_128);
                            velocity.y = 0;
                            grounded = true;
                        }
                    }
                    break;
                case Dirs.wallL:
                    if (facingDown && disDown < Mathf.Abs(velocity.y) * WALL_CHECK_MOD)
                    {
                        velocity.y = WALKSPEED * Time.fixedDeltaTime;
                        turn = true;
                    }
                    if (!facingDown && disUp < Mathf.Abs(velocity.y) * WALL_CHECK_MOD)
                    {
                        velocity.y = -WALKSPEED * Time.fixedDeltaTime;
                        turn = true;
                    }
                    if (grounded && disLeft > 0.25f)
                        grounded = false;
                    else if (!grounded)
                    {
                        velocity.x -= GRAVITY * Time.fixedDeltaTime;
                        if (disRight < Mathf.Abs(velocity.x) && velocity.x > 0)
                        {
                            transform.position = new Vector2(hitPoints[2] - (box.size.x * 0.5f) - PlayState.FRAC_128, transform.position.y);
                            velocity.x = 0;
                            flipTimeout = 0;
                        }
                        else if (disLeft < Mathf.Abs(velocity.x) && velocity.x < 0)
                        {
                            transform.position = new Vector2(hitPoints[1] + (box.size.x * 0.5f) + PlayState.FRAC_128, transform.position.y);
                            velocity.x = 0;
                            grounded = true;
                        }
                    }
                    break;
                case Dirs.wallR:
                    if (facingDown && disDown < Mathf.Abs(velocity.y) * WALL_CHECK_MOD)
                    {
                        velocity.y = WALKSPEED * Time.fixedDeltaTime;
                        turn = true;
                    }
                    if (!facingDown && disUp < Mathf.Abs(velocity.y) * WALL_CHECK_MOD)
                    {
                        velocity.y = -WALKSPEED * Time.fixedDeltaTime;
                        turn = true;
                    }
                    if (grounded && disRight > 0.25f)
                        grounded = false;
                    else if (!grounded)
                    {
                        velocity.x += GRAVITY * Time.fixedDeltaTime;
                        if (disRight < Mathf.Abs(velocity.x) && velocity.x > 0)
                        {
                            transform.position = new Vector2(hitPoints[2] - (box.size.x * 0.5f) - PlayState.FRAC_128, transform.position.y);
                            velocity.x = 0;
                            grounded = true;
                        }
                        else if (disLeft < Mathf.Abs(velocity.x) && velocity.x < 0)
                        {
                            transform.position = new Vector2(hitPoints[1] + (box.size.x * 0.5f) + PlayState.FRAC_128, transform.position.y);
                            velocity.x = 0;
                            flipTimeout = 0;
                        }
                    }
                    break;
                case Dirs.ceiling:
                    if (facingLeft && disLeft < Mathf.Abs(velocity.x) * WALL_CHECK_MOD)
                    {
                        velocity.x = WALKSPEED * Time.fixedDeltaTime;
                        turn = true;
                    }
                    if (!facingLeft && disRight < Mathf.Abs(velocity.x) * WALL_CHECK_MOD)
                    {
                        velocity.x = -WALKSPEED * Time.fixedDeltaTime;
                        turn = true;
                    }
                    if (grounded && disUp > 0.25f)
                        grounded = false;
                    else if (!grounded)
                    {
                        velocity.y += GRAVITY * Time.fixedDeltaTime;
                        if (disUp < Mathf.Abs(velocity.y) && velocity.y > 0)
                        {
                            transform.position = new Vector2(transform.position.x, hitPoints[3] - (box.size.y * 0.5f) - PlayState.FRAC_128);
                            velocity.y = 0;
                            grounded = true;
                        }
                        else if (disDown < Mathf.Abs(velocity.y) && velocity.y < 0)
                        {
                            transform.position = new Vector2(transform.position.x, hitPoints[0] + (box.size.y * 0.5f) + PlayState.FRAC_128);
                            velocity.y = 0;
                            flipTimeout = 0;
                        }
                    }
                    break;
            }
            if (turn)
            {
                SwapFacing(true, false);
                animState = AnimStates.turn;
            }
            transform.position += (Vector3)velocity;
        }
    }

    private void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (PlayState.OnScreen(transform.position, box))
        {
            string animPrefix = "Enemy_gravturtle1_" + gravityDir.ToString() + "_";
            switch (animState)
            {
                case AnimStates.walk:
                    if (anim.currentAnimName != animPrefix + "walk")
                        anim.Play(animPrefix + "walk");
                    hasSwapped = false;
                    break;
                case AnimStates.turn:
                    if (anim.currentAnimName != animPrefix + "turn" && !hasSwapped)
                        anim.Play(animPrefix + "turn");
                    if (animData[GetAnimTransitionID()] == -1)
                    {
                        SwapFacing(false, true);
                        anim.Stop();
                    }
                    else if (anim.GetCurrentFrame() >= animData[GetAnimTransitionID()] && !hasSwapped)
                    {
                        SwapFacing(false, true);
                        hasSwapped = true;
                    }
                    if (!anim.isPlaying)
                        animState = AnimStates.walk;
                    break;
                case AnimStates.jump:
                    if (anim.currentAnimName != animPrefix + "jump")
                        anim.Play(animPrefix + "jump");
                    hasSwapped = false;
                    break;
                case AnimStates.flipTo:
                    if (animData[GetAnimTransitionID()] == -1)
                    {
                        SwapRelativeVertical();
                        animState = AnimStates.walk;
                        return;
                    }
                    else if (anim.currentAnimName != animPrefix + "flipTo")
                        anim.Play(animPrefix + "flipTo");
                    if (anim.GetCurrentFrame() >= animData[GetAnimTransitionID()] && !hasSwapped)
                    {
                        hasSwapped = true;
                        SwapRelativeVertical();
                    }
                    if (!anim.isPlaying)
                        animState = AnimStates.walk;
                    break;
            }
            if (animState != AnimStates.flipTo)
                SwapRelativeVertical(true);
        }
    }

    public void SetGravity(Dirs dir)
    {
        gravityDir = dir;
        switch (dir)
        {
            case Dirs.floor:
                box.size = boxSize;
                facingDown = true;
                break;
            case Dirs.wallL:
                box.size = new Vector2(boxSize.y, boxSize.x);
                facingLeft = true;
                break;
            case Dirs.wallR:
                box.size = new Vector2(boxSize.y, boxSize.x);
                facingLeft = false;
                break;
            case Dirs.ceiling:
                box.size = boxSize;
                facingDown = false;
                break;
        }
    }

    private void SwapFacing(bool flipBool, bool flipSprite)
    {
        switch (gravityDir)
        {
            case Dirs.floor:
            case Dirs.ceiling:
                if (flipBool)
                    facingLeft = !facingLeft;
                if (animData[2] == 1 && flipSprite)
                    sprite.flipX = !sprite.flipX;
                break;
            case Dirs.wallL:
            case Dirs.wallR:
                if (flipBool)
                    facingDown = !facingDown;
                if (animData[3] == 1 && flipSprite)
                    sprite.flipY = !sprite.flipY;
                break;
        }
    }

    private void SwapRelativeVertical(bool align = false)
    {
        switch (gravityDir)
        {
            case Dirs.floor:
                if (animData[2] == 1)
                    sprite.flipY = !align && !sprite.flipY;
                break;
            case Dirs.wallL:
                if (animData[3] == 1)
                    sprite.flipX = align || !sprite.flipX;
                break;
            case Dirs.wallR:
                if (animData[3] == 1)
                    sprite.flipX = !align && !sprite.flipX;
                break;
            case Dirs.ceiling:
                if (animData[2] == 1)
                    sprite.flipY = align || !sprite.flipY;
                break;
        }
    }

    private float GetDistance(Dirs dir)
    {
        float shortestDis = Mathf.Infinity;
        Vector2 a = (Vector2)transform.position - (box.size * 0.5f);
        Vector2 b = (Vector2)transform.position + (box.size * 0.5f);
        Vector2 origin;
        RaycastHit2D hit;
        for (int i = 0; i < CAST_COUNT; i++)
        {
            float t = (float)i / (float)(CAST_COUNT - 1);
            switch (dir)
            {
                default:
                case Dirs.floor:
                    origin = Vector2.Lerp(a, new Vector2(b.x, a.y), t);
                    hit = Physics2D.Raycast(origin, Vector2.down, Mathf.Infinity, enemyCollide);
                    break;
                case Dirs.wallL:
                    origin = Vector2.Lerp(a, new Vector2(a.x, b.y), t);
                    hit = Physics2D.Raycast(origin, Vector2.left, Mathf.Infinity, enemyCollide);
                    break;
                case Dirs.wallR:
                    origin = Vector2.Lerp(new Vector2(b.x, a.y), b, t);
                    hit = Physics2D.Raycast(origin, Vector2.right, Mathf.Infinity, enemyCollide);
                    break;
                case Dirs.ceiling:
                    origin = Vector2.Lerp(new Vector2(a.x, b.y), b, t);
                    hit = Physics2D.Raycast(origin, Vector2.up, Mathf.Infinity, enemyCollide);
                    break;
            }
            if (hit.collider != null && !PlayState.IsPointEnemyCollidable(origin))
            {
                if (shortestDis > hit.distance)
                {
                    shortestDis = hit.distance;
                    switch (dir)
                    {
                        case Dirs.floor:
                            hitPoints[0] = hit.point.y;
                            break;
                        case Dirs.wallL:
                            hitPoints[1] = hit.point.x;
                            break;
                        case Dirs.wallR:
                            hitPoints[2] = hit.point.x;
                            break;
                        case Dirs.ceiling:
                            hitPoints[3] = hit.point.y;
                            break;
                    }
                }
                Debug.DrawLine(origin, hit.point, Color.red, 0);
            }
        }
        return shortestDis;
    }

    private int GetAnimTransitionID()
    {
        /*\
         *  4 - Frame to flip sprite X on floor turnaround
         *  5 - Frame to flip sprite Y on floor -> ceiling flip
         *  6 - Frame to flip sprite X on wallL turnaround
         *  7 - Frame to flip sprite Y on wallL -> wallR flip
         *  8 - Frame to flip sprite X on wallR turnaround
         *  9 - Frame to flip sprite Y on wallR -> wallL flip
         * 10 - Frame to flip sprite X on ceiling turnaround
         * 11 - Frame to flip sprite Y on ceiling -> floor flip
        \*/

        switch (gravityDir)
        {
            case Dirs.floor:
                if (animState == AnimStates.turn)
                    return 4;
                if (animState == AnimStates.flipTo)
                    return 11;
                break;
            case Dirs.wallL:
                if (animState == AnimStates.turn)
                    return 6;
                if (animState == AnimStates.flipTo)
                    return 9;
                break;
            case Dirs.wallR:
                if (animState == AnimStates.turn)
                    return 8;
                if (animState == AnimStates.flipTo)
                    return 7;
                break;
            case Dirs.ceiling:
                if (animState == AnimStates.turn)
                    return 10;
                if (animState == AnimStates.flipTo)
                    return 5;
                break;
        }
        return -1;
    }
}
