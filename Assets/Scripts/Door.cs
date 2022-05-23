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

    public AnimationModule anim;
    public SpriteRenderer sprite;
    //public AudioSource sfx;
    public BoxCollider2D box;
    public GameObject player;

    //public AudioClip open;
    //public AudioClip close;

    public Sprite[] editorSprites;
    
    void Start()
    {
        anim = GetComponent<AnimationModule>();
        sprite = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        //sfx = GetComponent<AudioSource>();
        player = GameObject.FindWithTag("Player");

        //open = (AudioClip)Resources.Load("Sounds/Sfx/DoorOpen");
        //close = (AudioClip)Resources.Load("Sounds/Sfx/DoorClose");

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

        string[] doorDirs = new string[] { "horiz", "vert" };
        string[] doorColors = new string[] { "blue", "purple", "red", "green", "locked" };
        string[] doorStates = new string[] { "open", "hold", "close" };
        for (int i = 0; i < doorDirs.Length; i++)
        {
            for (int j = 0; j < doorColors.Length; j++)
            {
                for (int k = 0; k < doorStates.Length; k++)
                    anim.Add("Door_" + doorDirs[i] + "_" + doorColors[j] + "_" + doorStates[k]);
            }
        }
    }

    //void Update()
    //{
    //    sfx.volume = PlayState.gameOptions[0] * 0.1f;
    //}

    public void SetClosedSprite()
    {
        PlayAnim("hold");
    }

    public void PlayAnim(string animType)
    {
        if (animType == "blank")
            sprite.enabled = false;
        else
        {
            sprite.enabled = true;
            string animToPlay = "Door_";
            if (direction == 1 || direction == 3)
                animToPlay += "vert_";
            else
                animToPlay += "horiz_";
            if (locked)
                animToPlay += "locked_";
            else
            {
                switch (doorWeapon)
                {
                    case 0:
                        animToPlay += "blue_";
                        break;
                    case 1:
                        animToPlay += "purple_";
                        break;
                    case 2:
                        animToPlay += "red_";
                        break;
                    case 3:
                        animToPlay += "green_";
                        break;
                }
            }
            //switch (animType)
            //{
            //    case "hold":
            //        if (PlayState.colorblindMode)
            //            animToPlay += "Hold2";
            //        else
            //            animToPlay += "Hold";
            //        break;
            //    case "open":
            //        if (PlayState.colorblindMode)
            //            animToPlay += "Open2";
            //        else
            //            animToPlay += "Open";
            //        break;
            //    case "close":
            //        if (PlayState.colorblindMode)
            //            animToPlay += "Close2";
            //        else
            //            animToPlay += "Close";
            //        break;
            //}
            anim.Play(animToPlay + animType);
        }
    }

    // State 0 is for doors that are opened from being shot
    public void SetState0()
    {
        PlayAnim("open");
        //sfx.PlayOneShot(open);
        PlayState.PlaySound("DoorOpen");
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
        //sfx.PlayOneShot(close);
        PlayState.PlaySound("DoorClose");
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
            if (!locked && ((collision.GetComponent<Bullet>().bulletType > doorWeapon && doorWeapon != 3) || (collision.GetComponent<Bullet>().bulletType >= 4 && doorWeapon == 3)))
            {
                SetState0();
            }
        }
    }

    //public void FlipHit()
    //{
    //    anim.SetBool("hit", false);
    //}
    //
    //public void FlipUnlock()
    //{
    //    anim.SetBool("playUnlock", false);
    //}
}
