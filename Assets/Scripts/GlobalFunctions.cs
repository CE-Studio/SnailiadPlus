using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public TextObject itemText;
    public TextObject itemPercentageText;
    public TextObject itemCompletionText;
    public TextObject gameSaveText;
    public TextObject newBestTimeText;
    public TextObject newUnlocksText;

    // Debug keys
    public AnimationModule[] keySprites = new AnimationModule[7];

    // Weapon icons
    public AnimationModule[] weaponIcons = new AnimationModule[3];
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
    int pingTimer = 0;
    int explodeTimer = 0;

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
            weaponIcons[i].Add("WeaponIcon_" + (i + 1) + "_locked");
            weaponIcons[i].Add("WeaponIcon_" + (i + 1) + "_inactive");
            weaponIcons[i].Add("WeaponIcon_" + (i + 1) + "_active");
            weaponIcons[i].Play("WeaponIcon_" + (i + 1) + "_locked");
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
        PlayState.groundLayer = GameObject.Find("Grid/Ground");
        PlayState.fg2Layer = GameObject.Find("Grid/Foreground 2");
        PlayState.fg1Layer = GameObject.Find("Grid/Foreground");
        PlayState.bgLayer = GameObject.Find("Grid/Background");
        PlayState.skyLayer = GameObject.Find("Grid/Sky");
        PlayState.specialLayer = GameObject.Find("Grid/Special");
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

        PlayState.titleRoom = PlayState.roomTriggerParent.transform.Find("Backdrop Rooms/Title").GetComponent<RoomTrigger>();
        PlayState.moonCutsceneRoom = PlayState.roomTriggerParent.transform.Find("Backdrop Rooms/Moon Snail Cutscene").GetComponent<RoomTrigger>();
        PlayState.creditsRoom = PlayState.roomTriggerParent.transform.Find("Backdrop Rooms/Credits").GetComponent<RoomTrigger>();

        PlayState.globalFunctions = this;

        PlayState.hudFps = GameObject.Find("View/FPS").GetComponent<TextObject>();
        PlayState.hudTime = GameObject.Find("View/Time").GetComponent<TextObject>();
        PlayState.hudPause = GameObject.Find("View/Bottom Keys/Pause").GetComponent<TextObject>();
        PlayState.hudMap = GameObject.Find("View/Bottom Keys/Map").GetComponent<TextObject>();
        PlayState.hudRoomName = GameObject.Find("View/Minimap Panel/Room Name").GetComponent<TextObject>();

        PlayState.palette = (Texture2D)Resources.Load("Images/Palette");

        for (int i = 0; i < 7; i++)
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
            GameObject.Find("View/Control Guide")              // 17
        };

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

            // Cheat management
            addedCheatInputThisFrame = false;

            // Skyfish cheat
            bool skyfishActive = false;
            if (PlayState.currentArea == 0)
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
                        if (PlayState.GetNPCVar(PlayState.NPCVarIDs.HasSeenIris) == 1)
                            areaName = PlayState.GetText("area_iris");
                        break;
                    case 6:
                        areaName = PlayState.GetText("area_iris");
                        break;
                    case 7:
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
            else if (currentBossName != "" && currentBossName != PlayState.GetText("boss_gigaSnail") && !flashedBossName)
            {
                areaText.SetText(currentBossName);
                radarText.SetText("");
                areaTextTimer = 0;
                flashedBossName = true;
            }
            else if (!PlayState.inBossFight && currentBossName != "")
            {
                if (currentBossName != PlayState.GetText("boss_moonSnail"))
                {
                    if (displayDefeatText)
                    {
                        string thisBossName = currentBossName;
                        if (thisBossName == PlayState.GetText("boss_gigaSnail"))
                            thisBossName = PlayState.GetText("boss_moonSnail");
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
            PlayState.TogglableHUDElements[11].SetActive(PlayState.generalData.bottomKeyState == 2);
            PlayState.TogglableHUDElements[3].SetActive(PlayState.generalData.bottomKeyState >= 1);
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
            PlayState.currentProfile.gameTime[2] += Time.deltaTime;
            PlayState.TogglableHUDElements[9].SetActive(PlayState.generalData.timeState);
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
        PlayState.hudTime.SetText(PlayState.GetTimeString());
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
        while (delayTime > 0 && !PlayState.resetInducingFadeActive && PlayState.gameState == PlayState.GameState.game)
        {
            if (PlayState.gameState == PlayState.GameState.game)
                delayTime -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        if (!PlayState.resetInducingFadeActive && PlayState.gameState == PlayState.GameState.game)
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
            {
                Destroy(hearts.transform.GetChild(i).gameObject);
            }
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

    public void FlashHUDText(TextTypes textType, string textValue = "No text")
    {
        StartCoroutine(FlashText(textType, textValue));
    }

    public enum TextTypes
    {
        item,
        collection,
        completion,
        save,
        bestTime,
        unlock
    }
    public IEnumerator FlashText(TextTypes textType, string textValue)
    {
        float timer = 0;
        int colorPointer = 0;
        int colorCooldown = 0;
        byte alpha;
        switch (textType)
        {
            default:
                yield return new WaitForEndOfFrame();
                break;
            case TextTypes.item:
                itemText.SetText(textValue);
                while (timer < 3.5f)
                {
                    alpha = timer > 2.8f ? (byte)Mathf.RoundToInt(Mathf.Lerp(255, 0, Mathf.InverseLerp(2.8f, 3.5f, timer))) : (byte)255;
                    if (colorCooldown <= 0)
                    {
                        itemText.SetColor(colorPointer switch
                        {
                            0 => new Color32(189, 191, 198, alpha),
                            1 => new Color32(247, 198, 223, alpha),
                            2 => new Color32(252, 214, 136, alpha),
                            _ => new Color32(170, 229, 214, alpha)
                        });
                        colorPointer = (colorPointer + 1) % 4;
                        colorCooldown = 2;
                    }
                    else
                        colorCooldown--;
                    yield return new WaitForEndOfFrame();
                    timer += Time.deltaTime;
                }
                itemText.SetColor(new Color(1, 1, 1, 0));
                break;
            case TextTypes.collection:
                itemPercentageText.SetText(string.Format(PlayState.GetText("hud_collectedItemPercentage"), PlayState.GetItemPercentage().ToString()));
                while (timer < 2.5f)
                {
                    alpha = timer > 1.8f ? (byte)Mathf.RoundToInt(Mathf.Lerp(255, 0, Mathf.InverseLerp(1.8f, 2.5f, timer))) : (byte)255;
                    if (colorCooldown <= 0)
                    {
                        itemPercentageText.SetColor(colorPointer switch
                        {
                            0 => new Color32(189, 191, 198, alpha),
                            1 => new Color32(247, 198, 223, alpha),
                            2 => new Color32(252, 214, 136, alpha),
                            _ => new Color32(170, 229, 214, alpha)
                        });
                        colorPointer = (colorPointer + 1) % 4;
                        colorCooldown = 2;
                    }
                    else
                        colorCooldown--;
                    yield return new WaitForEndOfFrame();
                    timer += Time.deltaTime;
                }
                itemPercentageText.SetColor(new Color(1, 1, 1, 0));
                break;
            case TextTypes.completion:
                string thisText = PlayState.GetText("hud_collectedAllItems");
                if (!PlayState.generalData.achievements[7] && !Application.version.ToLower().Contains("demo"))
                    thisText += "\n" + PlayState.GetText("hud_unlockRandoMode");
                itemCompletionText.SetText(thisText);
                while (timer < 8.5f)
                {
                    alpha = timer > 7.8f ? (byte)Mathf.RoundToInt(Mathf.Lerp(255, 0, Mathf.InverseLerp(7.8f, 8.5f, timer))) : (byte)255;
                    if (colorCooldown <= 0)
                    {
                        itemCompletionText.SetColor(colorPointer switch
                        {
                            0 => new Color32(189, 191, 198, alpha),
                            1 => new Color32(247, 198, 223, alpha),
                            2 => new Color32(252, 214, 136, alpha),
                            _ => new Color32(170, 229, 214, alpha)
                        });
                        colorPointer = (colorPointer + 1) % 4;
                        colorCooldown = 2;
                    }
                    else
                        colorCooldown--;
                    yield return new WaitForEndOfFrame();
                    timer += Time.deltaTime;
                }
                itemCompletionText.SetColor(new Color(1, 1, 1, 0));
                break;
            case TextTypes.save:
                gameSaveText.SetText(PlayState.GetText("hud_gameSaved"));
                while (timer < 2.5f)
                {
                    gameSaveText.SetColor(new Color(1, 1, 1, timer > 1.8f ? Mathf.Lerp(1, 0, Mathf.InverseLerp(1.8f, 2.5f, timer)) : 1));
                    yield return new WaitForEndOfFrame();
                    timer += Time.deltaTime;
                }
                gameSaveText.SetColor(new Color(1, 1, 1, 0));
                break;
            case TextTypes.bestTime:
                string character = PlayState.GetText(PlayState.currentProfile.character.ToLower());
                string diff = PlayState.GetText("difficulty_" + PlayState.currentProfile.difficulty switch { 1 => "normal", 2 => "insane", _ => "easy" });
                string time = PlayState.GetTimeString();
                newBestTimeText.SetText(string.Format(PlayState.GetText("hud_newBestTime"), character, diff, time));
                while (timer < 4f)
                {
                    newBestTimeText.SetColor(areaText.thisText.color);
                    yield return new WaitForEndOfFrame();
                    timer += Time.deltaTime;
                }
                newBestTimeText.SetColor(new Color(1, 1, 1, 0));
                break;
            case TextTypes.unlock:
                if (Application.version.ToLower().Contains("demo"))
                    break;
                newUnlocksText.SetText(PlayState.GetText("hud_unlock" + textValue));
                while (timer < 4f)
                {
                    newUnlocksText.SetColor(areaText.thisText.color);
                    yield return new WaitForEndOfFrame();
                    timer += Time.deltaTime;
                }
                newUnlocksText.SetColor(new Color(1, 1, 1, 0));
                break;
        }
        yield return new WaitForEndOfFrame();
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
        int preGravJumpFrames = 0;
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
                        ChangeActiveWeapon(2);
                        Control.SetVirtual(Control.Keyboard.Right1, false);
                        Control.SetVirtual(Control.Keyboard.Jump1, true);
                        PlayState.playerScript.Shoot();
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 2: // Shoot once and jump
                    preGravJumpFrames++;
                    Control.SetVirtual(Control.Keyboard.Jump1, preGravJumpFrames < 3);
                    if (PlayState.player.transform.position.y - itemOrigin.y > 3f)
                    {
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 3: // Grav up
                    Control.SetVirtual(Control.Keyboard.Jump1, stepElapsed < 0.125f);
                    if (PlayState.player.transform.position.y - itemOrigin.y > 16.5f)
                    {
                        Control.SetVirtual(Control.Keyboard.Jump1, false);
                        Control.SetVirtual(Control.Keyboard.Up1, false);
                        Control.SetVirtual(Control.Keyboard.Left1, true);
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 4: // Grav left
                    Control.SetVirtual(Control.Keyboard.Jump1, stepElapsed < 0.125f);
                    if (PlayState.player.transform.position.x - itemOrigin.x < -3.5f)
                    {
                        Control.SetVirtual(Control.Keyboard.Jump1, false);
                        Control.SetVirtual(Control.Keyboard.Left1, false);
                        Control.SetVirtual(Control.Keyboard.Right1, true);
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 5: // Grav right
                    Control.SetVirtual(Control.Keyboard.Jump1, stepElapsed < 0.125f);
                    if (PlayState.player.transform.position.x - itemOrigin.x > 3f)
                    {
                        Control.SetVirtual(Control.Keyboard.Jump1, false);
                        Control.SetVirtual(Control.Keyboard.Right1, false);
                        Control.SetVirtual(Control.Keyboard.Down1, true);
                        step++;
                        stepElapsed = 0;
                    }
                    break;
                case 6: // Grav down
                    Control.SetVirtual(Control.Keyboard.Jump1, stepElapsed < 0.125f);
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
        yield return new WaitForEndOfFrame();
        PlayState.FadeMusicBackIn();
        PlayState.paralyzed = false;
    }

    private IEnumerator LegacyGravCutsceneLeggy(Vector2 itemOrigin)
    {
        yield return new WaitForEndOfFrame();
        PlayState.FadeMusicBackIn();
        PlayState.paralyzed = false;
    }

    private IEnumerator LegacyGravCutsceneBlobby(Vector2 itemOrigin)
    {
        yield return new WaitForEndOfFrame();
        PlayState.FadeMusicBackIn();
        PlayState.paralyzed = false;
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
}
