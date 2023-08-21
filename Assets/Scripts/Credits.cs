using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Credits : MonoBehaviour
{
    private const float START_DELAY = 0.8f;
    private const float BG_START_DELAY = 4f;
    private const float BG_FADE_SPEED = 0.5f;
    private const float INTRO_MOON_FADE_SPEED = 2f;
    private const float MOON_FADE_SPEED = 1f;
    private const float ZZZ_FADE_SPEED = 1f;
    private const float ZZZ_SPAWN_TIMEOUT = 1.6f;
    private const float CUTSCENE_CHAR_TIMEOUT = 0.067f;

    private Vector2 camTarget = new(-324.5f, 152.5f);
    private float modeTimer = 0;
    private float startDelay = START_DELAY;
    private bool startedFade = false;
    private bool backgroundActive = false;
    private float backgroundDelay = BG_START_DELAY;
    private float fadeTransitionCountdown;
    private bool zzzActive = false;
    private float zzzDelay = ZZZ_SPAWN_TIMEOUT;

    private float[] completionTime = new float[] { };

    private struct CreditsObj
    {
        public GameObject obj;
        public SpriteRenderer sprite;
        public AnimationModule anim;
    }
    private CreditsObj background;
    private bool fadeBg;
    private List<CreditsObj> moonSprites = new();
    private bool fadeMoon;
    private CreditsObj zzz;
    private bool fadeZzz;
    private bool spawnZzz;

    private bool isSun = false;

    private TextObject cutsceneText;
    private string[] cutsceneLines = new string[3];
    private int charPointer = 0;
    private int stringIndex = 0;
    private bool textVisible = false;
    private float nextCharTimeout = 0;
    private bool startedNewLine = false;

    void Start()
    {
        cutsceneText = transform.Find("Cutscene Text").GetComponent<TextObject>();
        cutsceneText.SetText("");
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
                modeTimer += Time.deltaTime;

                if (modeTimer > 0.9f && !startedFade)
                {
                    startedFade = true;
                    PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 0, 1, 0, -75);
                    PlayState.PlayMusic(0, 4);
                }

                if (!backgroundActive)
                {
                    backgroundDelay -= Time.deltaTime;
                    if (backgroundDelay <= 0)
                    {
                        backgroundActive = true;
                        if (!fadeBg)
                            background.sprite.color = new Color(1, 1, 1, 1);
                        background.sprite.enabled = true;
                        background.anim.Play("EndingBackground_" + (isSun ? "sunIntro" : "moonEnding"));
                    }
                }
                else if (fadeBg)
                    background.sprite.color = new Color(1, 1, 1, PlayState.Integrate(background.sprite.color.a, 1, BG_FADE_SPEED, Time.deltaTime, 0.01f));

                if (modeTimer < 5f || !fadeMoon)
                    FadeMoonSprite(0, 1f, INTRO_MOON_FADE_SPEED);
                else if (modeTimer < 10.5f)
                {
                    FadeMoonSprite(0, 0f, MOON_FADE_SPEED);
                    FadeMoonSprite(1, 1f, MOON_FADE_SPEED);
                }
                else if (modeTimer < 16.5f)
                {
                    if (moonSprites[0].sprite.sortingOrder != -49)
                    {
                        foreach (CreditsObj sprite in moonSprites)
                            sprite.sprite.sortingOrder = -49;
                    }
                    FadeMoonSprite(1, 0f, MOON_FADE_SPEED);
                    FadeMoonSprite(2, 1f, MOON_FADE_SPEED);
                }
                else
                {
                    FadeMoonSprite(2, 0f, MOON_FADE_SPEED);
                    FadeMoonSprite(3, 1f, MOON_FADE_SPEED);
                    zzzDelay -= Time.deltaTime;
                    if (fadeZzz)
                    {
                        if (zzzDelay <= 0 && !zzzActive)
                        {
                            zzzActive = true;
                            zzz.sprite.enabled = true;
                            zzz.sprite.color = new Color(1, 1, 1, 0);
                            zzz.anim.Play("EndingZzz");
                        }
                        if (zzzActive && zzz.sprite.color.a < 1f)
                            FadeZzz(ZZZ_FADE_SPEED);
                    }
                    else if (!fadeZzz && !zzzActive && zzzDelay <= 0)
                    {
                        zzzActive = true;
                        zzz.sprite.enabled = true;
                        zzz.anim.Play("EndingZzz");
                    }
                }

                if (modeTimer > 0.6f && stringIndex == 0 && !textVisible)
                {
                    for (int i = 0; i < 3; i++)
                        cutsceneLines[i] = PlayState.GetText("ending_" + (isSun ? "sun" : "moon") + (i + 1).ToString());
                    textVisible = true;
                }
                else if (modeTimer > 6f && stringIndex == 0)
                {
                    stringIndex++;
                    textVisible = false;
                }
                if (modeTimer > 7f && stringIndex == 1 && !textVisible)
                    textVisible = true;
                else if (modeTimer > 13f && stringIndex == 1)
                {
                    stringIndex++;
                    textVisible = false;
                }
                if (modeTimer > 14f && stringIndex == 2 && !textVisible)
                    textVisible = true;
                else if (modeTimer > 25f && stringIndex == 2)
                {
                    stringIndex++;
                    textVisible = false;
                }

                if (textVisible)
                {
                    if (cutsceneText.thisText.color.a != 1f)
                    {
                        cutsceneText.SetColor(new Color(1, 1, 1, 1));
                        cutsceneText.SetText("");
                    }
                    if (charPointer < cutsceneLines[stringIndex].Length)
                    {
                        nextCharTimeout -= Time.deltaTime;
                        while (nextCharTimeout <= 0)
                        {
                            if (startedNewLine)
                            {
                                string extraWhiteSpace = "";
                                while (cutsceneLines[stringIndex][charPointer] == ' ' || cutsceneLines[stringIndex][charPointer] == '\n')
                                {
                                    extraWhiteSpace += cutsceneLines[stringIndex][charPointer];
                                    charPointer++;
                                }
                                startedNewLine = false;
                                cutsceneText.SetText(cutsceneText.GetText() + extraWhiteSpace);
                            }
                            nextCharTimeout += CUTSCENE_CHAR_TIMEOUT;
                            if (charPointer < cutsceneLines[stringIndex].Length)
                            {
                                if (cutsceneLines[stringIndex][charPointer] != ' ')
                                    PlayState.PlaySound("Dialogue1");
                                if (cutsceneLines[stringIndex][charPointer] == '\n')
                                {
                                    cutsceneText.SetText(cutsceneText.GetText() + '\n');
                                    startedNewLine = true;
                                }
                                else
                                    cutsceneText.SetText(cutsceneText.GetText() + cutsceneLines[stringIndex][charPointer]);
                                charPointer++;
                            }
                        }
                    }
                }
                else
                {
                    cutsceneText.SetColor(new Color(1, 1, 1, cutsceneText.thisText.color.a - Time.deltaTime));
                    charPointer = 0;
                    startedNewLine = true;
                }
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

    private void FadeMoonSprite(int index, float targetAlpha, float speed)
    {
        float alpha = PlayState.Integrate(moonSprites[index].sprite.color.a, targetAlpha, speed, Time.deltaTime, 0.025f);
        moonSprites[index].sprite.color = new Color(1, 1, 1, alpha);
    }

    private void FadeZzz(float speed)
    {
        float alpha = PlayState.Integrate(zzz.sprite.color.a, 1f, speed, Time.deltaTime, 0.025f);
        zzz.sprite.color = new Color(1, 1, 1, alpha);
    }

    public void StartCredits(float[] timeToDisplay)
    {
        modeTimer = 0;
        PlayState.creditsState = PlayState.CreditsStates.fadeIn;
        PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 255, 3, 6);
        isSun = PlayState.CountFragments() == PlayState.MAX_FRAGMENTS;

        completionTime = (float[])timeToDisplay.Clone();

        GameObject newBG = new("Credits Background");
        newBG.transform.parent = PlayState.cam.transform;
        newBG.transform.localPosition = Vector2.zero;
        background = new CreditsObj
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
        background.sprite.sortingOrder = -120;

        fadeBg = PlayState.GetAnim("EndingBackground_data").frames[0] == 1;
        backgroundActive = false;
        background.sprite.color = new Color(1, 1, 1, 0);

        int[] moonData = PlayState.GetAnim("EndingMoonSnail_data").frames;
        fadeMoon = moonData[0] == 1;
        spawnZzz = moonData[1] == 1;
        if (fadeMoon)
        {
            for (int i = 0; i < 4; i++)
            {
                GameObject newMoonSprite = new("Moon Sprite " + (i + 1));
                newMoonSprite.transform.parent = transform;
                newMoonSprite.transform.localPosition = new Vector2(4.5f, -2f);
                CreditsObj newObj = new()
                {
                    obj = newMoonSprite,
                    sprite = newMoonSprite.AddComponent<SpriteRenderer>(),
                    anim = newMoonSprite.AddComponent<AnimationModule>()
                };
                string thisAnimName = i switch
                {
                    0 => "EndingMoonSnail_large",
                    1 => "EndingMoonSnail_medium",
                    2 => "EndingMoonSnail_small",
                    _ => "EndingMoonSnail_" + (isSun ? "sun" : "shell")
                };
                newObj.sprite.sortingOrder = -49;
                newObj.sprite.color = new Color(1, 1, 1, 0);
                newObj.anim.Add(thisAnimName);
                newObj.anim.Play(thisAnimName);
                if (i != 0)
                    newObj.sprite.color = new Color(1, 1, 1, 0);
                moonSprites.Add(newObj);
            }
        }
        else
        {
            GameObject newMoonSprite = new("Moon Sprite");
            newMoonSprite.transform.parent = transform;
            newMoonSprite.transform.localPosition = new Vector2(4.5f, -2f);
            CreditsObj newObj = new()
            {
                obj = newMoonSprite,
                sprite = newMoonSprite.AddComponent<SpriteRenderer>(),
                anim = newMoonSprite.AddComponent<AnimationModule>()
            };
            newObj.sprite.sortingOrder = -49;
            newObj.sprite.color = new Color(1, 1, 1, 0);
            newObj.anim.Add("EndingMoonSnail_large");
            newObj.anim.Add("EndingMoonSnail_medium");
            newObj.anim.Add("EndingMoonSnail_small");
            newObj.anim.Add("EndingMoonSnail_shell");
            newObj.anim.Add("EndingMoonSnail_sun");
            newObj.anim.Play("EndingMoonSnail_large");
        }

        fadeZzz = PlayState.GetAnim("EndingZzz_data").frames[0] == 1;
        GameObject newZzz = new("Moon Zzz");
        newZzz.transform.parent = transform;
        newZzz.transform.localPosition = new Vector2(5.4375f, -2.6875f);
        zzz = new CreditsObj
        {
            obj = newZzz,
            sprite = newZzz.AddComponent<SpriteRenderer>(),
            anim = newZzz.AddComponent<AnimationModule>()
        };
        zzz.anim.Add("EndingZzz");
        zzz.sprite.enabled = false;
        zzz.sprite.sortingOrder = -48;
    }

    public void StartDelay()
    {
        PlayState.creditsState = PlayState.CreditsStates.startDelay;
        PlayState.gameState = PlayState.GameState.credits;
        startDelay = START_DELAY;
        backgroundDelay = BG_START_DELAY;
    }

    public void RunMoonCutscene()
    {
        modeTimer = 0;
        PlayState.ResetAllParticles();
        PlayState.creditsState = PlayState.CreditsStates.moonScene;
        PlayState.cam.transform.position = camTarget;
        PlayState.ToggleHUD(false);
        PlayState.moonCutsceneRoom.RemoteActivateRoom(true);
        PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 255, 0, 0, -75);
        for (int i = 0; i < moonSprites.Count; i++)
            moonSprites[i].sprite.enabled = true;
        zzzActive = false;
        zzzDelay = ZZZ_SPAWN_TIMEOUT;
        charPointer = 0;
        stringIndex = 0;
        textVisible = false;
        nextCharTimeout = 0;
        startedNewLine = true;
        cutsceneText.SetText("");
        cutsceneText.SetColor(new Color(1, 1, 1, 1));
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
