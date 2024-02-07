using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonBuster : Enemy
{
    private const float WAVE_DISTANCE = 6f;
    private const float WAVE_SPEED = 1.2f;
    private const float RISE_SPEED = 1.5f;
    private const float WEAPON_SPEED = 6f;
    private const float SHOT_TIMEOUT = 2.4f;

    private float theta = 0f;
    private float originX = 0f;
    private float shotTimeout = SHOT_TIMEOUT;

    private void Awake()
    {
        Spawn(50, 3, 1, true, 4);
        originX = transform.position.x;
        anim.Add("Enemy_balloon_floatL");
        anim.Add("Enemy_balloon_floatR");
        anim.Add("Enemy_balloon_shootL");
        anim.Add("Enemy_balloon_shootR");
    }

    private void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game || col == null)
            return;

        theta += Time.deltaTime;
        transform.position = new Vector2(originX + WAVE_DISTANCE * Mathf.Sin(theta * WAVE_SPEED), transform.position.y + RISE_SPEED * Time.deltaTime);
        if (!anim.currentAnimName.Contains("shoot"))
        {
            if (theta > PlayState.PI_OVER_TWO && theta <= PlayState.THREE_PI_OVER_TWO && anim.currentAnimName != "Enemy_balloon_floatR")
                anim.Play("Enemy_balloon_floatR");
            else if (anim.currentAnimName != "Enemy_balloon_floatL")
                anim.Play("Enemy_balloon_floatL");
        }

        if (PlayState.OnScreen(transform.position, col))
        {
            shotTimeout -= Time.deltaTime;
            if (shotTimeout <= 0)
            {
                shotTimeout = SHOT_TIMEOUT;
                Shoot();
            }
        }
    }

    private void Shoot()
    {
        float angle = Mathf.Atan2(transform.position.y - PlayState.player.transform.position.y,
            transform.position.x - PlayState.player.transform.position.x);
        PlayState.ShootEnemyBullet(this, transform.position, EnemyBullet.BulletType.donutLinear, new float[] { WEAPON_SPEED, -Mathf.Cos(angle), -Mathf.Sin(angle) });
        anim.Play(anim.currentAnimName.Contains("floatL") ? "Enemy_balloon_shootL" : "Enemy_balloon_shootR");
    }
}
