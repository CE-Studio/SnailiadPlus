using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Credits : MonoBehaviour
{
    Vector2 camTarget = new(-324.5f, 168.5f);
    float modeTimer = 0;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void StartCredits()
    {
        PlayState.creditsState = PlayState.CreditsStates.fadeIn;
    }

    public void RunMoonCutscene()
    {
        PlayState.creditsState = PlayState.CreditsStates.moonScene;
    }

    public void CreditsRoll()
    {
        PlayState.creditsState = PlayState.CreditsStates.credits;
    }

    public void DisplayFinalTime()
    {
        PlayState.creditsState = PlayState.CreditsStates.time;
    }

    public void EndCredits()
    {
        PlayState.creditsState = PlayState.CreditsStates.none;
    }
}
