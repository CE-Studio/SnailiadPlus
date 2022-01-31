using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private int[] menuVarFlags = new int[] { 0, 0, 0, 0, 0 };

    private const float LIST_CENTER_Y = -2.5f;
    private const float LIST_OPTION_SPACING = 1.25f;
    private float currentSpawnY = LIST_CENTER_Y;

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
    public GameObject[] selector;

    public GameObject[] menuHUDElements;

    void Start()
    {
        PlayState.LoadRecords();

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
            selector[0]
        };
    }

    void Update()
    {
        if ((PlayState.gameState == "Menu" || PlayState.gameState == "Pause") && !PlayState.isMenuOpen)
        {
            if (PlayState.gameState == "Menu")
            {
                music.Play();
                music.volume = 1;
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
                            Vector3 startPos = new Vector3(panPoints[currentPointInIndex].x, panPoints[currentPointInIndex].y, 0);
                            Vector3 endPos = new Vector3(panPoints[targetPointInIndex].x, panPoints[targetPointInIndex].y, 0);
                            cam.position = Vector3.Slerp(startPos, endPos, moveTimer);
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
        }
        if (PlayState.gameState == "Menu" || PlayState.gameState == "Pause")
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                bool nextDown = Input.GetAxisRaw("Vertical") == -1;
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

            selector[0].transform.localPosition = new Vector2(0,
                Mathf.Lerp(selector[0].transform.localPosition.y, currentOptions[selectedOption].optionObject.transform.localPosition.y + 0.55f, 15 * Time.deltaTime));
            selector[1].transform.localPosition = new Vector2(Mathf.Lerp(selector[1].transform.localPosition.x, -selectSnailOffset, 15 * Time.deltaTime), 0);
            selector[2].transform.localPosition = new Vector2(Mathf.Lerp(selector[2].transform.localPosition.x, selectSnailOffset, 15 * Time.deltaTime), 0);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (backPage != null)
                {
                    backPage();
                    sfx.PlayOneShot(beep2);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                if (currentOptions[selectedOption].menuParam != null)
                {
                    for (int i = 0; i < currentOptions[selectedOption].menuParam.Length; i += 2)
                        menuVarFlags[currentOptions[selectedOption].menuParam[i]] = currentOptions[selectedOption].menuParam[i + 1];
                }
                if (currentOptions[selectedOption].destinationPage != null)
                {
                    currentOptions[selectedOption].destinationPage();
                    sfx.PlayOneShot(beep2);
                }
            }

            foreach (MenuOption option in currentOptions)
            {
                switch (option.optionText)
                {
                    default:
                        break;
                    case "Difficulty: ":
                        if (selectedOption == currentOptions.IndexOf(option))
                        {
                            if (Input.GetKeyDown(KeyCode.LeftArrow))
                            {
                                menuVarFlags[0]--;
                                if (menuVarFlags[0] < 0)
                                    menuVarFlags[0] = PlayState.achievementStates[14] == 1 ? 2 : 1;
                                sfx.PlayOneShot(beep1);
                            }
                            else if (Input.GetKeyDown(KeyCode.RightArrow))
                            {
                                menuVarFlags[0]++;
                                if (menuVarFlags[0] > (PlayState.achievementStates[14] == 1 ? 2 : 1))
                                    menuVarFlags[0] = 0;
                                sfx.PlayOneShot(beep1);
                            }
                        }
                        switch (menuVarFlags[0])
                        {
                            case 0:
                                option.textParts[0].text = "Difficulty: Easy";
                                option.textParts[1].text = "Difficulty: Easy";
                                break;
                            case 1:
                                option.textParts[0].text = "Difficulty: Normal";
                                option.textParts[1].text = "Difficulty: Normal";
                                break;
                            case 2:
                                option.textParts[0].text = "Difficulty: Insane";
                                option.textParts[1].text = "Difficulty: Insane";
                                break;
                        }
                        break;
                    case "Character: ":
                        if (selectedOption == currentOptions.IndexOf(option))
                        {
                            if (Input.GetKeyDown(KeyCode.LeftArrow))
                            {
                                menuVarFlags[1]--;
                                if (menuVarFlags[1] < 0)
                                    menuVarFlags[1] = 5;
                                sfx.PlayOneShot(beep1);
                            }
                            else if (Input.GetKeyDown(KeyCode.RightArrow))
                            {
                                menuVarFlags[1]++;
                                if (menuVarFlags[1] > 5)
                                    menuVarFlags[1] = 0;
                                sfx.PlayOneShot(beep1);
                            }
                        }
                        switch (menuVarFlags[1])
                        {
                            case 0:
                                option.textParts[0].text = "Character: Snaily";
                                option.textParts[1].text = "Character: Snaily";
                                break;
                            case 1:
                                option.textParts[0].text = "Character: Sluggy";
                                option.textParts[1].text = "Character: Sluggy";
                                break;
                            case 2:
                                option.textParts[0].text = "Character: Upside";
                                option.textParts[1].text = "Character: Upside";
                                break;
                            case 3:
                                option.textParts[0].text = "Character: Leggy";
                                option.textParts[1].text = "Character: Leggy";
                                break;
                            case 4:
                                option.textParts[0].text = "Character: Blobby";
                                option.textParts[1].text = "Character: Blobby";
                                break;
                            case 5:
                                option.textParts[0].text = "Character: Leechy";
                                option.textParts[1].text = "Character: Leechy";
                                break;
                        }
                        break;
                    case "Randomized: ":
                        if (selectedOption == currentOptions.IndexOf(option))
                        {
                            if (Input.GetKeyDown(KeyCode.LeftArrow))
                            {
                                menuVarFlags[2]--;
                                if (menuVarFlags[2] < 0)
                                    menuVarFlags[2] = 1;
                                sfx.PlayOneShot(beep1);
                            }
                            else if (Input.GetKeyDown(KeyCode.RightArrow))
                            {
                                menuVarFlags[2]++;
                                if (menuVarFlags[2] > 1)
                                    menuVarFlags[2] = 0;
                                sfx.PlayOneShot(beep1);
                            }
                        }
                        switch (menuVarFlags[2])
                        {
                            case 0:
                                option.textParts[0].text = "Randomized: No";
                                option.textParts[1].text = "Randomized: No";
                                break;
                            case 1:
                                option.textParts[0].text = "Randomized: Yes";
                                option.textParts[1].text = "Randomized: Yes";
                                break;
                        }
                        break;
                }
            }
            GetNewSnailOffset();
        }
    }

    public void AddOption(string text = "", bool isSelectable = true, DestinationDelegate destination = null, int[] paramChange = null)
    {
        foreach (Transform entry in transform)
            entry.localPosition = new Vector2(0, entry.transform.localPosition.y + (LIST_OPTION_SPACING * 0.5f));

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
            element.SetActive(state);
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
        selectSnailOffset = textBounds.max.x + 1;
    }

    public void ForceSelect(int optionNum)
    {
        selectedOption = optionNum;
        selector[0].transform.position = new Vector2(0, currentOptions[optionNum].optionObject.transform.position.y + 0.55f);
        GetNewSnailOffset();
        selector[1].transform.position = new Vector2(-selectSnailOffset, 0);
        selector[2].transform.position = new Vector2(selectSnailOffset, 0);
    }

    public void PageMain()
    {
        ClearOptions();
        bool returnAvailable = false;
        if (PlayState.gameState == "Pause")
        {
            AddOption("Return to game", true);
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
        AddOption("Options", true);
        AddOption("Credits", true, CreditsPage1);
        if (PlayState.HasTime())
            AddOption("Records", true);
        AddOption("Quit", true, QuitConfirm);
        if (returnAvailable)
            ForceSelect(2);
        else
            ForceSelect(0);
        backPage = QuitConfirm;
    }

    public void ProfileScreen()
    {
        ClearOptions();
        AddOption("Select a profile", false);
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
                time += Mathf.Round(data.gameTime[2] * 100) * 0.01f;
                AddOption(i + " / " + data.character + " / " + time + " / " + data.percentage + "%", true, PickSpawn, new int[] { 0, i });
            }
            else
                AddOption("Empty profile", true, StartNewGame, new int[] { 0, 1, 1, 0, 2, 0 });
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
        AddOption("Back to profile selection", true, ProfileScreen);
        ForceSelect(2);
        backPage = ProfileScreen;
    }

    public void PickSpawn()
    {
        ClearOptions();
        AddOption("Okay, where would you", false);
        AddOption("like to start from?", false);
        AddOption("", false);
        AddOption("Start from save point", true);
        AddOption("Start from Snail Town", true);
        AddOption("", false);
        AddOption("Back to profile selection", true, ProfileScreen);
        ForceSelect(3);
        backPage = ProfileScreen;
    }

    public void CopyData()
    {
        ClearOptions();
        AddOption("Okay, copy from which profile?", false);
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
                time += Mathf.Round(data.gameTime[2] * 100) * 0.01f;
                AddOption(i + " / " + data.character + " / " + time + " / " + data.percentage + "%", true, CopyData2, new int[] { 0, i });
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

    public void CopyData2()
    {
        ClearOptions();
        AddOption("Copy to which profile?", false);
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
                time += Mathf.Round(data.gameTime[2] * 100) * 0.01f;
                AddOption((menuVarFlags[0] == i ? "> " : "") + i + " / " + data.character + " / " + time + " / " + data.percentage + "%" + (menuVarFlags[0] == i ? " <" : ""),
                    menuVarFlags[0] != i, CopyConfirm, new int[] { 1, i });
            }
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
                time += Mathf.Round(data.gameTime[2] * 100) * 0.01f;
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
