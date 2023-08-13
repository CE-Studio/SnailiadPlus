using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Credits : MonoBehaviour
{
    private Vector2 camTarget = new(-324.5f, 168.5f);
    private float modeTimer = 0;
    private bool startedFade = false;

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
                    RunMoonCutscene();
                break;
            case PlayState.CreditsStates.moonScene:
                break;
            case PlayState.CreditsStates.credits:
                break;
            case PlayState.CreditsStates.time:
                break;
        }
    }

    public void StartCredits()
    {
        modeTimer = 0;
        PlayState.creditsState = PlayState.CreditsStates.fadeIn;
        PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 1, 3, 6);
    }

    public void RunMoonCutscene()
    {
        modeTimer = 0;
        PlayState.creditsState = PlayState.CreditsStates.moonScene;
        PlayState.cam.transform.position = camTarget;
    }

    public void CreditsRoll()
    {
        modeTimer = 0;
        PlayState.creditsState = PlayState.CreditsStates.credits;
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
    }
}
