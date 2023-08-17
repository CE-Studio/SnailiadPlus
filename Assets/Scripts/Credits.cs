using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Credits : MonoBehaviour
{
    private const float START_DELAY = 0.8f;

    private Vector2 camTarget = new(-324.5f, 152.5f);
    private float modeTimer = 0;
    private float startDelay = START_DELAY;
    private bool startedFade = false;

    private float[] completionTime = new float[] { };

    private struct BGObj
    {
        public GameObject obj;
        public SpriteRenderer sprite;
        public AnimationModule anim;
    }
    private BGObj background;

    void Start()
    {
        
    }

    void Update()
    {
        switch (PlayState.creditsState)
        {
            default:
            case PlayState.CreditsStates.none:
                break;
            case PlayState.CreditsStates.fadeIn:
                modeTimer += Time.deltaTime;
                if (modeTimer > 9f)
                    StartDelay();
                break;
            case PlayState.CreditsStates.startDelay:
                startDelay -= Time.deltaTime;
                if (startDelay <= 0)
                    RunMoonCutscene();
                break;
            case PlayState.CreditsStates.moonScene:
            case PlayState.CreditsStates.fadeToCredits:
                break;
            case PlayState.CreditsStates.credits:
            case PlayState.CreditsStates.fadeToTime:
                break;
            case PlayState.CreditsStates.time:
                break;
            case PlayState.CreditsStates.fadeOut:
                break;
        }
    }

    public void StartCredits(float[] timeToDisplay)
    {
        modeTimer = 0;
        PlayState.creditsState = PlayState.CreditsStates.fadeIn;
        PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 255, 3, 6);

        completionTime = (float[])timeToDisplay.Clone();

        GameObject newBG = new("Credits Background");
        newBG.transform.parent = PlayState.cam.transform;
        background = new BGObj
        {
            obj = newBG,
            sprite = newBG.AddComponent<SpriteRenderer>(),
            anim = newBG.AddComponent<AnimationModule>()
        };
        background.anim.Add("EndingBackground_moonEnding");
        background.anim.Add("EndingBackground_sunIntro");
        background.anim.Add("EndingBackground_sunEnding");
        background.anim.Add("EndingBackground_credits");
        background.sprite.enabled = false;
    }

    public void StartDelay()
    {
        PlayState.creditsState = PlayState.CreditsStates.startDelay;
        startDelay = START_DELAY;
    }

    public void RunMoonCutscene()
    {
        modeTimer = 0;
        PlayState.ResetAllParticles();
        PlayState.gameState = PlayState.GameState.credits;
        PlayState.creditsState = PlayState.CreditsStates.moonScene;
        PlayState.cam.transform.position = camTarget;
        PlayState.ToggleHUD(false);
        PlayState.moonCutsceneRoom.RemoteActivateRoom(true);
        PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 0, 1);
        PlayState.PlayMusic(0, 4);
    }

    public void CreditsRoll()
    {
        modeTimer = 0;
        PlayState.creditsState = PlayState.CreditsStates.credits;
        PlayState.moonCutsceneRoom.DespawnEverything();
        PlayState.creditsRoom.RemoteActivateRoom(true);
    }

    public void DisplayFinalTime()
    {
        modeTimer = 0;
        PlayState.creditsState = PlayState.CreditsStates.time;
    }

    public void EndCredits()
    {
        modeTimer = 0;
        PlayState.creditsState = PlayState.CreditsStates.none;
        PlayState.LastRoom().ResetEffects();
        Destroy(background.obj);
    }
}
