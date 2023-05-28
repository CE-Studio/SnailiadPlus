using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tallfish2 : Enemy
{
    private const float MOVE_TIMEOUT = 2.3f;
    private const float SHOT_TIMEOUT = 0.1f;
    private const int SHOT_COUNT = 9;
    private const float WEAPON_SPEED = 6.25f;
    private const float SPEED = 10f;
    private const float DECELERATION = 0.00245f;

    private float moveTimeout = 0;
    private float shotTimeout = 0;
    private int shotNum = 0;
    private float velocity;
    private float elapsed;
    private bool facingLeft;

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(900, 4, 10, true);

        moveTimeout = MOVE_TIMEOUT * 0.125f;
        anim.Add("Enemy_tallfish_pink_idle");
        anim.Add("Enemy_tallfish_pink_attackForward");
        anim.Add("Enemy_tallfish_pink_attackTurnaround");
        anim.Play("Enemy_tallfish_pink_idle");
    }

    private void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        elapsed += Time.fixedDeltaTime;
        if (PlayState.OnScreen(transform.position, col))
        {
            moveTimeout -= Time.fixedDeltaTime;
            if (moveTimeout <= 0)
            {
                if (PlayState.player.transform.position.x < transform.position.x)
                {
                    if (facingLeft)
                        anim.Play("Enemy_tallfish_pink_attackForward");
                    else
                        anim.Play("Enemy_tallfish_pink_attackTurnaround");
                    facingLeft = true;
                    sprite.flipX = true;
                    velocity = -SPEED * Time.fixedDeltaTime;
                }
                else
                {
                    if (facingLeft)
                        anim.Play("Enemy_tallfish_pink_attackTurnaround");
                    else
                        anim.Play("Enemy_tallfish_pink_attackForward");
                    facingLeft = false;
                    sprite.flipX = false;
                    velocity = SPEED * Time.fixedDeltaTime;
                }
                shotNum = SHOT_COUNT;
                shotTimeout = 0;
                moveTimeout = MOVE_TIMEOUT;
            }
            if (velocity < 0)
            {
                velocity += DECELERATION;
                if (velocity > 0)
                    velocity = 0;
            }
            else if (velocity > 0)
            {
                velocity -= DECELERATION;
                if (velocity < 0)
                    velocity = 0;
            }
            transform.localPosition = new Vector3(transform.localPosition.x + velocity, origin.y + (4f * Mathf.Sin(elapsed * 2f) * PlayState.FRAC_16), 0);

            shotTimeout -= Time.fixedDeltaTime;
            if (shotTimeout <= 0 && shotNum > 0)
            {
                float angle = Mathf.Atan2(transform.position.y - PlayState.player.transform.position.y,
                    transform.position.x - PlayState.player.transform.position.x);
                Shoot(angle);
                shotTimeout = SHOT_TIMEOUT;
                shotNum--;
            }

            if (!anim.isPlaying)
                anim.Play("Enemy_tallfish_pink_idle");
        }
    }
    private void Shoot(float angle)
    {
        float angleX = -Mathf.Cos(angle);
        float angleY = -Mathf.Sin(angle);
        PlayState.ShootEnemyBullet(transform.position, EnemyBullet.BulletType.donutLinear, new float[] { WEAPON_SPEED, angleX, angleY });
    }
}
