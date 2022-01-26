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

    private const float LIST_CENTER_Y = -1;
    private const float LIST_OPTION_SPACING = 1.5f;
    private float currentSpawnY = LIST_CENTER_Y;

    public Vector2[] panPoints = new Vector2[] // Points in world space that the main menu camera should pan over; set only one point for a static cam
    {
        new Vector2(0.5f, 0.5f)
    };
    public float panSpeed = 0.015f; // The speed at which the camera should pan
    public float stopTime = 3; // The time the camera spends at each point
    public string easeType = "linear"; // The easing type between points; can be set to "linear" or "smooth"
    public string edgeCase = "loop"; // What the camera should do when encountering the end of the point array ; can be set to "loop", "bounce", or "warp"
    private int currentPointInIndex = 0;
    private bool direction = true;

    public AudioSource music;
    public GameObject textObject;

    void Start()
    {
        music = GetComponent<AudioSource>();
    }

    void Update()
    {
        if ((PlayState.gameState == "Menu" || PlayState.gameState == "Pause") && !PlayState.isMenuOpen)
        {
            if (PlayState.gameState == "Menu")
                music.Play();
            PlayState.isMenuOpen = true;
            PageMain();
        }
    }

    public void AddOption(string text = "", bool isSelectable = false, DestinationDelegate destination = null)
    {
        foreach (Transform entry in transform)
            entry.localPosition = new Vector2(entry.transform.localPosition.x, entry.transform.localPosition.y + (LIST_OPTION_SPACING * 0.5f));

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
        newText.transform.localPosition = new Vector3(newText.transform.localPosition.x, currentSpawnY);
        currentSpawnY -= LIST_OPTION_SPACING * 0.5f;

        currentOptions.Add(option);
    }

    public void PageMain()
    {
        AddOption("New game");
        AddOption("Load game");
        AddOption("Boss rush");
        AddOption("");
        AddOption("Options");
        AddOption("Credits");
        AddOption("Records");
        AddOption("Gallery");
    }
}
