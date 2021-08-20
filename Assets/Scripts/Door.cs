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
        string animToPlay = "Base Layer.";
        if (direction == 1 || direction == 3)
            animToPlay += "V ";
        else
            animToPlay += "H ";
        if (locked)
            animToPlay += "Locked ";
        else
        {
            switch (doorWeapon)
            {
                case 0:
                    animToPlay += "Blue ";
                    break;
                case 1:
                    animToPlay += "Purple ";
                    break;
                case 2:
                    animToPlay += "Red ";
                    break;
                case 3:
                    animToPlay += "Green ";
                    break;
            }
        }
        switch (animType)
        {
            case "hold":
                if (PlayState.colorblindMode)
                    animToPlay += "Hold2";
                else
                    animToPlay += "Hold";
                break;
            case "open":
                if (PlayState.colorblindMode)
                    animToPlay += "Open2";
                else
                    animToPlay += "Open";
                break;
            case "close":
                if (PlayState.colorblindMode)
                    animToPlay += "Close2";
                else
                    animToPlay += "Close";
                break;
        }
        anim.Play(animToPlay, 0, 0);
    }

    // State 0 is for doors that are opened from being shot
    public void SetState0()
    {
        PlayAnim("open");
        sfx.PlayOneShot(open);
        box.enabled = false;
    }

    // State 1 is for doors that are opened for a limited time before closing or despawning as a result of being entered through
    public void SetState1()
    {
        sprite.enabled = false;
        box.enabled = false;
        StartCoroutine(nameof(WaitForClose));
    }

    // State 2 is for doors that are closed upon spawning
    public void SetState2()
    {
        sprite.enabled = true;
        PlayAnim("hold");
        box.enabled = true;
    }

    // State 3 is for doors that are closed only after being entered through
    public void SetState3()
    {
        box.enabled = true;
        sprite.enabled = true;
        PlayAnim("close");
        sfx.PlayOneShot(close);
    }

    public void SetStateDespawn()
    {
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
            if (!locked && (collision.GetComponent<Bullet>().bulletTypeInt >= doorWeapon || (collision.GetComponent<Bullet>().bulletTypeInt >= 3 && doorWeapon == 3)))
            {
                SetState0();
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
