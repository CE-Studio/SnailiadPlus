using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public int ID = 0;
    public bool upsideDown = false;
    private bool chatting = false;
    private bool buttonDown = false;

    public string playerName;

    public List<Color32> colors = new List<Color32>();
    public List<int> IDsWithLongDialogue = new List<int>();

    private List<Color32> savedColors = new List<Color32>();
    private List<Color32> portraitColors = new List<Color32>();

    public SpriteRenderer outline;
    public SpriteRenderer body;
    public SpriteRenderer shell;
    public Animator outlineAnim;
    public Animator bodyAnim;
    public Animator shellAnim;
    public GameObject speechBubble;

    public GameObject player;

    void Start()
    {
        playerName = "Snaily";

        colors.Add(new Color32(96, 96, 96, 255));
        colors.Add(new Color32(220, 0, 212, 255));
        colors.Add(new Color32(0, 0, 0, 0));
        colors.Add(new Color32(220, 0, 212, 255));
        colors.Add(new Color32(96, 96, 96, 255));
        colors.Add(new Color32(0, 0, 0, 0));

        outline = GetComponent<SpriteRenderer>();
        body = transform.Find("NPC body").GetComponent<SpriteRenderer>();
        shell = transform.Find("NPC shell").GetComponent<SpriteRenderer>();
        outlineAnim = GetComponent<Animator>();
        bodyAnim = transform.Find("NPC body").GetComponent<Animator>();
        shellAnim = transform.Find("NPC shell").GetComponent<Animator>();
        speechBubble = transform.Find("Speech bubble").gameObject;
        player = GameObject.FindWithTag("Player");

        body.color = colors[ID * 3];
        shell.color = colors[ID * 3 + 1];

        savedColors.Add(colors[ID * 3 + 1]);
        savedColors.Add(colors[ID * 3]);
        savedColors.Add(colors[ID * 3 + 2]);

        IDsWithLongDialogue.Add(1);

        if (upsideDown)
        {
            outline.flipY = true;
            body.flipY = true;
            shell.flipY = true;
            speechBubble.GetComponent<SpriteRenderer>().flipY = true;
            speechBubble.transform.localPosition = new Vector2(0, -0.75f);
        }
        speechBubble.GetComponent<SpriteRenderer>().enabled = false;
    }

    void Update()
    {
        if (player.transform.position.x < transform.position.x)
        {
            outline.flipX = true;
            body.flipX = true;
            shell.flipX = true;
            speechBubble.GetComponent<SpriteRenderer>().flipX = false;
        }
        else
        {
            outline.flipX = false;
            body.flipX = false;
            shell.flipX = false;
            speechBubble.GetComponent<SpriteRenderer>().flipX = true;
        }

        if (PlayState.gameState != "Game")
        {
            outlineAnim.speed = 0;
            bodyAnim.speed = 0;
            shellAnim.speed = 0;
        }
        else
        {
            outlineAnim.speed = 1;
            bodyAnim.speed = 1;
            shellAnim.speed = 1;
        }

        if (Vector2.Distance(transform.position, player.transform.position) < 3 && !chatting)
        {
            List<string> textToSend = new List<string>();
            portraitColors.Clear();
            if (IDsWithLongDialogue.Contains(ID))
            {
                speechBubble.GetComponent<SpriteRenderer>().enabled = true;
                if (Input.GetAxisRaw("Shoot") == 1 && !buttonDown)
                {
                    chatting = true;
                    PlayState.paralyzed = true;
                    switch (ID)
                    {
                        case 1:
                            textToSend.Add("Hey there, " + playerName + "!! I see you\nfigured out how to start a\nmulti-page conversation!");
                            AddColorsNPC();
                            textToSend.Add("The hope is this talk should go\n100% smoothly. What do you\nthink?");
                            AddColorsNPC();
                            textToSend.Add("Impressive! I do hope that\'s my\nportrait showing right now, if it\neven is there.");
                            AddColorsPlayer();
                            textToSend.Add("I\'m here to test multiple things,\nit seems!");
                            AddColorsNPC();
                            break;
                        default:
                            textToSend.Add("Hey " + playerName + "!  Unfortunately I,\nsnail #" + ID + ", don\'t have any\ndialogue to offer.");
                            AddColorsNPC();
                            break;
                    }
                    PlayState.OpenDialogue(3, ID, textToSend, portraitColors);
                }
            }
            else
            {
                chatting = true;
                switch (ID)
                {
                    case 0:
                        if (PlayState.hasRainbowWave)
                        {
                            textToSend.Add("Woah!!  Nice Rainbow Wave, " + playerName + "!!\nI\'d love one too, but I don\'t\nhave a jump button.");
                        }
                        else
                        {
                            textToSend.Add("Oh, hey, " + playerName + "! I'm here to test\nsingle-page dialogue!!");
                        }
                        break;
                    default:
                        textToSend.Add("Hey " + playerName + "!  Unfortunately I,\nsnail #" + ID + ", don\'t have any\ndialogue to offer.");
                        break;
                }
                portraitColors.Add(new Color32(0, 0, 0, 0));
                PlayState.OpenDialogue(2, ID, textToSend, portraitColors);
            }
        }
        else if (Vector2.Distance(transform.position, player.transform.position) > 5 && chatting)
        {
            chatting = false;
            PlayState.CloseDialogue();
        }
        else if (Vector2.Distance(transform.position, player.transform.position) > 3 && !chatting && IDsWithLongDialogue.Contains(ID))
        {
            speechBubble.GetComponent<SpriteRenderer>().enabled = false;
        }

        if (Input.GetAxisRaw("Shoot") == 1)
        {
            buttonDown = true;
        }
        else
        {
            buttonDown = false;
        }
    }

    private void AddColorsNPC()
    {
        portraitColors.Add(savedColors[0]);
        portraitColors.Add(savedColors[1]);
        portraitColors.Add(savedColors[2]);
    }

    private void AddColorsPlayer()
    {
        portraitColors.Add(new Color32(252, 160, 72, 255));
        portraitColors.Add(new Color32(252, 120, 252, 255));
        portraitColors.Add(new Color32(0, 0, 0, 0));
    }
}
