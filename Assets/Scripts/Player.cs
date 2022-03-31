using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public const int DIR_FLOOR = 0;
    public int currentSurface = 0;
    public bool facingLeft = false;
    public bool facingDown = false;
    public int selectedWeapon = 0;
    public int health = 12;
    public int maxHealth = 12;
    public bool stunned = false;
    private string currentAnim = "";
    public bool inDeathCutscene = false;
    public int gravityDir = 0;
    public bool underwater = false;
    public Vector2 velocity = Vector2.zero;

    public Animator anim;
    public SpriteRenderer sprite;
    public BoxCollider2D box;
    public Rigidbody2D rb;
    public AudioSource sfx;
    public AudioClip hurt;
    public AudioClip die;
    public GameObject bulletPool;
    public Sprite blank;
    public Sprite missing;
    //public Sprite iconPeaDeselected;
    //public Sprite iconPeaSelected;
    //public Sprite iconBoomDeselected;
    //public Sprite iconBoomSelected;
    //public Sprite iconWaveDeselected;
    //public Sprite iconWaveSelected;
    public GameObject hearts;
    //public Sprite heart0;
    //public Sprite heart1;
    //public Sprite heart2;
    //public Sprite heart3;
    //public Sprite heart4;
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
    //public Sprite keyIdle;
    //public Sprite keyPressed;
    //public Sprite keyHeld;
    public List<SpriteRenderer> keySprites = new List<SpriteRenderer>();

    public GameObject weaponIcon1;
    public GameObject weaponIcon2;
    public GameObject weaponIcon3;
    public SpriteRenderer[] weaponIcons;

    public Snaily playerScriptSnaily;

    public double nextLoopEvent;

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

    // Start() is called at the very beginning of the script's lifetime. It's used to initialize certain variables and states for components to be in.
    void Start()
    {
        // All this does is set Snaily's components to simpler variables that can be more easily called
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        sfx = GetComponent<AudioSource>();
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

        PlayState.AssignProperCollectibleIDs();

        keySprites.Add(debugUp.GetComponent<SpriteRenderer>());
        keySprites.Add(debugDown.GetComponent<SpriteRenderer>());
        keySprites.Add(debugLeft.GetComponent<SpriteRenderer>());
        keySprites.Add(debugRight.GetComponent<SpriteRenderer>());
        keySprites.Add(debugJump.GetComponent<SpriteRenderer>());
        keySprites.Add(debugShoot.GetComponent<SpriteRenderer>());
        keySprites.Add(debugStrafe.GetComponent<SpriteRenderer>());
        StartCoroutine("DebugKeys");

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

            anim.speed = 1;

            // These are only here to make sure they're called once, before anything else that needs it
            if (PlayState.armorPingPlayedThisFrame)
            {
                pingTimer++;
                if (pingTimer >= 7)
                {
                    pingTimer = 0;
                    PlayState.armorPingPlayedThisFrame = false;
                }
            }
            if (PlayState.explodePlayedThisFrame)
            {
                explodeTimer++;
                if (explodeTimer >= 7)
                {
                    explodeTimer = 0;
                    PlayState.explodePlayedThisFrame = false;
                }
            }

            // Marking the "has jumped" flag for Snail NPC 01's dialogue
            if (Control.JumpHold())
                PlayState.hasJumped = true;

            // Weapon swapping
            if (Control.Weapon1() && PlayState.CheckForItem(0))
                ChangeActiveWeapon(0);
            else if (Control.Weapon2() && (PlayState.CheckForItem(1) || PlayState.CheckForItem(11)))
                ChangeActiveWeapon(1);
            else if (Control.Weapon3() && (PlayState.CheckForItem(2) || PlayState.CheckForItem(12)))
                ChangeActiveWeapon(2);

            // Area name text
            if (lastAreaID != PlayState.currentArea)
            {
                lastAreaID = PlayState.currentArea;
                string areaName = "???";
                switch (lastAreaID)
                {
                    case 0:
                        areaName = "Snail Town";
                        break;
                    case 1:
                        areaName = "Mare Carelia";
                        break;
                    case 2:
                        areaName = "Spiralis Silere";
                        break;
                    case 3:
                        areaName = "Amastrida Abyssus";
                        break;
                    case 4:
                        areaName = "Lux Lirata";
                        break;
                    case 5:
                        if (PlayState.hasSeenIris)
                            areaName = "Shrine of Iris";
                        break;
                    case 6:
                        areaName = "Shrine of Iris";
                        break;
                    case 7:
                        areaName = "Boss Rush";
                        break;
                }
                if (areaName != areaText[0].text)
                    areaTextTimer = 0;
                areaText[0].text = areaName;
                areaText[1].text = areaName;
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

            sfx.volume = PlayState.gameOptions[0] * 0.1f;
        }
        else
            anim.speed = 0;

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
            for (int i = 0 + (PlayState.musFlag ? 0 : 1); i < PlayState.musicParent.GetChild(PlayState.currentArea).childCount; i += 2)
            {
                AudioSource source = PlayState.musicParent.GetChild(PlayState.currentArea).GetChild(i).GetComponent<AudioSource>();
                source.time = PlayState.musicLoopOffsetLibrary[PlayState.currentArea].offset;
                source.PlayScheduled(nextLoopEvent);
            }
            nextLoopEvent += PlayState.musicLibrary.library[PlayState.currentArea + 1][0].length - PlayState.musicLoopOffsetLibrary[PlayState.currentArea].offset;
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
            PlayState.pauseText.gameObject.transform.localPosition = new Vector2(-12.4375f, -7.3775f + (PlayState.gameOptions[5] == 1 ? 2 : (
                PlayState.gameOptions[6] == 1 && PlayState.gameOptions[7] == 1 ? 1 : (PlayState.gameOptions[6] != 1 && PlayState.gameOptions[7] != 1 ? 0 : 0.5f))));
            PlayState.pauseShadow.gameObject.transform.localPosition = new Vector2(-12.375f, -7.44f + (PlayState.gameOptions[5] == 1 ? 2 : (
                PlayState.gameOptions[6] == 1 && PlayState.gameOptions[7] == 1 ? 1 : (PlayState.gameOptions[6] != 1 && PlayState.gameOptions[7] != 1 ? 0 : 0.5f))));
            PlayState.pauseText.text = Control.ParseKeyName(Control.inputs[22], true);
            PlayState.pauseShadow.text = Control.ParseKeyName(Control.inputs[22], true);
            PlayState.mapText.text = Control.ParseKeyName(Control.inputs[21], true);
            PlayState.mapShadow.text = Control.ParseKeyName(Control.inputs[21], true);
        }

        // FPS calculator
        if (PlayState.gameState == "Game")
        {
            PlayState.TogglableHUDElements[8].SetActive(PlayState.gameOptions[7] == 1);
            PlayState.fpsText.gameObject.transform.localPosition = new Vector2(PlayState.gameOptions[5] == 1 ? -10.4375f : -12.4375f,
                PlayState.gameOptions[6] == 1 ? -6.8775f : -7.3775f);
            PlayState.fpsShadow.gameObject.transform.localPosition = new Vector2(PlayState.gameOptions[5] == 1 ? -10.375f : -12.375f,
                PlayState.gameOptions[6] == 1 ? -6.94f : -7.44f);
        }
        frameCount++;
        dt += Time.deltaTime;
        if (dt > 1 / updateRate)
        {
            fps = frameCount / dt;
            frameCount = 0;
            dt -= 1 / updateRate;
        }
        PlayState.fpsText.text = "" + Mathf.Round(fps) + "FPS";
        PlayState.fpsShadow.text = "" + Mathf.Round(fps) + "FPS";

        // Game time counter
        if (PlayState.gameState == "Game")
        {
            PlayState.currentTime[2] += Time.deltaTime;
            PlayState.TogglableHUDElements[9].SetActive(PlayState.gameOptions[6] == 1);
            PlayState.timeText.gameObject.transform.localPosition = new Vector2(PlayState.gameOptions[5] == 1 ? -10.4375f : -12.4375f, -7.3775f);
            PlayState.timeShadow.gameObject.transform.localPosition = new Vector2(PlayState.gameOptions[5] == 1 ? -10.375f : -12.375f, -7.44f);
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
        if (resetFlag >= 2) // Hard reset array and play
        {
            PlayState.musicSourceArray.Clear();
            foreach (Transform obj in PlayState.musicParent.transform)
                Destroy(obj.gameObject);

            for (int i = 0; i < PlayState.musicLibrary.library.Length - 1; i++)
            {
                GameObject newSourceParent = new GameObject();
                newSourceParent.transform.parent = PlayState.musicParent;
                newSourceParent.name = "Area " + i + " music group";
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
        }
        if (resetFlag != 3) // Hard reset array only
        {
            if (resetFlag >= 1) // Change song
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
                        if (i == area)
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
                nextLoopEvent = AudioSettings.dspTime + PlayState.musicLibrary.library[area + 1][0].length;
            }
            for (int i = 0; i * 2 < PlayState.musicParent.GetChild(area).childCount; i++)
            {
                if (i == subzone)
                {
                    PlayState.musicParent.GetChild(area).GetChild(i * 2).GetComponent<AudioSource>().mute = false;
                    PlayState.musicParent.GetChild(area).GetChild(i * 2 + 1).GetComponent<AudioSource>().mute = false;
                }
                else
                {
                    PlayState.musicParent.GetChild(area).GetChild(i * 2).GetComponent<AudioSource>().mute = true;
                    PlayState.musicParent.GetChild(area).GetChild(i * 2 + 1).GetComponent<AudioSource>().mute = true;
                }
            }
            PlayState.playingMusic = true;
        }
    }

    public void StopMusic()
    {
        PlayState.playingMusic = false;
        //PlayState.musicSourceArray.Clear();
        //foreach (Transform obj in PlayState.musicParent.transform)
        //    Destroy(obj.gameObject);
        foreach (AudioSource source in PlayState.musicSourceArray)
            source.Stop();
    }

    public void ChangeActiveWeapon(int weaponID, bool activateThisWeapon = false)
    {
        weaponIcons[0].sprite = PlayState.GetSprite("UI/WeaponIcons", 0);
        weaponIcons[1].sprite = PlayState.GetSprite("UI/WeaponIcons", 1);
        weaponIcons[2].sprite = PlayState.GetSprite("UI/WeaponIcons", 2);
        selectedWeapon = weaponID + 1;
        if (activateThisWeapon)
            weaponIcons[weaponID].enabled = true;
        if (weaponID == 2)
            weaponIcons[2].sprite = PlayState.GetSprite("UI/WeaponIcons", 5);
        else if (weaponID == 1)
            weaponIcons[1].sprite = PlayState.GetSprite("UI/WeaponIcons", 4);
        else
            weaponIcons[0].sprite = PlayState.GetSprite("UI/WeaponIcons", 3);
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
            NewHeart.GetComponent<SpriteRenderer>().sprite = PlayState.GetSprite("UI/Heart", 4);
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
                        hearts.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = PlayState.GetSprite("UI/Heart", 1);
                        break;
                    case 2:
                        hearts.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = PlayState.GetSprite("UI/Heart", 2);
                        break;
                    case 3:
                        hearts.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = PlayState.GetSprite("UI/Heart", 3);
                        break;
                    default:
                        if (Mathf.Sign(health - totalOfPreviousHearts) == 1 && (health - totalOfPreviousHearts) != 0)
                        {
                            hearts.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = PlayState.GetSprite("UI/Heart", 4);
                        }
                        else
                        {
                            hearts.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = PlayState.GetSprite("UI/Heart", 0);
                        }
                        break;
                }
                totalOfPreviousHearts += 4;
            }
        }
    }

    public IEnumerator StunTimer()
    {
        stunned = true;
        sfx.PlayOneShot(hurt);
        UpdateHearts();
        currentSurface = DIR_FLOOR;
        ExitShell();
        float timer = 0;
        while (timer < 1)
        {
            sprite.enabled = !sprite.enabled;
            timer += 0.02f;
            yield return new WaitForFixedUpdate();
        }
        sprite.enabled = true;
        stunned = false;
    }

    public void BecomeStunned()
    {
        StartCoroutine(nameof(StunTimer));
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
                SetTextDisplayed("collection", "Item collection " + PlayState.GetItemPercentage() + "% complete!  Game saved.");
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
        sfx.PlayOneShot(die);
        PlayAnim("die");
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
        transform.position = PlayState.respawnCoords;
        inDeathCutscene = false;
        box.enabled = true;
        PlayAnim("idle");
        PlayState.paralyzed = false;
        health = maxHealth;
        UpdateHearts();
        yield return new WaitForEndOfFrame();
        PlayState.ScreenFlash("Room Transition");
    }

    public void Die()
    {
        StartCoroutine(nameof(DieAndRespawn));
    }

    public void PlayAnim(string state)
    {
        string newAnim = "Normal ";
        if (state != "die")
        {
            if (currentSurface == 1)
                newAnim += "wall ";
            else
                newAnim += "floor ";
        }
        newAnim += state;
        if (newAnim != currentAnim)
        {
            currentAnim = newAnim;
            switch (PlayState.currentCharacter)
            {
                case "Snaily":
                    playerScriptSnaily.anim.Play(newAnim, 0, 0);
                    break;
            }
        }
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
}
