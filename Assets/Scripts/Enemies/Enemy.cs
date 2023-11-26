using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health;
    public int maxHealth;
    public int attack;
    public int defense;
    public List<int> weaknesses;  // Enemies take double damage from bullet types in this list
    public List<int> resistances; // Enemies take half damage from bullet types in this list
    public List<int> immunities;  // Enemies resist all damage from bullet types in this list
    public bool letsPermeatingShotsBy;
    public bool stunInvulnerability = false;
    public bool makeSoundOnPing = true;
    public string elementType; // Currently supports "ice" and "fire"
    public bool invulnerable = false;
    public bool canDamage = true;
    public int parryDamage = 0;

    public Collider2D col;
    public Rigidbody2D rb;
    public SpriteRenderer sprite;
    public AnimationModule anim;
    public SpriteMask mask;

    protected int pingPlayer = 0;

    public List<float> spawnConditions;
    public Vector2 origin;
    protected bool intersectingPlayer = false;
    protected List<Bullet> intersectingBullets = new();
    protected List<EnemyBullet> intersectingEnemyBullets = new();
    public LayerMask playerCollide;
    public LayerMask enemyCollide;
    
    public void Spawn(int hp, int atk, int def, bool piercable, List<int> wea = null, List<int> res = null, List<int> imm = null)
    {
        TryGetComponent(out col);
        TryGetComponent(out rb);
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<AnimationModule>();
        BuildMask();

        origin = transform.localPosition;

        playerCollide = LayerMask.GetMask("PlayerCollide");
        enemyCollide = LayerMask.GetMask("PlayerCollide", "EnemyCollide");

        health = hp;
        maxHealth = hp;
        attack = atk;
        defense = def;
        weaknesses = wea;
        resistances = res;
        immunities = imm;
        letsPermeatingShotsBy = piercable;

        if (weaknesses == null)
            weaknesses = new List<int> { -1 };
        if (resistances == null)
            resistances = new List<int> { -1 };
        if (immunities == null)
            immunities = new List<int> { -1 };
    }

    public virtual void LateUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game && !PlayState.playerScript.inDeathCutscene)
            return;

        if (intersectingPlayer && !PlayState.playerScript.stunned && canDamage)
        {
            bool canHit = true;
            if ((elementType.ToLower() == "ice" && PlayState.CheckShellLevel(1) && PlayState.currentProfile.difficulty != 2) ||
                (elementType.ToLower() == "fire" && PlayState.CheckShellLevel(3)))
                canHit = false;
            if (canHit)
                PlayState.playerScript.HitFor(attack, this);
        }

        if (!stunInvulnerability && PlayState.OnScreen(transform.position, col) && !invulnerable)
        {
            List<Bullet> bulletsToDespawn = new();
            List<EnemyBullet> enemyBulletsToDespawn = new();
            bool killFlag = false;
            int maxDamage = parryDamage;
            foreach (Bullet bullet in intersectingBullets)
            {
                int thisDamage = bullet.damage;
                if ((bullet.bulletType == 7 || bullet.bulletType == 8) && col.bounds.Contains(bullet.transform.position) && PlayState.CheckForItem(9))
                    thisDamage = Mathf.CeilToInt(thisDamage * 1.35f);
                if (!immunities.Contains(bullet.bulletType) && bullet.damage - defense > 0)
                {
                    thisDamage = Mathf.FloorToInt((bullet.damage - defense) *
                        (weaknesses.Contains(bullet.bulletType) ? 2 : 1) * (resistances.Contains(bullet.bulletType) ? 0.5f : 1));
                    if (thisDamage > maxDamage)
                        maxDamage = thisDamage;
                }
                else
                {
                    if (!PlayState.armorPingPlayedThisFrame && makeSoundOnPing)
                    {
                        PlayState.armorPingPlayedThisFrame = true;
                        PlayState.PlaySound("Ping");
                    }
                    pingPlayer -= 1;
                }
                if (!letsPermeatingShotsBy || bullet.bulletType == 1 || !bullet.isActive)
                    bulletsToDespawn.Add(bullet);
            }
            foreach (EnemyBullet bullet in intersectingEnemyBullets)
            {
                if (bullet.hasBeenParried)
                {
                    if (bullet.damage - defense > 0)
                    {
                        int thisDamage = Mathf.FloorToInt(bullet.damage - defense);
                        if (thisDamage > maxDamage)
                            maxDamage = thisDamage;
                    }
                    else
                    {
                        if (!PlayState.armorPingPlayedThisFrame && makeSoundOnPing)
                        {
                            PlayState.armorPingPlayedThisFrame = true;
                            PlayState.PlaySound("Ping");
                        }
                        pingPlayer -= 1;
                    }
                    if (!letsPermeatingShotsBy || bullet.bulletType == EnemyBullet.BulletType.pea || !bullet.isActive)
                        enemyBulletsToDespawn.Add(bullet);
                }
            }
            if (maxDamage > 0)
            {
                health -= maxDamage;
                if (health <= 0)
                    killFlag = true;
                else
                    StartCoroutine(Flash());
            }
            parryDamage = 0;
            while (bulletsToDespawn.Count > 0)
            {
                intersectingBullets.RemoveAt(0);
                bulletsToDespawn[0].Despawn(true);
                bulletsToDespawn.RemoveAt(0);
            }
            while (enemyBulletsToDespawn.Count > 0)
            {
                intersectingEnemyBullets.RemoveAt(0);
                enemyBulletsToDespawn[0].Despawn();
                enemyBulletsToDespawn.RemoveAt(0);
            }
            if (killFlag)
                Kill();
        }

        if (intersectingBullets.Count == 0)
        {
            pingPlayer = 0;
        }

        mask.transform.localScale = new Vector2(sprite.flipX ? -1 : 1, sprite.flipY ? -1 : 1);
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "PlayerBullet":
                intersectingBullets.Add(collision.GetComponent<Bullet>());
                break;
            case "Player":
                intersectingPlayer = true;
                break;
            case "EnemyBullet":
                intersectingEnemyBullets.Add(collision.GetComponent<EnemyBullet>());
                break;
            default:
                break;
        }
    }

    public virtual void OnTriggerExit2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "PlayerBullet":
                intersectingBullets.Remove(collision.GetComponent<Bullet>());
                break;
            case "Player":
                intersectingPlayer = false;
                break;
            case "EnemyBullet":
                intersectingEnemyBullets.Remove(collision.GetComponent<EnemyBullet>());
                break;
            default:
                break;
        }
    }

    public virtual IEnumerator Flash(bool playSound = true)
    {
        mask.enabled = true;
        stunInvulnerability = true;
        if (playSound)
            PlayState.PlaySound("Explode" + Random.Range(1, 5));
        yield return new WaitForFixedUpdate();
        mask.enabled = false;
        yield return new WaitForFixedUpdate();
        stunInvulnerability = false;
    }

    public virtual void Kill()
    {
        PlayState.PlaySound("EnemyKilled1");
        for (int i = Random.Range(1, 4); i > 0; i--)
            PlayState.RequestParticle(new Vector2(Random.Range(transform.position.x - 0.5f, transform.position.x + 0.5f),
                Random.Range(transform.position.y - 0.5f, transform.position.y + 0.5f)), "explosion", new float[] { 2 });
        Destroy(gameObject);
    }

    private void BuildMask()
    {
        if (anim != null)
        {
            GameObject child = new("Mask Object");
            child.transform.parent = transform;
            child.transform.localPosition = Vector2.zero;
            mask = child.AddComponent<SpriteMask>();
            anim.AddMask(mask);
            mask.isCustomRangeActive = true;
            mask.frontSortingOrder = -2;
            mask.backSortingOrder = -3;
            mask.enabled = false;
        }
    }
}
