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
    }

    private List<MenuOption> currentOptions = new List<MenuOption>();
    private DestinationDelegate backPage;

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
        }
    }

    public void AddOption(string text = "", bool isSelectable = true, DestinationDelegate destination = null)
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
            currentOptions.Remove(option);
        }
    }

    public void GetNewSnailOffset()
    {
        Bounds textBounds = currentOptions[selectedOption].optionObject.transform.GetChild(0).GetComponent<MeshRenderer>().bounds;
        selectSnailOffset = textBounds.max.x + 1;
    }

    public void ForceSelect(int optionNum)
    {
        selectedOption = optionNum;
        selector[0].transform.position = new Vector2(0, currentOptions[optionNum].optionObject.transform.position.y);
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
        AddOption("Select profile", true);
        AddOption("Boss rush", true);
        AddOption("Multiplayer", true);
        AddOption("", false);
        AddOption("Options", true);
        AddOption("Credits", true);
        AddOption("Records", true);
        AddOption("Quit", true);
        backPage = null;
        if (returnAvailable)
            ForceSelect(2);
        else
            ForceSelect(0);
    }
}
