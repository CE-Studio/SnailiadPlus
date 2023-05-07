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

    public Collider2D col;
    public Rigidbody2D rb;
    public SpriteRenderer sprite;
    public AnimationModule anim;
    public SpriteMask mask;

    protected int pingPlayer = 0;

    public List<float> spawnConditions;
    public Vector2 origin;
    private bool intersectingPlayer = false;
    protected List<GameObject> intersectingBullets = new List<GameObject>();
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
        if (intersectingPlayer && !PlayState.playerScript.stunned && canDamage)
        {
            bool canHit = true;
            if ((elementType.ToLower() == "ice" && PlayState.CheckShellLevel(1) && PlayState.currentDifficulty != 2) ||
                (elementType.ToLower() == "fire" && PlayState.CheckShellLevel(3)))
                canHit = false;
            if (canHit)
                PlayState.playerScript.HitFor(attack);
        }

        if (!stunInvulnerability && PlayState.OnScreen(transform.position, col) && !invulnerable)
        {
            List<GameObject> bulletsToDespawn = new List<GameObject>();
            bool killFlag = false;
            foreach (GameObject bullet in intersectingBullets)
            {
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (!immunities.Contains(bulletScript.bulletType) && bulletScript.damage - defense > 0)
                {
                    health -= Mathf.FloorToInt((bulletScript.damage - defense) *
                        (weaknesses.Contains(bulletScript.bulletType) ? 2 : 1) * (resistances.Contains(bulletScript.bulletType) ? 0.5f : 1));
                    if (health <= 0)
                        killFlag = true;
                    else
                        StartCoroutine(Flash());
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
                if (!letsPermeatingShotsBy || bulletScript.bulletType == 1)
                    bulletsToDespawn.Add(bullet);
            }
            while (bulletsToDespawn.Count > 0)
            {
                intersectingBullets.RemoveAt(0);
                bulletsToDespawn[0].GetComponent<Bullet>().Despawn(true);
                bulletsToDespawn.RemoveAt(0);
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
                intersectingBullets.Add(collision.gameObject);
                break;
            case "Player":
                intersectingPlayer = true;
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
                intersectingBullets.Remove(collision.gameObject);
                break;
            case "Player":
                intersectingPlayer = false;
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
        yield return new WaitForSeconds(0.0125f);
        mask.enabled = false;
        yield return new WaitForSeconds(0.0125f);
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
