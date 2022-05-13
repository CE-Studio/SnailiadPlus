using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public int ID = 0;
    public bool upsideDown = false;
    public bool chatting = false;
    public bool needsSpace = false; // On the off chance that two snails are close enough to each other to trigger simultaneously, like 06 and 17
    public bool buttonDown = false;
    private int nexted = 0;

    public string playerName;
    public string playerFullName;
    public string playerSpecies;

    public List<Color32> colors = new List<Color32>();

    private List<Color32> portraitColors = new List<Color32>();
    private List<int> portraitStateList = new List<int>();         // 0 for the player, any other positive number for whatever other NPC is speaking
    public Texture2D colorTable;
    public Sprite[] npcSpriteSheet;
    public Sprite[] sprites;

    public List<SpriteRenderer> parts = new List<SpriteRenderer>();
    public SpriteRenderer sprite;
    public AnimationModule anim;
    public GameObject speechBubble;

    public Vector2 origin;

    private RaycastHit2D groundCheck;
    public float velocity;
    private const float GRAVITY = 1.35f;
    private const float TERMINAL_VELOCITY = -0.66f;

    private List<string> textToSend = new List<string>();

    public virtual void Awake()
    {
        switch (PlayState.currentCharacter)
        {
            case "Snaily":
                playerName = "Snaily";
                playerFullName = "Snaily Snail";
                playerSpecies = "snail";
                break;
            default:
                playerName = "Snaily";
                playerFullName = "Snaily Snail";
                playerSpecies = "snail";
                break;
        }

        //colorTable = (Texture2D)Resources.Load("Images/Entities/SnailNpcColor");

        parts.Add(transform.GetComponent<SpriteRenderer>());
        parts.Add(transform.GetChild(0).GetComponent<SpriteRenderer>());
        parts.Add(transform.GetChild(1).GetComponent<SpriteRenderer>());
        parts.Add(transform.GetChild(2).GetComponent<SpriteRenderer>());
        parts.Add(transform.GetChild(3).GetComponent<SpriteRenderer>());
        speechBubble = transform.Find("Speech bubble").gameObject;

        //parts[0].color = colorTable.GetPixel(0, ID);
        //parts[1].color = colorTable.GetPixel(1, ID);
        //parts[2].color = colorTable.GetPixel(2, ID);
        //parts[3].color = colorTable.GetPixel(3, ID);
        //parts[4].color = colorTable.GetPixel(4, ID);

        //CreateNewSprites();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<AnimationModule>();
        anim.updateSprite = false;
        //anim.Play("NPC_idle");

        if (upsideDown)
        {
            for (int j = 0; j < parts.Count; j++)
                parts[j].flipY = true;
            speechBubble.GetComponent<SpriteRenderer>().flipY = true;
            speechBubble.transform.localPosition = new Vector2(0, -0.75f);
        }
        speechBubble.GetComponent<SpriteRenderer>().enabled = false;

        origin = transform.localPosition;

        groundCheck = Physics2D.BoxCast(
            transform.position,
            new Vector2(1.467508f, 0.82375f),
            0,
            upsideDown ? Vector2.up : Vector2.down,
            Mathf.Infinity,
            LayerMask.GetMask("PlayerCollide"),
            Mathf.Infinity,
            Mathf.Infinity
            );
    }

    public virtual void OnEnable()
    {
        nexted = 0;
        chatting = false;
        //CreateNewSprites();
        //anim.Play("NPC_idle");
    }

    public virtual void Spawn()
    {
        if (sprites.Length == 0)
        {
            CreateNewSprites();
            anim.Add("NPC_idle");
            anim.Add("NPC_shell");
            anim.Add("NPC_sleep");
        }
        anim.Play("NPC_idle");
    }

    public virtual void FixedUpdate()
    {
        if (groundCheck.distance != 0 && groundCheck.distance > 0.01f)
            {
            if (upsideDown)
                velocity = Mathf.Clamp(velocity + GRAVITY * Time.fixedDeltaTime, -Mathf.Infinity, -TERMINAL_VELOCITY);
            else
                velocity = Mathf.Clamp(velocity - GRAVITY * Time.fixedDeltaTime, TERMINAL_VELOCITY, Mathf.Infinity);
            bool resetVelFlag = false;
            if (Mathf.Abs(velocity) > Mathf.Abs(groundCheck.distance))
            {
                RaycastHit2D groundCheckRay = Physics2D.Raycast(
                    new Vector2(groundCheck.point.x, transform.position.y + (upsideDown ? 0.5f : -0.5f)),
                    upsideDown ? Vector2.up : Vector2.down,
                    Mathf.Infinity,
                    LayerMask.GetMask("PlayerCollide"),
                    Mathf.Infinity,
                    Mathf.Infinity
                    );
                velocity = groundCheckRay.distance * (upsideDown ? 1 : -1);
                resetVelFlag = true;
            }
            transform.position = new Vector2(transform.position.x, transform.position.y + velocity);
            if (resetVelFlag)
                velocity = 0;
        }
        else
            velocity = 0;
        groundCheck = Physics2D.BoxCast(
            transform.position,
            new Vector2(1, 0.98f),
            0,
            upsideDown ? Vector2.up : Vector2.down,
            Mathf.Infinity,
            LayerMask.GetMask("PlayerCollide"),
            Mathf.Infinity,
            Mathf.Infinity
            );
    }

    public virtual void Update()
    {
        if (PlayState.gameState == "Game")
        {
            if (anim.isPlaying)
                sprite.sprite = sprites[anim.GetCurrentFrameValue()];

            if (PlayState.player.transform.position.x < transform.position.x)
            {
                for (int i = 0; i < parts.Count; i++)
                    parts[i].flipX = true;
                speechBubble.GetComponent<SpriteRenderer>().flipX = false;
            }
            else
            {
                for (int i = 0; i < parts.Count; i++)
                    parts[i].flipX = false;
                speechBubble.GetComponent<SpriteRenderer>().flipX = true;
            }

            if (Vector2.Distance(transform.position, PlayState.player.transform.position) < 1.5f && !chatting && !needsSpace)
            {
                if (!PlayState.isTalking)
                {
                    int boxShape = 0;
                    string boxColor = "0005";
                    textToSend.Clear();
                    portraitColors.Clear();
                    portraitStateList.Clear();
                    switch (ID)
                    {
                        case 0:
                            if (!PlayState.CheckForItem("Peashooter"))
                                AddText("explainWallClimb");
                            else if (!PlayState.CheckForItem("Boomerang"))
                                AddText("explainPeashooter");
                            else if (!PlayState.CheckForItem("Rainbow Wave"))
                                AddText("explainBoomerang");
                            else if (!PlayState.CheckForItem("Devastator"))
                                AddText("explainRainbowWave");
                            else if (PlayState.GetItemPercentage() < 100)
                                AddText("explainDevastator");
                            else
                                AddText("default");
                            break;

                        case 1:
                            nexted = 1;
                            if (!PlayState.hasJumped && !PlayState.CheckForItem("Peashooter") && !PlayState.CheckForItem("Boomerang") && !PlayState.CheckForItem("Super Secret Boomerang"))
                            {
                                nexted = 0;
                                AddText("promptJump");
                            }
                            else if (!PlayState.CheckForItem("Peashooter"))
                                AddText("promptStory");
                            else if (PlayState.GetItemPercentage() < 100)
                                AddText("smallTalk");
                            else
                                AddText("default");
                            break;

                        case 2:
                            if (!PlayState.CheckForItem("Peashooter") && !PlayState.CheckForItem("Boomerang") && !PlayState.CheckForItem("Super Secret Boomerang"))
                                AddText("predictPeashooter");
                            else if (!PlayState.CheckBossState(0) && !PlayState.CheckBossState(1))
                                AddText("predictShellbreaker");
                            else if (!PlayState.CheckForItem("Boomerang"))
                                AddText("predictBoomerang");
                            else if (!PlayState.CheckBossState(1))
                                AddText("predictStompy");
                            else if (!PlayState.CheckForItem("Rainbow Wave"))
                                AddText("predictRainbowWave");
                            else if (!PlayState.CheckBossState(2))
                                AddText("predictSpaceBox");
                            else if (!PlayState.CheckForItem(6))
                                AddText("predictRapidFire");
                            else if (!PlayState.CheckBossState(3))
                                AddText("predictMoonSnail");
                            else if (PlayState.helixCount < 30)
                                AddText("predictHelixFragments");
                            else if (!PlayState.hasSeenIris)
                                AddText("predictIris");
                            else
                                AddText("default");
                            break;

                        case 3:
                            if (!PlayState.CheckForItem(8))
                                AddText("cantCorner");
                            else if (!PlayState.CheckForItem("Full-Metal Snail"))
                                AddText("admireGravity");
                            else
                                AddText("default");
                            break;

                        case 4:
                            if (PlayState.IsTileSolid(new Vector2(transform.position.x - 2.5f, transform.position.y)))
                                AddText("greenBlock");
                            else
                                AddText("default");
                            break;

                        case 5:
                            if (PlayState.GetItemPercentage() < 100)
                                AddText("secrets");
                            else
                                AddText("default");
                            break;

                        case 6:
                            if (PlayState.CheckForItem(4) || PlayState.CheckForItem(8))
                                AddText("treehouses");
                            else
                                AddText("default");
                            break;

                        case 7:
                            if (!PlayState.CheckForItem("Peashooter"))
                                AddText("save");
                            else if (!PlayState.CheckForItem(1))
                                AddText("peashooter");
                            else if (!PlayState.CheckForItem(2))
                                AddText("boomerang");
                            else if (!PlayState.CheckForItem(3))
                                AddText("rainbowWave");
                            else if (PlayState.CheckBossState(3))
                                AddText("scared");
                            else
                                AddText("default");
                            break;

                        case 8:
                            if (PlayState.IsTileSolid(new Vector2(transform.position.x + 8.5f, transform.position.y)))
                                AddText("suspicious");
                            else
                                AddText("default");
                            break;

                        case 9:
                            if (PlayState.itemPercentage < 100)
                                AddText("dirtHome");
                            else
                                AddText("default");
                            break;

                        case 10:
                            AddText("default");
                            break;

                        case 12:
                            if (!transform.parent.Find("Item").GetComponent<Item>().collected)
                                AddText("funBlocks");
                            else
                                AddText("default");
                            break;

                        case 13:
                            AddText("default");
                            break;

                        case 14:
                            if (PlayState.helixCount < 15)
                                AddText("helixFragments");
                            else if (PlayState.helixCount < 30 || !PlayState.hasSeenIris)
                                AddText("shrine");
                            else
                                AddText("default");
                            break;

                        case 15:
                            if (PlayState.GetItemPercentage() < 20)
                                AddText("hintSecret");
                            else if (PlayState.GetItemPercentage() < 40)
                            {
                                switch (playerName)
                                {
                                    case "Snaily":
                                        AddText("hintSnaily");
                                        break;
                                    case "Sluggy":
                                        AddText("hintSluggy");
                                        break;
                                    case "Upside":
                                        AddText("hintUpside");
                                        break;
                                    case "Leggy":
                                        AddText("hintLeggy");
                                        break;
                                    case "Blobby":
                                        AddText("hintBlobby");
                                        break;
                                    case "Leechy":
                                        AddText("hintLeechy");
                                        break;
                                }
                            }
                            else if (PlayState.GetItemPercentage() < 60)
                                AddText("hintMissedSecret");
                            else if (PlayState.GetItemPercentage() < 80)
                                AddText("hintEarlyHighJump");
                            else if (PlayState.GetItemPercentage() < 100)
                                AddText("hintSSB");
                            else
                                AddText("default");
                            break;

                        case 16:
                            if (!PlayState.CheckForItem("Peashooter") && !PlayState.CheckForItem("Boomerang") && !PlayState.CheckForItem("Super Secret Boomerang"))
                            {
                                if (playerName == "Leechy")
                                    AddText("healTipLeechy");
                                else
                                    AddText("healTipGeneric");
                            }
                            else if (transform.localPosition.y > origin.y - 21)
                                AddText("ride");
                            else
                                AddText("default");
                            break;

                        case 17:
                            if (PlayState.GetItemPercentage() < 100)
                                AddText("secret");
                            else
                                AddText("default");
                            break;

                        case 18:
                            if (PlayState.CheckForItem("Super Secret Boomerang"))
                                AddText("remindShoot");
                            else
                                AddText("default");
                            break;

                        case 19:
                            if (!PlayState.CheckForItem("Boomerang") && !PlayState.CheckForItem("Super Secret Boomerang"))
                                AddText("boomerang");
                            else
                                AddText("default");
                            break;

                        case 22:
                            if (PlayState.GetItemPercentage() < 100)
                                AddText("thorgleBorgle");
                            else
                                AddText("default");
                            PlayState.talkedToCaveSnail = true;
                            break;

                        case 23:
                            if (PlayState.GetItemPercentage() < 100 && !PlayState.talkedToCaveSnail)
                                AddText("caveSnail");
                            else if (PlayState.GetItemPercentage() < 60)
                                AddText("loadGame");
                            else
                                AddText("default");
                            break;

                        case 24:
                            if (!transform.parent.Find("Item").GetComponent<Item>().collected)
                                AddText("offerHelixFragment");
                            else
                                AddText("default");
                            break;

                        case 25:
                            if (!PlayState.CheckForItem("Peashooter"))
                                AddText("explainMap");
                            else
                                AddText("default");
                            break;

                        case 50:
                            if (PlayState.CheckForItem("Debug Rainbow Wave"))
                                AddText("admireRainbowWave");
                            else
                                AddText("default");
                            break;

                        case 51:
                            AddText("default");
                            break;

                        default:
                            break;
                    }
                    if (textToSend.Count == 0)
                        textToSend.Add(PlayState.GetText("npc_?"
                            .Replace("##", PlayState.GetItemPercentage().ToString())
                            .Replace("{P}", PlayState.GetText("char_" + PlayState.currentCharacter.ToLower()))
                            .Replace("{PF}", PlayState.GetText("char_full_" + PlayState.currentCharacter.ToLower()))
                            .Replace("{S}", PlayState.GetText("species_" + PlayState.currentCharacter.ToLower()))
                            .Replace("{SS}", PlayState.GetText("species_plural_" + PlayState.currentCharacter.ToLower()))
                            .Replace("{ID}", ID.ToString())));
                    if (textToSend.Count > 1)
                    {
                        speechBubble.GetComponent<SpriteRenderer>().enabled = true;
                        if (Control.SpeakPress())// && !buttonDown)
                        {
                            chatting = true;
                            PlayState.isTalking = true;
                            PlayState.paralyzed = true;
                            PlayState.OpenDialogue(3, ID, textToSend, boxShape, boxColor, portraitColors, portraitStateList, PlayState.player.transform.position.x < transform.position.x);
                        }
                    }
                    else
                    {
                        chatting = true;
                        PlayState.isTalking = true;
                        PlayState.OpenDialogue(2, ID, textToSend, boxShape, boxColor);
                    }
                }
                else
                    needsSpace = true;
            }
            else if (Vector2.Distance(transform.position, PlayState.player.transform.position) > 7 && chatting)
            {
                chatting = false;
                PlayState.CloseDialogue();
            }
            else if (Vector2.Distance(transform.position, PlayState.player.transform.position) > 7 && needsSpace)
                needsSpace = false;
            else if (Vector2.Distance(transform.position, PlayState.player.transform.position) > 1.5f && (!chatting || PlayState.paralyzed))
            {
                speechBubble.GetComponent<SpriteRenderer>().enabled = false;
            }

            switch (ID)
            {
                default:
                    break;
                case 1:
                    if (PlayState.hasJumped && nexted == 0)
                        Next();
                    break;
                case 4:
                    if (!PlayState.IsTileSolid(new Vector2(transform.position.x - 2.5f, transform.position.y)) && nexted == 0)
                        Next();
                    break;
                case 8:
                    if (!PlayState.IsTileSolid(new Vector2(transform.position.x + 8.5f, transform.position.y)) && nexted == 0)
                        Next();
                    break;
                case 16:
                    if (transform.localPosition.y < origin.y - 21 && nexted == 0)
                        Next();
                    break;
            }
        }
    }

    private void AddText(string textID)
    {
        bool locatedAll = false;
        int i = 0;
        while (!locatedAll)
        {
            string fullID = "npc_" + ID.ToString() + "_" + textID + "_" + i;
            string newText = PlayState.GetText(fullID);
            if (newText != fullID)
            {
                string finalText = newText
                    .Replace("##", PlayState.GetItemPercentage().ToString())
                    .Replace("{P}", PlayState.GetText("char_" + PlayState.currentCharacter.ToLower()))
                    .Replace("{PF}", PlayState.GetText("char_full_" + PlayState.currentCharacter.ToLower()))
                    .Replace("{S}", PlayState.GetText("species_" + PlayState.currentCharacter.ToLower()))
                    .Replace("{SS}", PlayState.GetText("species_plural_" + PlayState.currentCharacter.ToLower()))
                    .Replace("{ID}", ID.ToString());
                textToSend.Add(finalText);
                portraitStateList.Add(PlayState.GetTextInfo(fullID).value);
            }
            else
                locatedAll = true;
            i++;
        }
    }

    private void CreateNewSprites()
    {
        List<Sprite> newSprites = new List<Sprite>();

        for (int i = 0; i < PlayState.textureLibrary.library[Array.IndexOf(PlayState.textureLibrary.referenceList, "Entities/SnailNpc")].Length; i++)
        {
            newSprites.Add(PlayState.Colorize("Entities/SnailNpc", i, "Entities/SnailNpcColor", ID));
        }

        sprites = newSprites.ToArray();
    }

    public void ChangeSprite(int spriteID)
    {
        for (int i = 0; i < parts.Count; i++)
            parts[i].sprite = npcSpriteSheet[(6 * i) + spriteID];
    }

    public void Next()
    {
        nexted++;
        PlayState.CloseDialogue();
        chatting = false;
    }
}
