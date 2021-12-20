using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    private string itemType;
    public bool countedInPercentage;
    public bool collected;
    public int itemID;
    public bool isSuperUnique;

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

        if (itemID >= 23)
        {
            anim.Play("Helix Fragment", 0, 0);
            box.size = new Vector2(0.95f, 0.95f);
        }
        else
        {
            switch (itemID)
            {
                case 2:
                    anim.Play("Rainbow Wave", 0, 0);
                    box.size = new Vector2(1.25f, 1.825f);
                    break;
                default:
                    anim.Play("Helix Fragment", 0, 0);
                    box.size = new Vector2(0.95f, 0.95f);
                    break;
            }
        }

        itemType = PlayState.TranslateIDToItemName(itemID);
    }

    void Update()
    {
        
    }

    public void SetAnim()
    {
        if (itemID >= 23)
        {
            anim.Play("Helix Fragment", 0, 0);
            box.size = new Vector2(0.95f, 0.95f);
        }
        else
        {
            switch (itemID)
            {
                case 2:
                    anim.Play("Rainbow Wave", 0, 0);
                    box.size = new Vector2(1.25f, 1.825f);
                    break;
                default:
                    anim.Play("Helix Fragment", 0, 0);
                    box.size = new Vector2(0.95f, 0.95f);
                    break;
            }
        }
    }

    public void CheckIfCollected()
    {
        if (PlayState.itemCollection[itemID] == 1)
            SetDeactivated();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayState.AddItem(itemID);
            if (isSuperUnique)
                sfx.PlayOneShot(majorJingle);
            else
                sfx.PlayOneShot(minorJingle);
            switch (itemID)
            {
                case 2:
                    PlayState.isArmed = true;
                    collision.GetComponent<Player>().selectedWeapon = 3;
                    break;
                default:
                    break;
            }
            PlayState.FlashItemText(itemType);
            PlayState.FlashCollectionText();
            StartCoroutine(nameof(HoverOverPlayer));
        }
    }

    public void SetDeactivated()
    {
        transform.localPosition = originPos;
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
            switch (player.GetComponent<Player>().gravityDir)
            {
                case 0:
                    transform.position = new Vector2(player.transform.position.x, player.transform.position.y + (box.size.y * 0.75f));
                    break;
                case 1:
                    transform.position = new Vector2(player.transform.position.x + (box.size.y * 0.75f), player.transform.position.y);
                    break;
                case 2:
                    transform.position = new Vector2(player.transform.position.x - (box.size.y * 0.75f), player.transform.position.y);
                    break;
                case 3:
                    transform.position = new Vector2(player.transform.position.x, player.transform.position.y - (box.size.y * 0.75f));
                    break;
            }
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
        SetDeactivated();
    }
}
