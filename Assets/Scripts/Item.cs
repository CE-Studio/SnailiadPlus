using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public string itemType = "Rainbow Wave";
    public bool countedInPercentage;
    public bool collected;
    public int itemID;

    public Animator anim;
    public BoxCollider2D box;
    public SpriteRenderer sprite;
    public AudioSource sfx;

    public AudioClip minorJingle;
    public AudioClip majorJingle;
    
    void Start()
    {
        anim = GetComponent<Animator>();
        box = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        sfx = GetComponent<AudioSource>();

        switch (itemType)
        {
            case "Rainbow Wave":
                anim.SetInteger("itemType", 0);
                box.size = new Vector2(1.25f, 2f);
                break;
            default:
                anim.SetInteger("itemType", 0);
                box.size = new Vector2(1.25f, 2f);
                break;
        }
    }

    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            switch (itemType)
            {
                case "Rainbow Wave":
                    PlayState.hasRainbowWave = true;
                    PlayState.isArmed = true;
                    collision.GetComponent<Player>().selectedWeapon = 0;
                    GetMinorItem("Rainbow Wave");
                    break;
                default:
                    break;
            }
            //PlayState.RunItemPopup(itemType);
            //Destroy(gameObject);
            SetDeactivated();
        }
    }

    void GetMinorItem(string item)
    {
        sfx.PlayOneShot(minorJingle);
    }

    void GetMajorItem(string item)
    {
        sfx.PlayOneShot(majorJingle);
    }

    public void SetDeactivated()
    {
        box.enabled = false;
        sprite.enabled = false;
    }
}
