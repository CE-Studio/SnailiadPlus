using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walleye : Enemy
{
    private const float SHOT_TIMEOUT = 0.08f;
    private const float SHOT_SPEED = 43.75f;
    private const float RANGE_Y = 0.25f;

    private float shotTimeout;
    private bool facingLeft = false;

    private BoxCollider2D box;

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(400, 0, 0, true);
        canDamage = false;
        col.TryGetComponent(out box);
    }

    void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        shotTimeout -= Time.deltaTime;
        if (Mathf.Abs(PlayState.player.transform.position.y - transform.position.y) <= RANGE_Y)
        {
            if (((!facingLeft && PlayState.player.transform.position.x > transform.position.x) ||
                (facingLeft && PlayState.player.transform.position.x < transform.position.x)) && shotTimeout <= 0f)
                Shoot();
        }
    }

    private void Shoot()
    {
        shotTimeout = SHOT_TIMEOUT;
        PlayState.ShootEnemyBullet(transform.position, EnemyBullet.BulletType.laser, new float[] { SHOT_SPEED, facingLeft ? -1 : 1, 0 });
    }

    public void Face(bool left)
    {
        sprite.flipX = left;
        box.offset = new Vector2(0.25f * (left ? 1 : -1), 0);
        facingLeft = left;
    }
}
