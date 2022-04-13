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

        //CloseAchievement();
        text.text = "";
        shadow.text = "";
        sprite.enabled = false;
        GetComponent<SpriteRenderer>().sprite = PlayState.BlankTexture();
    }

    private void Update()
    {
        if (popupQueue.Count != 0 && runState == 0)
            OpenBox();
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
        sfx.PlayOneShot(jingle);
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
