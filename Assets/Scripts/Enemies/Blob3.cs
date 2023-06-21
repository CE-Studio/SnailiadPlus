using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blob3 : Enemy
{
    private readonly float[] HOP_TIMEOUTS = { 0.4f, 0.5f, 1.6f, 0.4f, 0.9f, 1.1f, 0.9f, 0.5f, 0.9f };
    private readonly float[] HOP_HEIGHTS = { 0.2f, 0.3f, 3f, 0.2f, 1.6f, 0.4f, 2.5f, 2.7f, 0.5f };

    private int hopNum = 0;
    private float hopTimeout = 0;
    private Vector2 velocity = Vector2.zero;
    private bool facingRight = false;
    private float shotTimeout = 0f;
    private readonly Vector2 jumpVelocity = new Vector2(17.5f, 20f);

    private const float SHOT_TIMEOUT = 0.7f;
    private const int SHOT_COUNT = 4;

    private RaycastHit2D hCast;
    private RaycastHit2D vCast;

    private BoxCollider2D box;

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(10000, 12, 30, true);
        col.TryGetComponent(out box);

        anim.Add("Enemy_blob3_normal");
        anim.Add("Enemy_blob3_jump");
        anim.Add("Enemy_blob3_quiver");
        anim.Play("Enemy_blob3_normal");

        Face(PlayState.player.transform.position.x > transform.position.x);
        hopNum = Mathf.Abs(Mathf.FloorToInt(transform.position.x) % HOP_TIMEOUTS.Length);
        hopTimeout = HOP_TIMEOUTS[hopNum];

        UpdateBoxcasts();
    }

    void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game || box == null)
            return;

        UpdateBoxcasts();
        if (vCast.collider != null)
        {
            transform.position = new Vector2(transform.position.x, Mathf.RoundToInt(vCast.point.y) + (vCast.point.y > transform.position.y ? -0.5f : 0.5f));
            if (vCast.point.y > transform.position.y && velocity.y > 0)
                velocity.y = 0;
            else if (vCast.point.y < transform.position.y && velocity.y < 0)
            {
                velocity.x = 0;
                velocity.y *= -0.25f;
                PlayAnim("Enemy_blob3_quiver");
            }
        }
        else
            transform.position = new Vector2(transform.position.x, transform.position.y + (velocity.y * Time.deltaTime));
        if (hCast.collider != null)
        {
            transform.position = new Vector2(Mathf.RoundToInt(hCast.point.x) + (facingRight ? -0.5f : 0.5f), transform.position.y);
            Face(!facingRight);
            velocity.x *= -1;
        }
        else
            transform.position = new Vector2(transform.position.x + (velocity.x * Time.deltaTime), transform.position.y);

        if (PlayState.OnScreen(transform.position, box))
        {
            hopTimeout -= Time.deltaTime;
            if (hopTimeout <= 0)
            {
                velocity.x = jumpVelocity.x * (transform.position.x > PlayState.player.transform.position.x ? -1 : 1);
                velocity.y = jumpVelocity.y * HOP_HEIGHTS[hopNum];
                PlayAnim("Enemy_blob3_jump");
                hopNum = (hopNum + 1) % HOP_TIMEOUTS.Length;
                hopTimeout = HOP_TIMEOUTS[hopNum];
            }
        }
        Face(velocity.x != 0 ? velocity.x > 0 : facingRight);
        velocity.y -= 75 * Time.deltaTime;

        if (PlayState.currentProfile.difficulty == 2)
        {
            shotTimeout -= Time.deltaTime;
            if (shotTimeout <= 0)
            {
                shotTimeout = SHOT_TIMEOUT;
                for (int i = 0; i < SHOT_COUNT; i++)
                    PlayState.ShootEnemyBullet(transform.position, EnemyBullet.BulletType.donutRotary,
                        new float[] { 3.75f, 4, PlayState.TAU / SHOT_COUNT * i }, i == 0);
            }
        }
    }

    private void Face(bool direction)
    {
        facingRight = direction;
        sprite.flipX = direction;
    }

    private void PlayAnim(string animName)
    {
        if (anim.currentAnimName != animName)
            anim.Play(animName);
    }

    private void UpdateBoxcasts()
    {
        if (box == null)
            return;
        hCast = Physics2D.BoxCast(
            transform.position,
            box.size,
            0,
            Vector2.right,
            velocity.x * Time.deltaTime,
            enemyCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );
        vCast = Physics2D.BoxCast(
            transform.position,
            box.size,
            0,
            Vector2.up,
            velocity.y * Time.deltaTime,
            enemyCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );
    }
}
