using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public bool countedInPercentage = true;
    public bool collected;
    public int itemID = -1;
    public bool isSuperUnique = false;
    public bool[] difficultiesPresentIn = new bool[] { true, true, true };
    public bool[] charactersPresentFor = new bool[] { true, true, true, true, true, true };

    public Vector2 originPos;

    public AnimationModule anim;
    public BoxCollider2D box;
    public SpriteRenderer sprite;
    public AudioSource sfx;

    public AudioClip minorJingle;
    public AudioClip majorJingle;
    
    void Awake()
    {
        if (PlayState.gameState == "Game")
        {
            anim = GetComponent<AnimationModule>();
            box = GetComponent<BoxCollider2D>();
            sprite = GetComponent<SpriteRenderer>();
            sfx = GetComponent<AudioSource>();

            originPos = transform.localPosition;

            if (!difficultiesPresentIn[PlayState.currentDifficulty] || !charactersPresentFor[PlayState.currentCharacter switch
            {
                "Snaily" => 0,
                "Sluggy" => 1,
                "Upside" => 2,
                "Leggy" => 3,
                "Blobby" => 4,
                "Leechy" => 5,
                _ => 0
            }])
            {
                PlayState.itemCollection[itemID] = -1;
                Destroy(gameObject);
            }
        }
    }

    public void Spawn(int[] spawnData)
    {
        itemID = spawnData[0];
        isSuperUnique = spawnData[1] == 1;
        for (int i = 2; i < 5; i++)
            difficultiesPresentIn[i - 2] = spawnData[i] == 1;
        for (int i = 5; i < 11; i++)
            charactersPresentFor[i - 5] = spawnData[i] == 1;

        string animName;

        if (itemID >= PlayState.OFFSET_FRAGMENTS)
        {
            animName = "Item_helixFragment";
            box.size = new Vector2(0.95f, 0.95f);
        }
        else if (itemID >= PlayState.OFFSET_HEARTS)
        {
            animName = "Item_heartContainer";
            box.size = new Vector2(1.95f, 1.95f);
        }
        else
        {
            switch (itemID)
            {
                case 0:
                    animName = "Item_peashooter";
                    box.size = new Vector2(1.825f, 1.825f);
                    break;
                case 1:
                case 11:
                    animName = "Item_boomerang";
                    box.size = new Vector2(1.25f, 1.825f);
                    break;
                case 2:
                case 12:
                    animName = "Item_rainbowWave";
                    box.size = new Vector2(1.25f, 1.825f);
                    break;
                case 4:
                    if (PlayState.currentCharacter == "Blobby")
                        animName = "Item_wallGrab";
                    else
                        animName = "Item_highJump";
                    box.size = new Vector2(1.95f, 1.95f);
                    break;
                case 5:
                    if (PlayState.currentCharacter == "Blobby")
                    {
                        animName = "Item_shelmet";
                        box.size = new Vector2(1.45f, 1.825f);
                    }
                    else
                    {
                        animName = "Item_shellShield";
                        box.size = new Vector2(1.45f, 1.675f);
                    }
                    break;
                case 7:
                    animName = "Item_iceSnail";
                    box.size = new Vector2(1.95f, 1.95f);
                    break;
                default:
                    animName = "Item_helixFragment";
                    box.size = new Vector2(0.95f, 0.95f);
                    break;
            }
        }

        anim.Add(animName);
        anim.Play(animName);
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
                PlayState.playerScript.maxHealth += PlayState.playerScript.hpPerHeart[PlayState.currentDifficulty];
                PlayState.playerScript.health = PlayState.playerScript.maxHealth;
                PlayState.playerScript.RenderNewHearts();
            }
            if (isSuperUnique)
            {
                PlayState.MuteMusic();
                PlayState.PlayMusic(0, 2);
                PlayState.paralyzed = true;
            }
            else
                PlayState.PlayMusic(0, 1);
            switch (itemID)
            {
                case 0:
                    PlayState.isArmed = true;
                    PlayState.playerScript.selectedWeapon = 1;
                    PlayState.playerScript.ChangeActiveWeapon(0, true);
                    break;
                case 1:
                case 11:
                    PlayState.isArmed = true;
                    PlayState.playerScript.selectedWeapon = 2;
                    PlayState.playerScript.ChangeActiveWeapon(1, true);
                    if (itemID == 11)
                        PlayState.QueueAchievementPopup("secrt");
                    break;
                case 2:
                case 12:
                    PlayState.isArmed = true;
                    PlayState.playerScript.selectedWeapon = 3;
                    PlayState.playerScript.ChangeActiveWeapon(2, true);
                    break;
                case 7:
                    if (isSuperUnique)
                        PlayState.playerScript.RunDustRing(1);
                    else
                        PlayState.playerScript.shellStateBuffer = PlayState.GetShellLevel();
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
        else
            PlayState.FlashItemText(IDToName());
    }

    private string IDToName()
    {
        string species = PlayState.GetText("species_" + PlayState.currentCharacter.ToLower());
        return itemID switch
        {
            1 => PlayState.GetText("item_boomerang"),
            2 => PlayState.GetText("item_rainbowWave"),
            3 => PlayState.GetText("item_devastator"),
            4 => PlayState.GetText(PlayState.currentCharacter == "Blobby" ? "item_wallGrab" : "item_highJump"),
            5 => PlayState.GetText(PlayState.currentCharacter == "Blobby" ? "item_shelmet" : "item_shellShield"),
            6 => PlayState.GetText(PlayState.currentCharacter == "Leechy" ? "item_backfire" : "item_rapidFire"),
            7 => PlayState.GetText("item_iceSnail").Replace("_", species),
            8 => (PlayState.currentCharacter switch
            {
                "Upside" => "item_magneticFoot",
                "Leggy" => "item_corkscrewJump",
                "Blobby" => "item_angelJump",
                _ => "item_gravitySnail"
            }).Replace("_", species),
            9 => (PlayState.currentCharacter switch
            {
                "Sluggy" => "item_fullMetalSnail_noShell",
                "Blobby" => "item_fullMetalSnail_blob",
                "Leechy" => "item_fullMetalSnail_noShell",
                _ => "item_fullMetalSnail_generic"
            }).Replace("_", species),
            10 => PlayState.GetText("item_gravityShock"),
            11 => PlayState.GetText("item_boomerang_secret"),
            12 => PlayState.GetText("item_rainbowWave_secret"),
            _ => PlayState.GetText("item_peashooter"),
        };
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
        float jingleTime = PlayState.GetMusic(0, 2).length + 0.5f;
        bool musicMuted = isSuperUnique;
        while (timer < 2)
        {
            Vector2 targetPos = PlayState.playerScript.gravityDir switch
            {
                1 => new Vector2(PlayState.player.transform.position.x + (box.size.y * 0.75f) + 0.25f, PlayState.player.transform.position.y),
                2 => new Vector2(PlayState.player.transform.position.x - (box.size.y * 0.75f) - 0.25f, PlayState.player.transform.position.y),
                3 => new Vector2(PlayState.player.transform.position.x, PlayState.player.transform.position.y - (box.size.y * 0.75f) - 0.25f),
                _ => new Vector2(PlayState.player.transform.position.x, PlayState.player.transform.position.y + (box.size.y * 0.75f) + 0.25f)
            };
            transform.position = Vector2.Lerp(transform.position, targetPos, 15 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
            if (PlayState.gameState == "Game")
                timer += Time.deltaTime;
            if (musicMuted && timer >= jingleTime)
            {
                musicMuted = false;
                PlayState.FadeMusicBackIn();
                PlayState.paralyzed = false;
            }
        }
        SetDeactivated();
        while (musicMuted && timer <= jingleTime)
        {
            yield return new WaitForEndOfFrame();
            if (PlayState.gameState == "Game")
                timer += Time.deltaTime;
        }
        if (musicMuted)
        {
            musicMuted = false;
            PlayState.FadeMusicBackIn();
            PlayState.paralyzed = false;
        }
    }
}
