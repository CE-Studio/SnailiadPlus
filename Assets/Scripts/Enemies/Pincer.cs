using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pincer : Enemy
{
    private const float SPEED = 12.5f;
    private const float SHOT_TIMEOUT = 0.4f;
    private const float BASE_JUMP_POWER = 8.125f;
    private const float GRAVITY = 1.25f;
    private const float DECELERATION = SPEED * 0.04f;
    private const float WEAPON_SPEED = 6.5f;

    private readonly float[] moveTimeouts = new float[]
    {
        0.4f, 0.3f, 0.4f, 0.2f, 0.4f, 0.3f, 0.4f, 0.2f, 0.4f, 0.3f, 0.4f, 0.2f, 0.2f, 0.2f, 0.2f, 0.1f, 0.4f
    };
    private readonly float[] jumpHeights = new float[]
    {
        1f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 0.5f, 0.5f, 0.5f, 0f, 2.5f
    };

    private bool[] flipData;

    private float moveTimeout;
    private int moveTimeoutIndex = 0;
    private float reactDistance = 33.75f;
    private float shotTimeout = 0.4f;
    private Vector2 velocity = Vector2.zero;
    private bool grounded = true;
    private PlayState.EDirsCardinal gravState;
    private Vector2 halfBox;
    private float lastDistance;

    public bool axis = false;
    private bool facingDownLeft = true;

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(850, 6, 10, true);

        for (int i = 0; i < 2; i++)
        {
            string action = i == 0 ? "walk" : "jump";
            anim.Add("Enemy_pincer_floor_" + action + "L");
            anim.Add("Enemy_pincer_floor_" + action + "R");
            anim.Add("Enemy_pincer_wallL_" + action + "D");
            anim.Add("Enemy_pincer_wallL_" + action + "U");
            anim.Add("Enemy_pincer_wallR_" + action + "D");
            anim.Add("Enemy_pincer_wallR_" + action + "U");
            anim.Add("Enemy_pincer_ceiling_" + action + "L");
            anim.Add("Enemy_pincer_ceiling_" + action + "R");
        }

        int[] rawAnimData = PlayState.GetAnim("Enemy_pincer_data").frames;
        flipData = new bool[] { rawAnimData[0] == 1, rawAnimData[1] == 1 };

        moveTimeoutIndex = Mathf.RoundToInt(transform.position.x * 0.0625f + transform.position.y * 0.0625f) % moveTimeouts.Length;
        moveTimeout = moveTimeouts[moveTimeoutIndex];

        col.TryGetComponent(out BoxCollider2D box);
        halfBox = box.size * 0.5f;
    }

    public void SetGravity(PlayState.EDirsCardinal dir)
    {
        gravState = dir;
        switch (dir)
        {
            case PlayState.EDirsCardinal.Down:
            case PlayState.EDirsCardinal.Left:
                break;
            case PlayState.EDirsCardinal.Right:
                if (flipData[1])
                    sprite.flipX = true;
                break;
            case PlayState.EDirsCardinal.Up:
                if (flipData[0])
                    sprite.flipY = true;
                break;
        }
    }

    private void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (PlayState.OnScreen(transform.position, col))
        {
            moveTimeout -= Time.fixedDeltaTime;
            if (moveTimeout <= 0 && (axis ? (Mathf.Abs(transform.position.y - PlayState.player.transform.position.y) <= reactDistance) :
                (Mathf.Abs(transform.position.x - PlayState.player.transform.position.x) <= reactDistance)))
            {
                moveTimeoutIndex = ++moveTimeoutIndex % moveTimeouts.Length;
                moveTimeout = moveTimeouts[moveTimeoutIndex];
                switch (gravState)
                {
                    case PlayState.EDirsCardinal.Down:
                        if (Random.Range(0f, 1f) > 0.77f)
                            velocity.x = (Random.Range(0f, 1f) < 0.5f ? -SPEED : SPEED) * Time.fixedDeltaTime;
                        else if (PlayState.player.transform.position.x < transform.position.x)
                            velocity.x = -SPEED * Time.fixedDeltaTime;
                        else
                            velocity.x = SPEED * Time.fixedDeltaTime;
                        PlayAnim("walk", velocity.x < 0);
                        if (Random.Range(0f, 1f) > 0.9f && grounded)
                        {
                            velocity.y = BASE_JUMP_POWER * jumpHeights[moveTimeoutIndex] * Time.fixedDeltaTime;
                            grounded = false;
                            PlayAnim("jump", velocity.x < 0);
                        }
                        break;
                    case PlayState.EDirsCardinal.Left:
                        if (Random.Range(0f, 1f) > 0.77f)
                            velocity.y = Random.Range(0f, 1f) < 0.5f ? -SPEED : SPEED;
                        else if (PlayState.player.transform.position.y < transform.position.y)
                            velocity.y = -SPEED;
                        else
                            velocity.y = SPEED;
                        PlayAnim("walk", velocity.y > 0);
                        if (Random.Range(0f, 1f) > 0.9f && grounded)
                        {
                            velocity.x = BASE_JUMP_POWER * jumpHeights[moveTimeoutIndex];
                            grounded = false;
                            PlayAnim("jump", velocity.y > 0);
                        }
                        break;
                    case PlayState.EDirsCardinal.Right:
                        if (Random.Range(0f, 1f) > 0.77f)
                            velocity.y = Random.Range(0f, 1f) < 0.5f ? -SPEED : SPEED;
                        else if (PlayState.player.transform.position.y < transform.position.y)
                            velocity.y = -SPEED;
                        else
                            velocity.y = SPEED;
                        PlayAnim("walk", velocity.y > 0);
                        if (Random.Range(0f, 1f) > 0.9f && grounded)
                        {
                            velocity.x = -BASE_JUMP_POWER * jumpHeights[moveTimeoutIndex];
                            grounded = false;
                            PlayAnim("jump", velocity.y > 0);
                        }
                        break;
                    case PlayState.EDirsCardinal.Up:
                        if (Random.Range(0f, 1f) > 0.77f)
                            velocity.x = Random.Range(0f, 1f) < 0.5f ? -SPEED : SPEED;
                        else if (PlayState.player.transform.position.x < transform.position.x)
                            velocity.x = -SPEED;
                        else
                            velocity.x = SPEED;
                        PlayAnim("walk", velocity.x > 0);
                        if (Random.Range(0f, 1f) > 0.9f && grounded)
                        {
                            velocity.y = -BASE_JUMP_POWER * jumpHeights[moveTimeoutIndex];
                            grounded = false;
                            PlayAnim("jump", velocity.x > 0);
                        }
                        break;
                }
                shotTimeout -= Time.fixedDeltaTime;
                if (shotTimeout <= 0)
                {
                    shotTimeout = SHOT_TIMEOUT;
                    Shoot();
                }
            }

            switch (gravState)
            {
                case PlayState.EDirsCardinal.Down:
                    GetDistance(PlayState.EDirsCardinal.Down);
                    if (grounded)
                    {
                        velocity.y = 0;
                        if (lastDistance > 0.25f)
                            grounded = false;
                    }
                    else
                    {
                        velocity.y -= GRAVITY * Time.fixedDeltaTime;
                        if (lastDistance < -velocity.y && velocity.y < 0)
                        {
                            transform.position += (lastDistance - PlayState.FRAC_32) * Vector3.down;
                            velocity.y *= -0.1f;
                            grounded = true;
                        }
                        if (GetDistance(PlayState.EDirsCardinal.Up) < velocity.y && velocity.y > 0)
                        {
                            transform.position += (lastDistance - PlayState.FRAC_32) * Vector3.up;
                            velocity.y = 0;
                        }
                    }
                    if (velocity.x != 0)
                        velocity.x -= DECELERATION * Time.fixedDeltaTime * (facingDownLeft ? -1 : 1);
                    if ((facingDownLeft && velocity.x > 0) || (!facingDownLeft && velocity.x < 0))
                        velocity.x = 0;
                    if (GetDistance(facingDownLeft ? PlayState.EDirsCardinal.Left : PlayState.EDirsCardinal.Right) < Mathf.Abs(velocity.x))
                    {
                        transform.position += (facingDownLeft ? -1 : 1) * (lastDistance - PlayState.FRAC_32) * Vector3.right;
                        velocity.x = -velocity.x;
                        PlayAnim("walk", facingDownLeft);
                    }
                    break;
                case PlayState.EDirsCardinal.Left:
                    break;
                case PlayState.EDirsCardinal.Right:
                    break;
                case PlayState.EDirsCardinal.Up:
                    break;
            }

            transform.position += (Vector3)velocity;
        }
    }

    private float GetDistance(PlayState.EDirsCardinal dir)
    {
        Vector2 a = Vector2.zero;
        Vector2 b = Vector2.zero;
        switch (dir)
        {
            case PlayState.EDirsCardinal.Down:
                a = (Vector2)transform.position + new Vector2(-halfBox.x, -halfBox.y);
                b = (Vector2)transform.position + new Vector2(halfBox.x, -halfBox.y);
                break;
            case PlayState.EDirsCardinal.Left:
                a = (Vector2)transform.position + new Vector2(-halfBox.x, -halfBox.y);
                b = (Vector2)transform.position + new Vector2(-halfBox.x, halfBox.y);
                break;
            case PlayState.EDirsCardinal.Right:
                a = (Vector2)transform.position + new Vector2(halfBox.x, -halfBox.y);
                b = (Vector2)transform.position + new Vector2(halfBox.x, halfBox.y);
                break;
            case PlayState.EDirsCardinal.Up:
                a = (Vector2)transform.position + new Vector2(-halfBox.x, halfBox.y);
                b = (Vector2)transform.position + new Vector2(halfBox.x, halfBox.y);
                break;
        }
        lastDistance = PlayState.GetDistance(dir, a, b, 2, enemyCollide);
        return lastDistance;
    }

    private void Shoot()
    {
        if (PlayState.currentProfile.difficulty != 2)
            return;

        float angle = Mathf.Atan2(transform.position.y - PlayState.player.transform.position.y,
            transform.position.x - PlayState.player.transform.position.x);
        PlayState.ShootEnemyBullet(transform.position, EnemyBullet.BulletType.pea, new float[] { WEAPON_SPEED, -Mathf.Cos(angle), -Mathf.Sin(angle) });
    }

    private void PlayAnim(string action, bool lookState)
    {
        string parsedLookState;
        if (gravState == PlayState.EDirsCardinal.Down || gravState == PlayState.EDirsCardinal.Up)
            parsedLookState = lookState ? "L" : "R";
        else
            parsedLookState = lookState ? "D" : "U";
        string parsedDir = gravState switch
        {
            PlayState.EDirsCardinal.Down => "floor",
            PlayState.EDirsCardinal.Left => "wallL",
            PlayState.EDirsCardinal.Right => "wallR",
            _ => "ceiling"
        };

        anim.Play("Enemy_pincer_" + parsedDir + "_" + action + parsedLookState);

        if (gravState == PlayState.EDirsCardinal.Down || gravState == PlayState.EDirsCardinal.Up)
        {
            facingDownLeft = lookState;
            if (lookState && flipData[0])
                sprite.flipX = lookState;
        }
        else
        {
            facingDownLeft = lookState;
            if (lookState && flipData[1])
                sprite.flipY = lookState;
        }
    }
}
