using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kitty2 : Enemy
{
    private const float SPEED = 4;
    private const float JUMP_POWER = 17.3333f;
    private const float GRAVITY = 75;
    private const int MAX_SHOTS = 10;
    private const int MAX_HOPS = 3;
    private const float WEAPON_SPEED = 5;
    private const float WEAPON_TIMEOUT = 0.08f;
    private readonly float[] hopTimeouts = new float[] { 0.7f, 0.8f, 0.6f, 0.7f, 0.8f, 0.6f, 0.7f, 0.8f, 0.6f };
    private readonly float[] hopHeight = new float[] { 1f, 1f, 1f, 1.2f, 1.3f, 1f, 1.2f, 1f, 0.9f };

    private int hopNum = 0;
    private int hopCount = 0;
    private int shotCount = 0;
    private float actionTimeout = 0;
    private Vector2 velocity = Vector2.zero;
    private bool facingRight = false;
    private bool isAttacking = false;

    private RaycastHit2D hCast;
    private RaycastHit2D vCast;

    void Awake()
    {
        Spawn(900, 2, 1, true, new Vector2(1.95f, 0.95f));

        anim.Add("Enemy_kitty2_idle");
        anim.Add("Enemy_kitty2_jump");
        anim.Add("Enemy_kitty2_fall");
        anim.Add("Enemy_kitty2_shoot");

        Face(PlayState.player.transform.position.x > transform.position.x);
        hopNum = Mathf.Abs(Mathf.FloorToInt(transform.position.x) % hopTimeouts.Length);
        actionTimeout = hopTimeouts[hopNum] / 3;
        hopCount = MAX_HOPS;

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
                PlayAnim("Enemy_kitty2_idle");
            }
        }
        else
            transform.position = new Vector2(transform.position.x, transform.position.y + (velocity.y * Time.deltaTime));
        if (hCast.collider != null)
        {
            transform.position = new Vector2(Mathf.RoundToInt(hCast.point.x) + (facingRight ? -1 : 1), transform.position.y);
            Face(!facingRight);
            velocity.x *= -1;
        }
        else
            transform.position = new Vector2(transform.position.x + (velocity.x * Time.deltaTime), transform.position.y);

        if (PlayState.OnScreen(transform.position, box))
        {
            actionTimeout -= Time.deltaTime;
            if (isAttacking && actionTimeout < 0 && shotCount >= 0)
            {
                shotCount--;
                actionTimeout = WEAPON_TIMEOUT;
                float fireAngle = (Mathf.PI - Mathf.PI * 0.6f * shotCount / MAX_SHOTS) * Mathf.Rad2Deg - 90;
                PlayState.ShootEnemyBullet(transform.position, 0, facingRight ? Mathf.PI - fireAngle : fireAngle, WEAPON_SPEED);
                anim.Play("Enemy_kitty2_shoot");
            }
            if (shotCount <= 0)
            {
                if (actionTimeout <= 0)
                {
                    if (!isAttacking && hopCount <= 0)
                    {
                        isAttacking = true;
                        shotCount = MAX_SHOTS;
                        actionTimeout = WEAPON_TIMEOUT;
                        hopCount = MAX_HOPS;
                    }
                    else
                    {
                        isAttacking = false;
                        hopCount--;
                        velocity.x = SPEED * (transform.position.x > PlayState.player.transform.position.x ? -1 : 1);
                        velocity.y = JUMP_POWER * hopHeight[hopNum];
                        PlayAnim("Enemy_kitty2_jump");
                    }
                    hopNum = (hopNum + 1) % hopTimeouts.Length;
                    actionTimeout = hopTimeouts[hopNum];
                }
            }
        }
        Face(velocity.x != 0 ? velocity.x > 0 : facingRight);
        velocity.y -= GRAVITY * Time.deltaTime;
        if (velocity.y < 0 && anim.currentAnimName == "Enemy_kitty2_jump")
            PlayAnim("Enemy_kitty2_fall");
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
