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
    private bool playerWasLastAbove = false;

    private BoxCollider2D box;

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(400, 0, 0, true, 0);
        canDamage = false;
        invulnerable = true;
        col.TryGetComponent(out box);
        anim = GetComponent<AnimationModule>();
        foreach (string dir in new string[] { "left", "right" })
        {
            anim.Add(string.Format("Enemy_walleye_{0}_idle", dir));
            anim.Add(string.Format("Enemy_walleye_{0}_shoot", dir));
        }
        playerWasLastAbove = PlayState.player.transform.position.y - transform.position.y > 0;
    }

    void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (!anim.isPlaying)
            anim.Play(string.Format("Enemy_walleye_{0}_idle", facingLeft ? "left" : "right"));

        shotTimeout -= Time.deltaTime;
        if (Mathf.Abs(PlayState.player.transform.position.y - transform.position.y) <= RANGE_Y ||
            (PlayState.player.transform.position.y - transform.position.y > 0) != playerWasLastAbove)
        {
            if (((!facingLeft && PlayState.player.transform.position.x > transform.position.x) ||
                (facingLeft && PlayState.player.transform.position.x < transform.position.x)) && shotTimeout <= 0f)
                Shoot();
            playerWasLastAbove = PlayState.player.transform.position.y - transform.position.y > 0;
        }
    }

    private void Shoot()
    {
        shotTimeout = SHOT_TIMEOUT;
        PlayState.ShootEnemyBullet(this, transform.position, EnemyBullet.BulletType.laser, new float[] { SHOT_SPEED, facingLeft ? -1 : 1, 0 });
        anim.Play(string.Format("Enemy_walleye_{0}_shoot", facingLeft ? "left" : "right"));
    }

    public void Face(bool left)
    {
        sprite.flipX = left;
        box.offset = new Vector2(0.25f * (left ? 1 : -1), 0);
        facingLeft = left;
        anim.Play(string.Format("Enemy_walleye_{0}_idle", left ? "left" : "right"));
    }
}
