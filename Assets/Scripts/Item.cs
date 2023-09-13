using System.Collections;
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

    private readonly bool legacyGravCutscene = true;

    private const float UNIQUE_ITEM_CUTSCENE_TIME = 3.5f;

    public Dictionary<string, object> resave() {
        return null;
    }

    public static readonly string myType = "Item";

    public string objType {
        get {
            return myType;
        }
    }

    public Dictionary<string, object> save()
    {
        if (PlayState.itemData.Length == 0)
            PlayState.itemData = new bool[PlayState.currentProfile.items.Length][];
        PlayState.itemData[itemID] = difficultiesPresentIn.Concat(charactersPresentFor).ToArray();
        if (PlayState.countedItems.Length == 0)
            PlayState.countedItems = new bool[PlayState.currentProfile.items.Length];
        PlayState.countedItems[itemID] = countedInPercentage;
        int areaID = transform.parent.GetComponent<RoomTrigger>().areaID;
        if (areaID < 7)
        {
            if (areaID == 6)
                areaID = 5;
            if (!PlayState.itemAreas[areaID].Contains(itemID))
                PlayState.itemAreas[areaID].Add(itemID);
        }
        Dictionary<string, object> content = new();
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

        int charCheck = (PlayState.currentProfile.character switch { "Snaily" => 0, "Sluggy" => 1, "Upside" => 2, "Leggy" => 3, "Blobby" => 4, "Leechy" => 5, _ => 0 });
        if (PlayState.currentProfile.items[itemID] == 0 || !PlayState.itemData[itemID][PlayState.currentProfile.difficulty] || !PlayState.itemData[itemID][charCheck]) {
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

            if (!difficultiesPresentIn[PlayState.currentProfile.difficulty] || !charactersPresentFor[PlayState.currentProfile.character switch {
                "Snaily" => 0,
                "Sluggy" => 1,
                "Upside" => 2,
                "Leggy" => 3,
                "Blobby" => 4,
                "Leechy" => 5,
                _ => 0
            }]) {
                PlayState.currentProfile.items[itemID] = -1;
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
                case 3:
                    animName = "Item_devastator";
                    box.size = new Vector2(2.95f, 1.95f);
                    break;
                case 4:
                    if (PlayState.currentProfile.character == "Blobby")
                        animName = "Item_wallGrab";
                    else
                        animName = "Item_highJump";
                    box.size = new Vector2(1.95f, 1.95f);
                    break;
                case 5:
                    if (PlayState.currentProfile.character == "Blobby") {
                        animName = "Item_shelmet";
                        box.size = new Vector2(1.45f, 1.825f);
                    } else {
                        animName = "Item_shellShield";
                        box.size = new Vector2(1.45f, 1.675f);
                    }
                    break;
                case 6:
                    if (PlayState.currentProfile.character == "Leechy")
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
                    animName = PlayState.currentProfile.character switch
                    {
                        "Upside" => "Item_magneticFoot",
                        "Leggy" => "Item_corkscrewJump",
                        "Blobby" => "Item_angelJump",
                        _ => "Item_gravitySnail"
                    };
                    box.size = new Vector2(1.95f, 1.95f);
                    break;
                case 9:
                    animName = "Item_fullMetalSnail";
                    box.size = new Vector2(1.95f, 1.95f);
                    break;
                case 10:
                    animName = "Item_gravityShock";
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
            if (itemID >= PlayState.OFFSET_HEARTS && itemID < PlayState.OFFSET_FRAGMENTS) {
                PlayState.playerScript.maxHealth += PlayState.globalFunctions.hpPerHeart[PlayState.currentProfile.difficulty];
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
                    if (!PlayState.isArmed)
                        PlayState.TogglableHUDElements[17].GetComponent<ControlPopup>().RunPopup(true, false);
                    PlayState.isArmed = true;
                    PlayState.playerScript.selectedWeapon = 1;
                    PlayState.globalFunctions.ChangeActiveWeapon(0, true);
                    break;
                case 1:
                case 11:
                    if (!PlayState.isArmed)
                        PlayState.TogglableHUDElements[17].GetComponent<ControlPopup>().RunPopup(true, false);
                    PlayState.isArmed = true;
                    PlayState.playerScript.selectedWeapon = 2;
                    PlayState.globalFunctions.ChangeActiveWeapon(1, true);
                    if (itemID == 11)
                        PlayState.QueueAchievementPopup(AchievementPanel.Achievements.SuperSecretBoom);
                    break;
                case 2:
                case 12:
                    if (!PlayState.isArmed)
                        PlayState.TogglableHUDElements[17].GetComponent<ControlPopup>().RunPopup(true, false);
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
                    {
                        PlayState.globalFunctions.RunDustRing(2);
                        PlayState.suppressPause = true;
                        if (legacyGravCutscene)
                            PlayState.globalFunctions.RunLegacyGravCutscene(transform.position);
                    }
                    else
                        PlayState.globalFunctions.shellStateBuffer = PlayState.GetShellLevel();
                    break;
                case 9:
                    if (isSuperUnique)
                        PlayState.globalFunctions.RunDustRing(3);
                    else
                        PlayState.globalFunctions.shellStateBuffer = PlayState.GetShellLevel();
                    break;
                default:
                    if (isSuperUnique)
                        PlayState.globalFunctions.RunDustRing();
                    break;
            }
            FlashItemText();
            if (PlayState.GetItemPercentage() == 100)
            {
                PlayState.globalFunctions.FlashHUDText(GlobalFunctions.TextTypes.completion);
                PlayState.QueueAchievementPopup(AchievementPanel.Achievements.Items100);
            }
            else
                PlayState.globalFunctions.FlashHUDText(GlobalFunctions.TextTypes.collection);
            StartCoroutine(nameof(HoverOverPlayer));
            PlayState.currentProfile.percentage = PlayState.GetItemPercentage();
            PlayState.WriteSave(PlayState.currentProfileNumber, true);
        }
    }

    public void FlashItemText() {
        string itemName = IDToName();
        if (itemID >= PlayState.OFFSET_FRAGMENTS)
            itemName = string.Format(PlayState.GetText("item_helixFragment"), PlayState.CountFragments().ToString());
        else if (itemID >= PlayState.OFFSET_HEARTS)
            itemName = string.Format(PlayState.GetText("item_heartContainer"), PlayState.CountHearts().ToString());
        PlayState.globalFunctions.FlashHUDText(GlobalFunctions.TextTypes.item, itemName);
    }

    private string IDToName()
    {
        string species = PlayState.GetText("species_" + PlayState.currentProfile.character.ToLower());
        return itemID switch
        {
            1 => PlayState.GetText("item_boomerang"),
            2 => PlayState.GetText("item_rainbowWave"),
            3 => PlayState.GetText("item_devastator"),
            4 => PlayState.GetText(PlayState.currentProfile.character == "Blobby" ? "item_wallGrab" : "item_highJump"),
            5 => PlayState.GetText(PlayState.currentProfile.character == "Blobby" ? "item_shelmet" : "item_shellShield"),
            6 => PlayState.GetText(PlayState.currentProfile.character == "Leechy" ? "item_backfire" : "item_rapidFire"),
            7 => string.Format(PlayState.GetText("item_iceSnail"), species),
            8 => string.Format(PlayState.GetText(PlayState.currentProfile.character switch
            {
                "Upside" => "item_magneticFoot",
                "Leggy" => "item_corkscrewJump",
                "Blobby" => "item_angelJump",
                _ => "item_gravitySnail"
            }), species),
            9 => string.Format(PlayState.GetText(PlayState.currentProfile.character switch
            {
                "Sluggy" => "item_fullMetalSnail_noShell",
                "Blobby" => "item_fullMetalSnail_blob",
                "Leechy" => "item_fullMetalSnail_noShell",
                _ => "item_fullMetalSnail_generic"
            }), species),
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
        bool musicMuted = isSuperUnique;
        while (timer < 2)
        {
            Vector2 targetPos = PlayState.playerScript.gravityDir switch
            {
                Player.Dirs.WallL => new Vector2(PlayState.player.transform.position.x + (box.size.y * 0.75f) + 0.25f, PlayState.player.transform.position.y),
                Player.Dirs.WallR => new Vector2(PlayState.player.transform.position.x - (box.size.y * 0.75f) - 0.25f, PlayState.player.transform.position.y),
                Player.Dirs.Ceiling => new Vector2(PlayState.player.transform.position.x, PlayState.player.transform.position.y - (box.size.y * 0.75f) - 0.25f),
                _ => new Vector2(PlayState.player.transform.position.x, PlayState.player.transform.position.y + (box.size.y * 0.75f) + 0.25f)
            };
            transform.position = Vector2.Lerp(transform.position, targetPos, 15 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
            if (PlayState.gameState == PlayState.GameState.game)
                timer += Time.deltaTime;
            if (musicMuted && timer >= UNIQUE_ITEM_CUTSCENE_TIME)
            {
                musicMuted = false;
                if (!(itemID == 8 && legacyGravCutscene))
                    PlayState.FadeMusicBackIn();
                PlayState.paralyzed = false;
            }
        }
        SetDeactivated();
        while (musicMuted && timer <= UNIQUE_ITEM_CUTSCENE_TIME)
        {
            yield return new WaitForEndOfFrame();
            if (PlayState.gameState == PlayState.GameState.game)
                timer += Time.deltaTime;
        }
        if (musicMuted)
        {
            if (!(itemID == 8 && legacyGravCutscene))
                PlayState.FadeMusicBackIn();
            PlayState.paralyzed = false;
        }
    }
}
