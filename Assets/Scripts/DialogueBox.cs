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
    //private List<SpriteRenderer> portraitParts = new List<SpriteRenderer>();
    public Sprite[] playerPortraits;
    private bool forceDownPosition;
    private float posVar;

    private SpriteRenderer portraitFrame;
    private AnimationModule portraitFrameAnim;
    private SpriteRenderer portraitChar;
    private AnimationModule portraitCharAnim;
    //private List<Sprite> coloredSprites = new List<Sprite>();
    //private Dictionary<int, int> spriteReferences = new Dictionary<int, int>();
    private Dictionary<int, Sprite> colorizedSprites = new Dictionary<int, Sprite>();
    private string currentFrameColor = "0005";

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

        //portraitParts.Add(portrait.transform.GetChild(0).GetComponent<SpriteRenderer>());
        //portraitParts.Add(portrait.transform.GetChild(1).GetComponent<SpriteRenderer>());
        //portraitParts.Add(portrait.transform.GetChild(2).GetComponent<SpriteRenderer>());
        //portraitParts.Add(portrait.transform.GetChild(3).GetComponent<SpriteRenderer>());
        //portraitParts.Add(portrait.transform.GetChild(4).GetComponent<SpriteRenderer>());
        //portraitParts.Add(portrait.transform.GetChild(5).GetComponent<SpriteRenderer>());
        portraitFrame = portrait.GetComponent<SpriteRenderer>();
        portraitFrameAnim = portrait.GetComponent<AnimationModule>();
        portraitChar = portrait.transform.GetChild(0).GetComponent<SpriteRenderer>();
        portraitCharAnim = portrait.transform.GetChild(0).GetComponent<AnimationModule>();
        portraitFrame.color = PlayState.GetColor("0006");

        for (int i = 0; i < boxShapeIDs.Length; i++)
        {
            anim.Add("Dialogue_" + boxShapeIDs[i] + "_open");
            anim.Add("Dialogue_" + boxShapeIDs[i]);
            anim.Add("Dialogue_" + boxShapeIDs[i] + "_close");
        }

        portraitFrameAnim.Add("Dialogue_portrait_frame");
        portraitFrameAnim.Play("Dialogue_portrait_frame");
        portraitCharAnim.Add("Dialogue_portrait_snail");
        portraitCharAnim.Add("Dialogue_portrait_upsideDownSnail");
        portraitCharAnim.Add("Dialogue_portrait_turtle");
        for (int i = 0; i <= 3; i++)
        {
            portraitCharAnim.Add("Dialogue_portrait_Snaily" + i);
            portraitCharAnim.Add("Dialogue_portrait_Sluggy" + i);
            portraitCharAnim.Add("Dialogue_portrait_Upside" + i);
            portraitCharAnim.Add("Dialogue_portrait_Leggy" + i);
            portraitCharAnim.Add("Dialogue_portrait_Blobby" + i);
            portraitCharAnim.Add("Dialogue_portrait_Leechy" + i);
        }

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
                if (portraitCharAnim.isPlaying && (portraitCharAnim.currentAnimName == "Dialogue_portrait_snail" ||
                    portraitCharAnim.currentAnimName == "Dialogue_portrait_upsideDownSnail" || portraitCharAnim.currentAnimName == "Dialogue_portrait_snail"))
                    portraitChar.sprite = colorizedSprites[portraitCharAnim.GetCurrentFrameValue()];
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
                    anim.Play("Dialogue_" + boxShapeIDs[currentShape] + "_open");
                    boxState = 1;
                    playSound = true;
                    if (dialogueType == 3)
                    {
                        portrait.SetActive(true);
                        buttonDown = true;
                        GenerateColorizedPortraitSprites();
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
                            UpdatePortrait("npc", 0);
                            if (left)
                                portraitChar.flipX = true;
                            else
                                portraitChar.flipX = false;
                            currentSound = 0;
                        }
                        else if (states[(int)pointer.x] == 0)
                        {
                            UpdatePortrait(PlayState.currentCharacter, PlayState.CheckForItem(9) ? 3 : (PlayState.CheckForItem(8) ? 2 : (PlayState.CheckForItem(7) ? 1 : 0)));
                            if (left)
                                portraitChar.flipX = false;
                            else
                                portraitChar.flipX = true;
                            switch (PlayState.currentCharacter)
                            {
                                case "Snaily":
                                    currentSound = 1;
                                    break;
                                case "Leggy":
                                    currentSound = 1;
                                    break;
                                case "Upside":
                                    currentSound = 2;
                                    break;
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
                                    ParseNextChar();
                                }
                                if (!Control.SpeakHold() && buttonDown)
                                    buttonDown = false;
                                if (Control.SpeakPress() && !buttonDown && dialogueType == 3)
                                {
                                    buttonDown = true;
                                    //dialogueText.text = textList[(int)pointer.x];
                                    //dialogueShadow.text = dialogueText.text;
                                    //pointer.y = textList[(int)pointer.x].Length;
                                    while (pointer.y < textList[(int)pointer.x].Length)
                                    {
                                        ParseNextChar(true);
                                    }
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
                    if (Control.SpeakPress())
                    {
                        buttonDown = true;
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

    private void ParseNextChar(bool mute = false)
    {
        char thisChar = textList[(int)pointer.x][(int)pointer.y];
        bool advanceChar = true;

        while (thisChar == '{')
        {
            string command = "";
            for (float i = pointer.y + 1; textList[(int)pointer.x][(int)i] != '}' && i < textList[(int)pointer.x].Length; i++)
                command += textList[(int)pointer.x][(int)i];
            if (textList[(int)pointer.x][(int)(pointer.y + command.Length + 1)] == '}')
            {
                string[] args;
                if (command.Contains("|"))
                    args = command.Split('|');
                else
                    args = new string[] { command };
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

            if (mute)
                playSound = false;
            else if (currentTimerMax < 0.04f)
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

    public void RunBox(int type, int speaker, List<string> text, int shape, string boxColor = "0005", List<Color32> colors = null, List<int> stateList = null, bool facingLeft = false)
    {
        boxState = 0;
        pointer = Vector2.zero;

        dialogueType = type;
        currentSpeaker = speaker;
        currentShape = shape;
        sprite.color = PlayState.GetColor(boxColor);
        currentFrameColor = boxColor;
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

    private void GenerateColorizedPortraitSprites()
    {
        colorizedSprites.Clear();
        int[] baseSprites = PlayState.GetAnim("Dialogue_portrait_colorize").frames;
        for (int i = 0; i < baseSprites.Length; i++)
        {
            colorizedSprites.Add(baseSprites[i], PlayState.Colorize("UI/DialoguePortrait", baseSprites[i], "Entities/SnailNpcColor", currentSpeaker));
        }
    }

    private void UpdatePortrait(string state, int value)
    {
        PlayState.AnimationData currentAnim = PlayState.GetAnim("Dialogue_portrait_" + (state.ToLower() == "npc" ?
            (value == 2 ? "turtle" : (value == 1 ? "upsideDownSnail" : "snail")) : state[0].ToString().ToUpper() + state.Substring(1, state.Length - 1).ToLower() + value));

        if (state.ToLower() == "npc" && (value == 0 || value == 1))
            portraitCharAnim.updateSprite = false;
        else
            portraitCharAnim.updateSprite = true;
        portraitCharAnim.Play(currentAnim.name);

        if (state == "npc")
        {
            sprite.color = PlayState.GetColor(currentFrameColor);
            portraitFrame.color = PlayState.GetColor(currentFrameColor);
            if (anim.currentAnimName != "Dialogue_" + boxShapeIDs[currentShape] && anim.currentAnimName != "Dialogue_" + boxShapeIDs[currentShape] + "_open")
                anim.Play("Dialogue_" + boxShapeIDs[currentShape]);
        }
        else
        {
            int[] colorList = PlayState.GetAnim("Dialogue_characterColors").frames;
            int[] shapeList = PlayState.GetAnim("Dialogue_characterShapes").frames;
            int charID = 0;
            switch (PlayState.currentCharacter)
            {
                case "Snaily":
                    currentSound = 1;
                    charID = 0;
                    break;
                case "Leggy":
                    currentSound = 1;
                    charID = 3;
                    break;
                case "Upside":
                    currentSound = 2;
                    charID = 2;
                    break;
                case "Blobby":
                    currentSound = 2;
                    charID = 4;
                    break;
                case "Leechy":
                    currentSound = 3;
                    charID = 5;
                    break;
                case "Sluggy":
                    currentSound = 4;
                    charID = 1;
                    break;
            }
            sprite.color = PlayState.GetColor(PlayState.ParseColorCodeToString(colorList[charID]));
            portraitFrame.color = PlayState.GetColor(PlayState.ParseColorCodeToString(colorList[charID]));
            if (anim.currentAnimName != "Dialogue_" + boxShapeIDs[shapeList[charID]] && anim.currentAnimName != "Dialogue_" + boxShapeIDs[currentShape] + "_open")
                anim.Play("Dialogue_" + boxShapeIDs[shapeList[charID]]);
        }
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
