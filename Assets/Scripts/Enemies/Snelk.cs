using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snelk : Enemy, IRoomObject
{
    public int state; // 0 = default, 1 = run from player, 2 = sleeping
    public float spawnChance = 1;
    public int facingState = 3; // 0 = right, 1 = left, 2 = either direction at random, 3 = facing player, 4 = facing away from player
    public Particle zzz;

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

    public static readonly string myType = "Enemies/Snelk";

    public Dictionary<string, object> resave()
    {
        return null;
    }

    public string objType
    {
        get
        {
            return myType;
        }
    }

    public Dictionary<string, object> save()
    {
        Dictionary<string, object> content = new();
        content["state"] = state;
        content["spawnChance"] = spawnChance;
        content["facingState"] = facingState;
        return content;
    }

    public void load(Dictionary<string, object> content)
    {
        state = (int)content["state"];
        spawnChance = (float)content["spawnChance"];
        facingState = (int)content["facingState"];

        facingLeft = facingState switch
        {
            0 => false,
            1 => true,
            2 => Random.Range(0, 2) == 1,
            3 => PlayState.player.transform.position.x < transform.position.x,
            _ => PlayState.player.transform.position.x > transform.position.x
        };
        sprite.flipX = facingLeft;

        if (Random.Range(0f, 1f) > spawnChance)
        {
            Destroy(gameObject);
            return;
        }

        SetState(state);
    }

    void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(50, 2, 1, true, 0);
        col.TryGetComponent(out box);
        invulnerable = true;
        canDamage = false;

        anim.Add("Enemy_snelk_jump");
        anim.Add("Enemy_snelk_sleep");

        currentHop = Mathf.Abs(Mathf.FloorToInt(transform.position.x)) % hopHeights.Length;
    }

    public void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (!(state == 2 && velocity.y == 0 && hasLandedOnce))
            velocity = new Vector2((hasLandedOnce ? (state == 1 ? SPEED_RUN : SPEED_NORMAL) * (facingLeft ? -1 : 1) : 0) * Time.deltaTime,
                Mathf.Clamp(velocity.y - GRAVITY * Time.deltaTime, TERMINAL_VELOCITY, Mathf.Infinity));

        if (state != 2)
        {
            float wallDis = GetDistance(facingLeft ? PlayState.EDirsSurface.WallL : PlayState.EDirsSurface.WallR);
            if (wallDis < Mathf.Abs(velocity.x))
            {
                transform.position = new Vector2(transform.position.x + ((wallDis - PlayState.FRAC_32) * (facingLeft ? -1 : 1)), transform.position.y);
                facingLeft = !facingLeft;
            }
            else
                transform.position = new Vector2(transform.position.x + velocity.x, transform.position.y);
        }
        float vertDis = GetDistance(velocity.y > 0 ? PlayState.EDirsSurface.Ceiling : PlayState.EDirsSurface.Floor);
        if (vertDis < Mathf.Abs(velocity.y))
        {
            transform.position = new Vector2(transform.position.x, transform.position.y + (vertDis - PlayState.FRAC_32) * (velocity.y > 0 ? 1 : -1));
            if (velocity.y > 0)
                velocity.y = 0;
            else
            {
                velocity.y = state == 2 ? 0 : hopHeights[currentHop] * Time.deltaTime * 16f;
                currentHop = (currentHop + 1) % hopHeights.Length;
                hasLandedOnce = true;
                if (state != 2)
                {
                    facingLeft = state == 1 ? (PlayState.player.transform.position.x > transform.position.x) :
                        (Random.Range(0f, 1f) > 0.8f ? !facingLeft : facingLeft);
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

    private float GetDistance(PlayState.EDirsSurface dir)
    {
        Vector2 pos = transform.position;
        Vector2 halfBox = box.size * 0.5f;
        Vector2 a = dir switch
        {
            PlayState.EDirsSurface.Floor or PlayState.EDirsSurface.WallL => new Vector2(pos.x - halfBox.x, pos.y - halfBox.y),
            _ => new Vector2(pos.x + halfBox.x, pos.y + halfBox.y)
        };
        Vector2 b = dir switch
        {
            PlayState.EDirsSurface.WallL or PlayState.EDirsSurface.Ceiling => new Vector2(pos.x - halfBox.x, pos.y + halfBox.y),
            _ => new Vector2(pos.x + halfBox.x, pos.y - halfBox.y)
        };
        return PlayState.GetDistance(dir, a, b, 4, playerCollide);
    }
}
