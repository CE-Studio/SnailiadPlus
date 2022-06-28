using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementPanel : MonoBehaviour
{
    public SpriteRenderer sprite;
    public TextMesh text;
    public TextMesh shadow;
    public Sprite blankIcon;
    public Sprite[] achIconArray;
    public AnimationModule anim;
    public AudioSource sfx;
    public AudioClip jingle;

    public List<string> popupQueue = new List<string>();
    public string currentAchievement;
    public Dictionary<string, int> achievementIDs = new Dictionary<string, int>
    {
        { "fo4", 1 },    // "First of Four"            (Beat Shellbreaker)
        { "stink", 2 },  // "Stinky Toe"               (Beat Stompy)
        { "grav", 3 },   // "Gravity Battle"           (Beat Space Box)
        { "vict", 4 },   // "Victory"                  (Beat Moon Snail)
        { "scout", 5 },  // "Scout"                    (Beat Moon Snail without a Full-Metal item)
        { "expl", 6 },   // "Explorer"                 (Find 100% of the map)
        { "happy", 7 },  // "Happy Ending"             (Return Sun Snail's light)
        { "hunt", 8 },   // "Treasure Hunter"          (Find 100% of all items)
        { "hless", 9 },  // "Homeless"                 (Beat the game as Sluggy)
        { "topfl", 10 }, // "Top Floor"                (Beat the game as Upside)
        { "mnsn", 11 },  // "Mansion"                  (Beat the game as Leggy)
        { "rent", 12 },  // "Just Renting"             (Beat the game as Blobby)
        { "attic", 13 }, // "Attic Dweller"            (Beat the game as Leechy)
        { "speed", 14 }, // "Speedrunner"              (Beat the game in under thirty minutes)
        { "gaunt", 15 }, // "The Gauntlet"             (Beat the Boss Rush)
        { "plgrm", 16 }, // "Pilgrim"                  (Find the Shrine of Iris)
        { "snlka", 17 }, // "Snelk Hunter A"           (Find the first secret snelk room)
        { "snlkb", 18 }, // "Snelk Hunter B"           (Find the second secret snelk room)
        { "secrt", 19 }, // "Super Secret"             (Find the Super Secret Boomerang)
        { "count", 20 }, // "Counter Snail"            (Find the remake test rooms)
        { "maze", 21 },  // "Birds in the Maze Room"   (Find them)
        { "where", 22 }, // "Where are we, Snaily?"    (Find the original test rooms)
        { "omega", 23 }, // "Omega Snail"              (Beat the game on insane difficulty)
        { "rando", 24 }  // "How did you get up here?" (Beat the game on a randomized seed)
    };
    public bool runningPopup = false;
    private int runState = 0;
    private float openTime = 0;
    
    public void Start()
    {
        anim = GetComponent<AnimationModule>();
        sprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        text = transform.GetChild(1).GetChild(0).GetComponent<TextMesh>();
        shadow = transform.GetChild(1).GetChild(1).GetComponent<TextMesh>();
        sfx = GetComponent<AudioSource>();
        jingle = (AudioClip)Resources.Load("Sounds/Music/AchievementJingle");

        anim.Add("AchievementPanel_open");
        anim.Add("AchievementPanel_hold");
        anim.Add("AchievementPanel_close");

        text.text = "";
        shadow.text = "";
        sprite.enabled = false;
        GetComponent<SpriteRenderer>().sprite = PlayState.BlankTexture();
    }

    private void Update()
    {
        if (popupQueue.Count != 0 && runState == 0)
        {
            if (PlayState.achievementStates[achievementIDs[popupQueue[0]] - 1] == 0)
                OpenBox();
            else
                popupQueue.RemoveAt(0);
        }
        if (!anim.isPlaying && runState == 1)
            RunPopup(popupQueue[0]);
        if (runState == 2)
        {
            if (openTime < 4)
                openTime += Time.deltaTime;
            else
                CloseAchievement();
        }
        if (!anim.isPlaying && runState == 3)
            ClearActiveAchievementSlot();
    }

    private void OpenBox()
    {
        runState = 1;
        PlayState.PlayMusic(0, 3);
        runningPopup = true;
        anim.Play("AchievementPanel_open");
    }

    private void RunPopup(string achievementID)
    {
        runState = 2;
        currentAchievement = achievementID;
        anim.Play("AchievementPanel_hold");
        DisplayAchievement();
    }

    public void DisplayAchievement()
    {
        sprite.enabled = true;
        switch (currentAchievement)
        {
            case "fo4":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 1);
                PlayState.achievementStates[0] = 1;
                break;
            case "stink":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 2);
                PlayState.achievementStates[1] = 1;
                break;
            case "grav":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 3);
                PlayState.achievementStates[2] = 1;
                break;
            case "vict":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 4);
                PlayState.achievementStates[3] = 1;
                break;
            case "scout":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 5);
                PlayState.achievementStates[4] = 1;
                break;
            case "expl":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 6);
                PlayState.achievementStates[5] = 1;
                break;
            case "happy":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 7);
                PlayState.achievementStates[6] = 1;
                break;
            case "hunt":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 8);
                PlayState.achievementStates[7] = 1;
                break;
            case "hless":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 9);
                PlayState.achievementStates[8] = 1;
                break;
            case "topfl":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 10);
                PlayState.achievementStates[9] = 1;
                break;
            case "mnsn":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 11);
                PlayState.achievementStates[10] = 1;
                break;
            case "rent":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 12);
                PlayState.achievementStates[11] = 1;
                break;
            case "attic":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 13);
                PlayState.achievementStates[12] = 1;
                break;
            case "speed":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 14);
                PlayState.achievementStates[13] = 1;
                break;
            case "gaunt":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 15);
                PlayState.achievementStates[14] = 1;
                break;
            case "plgrm":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 16);
                PlayState.achievementStates[15] = 1;
                break;
            case "snlka":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 17);
                PlayState.achievementStates[16] = 1;
                break;
            case "snlkb":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 18);
                PlayState.achievementStates[17] = 1;
                break;
            case "secrt":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 19);
                PlayState.achievementStates[18] = 1;
                break;
            case "count":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 20);
                PlayState.achievementStates[19] = 1;
                break;
            case "maze":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 21);
                PlayState.achievementStates[20] = 1;
                break;
            case "where":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 22);
                PlayState.achievementStates[21] = 1;
                break;
            case "omega":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 23);
                PlayState.achievementStates[22] = 1;
                break;
            case "rando":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 24);
                PlayState.achievementStates[23] = 1;
                break;
            default:
                popupQueue.RemoveAt(0);
                break;
        }
    }

    public void CloseAchievement()
    {
        runState = 3;
        text.text = "";
        shadow.text = "";
        sprite.enabled = false;
        anim.Play("AchievementPanel_close");
    }

    public void ClearActiveAchievementSlot()
    {
        popupQueue.RemoveAt(0);
        currentAchievement = "";
        runningPopup = false;
        openTime = 0;
        runState = 0;
    }
}
