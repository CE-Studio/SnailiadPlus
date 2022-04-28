using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueBox : MonoBehaviour
{
    //public Animator anim;
    public AnimationModule anim;
    public AudioSource sfx;
    public SpriteRenderer sprite;

    public GameObject cam;
    public GameObject player;
    public GameObject portrait;
    public TextMesh dialogueText;
    public TextMesh dialogueShadow;
    public Transform roomText;

    //public AudioClip dialogue0;
    //public AudioClip dialogue1;
    //public AudioClip dialogue2;
    //public AudioClip dialogue3;

    public GameObject letter;

    public int[] charWidths;

    private float camPos = 0;
    private float portraitPos = 0;
    private int boxState = 0;
    private Vector2 pointer = Vector2.zero;          // This pointer points to what letter of page of text it's looking at
    private Vector2 posPointerOrigin = new Vector2(-10.6875f, 1.125f);
    private Vector2 posPointer;
    private const int MAX_LINE_WIDTH = 246;
    private bool buttonDown = false;
    private bool active = false;
    private bool playSound = true;
    private bool forcedClosed = false;
    private Vector2 roomTextOrigin;
    private List<SpriteRenderer> portraitParts = new List<SpriteRenderer>();
    public Sprite[] playerPortraits;
    private bool forceDownPosition;
    private float posVar;

    private int dialogueType = 0;     // 1 = Item popup, 2 = single-page dialogue, 3 = involved multi-page dialogue
    private int currentSpeaker = 0;
    private int currentShape = 0;
    private List<string> textList = new List<string>();
    private List<Color32> portraitColors = new List<Color32>();
    private List<int> states = new List<int>();
    private bool left = false;
    private const float INITIALIZATION_MAX = 0.027f;
    private float initializationCooldown;

    private float currentTimerMax = 0.02f;
    private float timer = 0;
    private int currentSound = 0;
    private Vector2 currentColor = new Vector2(3, 12);
    private string currentEffect = "None";
    public bool boxOpenAnimComplete = false;

    private string[] boxShapeIDs = new string[]
    {
        "square",
        "round",
        "angle",
        "loud",
        "bevel",
        "outline",
        "cloud",
        "panel"
    };
    
    void Start()
    {
        //anim = GetComponent<Animator>();
        anim = GetComponent<AnimationModule>();
        sfx = GetComponent<AudioSource>();
        sprite = GetComponent<SpriteRenderer>();
        cam = transform.parent.gameObject;
        portrait = transform.Find("Portrait").gameObject;
        portrait.SetActive(false);
        player = GameObject.FindWithTag("Player");
        dialogueText = transform.Find("Text").gameObject.GetComponent<TextMesh>();
        dialogueShadow = transform.Find("Shadow").gameObject.GetComponent<TextMesh>();
        roomText = GameObject.Find("View/Minimap Panel/Room Name Parent").transform;
        roomTextOrigin = roomText.localPosition;

        portraitParts.Add(portrait.transform.GetChild(0).GetComponent<SpriteRenderer>());
        portraitParts.Add(portrait.transform.GetChild(1).GetComponent<SpriteRenderer>());
        portraitParts.Add(portrait.transform.GetChild(2).GetComponent<SpriteRenderer>());
        portraitParts.Add(portrait.transform.GetChild(3).GetComponent<SpriteRenderer>());
        portraitParts.Add(portrait.transform.GetChild(4).GetComponent<SpriteRenderer>());
        portraitParts.Add(portrait.transform.GetChild(5).GetComponent<SpriteRenderer>());

        anim.Add("Dialogue_square");
        anim.Add("Dialogue_square_close");
        anim.Add("Dialogue_round");
        anim.Add("Dialogue_round_close");
        anim.Add("Dialogue_angle");
        anim.Add("Dialogue_angle_close");
        anim.Add("Dialogue_loud");
        anim.Add("Dialogue_loud_close");
        anim.Add("Dialogue_bevel");
        anim.Add("Dialogue_bevel_close");
        anim.Add("Dialogue_outline");
        anim.Add("Dialogue_outline_close");
        anim.Add("Dialogue_cloud");
        anim.Add("Dialogue_cloud_close");
        anim.Add("Dialogue_panel");
        anim.Add("Dialogue_panel_close");

        charWidths = PlayState.GetAnim("TextWidth").frames;
    }

    void Update()
    {
        if (PlayState.gameState == "Game")
        {
            sfx.volume = PlayState.gameOptions[0] * 0.1f;

            timer = Mathf.Clamp(timer - Time.deltaTime, 0, Mathf.Infinity);

            if (dialogueType != 3)
            {
                if (player.transform.position.y > cam.transform.position.y + 0.125f || forceDownPosition)
                {
                    if (active)
                        posVar = Mathf.Lerp(posVar, 1, 7 * Time.deltaTime);
                    else
                        posVar = 1;
                }
                else
                {
                    if (active)
                        posVar = Mathf.Lerp(posVar, 0, 7 * Time.deltaTime);
                    else
                        posVar = 0;
                }
                camPos = posVar > 0.5f ? Mathf.Lerp(-8.5f, -4.5f, (posVar - 0.5f) * 2) : Mathf.Lerp(4.5f, 8.5f, posVar * 2);
            }
            else
            {
                camPos = -4.5f;
            }

            if (dialogueType != 1)
            {
                transform.localPosition = new Vector2(0, camPos);
            }
            else
            {
                transform.localPosition = Vector2.zero;
            }

            portrait.transform.localPosition = new Vector2(-10, portraitPos);
            portraitPos = Mathf.Lerp(portraitPos, 3, 7 * Time.deltaTime);

            if (dialogueType == 2 && boxState == 0)
            {
                if (player.transform.position.y > cam.transform.position.y + 0.125f)
                {
                    camPos = transform.localPosition.y - 4.5f;
                }
                else
                {
                    camPos = transform.localPosition.y + 4.5f;
                }
            }

            if (active && dialogueType == 2 && player.transform.position.y < cam.transform.position.y + 0.125f)
                roomText.localPosition = new Vector2(Mathf.Lerp(roomText.localPosition.x, roomTextOrigin.x + 5, 8 * Time.deltaTime), roomTextOrigin.y);
            else
                roomText.localPosition = new Vector2(Mathf.Lerp(roomText.localPosition.x, roomTextOrigin.x, 8 * Time.deltaTime), roomTextOrigin.y);

            if (!active)
                return;
            forcedClosed = false;
            switch (boxState)
            // Case 0 = dialogue box opens
            // Case 1 = initalization of text
            // Case 2 = waiting for a button press to advance text
            // Case 3 = dialogue box closes
            // Case 4 = static box for single-page dialogue
            {
                case 0:
                    anim.Play("Dialogue_" + boxShapeIDs[currentShape]);
                    boxState = 1;
                    playSound = true;
                    if (dialogueType == 3)
                    {
                        portrait.SetActive(true);
                        buttonDown = true;
                    }
                    portraitPos = 1;
                    currentTimerMax = 0.02f;
                    currentSound = 0;
                    currentColor = new Vector2(3, 12);
                    currentEffect = "None";
                    initializationCooldown = INITIALIZATION_MAX;
                    break;
                case 1:
                    if (initializationCooldown == 0)
                        MarkOpenAnimComplete();
                    else
                        initializationCooldown = Mathf.Clamp(initializationCooldown - Time.deltaTime, 0, Mathf.Infinity);
                    if (dialogueType == 3)
                    {
                        if (states[(int)pointer.x] != 0)
                        {
                            for (int i = 0; i < portraitParts.Count - 1; i++)
                                portraitParts[i].color = portraitColors[(i + 1) * states[(int)pointer.x] - 1];
                            for (int i = 0; i < portraitParts.Count - 1; i++)
                            {
                                portraitParts[i].enabled = true;
                                if (left)
                                    portraitParts[i].flipX = true;
                                else
                                    portraitParts[i].flipX = false;
                            }
                            portraitParts[5].enabled = false;
                            currentSound = 0;
                        }
                        else if (states[(int)pointer.x] == 0)
                        {
                            UpdatePlayerPortrait();
                            for (int i = 0; i < portraitParts.Count - 1; i++)
                                portraitParts[i].enabled = false;
                            portraitParts[5].enabled = true;
                            if (left)
                                portraitParts[5].flipX = false;
                            else
                                portraitParts[5].flipX = true;
                            switch (PlayState.currentCharacter)
                            {
                                case "Snaily":
                                case "Leggy":
                                    currentSound = 1;
                                    break;
                                case "Upside":
                                case "Blobby":
                                    currentSound = 2;
                                    break;
                                case "Leechy":
                                    currentSound = 3;
                                    break;
                                case "Sluggy":
                                    currentSound = 4;
                                    break;
                            }
                        }
                    }

                    if (dialogueType == 1)
                    {
                        dialogueText.text = textList[(int)pointer.x];
                        dialogueShadow.text = textList[(int)pointer.x];
                        pointer.x++;
                        boxState = 2;
                        if (pointer.x == 1)
                        {
                            //yield return new WaitForSeconds(4);
                        }
                        else
                        {
                            //yield return new WaitForEndOfFrame();
                        }
                    }
                    else
                    {
                        if (boxOpenAnimComplete)
                        {
                            if (pointer.y < textList[(int)pointer.x].Length)
                            {
                                if (forcedClosed)
                                    break;
                                if (timer == 0)
                                {
                                    //dialogueText.text += textList[(int)pointer.x][(int)pointer.y];
                                    //dialogueShadow.text = dialogueText.text;

                                    char thisChar = textList[(int)pointer.x][(int)pointer.y];
                                    bool advanceChar = true;
                                    
                                    while (thisChar == '{')
                                    {
                                        string command = "";
                                        for (float i = pointer.y + 1; textList[(int)pointer.x][(int)i] != '}' && i < textList[(int)pointer.x].Length; i++)
                                            command += textList[(int)pointer.x][(int)i];
                                        if (textList[(int)pointer.x][(int)(pointer.y + command.Length + 1)] == '}' && command.Contains("|"))
                                        {
                                            string[] args = command.Split('|');
                                            switch (args[0].ToLower())
                                            {
                                                case "nl":    // New line
                                                    posPointer = new Vector2(posPointerOrigin.x, posPointer.y - 1.125f);
                                                    break;
                                                case "eff":   // Effect
                                                    currentEffect = args[1];
                                                    break;
                                                case "spd":   // Speed
                                                    currentTimerMax = float.Parse(args[1]);
                                                    break;
                                                case "sfx":   // Speaker sound
                                                    currentSpeaker = int.Parse(args[1]);
                                                    break;
                                                case "col":   // Color
                                                    currentColor = new Vector2(int.Parse(args[1].Substring(0, 2)), int.Parse(args[1].Substring(2, 2)));
                                                    break;
                                                case "p":     // Pause
                                                    timer = float.Parse(args[1]);
                                                    advanceChar = false;
                                                    break;
                                                case "ctrl":  // Parse keybind to string
                                                    textList[(int)pointer.x] = textList[(int)pointer.x].Insert((int)pointer.y + command.Length + 2,
                                                        Control.ParseKeyName(int.Parse(args[1])));
                                                    break;
                                                default:
                                                    Debug.LogWarning("Unknown command prefix \"" + args[0].ToLower() + "\".");
                                                    break;
                                            }
                                            pointer.y += command.Length + 2;
                                            thisChar = textList[(int)pointer.x][(int)pointer.y];
                                        }
                                        else
                                            break;
                                    }
                                    if (advanceChar)
                                    {
                                        GameObject newLetter = Instantiate(letter);
                                        newLetter.transform.parent = transform;
                                        newLetter.transform.localPosition = posPointer;
                                        FontObject newLetterScript = newLetter.GetComponent<FontObject>();
                                        newLetterScript.Create(thisChar, 3, currentEffect, currentColor);
                                        int addedWidth = (charWidths[newLetterScript.ID] + 1) * 2;
                                        posPointer.x += addedWidth * 0.0625f;

                                        if (currentTimerMax < 0.04f)
                                            playSound = !playSound;
                                        else
                                            playSound = true;
                                        if (thisChar != ' ' && playSound)
                                        {
                                            switch (currentSound == 0 ? (currentSpeaker % 4) + 1 : currentSound)
                                            {
                                                case 1:
                                                    PlayState.PlaySound("Dialogue0");
                                                    break;
                                                case 2:
                                                    PlayState.PlaySound("Dialogue1");
                                                    break;
                                                case 3:
                                                    PlayState.PlaySound("Dialogue2");
                                                    break;
                                                case 4:
                                                    PlayState.PlaySound("Dialogue3");
                                                    break;
                                            }
                                        }
                                        pointer.y++;
                                        timer = currentTimerMax;
                                    }
                                }
                                if (!Control.SpeakHold() && buttonDown)
                                    buttonDown = false;
                                if (Control.SpeakPress() && !buttonDown && dialogueType == 3)
                                {
                                    buttonDown = true;
                                    dialogueText.text = textList[(int)pointer.x];
                                    dialogueShadow.text = dialogueText.text;
                                    pointer.y = textList[(int)pointer.x].Length;
                                    break;
                                }
                            }
                            else
                            {
                                if (dialogueType == 2)
                                {
                                    boxState = 4;
                                }
                                else if (dialogueType == 3)
                                {
                                    pointer.x++;
                                    pointer.y = 0;
                                    boxState = 2;
                                }
                            }
                        }
                    }
                    break;
                case 2:
                    //anim.Play("Dialogue continue", 0, 0);
                    //if (!Control.SpeakHold() && buttonDown)
                    //{
                    //    buttonDown = false;
                    //}
                    if (Control.SpeakPress())// && !buttonDown)
                    {
                        buttonDown = true;
                        //anim.Play("Dialogue hold", 0, 0);
                        if (pointer.x == textList.Count)
                        {
                            boxState = 3;
                        }
                        else
                        {
                            if (dialogueType == 3)
                            {
                                dialogueText.text = "";
                                dialogueShadow.text = "";
                                for (int i = transform.childCount - 1; i >= 0; i--)
                                {
                                    if (transform.GetChild(i).name.Contains("Font Object"))
                                        Destroy(transform.GetChild(i).gameObject);
                                }
                                posPointer = posPointerOrigin;
                            }
                            boxState = 1;
                        }
                    }
                    break;
                case 3:
                    CloseBox();
                    break;
                case 4:
                    break;
                default:
                    break;
            }
            if (boxState == 3 && dialogueType == 1)
            {
                PlayState.activeMus.clip = PlayState.areaMus;
                PlayState.activeMus.time = PlayState.playbackTime;
                PlayState.activeMus.volume = 0;
                PlayState.activeMus.Play();
                PlayState.gameState = "Game";
                StartCoroutine(nameof(ReturnMusicVol));
            }
        }
    }

    public void RunBox(int type, int speaker, List<string> text, int shape, string boxColor = "0005", List<Color32> colors = null, List<int> stateList = null, bool facingLeft = false)
    {
        boxState = 0;
        pointer = Vector2.zero;

        dialogueType = type;
        currentSpeaker = speaker;
        currentShape = shape;
        sprite.color = PlayState.GetColor(boxColor);
        textList = text;
        portraitColors = colors;
        states = stateList;
        left = facingLeft;

        active = true;
    }

    public IEnumerator ReturnMusicVol()
    {
        while (PlayState.activeMus.volume < 1)
        {
            PlayState.activeMus.volume += 0.025f * PlayState.musicVol;
            yield return new WaitForFixedUpdate();
        }
    }

    public void CloseBox()
    {
        if (dialogueType == 3)
            forceDownPosition = true;
        forcedClosed = true;
        pointer = Vector2.zero;
        posPointer = posPointerOrigin;
        dialogueText.text = "";
        dialogueShadow.text = "";
        //anim.Play("Dialogue close", 0, 0);
        anim.Play("Dialogue_" + boxShapeIDs[currentShape] + "_close");
        portrait.SetActive(false);
        PlayState.paralyzed = false;
        dialogueType = 0;
        boxOpenAnimComplete = false;
        active = false;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).name.Contains("Font Object"))
                Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void UpdatePlayerPortrait()
    {
        int portraitID = 0;

        if (PlayState.CheckForItem(9))
            portraitID = 3;
        else if (PlayState.CheckForItem(8))
            portraitID = 2;
        else if (PlayState.CheckForItem(7))
            portraitID = 1;

        switch (PlayState.currentCharacter)
        {
            default:
            case "Snaily":
                portraitID += 0;
                break;
            case "Sluggy":
                portraitID += 4;
                break;
            case "Upside":
                portraitID += 8;
                break;
            case "Leggy":
                portraitID += 12;
                break;
            case "Blobby":
                portraitID += 16;
                break;
            case "Leechy":
                portraitID += 20;
                break;
        }

        portraitParts[5].sprite = playerPortraits[portraitID];
    }

    public void DeactivateForceDown()
    {
        forceDownPosition = false;
    }

    public void MarkOpenAnimComplete()
    {
        boxOpenAnimComplete = true;
    }

    public void ToggleSpriteOn()
    {
        sprite.enabled = true;
    }

    public void ToggleSpriteOff()
    {
        sprite.enabled = false;
    }
}
