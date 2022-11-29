using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CutsceneController : MonoBehaviour
{
    public string sceneScript;
    public List<NPC> actors;
    public bool triggerActive = false;
    public bool endedForever = false;
    public int cutsceneID = 0;
    public BoxCollider2D box;

    private string[] lines = new string[] { };

    private bool isActive = false;
    private bool endFlag = false;
    private bool playerIntersecting = false;
    private int lineNum = 0;
    private int tokenNum = 0;
    private int runningActions = 0;
    private float mainDelay = 0;
    private List<Vector3> loopDepth = new List<Vector3>(); // X = count, Y = loop start line number, Z = delay

    private struct BatchedDialogue
    {
        public List<string> dialogue;
        public int speaker;
        public int shape;
        public int sound;
        public string color;
        public List<int> states;
        public bool facingLeft;
    };
    private List<BatchedDialogue> batchedDialogue = new List<BatchedDialogue>();

    public void Spawn()
    {
        box = GetComponent<BoxCollider2D>();
        box.enabled = triggerActive;
        lines = sceneScript.Split('\n');
        BatchDialogue();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !playerIntersecting)
        {
            isActive = true;
            playerIntersecting = true;
            box.enabled = false;
            throw new System.NotImplementedException("Please use 'CutsceneManager' instead.");
            //StartCoroutine(MainLoop());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isActive)
            playerIntersecting = false;
    }

    private IEnumerator MainLoop()
    {
        while (PlayState.cutsceneActive) //TODO Two cutscenes waiting?
        {
            PlayState.paralyzed = true;
            yield return new WaitForEndOfFrame();
        }
        PlayState.paralyzed = false;
        PlayState.cutsceneActive = true;
        while (lineNum < lines.Length && !endFlag)
        {
            string[] tokens = lines[lineNum].Split(' '); //TODO extra whitespace?
            float commandDelay = 0;
            if (mainDelay == 0)
            {
                if (tokens[0] == "with" || tokens[0] == "after")
                {
                    tokenNum++;
                    if (tokens[0] == "after")
                    {
                        while (runningActions > 0)
                            yield return new WaitForEndOfFrame();
                    }
                    if (IsFloat(tokens[1], out commandDelay))
                        tokenNum++;
                }
                else
                {
                    while (runningActions > 0)
                        yield return new WaitForEndOfFrame();
                }

                bool parseThisCommand = false;
                float depth = 0;
                bool foundEnd = false;
                switch (ExtractCommand(tokens, out int index))
                {
                    case "if":
                        if (!ParseIf(tokens))
                        {
                            string lastCommand = "";
                            while (!foundEnd)
                            {
                                lineNum++;
                                tokens = lines[lineNum].Split(' ');
                                string thisCommand = ExtractCommand(tokens, out _);
                                if (thisCommand == "else" || thisCommand == "endif")
                                {
                                    if (depth == 0)
                                        foundEnd = true;
                                    else
                                    {
                                        if (thisCommand == "endif" || (thisCommand == "else" && lastCommand == "else"))
                                            depth--;
                                    }
                                    lastCommand = thisCommand;
                                }
                                else if (thisCommand == "if")
                                {
                                    depth++;
                                    lastCommand = thisCommand;
                                }
                            }
                            lineNum++;
                        }
                        break;
                    case "else":
                        while (!foundEnd)
                        {
                            lineNum++;
                            tokens = lines[lineNum].Split(' ');
                            string thisCommand = ExtractCommand(tokens, out _);
                            if (thisCommand == "else" || thisCommand == "endif")
                            {
                                if (depth == 0)
                                    foundEnd = true;
                                else
                                {
                                    if (thisCommand == "endif")
                                        depth--;
                                }
                            }
                            else if (thisCommand == "if")
                            {
                                depth++;
                            }
                        }
                        lineNum++;
                        break;
                    case "loop": // X = count, Y = loop start line number, Z = delay
                        TryForToken(tokens, index + 1, out string loopCount);
                        TryForToken(tokens, index + 2, out string loopDelay);
                        loopDepth.Add(new Vector3(ParseInt(loopCount), lineNum, ParseFloat(loopDelay)));
                        break;
                    case "endloop":
                        if (loopDepth.Count != 0) // X = count, Y = loop start line number, Z = delay
                        {
                            Vector3 thisData = loopDepth[loopDepth.Count - 1];
                            thisData.x--;
                            if (thisData.z != 0)
                                yield return new WaitForSeconds(thisData.z);
                            lineNum = (int)thisData.y;
                            if (thisData.x == 1)
                                loopDepth.RemoveAt(loopDepth.Count - 1);
                            else
                                loopDepth[loopDepth.Count - 1] = thisData;
                        }
                        break;
                    default:
                        parseThisCommand = true;
                        break;
                }

                if (parseThisCommand)
                {
                    runningActions++;
                    StartCoroutine(ParseCommand(commandDelay, tokens, tokenNum));
                }

                tokenNum = 0;
                lineNum++;
                yield return new WaitForEndOfFrame();
            }
            else
            {
                yield return new WaitForSeconds(mainDelay);
                mainDelay = 0;
            }
        }
        PlayState.SetCamFocus(PlayState.player.transform);
        PlayState.SetCamSpeed();
        PlayState.camCutsceneOffset = Vector2.zero;
        PlayState.paralyzed = false;
        PlayState.cutsceneActive = false;
        isActive = false;
    }

    private bool IsInt(string token, out int result)
    {
        return int.TryParse(token, out result);
    }

    private bool IsFloat(string token, out float result)
    {
        return float.TryParse(token, out result);
    }

    private bool TryForToken(string[] tokens, int target, out string result)
    {
        if (target < tokens.Length)
        {
            result = tokens[target];
            return true;
        }
        else
        {
            result = "null";
            return false;
        }
    }

    private Transform ParseActor(string token)
    {
        if (token == "player")
            return PlayState.player.transform;
        if (IsInt(token, out int actorID))
            return actors[actorID].transform;
        return actors[int.Parse(token.Substring(5, token.Length - 5))].transform;
    }

    private int ParseInt(string input)
    {
        if (input.Contains("/"))
        {
            string[] randParts = input.Split('/');
            if (IsInt(randParts[0], out int int1) && IsInt(randParts[1], out int int2))
            {
                if (int1 == int2)
                    return int1;
                else if (int1 < int2)
                    return Random.Range(int1, int2 + 1);
                else
                    return Random.Range(int2, int1 + 1);
            }
        }
        else if (IsInt(input, out int result))
            return result;
        return 0;
    }

    private float ParseFloat(string input)
    {
        if (input.Contains("/"))
        {
            string[] randParts = input.Split('/');
            if (IsFloat(randParts[0], out float float1) && IsFloat(randParts[1], out float float2))
            {
                if (float1 == float2)
                    return float1;
                else if (float1 < float2)
                    return Random.Range(float1, float2);
                else
                    return Random.Range(float2, float1);
            }
        }
        else if (IsFloat(input, out float result))
            return result;
        return 0f;
    }

    private bool ParseIf(string[] tokens)
    {
        ExtractCommand(tokens, out int index);
        List<string> comparators = new List<string> { "=", "==", "<", ">", "<=", "=<", ">=", "=>", "!=" }; //TODO own contains implementation for array?
        List<string> var1List = new List<string>();
        float var1 = 0f;
        List<string> var2List = new List<string>();
        float var2 = 0f;
        string comparator = "";
        bool isComparing = false;
        for (int i = index + 1; i < tokens.Length; i++)
        {
            if (comparators.Contains(tokens[i]))
            {
                comparator = tokens[i];
                isComparing = true;
            }
            else
                (isComparing ? var2List : var1List).Add(tokens[i]);
        }
        for (int i = 0; i < 2; i++)
        {
            List<string> thisVarList = i == 1 ? var2List : var1List;
            float thisVar = 0f;
            switch (thisVarList[0])
            {
                case "true":
                    thisVar = 1;
                    break;
                case "false":
                    thisVar = 0;
                    break;
                case "player":
                    thisVar = PlayState.currentCharacter switch { "Sluggy" => 1, "Upside" => 2, "Leggy" => 3, "Blobby" => 4, "Leechy" => 5, _ => 0 };
                    break;
                case "itemstate":
                    thisVar = PlayState.itemCollection[ParseInt(thisVarList[1])] == 1 ? 1 : 0;
                    break;
                case "bossstate":
                    thisVar = PlayState.bossStates[ParseInt(thisVarList[1])] == 1 ? 1 : 0;
                    break;
                case "npcvar":
                    break;
                case "achievement":
                    thisVar = PlayState.achievementStates[ParseInt(thisVarList[1])] == 1 ? 1 : 0;
                    break;
                case "gametime":
                    thisVar = PlayState.currentTime[1] + (PlayState.currentTime[0] * 60) + (PlayState.currentTime[2] / 60);
                    break;
                case "health":
                    thisVar = thisVarList[1] == "current" ? (PlayState.playerScript.health / PlayState.playerScript.maxHealth) : PlayState.playerScript.maxHealth;
                    break;
                default:
                    Transform thisActor = ParseActor(thisVarList[0]);
                    if (thisActor != null)
                        thisVar = thisVarList[1] == "y" ? thisActor.position.y : thisActor.position.x;
                    else
                        thisVar = ParseFloat(thisVarList[0]);
                    break;
            }
            if (i == 1)
                var2 = thisVar;
            else
                var1 = thisVar;
        }
        switch (comparator)
        {
            case "=":
            case "==":
                return var1 == var2;
            case "<":
                return var1 < var2;
            case ">":
                return var1 > var2;
            case "<=":
            case "=<":
                return var1 <= var2;
            case ">=":
            case "=>":
                return var1 >= var2;
            case "!=":
                return var1 != var2;
            default:
                return var1 != 0;
        }
    }

    private string ExtractCommand(string[] tokens, out int commandIndex)
    {
        if (tokens[0] == "with" || tokens[0] == "after")
        {
            if (IsFloat(tokens[1], out _))
            {
                commandIndex = 2;
                return tokens[2];
            }
            else
            {
                commandIndex = 1;
                return tokens[1];
            }
        }
        else
        {
            commandIndex = 0;
            return tokens[0];
        }
    }

    private void BatchDialogue()
    {
        BatchedDialogue newBatch = new BatchedDialogue { dialogue = new List<string>(), states = new List<int>() };
        int depth = 0;
        int startIndex = -1;
        string previousInitial = "";

        for (int i = 0; i < lines.Length; i++)
        {
            string[] tokens = lines[i].Split(' ');
            bool parse = false;
            string commandInitial = "";

            ExtractCommand(tokens, out int num);
            if (tokens[num] == "dialogue")
            {
                if (depth == 0 || tokens[0] == "with")
                    parse = true;
                if (tokens[num + 1] != "prompted")
                    parse = false;
            }
            for (int j = 0; j <= num; j++)
                commandInitial += tokens[j] + " ";

            if (parse)
            {
                if (startIndex == -1)
                    startIndex = i;

                int speaker = 0;
                int shape = 0;
                int sound = 0;
                string color = "0005";
                bool facingLeft = false;

                string lastType = "";

                num += 2;
                newBatch.dialogue.Add(PlayState.GetText(tokens[num])
                    .Replace("##", PlayState.GetItemPercentage().ToString())
                    .Replace("{P}", PlayState.GetText("char_" + PlayState.currentCharacter.ToLower()))
                    .Replace("{PF}", PlayState.GetText("char_full_" + PlayState.currentCharacter.ToLower()))
                    .Replace("{S}", PlayState.GetText("species_" + PlayState.currentCharacter.ToLower()))
                    .Replace("{SS}", PlayState.GetText("species_plural_" + PlayState.currentCharacter.ToLower())));
                while (num < tokens.Length)
                {
                    if (tokens[num] == "as" || tokens[num] == "box" || tokens[num] == "sound")
                        lastType = tokens[num];
                    else
                    {
                        switch (lastType)
                        {
                            case "as":
                                Transform thisSpeaker = ParseActor(tokens[num]);
                                speaker = thisSpeaker.GetComponent<NPC>().ID;
                                facingLeft = thisSpeaker.GetComponent<SpriteRenderer>().flipX;
                                newBatch.states.Add(tokens[num] == "player" ? 0 : 1);
                                break;
                            case "box":
                                if (tokens[num].Length == 4)
                                    color = tokens[num];
                                else
                                    shape = ParseInt(tokens[num]);
                                break;
                            case "sound":
                                sound = ParseInt(tokens[num]);
                                break;
                            default:
                                break;
                        }
                    }
                    num++;
                }

                if (depth == 0)
                {
                    newBatch.speaker = speaker;
                    newBatch.shape = shape;
                    newBatch.sound = sound;
                    newBatch.color = color;
                    newBatch.facingLeft = facingLeft;
                }
                depth++;
            }
            else
            {
                if (depth > 0)
                {
                    while (depth > 0)
                    {
                        if (depth == 1)
                            lines[startIndex + depth - 1] = (previousInitial == "" ? commandInitial : previousInitial) + batchedDialogue.Count.ToString();
                        else
                            lines[startIndex + depth - 1] = "nop";
                        depth--;
                    }
                    batchedDialogue.Add(newBatch);
                    newBatch = new BatchedDialogue { dialogue = new List<string>(), states = new List<int>() };
                }
                startIndex = -1;
            }

            previousInitial = commandInitial;
        }
    }

    private bool FindWall(Transform actor, string dir, float speed, out float distance)
    {
        Vector2 boxSizeHalved = actor.GetComponent<BoxCollider2D>().size * 0.5f;
        Vector2 start = dir switch
        {
            "left" => new Vector2(actor.position.x - boxSizeHalved.x - 0.0625f, actor.position.y),
            "right" => new Vector2(actor.position.x + boxSizeHalved.x + 0.0625f, actor.position.y),
            "up" => new Vector2(actor.position.x, actor.position.y + boxSizeHalved.y + 0.0625f),
            _ => new Vector2(actor.position.x, actor.position.y - boxSizeHalved.y - 0.0625f)
        };
        Vector2 direction = dir switch { "left" => Vector2.left, "right" => Vector2.right, "up" => Vector2.up, _ => Vector2.down };
        RaycastHit2D hitBox = Physics2D.BoxCast(actor.position, boxSizeHalved * 2, 0, direction, speed * Time.deltaTime, PlayState.playerScript.playerCollide);
        if (hitBox.collider != null)
        {
            Vector2 rayStart;
            if (dir == "left" || dir == "right")
                rayStart = new Vector2(actor.position.x, hitBox.point.y);
            else
                rayStart = new Vector2(hitBox.point.x, actor.position.y);
            RaycastHit2D hitRay = Physics2D.Raycast(rayStart, direction, Mathf.Infinity, PlayState.playerScript.playerCollide);
            distance = hitRay.distance;
            return true;
        }
        else
        {
            distance = 0;
            return false;
        }
    }

    private IEnumerator ParseCommand(float delay, string[] tokens, int thisTokenNum)
    {
        bool endActionHere = true;
        if (delay != 0)
            yield return new WaitForSeconds(delay);
        switch (tokens[thisTokenNum]) {
            case "move":
                Transform actorMove = ParseActor(tokens[thisTokenNum + 1]);
                NPC actorScriptMove = actorMove.GetComponent<NPC>();
                float tileCountMove = 0;
                bool verticalMove = false;
                float speedMove = 0;
                float secondsMove = 0;
                float jumpPowerMove = 0;
                bool jumpContinuouslyMove = false;
                int finalTypeMove = 0;
                // 4 - over
                // 2 - at
                // 1 - tiles
                // Accepted types are 1, 3, 5, and 6
                for (int i = 2; thisTokenNum + i < tokens.Length; i++)
                {
                    if (i == 2)
                    {
                        if (IsFloat(tokens[thisTokenNum + 2], out float tileCountTest))
                        {
                            tileCountMove = tileCountTest;
                            finalTypeMove += 1;
                        }
                        else
                            verticalMove = tokens[thisTokenNum + 2] == "y";
                    }
                    else
                    {
                        switch (tokens[thisTokenNum + i])
                        {
                            case "x":
                                verticalMove = false;
                                break;
                            case "y":
                                verticalMove = true;
                                break;
                            case "over":
                                secondsMove = ParseFloat(tokens[thisTokenNum + i + 1]);
                                i++;
                                finalTypeMove += 4;
                                break;
                            case "at":
                                speedMove = ParseFloat(tokens[thisTokenNum + i + 1]);
                                i++;
                                finalTypeMove += 2;
                                break;
                            case "jump":
                                jumpPowerMove = ParseFloat(tokens[thisTokenNum + i + 1]);
                                TryForToken(tokens, thisTokenNum + i + 2, out string jumpStateTestMove);
                                jumpContinuouslyMove = jumpStateTestMove == "repeatedly";
                                i += 1 + (jumpStateTestMove == "repeatedly" ? 1 : 0);
                                break;
                        }
                    }
                }

                string directionMove;
                if (verticalMove)
                {
                    if (tileCountMove == 0)
                        directionMove = speedMove >= 0 ? "up" : "down";
                    else
                        directionMove = tileCountMove >= 0 ? "up" : "down";
                }
                else
                {
                    if (tileCountMove == 0)
                        directionMove = speedMove >= 0 ? "right" : "left";
                    else
                        directionMove = tileCountMove >= 0 ? "right" : "left";
                }

                switch (finalTypeMove)
                {
                    case 1: // Just tile count
                        actorMove.position += new Vector3(verticalMove ? 0 : tileCountMove, verticalMove ? tileCountMove : 0, 0);
                        break;
                    case 3: // Tile count and speed
                        float distanceCoveredMove = 0;
                        int dirModMove = (directionMove == "left" || directionMove == "down") ? -1 : 1;
                        while (distanceCoveredMove < Mathf.Abs(tileCountMove))
                        {
                            //if (FindWall(actorMove, directionMove, speedMove, out float distanceMove))
                            //{
                            //    if (jumpPowerMove != 0 && actorScriptMove.velocity == 0)
                            //        actorScriptMove.velocity = actorScriptMove.upsideDown ? -jumpPowerMove : jumpPowerMove;
                            //    actorMove.position += new Vector3(verticalMove ? 0 : distanceMove * dirModMove, verticalMove ? distanceMove * dirModMove : 0, 0);
                            //    distanceCoveredMove += distanceMove;
                            //}
                            //else
                            //{
                                actorMove.position += new Vector3(verticalMove ? 0 : speedMove * Time.deltaTime * dirModMove, verticalMove ? speedMove * Time.deltaTime * dirModMove : 0, 0);
                                distanceCoveredMove += speedMove * Time.deltaTime;
                            //    Debug.Log(speedMove * Time.deltaTime);
                            //}
                            if (jumpContinuouslyMove && actorScriptMove.velocity == 0)
                                actorScriptMove.velocity = actorScriptMove.upsideDown ? -jumpPowerMove : jumpPowerMove;
                            yield return new WaitForEndOfFrame();
                        }
                        break;
                    case 5: // Tile count and time
                        //float distanceCoveredTimeMove = 0;
                        //while (distanceCoveredTimeMove < tileCountMove)
                        //{
                        //    if (FindWall(actorMove, directionMove, speedMove, out float distanceMove))
                        //    {
                        //        if (jumpPowerMove != 0 && actorScriptMove.velocity == 0)
                        //            actorScriptMove.velocity = actorScriptMove.upsideDown ? -jumpPowerMove : jumpPowerMove;
                        //        actorMove.position += new Vector3(verticalMove ? 0 : distanceMove, verticalMove ? distanceMove : 0, 0);
                        //        distanceCoveredTimeMove += distanceMove;
                        //    }
                        //    else
                        //    {
                        //        actorMove.position += new Vector3(verticalMove ? 0 : speedMove * Time.deltaTime, verticalMove ? speedMove * Time.deltaTime : 0, 0);
                        //        distanceCoveredTimeMove += speedMove * Time.deltaTime;
                        //    }
                        //    if (jumpContinuouslyMove && actorScriptMove.velocity == 0)
                        //        actorScriptMove.velocity = actorScriptMove.upsideDown ? -jumpPowerMove : jumpPowerMove;
                        //}
                        break;
                    case 6: // Speed and time
                        break;
                }
                break;

            case "jump":
                if (TryForToken(tokens, thisTokenNum + 1, out string actorJump) && TryForToken(tokens, thisTokenNum + 2, out string powerJump))
                {
                    float clampedPowerJump = Mathf.Abs(ParseFloat(powerJump));
                    if (actorJump == "player")
                        PlayState.playerScript.RemoteJump(clampedPowerJump);
                    else
                    {
                        NPC actorScriptJump = ParseActor(actorJump).GetComponent<NPC>();
                        actorScriptJump.transform.position = new Vector2(actorScriptJump.transform.position.x, actorScriptJump.transform.position.y + (0.0625f * (actorScriptJump.upsideDown ? -1 : 1)));
                        actorScriptJump.velocity = (actorScriptJump.upsideDown ? -clampedPowerJump : clampedPowerJump) * Time.deltaTime;
                    }
                }
                break;

            case "paralyze":
                PlayState.paralyzed = true;
                break;

            case "unparalyze":
                PlayState.paralyzed = false;
                break;

            case "face":
                if (TryForToken(tokens, thisTokenNum + 1, out string actorFace) && TryForToken(tokens, thisTokenNum + 2, out string directionFace))
                {
                    if (actorFace == "player")
                        PlayState.playerScript.RemoteGravity(directionFace switch { "left" => 1, "right" => 2, "up" => 3, _ => 0 });
                    else
                    {
                        if (directionFace == "left" || directionFace == "right")
                            ParseActor(actorFace).GetComponent<SpriteRenderer>().flipX = directionFace == "right";
                        else
                            ParseActor(actorFace).GetComponent<SpriteRenderer>().flipY = directionFace == "up";
                    }
                }
                break;

            case "flip":
                if (TryForToken(tokens, thisTokenNum + 1, out string actorFlip) && TryForToken(tokens, thisTokenNum + 2, out string directionFlip))
                {
                    if (directionFlip == "x")
                        ParseActor(actorFlip).GetComponent<SpriteRenderer>().flipX = !ParseActor(actorFlip).GetComponent<SpriteRenderer>().flipX;
                    else
                        ParseActor(actorFlip).GetComponent<SpriteRenderer>().flipY = !ParseActor(actorFlip).GetComponent<SpriteRenderer>().flipY;
                }
                break;

            case "show":
                if (TryForToken(tokens, thisTokenNum + 1, out string actorShow))
                    ParseActor(actorShow).GetComponent<SpriteRenderer>().enabled = true;
                break;

            case "hide":
                if (TryForToken(tokens, thisTokenNum + 1, out string actorHide))
                    ParseActor(actorHide).GetComponent<SpriteRenderer>().enabled = false;
                break;

            case "toggle":
                if (TryForToken(tokens, thisTokenNum + 1, out string actorToggle))
                    ParseActor(actorToggle).GetComponent<SpriteRenderer>().enabled = !ParseActor(actorToggle).GetComponent<SpriteRenderer>().enabled;
                break;

            case "animate":
                if (TryForToken(tokens, thisTokenNum + 1, out string actorAnimate) && TryForToken(tokens, thisTokenNum + 2, out string animationAnimate))
                    ParseActor(actorAnimate).GetComponent<AnimationModule>().Play(animationAnimate);
                break;

            case "dialogue":
                while (PlayState.dialogueOpen)
                    yield return new WaitForEndOfFrame();

                TryForToken(tokens, thisTokenNum + 1, out string typeDialogue);
                if (IsInt(typeDialogue, out int idDialogue))
                {
                    BatchedDialogue thisBatch = batchedDialogue[idDialogue];
                    PlayState.OpenDialogue(3, thisBatch.speaker, thisBatch.dialogue, thisBatch.shape, thisBatch.color, thisBatch.states, thisBatch.facingLeft);
                    PlayState.StallDialoguePrompted(this);
                }
                else
                {
                    BatchedDialogue newDialogue = new BatchedDialogue { dialogue = new List<string>(), speaker = 0, shape = 0, sound = 0, color = "0005" };
                    string parseType = "";
                    float lingerTime = 0;
                    for (int parseIndex = 2; thisTokenNum + parseIndex < tokens.Length; parseIndex++)
                    {
                        if (parseIndex == 2)
                            lingerTime = ParseFloat(tokens[thisTokenNum + parseIndex]);
                        else if (parseIndex == 3)
                            newDialogue.dialogue.Add(PlayState.GetText(tokens[thisTokenNum + parseIndex])
                                .Replace("##", PlayState.GetItemPercentage().ToString())
                                .Replace("{P}", PlayState.GetText("char_" + PlayState.currentCharacter.ToLower()))
                                .Replace("{PF}", PlayState.GetText("char_full_" + PlayState.currentCharacter.ToLower()))
                                .Replace("{S}", PlayState.GetText("species_" + PlayState.currentCharacter.ToLower()))
                                .Replace("{SS}", PlayState.GetText("species_plural_" + PlayState.currentCharacter.ToLower())));
                        else
                        {
                            if (tokens[thisTokenNum + parseIndex] == "as" || tokens[thisTokenNum + parseIndex] == "box" || tokens[thisTokenNum + parseIndex] == "sound")
                                parseType = tokens[thisTokenNum + parseIndex];
                            else
                            {
                                switch (parseType)
                                {
                                    case "as":
                                        newDialogue.speaker = ParseActor(tokens[thisTokenNum + parseIndex]).GetComponent<NPC>().ID;
                                        break;
                                    case "box":
                                        if (tokens[thisTokenNum + parseIndex].Length == 4)
                                            newDialogue.color = tokens[thisTokenNum + parseIndex];
                                        else
                                            newDialogue.shape = ParseInt(tokens[thisTokenNum + parseIndex]);
                                        break;
                                    case "sound":
                                        newDialogue.sound = ParseInt(tokens[thisTokenNum + parseIndex]);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    PlayState.OpenDialogue(2, newDialogue.speaker, newDialogue.dialogue, newDialogue.shape, newDialogue.color);
                    PlayState.StallDialogueContinuous(this, lingerTime);
                }
                endActionHere = false;
                break;

            case "fade":
                if (TryForToken(tokens, thisTokenNum + 1, out string timeFade) && TryForToken(tokens, thisTokenNum + 2, out string rFade) &&
                    TryForToken(tokens, thisTokenNum + 3, out string gFade) && TryForToken(tokens, thisTokenNum + 4, out string bFade) &&
                    TryForToken(tokens, thisTokenNum + 5, out string aFade))
                    PlayState.ScreenFlash("custom", ParseInt(rFade), ParseInt(gFade), ParseInt(bFade), ParseInt(aFade), ParseFloat(timeFade),
                        TryForToken(tokens, thisTokenNum + 6, out string sortingOrderFade) ? ParseInt(sortingOrderFade) : 1001);
                break;

            case "cam":
                if (TryForToken(tokens, thisTokenNum + 1, out string modeCam))
                {
                    if (modeCam == "focus")
                    {
                        if (TryForToken(tokens, thisTokenNum + 2, out string actorCam))
                        {
                            PlayState.SetCamFocus(ParseActor(actorCam));
                            PlayState.SetCamSpeed();
                            if (TryForToken(tokens, thisTokenNum + 3, out string xOffsetCamFocus) && TryForToken(tokens, thisTokenNum + 4, out string yOffsetCamFocus))
                                PlayState.camCutsceneOffset = new Vector2(ParseFloat(xOffsetCamFocus), ParseFloat(yOffsetCamFocus));
                            else
                                PlayState.camCutsceneOffset = Vector2.zero;
                        }
                    }
                    else if (modeCam == "move")
                    {
                        if (TryForToken(tokens, thisTokenNum + 2, out string timeCam) && TryForToken(tokens, thisTokenNum + 3, out string xCam) &&
                            TryForToken(tokens, thisTokenNum + 4, out string yCam) && TryForToken(tokens, thisTokenNum + 5, out string spaceCam))
                        {
                            PlayState.camCutsceneOffset = PlayState.cam.transform.position;
                            PlayState.SetCamFocus();
                            PlayState.SetCamSpeed(1);
                            float elapsed = 0;
                            float maxTime = ParseFloat(timeCam);
                            Vector2 start = PlayState.camCutsceneOffset;
                            Vector2 destination = new Vector2(ParseFloat(xCam), ParseFloat(yCam));
                            if (spaceCam == "local")
                                destination += (Vector2)PlayState.cam.transform.position;
                            else if (spaceCam != "world")
                                destination += (Vector2)ParseActor(spaceCam).position;
                            TryForToken(tokens, thisTokenNum + 6, out string easeType);

                            if (easeType == "linear" || easeType == "smooth")
                            {
                                while (elapsed < maxTime)
                                {
                                    elapsed += Time.deltaTime;
                                    switch (easeType)
                                    {
                                        case "linear":
                                            PlayState.camCutsceneOffset = Vector2.Lerp(start, destination, elapsed / maxTime);
                                            break;
                                        case "smooth":
                                            PlayState.camCutsceneOffset = new Vector2(Mathf.SmoothStep(start.x, destination.x, elapsed / maxTime),
                                                Mathf.SmoothStep(start.y, destination.y, elapsed / maxTime));
                                            break;
                                    }
                                    yield return new WaitForEndOfFrame();
                                }
                            }
                            else
                            {
                                while (Vector2.Distance(start, destination) > 0.01)
                                {
                                    PlayState.camCutsceneOffset = Vector2.Lerp(start, destination, maxTime);
                                    yield return new WaitForEndOfFrame();
                                }
                            }
                            PlayState.camCutsceneOffset = destination;
                        }
                    }
                }
                break;

            case "if":
                break;

            case "else":
                break;

            case "endif":
                break;

            case "loop":
                break;

            case "endloop":
                break;

            case "wait":
                if (TryForToken(tokens, thisTokenNum + 1, out string timeWait))
                    yield return new WaitForSeconds(ParseFloat(timeWait));
                break;

            case "create":
                if (TryForToken(tokens, thisTokenNum + 1, out string objectCreate))
                {
                    Vector2 position = Vector2.zero;
                    TryForToken(tokens, thisTokenNum + 2, out string atCreate);
                    if (atCreate == "at")
                    {
                        if (tokens.Length - thisTokenNum + 1 == 4)
                        {
                            if (TryForToken(tokens, thisTokenNum + 2, out string actorTargetCreate) &&
                                TryForToken(tokens, thisTokenNum + 3, out string xOffsetCreate) && TryForToken(tokens, thisTokenNum + 4, out string yOffsetCreate))
                                position = (Vector2)ParseActor(actorTargetCreate).position + new Vector2(ParseFloat(xOffsetCreate), ParseFloat(yOffsetCreate));
                        }
                        else if (tokens.Length - thisTokenNum + 1 == 3)
                        {
                            if (TryForToken(tokens, thisTokenNum + 2, out string xCreate) && TryForToken(tokens, thisTokenNum + 3, out string yCreate))
                                position = new Vector2(ParseFloat(xCreate), ParseFloat(yCreate));
                        }
                        else if (tokens.Length - thisTokenNum + 1 == 2)
                        {
                            if (TryForToken(tokens, thisTokenNum + 2, out string actorCreate))
                                position = ParseActor(actorCreate).position;
                        }
                    }
                    Instantiate(Resources.Load<GameObject>(objectCreate), position, Quaternion.identity, transform.parent);
                }
                break;

            case "particle":
                if (TryForToken(tokens, thisTokenNum + 1, out string particleParticle))
                {
                    Vector2 position = Vector2.zero;
                    TryForToken(tokens, thisTokenNum + 2, out string atCreate);
                    if (atCreate == "at")
                    {
                        if (tokens.Length - thisTokenNum + 1 == 4)
                        {
                            if (TryForToken(tokens, thisTokenNum + 2, out string actorTargetParticle) &&
                                TryForToken(tokens, thisTokenNum + 3, out string xOffsetParticle) && TryForToken(tokens, thisTokenNum + 4, out string yOffsetParticle))
                                position = (Vector2)ParseActor(actorTargetParticle).position + new Vector2(ParseFloat(xOffsetParticle), ParseFloat(yOffsetParticle));
                        }
                        else if (tokens.Length - thisTokenNum + 1 == 3)
                        {
                            if (TryForToken(tokens, thisTokenNum + 2, out string xParticle) && TryForToken(tokens, thisTokenNum + 3, out string yParticle))
                                position = new Vector2(ParseFloat(xParticle), ParseFloat(yParticle));
                        }
                        else if (tokens.Length - thisTokenNum + 1 == 2)
                        {
                            if (TryForToken(tokens, thisTokenNum + 2, out string actorParticle))
                                position = ParseActor(actorParticle).position;
                        }
                    }
                    PlayState.RequestParticle(position, particleParticle);
                }
                break;

            case "shake":
                List<float> timesShake = new List<float>();
                List<float> intensitiesShake = new List<float>();
                bool assignFlag = false;
                for (int i = thisTokenNum + 1; i < tokens.Length; i++)
                {
                    if (assignFlag)
                        intensitiesShake.Add(ParseFloat(tokens[i]));
                    else
                        timesShake.Add(ParseFloat(tokens[i]));
                    assignFlag = !assignFlag;
                }
                PlayState.globalFunctions.ScreenShake(intensitiesShake, timesShake);
                break;

            case "sound":
                if (TryForToken(tokens, thisTokenNum + 1, out string soundSound))
                    PlayState.PlaySound(PlayState.GetSound(soundSound));
                break;

            case "music":
                if (TryForToken(tokens, thisTokenNum + 1, out string areaMusic) && TryForToken(tokens, thisTokenNum + 2, out string subzoneMusic))
                    PlayState.PlayAreaSong(ParseInt(areaMusic), ParseInt(subzoneMusic));
                break;

            case "settile":
                if (TryForToken(tokens, thisTokenNum + 1, out string xTile) && TryForToken(tokens, thisTokenNum + 2, out string yTile) &&
                    TryForToken(tokens, thisTokenNum + 3, out string spaceTile) && TryForToken(tokens, thisTokenNum + 4, out string idTile) &&
                    TryForToken(tokens, thisTokenNum + 5, out string layerTile))
                {
                    Vector3Int position = new Vector3Int(ParseInt(xTile), ParseInt(yTile), 0);
                    if (spaceTile != "world")
                        position += Vector3Int.RoundToInt(ParseActor(spaceTile).position);
                    Tilemap map = layerTile switch
                    {
                        "sp" => PlayState.specialLayer.GetComponent<Tilemap>(),
                        "fg2" => PlayState.fg2Layer.GetComponent<Tilemap>(),
                        "fg1" => PlayState.fg1Layer.GetComponent<Tilemap>(),
                        "bg" => PlayState.bgLayer.GetComponent<Tilemap>(),
                        "sky" => PlayState.skyLayer.GetComponent<Tilemap>(),
                        _ => PlayState.groundLayer.GetComponent<Tilemap>()
                    };
                    PlayState.tempTiles.Add(ParseInt(xTile));
                    PlayState.tempTiles.Add(ParseInt(yTile));
                    PlayState.tempTiles.Add(layerTile switch
                    {
                        "sp" => 0,
                        "fg2" => 1,
                        "fg1" => 2,
                        "ground" => 3,
                        "bg" => 4,
                        "sky" => 5,
                        _ => 0
                    });
                    if (ParseInt(idTile) == -1)
                    {
                        PlayState.tempTiles.Add(-1);
                        map.SetTile(position, null);
                    }
                    else
                    {
                        PlayState.tempTiles.Add(ParseInt(map.GetTile(position).name.Split('_')[1]));
                        Tile newTile = ScriptableObject.CreateInstance<Tile>();
                        Sprite newSprite = PlayState.GetSprite("Tilesheet", ParseInt(idTile));
                        newSprite.OverridePhysicsShape(new List<Vector2[]> {
                            new Vector2[] { new Vector2(0, 0), new Vector2(0, 16), new Vector2(16, 16), new Vector2(16, 0) }
                            });
                        newTile.sprite = newSprite;
                        newTile.name = "Tilesheet_" + ParseInt(idTile);
                        map.SetTile(position, newTile);
                    }
                }
                break;

            case "end":
                box.enabled = false;
                endFlag = true;
                if (TryForToken(tokens, thisTokenNum + 1, out string foreverEnd))
                {
                    if (foreverEnd == "forever")
                    {
                        endedForever = true;
                        PlayState.cutscenesToNotSpawn.Add(cutsceneID);
                    }
                }    
                break;
        }
        if (endActionHere)
            runningActions--;
    }

    public void EndActionRemote()
    {
        runningActions--;
    }
}
