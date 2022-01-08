using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health;
    public int maxHealth;
    public int attack;
    public int defense;
    public List<int> resistances;
    public bool letsPermeatingShotsBy;
    public bool stunInvulnerability = false;

    public BoxCollider2D box;
    public Rigidbody2D rb;
    public SpriteRenderer sprite;
    public AudioSource sfx;

    public AudioClip ping;
    public AudioClip[] hit;
    public AudioClip die;
    private int pingPlayer = 0;

    public float[] spawnConditions;
    public Vector2 origin;
    private bool intersectingPlayer = false;
    private List<GameObject> intersectingBullets = new List<GameObject>();
    
    public virtual void Begin()
    {
        box = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        sfx = GetComponent<AudioSource>();

        ping = (AudioClip)Resources.Load("Sounds/Sfx/Ping");
        hit = new AudioClip[]
        {
            (AudioClip)Resources.Load("Sounds/Sfx/Explode1"),
            (AudioClip)Resources.Load("Sounds/Sfx/Explode2"),
            (AudioClip)Resources.Load("Sounds/Sfx/Explode3"),
            (AudioClip)Resources.Load("Sounds/Sfx/Explode4")
        };
        die = (AudioClip)Resources.Load("Sounds/Sfx/EnemyKilled1");

        origin = transform.localPosition;
    }

    public virtual void OnEnable()
    {
        transform.localPosition = origin;
        health = maxHealth;
    }

    private void LateUpdate()
    {
        if (intersectingPlayer && !PlayState.playerScript.stunned)
        {
            PlayState.playerScript.health = Mathf.RoundToInt(Mathf.Clamp(PlayState.playerScript.health - attack, 0, Mathf.Infinity));
            if (PlayState.playerScript.health <= 0)
                PlayState.playerScript.Die();
            else
                PlayState.playerScript.BecomeStunned();
        }

        if (!stunInvulnerability)
        {
            foreach (GameObject bullet in intersectingBullets)
            {
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (!resistances.Contains(bulletScript.bulletType) && bulletScript.damage - defense > 0)
                {
                    health -= bulletScript.damage - defense;
                    if (health <= 0)
                        Kill();
                    else
                        StartCoroutine(nameof(Flash));
                }
                else
                {
                    //if (!PlayState.armorPingPlayedThisFrame && pingPlayer <= 0)
                    if (!PlayState.armorPingPlayedThisFrame)
                    {
                        PlayState.armorPingPlayedThisFrame = true;
                        //pingPlayer = 8;
                        sfx.PlayOneShot(ping);
                    }
                    pingPlayer -= 1;
                }
                if (!letsPermeatingShotsBy || bulletScript.bulletType == 1)
                    bulletScript.Despawn();
            }
        }

        if (intersectingBullets.Count == 0)
        {
            pingPlayer = 0;
        }
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

    public IEnumerator Flash()
    {
        sprite.material.SetFloat("_FlashAmount", 0.75f);
        stunInvulnerability = true;
        sfx.PlayOneShot(hit[Random.Range(1, 4)]);
        yield return new WaitForSeconds(0.025f);
        sprite.material.SetFloat("_FlashAmount", 0);
        stunInvulnerability = false;
    }

    public virtual void Kill()
    {
        sfx.PlayOneShot(die);
        box.enabled = false;
        sprite.enabled = false;
        for (int i = Random.Range(1, 4); i > 0; i--)
            PlayState.RequestExplosion(1, new Vector2(Random.Range(transform.position.x - 0.5f, transform.position.x + 0.5f),
                Random.Range(transform.position.y - 0.5f, transform.position.y + 0.5f)));
    }
}
