﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Item:MonoBehaviour, IRoomObject {
    [SerializeField] private bool countedInPercentage = true;
    public bool collected;
    public int itemID = -1;
    [SerializeField] private bool isSuperUnique = false;
    public bool[] difficultiesPresentIn = new bool[] { true, true, true };
    public bool[] charactersPresentFor = new bool[] { true, true, true, true, true, true };

    public Vector2 originPos;

    public AnimationModule anim;
    public BoxCollider2D box;
    public SpriteRenderer sprite;
    public AudioSource sfx;

    public AudioClip minorJingle;
    public AudioClip majorJingle;

    public Dictionary<string, object> resave() {
        return null;
    }

    public static readonly string myType = "Item";

    public string objType {
        get {
            return myType;
        }
    }

    public Dictionary<string, object> save() {
        if (PlayState.itemData.Length == 0) {
            PlayState.itemData = new bool[PlayState.itemCollection.Length][];
        }
        PlayState.itemData[itemID] = difficultiesPresentIn.Concat(charactersPresentFor).ToArray();
        Dictionary<string, object> content = new Dictionary<string, object>();
        content["countedInPercentage"] = countedInPercentage;
        content["collected"] = collected;
        content["itemID"] = itemID;
        content["isSuperUnique"] = isSuperUnique;
        content["difficultiesPresentIn"] = difficultiesPresentIn;
        content["charactersPresentFor"] = charactersPresentFor;
        return content;
    }

    public void load(Dictionary<string, object> content) {
        countedInPercentage = (bool)content["countedInPercentage"];
        collected = (bool)content["collected"];
        itemID = (int)content["itemID"];
        isSuperUnique = (bool)content["isSuperUnique"];
        difficultiesPresentIn = (bool[])content["difficultiesPresentIn"];
        charactersPresentFor = (bool[])content["charactersPresentFor"];

        int charCheck = (PlayState.currentCharacter switch { "Snaily" => 0, "Sluggy" => 1, "Upside" => 2, "Leggy" => 3, "Blobby" => 4, "Leechy" => 5, _ => 0 });
        if (PlayState.itemCollection[itemID] == 0 || !PlayState.itemData[itemID][PlayState.currentDifficulty] || !PlayState.itemData[itemID][charCheck]) {
            Spawn();
        } else {
            Destroy(gameObject);
        }

    }

    void Awake() {
        if (PlayState.gameState == PlayState.GameState.game) {
            anim = GetComponent<AnimationModule>();
            box = GetComponent<BoxCollider2D>();
            sprite = GetComponent<SpriteRenderer>();
            sfx = GetComponent<AudioSource>();

            originPos = transform.localPosition;

            if (!difficultiesPresentIn[PlayState.currentDifficulty] || !charactersPresentFor[PlayState.currentCharacter switch {
                "Snaily" => 0,
                "Sluggy" => 1,
                "Upside" => 2,
                "Leggy" => 3,
                "Blobby" => 4,
                "Leechy" => 5,
                _ => 0
            }]) {
                PlayState.itemCollection[itemID] = -1;
                Destroy(gameObject);
            }
        }
    }

    public void Spawn() {
        string animName;

        if (itemID >= PlayState.OFFSET_FRAGMENTS) {
            animName = "Item_helixFragment";
            box.size = new Vector2(0.95f, 0.95f);
        } else if (itemID >= PlayState.OFFSET_HEARTS) {
            animName = "Item_heartContainer";
            box.size = new Vector2(1.95f, 1.95f);
        } else {
            switch (itemID) {
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
                    if (PlayState.currentCharacter == "Blobby") {
                        animName = "Item_shelmet";
                        box.size = new Vector2(1.45f, 1.825f);
                    } else {
                        animName = "Item_shellShield";
                        box.size = new Vector2(1.45f, 1.675f);
                    }
                    break;
                case 6:
                    if (PlayState.currentCharacter == "Leechy")
                        animName = "Item_backfire";
                    else
                        animName = "Item_rapidFire";
                    box.size = new Vector2(1.95f, 1.95f);
                    break;
                case 7:
                    animName = "Item_iceSnail";
                    box.size = new Vector2(1.95f, 1.95f);
                    break;
                case 8:
                    animName = PlayState.currentCharacter switch
                    {
                        "Upside" => "Item_magneticFoot",
                        "Leggy" => "Item_corkscrewJump",
                        "Blobby" => "Item_angelJump",
                        _ => "Item_gravitySnail"
                    };
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

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            collected = true;
            if (PlayState.itemLocations.ContainsKey(PlayState.WorldPosToMapGridID(transform.position)))
                PlayState.itemLocations.Remove(PlayState.WorldPosToMapGridID(transform.position));
            PlayState.minimapScript.RefreshMap();
            PlayState.AddItem(itemID);
            if (itemID >= PlayState.OFFSET_FRAGMENTS)
                PlayState.helixCount++;
            else if (itemID >= PlayState.OFFSET_HEARTS) {
                PlayState.heartCount++;
                PlayState.playerScript.maxHealth += PlayState.globalFunctions.hpPerHeart[PlayState.currentDifficulty];
                PlayState.playerScript.health = PlayState.playerScript.maxHealth;
                PlayState.globalFunctions.RenderNewHearts();
            }
            if (isSuperUnique) {
                PlayState.MuteMusic();
                PlayState.PlayMusic(0, 2);
                PlayState.paralyzed = true;
                PlayState.playerScript.ZeroWalkVelocity();
            } else
                PlayState.PlayMusic(0, 1);
            switch (itemID) {
                case 0:
                    PlayState.isArmed = true;
                    PlayState.playerScript.selectedWeapon = 1;
                    PlayState.globalFunctions.ChangeActiveWeapon(0, true);
                    break;
                case 1:
                case 11:
                    PlayState.isArmed = true;
                    PlayState.playerScript.selectedWeapon = 2;
                    PlayState.globalFunctions.ChangeActiveWeapon(1, true);
                    if (itemID == 11)
                        PlayState.QueueAchievementPopup("secrt");
                    break;
                case 2:
                case 12:
                    PlayState.isArmed = true;
                    PlayState.playerScript.selectedWeapon = 3;
                    PlayState.globalFunctions.ChangeActiveWeapon(2, true);
                    break;
                case 7:
                    if (isSuperUnique)
                        PlayState.globalFunctions.RunDustRing(1);
                    else
                        PlayState.globalFunctions.shellStateBuffer = PlayState.GetShellLevel();
                    break;
                case 8:
                    if (isSuperUnique)
                        PlayState.globalFunctions.RunDustRing(2);
                    else
                        PlayState.globalFunctions.shellStateBuffer = PlayState.GetShellLevel();
                    break;
                default:
                    break;
            }
            FlashItemText();
            PlayState.globalFunctions.FlashCollectionText();
            StartCoroutine(nameof(HoverOverPlayer));
            PlayState.WriteSave("game");
        }
    }

    public void FlashItemText() {
        if (itemID >= PlayState.OFFSET_FRAGMENTS)
            PlayState.globalFunctions.FlashItemText(PlayState.GetText("item_helixFragment").Replace("_", PlayState.helixCount.ToString()));
        else if (itemID >= PlayState.OFFSET_HEARTS)
            PlayState.globalFunctions.FlashItemText(PlayState.GetText("item_heartContainer").Replace("_", PlayState.heartCount.ToString()));
        else
            PlayState.globalFunctions.FlashItemText(IDToName());
    }

    private string IDToName() {
        string species = PlayState.GetText("species_" + PlayState.currentCharacter.ToLower());
        return itemID switch {
            1 => PlayState.GetText("item_boomerang"),
            2 => PlayState.GetText("item_rainbowWave"),
            3 => PlayState.GetText("item_devastator"),
            4 => PlayState.GetText(PlayState.currentCharacter == "Blobby" ? "item_wallGrab" : "item_highJump"),
            5 => PlayState.GetText(PlayState.currentCharacter == "Blobby" ? "item_shelmet" : "item_shellShield"),
            6 => PlayState.GetText(PlayState.currentCharacter == "Leechy" ? "item_backfire" : "item_rapidFire"),
            7 => PlayState.GetText("item_iceSnail").Replace("_", species),
            8 => PlayState.GetText(PlayState.currentCharacter switch {
                "Upside" => "item_magneticFoot",
                "Leggy" => "item_corkscrewJump",
                "Blobby" => "item_angelJump",
                _ => "item_gravitySnail"
            }).Replace("_", species),
            9 => PlayState.GetText(PlayState.currentCharacter switch {
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

    public void SetDeactivated() {
        transform.localPosition = originPos;
        box.enabled = false;
        sprite.enabled = false;
    }

    public IEnumerator HoverOverPlayer() {
        box.enabled = false;
        float timer = 0;
        float jingleTime = PlayState.GetMusic(0, 2).length + 0.5f;
        bool musicMuted = isSuperUnique;
        while (timer < 2) {
            Vector2 targetPos = PlayState.playerScript.gravityDir switch {
                Player.Dirs.WallL => new Vector2(PlayState.player.transform.position.x + (box.size.y * 0.75f) + 0.25f, PlayState.player.transform.position.y),
                Player.Dirs.WallR => new Vector2(PlayState.player.transform.position.x - (box.size.y * 0.75f) - 0.25f, PlayState.player.transform.position.y),
                Player.Dirs.Ceiling => new Vector2(PlayState.player.transform.position.x, PlayState.player.transform.position.y - (box.size.y * 0.75f) - 0.25f),
                _ => new Vector2(PlayState.player.transform.position.x, PlayState.player.transform.position.y + (box.size.y * 0.75f) + 0.25f)
            };
            transform.position = Vector2.Lerp(transform.position, targetPos, 15 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
            if (PlayState.gameState == PlayState.GameState.game)
                timer += Time.deltaTime;
            if (musicMuted && timer >= jingleTime) {
                musicMuted = false;
                PlayState.FadeMusicBackIn();
                PlayState.paralyzed = false;
            }
        }
        SetDeactivated();
        while (musicMuted && timer <= jingleTime) {
            yield return new WaitForEndOfFrame();
            if (PlayState.gameState == PlayState.GameState.game)
                timer += Time.deltaTime;
        }
        if (musicMuted) {
            musicMuted = false;
            PlayState.FadeMusicBackIn();
            PlayState.paralyzed = false;
        }
    }
}
