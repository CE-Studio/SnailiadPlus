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
        if (PlayState.gameState == "Game")
        {
            anim.speed = 1;

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
                                textToSend.Add("Hi, " + playerName + "!!  {p|0.25}Why don\'t you try{nl|}climbing up the walls?{nl|}    {p|0.25}Just {col|0309}hold \"" +
                                    Control.ParseKeyName(2) + "\" and \"" + Control.ParseKeyName(1) + "\"{col|0312}.");
                            else if (!PlayState.CheckForItem("Boomerang"))
                                textToSend.Add("Oh, {p|0.0625}nice pea shooter!  {p|0.125}I heard{nl|}you can shoot a {col|0104}blue door {col|0312}open{nl|}with one of those!");
                            else if (!PlayState.CheckForItem("Rainbow Wave"))
                                textToSend.Add("Wow, a boomerang!  {p|0.125}You could{nl|}break a {col|0201}pink door {col|0312}open with{nl|}just one of those!");
                            else if (!PlayState.CheckForItem("Devastator"))
                                textToSend.Add("Is that a rainbow wave!?  {p|0.25}Well,{nl|}then!  {p|0.25}You can open a {col|0111}red door{col|0312}{nl|}with one of those!");
                            else if (PlayState.GetItemPercentage() < 100)
                                textToSend.Add("{sfx|1}A {eff|Shake}devastator!?  {eff|None}{sfx|0}{p|0.4}That opens up{nl|}{col|0207}green doors!  {p|0.25}{col|0312}It also upgrades{nl|}all three weapons!  Wow!!");
                            else
                                textToSend.Add("I hope the next game has more{nl|}weapons.  {p|0.25}This game could have{nl|}used a {eff|Wave}flame whip!");
                            break;

                        case 1:
                            if (!PlayState.hasJumped && !PlayState.CheckForItem("Peashooter") && !PlayState.CheckForItem("Boomerang") && !PlayState.CheckForItem("Super Secret Boomerang"))
                                textToSend.Add("Hiya, " + playerName + "!  {p|0.125}Did you know you{nl|}can jump?  {p|0.125}Just {col|0309}press \"" + Control.ParseKeyName(4) + "\"{col|0312}!");
                            else if (!PlayState.CheckForItem("Peashooter"))
                                textToSend.Add(playerName + ", {p|0.125}some snails are missing!{p|0.25}{nl|}Do you think you could go look{nl|}for them?  {p|0.25}I\'m getting worried!");
                            else if (PlayState.GetItemPercentage() < 100)
                                textToSend.Add("I have a goal in life.  {p|0.25}One day,{nl|}I will eat a pizza.  {p|0.4}I mean it!!{p|0.4}{spd|0.06}{nl|}Just you watch, {p|0.0625}" + playerName + "!!");
                            else
                                textToSend.Add(playerName + ", {p|0.125}you missed it!  {p|0.25}I made a{nl|}delicious pizza, {p|0.125}and I ate the{nl|}whole {p|0.125}thing!!!  {p|0.3}{sfx|2}{eff|Wave}{spd|0.04}Om nom nom!");
                            break;

                        case 2:
                            if (!PlayState.CheckForItem("Peashooter") && !PlayState.CheckForItem("Boomerang"))
                                textToSend.Add("{col|0303}I can see the future, " + playerName + "!{col|0312}{p|0.3}{nl|}I see you finding a weapon{nl|}somewhere under the water!");
                            else if (!PlayState.CheckBossState(0) && !PlayState.CheckBossState(1))
                                textToSend.Add("{col|0303}I can see the future, " + playerName + "!{col|0312}{p|0.3}{nl|}I see you heading to the upper{nl|}right part of the map!!");
                            else if (!PlayState.CheckForItem("Boomerang"))
                                textToSend.Add("{col|0303}I can see the past, " + playerName + "!{col|0312}{p|0.3}{nl|}I see you forgot to grab the{nl|}boomerang after Shellbreaker!");
                            else if (!PlayState.CheckBossState(1))
                                textToSend.Add("{col|0303}I can see the future, " + playerName + "!{col|0312}{p|0.3}{nl|}I see you heading to the lower{nl|}right part of the map!!");
                            else if (!PlayState.CheckForItem("Rainbow Wave"))
                                textToSend.Add("{col|0303}I can see the past, " + playerName + "!{col|0312}{p|0.3}{nl|}I see you forgot to grab the{nl|}rainbow wave after Stompy!");
                            else if (!PlayState.CheckBossState(2))
                                textToSend.Add("{col|0303}I can see the future, " + playerName + "!{col|0312}{p|0.3}{nl|}I see you heading to the lower{nl|}left part of the map!!");
                            else if (!PlayState.CheckForItem(6))
                                textToSend.Add("{col|0303}I can see the past, " + playerName + "!{col|0312}{p|0.3}{nl|}I see you forgot to grab the{nl|}rapid fire before Space Box!");
                            else if (!PlayState.CheckBossState(3))
                                textToSend.Add("{col|0303}I can see the future, " + playerName + "!{col|0312}{p|0.3}{nl|}I see you heading to the upper{nl|}left part of the map!!");
                            else if (PlayState.helixCount < 30)
                                textToSend.Add("{col|0303}I can see the future, " + playerName + "!{col|0312}{p|0.3}{nl|}I see you heading all over the{nl|}map, finding little shells!");
                            else if (!PlayState.hasSeenIris)
                                textToSend.Add("{col|0303}I can see the future, " + playerName + "!{col|0312}{p|0.3}{nl|}I see you heading precisely two{nl|}screens to the left of here!!");
                            else
                                textToSend.Add("{col|0303}I can see the future, " + playerName + "!{col|0312}{p|0.3}{nl|}I see you having 18,000,000{nl|}baby " +
                                    playerSpecies + (playerSpecies == "Leech" ? "e" : "") + "s!!  {p|0.25}{eff|Wave}Congratulations!!");
                            break;

                        case 3:
                            if (!PlayState.CheckForItem(8))
                                textToSend.Add("I wonder why I can\'t crawl on{nl|}ceiling corners...  {p|0.25}Do you think{nl|}I\'ll ever be able to, " + playerName + "?");
                            else if (!PlayState.CheckForItem("Full-Metal Snail"))
                                textToSend.Add("Oh, my, {p|0.125}you\'re a {col|0204}gravity " + playerSpecies + "{col|0312}!{p|0.25}{nl|}You must be really good at{nl|}crawling around ceilings!!");
                            else
                                textToSend.Add("Oh, my, {p|0.125}you\'re a {col|0301}full metal{nl|}" + playerSpecies + "{col|0312}!{p|0.25} You must be really good{nl|}at crawling around ceilings!!");
                            break;

                        case 4:
                            if (PlayState.IsTileSolid(new Vector2(transform.position.x - 2.5f, transform.position.y)))
                                textToSend.Add("I wish I had some way to break{nl|}green blocks.  {p|0.125}Those suckers{nl|}are always getting in my way!!");
                            else
                                textToSend.Add("{eff|Wave}Whew!  {eff|None}{p|0.25}Thanks, " + playerName + "!  {p|0.125}I think{nl|}those blocks were planning to{nl|}attack!!  {p|0.125}You saved the day!!");
                            break;

                        case 5:
                            if (PlayState.GetItemPercentage() < 100)
                                textToSend.Add("There\'s a lot of secrets{nl|}in and around Snail Town.{p|0.25}{nl|}Have you found them all?");
                            else
                                textToSend.Add("They say {col|0311}Boss Rush {col|0312}is the{nl|}true test of snail skill.{p|0.3}{nl|}{spd|0.04}But what about slug skill?");
                            break;

                        case 6:
                            if (PlayState.CheckForItem(4) || PlayState.CheckForItem(8))
                                textToSend.Add("Don\'t you think it\'s weird that{nl|}we all live in treehouses?");
                            else
                                textToSend.Add("Hey " + playerName + ", {p|0.125}how\'d you get up{nl|}here?  {p|0.4}And how do I get down?");
                            break;

                        case 7:
                            if (!PlayState.CheckForItem("Peashooter"))
                                textToSend.Add("Are you leaving town, " + playerName + "?{p|0.25}{nl|}Well,{p|0.125} be careful!  {p|0.25}Make sure{nl|}you {col|0206}save your game often{col|0312}!!");
                            else if (!PlayState.CheckForItem(1))
                                textToSend.Add("Hey, " + playerName + "!  {p|0.25}Where\'d you get{nl|}the pea shooter?");
                            else if (!PlayState.CheckForItem(2))
                                textToSend.Add("Ooh, {p|0.125}boomerangs!  {p|0.25}If I had{nl|}those, {p|0.125}I\'d try breaking the{nl|}ceiling over the save spot!");
                            else if (!PlayState.CheckForItem(3))
                                textToSend.Add("Rainbows are so pretty!{nl|}{p|0.125}Don\'t you think so, " + playerName + "?");
                            else if (PlayState.CheckBossState(3))
                                textToSend.Add("I\'m scared, " + playerName + "!{p|0.25}{nl|}Is Moon Snail going to take{nl|}me away like the others?");
                            else
                                textToSend.Add("Snail Town is safe again,{p|0.125}{nl|}thanks to you, {p|0.125}" + playerName + "!");
                            break;

                        case 8:
                            if (PlayState.IsTileSolid(new Vector2(transform.position.x + 8.5f, transform.position.y)))
                                textToSend.Add("There\'s something funny about{nl|}that tree...");
                            else
                                textToSend.Add("I {p|0.125}{eff|Shake}knew {p|0.125}{eff|None}there was something{nl|}weird about that tree!!");
                            break;

                        case 9:
                            if (PlayState.itemPercentage < 100)
                                textToSend.Add("The other snails live in houses,{p|0.125}{nl|}but I like it here in the dirt.{p|0.25}{nl|}Isn\'t it nice in here?");
                            else
                                textToSend.Add("It\'s so cozy in here!  {p|0.25}I just{nl|}{p|0.125}{eff|Wave}{spd|0.06}love {p|0.125}{eff|None}{spd|0.02}my little underground{nl|}home!");
                            break;

                        case 10:
                            textToSend.Add("{spd|0.05}Oh, {p|0.125}" + playerFullName + "!  {p|0.25}{spd|0.02}My heart{nl|}will forever belong to you!{nl|}             {col|0201}{eff|Wave}<3");
                            break;

                        case 12:
                            if (!transform.parent.Find("Item").GetComponent<Item>().collected)
                                textToSend.Add("Heya, " + playerName + "! {p|0.125}I filled the heart{nl|}container over there with some{nl|}fresh slime! {p|0.25}Enjoy!!");
                            else
                                textToSend.Add("Isn\'t breaking blocks {eff|Wave}fun!?");
                            break;

                        case 13:
                            textToSend.Add("{eff|Wave}Wow!  {eff|None}{p|0.25}It looks like you\'ve{nl|}found {col|0302}" + PlayState.GetItemPercentage() + "% {col|0312}of the items in this{nl|}game!  {p|0.25}Nice going, " + playerName + "!");
                            break;

                        case 14:
                            if (PlayState.helixCount < 15)
                                textToSend.Add("Hey, " + playerName + "!  {p|0.25}Keep an eye out{nl|}for {col|0302}\"Helix Fragments\"{col|0312}.  {p|0.25}They{nl|}look like spinning white shells!");
                            else if (PlayState.helixCount < 30 || !PlayState.hasSeenIris)
                                textToSend.Add("They say that the {col|0302}Shrine of{nl|}Iris {col|0312}is not located on any map!");
                            else
                                textToSend.Add("Wow, " + playerName + "!  {p|0.125}You\'re pretty good{nl|}at finding secrets!  {p|0.25}Maybe you{nl|}should become a detective!");
                            break;

                        case 15:
                            if (PlayState.GetItemPercentage() < 20)
                                textToSend.Add("Hi, " + playerName + "!  {p|0.125}Let me give you a{nl|}hint: {p|0.125}Come back to town after{nl|}each area to find secret items!");
                            else if (PlayState.GetItemPercentage() < 40)
                            {
                                switch (playerName)
                                {
                                    case "Snaily":
                                    case "Upside":
                                        textToSend.Add("Hi, " + playerName + "!  {p|0.125}Let me give you a{nl|}little hint: {p|0.125}A shell can fit{nl|}where a snail cannot!");
                                        break;
                                    case "Sluggy":
                                        textToSend.Add("Hi, " + playerName + "!  {p|0.125}Here\'s a li\'l hint:{p|0.125}{nl|}Slugs may be more fragile than{nl|}snails, {p|0.125}but they\'re also faster!");
                                        break;
                                    case "Leggy":
                                        textToSend.Add("Hi, " + playerName + "!  {p|0.125}Here's a tip: {p|0.125}It may{nl|}look too big, {p|0.125}but your shell{nl|}can fit in places ours can!");
                                        break;
                                    case "Blobby":
                                        textToSend.Add("Hi, " + playerName + "!  {p|0.125}Here's a little tip:{p|0.125}{nl|}You can still jump after a side{nl|}hop!  {p|0.125}It\'s called \"Coyote Time\"!!");
                                        break;
                                    case "Leechy":
                                        textToSend.Add("Hi, " + playerName + "!  {p|0.125}Here\'s a li\'l hint:{p|0.125}{nl|}Leeches may be more fragile than{nl|}snails, {p|0.125}but they\'re also faster!");
                                        break;
                                }
                            }
                            else if (PlayState.GetItemPercentage() < 60)
                                textToSend.Add("Hi, " + playerName + "!  {p|0.125}Let me give you a{nl|}hint: {p|0.125}Sometimes, you\'ll miss a{nl|}secret if you cling to a wall!");
                            else if (PlayState.GetItemPercentage() < 80)
                                textToSend.Add("Hi, " + playerName + "!  {p|0.125}Let me give you a{nl|}hint: {p|0.125}You can find the \"high{nl|}jump\" before any other item!");
                            else if (PlayState.GetItemPercentage() < 100)
                                textToSend.Add("Hi, " + playerName + "!  {p|0.125}Let me tell you a{nl|}secret: {p|0.125}There\'s a boomerang{nl|}somewhere in Snail Town!");
                            else
                                textToSend.Add("Hi, " + playerName + "!  {p|0.125}Let me tell you a{nl|}secret: {p|0.125}there\'s a secret{nl|}somewhere in the main menu!");
                            break;

                        case 16:
                            if (!PlayState.CheckForItem("Peashooter") && !PlayState.CheckForItem("Boomerang") && !PlayState.CheckForItem("Super Secret Boomerang"))
                            {
                                if (playerName == "Leechy")
                                    textToSend.Add("If you ever get hurt, {p|0.125}defeat{nl|}enemies!  {p|0.25}As a leech, {p|0.125}you can{nl|}earn {col|0201}pink heath orbs {col|0312}off them!");
                                else
                                    textToSend.Add("If you ever get hurt, {p|0.125}{col|0207}eat some{nl|}plants{col|0312}!  {p|0.25}You need to eat well{nl|}to stay fit and healthy!");
                            }
                            else if (transform.localPosition.y > origin.y - 21)
                                textToSend.Add("I want to go for a ride!!!");
                            else
                                textToSend.Add("{eff|Wave}{sfx|2}{spd|0.05}WHEEEE!!  {p|0.125}{eff|None}{sfx|0}{spd|0.02}That was fun,{nl|}" + playerName + "!  {p|0.125}Let\'s do it again!!");
                            break;

                        case 17:
                            if (PlayState.GetItemPercentage() < 100)
                                textToSend.Add("There\'s a hidden room under the{nl|}path into town.  {p|0.25}I don\'t know{nl|}how to get in yet, though...");
                            else
                                textToSend.Add("They call me {col|0209}upside-down snail{col|0312},{p|0.25}{nl|}but I think everyone else is{nl|}upside down!");
                            break;

                        case 18:
                            if (PlayState.CheckForItem("Super Secret Boomerang"))
                                textToSend.Add("Don\'t forget!{p|0.25}{nl|}Press {col|0309}\"" + Control.ParseKeyName(5) + "\" {col|0312}to shoot your{nl|}weapon at stuff!!");
                            else
                                textToSend.Add("You found the {eff|Wave}super secret{nl|}boomerang!  {eff|None}{p|0.125}Way to go!{p|0.25}{nl|}Press {col|0309}\"" + Control.ParseKeyName(5) + "\" {col|0312}to shoot with it!");
                            break;

                        case 19:
                            if (!PlayState.CheckForItem("Boomerang") && !PlayState.CheckForItem("Super Secret Boomerang"))
                                textToSend.Add("Hey, " + playerName + "! {p|0.25}If you had a{nl|}boomerang, {p|0.125}you could break{nl|}all sorts of walls!");
                            else
                                textToSend.Add("{col|0204}Up, {p|0.125}up, {p|0.125}down, {p|0.125}down, {p|0.125}left,{p|0.25}{nl|}{spd|0.06}right...  {p|0.25}{spd|0.02}{col|0312}Wait, {p|0.125}never mind,{p|0.25}{nl|}that\'s for some other game.");
                            break;

                        case 22:
                            if (PlayState.GetItemPercentage() < 100)
                                textToSend.Add("I are {col|0200}Cave Snail{col|0312}!{nl|}{p|0.25}{eff|Shake}Thorgle Borgle!!!!");
                            else
                                textToSend.Add("{eff|Shake}Thorgle Borgle!!!!!!!");
                            PlayState.talkedToCaveSnail = true;
                            break;

                        case 23:
                            if (PlayState.GetItemPercentage() < 100 && !PlayState.talkedToCaveSnail)
                                textToSend.Add("{col|0204}Cave Snail {col|0312}scares me!{nl|}{p|0.25}I\'m staying over here!");
                            else if (PlayState.GetItemPercentage() < 60)
                                textToSend.Add("Hey, {p|0.125}if you get stuck, {p|0.125}just hit{nl|}{col|0309}ESCAPE {col|0312}and load your game from{nl|}town!  {col|0.125}You won\'t lose any items!");
                            else
                                textToSend.Add("Do you think {col|0204}Cave Snail{nl|}{col|0312}is single?");
                            break;

                        case 24:
                            if (!transform.parent.Find("Item").GetComponent<Item>().collected)
                                textToSend.Add("Take this {col|0302}Helix Fragment{col|0312}!{nl|}{p|0.25}Legend says it is but one{nl|}piece of {col|0302}Iris, the Godsnail{col|0312}!");
                            else
                                textToSend.Add(playerName + ", {p|0.125}legend says the{nl|}{col|0302}Shrine of Iris {col|0312}is somewhere{nl|}very close to Snail Town!!");
                            break;

                        case 25:
                            if (!PlayState.CheckForItem("Peashooter"))
                                textToSend.Add("Have you tried hitting {col|0309}\"" + Control.ParseKeyName(21) +
                                    "\"{col|0312}{nl|}yet?  {p|0.25}It makes the map big!  {p|0.25}Oh,{nl|}{p|0.125}and hit {col|0309}\"" + Control.ParseKeyName(22) + "\" {col|0312}for the menu!");
                            else
                                textToSend.Add("Hey " + playerName + ", {p|0.125}I\'m hungry!  {p|0.25}Know{nl|}any good plants around town?");
                            break;

                        case 50:
                            if (PlayState.CheckForItem("Debug Rainbow Wave"))
                                textToSend.Add("{eff|Wave}Woah!!  {p|0.25}{eff|None}Nice Rainbow Wave, " + playerName + "!!{p|0.25}{nl|}I\'d love one too, {p|0.125}but I don\'t{nl|}have a jump button.");
                            else
                                textToSend.Add("Oh, {p|0.125}hey, " + playerName + "! {p|0.25}I'm here to test{nl|}single-page dialogue!!");
                            break;

                        case 51:
                            AddNPCColors(ID);
                            textToSend.Add("Hey there, " + playerName + "!! {p|0.25}I see you{nl|}figured out how to start a{nl|}multi-page conversation!");
                            portraitStateList.Add(1);
                            textToSend.Add("The hope is this talk should go{nl|}100% smoothly. {p|0.125}What do you{nl|}think?");
                            portraitStateList.Add(1);
                            textToSend.Add("Impressive! {p|0.25}I do hope that\'s my{nl|}portrait showing right now, {p|0.125}if it{nl|}even is there.");
                            portraitStateList.Add(0);
                            textToSend.Add("I\'m here to test multiple things,{nl|}it seems!");
                            portraitStateList.Add(1);
                            break;

                        default:
                            textToSend.Add("Hey " + playerName + "!  {p|0.25}Unfortunately I,{nl|}{p|0.125}snail #" + ID + ", {p|0.125}don\'t have any{nl|}dialogue to offer.  {p|0.25}Sorry!!");
                            break;
                    }
                    if (textToSend.Count > 1)
                    {
                        speechBubble.GetComponent<SpriteRenderer>().enabled = true;
                        if (Control.SpeakPress())// && !buttonDown)
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

            //if (Input.GetAxisRaw("Speak") == 1)
            //{
            //    buttonDown = true;
            //}
            //else
            //{
            //    buttonDown = false;
            //}

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
        else
            anim.speed = 0;
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
