using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    private const float SUN_RISE_SPEED = 1f;
    private const float SUN_MAX_RISE = 3f;
    private const float SUN_FLOAT_SPEED = 2f;
    private const float SUN_FLOAT_AMPLITUDE = 0.25f;
    private const float SUN_SCENE_START_OFFSET = 15.5f;
    private const float CREDITS_SCROLL_SPEED = 2.0625f;
    private const float TEXT_VERTICAL_SIZE = 1.125f;
    private const float CREDITS_DONE_TIMER = 7f;
    private const float STATS_FADE_DELAY = 3f;
    private const float OVERWRITE_LERP_VALUE = 15f;
    private const float FINAL_FADEOUT_TIME = 2f;

    private float modeTimer = 0;
    private float startDelay = START_DELAY;
    private bool startedFade = false;
    private bool backgroundActive = false;
    private float backgroundDelay = BG_START_DELAY;
    private float fadeTransitionCountdown;
    private int moonState;
    private bool zzzActive = false;
    private float zzzDelay = ZZZ_SPAWN_TIMEOUT;
    private float sunOriginY;
    private float sunRiseTimer;
    private float sunFloatTimer;
    private bool fadingIntoSun;
    private bool sunSpawned;
    private float creditsGenerateY;
    public bool creditsDone;
    private float creditsDoneTimer = CREDITS_DONE_TIMER;

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
    private bool updateAnimOnRise;
    private CreditsObj zzz;
    private bool fadeZzz;
    private bool spawnZzz;
    private CreditsObj imageBg;
    private CreditsObj endImage;

    private bool isSun = false;

    private TextObject cutsceneText;
    private string[] cutsceneLines = new string[3];
    private int charPointer = 0;
    private int stringIndex = 0;
    private bool textVisible = false;
    private float nextCharTimeout = 0;
    private bool startedNewLine = false;
    private string overwriteString;
    private bool confirmOverwrite;

    private TextObject statsText;
    private TextObject overwriteText;

    private Transform creditsParent;

    private GameObject textObj;
    private GameObject creditsEntity;

    public PlayState.TimeIndeces oldTime = PlayState.TimeIndeces.none;

    #region Entity Roll Call

    public struct EntityEntry
    {
        public string name;
        public int[] tileIDs;
        public bool isPresent;
    }

    public EntityEntry[] presentEntities = new EntityEntry[]
    {
        new EntityEntry { name = "Spikey (blue)", tileIDs = new int[] { 11, 12 } },
        new EntityEntry { name = "Spikey (orange)", tileIDs = new int[] { 13, 14 } },
        new EntityEntry { name = "Babyfish", tileIDs = new int[] { 445, 446 } },
        new EntityEntry { name = "Floatspike (black)", tileIDs = new int[] { 389 } },
        new EntityEntry { name = "Floatspike (blue)", tileIDs = new int[] { 418 } },
        new EntityEntry { name = "Blob", tileIDs = new int[] { 4 } },
        new EntityEntry { name = "Blub", tileIDs = new int[] { 5 } },
        new EntityEntry { name = "Angelblob", tileIDs = new int[] { 415 } },
        new EntityEntry { name = "Devilblob", tileIDs = new int[] { 6 } },
        new EntityEntry { name = "Chirpy (blue)", tileIDs = new int[] { 7, 10 } },
        new EntityEntry { name = "Chirpy (aqua)", tileIDs = new int[] { 397, 451 } },
        new EntityEntry { name = "Batty Bat", tileIDs = new int[] { 414 } },
        new EntityEntry { name = "Fireball", tileIDs = new int[] { 15, 16 } },
        new EntityEntry { name = "Iceball", tileIDs = new int[] { 17, 18 } },
        new EntityEntry { name = "Snelk", tileIDs = new int[] { 424, 425, 452 } },
        new EntityEntry { name = "Kitty (gray)", tileIDs = new int[] { 9 } },
        new EntityEntry { name = "Kitty (orange)", tileIDs = new int[] { 8 } },
        new EntityEntry { name = "Ghost Dandelion", tileIDs = new int[] { 387, 19 } },
        new EntityEntry { name = "Canon", tileIDs = new int[] { 381, 382, 383, 384 } },
        new EntityEntry { name = "Non-Canon", tileIDs = new int[] { 420, 421, 422, 423 } },
        new EntityEntry { name = "Snakey (green)", tileIDs = new int[] { 398 } },
        new EntityEntry { name = "Snakey (blue)", tileIDs = new int[] { 410 } },
        new EntityEntry { name = "Sky Viper", tileIDs = new int[] { 419 } },
        new EntityEntry { name = "Spider", tileIDs = new int[] { 406 } },
        new EntityEntry { name = "Spider Mama", tileIDs = new int[] { 407 } },
        new EntityEntry { name = "Gravity Turtle (green)", tileIDs = new int[] { 408, 409, 453, 454 } },
        new EntityEntry { name = "Gravity Turtle (cherry red)", tileIDs = new int[] { 411, 412, 455, 456 } },
        new EntityEntry { name = "Jellyfish", tileIDs = new int[] { 401 } },
        new EntityEntry { name = "Syngnathida", tileIDs = new int[] { 402 } },
        new EntityEntry { name = "Tallfish (normal)", tileIDs = new int[] { 403 } },
        new EntityEntry { name = "Tallfish (angry)", tileIDs = new int[] { 416 } },
        new EntityEntry { name = "Walleye", tileIDs = new int[] { 405, 417 } },
        new EntityEntry { name = "Pincer", tileIDs = new int[] { 399, 400, 458, 459 } },
        new EntityEntry { name = "Spinnygear", tileIDs = new int[] { 393, 394, 395, 396 } },
        new EntityEntry { name = "Federation Drone", tileIDs = new int[] { 404 } },
        new EntityEntry { name = "Balloon Buster", tileIDs = new int[] { 413, 457 } },
        new EntityEntry { name = "Shellbreaker", tileIDs = new int[] { 23 } },
        new EntityEntry { name = "Stompy", tileIDs = new int[] { 24 } },
        new EntityEntry { name = "Space Box", tileIDs = new int[] { 25 } },
        new EntityEntry { name = "Moon Snail", tileIDs = new int[] { 26 } }
    };
    private readonly List<int> entryEnumIDs = new()
    {
        1, 2, 3, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 22, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 39, 40, 41, 42, 45, 54, 55
    };
    private readonly List<int> entryExceptions = new()
    {
        4, 37, 38
    };

    public void BuildEntityRollCall()
    {
        for (int i = 0; i < presentEntities.Length; i++)
        {
            if (presentEntities[i].name != "Kitty (gray)")
                presentEntities[i].isPresent = true;
            else
                presentEntities[i].isPresent = false;
        }

        //if (PlayState.specialLayer == null)
        //    PlayState.specialLayer = GameObject.Find("Grid/Special");
        //
        //List<int> compiledTileIDs = new() { };
        //List<string> compiledEntityNames = new() { };
        //foreach (EntityEntry entity in presentEntities)
        //{
        //    compiledEntityNames.Add(entity.name);
        //    for (int i = 0; i < entity.tileIDs.Length; i++)
        //        compiledTileIDs.Add(entity.tileIDs[i]);
        //}
        //Tilemap sp = PlayState.specialLayer.GetComponent<Tilemap>();
        //for (int x = 0; x < sp.size.x; x++)
        //{
        //    for (int y = 0; y < sp.size.y; y++)
        //    {
        //        int thisTile = int.Parse(sp.GetTile(new((int)sp.origin.x + x, (int)sp.origin.y + y, 0)).name.Split('_')[1]);
        //        if (compiledTileIDs.Contains(thisTile))
        //        {
        //            for (int i = 0; i < presentEntities.Length; i++)
        //            {
        //                for (int j = 0; j < presentEntities[i].tileIDs.Length; j++)
        //                {
        //                    if (presentEntities[i].tileIDs[j] == thisTile)
        //                    {
        //                        presentEntities[i].isPresent = true;
        //                        j = presentEntities[i].tileIDs.Length;
        //                        i = presentEntities.Length;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        //for (int area = 0; area < PlayState.roomTriggerParent.transform.childCount; area++)
        //{
        //    for (int room = 0; room < PlayState.roomTriggerParent.transform.GetChild(area).childCount; room++)
        //    {
        //        for (int entityNum = 0; entityNum < PlayState.roomTriggerParent.transform.GetChild(area).GetChild(room).childCount; entityNum++)
        //        {
        //            GameObject thisEntity = PlayState.roomTriggerParent.transform.GetChild(area).GetChild(room).GetChild(entityNum).gameObject;
        //            if (compiledEntityNames.Contains(thisEntity.name))
        //            {
        //                for (int i = 0; i < presentEntities.Length; i++)
        //                {
        //                    if (presentEntities[i].name == thisEntity.name)
        //                    {
        //                        presentEntities[i].isPresent = true;
        //                        i = presentEntities.Length;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        //
        //int foundEntities = 0;
        //int totalEntities = 0;
        //for (int i = 0; i < presentEntities.Length; i++)
        //{
        //    totalEntities++;
        //    if (presentEntities[i].isPresent)
        //        foundEntities++;
        //}
        //Debug.Log(string.Format("Found {0} of {1} possible entities", foundEntities, totalEntities));
    }

    #endregion

    void Start()
    {
        cutsceneText = transform.Find("Cutscene Text").GetComponent<TextObject>();
        cutsceneText.SetText("");
        statsText = transform.Find("Final Stats Text").GetComponent<TextObject>();
        statsText.SetColor(new Color(1, 1, 1, 0));
        overwriteText = transform.Find("Overwrite Prompt Text").GetComponent<TextObject>();
        overwriteText.SetColor(new Color(1, 1, 1, 0));
        creditsParent = transform.Find("Credits Roll").transform;
        textObj = Resources.Load<GameObject>("Objects/Text Object");
        creditsEntity = Resources.Load<GameObject>("Objects/Credits Entity");
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
                if (startDelay <= 1)
                    PlayState.fader = startDelay;
                if (startDelay <= 0)
                    RunMoonCutscene();
                break;
            case PlayState.CreditsStates.fadeToCredits:
            case PlayState.CreditsStates.moonScene:
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

                if (modeTimer < 5f)
                    FadeMoonSprite(0, 1f, INTRO_MOON_FADE_SPEED);
                else
                {
                    if (fadeMoon)
                    {
                        if (modeTimer < 10.5f)
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
                    }
                    else
                    {
                        if (modeTimer > 5f && moonState == 0)
                        {
                            moonSprites[0].anim.Play("EndingMoonSnail_medium");
                            moonState = 1;
                        }
                        else if (modeTimer > 10.5f && moonState == 1)
                        {
                            moonSprites[0].sprite.sortingOrder = -49;
                            moonSprites[0].anim.Play("EndingMoonSnail_small");
                            moonState = 2;
                        }
                    }
                }

                if (modeTimer >= 16.5f && !isSun)
                {
                    if (fadeMoon)
                    {
                        FadeMoonSprite(2, 0f, MOON_FADE_SPEED);
                        FadeMoonSprite(3, 1f, MOON_FADE_SPEED);
                    }
                    else if (moonState == 2)
                    {
                        moonSprites[0].anim.Play("EndingMoonSnail_shell");
                        moonState = 3;
                    }
                    if (spawnZzz)
                    {
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
                }

                if (isSun && modeTimer >= SUN_SCENE_START_OFFSET)
                {
                    float currentElapsed = modeTimer - SUN_SCENE_START_OFFSET;

                    CreditsObj moon = fadeMoon ? moonSprites[2] : moonSprites[0];
                    CreditsObj sun = fadeMoon ? moonSprites[3] : moonSprites[0];

                    if (sunRiseTimer >= 1 && !fadingIntoSun)
                    {
                        fadingIntoSun = true;
                        if (updateAnimOnRise)
                            moon.anim.Play("EndingMoonSnail_becomeSun");
                        PlayState.ScreenFlash("Custom Fade", 255, 255, 255, 255, 0.6f);
                    }
                    if (currentElapsed > 1.6f)
                    {
                        sunFloatTimer += Time.deltaTime * SUN_FLOAT_SPEED;
                        if (!sunSpawned)
                        {
                            sunSpawned = true;
                            background.anim.Play("EndingBackground_sunEnding");
                            if (fadeMoon)
                            {
                                moon.sprite.color = new Color(1, 1, 1, 0);
                                sun.sprite.color = new Color(1, 1, 1, 1);
                            }
                            else
                                moon.anim.Play("EndingMoonSnail_sun");
                            PlayState.ScreenFlash("Custom Fade", 255, 255, 255, 0, 1.4f);
                        }
                    }
                    sunRiseTimer = Mathf.Clamp(sunRiseTimer + (Time.deltaTime * SUN_RISE_SPEED), 0, Mathf.PI);
                    float halfRise = SUN_MAX_RISE * 0.5f;
                    float newY = sunOriginY + halfRise - (Mathf.Cos(sunRiseTimer) * halfRise) + (Mathf.Sin(sunFloatTimer) * SUN_FLOAT_AMPLITUDE);
                    moon.obj.transform.position = new Vector2(moon.obj.transform.position.x, newY);
                    if (fadeMoon)
                        sun.obj.transform.position = new Vector2(sun.obj.transform.position.x, newY);
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

                if (PlayState.creditsState == PlayState.CreditsStates.fadeToCredits)
                {
                    fadeTransitionCountdown -= Time.deltaTime;
                    PlayState.fader = fadeTransitionCountdown * 0.5f;
                    if (fadeTransitionCountdown <= 0f)
                    {
                        PlayState.creditsState = PlayState.CreditsStates.credits;
                        PlayState.globalMusic.Stop();
                        PlayState.fader = 1f;
                        CreditsRoll();
                    }
                }
                else if (modeTimer > 26f || (modeTimer > 2f && (Control.CheckKey(Control.Keyboard.Pause) || Control.CheckButton(Control.Controller.Pause))))
                {
                    PlayState.creditsState = PlayState.CreditsStates.fadeToCredits;
                    PlayState.screenCover.color = new Color(0, 0, 0, 0);
                    PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 255, 1.9f);
                    fadeTransitionCountdown = 2f;
                }
                break;
            case PlayState.CreditsStates.credits:
            case PlayState.CreditsStates.fadeToTime:
                modeTimer += Time.deltaTime;
                if (creditsDone)
                    creditsDoneTimer -= Time.deltaTime;
                else
                    creditsParent.transform.localPosition += CREDITS_SCROLL_SPEED * Time.deltaTime * Vector3.up;
                if (PlayState.creditsState == PlayState.CreditsStates.fadeToTime)
                {
                    fadeTransitionCountdown -= Time.deltaTime;
                    if (fadeTransitionCountdown <= 0)
                    {
                        for (int i = creditsParent.childCount - 1; i >= 0; i--)
                            Destroy(creditsParent.GetChild(i).gameObject);
                        DisplayFinalTime();
                    }
                }
                else if (creditsDoneTimer <= 0 || (modeTimer > 1.5f && (Control.CheckKey(Control.Keyboard.Pause) || Control.CheckButton(Control.Controller.Pause))))
                {
                    PlayState.creditsState = PlayState.CreditsStates.fadeToTime;
                    PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 255, 1.9f);
                    fadeTransitionCountdown = 2f;
                }
                break;
            case PlayState.CreditsStates.time:
            case PlayState.CreditsStates.overwriteOldTime:
            case PlayState.CreditsStates.fadeOut:
                modeTimer += Time.deltaTime;
                float layerX = Mathf.Clamp(30f - (modeTimer * 10f), 0, Mathf.Infinity);
                imageBg.obj.transform.localPosition = layerX * PlayState.FRAC_16 * Vector3.right;
                endImage.obj.transform.localPosition = layerX * PlayState.FRAC_16 * Vector3.left;
                if (modeTimer > STATS_FADE_DELAY)
                {
                    float tweakedTime = modeTimer - STATS_FADE_DELAY;
                    statsText.SetColor(new Color(1, 1, 1, tweakedTime));
                }
                if (PlayState.creditsState == PlayState.CreditsStates.fadeOut)
                {
                    fadeTransitionCountdown -= Time.deltaTime;
                    PlayState.fader = Mathf.InverseLerp(0, FINAL_FADEOUT_TIME, fadeTransitionCountdown);
                    if (fadeTransitionCountdown <= 0)
                        EndCredits();
                }
                else if (PlayState.creditsState == PlayState.CreditsStates.overwriteOldTime)
                {
                    statsText.position = new Vector2(0, Mathf.Lerp(statsText.position.y, 6.5f, OVERWRITE_LERP_VALUE * Time.deltaTime));
                    overwriteText.position = new Vector2(0, Mathf.Lerp(overwriteText.position.y, -0.5f, OVERWRITE_LERP_VALUE * Time.deltaTime));
                    if (Control.LeftPress() || Control.RightPress())
                    {
                        confirmOverwrite = !confirmOverwrite;
                        PlayState.PlaySound("MenuBeep1");
                    }
                    overwriteText.SetText(overwriteString + "\n\n" + string.Format(confirmOverwrite ? "> {0} <   {1}  " : "  {0}   > {1} <",
                        PlayState.GetText("ending_overwriteConfirm"), PlayState.GetText("ending_overwriteCancel")));
                    if (Control.JumpPress())
                    {
                        PlayState.PlaySound("MenuBeep2");
                        if (confirmOverwrite)
                            PlayState.SetTime(oldTime, completionTime);
                        PlayState.creditsState = PlayState.CreditsStates.fadeOut;
                        PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 255, FINAL_FADEOUT_TIME);
                        fadeTransitionCountdown = FINAL_FADEOUT_TIME;
                    }
                }
                else if (modeTimer >= 1.5f && (Control.Pause() || Control.JumpHold() || Control.ShootHold() || Control.StrafeHold() ||
                    Control.CheckKey(Control.Keyboard.Return) || Control.CheckButton(Control.Controller.Pause)))
                {
                    if (oldTime != PlayState.TimeIndeces.none)
                    {
                        modeTimer = STATS_FADE_DELAY + 1f;
                        PlayState.creditsState = PlayState.CreditsStates.overwriteOldTime;
                    }
                    else
                    {
                        PlayState.creditsState = PlayState.CreditsStates.fadeOut;
                        PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 255, FINAL_FADEOUT_TIME);
                        fadeTransitionCountdown = FINAL_FADEOUT_TIME;
                    }
                }
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
        if (isSun)
            PlayState.QueueAchievementPopup(AchievementPanel.Achievements.SunSnail);

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
        updateAnimOnRise = moonData[2] == 1;
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
                if (i == 2)
                    newObj.anim.Add("EndingMoonSnail_becomeSun");
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
            newObj.anim.Add("EndingMoonSnail_becomeSun");
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
        PlayState.fader = 1;
        PlayState.globalFunctions.UpdateMusic(-1, -1, 4);
        PlayState.ResetAllParticles();
        PlayState.creditsState = PlayState.CreditsStates.moonScene;
        PlayState.cam.transform.position = PlayState.moonCutsceneRoom.transform.position;
        PlayState.ToggleHUD(false);
        PlayState.moonCutsceneRoom.RemoteActivateRoom(true);
        PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 255, 0, 0, -75);
        startedFade = false;
        for (int i = 0; i < moonSprites.Count; i++)
            moonSprites[i].sprite.enabled = true;
        moonState = 0;
        sunOriginY = moonSprites[0].obj.transform.position.y;
        sunRiseTimer = 0;
        sunFloatTimer = 0;
        fadingIntoSun = false;
        sunSpawned = false;
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
        PlayState.ResetAllParticles();
        PlayState.creditsState = PlayState.CreditsStates.credits;
        PlayState.moonCutsceneRoom.DespawnEverything();
        for (int i = moonSprites.Count - 1; i >= 0; i--)
            Destroy(moonSprites[i].obj);
        moonSprites.Clear();
        Destroy(zzz.obj);
        PlayState.creditsRoom.RemoteActivateRoom(true);
        PlayState.cam.transform.position = PlayState.creditsRoom.transform.position;
        cutsceneText.SetText("");
        background.sprite.enabled = true;
        background.sprite.color = new Color(1, 1, 1, 1);
        background.anim.Play("EndingBackground_credits");
        PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 0, 1.4f);
        PlayState.globalFunctions.UpdateMusic(1 - PlayState.musicLibrary.areaThemeOffset, 0, 1);
        creditsGenerateY = 0;
        creditsDone = false;
        creditsDoneTimer = CREDITS_DONE_TIMER;
        CreateCredits();
        creditsParent.transform.localPosition = new Vector2(0, -8);
    }

    public void DisplayFinalTime()
    {
        modeTimer = 0;
        PlayState.creditsState = PlayState.CreditsStates.time;
        PlayState.ResetAllParticles();
        PlayState.creditsRoom.DespawnEverything();
        PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 0, 1.4f);

        GameObject imgBgObj = new("Ending Background");
        imgBgObj.transform.parent = transform;
        imageBg = new CreditsObj
        {
            obj = imgBgObj,
            sprite = imgBgObj.AddComponent<SpriteRenderer>(),
            anim = imgBgObj.AddComponent<AnimationModule>()
        };
        imageBg.sprite.sortingOrder = -5;
        imageBg.anim.Add("Ending_background");
        imageBg.anim.Play("Ending_background");
        GameObject imgObj = new("Ending Image");
        imgObj.transform.parent = transform;
        endImage = new CreditsObj
        {
            obj = imgObj,
            sprite = imgObj.AddComponent<SpriteRenderer>(),
            anim = imgObj.AddComponent<AnimationModule>()
        };
        endImage.sprite.sortingOrder = -4;
        string imageAnimName = "Ending_";
        if (PlayState.currentProfile.difficulty == 2)
            imageAnimName += "insane";
        else if (completionTime[0] == 0 && completionTime[1] < 30)
            imageAnimName += "sub30";
        else if (PlayState.currentProfile.percentage == 100)
            imageAnimName += "100";
        else
            imageAnimName += "normal";
        endImage.anim.Add(imageAnimName);
        endImage.anim.Play(imageAnimName);

        statsText.SetText(string.Format(PlayState.GetText("ending_stats"), PlayState.GetText("char_" + PlayState.currentProfile.character.ToLower()),
            PlayState.GetText("difficulty_" + PlayState.currentProfile.difficulty switch { 1 => "normal", 2 => "insane", _ => "easy" }),
            PlayState.currentProfile.percentage, PlayState.GetTimeString(completionTime)));
        statsText.position = new Vector2(0, 2.5f);
        if (oldTime != PlayState.TimeIndeces.none)
            overwriteString = string.Format(PlayState.GetText("ending_promptOverwrite"), PlayState.GetTimeString(oldTime), PlayState.GetTimeVersion(oldTime));
        overwriteText.position = new Vector2(0, -7.5f);
        overwriteText.SetColor(new Color(1, 1, 1, 1));
        confirmOverwrite = false;
    }

    public void EndCredits()
    {
        modeTimer = 0;
        PlayState.creditsState = PlayState.CreditsStates.none;
        PlayState.ResetAllParticles();
        PlayState.LastRoom().ResetEffects();
        PlayState.globalFunctions.UpdateMusic(-1, -1, 4);
        PlayState.fader = 1f;
        PlayState.ScreenFlash("Custom Fade", 0, 0, 0, 0, 0.34f);
        statsText.SetColor(new Color(1, 1, 1, 0));
        overwriteText.SetColor(new Color(1, 1, 1, 0));
        PlayState.gameState = PlayState.GameState.game;
        Destroy(background.obj);
        Destroy(endImage.obj);
        Destroy(imageBg.obj);
        PlayState.ToggleHUD(true);
        if (PlayState.currentArea == 6)
            PlayState.globalFunctions.UpdateMusic(PlayState.currentArea, PlayState.currentSubzone, 1);
        else
            PlayState.globalFunctions.DelayStartAreaTheme(PlayState.currentArea, PlayState.currentSubzone, 4f);
    }

    private void CreateCredits()
    {
        AddCreditsEntry("credits_header");
        creditsGenerateY -= 2f;
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.BlueSpikey, CreditsEntity.Entities.OrangeSpikey },
            new string[] { "credits_entity_spikey" }, 3.75f);
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.GreenBabyfish, CreditsEntity.Entities.PinkBabyfish },
            new string[] { "credits_entity_babyfish" }, 3.125f);
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.BlackFloatspike, CreditsEntity.Entities.BlueFloatspike },
            new string[] { "credits_entity_floatspike" }, 1.625f);
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Blob, CreditsEntity.Entities.Blub, CreditsEntity.Entities.Angelblob, CreditsEntity.Entities.Devilblob },
            new string[] { "credits_entity_blob1", "credits_entity_blob2", "credits_entity_blob4", "credits_entity_blob3" }, 1.25f);
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.BlueChirpy, CreditsEntity.Entities.AquaChirpy },
            new string[] { "credits_entity_chirpy" }, 1.75f);
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.BattyBat },
            new string[] { "credits_entity_bat" });
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Fireball, CreditsEntity.Entities.Iceball },
            new string[] { "credits_entity_fireball", "credits_entity_iceball" }, 1.5f);
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Snelk },
            new string[] { "credits_entity_snelk" });
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.GrayKitty, CreditsEntity.Entities.OrangeKitty },
            new string[] { "credits_entity_kitty" }, 2.75f);
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Dandelion },
            new string[] { "credits_entity_dandelion"});
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Canon, CreditsEntity.Entities.NonCanon },
            new string[] { "credits_entity_cannon1", "credits_entity_cannon2" }, 3.75f);
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.GreenSnakey, CreditsEntity.Entities.BlueSnakey },
            new string[] { "credits_entity_snakey" }, 2.5f);
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.SkyViper },
            new string[] { "credits_entity_skyviper" });
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Spider },
            new string[] { "credits_entity_spider1" });
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.SpiderMama },
            new string[] { "credits_entity_spider2" });
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.GreenTurtle },
            new string[] { "credits_entity_turtle1" });
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.CherryTurtle },
            new string[] { "credits_entity_turtle2" });
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Jellyfish },
            new string[] { "credits_entity_jellyfish" });
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Syngnathida },
            new string[] { "credits_entity_syngnathida" });
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Tallfish, CreditsEntity.Entities.AngryTallfish },
            new string[] { "credits_entity_tallfish1", "credits_entity_tallfish2" }, 3.25f);
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Walleye },
            new string[] { "credits_entity_walleye" });
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Pincer, CreditsEntity.Entities.SkyPincer },
            new string[] { "credits_entity_pincerFloor", "credits_entity_pincerCeiling" }, 2.125f);
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Spinnygear },
            new string[] { "credits_entity_spinnygear" });
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.FederationDrone },
            new string[] { "credits_entity_drone" });
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.BalloonBuster },
            new string[] { "credits_entity_balloon" });
        creditsGenerateY -= 3.75f;
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Shellbreaker },
            new string[] { "boss_shellbreaker" });
        creditsGenerateY -= 3.75f;
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Stompy },
            new string[] { "boss_stompy" });
        creditsGenerateY -= 1.875f;
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.SpaceBox },
            new string[] { "boss_spaceBox" });
        creditsGenerateY -= 1.875f;
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.MoonSnail },
            new string[] { "boss_moonSnail" });
        creditsGenerateY -= 1.875f;
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.GigaSnail },
            new string[] { "boss_gigaSnail" });
        creditsGenerateY -= 1.875f;

        switch (PlayState.currentProfile.character)
        {
            default:
            case "Snaily":
                AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Snaily },
                    new string[] { "char_full_snaily" });
                break;
            case "Sluggy":
                AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Sluggy },
                    new string[] { "char_full_sluggy" });
                break;
            case "Upside":
                AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Upside },
                    new string[] { "char_full_upside" });
                break;
            case "Leggy":
                AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Leggy },
                    new string[] { "char_full_leggy" });
                break;
            case "Blobby":
                AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Blobby },
                    new string[] { "char_full_blobby" });
                break;
            case "Leechy":
                AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Leechy },
                    new string[] { "char_full_leechy" });
                break;
        }
        creditsGenerateY -= 1.875f;

        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.NewStarshipSmell },
            new string[] { "credits_entity_tarsh" });
        creditsGenerateY -= 0.625f;
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Xdanond },
            new string[] { "credits_entity_xdanond" });
        creditsGenerateY -= 0.625f;
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.AdamAtomic },
            new string[] { "credits_entity_adamatomic" });
        creditsGenerateY -= 0.625f;
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Auriplane },
            new string[] { "credits_entity_auriplane" });
        creditsGenerateY -= 0.625f;
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Epsilon },
            new string[] { "credits_entity_epsilon" });
        creditsGenerateY -= 0.625f;
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Clarence },
            new string[] { "credits_entity_clarence" });
        creditsGenerateY -= 0.625f;
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Nat },
            new string[] { "credits_entity_nat" });
        creditsGenerateY -= 0.625f;
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Broomie },
            new string[] { "credits_entity_broomie" });
        creditsGenerateY -= 0.625f;
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Zettex },
            new string[] { "credits_entity_zettex" });
        creditsGenerateY -= 0.625f;
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Minervo },
            new string[] { "credits_entity_minervo" });
        creditsGenerateY -= 0.625f;
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.GoldGuy },
            new string[] { "credits_entity_goldguy" });
        creditsGenerateY -= 0.625f;
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Xander },
            new string[] { "credits_entity_xander" });
        creditsGenerateY -= 0.625f;
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.Discord },
            new string[] { "credits_entity_discord" });
        if (Random.Range(0f, 1f) >= 0.9f)
        {
            creditsGenerateY -= 0.625f;
            AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.ForTheFunny },
                new string[] { "credits_entity_forthefunny" });
        }
        creditsGenerateY -= 4.375f;
        AddCreditsEntry("credits_entity_you");
        creditsGenerateY -= 2.5f;
        AddCreditsEntry(new CreditsEntity.Entities[] { CreditsEntity.Entities.TheEnd },
            new string[] { });
    }

    private void AddCreditsEntry(string textID)
    {
        AddCreditsEntry(new CreditsEntity.Entities[] { }, new string[] { textID });
    }
    private void AddCreditsEntry(CreditsEntity.Entities[] entityIDs, string[] textIDs, float horizBuffer = 0)
    {
        List<CreditsEntity.Entities> listedEntities = new();
        List<string> filteredText = new();
        if (entityIDs.Length > 0)
        {
            for (int i = 0; i < entityIDs.Length; i++)
            {
                bool logEntity = false;
                if (entryEnumIDs.Contains((int)entityIDs[i]))
                {
                    if (presentEntities[entryEnumIDs.IndexOf((int)entityIDs[i])].isPresent)
                        logEntity = true;
                }
                else if (entryExceptions.Contains((int)entityIDs[i]) && listedEntities.Count > 0)
                    logEntity = true;
                else if (!entryExceptions.Contains((int)entityIDs[i]))
                    logEntity = true;
                if (logEntity)
                {
                    listedEntities.Add(entityIDs[i]);
                    if (textIDs.Length > 1)
                        filteredText.Add(textIDs[i]);
                    else if (textIDs.Length == 1 && filteredText.Count == 0)
                        filteredText.Add(textIDs[0]);
                }
            }
            if (listedEntities.Count > 0)
            {
                float spawnX = (listedEntities.Count - 1) * horizBuffer;
                spawnX = -spawnX * 0.5f;
                float largestHeight = 0;
                for (int i = 0; i < listedEntities.Count; i++)
                {
                    CreditsEntity newEntity = Instantiate(creditsEntity, creditsParent).GetComponent<CreditsEntity>();
                    float thisHeight = newEntity.Spawn(listedEntities[i], spawnX, creditsGenerateY);
                    if (thisHeight > largestHeight)
                        largestHeight = thisHeight;
                    spawnX += horizBuffer;
                }
                creditsGenerateY -= largestHeight + PlayState.FRAC_8;
            }
        }
        if (textIDs.Length > 0)
        {
            if (entityIDs.Length == 0)
                creditsGenerateY -= AddText(textIDs);
            if (entityIDs.Length > 0 && listedEntities.Count > 0)
                creditsGenerateY -= AddText(filteredText.ToArray());
        }
        else
            creditsGenerateY -= 2f;
    }

    private float AddText(string[] textIDs)
    {
        TextObject newText = Instantiate(textObj, creditsParent).GetComponent<TextObject>();
        newText.position = new Vector2(0, creditsGenerateY);
        newText.SetAlignment("center");
        newText.CreateOutline();
        for (int i = 0; i < textIDs.Length; i++)
            textIDs[i] = PlayState.GetText(textIDs[i]);
        string finalText = textIDs.Length switch
        {
            2 => string.Format(PlayState.GetText("credits_generic_join2"), textIDs[0], textIDs[1]),
            3 => string.Format(PlayState.GetText("credits_generic_join3"), textIDs[0], textIDs[1], textIDs[2]),
            4 => string.Format(PlayState.GetText("credits_generic_join4"), textIDs[0], textIDs[1], textIDs[2], textIDs[3]),
            _ => PlayState.GetText(textIDs[0])
        };
        newText.SetText(finalText);
        int lineCount = 1;
        for (int i = 0; i < finalText.Length; i++)
            if (finalText[i] == '\n')
                lineCount++;
        return TEXT_VERTICAL_SIZE * lineCount + 2f;
    }
}
