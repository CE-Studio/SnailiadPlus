using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : MonoBehaviour
{
    public const int DIR_FLOOR = 0;
    public const int DIR_WALL_LEFT = 1;
    public const int DIR_WALL_RIGHT = 2;
    public const int DIR_CEILING = 3;
    public int currentSurface = 0;
    public bool facingLeft = false;
    public bool facingDown = false;
    public int selectedWeapon = 0;
    public bool armed;
    public int health = 12;
    public int maxHealth = 12;
    public bool stunned = false;
    public bool inDeathCutscene = false;
    public int gravityDir = 0;
    public bool underwater = false;
    public Vector2 velocity = Vector2.zero;
    public bool grounded;
    public bool shelled;

    public AnimationModule anim;
    public SpriteRenderer sprite;
    public BoxCollider2D box;
    public Rigidbody2D rb;
    public GameObject bulletPool;
    public Sprite blank;
    public Sprite smallBlank;
    public Sprite missing;
    public GameObject hearts;
    public GameObject itemTextGroup;
    public GameObject itemPercentageGroup;
    public GameObject gameSaveGroup;

    public LayerMask playerCollide;

    public GameObject debugUp;
    public GameObject debugDown;
    public GameObject debugLeft;
    public GameObject debugRight;
    public GameObject debugJump;
    public GameObject debugShoot;
    public GameObject debugStrafe;
    public List<SpriteRenderer> keySprites = new List<SpriteRenderer>();

    public GameObject weaponIcon1;
    public GameObject weaponIcon2;
    public GameObject weaponIcon3;
    public SpriteRenderer[] weaponIcons;

    public Snaily playerScriptSnaily;

    public double nextLoopEvent;
    private int offsetID;

    // FPS stuff
    int frameCount = 0;
    float dt = 0f;
    float fps = 0f;
    float updateRate = 4;

    // Global sound flag stuff
    int pingTimer = 0;
    int explodeTimer = 0;

    // Area name text
    float areaTextTimer = 0;
    int lastAreaID = -1;
    TextMesh[] areaText;
    public string currentBossName = "";
    bool flashedBossName = false;
    public bool displayDefeatText = false;

    // Start() is called at the very beginning of the script's lifetime. It's used to initialize certain variables and states for components to be in.
    void Start()
    {
        // All this does is set Snaily's components to simpler variables that can be more easily called
        anim = GetComponent<AnimationModule>();
        sprite = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        weaponIcons = new SpriteRenderer[]
        {
            weaponIcon1.GetComponent<SpriteRenderer>(),
            weaponIcon2.GetComponent<SpriteRenderer>(),
            weaponIcon3.GetComponent<SpriteRenderer>()
        };
        foreach (SpriteRenderer sprite in weaponIcons)
            sprite.enabled = false;
        weaponIcons[0].sprite = PlayState.GetSprite("UI/WeaponIcons", 0);
        weaponIcons[1].sprite = PlayState.GetSprite("UI/WeaponIcons", 1);
        weaponIcons[2].sprite = PlayState.GetSprite("UI/WeaponIcons", 2);

        RenderNewHearts();
        UpdateHearts();

        keySprites.Add(debugUp.GetComponent<SpriteRenderer>());
        keySprites.Add(debugDown.GetComponent<SpriteRenderer>());
        keySprites.Add(debugLeft.GetComponent<SpriteRenderer>());
        keySprites.Add(debugRight.GetComponent<SpriteRenderer>());
        keySprites.Add(debugJump.GetComponent<SpriteRenderer>());
        keySprites.Add(debugShoot.GetComponent<SpriteRenderer>());
        keySprites.Add(debugStrafe.GetComponent<SpriteRenderer>());
        StartCoroutine(nameof(DebugKeys));

        areaText = new TextMesh[]
        {
            GameObject.Find("View/Area Name Text/Text").GetComponent<TextMesh>(),
            GameObject.Find("View/Area Name Text/Shadow").GetComponent<TextMesh>()
        };

        UpdateMusic(-1, -1, 3);
    }

    // Update(), called less frequently (every drawn frame), actually gets most of the inputs and converts them to what they should be given any current surface state
    void Update()
    {
        if (PlayState.gameState == "Game")
        {
            rb.WakeUp();

            // Making sure we have weapons
            int[] weaponIDs = new int[] { 0, 1, 2, 11, 12 };
            armed = false;
            foreach (int weapon in weaponIDs)
                if (PlayState.CheckForItem(weapon))
                    armed = true;

            // Noclip!!!
            if (PlayState.noclipMode)
            {
                if (Control.ShootHold())
                {
                    if (Control.UpPress())
                        transform.position = new Vector2(transform.position.x, transform.position.y + 16);
                    if (Control.DownPress())
                        transform.position = new Vector2(transform.position.x, transform.position.y - 16);
                    if (Control.LeftPress())
                        transform.position = new Vector2(transform.position.x - 26, transform.position.y);
                    if (Control.RightPress())
                        transform.position = new Vector2(transform.position.x + 26, transform.position.y);
                }
                else
                    transform.position = new Vector2(transform.position.x + (10 * Control.AxisX() * (Control.JumpHold() ? 2.5f : 1) * Time.deltaTime),
                        transform.position.y + (10 * Control.AxisY() * (Control.JumpHold() ? 2.5f : 1) * Time.deltaTime));
                box.enabled = false;
            }
            else
                box.enabled = true;

            // These are only here to make sure they're called once, before anything else that needs it
            if (PlayState.armorPingPlayedThisFrame)
            {
                pingTimer--;
                if (pingTimer <= 0)
                {
                    pingTimer = Application.targetFrameRate switch { 30 => 1, 60 => 2, 120 => 4, _ => 8 };
                    PlayState.armorPingPlayedThisFrame = false;
                }
            }
            if (PlayState.explodePlayedThisFrame)
            {
                explodeTimer--;
                if (explodeTimer <= 0)
                {
                    explodeTimer = Application.targetFrameRate switch { 30 => 1, 60 => 2, 120 => 4, _ => 8 };
                    PlayState.explodePlayedThisFrame = false;
                }
            }

            // Marking the "has jumped" flag for Snail NPC 01's dialogue
            if (Control.JumpHold())
                PlayState.hasJumped = true;

            // Weapon swapping
            if (Control.Weapon1() && PlayState.CheckForItem(0))
                ChangeActiveWeapon(0);
            if (Control.Weapon2() && (PlayState.CheckForItem(1) || PlayState.CheckForItem(11)))
                ChangeActiveWeapon(1);
            if (Control.Weapon3() && (PlayState.CheckForItem(2) || PlayState.CheckForItem(12)))
                ChangeActiveWeapon(2);

            // Area name text
            if (lastAreaID != PlayState.currentArea)
            {
                lastAreaID = PlayState.currentArea;
                string areaName = PlayState.GetText("area_?");
                switch (lastAreaID)
                {
                    case 0:
                        areaName = PlayState.GetText("area_00");
                        break;
                    case 1:
                        areaName = PlayState.GetText("area_01");
                        break;
                    case 2:
                        areaName = PlayState.GetText("area_02");
                        break;
                    case 3:
                        areaName = PlayState.GetText("area_03");
                        break;
                    case 4:
                        areaName = PlayState.GetText("area_04");
                        break;
                    case 5:
                        if (PlayState.hasSeenIris)
                            areaName = PlayState.GetText("area_iris");
                        break;
                    case 6:
                        areaName = PlayState.GetText("area_iris");
                        break;
                    case 7:
                        areaName = PlayState.GetText("area_bossRush");
                        break;
                }
                if (areaName != areaText[0].text)
                    areaTextTimer = 0;
                areaText[0].text = areaName;
                areaText[1].text = areaName;
            }
            else if (currentBossName != "" && !flashedBossName)
            {
                areaText[0].text = currentBossName;
                areaText[1].text = currentBossName;
                areaTextTimer = 0;
                flashedBossName = true;
            }
            else if (!PlayState.inBossFight && currentBossName != "")
            {
                if (currentBossName == PlayState.GetText("boss_moonSnail"))
                {

                }
                else
                {
                    if (displayDefeatText)
                    {
                        areaText[0].text = PlayState.GetText("boss_defeated").Replace("_", currentBossName);
                        areaText[1].text = PlayState.GetText("boss_defeated").Replace("_", currentBossName);
                        areaTextTimer = 0;
                    }
                    currentBossName = "";
                    flashedBossName = false;
                }
            }
            areaTextTimer = Mathf.Clamp(areaTextTimer + Time.deltaTime, 0, 10);
            if (areaTextTimer < 0.5f)
            {
                areaText[0].color = new Color32(255, 255, 255, (byte)Mathf.Round(Mathf.Lerp(0, 255, areaTextTimer * 2)));
                areaText[1].color = new Color32(0, 0, 0, (byte)Mathf.Round(Mathf.Lerp(0, 255, areaTextTimer * 2)));
            }
            else if (areaTextTimer < 3.5f)
            {
                areaText[0].color = new Color32(255, 255, 255, 255);
                areaText[1].color = new Color32(0, 0, 0, 255);
            }
            else if (areaTextTimer < 4)
            {
                areaText[0].color = new Color32(255, 255, 255, (byte)Mathf.Round(Mathf.Lerp(255, 0, (areaTextTimer - 3.5f) * 2)));
                areaText[1].color = new Color32(0, 0, 0, (byte)Mathf.Round(Mathf.Lerp(255, 0, (areaTextTimer - 3.5f) * 2)));
            }
            else
            {
                areaText[0].color = new Color32(255, 255, 255, 0);
                areaText[1].color = new Color32(0, 0, 0, 0);
            }
        }

        // Audiosource volume control
        PlayState.globalSFX.volume = PlayState.gameOptions[0] * 0.1f;

        // Music
        foreach (AudioSource audio in PlayState.musicSourceArray)
            audio.volume = PlayState.gameOptions[1] * 0.1f;

        if (!PlayState.playingMusic)
            return;

        double time = AudioSettings.dspTime;
        
        if (time + 1 > nextLoopEvent)
        {
            float loadMakeupOffset = 0;
            for (int i = 0 + (PlayState.musFlag ? 0 : 1); i < PlayState.musicParent.GetChild(offsetID).childCount; i += 2)
            {
                AudioSource source = PlayState.musicParent.GetChild(offsetID).GetChild(i).GetComponent<AudioSource>();
                AudioSource altSource = PlayState.musicParent.GetChild(offsetID).GetChild(i + (PlayState.musFlag ? 1 : -1)).GetComponent<AudioSource>();
                source.time = PlayState.musicLoopOffsetLibrary[offsetID].offset;
                loadMakeupOffset = Mathf.Clamp(altSource.clip.length - altSource.time - 1, 0, Mathf.Infinity);
                source.PlayScheduled(nextLoopEvent + loadMakeupOffset);
            }
            nextLoopEvent += PlayState.musicLibrary.library[offsetID + 1][0].length - PlayState.musicLoopOffsetLibrary[offsetID].offset + loadMakeupOffset;
            PlayState.musFlag = !PlayState.musFlag;
        }
    }

    void LateUpdate()
    {
        if (PlayState.gameState == "Game")
        {
            PlayState.fg2Layer.transform.localPosition = new Vector2(
                Mathf.Round((PlayState.cam.transform.position.x - PlayState.camCenter.x) * PlayState.parallaxFg2Mod.x * 16) * 0.0625f,
                Mathf.Round((PlayState.cam.transform.position.y - PlayState.camCenter.y) * PlayState.parallaxFg2Mod.y * 16) * 0.0625f
                );
            PlayState.fg1Layer.transform.localPosition = new Vector2(
                Mathf.Round((PlayState.cam.transform.position.x - PlayState.camCenter.x) * PlayState.parallaxFg1Mod.x * 16) * 0.0625f,
                Mathf.Round((PlayState.cam.transform.position.y - PlayState.camCenter.y) * PlayState.parallaxFg1Mod.y * 16) * 0.0625f
                );
            PlayState.bgLayer.transform.localPosition = new Vector2(
                Mathf.Round((PlayState.cam.transform.position.x - PlayState.camCenter.x) * PlayState.parallaxBgMod.x * 16) * 0.0625f,
                Mathf.Round((PlayState.cam.transform.position.y - PlayState.camCenter.y) * PlayState.parallaxBgMod.y * 16) * 0.0625f
                );
            PlayState.skyLayer.transform.localPosition = new Vector2(
                Mathf.Round((PlayState.cam.transform.position.x - PlayState.camCenter.x) * PlayState.parallaxSkyMod.x * 16) * 0.0625f,
                Mathf.Round((PlayState.cam.transform.position.y - PlayState.camCenter.y) * PlayState.parallaxSkyMod.y * 16) * 0.0625f
                );
        }

        // Update bottom keys
        if (PlayState.gameState == "Game")
        {
            PlayState.TogglableHUDElements[11].SetActive(PlayState.gameOptions[4] == 2);
            PlayState.TogglableHUDElements[3].SetActive(PlayState.gameOptions[4] >= 1);
            PlayState.pauseText.text = Control.ParseKeyName(Control.inputs[22], true);
            PlayState.pauseShadow.text = Control.ParseKeyName(Control.inputs[22], true);
            PlayState.mapText.text = Control.ParseKeyName(Control.inputs[21], true);
            PlayState.mapShadow.text = Control.ParseKeyName(Control.inputs[21], true);
        }

        // FPS calculator
        if (PlayState.gameState == "Game")
        {
            PlayState.TogglableHUDElements[8].SetActive(PlayState.gameOptions[7] == 1);
        }
        frameCount++;
        dt += Time.deltaTime;
        if (dt > 1 / updateRate)
        {
            fps = frameCount / dt;
            frameCount = 0;
            dt -= 1 / updateRate;
        }
        PlayState.fpsText.text = "" + Mathf.Round(fps) + (PlayState.gameOptions[14] != 0 ? "/" + Application.targetFrameRate : "") + PlayState.GetText("hud_fps");
        PlayState.fpsShadow.text = "" + Mathf.Round(fps) + (PlayState.gameOptions[14] != 0 ? "/" + Application.targetFrameRate : "") + PlayState.GetText("hud_fps");

        // Game time counter
        if (PlayState.gameState == "Game")
        {
            PlayState.currentTime[2] += Time.deltaTime;
            PlayState.TogglableHUDElements[9].SetActive(PlayState.gameOptions[6] == 1);
        }
        if (PlayState.currentTime[2] >= 60)
        {
            PlayState.currentTime[2] -= 60;
            PlayState.currentTime[1] += 1;
        }
        if (PlayState.currentTime[1] >= 60)
        {
            PlayState.currentTime[1] -= 60;
            PlayState.currentTime[0] += 1;
        }
        string hourInt = PlayState.currentTime[0] < 10 ? "0" + PlayState.currentTime[0] : (PlayState.currentTime[0] == 0 ? "00" : PlayState.currentTime[0].ToString());
        string minuteInt = PlayState.currentTime[1] < 10 ? "0" + PlayState.currentTime[1] : (PlayState.currentTime[1] == 0 ? "00" : PlayState.currentTime[1].ToString());
        string secondsInt = (Mathf.RoundToInt(PlayState.currentTime[2] * 100) + 10000).ToString();
        PlayState.timeText.text = hourInt + ":" + minuteInt + ":" + secondsInt.Substring(1, 2) + "." + secondsInt.Substring(3, 2);
        PlayState.timeShadow.text = hourInt + ":" + minuteInt + ":" + secondsInt.Substring(1, 2) + "." + secondsInt.Substring(3, 2);
    }

    public void UpdateMusic(int area, int subzone, int resetFlag = 0)
    {
        // resetFlag = 0  -  nothing
        // resetFlag = 1  -  change song
        // resetFlag = 2  -  rebuild array and change song
        // resetFlag = 3  -  rebuild array
        if (resetFlag >= 2) // Hard reset array
        {
            PlayState.musicSourceArray.Clear();
            foreach (Transform obj in PlayState.musicParent.transform)
                Destroy(obj.gameObject);

            for (int i = 0; i < PlayState.musicLibrary.library.Length - 1; i++)
            {
                GameObject newSourceParent = new GameObject();
                newSourceParent.transform.parent = PlayState.musicParent;
                newSourceParent.name = (i < PlayState.musicLibrary.areaThemeOffset - 1) ? "Auxillary group " + i
                    : "Area " + (i - PlayState.musicLibrary.areaThemeOffset + 1) + " music group";
                for (int j = 0; j < PlayState.musicLibrary.library[i + 1].Length; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        GameObject newSource = new GameObject();
                        newSource.transform.parent = newSourceParent.transform;
                        newSource.name = "Subzone " + j + " source " + (k + 1);
                        newSource.AddComponent<AudioSource>();
                        AudioSource newSourceComponent = newSource.GetComponent<AudioSource>();
                        newSourceComponent.clip = PlayState.musicLibrary.library[i + 1][j];
                        PlayState.musicSourceArray.Add(newSourceComponent);
                    }
                }
            }

            StartCoroutine(nameof(LoadAllMusic));
        }
        if (resetFlag != 3) // Change song
        {
            offsetID = area + PlayState.musicLibrary.areaThemeOffset - 1;
            if (resetFlag >= 1)
            {
                PlayState.musFlag = false;
                for (int i = 0; i < PlayState.musicParent.childCount; i++)
                {
                    foreach (Transform source in PlayState.musicParent.GetChild(i))
                    {
                        AudioSource sourceComponent = source.GetComponent<AudioSource>();
                        sourceComponent.time = 0;
                        sourceComponent.mute = true;
                        string[] sourceName = source.name.Split(' ');
                        if (i == offsetID)
                        {
                            if (int.Parse(sourceName[1]) == subzone)
                                sourceComponent.mute = false;
                            else
                                sourceComponent.mute = true;
                            if (int.Parse(sourceName[3]) == 1)
                                sourceComponent.Play();
                            else
                                sourceComponent.Stop();
                        }
                    }
                }
                nextLoopEvent = AudioSettings.dspTime + PlayState.musicLibrary.library[offsetID + 1][0].length;
            }
            for (int i = 0; i * 2 < PlayState.musicParent.GetChild(offsetID).childCount; i++)
            {
                if (i == subzone)
                {
                    PlayState.musicParent.GetChild(offsetID).GetChild(i * 2).GetComponent<AudioSource>().mute = false;
                    PlayState.musicParent.GetChild(offsetID).GetChild(i * 2 + 1).GetComponent<AudioSource>().mute = false;
                }
                else
                {
                    PlayState.musicParent.GetChild(offsetID).GetChild(i * 2).GetComponent<AudioSource>().mute = true;
                    PlayState.musicParent.GetChild(offsetID).GetChild(i * 2 + 1).GetComponent<AudioSource>().mute = true;
                }
            }
            PlayState.playingMusic = true;
        }
    }

    public IEnumerator LoadAllMusic()
    {
        foreach (AudioSource source in PlayState.musicSourceArray)
        {
            source.mute = true;
            source.Play();
        }
        int numberPlaying = 0;
        while (numberPlaying < PlayState.musicSourceArray.Count)
        {
            foreach (AudioSource source in PlayState.musicSourceArray)
                if (source.isPlaying)
                    numberPlaying++;
            yield return null;
        }
        while (numberPlaying > 0)
        {
            foreach (AudioSource source in PlayState.musicSourceArray)
            {
                if (source.isPlaying)
                {
                    numberPlaying--;
                    source.Stop();
                }
            }
            yield return null;
        }
    }

    public void StopMusic()
    {
        PlayState.playingMusic = false;
        foreach (AudioSource source in PlayState.musicSourceArray)
            source.Stop();
    }

    public void ChangeActiveWeapon(int weaponID, bool activateThisWeapon = false)
    {
        weaponIcons[0].sprite = PlayState.GetSprite("UI/WeaponIcons", 0);
        weaponIcons[1].sprite = PlayState.GetSprite("UI/WeaponIcons", 1);
        weaponIcons[2].sprite = PlayState.GetSprite("UI/WeaponIcons", 2);
        if ((weaponID + 1 > selectedWeapon && activateThisWeapon) || !activateThisWeapon)
            selectedWeapon = weaponID + 1;
        if (activateThisWeapon)
            weaponIcons[weaponID].enabled = true;
        if (weaponID == 2)
            weaponIcons[2].sprite = PlayState.GetSprite("UI/WeaponIcons", selectedWeapon == weaponID + 1 ? 5 : 2);
        else if (weaponID == 1)
            weaponIcons[1].sprite = PlayState.GetSprite("UI/WeaponIcons", selectedWeapon == weaponID + 1 ? 4 : 1);
        else
            weaponIcons[0].sprite = PlayState.GetSprite("UI/WeaponIcons", selectedWeapon == weaponID + 1 ? 3 : 0);
    }

    public void ChangeWeaponIconSprite(int weaponID, int state)
    {
        weaponIcons[weaponID].enabled = state != 0;
        Sprite icon = null;
        if (weaponID == 0)
            icon = state == 2 ? PlayState.GetSprite("UI/WeaponIcons", 3) : PlayState.GetSprite("UI/WeaponIcons", 0);
        else if (weaponID == 1)
            icon = state == 2 ? PlayState.GetSprite("UI/WeaponIcons", 4) : PlayState.GetSprite("UI/WeaponIcons", 1);
        else if (weaponID == 2)
            icon = state == 2 ? PlayState.GetSprite("UI/WeaponIcons", 5) : PlayState.GetSprite("UI/WeaponIcons", 2);
        weaponIcons[weaponID].sprite = icon;
    }

    // This coroutine here is meant to display the keypress indicators intended for debugging purposes
    IEnumerator DebugKeys()
    {
        while (true)
        {
            foreach (SpriteRenderer key in keySprites)
                key.gameObject.SetActive(PlayState.gameOptions[5] == 1);

            keySprites[0].sprite = Control.UpHold() ? PlayState.GetSprite("UI/DebugKey", 2) : PlayState.GetSprite("UI/DebugKey", 0);
            keySprites[1].sprite = Control.DownHold() ? PlayState.GetSprite("UI/DebugKey", 2) : PlayState.GetSprite("UI/DebugKey", 0);
            keySprites[2].sprite = Control.LeftHold() ? PlayState.GetSprite("UI/DebugKey", 2) : PlayState.GetSprite("UI/DebugKey", 0);
            keySprites[3].sprite = Control.RightHold() ? PlayState.GetSprite("UI/DebugKey", 2) : PlayState.GetSprite("UI/DebugKey", 0);
            keySprites[4].sprite = Control.JumpHold() ? PlayState.GetSprite("UI/DebugKey", 2) : PlayState.GetSprite("UI/DebugKey", 0);
            keySprites[5].sprite = Control.ShootHold() ? PlayState.GetSprite("UI/DebugKey", 2) : PlayState.GetSprite("UI/DebugKey", 0);
            keySprites[6].sprite = Control.StrafeHold() ? PlayState.GetSprite("UI/DebugKey", 2) : PlayState.GetSprite("UI/DebugKey", 0);

            yield return new WaitForEndOfFrame();
        }
    }

    public void ExecuteCoverCommand(string type, byte r = 0, byte g = 0, byte b = 0, byte a = 0, float maxTime = 0)
    {
        switch (type)
        {
            case "Room Transition":
                StartCoroutine(nameof(CoverRoomTransition));
                break;
            case "Death Transition":
                StartCoroutine(nameof(CoverDeathTransition));
                break;
            case "Custom Fade":
                StartCoroutine(CoverCustomFade(r, g, b, a, maxTime));
                break;
        }
    }

    public IEnumerator CoverRoomTransition()
    {
        SpriteRenderer sprite = PlayState.screenCover;
        while (sprite.color.a > 0)
        {
            yield return new WaitForFixedUpdate();
            sprite.color = new Color32(0, 0, 0, (byte)Mathf.Clamp((sprite.color.a * 255) - 15, 0, Mathf.Infinity));
        }
    }

    public IEnumerator CoverDeathTransition()
    {
        SpriteRenderer sprite = PlayState.screenCover;
        float timer = 0;
        while (sprite.color.a < 1)
        {
            yield return new WaitForFixedUpdate();
            sprite.color = new Color32(0, 64, 127, (byte)Mathf.Lerp(0, 255, timer * 2));
            timer += Time.fixedDeltaTime;
        }
    }

    public IEnumerator CoverCustomFade(byte r, byte g, byte b, byte a, float maxTime)
    {
        SpriteRenderer sprite = PlayState.screenCover;
        float timer = 0;
        Color32 startColor = sprite.color;
        while (timer < maxTime)
        {
            yield return new WaitForFixedUpdate();
            sprite.color = new Color32((byte)Mathf.Lerp(startColor.r, r, timer / maxTime),
                (byte)Mathf.Lerp(startColor.g, g, timer / maxTime),
                (byte)Mathf.Lerp(startColor.b, b, timer / maxTime),
                (byte)Mathf.Lerp(startColor.a, a, timer / maxTime));
            timer += Time.fixedDeltaTime;
        }
    }

    public void RenderNewHearts()
    {
        if (hearts.transform.childCount != 0)
        {
            for (int i = hearts.transform.childCount - 1; i > -1; i--)
            {
                Destroy(hearts.transform.GetChild(i).gameObject);
            }
        }
        for (int i = 0; i < maxHealth * 0.25f; i++)
        {
            GameObject NewHeart = new GameObject();
            NewHeart.transform.parent = hearts.transform;
            NewHeart.transform.localPosition = new Vector3(-12 + (0.5f * (i % 7)), 7 - (0.5f * ((i / 7) % 7)), 0);
            NewHeart.AddComponent<SpriteRenderer>();
            NewHeart.AddComponent<AnimationModule>();
            AnimationModule heartAnim = NewHeart.GetComponent<AnimationModule>();
            heartAnim.Add("Heart0");
            heartAnim.Add("Heart1");
            heartAnim.Add("Heart2");
            heartAnim.Add("Heart3");
            heartAnim.Add("Heart4");
            heartAnim.Play("Heart4");
            NewHeart.GetComponent<SpriteRenderer>().sortingOrder = -1;
            NewHeart.name = "Heart " + (i + 1) + " (HP " + (i * 4) + "-" + (i * 4 + 4) + ")";
        }
    }

    public void UpdateHearts()
    {
        if (hearts.transform.childCount != 0)
        {
            int totalOfPreviousHearts = 0;
            for (int i = 0; i < hearts.transform.childCount; i++)
            {
                switch (health - totalOfPreviousHearts)
                {
                    case 1:
                        hearts.transform.GetChild(i).GetComponent<AnimationModule>().Play("Heart1");
                        break;
                    case 2:
                        hearts.transform.GetChild(i).GetComponent<AnimationModule>().Play("Heart2");
                        break;
                    case 3:
                        hearts.transform.GetChild(i).GetComponent<AnimationModule>().Play("Heart3");
                        break;
                    default:
                        if (Mathf.Sign(health - totalOfPreviousHearts) == 1 && (health - totalOfPreviousHearts) != 0)
                            hearts.transform.GetChild(i).GetComponent<AnimationModule>().Play("Heart4");
                        else
                            hearts.transform.GetChild(i).GetComponent<AnimationModule>().Play("Heart0");
                        break;
                }
                totalOfPreviousHearts += 4;
            }
        }
    }

    public void HitFor(int damage)
    {
        if (stunned || inDeathCutscene)
            return;

        if (health - damage <= 0)
            StartCoroutine(nameof(DieAndRespawn));
        else
            StartCoroutine(StunTimer(damage));
    }

    public IEnumerator StunTimer(int damage)
    {
        if (shelled && PlayState.CheckForItem("Shell Shield"))
            PlayState.PlaySound("Ping");
        else
        {
            health = Mathf.RoundToInt(Mathf.Clamp(health - damage, 0, Mathf.Infinity));
            UpdateHearts();
            PlayState.PlaySound("Hurt");
        }
        stunned = true;
        float timer = 0;
        while (timer < 1)
        {
            if (PlayState.gameState == "Game")
            {
                sprite.enabled = !sprite.enabled;
                timer += Time.deltaTime;
            }
            else if (PlayState.gameState == "Menu")
                timer = 1;
            yield return new WaitForEndOfFrame();
        }
        if (PlayState.gameState != "Menu")
        {
            sprite.enabled = true;
            stunned = false;
        }
    }

    public void FlashItemText(string itemName)
    {
        StartCoroutine(FlashText("item", itemName));
    }

    public void FlashCollectionText()
    {
        StartCoroutine(FlashText("collection"));
    }

    public void FlashSaveText()
    {
        StartCoroutine(FlashText("save"));
    }

    public IEnumerator FlashText(string textType, string itemName = "No item")
    {
        float timer = 0;
        int colorPointer = 0;
        int colorCooldown = 0;
        switch (textType)
        {
            default:
                yield return new WaitForEndOfFrame();
                break;
            case "item":
                SetTextAlpha("item", 255);
                SetTextDisplayed("item", itemName);
                while (timer < 3)
                {
                    if (colorCooldown <= 0)
                    {
                        switch (colorPointer)
                        {
                            case 0:
                                itemTextGroup.transform.GetChild(0).GetComponent<TextMesh>().color = new Color32(189, 191, 198, 255);
                                break;
                            case 1:
                                itemTextGroup.transform.GetChild(0).GetComponent<TextMesh>().color = new Color32(247, 196, 223, 255);
                                break;
                            case 2:
                                itemTextGroup.transform.GetChild(0).GetComponent<TextMesh>().color = new Color32(252, 214, 136, 255);
                                break;
                            case 3:
                                itemTextGroup.transform.GetChild(0).GetComponent<TextMesh>().color = new Color32(170, 229, 214, 255);
                                break;
                        }
                        colorPointer++;
                        if (colorPointer >= 4)
                        {
                            colorPointer = 0;
                        }
                        colorCooldown = 2;
                    }
                    else
                        colorCooldown--;

                    if (timer > 2.5f)
                    {
                        SetTextAlpha("item", Mathf.RoundToInt(Mathf.Lerp(255, 0, (timer - 2.5f) * 2)));
                    }
                    yield return new WaitForFixedUpdate();
                    timer += Time.deltaTime;
                }
                SetTextAlpha("item", 0);
                break;
            case "collection":
                SetTextAlpha("collection", 255);
                SetTextDisplayed("collection", PlayState.GetText("hud_collectedItemPercentage").Replace("#", PlayState.GetItemPercentage().ToString()));
                while (timer < 2)
                {
                    if (colorCooldown <= 0)
                    {
                        switch (colorPointer)
                        {
                            case 0:
                                itemPercentageGroup.transform.GetChild(0).GetComponent<TextMesh>().color = new Color32(189, 191, 198, 255);
                                break;
                            case 1:
                                itemPercentageGroup.transform.GetChild(0).GetComponent<TextMesh>().color = new Color32(247, 196, 223, 255);
                                break;
                            case 2:
                                itemPercentageGroup.transform.GetChild(0).GetComponent<TextMesh>().color = new Color32(252, 214, 136, 255);
                                break;
                            case 3:
                                itemPercentageGroup.transform.GetChild(0).GetComponent<TextMesh>().color = new Color32(170, 229, 214, 255);
                                break;
                        }
                        colorPointer++;
                        if (colorPointer >= 4)
                        {
                            colorPointer = 0;
                        }
                        colorCooldown = 2;
                    }
                    else
                        colorCooldown--;

                    if (timer > 1.5f)
                    {
                        SetTextAlpha("collection", Mathf.RoundToInt(Mathf.Lerp(255, 0, (timer - 1.5f) * 2)));
                    }
                    yield return new WaitForFixedUpdate();
                    timer += Time.deltaTime;
                }
                SetTextAlpha("collection", 0);
                break;
            case "save":
                SetTextAlpha("save", 255);
                while (timer < 2.5f)
                {
                    if (timer > 2)
                    {
                        SetTextAlpha("save", Mathf.RoundToInt(Mathf.Lerp(255, 0, (timer - 2) * 1.5f)));
                    }
                    yield return new WaitForFixedUpdate();
                    timer += Time.deltaTime;
                }
                SetTextAlpha("save", 0);
                break;
        }
        yield return new WaitForEndOfFrame();
    }

    void SetTextAlpha(string textGroup, int alpha)
    {
        switch (textGroup)
        {
            case "item":
                foreach (Transform textObj in itemTextGroup.transform)
                {
                    textObj.GetComponent<TextMesh>().color = new Color32(
                        (byte)(textObj.GetComponent<TextMesh>().color.r * 255),
                        (byte)(textObj.GetComponent<TextMesh>().color.g * 255),
                        (byte)(textObj.GetComponent<TextMesh>().color.b * 255),
                        (byte)alpha
                        );
                }
                break;
            case "collection":
                foreach (Transform textObj in itemPercentageGroup.transform)
                {
                    textObj.GetComponent<TextMesh>().color = new Color32(
                        (byte)(textObj.GetComponent<TextMesh>().color.r * 255),
                        (byte)(textObj.GetComponent<TextMesh>().color.g * 255),
                        (byte)(textObj.GetComponent<TextMesh>().color.b * 255),
                        (byte)alpha
                        );
                }
                break;
            case "save":
                foreach (Transform textObj in gameSaveGroup.transform)
                {
                    textObj.GetComponent<TextMesh>().color = new Color32(
                        (byte)(textObj.GetComponent<TextMesh>().color.r * 255),
                        (byte)(textObj.GetComponent<TextMesh>().color.g * 255),
                        (byte)(textObj.GetComponent<TextMesh>().color.b * 255),
                        (byte)alpha
                        );
                }
                break;
        }
    }

    void SetTextDisplayed(string textGroup, string textToDisplay)
    {
        switch (textGroup)
        {
            case "item":
                foreach (Transform textObj in itemTextGroup.transform)
                    textObj.GetComponent<TextMesh>().text = textToDisplay;
                break;
            case "collection":
                foreach (Transform textObj in itemPercentageGroup.transform)
                    textObj.GetComponent<TextMesh>().text = textToDisplay;
                break;
            case "save":
                foreach (Transform textObj in gameSaveGroup.transform)
                    textObj.GetComponent<TextMesh>().text = textToDisplay;
                break;
        }
    }

    IEnumerator DieAndRespawn()
    {
        ExitShell();
        health = 0;
        UpdateHearts();
        inDeathCutscene = true;
        box.enabled = false;
        PlayState.paralyzed = true;
        PlayState.PlaySound("Death");
        float timer = 0;
        bool hasStartedTransition = false;
        Vector3 fallDir = new Vector3(0.125f, 0.35f, 0);
        if (!facingLeft)
            fallDir = new Vector3(-0.125f, 0.35f, 0);
        while ((timer < 1.6f && PlayState.quickDeathTransition) || (timer < 2 && !PlayState.quickDeathTransition))
        {
            transform.position += fallDir;
            fallDir = new Vector3(fallDir.x, Mathf.Clamp(fallDir.y - 0.025f, -0.5f, Mathf.Infinity), 0);
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
            if (timer > 1 && !hasStartedTransition)
            {
                hasStartedTransition = true;
                PlayState.ScreenFlash("Death Transition");
            }
        }
        yield return new WaitForEndOfFrame();
        if (PlayState.positionOfLastRoom == PlayState.positionOfLastSave)
        {
            Transform deathLocation = PlayState.roomTriggerParent.transform.GetChild((int)PlayState.positionOfLastRoom.x).GetChild((int)PlayState.positionOfLastRoom.y);
            deathLocation.GetComponent<Collider2D>().enabled = true;
            deathLocation.GetComponent<RoomTrigger>().active = true;
            deathLocation.GetComponent<RoomTrigger>().DespawnEverything();
        }
        PlayState.ToggleBossfightState(false, 0, true);
        transform.position = PlayState.respawnCoords;
        inDeathCutscene = false;
        box.enabled = true;
        PlayState.paralyzed = false;
        health = maxHealth;
        UpdateHearts();
        yield return new WaitForEndOfFrame();
        PlayState.ScreenFlash("Room Transition");
    }

    public void ExitShell()
    {
        switch (PlayState.currentCharacter)
        {
            case "Snaily":
                if (playerScriptSnaily.shelled)
                    playerScriptSnaily.ToggleShell();
                break;
        }
    }

    public void LoadClip(string path, string name, Vector2 location)
    {
        StartCoroutine(LoadClipCoroutine(path, name, location));
    }
    public IEnumerator LoadClipCoroutine(string path, string name, Vector2 location)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + UnityWebRequest.EscapeURL(path), AudioType.OGGVORBIS))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                AudioClip newSound = DownloadHandlerAudioClip.GetContent(www);
                newSound.name = name;
                if (location.x == -1 && location.y == -1)
                    PlayState.soundLibrary.library[Array.IndexOf(PlayState.soundLibrary.referenceList, name)] = newSound;
                else
                    PlayState.musicLibrary.library[(int)location.x][(int)location.y] = newSound;
                PlayState.importJobs--;
            }
        }
    }

    public void WaitForImportJobCompletion(bool startMenuMusic = false)
    {
        if (!PlayState.paralyzed)
        {
            PlayState.paralyzed = true;
            StartCoroutine(WaitForJobCompletionCoroutine(startMenuMusic));
        }
    }
    public IEnumerator WaitForJobCompletionCoroutine(bool startMenuMusic)
    {
        while (PlayState.importJobs > 0)
            yield return null;

        PlayState.importJobs = 0;
        PlayState.paralyzed = false;
        if (startMenuMusic)
        {
            UpdateMusic(-1, -1, 3);
            PlayState.mainMenu.GetComponent<MainMenu>().music.clip = PlayState.GetMusic(0, 0);
            PlayState.mainMenu.GetComponent<MainMenu>().music.Play();
        }
        PlayState.ToggleLoadingIcon(false);
    }

    public void RequestQueuedExplosion(Vector2 pos, float lifeTime, int size, bool loudly)
    {
        GameObject queuedExplosion = Instantiate(Resources.Load<GameObject>("Objects/Queued Explosion"), pos, Quaternion.identity,
            PlayState.roomTriggerParent.transform.GetChild((int)PlayState.positionOfLastRoom.x).GetChild((int)PlayState.positionOfLastRoom.y).transform);
        queuedExplosion.GetComponent<QueuedExplosion>().Spawn(lifeTime, size, loudly);
    }
}
