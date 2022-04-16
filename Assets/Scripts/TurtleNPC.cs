using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleNPC : NPC
{
    public override void Awake()
    {
        playerName = "Snaily";

        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<AnimationModule>();
        anim.Add("NPC_turtle");
        anim.Play("NPC_turtle");
    }

    public override void Update()
    {
        if (PlayState.gameState == "Game")
        {
            if (PlayState.player.transform.position.x < transform.position.x)
            {
                sprite.flipX = true;
            }
            else
            {
                sprite.flipX = false;
            }

            if (Vector2.Distance(transform.position, PlayState.player.transform.position) < 3 && !chatting)
            {
                List<string> textToSend = new List<string>();
                textToSend.Add("After this game is over, {p|0.125}I\'m{nl|}going to get some pizza!!");
                chatting = true;
                PlayState.OpenDialogue(2, 52, textToSend, 1);
            }
            else if (Vector2.Distance(transform.position, PlayState.player.transform.position) > 5 && chatting)
            {
                chatting = false;
                PlayState.CloseDialogue();
            }
        }
    }
}
