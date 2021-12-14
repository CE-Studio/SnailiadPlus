using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleNPC : NPC
{
    public SpriteRenderer sprite;
    
    public override void Start()
    {
        playerName = "Snaily";
        player = GameObject.FindWithTag("Player");

        sprite = GetComponent<SpriteRenderer>();
    }

    public override void Update()
    {
        if (player.transform.position.x < transform.position.x)
        {
            sprite.flipX = true;
        }
        else
        {
            sprite.flipX = false;
        }

        if (Vector2.Distance(transform.position, player.transform.position) < 3 && !chatting)
        {
            List<string> textToSend = new List<string>();
            textToSend.Add("After this game is over, I\'m\ngoing to get some pizza!!\n");
            chatting = true;
            PlayState.OpenDialogue(2, 52, textToSend);
        }
        else if (Vector2.Distance(transform.position, player.transform.position) > 5 && chatting)
        {
            chatting = false;
            PlayState.CloseDialogue();
        }
    }
}
