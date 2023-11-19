using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FederationDrone : Enemy
{
    private const float SHOT_TIMEOUT = 0.08f;
    private const float LASER_SPEED = 43.75f;
    private const int LASER_COUNT = 4;
    private const float DONUT_SPEED_ORBIT = 3.75f;
    private const float DONUT_SPEED_RADIUS = 4f;
    private const int DONUT_COUNT = 3;

    private float X_RADIUS = 8.125f;
    private float Y_RADIUS = 2.5f;
    private float MOVE_TIME = 2.2f;

    private float shotTimeout;
    private int shotNum;

    private enum MoveMode
    {
        Wait,
        Cos_UL,
        Cos_DL,
        Cos_UR,
        Cos_DR,
        Semicircle_UL,
        Semicircle_DL,
        Semicircle_UR,
        Semicircle_DR,
        Attack
    };
    private MoveMode mode = MoveMode.Wait;

    private Vector2 moveOrigin;
    private float elapsed;
    private bool facingLeft;
    private bool isTurning;

    private int[] animData;
    // 0 - how many frames into the turnaround animation (while idle) that the sprite flips (int >= 0)
    // 1 - how many frames into the turnaround animation (going down) that the sprite flips (int >= 0)
    // 2 - how many frames into the turnaround animation (going up) that the sprite flips (int >= 0)
    // 3 - if the laser-firing animation restarts for every laser fired (0/1)
    // 4 - if the donut-firing animation plays at all (0/1)

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(1850, 4, 10, true);
        if (PlayState.currentProfile.difficulty == 2)
        {
            MOVE_TIME = 1.3f;
            X_RADIUS = 6.875f;
            Y_RADIUS = 3.75f;
        }
        elapsed = MOVE_TIME;
        facingLeft = PlayState.player.transform.position.x < transform.position.x;
        sprite.flipX = facingLeft;

        anim.Add("Enemy_drone_idle");
        anim.Add("Enemy_drone_up");
        anim.Add("Enemy_drone_down");
        anim.Add("Enemy_drone_turnIdle");
        anim.Add("Enemy_drone_turnUp");
        anim.Add("Enemy_drone_turnDown");
        anim.Add("Enemy_drone_fireLaser");
        anim.Add("Enemy_drone_fireDonut");
        animData = PlayState.GetAnim("Enemy_drone_data").frames;
    }

    private void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (PlayState.OnScreen(transform.position, col))
        {
            elapsed += Time.deltaTime;
            if (mode == MoveMode.Attack)
            {
                shotTimeout -= Time.deltaTime;
                if (shotTimeout <= 0)
                {
                    shotTimeout = SHOT_TIMEOUT;
                    shotNum--;
                    if (!(shotNum < LASER_COUNT && animData[3] == 1))
                        anim.Play("Enemy_drone_fireLaser");
                    if (shotNum <= 0)
                    {
                        mode = MoveMode.Wait;
                        ShootDonuts();
                        if (animData[4] == 1)
                            anim.Play("Enemy_drone_fireDonut");
                    }
                    ShootLaser();
                }
            }
            else
            {
                UpdatePosition();
                if (elapsed >= MOVE_TIME && mode != MoveMode.Wait)
                {
                    mode = MoveMode.Attack;
                    shotNum = LASER_COUNT;
                }
                else if (mode == MoveMode.Wait)
                {
                    elapsed = 0;
                    moveOrigin = transform.position;
                    if (PlayState.player.transform.position.x < transform.position.x)
                    {
                        if (facingLeft)
                        {
                            if (PlayState.player.transform.position.y > transform.position.y)
                                mode = MoveMode.Cos_UL;
                            else
                                mode = MoveMode.Cos_DL;
                        }
                        else if (PlayState.player.transform.position.y > transform.position.y)
                            mode = MoveMode.Semicircle_UR;
                        else
                            mode = MoveMode.Semicircle_DR;
                    }
                    else
                    {
                        if (!facingLeft)
                        {
                            if (PlayState.player.transform.position.y > transform.position.y)
                                mode = MoveMode.Cos_UR;
                            else
                                mode = MoveMode.Cos_DR;
                        }
                        else if (PlayState.player.transform.position.y > transform.position.y)
                            mode = MoveMode.Semicircle_UL;
                        else
                            mode = MoveMode.Semicircle_DL;
                    }

                    switch (mode)
                    {
                        default:
                            anim.Play("Enemy_drone_up");
                            break;
                        case MoveMode.Cos_DL:
                        case MoveMode.Cos_DR:
                        case MoveMode.Semicircle_DL:
                        case MoveMode.Semicircle_DR:
                            anim.Play("Enemy_drone_down");
                            break;
                    }
                }
            }

            if (!anim.isPlaying)
                anim.Play("Enemy_drone_idle");

            if ((facingLeft && PlayState.player.transform.position.x > transform.position.x) ||
                (!facingLeft && PlayState.player.transform.position.x < transform.position.x))
            {
                sprite.flipX = facingLeft;
                facingLeft = !facingLeft;
                isTurning = true;
                switch (anim.currentAnimName)
                {
                    default:
                        break;
                    case "Enemy_drone_idle":
                        anim.Play("Enemy_drone_turnIdle");
                        break;
                    case "Enemy_drone_up":
                        anim.Play("Enemy_drone_turnUp");
                        break;
                    case "Enemy_drone_down":
                        anim.Play("Enemy_drone_turnDown");
                        break;
                }
            }

            if (isTurning && anim.GetCurrentFrame() >= animData[anim.currentAnimName switch { "Enemy_drone_turnIdle" => 0, "Enemy_drone_turnUp" => 1, _ => 2 }])
            {
                sprite.flipX = facingLeft;
                isTurning = false;
            }
        }
        else
        {
            facingLeft = PlayState.player.transform.position.x < transform.position.x;
            sprite.flipX = facingLeft;
        }
    }

    private float NormalizedSigmoid(float input)
    {
        return 1f / (1f + Mathf.Exp(-(input * 12f - 6f)));
    }

    private void UpdatePosition()
    {
        if (mode == MoveMode.Wait || mode == MoveMode.Attack)
            return;

        float lerpValue = NormalizedSigmoid(elapsed / MOVE_TIME);
        switch (mode)
        {
            case MoveMode.Cos_UL:
                transform.position = new Vector2(moveOrigin.x - (X_RADIUS * lerpValue), moveOrigin.y + (Y_RADIUS * (1f - Mathf.Cos(lerpValue * Mathf.PI))));
                break;
            case MoveMode.Cos_DL:
                transform.position = new Vector2(moveOrigin.x - (X_RADIUS * lerpValue), moveOrigin.y - (Y_RADIUS * (1f - Mathf.Cos(lerpValue * Mathf.PI))));
                break;
            case MoveMode.Cos_UR:
                transform.position = new Vector2(moveOrigin.x + (X_RADIUS * lerpValue), moveOrigin.y + (Y_RADIUS * (1f - Mathf.Cos(lerpValue * Mathf.PI))));
                break;
            case MoveMode.Cos_DR:
                transform.position = new Vector2(moveOrigin.x + (X_RADIUS * lerpValue), moveOrigin.y - (Y_RADIUS * (1f - Mathf.Cos(lerpValue * Mathf.PI))));
                break;
            case MoveMode.Semicircle_UL:
                transform.position = new Vector2(moveOrigin.x - (Y_RADIUS * Mathf.Sin(lerpValue * Mathf.PI)),
                    moveOrigin.y + (Y_RADIUS * (1f - Mathf.Cos(lerpValue * Mathf.PI))));
                break;
            case MoveMode.Semicircle_DL:
                transform.position = new Vector2(moveOrigin.x - (Y_RADIUS * Mathf.Sin(lerpValue * Mathf.PI)),
                    moveOrigin.y - (Y_RADIUS * (1f - Mathf.Cos(lerpValue * Mathf.PI))));
                break;
            case MoveMode.Semicircle_UR:
                transform.position = new Vector2(moveOrigin.x + (Y_RADIUS * Mathf.Sin(lerpValue * Mathf.PI)),
                    moveOrigin.y + (Y_RADIUS * (1f - Mathf.Cos(lerpValue * Mathf.PI))));
                break;
            case MoveMode.Semicircle_DR:
                transform.position = new Vector2(moveOrigin.x + (Y_RADIUS * Mathf.Sin(lerpValue * Mathf.PI)),
                    moveOrigin.y - (Y_RADIUS * (1f - Mathf.Cos(lerpValue * Mathf.PI))));
                break;
        }
    }

    private void ShootLaser()
    {
        PlayState.ShootEnemyBullet(this, transform.position, EnemyBullet.BulletType.laser, new float[] { LASER_SPEED, facingLeft ? -1 : 1, 0 });
    }

    private void ShootDonuts()
    {
        for (int i = 0; i < DONUT_COUNT; i++)
            PlayState.ShootEnemyBullet(this, transform.position, EnemyBullet.BulletType.donutRotary,
                new float[] { DONUT_SPEED_ORBIT, DONUT_SPEED_RADIUS, PlayState.TAU / DONUT_COUNT * i }, i == 0);
    }
}
