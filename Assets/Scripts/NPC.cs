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

    public List<SpriteRenderer> parts = new List<SpriteRenderer>();
    public Animator anim;
    public GameObject speechBubble;

    public Vector2 origin;

    private RaycastHit2D groundCheck;
    public float velocity;
    private const float GRAVITY = 1.35f;
    private const float TERMINAL_VELOCITY = -0.66f;

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

        colorTable = (Texture2D)Resources.Load("Images/Entities/SnailNpcColor");

        parts.Add(transform.GetComponent<SpriteRenderer>());
        parts.Add(transform.GetChild(0).GetComponent<SpriteRenderer>());
        parts.Add(transform.GetChild(1).GetComponent<SpriteRenderer>());
        parts.Add(transform.GetChild(2).GetComponent<SpriteRenderer>());
        parts.Add(transform.GetChild(3).GetComponent<SpriteRenderer>());
        speechBubble = transform.Find("Speech bubble").gameObject;

        parts[0].color = colorTable.GetPixel(0, ID);
        parts[1].color = colorTable.GetPixel(1, ID);
        parts[2].color = colorTable.GetPixel(2, ID);
        parts[3].color = colorTable.GetPixel(3, ID);
        parts[4].color = colorTable.GetPixel(4, ID);

        anim = GetComponent<Animator>();
        anim.Play("NPC base", 0, 0);

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
                List<string> textToSend = new List<string>();
                portraitColors.Clear();
                portraitStateList.Clear();
                switch (ID)
                {
                    case 0:
                        if (!PlayState.CheckForItem("Peashooter"))
                            textToSend.Add("Hi, " + playerName + "!!  Why don\'t you try\nclimbing up the walls?\n    Just hold \"UP\" and \"RIGHT\".");
                        else if (!PlayState.CheckForItem("Boomerang"))
                            textToSend.Add("Oh, nice pea shooter!  I heard\nyou can shoot a blue door open\nwith one of those!");
                        else if (!PlayState.CheckForItem("Rainbow Wave"))
                            textToSend.Add("Wow, a boomerang!  You could\nbreak a pink door open with\njust one of those!");
                        else if (!PlayState.CheckForItem("Devastator"))
                            textToSend.Add("Is that a rainbow wave!?  Well,\nthen!  You can open a red door\nwith one of those!");
                        else if (PlayState.GetItemPercentage() < 100)
                            textToSend.Add("A devastator!?  That opens up\ngreen doors!  It also upgrades\nall three weapons!  Wow!!");
                        else
                            textToSend.Add("I hope the next game has more\nweapons.  This game could have\nused a flame whip!");
                        break;

                    case 1:
                        if (!PlayState.hasJumped && !PlayState.CheckForItem("Peashooter") && !PlayState.CheckForItem("Boomerang") && !PlayState.CheckForItem("Super Secret Boomerang"))
                            textToSend.Add("Hiya, " + playerName + "!  Did you know you\ncan jump?  Just press \"Z\"!");
                        else if (!PlayState.CheckForItem("Peashooter"))
                            textToSend.Add(playerName + ", some snails are missing!\nDo you think you could go look\nfor them?  I\'m getting worried!");
                        else if (PlayState.GetItemPercentage() < 100)
                            textToSend.Add("I have a goal in life.  One day,\nI will eat a pizza.  I mean it!!\nJust you watch, " + playerName + "!!");
                        else
                            textToSend.Add(playerName + ", you missed it!  I made a\ndelicious pizza, and I ate the\nwhole thing!!!  Om nom nom!");
                        break;

                    case 2:
                        if (!PlayState.CheckForItem("Peashooter") && !PlayState.CheckForItem("Boomerang"))
                            textToSend.Add("I can see the future, " + playerName + "!\nI see you finding a weapon\nsomewhere under the water!");
                        else if (!PlayState.CheckBossState(0) && !PlayState.CheckBossState(1))
                            textToSend.Add("I can see the future, " + playerName + "!\nI see you heading to the upper\nright part of the map!!");
                        else if (!PlayState.CheckForItem("Boomerang"))
                            textToSend.Add("I can see the past, " + playerName + "!\nI see you forgot to grab the\nboomerang after Shellbreaker!");
                        else if (!PlayState.CheckBossState(1))
                            textToSend.Add("I can see the future, " + playerName + "!\nI see you heading to the lower\nright part of the map!!");
                        else if (!PlayState.CheckForItem("Rainbow Wave"))
                            textToSend.Add("I can see the past, " + playerName + "!\nI see you forgot to grab the\nrainbow wave after Stompy!");
                        else if (!PlayState.CheckBossState(2))
                            textToSend.Add("I can see the future, " + playerName + "!\nI see you heading to the lower\nleft part of the map!!");
                        else if (!PlayState.CheckForItem(6))
                            textToSend.Add("I can see the past, " + playerName + "!\nI see you forgot to grab the\nrapid fire before Space Box!");
                        else if (!PlayState.CheckBossState(3))
                            textToSend.Add("I can see the future, " + playerName + "!\nI see you heading to the upper\nleft part of the map!!");
                        else if (PlayState.helixCount < 30)
                            textToSend.Add("I can see the future, " + playerName + "!\nI see you heading all over the\nmap, finding little shells!");
                        else if (!PlayState.hasSeenIris)
                            textToSend.Add("I can see the future, " + playerName + "!\nI see you heading precisely two\nscreens to the left of here!!");
                        else
                            textToSend.Add("I can see the future, " + playerName + "!\nI see you having 18,000,000\nbaby " +
                                playerSpecies + (playerSpecies == "Leech" ? "e" : "") + "s!!  Congratulations!!");
                        break;

                    case 3:
                        if (!PlayState.CheckForItem(8))
                            textToSend.Add("I wonder why I can\'t crawl on\nceiling corners...  Do you think\nI\'ll ever be able to, " + playerName + "?");
                        else if (!PlayState.CheckForItem("Full-Metal Snail"))
                            textToSend.Add("Oh, my, you\'re a gravity " + playerSpecies + "!\nYou must be really good at\ncrawling around ceilings!!");
                        else
                            textToSend.Add("Oh, my, you\'re a full metal\n" + playerSpecies + "! You must be really good\nat crawling around ceilings!!");
                        break;

                    case 4:
                        if (PlayState.IsTileSolid(new Vector2(transform.position.x - 2.5f, transform.position.y)))
                            textToSend.Add("I wish I had some way to break\ngreen blocks.  Those suckers\nare always getting in my way!!");
                        else
                            textToSend.Add("Whew!  Thanks, " + playerName + "!  I think\nthose blocks were planning to\nattack!!  You saved the day!!");
                        break;

                    case 5:
                        if (PlayState.GetItemPercentage() < 100)
                            textToSend.Add("There\'s a lot of secrets\nin and around Snail Town.\nHave you found them all?");
                        else
                            textToSend.Add("They say Boss Rush is the\ntrue test of snail skill.\nBut what about slug skill?");
                        break;

                    case 6:
                        if (PlayState.CheckForItem(4) || PlayState.CheckForItem(8))
                            textToSend.Add("Don\'t you think it\'s weird that\nwe all live in treehouses?");
                        else
                            textToSend.Add("Hey " + playerName + ", how\'d you get up\nhere?  And how do I get down?");
                        break;

                    case 7:
                        if (!PlayState.CheckForItem("Peashooter"))
                            textToSend.Add("Are you leaving town, " + playerName + "?\nWell, be careful!  Make sure\nyou save your game often!!");
                        else if (!PlayState.CheckForItem(1))
                            textToSend.Add("Hey, " + playerName + "!  Where\'d you get\nthe pea shooter?");
                        else if (!PlayState.CheckForItem(2))
                            textToSend.Add("Ooh, boomerangs!  If I had\nthose, I\'d try breaking the\nceiling over the save spot!");
                        else if (!PlayState.CheckForItem(3))
                            textToSend.Add("Rainbows are so pretty!\nDon\'t you think so, " + playerName + "?");
                        else if (PlayState.CheckBossState(3))
                            textToSend.Add("I\'m scared, " + playerName + "!\nIs Moon Snail going to take\nme away like the others?");
                        else
                            textToSend.Add("Snail Town is safe again,\nthanks to you, " + playerName + "!");
                        break;

                    case 9:
                        if (PlayState.itemPercentage < 100)
                            textToSend.Add("The other snails live in houses,\nbut I like it here in the dirt.\nIsn\'t it nice in here?");
                        else
                            textToSend.Add("It\'s so cozy in here!  I just\nlove my little underground\nhome!");
                        break;

                    case 10:
                        textToSend.Add("Oh, " + playerFullName + "!  My heart\nwill forever belong to you!\n             <3\n");
                        break;

                    case 14:
                        if (PlayState.helixCount < 15)
                            textToSend.Add("Hey, " + playerName + "!  Keep an eye out\nfor \"Helix Fragments\".  They\nlook like spinning white shells!");
                        else if (PlayState.helixCount < 30 || !PlayState.hasSeenIris)
                            textToSend.Add("They say that the Shrine of\nIris is not located on any map!");
                        else
                            textToSend.Add("Wow, " + playerName + "!  You\'re pretty good\nat finding secrets!  Maybe you\nshould become a detective!");
                        break;

                    case 15:
                        if (PlayState.GetItemPercentage() < 20)
                            textToSend.Add("Hi, " + playerName + "!  Let me give you a\nhint: Come back to town after\neach area to find secret items!");
                        else if (PlayState.GetItemPercentage() < 40)
                        {
                            switch (playerName)
                            {
                                case "Snaily":
                                case "Upside":
                                    textToSend.Add("Hi, " + playerName + "!  Let me give you a\nlittle hint: A shell can fit\nwhere a snail cannot!");
                                    break;
                                case "Sluggy":
                                    textToSend.Add("Hi, " + playerName + "!  Here\'s a li\'l hint:\nSlugs may be more fragile than\nsnails, but they\'re also faster!");
                                    break;
                                case "Leggy":
                                    textToSend.Add("Hi, " + playerName + "!  Here's a tip: It may\nlook too big, but your shell\ncan fit in places ours can!");
                                    break;
                                case "Blobby":
                                    textToSend.Add("Hi, " + playerName + "!  Here's a little tip:\nYou can still jump after a side\nhop!  It\'s called \"Coyote Time\"!!");
                                    break;
                                case "Leechy":
                                    textToSend.Add("Hi, " + playerName + "!  Here\'s a li\'l hint:\nLeeches may be more fragile than\nsnails, but they\'re also faster!");
                                    break;
                            }
                        }
                        else if (PlayState.GetItemPercentage() < 60)
                            textToSend.Add("Hi, " + playerName + "!  Let me give you a\nhint: Sometimes, you\'ll miss a\nsecret if you cling to a wall!");
                        else if (PlayState.GetItemPercentage() < 80)
                            textToSend.Add("Hi, " + playerName + "!  Let me give you a\nhint: You can find the \"high\njump\" before any other item!");
                        else if (PlayState.GetItemPercentage() < 100)
                            textToSend.Add("Hi, " + playerName + "!  Let me tell you a\nsecret: There\'s a boomerang\nsomewhere in Snail Town!");
                        else
                            textToSend.Add("Hi, " + playerName + "!  Let me tell you a\nsecret: there\'s a secret\nsomewhere in the main menu!");
                        break;

                    case 16:
                        if (!PlayState.CheckForItem("Peashooter") && !PlayState.CheckForItem("Boomerang") && !PlayState.CheckForItem("Super Secret Boomerang"))
                        {
                            if (playerName == "Leechy")
                                textToSend.Add("If you ever get hurt, defeat\nenemies!  As a leech, you can\nearn red heath orbs off them!");
                            else
                                textToSend.Add("If you ever get hurt, eat some\nplants!  You need to eat well\nto stay fit and healthy!");
                        }
                        else if (transform.localPosition.y > origin.y - 21)
                            textToSend.Add("I want to go for a ride!!!");
                        else
                            textToSend.Add("WHEEEE!!  That was fun,\n" + playerName + "!  Let\'s do it again!!");
                        break;

                    case 17:
                        if (PlayState.GetItemPercentage() < 100)
                            textToSend.Add("There\'s a hidden room under the\npath into town.  I don\'t know\nhow to get in yet, though...");
                        else
                            textToSend.Add("They call me upside-down snail,\nbut I think everyone else is\nupside down!");
                        break;

                    case 18:
                        if (PlayState.CheckForItem("Super Secret Boomerang"))
                            textToSend.Add("Don\'t forget!\nPress \"X\" to shoot your\nweapon at stuff!!");
                        else
                            textToSend.Add("You found the super secret\nboomerang!  Way to go!\nPress \"X\" to shoot with it!");
                        break;

                    case 19:
                        if (!PlayState.CheckForItem("Boomerang"))
                            textToSend.Add("Hey, " + playerName + "! If you had a\nboomerang, you could break\nall sorts of walls!");
                        else
                            textToSend.Add("Up, up, down, down, left,\nright...  Wait, never mind,\nthat\'s for some other game.");
                        break;

                    case 25:
                        if (PlayState.CheckForItem("Peashooter"))
                            textToSend.Add("Have you tried hitting \"M\"\nyet?  It makes the map big!  Oh,\nand hit \"ESC\" for the menu!");
                        else if (PlayState.GetItemPercentage() < 60)
                            textToSend.Add(playerName + ", if you need to see the\ncontrols again, just press \"F1\"!\nBy the way, try hitting \"M\"!");
                        else
                            textToSend.Add("Hey " + playerName + ", I\'m hungry!  Know\nany good plants around town?");
                        break;

                    case 50:
                        if (PlayState.CheckForItem("Rainbow Wave") || PlayState.CheckForItem("Debug Rainbow Wave"))
                            textToSend.Add("Woah!!  Nice Rainbow Wave, " + playerName + "!!\nI\'d love one too, but I don\'t\nhave a jump button.");
                        else
                            textToSend.Add("Oh, hey, " + playerName + "! I'm here to test\nsingle-page dialogue!!");
                        break;

                    case 51:
                        AddNPCColors(ID);
                        textToSend.Add("Hey there, " + playerName + "!! I see you\nfigured out how to start a\nmulti-page conversation!");
                        portraitStateList.Add(1);
                        textToSend.Add("The hope is this talk should go\n100% smoothly. What do you\nthink?");
                        portraitStateList.Add(1);
                        textToSend.Add("Impressive! I do hope that\'s my\nportrait showing right now, if it\neven is there.");
                        portraitStateList.Add(0);
                        textToSend.Add("I\'m here to test multiple things,\nit seems!");
                        portraitStateList.Add(1);
                        break;

                    default:
                        textToSend.Add("Hey " + playerName + "!  Unfortunately I,\nsnail #" + ID + ", don\'t have any\ndialogue to offer.  Sorry!!");
                        break;
                }
                if (textToSend.Count > 1)
                {
                    speechBubble.GetComponent<SpriteRenderer>().enabled = true;
                    if (Input.GetAxisRaw("Speak") == 1 && !buttonDown)
                    {
                        chatting = true;
                        PlayState.isTalking = true;
                        PlayState.paralyzed = true;
                        PlayState.OpenDialogue(3, ID, textToSend, portraitColors, portraitStateList, PlayState.player.transform.position.x < transform.position.x);
                    }
                }
                else
                {
                    chatting = true;
                    PlayState.isTalking = true;
                    PlayState.OpenDialogue(2, ID, textToSend);
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

        if (Input.GetAxisRaw("Speak") == 1)
        {
            buttonDown = true;
        }
        else
        {
            buttonDown = false;
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
            case 16:
                if (transform.localPosition.y < origin.y - 21 && nexted == 0)
                    Next();
                break;
        }
    }

    private void AddNPCColors(int inputID)
    {
        portraitColors.Add(colorTable.GetPixel(0, inputID));
        portraitColors.Add(colorTable.GetPixel(1, inputID));
        portraitColors.Add(colorTable.GetPixel(2, inputID));
        portraitColors.Add(colorTable.GetPixel(3, inputID));
        portraitColors.Add(colorTable.GetPixel(4, inputID));
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
