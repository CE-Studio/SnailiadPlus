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
        public TextObject textScript;
        public int[] menuParam;
    }

    private List<MenuOption> currentOptions = new List<MenuOption>();
    private DestinationDelegate backPage;
    private int[] menuVarFlags = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    private int controlScreen = 0;
    private bool isRebinding = false;
    private bool pauseButtonDown = false;
    private bool fadingToIntro = false;

    private const float LIST_CENTER_Y = -1.25f;
    private const float LIST_OPTION_SPACING = 1.25f;
    private float currentSpawnY = LIST_CENTER_Y;
    private const float SELECT_SNAIL_VERTICAL_OFFSET = -0.625f;
    private const float LETTER_SPAWN_TIME = Mathf.PI / 11;
    private const float ACHIEVEMENT_ICON_SPACING = 4f;
    private const float ACHIEVEMENT_ICON_Y = -0.25f;
    private const float ACHIEVEMENT_ICON_LERP_VALUE = 12f;
    private const float GALLERY_LERP_VALUE = 7.5f;

    private List<TextObject> activeOptions = new();

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
    private Vector2 titleHomePos = Vector2.zero;
    private float letterOffsetForIntro = -3.75f;
    private bool lerpLetterOffsetToZero = false;
    
    private struct AchievementIcon
    {
        public GameObject obj;
        public SpriteRenderer icon;
        public AnimationModule iconAnim;
        public SpriteRenderer frame;
        public AnimationModule frameAnim;
    }
    private string[] achievements = new string[] { };
    private List<AchievementIcon> achievementIcons = new();

    private struct GalleryPart
    {
        public Transform obj;
        public SpriteRenderer image;
        public AnimationModule anim;
    }
    private GalleryPart galleryBG;
    private GalleryPart galleryImage;
    private bool viewingGallery = false;

    public static AudioSource music;
    public GameObject textObject;
    public GameObject titleLetter;
    public GameObject titlePlus;
    public GameObject[] selector;

    public List<GameObject> letters = new();

    public GameObject[] menuHUDElements;

    private readonly string acceptedChars = "abcdefghijklmnopqrstuvwxyz +";
    public Dictionary<char, int> letterPixelWidths = new()
    {
        { 'a', 28 }, { 'b', 28 }, { 'c', 24 }, { 'd', 28 }, { 'e', 24 }, { 'f', 24 }, { 'g', 28 }, { 'h', 24 },
        { 'i', 6 }, { 'j', 24 }, { 'k', 24 }, { 'l', 6 }, { 'm', 32 }, { 'n', 24 }, { 'o', 28 }, { 'p', 28 },
        { 'q', 28 }, { 'r', 24 }, { 's', 25 }, { 't', 24 }, { 'u', 28 }, { 'v', 24 }, { 'w', 32 }, { 'x', 28 },
        { 'y', 24 }, { 'z', 24 }, { ' ', 12 }, { '+', 24 }
    };

    void Start()
    {
        PlayState.screenCover.sortingOrder = 1001;
        PlayState.ScreenFlash("Solid Color", 0, 0, 0, 255);

        if (!Directory.Exists(Application.persistentDataPath + "/Saves"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Saves");
        for (int i = 1; i <= 3; i++)
        {
            if (!File.Exists(Application.persistentDataPath + "/Saves/" + PlayState.SAVE_FILE_PREFIX + "_Profile" + i + ".json"))
                File.WriteAllText(Application.persistentDataPath + "/Saves/" + PlayState.SAVE_FILE_PREFIX + "_Profile" + i + ".json",
                    JsonUtility.ToJson(PlayState.blankProfile));
        }
        if (!File.Exists(Application.persistentDataPath + "/Saves/" + PlayState.SAVE_FILE_PREFIX + "_OptionsAndRecords.json"))
            File.WriteAllText(Application.persistentDataPath + "/Saves/" + PlayState.SAVE_FILE_PREFIX + "_OptionsAndRecords.json",
                JsonUtility.ToJson(PlayState.blankData));

        if (!Directory.Exists(Application.persistentDataPath + "/TexturePacks"))
            Directory.CreateDirectory(Application.persistentDataPath + "/TexturePacks");
        if (!Directory.Exists(Application.persistentDataPath + "/SoundPacks"))
            Directory.CreateDirectory(Application.persistentDataPath + "/SoundPacks");
        if (!Directory.Exists(Application.persistentDataPath + "/MusicPacks"))
            Directory.CreateDirectory(Application.persistentDataPath + "/MusicPacks");
        if (!Directory.Exists(Application.persistentDataPath + "/TextPacks"))
            Directory.CreateDirectory(Application.persistentDataPath + "/TextPacks");

        PlayState.LoadAllMainData();

        PlayState.LoadPacks();

        PlayState.loadingIcon.GetComponent<AnimationModule>().Add("Loading");
        PlayState.loadingIcon.GetComponent<AnimationModule>().Play("Loading");

        Screen.SetResolution(400 * (PlayState.generalData.windowSize + 1), 240 * (PlayState.generalData.windowSize + 1), false);

        PlayState.player.GetComponent<BoxCollider2D>().enabled = false;
        cam = PlayState.cam.transform;
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
        PlayState.credits.BuildEntityRollCall();

        foreach (Transform area in PlayState.roomTriggerParent.transform)
        {
            foreach (Transform room in area)
            {
                room.GetComponent<RoomTrigger>().MoveEntitiesToInternalList();
                room.GetComponent<RoomTrigger>().LogBreakables();
            }
        }

        menuHUDElements = new GameObject[]
        {
            selector[0],
            GameObject.Find("Version Text")
        };

        string[] version = Application.version.Split(' ');
        string versionText = PlayState.GetText("menu_version_header") + "\n" + (version[0].ToLower() == "release" ? PlayState.GetText("menu_version_release") :
            (version[0].ToLower() == "demo" ? PlayState.GetText("menu_version_demo") : PlayState.GetText("menu_version_developer"))) + " " + version[1];
        menuHUDElements[1].GetComponent<TextObject>().SetText(versionText);

        CreateTitle();
        PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 0, 0.5f);
        PlayState.loadingIcon.SetActive(false);
        preloading = false;

        titleHomePos = PlayState.titleParent.transform.localPosition;
        PlayState.titleParent.transform.localPosition = new Vector2(titleHomePos.x, titleHomePos.y + letterOffsetForIntro);

        textObject = Resources.Load<GameObject>("Objects/Text Object");

        for (int i = 0; i < 2; i++)
        {
            GameObject newGalleryPart = new("Gallery Part");
            newGalleryPart.transform.parent = transform;
            SpriteRenderer partSprite = newGalleryPart.AddComponent<SpriteRenderer>();
            AnimationModule partAnim = newGalleryPart.AddComponent<AnimationModule>();
            if (i == 0)
            {
                galleryBG = new GalleryPart
                {
                    obj = newGalleryPart.transform,
                    image = partSprite,
                    anim = partAnim
                };
                galleryBG.image.sortingOrder = 1010;
                galleryBG.anim.Add("Ending_background");
                galleryBG.obj.localPosition = new Vector2(26, 0);
            }
            else
            {
                galleryImage = new GalleryPart
                {
                    obj = newGalleryPart.transform,
                    image = partSprite,
                    anim = partAnim
                };
                galleryImage.image.sortingOrder = 1011;
                galleryImage.anim.Add("Ending_normal");
                galleryImage.anim.Add("Ending_bossRush");
                galleryImage.anim.Add("Ending_100");
                galleryImage.anim.Add("Ending_sub30");
                galleryImage.anim.Add("Ending_insane");
                galleryImage.obj.localPosition = new Vector2(-26, 0);
            }
            partSprite.color = new Color(1, 1, 1, 0);
        }
    }

    void Update()
    {
        if (PlayState.gameState == PlayState.GameState.preload)
        {
            PlayState.titleRoom.RemoteActivateRoom();
            PlayState.gameState = PlayState.GameState.menu;
        }

        if (lerpLetterOffsetToZero && letterOffsetForIntro != 0)
        {
            letterOffsetForIntro = Mathf.Lerp(letterOffsetForIntro, 0, 10f * Time.deltaTime);
            PlayState.titleParent.transform.localPosition = new Vector2(titleHomePos.x, titleHomePos.y + letterOffsetForIntro);
        }
        if ((PlayState.gameState == PlayState.GameState.menu || PlayState.gameState == PlayState.GameState.pause) && !PlayState.isMenuOpen)
        {
            if (PlayState.gameState == PlayState.GameState.menu && !preloading)
            {
                music.volume = PlayState.generalData.musicVolume * 0.1f;
                music.Play();
            }
            PlayState.isMenuOpen = true;
            PlayState.ToggleHUD(false);
            ToggleHUD(true);
            PageIntro();
            currentPointInIndex = 0;
            moveTimer = 0;
            isMoving = true;
            int i = 0;
            while (!currentOptions[i].selectable && i < currentOptions.Count)
                i++;
            selectedOption = i;
            GetNewSnailOffset();
            selector[1].GetComponent<AnimationModule>().Play("Title_selector_" + (PlayState.currentProfileNumber != 0 ? PlayState.currentProfile.character : "Snaily"));
            selector[2].GetComponent<AnimationModule>().Play("Title_selector_" + (PlayState.currentProfileNumber != 0 ? PlayState.currentProfile.character : "Snaily"));
        }
        if (PlayState.gameState == PlayState.GameState.menu)
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
        if (PlayState.gameState == PlayState.GameState.menu || PlayState.gameState == PlayState.GameState.pause)
        {
            music.volume = (PlayState.generalData.musicVolume * 0.1f) * PlayState.fader;
            Application.targetFrameRate = PlayState.generalData.frameLimiter switch
            {
                1 => 30,
                2 => 60,
                3 => 120,
                _ => -1
            };

            if (!isRebinding && !fadingToIntro && !viewingGallery)
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
                else if (Control.JumpPress(1) || Input.GetKeyDown(KeyCode.Return))
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
            else if (!isRebinding && !fadingToIntro && viewingGallery)
            {
                if (Control.JumpPress() || Control.Pause())
                {
                    PlayState.PlaySound("MenuBeep2");
                    viewingGallery = false;
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
                        TestForArrowAdjust(option, 0, PlayState.generalData.achievements[14] ? 2 : 1);
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
                        PlayState.generalData.shootMode = menuVarFlags[0] == 1;
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
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_all"));
                                break;
                        }
                        PlayState.generalData.breakableState = menuVarFlags[1];
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
                        PlayState.generalData.secretMapTilesVisible = menuVarFlags[2] == 1;
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
                        PlayState.generalData.frameLimiter = menuVarFlags[3];
                        break;
                    case "soundVolume":
                        TestForArrowAdjust(option, 0, 10);
                        AddToOptionText(option, menuVarFlags[0].ToString());
                        PlayState.generalData.soundVolume = menuVarFlags[0];
                        break;
                    case "musicVolume":
                        TestForArrowAdjust(option, 1, 10);
                        AddToOptionText(option, menuVarFlags[1].ToString());
                        PlayState.generalData.musicVolume = menuVarFlags[1];
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
                        PlayState.generalData.windowSize = menuVarFlags[0];
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
                        PlayState.generalData.minimapState = menuVarFlags[1];
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
                        PlayState.generalData.bottomKeyState = menuVarFlags[2];
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
                        PlayState.generalData.keymapState = menuVarFlags[3] == 1;
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
                        PlayState.generalData.timeState = menuVarFlags[4] == 1;
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
                        PlayState.generalData.FPSState = menuVarFlags[5] == 1;
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
                        PlayState.generalData.particleState = menuVarFlags[6];
                        break;
                    case "paletteShader":
                        TestForArrowAdjust(option, 7, 1);
                        switch (menuVarFlags[7])
                        {
                            case 0:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_off"));
                                break;
                            case 1:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_on"));
                                break;
                        }
                        PlayState.generalData.paletteFilterState = menuVarFlags[7] == 1;
                        break;
                    case "control_jump":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(12) : Control.ParseKeyName(4));
                        break;
                    case "control_jump1":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseButtonName(4));
                        break;
                    case "control_jump2":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseButtonName(12));
                        break;
                    case "control_shoot":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(13) : Control.ParseKeyName(5));
                        break;
                    case "control_shoot1":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseButtonName(5));
                        break;
                    case "control_shoot2":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseButtonName(13));
                        break;
                    case "control_strafe":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(14) : Control.ParseKeyName(6));
                        break;
                    case "control_strafe1":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseButtonName(6));
                        break;
                    case "control_strafe2":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseButtonName(14));
                        break;
                    case "control_speak":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(15) : Control.ParseKeyName(7));
                        break;
                    case "control_speak1":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseButtonName(7));
                        break;
                    case "control_speak2":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseButtonName(15));
                        break;
                    case "control_up":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 4 ? Control.ParseButtonName(2) :
                                (controlScreen == 2 ? Control.ParseKeyName(10) : Control.ParseKeyName(2)));
                        break;
                    case "control_left":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 4 ? Control.ParseButtonName(0) :
                                (controlScreen == 2 ? Control.ParseKeyName(8) : Control.ParseKeyName(0)));
                        break;
                    case "control_right":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 4 ? Control.ParseButtonName(1) :
                                (controlScreen == 2 ? Control.ParseKeyName(9) : Control.ParseKeyName(1)));
                        break;
                    case "control_down":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 4 ? Control.ParseButtonName(3) :
                                (controlScreen == 2 ? Control.ParseKeyName(11) : Control.ParseKeyName(3)));
                        break;
                    case "control_aimUp":
                        AddToOptionText(option, Control.ParseButtonName(10));
                        break;
                    case "control_aimLeft":
                        AddToOptionText(option, Control.ParseButtonName(8));
                        break;
                    case "control_aimRight":
                        AddToOptionText(option, Control.ParseButtonName(9));
                        break;
                    case "control_aimDown":
                        AddToOptionText(option, Control.ParseButtonName(11));
                        break;
                    case "control_weapon1":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 4 ? Control.ParseButtonName(16) : Control.ParseKeyName(16));
                        break;
                    case "control_weapon2":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 4 ? Control.ParseButtonName(17) : Control.ParseKeyName(17));
                        break;
                    case "control_weapon3":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 4 ? Control.ParseButtonName(18) : Control.ParseKeyName(18));
                        break;
                    case "control_weaponNext":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 4 ? Control.ParseButtonName(19) : Control.ParseKeyName(19));
                        break;
                    case "control_weaponPrev":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 4 ? Control.ParseButtonName(20) : Control.ParseKeyName(20));
                        break;
                    case "control_map":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 4 ? Control.ParseButtonName(21) : Control.ParseKeyName(21));
                        break;
                    case "control_menu":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 4 ? Control.ParseButtonName(22) : Control.ParseKeyName(22));
                        break;
                    case "control_back":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 4 ? Control.ParseButtonName(23) : Control.ParseKeyName(23));
                        break;
                    case "buttonType":
                        TestForArrowAdjust(option, 0, 3);
                        switch (menuVarFlags[0])
                        {
                            case 0:
                                AddToOptionText(option, PlayState.GetText("menu_option_controls_buttonType_xbox"));
                                break;
                            case 1:
                                AddToOptionText(option, PlayState.GetText("menu_option_controls_buttonType_nintendo"));
                                break;
                            case 2:
                                AddToOptionText(option, PlayState.GetText("menu_option_controls_buttonType_playstation"));
                                break;
                            case 3:
                                AddToOptionText(option, PlayState.GetText("menu_option_controls_buttonType_ouya"));
                                break;
                        }
                        PlayState.generalData.controllerFaceType = menuVarFlags[0];
                        break;
                    case "screenShake":
                        TestForArrowAdjust(option, 4, 4);
                        switch (menuVarFlags[4])
                        {
                            case 0:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_off"));
                                break;
                            case 1:
                                AddToOptionText(option, PlayState.GetText("menu_add_shake_minimal"));
                                break;
                            case 2:
                                AddToOptionText(option, PlayState.GetText("menu_add_generic_on"));
                                break;
                            case 3:
                                AddToOptionText(option, PlayState.GetText("menu_add_shake_minNoHud"));
                                break;
                            case 4:
                                AddToOptionText(option, PlayState.GetText("menu_add_shake_noHud"));
                                break;
                        }
                        PlayState.generalData.screenShake = menuVarFlags[4];
                        break;
                    case "slot":
                        TestForArrowAdjust(option, 0, 9);
                        AddToOptionText(option, (menuVarFlags[0] + 1).ToString() +
                            (File.Exists(Application.persistentDataPath + "/Saves/" + PlayState.SAVE_FILE_PREFIX + "_" + (menuVarFlags[0] + 1) + ".json") ? " (full)" : " (empty)"));
                        break;
                    case "achievements":
                        if (TestForArrowAdjust(option, 0, achievements.Length - 1))
                        {
                            foreach (AchievementIcon icon in achievementIcons)
                            {
                                int thisID = int.Parse(icon.obj.name.Split(' ')[1]);
                                if (thisID == menuVarFlags[0])
                                    icon.frameAnim.Play("AchievementFrame_selected");
                                else if (thisID != menuVarFlags[0] && icon.frameAnim.currentAnimName == "AchievementFrame_selected")
                                    icon.frameAnim.Play("AchievementFrame_idle");
                            }

                            if (PlayState.generalData.achievements[menuVarFlags[0]])
                            {
                                currentOptions[4].textScript.SetText(PlayState.GetText(string.Format("menu_option_achievements_{0}_title",
                                    achievements[menuVarFlags[0]].ToLower())));
                                currentOptions[5].textScript.SetText(PlayState.GetText(string.Format("menu_option_achievements_{0}_desc",
                                    achievements[menuVarFlags[0]].ToLower())));
                            }
                            else
                            {
                                currentOptions[4].textScript.SetText(PlayState.GetText("menu_option_achievements_locked_title"));
                                currentOptions[5].textScript.SetText(PlayState.GetText("menu_option_achievements_locked_desc"));
                            }
                        }
                        break;
                }
            }
            GetNewSnailOffset();
        }

        if (PlayState.gameState != PlayState.GameState.menu && PlayState.gameState != PlayState.GameState.pause &&
            PlayState.gameState != PlayState.GameState.map && PlayState.gameState != PlayState.GameState.debug)
        {
            if (PlayState.isMenuOpen)
            {
                PlayState.isMenuOpen = false;
                ClearOptions();
                music.Stop();
                PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 0, 0.25f, 0, 999);
                ToggleHUD(false);
            }
            if (!PlayState.isMenuOpen && Control.Pause() && !pauseButtonDown && (PlayState.gameState != PlayState.GameState.error)
                && !PlayState.playerScript.inDeathCutscene && PlayState.creditsState == 0)
            {
                PlayState.isMenuOpen = true;
                PlayState.ToggleHUD(false);
                ToggleHUD(true);
                PlayState.gameState = PlayState.GameState.pause;
                PlayState.ScreenFlash("Solid Color", 0, 0, 0, 0);
                PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 150, 0.25f, 0, 0);
                PageMain();
                CreateTitle();
            }
            if (pauseButtonDown && !Control.Pause())
                pauseButtonDown = false;
        }
    }

    private void LateUpdate()
    {
        if (PlayState.gameState == PlayState.GameState.menu || PlayState.gameState == PlayState.GameState.pause)
        {
            selector[0].transform.localPosition = new Vector2(0,
                    Mathf.Lerp(selector[0].transform.localPosition.y,
                    currentOptions[selectedOption].textScript.position.y + SELECT_SNAIL_VERTICAL_OFFSET, 15 * Time.deltaTime));
            selector[1].transform.localPosition = new Vector2(Mathf.Lerp(selector[1].transform.localPosition.x, -selectSnailOffset, 15 * Time.deltaTime), 0);
            selector[2].transform.localPosition = new Vector2(Mathf.Lerp(selector[2].transform.localPosition.x, selectSnailOffset, 15 * Time.deltaTime), 0);

            if (achievementIcons.Count != 0)
            {
                foreach (AchievementIcon icon in achievementIcons)
                {
                    int iconID = int.Parse(icon.obj.name.Split(' ')[1]);
                    icon.obj.transform.localPosition = Vector2.Lerp(icon.obj.transform.localPosition,
                        new Vector2((iconID - menuVarFlags[0]) * ACHIEVEMENT_ICON_SPACING, ACHIEVEMENT_ICON_Y), ACHIEVEMENT_ICON_LERP_VALUE * Time.deltaTime);
                }
            }

            float timeStep = GALLERY_LERP_VALUE * Time.deltaTime;
            if (viewingGallery)
            {
                galleryBG.obj.transform.localPosition = Vector2.Lerp(galleryBG.obj.transform.localPosition, Vector2.zero, timeStep);
                galleryBG.image.color = Color.Lerp(galleryBG.image.color, new Color(1, 1, 1, 1), timeStep);
                galleryImage.obj.transform.localPosition = Vector2.Lerp(galleryImage.obj.transform.localPosition, Vector2.zero, timeStep);
                galleryImage.image.color = Color.Lerp(galleryImage.image.color, new Color(1, 1, 1, 1), timeStep);
            }
            else
            {
                galleryBG.obj.transform.localPosition = Vector2.Lerp(galleryBG.obj.transform.localPosition, new Vector2(26, 0), timeStep);
                galleryBG.image.color = Color.Lerp(galleryBG.image.color, new Color(1, 1, 1, 0), timeStep);
                galleryImage.obj.transform.localPosition = Vector2.Lerp(galleryImage.obj.transform.localPosition, new Vector2(-26, 0), timeStep);
                galleryImage.image.color = Color.Lerp(galleryImage.image.color, new Color(1, 1, 1, 0), timeStep);
            }
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

    public bool TestForArrowAdjust(MenuOption option, int varSlot, int max)
    {
        if (selectedOption == currentOptions.IndexOf(option))
        if (Control.LeftPress(1))
        {
            menuVarFlags[varSlot]--;
            if (menuVarFlags[varSlot] < 0)
                menuVarFlags[varSlot] = max;
            PlayState.PlaySound("MenuBeep1");
            return true;
        }
        else if (Control.RightPress(1))
        {
            menuVarFlags[varSlot]++;
            if (menuVarFlags[varSlot] > max)
                menuVarFlags[varSlot] = 0;
            PlayState.PlaySound("MenuBeep1");
            return true;
        }
        return false;
    }

    public void TestForRebind()
    {
        StartCoroutine(RebindKey(menuVarFlags[0], menuVarFlags[1]));
    }

    public IEnumerator RebindKey(int controlID, int keyOrCon)
    {
        bool bindingController = keyOrCon == 1;
        while (Control.JumpHold(1) || Control.Generic(KeyCode.Return))
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
                    if (bindingController)
                        PlayState.generalData.controllerInputs[controlID] = key;
                    else
                        PlayState.generalData.keyboardInputs[controlID] = key;
                    isRebinding = false;
                }
            }
            if (bindingController)
            {
                if (Input.GetAxis("LStickX") > 0.75f)
                {
                    PlayState.generalData.controllerInputs[controlID] = KeyCode.Alpha0;
                    isRebinding = false;
                }
                else if (Input.GetAxis("LStickX") < -0.75f)
                {
                    PlayState.generalData.controllerInputs[controlID] = KeyCode.Keypad0;
                    isRebinding = false;
                }
                if (Input.GetAxis("LStickY") > 0.75f)
                {
                    PlayState.generalData.controllerInputs[controlID] = KeyCode.Alpha1;
                    isRebinding = false;
                }
                else if (Input.GetAxis("LStickY") < -0.75f)
                {
                    PlayState.generalData.controllerInputs[controlID] = KeyCode.Keypad1;
                    isRebinding = false;
                }
                if (Input.GetAxis("RStickX") > 0.75f)
                {
                    PlayState.generalData.controllerInputs[controlID] = KeyCode.Alpha2;
                    isRebinding = false;
                }
                else if (Input.GetAxis("RStickX") < -0.75f)
                {
                    PlayState.generalData.controllerInputs[controlID] = KeyCode.Keypad2;
                    isRebinding = false;
                }
                if (Input.GetAxis("RStickY") > 0.75f)
                {
                    PlayState.generalData.controllerInputs[controlID] = KeyCode.Alpha3;
                    isRebinding = false;
                }
                else if (Input.GetAxis("RStickY") < -0.75f)
                {
                    PlayState.generalData.controllerInputs[controlID] = KeyCode.Keypad3;
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
        option.textScript.SetText((option.optionID == selectedOption ? "< " : "") + option.optionText + text + (option.optionID == selectedOption ? " >" : ""));
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
        foreach (TextObject selectedText in activeOptions)
        {
            selectedText.position += new Vector2(0f, LIST_OPTION_SPACING * 0.5f);
            selectedText.transform.localPosition = selectedText.position;
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
        option.textScript = newText.GetComponent<TextObject>();
        option.textScript.SetText(option.optionText);
        option.textScript.SetAlignment("center");
        option.textScript.CreateShadow();
        option.textScript.position = new Vector3(0, currentSpawnY);
        option.textScript.transform.localPosition = option.textScript.position;
        activeOptions.Add(option.textScript);
        currentSpawnY -= LIST_OPTION_SPACING * 0.5f;

        if (paramChange != null)
            option.menuParam = paramChange;

        option.textScript.SetColor(option.selectable ? PlayState.GetColor("0312") : PlayState.GetColor("0309"));

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

    public string ConvertDifficultyToString(int difficulty)
    {
        return difficulty switch
        {
            1 => PlayState.GetText("difficulty_normal"),
            2 => PlayState.GetText("difficulty_insane"),
            _ => PlayState.GetText("difficulty_easy")
        };
    }

    public void ClearOptions()
    {
        activeOptions.Clear();
        foreach (MenuOption option in currentOptions)
            Destroy(option.textScript.gameObject);
        currentOptions.Clear();
        currentSpawnY = LIST_CENTER_Y;
    }

    public void GetNewSnailOffset()
    {
        float textBounds = currentOptions[selectedOption].textScript.GetWidth(true);
        selectSnailOffset = textBounds * 0.5f + 1.5f;
    }

    public void ForceSelect(int optionNum)
    {
        selectedOption = optionNum;
        selector[0].transform.localPosition = new Vector2(0, currentOptions[optionNum].textScript.position.y + SELECT_SNAIL_VERTICAL_OFFSET);
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
                GameObject newLetter = Instantiate(titleLetter, PlayState.titleParent.transform);
                TitleLetter letterScript = newLetter.GetComponent<TitleLetter>();
                letterScript.Create(title[i], new Vector2(letterSpawnX + (title[i] == '+' ? -0.25f : 0),
                    title[i] == '+' ? 0.0625f : 0), currentDelay + (title[i] == '+' ? 2 : 0));
                currentDelay += LETTER_SPAWN_TIME;
                letters.Add(newLetter);
            }
            letterSpawnX += (letterPixelWidths[title[i]] + 4) * 0.0625f;
        }
    }

    public IEnumerator LoadFade(Vector2 spawnPos, bool runIntro = false)
    {
        RoomTrigger lastRoomTrigger;

        if (PlayState.currentArea != -1)
        {
            Transform lastRoom = PlayState.roomTriggerParent.transform.GetChild((int)PlayState.positionOfLastRoom.x).GetChild((int)PlayState.positionOfLastRoom.y);
            lastRoom.GetComponent<Collider2D>().enabled = true;
            lastRoom.GetComponent<RoomTrigger>().active = true;
            lastRoomTrigger = lastRoom.GetComponent<RoomTrigger>();
            PlayState.currentArea = -1;
            PlayState.currentSubzone = -1;
        }
        else
        {
            lastRoomTrigger = PlayState.titleRoom;
            PlayState.titleRoom.active = true;
        }

        fadingToIntro = true;
        PlayState.screenCover.sortingOrder = 1001;
        PlayState.ScreenFlash("Solid Color", 0, 63, 125, 0);
        PlayState.ScreenFlash("Custom Fade", 0, 63, 125, 255, 0.5f);
        yield return new WaitForSeconds(0.5f);

        if (runIntro)
        {

        }

        lastRoomTrigger.DespawnEverything();
        PlayState.ResetAllParticles();
        PlayState.screenCover.sortingOrder = 999;
        PlayState.SetCamFocus(PlayState.player.transform);
        PlayState.cam.transform.position = spawnPos;
        PlayState.player.transform.position = spawnPos;
        PlayState.playerScript.CorrectGravity(false);
        PlayState.playerScript.ResetState();
        PlayState.gameState = PlayState.GameState.game;
        PlayState.player.GetComponent<BoxCollider2D>().enabled = true;
        PlayState.ToggleHUD(true);
        PlayState.minimapScript.RefreshMap();
        PlayState.BuildPlayerMarkerArray();
        PlayState.globalFunctions.CalculateMaxHealth();
        PlayState.playerScript.health = PlayState.playerScript.maxHealth;
        PlayState.globalFunctions.RenderNewHearts();
        PlayState.globalFunctions.UpdateHearts();
        PlayState.globalFunctions.shellStateBuffer = PlayState.GetShellLevel();
        PlayState.ToggleBossfightState(false, 0, true);
        PlayState.hasJumped = false;
        SetTextComponentOrigins();
        Control.ClearVirtual(true, true);
        fadingToIntro = false;

        PlayState.playerScript.holdingJump = true;
        if (PlayState.lastLoadedWeapon != 0)
            PlayState.globalFunctions.ChangeActiveWeapon(PlayState.lastLoadedWeapon - 1);
        else
            PlayState.globalFunctions.ChangeActiveWeapon(PlayState.CheckForItem(2) || PlayState.CheckForItem(12) ? 2 :
                (PlayState.CheckForItem(1) || PlayState.CheckForItem(11) ? 1 : 0));
    }

    public void SetTextComponentOrigins()
    {
        PlayState.hudPause.position = new Vector2(-12.4375f, -6.875f + (PlayState.generalData.keymapState ? 2 : (
            PlayState.generalData.timeState && PlayState.generalData.FPSState ? 1 : (!PlayState.generalData.timeState && !PlayState.generalData.FPSState ? 0 : 0.5f))));
        PlayState.hudFps.position = new Vector2(PlayState.generalData.keymapState ? -10.4375f : -12.4375f, PlayState.generalData.timeState ? -6.375f : -6.875f);
        PlayState.hudTime.position = new Vector2(PlayState.generalData.keymapState ? -10.4375f : -12.4375f, -6.875f);
    }

    public int[] ToggleAchievementInterface(bool state)
    {
        if (state)
        {
            if (achievements.Length == 0)
                achievements = Enum.GetNames(typeof(AchievementPanel.Achievements));
            int totalAchievementCount = achievements.Length;
            int collectedAchievementCount = 0;
            for (int i = 0; i < totalAchievementCount; i++)
            {
                GameObject newObj = new("Icon " + i.ToString());
                GameObject newObjFrame = new("Icon " + i.ToString() + " Frame");
                newObj.transform.parent = transform;
                newObjFrame.transform.parent = newObj.transform;
                newObj.transform.localPosition = new Vector2(ACHIEVEMENT_ICON_SPACING * i, ACHIEVEMENT_ICON_Y);
                AchievementIcon thisIcon = new()
                {
                    obj = newObj,
                    icon = newObj.AddComponent<SpriteRenderer>(),
                    iconAnim = newObj.AddComponent<AnimationModule>(),
                    frame = newObjFrame.AddComponent<SpriteRenderer>(),
                    frameAnim = newObjFrame.AddComponent<AnimationModule>()
                };
                thisIcon.icon.sortingOrder = 1010;
                thisIcon.frame.sortingOrder = 1011;
                achievementIcons.Add(thisIcon);
                thisIcon.frameAnim.Add("AchievementFrame_idle");
                thisIcon.frameAnim.Add("AchievementFrame_selected");
                thisIcon.frameAnim.Play(i == 0 ? "AchievementFrame_selected" : "AchievementFrame_idle");

                string iconAnimName = "Achievement_locked";
                if (PlayState.generalData.achievements[i])
                {
                    collectedAchievementCount++;
                    iconAnimName = "Achievement_" + achievements[i].ToLower();
                }
                thisIcon.iconAnim.Add(iconAnimName);
                thisIcon.iconAnim.Play(iconAnimName);
            }
            return new int[] { collectedAchievementCount, totalAchievementCount };
        }
        else
        {
            for (int i = achievementIcons.Count - 1; i >= 0; i--)
                Destroy(achievementIcons[i].obj);
            achievementIcons.Clear();
            return new int[] { };
        }
    }

    public void PageIntro()
    {
        ClearOptions();
        if (PlayState.IsControllerConnected())
            AddOption(string.Format(PlayState.GetText("menu_intro_controller"),
                PlayState.generalData.controllerInputs[(int)Control.Controller.Jump1].ToString()), true, PageMain);
        else
            AddOption(string.Format(PlayState.GetText("menu_intro_keyboard"),
                PlayState.generalData.keyboardInputs[(int)Control.Keyboard.Jump1].ToString()), true, PageMain);
        AddOption("", false);
        AddOption("", false);
        ForceSelect(0);
        backPage = null;
    }

    public void PageMain()
    {
        ClearOptions();
        lerpLetterOffsetToZero = true;
        bool returnAvailable = false;
        if (PlayState.gameState == PlayState.GameState.pause)
        {
            AddOption(PlayState.GetText("menu_option_main_return"), true, Unpause);
            returnAvailable = true;
        }
        AddOption(PlayState.GetText("menu_option_main_profile"), true, ProfileScreen);
        if (PlayState.generalData.achievements[6])
            AddOption(PlayState.GetText("menu_option_main_bossRush"), true);
        AddOption(PlayState.GetText("menu_option_main_multiplayer"), true);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_main_options"), true, OptionsScreen);
        AddOption(PlayState.GetText("menu_option_main_credits"), true, CreditsPage1);
        if (PlayState.HasTime() || PlayState.HasAchievemements())
            AddOption(PlayState.GetText("menu_option_main_records"), true, RecordsScreen);
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
        PlayState.LoadAllProfiles();
        AddOption(PlayState.GetText("menu_option_profile_header"), false);
        for (int i = 1; i <= 3; i++)
        {
            PlayState.ProfileData data = i switch { 1 => PlayState.profile1, 2 => PlayState.profile2, _ => PlayState.profile3 };
            if (data.isEmpty)
                AddOption(PlayState.GetText("menu_option_profile_empty"), true, StartNewGame, new int[] { 0, 1, 1, 0, 2, 0, 3, i });
            else
                AddOption(data.character + " | " + ConvertDifficultyToString(data.difficulty) + " | " + PlayState.GetTimeString(data.gameTime) +
                    " | " + data.percentage + "%", true, PickSpawn, new int[] { 0, i });
        }
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_profile_copy"), true, CopyData);
        AddOption(PlayState.GetText("menu_option_profile_erase"), true, EraseData);
        AddOption(PlayState.GetText(PlayState.currentProfileNumber != 0 ? "menu_option_sub_returnTo" : "menu_option_main_returnTo"), true, PageMain);
        ForceSelect(1);
        backPage = PageMain;
    }

    public void StartNewGame()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_newGame_header"), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_newGame_difficulty") + ": ", true, "difficulty");
        if (PlayState.generalData.achievements[14])
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
        PlayState.currentProfileNumber = menuVarFlags[3];
        PlayState.currentProfile = PlayState.blankProfile;
        PlayState.currentProfile.difficulty = menuVarFlags[0];
        PlayState.SetPlayer(CharacterIDToName(menuVarFlags[1]));
        PlayState.playerScript.selectedWeapon = 0;
        PlayState.currentProfile.isEmpty = false;
        PlayState.WriteSave(PlayState.currentProfileNumber, false);
        PlayState.LoadGame(PlayState.currentProfileNumber, true);

        if (PlayState.gameState == PlayState.GameState.pause)
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
        if (menuVarFlags[0] != PlayState.currentProfileNumber)
            PlayState.LoadGame(menuVarFlags[0], true);
        PlayState.currentProfileNumber = menuVarFlags[0];
        PlayState.SetPlayer(PlayState.currentProfile.character);

        StartCoroutine(LoadFade(menuVarFlags[1] == 1 ? PlayState.WORLD_SPAWN : PlayState.currentProfile.saveCoords));
    }

    public void Unpause()
    {
        PlayState.gameState = PlayState.GameState.game;
        PlayState.ToggleHUD(true);
        ToggleHUD(false);
        pauseButtonDown = true;
        PlayState.minimapScript.RefreshMap();
        SetTextComponentOrigins();

        PlayState.playerScript.holdingJump = true;
    }

    public void CopyData()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_copyGame_header1"), false);
        for (int i = 1; i <= 3; i++)
        {
            PlayState.ProfileData data = i switch { 1 => PlayState.profile1, 2 => PlayState.profile2, _ => PlayState.profile3 };
            if (data.isEmpty)
                AddOption(PlayState.GetText("menu_option_profile_empty"), false);
            else
                AddOption(data.character + " | " + ConvertDifficultyToString(data.difficulty) + " | " + PlayState.GetTimeString(data.gameTime) +
                    " | " + data.percentage + "%", true, CopyData2, new int[] { 0, i });
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
            PlayState.ProfileData data = i switch { 1 => PlayState.profile1, 2 => PlayState.profile2, _ => PlayState.profile3 };
            if (data.isEmpty)
                AddOption(PlayState.GetText("menu_option_profile_empty"), true, CopyConfirm, new int[] { 1, i });
            else
                AddOption((menuVarFlags[0] == i ? "> " : "") + data.character + " | " + ConvertDifficultyToString(data.difficulty) + " | " +
                    PlayState.GetTimeString(data.gameTime) + " | " + data.percentage + "%" + (menuVarFlags[0] == i ? " <" : ""),
                    menuVarFlags[0] != i && PlayState.currentProfileNumber != i, CopyConfirm, new int[] { 1, i });
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
        bool isChosenSlotEmpty = (menuVarFlags[1] switch { 1 => PlayState.profile1, 2 => PlayState.profile2, _ => PlayState.profile3 }).isEmpty;
        ClearOptions();
        AddOption(string.Format(PlayState.GetText("menu_option_copyGame_header3"), menuVarFlags[0].ToString(), menuVarFlags[1].ToString(),
            isChosenSlotEmpty ? PlayState.GetText("menu_option_copyGame_empty") : PlayState.GetText("menu_option_copyGame_full")), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_copyGame_confirm"), true, ActuallyCopyData);
        AddOption(PlayState.GetText("menu_option_copyGame_cancelConfirm"), true, ProfileScreen);
        ForceSelect(3);
        backPage = ProfileScreen;
    }

    public void ActuallyCopyData()
    {
        PlayState.CopySave(menuVarFlags[0], menuVarFlags[1]);
        ProfileScreen();
    }

    public void EraseData()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_eraseGame_header1"), false);
        for (int i = 1; i <= 3; i++)
        {
            PlayState.ProfileData data = i switch { 1 => PlayState.profile1, 2 => PlayState.profile2, _ => PlayState.profile3 };
            if (data.isEmpty)
                AddOption(PlayState.GetText("menu_option_profile_empty"), false);
            else
                AddOption(data.character + " | " + ConvertDifficultyToString(data.difficulty) + " | " + PlayState.GetTimeString(data.gameTime) +
                    " | " + data.percentage + "%", true, ConfirmErase, new int[] { 0, i });
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
        if (PlayState.currentProfileNumber == menuVarFlags[0])
        {
            AddOption(string.Format(PlayState.GetText("menu_option_eraseGame_header3"), menuVarFlags[0].ToString()), false);
            AddOption(string.Format(PlayState.GetText("menu_option_eraseGame_header4"), menuVarFlags[0].ToString()), false);
            AddOption(string.Format(PlayState.GetText("menu_option_eraseGame_header5"), menuVarFlags[0].ToString()), false);
            AddOption(string.Format(PlayState.GetText("menu_option_eraseGame_header2"), menuVarFlags[0].ToString()), false);
            AddOption("", false);
            AddOption(PlayState.GetText("menu_option_eraseGame_confirm"), true, EraseAndBoot);
            AddOption(PlayState.GetText("menu_option_eraseGame_cancelConfirm"), true, ProfileScreen);
            ForceSelect(6);
        }
        else
        {
            AddOption(string.Format(PlayState.GetText("menu_option_eraseGame_header2"), menuVarFlags[0].ToString()), false);
            AddOption("", false);
            AddOption(PlayState.GetText("menu_option_eraseGame_confirm"), true, ActuallyEraseData);
            AddOption(PlayState.GetText("menu_option_eraseGame_cancelConfirm"), true, ProfileScreen);
            ForceSelect(3);
        }
        backPage = ProfileScreen;
    }

    public void ActuallyEraseData()
    {
        PlayState.EraseGame(menuVarFlags[0]);
        ProfileScreen();
    }

    public void EraseAndBoot()
    {
        PlayState.EraseGame(menuVarFlags[0]);
        PlayState.currentProfile = PlayState.blankProfile;
        ReturnToMenu();
    }

    public void OptionsScreen()
    {
        ClearOptions();
        menuVarFlags[0] = PlayState.generalData.shootMode ? 1 : 0;
        AddOption(PlayState.GetText("menu_option_options_sound"), true, SoundOptions, new int[] { 0, PlayState.generalData.soundVolume, 1, PlayState.generalData.musicVolume });
        AddOption(PlayState.GetText("menu_option_options_display"), true, DisplayOptions, new int[]
        {
            0, PlayState.generalData.windowSize, 1, PlayState.generalData.minimapState,
            2, PlayState.generalData.bottomKeyState, 3, PlayState.generalData.keymapState ? 1 : 0,
            4, PlayState.generalData.timeState ? 1 : 0, 5, PlayState.generalData.FPSState ? 1 : 0,
            6, PlayState.generalData.particleState, 7, PlayState.generalData.paletteFilterState ? 1 : 0
        });
        AddOption(PlayState.GetText("menu_option_options_controls"), true, ControlMain);
        AddOption(PlayState.GetText("menu_option_options_gameplay"), true, GameplayScreen, new int[]
            { 0, PlayState.generalData.shootMode ? 1 : 0, 1, PlayState.generalData.breakableState,
                2, PlayState.generalData.secretMapTilesVisible ? 1 : 0, 3, PlayState.generalData.frameLimiter,
                4, PlayState.generalData.screenShake });
        if (PlayState.gameState == PlayState.GameState.menu)
            AddOption(PlayState.GetText("menu_option_options_assets"), true, AssetPackMenu);
        else
            AddOption("", false);
        AddOption(PlayState.GetText("menu_option_options_eraseRecords"), true, RecordEraseSelect);
        AddOption("", false);
        AddOption(PlayState.GetText(PlayState.currentProfileNumber != 0 ? "menu_option_sub_returnTo" : "menu_option_main_returnTo"), true, PageMain);
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
        AddOption(PlayState.GetText("menu_option_display_paletteShader") + ": ", true, "paletteShader");
        AddOption(PlayState.GetText("menu_option_options_returnTo"), true, SaveOptions);
        ForceSelect(0);
        backPage = SaveOptions;
    }

    public void ControlMain()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_controls_main_keyboard"), true, ControlKeyboard);
        if (PlayState.IsControllerConnected())
            AddOption(PlayState.GetText("menu_option_controls_main_controller"), true, ControlController, new int[] { 0, PlayState.generalData.controllerFaceType });
        else
            AddOption(PlayState.GetText("menu_option_controls_main_noController"), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_options_returnTo"), true, OptionsScreen);
        ForceSelect(0);
        backPage = OptionsScreen;
    }

    public void ControlKeyboard()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_controls_keyboard_set1"), true, ControlsKey1);
        AddOption(PlayState.GetText("menu_option_controls_keyboard_set2"), true, ControlsKey2);
        AddOption(PlayState.GetText("menu_option_controls_keyboard_set3"), true, ControlsKey3);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_controls_default"), true, ResetKeyboardControls);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_options_returnTo"), true, SaveControls);
        ForceSelect(0);
        backPage = SaveControls;
    }

    public void ControlController()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_controls_controller_set1"), true, ControlsCon1);
        AddOption(PlayState.GetText("menu_option_controls_controller_set2"), true, ControlsCon2);
        AddOption(PlayState.GetText("menu_option_controls_controller_set3"), true, ControlsCon3);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_controls_buttonType") + ": ", true, "buttonType");
        AddOption(PlayState.GetText("menu_option_controls_default"), true, ResetControllerControls);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_options_returnTo"), true, SaveControls);
        ForceSelect(0);
        backPage = SaveControls;
    }

    public void ControlsKey1()
    {
        ClearOptions();
        controlScreen = 1;
        AddOption(PlayState.GetText("menu_option_controls_jump") + ":   ", true, TestForRebind, new int[] { 0, 4, 1, 0 }, "control_jump");
        AddOption(PlayState.GetText("menu_option_controls_shoot") + ":   ", true, TestForRebind, new int[] { 0, 5, 1, 0 }, "control_shoot");
        AddOption(PlayState.GetText("menu_option_controls_strafe") + ":   ", true, TestForRebind, new int[] { 0, 6, 1, 0 }, "control_strafe");
        AddOption(PlayState.GetText("menu_option_controls_speak") + ":   ", true, TestForRebind, new int[] { 0, 7, 1, 0 }, "control_speak");
        AddOption(PlayState.GetText("menu_option_controls_up") + ":   ", true, TestForRebind, new int[] { 0, 2, 1, 0 }, "control_up");
        AddOption(PlayState.GetText("menu_option_controls_left") + ":   ", true, TestForRebind, new int[] { 0, 0, 1, 0 }, "control_left");
        AddOption(PlayState.GetText("menu_option_controls_down") + ":   ", true, TestForRebind, new int[] { 0, 3, 1, 0 }, "control_down");
        AddOption(PlayState.GetText("menu_option_controls_right") + ":   ", true, TestForRebind, new int[] { 0, 1, 1, 0 }, "control_right");
        AddOption(PlayState.GetText("menu_option_exitControlMenu"), true, ControlKeyboard);
        ForceSelect(0);
        backPage = ControlKeyboard;
    }

    public void ControlsKey2()
    {
        ClearOptions();
        controlScreen = 2;
        AddOption(PlayState.GetText("menu_option_controls_jump") + ":   ", true, TestForRebind, new int[] { 0, 12, 1, 0 }, "control_jump");
        AddOption(PlayState.GetText("menu_option_controls_shoot") + ":   ", true, TestForRebind, new int[] { 0, 13, 1, 0 }, "control_shoot");
        AddOption(PlayState.GetText("menu_option_controls_strafe") + ":   ", true, TestForRebind, new int[] { 0, 14, 1, 0 }, "control_strafe");
        AddOption(PlayState.GetText("menu_option_controls_speak") + ":   ", true, TestForRebind, new int[] { 0, 15, 1, 0 }, "control_speak");
        AddOption(PlayState.GetText("menu_option_controls_up") + ":   ", true, TestForRebind, new int[] { 0, 10, 1, 0 },  "control_up");
        AddOption(PlayState.GetText("menu_option_controls_left") + ":   ", true, TestForRebind, new int[] { 0, 8, 1, 0 }, "control_left");
        AddOption(PlayState.GetText("menu_option_controls_down") + ":   ", true, TestForRebind, new int[] { 0, 11, 1, 0 }, "control_down");
        AddOption(PlayState.GetText("menu_option_controls_right") + ":   ", true, TestForRebind, new int[] { 0, 9, 1, 0 }, "control_right");
        AddOption(PlayState.GetText("menu_option_exitControlMenu"), true, ControlKeyboard);
        ForceSelect(0);
        backPage = ControlKeyboard;
    }

    public void ControlsKey3()
    {
        ClearOptions();
        controlScreen = 3;
        AddOption(PlayState.GetText("menu_option_controls_weapon1") + ":   ", true, TestForRebind, new int[] { 0, 16, 1, 0 }, "control_weapon1");
        AddOption(PlayState.GetText("menu_option_controls_weapon2") + ":   ", true, TestForRebind, new int[] { 0, 17, 1, 0 }, "control_weapon2");
        AddOption(PlayState.GetText("menu_option_controls_weapon3") + ":   ", true, TestForRebind, new int[] { 0, 18, 1, 0 }, "control_weapon3");
        AddOption(PlayState.GetText("menu_option_controls_weaponNext") + ":   ", true, TestForRebind, new int[] { 0, 19, 1, 0 }, "control_weaponNext");
        AddOption(PlayState.GetText("menu_option_controls_weaponPrev") + ":   ", true, TestForRebind, new int[] { 0, 20, 1, 0 }, "control_weaponPrev");
        AddOption(PlayState.GetText("menu_option_controls_map") + ":   ", true, TestForRebind, new int[] { 0, 21, 1, 0 }, "control_map");
        AddOption(PlayState.GetText("menu_option_controls_menu") + ":   ", true, TestForRebind, new int[] { 0, 22, 1, 0 }, "control_menu");
        AddOption(PlayState.GetText("menu_option_controls_backButton") + ":   ", true, TestForRebind, new int[] { 0, 23, 1, 0 }, "control_back");
        AddOption(PlayState.GetText("menu_option_exitControlMenu"), true, ControlKeyboard);
        ForceSelect(0);
        backPage = ControlKeyboard;
    }

    public void ControlsCon1()
    {
        ClearOptions();
        controlScreen = 4;
        AddOption(PlayState.GetText("menu_option_controls_up") + ":   ", true, TestForRebind, new int[] { 0, 2, 1, 1 }, "control_up");
        AddOption(PlayState.GetText("menu_option_controls_left") + ":   ", true, TestForRebind, new int[] { 0, 0, 1, 1 }, "control_left");
        AddOption(PlayState.GetText("menu_option_controls_down") + ":   ", true, TestForRebind, new int[] { 0, 3, 1, 1 }, "control_down");
        AddOption(PlayState.GetText("menu_option_controls_right") + ":   ", true, TestForRebind, new int[] { 0, 1, 1, 1 }, "control_right");
        AddOption(PlayState.GetText("menu_option_controls_aimUp") + ":   ", true, TestForRebind, new int[] { 0, 10, 1, 1 }, "control_aimUp");
        AddOption(PlayState.GetText("menu_option_controls_aimLeft") + ":   ", true, TestForRebind, new int[] { 0, 8, 1, 1 }, "control_aimLeft");
        AddOption(PlayState.GetText("menu_option_controls_aimDown") + ":   ", true, TestForRebind, new int[] { 0, 11, 1, 1 }, "control_aimDown");
        AddOption(PlayState.GetText("menu_option_controls_aimRight") + ":   ", true, TestForRebind, new int[] { 0, 9, 1, 1 }, "control_aimRight");
        AddOption(PlayState.GetText("menu_option_exitControlMenu"), true, ControlController);
        ForceSelect(0);
        backPage = ControlController;
    }

    public void ControlsCon2()
    {
        ClearOptions();
        controlScreen = 4;
        AddOption(PlayState.GetText("menu_option_controls_jump1") + ":   ", true, TestForRebind, new int[] { 0, 4, 1, 1 }, "control_jump1");
        AddOption(PlayState.GetText("menu_option_controls_jump2") + ":   ", true, TestForRebind, new int[] { 0, 12, 1, 1 }, "control_jump2");
        AddOption(PlayState.GetText("menu_option_controls_shoot1") + ":   ", true, TestForRebind, new int[] { 0, 5, 1, 1 }, "control_shoot1");
        AddOption(PlayState.GetText("menu_option_controls_shoot2") + ":   ", true, TestForRebind, new int[] { 0, 13, 1, 1 }, "control_shoot2");
        AddOption(PlayState.GetText("menu_option_controls_strafe1") + ":   ", true, TestForRebind, new int[] { 0, 6, 1, 1 }, "control_strafe1");
        AddOption(PlayState.GetText("menu_option_controls_strafe2") + ":   ", true, TestForRebind, new int[] { 0, 14, 1, 1 }, "control_strafe2");
        AddOption(PlayState.GetText("menu_option_controls_speak1") + ":   ", true, TestForRebind, new int[] { 0, 7, 1, 1 }, "control_speak1");
        AddOption(PlayState.GetText("menu_option_controls_speak2") + ":   ", true, TestForRebind, new int[] { 0, 15, 1, 1 }, "control_speak2");
        AddOption(PlayState.GetText("menu_option_exitControlMenu"), true, ControlController);
        ForceSelect(0);
        backPage = ControlController;
    }

    public void ControlsCon3()
    {
        ClearOptions();
        controlScreen = 4;
        AddOption(PlayState.GetText("menu_option_controls_weapon1") + ":   ", true, TestForRebind, new int[] { 0, 16, 1, 1 }, "control_weapon1");
        AddOption(PlayState.GetText("menu_option_controls_weapon2") + ":   ", true, TestForRebind, new int[] { 0, 17, 1, 1 }, "control_weapon2");
        AddOption(PlayState.GetText("menu_option_controls_weapon3") + ":   ", true, TestForRebind, new int[] { 0, 18, 1, 1 }, "control_weapon3");
        AddOption(PlayState.GetText("menu_option_controls_weaponNext") + ":   ", true, TestForRebind, new int[] { 0, 19, 1, 1 }, "control_weaponNext");
        AddOption(PlayState.GetText("menu_option_controls_weaponPrev") + ":   ", true, TestForRebind, new int[] { 0, 20, 1, 1 }, "control_weaponPrev");
        AddOption(PlayState.GetText("menu_option_controls_map") + ":   ", true, TestForRebind, new int[] { 0, 21, 1, 1 }, "control_map");
        AddOption(PlayState.GetText("menu_option_controls_menu") + ":   ", true, TestForRebind, new int[] { 0, 22, 1, 1 }, "control_menu");
        AddOption(PlayState.GetText("menu_option_controls_back") + ":   ", true, TestForRebind, new int[] { 0, 23, 1, 1 }, "control_back");
        AddOption(PlayState.GetText("menu_option_exitControlMenu"), true, ControlController);
        ForceSelect(0);
        backPage = ControlController;
    }

    public void ResetKeyboardControls()
    {
        Control.keyboardInputs = Control.defaultKeyboardInputs;
        SaveControls();
    }

    public void ResetControllerControls()
    {
        Control.controllerInputs = Control.defaultControllerInputs;
        SaveControls();
    }

    public void SaveControls()
    {
        PlayState.WriteSave(0, true);
        controlScreen = 0;
        ControlMain();
    }

    public void GameplayScreen()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_gameplay_shooting") + ": ", true, "shooting");
        AddOption(PlayState.GetText("menu_option_gameplay_breakables") + ": ", true, "showBreakables");
        AddOption(PlayState.GetText("menu_option_gameplay_secretTiles") + ": ", true, "secretTiles");
        AddOption(PlayState.GetText("menu_option_gameplay_frameLimit") + ": ", true, "frameLimit");
        AddOption(PlayState.GetText("menu_option_gameplay_screenShake") + ": ", true, "screenShake");
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
            AddOption(string.Format(PlayState.GetText("menu_option_assetConfirm_defaultInfo1"), insert), false);
            AddOption(string.Format(PlayState.GetText("menu_option_assetConfirm_defaultInfo2"), insert), false);
            AddOption(string.Format(PlayState.GetText("menu_option_assetConfirm_defaultInfo3"), insert), false);
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
            AddOption(packInfo[0] == null ? PlayState.GetText("menu_option_assetConfirm_noInfo") : string.Format(PlayState.GetText("menu_option_assetConfirm_author"), packInfo[0]), false);
            AddOption(packInfo[0] == null ? "" : string.Format(PlayState.GetText("menu_option_assetConfirm_version"), packInfo[1], packInfo[2]), false);
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
        switch (menuVarFlags[0])
        {
            case 1:
                PlayState.generalData.texturePackID = (menuVarFlags[2] == -1) ? "DEFAULT" : tempPackNameBuffer;
                break;
            case 2:
                PlayState.generalData.soundPackID = (menuVarFlags[2] == -1) ? "DEFAULT" : tempPackNameBuffer;
                break;
            case 3:
                PlayState.generalData.musicPackID = (menuVarFlags[2] == -1) ? "DEFAULT" : tempPackNameBuffer;
                break;
            case 4:
                PlayState.generalData.textPackID = (menuVarFlags[2] == -1) ? "DEFAULT" : tempPackNameBuffer;
                break;
        }
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
                    PlayState.minimapScript.RefreshAnims();
                    PlayState.subscreenScript.RefreshAnims();
                    PlayState.RefreshPoolAnims();
                    PlayState.player.GetComponent<AnimationModule>().ReloadList();
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
        PlayState.WriteSave(0, true);
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
        PlayState.generalData.achievements = new bool[Enum.GetNames(typeof(AchievementPanel.Achievements)).Length];
        PlayState.WriteSave(0, true);
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
        PlayState.generalData.times = PlayState.timeDefault;
        PlayState.WriteSave(0, true);
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
        PlayState.generalData.achievements = new bool[Enum.GetNames(typeof(AchievementPanel.Achievements)).Length];
        PlayState.generalData.times = PlayState.timeDefault;
        PlayState.WriteSave(0, true);
        OptionsScreen();
    }

    public void ImportExportData()
    {
        if (PlayState.currentProfileNumber == 0)
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

    // This function is a scrapped "export collective data" function that I realized was kinda redundant when you had to go to the same place
    // that your current save file was stored to find the exports in the first place, and with the new save system it's easier to just
    // copy and paste individual profiles anyway
    // The structs that this function relied on have since been removed. If for whatever reason you want to add them back in and add this functionality back,
    // refer to the main project's commit history during the "save data refactor" branch's existence (June of 2023). Or write a better system
    public void WriteDataToFile()
    {
        //string dataPath = Application.persistentDataPath + "/Saves/" + PlayState.SAVE_FILE_PREFIX + "_" + (menuVarFlags[0] + 1) + ".json";
        //
        //CollectiveData fullData = new CollectiveData { profile1 = PlayState.LoadGame(1), profile2 = PlayState.LoadGame(2), profile3 = PlayState.LoadGame(3) };
        //
        //PlayState.OptionData optionDataForCollective = new PlayState.OptionData { options = PlayState.gameOptions  };
        //fullData.options = optionDataForCollective;
        //
        //PlayState.PackData packDataForCollective = new PlayState.PackData { packs = PlayState.currentPacks };
        //fullData.packs = packDataForCollective;
        //
        //PlayState.ControlData controlDataForCollective = new PlayState.ControlData { keyboard = Control.keyboardInputs, controller = Control.controllerInputs };
        //fullData.controls = controlDataForCollective;
        //
        //PlayState.RecordData recordDataForCollective = new PlayState.RecordData { achievements = PlayState.achievementStates, times = PlayState.savedTimes  };
        //fullData.records = recordDataForCollective;
        //
        //File.WriteAllText(dataPath, JsonUtility.ToJson(fullData));
        //
        //ClearOptions();
        //AddOption(PlayState.GetText("menu_option_export_success1"), false);
        //AddOption(PlayState.GetText("menu_option_export_success2"), false);
        //
        //string currentText = "";
        //int j = 32;
        //for (int i = 0; i < dataPath.Length; i++)
        //{
        //    currentText += dataPath[i];
        //    j--;
        //    if (j == 0 || i == dataPath.Length - 1)
        //    {
        //        AddOption(currentText, false);
        //        j = 32;
        //        currentText = "";
        //    }
        //}
        //
        //AddOption("", false);
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

    // All of the below commented functions fall under the same scrapped feature as the above WriteDataToFile() function
    public void ReadDataFromFile()
    {
        //string dataPath = Application.persistentDataPath + "/Saves/" + PlayState.SAVE_FILE_PREFIX + "_" + (menuVarFlags[0] + 1) + ".json";
        //
        //PlayState.CollectiveData fullData = JsonUtility.FromJson<PlayState.CollectiveData>(File.ReadAllText(dataPath));
        //
        //if (fullData.version == Application.version)
        //{
        //    PlayState.gameData = fullData;
        //    PlayState.LoadOptions();
        //    PlayState.LoadPacks();
        //    PlayState.LoadControls();
        //    //PlayState.LoadRecords();
        //
        //    ClearOptions();
        //    AddOption(PlayState.GetText("menu_option_import_success"), false);
        //    AddOption("", false);
        //    AddOption(PlayState.GetText("menu_option_import_success_confirm"), true, ImportExportData);
        //    ForceSelect(2);
        //    backPage = ImportExportData;
        //}
        //else
        //{
        //    string[] importVersionStrings = fullData.version.Split(' ')[1].Split('.');
        //    int importVersion = (int.Parse(importVersionStrings[0]) * 10000) + (int.Parse(importVersionStrings[1]) * 100) + int.Parse(importVersionStrings[2]);
        //    string[] currentVersionStrings = Application.version.Split(' ')[1].Split('.');
        //    int currentVersion = (int.Parse(currentVersionStrings[0]) * 10000) + (int.Parse(currentVersionStrings[1]) * 100) + int.Parse(currentVersionStrings[2]);
        //    tempDataSlot = fullData;
        //    ConfirmMismatchedImport(importVersion > currentVersion);
        //}
    }

    //PlayState.CollectiveData tempDataSlot;
    //public void ConfirmMismatchedImport(bool isNewer)
    //{
    //    string add = isNewer ? PlayState.GetText("menu_option_import_mismatched_newer") : PlayState.GetText("menu_option_import_mismatched_older");
    //    ClearOptions();
    //    AddOption(PlayState.GetText("menu_option_import_mismatched1").Replace("_", add), false);
    //    AddOption(PlayState.GetText("menu_option_import_mismatched2").Replace("_", add), false);
    //    AddOption(PlayState.GetText("menu_option_import_mismatched3").Replace("_", add), false);
    //    AddOption(PlayState.GetText("menu_option_import_mismatched4").Replace("_", add), false);
    //    AddOption(PlayState.GetText("menu_option_import_mismatched5").Replace("_", add), false);
    //    AddOption("", false);
    //    AddOption(PlayState.GetText("menu_option_import_mismatched_confirm"), true, ImportMismatched);
    //    AddOption(PlayState.GetText("menu_option_import_mismatched_return"), true, ClearTempAndReturn);
    //    ForceSelect(6);
    //    backPage = ClearTempAndReturn;
    //}

    //public void ImportMismatched()
    //{
    //    for (int i = 1; i < 4; i++)
    //    {
    //        PlayState.GameSaveData newProfile = new PlayState.GameSaveData();
    //        var oldProfile = i == 1 ? tempDataSlot.profile1 : (i == 2 ? tempDataSlot.profile2 : tempDataSlot.profile3);
    //
    //        if (oldProfile.profile == -1)
    //            newProfile = new PlayState.GameSaveData { profile = -1 };
    //        else
    //        {
    //            newProfile.profile = i;
    //            newProfile.difficulty = oldProfile.difficulty;
    //            newProfile.gameTime = oldProfile.gameTime;
    //            newProfile.saveCoords = oldProfile.saveCoords;
    //            newProfile.character = oldProfile.character;
    //            newProfile.items = oldProfile.items;
    //            newProfile.weapon = oldProfile.weapon;
    //            newProfile.bossStates = oldProfile.bossStates;
    //            newProfile.NPCVars = PlayState.NPCvarDefault;
    //            for (int j = 0; j < oldProfile.NPCVars.Length; j++)
    //                newProfile.NPCVars[j] = oldProfile.NPCVars[j];
    //            newProfile.percentage = oldProfile.percentage;
    //            newProfile.exploredMap = oldProfile.exploredMap;
    //        }
    //        switch (i)
    //        {
    //            case 1:
    //                PlayState.gameData.profile1 = newProfile;
    //                break;
    //            case 2:
    //                PlayState.gameData.profile2 = newProfile;
    //                break;
    //            case 3:
    //                PlayState.gameData.profile3 = newProfile;
    //                break;
    //        }
    //    }
    //
    //    int[] newAchievementStates = PlayState.achievementDefault;
    //    for (int i = 0; i < tempDataSlot.records.achievements.Length; i++)
    //        newAchievementStates[i] = tempDataSlot.records.achievements[i];
    //    PlayState.gameData.records.achievements = newAchievementStates;
    //
    //    float[][] newTimes = PlayState.timeDefault;
    //    for (int i = 0; i < tempDataSlot.records.times.Length; i++)
    //        newTimes[i] = tempDataSlot.records.times[i];
    //    PlayState.gameData.records.times = newTimes;
    //    //PlayState.LoadRecords();
    //    ClearOptions();
    //    AddOption(PlayState.GetText("menu_option_import_success"), false);
    //    AddOption("", false);
    //    AddOption(PlayState.GetText("menu_option_import_success_confirm"), true, ImportExportData);
    //    ForceSelect(2);
    //    backPage = ImportExportData;
    //}

    //public void ClearTempAndReturn()
    //{
    //    tempDataSlot = new PlayState.CollectiveData();
    //    ImportExportData();
    //}

    public void SaveOptions()
    {
        PlayState.WriteSave(0, true);
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

    public void RecordsScreen()
    {
        ClearOptions();
        ToggleAchievementInterface(false);
        if (PlayState.HasTime(PlayState.TimeCategories.normal))
            AddOption(PlayState.GetText("menu_option_records_normal"), true, NormalTimes);
        if (PlayState.HasTime(PlayState.TimeCategories.insane))
            AddOption(PlayState.GetText("menu_option_records_insane"), true, InsaneTimes);
        if (PlayState.HasTime(PlayState.TimeCategories.hundo))
            AddOption(PlayState.GetText("menu_option_records_100"), true, HundoTimes);
        if (PlayState.HasTime(PlayState.TimeCategories.rush))
            AddOption(PlayState.GetText("menu_option_records_bossRush"), true, RushTimes);
        if (currentOptions.Count == 0)
            AddOption(PlayState.GetText("menu_option_records_noTimes"), false);
        AddOption("", false);
        if (PlayState.HasAchievemements())
            AddOption(PlayState.GetText("menu_option_records_achievements"), true, AchievementScreen, new int[] { 0, 0 });
        if (PlayState.HasTime())
            AddOption(PlayState.GetText("menu_option_records_gallery"), true, GalleryScreen);
        AddOption(PlayState.GetText("menu_option_main_returnTo"), true, PageMain);
        ForceSelect(0);
        backPage = PageMain;
    }

    public void NormalTimes()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_records_normal"), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.snailyNormal))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_snaily"),
                PlayState.GetTimeString(PlayState.TimeIndeces.snailyNormal)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.sluggyNormal))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_sluggy"),
                PlayState.GetTimeString(PlayState.TimeIndeces.sluggyNormal)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.upsideNormal))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_upside"),
                PlayState.GetTimeString(PlayState.TimeIndeces.upsideNormal)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.leggyNormal))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_leggy"),
                PlayState.GetTimeString(PlayState.TimeIndeces.leggyNormal)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.blobbyNormal))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_blobby"),
                PlayState.GetTimeString(PlayState.TimeIndeces.blobbyNormal)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.leechyNormal))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_leechy"),
                PlayState.GetTimeString(PlayState.TimeIndeces.leechyNormal)), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_records_returnTo"), true, RecordsScreen);
        ForceSelect(currentOptions.Count - 1);
        backPage = RecordsScreen;
    }

    public void InsaneTimes()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_records_insane"), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.snailyInsane))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_snaily"),
                PlayState.GetTimeString(PlayState.TimeIndeces.snailyInsane)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.sluggyInsane))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_sluggy"),
                PlayState.GetTimeString(PlayState.TimeIndeces.sluggyInsane)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.upsideInsane))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_upside"),
                PlayState.GetTimeString(PlayState.TimeIndeces.upsideInsane)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.leggyInsane))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_leggy"),
                PlayState.GetTimeString(PlayState.TimeIndeces.leggyInsane)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.blobbyInsane))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_blobby"),
                PlayState.GetTimeString(PlayState.TimeIndeces.blobbyInsane)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.leechyInsane))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_leechy"),
                PlayState.GetTimeString(PlayState.TimeIndeces.leechyInsane)), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_records_returnTo"), true, RecordsScreen);
        ForceSelect(currentOptions.Count - 1);
        backPage = RecordsScreen;
    }

    public void HundoTimes()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_records_100"), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.snaily100))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_snaily"),
                PlayState.GetTimeString(PlayState.TimeIndeces.snaily100)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.sluggy100))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_sluggy"),
                PlayState.GetTimeString(PlayState.TimeIndeces.sluggy100)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.upside100))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_upside"),
                PlayState.GetTimeString(PlayState.TimeIndeces.upside100)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.leggy100))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_leggy"),
                PlayState.GetTimeString(PlayState.TimeIndeces.leggy100)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.blobby100))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_blobby"),
                PlayState.GetTimeString(PlayState.TimeIndeces.blobby100)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.leechy100))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_leechy"),
                PlayState.GetTimeString(PlayState.TimeIndeces.leechy100)), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_records_returnTo"), true, RecordsScreen);
        ForceSelect(currentOptions.Count - 1);
        backPage = RecordsScreen;
    }

    public void RushTimes()
    {
        ClearOptions();
        AddOption(PlayState.GetText("menu_option_records_bossRush"), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.snailyRush))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_snaily"),
                PlayState.GetTimeString(PlayState.TimeIndeces.snailyRush)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.sluggyRush))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_sluggy"),
                PlayState.GetTimeString(PlayState.TimeIndeces.sluggyRush)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.upsideRush))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_upside"),
                PlayState.GetTimeString(PlayState.TimeIndeces.upsideRush)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.leggyRush))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_leggy"),
                PlayState.GetTimeString(PlayState.TimeIndeces.leggyRush)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.blobbyRush))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_blobby"),
                PlayState.GetTimeString(PlayState.TimeIndeces.blobbyRush)), false);
        if (PlayState.HasTime(PlayState.TimeIndeces.leechyRush))
            AddOption(string.Format(PlayState.GetText("menu_option_records_time"), PlayState.GetText("char_leechy"),
                PlayState.GetTimeString(PlayState.TimeIndeces.leechyRush)), false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_records_returnTo"), true, RecordsScreen);
        ForceSelect(currentOptions.Count - 1);
        backPage = RecordsScreen;
    }

    public void AchievementScreen()
    {
        ClearOptions();
        int[] counts = ToggleAchievementInterface(true);
        AddOption(string.Format(PlayState.GetText("menu_option_achievements_progress"), counts[0], counts[1]), false);
        currentOptions[0].textScript.SetSize(1);
        AddOption("", false);
        AddOption("", false);
        AddOption("", false);
        if (PlayState.generalData.achievements[0])
        {
            AddOption(PlayState.GetText(string.Format("menu_option_achievements_{0}_title", achievements[0].ToLower())), true, "achievements");
            AddOption(PlayState.GetText(string.Format("menu_option_achievements_{0}_desc", achievements[0].ToLower())), false);
        }
        else
        {
            AddOption(PlayState.GetText("menu_option_achievements_locked_title"), true, "achievements");
            AddOption(PlayState.GetText("menu_option_achievements_locked_desc"), false);
        }
        currentOptions[5].textScript.SetSize(1);
        AddOption(PlayState.GetText("menu_option_records_returnTo"), true, RecordsScreen);
        ForceSelect(4);
        backPage = RecordsScreen;
    }

    public void GalleryScreen()
    {
        ClearOptions();
        if (PlayState.generalData.achievements[3])
            AddOption(PlayState.GetText("menu_option_gallery_normal"), true, GalleryNormal);
        else
            AddOption("-", false);
        if (PlayState.generalData.achievements[14])
            AddOption(PlayState.GetText("menu_option_gallery_bossRush"), true, GalleryRush);
        else
            AddOption("-", false);
        if (PlayState.generalData.achievements[5])
            AddOption(PlayState.GetText("menu_option_gallery_100"), true, Gallery100);
        else
            AddOption("-", false);
        if (PlayState.generalData.achievements[13])
            AddOption(PlayState.GetText("menu_option_gallery_lessThan30"), true, GallerySub30);
        else
            AddOption("-", false);
        if (PlayState.generalData.achievements[22])
            AddOption(PlayState.GetText("menu_option_gallery_insane"), true, GalleryInsane);
        else
            AddOption("-", false);
        AddOption("", false);
        AddOption(PlayState.GetText("menu_option_records_returnTo"), true, RecordsScreen);
        ForceSelect(0);
        backPage = RecordsScreen;
    }

    public void GalleryNormal()
    {
        galleryBG.anim.Play("Ending_background");
        galleryImage.anim.Play("Ending_normal");
        viewingGallery = true;
    }

    public void GalleryRush()
    {
        galleryBG.anim.Play("Ending_background");
        galleryImage.anim.Play("Ending_bossRush");
        viewingGallery = true;
    }

    public void Gallery100()
    {
        galleryBG.anim.Play("Ending_background");
        galleryImage.anim.Play("Ending_100");
        viewingGallery = true;
    }

    public void GallerySub30()
    {
        galleryBG.anim.Play("Ending_background");
        galleryImage.anim.Play("Ending_sub30");
        viewingGallery = true;
    }

    public void GalleryInsane()
    {
        galleryBG.anim.Play("Ending_background");
        galleryImage.anim.Play("Ending_insane");
        viewingGallery = true;
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
        PlayState.SaveAll();
        ReturnToMenu();
    }

    public void ReturnToMenu()
    {
        PlayState.gameState = PlayState.GameState.menu;
        PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 0, 0.5f, 0, 0);
        cam.position = panPoints[0];
        PageMain();
        PlayState.player.GetComponent<BoxCollider2D>().enabled = false;
        PlayState.globalFunctions.StopMusic();
        PlayState.ResetAllParticles();

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
        PlayState.currentProfileNumber = 0;
        PlayState.currentProfile = PlayState.blankProfile;

        PlayState.titleRoom.RemoteActivateRoom();

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
