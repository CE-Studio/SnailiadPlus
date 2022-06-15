using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleNPC : NPC
{
    public override void Awake()
    {
        ID = 52;

        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<AnimationModule>();

        speechBubble = transform.Find("Speech bubble").gameObject;
        speechBubbleSprite = speechBubble.GetComponent<SpriteRenderer>();
        speechBubbleAnim = speechBubble.GetComponent<AnimationModule>();
        speechBubbleSprite.enabled = false;
    }

    public override void Spawn(int[] spawnData)
    {
        upsideDown = spawnData[0] == 1;

        anim.Add("NPC_turtle");
        anim.Play("NPC_turtle");

        if (upsideDown)
        {
            sprite.flipY = true;
            speechBubbleSprite.flipY = true;
            speechBubble.transform.localPosition = new Vector2(0, -0.75f);
        }
    }

    public override void Update()
    {
        if (PlayState.gameState == "Game")
        {
            if (PlayState.player.transform.position.x < transform.position.x)
            {
                sprite.flipX = true;
                speechBubbleSprite.flipX = true;
            }
            else
            {
                sprite.flipX = false;
                speechBubbleSprite.flipX = false;
            }

            if (Vector2.Distance(transform.position, PlayState.player.transform.position) < 1.5f && !chatting && !needsSpace)
            {
                if (!PlayState.isTalking)
                {
                    int boxShape = 2;
                    string boxColor = PlayState.ParseColorCodeToString(PlayState.GetAnim("Dialogue_characterColors").frames[3]);
                    textToSend.Clear();
                    portraitStateList.Clear();

                    AddText("default");

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
                        if (!speechBubbleSprite.enabled)
                            speechBubbleSprite.enabled = true;
                        ToggleBubble(true);
                        if (Control.SpeakPress())
                        {
                            chatting = true;
                            PlayState.isTalking = true;
                            PlayState.paralyzed = true;
                            PlayState.OpenDialogue(3, ID, textToSend, boxShape, boxColor, portraitStateList, PlayState.player.transform.position.x < transform.position.x);
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
                ToggleBubble(false);
            }
        }
    }

    public override void AddText(string textID)
    {
        bool locatedAll = false;
        int i = 0;
        while (!locatedAll)
        {
            string fullID = "npc_turtle_" + textID + "_" + i;
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
}
