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

    public enum Achievements
    {
        BeatShellbreaker,      // (Beat Shellbreaker)
        BeatStompy,            // (Beat Stompy)
        BeatSpaceBox,          // (Beat Space Box)
        BeatMoonSnail,         // (Beat Moon Snail)
        BeatMoonSnailNoFMS,    // (Beat Moon Snail without a Full-Metal item)
        Map100,                // (Find 100% of the map)
        SunSnail,              // (Return Sun Snail's light)
        Items100,              // (Find 100% of all items)
        WinSluggy,             // (Beat the game as Sluggy)
        WinUpside,             // (Beat the game as Upside)
        WinLeggy,              // (Beat the game as Leggy)
        WinBlobby,             // (Beat the game as Blobby)
        WinLeechy,             // (Beat the game as Leechy)
        Under30Minutes,        // (Beat the game in under thirty minutes)
        BossRush,              // (Beat the Boss Rush)
        FindShrine,            // (Find the Shrine of Iris)
        SnelkA,                // (Find the first secret snelk room)
        SnelkB,                // (Find the second secret snelk room)
        SuperSecretBoom,       // (Find the Super Secret Boomerang)
        RemakeTestRooms,       // (Find the remake test rooms)
        MazeBirds,             // (Find them)
        FlashTestRooms,        // (Find the original test rooms)
        WinInsane,             // (Beat the game on insane difficulty)
        WinRandomizer          // (Beat the game on a randomized seed)
    };
    public Achievements currentAchievement;
    public List<Achievements> popupQueue = new List<Achievements>();
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
            if (!PlayState.generalData.achievements[(int)popupQueue[0]])
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

    private void RunPopup(Achievements achievementID)
    {
        runState = 2;
        currentAchievement = achievementID;
        anim.Play("AchievementPanel_hold");
        DisplayAchievement();
    }

    public void DisplayAchievement()
    {
        sprite.enabled = true;
        sprite.sprite = PlayState.GetSprite("AchievementIcons", (int)currentAchievement + 1);
        PlayState.generalData.achievements[(int)currentAchievement] = true;
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
        runningPopup = false;
        openTime = 0;
        runState = 0;
    }
}
