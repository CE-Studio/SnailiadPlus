using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public int doorWeapon;
    public bool locked;
    public int direction;
    public int state;
    // -1 = despawned
    //  0 = opened from shot
    //  1 = opened from entrance through
    //  2 = closed upon spawn
    //  3 = closed after entrance through

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
        sfx = GetComponent<AudioSource>();
        player = GameObject.FindWithTag("Player");

        open = (AudioClip)Resources.Load("Sounds/Sfx/DoorOpen");
        close = (AudioClip)Resources.Load("Sounds/Sfx/DoorClose");

        if (direction == 1 || direction == 3)
        {
            box.size = new Vector2(3, 1);
            if (direction == 3)
            {
                sprite.flipY = true;
            }
        }
        else if (direction == 2)
        {
            sprite.flipX = true;
        }
    }

    public void SetClosedSprite()
    {
        PlayAnim("hold");
    }

    public void PlayAnim(string animType)
    {
        switch (animType)
        {
            default:
                anim.Play("Base Layer.H Blue Hold", 0, 0);
                break;
            case "hold":
                if (PlayState.colorblindMode)
                {
                    if (locked)
                    {
                        anim.Play("Base Layer.H Locked Hold2", 0, 0);
                    }
                    else
                    {
                        switch (doorWeapon)
                        {
                            case 0:
                                anim.Play("Base Layer.H Blue Hold2", 0, 0);
                                break;
                            case 1:
                                anim.Play("Base Layer.H Purple Hold2", 0, 0);
                                break;
                            case 2:
                                anim.Play("Base Layer.H Red Hold2", 0, 0);
                                break;
                            case 3:
                                anim.Play("Base Layer.H Green Hold2", 0, 0);
                                break;
                        }
                    }
                }
                else
                {
                    if (locked)
                    {
                        anim.Play("Base Layer.H Locked Hold", 0, 0);
                    }
                    else
                    {
                        switch (doorWeapon)
                        {
                            case 0:
                                anim.Play("Base Layer.H Blue Hold", 0, 0);
                                break;
                            case 1:
                                anim.Play("Base Layer.H Purple Hold", 0, 0);
                                break;
                            case 2:
                                anim.Play("Base Layer.H Red Hold", 0, 0);
                                break;
                            case 3:
                                anim.Play("Base Layer.H Green Hold", 0, 0);
                                break;
                        }
                    }
                }
                break;
            case "open":
                if (locked)
                {
                    anim.Play("Base Layer.H Locked Open", 0, 0);
                }
                else
                {
                    switch (doorWeapon)
                    {
                        case 0:
                            anim.Play("Base Layer.H Blue Open", 0, 0);
                            break;
                        case 1:
                            anim.Play("Base Layer.H Purple Open", 0, 0);
                            break;
                        case 2:
                            anim.Play("Base Layer.H Red Open", 0, 0);
                            break;
                        case 3:
                            anim.Play("Base Layer.H Green Open", 0, 0);
                            break;
                    }
                }
                break;
            case "close":
                if (locked)
                {
                    anim.Play("Base Layer.H Locked Close", 0, 0);
                }
                else
                {
                    switch (doorWeapon)
                    {
                        case 0:
                            anim.Play("Base Layer.H Blue Close", 0, 0);
                            break;
                        case 1:
                            anim.Play("Base Layer.H Purple Close", 0, 0);
                            break;
                        case 2:
                            anim.Play("Base Layer.H Red Close", 0, 0);
                            break;
                        case 3:
                            anim.Play("Base Layer.H Green Close", 0, 0);
                            break;
                    }
                }
                break;
        }
    }

    // State 0 is for doors that are opened from being shot
    public void SetState0()
    {
        //if (direction == 1 || direction == 3)
        //{
        //    anim.SetBool("flipDir", true);
        //}
        //anim.SetInteger("state", 0);
        PlayAnim("open");
        sfx.PlayOneShot(open);
        box.enabled = false;
    }

    // State 1 is for doors that are opened for a limited time before closing or despawning as a result of being entered through
    public void SetState1()
    {
        //if (direction == 1 || direction == 3)
        //{
        //    anim.SetBool("flipDir", true);
        //}
        //anim.SetInteger("state", 1);
        sprite.enabled = false;
        box.enabled = false;
        StartCoroutine(nameof(WaitForClose));
    }

    // State 2 is for doors that are closed upon spawning
    public void SetState2()
    {
        //if (direction == 1 || direction == 3)
        //{
        //    anim.SetBool("flipDir", true);
        //}
        //anim.SetInteger("weaponType", doorWeapon);
        //anim.SetBool("isLocked", locked);
        //anim.SetInteger("state", 2);
        sprite.enabled = true;
        PlayAnim("hold");
        box.enabled = true;
    }

    // State 3 is for doors that are closed only after being entered through
    public void SetState3()
    {
        //if (direction == 1 || direction == 3)
        //{
        //    anim.SetBool("flipDir", true);
        //}
        //anim.SetInteger("weaponType", doorWeapon);
        //anim.SetBool("isLocked", locked);
        //anim.SetInteger("state", 3);
        box.enabled = true;
        sprite.enabled = true;
        PlayAnim("close");
        sfx.PlayOneShot(close);
    }

    public void SetStateDespawn()
    {
        //anim.SetInteger("state", -1);
        gameObject.SetActive(false);
    }

    private IEnumerator WaitForClose()
    {
        while (Vector2.Distance(transform.position, player.transform.position) < 4 && gameObject.activeSelf)
        {
            yield return new WaitForEndOfFrame();
        }
        if (!gameObject.activeSelf)
        {
            SetStateDespawn();
        }
        else
        {
            SetState3();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
        {
            //if (locked)
            //{
            //    anim.SetBool("hit", true);
            //}
            //else
            //{
            //    switch (collision.GetComponent<Bullet>().bulletType)
            //    {
            //        case "Rainbow Wave":
            //            if (doorWeapon == 0)
            //            {
            //                SetState0();
            //            }
            //            break;
            //        case "Paralaser":
            //            if (doorWeapon == 0 || doorWeapon == 1)
            //            {
            //                SetState0();
            //            }
            //            break;
            //        case "Phaser-rang":
            //            if (doorWeapon == 0 || doorWeapon == 2)
            //            {
            //                SetState0();
            //            }
            //            break;
            //        case "Scatter Flares":
            //            if (doorWeapon == 0 || doorWeapon == 3)
            //            {
            //                SetState0();
            //            }
            //            break;
            //        case "Shooting Star":
            //            if (doorWeapon == 0 || doorWeapon == 4)
            //            {
            //                SetState0();
            //            }
            //            break;
            //    }
            //}
            if (!locked && collision.GetComponent<Bullet>().bulletTypeInt >= doorWeapon)
            {
                SetState0();
            }

            switch (direction)
            {
                case 0:
                    collision.transform.position = new Vector2(collision.transform.position.x - 1, collision.transform.position.y);
                    break;
                case 1:
                    collision.transform.position = new Vector2(collision.transform.position.x, collision.transform.position.y + 1);
                    break;
                case 2:
                    collision.transform.position = new Vector2(collision.transform.position.x + 1, collision.transform.position.y);
                    break;
                case 3:
                    collision.transform.position = new Vector2(collision.transform.position.x, collision.transform.position.y - 1);
                    break;
            }
        }
    }

    public void FlipHit()
    {
        anim.SetBool("hit", false);
    }

    public void FlipUnlock()
    {
        anim.SetBool("playUnlock", false);
    }
}
