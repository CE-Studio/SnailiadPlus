using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snelk : Enemy
{
    public int state; // 0 = default, 1 = run from player, 2 = sleeping
    public float spawnChance = 1;
    public Particle zzz;

    private RaycastHit2D hCast;
    private RaycastHit2D vCast;

    private readonly float[] hopHeights = new float[] { 2f, 1f, 1.9f, 1.2f, 2f, 0.4f, 1.2f, 2f, 0.3f };
    private int currentHop;
    private const float GRAVITY = 1.25f;
    private const float SPEED_NORMAL = 6.25f;
    private const float SPEED_RUN = 8.75f;
    private const float TERMINAL_VELOCITY = -0.5208f;
    private Vector2 velocity = Vector2.zero;
    private bool hasLandedOnce = false;
    private bool facingLeft = false;

    private BoxCollider2D box;

    void Awake()
    {
        if (Random.Range(0f, 1f) > spawnChance)
        {
            Destroy(gameObject);
            return;
        }
        Spawn(50, 2, 1, true);
        col.TryGetComponent(out box);
        invulnerable = true;
        canDamage = false;

        anim.Add("Enemy_snelk_jump");
        anim.Add("Enemy_snelk_sleep");
        SetState(state);

        currentHop = Mathf.Abs(Mathf.FloorToInt(transform.position.x)) % hopHeights.Length;
        facingLeft = PlayState.player.transform.position.x < transform.position.x;
    }

    public void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (!(state == 2 && velocity.y == 0 && hasLandedOnce))
            velocity = new Vector2((hasLandedOnce ? (state == 1 ? SPEED_RUN : SPEED_NORMAL) * (facingLeft ? -1 : 1) : 0) * Time.deltaTime,
                Mathf.Clamp(velocity.y - GRAVITY * Time.deltaTime, TERMINAL_VELOCITY, Mathf.Infinity));
        UpdateBoxcasts();

        if (state != 2)
        {
            if (hCast.collider != null && ((hCast.point.x > transform.position.x) ? !facingLeft : facingLeft))
            {
                transform.position = new Vector2(hCast.point.x + (0.75f * (facingLeft ? 1 : -1)), transform.position.y);
                facingLeft = !facingLeft;
                UpdateBoxcasts();
            }
            else
                transform.position = new Vector2(transform.position.x + velocity.x, transform.position.y);
        }
        if (vCast.collider != null)
        {
            transform.position = new Vector2(transform.position.x, vCast.point.y + (0.75f * (velocity.y > 0 ? -1 : 1)));
            if (velocity.y > 0)
                velocity.y = 0;
            else
            {
                velocity.y = state == 2 ? 0 : hopHeights[currentHop] * Time.deltaTime * 16;
                currentHop = (currentHop + 1) % hopHeights.Length;
                hasLandedOnce = true;
                if (state != 2)
                {
                    facingLeft = state == 1 ? (PlayState.player.transform.position.x > transform.position.x) : (Random.Range(0f, 1f) > 0.8f ? !facingLeft : facingLeft);
                    if (PlayState.OnScreen(transform.position, box) && Random.Range(0f, 1f) > 0.4f)
                        PlayState.PlaySound("Snelk");
                    anim.Play("Enemy_snelk_jump");
                }
            }
        }
        else
            transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);

        if (state != 2)
            sprite.flipX = facingLeft;
        if (state == 2 && Vector2.Distance(transform.position, PlayState.player.transform.position) < 5f)
            SetState(1);

        if (PlayState.IsTileSolid(transform.position))
        {
            while (PlayState.IsTileSolid(transform.position))
            {
                transform.position = new Vector2(transform.position.x, transform.position.y + 1);
            }
        }

        if (zzz != null)
            zzz.transform.position = new Vector2(transform.position.x + 1, transform.position.y + 0.25f);
    }

    public void SetState(int newState)
    {
        switch (newState)
        {
            default:
            case 0:
                anim.Play("Enemy_snelk_jump");
                if (state == 2)
                {
                    PlayState.PlaySound("Snelk");
                    zzz.ResetParticle();
                    zzz = null;
                }
                break;
            case 1:
                anim.Play("Enemy_snelk_jump");
                if (state == 2)
                {
                    PlayState.PlaySound("Snelk");
                    zzz.ResetParticle();
                    zzz = null;
                }
                break;
            case 2:
                sprite.flipX = Random.Range(0, 2) == 1;
                anim.Play("Enemy_snelk_sleep");
                zzz = PlayState.RequestParticle(new Vector2(transform.position.x + 1, transform.position.y + 0.25f), "zzz");
                break;
        }
        state = newState;
    }

    private void UpdateBoxcasts()
    {
        if (box == null)
            return;
        hCast = Physics2D.BoxCast(
            transform.position,
            new Vector2(box.size.x, box.size.y - 0.25f),
            0,
            Vector2.right,
            velocity.x,
            playerCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );
        vCast = Physics2D.BoxCast(
            transform.position,
            new Vector2(box.size.x - 0.25f, box.size.y),
            0,
            Vector2.up,
            velocity.y,
            playerCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );
    }
}
