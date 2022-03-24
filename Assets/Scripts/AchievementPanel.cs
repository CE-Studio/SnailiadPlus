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
    public Animator anim;
    public AudioSource sfx;
    public AudioClip jingle;

    public List<string> popupQueue = new List<string>();
    public string currentAchievement;
    // All achievement IDs sent to and handled by this script are abbreviated or simplified in some way
    //
    //  1 - fo4   (First of Four)
    //  2 - stink (Stinky Toe)
    //  3 - grav  (Gravity Battle)
    //  4 - vict  (Victory)
    //  5 - scout (Scout)
    //  6 - expl  (Explorer)
    //  7 - happy (Happy Ending)
    //  8 - hunt  (Treasure Hunter)
    //  9 - hless (Homeless)
    // 10 - topfl (Top Floor)
    // 11 - mnsn  (Mansion)
    // 12 - rent  (Just Renting)
    // 13 - attic (Attic Dweller)
    // 14 - speed (Speedrunner)
    // 15 - gaunt (The Gauntlet)
    // 16 - plgrm (Pilgrim)
    // 17 - snlka (Snelk Hunter A)
    // 18 - snlkb (Snelk Hunter B)
    // 19 - secrt (Super Secret)
    // 20 - count (Counter-Snail)
    // 21 - maze  (Birds in the Maze Room)
    // 22 - where (Where are we, Snaily?)
    // 23 - omega (Omega Snail)
    // 24 - rando (How did you get up here?)
    public bool runningPopup = false;
    
    public void Start()
    {
        //blankIcon = (Sprite)Resources.Load("Images/Blank");

        anim = GetComponent<Animator>();
        sprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
        text = transform.GetChild(1).GetChild(0).GetComponent<TextMesh>();
        shadow = transform.GetChild(1).GetChild(1).GetComponent<TextMesh>();
        sfx = GetComponent<AudioSource>();
        jingle = (AudioClip)Resources.Load("Sounds/Music/AchievementJingle");

        CloseAchievement();
    }

    private void Update()
    {
        if (popupQueue.Count != 0 && !runningPopup)
            RunPopup(popupQueue[0]);
    }

    private void RunPopup(string achievementID)
    {
        runningPopup = true;
        currentAchievement = achievementID;
        anim.Play("Achievement Popup", 0, 0);
        sfx.PlayOneShot(jingle);
    }

    public void DisplayAchievement()
    {
        sprite.enabled = true;
        switch (currentAchievement)
        {
            case "fo4":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 0);
                text.text = "First of\nFour";
                shadow.text = "First of\nFour";
                PlayState.achievementStates[0] = 1;
                break;
            case "stink":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 1);
                text.text = "Stinky Toe";
                shadow.text = "Stinky Toe";
                PlayState.achievementStates[1] = 1;
                break;
            case "grav":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 2);
                text.text = "Gravity\nBattle";
                shadow.text = "Gravity\nBattle";
                PlayState.achievementStates[2] = 1;
                break;
            case "vict":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 3);
                text.text = "Victory";
                shadow.text = "Victory";
                PlayState.achievementStates[3] = 1;
                break;
            case "scout":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 4);
                text.text = "Scout";
                shadow.text = "Scout";
                PlayState.achievementStates[4] = 1;
                break;
            case "expl":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 5);
                text.text = "Explorer";
                shadow.text = "Explorer";
                PlayState.achievementStates[5] = 1;
                break;
            case "happy":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 6);
                text.text = "Happy\nEnding";
                shadow.text = "Happy\nEnding";
                PlayState.achievementStates[6] = 1;
                break;
            case "hunt":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 7);
                text.text = "Treasure\nHunter";
                shadow.text = "Treasure\nHunter";
                PlayState.achievementStates[7] = 1;
                break;
            case "hless":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 8);
                text.text = "Homeless";
                shadow.text = "Homeless";
                PlayState.achievementStates[8] = 1;
                break;
            case "topfl":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 9);
                text.text = "Top Floor";
                shadow.text = "Top Floor";
                PlayState.achievementStates[9] = 1;
                break;
            case "mnsn":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 10);
                text.text = "Mansion";
                shadow.text = "Mansion";
                PlayState.achievementStates[10] = 1;
                break;
            case "rent":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 11);
                text.text = "Just\nRenting";
                shadow.text = "Just\nRenting";
                PlayState.achievementStates[11] = 1;
                break;
            case "attic":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 12);
                text.text = "Attic\nDweller";
                shadow.text = "Attic\nDweller";
                PlayState.achievementStates[12] = 1;
                break;
            case "speed":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 13);
                text.text = "Speedrunner";
                shadow.text = "Speedrunner";
                PlayState.achievementStates[13] = 1;
                break;
            case "gaunt":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 14);
                text.text = "The\nGauntlet";
                shadow.text = "The\nGauntlet";
                PlayState.achievementStates[14] = 1;
                break;
            case "plgrm":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 15);
                text.text = "Pilgrim";
                shadow.text = "Pilgrim";
                PlayState.achievementStates[15] = 1;
                break;
            case "snlka":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 16);
                text.text = "Snelk\nHunter A";
                shadow.text = "Snelk\nHunter A";
                PlayState.achievementStates[16] = 1;
                break;
            case "snlkb":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 17);
                text.text = "Snelk\nHunter B";
                shadow.text = "Snelk\nHunter B";
                PlayState.achievementStates[17] = 1;
                break;
            case "secrt":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 18);
                text.text = "Super\nSecret";
                shadow.text = "Super\nSecret";
                PlayState.achievementStates[18] = 1;
                break;
            case "count":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 19);
                text.text = "Counter-\nSnail";
                shadow.text = "Counter-\nSnail";
                PlayState.achievementStates[19] = 1;
                break;
            case "maze":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 20);
                text.text = "Birds in the\nMaze Room";
                shadow.text = "Birds in the\nMaze Room";
                PlayState.achievementStates[20] = 1;
                break;
            case "where":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 21);
                text.text = "Where are\nwe, Snaily?";
                shadow.text = "Where are\nwe, Snaily?";
                PlayState.achievementStates[21] = 1;
                break;
            case "omega":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 22);
                text.text = "Omega Snail";
                shadow.text = "Omega Snail";
                PlayState.achievementStates[22] = 1;
                break;
            case "rando":
                sprite.sprite = PlayState.GetSprite("AchievementIcons", 23);
                text.text = "How did you get up here?";
                shadow.text = "How did you get up here?";
                PlayState.achievementStates[23] = 1;
                break;
            default:
                popupQueue.RemoveAt(0);
                break;
        }
    }

    public void CloseAchievement()
    {
        text.text = "";
        shadow.text = "";
        sprite.enabled = false;
    }

    public void ClearActiveAchievementSlot()
    {
        popupQueue.RemoveAt(0);
        currentAchievement = "";
        runningPopup = false;
    }
}
