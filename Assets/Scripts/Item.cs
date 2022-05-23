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

    public AnimationModule anim;
    public BoxCollider2D box;
    public SpriteRenderer sprite;
    public AudioSource sfx;

    public AudioClip minorJingle;
    public AudioClip majorJingle;
    
    void Start()
    {
        anim = GetComponent<AnimationModule>();
        box = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        sfx = GetComponent<AudioSource>();

        anim.Add("Item_boomerang");
        anim.Add("Item_rainbowWave");
        anim.Add("Item_helixFragment");
        anim.Add("Item_heartContainer");

        originPos = transform.localPosition;

        if (itemID >= PlayState.OFFSET_FRAGMENTS)
        {
            anim.Play("Item_helixFragment");
            box.size = new Vector2(0.95f, 0.95f);
        }
        else if (itemID >= PlayState.OFFSET_HEARTS)
        {
            anim.Play("Item_heartContainer");
            box.size = new Vector2(1.95f, 1.95f);
        }
        else
        {
            switch (itemID)
            {
                case 1:
                case 11:
                    anim.Play("Item_boomerang");
                    box.size = new Vector2(1.25f, 1.825f);
                    break;
                case 2:
                case 12:
                    anim.Play("Item_rainbowWave");
                    box.size = new Vector2(1.25f, 1.825f);
                    break;
                default:
                    anim.Play("Item_helixFragment");
                    box.size = new Vector2(0.95f, 0.95f);
                    break;
            }
        }

        itemType = PlayState.TranslateIDToItemName(itemID);
    }

    void Update()
    {
        if (PlayState.gameState == "Game")
        {
            anim.speed = 1;
            sfx.volume = PlayState.gameOptions[0] * 0.1f;
        }
        else
            anim.speed = 0;
    }

    public void SetAnim()
    {
        if (itemID >= PlayState.OFFSET_FRAGMENTS)
        {
            anim.Play("Item_helixFragment");
            box.size = new Vector2(0.95f, 0.95f);
        }
        else if (itemID >= PlayState.OFFSET_HEARTS)
        {
            anim.Play("Item_heartContainer");
            box.size = new Vector2(1.95f, 1.95f);
        }
        else
        {
            switch (itemID)
            {
                case 1:
                case 11:
                    anim.Play("Item_boomerang");
                    box.size = new Vector2(1.25f, 1.825f);
                    break;
                case 2:
                case 12:
                    anim.Play("Item_rainbowWave");
                    box.size = new Vector2(1.25f, 1.825f);
                    break;
                default:
                    anim.Play("Item_helixFragment");
                    box.size = new Vector2(0.95f, 0.95f);
                    break;
            }
        }
    }

    public void CheckIfCollected()
    {
        if (PlayState.itemCollection[itemID] == 1)
        {
            collected = true;
            SetDeactivated();
        }
        else
        {
            collected = false;
            box.enabled = true;
            sprite.enabled = true;
            if (PlayState.itemLocations.ContainsValue(itemID))
                PlayState.itemLocations.Remove(itemID);
            PlayState.minimapScript.RefreshMap();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collected = true;
            if (PlayState.itemLocations.ContainsKey(PlayState.WorldPosToMapGridID(transform.position)))
                PlayState.itemLocations.Remove(PlayState.WorldPosToMapGridID(transform.position));
            PlayState.minimapScript.RefreshMap();
            PlayState.AddItem(itemID);
            if (itemID >= PlayState.OFFSET_FRAGMENTS)
                PlayState.helixCount++;
            else if (itemID >= PlayState.OFFSET_HEARTS)
            {
                PlayState.heartCount++;
                PlayState.playerScript.maxHealth += 4;
                PlayState.playerScript.health = PlayState.playerScript.maxHealth;
                PlayState.playerScript.RenderNewHearts();
            }
            if (isSuperUnique)
                sfx.PlayOneShot(majorJingle);
            else
                sfx.PlayOneShot(minorJingle);
            switch (itemID)
            {
                case 1:
                case 11:
                    PlayState.isArmed = true;
                    collision.GetComponent<Player>().selectedWeapon = 2;
                    PlayState.playerScript.ChangeActiveWeapon(1, true);
                    if (itemID == 11)
                        PlayState.QueueAchievementPopup("secrt");
                    break;
                case 2:
                case 12:
                    PlayState.isArmed = true;
                    collision.GetComponent<Player>().selectedWeapon = 3;
                    PlayState.playerScript.ChangeActiveWeapon(2, true);
                    break;
                default:
                    break;
            }
            FlashItemText();
            PlayState.FlashCollectionText();
            StartCoroutine(nameof(HoverOverPlayer));
            PlayState.WriteSave("game");
        }
    }

    public void FlashItemText()
    {
        if (itemID >= PlayState.OFFSET_FRAGMENTS)
            PlayState.FlashItemText(PlayState.GetText("item_helixFragment").Replace("_", PlayState.helixCount.ToString()));
        else if (itemID >= PlayState.OFFSET_HEARTS)
            PlayState.FlashItemText(PlayState.GetText("item_heartContainer").Replace("_", PlayState.heartCount.ToString()));
        //else if (itemID == PlayState.TranslateItemNameToID("Super Secret Boomerang"))
        //    PlayState.FlashItemText("Boomerang");
        //else if (itemID == PlayState.TranslateItemNameToID("Debug Rainbow Wave"))
        //    PlayState.FlashItemText("Rainbow Wave");
        else
            PlayState.FlashItemText(ConvertToTextID(itemType));
    }

    private string ConvertToTextID(string item)
    {
        string output = "";
        string buffer = "item_";
        string species = PlayState.GetText("species_" + PlayState.currentCharacter.ToLower());
        switch (item)
        {
            default:
                for (int i = 0; i < item.Length; i++)
                {
                    if (item[i] == ' ')
                    {
                        i++;
                        if (i < item.Length)
                            buffer += item[i].ToString().ToUpper();
                    }
                    else
                        buffer += item[i].ToString().ToLower();
                }
                output = PlayState.GetText(buffer);
                break;
            case "Ice Snail":
                output = PlayState.GetText("item_iceSnail").Replace("_", species);
                break;
            case "Gravity Snail":
                output = PlayState.GetText("item_gravitySnail").Replace("_", species);
                break;
            case "Full-Metal Snail":
                if (PlayState.currentCharacter == "Sluggy" || PlayState.currentCharacter == "Blobby" || PlayState.currentCharacter == "Leechy")
                    buffer += "fullMetalSnail_noShell";
                else
                    buffer += "fullMetalSnail_generic";
                output = PlayState.GetText(buffer).Replace("_", species);
                break;
            case "Super Secret Boomerang":
                output = PlayState.GetText("item_boomerang_secret");
                break;
            case "Debug Rainbow Wave":
                output = PlayState.GetText("item_rainbowWave_secret");
                break;
        }
        return output;
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
                    transform.position = new Vector2(player.transform.position.x, player.transform.position.y + (box.size.y * 0.75f) + 0.25f);
                    break;
                case 1:
                    transform.position = new Vector2(player.transform.position.x + (box.size.y * 0.75f) + 0.25f, player.transform.position.y);
                    break;
                case 2:
                    transform.position = new Vector2(player.transform.position.x - (box.size.y * 0.75f) - 0.25f, player.transform.position.y);
                    break;
                case 3:
                    transform.position = new Vector2(player.transform.position.x, player.transform.position.y - (box.size.y * 0.75f) - 0.25f);
                    break;
            }
            yield return new WaitForEndOfFrame();
            if (PlayState.gameState == "Game")
                timer += Time.deltaTime;
        }
        SetDeactivated();
    }
}
