using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blob2 : Enemy
{
    private readonly float[] HOP_TIMEOUTS = { 2.4f, 3.5f, 2.2f, 1.6f, 3.9f, 3.5f, 2.9f, 3.1f, 1.8f };
    private readonly float[] HOP_HEIGHTS = { 1, 1, 1, 1.2f, 2, 1, 1.2f, 1, 2 };

    private int hopNum = 0;
    private float hopTimeout = 0;
    private Vector2 velocity = Vector2.zero;
    private bool facingRight = false;

    private RaycastHit2D hCast;
    private RaycastHit2D vCast;

    private BoxCollider2D box;

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(270, 10, 2, true, new List<int>(), new List<int>(), new List<int> { 1 });
        col.TryGetComponent(out box);

        anim.Add("Enemy_blob2_normal");
        anim.Add("Enemy_blob2_jump");
        anim.Add("Enemy_blob2_quiver");
        anim.Play("Enemy_blob2_normal");

        Face(PlayState.player.transform.position.x > transform.position.x);
        hopNum = Mathf.Abs(Mathf.FloorToInt(transform.position.x) % HOP_TIMEOUTS.Length);
        hopTimeout = HOP_TIMEOUTS[hopNum] / (PlayState.currentDifficulty == 2 ? 2 : 1);

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
                PlayAnim("Enemy_blob2_quiver");
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
                velocity.x = 6.25f * (transform.position.x > PlayState.player.transform.position.x ? -1 : 1);
                velocity.y = 15 * HOP_HEIGHTS[hopNum];
                PlayAnim("Enemy_blob2_jump");
                hopNum = (hopNum + 1) % HOP_TIMEOUTS.Length;
                hopTimeout = HOP_TIMEOUTS[hopNum];
            }
        }
        Face(velocity.x != 0 ? velocity.x > 0 : facingRight);
        velocity.y -= 75 * Time.deltaTime;
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
