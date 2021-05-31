using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    public int ID = 0;
    public bool upsideDown = false;
    bool chatting = false;

    public string playerName;

    public List<Color32> colors = new List<Color32>();
    public List<int> IDsWithLongDialogue = new List<int>();

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

        outline = GetComponent<SpriteRenderer>();
        body = transform.Find("NPC body").GetComponent<SpriteRenderer>();
        shell = transform.Find("NPC shell").GetComponent<SpriteRenderer>();
        outlineAnim = GetComponent<Animator>();
        bodyAnim = transform.Find("NPC body").GetComponent<Animator>();
        shellAnim = transform.Find("NPC shell").GetComponent<Animator>();
        speechBubble = transform.Find("Speech bubble").gameObject;
        player = GameObject.FindWithTag("Player");

        body.color = colors[ID * 2];
        shell.color = colors[ID * 2 + 1];

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
            chatting = true;
            if (IDsWithLongDialogue.Contains(ID))
            {

            }
            else
            {
                List<string> textToSend = new List<string>();
                switch (ID)
                {
                    case 0:
                        if (PlayState.hasRainbowWave)
                        {
                            textToSend.Add("Woah!!  Nice Rainbow Wave, " + playerName + "!!\nI'd love one too, but I don\'t\nhave a jump button.");
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
                PlayState.OpenDialogue(2, ID, textToSend);
            }
        }
        else if (Vector2.Distance(transform.position, player.transform.position) > 3 && chatting)
        {
            chatting = false;
            PlayState.CloseDialogue();
        }
    }
}
