using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health;
    public int attack;
    public int defense;
    public List<int> resistances;
    public bool letsPermeatingShotsBy;

    public BoxCollider2D box;
    public Rigidbody2D rb;
    public SpriteRenderer sprite;
    public AudioSource sfx;

    public AudioClip ping;
    private int pingPlayer = 0;

    public GameObject player;
    private bool intersectingPlayer = false;
    private List<GameObject> intersectingBullets = new List<GameObject>();
    
    public virtual void Begin()
    {
        box = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        sfx = GetComponent<AudioSource>();

        ping = (AudioClip)Resources.Load("Sounds/Sfx/Ping");

        player = GameObject.Find("Player");
    }

    private void LateUpdate()
    {
        if (intersectingPlayer && !player.GetComponent<Player>().stunned)
        {
            player.GetComponent<Player>().health = Mathf.RoundToInt(Mathf.Clamp(player.GetComponent<Player>().health - attack, 0, Mathf.Infinity));
            if (player.GetComponent<Player>().health <= 0)
                player.GetComponent<Player>().Die();
            else
                player.GetComponent<Player>().BecomeStunned();
        }

        foreach (GameObject bullet in intersectingBullets)
        {
            if (!resistances.Contains(bullet.GetComponent<Bullet>().bulletType))
            {
                //if (bullet.GetComponent<Bullet>().bulletType == "Peashooter")
                //    bullet.GetComponent<Bullet>().Despawn();
                StartCoroutine(nameof(Flash));
            }
            else
            {
                //bullet.GetComponent<Bullet>().Despawn();
                if (!PlayState.armorPingPlayedThisFrame && pingPlayer <= 0)
                {
                    PlayState.armorPingPlayedThisFrame = true;
                    pingPlayer = 8;
                    sfx.PlayOneShot(ping);
                }
                pingPlayer -= 1;
            }
            if (!letsPermeatingShotsBy)
                bullet.GetComponent<Bullet>().Despawn();
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
                //if (!resistances.Contains(collision.GetComponent<Bullet>().bulletType))
                //{
                //    if (collision.GetComponent<Bullet>().bulletType == "Peashooter")
                //        collision.GetComponent<Bullet>().Despawn();
                //    StartCoroutine(nameof(Flash));
                //}
                //else
                //{
                //    collision.GetComponent<Bullet>().Despawn();
                //    sfx.PlayOneShot(ping);
                //}
                intersectingBullets.Add(collision.gameObject);
                break;
            case "Player":
                //if (!collision.GetComponent<Player>().stunned)
                //{
                //    collision.GetComponent<Player>().health = Mathf.RoundToInt(Mathf.Clamp(collision.GetComponent<Player>().health - attack, 0, Mathf.Infinity));
                //    if (collision.GetComponent<Player>().health <= 0)
                //        collision.GetComponent<Player>().Die();
                //    else
                //        collision.GetComponent<Player>().BecomeStunned();
                //}
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
        box.enabled = false;
        yield return new WaitForSeconds(0.025f);
        sprite.material.SetFloat("_FlashAmount", 0);
        box.enabled = true;
    }
}
