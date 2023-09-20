using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class DialogueBox : MonoBehaviour
{
    public AnimationModule anim;
    public AudioSource sfx;
    public SpriteRenderer sprite;

    public GameObject cam;
    public GameObject player;
    public GameObject portrait;
    public TextMesh dialogueText;
    public TextMesh dialogueShadow;

    private float roomNameAdjustAmount = 0;

    public GameObject letter;
    public GameObject textObj;
    public Font font;

    public int[] charWidths;

    private float camPos = 0;
    private float portraitPos = 0;
    private int boxState = 0;
    private Vector2 pointer = Vector2.zero;          // This pointer points to what letter of page of text it's looking at
    private Vector2 posPointerOrigin = new(-11.1875f, 1.875f);
    private Vector2 posPointer;
    private const int MAX_LINE_WIDTH = 246;
    private const float PLAYER_OFFSET_FOR_DOWN_POS = 1f;
    private bool buttonDown = false;
    private bool active = false;
    private bool playSound = true;
    private bool forcedClosed = false;
    private Vector2 roomTextOrigin;
    public Sprite[] playerPortraits;
    private bool forceDownPosition;
    private float posVar;
    private List<TextObject> textObjs;

    private SpriteRenderer portraitFrame;
    private AnimationModule portraitFrameAnim;
    private SpriteRenderer portraitChar;
    private AnimationModule portraitCharAnim;
    private Dictionary<int, Sprite> colorizedSprites = new();
    private string currentFrameColor = "0005";
    private CutsceneController stalledCutscene = null;

    private int dialogueType = 0;     // 1 = Item popup, 2 = single-page dialogue, 3 = involved multi-page dialogue
    private int currentSpeaker = 0;
    private int currentShape = 0;
    private List<string> textList = new();
    private List<int> states = new();
    private bool left = false;
    private const float INITIALIZATION_MAX = 0.027f;
    private float initializationCooldown;

    private float currentTimerMax = 0.02f;
    private float timer = 0;
    private int currentSound = 0;
    private Vector2 currentColor = new(3, 12);
    private string currentEffect = "none";
    public bool boxOpenAnimComplete = false;

    private readonly string[] boxShapeIDs = new string[]
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
        anim = GetComponent<AnimationModule>();
        sfx = GetComponent<AudioSource>();
        sprite = GetComponent<SpriteRenderer>();
        cam = transform.parent.gameObject;
        portrait = transform.Find("Portrait").gameObject;
        portrait.SetActive(false);
        player = GameObject.FindWithTag("Player");
        dialogueText = transform.Find("Text").gameObject.GetComponent<TextMesh>();
        dialogueShadow = transform.Find("Shadow").gameObject.GetComponent<TextMesh>();
        roomTextOrigin = PlayState.hudRoomName.transform.localPosition;

        textObj = Resources.Load<GameObject>("Objects/Text Object");
        font = textObj.GetComponent<TextMesh>().font;

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
        if (PlayState.gameState == PlayState.GameState.game)
        {
            sfx.volume = PlayState.generalData.soundVolume * 0.1f;

            timer = Mathf.Clamp(timer - Time.deltaTime, 0, Mathf.Infinity);

            if (dialogueType != 3)
            {
                if (player.transform.position.y > cam.transform.position.y + PLAYER_OFFSET_FOR_DOWN_POS || forceDownPosition || PlayState.achievementOpen)
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
                transform.localPosition = new Vector2(0, camPos);
            else
                transform.localPosition = Vector2.zero;

            portrait.transform.localPosition = new Vector2(-10, portraitPos);
            portraitPos = Mathf.Lerp(portraitPos, 3, 7 * Time.deltaTime);

            if (dialogueType == 2 && boxState == 0)
                camPos = transform.localPosition.y + 4.5f * (player.transform.position.y > cam.transform.position.y + 0.125f ? -1 : 1);

            if (active && dialogueType == 2 && player.transform.position.y < cam.transform.position.y + PLAYER_OFFSET_FOR_DOWN_POS)
            {
                if (roomNameAdjustAmount == 0)
                    roomNameAdjustAmount = PlayState.hudRoomName.GetWidth(true) + 0.25f;
            }
            else
                roomNameAdjustAmount = 0;
            PlayState.hudRoomName.position = new Vector2(Mathf.Lerp(PlayState.hudRoomName.position.x,
                roomTextOrigin.x + roomNameAdjustAmount, 8 * Time.deltaTime), roomTextOrigin.y);

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
                    currentEffect = "none";
                    initializationCooldown = INITIALIZATION_MAX;
                    break;
                case 1:
                    if (initializationCooldown == 0)
                        boxOpenAnimComplete = true;
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
                            UpdatePortrait(PlayState.currentProfile.character,
                                PlayState.CheckForItem(9) ? 3 : (PlayState.CheckForItem(8) ? 2 : (PlayState.CheckForItem(7) ? 1 : 0)));
                            if (left)
                                portraitChar.flipX = false;
                            else
                                portraitChar.flipX = true;
                            switch (PlayState.currentProfile.character)
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
                                if (!Control.SpeakHold(0, true) && buttonDown)
                                    buttonDown = false;
                                if (Control.SpeakPress(0, true) && !buttonDown && dialogueType == 3)
                                {
                                    buttonDown = true;
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
                    if (Control.SpeakPress(0, true))
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
                                    if (transform.GetChild(i).name.Contains("Text Object"))
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
                PlayState.gameState = PlayState.GameState.game;
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
                        currentEffect = args[1].ToLower();
                        break;
                    case "spd":   // Speed
                        currentTimerMax = float.Parse(args[1], CultureInfo.InvariantCulture);
                        break;
                    case "sfx":   // Speaker sound
                        currentSound = int.Parse(args[1]);
                        break;
                    case "col":   // Color
                        currentColor = new Vector2(int.Parse(args[1].Substring(0, 2)), int.Parse(args[1].Substring(2, 2)));
                        break;
                    case "p":     // Pause
                        timer = float.Parse(args[1], CultureInfo.InvariantCulture);
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
            GameObject newLetter = Instantiate(textObj);
            newLetter.transform.parent = transform;
            newLetter.transform.localPosition = posPointer;
            TextObject newLetterScript = newLetter.GetComponent<TextObject>();
            newLetterScript.position = posPointer;
            newLetterScript.CreateShadow();
            newLetterScript.SetText(thisChar.ToString());
            newLetterScript.SetColor(PlayState.GetColor(currentColor));
            newLetterScript.SetMovement((TextObject.MoveEffects)System.Enum.Parse(typeof(TextObject.MoveEffects), currentEffect));
            font.RequestCharactersInTexture(thisChar.ToString());
            font.GetCharacterInfo(thisChar, out CharacterInfo info);
            posPointer.x += info.advance * PlayState.FRAC_16;

            if (mute)
                playSound = false;
            else if (currentTimerMax < 0.04f && Application.targetFrameRate > 60)
                playSound = !playSound;
            else
                playSound = true;
            if (thisChar != ' ' && playSound)
            {
                switch (currentSound == 5 ? 5 : (currentSound == 0 ? (currentSpeaker % 4) + 1 : currentSound))
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
                    case 5:
                        PlayState.PlaySound("Dialogue4");
                        break;
                }
            }
            pointer.y++;
            timer = currentTimerMax;
        }
    }

    public void RunBox(int type, int speaker, List<string> text, int shape, string boxColor = "0005", List<int> stateList = null, bool facingLeft = false)
    {
        boxState = 0;
        pointer = Vector2.zero;
        PlayState.dialogueOpen = true;

        dialogueType = type;
        currentSpeaker = speaker;
        currentShape = shape;
        sprite.color = PlayState.GetColor(boxColor);
        currentFrameColor = boxColor;

        for (int i = 0; i < text.Count; i++)
            text[i] = text[i]
                .Replace("##", PlayState.GetItemPercentage().ToString())
                .Replace("{P}", PlayState.GetText("char_" + PlayState.currentProfile.character.ToLower()))
                .Replace("{PF}", PlayState.GetText("char_full_" + PlayState.currentProfile.character.ToLower()))
                .Replace("{S}", PlayState.GetText("species_" + PlayState.currentProfile.character.ToLower()))
                .Replace("{SS}", PlayState.GetText("species_plural_" + PlayState.currentProfile.character.ToLower()))
                .Replace("{ID}", speaker.ToString())
                .Replace("{Helix}", PlayState.CountFragments().ToString())
                .Replace("{HelixLeft}", (PlayState.MAX_FRAGMENTS - PlayState.CountFragments()).ToString());

        textList = text;
        states = stateList;
        left = facingLeft;

        forceDownPosition = type == 3;
        active = true;
    }

    public IEnumerator ReturnMusicVol()
    {
        while (PlayState.activeMus.volume < 0.1f * PlayState.generalData.musicVolume)
        {
            PlayState.activeMus.volume += 0.025f * PlayState.generalData.musicVolume;
            yield return new WaitForFixedUpdate();
        }
    }

    public void CloseBox()
    {
        forceDownPosition = dialogueType == 3;
        forcedClosed = true;
        pointer = Vector2.zero;
        posPointer = posPointerOrigin;
        dialogueText.text = "";
        dialogueShadow.text = "";
        anim.Play("Dialogue_" + boxShapeIDs[currentShape] + "_close");
        portrait.SetActive(false);
        if (!PlayState.cutsceneActive)
            PlayState.paralyzed = false;
        PlayState.dialogueOpen = false;
        dialogueType = 0;
        timer = 0;
        boxOpenAnimComplete = false;
        active = false;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (transform.GetChild(i).name.Contains("Text Object"))
                Destroy(transform.GetChild(i).gameObject);
        }
        if (stalledCutscene != null)
        {
            stalledCutscene.EndActionRemote();
            stalledCutscene = null;
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
            switch (PlayState.currentProfile.character)
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

    public void StallCutsceneDialogue(CutsceneController cutscene, float lingerTime)
    {
        StartCoroutine(StallCutsceneDialogueCoroutine(cutscene, lingerTime));
    }
    private IEnumerator StallCutsceneDialogueCoroutine(CutsceneController cutscene, float lingerTime)
    {
        while (boxState != 4)
            yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(lingerTime);
        if (boxState == 4)
            CloseBox();
        cutscene.EndActionRemote();
    }

    public void StallCutsceneDialoguePrompted(CutsceneController cutscene)
    {
        stalledCutscene = cutscene;
    }
}
