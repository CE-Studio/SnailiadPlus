using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GigaZzz : Enemy
{
    private const float ATTACK_TIMEOUT = 3f;
    private const float ACCELERATION = 2.75f;

    private float attackTimeout = ATTACK_TIMEOUT;
    private float theta;
    public Vector2 targetPoint;
    private Vector2 velocity;
    private Vector2 accel = Vector2.zero;

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(100, 6, 0, true, 0);
        invulnerable = true;
        origin = transform.position;

        for (int i = 0; i < PlayState.DIRS_COMPASS.Length; i++)
            anim.Add("Boss_gigaSnail_zzz_attack_" + PlayState.DIRS_COMPASS[i]);
        anim.Add("Boss_gigaSnail_zzz_spawn");
        anim.Play("Boss_gigaSnail_zzz_spawn");
    }

    private void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (attackTimeout >= 0)
        {
            theta += Time.fixedDeltaTime;
            origin = new Vector2(PlayState.Integrate(origin.x, targetPoint.x, 2, Time.fixedDeltaTime),
                PlayState.Integrate(origin.y, targetPoint.y, 2, Time.fixedDeltaTime));
            transform.position = new Vector2(origin.x, origin.y - 0.875f * Mathf.Sin(theta * Mathf.PI * 2));
            attackTimeout -= Time.fixedDeltaTime;
            if (attackTimeout < 0)
                ShootAtPlayer();
        }
        else
        {
            velocity += accel * Time.fixedDeltaTime;
            transform.position += (Vector3)velocity;
            if (!PlayState.OnScreen(transform.position, col))
                Destroy(gameObject);
        }
    }

    private void ShootAtPlayer()
    {
        float angle = Mathf.Atan2(PlayState.player.transform.position.y - transform.position.y, PlayState.player.transform.position.x - transform.position.x);
        accel = ACCELERATION * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        anim.Play("Boss_gigaSnail_zzz_attack_" + PlayState.VectorToCompass(accel));
    }
}
