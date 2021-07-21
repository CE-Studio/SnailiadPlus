using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public string itemType = "Rainbow Wave";
    public bool countedInPercentage;
    public bool collected;
    public int itemID;

    public Vector2 originPos;

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

        originPos = transform.localPosition;

        switch (itemType)
        {
            case "Rainbow Wave":
                anim.Play("RainbowWave", 0, 0);
                box.size = new Vector2(1.25f, 2f);
                break;
            default:
                anim.Play("RainbowWave", 0, 0);
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
            PlayState.FlashItemText(itemType);
            PlayState.FlashCollectionText();
            //SetDeactivated();
            StartCoroutine(nameof(HoverOverPlayer));
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

    public IEnumerator HoverOverPlayer()
    {
        box.enabled = false;
        float timer = 0;
        GameObject player = GameObject.Find("Player");
        while (timer < 2)
        {
            switch (player.GetComponent<Player>()._currentSurface)
            {
                case 0:
                    transform.position = new Vector2(player.transform.position.x, player.transform.position.y + (box.size.y * 0.5f));
                    break;
                case 1:
                    if (player.GetComponent<Player>()._facingLeft)
                        transform.position = new Vector2(player.transform.position.x + (box.size.y * 0.5f), player.transform.position.y);
                    else
                        transform.position = new Vector2(player.transform.position.x - (box.size.y * 0.5f), player.transform.position.y);
                    break;
                case 2:
                    transform.position = new Vector2(player.transform.position.x, player.transform.position.y - (box.size.y * 0.5f));
                    break;
            }
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
        transform.localPosition = originPos;
        SetDeactivated();
    }
}
