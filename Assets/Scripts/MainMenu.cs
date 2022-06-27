using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MainMenu : MonoBehaviour
{
    public delegate void DestinationDelegate();
    [Serializable]
    public struct MenuOption
    {
        public string optionText;
        public string varType;
        public int optionID;
        public bool selectable;
        public DestinationDelegate destinationPage;
        public GameObject optionObject;
        public TextMesh[] textParts;
        public int[] menuParam;
    }

    private List<MenuOption> currentOptions = new List<MenuOption>();
    private DestinationDelegate backPage;
    private int[] menuVarFlags = new int[] { 0, 0, 0, 0, 0, 0, 0 };
    private int controlScreen = 0;
    private bool isRebinding = false;
    private bool pauseButtonDown = false;
    private bool fadingToIntro = false;

    private const float LIST_CENTER_Y = -2.5f;
    private const float LIST_OPTION_SPACING = 1.25f;
    private float currentSpawnY = LIST_CENTER_Y;
    private const float SELECT_SNAIL_VERTICAL_OFFSET = 0.625f;
    private const float LETTER_SPAWN_Y = 5;
    private const float LETTER_SPAWN_TIME = Mathf.PI / 11;

    private int selectedOption = 0;
    private float selectSnailOffset = 0;

    private bool preloading = true;

    private string tempPackNameBuffer;

    public Transform cam;
    public Vector2[] panPoints = new Vector2[] // Points in world space that the main menu camera should pan over; set only one point for a static cam
    {
        new Vector2(0.5f, 0.5f),
    };
    public float panSpeed = 0.15f; // The speed at which the camera should pan
    public float stopTime = 3; // The time the camera spends at each point
    public string easeType = "linear"; // The easing type between points; can be set to "linear" or "smooth"
    public string edgeCase = "loop"; // What the camera should do when encountering the end of the point array ; can be set to "loop", "bounce", or "warp"
    private int currentPointInIndex = 0;
    private int targetPointInIndex = 1;
    private float moveTimer = 0;
    private bool direction = true;
    private bool isMoving = true;

    public AudioSource music;
    public GameObject textObject;
    public GameObject titleLetter;
    public GameObject titlePlus;
    public GameObject[] selector;

    public List<GameObject> letters = new List<GameObject>();

    public GameObject[] menuHUDElements;

    //public int[] letterPixelWidths = new int[]
    //{
    //    28, 28, 24, 28, 24, 24, 28, 24, 6, 24, 24, 6, 32, 24, 28, 28, 28, 24, 25, 24, 28, 24, 32, 32, 28, 24, 12
    ////  A   B   C   D   E   F   G   H   I  J   K   L  M   N   O   P   Q   R   S   T   U   V   W   X   Y   Z
    //};
    private readonly string acceptedChars = "abcdefghijklmnopqrstuvwxyz +";
    public Dictionary<char, int> letterPixelWidths = new Dictionary<char, int>
    {
        { 'a', 28 }, { 'b', 28 }, { 'c', 24 }, { 'd', 28 }, { 'e', 24 }, { 'f', 24 }, { 'g', 28 }, { 'h', 24 },
        { 'i', 6 }, { 'j', 24 }, { 'k', 24 }, { 'l', 6 }, { 'm', 32 }, { 'n', 24 }, { 'o', 28 }, { 'p', 28 },
        { 'q', 28 }, { 'r', 24 }, { 's', 25 }, { 't', 24 }, { 'u', 28 }, { 'v', 24 }, { 'w', 32 }, { 'x', 28 },
        { 'y', 24 }, { 'z', 24 }, { ' ', 12 }, { '+', 24 }
    };

    [Serializable]
    public struct CollectiveData
    {
        public PlayState.GameSaveData profile1;
        public PlayState.GameSaveData profile2;
        public PlayState.GameSaveData profile3;
        public PlayState.OptionData options;
        public PlayState.PackData packs;
        public PlayState.ControlData controls;
        public PlayState.RecordData records;
    }

    void Start()
    {
        PlayState.screenCover.sortingOrder = 1001;
        PlayState.ScreenFlash("Solid Color", 0, 0, 0, 255);

        if (!Directory.Exists(Application.persistentDataPath + "/Saves"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Saves");
        if (!File.Exists(Application.persistentDataPath + "/Saves/" + PlayState.SAVE_FILE_PREFIX + "_CurrentSave.json"))
        {
            PlayState.CollectiveData newData = new PlayState.CollectiveData
            {
                version = Application.version,
                profile1 = new PlayState.GameSaveData
                {
                    profile = -1
                },
                profile2 = new PlayState.GameSaveData
                {
                    profile = -1
                },
                profile3 = new PlayState.GameSaveData
                {
                    profile = -1
                },
                controls = new PlayState.ControlData
                {
                    controls = Control.defaultInputs
                },
                options = new PlayState.OptionData
                {
                    options = PlayState.optionsDefault
                },
                packs = new PlayState.PackData
                {
                    packs = new string[] { "DEFAULT", "DEFAULT", "DEFAULT", "DEFAULT" }
                },
                records = new PlayState.RecordData
                {
                    achievements = PlayState.achievementDefault,
                    times = PlayState.timeDefault
                }
            };
            PlayState.gameData = newData;
            File.WriteAllText(Application.persistentDataPath + "/Saves/" + PlayState.SAVE_FILE_PREFIX + "_CurrentSave.json", JsonUtility.ToJson(newData));
        }
        else
            PlayState.gameData = JsonUtility.FromJson<PlayState.CollectiveData>(
                File.ReadAllText(Application.persistentDataPath + "/Saves/" + PlayState.SAVE_FILE_PREFIX + "_CurrentSave.json"));

        if (!Directory.Exists(Application.persistentDataPath + "/TexturePacks"))
            Directory.CreateDirectory(Application.persistentDataPath + "/TexturePacks");
        if (!Directory.Exists(Application.persistentDataPath + "/SoundPacks"))
            Directory.CreateDirectory(Application.persistentDataPath + "/SoundPacks");
        if (!Directory.Exists(Application.persistentDataPath + "/MusicPacks"))
            Directory.CreateDirectory(Application.persistentDataPath + "/MusicPacks");
        if (!Directory.Exists(Application.persistentDataPath + "/TextPacks"))
            Directory.CreateDirectory(Application.persistentDataPath + "/TextPacks");

        PlayState.LoadPacks();

        PlayState.loadingIcon.GetComponent<AnimationModule>().Add("Loading");
        PlayState.loadingIcon.GetComponent<AnimationModule>().Play("Loading");

        PlayState.LoadOptions();
        PlayState.LoadControls();
        Screen.SetResolution(400 * (PlayState.gameOptions[2] + 1), 240 * (PlayState.gameOptions[2] + 1), false);

        //PlayState.LoadRecords();

        PlayState.player.GetComponent<BoxCollider2D>().enabled = false;
        cam = PlayState.cam.transform;
        PlayState.SetCamFocus(PlayState.player.transform);
        music = GetComponent<AudioSource>();
        PlayState.TogglableHUDElements[12].GetComponent<SpriteRenderer>().enabled = false;
        PlayState.TogglableHUDElements[12].transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        selector = new GameObject[]
        {
            GameObject.Find("Selection Pointer"),
            GameObject.Find("Selection Pointer/Left Snaily"),
            GameObject.Find("Selection Pointer/Right Snaily")
        };
        selector[1].GetComponent<AnimationModule>().pauseOnMenu = false;
        selector[2].GetComponent<AnimationModule>().pauseOnMenu = false;
        selector[1].GetComponent<AnimationModule>().Add("Title_selector_Snaily");
        selector[2].GetComponent<AnimationModule>().Add("Title_selector_Snaily");
        selector[1].GetComponent<AnimationModule>().Play("Title_selector_Snaily");
        selector[2].GetComponent<AnimationModule>().Play("Title_selector_Snaily");

        PlayState.AssignProperCollectibleIDs();
        PlayState.BuildMapMarkerArrays();

        foreach (Transform area in PlayState.roomTriggerParent.transform)
        {
            foreach (Transform room in area)
                room.GetComponent<RoomTrigger>().MoveEntitiesToInternalList();
        }

        menuHUDElements = new GameObject[]
        {
            selector[0],
            GameObject.Find("Version Text")
        };

        string[] version = Application.version.Split(' ');
        string versionText = PlayState.GetText("menu_version_header") + "\n" + (version[0].ToLower() == "release" ? PlayState.GetText("menu_version_release") :
            (version[0].ToLower() == "demo" ? PlayState.GetText("menu_version_demo") : PlayState.GetText("menu_version_developer"))) + " " + version[1];
        menuHUDElements[1].transform.GetChild(0).GetComponent<TextMesh>().text = versionText;
        menuHUDElements[1].transform.GetChild(1).GetComponent<TextMesh>().text = versionText;

        CreateTitle();
        PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 0, 0.5f);
        PlayState.loadingIcon.SetActive(false);
        preloading = false;
    }

    void Update()
    {
        if ((PlayState.gameState == "Menu" || PlayState.gameState == "Pause") && !PlayState.isMenuOpen)
        {
            if (PlayState.gameState == "Menu" && !preloading)
            {
                music.volume = PlayState.gameOptions[1] * 0.1f;
                music.Play();
            }
            PlayState.isMenuOpen = true;
            PlayState.ToggleHUD(false);
            ToggleHUD(true);
            PageMain();
            currentPointInIndex = 0;
            moveTimer = 0;
            isMoving = true;
            int i = 0;
            while (!currentOptions[i].selectable && i < currentOptions.Count)
                i++;
            selectedOption = i;
            GetNewSnailOffset();
            selector[1].GetComponent<AnimationModule>().Play("Title_selector_" + (PlayState.currentProfile != -1 ? PlayState.currentCharacter : "Snaily"));
            selector[2].GetComponent<AnimationModule>().Play("Title_selector_" + (PlayState.currentProfile != -1 ? PlayState.currentCharacter : "Snaily"));
        }
        if (PlayState.gameState == "Menu")
        {
            if (panPoints.Length > 1)
            {
                if (isMoving)
                {
                    if (moveTimer < 1)
                    {
                        if (easeType == "linear")
                        {
                            cam.position = new Vector2(
                                Mathf.Lerp(panPoints[currentPointInIndex].x, panPoints[targetPointInIndex].x, moveTimer),
                                Mathf.Lerp(panPoints[currentPointInIndex].y, panPoints[targetPointInIndex].y, moveTimer)
                                );
                        }
                        else if (easeType == "smooth")
                        {
                            cam.position = new Vector2(
                                Mathf.SmoothStep(panPoints[currentPointInIndex].x, panPoints[targetPointInIndex].x, moveTimer),
                                Mathf.SmoothStep(panPoints[currentPointInIndex].y, panPoints[targetPointInIndex].y, moveTimer)
                                );
                        }
                        moveTimer = Mathf.Clamp(moveTimer + (panSpeed * Time.deltaTime), 0, 1);
                    }
                    else
                    {
                        isMoving = false;
                        moveTimer = 0;
                        currentPointInIndex = targetPointInIndex;
                    }
                }
                else
                {
                    cam.position = new Vector2(panPoints[targetPointInIndex].x, panPoints[targetPointInIndex].y);
                    if (moveTimer < stopTime)
                    {
                        moveTimer += Time.deltaTime;
                    }
                    else
                    {
                        if (!direction && currentPointInIndex == 0)
                        {
                            targetPointInIndex = 1;
                            direction = true;
                        }
                        else
                        {
                            if (currentPointInIndex == panPoints.Length - 1)
                            {
                                switch (edgeCase)
                                {
                                    case "loop":
                                        targetPointInIndex = 0;
                                        break;
                                    case "bounce":
                                        direction = false;
                                        targetPointInIndex = panPoints.Length - 2;
                                        break;
                                    case "warp":
                                        currentPointInIndex = 0;
                                        targetPointInIndex = 1;
                                        break;
                                }
                            }
                            else
                                targetPointInIndex += direction ? 1 : -1;
                        }
                        moveTimer = 0;
                        isMoving = true;
                    }
                }
            }
            else
                cam.position = panPoints[0];
        }
        if (PlayState.gameState == "Menu" || PlayState.gameState == "Pause")
        {
            music.volume = PlayState.gameOptions[1] * 0.1f;
            Application.targetFrameRate = PlayState.gameOptions[14] == 3 ? 120 : (PlayState.gameOptions[14] == 2 ? 60 : (PlayState.gameOptions[14] == 1 ? 30 : -1));

            if (!isRebinding && !fadingToIntro && !PlayState.paralyzed)
            {
                if (Control.UpPress(1) || Control.DownPress(1))
                {
                    bool nextDown = Control.AxisY(1) == -1;
                    int intendedSelection = selectedOption + (nextDown ? 1 : -1);
                    if (intendedSelection >= currentOptions.Count)
                        intendedSelection = 0;
                    else if (intendedSelection < 0)
                        intendedSelection = currentOptions.Count - 1;
                    while (!currentOptions[intendedSelection].selectable)
                    {
                        intendedSelection += nextDown ? 1 : -1;
                        if (intendedSelection >= currentOptions.Count)
                            intendedSelection = 0;
                        else if (intendedSelection < 0)
                            intendedSelection = currentOptions.Count - 1;
                    }
                    if (intendedSelection != selectedOption)
                        PlayState.PlaySound("MenuBeep1");
                    selectedOption = intendedSelection;
                    GetNewSnailOffset();
                }

                if (Control.Pause())
                {
                    if (backPage != null)
                    {
                        backPage();
                        PlayState.PlaySound("MenuBeep2");
                    }
                }
                else if (Control.JumpPress(1))
                {
                    if (currentOptions[selectedOption].menuParam != null)
                    {
                        for (int i = 0; i < currentOptions[selectedOption].menuParam.Length; i += 2)
                            menuVarFlags[currentOptions[selectedOption].menuParam[i]] = currentOptions[selectedOption].menuParam[i + 1];
                    }
                    if (currentOptions[selectedOption].destinationPage != null)
                    {
                        currentOptions[selectedOption].destinationPage();
                        PlayState.PlaySound("MenuBeep2");
                    }
                }
            }

            foreach (MenuOption option in currentOptions)
            {
                switch (option.varType)
                {
                    default:
                        if (option.varType != "none")
                            Debug.LogWarning("Menu option variable type \"" + option.varType + "\" is not recognized");
                        break;
                    case "difficulty":
                        TestForArrowAdjust(option, 0, PlayState.achievementStates[14] == 1 ? 2 : 1);
                        switch (menuVarFlags[0])
                        {
                            case 0:
                                AddToOptionText(option, PlayState.GetText("difficulty_easy"));
                                break;
                            case 1:
                                AddToOptionText(option, PlayState.GetText("difficulty_normal"));
                                break;
                            case 2:
                                AddToOptionText(option, PlayState.GetText("difficulty_insane"));
                                break;
                        }
                        break;
                    case "character":
                        TestForArrowAdjust(option, 1, 5);
                        switch (menuVarFlags[1])
                        {
                            case 0:
                                AddToOptionText(option, PlayState.GetText("char_snaily"));
                                break;
                            case 1:
                                AddToOptionText(option, PlayState.GetText("char_sluggy"));
                                break;
                            case 2:
                                AddToOptionText(option, PlayState.GetText("char_upside"));
                                break;
                            case 3:
                                AddToOptionText(option, PlayState.GetText("char_leggy"));
                                break;
                            case 4:
                                AddToOptionText(option, PlayState.GetText("char_blobby"));
                                break;
                            case 5:
                                AddToOptionText(option, PlayState.GetText("char_leechy"));
                                break;
                        }
                        break;
                    case "isRandomized":
                        TestForArrowAdjust(option, 2, 1);
                        switch (menuVarFlags[2])
                        {
                            case 0:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_no"));
                                break;
                            case 1:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_yes"));
                                break;
                        }
                        break;
                    case "shooting":
                        TestForArrowAdjust(option, 0, 1);
                        switch (menuVarFlags[0])
                        {
                            case 0:
                                AddToOptionText(option, PlayState.GetText("menu_add_shooting_normal"));
                                break;
                            case 1:
                                AddToOptionText(option, PlayState.GetText("menu_add_shooting_toggle"));
                                break;
                        }
                        PlayState.gameOptions[8] = menuVarFlags[0];
                        break;
                    case "showBreakables":
                        TestForArrowAdjust(option, 1, 2);
                        switch (menuVarFlags[1])
                        {
                            case 0:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_off"));
                                break;
                            case 1:
                                AddToOptionText(option, PlayState.GetText("menu_add_breakables1"));
                                break;
                            case 2:
                                AddToOptionText(option, PlayState.GetText("menu_add_breakables2"));
                                break;
                        }
                        PlayState.gameOptions[12] = menuVarFlags[1];
                        break;
                    case "secretTiles":
                        TestForArrowAdjust(option, 2, 1);
                        switch (menuVarFlags[2])
                        {
                            case 0:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_hide"));
                                break;
                            case 1:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_show"));
                                break;
                        }
                        PlayState.gameOptions[13] = menuVarFlags[2];
                        break;
                    case "frameLimit":
                        TestForArrowAdjust(option, 3, 3);
                        switch (menuVarFlags[3])
                        {
                            case 0:
                                AddToOptionText(option, PlayState.GetText("menu_add_frameLimit_none"));
                                break;
                            case 1:
                                AddToOptionText(option, PlayState.GetText("menu_add_frameLimit_30"));
                                break;
                            case 2:
                                AddToOptionText(option, PlayState.GetText("menu_add_frameLimit_60"));
                                break;
                            case 3:
                                AddToOptionText(option, PlayState.GetText("menu_add_frameLimit_120"));
                                break;
                        }
                        PlayState.gameOptions[14] = menuVarFlags[3];
                        break;
                    case "soundVolume":
                        TestForArrowAdjust(option, 0, 10);
                        AddToOptionText(option, menuVarFlags[0].ToString());
                        PlayState.gameOptions[0] = menuVarFlags[0];
                        break;
                    case "musicVolume":
                        TestForArrowAdjust(option, 1, 10);
                        AddToOptionText(option, menuVarFlags[1].ToString());
                        PlayState.gameOptions[1] = menuVarFlags[1];
                        break;
                    case "resolution":
                        TestForArrowAdjust(option, 0, 3);
                        switch (menuVarFlags[0])
                        {
                            case 0:
                                AddToOptionText(option, PlayState.GetText("menu_add_resolution_1x"));
                                Screen.SetResolution(400, 240, false);
                                break;
                            case 1:
                                AddToOptionText(option, PlayState.GetText("menu_add_resolution_2x"));
                                Screen.SetResolution(800, 480, false);
                                break;
                            case 2:
                                AddToOptionText(option, PlayState.GetText("menu_add_resolution_3x"));
                                Screen.SetResolution(1200, 720, false);
                                break;
                            case 3:
                                AddToOptionText(option, PlayState.GetText("menu_add_resolution_4x"));
                                Screen.SetResolution(1600, 960, false);
                                break;
                        }
                        PlayState.gameOptions[2] = menuVarFlags[0];
                        break;
                    case "minimap":
                        TestForArrowAdjust(option, 1, 2);
                        switch (menuVarFlags[1])
                        {
                            case 0:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_hide"));
                                break;
                            case 1:
                                AddToOptionText(option, PlayState.GetText("menu_add_mapOnly"));
                                break;
                            case 2:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_show"));
                                break;
                        }
                        PlayState.gameOptions[3] = menuVarFlags[1];
                        break;
                    case "bottomKeys":
                        TestForArrowAdjust(option, 2, 2);
                        switch (menuVarFlags[2])
                        {
                            case 0:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_hide"));
                                break;
                            case 1:
                                AddToOptionText(option, PlayState.GetText("menu_add_bottomKeyWeaponOnly"));
                                break;
                            case 2:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_show"));
                                break;
                        }
                        PlayState.gameOptions[4] = menuVarFlags[2];
                        break;
                    case "keymap":
                        TestForArrowAdjust(option, 3, 1);
                        switch (menuVarFlags[3])
                        {
                            case 0:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_hide"));
                                break;
                            case 1:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_show"));
                                break;
                        }
                        PlayState.gameOptions[5] = menuVarFlags[3];
                        break;
                    case "gameTime":
                        TestForArrowAdjust(option, 4, 1);
                        switch (menuVarFlags[4])
                        {
                            case 0:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_hide"));
                                break;
                            case 1:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_show"));
                                break;
                        }
                        PlayState.gameOptions[6] = menuVarFlags[4];
                        break;
                    case "fps":
                        TestForArrowAdjust(option, 5, 1);
                        switch (menuVarFlags[5])
                        {
                            case 0:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_hide"));
                                break;
                            case 1:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_show"));
                                break;
                        }
                        PlayState.gameOptions[7] = menuVarFlags[5];
                        break;
                    case "particles":
                        TestForArrowAdjust(option, 6, 5);
                        switch (menuVarFlags[6])
                        {
                            case 0:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_none"));
                                break;
                            case 1:
                                AddToOptionText(option, PlayState.GetText("menu_add_particles1"));
                                break;
                            case 2:
                                AddToOptionText(option, PlayState.GetText("menu_add_particles2"));
                                break;
                            case 3:
                                AddToOptionText(option, PlayState.GetText("menu_add_particles3"));
                                break;
                            case 4:
                                AddToOptionText(option, PlayState.GetText("menu_add_particles4"));
                                break;
                            case 5:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_all"));
                                break;
                        }
                        PlayState.gameOptions[11] = menuVarFlags[6];
                        break;
                    case "control_jump":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(12) : Control.ParseKeyName(4));
                        break;
                    case "control_shoot":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(13) : Control.ParseKeyName(5));
                        break;
                    case "control_strafe":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(14) : Control.ParseKeyName(6));
                        break;
                    case "control_speak":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(15) : Control.ParseKeyName(7));
                        break;
                    case "control_up":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(10) : Control.ParseKeyName(2));
                        break;
                    case "control_left":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(8) : Control.ParseKeyName(0));
                        break;
                    case "control_right":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(9) : Control.ParseKeyName(1));
                        break;
                    case "control_down":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(11) : Control.ParseKeyName(3));
                        break;
                    case "control_weapon1":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseKeyName(16));
                        break;
                    case "control_weapon2":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseKeyName(17));
                        break;
                    case "control_weapon3":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseKeyName(18));
                        break;
                    case "control_weaponNext":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseKeyName(19));
                        break;
                    case "control_weaponPrev":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseKeyName(20));
                        break;
                    case "control_map":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseKeyName(21));
                        break;
                    case "control_menu":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseKeyName(22));
                        break;
                    case "slot":
                        TestForArrowAdjust(option, 0, 9);
                        AddToOptionText(option, (menuVarFlags[0] + 1).ToString() +
                            (File.Exists(Application.persistentDataPath + "/Saves/" + PlayState.SAVE_FILE_PREFIX + "_" + (menuVarFlags[0] + 1) + ".json") ? " (full)" : " (empty)"));
                        break;
                }
            }
            GetNewSnailOffset();
        }

        if (PlayState.gameState != "Menu" && PlayState.gameState != "Pause" && PlayState.gameState != "Map" && PlayState.gameState != "Debug")
        {
            if (PlayState.isMenuOpen)
            {
                PlayState.isMenuOpen = false;
                ClearOptions();
                music.Stop();
                PlayState.screenCover.sortingOrder = 999;
                PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 0, 0.25f);
                ToggleHUD(false);
            }
            if (!PlayState.isMenuOpen && Control.Pause() && !pauseButtonDown)
            {
                PlayState.isMenuOpen = true;
                PlayState.ToggleHUD(false);
                ToggleHUD(true);
                PlayState.gameState = "Pause";
                PlayState.screenCover.sortingOrder = 0;
                PlayState.ScreenFlash("Solid Color", 0, 0, 0, 0);
                PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 75, 0.25f);
                PageMain();
                CreateTitle();
            }
            if (pauseButtonDown && !Control.Pause())
                pauseButtonDown = false;
        }
    }

    private void LateUpdate()
    {
        if (PlayState.gameState == "Menu" || PlayState.gameState == "Pause")
        {
            selector[0].transform.localPosition = new Vector2(0,
                    Mathf.Lerp(selector[0].transform.localPosition.y,
                    currentOptions[selectedOption].optionObject.transform.localPosition.y + SELECT_SNAIL_VERTICAL_OFFSET, 15 * Time.deltaTime));
            selector[1].transform.localPosition = new Vector2(Mathf.Lerp(selector[1].transform.localPosition.x, -selectSnailOffset, 15 * Time.deltaTime), 0);
            selector[2].transform.localPosition = new Vector2(Mathf.Lerp(selector[2].transform.localPosition.x, selectSnailOffset, 15 * Time.deltaTime), 0);
        }
    }

    public string CharacterIDToName(int ID)
    {
        return ID switch
        {
            1 => "Sluggy",
            2 => "Upside",
            3 => "Leggy",
            4 => "Blobby",
            5 => "Leechy",
            _ => "Snaily",
        };
    }

    public void TestForArrowAdjust(MenuOption option, int varSlot, int max)
    {
        if (selectedOption == currentOptions.IndexOf(option))
        if (Control.LeftPress(1))
        {
            menuVarFlags[varSlot]--;
            if (menuVarFlags[varSlot] < 0)
                menuVarFlags[varSlot] = max;
            PlayState.PlaySound("MenuBeep1");
        }
        else if (Control.RightPress(1))
        {
            menuVarFlags[varSlot]++;
            if (menuVarFlags[varSlot] > max)
                menuVarFlags[varSlot] = 0;
            PlayState.PlaySound("MenuBeep1");
        }
    }

    public void TestForRebind()
    {
        StartCoroutine(RebindKey(menuVarFlags[0]));
    }

    public IEnumerator RebindKey(int controlID)
    {
        while (Control.JumpHold(1))
        {
            yield return new WaitForEndOfFrame();
        }
        float timer = 0;
        isRebinding = true;
        while (timer < 3 && isRebinding)
        {
            AddToOptionText(currentOptions[selectedOption], timer < 1 ? "." : (timer < 2 ? ".." : "..."));
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(key))
                {
                    Control.inputs[controlID] = key;
                    isRebinding = false;
                }
            }
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
        isRebinding = false;
    }

    public void AddToOptionText(MenuOption option, string text)
    {
        option.textParts[0].text = (option.optionID == selectedOption ? "< " : "") + option.optionText + text + (option.optionID == selectedOption ? " >" : "");
        option.textParts[1].text = (option.optionID == selectedOption ? "< " : "") + option.optionText + text + (option.optionID == selectedOption ? " >" : "");
    }

    public void AddOption(string text = "", bool isSelectable = true)
    {
        AddOption(text, isSelectable, null, null, "none");
    }
    public void AddOption(string text = "", bool isSelectable = true, DestinationDelegate destination = null)
    {
        AddOption(text, isSelectable, destination, null, "none");
    }
    public void AddOption(string text = "", bool isSelectable = true, string variable = "none")
    {
        AddOption(text, isSelectable, null, null, variable);
    }
    public void AddOption(string text = "", bool isSelectable = true, DestinationDelegate destination = null, string variable = "none")
    {
        AddOption(text, isSelectable, destination, null, variable);
    }
    public void AddOption(string text = "", bool isSelectable = true, DestinationDelegate destination = null, int[] paramChange = null)
    {
        AddOption(text, isSelectable, destination, paramChange, "none");
    }
    public void AddOption(string text = "", bool isSelectable = true, DestinationDelegate destination = null, int[] paramChange = null, string variable = "none")
    {
        foreach (Transform entry in transform)
        {
            if (entry.name.Contains("Text Object"))
                entry.localPosition = new Vector2(0, entry.transform.localPosition.y + (LIST_OPTION_SPACING * 0.5f));
        }

        MenuOption option = new MenuOption
        {
            optionText = text,
            optionID = currentOptions.Count,
            selectable = isSelectable,
            destinationPage = destination,
            varType = variable
        };

        GameObject newText = Instantiate(textObject);
        newText.transform.parent = transform;
        option.optionObject = newText;
        option.textParts = new TextMesh[]
        {
            option.optionObject.transform.GetChild(0).GetComponent<TextMesh>(),
            option.optionObject.transform.GetChild(1).GetComponent<TextMesh>()
        };
        option.textParts[0].text = option.optionText;
        option.textParts[1].text = option.optionText;
        newText.transform.localPosition = new Vector3(0, currentSpawnY);
        currentSpawnY -= LIST_OPTION_SPACING * 0.5f;

        if (paramChange != null)
            option.menuParam = paramChange;

        option.textParts[0].color = option.selectable ? PlayState.GetColor("0312") : PlayState.GetColor("0309");

        currentOptions.Add(option);
    }

    public void ToggleHUD(bool state)
    {
        foreach (GameObject element in menuHUDElements)
        {
            element.SetActive(state);
        }
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (!state && (transform.GetChild(i).name.Contains("Title Letter") || transform.GetChild(i).name.Contains("Title Plus")))
                Destroy(transform.GetChild(i).gameObject);
        }
    }

    public string ConvertTimeToString(float[] gameTime)
    {
        string time = gameTime[0] + ":";
        if (gameTime[1] < 10)
            time += "0";
        time += gameTime[1] + ":";
        int seconds = Mathf.RoundToInt(gameTime[2] * 100);
        bool lessThanTen = seconds < 1000;
        bool lessThanOne = seconds < 100;
        if (seconds == 0)
            time += "00.00";
        else
            time += (lessThanOne ? "00" : (lessThanTen ? "0" + seconds.ToString()[0] : seconds.ToString().Substring(0, 2))) + "." +
                seconds.ToString().Substring(lessThanOne ? 0 : (lessThanTen ? 1 : 2), 2);
        return time;
    }

    public string ConvertDifficultyToString(int difficulty)
    {
        string output = "";
        switch (difficulty)
        {
            case 0:
                output = PlayState.GetText("difficulty_easy");
                break;
            case 1:
                output = PlayState.GetText("difficulty_normal");
                break;
            case 2:
                output = PlayState.GetText("difficulty_insane");
                break;
        }
        return output;
    }

    public void ClearOptions()
    {
        foreach (MenuOption option in currentOptions)
        {
            Destroy(option.optionObject);
        }
        currentOptions.Clear();
        currentSpawnY = LIST_CENTER_Y;
    }

    public void GetNewSnailOffset()
    {
        Bounds textBounds = currentOptions[selectedOption].optionObject.transform.GetChild(0).GetComponent<MeshRenderer>().bounds;
        selectSnailOffset = textBounds.max.x - textBounds.center.x + 1.5f;
    }

    public void ForceSelect(int optionNum)
    {
        selectedOption = optionNum;
        selector[0].transform.localPosition = new Vector2(0, currentOptions[optionNum].optionObject.transform.localPosition.y + SELECT_SNAIL_VERTICAL_OFFSET);
        GetNewSnailOffset();
        selector[1].transform.localPosition = new Vector2(-selectSnailOffset, 0);
        selector[2].transform.localPosition = new Vector2(selectSnailOffset, 0);
    }

    public void GetNewLetterPixelWidths()
    {
        int[] newWidths = PlayState.GetAnim("Title_letterWidths").frames;
        Dictionary<char, int> newDict = new Dictionary<char, int>();
        for (int i = 0; i < acceptedChars.Length; i++)
            newDict.Add(acceptedChars[i], newWidths[i]);
        letterPixelWidths = newDict;
    }

    public void CreateTitle()
    {
        for (int i = letters.Count - 1; i >= 0; i--)
            Destroy(letters[i]);

        string title = PlayState.GetText("menu_title").ToLower();
        GetNewLetterPixelWidths();
        int titleLength = 0;
        for (int i = 0; i < title.Length; i++)
            titleLength += letterPixelWidths[title[i]] + (i != title.Length - 1 ? 4 : 0);
        float letterSpawnX = (-(titleLength * 0.5f) + (letterPixelWidths[title[0]] * 0.5f)) * 0.0625f;
        float currentDelay = 0;

        for (int i = 0; i < title.Length; i++)
        {
            if (title[i] != ' ')
            {
                GameObject newLetter = Instantiate(titleLetter);
                newLetter.GetComponent<TitleLetter>().Create(title[i], new Vector2(letterSpawnX + (title[i] == '+' ? -0.25f : 0),
                    LETTER_SPAWN_Y + (title[i] == '+' ? 0.0625f : 0)), currentDelay + (title[i] == '+' ? 2 : 0));
                currentDelay += LETTER_SPAWN_TIME;
                letters.Add(newLetter);
            }
            letterSpawnX += (letterPixelWidths[title[i]] + 4) * 0.0625f;
        }
    }

    public IEnumerator LoadFade(Vector2 spawnPos, bool runIntro = false)
    {
        if (PlayState.currentArea != -1)
        {
            Transform lastRoom = PlayState.roomTriggerParent.transform.GetChild((int)PlayState.positionOfLastRoom.x).GetChild((int)PlayState.positionOfLastRoom.y);
            lastRoom.GetComponent<Collider2D>().enabled = true;
            lastRoom.GetComponent<RoomTrigger>().active = true;
            lastRoom.GetComponent<RoomTrigger>().DespawnEverything();
            PlayState.currentArea = -1;
            PlayState.currentSubzone = -1;
            PlayState.currentProfile = -1;
        }

        fadingToIntro = true;
        PlayState.screenCover.sortingOrder = 1001;
        PlayState.ScreenFlash("Solid Color", 0, 63, 125, 0);
        PlayState.ScreenFlash("Custom Fade", 0, 63, 125, 255, 0.5f);
        yield return new WaitForSeconds(0.5f);

        if (runIntro)
        {

        }

        PlayState.screenCover.sortingOrder = 999;
        PlayState.player.transform.position = spawnPos;
        PlayState.gameState = "Game";
        PlayState.player.GetComponent<BoxCollider2D>().enabled = true;
        PlayState.ToggleHUD(true);
        PlayState.minimapScript.RefreshMap();
        PlayState.playerScript.ChangeActiveWeapon(PlayState.CheckForItem(2) || PlayState.CheckForItem(12) ? 2 : (PlayState.CheckForItem(1) || PlayState.CheckForItem(11) ? 1 : 0));
        PlayState.ToggleBossfightState(false, 0, true);
        SetTextComponentOrigins();
        fadingToIntro = false;

        PlayState.player.GetComponent<Snaily>().enabled = false;
        //PlayState.player.GetComponent<Sluggy>().enabled = false;
        //PlayState.player.GetComponent<Upside>().enabled = false;
        //PlayState.player.GetComponent<Leggy>().enabled = false;
        //PlayState.player.GetComponent<Blobby>().enabled = false;
        //PlayState.player.GetComponent<Leechy>().enabled = false;
        switch (runIntro ? CharacterIDToName(menuVarFlags[1]) : PlayState.currentCharacter)
        {
            default:
            case "Snaily":
                PlayState.player.GetComponent<Snaily>().enabled = true;
                PlayState.player.GetComponent<Snaily>().holdingJump = true;
                break;
                case "Sluggy":
                //    PlayState.player.GetComponent<Sluggy>().enabled = true;
                //    PlayState.player.GetComponent<Sluggy>().holdingJump = true;
                    PlayState.itemCollection[5] = 1;
                    break;
                case "Upside":
                //    PlayState.player.GetComponent<Upside>().enabled = true;
                //    PlayState.player.GetComponent<Upside>().holdingJump = true;
                    break;
                case "Leggy":
                //    PlayState.player.GetComponent<Leggy>().enabled = true;
                //    PlayState.player.GetComponent<Leggy>().holdingJump = true;
                    break;
                case "Blobby":
                //    PlayState.player.GetComponent<Blobby>().enabled = true;
                //    PlayState.player.GetComponent<Blobby>().holdingJump = true;
                    break;
                case "Leechy":
                //    PlayState.player.GetComponent<Leechy>().enabled = true;
                //    PlayState.player.GetComponent<Leechy>().holdingJump = true;
                    PlayState.itemCollection[5] = 1;
                    break;
        }
    }

    public void SetTextComponentOrigins()
    {
        PlayState.pauseText.GetComponent<TextAligner>().originalPos = new Vector2(-12.4375f, -7.3775f + (PlayState.gameOptions[5] == 1 ? 2 : (
            PlayState.gameOptions[6] == 1 && PlayState.gameOptions[7] == 1 ? 1 : (PlayState.gameOptions[6] != 1 && PlayState.gameOptions[7] != 1 ? 0 : 0.5f))));
        PlayState.pauseShadow.GetComponent<TextAligner>().originalPos = new Vector2(-12.375f, -7.44f + (PlayState.gameOptions[5] == 1 ? 2 : (
            PlayState.gameOptions[6] == 1 && PlayState.gameOptions[7] == 1 ? 1 : (PlayState.gameOptions[6] != 1 && PlayState.gameOptions[7] != 1 ? 0 : 0.5f))));

        PlayState.fpsText.GetComponent<TextAligner>().originalPos = new Vector2(PlayState.gameOptions[5] == 1 ? -10.4375f : -12.4375f,
            PlayState.gameOptions[6] == 1 ? -6.8775f : -7.3775f);
        PlayState.fpsShadow.GetComponent<TextAligner>().originalPos = new Vector2(PlayState.gameOptions[5] == 1 ? -10.375f : -12.375f,
            PlayState.gameOptions[6] == 1 ? -6.94f : -7.44f);

        PlayState.timeText.GetComponent<TextAligner>().originalPos = new Vector2(PlayState.gameOptions[5] == 1 ? -10.4375f : -12.4375f, -7.3775f);
        PlayState.timeShadow.GetComponent<TextAligner>().originalPos = new Vector2(PlayState.gameOptions[5] == 1 ? -10.375f : -12.375f, -7.44f);
    }

    public void PageMain()
    {
        ClearOptions();
        bool returnAvailable = false;
        if (PlayState.gameState == "Pause")
        {
            AddOption(PlayState.GetText("menu_option_main_return"), true, Unpause);
            returnAvailable = true;
        }
        AddOption(PlayState.GetText("menu_option_main_profile"), true, ProfileScreen);
        if (PlayState.achievementStates[6] == 1)
            AddOption(PlayState.GetText("menu_option_main_bossRush"), true);
        AddOption(PlayState.GetText("menu_option_main_multiplayer"), true);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_main_options"), true, OptionsScreen);
        AddOption(PlayState.GetText("menu_option_main_credits"), true, CreditsPage1);
        if (PlayState.HasTime())
            AddOption(PlayState.GetText("menu_option_records"), true);
        if (returnAvailable)
        {
            AddOption(PlayState.GetText("menu_option_main_returnTo"), true, MenuReturnConfirm);
            backPage = Unpause;
        }
        else
        {
            AddOption(PlayState.GetText("menu_option_main_quit"), true, QuitConfirm);
            backPage = QuitConfirm;
        }
        ForceSelect(0);
    }

    public void ProfileScreen()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_profile_header"), false);
        for (int i = 1; i <= 3; i++)
        {
            PlayState.GameSaveData data = PlayState.LoadGame(i, false);
            if (data.profile != -1)
                AddOption(data.character + " | " + ConvertDifficultyToString(data.difficulty) + " | " + ConvertTimeToString(data.gameTime) +
                    " | " + data.percentage + "%", true, PickSpawn, new int[] { 0, i });
            else
                AddOption(PlayState.GetText("menu_option_profile_empty"), true, StartNewGame, new int[] { 0, 1, 1, 0, 2, 0, 3, i });
        }
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_profile_copy"), true, CopyData);
        AddOption(PlayState.GetText("menu_option_profile_erase"), true, EraseData);
        AddOption(PlayState.GetText("menu_option_main_returnTo"), true, PageMain);
        ForceSelect(1);
        backPage = PageMain;
    }

    public void StartNewGame()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_newGame_header"), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_newGame_difficulty") + ": ", true, "difficulty");
        if (PlayState.achievementStates[14] == 1)
        {
            AddOption(PlayState.GetText("menu_option_newGame_character") + ": ", true, "character");
            AddOption(PlayState.GetText("menu_option_newGame_randomized") + ": ", true, "isRandomized");
        }
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_newGame_confirm"), true, StartNewSave);
        AddOption(PlayState.GetText("menu_option_profile_returnTo"), true, ProfileScreen);
        ForceSelect(2);
        backPage = ProfileScreen;
    }

    public void StartNewSave()
    {
        PlayState.player.GetComponent<BoxCollider2D>().enabled = false;
        PlayState.currentProfile = menuVarFlags[3];
        PlayState.currentDifficulty = menuVarFlags[0];
        PlayState.currentTime = new float[] { 0, 0, 0 };
        PlayState.respawnCoords = PlayState.WORLD_SPAWN;
        PlayState.currentCharacter = CharacterIDToName(menuVarFlags[1]);
        PlayState.itemCollection = new int[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0
        };
        PlayState.playerScript.selectedWeapon = 0;
        PlayState.bossStates = new int[] { 1, 1, 1, 1 };
        PlayState.hasSeenIris = false;
        PlayState.talkedToCaveSnail = false;
        PlayState.minimap.transform.parent.GetComponent<Minimap>().currentMap = PlayState.defaultMinimapState;
        PlayState.WriteSave("game");

        if (PlayState.gameState == "Pause")
        {
            Transform lastRoom = PlayState.roomTriggerParent.transform.GetChild((int)PlayState.positionOfLastRoom.x).GetChild((int)PlayState.positionOfLastRoom.y);
            lastRoom.GetComponent<Collider2D>().enabled = true;
            lastRoom.GetComponent<RoomTrigger>().active = true;
            lastRoom.GetComponent<RoomTrigger>().DespawnEverything();
        }

        StartCoroutine(LoadFade(PlayState.WORLD_SPAWN, true));
    }

    public void PickSpawn()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_loadGame_header1"), false);
        AddOption(PlayState.GetText("menu_option_loadGame_header2"), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_loadGame_save"), true, LoadAndSpawn, new int[] { 1, 0 });
        AddOption(PlayState.GetText("menu_option_loadGame_spawn"), true, LoadAndSpawn, new int[] { 1, 1 });
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_profile_returnTo"), true, ProfileScreen);
        ForceSelect(3);
        backPage = ProfileScreen;
    }

    public void LoadAndSpawn()
    {
        PlayState.player.GetComponent<BoxCollider2D>().enabled = false;
        if (menuVarFlags[0] != PlayState.currentProfile)
            PlayState.LoadGame(menuVarFlags[0], true);

        if (PlayState.gameState == "Pause")
        {
            Transform lastRoom = PlayState.roomTriggerParent.transform.GetChild((int)PlayState.positionOfLastRoom.x).GetChild((int)PlayState.positionOfLastRoom.y);
            lastRoom.GetComponent<Collider2D>().enabled = true;
            lastRoom.GetComponent<RoomTrigger>().active = true;
            lastRoom.GetComponent<RoomTrigger>().DespawnEverything();
        }

        StartCoroutine(LoadFade(menuVarFlags[1] == 1 ? PlayState.WORLD_SPAWN : PlayState.respawnCoords));
    }

    public void Unpause()
    {
        PlayState.gameState = "Game";
        PlayState.ToggleHUD(true);
        ToggleHUD(false);
        pauseButtonDown = true;
        PlayState.minimapScript.RefreshMap();
        SetTextComponentOrigins();

        switch (PlayState.currentCharacter)
        {
            default:
            case "Snaily":
                PlayState.player.GetComponent<Snaily>().holdingJump = true;
                break;
                //case "Sluggy":
                //    PlayState.player.GetComponent<Sluggy>().holdingJump = true;
                //    break;
                //case "Upside":
                //    PlayState.player.GetComponent<Upside>().holdingJump = true;
                //    break;
                //case "Leggy":
                //    PlayState.player.GetComponent<Leggy>().holdingJump = true;
                //    break;
                //case "Blobby":
                //    PlayState.player.GetComponent<Blobby>().holdingJump = true;
                //    break;
                //case "Leechy":
                //    PlayState.player.GetComponent<Leechy>().holdingJump = true;
                //    break;
        }
    }

    public void CopyData()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_copyGame_header1"), false);
        for (int i = 1; i <= 3; i++)
        {
            PlayState.GameSaveData data = PlayState.LoadGame(i);
            if (data.profile != -1)
                AddOption(data.character + " | " + ConvertDifficultyToString(data.difficulty) + " | " + ConvertTimeToString(data.gameTime) +
                    " | " + data.percentage + "%", true, CopyData2, new int[] { 0, i });
            else
                AddOption(PlayState.GetText("menu_option_profile_empty"), false);
        }
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_copyGame_cancel"), true, ProfileScreen);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_main_returnTo"), true, PageMain);
        ForceSelect(5);
        backPage = ProfileScreen;
    }

    public void CopyData2()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_copyGame_header2"), false);
        for (int i = 1; i <= 3; i++)
        {
            PlayState.GameSaveData data = PlayState.LoadGame(i);
            if (data.profile != -1)
                AddOption((menuVarFlags[0] == i ? "> " : "") + data.character + " | " + ConvertDifficultyToString(data.difficulty) + " | " + ConvertTimeToString(data.gameTime) +
                    " | " + data.percentage + "%" + (menuVarFlags[0] == i ? " <" : ""), menuVarFlags[0] != i && PlayState.currentProfile != i, CopyConfirm, new int[] { 1, i });
            else
                AddOption(PlayState.GetText("menu_option_profile_empty"), true, CopyConfirm, new int[] { 1, i });
        }
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_copyGame_cancel"), true, ProfileScreen);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_main_returnTo"), true, PageMain);
        ForceSelect(5);
        backPage = ProfileScreen;
    }

    public void CopyConfirm()
    {
        bool isChosenSlotEmpty = PlayState.LoadGame(menuVarFlags[1]).profile == -1;
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_copyGame_header3").Replace("#1", menuVarFlags[0].ToString()).Replace("#2", menuVarFlags[1].ToString()).Replace("_",
            isChosenSlotEmpty ? PlayState.GetText("menu_option_copyGame_empty") : PlayState.GetText("menu_option_copyGame_full")), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_copyGame_confirm"), true, ActuallyCopyData);
        AddOption(PlayState.GetText("menu_option_copyGame_cancelConfirm"), true, ProfileScreen);
        ForceSelect(3);
        backPage = ProfileScreen;
    }

    public void ActuallyCopyData()
    {
        PlayState.WriteSave(PlayState.LoadGame(menuVarFlags[0]), menuVarFlags[1]);
        ProfileScreen();
    }

    public void EraseData()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_eraseGame_header1"), false);
        for (int i = 1; i <= 3; i++)
        {
            PlayState.GameSaveData data = PlayState.LoadGame(i);
            if (data.profile != -1)
            {
                AddOption(data.character + " | " + ConvertDifficultyToString(data.difficulty) + " | " + ConvertTimeToString(data.gameTime) +
                    " | " + data.percentage + "%", PlayState.currentProfile != i, ConfirmErase, new int[] { 0, i });
            }
            else
                AddOption("Empty profile", false);
        }
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_eraseGame_cancel"), true, ProfileScreen);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_main_returnTo"), true, PageMain);
        ForceSelect(5);
        backPage = ProfileScreen;
    }

    public void ConfirmErase()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_eraseGame_header2").Replace("#", menuVarFlags[0].ToString()), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_eraseGame_confirm"), true, ActuallyEraseData);
        AddOption(PlayState.GetText("menu_option_eraseGame_cancelConfirm"), true, ProfileScreen);
        ForceSelect(3);
        backPage = ProfileScreen;
    }

    public void ActuallyEraseData()
    {
        PlayState.EraseGame(menuVarFlags[0]);
        ProfileScreen();
    }

    public void OptionsScreen()
    {
        ClearOptions();
        menuVarFlags[0] = PlayState.gameOptions[8];
        AddOption(PlayState.GetText("menu_option_options_sound"), true, SoundOptions, new int[] { 0, PlayState.gameOptions[0], 1, PlayState.gameOptions[1] });
        AddOption(PlayState.GetText("menu_option_options_display"), true, DisplayOptions, new int[]
        {
            0, PlayState.gameOptions[2], 1, PlayState.gameOptions[3],
            2, PlayState.gameOptions[4], 3, PlayState.gameOptions[5],
            4, PlayState.gameOptions[6], 5, PlayState.gameOptions[7],
            6, PlayState.gameOptions[11]
        });
        AddOption(PlayState.GetText("menu_option_options_controls"), true, ControlMain);
        AddOption(PlayState.GetText("menu_option_options_gameplay"), true, GameplayScreen, new int[]
            { 0, PlayState.gameOptions[8], 1, PlayState.gameOptions[12], 2, PlayState.gameOptions[13], 3, PlayState.gameOptions[14] });
        if (PlayState.gameState == "Menu")
            AddOption(PlayState.GetText("menu_option_options_assets"), true, AssetPackMenu);
        else
            AddOption("", false);
        AddOption(PlayState.GetText("menu_option_options_eraseRecords"), true, RecordEraseSelect);
        if (PlayState.gameState == "Menu")
            AddOption(PlayState.GetText("menu_option_options_importExport"), true, ImportExportData);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_main_returnTo"), true, PageMain);
        ForceSelect(0);
        backPage = PageMain;
    }

    public void SoundOptions()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_sound_soundVol") + ": ", true, "soundVolume");
        AddOption(PlayState.GetText("menu_option_sound_musicVol") + ": ", true, "musicVolume");
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_options_returnTo"), true, SaveOptions);
        ForceSelect(0);
        backPage = SaveOptions;
    }

    public void DisplayOptions()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_display_resolution") + ": ", true, "resolution");
        AddOption(PlayState.GetText("menu_option_display_minimap") + ": ", true, "minimap");
        AddOption(PlayState.GetText("menu_option_display_bottomKeys") + ": ", true, "bottomKeys");
        AddOption(PlayState.GetText("menu_option_display_keymap") + ": ", true, "keymap");
        AddOption(PlayState.GetText("menu_option_display_gameTime") + ": ", true, "gameTime");
        AddOption(PlayState.GetText("menu_option_display_fps") + ": ", true, "fps");
        AddOption(PlayState.GetText("menu_option_display_particles") + ": ", true, "particles");
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_options_returnTo"), true, SaveOptions);
        ForceSelect(0);
        backPage = SaveOptions;
    }

    public void ControlMain()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_controls_control1"), true, Controls1);
        AddOption(PlayState.GetText("menu_option_controls_control2"), true, Controls2);
        AddOption(PlayState.GetText("menu_option_controls_controlMisc"), true, Controls3);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_controls_default"), true, ResetControls);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_options_returnTo"), true, SaveControls);
        ForceSelect(0);
        backPage = SaveControls;
    }

    public void Controls1()
    {
        ClearOptions();
        controlScreen = 1;
        AddOption(PlayState.GetText("menu_option_controls_jump") + ":   ", true, TestForRebind, new int[] { 0, 4 }, "control_jump");
        AddOption(PlayState.GetText("menu_option_controls_shoot") + ":   ", true, TestForRebind, new int[] { 0, 5 }, "control_shoot");
        AddOption(PlayState.GetText("menu_option_controls_strafe") + ":   ", true, TestForRebind, new int[] { 0, 6 }, "control_strafe");
        AddOption(PlayState.GetText("menu_option_controls_speak") + ":   ", true, TestForRebind, new int[] { 0, 7 }, "control_speak");
        AddOption(PlayState.GetText("menu_option_controls_up") + ":   ", true, TestForRebind, new int[] { 0, 2 }, "control_up");
        AddOption(PlayState.GetText("menu_option_controls_left") + ":   ", true, TestForRebind, new int[] { 0, 0 }, "control_left");
        AddOption(PlayState.GetText("menu_option_controls_down") + ":   ", true, TestForRebind, new int[] { 0, 3 }, "control_down");
        AddOption(PlayState.GetText("menu_option_controls_right") + ":   ", true, TestForRebind, new int[] { 0, 1 }, "control_right");
        AddOption(PlayState.GetText("menu_option_controls_return"), true, ControlMain);
        ForceSelect(0);
        backPage = ControlMain;
    }

    public void Controls2()
    {
        ClearOptions();
        controlScreen = 2;
        AddOption(PlayState.GetText("menu_option_controls_jump") + ":   ", true, TestForRebind, new int[] { 0, 12 }, "control_jump");
        AddOption(PlayState.GetText("menu_option_controls_shoot") + ":   ", true, TestForRebind, new int[] { 0, 13 }, "control_shoot");
        AddOption(PlayState.GetText("menu_option_controls_strafe") + ":   ", true, TestForRebind, new int[] { 0, 14 }, "control_strafe");
        AddOption(PlayState.GetText("menu_option_controls_speak") + ":   ", true, TestForRebind, new int[] { 0, 15 }, "control_speak");
        AddOption(PlayState.GetText("menu_option_controls_up") + ":   ", true, TestForRebind, new int[] { 0, 10 },  "control_up");
        AddOption(PlayState.GetText("menu_option_controls_left") + ":   ", true, TestForRebind, new int[] { 0, 8 }, "control_left");
        AddOption(PlayState.GetText("menu_option_controls_down") + ":   ", true, TestForRebind, new int[] { 0, 11 }, "control_down");
        AddOption(PlayState.GetText("menu_option_controls_right") + ":   ", true, TestForRebind, new int[] { 0, 9 }, "control_right");
AddOption(PlayState.GetText("menu_option_controls_return"), true, ControlMain);
        ForceSelect(0);
        backPage = ControlMain;
    }

    public void Controls3()
    {
        ClearOptions();
        controlScreen = 3;
        AddOption(PlayState.GetText("menu_option_controls_weapon1") + ":   ", true, TestForRebind, new int[] { 0, 16 }, "control_weapon1");
        AddOption(PlayState.GetText("menu_option_controls_weapon2") + ":   ", true, TestForRebind, new int[] { 0, 17 }, "control_weapon2");
        AddOption(PlayState.GetText("menu_option_controls_weapon3") + ":   ", true, TestForRebind, new int[] { 0, 18 }, "control_weapon3");
        AddOption(PlayState.GetText("menu_option_controls_weaponNext") + ":   ", true, TestForRebind, new int[] { 0, 19 }, "control_weaponNext");
        AddOption(PlayState.GetText("menu_option_controls_weaponPrev") + ":   ", true, TestForRebind, new int[] { 0, 20 }, "control_weaponPrev");
        AddOption(PlayState.GetText("menu_option_controls_map") + ":   ", true, TestForRebind, new int[] { 0, 21 }, "control_map");
        AddOption(PlayState.GetText("menu_option_controls_menu") + ":   ", true, TestForRebind, new int[] { 0, 22 }, "control_menu");
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_controls_return"), true, ControlMain);
        ForceSelect(0);
        backPage = ControlMain;
    }

    public void ResetControls()
    {
        Control.inputs = Control.defaultInputs;
        SaveControls();
    }

    public void SaveControls()
    {
        PlayState.WriteSave("controls");
        controlScreen = 0;
        OptionsScreen();
    }

    public void GameplayScreen()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_gameplay_shooting") + ": ", true, "shooting");
        AddOption(PlayState.GetText("menu_option_gameplay_breakables") + ": ", true, "showBreakables");
        AddOption(PlayState.GetText("menu_option_gameplay_secretTiles") + ": ", true, "secretTiles");
        AddOption(PlayState.GetText("menu_option_gameplay_frameLimit") + ": ", true, "frameLimit");
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_options_returnTo"), true, OptionsScreen);
        ForceSelect(0);
        backPage = OptionsScreen;
    }

    public void AssetPackMenu()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_assets_texture"), true, AssetPackSelection, new int[] { 0, 1, 1, 0, 6, 0 });
        AddOption(PlayState.GetText("menu_option_assets_sound"), true, AssetPackSelection, new int[] { 0, 2, 1, 0, 6, 0 });
        AddOption(PlayState.GetText("menu_option_assets_music"), true, AssetPackSelection, new int[] { 0, 3, 1, 0, 6, 0 });
        AddOption(PlayState.GetText("menu_option_assets_text"), true, AssetPackSelection, new int[] { 0, 4, 1, 0, 6, 0 });
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_assets_path"), true, ReturnAssetPath);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_options_returnTo"), true, OptionsScreen);
        ForceSelect(0);
        backPage = OptionsScreen;
    }

    public void AssetPackSelection()
    {
        ClearOptions();
        string path = Application.persistentDataPath + menuVarFlags[0] switch
        {
            2 => "/SoundPacks/",
            3 => "/MusicPacks/",
            4 => "/TextPacks/",
            _ => "/TexturePacks/"
        };
        string[] entries = Directory.GetDirectories(path);
        if (entries.Length == 0)
        {
            AddOption(PlayState.GetText("menu_option_assetSelect_noPack1"), false);
            AddOption(PlayState.GetText("menu_option_assetSelect_noPack2"), false);
            AddOption("", false);
            AddOption(PlayState.GetText("menu_option_assets_returnTo"), true, AssetPackMenu);
            ForceSelect(3);
            backPage = AssetPackMenu;
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                if (menuVarFlags[1] + i < entries.Length)
                {
                    string[] packTitleParts = entries[menuVarFlags[1] + i].Split('/');
                    string packTitle = packTitleParts[packTitleParts.Length - 1];
                    AddOption(packTitle, true, ConfirmAssetPack, new int[] { 2, menuVarFlags[1] + i });
                }
                else
                    AddOption("", false);
            }
            AddOption(PlayState.GetText("menu_option_assetSelect_defaultPack"), true, ConfirmAssetPack, new int[] { 2, -1 });
            AddOption(PlayState.GetText("menu_option_assetSelect_next"), menuVarFlags[1] + 5 < entries.Length, AssetPackSelection, new int[] { 1, menuVarFlags[1] + 5, 6, 7 });
            AddOption(PlayState.GetText("menu_option_assetSelect_prev"), menuVarFlags[1] - 5 >= 0, AssetPackSelection, new int[] { 1, menuVarFlags[1] - 5, 6, 8 });
            AddOption(PlayState.GetText("menu_option_assets_returnTo"), true, AssetPackMenu);
            ForceSelect(menuVarFlags[6]);
            backPage = AssetPackMenu;
        }
    }

    public void ConfirmAssetPack()
    {
        if (menuVarFlags[2] == -1)
        {
            ClearOptions();
            string insert = (menuVarFlags[0] == 2 || menuVarFlags[0] == 3) ?
                PlayState.GetText("menu_option_assetConfirm_defaultInfo_audio") : PlayState.GetText("menu_option_assetConfirm_defaultInfo_video");
            AddOption(PlayState.GetText("menu_option_assetConfirm_defaultInfo1").Replace("_", insert), false);
            AddOption(PlayState.GetText("menu_option_assetConfirm_defaultInfo2").Replace("_", insert), false);
            AddOption(PlayState.GetText("menu_option_assetConfirm_defaultInfo3").Replace("_", insert), false);
        }
        else
        {
            string path = Application.persistentDataPath + menuVarFlags[0] switch
            {
                2 => "/SoundPacks/",
                3 => "/MusicPacks/",
                4 => "/TextPacks/",
                _ => "/TexturePacks/"
            };
            string[] entries = Directory.GetDirectories(path);
            string[] packTitleParts = entries[menuVarFlags[2]].Split('/');
            string packTitle = packTitleParts[packTitleParts.Length - 1];

            string[] packInfo = new string[3];
            if (File.Exists(entries[menuVarFlags[2]] + "/Info.txt"))
            {
                string[] infoText = File.ReadAllLines(entries[menuVarFlags[2]] + "/Info.txt");
                packInfo[0] = infoText[1];
                packInfo[1] = infoText[3];
                packInfo[2] = infoText[5];
            }

            ClearOptions();
            tempPackNameBuffer = packTitle;
            AddOption(packTitle, false);
            AddOption(packInfo[0] == null ? PlayState.GetText("menu_option_assetConfirm_noInfo") : PlayState.GetText("menu_option_assetConfirm_author").Replace("_", packInfo[0]), false);
            AddOption(packInfo[0] == null ? "" : PlayState.GetText("menu_option_assetConfirm_version").Replace("#1", packInfo[1]).Replace("#2", packInfo[2]), false);
        }
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_assetConfirm_confirm"), false);
        AddOption(PlayState.GetText("menu_option_assetConfirm_yes"), true, ApplyAssetPack);
        AddOption(PlayState.GetText("menu_option_assetConfirm_no"), true, AssetPackSelection);
        ForceSelect(6);
        backPage = AssetPackSelection;
    }

    public void ApplyAssetPack()
    {
        PlayState.ToggleLoadingIcon(true);
        string packType = menuVarFlags[0] switch
        {
            2 => "Sound",
            3 => "Music",
            4 => "Text",
            _ => "Texture"
        };
        PlayState.currentPacks[menuVarFlags[0] - 1] = menuVarFlags[2] == -1 ? "DEFAULT" : tempPackNameBuffer;
        if (menuVarFlags[2] == -1)
        {
            switch (packType)
            {
                default:
                case "Texture":
                    PlayState.textureLibrary.BuildDefaultSpriteSizeLibrary();
                    PlayState.textureLibrary.BuildDefaultAnimLibrary();
                    PlayState.textureLibrary.BuildDefaultLibrary();
                    PlayState.textureLibrary.BuildTilemap();
                    selector[1].GetComponent<AnimationModule>().ReloadList();
                    selector[1].GetComponent<AnimationModule>().ResetToStart();
                    selector[2].GetComponent<AnimationModule>().ReloadList();
                    selector[2].GetComponent<AnimationModule>().ResetToStart();
                    CreateTitle();
                    break;
                case "Sound":
                    PlayState.soundLibrary.BuildDefaultLibrary();
                    break;
                case "Music":
                    music.Stop();
                    PlayState.musicLibrary.BuildDefaultOffsetLibrary();
                    PlayState.musicLibrary.BuildDefaultLibrary();
                    music.clip = PlayState.GetMusic(0, 0);
                    music.Play();
                    break;
                case "Text":
                    PlayState.textLibrary.BuildDefaultLibrary();
                    CreateTitle();
                    break;
            }
        }
        else
        {
            string path = Application.persistentDataPath + "/" + packType + "Packs/";
            string[] entries = Directory.GetDirectories(path);
            string packPath = entries[menuVarFlags[2]].Replace('\\', '/');

            switch (packType)
            {
                default:
                case "Texture":
                    PlayState.textureLibrary.BuildSpriteSizeLibrary(packPath + "/SpriteSizes.json");
                    PlayState.textureLibrary.BuildAnimationLibrary(packPath + "/Animations.json");
                    PlayState.textureLibrary.BuildLibrary(packPath);
                    selector[1].GetComponent<AnimationModule>().ReloadList();
                    selector[1].GetComponent<AnimationModule>().ResetToStart();
                    selector[2].GetComponent<AnimationModule>().ReloadList();
                    selector[2].GetComponent<AnimationModule>().ResetToStart();
                    CreateTitle();
                    break;
                case "Sound":
                    PlayState.soundLibrary.BuildLibrary(packPath);
                    break;
                case "Music":
                    music.Stop();
                    PlayState.musicLibrary.BuildOffsetLibrary(packPath + "/MusicLoopOffsets.json");
                    PlayState.musicLibrary.BuildLibrary(packPath);
                    break;
                case "Text":
                    PlayState.textLibrary.BuildLibrary(packPath + "/Text.json");
                    CreateTitle();
                    break;
            }
        }
        PlayState.WriteSave("packs");
        PlayState.ToggleLoadingIcon(false);
        AssetPackMenu();
    }

    public void ReturnAssetPath()
    {
        string dataPath = Application.persistentDataPath + "/";

        ClearOptions();
        AddOption(PlayState.GetText("menu_option_assetPath_header1"), false);
        AddOption(PlayState.GetText("menu_option_assetPath_header2"), false);

        string currentText = "";
        int j = 32;
        for (int i = 0; i < dataPath.Length; i++)
        {
            currentText += dataPath[i];
            j--;
            if (j == 0 || i == dataPath.Length - 1)
            {
                AddOption(currentText, false);
                j = 32;
                currentText = "";
            }
        }

        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_assetPath_confirm"), true, AssetPackMenu);
        ForceSelect(currentOptions.Count - 1);
        backPage = AssetPackMenu;
    }

    public void RecordEraseSelect()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_recordErase_achievements"), true, AchievementEraseConfirm);
        AddOption(PlayState.GetText("menu_option_recordErase_times"), true, TimeEraseConfirm);
        AddOption(PlayState.GetText("menu_option_recordErase_all"), true, RecordEraseConfirm);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_options_returnTo"), true, OptionsScreen);
        ForceSelect(4);
        backPage = OptionsScreen;
    }

    public void AchievementEraseConfirm()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_recordErase_header"), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_recordErase_confirm"), true, EraseAchievements);
        AddOption(PlayState.GetText("menu_option_recordErase_return"), true, OptionsScreen);
        ForceSelect(3);
        backPage = OptionsScreen;
    }

    public void EraseAchievements()
    {
        PlayState.gameData.records.achievements = PlayState.achievementDefault;
        PlayState.achievementStates = PlayState.achievementDefault;
        OptionsScreen();
    }

    public void TimeEraseConfirm()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_recordErase_header"), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_recordErase_confirm"), true, EraseTimes);
        AddOption(PlayState.GetText("menu_option_recordErase_return"), true, OptionsScreen);
        ForceSelect(3);
        backPage = OptionsScreen;
    }

    public void EraseTimes()
    {
        PlayState.gameData.records.times = PlayState.timeDefault;
        PlayState.savedTimes = PlayState.timeDefault;
        OptionsScreen();
    }

    public void RecordEraseConfirm()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_recordErase_header"), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_recordErase_confirm"), true, EraseRecords);
        AddOption(PlayState.GetText("menu_option_recordErase_return"), true, OptionsScreen);
        ForceSelect(3);
        backPage = OptionsScreen;
    }

    public void EraseRecords()
    {
        PlayState.gameData.records.achievements = PlayState.achievementDefault;
        PlayState.gameData.records.times = PlayState.timeDefault;
        PlayState.achievementStates = PlayState.achievementDefault;
        PlayState.savedTimes = PlayState.timeDefault;
        OptionsScreen();
    }

    public void ImportExportData()
    {
        if (PlayState.currentProfile == -1)
        {
            ClearOptions();
            AddOption(PlayState.GetText("menu_option_importExport_export"), true, ExportSelect, new int[] { 0, 0 });
            AddOption(PlayState.GetText("menu_option_importExport_import"), true, ImportSelect, new int[] { 0, 0 });
            AddOption("", false);
            AddOption(PlayState.GetText("menu_option_options_returnTo"), true, OptionsScreen);
            ForceSelect(0);
            backPage = OptionsScreen;
        }
        else
        {
            
            ClearOptions();
            AddOption("You can only import and export", false);
            AddOption("data from the main menu", false);
            AddOption("", false);
            AddOption("Whoops!! Go back", true, OptionsScreen);
            ForceSelect(3);
            backPage = OptionsScreen;
        }
    }

    public void ExportSelect()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_importExport_selectSlot") + ": ", true, "slot");
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_importExport_confirm"), true, ExportConfirm);
        AddOption(PlayState.GetText("menu_option_options_returnTo"), true, OptionsScreen);
        ForceSelect(0);
        backPage = ImportExportData;
    }

    public void ExportConfirm()
    {
        ClearOptions();
        if (File.Exists(Application.persistentDataPath + "/Saves/" + PlayState.SAVE_FILE_PREFIX + "_" + (menuVarFlags[0] + 1) + ".json"))
        {
            AddOption(PlayState.GetText("menu_option_export_header1").Replace("#", (menuVarFlags[0] + 1).ToString()), false);
            AddOption(PlayState.GetText("menu_option_export_header2").Replace("#", (menuVarFlags[0] + 1).ToString()), false);
        }
        else
        {
            AddOption(PlayState.GetText("menu_option_export_header3").Replace("#", (menuVarFlags[0] + 1).ToString()), false);
            AddOption(PlayState.GetText("menu_option_export_header4").Replace("#", (menuVarFlags[0] + 1).ToString()), false);
        }
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_export_confirm"), true, WriteDataToFile);
        AddOption(PlayState.GetText("menu_option_export_return"), true, ImportExportData);
        ForceSelect(4);
        backPage = ImportExportData;
    }

    public void WriteDataToFile()
    {
        string dataPath = Application.persistentDataPath + "/Saves/" + PlayState.SAVE_FILE_PREFIX + "_" + (menuVarFlags[0] + 1) + ".json";

        CollectiveData fullData = new CollectiveData { profile1 = PlayState.LoadGame(1), profile2 = PlayState.LoadGame(2), profile3 = PlayState.LoadGame(3) };

        PlayState.OptionData optionDataForCollective = new PlayState.OptionData { options = PlayState.gameOptions  };
        fullData.options = optionDataForCollective;

        PlayState.PackData packDataForCollective = new PlayState.PackData { packs = PlayState.currentPacks };
        fullData.packs = packDataForCollective;

        PlayState.ControlData controlDataForCollective = new PlayState.ControlData { controls = Control.inputs };
        fullData.controls = controlDataForCollective;

        PlayState.RecordData recordDataForCollective = new PlayState.RecordData { achievements = PlayState.achievementStates, times = PlayState.savedTimes  };
        fullData.records = recordDataForCollective;

        File.WriteAllText(dataPath, JsonUtility.ToJson(fullData));

        ClearOptions();
        AddOption(PlayState.GetText("menu_option_export_success1"), false);
        AddOption(PlayState.GetText("menu_option_export_success2"), false);

        string currentText = "";
        int j = 32;
        for (int i = 0; i < dataPath.Length; i++)
        {
            currentText += dataPath[i];
            j--;
            if (j == 0 || i == dataPath.Length - 1)
            {
                AddOption(currentText, false);
                j = 32;
                currentText = "";
            }
        }

        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_export_success_confirm"), true, ImportExportData);
        ForceSelect(currentOptions.Count - 1);
        backPage = ImportExportData;
    }

    public void ImportSelect()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_importExport_selectSlot") + ": ", true, "slot");
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_importExport_confirm"), true, ImportConfirm);
        AddOption(PlayState.GetText("menu_option_options_returnTo"), true, OptionsScreen);
        ForceSelect(0);
        backPage = ImportExportData;
    }

    public void ImportConfirm()
    {
        ClearOptions();
        if (File.Exists(Application.persistentDataPath + "/Saves/" + PlayState.SAVE_FILE_PREFIX + "_" + (menuVarFlags[0] + 1) + ".json"))
        {
            AddOption(PlayState.GetText("menu_option_import_header1").Replace("#", (menuVarFlags[0] + 1).ToString()), false);
            AddOption(PlayState.GetText("menu_option_import_header2").Replace("#", (menuVarFlags[0] + 1).ToString()), false);
            AddOption("", false);
            AddOption(PlayState.GetText("menu_option_import_confirm"), true, ReadDataFromFile);
            AddOption(PlayState.GetText("menu_option_import_return"), true, ImportExportData);
            ForceSelect(4);
            backPage = ImportExportData;
        }
        else
        {
            AddOption(PlayState.GetText("menu_option_import_empty").Replace("#", (menuVarFlags[0] + 1).ToString()), false);
            AddOption("", false);
            AddOption(PlayState.GetText("menu_option_import_empty_return"), true, ImportExportData);
            ForceSelect(2);
            backPage = ImportExportData;
        }
    }

    public void ReadDataFromFile()
    {
        string dataPath = Application.persistentDataPath + "/Saves/" + PlayState.SAVE_FILE_PREFIX + "_" + (menuVarFlags[0] + 1) + ".json";

        PlayState.CollectiveData fullData = JsonUtility.FromJson<PlayState.CollectiveData>(File.ReadAllText(dataPath));

        if (fullData.version == Application.version)
        {
            PlayState.gameData = fullData;
            PlayState.LoadOptions();
            PlayState.LoadPacks();
            PlayState.LoadControls();
            //PlayState.LoadRecords();

            ClearOptions();
            AddOption(PlayState.GetText("menu_option_import_success"), false);
            AddOption("", false);
            AddOption(PlayState.GetText("menu_option_import_success_confirm"), true, ImportExportData);
            ForceSelect(2);
            backPage = ImportExportData;
        }
        else
        {
            string[] importVersionStrings = fullData.version.Split(' ')[1].Split('.');
            int importVersion = (int.Parse(importVersionStrings[0]) * 10000) + (int.Parse(importVersionStrings[1]) * 100) + int.Parse(importVersionStrings[2]);
            string[] currentVersionStrings = Application.version.Split(' ')[1].Split('.');
            int currentVersion = (int.Parse(currentVersionStrings[0]) * 10000) + (int.Parse(currentVersionStrings[1]) * 100) + int.Parse(currentVersionStrings[2]);
            tempDataSlot = fullData;
            ConfirmMismatchedImport(importVersion > currentVersion);
        }
    }

    PlayState.CollectiveData tempDataSlot;
    public void ConfirmMismatchedImport(bool isNewer)
    {
        string add = isNewer ? PlayState.GetText("menu_option_import_mismatched_newer") : PlayState.GetText("menu_option_import_mismatched_older");
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_import_mismatched1").Replace("_", add), false);
        AddOption(PlayState.GetText("menu_option_import_mismatched2").Replace("_", add), false);
        AddOption(PlayState.GetText("menu_option_import_mismatched3").Replace("_", add), false);
        AddOption(PlayState.GetText("menu_option_import_mismatched4").Replace("_", add), false);
        AddOption(PlayState.GetText("menu_option_import_mismatched5").Replace("_", add), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_import_mismatched_confirm"), true, ImportMismatched);
        AddOption(PlayState.GetText("menu_option_import_mismatched_return"), true, ClearTempAndReturn);
        ForceSelect(6);
        backPage = ClearTempAndReturn;
    }

    public void ImportMismatched()
    {
        for (int i = 1; i < 4; i++)
        {
            PlayState.GameSaveData newProfile = new PlayState.GameSaveData();
            var oldProfile = i == 1 ? tempDataSlot.profile1 : (i == 2 ? tempDataSlot.profile2 : tempDataSlot.profile3);

            if (oldProfile.profile == -1)
                newProfile = new PlayState.GameSaveData { profile = -1 };
            else
            {
                newProfile.profile = i;
                newProfile.difficulty = oldProfile.difficulty;
                newProfile.gameTime = oldProfile.gameTime;
                newProfile.saveCoords = oldProfile.saveCoords;
                newProfile.character = oldProfile.character;
                newProfile.items = oldProfile.items;
                newProfile.weapon = oldProfile.weapon;
                newProfile.bossStates = oldProfile.bossStates;
                newProfile.NPCVars = PlayState.NPCvarDefault;
                for (int j = 0; j < oldProfile.NPCVars.Length; j++)
                    newProfile.NPCVars[j] = oldProfile.NPCVars[j];
                newProfile.percentage = oldProfile.percentage;
                newProfile.exploredMap = oldProfile.exploredMap;
            }
            switch (i)
            {
                case 1:
                    PlayState.gameData.profile1 = newProfile;
                    break;
                case 2:
                    PlayState.gameData.profile2 = newProfile;
                    break;
                case 3:
                    PlayState.gameData.profile3 = newProfile;
                    break;
            }
        }

        int[] newAchievementStates = PlayState.achievementDefault;
        for (int i = 0; i < tempDataSlot.records.achievements.Length; i++)
            newAchievementStates[i] = tempDataSlot.records.achievements[i];
        PlayState.gameData.records.achievements = newAchievementStates;

        float[][] newTimes = PlayState.timeDefault;
        for (int i = 0; i < tempDataSlot.records.times.Length; i++)
            newTimes[i] = tempDataSlot.records.times[i];
        PlayState.gameData.records.times = newTimes;
        //PlayState.LoadRecords();
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_import_success"), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_import_success_confirm"), true, ImportExportData);
        ForceSelect(2);
        backPage = ImportExportData;
    }

    public void ClearTempAndReturn()
    {
        tempDataSlot = new PlayState.CollectiveData();
        ImportExportData();
    }

    public void SaveOptions()
    {
        PlayState.WriteSave("options");
        OptionsScreen();
    }

    public void CreditsPage1()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_credits_1-1"), false);
        AddOption(PlayState.GetText("menu_option_credits_1-2"), false);
        AddOption(PlayState.GetText("menu_option_credits_1-3"), false);
        AddOption(PlayState.GetText("menu_option_credits_1-4"), false);
        AddOption(PlayState.GetText("menu_option_credits_1-5"), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_credits_next"), true, CreditsPage2);
        ForceSelect(6);
        backPage = PageMain;
    }

    public void CreditsPage2()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_credits_2-1"), false);
        AddOption(PlayState.GetText("menu_option_credits_2-2"), false);
        AddOption(PlayState.GetText("menu_option_credits_2-3"), false);
        AddOption(PlayState.GetText("menu_option_credits_2-4"), false);
        AddOption(PlayState.GetText("menu_option_credits_2-5"), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_credits_next"), true, CreditsPage3);
        ForceSelect(6);
        backPage = PageMain;
    }

    public void CreditsPage3()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_credits_3-1"), false);
        AddOption(PlayState.GetText("menu_option_credits_3-2"), false);
        AddOption(PlayState.GetText("menu_option_credits_3-3"), false);
        AddOption(PlayState.GetText("menu_option_credits_3-4"), false);
        AddOption(PlayState.GetText("menu_option_credits_3-5"), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_credits_next"), true, CreditsPage4);
        ForceSelect(6);
        backPage = PageMain;
    }

    public void CreditsPage4()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_credits_4-1"), false);
        AddOption(PlayState.GetText("menu_option_credits_4-2"), false);
        AddOption(PlayState.GetText("menu_option_credits_4-3"), false);
        AddOption(PlayState.GetText("menu_option_credits_4-4"), false);
        AddOption(PlayState.GetText("menu_option_credits_4-5"), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_credits_next"), true, CreditsPage5);
        ForceSelect(6);
        backPage = PageMain;
    }

    public void CreditsPage5()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_credits_5-1"), false);
        AddOption(PlayState.GetText("menu_option_credits_5-2"), false);
        AddOption(PlayState.GetText("menu_option_credits_5-3"), false);
        AddOption(PlayState.GetText("menu_option_credits_5-4"), false);
        AddOption(PlayState.GetText("menu_option_credits_5-5"), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_credits_next"), true, CreditsPage6);
        ForceSelect(6);
        backPage = PageMain;
    }

    public void CreditsPage6()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_credits_6-1"), false);
        AddOption(PlayState.GetText("menu_option_credits_6-2"), false);
        AddOption(PlayState.GetText("menu_option_credits_6-3"), false);
        AddOption(PlayState.GetText("menu_option_credits_6-4"), false);
        AddOption(PlayState.GetText("menu_option_credits_6-5"), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_credits_next"), true, CreditsPage7);
        ForceSelect(6);
        backPage = PageMain;
    }

    public void CreditsPage7()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_credits_7-1"), false);
        AddOption(PlayState.GetText("menu_option_credits_7-2"), false);
        AddOption(PlayState.GetText("menu_option_credits_7-3"), false);
        AddOption(PlayState.GetText("menu_option_credits_7-4"), false);
        AddOption(PlayState.GetText("menu_option_credits_7-5"), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_credits_next"), true, CreditsPage8);
        ForceSelect(6);
        backPage = PageMain;
    }

    public void CreditsPage8()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_credits_8-1"), false);
        AddOption(PlayState.GetText("menu_option_credits_8-2"), false);
        AddOption(PlayState.GetText("menu_option_credits_8-3"), false);
        AddOption(PlayState.GetText("menu_option_credits_8-4"), false);
        AddOption(PlayState.GetText("menu_option_credits_8-5"), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_main_returnTo"), true, PageMain);
        ForceSelect(6);
        backPage = PageMain;
    }

    public void MenuReturnConfirm()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_return_header"), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_return_confirmSave"), true, SaveQuit);
        AddOption(PlayState.GetText("menu_option_return_confirmNoSave"), true, ReturnToMenu);
        AddOption(PlayState.GetText("menu_option_return_cancel"), true, PageMain);
        ForceSelect(2);
        backPage = PageMain;
    }

    public void SaveQuit()
    {
        PlayState.WriteSave("game");
        ReturnToMenu();
    }

    public void ReturnToMenu()
    {
        PlayState.gameState = "Menu";
        PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 0, 0.5f);
        cam.position = panPoints[0];
        PageMain();
        PlayState.player.GetComponent<BoxCollider2D>().enabled = false;
        PlayState.playerScript.StopMusic();

        PlayState.skyLayer.transform.localPosition = Vector2.zero;
        PlayState.bgLayer.transform.localPosition = Vector2.zero;
        PlayState.fg1Layer.transform.localPosition = Vector2.zero;
        PlayState.fg2Layer.transform.localPosition = Vector2.zero;
        Transform lastRoom = PlayState.roomTriggerParent.transform.GetChild((int)PlayState.positionOfLastRoom.x).GetChild((int)PlayState.positionOfLastRoom.y);
        lastRoom.GetComponent<Collider2D>().enabled = true;
        lastRoom.GetComponent<RoomTrigger>().active = true;
        lastRoom.GetComponent<RoomTrigger>().DespawnEverything();
        PlayState.currentArea = -1;
        PlayState.currentSubzone = -1;
        PlayState.currentProfile = -1;

        music.Play();
    }

    public void QuitConfirm()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_quit_header"), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_quit_confirm"), true, QuitGame);
        AddOption(PlayState.GetText("menu_option_quit_return"), true, PageMain);
        ForceSelect(3);
        backPage = PageMain;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
