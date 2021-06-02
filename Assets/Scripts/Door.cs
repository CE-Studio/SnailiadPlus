using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public int doorWeapon;
    public bool locked;
    public int direction;

    public Animator anim;
    public SpriteRenderer sprite;
    public AudioSource sfx;
    public BoxCollider2D box;
    public GameObject player;

    public AudioClip open;
    public AudioClip close;
    public AudioClip reflect;
    
    void Start()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        player = GameObject.FindWithTag("Player");

        if (direction == 1 || direction == 3)
        {
            anim.SetBool("flipDir", true);
            box.size = new Vector2(3, 1);
        }
        if (direction == 2)
        {
            sprite.flipX = true;
        }
        else if (direction == 3)
        {
            sprite.flipY = true;
        }
        anim.SetInteger("weaponType", doorWeapon);
        anim.SetBool("isLocked", locked);
    }

    void EnableCollider()
    {
        box.enabled = true;
    }

    void DisableCollider()
    {
        box.enabled = false;
    }

    public void Spawn()
    {
        if (Vector2.Distance(player.transform.position, transform.position) < 2)
        {
            StartCoroutine(nameof(EntranceThrough));
        }
        else
        {
            anim.SetBool("isOpen", false);
        }
    }

    IEnumerator EntranceThrough()
    {
        anim.SetBool("enteredThrough", true);
        DisableCollider();
        while (Vector2.Distance(player.transform.position, transform.position) < 5)
        {
            yield return new WaitForEndOfFrame();
        }
        anim.SetBool("enteredThrough", false);
        anim.SetBool("isOpen", false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
        {
            if (locked)
            {
                anim.SetBool("hit", true);
            }
            else
            {
                switch (collision.GetComponent<Bullet>().bulletType)
                {
                    case "Rainbow Wave":
                        anim.SetBool("isOpen", true);
                        break;
                    case "Paralaser":
                        if (doorWeapon == 1)
                        {
                            anim.SetBool("isOpen", true);
                        }
                        break;
                    case "Phaser-rang":
                        if (doorWeapon == 2)
                        {
                            anim.SetBool("isOpen", true);
                        }
                        break;
                    case "Scatter Flares":
                        if (doorWeapon == 3)
                        {
                            anim.SetBool("isOpen", true);
                        }
                        break;
                    case "Shooting Star":
                        if (doorWeapon == 4)
                        {
                            anim.SetBool("isOpen", true);
                        }
                        break;
                }
            }
        }
    }
}
