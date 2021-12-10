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

    private List<Color32> portraitColors = new List<Color32>();
    private List<string> portraitStateList = new List<string>();
    public Texture2D colorTable;

    public List<SpriteRenderer> parts = new List<SpriteRenderer>();
    public GameObject speechBubble;

    public GameObject player;

    void Start()
    {
        playerName = "Snaily";

        colorTable = (Texture2D)Resources.Load("Images/Entities/SnailNpcColor");

        parts.Add(transform.GetComponent<SpriteRenderer>());
        parts.Add(transform.GetChild(0).GetComponent<SpriteRenderer>());
        parts.Add(transform.GetChild(1).GetComponent<SpriteRenderer>());
        parts.Add(transform.GetChild(2).GetComponent<SpriteRenderer>());
        parts.Add(transform.GetChild(3).GetComponent<SpriteRenderer>());
        speechBubble = transform.Find("Speech bubble").gameObject;
        player = GameObject.FindWithTag("Player");

        parts[0].color = colorTable.GetPixel(0, ID);
        parts[1].color = colorTable.GetPixel(1, ID);
        parts[2].color = colorTable.GetPixel(2, ID);
        parts[3].color = colorTable.GetPixel(3, ID);
        parts[4].color = colorTable.GetPixel(4, ID);

        if (upsideDown)
        {
            for (int i = 0; i < parts.Count; i++)
                parts[i].flipY = true;
            speechBubble.GetComponent<SpriteRenderer>().flipY = true;
            speechBubble.transform.localPosition = new Vector2(0, -0.75f);
        }
        speechBubble.GetComponent<SpriteRenderer>().enabled = false;
    }

    void Update()
    {
        if (player.transform.position.x < transform.position.x)
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

        if (Vector2.Distance(transform.position, player.transform.position) < 3 && !chatting)
        {
            List<string> textToSend = new List<string>();
            portraitColors.Clear();
            portraitStateList.Clear();
            switch (ID)
            {
                case 50:
                    if (PlayState.hasRainbowWave)
                        textToSend.Add("Woah!!  Nice Rainbow Wave, " + playerName + "!!\nI\'d love one too, but I don\'t\nhave a jump button.");
                    else
                        textToSend.Add("Oh, hey, " + playerName + "! I'm here to test\nsingle-page dialogue!!");
                    break;

                case 51:
                    AddNPCColors();
                    textToSend.Add("Hey there, " + playerName + "!! I see you\nfigured out how to start a\nmulti-page conversation!");
                    BuildPortraitStateList("npc");
                    textToSend.Add("The hope is this talk should go\n100% smoothly. What do you\nthink?");
                    BuildPortraitStateList("npc");
                    textToSend.Add("Impressive! I do hope that\'s my\nportrait showing right now, if it\neven is there.");
                    BuildPortraitStateList("player");
                    textToSend.Add("I\'m here to test multiple things,\nit seems!");
                    BuildPortraitStateList("npc");
                    break;

                default:
                    textToSend.Add("Hey " + playerName + "!  Unfortunately I,\nsnail #" + ID + ", don\'t have any\ndialogue to offer.  Sorry!!");
                    break;
            }
            if (textToSend.Count > 1)
            {
                speechBubble.GetComponent<SpriteRenderer>().enabled = true;
                if (Input.GetAxisRaw("Shoot") == 1 && !buttonDown)
                {
                    chatting = true;
                    PlayState.paralyzed = true;
                    PlayState.OpenDialogue(3, ID, textToSend, portraitColors, portraitStateList, player.transform.position.x < transform.position.x);
                }
            }
            else
            {
                chatting = true;
                PlayState.OpenDialogue(2, ID, textToSend);
            }
        }
        else if (Vector2.Distance(transform.position, player.transform.position) > 5 && chatting)
        {
            chatting = false;
            PlayState.CloseDialogue();
        }
        else if (Vector2.Distance(transform.position, player.transform.position) > 3 && (!chatting || PlayState.paralyzed))
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

    private void AddNPCColors()
    {
        portraitColors.Add(parts[0].color);
        portraitColors.Add(parts[1].color);
        portraitColors.Add(parts[2].color);
        portraitColors.Add(parts[3].color);
        portraitColors.Add(parts[4].color);
    }

    private void BuildPortraitStateList(string speaker)
    {
        switch (speaker)
        {
            case "player":
                portraitStateList.Add("player");
                break;
            case "npc":
            default:
                portraitStateList.Add("npc");
                break;
        }
    }
}
