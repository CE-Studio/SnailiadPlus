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
        public bool selectable;
        public DestinationDelegate destinationPage;
        public GameObject optionObject;
        public TextMesh[] textParts;
        public int[] menuParam;
    }

    private List<MenuOption> currentOptions = new List<MenuOption>();
    private DestinationDelegate backPage;
    private int[] menuVarFlags = new int[] { 0, 0, 0, 0, 0, 0 };
    private int controlScreen = 0;
    private bool isRebinding = false;
    private bool pauseButtonDown = false;

    private const float LIST_CENTER_Y = -2.5f;
    private const float LIST_OPTION_SPACING = 1.25f;
    private float currentSpawnY = LIST_CENTER_Y;
    private const float SELECT_SNAIL_VERTICAL_OFFSET = 0.625f;
    private const float LETTER_SPAWN_Y = 5;
    private const float LETTER_SPAWN_TIME = 0.25f;

    private int selectedOption = 0;
    private float selectSnailOffset = 0;

    public Transform cam;
    public Vector2[] panPoints = new Vector2[] // Points in world space that the main menu camera should pan over; set only one point for a static cam
    {
        new Vector2(0.5f, 0.5f)
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
    public AudioSource sfx;
    public AudioClip beep1;
    public AudioClip beep2;
    public GameObject textObject;
    public GameObject titleLetter;
    public GameObject[] selector;

    public GameObject[] menuHUDElements;

    public readonly int[] letterPixelWidths = new int[]
    {
        28, 28, 24, 28, 24, 24, 28, 24, 6, 24, 24, 6, 32, 24, 28, 28, 28, 24, 25, 24, 28, 24, 32, 32, 28, 24, 12
    //  A   B   C   D   E   F   G   H   I  J   K   L  M   N   O   P   Q   R   S   T   U   V   W   X   Y   Z
    };

    [Serializable]
    public struct CollectiveData
    {
        public PlayState.GameSaveData profile1;
        public PlayState.GameSaveData profile2;
        public PlayState.GameSaveData profile3;
        public PlayState.OptionData options;
        public PlayState.ControlData controls;
        public PlayState.RecordData records;
    }

    void Start()
    {
        PlayState.LoadOptions();
        PlayState.LoadControls();
        Screen.SetResolution(400 * (PlayState.gameOptions[2] + 1), 240 * (PlayState.gameOptions[2] + 1), false);

        //PlayState.LoadRecords();

        PlayState.player.GetComponent<BoxCollider2D>().enabled = false;
        cam = PlayState.cam.transform;
        music = GetComponent<AudioSource>();
        selector = new GameObject[]
        {
            GameObject.Find("Selection Pointer"),
            GameObject.Find("Selection Pointer/Left Snaily"),
            GameObject.Find("Selection Pointer/Right Snaily")
        };
        sfx = selector[0].GetComponent<AudioSource>();
        beep1 = (AudioClip)Resources.Load("Sounds/Sfx/MenuBeep1");
        beep2 = (AudioClip)Resources.Load("Sounds/Sfx/MenuBeep2");

        menuHUDElements = new GameObject[]
        {
            selector[0],
            GameObject.Find("Version Text")
        };

        menuHUDElements[1].transform.GetChild(0).GetComponent<TextMesh>().text = "Version " + Application.version;
        menuHUDElements[1].transform.GetChild(1).GetComponent<TextMesh>().text = "Version " + Application.version;

        StartCoroutine(nameof(CreateTitle));
    }

    void Update()
    {
        if ((PlayState.gameState == "Menu" || PlayState.gameState == "Pause") && !PlayState.isMenuOpen)
        {
            if (PlayState.gameState == "Menu")
            {
                music.Play();
                music.volume = PlayState.gameOptions[1] * 0.1f;
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
                cam.transform.position = panPoints[0];
        }
        if (PlayState.gameState == "Menu" || PlayState.gameState == "Pause")
        {
            sfx.volume = PlayState.gameOptions[0] * 0.1f;
            music.volume = PlayState.gameOptions[1] * 0.1f;

            if (!isRebinding)
            {
                if (Control.UpPress() || Control.DownPress())
                {
                    bool nextDown = Control.AxisY() == -1;
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
                        sfx.PlayOneShot(beep1);
                    selectedOption = intendedSelection;
                    GetNewSnailOffset();
                }

                if (Control.Pause())
                {
                    if (backPage != null)
                    {
                        backPage();
                        if (sfx.gameObject.activeInHierarchy)
                            sfx.PlayOneShot(beep2);
                    }
                }
                else if (Control.JumpPress())
                {
                    if (currentOptions[selectedOption].menuParam != null)
                    {
                        for (int i = 0; i < currentOptions[selectedOption].menuParam.Length; i += 2)
                            menuVarFlags[currentOptions[selectedOption].menuParam[i]] = currentOptions[selectedOption].menuParam[i + 1];
                    }
                    if (currentOptions[selectedOption].destinationPage != null)
                    {
                        currentOptions[selectedOption].destinationPage();
                        if (sfx.gameObject.activeInHierarchy)
                            sfx.PlayOneShot(beep2);
                    }
                }
            }

            foreach (MenuOption option in currentOptions)
            {
                switch (option.optionText)
                {
                    default:
                        break;
                    case "Difficulty: ":
                        TestForArrowAdjust(option, 0, PlayState.achievementStates[14] == 1 ? 2 : 1);
                        switch (menuVarFlags[0])
                        {
                            case 0:
                                AddToOptionText(option, "Easy");
                                break;
                            case 1:
                                AddToOptionText(option, "Normal");
                                break;
                            case 2:
                                AddToOptionText(option, "Insane");
                                break;
                        }
                        break;
                    case "Character: ":
                        TestForArrowAdjust(option, 1, 5);
                        switch (menuVarFlags[1])
                        {
                            case 0:
                                AddToOptionText(option, "Snaily");
                                break;
                            case 1:
                                AddToOptionText(option, "Sluggy");
                                break;
                            case 2:
                                AddToOptionText(option, "Upside");
                                break;
                            case 3:
                                AddToOptionText(option, "Leggy");
                                break;
                            case 4:
                                AddToOptionText(option, "Blobby");
                                break;
                            case 5:
                                AddToOptionText(option, "Leechy");
                                break;
                        }
                        break;
                    case "Randomized: ":
                        TestForArrowAdjust(option, 2, 1);
                        switch (menuVarFlags[2])
                        {
                            case 0:
                                AddToOptionText(option, "No");
                                break;
                            case 1:
                                AddToOptionText(option, "Yes");
                                break;
                        }
                        break;
                    case "Shooting: ":
                        TestForArrowAdjust(option, 0, 1);
                        switch (menuVarFlags[0])
                        {
                            case 0:
                                AddToOptionText(option, "Normal");
                                break;
                            case 1:
                                AddToOptionText(option, "Toggle");
                                break;
                        }
                        PlayState.gameOptions[8] = menuVarFlags[0];
                        break;
                    case "Sound volume: ":
                        TestForArrowAdjust(option, 0, 10);
                        AddToOptionText(option, menuVarFlags[0].ToString());
                        PlayState.gameOptions[0] = menuVarFlags[0];
                        break;
                    case "Music volume: ":
                        TestForArrowAdjust(option, 1, 10);
                        AddToOptionText(option, menuVarFlags[1].ToString());
                        PlayState.gameOptions[1] = menuVarFlags[1];
                        break;
                    case "Window resolution: ":
                        TestForArrowAdjust(option, 0, 3);
                        switch (menuVarFlags[0])
                        {
                            case 0:
                                AddToOptionText(option, "1x");
                                Screen.SetResolution(400, 240, false);
                                break;
                            case 1:
                                AddToOptionText(option, "2x");
                                Screen.SetResolution(800, 480, false);
                                break;
                            case 2:
                                AddToOptionText(option, "3x");
                                Screen.SetResolution(1200, 720, false);
                                break;
                            case 3:
                                AddToOptionText(option, "4x");
                                Screen.SetResolution(1600, 960, false);
                                break;
                        }
                        PlayState.gameOptions[2] = menuVarFlags[0];
                        break;
                    case "Minimap display: ":
                        TestForArrowAdjust(option, 1, 2);
                        switch (menuVarFlags[1])
                        {
                            case 0:
                                AddToOptionText(option, "hide");
                                break;
                            case 1:
                                AddToOptionText(option, "only map");
                                break;
                            case 2:
                                AddToOptionText(option, "show");
                                break;
                        }
                        PlayState.gameOptions[3] = menuVarFlags[1];
                        break;
                    case "Bottom keys: ":
                        TestForArrowAdjust(option, 2, 1);
                        switch (menuVarFlags[2])
                        {
                            case 0:
                                AddToOptionText(option, "hide");
                                break;
                            case 1:
                                AddToOptionText(option, "show");
                                break;
                        }
                        PlayState.gameOptions[4] = menuVarFlags[2];
                        break;
                    case "Reactive key displays: ":
                        TestForArrowAdjust(option, 3, 1);
                        switch (menuVarFlags[3])
                        {
                            case 0:
                                AddToOptionText(option, "hide");
                                break;
                            case 1:
                                AddToOptionText(option, "show");
                                break;
                        }
                        PlayState.gameOptions[5] = menuVarFlags[3];
                        break;
                    case "Game time: ":
                        TestForArrowAdjust(option, 4, 1);
                        switch (menuVarFlags[4])
                        {
                            case 0:
                                AddToOptionText(option, "hide");
                                break;
                            case 1:
                                AddToOptionText(option, "show");
                                break;
                        }
                        PlayState.gameOptions[6] = menuVarFlags[4];
                        break;
                    case "FPS counter: ":
                        TestForArrowAdjust(option, 5, 1);
                        switch (menuVarFlags[5])
                        {
                            case 0:
                                AddToOptionText(option, "hide");
                                break;
                            case 1:
                                AddToOptionText(option, "show");
                                break;
                        }
                        PlayState.gameOptions[7] = menuVarFlags[5];
                        break;
                    case "Jump:   ":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(12) : Control.ParseKeyName(4));
                        break;
                    case "Shoot:   ":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(13) : Control.ParseKeyName(5));
                        break;
                    case "Strafe:   ":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(14) : Control.ParseKeyName(6));
                        break;
                    case "Speak:   ":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(15) : Control.ParseKeyName(7));
                        break;
                    case "Move up:   ":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(10) : Control.ParseKeyName(2));
                        break;
                    case "Move left:   ":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(8) : Control.ParseKeyName(0));
                        break;
                    case "Move right:   ":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(9) : Control.ParseKeyName(1));
                        break;
                    case "Move down:   ":
                        if (!isRebinding)
                            AddToOptionText(option, controlScreen == 2 ? Control.ParseKeyName(11) : Control.ParseKeyName(3));
                        break;
                    case "Weapon one:   ":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseKeyName(16));
                        break;
                    case "Weapon two:   ":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseKeyName(17));
                        break;
                    case "Weapon three:   ":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseKeyName(18));
                        break;
                    case "Next weapon:   ":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseKeyName(19));
                        break;
                    case "Prev weapon:   ":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseKeyName(20));
                        break;
                    case "Open minimap:   ":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseKeyName(21));
                        break;
                    case "Open menu:   ":
                        if (!isRebinding)
                            AddToOptionText(option, Control.ParseKeyName(22));
                        break;
                    case "Select a slot: ":
                        TestForArrowAdjust(option, 0, 9);
                        AddToOptionText(option, (menuVarFlags[0] + 1).ToString() +
                            (File.Exists(Application.persistentDataPath + "/SnailySave_" + (menuVarFlags[0] + 1) + ".json") ? " (full)" : " (empty)"));
                        break;
                }
            }
            GetNewSnailOffset();
        }

        if (PlayState.gameState != "Menu" && PlayState.gameState != "Pause")
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
                PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 75, 0.25f);
                PageMain();
                StartCoroutine(nameof(CreateTitle));
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
        switch (ID)
        {
            default:
            case 0:
                return "Snaily";
            case 1:
                return "Sluggy";
            case 2:
                return "Upside";
            case 3:
                return "Leggy";
            case 4:
                return "Blobby";
            case 5:
                return "Leechy";
        }
    }

    public void TestForArrowAdjust(MenuOption option, int varSlot, int max)
    {
        if (selectedOption == currentOptions.IndexOf(option))
        if (Control.LeftPress())
        {
            menuVarFlags[varSlot]--;
            if (menuVarFlags[varSlot] < 0)
                menuVarFlags[varSlot] = max;
            sfx.PlayOneShot(beep1);
        }
        else if (Control.RightPress())
        {
            menuVarFlags[varSlot]++;
            if (menuVarFlags[varSlot] > max)
                menuVarFlags[varSlot] = 0;
            sfx.PlayOneShot(beep1);
        }
    }

    public void TestForRebind()
    {
        StartCoroutine(RebindKey(menuVarFlags[0]));
    }

    public IEnumerator RebindKey(int controlID)
    {
        while (Control.JumpHold())
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
        option.textParts[0].text = option.optionText + text;
        option.textParts[1].text = option.optionText + text;
    }

    public void AddOption(string text = "", bool isSelectable = true, DestinationDelegate destination = null, int[] paramChange = null)
    {
        foreach (Transform entry in transform)
        {
            if (entry.name.Contains("Text Object"))
                entry.localPosition = new Vector2(0, entry.transform.localPosition.y + (LIST_OPTION_SPACING * 0.5f));
        }

        MenuOption option = new MenuOption();
        option.optionText = text;
        option.selectable = isSelectable;
        option.destinationPage = destination;

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
            if (!state && transform.GetChild(i).name.Contains("Title Letter"))
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
        time += (lessThanOne ? "00" : (lessThanTen ? "0" + seconds.ToString()[0] : seconds.ToString().Substring(0, 2))) + "." +
            seconds.ToString().Substring(lessThanOne ? 0 : (lessThanTen ? 1 : 2), 2);
        return time;
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

    public IEnumerator CreateTitle()
    {
        string title = "Snailiad";
        int titleLength = 0;
        for (int i = 0; i < title.Length; i++)
        {
            titleLength += letterPixelWidths[LetterToNumber(title[i])];
            if (i != title.Length - 1)
                titleLength += 4;
        }
        float letterSpawnX = (-(titleLength * 0.5f) + (letterPixelWidths[LetterToNumber(title[0])] * 0.5f)) * 0.0625f + 0.25f;
        float timer = LETTER_SPAWN_TIME;
        int letterID = 0;
        bool doneSpawning = false;

        while (!doneSpawning && PlayState.gameState != "Game")
        {
            if (timer >= LETTER_SPAWN_TIME && letterID < title.Length)
            {
                GameObject newLetter = Instantiate(titleLetter);
                newLetter.transform.parent = transform;
                newLetter.transform.localPosition = new Vector2(letterSpawnX, LETTER_SPAWN_Y);
                TitleLetter letterScript = newLetter.GetComponent<TitleLetter>();
                letterScript.SetLetter(title[letterID]);
                letterScript.localFinalPos = newLetter.transform.localPosition;
                letterScript.readyToAnimate = true;
                timer -= LETTER_SPAWN_TIME;
                letterID++;
                if (letterID >= title.Length)
                    doneSpawning = true;
                else
                    letterSpawnX += (letterPixelWidths[LetterToNumber(title[letterID - 1])] + 4) * 0.0625f;
            }
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    public int LetterToNumber(char letter)
    {
        switch (char.ToLower(letter))
        {
            default:
            case 'a':
                return 0;
            case 'b':
                return 1;
            case 'c':
                return 2;
            case 'd':
                return 3;
            case 'e':
                return 4;
            case 'f':
                return 5;
            case 'g':
                return 6;
            case 'h':
                return 7;
            case 'i':
                return 8;
            case 'j':
                return 9;
            case 'k':
                return 10;
            case 'l':
                return 11;
            case 'm':
                return 12;
            case 'n':
                return 13;
            case 'o':
                return 14;
            case 'p':
                return 15;
            case 'q':
                return 16;
            case 'r':
                return 17;
            case 's':
                return 18;
            case 't':
                return 19;
            case 'u':
                return 20;
            case 'v':
                return 21;
            case 'w':
                return 22;
            case 'x':
                return 23;
            case 'y':
                return 24;
            case 'z':
                return 25;
            case ' ':
                return 26;
        }
    }

    public void PageMain()
    {
        ClearOptions();
        bool returnAvailable = false;
        if (PlayState.gameState == "Pause")
        {
            AddOption("Return to game", true, Unpause);
            returnAvailable = true;
        }
        //AddOption("New game", true);
        //AddOption("Load game", true);
        //AddOption("Boss rush", true);
        //AddOption("", false);
        //AddOption("Options", true);
        //AddOption("Credits", true);
        //AddOption("Records", true);
        //AddOption("Gallery", true);
        AddOption("Select profile", true, ProfileScreen);
        if (PlayState.achievementStates[6] == 1)
            AddOption("Boss rush", true);
        AddOption("Multiplayer", true);
        AddOption("", false);
        AddOption("Options", true, OptionsScreen, new int[] { 0, PlayState.gameOptions[8] });
        AddOption("Credits", true, CreditsPage1);
        if (PlayState.HasTime())
            AddOption("Records", true);
        if (returnAvailable)
        {
            AddOption("Back to main menu", true, MenuReturnConfirm);
            backPage = Unpause;
        }
        else
        {
            AddOption("Quit", true, QuitConfirm);
            backPage = QuitConfirm;
        }
        ForceSelect(0);
    }

    public void ProfileScreen()
    {
        ClearOptions();
        AddOption("Select a profile", false);
        for (int i = 1; i <= 3; i++)
        {
            PlayState.GameSaveData data = PlayState.LoadGame(i);
            if (data.profile != -1)
                AddOption(i + " / " + data.character + " / " + ConvertTimeToString(data.gameTime) + " / " + data.percentage + "%", true, PickSpawn, new int[] { 0, i });
            else
                AddOption("Empty profile", true, StartNewGame, new int[] { 0, 1, 1, 0, 2, 0, 3, i });
        }
        AddOption("", false);
        AddOption("Copy game", true, CopyData);
        AddOption("Erase game", true, EraseData);
        AddOption("Back to main menu", true, PageMain);
        ForceSelect(1);
        backPage = PageMain;
    }

    public void StartNewGame()
    {
        ClearOptions();
        AddOption("Game options", false);
        AddOption("", false);
        AddOption("Difficulty: ", true);
        if (PlayState.achievementStates[14] == 1)
        {
            AddOption("Character: ", true);
            AddOption("Randomized: ", true);
        }
        AddOption("", false);
        AddOption("Start new game", true, StartNewSave);
        AddOption("Back to profile selection", true, ProfileScreen);
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

        PlayState.player.transform.position = PlayState.WORLD_SPAWN;
        PlayState.gameState = "Game";
        PlayState.player.GetComponent<BoxCollider2D>().enabled = true;
        PlayState.ToggleHUD(true);
    }

    public void PickSpawn()
    {
        ClearOptions();
        AddOption("Okay, where would you", false);
        AddOption("like to start from?", false);
        AddOption("", false);
        AddOption("Start from save point", true, LoadAndSpawn, new int[] { 1, 0 });
        AddOption("Start from Snail Town", true, LoadAndSpawn, new int[] { 1, 1 });
        AddOption("", false);
        AddOption("Back to profile selection", true, ProfileScreen);
        ForceSelect(3);
        backPage = ProfileScreen;
    }

    public void LoadAndSpawn()
    {
        PlayState.player.GetComponent<BoxCollider2D>().enabled = false;
        PlayState.LoadGame(menuVarFlags[0], true);
        PlayState.player.transform.position = menuVarFlags[1] == 1 ? PlayState.WORLD_SPAWN : PlayState.respawnCoords;

        if (PlayState.gameState == "Pause")
        {
            Transform lastRoom = PlayState.roomTriggerParent.transform.GetChild((int)PlayState.positionOfLastRoom.x).GetChild((int)PlayState.positionOfLastRoom.y);
            lastRoom.GetComponent<Collider2D>().enabled = true;
            lastRoom.GetComponent<RoomTrigger>().active = true;
            lastRoom.GetComponent<RoomTrigger>().DespawnEverything();
        }

        PlayState.gameState = "Game";
        PlayState.player.GetComponent<BoxCollider2D>().enabled = true;
        PlayState.ToggleHUD(true);

        PlayState.player.GetComponent<Snaily>().enabled = false;
        //PlayState.player.GetComponent<Sluggy>().enabled = false;
        //PlayState.player.GetComponent<Upside>().enabled = false;
        //PlayState.player.GetComponent<Leggy>().enabled = false;
        //PlayState.player.GetComponent<Blobby>().enabled = false;
        //PlayState.player.GetComponent<Leechy>().enabled = false;
        switch (PlayState.currentCharacter)
        {
            default:
            case "Snaily":
                PlayState.player.GetComponent<Snaily>().enabled = true;
                PlayState.player.GetComponent<Snaily>().holdingJump = true;
                break;
            //case "Sluggy":
            //    PlayState.player.GetComponent<Sluggy>().enabled = true;
            //    PlayState.player.GetComponent<Sluggy>().holdingJump = true;
            //    break;
            //case "Snaily":
            //    PlayState.player.GetComponent<Upside>().enabled = true;
            //    PlayState.player.GetComponent<Upside>().holdingJump = true;
            //    break;
            //case "Snaily":
            //    PlayState.player.GetComponent<Leggy>().enabled = true;
            //    PlayState.player.GetComponent<Leggy>().holdingJump = true;
            //    break;
            //case "Snaily":
            //    PlayState.player.GetComponent<Blobby>().enabled = true;
            //    PlayState.player.GetComponent<Blobby>().holdingJump = true;
            //    break;
            //case "Snaily":
            //    PlayState.player.GetComponent<Leechy>().enabled = true;
            //    PlayState.player.GetComponent<Leechy>().holdingJump = true;
            //    break;
        }
    }

    public void Unpause()
    {
        PlayState.gameState = "Game";
        PlayState.ToggleHUD(true);
        ToggleHUD(false);
        pauseButtonDown = true;

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
        AddOption("Okay, copy from which profile?", false);
        for (int i = 1; i <= 3; i++)
        {
            PlayState.GameSaveData data = PlayState.LoadGame(i);
            if (data.profile != -1)
                AddOption(i + " / " + data.character + " / " + ConvertTimeToString(data.gameTime) + " / " + data.percentage + "%", true, CopyData2, new int[] { 0, i });
            else
                AddOption("Empty profile", false);
        }
        AddOption("", false);
        AddOption("Cancel", true, ProfileScreen);
        AddOption("", false);
        AddOption("Back to main menu", true, PageMain);
        ForceSelect(5);
        backPage = ProfileScreen;
    }

    public void CopyData2()
    {
        ClearOptions();
        AddOption("Copy to which profile?", false);
        for (int i = 1; i <= 3; i++)
        {
            PlayState.GameSaveData data = PlayState.LoadGame(i);
            if (data.profile != -1)
                AddOption((menuVarFlags[0] == i ? "> " : "") + i + " / " + data.character + " / " + ConvertTimeToString(data.gameTime) + " / " + data.percentage + "%" +
                    (menuVarFlags[0] == i ? " <" : ""), menuVarFlags[0] != i, CopyConfirm, new int[] { 1, i });
            else
                AddOption("Empty profile", true, CopyConfirm, new int[] { 1, i });
        }
        AddOption("", false);
        AddOption("Cancel", true, ProfileScreen);
        AddOption("", false);
        AddOption("Back to main menu", true, PageMain);
        ForceSelect(5);
        backPage = ProfileScreen;
    }

    public void CopyConfirm()
    {
        bool isChosenSlotEmpty = PlayState.LoadGame(menuVarFlags[1]).profile == -1;
        ClearOptions();
        AddOption("Copy file " + menuVarFlags[0] + " to " + (isChosenSlotEmpty ? "empty" : "used") + " slot " + menuVarFlags[1] + "?", false);
        AddOption("", false);
        AddOption("Yes, copy it!", true, ActuallyCopyData);
        AddOption("No, I changed my mind", true, ProfileScreen);
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
        AddOption("Okay, erase which profile?", false);
        for (int i = 1; i <= 3; i++)
        {
            PlayState.GameSaveData data = PlayState.LoadGame(i);
            if (data.profile != -1)
            {
                string time = data.gameTime[0] + ":";
                if (data.gameTime[1] < 10)
                    time += "0";
                time += data.gameTime[1] + ":";
                if (data.gameTime[2] < 10)
                    time += "0";
                string seconds = Mathf.RoundToInt(data.gameTime[2] * 100).ToString();
                if (seconds.Length == 4)
                    time += seconds.Substring(0, 1);
                else
                    time += seconds[0];
                time += "." + seconds.Substring(seconds.Length == 4 ? 2 : 1, seconds.Length == 4 ? 3 : 2);
                AddOption(i + " / " + data.character + " / " + time + " / " + data.percentage + "%", PlayState.currentProfile != i, ConfirmErase, new int[] { 0, i });
            }
            else
                AddOption("Empty profile", false);
        }
        AddOption("", false);
        AddOption("Cancel", true, ProfileScreen);
        AddOption("", false);
        AddOption("Back to main menu", true, PageMain);
        ForceSelect(5);
        backPage = ProfileScreen;
    }

    public void ConfirmErase()
    {
        ClearOptions();
        AddOption("Really erase file " + menuVarFlags[0] + "?", false);
        AddOption("", false);
        AddOption("Yes, I want to start over!", true, ActuallyEraseData);
        AddOption("No way, I like my game!", true, ProfileScreen);
        ForceSelect(3);
        backPage = ProfileScreen;
    }

    public void ActuallyEraseData()
    {
        PlayerPrefs.DeleteKey("SaveGameData" + menuVarFlags[0]);
        ProfileScreen();
    }

    public void OptionsScreen()
    {
        ClearOptions();
        menuVarFlags[0] = PlayState.gameOptions[8];
        AddOption("Sound options", true, SoundOptions, new int[] { 0, PlayState.gameOptions[0], 1, PlayState.gameOptions[1] });
        AddOption("Display options", true, DisplayOptions, new int[]
        {
            0, PlayState.gameOptions[2], 1, PlayState.gameOptions[3],
            2, PlayState.gameOptions[4], 3, PlayState.gameOptions[5],
            4, PlayState.gameOptions[6], 5, PlayState.gameOptions[7]
        });
        AddOption("Set controls", true, ControlMain);
        AddOption("Shooting: ", true);
        AddOption("Texture/Music packs", true);
        AddOption("Erase record data", true, RecordEraseConfirm);
        if (PlayState.gameState == "Menu")
            AddOption("Import/export all data", true, ImportExportData);
        AddOption("", false);
        AddOption("Back to main menu", true, PageMain);
        ForceSelect(0);
        backPage = PageMain;
    }

    public void SoundOptions()
    {
        ClearOptions();
        AddOption("Sound volume: ", true);
        AddOption("Music volume: ", true);
        AddOption("", false);
        AddOption("Back to options", true, SaveOptions);
        ForceSelect(0);
        backPage = SaveOptions;
    }

    public void DisplayOptions()
    {
        ClearOptions();
        AddOption("Window resolution: ", true);
        AddOption("Minimap display: ", true);
        AddOption("Bottom keys: ");
        AddOption("Reactive key displays: ", true);
        AddOption("Game time: ", true);
        AddOption("FPS counter: ", true);
        AddOption("", false);
        AddOption("Back to options", true, SaveOptions);
        ForceSelect(0);
        backPage = SaveOptions;
    }

    public void ControlMain()
    {
        ClearOptions();
        AddOption("Control set 1", true, Controls1);
        AddOption("Control set 2", true, Controls2);
        AddOption("Miscellaneous", true, Controls3);
        AddOption("", false);
        AddOption("Reset all to defaults", true, ResetControls);
        AddOption("", false);
        AddOption("Back to options", true, SaveControls);
        ForceSelect(0);
        backPage = SaveControls;
    }

    public void Controls1()
    {
        ClearOptions();
        controlScreen = 1;
        AddOption("Jump:   ", true, TestForRebind, new int[] { 0, 4 });
        AddOption("Shoot:   ", true, TestForRebind, new int[] { 0, 5 });
        AddOption("Strafe:   ", true, TestForRebind, new int[] { 0, 6 });
        AddOption("Speak:   ", true, TestForRebind, new int[] { 0, 7 });
        AddOption("Move up:   ", true, TestForRebind, new int[] { 0, 2 });
        AddOption("Move left:   ", true, TestForRebind, new int[] { 0, 0 });
        AddOption("Move down:   ", true, TestForRebind, new int[] { 0, 3 });
        AddOption("Move right:   ", true, TestForRebind, new int[] { 0, 1 });
        AddOption("Back", true, ControlMain);
        ForceSelect(0);
        backPage = ControlMain;
    }

    public void Controls2()
    {
        ClearOptions();
        controlScreen = 2;
        AddOption("Jump:   ", true, TestForRebind, new int[] { 0, 12 });
        AddOption("Shoot:   ", true, TestForRebind, new int[] { 0, 13 });
        AddOption("Strafe:   ", true, TestForRebind, new int[] { 0, 14 });
        AddOption("Speak:   ", true, TestForRebind, new int[] { 0, 15 });
        AddOption("Move up:   ", true, TestForRebind, new int[] { 0, 10 });
        AddOption("Move left:   ", true, TestForRebind, new int[] { 0, 8 });
        AddOption("Move down:   ", true, TestForRebind, new int[] { 0, 11 });
        AddOption("Move right:   ", true, TestForRebind, new int[] { 0, 9 });
        AddOption("Back", true, ControlMain);
        ForceSelect(0);
        backPage = ControlMain;
    }

    public void Controls3()
    {
        ClearOptions();
        controlScreen = 3;
        AddOption("Weapon one:   ", true, TestForRebind, new int[] { 0, 16 });
        AddOption("Weapon two:   ", true, TestForRebind, new int[] { 0, 17 });
        AddOption("Weapon three:   ", true, TestForRebind, new int[] { 0, 18 });
        AddOption("Next weapon:   ", true, TestForRebind, new int[] { 0, 19 });
        AddOption("Prev weapon:   ", true, TestForRebind, new int[] { 0, 20 });
        AddOption("Open minimap:   ", true, TestForRebind, new int[] { 0, 21 });
        AddOption("Open menu:   ", true, TestForRebind, new int[] { 0, 22 });
        AddOption("", false);
        AddOption("Back", true, ControlMain);
        ForceSelect(0);
        backPage = ControlMain;
    }

    public void ResetControls()
    {
        Control.inputs = new KeyCode[]
        {
            KeyCode.LeftArrow,
            KeyCode.RightArrow,
            KeyCode.UpArrow,
            KeyCode.DownArrow,
            KeyCode.Z,
            KeyCode.X,
            KeyCode.C,
            KeyCode.C,
            KeyCode.A,
            KeyCode.D,
            KeyCode.W,
            KeyCode.S,
            KeyCode.K,
            KeyCode.J,
            KeyCode.H,
            KeyCode.H,
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Plus,
            KeyCode.Equals,
            KeyCode.M,
            KeyCode.Escape
        };
        SaveControls();
    }

    public void SaveControls()
    {
        PlayState.WriteSave("controls");
        controlScreen = 0;
        OptionsScreen();
    }

    public void RecordEraseConfirm()
    {
        ClearOptions();
        AddOption("Are you sure??", false);
        AddOption("", false);
        AddOption("Yes, erase everything!", true, EraseRecords);
        AddOption("No way, I like my game!", true, OptionsScreen);
        ForceSelect(3);
        backPage = OptionsScreen;
    }

    public void EraseRecords()
    {
        PlayerPrefs.DeleteKey("RecordData");
        OptionsScreen();
    }

    public void ImportExportData()
    {
        ClearOptions();
        AddOption("Export data to JSON", true, ExportSelect, new int[] { 0, 0 });
        AddOption("Import data from JSON", true, ImportSelect, new int[] { 0, 0 });
        AddOption("", false);
        AddOption("Back to options", true, OptionsScreen);
        ForceSelect(0);
        backPage = OptionsScreen;
    }

    public void ExportSelect()
    {
        ClearOptions();
        AddOption("Select a slot: ", true);
        AddOption("", false);
        AddOption("Confirm", true, ExportConfirm);
        AddOption("Back to options", true, OptionsScreen);
        ForceSelect(0);
        backPage = ImportExportData;
    }

    public void ExportConfirm()
    {
        ClearOptions();
        bool fileFound = false;
        if (File.Exists(Application.persistentDataPath + "/SnailySave_" + (menuVarFlags[0] + 1) + ".json"))
        {
            fileFound = true;
            AddOption("Overwrite JSON save in slot " + (menuVarFlags[0] + 1) + "?", false);
        }
        else
        {
            AddOption("Write all saved data to a", false);
            AddOption("JSON file in slot " + (menuVarFlags[0] + 1) + "?", false);
        }
        AddOption("", false);
        AddOption("Yeah, write it!", true, WriteDataToFile);
        AddOption("No, I changed my mind", true, ImportExportData);
        ForceSelect(fileFound ? 4 : 3);
        backPage = ImportExportData;
    }

    public void WriteDataToFile()
    {
        string dataPath = Application.persistentDataPath + "/SnailySave_" + (menuVarFlags[0] + 1) + ".json";

        CollectiveData fullData = new CollectiveData();

        fullData.profile1 = PlayState.LoadGame(1);
        fullData.profile2 = PlayState.LoadGame(2);
        fullData.profile3 = PlayState.LoadGame(3);

        PlayState.OptionData optionDataForCollective = new PlayState.OptionData();
        optionDataForCollective.options = PlayState.gameOptions;
        fullData.options = optionDataForCollective;

        PlayState.ControlData controlDataForCollective = new PlayState.ControlData();
        controlDataForCollective.controls = Control.inputs;
        fullData.controls = controlDataForCollective;

        PlayState.RecordData recordDataForCollective = new PlayState.RecordData();
        recordDataForCollective.achievements = PlayState.achievementStates;
        recordDataForCollective.times = PlayState.savedTimes;
        fullData.records = recordDataForCollective;

        File.WriteAllText(dataPath, JsonUtility.ToJson(fullData));

        ClearOptions();
        AddOption("Success! Your new JSON", false);
        AddOption("can be found at:", false);

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
        AddOption("Yay!!", true, ImportExportData);
        ForceSelect(currentOptions.Count - 1);
        backPage = ImportExportData;
    }

    public void ImportSelect()
    {
        ClearOptions();
        AddOption("Select a slot: ", true);
        AddOption("", false);
        AddOption("Confirm", true, ImportConfirm);
        AddOption("Back to options", true, OptionsScreen);
        ForceSelect(0);
        backPage = ImportExportData;
    }

    public void ImportConfirm()
    {
        ClearOptions();
        if (File.Exists(Application.persistentDataPath + "/SnailySave_" + (menuVarFlags[0] + 1) + ".json"))
        {
            AddOption("Import data from slot " + (menuVarFlags[0] + 1) + "? This will", false);
            AddOption("overwrite *ALL* existing data!!", false);
            AddOption("", false);
            AddOption("Yes, import away!", true, ReadDataFromFile);
            AddOption("No way! Let me keep my data!", true, ImportExportData);
            ForceSelect(4);
            backPage = ImportExportData;
        }
        else
        {
            AddOption("There\'s no data in slot " + (menuVarFlags[0] + 1) + "!", false);
            AddOption("", false);
            AddOption("Whoops!! Go back", true, ImportExportData);
            ForceSelect(2);
            backPage = ImportExportData;
        }
    }

    public void ReadDataFromFile()
    {
        string dataPath = Application.persistentDataPath + "/SnailySave_" + (menuVarFlags[0] + 1) + ".json";

        CollectiveData fullData = JsonUtility.FromJson<CollectiveData>(File.ReadAllText(dataPath));

        PlayerPrefs.SetString("SaveGameData1", JsonUtility.ToJson(fullData.profile1));
        PlayerPrefs.Save();
        PlayerPrefs.SetString("SaveGameData2", JsonUtility.ToJson(fullData.profile2));
        PlayerPrefs.Save();
        PlayerPrefs.SetString("SaveGameData3", JsonUtility.ToJson(fullData.profile2));
        PlayerPrefs.Save();
        PlayerPrefs.SetString("OptionData", JsonUtility.ToJson(fullData.options));
        PlayerPrefs.Save();
        PlayerPrefs.SetString("ControlData", JsonUtility.ToJson(fullData.controls));
        PlayerPrefs.Save();
        PlayerPrefs.SetString("RecordData", JsonUtility.ToJson(fullData.records));
        PlayerPrefs.Save();

        PlayState.LoadOptions();
        PlayState.LoadControls();
        //PlayState.LoadRecords();

        ClearOptions();
        AddOption("Success! Data has been loaded", false);
        AddOption("", false);
        AddOption("Awesome!", true, ImportExportData);
        ForceSelect(2);
        backPage = ImportExportData;
    }

    public void SaveOptions()
    {
        PlayState.WriteSave("options");
        OptionsScreen();
    }

    public void CreditsPage1()
    {
        ClearOptions();
        AddOption("Snailiad - A Snaily Game", false);
        AddOption("", false);
        AddOption("Original Flash version by", false);
        AddOption("Crystal Jacobs (Auriplane) and", false);
        AddOption("sponsored by Newgrounds", false);
        AddOption("", false);
        AddOption("Next page", true, CreditsPage2);
        ForceSelect(6);
        backPage = PageMain;
    }

    public void CreditsPage2()
    {
        ClearOptions();
        AddOption("Flash release special thanks", false);
        AddOption("", false);
        AddOption("Adamatomic (Flixel)", false);
        AddOption("Newstarshipsmell (Testing)", false);
        AddOption("xdanond (Additional art)", false);
        AddOption("", false);
        AddOption("Next page", true, CreditsPage3);
        ForceSelect(6);
        backPage = PageMain;
    }

    public void CreditsPage3()
    {
        ClearOptions();
        AddOption("Remake created by", false);
        AddOption("Epsilon the Dragon", false);
        AddOption("with Auriplane\'s permission", false);
        AddOption("under the Unity engine", false);
        AddOption("", false);
        AddOption("", false);
        AddOption("Next page", true, CreditsPage4);
        ForceSelect(6);
        backPage = PageMain;
    }

    public void CreditsPage4()
    {
        ClearOptions();
        AddOption("Remake special thanks", false);
        AddOption("", false);
        AddOption("Broomietunes (Additional songs", false);
        AddOption("and built-in music pack)", false);
        AddOption("NegativeBread (Built-in skin pack)", false);
        AddOption("", false);
        AddOption("Next page", true, CreditsPage5);
        ForceSelect(6);
        backPage = PageMain;
    }

    public void CreditsPage5()
    {
        ClearOptions();
        AddOption("Remake special thanks", false);
        AddOption("", false);
        AddOption("Nat the Chicken (QA, testing)", false);
        AddOption("Zettex (Extra characters)", false);
        AddOption("Minervo Ionni (Another character)", false);
        AddOption("", false);
        AddOption("Next page", true, CreditsPage6);
        ForceSelect(6);
        backPage = PageMain;
    }

    public void CreditsPage6()
    {
        ClearOptions();
        AddOption("Remake special thanks", false);
        AddOption("", false);
        AddOption("Clarence112, my boyfriend", false);
        AddOption("(Emotional support, superior code", false);
        AddOption("knowledge, code assistance)", false);
        AddOption("", false);
        AddOption("Next page", true, CreditsPage7);
        ForceSelect(6);
        backPage = PageMain;
    }

    public void CreditsPage7()
    {
        ClearOptions();
        AddOption("Remake special thanks", false);
        AddOption("", false);
        AddOption("The official Snaily Discord", false);
        AddOption("(Testing, feedback, ideas, and", false);
        AddOption("generally being super cool people)", false);
        AddOption("", false);
        AddOption("Back to main menu", true, PageMain);
        ForceSelect(6);
        backPage = PageMain;
    }

    public void MenuReturnConfirm()
    {
        ClearOptions();
        AddOption("Really return to menu?", false);
        AddOption("", false);
        AddOption("Save and quit", true, SaveQuit);
        AddOption("Quit without saving", true, ReturnToMenu);
        AddOption("Back to pause menu", true, PageMain);
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

        music.Play();
    }

    public void QuitConfirm()
    {
        ClearOptions();
        AddOption("Really quit?", false);
        AddOption("", false);
        AddOption("Yeah, I\'m done playing for now", true, QuitGame);
        AddOption("No way! I\'m not done yet!", true, PageMain);
        ForceSelect(3);
        backPage = PageMain;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
