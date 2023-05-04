using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC:MonoBehaviour, IRoomObject, ICutsceneObject {
    #region vars
    public int ID = 0;
    public int lookMode = 0;
    public bool upsideDown = false;
    public string nameID = "pleaseNameMe";
    public int animationSet = 0;

    public bool chatting = false;
    public bool needsSpace = false; // On the off chance that two snails are close enough to each other to trigger simultaneously, like 06 and 17
    public bool buttonDown = false;
    public List<Color32> colors = new List<Color32>();
    public List<int> portraitStateList = new List<int>();         // 0 for the player, any other positive number for whatever other NPC is speaking
    public Texture2D colorTable;
    public Sprite[] npcSpriteSheet;
    public Sprite[] sprites;
    public SpriteRenderer sprite;
    public AnimationModule anim;
    public GameObject speechBubble;
    public SpriteRenderer speechBubbleSprite;
    public AnimationModule speechBubbleAnim;
    public bool bubbleState = false;
    public Vector2 origin;
    public float velocity;
    public List<string> textToSend = new List<string>();

    private int nexted = 0;
    private RaycastHit2D groundCheck;
    private const float GRAVITY = 1.25f;
    private const float TERMINAL_VELOCITY = -0.5208f;
    #endregion vars

    #region cutscene
    public void cutRegister() {

    }

    public void cutStart() {

    }

    public void cutEnd() {

    }
    #endregion cutscene

    public Dictionary<string, object> resave() {
        return null;
    }

    public string myType = "NPC";

    public string objType {
        get {
            return myType;
        }
    }

    public Dictionary<string, object> save() {
        Dictionary<string, object> content = new Dictionary<string, object>();
        content["ID"] = ID;
        content["lookMode"] = lookMode;
        content["upsideDown"] = upsideDown;
        content["nameID"] = nameID;
        content["animationSet"] = animationSet;
        return content;
    }

    public void load(Dictionary<string, object> content) {
        ID = (int)content["ID"];
        lookMode = (int)content["lookMode"];
        upsideDown = (bool)content["upsideDown"];
        nameID = (string)content["nameID"];
        animationSet = (int)content["animationSet"];
        Spawn();
    }

    public virtual void Awake() {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        speechBubble = transform.Find("Speech bubble").gameObject;
        speechBubbleSprite = speechBubble.GetComponent<SpriteRenderer>();
        speechBubbleAnim = speechBubble.GetComponent<AnimationModule>();

        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<AnimationModule>();
        anim.updateSprite = false;

        nexted = 0;
        chatting = false;
        speechBubbleSprite.enabled = false;

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

    public virtual void Spawn() {
        CreateNewSprites();
        anim.Add("NPC_" + animationSet + "_idle");
        anim.Add("NPC_" + animationSet + "_shell");
        anim.Add("NPC_" + animationSet + "_sleep");
        if (ID == 26 && (PlayState.currentCharacter == "Sluggy" || PlayState.currentCharacter == "Leechy")) {
            anim.Play("NPC_" + animationSet + "_sleep");
            sprite.flipX = PlayState.player.transform.position.x < transform.position.x;
        } else
            anim.Play("NPC_" + animationSet + "_idle");
        if (upsideDown) {
            sprite.flipY = true;
            speechBubbleSprite.flipY = true;
            speechBubble.transform.localPosition = new Vector2(0, -0.75f);
        }
    }

    public virtual void FixedUpdate() {
        groundCheck = Physics2D.BoxCast(
            transform.position,
            new Vector2(1, 0.98f),
            0,
            velocity > 0 ? Vector2.up : Vector2.down,
            Mathf.Infinity,
            LayerMask.GetMask("PlayerCollide"),
            Mathf.Infinity,
            Mathf.Infinity
            );
        if (groundCheck.distance != 0 && groundCheck.distance > 0.01f) {
            if (upsideDown) {
                velocity = Mathf.Clamp(velocity + GRAVITY * Time.fixedDeltaTime, -Mathf.Infinity, -TERMINAL_VELOCITY);
            } else {
                velocity = Mathf.Clamp(velocity - GRAVITY * Time.fixedDeltaTime, TERMINAL_VELOCITY, Mathf.Infinity);
            }
            bool resetVelFlag = false;
            if (Mathf.Abs(velocity) > Mathf.Abs(groundCheck.distance)) {
                RaycastHit2D groundCheckRay = Physics2D.Raycast(
                    new Vector2(groundCheck.point.x, transform.position.y + (upsideDown ? 0.5f : -0.5f)),
                    velocity > 0 ? Vector2.up : Vector2.down,
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
        } else {
            velocity = 0;
        }
    }

    public virtual void Update() {
        if (PlayState.gameState == PlayState.GameState.game) {
            if (anim.isPlaying)
                sprite.sprite = sprites[anim.GetCurrentFrameValue()];

            if (!PlayState.cutsceneActive) {
                if (lookMode == 0) {
                    if (PlayState.player.transform.position.x < transform.position.x && anim.currentAnimName != "NPC_sleep") {
                        sprite.flipX = true;
                        speechBubbleSprite.flipX = false;
                    } else {
                        sprite.flipX = false;
                        speechBubbleSprite.flipX = true;
                    }
                } else if (lookMode == 1) {
                    sprite.flipX = true;
                    speechBubbleSprite.flipX = false;
                } else {
                    sprite.flipX = false;
                    speechBubbleSprite.flipX = true;
                }
            }

            if (Vector2.Distance(transform.position, PlayState.player.transform.position) < 1.5f && !chatting && !needsSpace) {
                if (!PlayState.isTalking) {
                    int boxShape = 0;
                    string boxColor = "0005";
                    textToSend.Clear();
                    portraitStateList.Clear();
                    bool intentionallyEmpty = false;
                    switch (ID) {
                        case 0:
                            if (!PlayState.CheckForItem("Peashooter") && !PlayState.CheckForItem("Boomerang") && !PlayState.CheckForItem("Super Secret Boomerang"))
                                AddText("explainWallClimb");
                            else if (!PlayState.CheckForItem("Boomerang") && !PlayState.CheckForItem("Super Secret Boomerang"))
                                AddText("explainPeashooter");
                            else if (!PlayState.CheckForItem("Rainbow Wave") && !PlayState.CheckForItem("Debug Rainbow Wave"))
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
                            if (!PlayState.hasJumped && !PlayState.CheckForItem("Peashooter") && !PlayState.CheckForItem("Boomerang") && !PlayState.CheckForItem("Super Secret Boomerang")) {
                                nexted = 0;
                                AddText("promptJump");
                            } else if (!PlayState.CheckForItem("Peashooter"))
                                AddText("promptStory");
                            else if (PlayState.GetItemPercentage() < 100)
                                AddText("smallTalk");
                            else
                                AddText("default");
                            break;

                        case 2:
                            if (!PlayState.CheckForItem("Peashooter") && !PlayState.CheckForItem("Boomerang") && !PlayState.CheckForItem("Super Secret Boomerang"))
                                AddText("predictPeashooter");
                            else if (PlayState.IsBossAlive(0) && PlayState.IsBossAlive(1))
                                AddText("predictShellbreaker");
                            else if (!PlayState.CheckForItem("Boomerang") && !PlayState.CheckForItem("Super Secret Boomerang"))
                                AddText("predictBoomerang");
                            else if (PlayState.IsBossAlive(1))
                                AddText("predictStompy");
                            else if (!PlayState.CheckForItem("Rainbow Wave"))
                                AddText("predictRainbowWave");
                            else if (PlayState.IsBossAlive(2))
                                AddText("predictSpaceBox");
                            else if (!PlayState.CheckForItem(6))
                                AddText("predictRapidFire");
                            else if (PlayState.IsBossAlive(3))
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
                            else if (PlayState.IsBossAlive(3))
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

                        case 11:
                            if (!PlayState.CheckForItem("Peashooter"))
                                AddText("explainPeashooter");
                            else
                                AddText("default");
                            break;

                        case 12:
                            if (CheckForUncollectedItem(PlayState.OFFSET_HEARTS))
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
                            else if (PlayState.GetItemPercentage() < 40) {
                                switch (PlayState.currentCharacter) {
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
                            } else if (PlayState.GetItemPercentage() < 60)
                                AddText("hintMissedSecret");
                            else if (PlayState.GetItemPercentage() < 80)
                                AddText("hintEarlyHighJump");
                            else if (PlayState.GetItemPercentage() < 100)
                                AddText("hintSSB");
                            else
                                AddText("default");
                            break;

                        case 16:
                            if (!PlayState.CheckForItem("Peashooter") && !PlayState.CheckForItem("Boomerang") && !PlayState.CheckForItem("Super Secret Boomerang")) {
                                if (PlayState.currentCharacter == "Leechy")
                                    AddText("healTipLeechy");
                                else
                                    AddText("healTipGeneric");
                            } else if (transform.localPosition.y > origin.y - 21)
                                AddText("ride");
                            else
                                AddText("default");
                            break;

                        case 17:
                            boxShape = PlayState.GetAnim("Dialogue_characterShapes").frames[2];
                            boxColor = PlayState.ParseColorCodeToString(PlayState.GetAnim("Dialogue_characterColors").frames[2]);
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

                        case 20:
                            if (!PlayState.CheckForItem("Boomerang") && !PlayState.CheckForItem("Super Secret Boomerang"))
                                AddText("secret");
                            else if (PlayState.GetItemPercentage() < 100)
                                AddText("findSnails");
                            else
                                AddText("default");
                            break;

                        case 21:
                            if (!PlayState.CheckForItem("Boomerang"))
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
                            if (CheckForUncollectedItem(PlayState.OFFSET_FRAGMENTS))
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

                        case 26:
                            if (PlayState.currentCharacter == "Blobby")
                                AddText("blobby");
                            else if (PlayState.currentCharacter == "Snaily" || PlayState.currentCharacter == "Upside" || PlayState.currentCharacter == "Leggy")
                                AddText("default");
                            else
                                intentionallyEmpty = true;
                            break;

                        case 27:
                            if (PlayState.CheckForItem("Gravity Snail") || PlayState.CheckForItem("Full-Metal Snail"))
                                AddText("powerfulPlayer");
                            else if (PlayState.CheckForItem("Ice Snail"))
                                AddText("icyPlayer");
                            else
                                AddText("default");
                            break;

                        case 29:
                            boxColor = "0002";
                            if (PlayState.currentDifficulty == 2)
                                AddText("insane");
                            else
                                AddText("default");
                            break;

                        case 35:
                            boxColor = "0002";
                            AddText("default");
                            break;

                        case 40:
                            if (PlayState.IsBossAlive(2))
                                AddText("warnAboutStompy");
                            else
                                AddText("default");
                            break;

                        case 41:
                            if (PlayState.IsBossAlive(0))
                                AddText("warnAboutSB");
                            else if (!PlayState.CheckForItem("Boomerang"))
                                AddText("greyDoor");
                            else if (PlayState.IsBossAlive(3))
                                AddText(PlayState.currentCharacter switch { "Snaily" => "babySnails", "Upside" => "babySnails", _ => "goodLuck" });
                            else
                                AddText("default");
                            break;

                        case 43:
                            if (!PlayState.CheckForItem("Gravity Snail"))
                                AddText("cantCorner");
                            else if (PlayState.currentCharacter == "Upside")
                                AddText("upside");
                            else if (PlayState.currentCharacter == "Leggy")
                                AddText("leggy");
                            else if (PlayState.currentCharacter == "Blobby")
                                AddText("blobby");
                            else
                                AddText("default");
                            break;

                        case 47:
                            AddText("default");
                            break;

                        case 48:
                            if (nexted == 0)
                                AddText("greet");
                            else if (nexted == 1)
                                AddText("scream");
                            else
                                AddText("default");
                            break;

                        case 50:
                            AddText("default");
                            break;

                        case 51:
                            if (PlayState.CheckForItem("Debug Rainbow Wave"))
                                AddText("admireRainbowWave");
                            else
                                AddText("default");
                            break;

                        case 52:
                            AddText("default");
                            break;

                        case 53:
                            AddText("default");
                            break;

                        case 54:
                            AddText("default");
                            break;

                        case 55:
                            if (nexted == 0) {
                                AddText("default");
                                nexted++;
                            } else
                                AddText("second");
                            break;

                        case 56:
                            boxShape = PlayState.GetAnim("Dialogue_characterShapes").frames[3];
                            boxColor = PlayState.ParseColorCodeToString(PlayState.GetAnim("Dialogue_characterColors").frames[3]);
                            AddText("default");
                            break;

                        default:
                            AddText("?");
                            break;
                    }
                    if (intentionallyEmpty)
                        return;
                    if (textToSend.Count == 0)
                        textToSend.Add(PlayState.GetText("npc_?")
                            .Replace("##", PlayState.GetItemPercentage().ToString())
                            .Replace("{P}", PlayState.GetText("char_" + PlayState.currentCharacter.ToLower()))
                            .Replace("{PF}", PlayState.GetText("char_full_" + PlayState.currentCharacter.ToLower()))
                            .Replace("{S}", PlayState.GetText("species_" + PlayState.currentCharacter.ToLower()))
                            .Replace("{SS}", PlayState.GetText("species_plural_" + PlayState.currentCharacter.ToLower()))
                            .Replace("{ID}", ID.ToString()));
                    if (textToSend.Count > 1) {
                        if (!speechBubbleSprite.enabled)
                            speechBubbleSprite.enabled = true;
                        ToggleBubble(true);
                        if (Control.SpeakPress()) {
                            chatting = true;
                            PlayState.isTalking = true;
                            PlayState.paralyzed = true;
                            PlayState.OpenDialogue(3, ID, textToSend, boxShape, boxColor, portraitStateList, PlayState.player.transform.position.x < transform.position.x);
                        }
                    } else {
                        chatting = true;
                        PlayState.isTalking = true;
                        PlayState.OpenDialogue(2, ID, textToSend, boxShape, boxColor);
                    }
                } else
                    needsSpace = true;
            } else if (Vector2.Distance(transform.position, PlayState.player.transform.position) > 7 && chatting) {
                chatting = false;
                PlayState.CloseDialogue();
            } else if (Vector2.Distance(transform.position, PlayState.player.transform.position) > 7 && needsSpace)
                needsSpace = false;
            else if (Vector2.Distance(transform.position, PlayState.player.transform.position) > 1.5f && (!chatting || PlayState.paralyzed)) {
                ToggleBubble(false);
            }

            switch (ID) {
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
                case 48:
                    if (transform.localPosition.y < origin.y - 2 && nexted == 0)
                        Next();
                    if (transform.localPosition.y < origin.y - 35 && nexted == 1)
                        Next();
                    break;
            }
        }
    }

    public virtual void AddText(string textID) {
        if (textID == "?") {
            textToSend.Add(PlayState.GetText("npc_?")
                    .Replace("##", PlayState.GetItemPercentage().ToString())
                    .Replace("{P}", PlayState.GetText("char_" + PlayState.currentCharacter.ToLower()))
                    .Replace("{PF}", PlayState.GetText("char_full_" + PlayState.currentCharacter.ToLower()))
                    .Replace("{S}", PlayState.GetText("species_" + PlayState.currentCharacter.ToLower()))
                    .Replace("{SS}", PlayState.GetText("species_plural_" + PlayState.currentCharacter.ToLower()))
                    .Replace("{ID}", ID.ToString()));
            portraitStateList.Add(PlayState.GetTextInfo("npc_?").value);
        } else {
            bool locatedAll = false;
            int i = 0;
            while (!locatedAll) {
                string fullID = "npc_" + ID.ToString() + "_" + textID + "_" + i;
                string newText = PlayState.GetText(fullID);
                if (newText != fullID) {
                    string finalText = newText
                        .Replace("##", PlayState.GetItemPercentage().ToString())
                        .Replace("{P}", PlayState.GetText("char_" + PlayState.currentCharacter.ToLower()))
                        .Replace("{PF}", PlayState.GetText("char_full_" + PlayState.currentCharacter.ToLower()))
                        .Replace("{S}", PlayState.GetText("species_" + PlayState.currentCharacter.ToLower()))
                        .Replace("{SS}", PlayState.GetText("species_plural_" + PlayState.currentCharacter.ToLower()))
                        .Replace("{ID}", ID.ToString());
                    textToSend.Add(finalText);
                    portraitStateList.Add(PlayState.GetTextInfo(fullID).value);
                } else
                    locatedAll = true;
                i++;
            }
        }
    }

    private void CreateNewSprites() {
        List<Sprite> newSprites = new List<Sprite>();

        for (int i = 0; i < PlayState.textureLibrary.library[Array.IndexOf(PlayState.textureLibrary.referenceList, "Entities/SnailNpc")].Length; i++) {
            newSprites.Add(PlayState.Colorize("Entities/SnailNpc", i, "Entities/SnailNpcColor", ID));
        }

        sprites = newSprites.ToArray();
    }

    public void Next() {
        nexted++;
        PlayState.CloseDialogue();
        chatting = false;
    }

    public void ToggleBubble(bool state) {
        if (speechBubbleAnim.animList.Count == 0) {
            speechBubbleAnim.Add("NPC_bubble_open");
            speechBubbleAnim.Add("NPC_bubble_close");
        }
        if (state && !bubbleState) {
            speechBubbleSprite.enabled = true;
            speechBubbleAnim.Play("NPC_bubble_open");
            bubbleState = true;
        } else if (!state && bubbleState) {
            speechBubbleAnim.Play("NPC_bubble_close");
            bubbleState = false;
        }
    }

    private bool CheckForUncollectedItem(int ID, bool strictIDCheck = false)
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        if (items.Length == 0)
            return false;
        bool foundItem = false;
        foreach (GameObject obj in items)
        {
            Item objScript = obj.GetComponent<Item>();
            if (strictIDCheck)
            {
                if (objScript.itemID == ID)
                    foundItem = true;
            }
            else
            {
                if (objScript.itemID >= PlayState.OFFSET_FRAGMENTS && ID >= PlayState.OFFSET_FRAGMENTS)
                    foundItem = true;
                else if (objScript.itemID >= PlayState.OFFSET_HEARTS && ID >= PlayState.OFFSET_HEARTS)
                    foundItem = true;
                else if (objScript.itemID == ID)
                    foundItem = true;
            }
        }
        return foundItem;
    }
}
