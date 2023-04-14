using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GlobalFunctions : MonoBehaviour
{
    // Pools
    public GameObject playerBulletPool;
    public GameObject enemyBulletPool;

    // General-use textures
    public Sprite blank;
    public Sprite blankSmall;
    public Sprite missing;
    
    // HUD object groups
    public GameObject hearts;
    public GameObject itemTextGroup;
    public GameObject itemPercentageGroup;
    public GameObject gameSaveGroup;

    // Debug keys
    public AnimationModule[] keySprites = new AnimationModule[7];

    // Weapon icons
    public AnimationModule[] weaponIcons = new AnimationModule[3];

    // Health
    public readonly int[] hpPerHeart = new int[] { 8, 4, 2 };

    // Music stuff
    public double nextLoopEvent;
    private int offsetID;
    public bool musicMuted = false;

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

    // Shell state and transformation flags
    public int shellStateBuffer = 0;
    public float shellAnimTimer = 0f;

    // Reference to palette shader component
    public Assets.Scripts.Cam.Effects.RetroPixelMax paletteShader;

    public void Start()
    {
        for (int i = 0; i < weaponIcons.Length; i++)
        {
            weaponIcons[i].Add("WeaponIcon_" + (i + 1) + "_locked");
            weaponIcons[i].Add("WeaponIcon_" + (i + 1) + "_inactive");
            weaponIcons[i].Add("WeaponIcon_" + (i + 1) + "_active");
            weaponIcons[i].Play("WeaponIcon_" + (i + 1) + "_locked");
        }

        areaText = new TextMesh[]
        {
            GameObject.Find("View/Area Name Text/Text").GetComponent<TextMesh>(),
            GameObject.Find("View/Area Name Text/Shadow").GetComponent<TextMesh>()
        };

        paletteShader = GameObject.Find("View/Main Camera").transform.GetComponent<Assets.Scripts.Cam.Effects.RetroPixelMax>();
    }

    public void Update()
    {
        if (PlayState.gameState == PlayState.GameState.game)
        {
            // Global sound timers
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
        PlayState.globalMusic.volume = PlayState.gameOptions[1] * 0.1f;

        // Palette shader toggle
        if ((PlayState.gameOptions[16] == 1 && !paletteShader.enabled) || (PlayState.gameOptions[16] == 0 && paletteShader.enabled))
            paletteShader.enabled = !paletteShader.enabled;

        // Music
        foreach (AudioSource audio in PlayState.musicSourceArray)
            audio.volume = musicMuted ? 0 : Mathf.Lerp(audio.volume, (PlayState.gameOptions[1] * 0.1f) * PlayState.fader, 5 * Time.deltaTime);

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
        if (PlayState.gameState == PlayState.GameState.game)
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
        if (PlayState.gameState == PlayState.GameState.game)
        {
            PlayState.TogglableHUDElements[11].SetActive(PlayState.gameOptions[4] == 2);
            PlayState.TogglableHUDElements[3].SetActive(PlayState.gameOptions[4] >= 1);
            if (PlayState.IsControllerConnected())
            {
                PlayState.pauseText.text = Control.ParseButtonName(Control.Controller.Pause, true);
                PlayState.pauseShadow.text = Control.ParseButtonName(Control.Controller.Pause, true);
                PlayState.mapText.text = Control.ParseButtonName(Control.Controller.Map, true);
                PlayState.mapShadow.text = Control.ParseButtonName(Control.Controller.Map, true);
            }
            else
            {
                PlayState.pauseText.text = Control.ParseKeyName(Control.Keyboard.Pause, true);
                PlayState.pauseShadow.text = Control.ParseKeyName(Control.Keyboard.Pause, true);
                PlayState.mapText.text = Control.ParseKeyName(Control.Keyboard.Map, true);
                PlayState.mapShadow.text = Control.ParseKeyName(Control.Keyboard.Map, true);
            }
        }

        // FPS calculator
        if (PlayState.gameState == PlayState.GameState.game)
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
        if (PlayState.gameState == PlayState.GameState.game)
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
        PlayState.timeText.text = PlayState.GetTimeString();
        PlayState.timeShadow.text = PlayState.GetTimeString();
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

    public void RunDustRing(int tfType = -1)
    {
        StartCoroutine(DustRing(tfType));
    }

    private IEnumerator DustRing(int tfType)
    {
        List<Particle> dustRing = new List<Particle>();
        float spinSpeed = Mathf.PI * 2;
        int particleCount = 16;
        int repeatCount = 0;
        float radius = 14.625f;
        float inwardSpeed = 0.09375f;
        float spinMod = 0f;
        float radiusMod = radius;

        if (shellAnimTimer == 0)
        {
            if (!(PlayState.gameOptions[11] == 3 || PlayState.gameOptions[11] == 5))
                shellStateBuffer = PlayState.GetShellLevel();
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 thisDustPos = new Vector2(
                    transform.position.x + (Mathf.Sin((i / particleCount) * PlayState.TAU) * radius),
                    transform.position.y + (Mathf.Cos((i / particleCount) * PlayState.TAU) * radius)
                    );
                dustRing.Add(PlayState.RequestParticle(thisDustPos, "dust"));
            }
        }
        while (repeatCount >= 0)
        {
            for (int i = 0; i < dustRing.Count; i++)
            {
                float thisCurve = PlayState.TAU / dustRing.Count * i + spinMod * spinSpeed;
                dustRing[i].transform.position = new Vector2(
                    transform.position.x + Mathf.Cos(thisCurve) * radiusMod,
                    transform.position.y - Mathf.Sin(thisCurve) * radiusMod
                    );
            }
            spinMod += Time.deltaTime;
            spinMod = spinMod > PlayState.TAU ? spinMod - PlayState.TAU : spinMod;
            radiusMod -= inwardSpeed * 60 * Time.deltaTime;
            if (radiusMod <= 0)
            {
                repeatCount--;
                if (repeatCount < 0)
                {
                    for (int i = 0; i < dustRing.Count; i++)
                        dustRing[i].ResetParticle();
                }
                else
                    radiusMod = radius;
            }
            yield return new WaitForEndOfFrame();
        }
        if (tfType != -1 && (PlayState.gameOptions[11] == 3 || PlayState.gameOptions[11] == 5))
        {
            shellStateBuffer = tfType;
            PlayState.RequestParticle(transform.position, "transformation", new float[]
            {
                tfType switch
                {
                    2 => PlayState.currentCharacter switch
                    {
                        "Upside" => 3,
                        "Leggy" => 4,
                        "Blobby" => 5,
                        _ => 1
                    },
                    3 => 2,
                    _ => 0
                }
            });
            PlayState.PlaySound("Transformation");
        }
    }

    public void InitializeWeaponIcons()
    {
        
    }

    public void ChangeActiveWeapon(int weaponID, bool activateThisWeapon = false)
    {
        if ((weaponID + 1 > PlayState.playerScript.selectedWeapon && activateThisWeapon) || !activateThisWeapon)
            PlayState.playerScript.selectedWeapon = weaponID + 1;
        for (int i = 0; i < weaponIcons.Length; i++)
        {
            string animName = "WeaponIcon_" + (i + 1);
            if (i switch { 1 => PlayState.CheckForItem(1) || PlayState.CheckForItem(11),
                2 => PlayState.CheckForItem(2) || PlayState.CheckForItem(12),
                _ => PlayState.CheckForItem(0) })
            {
                if (weaponID == i)
                    animName += "_active";
                else
                    animName += "_inactive";
            }
            else
                animName += "_locked";
            weaponIcons[i].Play(animName);
        }
    }

    public void RunDebugKeys()
    {
        foreach (AnimationModule anim in keySprites)
        {
            anim.Add("DebugKey_idle");
            anim.Add("DebugKey_pressed");
            anim.Play("DebugKey_idle");
        }
        StartCoroutine(DebugKeys());
    }
    IEnumerator DebugKeys()
    {
        bool[] keyStates = new bool[keySprites.Length];
        while (true)
        {
            for (int i = 0; i < keySprites.Length; i++)
            {
                bool pressed = i switch
                {
                    1 => Control.DownHold(),
                    2 => Control.LeftHold(),
                    3 => Control.RightHold(),
                    4 => Control.JumpHold(),
                    5 => Control.ShootHold(),
                    6 => Control.StrafeHold(),
                    _ => Control.UpHold()
                };
                if (pressed && !keyStates[i])
                {
                    keySprites[i].Play("DebugKey_pressed");
                    keyStates[i] = true;
                }
                else if (!pressed && keyStates[i])
                {
                    keySprites[i].Play("DebugKey_idle");
                    keyStates[i] = false;
                }
            }

            foreach (AnimationModule sprite in keySprites)
                sprite.GetSpriteRenderer().enabled = PlayState.gameOptions[5] == 1;

            yield return new WaitForEndOfFrame();
        }
    }

    public void ExecuteCoverCommand(string type, byte r = 0, byte g = 0, byte b = 0, byte a = 0, float maxTime = 0, int sortingOrder = 1001)
    {
        switch (type)
        {
            case "Room Transition":
                StartCoroutine(CoverRoomTransition());
                break;
            case "Death Transition":
                StartCoroutine(CoverDeathTransition());
                break;
            case "Custom Fade":
                StartCoroutine(CoverCustomFade(r, g, b, a, maxTime, sortingOrder));
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

    public IEnumerator CoverCustomFade(byte r, byte g, byte b, byte a, float maxTime, int sortingOrder)
    {
        SpriteRenderer sprite = PlayState.screenCover;
        sprite.sortingOrder = sortingOrder;
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
        for (int i = 0; i < PlayState.playerScript.maxHealth * (PlayState.currentDifficulty == 2 ? 0.5f : (PlayState.currentDifficulty == 1 ? 0.25f : 0.125f)); i++)
        {
            GameObject NewHeart = new GameObject();
            NewHeart.transform.parent = hearts.transform;
            NewHeart.transform.localPosition = new Vector3(-12 + (0.5f * (i % 7)), 7 - (0.5f * ((i / 7) % 7)), 0);
            NewHeart.AddComponent<SpriteRenderer>();
            NewHeart.AddComponent<AnimationModule>();
            AnimationModule heartAnim = NewHeart.GetComponent<AnimationModule>();
            for (int j = 0; j <= 8; j++)
                heartAnim.Add("Heart_easy_" + j);
            for (int j = 0; j <= 4; j++)
                heartAnim.Add("Heart_normal_" + j);
            for (int j = 0; j <= 2; j++)
                heartAnim.Add("Heart_insane_" + j);
            heartAnim.Play(PlayState.currentDifficulty == 2 ? "Heart_insane_2" : (PlayState.currentDifficulty == 1 ? "Heart_normal_4" : "Heart_easy_8"));
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
                hearts.transform.GetChild(i).GetComponent<AnimationModule>().Play((PlayState.playerScript.health - totalOfPreviousHearts) switch
                {
                    1 => PlayState.currentDifficulty == 2 ? "Heart_insane_1" : (PlayState.currentDifficulty == 1 ? "Heart_normal_1" : "Heart_easy_1"),
                    2 => PlayState.currentDifficulty == 2 ? "Heart_insane_2" : (PlayState.currentDifficulty == 1 ? "Heart_normal_2" : "Heart_easy_2"),
                    3 => PlayState.currentDifficulty == 2 ? "Heart_insane_2" : (PlayState.currentDifficulty == 1 ? "Heart_normal_3" : "Heart_easy_3"),
                    4 => PlayState.currentDifficulty == 2 ? "Heart_insane_2" : (PlayState.currentDifficulty == 1 ? "Heart_normal_4" : "Heart_easy_4"),
                    5 => PlayState.currentDifficulty == 2 ? "Heart_insane_2" : (PlayState.currentDifficulty == 1 ? "Heart_normal_4" : "Heart_easy_5"),
                    6 => PlayState.currentDifficulty == 2 ? "Heart_insane_2" : (PlayState.currentDifficulty == 1 ? "Heart_normal_4" : "Heart_easy_6"),
                    7 => PlayState.currentDifficulty == 2 ? "Heart_insane_2" : (PlayState.currentDifficulty == 1 ? "Heart_normal_4" : "Heart_easy_7"),
                    8 => PlayState.currentDifficulty == 2 ? "Heart_insane_2" : (PlayState.currentDifficulty == 1 ? "Heart_normal_4" : "Heart_easy_8"),
                    _ => ((PlayState.playerScript.health - totalOfPreviousHearts) > 0) ?
                    (PlayState.currentDifficulty == 2 ? "Heart_insane_2" : (PlayState.currentDifficulty == 1 ? "Heart_normal_4" : "Heart_easy_8")) :
                    (PlayState.currentDifficulty == 2 ? "Heart_insane_0" : (PlayState.currentDifficulty == 1 ? "Heart_normal_0" : "Heart_easy_0"))
                });
                totalOfPreviousHearts += hpPerHeart[PlayState.currentDifficulty];
            }
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

    public void LoadClip(string path, string name, Vector2 location)
    {
        StartCoroutine(LoadClipCoroutine(path, name, location));
    }
    public IEnumerator LoadClipCoroutine(string path, string name, Vector2 location)
    {
        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + UnityWebRequest.EscapeURL(path), AudioType.OGGVORBIS);
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
            MainMenu.music.clip = PlayState.GetMusic(0, 0);
            MainMenu.music.Play();
        }
        PlayState.ToggleLoadingIcon(false);
    }

    public void RequestQueuedExplosion(Vector2 pos, float lifeTime, int size, bool loudly)
    {
        GameObject queuedExplosion = Instantiate(Resources.Load<GameObject>("Objects/Queued Explosion"), pos, Quaternion.identity,
            PlayState.roomTriggerParent.transform.GetChild((int)PlayState.positionOfLastRoom.x).GetChild((int)PlayState.positionOfLastRoom.y).transform);
        queuedExplosion.GetComponent<QueuedExplosion>().Spawn(lifeTime, size, loudly);
    }

    public void ScreenShake(List<float> intensities, List<float> times, float angle = -99999f, float angleVariation = 0f)
    {
        if (PlayState.gameOptions[15] >= 1)
            StartCoroutine(ScreenShakeCoroutine(intensities, times, PlayState.gameOptions[15] == 1 || PlayState.gameOptions[15] == 3, angle, angleVariation));
    }
    public IEnumerator ScreenShakeCoroutine(List<float> intensities, List<float> times, bool minimalShake, float angle = -99999f, float angleVariation = 0f)
    {
        if ((intensities.Count - times.Count == 1) || (intensities.Count == times.Count))
        {
            if (times.Count == intensities.Count + 1)
                intensities.Add(0);
            for (int i = 0; i < intensities.Count; i++)
                intensities[i] = Mathf.Clamp(intensities[i], 0, Mathf.Infinity);
            for (int i = 0; i < times.Count; i++)
                times[i] = Mathf.Clamp(times[i], 0, Mathf.Infinity);

            if (minimalShake)
                for (int i = 0; i < intensities.Count; i++)
                    intensities[i] = Mathf.Clamp(intensities[i] * 0.25f, 0, 0.5f);

            float intensity;
            float time = 0;
            int index = 0;
            while (index < times.Count)
            {
                time += Time.deltaTime;
                if (time >= times[index])
                {
                    time = 0;
                    index++;
                }
                if (index < times.Count)
                {
                    intensity = Mathf.Lerp(index == intensities.Count - 1 ? 0 : intensities[index], intensities[index + 1], time / times[index]);
                    Vector2 intensityVector;

                    if (angle == -99999f)
                        intensityVector = new Vector2(UnityEngine.Random.Range(-intensity, intensity), UnityEngine.Random.Range(-intensity, intensity));
                    else
                    {
                        angleVariation = Mathf.Abs(angleVariation);
                        angle += UnityEngine.Random.Range(-angleVariation, angleVariation);
                        intensityVector = (Vector2)(Quaternion.Euler(0, 0, angle) * Vector2.right) * UnityEngine.Random.Range(-intensity, intensity);
                    }

                    if (PlayState.gameOptions[15] > 2)
                        PlayState.camShakeOffset += intensityVector;
                    else
                        PlayState.camObj.transform.localPosition += (Vector3)intensityVector;
                }
                yield return new WaitForEndOfFrame();
                PlayState.camShakeOffset = Vector2.zero;
                PlayState.camObj.transform.localPosition = new Vector3(0, 0, -10);
            }
        }
        else
            Debug.Log("Unable to parse screen shake command. Expected time count - intensity count difference of 0 or 1, but got " + (times.Count - intensities.Count));
    }
}
