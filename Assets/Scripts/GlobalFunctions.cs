using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Tilemaps;

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
    public TextObject itemText;
    public TextObject itemPercentageText;
    public TextObject itemCompletionText;
    public TextObject gameSaveText;
    public TextObject newBestTimeText;
    public TextObject newUnlocksText;

    // Debug keys
    public AnimationModule[] keySprites = new AnimationModule[7];

    // Weapon icons
    public AnimationModule[] weaponIcons = new AnimationModule[4];
    private bool bottomKeysAreCon = false;

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
    readonly float updateRate = 4;

    // Global sound flag stuff
    float pingTimer = 0;
    float explodeTimer = 0;

    // Area name text
    float areaTextTimer = 0;
    int lastAreaID = -1;
    TextObject areaText;
    TextObject radarText;
    public string currentBossName = "";
    bool flashedBossName = false;
    public bool displayDefeatText = false;

    // Shell state and transformation flags
    public int shellStateBuffer = 0;
    public float shellAnimTimer = 0f;

    // Reference to palette shader component
    public Assets.Scripts.Cam.Effects.RetroPixelMax paletteShader;

    // Controller input mono
    public ControllerInput conInput;

    // Last sixteen keyboard inputs, for cheat codes
    public KeyCode[] cheatInputs = new KeyCode[16];
    public bool addedCheatInputThisFrame = false;

    // Light mask object
    public static GameObject lightMask;

    // Raw text popup timers
    public int textColorPointer;
    public int textColorCooldown;
    public Color32 textPopupColor;
    public float popupTimerItem;
    public float popupTimerCollection;
    public float popupTimerCompletion;
    public float popupTimerSave;
    public float popupTimerBestTime;
    public float popupTimerUnlock;
    public enum TextTypes
    {
        item,
        collection,
        areaCompletion,
        totalCompletion,
        save,
        bestTime,
        unlock
    }

    public void Awake()
    {
        DeclarePlayStateMono();
        conInput = new ControllerInput();
        Control.conInput = conInput;
        StartCoroutine(Control.HandleController());
    }

    private void OnEnable()
    {
        conInput.Enable();
    }
    private void OnDisable()
    {
        conInput.Disable();
    }

    public void Start()
    {
        for (int i = 0; i < weaponIcons.Length; i++)
        {
            weaponIcons[i].Add("WeaponIcon_" + i + "_locked");
            weaponIcons[i].Add("WeaponIcon_" + i + "_inactive");
            weaponIcons[i].Add("WeaponIcon_" + i + "_active");
            weaponIcons[i].Play("WeaponIcon_" + i + "_locked");
            weaponIcons[i].affectedByGlobalEntityColor = false;
        }

        itemText = GameObject.Find("View/Item Get").GetComponent<TextObject>();
        itemText.SetColor(new Color(1, 1, 1, 0));
        itemPercentageText = GameObject.Find("View/Item Percentage").GetComponent<TextObject>();
        itemPercentageText.SetColor(new Color(1, 1, 1, 0));
        itemCompletionText = GameObject.Find("View/Item Completion").GetComponent<TextObject>();
        itemCompletionText.SetColor(new Color(1, 1, 1, 0));
        gameSaveText = GameObject.Find("View/Game Saved").GetComponent<TextObject>();
        gameSaveText.SetColor(new Color(1, 1, 1, 0));
        newBestTimeText = GameObject.Find("View/New Best Time").GetComponent<TextObject>();
        newBestTimeText.SetColor(new Color(1, 1, 1, 0));
        newUnlocksText = GameObject.Find("View/Mode Unlocks").GetComponent<TextObject>();
        newUnlocksText.SetColor(new Color(1, 1, 1, 0));
        areaText = GameObject.Find("View/Area Name").GetComponent<TextObject>();
        areaText.SetColor(new Color(1, 1, 1, 0));
        radarText = GameObject.Find("View/Radar").GetComponent<TextObject>();
        radarText.SetColor(new Color(1, 1, 1, 0));

        paletteShader = GameObject.Find("View/Main Camera").transform.GetComponent<Assets.Scripts.Cam.Effects.RetroPixelMax>();

        lightMask = Resources.Load<GameObject>("Objects/Light Mask");
        CreateLightMask(12, PlayState.player.transform);
    }

    private void DeclarePlayStateMono()
    {
        PlayState.textureLibrary = GameObject.Find("View").GetComponent<LibraryManager>().textureLibrary;
        PlayState.soundLibrary = GameObject.Find("View").GetComponent<LibraryManager>().soundLibrary;
        PlayState.musicLibrary = GameObject.Find("View").GetComponent<LibraryManager>().musicLibrary;
        PlayState.textLibrary = GameObject.Find("View").GetComponent<LibraryManager>().textLibrary;

        PlayState.musicParent = GameObject.Find("View/Music Parent").transform;
        PlayState.globalSFX = GameObject.Find("View/Global SFX Source").GetComponent<AudioSource>();
        PlayState.globalMusic = GameObject.Find("View/Global Music Source").GetComponent<AudioSource>();

        PlayState.player = GameObject.Find("Player");
        PlayState.playerScript = PlayState.player.GetComponent<Player>();
        PlayState.cam = GameObject.Find("View");
        PlayState.camObj = PlayState.cam.transform.Find("Main Camera").gameObject;
        PlayState.camBorder = PlayState.cam.transform.Find("Border").gameObject;
        PlayState.mainCam = PlayState.camObj.GetComponent<Camera>();
        PlayState.camScript = PlayState.cam.GetComponent<CamMovement>();
        PlayState.screenCover = GameObject.Find("View/Cover").GetComponent<SpriteRenderer>();
        PlayState.groundLayer = GameObject.Find("Grid/Ground").GetComponent<Tilemap>();
        PlayState.fg2Layer = GameObject.Find("Grid/Foreground 2").GetComponent<Tilemap>();
        PlayState.fg1Layer = GameObject.Find("Grid/Foreground").GetComponent<Tilemap>();
        PlayState.bgLayer = GameObject.Find("Grid/Background").GetComponent<Tilemap>();
        PlayState.skyLayer = GameObject.Find("Grid/Sky").GetComponent<Tilemap>();
        PlayState.specialLayer = GameObject.Find("Grid/Special").GetComponent<Tilemap>();
        PlayState.minimap = GameObject.Find("View/Minimap Panel/Minimap");
        PlayState.minimapScript = PlayState.minimap.transform.parent.GetComponent<Minimap>();
        PlayState.achievement = GameObject.Find("View/Achievement Panel");
        PlayState.particlePool = GameObject.Find("Particle Pool");
        PlayState.camParticlePool = PlayState.cam.transform.Find("Camera-synced Particle Layer").gameObject;
        PlayState.roomTriggerParent = GameObject.Find("Room Triggers");
        PlayState.mainMenu = GameObject.Find("View/Menu Parent").GetComponent<MainMenu>();
        PlayState.credits = GameObject.Find("View/Credits Parent").GetComponent<Credits>();
        PlayState.loadingIcon = GameObject.Find("View/Loading Icon");
        PlayState.enemyBulletPool = GameObject.Find("Enemy Bullet Pool");
        PlayState.subscreen = GameObject.Find("View/Subscreen");
        PlayState.subscreenScript = PlayState.subscreen.GetComponent<Subscreen>();
        PlayState.dialogueBox = PlayState.cam.transform.Find("Dialogue Box").gameObject;
        PlayState.dialogueScript = PlayState.dialogueBox.GetComponent<DialogueBox>();
        PlayState.titleParent = GameObject.Find("View/Title Parent");
        PlayState.darknessLayer = GameObject.Find("View/Darkness Layer").GetComponent<SpriteRenderer>();
        PlayState.healthOrbPool = GameObject.Find("Health Orb Pool");
        PlayState.trapManager = GameObject.Find("View/Trap Timers").GetComponent<TrapManager>();

        PlayState.titleRoom = PlayState.roomTriggerParent.transform.Find("Backdrop Rooms/Title").GetComponent<RoomTrigger>();
        PlayState.moonCutsceneRoom = PlayState.roomTriggerParent.transform.Find("Backdrop Rooms/Moon Snail Cutscene").GetComponent<RoomTrigger>();
        PlayState.creditsRoom = PlayState.roomTriggerParent.transform.Find("Backdrop Rooms/Credits").GetComponent<RoomTrigger>();

        PlayState.globalFunctions = this;

        PlayState.hudFps = GameObject.Find("View/FPS").GetComponent<TextObject>();
        PlayState.hudTime = GameObject.Find("View/Time").GetComponent<TextObject>();
        PlayState.hudPause = GameObject.Find("View/Bottom Keys/Pause").GetComponent<TextObject>();
        PlayState.hudMap = GameObject.Find("View/Bottom Keys/Map").GetComponent<TextObject>();
        PlayState.hudRoomName = GameObject.Find("View/Minimap Panel/Room Name").GetComponent<TextObject>();
        PlayState.hudRushTime = GameObject.Find("View/Boss Rush Time").GetComponent<TextObject>();

        PlayState.palette = (Texture2D)Resources.Load("Images/Palette");

        for (int i = 0; i < Enum.GetValues(typeof(PlayState.Areas)).Length; i++)
            PlayState.itemAreas.Add(new List<int>());

        PlayState.TogglableHUDElements = new GameObject[]
        {
            GameObject.Find("View/Minimap Panel"),             //  0
            GameObject.Find("View/Hearts"),                    //  1
            GameObject.Find("View/Debug Keypress Indicators"), //  2
            GameObject.Find("View/Weapon Icons"),              //  3
            GameObject.Find("View/Game Saved"),                //  4
            GameObject.Find("View/Area Name"),                 //  5
            GameObject.Find("View/Item Get"),                  //  6
            GameObject.Find("View/Item Percentage"),           //  7
            GameObject.Find("View/FPS"),                       //  8
            GameObject.Find("View/Time"),                      //  9
            GameObject.Find("View/Dialogue Box"),              // 10
            GameObject.Find("View/Bottom Keys"),               // 11
            GameObject.Find("View/Boss Health Bar"),           // 12
            GameObject.Find("View/Radar"),                     // 13
            GameObject.Find("View/New Best Time"),             // 14
            GameObject.Find("View/Mode Unlocks"),              // 15
            GameObject.Find("View/Item Completion"),           // 16
            GameObject.Find("View/Control Guide"),             // 17
            GameObject.Find("View/Boss Rush Time"),            // 18
        };
        PlayState.TogglableHUDElements[12].GetComponent<AnimationModule>().affectedByGlobalEntityColor = false;
        PlayState.TogglableHUDElements[12].transform.GetChild(0).GetComponent<AnimationModule>().affectedByGlobalEntityColor = false;

        PlayState.respawnScene = SceneManager.GetActiveScene();

        PlayState.blankData.gameVersion = Application.version;
    }

    public void Update()
    {
        if (PlayState.gameState == PlayState.GameState.game)
        {
            // Global sound timers
            if (PlayState.armorPingPlayedThisFrame)
            {
                pingTimer -= Time.deltaTime;
                if (pingTimer <= 0)
                {
                    pingTimer = 0.025f;
                    PlayState.armorPingPlayedThisFrame = false;
                }
            }
            if (PlayState.explodePlayedThisFrame)
            {
                explodeTimer -= Time.deltaTime;
                if (explodeTimer <= 0)
                {
                    explodeTimer = 0.025f;
                    PlayState.explodePlayedThisFrame = false;
                }
            }

            // Cheat management
            addedCheatInputThisFrame = false;

            // Skyfish cheat
            bool skyfishActive = false;
            if (PlayState.currentArea == (int)PlayState.Areas.SnailTown)
            {
                if (CheckCheatCode(new KeyCode[] { KeyCode.S, KeyCode.K, KeyCode.Y, KeyCode.F, KeyCode.I, KeyCode.S, KeyCode.H }))
                {
                    for (int i = 0; i < PlayState.currentProfile.bossStates.Length; i++)
                        PlayState.currentProfile.bossStates[i] = 1;
                    skyfishActive = true;
                }
            }

            // Area name text
            if (lastAreaID != PlayState.currentArea)
            {
                lastAreaID = PlayState.currentArea;
                string areaName = PlayState.GetText("area_?");
                switch ((PlayState.Areas)lastAreaID)
                {
                    case PlayState.Areas.SnailTown:
                        areaName = PlayState.GetText("area_00");
                        break;
                    case PlayState.Areas.MareCarelia:
                        areaName = PlayState.GetText("area_01");
                        break;
                    case PlayState.Areas.SpiralisSilere:
                        areaName = PlayState.GetText("area_02");
                        break;
                    case PlayState.Areas.AmastridaAbyssus:
                        areaName = PlayState.GetText("area_03");
                        break;
                    case PlayState.Areas.LuxLirata:
                        areaName = PlayState.GetText("area_04");
                        break;
                    case PlayState.Areas.ShrineOfIris:
                        if (PlayState.GetNPCVar(PlayState.NPCVarIDs.HasSeenIris) == 1)
                            areaName = PlayState.GetText("area_iris");
                        break;
                    case PlayState.Areas.BossRush:
                        areaName = PlayState.GetText("area_bossRush");
                        break;
                }
                if (areaName != areaText.GetText())
                    areaTextTimer = 0;
                areaText.SetText(areaName);

                radarText.SetText("");
                if (!PlayState.IsBossAlive(3) && lastAreaID < 7)
                {
                    int[] itemData = PlayState.GetAreaItemRate(lastAreaID);
                    if (itemData[1] > 0)
                        radarText.SetText(string.Format(PlayState.GetText("hud_radar"), itemData[0].ToString(), itemData[1].ToString(),
                            itemData[2] == 1 ? "?" : ""));
                }
            }
            else if (currentBossName != "" && !flashedBossName &&
                currentBossName != PlayState.GetText("boss_gigaSnail") && currentBossName != PlayState.GetText("boss_gigaSnail_rush"))
            {
                areaText.SetText(currentBossName);
                radarText.SetText("");
                areaTextTimer = 0;
                flashedBossName = true;
            }
            else if (!PlayState.inBossFight && currentBossName != "")
            {
                if (currentBossName != PlayState.GetText("boss_moonSnail") && currentBossName != PlayState.GetText("boss_moonSnail_rush"))
                {
                    if (displayDefeatText)
                    {
                        string thisBossName = currentBossName;
                        if (thisBossName == PlayState.GetText("boss_gigaSnail"))
                            thisBossName = PlayState.GetText("boss_moonSnail");
                        if (thisBossName == PlayState.GetText("boss_gigaSnail_rush"))
                            thisBossName = PlayState.GetText("boss_moonSnail_rush");
                        areaText.SetText(string.Format(PlayState.GetText("boss_defeated"), thisBossName));
                        areaTextTimer = 0;
                    }
                }
                currentBossName = "";
                flashedBossName = false;
            }
            else if (skyfishActive)
            {
                areaText.SetText(PlayState.GetText("cheat_skyfish"));
                areaTextTimer = 0;
                PlayState.PlaySound("CheatSkyfish");
            }
            areaTextTimer = Mathf.Clamp(areaTextTimer + Time.deltaTime, 0, 10);
            Color textColor;
            if (areaTextTimer < 0.5f)
                textColor = new Color(1, 1, 1, Mathf.Lerp(0, 1, areaTextTimer * 2));
            else if (areaTextTimer < 3.5f)
                textColor = new Color(1, 1, 1, 1);
            else if (areaTextTimer < 4)
                textColor = new Color(1, 1, 1, Mathf.Lerp(1, 0, (areaTextTimer - 3.5f) * 2));
            else
                textColor = new Color(1, 1, 1, 0);
            areaText.SetColor(textColor);
            radarText.SetColor(textColor);
        }

        // All raw text popup timers
        textColorCooldown--;
        if (textColorCooldown <= 0)
        {
            textColorCooldown = 2;
            textPopupColor = textColorPointer switch
            {
                0 => new Color32(189, 191, 198, 255),
                1 => new Color32(247, 198, 223, 255),
                2 => new Color32(252, 214, 136, 255),
                _ => new Color32(170, 229, 214, 255)
            };
            textColorPointer = (textColorPointer + 1) % 4;
        }
        for (int i = 0; i < 6; i++)
        {
            bool useColor = false;
            bool useAreaTextColor = false;
            TextObject targetText = itemText;
            float fadeDeadzone = 0;
            float fadeTime = 0;
            switch (i)
            {
                case 0: // Item
                    targetText = itemText;
                    useColor = true;
                    fadeDeadzone = 0.7f;
                    fadeTime = popupTimerItem;
                    if (popupTimerItem >= 0)
                        popupTimerItem -= Time.deltaTime;
                    break;
                case 1: // Collection
                    targetText = itemPercentageText;
                    useColor = true;
                    fadeDeadzone = 0.7f;
                    fadeTime = popupTimerCollection;
                    if (popupTimerCollection >= 0)
                        popupTimerCollection -= Time.deltaTime;
                    break;
                case 2: // Area/total completion
                    targetText = itemCompletionText;
                    useColor = true;
                    fadeDeadzone = 0.7f;
                    fadeTime = popupTimerCompletion;
                    if (popupTimerCompletion >= 0)
                        popupTimerCompletion -= Time.deltaTime;
                    break;
                case 3: // Game saved
                    targetText = gameSaveText;
                    fadeDeadzone = 0.7f;
                    fadeTime = popupTimerSave;
                    if (popupTimerSave >= 0)
                        popupTimerSave -= Time.deltaTime;
                    break;
                case 4: // Best time
                    targetText = newBestTimeText;
                    useAreaTextColor = true;
                    fadeTime = popupTimerBestTime;
                    if (popupTimerBestTime >= 0)
                        popupTimerBestTime -= Time.deltaTime;
                    break;
                case 5: // Unlocks
                    targetText = newUnlocksText;
                    useAreaTextColor = true;
                    fadeTime = popupTimerUnlock;
                    if (popupTimerUnlock >= 0)
                        popupTimerUnlock -= Time.deltaTime;
                    break;
            }
            float alpha = useAreaTextColor ? areaText.GetColor().a : Mathf.InverseLerp(0, fadeDeadzone, fadeTime);
            Color thisColor = useColor ? textPopupColor : Color.white;
            thisColor.a = alpha;
            targetText.SetColor(thisColor);
            if (useAreaTextColor && fadeTime < 0)
                targetText.SetText("");
        }

        // Audiosource volume control
        PlayState.globalSFX.volume = PlayState.generalData.soundVolume * 0.1f * PlayState.sfxFader;
        PlayState.globalMusic.volume = PlayState.generalData.musicVolume * 0.1f * PlayState.fader;

        // Palette shader toggle
        if ((PlayState.generalData.paletteFilterState && !paletteShader.enabled) || (!PlayState.generalData.paletteFilterState && paletteShader.enabled))
            paletteShader.enabled = !paletteShader.enabled;

        // Music
        foreach (AudioSource audio in PlayState.musicSourceArray)
            audio.volume = musicMuted ? 0 : Mathf.Lerp(audio.volume, (PlayState.generalData.musicVolume * 0.1f) * PlayState.fader, 5 * Time.deltaTime);

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
            PlayState.fg2Layer.transform.localPosition = PlayState.fg2Offset + new Vector2(
                Mathf.Round((PlayState.cam.transform.position.x - PlayState.camCenter.x) * PlayState.parallaxFg2Mod.x * 16) * 0.0625f,
                Mathf.Round((PlayState.cam.transform.position.y - PlayState.camCenter.y) * PlayState.parallaxFg2Mod.y * 16) * 0.0625f
                );
            PlayState.fg1Layer.transform.localPosition = PlayState.fg1Offset + new Vector2(
                Mathf.Round((PlayState.cam.transform.position.x - PlayState.camCenter.x) * PlayState.parallaxFg1Mod.x * 16) * 0.0625f,
                Mathf.Round((PlayState.cam.transform.position.y - PlayState.camCenter.y) * PlayState.parallaxFg1Mod.y * 16) * 0.0625f
                );
            PlayState.bgLayer.transform.localPosition = PlayState.bgOffset + new Vector2(
                Mathf.Round((PlayState.cam.transform.position.x - PlayState.camCenter.x) * PlayState.parallaxBgMod.x * 16) * 0.0625f,
                Mathf.Round((PlayState.cam.transform.position.y - PlayState.camCenter.y) * PlayState.parallaxBgMod.y * 16) * 0.0625f
                );
            PlayState.skyLayer.transform.localPosition = PlayState.skyOffset + new Vector2(
                Mathf.Round((PlayState.cam.transform.position.x - PlayState.camCenter.x) * PlayState.parallaxSkyMod.x * 16) * 0.0625f,
                Mathf.Round((PlayState.cam.transform.position.y - PlayState.camCenter.y) * PlayState.parallaxSkyMod.y * 16) * 0.0625f
                );
        }

        // Update bottom keys
        if (PlayState.gameState == PlayState.GameState.game || PlayState.gameState == PlayState.GameState.map)
        {
            PlayState.TogglableHUDElements[11].SetActive(PlayState.generalData.bottomKeyState == 2 && !PlayState.isInBossRush);
            PlayState.TogglableHUDElements[3].SetActive(PlayState.generalData.bottomKeyState >= 1 && !PlayState.isInBossRush);
            if (Control.lastInputIsCon && !bottomKeysAreCon)
            {
                PlayState.hudPause.SetText(Control.ParseButtonName(Control.Controller.Pause, true));
                PlayState.hudMap.SetText(Control.ParseButtonName(Control.Controller.Map, true));
                bottomKeysAreCon = true;
            }
            else if (!Control.lastInputIsCon && bottomKeysAreCon)
            {
                PlayState.hudPause.SetText(Control.ParseKeyName(Control.Keyboard.Pause, true));
                PlayState.hudMap.SetText(Control.ParseKeyName(Control.Keyboard.Map, true));
                bottomKeysAreCon = false;
            }
        }

        // FPS calculator
        if (PlayState.gameState == PlayState.GameState.game)
            PlayState.TogglableHUDElements[8].SetActive(PlayState.generalData.FPSState);
        frameCount++;
        dt += Time.deltaTime;
        if (dt > 1 / updateRate)
        {
            fps = frameCount / dt;
            frameCount = 0;
            dt -= 1 / updateRate;
        }
        PlayState.hudFps.SetText(Mathf.Round(fps).ToString() +
            (PlayState.generalData.frameLimiter != 0 ? "/" + Application.targetFrameRate : "") + PlayState.GetText("hud_fps"));

        // Game time counter
        if (PlayState.gameState == PlayState.GameState.game)
        {
            if (!PlayState.isInBossRush || (PlayState.isInBossRush && PlayState.incrementRushTimer))
                PlayState.currentProfile.gameTime[2] += Time.deltaTime;
            PlayState.TogglableHUDElements[9].SetActive(PlayState.generalData.timeState && !PlayState.isInBossRush);
            PlayState.TogglableHUDElements[18].SetActive(PlayState.isInBossRush);
        }
        if (PlayState.currentProfile.gameTime[2] >= 60)
        {
            PlayState.currentProfile.gameTime[2] -= 60;
            PlayState.currentProfile.gameTime[1] += 1;
        }
        if (PlayState.currentProfile.gameTime[1] >= 60)
        {
            PlayState.currentProfile.gameTime[1] -= 60;
            PlayState.currentProfile.gameTime[0] += 1;
        }
        if (PlayState.isInBossRush && PlayState.incrementRushTimer)
            PlayState.hudRushTime.SetText(PlayState.GetTimeString(true, true));
        else if (!PlayState.isInBossRush)
            PlayState.hudTime.SetText(PlayState.GetTimeString());
        if (PlayState.isInBossRush)
            PlayState.hudRushTime.SetColor(PlayState.incrementRushTimer ? PlayState.GetColor("0312") : PlayState.GetColor("0112"));
    }

    public void UpdateMusic(int area, int subzone, int resetFlag = 0)
    {
        // resetFlag = 0  -  nothing
        // resetFlag = 1  -  change song
        // resetFlag = 2  -  rebuild array and change song
        // resetFlag = 3  -  rebuild array
        // resetFlag = 4  -  stop all music
        if (resetFlag >= 2) // Hard reset array
        {
            PlayState.musicSourceArray.Clear();
            foreach (Transform obj in PlayState.musicParent.transform)
                Destroy(obj.gameObject);

            for (int i = 0; i < PlayState.musicLibrary.library.Length - 1; i++)
            {
                GameObject newSourceParent = new();
                newSourceParent.transform.parent = PlayState.musicParent;
                newSourceParent.name = (i < PlayState.musicLibrary.areaThemeOffset - 1) ? "Auxillary group " + i
                    : "Area " + (i - PlayState.musicLibrary.areaThemeOffset + 1) + " music group";
                for (int j = 0; j < PlayState.musicLibrary.library[i + 1].Length; j++)
                {
                    for (int k = 0; k < 2; k++)
                    {
                        GameObject newSource = new();
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
                            if (int.Parse(sourceName[3]) == 1 && resetFlag != 4)
                                sourceComponent.Play();
                            else
                                sourceComponent.Stop();
                        }
                    }
                }
                nextLoopEvent = AudioSettings.dspTime + PlayState.musicLibrary.library[offsetID + 1][0].length;
            }
            if (resetFlag != 4)
            {
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
            else
                PlayState.playingMusic = false;
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

    public void DelayStartAreaTheme(int area, int subzone, float delayTime)
    {
        StartCoroutine(DelayStartThemeCoroutine(area, subzone, delayTime));
    }
    
    private IEnumerator DelayStartThemeCoroutine(int area, int subzone, float delayTime)
    {
        int profileStartedOn = PlayState.currentProfileNumber;
        while (delayTime > 0 && !PlayState.resetInducingFadeActive)
        {
            if (PlayState.gameState == PlayState.GameState.game)
                delayTime -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        while (PlayState.gameState != PlayState.GameState.game && PlayState.gameState != PlayState.GameState.menu)
            yield return new WaitForEndOfFrame();
        if (!PlayState.resetInducingFadeActive && PlayState.gameState == PlayState.GameState.game && PlayState.currentProfileNumber == profileStartedOn)
            UpdateMusic(area, subzone, 1);
    }

    public void RunDustRing(int tfType = -1)
    {
        StartCoroutine(DustRing(tfType));
    }

    private IEnumerator DustRing(int tfType)
    {
        List<Particle> dustRing = new();
        float spinSpeed = Mathf.PI * 2;
        int particleCount = 16;
        int repeatCount = 0;
        float radius = 14.625f;
        float inwardSpeed = 0.09375f;
        float spinMod = 0f;
        float radiusMod = radius;

        if (shellAnimTimer == 0)
        {
            if (!(PlayState.generalData.particleState == 3 || PlayState.generalData.particleState == 5))
                shellStateBuffer = PlayState.GetShellLevel();
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 thisDustPos = new(
                    PlayState.player.transform.position.x + (Mathf.Sin((i / particleCount) * PlayState.TAU) * radius),
                    PlayState.player.transform.position.y + (Mathf.Cos((i / particleCount) * PlayState.TAU) * radius)
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
                    PlayState.player.transform.position.x + Mathf.Cos(thisCurve) * radiusMod,
                    PlayState.player.transform.position.y - Mathf.Sin(thisCurve) * radiusMod
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
        if (tfType != -1 && (PlayState.generalData.particleState == 3 || PlayState.generalData.particleState == 5))
        {
            shellStateBuffer = tfType;
            PlayState.RequestParticle(PlayState.player.transform.position, "transformation", new float[]
            {
                tfType switch
                {
                    2 => PlayState.currentProfile.character switch
                    {
                        "Upside" => 4,
                        "Leggy" => 5,
                        "Blobby" => 6,
                        _ => 2
                    },
                    3 => 3,
                    _ => 1
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
        if ((weaponID > PlayState.playerScript.selectedWeapon && activateThisWeapon) || !activateThisWeapon)
            PlayState.playerScript.selectedWeapon = weaponID;
        UpdateWeaponIcons();
    }

    public void UpdateWeaponIcons()
    {
        for (int i = 0; i < weaponIcons.Length; i++)
        {
            string animName = "WeaponIcon_" + i + "_";
            bool hasWeapon = i switch
            {
                0 => PlayState.isRandomGame && PlayState.currentRando.broomStart,
                1 => PlayState.CheckForItem(0),
                2 => PlayState.CheckForItem(1) || PlayState.CheckForItem(11),
                3 => PlayState.CheckForItem(2) || PlayState.CheckForItem(12),
                _ => PlayState.CheckForItem(0)
            } && !PlayState.trapManager.lockedWeapons.Contains(i);
            if (i == 0)
                weaponIcons[i].GetSpriteRenderer().enabled = PlayState.isRandomGame && PlayState.currentRando.broomStart;
            if (PlayState.playerScript.selectedWeapon == i && hasWeapon)
                animName += "active";
            else if (hasWeapon)
                animName += "inactive";
            else
                animName += "locked";
            if (weaponIcons[i].lastAnimName != animName)
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
            anim.affectedByGlobalEntityColor = false;
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
                sprite.GetSpriteRenderer().enabled = PlayState.generalData.keymapState;

            yield return new WaitForEndOfFrame();
        }
    }

    public void ExecuteCoverCommand(string type, byte r = 0, byte g = 0, byte b = 0, byte a = 0, float maxTime = 0, float delay = 0, int sortingOrder = 1001)
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
                StartCoroutine(CoverCustomFade(r, g, b, a, maxTime, delay, sortingOrder));
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

    public IEnumerator CoverCustomFade(byte r, byte g, byte b, byte a, float maxTime, float delay, int sortingOrder)
    {
        SpriteRenderer sprite = PlayState.screenCover;
        sprite.sortingOrder = sortingOrder;
        float timer = -Mathf.Abs(delay);
        while (timer < 0)
        {
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
        Color32 startColor = sprite.color;
        while (timer < maxTime)
        {
            yield return new WaitForFixedUpdate();
            if (timer >= 0)
                sprite.color = new Color32((byte)Mathf.Lerp(startColor.r, r, timer / maxTime),
                    (byte)Mathf.Lerp(startColor.g, g, timer / maxTime),
                    (byte)Mathf.Lerp(startColor.b, b, timer / maxTime),
                    (byte)Mathf.Lerp(startColor.a, a, timer / maxTime));
            timer += Time.fixedDeltaTime;
        }
    }

    public void CalculateMaxHealth()
    {
        PlayState.playerScript.maxHealth = hpPerHeart[PlayState.currentProfile.difficulty] * (PlayState.CountHearts() + 3);
    }

    public void RenderNewHearts()
    {
        if (hearts.transform.childCount != 0)
        {
            for (int i = hearts.transform.childCount - 1; i > -1; i--)
                Destroy(hearts.transform.GetChild(i).gameObject);
        }
        int max = PlayState.CountHearts() + 3;
        for (int i = 0; i < max; i++)
        {
            GameObject NewHeart = new();
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
            heartAnim.Play(PlayState.currentProfile.difficulty == 2 ? "Heart_insane_2" :
                (PlayState.currentProfile.difficulty == 1 ? "Heart_normal_4" : "Heart_easy_8"));
            heartAnim.affectedByGlobalEntityColor = false;
            NewHeart.GetComponent<SpriteRenderer>().sortingOrder = -1;
            NewHeart.name = "Heart " + (i + 1) + " (HP " + (i * 4) + "-" + (i * 4 + 4) + ")";
        }
    }

    public void UpdateHearts()
    {
        if (hearts.transform.childCount != 0)
        {
            int totalOfPreviousHearts = 0;
            for (int i = 0; i < PlayState.CountHearts() + 3; i++)
            {
                hearts.transform.GetChild(i).GetComponent<AnimationModule>().Play((PlayState.playerScript.health - totalOfPreviousHearts) switch
                {
                    1 => PlayState.currentProfile.difficulty == 2 ? "Heart_insane_1" : (PlayState.currentProfile.difficulty == 1 ? "Heart_normal_1" : "Heart_easy_1"),
                    2 => PlayState.currentProfile.difficulty == 2 ? "Heart_insane_2" : (PlayState.currentProfile.difficulty == 1 ? "Heart_normal_2" : "Heart_easy_2"),
                    3 => PlayState.currentProfile.difficulty == 2 ? "Heart_insane_2" : (PlayState.currentProfile.difficulty == 1 ? "Heart_normal_3" : "Heart_easy_3"),
                    4 => PlayState.currentProfile.difficulty == 2 ? "Heart_insane_2" : (PlayState.currentProfile.difficulty == 1 ? "Heart_normal_4" : "Heart_easy_4"),
                    5 => PlayState.currentProfile.difficulty == 2 ? "Heart_insane_2" : (PlayState.currentProfile.difficulty == 1 ? "Heart_normal_4" : "Heart_easy_5"),
                    6 => PlayState.currentProfile.difficulty == 2 ? "Heart_insane_2" : (PlayState.currentProfile.difficulty == 1 ? "Heart_normal_4" : "Heart_easy_6"),
                    7 => PlayState.currentProfile.difficulty == 2 ? "Heart_insane_2" : (PlayState.currentProfile.difficulty == 1 ? "Heart_normal_4" : "Heart_easy_7"),
                    8 => PlayState.currentProfile.difficulty == 2 ? "Heart_insane_2" : (PlayState.currentProfile.difficulty == 1 ? "Heart_normal_4" : "Heart_easy_8"),
                    _ => ((PlayState.playerScript.health - totalOfPreviousHearts) > 0) ?
                    (PlayState.currentProfile.difficulty == 2 ? "Heart_insane_2" : (PlayState.currentProfile.difficulty == 1 ? "Heart_normal_4" : "Heart_easy_8")) :
                    (PlayState.currentProfile.difficulty == 2 ? "Heart_insane_0" : (PlayState.currentProfile.difficulty == 1 ? "Heart_normal_0" : "Heart_easy_0"))
                });
                totalOfPreviousHearts += hpPerHeart[PlayState.currentProfile.difficulty];
            }
        }
    }

    public void RefillPlayerHealth(float fillTime, float delay, int fillRate, bool timeMode, bool playSound)
    {
        StartCoroutine(HealthRefillCoroutine(fillTime, delay, fillRate, timeMode, playSound));
    }

    public IEnumerator HealthRefillCoroutine(float fillTime, float delay, int fillRate, bool timeMode, bool playSound)
    {
        yield return new WaitForSeconds(delay);

        int healthRemaining = PlayState.playerScript.maxHealth - PlayState.playerScript.health;
        float secondsPerPoint = timeMode ? fillTime : (fillTime / (float)healthRemaining);

        while (healthRemaining > 0 && !PlayState.resetInducingFadeActive && PlayState.gameState == PlayState.GameState.game)
        {
            healthRemaining -= fillRate;
            PlayState.playerScript.health = Mathf.Clamp(PlayState.playerScript.health + fillRate, 0, PlayState.playerScript.maxHealth);
            if (playSound)
                PlayState.PlaySound("EatPowerGrass");
            UpdateHearts();
            yield return new WaitForSeconds(secondsPerPoint);
        }
    }

    public void RemoveGigaBackgroundLayers()
    {
        for (int i = PlayState.gigaBGLayers.Count - 1; i >= 0; i--)
            Destroy(PlayState.gigaBGLayers[i]);
    }

    public LightMask CreateLightMask(int lightLevel, Vector2 position)
    {
        LightMask newMask = Instantiate(lightMask, position, Quaternion.identity).GetComponent<LightMask>();
        newMask.Instance(lightLevel, position);
        return newMask.GetComponent<LightMask>();
    }
    public LightMask CreateLightMask(int lightLevel, Transform parent)
    {
        LightMask newMask = Instantiate(lightMask, parent.position, Quaternion.identity).GetComponent<LightMask>();
        newMask.Instance(lightLevel, parent);
        return newMask.GetComponent<LightMask>();
    }

    public void FlashHUDText(TextTypes textType, string textValue = "No text")
    {
        //StartCoroutine(FlashText(textType, textValue));
        switch (textType)
        {
            case TextTypes.item:
                popupTimerItem = 3.5f;
                itemText.SetText(textValue);
                break;
            case TextTypes.collection:
                popupTimerCollection = 2.5f;
                itemPercentageText.SetText(string.Format(PlayState.GetText("hud_collectedItemPercentage"), PlayState.GetItemPercentage().ToString()));
                break;
            case TextTypes.areaCompletion:
                popupTimerCompletion = 3.5f;
                itemCompletionText.SetText(PlayState.GetText("hud_areaComplete"));
                break;
            case TextTypes.totalCompletion:
                popupTimerCompletion = 8.5f;
                string thisText = PlayState.GetText("hud_collectedAllItems");
                if (!PlayState.generalData.achievements[7] && !Application.version.ToLower().Contains("demo"))
                    thisText += "\n" + PlayState.GetText("hud_unlockRandoMode");
                itemCompletionText.SetText(thisText);
                break;
            case TextTypes.save:
                popupTimerSave = 2.5f;
                gameSaveText.SetText(PlayState.GetText("hud_gameSaved"));
                break;
            case TextTypes.bestTime:
                popupTimerBestTime = 4f;
                string character = PlayState.GetText(PlayState.currentProfile.character.ToLower());
                string diff = PlayState.GetText("difficulty_" + PlayState.currentProfile.difficulty switch { 1 => "normal", 2 => "insane", _ => "easy" });
                string time = PlayState.GetTimeString();
                newBestTimeText.SetText(string.Format(PlayState.GetText("hud_newBestTime"), character, diff, time));
                break;
            case TextTypes.unlock:
                popupTimerUnlock = 4f;
                if (Application.version.ToLower().Contains("demo"))
                    break;
                newUnlocksText.SetText(PlayState.GetText("hud_unlock" + textValue));
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
        if (PlayState.generalData.screenShake >= 1)
            StartCoroutine(ScreenShakeCoroutine(intensities, times, PlayState.generalData.screenShake == 1 ||
                PlayState.generalData.screenShake == 3, angle, angleVariation));
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
                if (PlayState.gameState == PlayState.GameState.game && !PlayState.resetInducingFadeActive)
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

                        if (PlayState.generalData.screenShake > 2)
                            PlayState.camShakeOffset += intensityVector;
                        else
                            PlayState.camObj.transform.localPosition += (Vector3)intensityVector;
                    }
                }
                else if (PlayState.gameState == PlayState.GameState.menu || PlayState.resetInducingFadeActive)
                    index = times.Count;
                yield return new WaitForEndOfFrame();
                PlayState.camShakeOffset = Vector2.zero;
                PlayState.camObj.transform.localPosition = new Vector3(0, 0, -10);
            }
        }
        else
            Debug.Log("Unable to parse screen shake command. Expected time count - intensity count difference of 0 or 1, but got " + (times.Count - intensities.Count));
    }

    public void RunLegacyGravCutscene(Vector2 itemOrigin)
    {
        switch (PlayState.currentProfile.character)
        {
            case "Upside":
                StartCoroutine(LegacyGravCutsceneUpside(itemOrigin));
                break;
            case "Leggy":
                StartCoroutine(LegacyGravCutsceneLeggy(itemOrigin));
                break;
            case "Blobby":
                StartCoroutine(LegacyGravCutsceneBlobby(itemOrigin));
                break;
            default:
                StartCoroutine(LegacyGravCutscene(itemOrigin));
                break;
        }
    }

    private IEnumerator LegacyGravCutscene(Vector2 itemOrigin)
    {
        int step = 0;
        float stepElapsed = 0;
        float totalElapsed = 0;
        bool sceneActive = true;
        while (sceneActive)
        {
            stepElapsed += Time.deltaTime;
            totalElapsed += Time.deltaTime;
            PlayState.paralyzed = true;
            switch (step)
            {
                case 0: // Initial delay
                    if (stepElapsed > 3.5f)
                    {
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 1: // Move to the right
                    Control.SetVirtual(Control.Keyboard.Right1, true);
                    Control.SetVirtual(Control.Keyboard.Up1, true);
                    if (PlayState.player.transform.position.x - itemOrigin.x > 9.5f)
                    {
                        ChangeActiveWeapon(3);
                        Control.SetVirtual(Control.Keyboard.Right1, false);
                        Control.SetVirtual(Control.Keyboard.Jump1, true);
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 2: // Shoot and jump
                    PlayState.playerScript.Shoot();
                    if (PlayState.playerScript.velocity.y <= 0)
                    {
                        Control.SetVirtual(Control.Keyboard.Jump1, false);
                        Control.SetVirtual(Control.Keyboard.Up1, false);
                        PlayState.playerScript.RemoteSetGravity(Player.Dirs.Ceiling);
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 3: // Grav up
                    if (PlayState.player.transform.position.y - itemOrigin.y > 16.5f)
                    {
                        PlayState.playerScript.RemoteSetGravity(Player.Dirs.WallL);
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 4: // Grav left
                    if (PlayState.player.transform.position.x - itemOrigin.x < -3.5f)
                    {
                        PlayState.playerScript.RemoteSetGravity(Player.Dirs.WallR);
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 5: // Grav right
                    if (PlayState.player.transform.position.x - itemOrigin.x > 3f)
                    {
                        PlayState.playerScript.RemoteSetGravity(Player.Dirs.Floor);
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 6: // Grav down
                    if (PlayState.playerScript.grounded)
                        sceneActive = false;
                    break;
            }
            if (totalElapsed > 10f)
                sceneActive = false;
            yield return new WaitForEndOfFrame();
        }
        Control.ClearVirtual(true, true);
        PlayState.FadeMusicBackIn();
        PlayState.paralyzed = false;
        PlayState.suppressPause = false;
    }

    private IEnumerator LegacyGravCutsceneUpside(Vector2 itemOrigin)
    {
        int step = 0;
        float stepElapsed = 0;
        float totalElapsed = 0;
        bool sceneActive = true;
        while (sceneActive)
        {
            stepElapsed += Time.deltaTime;
            totalElapsed += Time.deltaTime;
            PlayState.paralyzed = true;
            switch (step)
            {
                case 0: // Initial delay
                    if (stepElapsed > 3.5f)
                    {
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 1: // If not on ceiling, jump
                    if (PlayState.playerScript.gravityDir != Player.Dirs.Ceiling)
                        Control.SetVirtual(Control.Keyboard.Jump1, true);
                    step++;
                    stepElapsed = 0;
                    break;
                case 2: // Move right
                    Control.SetVirtual(Control.Keyboard.Right1, true);
                    Control.SetVirtual(Control.Keyboard.Down1, true);
                    if (PlayState.player.transform.position.x > itemOrigin.x + 4)
                    {
                        ChangeActiveWeapon(3);
                        Control.SetVirtual(Control.Keyboard.Down1, false);
                        Control.SetVirtual(Control.Keyboard.Up1, true);
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 3: // Move up right and shoot
                    PlayState.playerScript.Shoot();
                    if (PlayState.player.transform.position.y > itemOrigin.y + 8.5f)
                    {
                        Control.SetVirtual(Control.Keyboard.Right1, false);
                        Control.SetVirtual(Control.Keyboard.Left1, true);
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 4: // Move up left and shoot
                    PlayState.playerScript.Shoot();
                    if (PlayState.playerScript.gravityDir == Player.Dirs.Floor)
                    {
                        Control.SetVirtual(Control.Keyboard.Up1, false);
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 5: // Move left until you reach the NPC
                    if (PlayState.player.transform.position.x < itemOrigin.x + 4.5f)
                        sceneActive = false;
                    break;
            }
            if (totalElapsed > 10f)
                sceneActive = false;
            yield return new WaitForEndOfFrame();
        }
        Control.ClearVirtual(true, true);
        PlayState.FadeMusicBackIn();
        PlayState.paralyzed = false;
        PlayState.suppressPause = false;
    }

    private IEnumerator LegacyGravCutsceneLeggy(Vector2 itemOrigin)
    {
        int step = 0;
        float stepElapsed = 0;
        float totalElapsed = 0;
        bool sceneActive = true;
        while (sceneActive)
        {
            stepElapsed += Time.deltaTime;
            totalElapsed += Time.deltaTime;
            PlayState.paralyzed = true;
            switch (step)
            {
                case 0: // Initial delay + grav correction
                    if (PlayState.playerScript.gravityDir != Player.Dirs.Floor)
                    {
                        if (PlayState.playerScript.gravityDir != Player.Dirs.Ceiling)
                            PlayState.playerScript.SwitchSurfaceAxis();
                        PlayState.playerScript.gravityDir = Player.Dirs.Floor;
                        PlayState.playerScript.SwapDir(Player.Dirs.Floor);
                    }
                    if (stepElapsed > 3.5f)
                    {
                        step++;
                        stepElapsed = 0;
                        Control.SetVirtual(Control.Keyboard.Right1, true);
                        Control.SetVirtual(Control.Keyboard.Up1, true);
                        if (PlayState.playerScript.transform.position.x < itemOrigin.x - 1.125f)
                            Control.SetVirtual(Control.Keyboard.Jump1, true);
                    }
                    break;
                case 1: // Move right and flip
                    Control.SetVirtual(Control.Keyboard.Jump1, false);
                    if (PlayState.playerScript.transform.position.x > itemOrigin.x + 2.5f)
                    {
                        PlayState.playerScript.gravityDir = Player.Dirs.Ceiling;
                        PlayState.playerScript.SwapDir(Player.Dirs.Ceiling);
                    }
                    if (PlayState.playerScript.transform.position.x > itemOrigin.x + 9.5f)
                    {
                        ChangeActiveWeapon(3);
                        Control.SetVirtual(Control.Keyboard.Right1, false);
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 2: // Fall up and shoot
                    PlayState.playerScript.Shoot();
                    if (PlayState.playerScript.transform.position.y > itemOrigin.y + 17)
                    {
                        Control.SetVirtual(Control.Keyboard.Up1, false);
                        Control.SetVirtual(Control.Keyboard.Left1, true);
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 3: // Fly left
                    if ((PlayState.playerScript.transform.position.y > itemOrigin.y + 17 && PlayState.playerScript.gravityDir == Player.Dirs.Ceiling) ||
                        (PlayState.playerScript.transform.position.y < itemOrigin.y + 17 && PlayState.playerScript.gravityDir == Player.Dirs.Floor))
                        Control.SetVirtual(Control.Keyboard.Jump1, true);
                    else
                        Control.SetVirtual(Control.Keyboard.Jump1, false);
                    if (PlayState.playerScript.transform.position.x < itemOrigin.x - 2)
                    {
                        Control.SetVirtual(Control.Keyboard.Left1, false);
                        Control.SetVirtual(Control.Keyboard.Right1, true);
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 4: // Fly right
                    if ((PlayState.playerScript.transform.position.y > itemOrigin.y + 17 && PlayState.playerScript.gravityDir == Player.Dirs.Ceiling) ||
                        (PlayState.playerScript.transform.position.y < itemOrigin.y + 17 && PlayState.playerScript.gravityDir == Player.Dirs.Floor))
                        Control.SetVirtual(Control.Keyboard.Jump1, true);
                    else
                        Control.SetVirtual(Control.Keyboard.Jump1, false);
                    if (PlayState.playerScript.transform.position.x > itemOrigin.x + 4)
                    {
                        Control.SetVirtual(Control.Keyboard.Right1, false);
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 5: // Land at NPC
                    if (PlayState.playerScript.transform.position.y > itemOrigin.y + 17 && PlayState.playerScript.gravityDir == Player.Dirs.Ceiling)
                        Control.SetVirtual(Control.Keyboard.Jump1, true);
                    else
                        Control.SetVirtual(Control.Keyboard.Jump1, false);
                    if (PlayState.playerScript.grounded)
                        sceneActive = false;
                    break;
            }
            if (totalElapsed > 10f)
                sceneActive = false;
            yield return new WaitForEndOfFrame();
        }
        Control.ClearVirtual(true, true);
        PlayState.FadeMusicBackIn();
        PlayState.paralyzed = false;
        PlayState.suppressPause = false;
    }

    private IEnumerator LegacyGravCutsceneBlobby(Vector2 itemOrigin)
    {
        int step = 0;
        float stepElapsed = 0;
        float totalElapsed = 0;
        bool sceneActive = true;
        while (sceneActive)
        {

            stepElapsed += Time.deltaTime;
            totalElapsed += Time.deltaTime;
            PlayState.paralyzed = true;
            switch (step)
            {

                case 0: // Initial delay + grav correction
                    if (PlayState.playerScript.gravityDir != Player.Dirs.Floor)
                    {
                        if (PlayState.playerScript.gravityDir != Player.Dirs.Ceiling)
                            PlayState.playerScript.SwitchSurfaceAxis();
                        PlayState.playerScript.gravityDir = Player.Dirs.Floor;
                        PlayState.playerScript.SwapDir(Player.Dirs.Floor);
                    }
                    if (stepElapsed > 3.5f)
                    {
                        step++;
                        stepElapsed = 0;
                        Control.SetVirtual(Control.Keyboard.Right1, true);
                        if (PlayState.playerScript.transform.position.x < itemOrigin.x - 1.125f)
                            Control.SetVirtual(Control.Keyboard.Jump1, true);
                    }
                    break;
                case 1: // Move right, jumping when needed
                    Control.SetVirtual(Control.Keyboard.Jump1, PlayState.playerScript.lastPosition.x == PlayState.playerScript.transform.position.x);
                    if (PlayState.playerScript.transform.position.x > itemOrigin.x + 9.5f)
                        Control.SetVirtual(Control.Keyboard.Right1, false);
                    if (PlayState.playerScript.velocity.x == 0 && PlayState.playerScript.grounded && !PlayState.playerScript.ungroundedViaHop)
                    {
                        ChangeActiveWeapon(3);
                        Control.SetVirtual(Control.Keyboard.Up1, true);
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 2: // Jump up to gain height
                    if (!PlayState.playerScript.holdingJump)
                        Control.SetVirtual(Control.Keyboard.Jump1, true);
                    else
                        Control.SetVirtual(Control.Keyboard.Jump1, PlayState.playerScript.velocity.y >= 0);
                    if (PlayState.playerScript.transform.position.y < itemOrigin.y + 7)
                        PlayState.playerScript.Shoot();
                    if (PlayState.playerScript.transform.position.y > itemOrigin.y + 15)
                    {
                        step++;
                        stepElapsed = 0;
                        Control.SetVirtual(Control.Keyboard.Left1, true);
                    }
                    break;
                case 3: // Move to NPC
                    if (PlayState.playerScript.transform.position.x < itemOrigin.x + 4)
                        sceneActive = false;
                    break;
            }
            if (totalElapsed > 10f)
                sceneActive = false;
            yield return new WaitForEndOfFrame();
        }
        Control.ClearVirtual(true, true);
        PlayState.FadeMusicBackIn();
        PlayState.paralyzed = false;
        PlayState.suppressPause = false;
    }

    public void OnGUI()
    {
        if (Input.anyKeyDown && PlayState.gameState == PlayState.GameState.game && !addedCheatInputThisFrame)
        {
            KeyCode thisKey = Event.current.keyCode;
            if ((int)thisKey < 330 && thisKey != KeyCode.None)
            {
                AddNewCheatInput(thisKey);
                addedCheatInputThisFrame = true;
            }
        }
    }

    public void AddNewCheatInput(KeyCode input)
    {
        if (cheatInputs.Length > 1)
            for (int i = cheatInputs.Length - 2; i >= 0; i--)
                cheatInputs[i + 1] = cheatInputs[i];
        if (cheatInputs.Length > 0)
            cheatInputs[0] = input;
    }

    public bool CheckCheatCode(KeyCode[] code)
    {
        if (code.Length > cheatInputs.Length)
            return false;
        int inputIndex = 0;
        bool matched = true;
        for (int i = code.Length - 1; i >= 0; i--)
        {
            if (code[i] == cheatInputs[inputIndex])
                inputIndex++;
            else
            {
                matched = false;
                i = -1;
            }
        }
        if (matched)
            cheatInputs = new KeyCode[cheatInputs.Length];
        return matched;
    }

    public void RunBossRushResults()
    {
        StartCoroutine(nameof(BossRushResults));
    }

    public IEnumerator BossRushResults()
    {
        int state = 0;
        float stateTime = 0;
        string charName = PlayState.currentProfile.character;
        PlayState.TimeIndeces thisTime = charName switch
        {
            "Sluggy" => PlayState.TimeIndeces.sluggyRush,
            "Upside" => PlayState.TimeIndeces.upsideRush,
            "Leggy" => PlayState.TimeIndeces.leggyRush,
            "Blobby" => PlayState.TimeIndeces.blobbyRush,
            "Leechy" => PlayState.TimeIndeces.leechyRush,
            _ => PlayState.TimeIndeces.snailyRush
        };
        bool isFirstTime = !PlayState.generalData.achievements[(int)AchievementPanel.Achievements.BossRush];
        PlayState.QueueAchievementPopup(AchievementPanel.Achievements.BossRush);
        bool isNewTime = (PlayState.CompareTimes(PlayState.currentProfile.gameTime, new float[] { 0, 0, 0 }) == 1 &&
            PlayState.CompareTimes(PlayState.currentProfile.gameTime, thisTime) == -1) || (PlayState.CompareTimes(thisTime, new float[] { 0, 0, 0 }) == 0);

        Transform endingParent = GameObject.Find("View/Boss Rush Ending Parent").transform;

        GameObject endingBg = new("Ending Background");
        SpriteRenderer bgSprite = endingBg.AddComponent<SpriteRenderer>();
        AnimationModule bgAnim = endingBg.AddComponent<AnimationModule>();
        bgSprite.color = new Color(1, 1, 1, 0);
        bgSprite.sortingOrder = -5;
        bgAnim.Add("Ending_background");
        bgAnim.Play("Ending_background");
        GameObject endingPic = new("Ending Background");
        SpriteRenderer picSprite = endingPic.AddComponent<SpriteRenderer>();
        AnimationModule picAnim = endingPic.AddComponent<AnimationModule>();
        picSprite.color = new Color(1, 1, 1, 0);
        picSprite.sortingOrder = -4;
        picAnim.Add("Ending_bossRush");
        picAnim.Play("Ending_bossRush");

        GameObject textObj = Resources.Load<GameObject>("Objects/Text Object");
        TextObject header = Instantiate(textObj).GetComponent<TextObject>();
        header.SetAlignment("center");
        header.CreateBoth();
        header.SetText("");
        TextObject timeStats = Instantiate(textObj).GetComponent<TextObject>();
        timeStats.SetSize(1);
        timeStats.CreateOutline();
        timeStats.SetText("");
        TextObject itemStats = Instantiate(textObj).GetComponent<TextObject>();
        itemStats.SetSize(1);
        itemStats.SetAlignment("right");
        itemStats.CreateOutline();
        itemStats.SetText("");

        List<Transform> itemSprites = new();
        for (int i = 0; i < PlayState.currentProfile.items.Length; i++)
        {
            if (PlayState.currentProfile.items[i] == 1)
            {
                GameObject newItemSprite = new("Item sprite " + (i + 1));
                newItemSprite.AddComponent<SpriteRenderer>();
                AnimationModule itemAnim = newItemSprite.AddComponent<AnimationModule>();

                string animName = i switch
                {
                    0 => "Item_peashooter",
                    1 or 11 => "Item_boomerang",
                    2 or 12 => "Item_rainbowWave",
                    3 => "Item_devastator",
                    4 => charName switch { "Blobby" => "Item_wallGrab", _ => "Item_highJump" },
                    5 => charName switch { "Blobby" => "Item_shelmet", _ => "Item_shellShield" },
                    6 => charName switch { "Leechy" => "Item_backfire", _ => "Item_rapidFire" },
                    7 => "Item_iceSnail",
                    8 => charName switch { "Upside" => "Item_magneticFoot", "Leggy" => "Item_corkscrewJump", "Blobby" => "Item_angelJump", _ => "Item_gravitySnail" },
                    9 => "Item_fullMetalSnail",
                    10 => "Item_gravityShock",
                    _ => "Item_heartContainer"
                };
                itemAnim.Add(animName);
                itemAnim.Play(animName);
                itemSprites.Add(newItemSprite.transform);
            }
        }

        while (state < 5)
        {
            switch (state)
            {
                default:
                case 0: // Setup
                    endingBg.transform.parent = endingParent;
                    endingBg.transform.localPosition = new Vector2(26, 2);
                    endingPic.transform.parent = endingParent;
                    endingPic.transform.localPosition = new Vector2(-26, 2);
                    header.transform.parent = endingParent;
                    header.position = new Vector2(0, 2);
                    timeStats.transform.parent = endingParent;
                    timeStats.position = new Vector2(-26, 0);
                    itemStats.transform.parent = endingParent;
                    itemStats.position = new Vector2(26, 0);
                    for (int i = 0; i < itemSprites.Count; i++)
                    {
                        itemSprites[i].transform.parent = endingParent;
                        itemSprites[i].transform.localPosition = new Vector2(-10.75f + (i * 2.075f), -15);
                    }

                    header.SetText(string.Format(PlayState.GetText("ending_rush_header" + (isFirstTime ? "_unlockChars" : (isNewTime ? "_newBest" : ""))),
                        PlayState.GetText("char_" + charName.ToLower()), PlayState.GetTimeString()));
                    header.SetColor(new Color(1, 1, 1, 0));

                    string compiledTimeData = "";
                    for (int i = 0; i < 5; i++)
                    {
                        float baseTime;
                        string bossName;
                        switch (i)
                        {
                            default:
                            case 0:
                                baseTime = PlayState.activeRushData.ssbTime;
                                bossName = PlayState.GetText("boss_shellbreaker_rush");
                                break;
                            case 1:
                                baseTime = PlayState.activeRushData.visTime;
                                bossName = PlayState.GetText("boss_stompy_rush");
                                break;
                            case 2:
                                baseTime = PlayState.activeRushData.cubeTime;
                                bossName = PlayState.GetText("boss_spaceBox_rush");
                                break;
                            case 3:
                                baseTime = PlayState.activeRushData.sunTime;
                                bossName = PlayState.GetText("boss_moonSnail_rush");
                                break;
                            case 4:
                                baseTime = PlayState.activeRushData.gigaTime;
                                bossName = PlayState.GetText("boss_gigaSnail_rush");
                                break;
                        }
                        float[] newTime = new float[] { 0, 0, baseTime };
                        while (newTime[2] >= 3600)
                        {
                            newTime[2] -= 3600;
                            newTime[0]++;
                        }
                        while (newTime[2] >= 60)
                        {
                            newTime[2] -= 60;
                            newTime[1]++;
                        }
                        compiledTimeData += string.Format("{0} - {1}\n", bossName, PlayState.GetTimeString(newTime));
                    }
                    timeStats.SetText(compiledTimeData);

                    string compiledItemData = "";
                    if (PlayState.CheckForItem("Peashooter"))
                        compiledItemData += string.Format(PlayState.GetText("ending_rush_stats_peashooter"), PlayState.activeRushData.peasFired) + "\n";
                    if (PlayState.CheckForItem("Boomerang"))
                        compiledItemData += string.Format(PlayState.GetText("ending_rush_stats_boomerang"), PlayState.activeRushData.boomsFired) + "\n";
                    if (PlayState.CheckForItem("Rainbow Wave"))
                        compiledItemData += string.Format(PlayState.GetText("ending_rush_stats_rainbowWave"), PlayState.activeRushData.wavesFired) + "\n";
                    if (PlayState.CheckForItem("Gravity Shock"))
                        compiledItemData += string.Format(PlayState.GetText("ending_rush_stats_gravityShock"), PlayState.activeRushData.shocksFired) + "\n";
                    if (PlayState.CheckForItem("Shell Shield"))
                        compiledItemData += string.Format(PlayState.GetText("ending_rush_stats_shellShield"), PlayState.activeRushData.parries) + "\n";
                    compiledItemData += string.Format(PlayState.GetText("ending_rush_stats_health"), PlayState.activeRushData.healthLost);
                    itemStats.SetText(compiledItemData);

                    stateTime = 0;
                    state = 1;
                    break;
                case 1: // Fade header
                    float newVal = Mathf.Clamp(stateTime - 1.25f, 0f, 1f);
                    header.SetColor(new Color(1, 1, 1, newVal));
                    if (stateTime > 2.5f)
                    {
                        header.SetColor(new Color(1, 1, 1, 1));
                        stateTime = 0;
                        state = 2;
                    }
                    break;
                case 2: // Fade ending pic
                    float layerX = Mathf.Clamp(30f - (stateTime * 10f), 0, Mathf.Infinity);
                    endingBg.transform.localPosition = layerX * PlayState.FRAC_16 * Vector3.right;
                    endingPic.transform.localPosition = layerX * PlayState.FRAC_16 * Vector3.left;
                    bgSprite.color = new Color(1, 1, 1, stateTime * 0.35f);
                    picSprite.color = new Color(1, 1, 1, stateTime * 0.35f);
                    if (stateTime > 3.5f)
                    {
                        PlayState.paralyzed = true;
                        stateTime = 0;
                        state = 3;
                    }
                    break;
                case 3: // Summon stats and await input
                    float lerpScale = 6;
                    header.position.y = Mathf.Lerp(header.position.y, 5f, lerpScale * Time.deltaTime);
                    timeStats.position.x = Mathf.Lerp(timeStats.position.x, -11f, lerpScale * Time.deltaTime);
                    itemStats.position.x = Mathf.Lerp(itemStats.position.x, 11f, lerpScale * Time.deltaTime);
                    for (int i = 0; i < itemSprites.Count; i++)
                    {
                        if (stateTime > i * 0.125f)
                            itemSprites[i].localPosition = new Vector2(itemSprites[i].localPosition.x, Mathf.Lerp(itemSprites[i].localPosition.y,
                                -5, lerpScale * Time.deltaTime));
                    }
                    if (stateTime > 1f)
                    {
                        if (Control.JumpHold(0, true, true) || Control.Pause(true, true))
                        {
                            PlayState.PlaySound("MenuBeep2");
                            PlayState.globalFunctions.ExecuteCoverCommand("Custom Fade", 0, 0, 0, 255, 2);
                            if (isNewTime)
                            {
                                PlayState.SetTime(thisTime, PlayState.currentProfile.gameTime);
                                PlayState.WriteSave(0, true);
                            }
                            stateTime = 0;
                            state = 4;
                        }
                    }
                    break;
                case 4: // Fade back to menu
                    PlayState.fader = Mathf.InverseLerp(2, 0, stateTime);
                    if (stateTime > 2.5f)
                    {
                        PlayState.fader = 1;
                        for (int i = endingParent.childCount - 1; i >= 0; i--)
                            Destroy(endingParent.GetChild(i).gameObject);
                        PlayState.ToggleHUD(false);
                        PlayState.mainMenu.MenuOutOfBossRush();
                        state = 5;
                    }
                    break;
            }
            stateTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
