using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementPanel : MonoBehaviour
{
    public SpriteRenderer frameSprite;
    public SpriteRenderer iconSprite;
    public TextObject text;
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
    public List<Achievements> popupQueue = new();
    public bool runningPopup = false;
    private int runState = 0;
    private float openTime = 0;
    private int colorState = 0;
    private bool advanceColor = false;

    private const float OPEN_TIME = 2.5f;
    
    public void Start()
    {
        anim = GetComponent<AnimationModule>();
        frameSprite = GetComponent<SpriteRenderer>();
        iconSprite = transform.Find("Achievement Icon").GetComponent<SpriteRenderer>();
        text = transform.Find("Achievement Text").GetComponent<TextObject>();
        sfx = GetComponent<AudioSource>();
        jingle = (AudioClip)Resources.Load("Sounds/Music/AchievementJingle");

        anim.Add("AchievementPanel_open");
        anim.Add("AchievementPanel_hold");
        anim.Add("AchievementPanel_close");
        anim.pauseOnMenu = false;

        text.SetText("");
        iconSprite.enabled = false;
        GetComponent<SpriteRenderer>().sprite = PlayState.BlankTexture();
    }

    private void Update()
    {
        if (PlayState.gameState == PlayState.GameState.menu && popupQueue.Count != 0)
        {
            CloseAchievement();
            popupQueue.Clear();
        }
        if (PlayState.gameState != PlayState.GameState.game && PlayState.gameState != PlayState.GameState.menu && PlayState.gameState != PlayState.GameState.pause)
        {
            text.SetColor(new Color(0, 0, 0, 0));
            frameSprite.enabled = false;
            iconSprite.enabled = false;
            return;
        }
        frameSprite.enabled = true;
        iconSprite.enabled = runState == 2;
        if (popupQueue.Count != 0 && runState == 0)
            OpenBox();
        if (!anim.isPlaying && runState == 1)
            RunPopup(popupQueue[0]);
        if (runState == 2)
        {
            advanceColor = !advanceColor;
            if (advanceColor)
                colorState = (colorState + 1) % 4;
            text.SetColor(colorState switch
            {
                0 => new Color32(189, 191, 198, 255),
                1 => new Color32(247, 198, 223, 255),
                2 => new Color32(252, 214, 136, 255),
                _ => new Color32(170, 229, 214, 255)
            });
            if (openTime < OPEN_TIME)
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
        PlayState.achievementOpen = true;
        anim.Play("AchievementPanel_open");
    }

    private void RunPopup(Achievements achievementID)
    {
        runState = 2;
        colorState = 0;
        advanceColor = true;
        currentAchievement = achievementID;
        anim.Play("AchievementPanel_hold");
        DisplayAchievement();
    }

    public void DisplayAchievement()
    {
        iconSprite.enabled = true;
        iconSprite.sprite = PlayState.GetSprite("AchievementIcons", (int)currentAchievement + 1);
        text.SetText(PlayState.GetText("hud_achievement"));
    }

    public void CloseAchievement()
    {
        runState = 3;
        text.SetText("");
        iconSprite.enabled = false;
        PlayState.achievementOpen = false;
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
