using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health;
    public int attack;
    public int defense;
    public List<string> resistances;

    public BoxCollider2D box;
    public Rigidbody2D rb;
    public SpriteRenderer sprite;
    public AudioSource sfx;

    public AudioClip ping;
    
    public virtual void Begin()
    {
        box = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        sfx = GetComponent<AudioSource>();

        ping = (AudioClip)Resources.Load("Sounds/Sfx/Ping");
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "PlayerBullet":
                if (!resistances.Contains(collision.GetComponent<Bullet>().bulletType))
                {
                    if (collision.GetComponent<Bullet>().bulletType != "Phaser-rang")
                    {
                        collision.GetComponent<Bullet>().Despawn();
                    }
                    StartCoroutine(nameof(Flash));
                }
                else
                {
                    collision.GetComponent<Bullet>().Despawn();
                    sfx.PlayOneShot(ping);
                }
                break;
            case "Player":
                if (!collision.GetComponent<Player>().stunned)
                {
                    collision.GetComponent<Player>().health = Mathf.RoundToInt(Mathf.Clamp(collision.GetComponent<Player>().health - attack, 0, Mathf.Infinity));
                    collision.GetComponent<Player>().BecomeStunned();
                }
                break;
            default:
                break;
        }
    }

    public IEnumerator Flash()
    {
        sprite.material.SetFloat("_FlashAmount", 1);
        box.enabled = false;
        yield return new WaitForSeconds(0.025f);
        sprite.material.SetFloat("_FlashAmount", 0);
        box.enabled = true;
    }
}
