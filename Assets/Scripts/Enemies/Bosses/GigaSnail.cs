using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GigaSnail : Boss
{
    private const float GRAV_JUMP_TIMEOUT = 0.2f;
    private const float START_ATTACK_TIME = 0.45f;
    private const float JUMP_POWER = 22.5f;
    private const float JUMP_TIMEOUT = 0.8f;
    private const float WALK_SPEED = 12.5f;
    private const float ZZZ_TIMEOUT = 0.3f;
    private const float HEAL_TIMEOUT = 0.1f;
    private const float HEAL_DELAY = 0.8f;
    private const float GRAVITY = 0.9375f;
    private const float INTEGRATE_SPEED_STOMP = 0.7f;
    private const float INTEGRATE_SPEED_STRAFE = 1.7f;
    private const int CAST_COUNT = 6;
    private const float SMASH_REFLECT_BUFFER = 0.5f;
    private const float TRAIL_TIMEOUT = 0.0625f;

    private float STRAFE_TIMEOUT = 0.03f;
    private float STRAFE_SPEED = 25f;
    private float SMASH_SPEED = 25f;
    private float STOMP_TIMEOUT = 0.25f;
    private float WAVE_TIMEOUT = 0.9f;
    private float WAVE_SPEED = 1.875f;
    private int ZZZ_COUNT = 3;

    private readonly float[] decisionTable = new float[]
    {
        0.1640168826f, 0.3892556902f, 0.0336081053f, 0.2246864975f, 0.5434009453f, 0.4227320437f, 0.1017472328f, 0.2041907897f, 0.9950191347f, 0.3634705228f,
        0.0779175897f, 0.384822732f, 0.3284047846f, 0.0951552057f, 0.1941055446f, 0.496359046f, 0.2428007567f, 0.8280672868f, 0.852732986f, 0.6928913176f,
        0.2023843678f, 0.7280045905f, 0.4311591744f, 0.796788024f, 0.41191487f, 0.7108575032f, 0.1134556829f, 0.6883870615f, 0.8149317527f, 0.8392490375f,
        0.3647662453f, 0.3487805783f, 0.7900575239f, 0.1670561498f, 0.9810836953f, 0.0097847681f, 0.2244645569f, 0.0842442402f, 0.3263779227f, 0.1481701068f,
        0.6538572663f, 0.2544128409f, 0.1991950422f, 0.541057099f, 0.574700257f, 0.5926224371f, 0.310134571f, 0.6104650203f, 0.3545506087f, 0.2313309166f,
        0.3070387696f, 0.0790505658f, 0.9804949607f, 0.7704714904f, 0.7152660213f, 0.8215058975f, 0.9426850446f, 0.7483973576f, 0.7602092802f, 0.881605898f,
        0.5136580468f, 0.0190696615f, 0.28759162f, 0.1565554394f, 0.3664312259f, 0.2586407176f, 0.3185483313f, 0.9837348993f, 0.3330417452f, 0.2801789805f,
        0.3288621592f, 0.0230039287f, 0.303914672f, 0.7212895333f, 0.6296904139f, 0.8659332532f, 0.1715852607f, 0.3900271956f, 0.2824020982f, 0.1624092775f,
        0.7599701669f, 0.6952292831f, 0.2161165745f, 0.9005386635f, 0.3707154895f, 0.6392742953f, 0.452149187f, 0.5595775233f, 0.686286675f, 0.7266258821f,
        0.6904605229f, 0.6808205255f, 0.6856147591f, 0.299675182f, 0.8012191872f, 0.804475971f, 0.1926201715f, 0.8868517061f, 0.8347136807f, 0.1512707539f
    };

    private enum BossMode
    {
        Intro,
        Stomp,
        Strafe,
        Smash,
        Sleep
    };
    private BossMode mode = BossMode.Intro;

    private PlayState.EDirsCardinal gravity;

    private PlayState.EDirsCardinal lastHitDir = PlayState.EDirsCardinal.None;
    private int lastStomp;
    private int strafeCount;
    private Vector2 velocity;
    private Vector2 smashAccel = Vector2.zero;
    private int decisionTableIndex;
    private float bossSpeed = 1;
    private int attackPhase;
    private float elapsed;
    private float modeElapsed;
    private bool modeInitialized;
    private float modeTimeout;
    private Vector2 moveTarget = Vector2.zero;
    private float strafeTheta;
    private float strafeThetaVel;
    private float strafeThetaAccel = 0;
    private bool waitingToJump;
    private float strafeTimeout;
    private float stompTimeout;
    private float waveTimeout;
    private bool stomped;
    private bool aimed;
    private bool grounded;
    private float gravJumpTimeout;
    private float jumpTimeout;
    private float zzzTimeout;
    private int zzzNum;
    private string lastAnimState = "shell";
    private bool facingLeft = false;
    private bool flipHoriz = false;
    private bool facingDown = false;
    private bool flipVert = false;
    private PlayState.EDirsCardinal fallDir = PlayState.EDirsCardinal.None;
    private PlayState.EDirsCardinal lastFallDir = PlayState.EDirsCardinal.None;
    private float trailTimeout;
    private bool delayedDespawn = false;
    private float despawnCountdown = 5f;

    private int[] animData;
    /*\
     *   ANIMATION DATA
     *  0 - Allow horizontal sprite flip
     *  1 - Allow vertical sprite flip
     *  2 - Fade sprite in on spawn
     *  3 - Update animation on phase change
     *  4 - Update animation on jump/land during Stomp phase
     *  5 - Update animation on gravity jump during Stomp phase
     *  6 - Update animation on turnaround during Stomp phase
     *  7 - Frames into Stomp intro to flip sprite horizontally
     *  8 - Frames into Stomp intro to flip sprite vertically
     *  9 - Frames into Stomp outro to flip sprite horizontally
     * 10 - Frames into Stomp outro to flip sprite vertically
     * 11 - Frames into grounded Stomp turnaround to flip sprite
     * 12 - Frames into airborne Stomp turnaround to flip sprite
     * 13 - Frames into horizontal Stomp gravity jump to flip sprite
     * 14 - Frames into vertical Stomp gravity jump to flip sprite
     * 15 - Frames into Sleep intro to flip sprite horizontally
     * 16 - Frames into Sleep intro to flip sprite vertically
     * 17 - Update animation on collision during Smash phase
     * 18 - Update animation on landing during Sleep phase
     * 19 - Maximum sprite ID for phase 1; current sprite ID will be modulo'd by this to determine trail sprite ID
    \*/
    private enum AnimData
    {
        AllowHorizontalSpriteFlip,
        AllowVerticalSpriteFlip,
        FadeInOnSpawn,
        UpdateAnimOnPhaseChange,
        UpdateOnJumpLand,
        UpdateOnGravJump,
        UpdateOnTurnaround,
        FramesIntoStompIntroHorizontal,
        FramesIntoStompIntroVertical,
        FramesIntoStompOutroHorizontal,
        FramesIntoStompOutroVertical,
        FramesIntoGroundedTurnaround,
        FramesIntoAirborneTurnaround,
        FramesIntoHorizontalGravJump,
        FramesIntoVerticalGravJump,
        FramesIntoSleepIntroHorizontal,
        FramesIntoSleepIntroVertical,
        UpdateOnSmashCollision,
        UpdateOnSleepLand
    };

    private List<PlayState.TargetPoint> stompPoints = new();

    private GameObject zzzObj;

    private List<Particle> bgStars = new();

    private struct BGObj
    {
        public GameObject obj;
        public SpriteRenderer sprite;
        public AnimationModule anim;
    }
    private BGObj bgA;
    private BGObj bgB;
    private int[] bgAnimData;
    /*\
     *   ANIMATION DATA
     * 0 - fade in intro background
     * 1 - fade out outro background
     * 2 - crossfade attack backgrounds
     * 3 - display stars over the background
    \*/
    private string lastBGState = "none";

    private BoxCollider2D box;
    private Vector2 boxSize;
    private Vector2 halfBox;

    public void Awake()
    {
        SpawnBoss(34000, 6, 0, true, 3, false);

        PlayState.globalFunctions.RefillPlayerHealth(HEAL_TIMEOUT, HEAL_DELAY,
            PlayState.currentProfile.difficulty switch { 2 => 1, 1 => 2, _ => 4}, true, true);

        col.TryGetComponent(out box);
        boxSize = box.size;
        halfBox = boxSize * 0.5f;
        box.enabled = false;

        decisionTableIndex = Mathf.Abs(Mathf.FloorToInt(PlayState.WORLD_ORIGIN.x - (PlayState.WORLD_SIZE.x * PlayState.ROOM_SIZE.x * 0.5f)
            - PlayState.player.transform.position.x)) % decisionTable.Length;

        if (PlayState.currentProfile.difficulty == 2)
            bossSpeed += 0.2f;

        animData = PlayState.GetAnim("Boss_gigaSnail_data").frames;
        if (animData[(int)AnimData.FadeInOnSpawn] == 1)
            sprite.color = new Color32(255, 255, 255, 0);
        for (int i = 1; i <= 2; i++)
        {
            anim.Add("Boss_gigaSnail_idle" + i.ToString());
            anim.Add("Boss_gigaSnail_strafe" + i.ToString());
            for (int j = 0; j < PlayState.DIRS_COMPASS.Length; j++)
            {
                anim.Add("Boss_gigaSnail_smash" + i.ToString() + "_" + PlayState.DIRS_COMPASS[j]);
                anim.Add("Boss_gigaSnail_smash" + i.ToString() + "_collide_" + PlayState.DIRS_COMPASS[j]);
            }
            anim.Add("Boss_gigaSnail_sleep" + i.ToString() + "_shelled");
            anim.Add("Boss_gigaSnail_sleep" + i.ToString() + "_land");
            for (int j = 0; j < PlayState.DIRS_SURFACE.Length; j++)
            {
                for (int k = 0; k < 2; k++)
                {
                    string dirMod = k == 0 ? "L" : "R";
                    if (j == 1 || j == 2)
                        dirMod = k == 0 ? "D" : "U";
                    string thisDir = PlayState.DIRS_SURFACE[j];

                    anim.Add("Boss_gigaSnail_sleep" + i.ToString() + "_unshelled_" + thisDir + "_" + dirMod);
                    anim.Add("Boss_gigaSnail_stomp" + i.ToString() + "_unshell_" + thisDir + "_" + dirMod);
                    anim.Add("Boss_gigaSnail_stomp" + i.ToString() + "_idle_" + thisDir + "_" + dirMod);
                    anim.Add("Boss_gigaSnail_stomp" + i.ToString() + "_turn_" + thisDir + "_" + dirMod);
                    anim.Add("Boss_gigaSnail_stomp" + i.ToString() + "_jump_" + thisDir + "_" + dirMod);
                    anim.Add("Boss_gigaSnail_stomp" + i.ToString() + "_turnAir_" + thisDir + "_" + dirMod);
                    anim.Add("Boss_gigaSnail_stomp" + i.ToString() + "_flip_" + thisDir + "_" + dirMod);
                    anim.Add("Boss_gigaSnail_stomp" + i.ToString() + "_land_" + thisDir + "_" + dirMod);
                    anim.Add("Boss_gigaSnail_stomp" + i.ToString() + "_shell_" + thisDir + "_" + dirMod);
                    if (i == 1)
                    {
                        anim.Add("Boss_gigaSnail_stomp" + i.ToString() + "_turnRed_" + thisDir + "_" + dirMod);
                        anim.Add("Boss_gigaSnail_stomp" + i.ToString() + "_turnRedAir_" + thisDir + "_" + dirMod);
                    }
                }
            }
        }
        anim.Add("Boss_gigaSnail_intro");
        anim.Play("Boss_gigaSnail_intro");

        bgAnimData = PlayState.GetAnim("GigaBackground_data").frames;
        for (int i = 1; i <= 2; i++)
        {
            BGObj newBG = new() { obj = new GameObject("Giga Snail Background Layer " + i.ToString()) };
            newBG.obj.transform.parent = PlayState.cam.transform;
            newBG.obj.transform.localPosition = Vector2.zero;

            newBG.sprite = newBG.obj.AddComponent<SpriteRenderer>();
            newBG.sprite.sortingOrder = -99 + i;

            newBG.anim = newBG.obj.AddComponent<AnimationModule>();
            newBG.anim.Add("GigaBackground_fadeIn");
            newBG.anim.Add("GigaBackground_fadeOut");
            for (int j = 0; j < System.Enum.GetNames(typeof(BossMode)).Length; j++)
            {
                string parsedStateName = ((BossMode)j).ToString().ToLower();
                newBG.anim.Add("GigaBackground_" + parsedStateName + "_hold1");
                newBG.anim.Add("GigaBackground_" + parsedStateName + "_hold2");
                newBG.anim.Add("GigaBackground_" + parsedStateName + "_fadeOut1");
                newBG.anim.Add("GigaBackground_" + parsedStateName + "_fadeOut2");
            }

            if (i == 1)
                bgA = newBG;
            else
                bgB = newBG;

            PlayState.gigaBGLayers.Add(newBG.obj);
        }
        UpdateBackground("fadeIn");

        zzzObj = Resources.Load<GameObject>("Objects/Enemies/Bosses/Giga Zzz");

        CollectTargetPoints();
    }

    private void UpdateBackground(string newState)
    {
        if (bgAnimData[3] == 1 && bgStars.Count == 0)
        {
            for (int i = 0; i < 16; i++)
            {
                float angle = Random.Range(0, PlayState.TAU);
                Vector2 startingPos = new(Mathf.Clamp(Mathf.Cos(angle) * 30f, -13f, 13f), Mathf.Clamp(Mathf.Sin(angle) * 30f, -8f, 8f));
                bgStars.Add(PlayState.RequestParticle((Vector2)PlayState.cam.transform.position + startingPos, "gigaStar"));
            }
        }

        switch (newState)
        {
            case "fadeIn":
                SetBackgroundState(false, false);
                SetBackgroundState(true, false);
                bgA.anim.Play("GigaBackground_fadeIn");
                if (bgAnimData[0] == 1)
                    StartCoroutine(FadeBackground(false, 4f, true));
                else
                    SetBackgroundState(false, true);
                break;
            case "intro":
                bgB.anim.Play("GigaBackground_intro_hold1");
                if (bgAnimData[2] == 1)
                    StartCoroutine(FadeBackground(true, 2f, true));
                else
                    SetBackgroundState(true, true);
                break;
            default:
                SetBackgroundState(true, true);
                if (bgAnimData[2] == 1)
                    StartCoroutine(FadeBackground(true, 0.3333f, false));
                bgB.anim.Play("GigaBackground_" + lastBGState + "_fadeOut" + (attackPhase + 1).ToString());
                bgA.anim.Play("GigaBackground_" + newState + "_hold" + (attackPhase + 1).ToString());
                break;
            case "fadeOut":
                SetBackgroundState(true, false);
                if (bgAnimData[1] == 1)
                    StartCoroutine(FadeBackground(false, 4f, false));
                bgA.anim.Play("GigaBackground_fadeOut");
                break;
        }
        lastBGState = newState;

        if (PlayState.generalData.particleState != 4)
            foreach (Particle particle in bgStars)
                particle.vars[0] = (int)mode;
    }

    private void SetBackgroundState(bool layer, bool mode)
    {
        byte alpha = mode ? (byte)255 : (byte)0;
        if (layer)
            bgB.sprite.color = new Color32(255, 255, 255, alpha);
        else
            bgA.sprite.color = new Color32(255, 255, 255, alpha);
    }

    private IEnumerator FadeBackground(bool layer, float fadeTime, bool fadeMode)
    {
        Color32 startingColor = (layer ? bgB : bgA).sprite.color;
        Color32 targetColor = fadeMode ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
        float fadeElapsed = 0;
        while (fadeElapsed < fadeTime)
        {
            if (PlayState.gameState == PlayState.GameState.game)
            {
                fadeElapsed += Time.deltaTime;
                if (layer)
                    bgB.sprite.color = Color32.Lerp(startingColor, targetColor, fadeElapsed / fadeTime);
                else
                    bgA.sprite.color = Color32.Lerp(startingColor, targetColor, fadeElapsed / fadeTime);
            }
            yield return new WaitForEndOfFrame();
        }
        if (layer)
            bgB.sprite.color = targetColor;
        else
            bgA.sprite.color = targetColor;
    }

    private float GetDecision()
    {
        decisionTableIndex = ++decisionTableIndex % decisionTable.Length;
        return decisionTable[decisionTableIndex];
    }

    private void Stomp()
    {
        if (stomped)
            return;

        velocity = Vector2.zero;

        if (stompTimeout <= 0)
        {
            PlayState.globalFunctions.ScreenShake(new List<float> { 0.5f, 0 }, new List<float> { 0.5f } );
            PlayState.PlaySound("Stomp");
            stompTimeout = STOMP_TIMEOUT;
        }
        stomped = true;
        grounded = true;
        gravJumpTimeout = 999999f;
    }

    private void ShootWave()
    {
        if (waveTimeout > 0)
            return;

        waveTimeout = WAVE_TIMEOUT;
        Vector2 direction;
        if (gravity == PlayState.EDirsCardinal.Up || gravity == PlayState.EDirsCardinal.Down)
            direction = PlayState.player.transform.position.x < transform.position.x ? Vector2.left : Vector2.right;
        else
            direction = PlayState.player.transform.position.y < transform.position.y ? Vector2.down : Vector2.up;

        PlayState.ShootEnemyBullet(transform.position, EnemyBullet.BulletType.gigaWave, new float[] { WAVE_SPEED * Time.fixedDeltaTime, direction.x, direction.y });
    }

    private void SetMode(BossMode newMode)
    {
        if (newMode == BossMode.Stomp)
            lastStomp = 0;
        else
            lastStomp++;
        if (lastStomp >= 4)
        {
            newMode = BossMode.Stomp;
            lastStomp = 0;
        }

        mode = newMode;
        modeInitialized = false;
        stomped = false;
        velocity = Vector2.zero;
        modeElapsed = 0;
        waitingToJump = false;
        lastFallDir = fallDir;
        fallDir = PlayState.EDirsCardinal.None;
        lastAnimState = "shell";
    }

    private void PickStompTarget()
    {
        if (stompPoints.Count == 0)
            moveTarget = (Vector2)transform.parent.position + origin;
        else
        {
            int pointID = Mathf.FloorToInt(GetDecision() * stompPoints.Count);
            moveTarget = stompPoints[pointID].pos;
        }
    }

    private void UpdateAIIntro()
    {
        if (elapsed > 2f && elapsed < 3f && animData[(int)AnimData.FadeInOnSpawn] == 1)
            sprite.color = new Color32(255, 255, 255, (byte)((elapsed - 2) * 255));
        else if (elapsed > 3 && introTimer == 0)
        {
            sprite.color = new Color32(255, 255, 255, 255);
            StartCoroutine(RunIntro(true, true, true));
        }
        if (introTimer >= 1f && lastBGState == "fadeIn")
            UpdateBackground("intro");
        if (introDone)
        {
            PlayState.ToggleGigaTiles(false);
            SetMode(BossMode.Stomp);
            box.enabled = true;
        }
    }

    private void UpdateAISleep()
    {
        if (!modeInitialized)
        {
            if (Mathf.Abs(transform.position.x - PlayState.player.transform.position.x) < 2.5f &&
                transform.position.y > PlayState.player.transform.position.y)
            {
                SetMode(BossMode.Stomp);
                return;
            }
            modeInitialized = true;
            modeTimeout = 6.2f;
            PlayAnim(AnimTypes.sleepStart);
            zzzNum = 0;
            zzzTimeout = ZZZ_TIMEOUT;
            if (PlayState.currentProfile.difficulty == 2)
            {
                ZZZ_COUNT = 5;
                modeTimeout *= 1.23f;
            }
            fallDir = PlayState.EDirsCardinal.Down;
            PlayState.PlaySound("Shell");
            UpdateBackground("sleep");
            grounded = false;
        }
        if (stomped)
        {
            zzzTimeout -= Time.fixedDeltaTime * bossSpeed;
            if (zzzTimeout <= 0 && zzzNum < ZZZ_COUNT)
            {
                GameObject newZzz = Instantiate(zzzObj, transform.position, Quaternion.identity);
                newZzz.GetComponent<GigaZzz>().targetPoint = new Vector2(transform.position.x + halfBox.x + 1.5f * zzzNum, transform.position.y);
                zzzTimeout = ZZZ_TIMEOUT;
                zzzNum++;
            }
        }
        if (modeTimeout <= 0)
        {
            if (GetDecision() > 0.5f)
                SetMode(BossMode.Stomp);
            else
                SetMode(BossMode.Strafe);
            PlayAnim(AnimTypes.idle);
        }
    }

    private void UpdateAIStomp()
    {
        if (!modeInitialized)
        {
            modeInitialized = true;
            modeTimeout = 6f;
            PickStompTarget();
            UpdateBackground("stomp");
        }
        if (lastAnimState == "shell")
        {
            if (Vector2.Distance(transform.position, moveTarget) < 0.625f)
            {
                if (GetDecision() > 0.5f)
                    fallDir = PlayState.EDirsCardinal.Down;
                else
                    fallDir = PlayState.EDirsCardinal.Up;
                lastFallDir = fallDir;
                flipVert = fallDir == PlayState.EDirsCardinal.Up;
                PlayAnim(AnimTypes.shellIntoStomp);
                FacePlayer();
                grounded = false;
            }
            transform.position = new Vector2(PlayState.Integrate(transform.position.x, moveTarget.x, INTEGRATE_SPEED_STOMP, Time.fixedDeltaTime * bossSpeed),
                PlayState.Integrate(transform.position.y, moveTarget.y, INTEGRATE_SPEED_STOMP, Time.fixedDeltaTime * bossSpeed));
        }
        else
        {
            FacePlayer();
            if (stomped)
            {
                if (!waitingToJump)
                {
                    waitingToJump = true;
                    jumpTimeout = JUMP_TIMEOUT;
                }
                jumpTimeout -= Time.fixedDeltaTime * bossSpeed;
                if (waitingToJump && jumpTimeout <= 0)
                {
                    waitingToJump = false;
                    stomped = false;
                    grounded = false;
                    velocity = fallDir switch
                    {
                        PlayState.EDirsCardinal.Down => new Vector2(WALK_SPEED * (facingLeft ? -1 : 1), JUMP_POWER),
                        PlayState.EDirsCardinal.Left => new Vector2(JUMP_POWER, WALK_SPEED * (facingDown ? -1 : 1)),
                        PlayState.EDirsCardinal.Right => new Vector2(-JUMP_POWER, WALK_SPEED * (facingDown ? -1 : 1)),
                        PlayState.EDirsCardinal.Up => new Vector2(WALK_SPEED * (facingLeft ? -1 : 1), -JUMP_POWER),
                        _ => Vector2.zero
                    } * Time.fixedDeltaTime;
                    gravJumpTimeout = GRAV_JUMP_TIMEOUT;
                    jumpTimeout = 99999;
                    if (animData[(int)AnimData.UpdateOnJumpLand] == 1)
                        PlayAnim(AnimTypes.jump);
                }
                else if (((fallDir == PlayState.EDirsCardinal.Left || fallDir == PlayState.EDirsCardinal.Right) ? velocity.x : velocity.y) != 0)
                {
                    gravJumpTimeout -= Time.fixedDeltaTime;
                    if (gravJumpTimeout < 0)
                    {
                        if (GetDecision() > 0.66)
                        {
                            fallDir = fallDir switch
                            {
                                PlayState.EDirsCardinal.Down => PlayState.EDirsCardinal.Up,
                                PlayState.EDirsCardinal.Left => PlayState.EDirsCardinal.Right,
                                PlayState.EDirsCardinal.Right => PlayState.EDirsCardinal.Left,
                                PlayState.EDirsCardinal.Up => PlayState.EDirsCardinal.Down,
                                _ => PlayState.EDirsCardinal.None
                            };
                        }
                        gravJumpTimeout = 999999;
                    }
                }
            }
        }
        if ((attackPhase == 0 && modeElapsed > START_ATTACK_TIME * 2.5f) || (attackPhase == 1 && modeElapsed > START_ATTACK_TIME * 3.2f))
            ShootWave();
        if (modeTimeout <= 0)
        {
            PlayAnim(AnimTypes.stompIntoShell);
            transform.position += PlayState.FRAC_32 * fallDir switch
            {
                PlayState.EDirsCardinal.Left => Vector3.left,
                PlayState.EDirsCardinal.Right => Vector3.right,
                PlayState.EDirsCardinal.Up => Vector3.up,
                _ => Vector3.down
            };

            if (attackPhase == 1 && GetDecision() > 0.7)
                SetMode(BossMode.Sleep);
            else if (GetDecision() > 0.7)
                SetMode(BossMode.Smash);
            else if (GetDecision() > 0.8)
                SetMode(BossMode.Stomp);
            else
                SetMode(BossMode.Strafe);
        }
    }

    private void UpdateAIStrafe()
    {
        if (!modeInitialized)
        {
            modeInitialized = true;
            modeTimeout = 5.2f;
            PlayAnim(AnimTypes.idle);
            moveTarget = (Vector2)transform.parent.position + origin;
            aimed = false;
            UpdateBackground("strafe");
        }
        transform.position = new Vector2(PlayState.Integrate(transform.position.x, moveTarget.x, INTEGRATE_SPEED_STRAFE, Time.fixedDeltaTime * bossSpeed),
            PlayState.Integrate(transform.position.y, moveTarget.y, INTEGRATE_SPEED_STRAFE, Time.fixedDeltaTime * bossSpeed));
        if (modeElapsed > START_ATTACK_TIME && !aimed && Vector2.Distance(transform.position, moveTarget) < 0.625f)
        {
            AimStrafe();
            strafeThetaVel = Mathf.PI * 0.125f * (GetDecision() > 0.5f ? 1 : -1);
            aimed = true;
            if (PlayState.currentProfile.difficulty == 2)
                strafeThetaVel *= 1.6f;
            if (anim.currentAnimName != "Boss_gigaSnail_strafe" + (attackPhase + 1).ToString() && animData[(int)AnimData.UpdateAnimOnPhaseChange] == 1)
                PlayAnim(AnimTypes.strafe);
        }
        strafeTheta += strafeThetaVel * Time.fixedDeltaTime * bossSpeed;
        strafeThetaVel += strafeThetaAccel * Time.fixedDeltaTime * bossSpeed;
        if (modeElapsed > START_ATTACK_TIME && aimed)
            StrafeMulti();
        if (modeTimeout <= 0)
        {
            if (attackPhase == 1 && GetDecision() > 0.74)
                SetMode(BossMode.Sleep);
            else if (GetDecision() < 0.77)
                SetMode(BossMode.Stomp);
            else
                SetMode(BossMode.Smash);
        }
    }

    private void UpdateAISmash()
    {
        if (!modeInitialized)
        {
            modeInitialized = true;
            modeTimeout = 6f;
            PickSmashDir();
            PlayAnim(AnimTypes.smash);
            UpdateBackground("smash");
        }

        bool hitHorizontal = false;
        bool hitVertical = false;
        velocity += smashAccel * Time.fixedDeltaTime;
        float disHoriz = GetDistance(smashAccel.x > 0 ? PlayState.EDirsCardinal.Right : PlayState.EDirsCardinal.Left);
        if (Mathf.Abs(velocity.x) > disHoriz && velocity.x != 0)
        {
            transform.position += (disHoriz - PlayState.FRAC_32) * (smashAccel.x > 0 ? Vector3.right : Vector3.left);
            hitHorizontal = true;
            velocity.x = 0;
        }
        float disVert = GetDistance(smashAccel.y > 0 ? PlayState.EDirsCardinal.Up : PlayState.EDirsCardinal.Down);
        if (Mathf.Abs(velocity.y) > disVert && velocity.y != 0)
        {
            transform.position += (disVert - PlayState.FRAC_32) * (smashAccel.y > 0 ? Vector3.up : Vector3.down);
            hitVertical = true;
            velocity.y = 0;
        }
        transform.position += (Vector3)velocity;
        if ((hitHorizontal || hitVertical) && modeTimeout <= 5.975f)
        {
            Stomp();
            if (animData[(int)AnimData.UpdateAnimOnPhaseChange] == 1)
                PlayAnim(AnimTypes.smashCollide);
        }

        if (stomped)
        {
            stomped = false;
            if (GetDecision() > 0.7 || attackPhase == 1)
                PickSmashDir(true);
            else
            {
                if (hitHorizontal)
                    smashAccel.x *= -1;
                if (hitVertical)
                    smashAccel.y *= -1;
            }
        }
        if (modeTimeout <= 0)
        {
            lastHitDir = PlayState.EDirsCardinal.None;
            if (GetDecision() > 0.5)
                SetMode(BossMode.Stomp);
            else
                SetMode(BossMode.Strafe);
        }
    }

    private void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (delayedDespawn)
        {
            if (PlayState.creditsState == PlayState.CreditsStates.none)
            {
                PlayState.globalFunctions.RemoveGigaBackgroundLayers();
                despawnCountdown = 3.75f;
            }
            if (despawnCountdown < 5f)
                despawnCountdown -= Time.fixedDeltaTime;
            if (despawnCountdown < 0f)
            {
                PlayState.PlayAreaSong(PlayState.currentArea, PlayState.currentSubzone);
                Destroy(gameObject);
            }
            return;
        }

        waveTimeout -= Time.fixedDeltaTime * bossSpeed;
        modeTimeout -= Time.fixedDeltaTime * bossSpeed;
        strafeTimeout -= Time.fixedDeltaTime * bossSpeed;
        stompTimeout -= Time.fixedDeltaTime * bossSpeed;
        modeElapsed += Time.fixedDeltaTime * bossSpeed;
        switch (mode)
        {
            case BossMode.Intro:
                UpdateAIIntro();
                break;
            case BossMode.Stomp:
                UpdateAIStomp();
                break;
            case BossMode.Strafe:
                UpdateAIStrafe();
                break;
            case BossMode.Smash:
                UpdateAISmash();
                break;
            case BossMode.Sleep:
                UpdateAISleep();
                break;
        }
        elapsed += Time.fixedDeltaTime;

        if (fallDir != PlayState.EDirsCardinal.None)
        {
            float floorDis = GetDistance(fallDir);
            switch (fallDir)
            {
                default:
                case PlayState.EDirsCardinal.Down:
                    if (!grounded)
                    {
                        velocity.y -= GRAVITY * Time.fixedDeltaTime;
                        if (floorDis < Mathf.Abs(velocity.y) && velocity.y < 0)
                        {
                            transform.position += (floorDis - PlayState.FRAC_32) * Vector3.down;
                            Stomp();
                            if (lastAnimState == "sleepTurn" && animData[(int)AnimData.UpdateOnSleepLand] == 1)
                            {
                                PlayAnim(AnimTypes.sleepLand);
                                facingLeft = false;
                                facingDown = false;
                            }
                            if ((lastAnimState == "stomp" || lastAnimState == "stompTurn") && animData[(int)AnimData.UpdateOnJumpLand] == 1)
                                PlayAnim(AnimTypes.land);
                        }
                        float ceilDis = GetDistance(PlayState.EDirsCardinal.Up);
                        if (ceilDis < Mathf.Abs(velocity.y) && velocity.y > 0)
                        {
                            transform.position += (ceilDis - PlayState.FRAC_32) * Vector3.up;
                            velocity.y = 0;
                        }
                        float wallDis = GetDistance(velocity.x < 0 ? PlayState.EDirsCardinal.Left : PlayState.EDirsCardinal.Right);
                        if (wallDis < Mathf.Abs(velocity.x))
                        {
                            transform.position += (wallDis - PlayState.FRAC_32) * (velocity.x < 0 ? Vector3.left : Vector3.right);
                            velocity.x *= -1;
                        }
                    }
                    break;
                case PlayState.EDirsCardinal.Left:
                    if (!grounded)
                    {
                        velocity.x -= GRAVITY * Time.fixedDeltaTime;
                        if (floorDis < Mathf.Abs(velocity.x) && velocity.x < 0)
                        {
                            transform.position += (floorDis - PlayState.FRAC_32) * Vector3.left;
                            Stomp();
                            if ((lastAnimState == "stomp" || lastAnimState == "stompTurn") && animData[(int)AnimData.UpdateOnJumpLand] == 1)
                                PlayAnim(AnimTypes.land);
                        }
                        float ceilDis = GetDistance(PlayState.EDirsCardinal.Right);
                        if (ceilDis < Mathf.Abs(velocity.x) && velocity.x > 0)
                        {
                            transform.position += (ceilDis - PlayState.FRAC_32) * Vector3.right;
                            velocity.x = 0;
                        }
                        float wallDis = GetDistance(velocity.y < 0 ? PlayState.EDirsCardinal.Down : PlayState.EDirsCardinal.Up);
                        if (wallDis < Mathf.Abs(velocity.y))
                        {
                            transform.position += (wallDis - PlayState.FRAC_32) * (velocity.y < 0 ? Vector3.down : Vector3.up);
                            velocity.y *= -1;
                        }
                    }
                    break;
                case PlayState.EDirsCardinal.Right:
                    if (!grounded)
                    {
                        velocity.x += GRAVITY * Time.fixedDeltaTime;
                        if (floorDis < Mathf.Abs(velocity.x) && velocity.x > 0)
                        {
                            transform.position += (floorDis - PlayState.FRAC_32) * Vector3.right;
                            Stomp();
                            if ((lastAnimState == "stomp" || lastAnimState == "stompTurn") && animData[(int)AnimData.UpdateOnJumpLand] == 1)
                                PlayAnim(AnimTypes.land);
                        }
                        float ceilDis = GetDistance(PlayState.EDirsCardinal.Left);
                        if (ceilDis < Mathf.Abs(velocity.x) && velocity.x < 0)
                        {
                            transform.position += (ceilDis - PlayState.FRAC_32) * Vector3.left;
                            velocity.x = 0;
                        }
                        float wallDis = GetDistance(velocity.y < 0 ? PlayState.EDirsCardinal.Down : PlayState.EDirsCardinal.Up);
                        if (wallDis < Mathf.Abs(velocity.y))
                        {
                            transform.position += (wallDis - PlayState.FRAC_32) * (velocity.y < 0 ? Vector3.down : Vector3.up);
                            velocity.y *= -1;
                        }
                    }
                    break;
                case PlayState.EDirsCardinal.Up:
                    if (!grounded)
                    {
                        velocity.y += GRAVITY * Time.fixedDeltaTime;
                        if (floorDis < Mathf.Abs(velocity.y) && velocity.y > 0)
                        {
                            transform.position += (floorDis - PlayState.FRAC_32) * Vector3.up;
                            Stomp();
                            if ((lastAnimState == "stomp" || lastAnimState == "stompTurn") && animData[(int)AnimData.UpdateOnJumpLand] == 1)
                                PlayAnim(AnimTypes.land);
                        }
                        float ceilDis = GetDistance(PlayState.EDirsCardinal.Down);
                        if (ceilDis < Mathf.Abs(velocity.y) && velocity.y < 0)
                        {
                            transform.position += (ceilDis - PlayState.FRAC_32) * Vector3.down;
                            velocity.y = 0;
                        }
                        float wallDis = GetDistance(velocity.x < 0 ? PlayState.EDirsCardinal.Left : PlayState.EDirsCardinal.Right);
                        if (wallDis < Mathf.Abs(velocity.x))
                        {
                            transform.position += (wallDis - PlayState.FRAC_32) * (velocity.x < 0 ? Vector3.left : Vector3.right);
                            velocity.x *= -1;
                        }
                    }
                    break;
            }
            transform.position += (Vector3)velocity;
        }

        int thisFrame = anim.GetCurrentFrame();
        switch (lastAnimState)
        {
            case "sleepTurn":
                if (thisFrame < animData[(int)AnimData.FramesIntoSleepIntroHorizontal] && sprite.flipX == true)
                    sprite.flipX = true;
                else
                    sprite.flipX = false;
                if (thisFrame < animData[(int)AnimData.FramesIntoSleepIntroVertical] && sprite.flipY == true)
                    sprite.flipY = true;
                else
                    sprite.flipY = false;
                break;
            case "stompTurn":
                if (fallDir == PlayState.EDirsCardinal.Up || fallDir == PlayState.EDirsCardinal.Down)
                {
                    sprite.flipX = !facingLeft;
                    if (thisFrame < animData[(int)(grounded ? AnimData.FramesIntoGroundedTurnaround : AnimData.FramesIntoAirborneTurnaround)])
                        sprite.flipY = !flipVert;
                    else
                        sprite.flipY = flipVert;
                }
                else
                {
                    sprite.flipY = !facingDown;
                    if (thisFrame < animData[(int)(grounded ? AnimData.FramesIntoGroundedTurnaround : AnimData.FramesIntoAirborneTurnaround)])
                        sprite.flipX = !flipHoriz;
                    else
                        sprite.flipX = flipHoriz;
                }
                break;
            case "stomp":
                sprite.flipX = flipHoriz;
                sprite.flipY = flipVert;
                break;
            case "stompEnter":
                if (thisFrame >= animData[(int)AnimData.FramesIntoStompIntroHorizontal] && flipHoriz)
                    sprite.flipX = true;
                else
                    sprite.flipX = false;
                if (thisFrame >= animData[(int)AnimData.FramesIntoStompIntroVertical] && flipVert)
                    sprite.flipY = true;
                else
                    sprite.flipY = false;
                break;
            case "stompExit":
                if (thisFrame < animData[(int)AnimData.FramesIntoStompOutroHorizontal] && flipHoriz)
                    sprite.flipX = true;
                else
                    sprite.flipX = false;
                if (thisFrame < animData[(int)AnimData.FramesIntoStompOutroVertical] && flipVert)
                    sprite.flipY = true;
                else
                    sprite.flipY = false;
                break;
            default:
                sprite.flipX = facingLeft;
                sprite.flipY = facingDown;
                break;
        }
        if (animData[(int)AnimData.AllowHorizontalSpriteFlip] == 0)
            sprite.flipX = false;
        if (animData[(int)AnimData.AllowVerticalSpriteFlip] == 0)
            sprite.flipY = false;

        if ((lastAnimState == "shell" || (mode == BossMode.Sleep && !grounded)) && mode != BossMode.Intro)
        {
            trailTimeout -= Time.fixedDeltaTime;
            if (trailTimeout <= 0)
            {
                trailTimeout += TRAIL_TIMEOUT;

                int baseSpriteID = anim.GetCurrentFrameValue() % animData[19];
                int attackOffset = mode switch
                {
                    BossMode.Stomp => 0,
                    BossMode.Smash => 1,
                    BossMode.Strafe => 2,
                    BossMode.Sleep => 3,
                    _ => 0
                };
                int finalSpriteID = baseSpriteID + (attackOffset * animData[19]);
                PlayState.RequestParticle(transform.position, "gigaTrail", new float[] { finalSpriteID, sprite.flipX ? 1 : 0, sprite.flipY ? 1 : 0 });
            }
        }
        else
            trailTimeout = 0;
    }

    public override void LateUpdate()
    {
        invulnerable = !(lastAnimState == "stomp" || lastAnimState == "stompTurn" || lastAnimState == "stompIntro");
        base.LateUpdate();
        if (health < maxHealth * 0.4f && attackPhase == 0)
        {
            PlayAnim(grounded ? AnimTypes.turnRedGround : AnimTypes.turnRedAir);
            attackPhase = 1;
            bossSpeed += 0.5f;
        }
    }

    private enum AnimTypes
    {
        idle,
        strafe,
        smash,
        smashCollide,
        sleepStart,
        sleepLand,
        shellIntoStomp,
        stompIdle,
        jump,
        turnGround,
        turnAir,
        flip,
        land,
        stompIntoShell,
        turnRedGround,
        turnRedAir
    };
    private void PlayAnim(AnimTypes state)
    {
        string animName = "Boss_gigaSnail_";
        string att = (attackPhase + 1).ToString();

        switch (state)
        {
            default:
            case AnimTypes.idle:
                animName += "idle" + att;
                lastAnimState = "shell";
                sprite.flipX = false;
                sprite.flipY = false;
                SetBoxState(0);
                break;
            case AnimTypes.strafe:
                animName += "strafe" + att;
                lastAnimState = "shell";
                sprite.flipX = false;
                sprite.flipY = false;
                SetBoxState(0);
                break;
            case AnimTypes.smash:
                animName += "smash" + att + "_" + GetSmashDir();
                lastAnimState = "shell";
                SetBoxState(0);
                break;
            case AnimTypes.smashCollide:
                animName += "smash" + att + "_collide_" + GetSmashDir();
                lastAnimState = "shell";
                break;
            case AnimTypes.sleepStart:
                if (lastAnimState == "shell" || lastFallDir == PlayState.EDirsCardinal.None)
                    animName += "sleep" + att + "_shelled";
                else
                    animName += "sleep" + att + "_unshelled_" + FallEnumToString(lastFallDir) + "_" + GetFacingString();
                lastAnimState = "sleepTurn";
                SetBoxState(0);
                break;
            case AnimTypes.sleepLand:
                animName += "sleep" + att + "_land";
                lastAnimState = "shell";
                sprite.flipX = false;
                sprite.flipY = false;
                break;
            case AnimTypes.shellIntoStomp:
                animName += "stomp" + att + "_unshell_" + FallEnumToString(fallDir) + "_" + GetFacingString();
                lastAnimState = animData[(int)AnimData.UpdateOnJumpLand] == 1 ? "stompIntro" : "stomp";
                SetBoxState(1);
                break;
            case AnimTypes.stompIdle:
                animName += "stomp" + att + "_idle_" + FallEnumToString(fallDir) + "_" + GetFacingString();
                lastAnimState = "stomp";
                break;
            case AnimTypes.jump:
                animName += "stomp" + att + "_jump_" + FallEnumToString(fallDir) + "_" + GetFacingString();
                lastAnimState = "stomp";
                break;
            case AnimTypes.turnGround:
                animName += "stomp" + att + "_turn_" + FallEnumToString(fallDir) + "_" + GetFacingString();
                lastAnimState = "stompTurn";
                break;
            case AnimTypes.turnAir:
                animName += "stomp" + att + "_turnAir_" + FallEnumToString(fallDir) + "_" + GetFacingString();
                lastAnimState = "stompTurn";
                break;
            case AnimTypes.flip:
                animName += "stomp" + att + "_flip_" + FallEnumToString(fallDir) + "_" + GetFacingString();
                lastAnimState = "stompTurn";
                break;
            case AnimTypes.land:
                animName += "stomp" + att + "_land_" + FallEnumToString(fallDir) + "_" + GetFacingString();
                lastAnimState = "stomp";
                break;
            case AnimTypes.stompIntoShell:
                animName += "stomp" + att + "_shell_" + FallEnumToString(fallDir) + "_" + GetFacingString();
                lastAnimState = "stompOutro";
                SetBoxState(0);
                break;
            case AnimTypes.turnRedGround:
                animName += "stomp" + att + "_turnRed_" + FallEnumToString(fallDir) + "_" + GetFacingString();
                lastAnimState = "stomp";
                break;
            case AnimTypes.turnRedAir:
                animName += "stomp" + att + "_turnRedAir_" + FallEnumToString(fallDir) + "_" + GetFacingString();
                lastAnimState = "stomp";
                break;
        }

        if (!anim.isPlaying)
            animName = "Boss_gigaSnail_idle" + (attackPhase + 1).ToString();
        if (anim.currentAnimName != animName)
            anim.Play(animName);
    }

    private string GetSmashDir()
    {
        float angle = Vector2.SignedAngle(Vector2.up, smashAccel.normalized);
        while (angle < 0)
            angle += 360;
        while (angle > 360)
            angle -= 360;
        if (angle > 337.5f)
            return "N";
        else if (angle > 292.5f)
            return "NE";
        else if (angle > 247.5f)
            return "E";
        else if (angle > 202.5f)
            return "SE";
        else if (angle > 157.5f)
            return "S";
        else if (angle > 112.5f)
            return "SW";
        else if (angle > 67.5f)
            return "W";
        else if (angle > 22.5f)
            return "NW";
        else
            return "N";
    }

    private string GetFacingString()
    {
        if (lastFallDir == PlayState.EDirsCardinal.Up || lastFallDir == PlayState.EDirsCardinal.Down)
            return facingLeft ? "L" : "R";
        else
            return facingDown ? "D" : "U";
    }

    private string FallEnumToString(PlayState.EDirsCardinal dir)
    {
        return dir switch
        {
            PlayState.EDirsCardinal.Down => "floor",
            PlayState.EDirsCardinal.Left => "wallL",
            PlayState.EDirsCardinal.Right => "wallR",
            PlayState.EDirsCardinal.Up => "ceiling",
            _ => "floor"
        };
    }

    private void AimStrafe()
    {
        float fireAngle = Mathf.Atan2(PlayState.player.transform.position.y - transform.position.y, PlayState.player.transform.position.x - transform.position.x);
        strafeCount = Mathf.Clamp(Mathf.FloorToInt(2.3f + 5f * (maxHealth - health) / maxHealth), 2, 7);
        strafeTheta = fireAngle - Mathf.PI / strafeCount;
    }

    private void StrafeSingle(float angle, bool playSound)
    {
        PlayState.ShootEnemyBullet(transform.position, EnemyBullet.BulletType.bigPea, new float[] { STRAFE_SPEED, Mathf.Cos(angle), Mathf.Sin(angle) }, playSound);
    }

    private void StrafeMulti()
    {
        if (strafeTimeout > 0)
            return;

        strafeTimeout = STRAFE_TIMEOUT;
        for (int i = 0; i < strafeCount; i++)
            StrafeSingle(strafeTheta + 2 * Mathf.PI / strafeCount * i, i == 0);
    }

    private void PickSmashDir(bool guaranteeTargetPlayer = false)
    {
        float angle = Random.Range(0f, 1f) * PlayState.TAU;
        if (GetDecision() > 0.5f || guaranteeTargetPlayer)
            angle = Mathf.Atan2(PlayState.player.transform.position.y - transform.position.y, PlayState.player.transform.position.x - transform.position.x);
        velocity = Vector2.zero;
        smashAccel = new Vector2(SMASH_SPEED * bossSpeed * Mathf.Cos(angle), SMASH_SPEED * bossSpeed * Mathf.Sin(angle)) * Time.fixedDeltaTime;

        if ((lastHitDir == PlayState.EDirsCardinal.Right && smashAccel.x > 0) || (lastHitDir == PlayState.EDirsCardinal.Left && smashAccel.x < 0) ||
            GetDistance(smashAccel.x > 0 ? PlayState.EDirsCardinal.Right : PlayState.EDirsCardinal.Left) < SMASH_REFLECT_BUFFER)
            smashAccel.x *= -1;


        if ((lastHitDir == PlayState.EDirsCardinal.Up && smashAccel.y > 0) || (lastHitDir == PlayState.EDirsCardinal.Down && smashAccel.y < 0) ||
            GetDistance(smashAccel.y > 0 ? PlayState.EDirsCardinal.Up : PlayState.EDirsCardinal.Down) < SMASH_REFLECT_BUFFER)
            smashAccel.y *= -1;
    }

    private void FacePlayer()
    {
        if (fallDir == PlayState.EDirsCardinal.None)
            return;

        bool lastState = (fallDir == PlayState.EDirsCardinal.Left || fallDir == PlayState.EDirsCardinal.Right) ? facingLeft : facingDown;
        if (fallDir == PlayState.EDirsCardinal.Left || fallDir == PlayState.EDirsCardinal.Right)
        {
            bool targetState = PlayState.player.transform.position.y < transform.position.y;
            if (targetState != facingDown)
            {
                facingDown = targetState;
                if (animData[(int)AnimData.UpdateOnTurnaround] != 1)
                    flipVert = targetState;
                else if (flipVert == targetState)
                    flipVert = !targetState;
            }
        }
        else
        {
            bool targetState = PlayState.player.transform.position.x < transform.position.x;
            if (targetState != facingLeft)
            {
                facingLeft = targetState;
                if (animData[(int)AnimData.UpdateOnTurnaround] != 1)
                    flipHoriz = targetState;
                else if (flipHoriz == targetState)
                    flipHoriz = !targetState;
            }
        }
        bool currentState = (fallDir == PlayState.EDirsCardinal.Left || fallDir == PlayState.EDirsCardinal.Right) ? facingLeft : facingDown;
        if (currentState != lastState && animData[(int)AnimData.UpdateOnTurnaround] == 1)
            PlayAnim(grounded ? AnimTypes.turnGround : AnimTypes.turnAir);
    }

    private void CollectTargetPoints()
    {
        for (int i = 0; i < PlayState.activeTargets.Count; i++)
        {
            if (PlayState.activeTargets[i].type == PlayState.TargetTypes.GigaStomp)
                stompPoints.Add(PlayState.activeTargets[i]);
        }
    }

    private float GetDistance(PlayState.EDirsCardinal direction)
    {
        Vector2 ul = new(transform.position.x - halfBox.x, transform.position.y + halfBox.y);
        Vector2 dl = (Vector2)transform.position - halfBox;
        Vector2 dr = new(transform.position.x + halfBox.x, transform.position.y - halfBox.y);
        Vector2 ur = (Vector2)transform.position + halfBox;

        return direction switch
        {
            PlayState.EDirsCardinal.Down => PlayState.GetDistance(direction, dl, dr, CAST_COUNT, enemyCollide),
            PlayState.EDirsCardinal.Left => PlayState.GetDistance(direction, dl, ul, CAST_COUNT, enemyCollide),
            PlayState.EDirsCardinal.Right => PlayState.GetDistance(direction, dr, ur, CAST_COUNT, enemyCollide),
            _ => PlayState.GetDistance(direction, ul, ur, CAST_COUNT, enemyCollide)
        };
    }

    private void SetBoxState(int state)
    {
        box.size = new Vector2(state == 1 ? boxSize.x : boxSize.y, state == -1 ? boxSize.x : boxSize.y);
    }

    public override void Kill()
    {
        foreach (Particle star in bgStars)
            star.ResetParticle();
        UpdateBackground("fadeOut");
        PlayState.ToggleGigaTiles(true);

        string unlocks = "";
        PlayState.QueueAchievementPopup(AchievementPanel.Achievements.BeatMoonSnail);
        if (!PlayState.CheckForItem("Full-Metal Snail"))
            PlayState.QueueAchievementPopup(AchievementPanel.Achievements.BeatMoonSnailNoFMS);
        if (PlayState.currentProfile.gameTime[0] == 0 && PlayState.currentProfile.gameTime[1] < 30 && !PlayState.generalData.achievements[13])
        {
            PlayState.QueueAchievementPopup(AchievementPanel.Achievements.Under30Minutes);
            unlocks += "Insane";
        }
        PlayState.QueueAchievementPopup(PlayState.currentProfile.character switch
        {
            "Sluggy" => AchievementPanel.Achievements.WinSluggy,
            "Upside" => AchievementPanel.Achievements.WinUpside,
            "Leggy" => AchievementPanel.Achievements.WinLeggy,
            "Blobby" => AchievementPanel.Achievements.WinBlobby,
            "Leechy" => AchievementPanel.Achievements.WinLeechy,
            _ => AchievementPanel.Achievements.BeatMoonSnail
        });
        if (!PlayState.HasTime())
            unlocks = unlocks == "" ? "BossRush" : "BossRushAndInsane";
        PlayState.credits.StartCredits(PlayState.currentProfile.gameTime);
        if (PlayState.currentProfile.difficulty != 0)
        {
            PlayState.TimeIndeces targetTime = PlayState.currentProfile.character switch
            {
                "Sluggy" => PlayState.currentProfile.difficulty == 2 ? PlayState.TimeIndeces.sluggyInsane : PlayState.TimeIndeces.sluggyNormal,
                "Upside" => PlayState.currentProfile.difficulty == 2 ? PlayState.TimeIndeces.upsideInsane : PlayState.TimeIndeces.upsideNormal,
                "Leggy" => PlayState.currentProfile.difficulty == 2 ? PlayState.TimeIndeces.leggyInsane : PlayState.TimeIndeces.leggyNormal,
                "Blobby" => PlayState.currentProfile.difficulty == 2 ? PlayState.TimeIndeces.blobbyInsane : PlayState.TimeIndeces.blobbyNormal,
                "Leechy" => PlayState.currentProfile.difficulty == 2 ? PlayState.TimeIndeces.leechyInsane : PlayState.TimeIndeces.leechyNormal,
                _ => PlayState.currentProfile.difficulty == 2 ? PlayState.TimeIndeces.snailyInsane : PlayState.TimeIndeces.snailyNormal
            };

            if (PlayState.CompareTimes(PlayState.GetTime(targetTime), new float[] { 0, 0, 0 }) == 0 ||
                PlayState.CompareTimes(PlayState.GetTime(targetTime), PlayState.currentProfile.gameTime) == 1)
            {
                PlayState.SetTime(targetTime, PlayState.currentProfile.gameTime);
                PlayState.globalFunctions.FlashHUDText(GlobalFunctions.TextTypes.bestTime);
                if (unlocks != "")
                    PlayState.globalFunctions.FlashHUDText(GlobalFunctions.TextTypes.unlock, unlocks);
            }
        }

        PlayState.currentProfile.bossStates[ID] = 0;
        PlayState.WriteSave(PlayState.currentProfileNumber, true);
        PlayState.ToggleBossfightState(false, -1);
        PlayState.globalFunctions.RequestQueuedExplosion(transform.position, 11.7f, 1, true);
        PlayState.globalFunctions.ScreenShake(new List<float> { 0.075f, 0.075f, 0 }, new List<float> { 10.7f, 1.2f });
        foreach (Transform bullet in PlayState.enemyBulletPool.transform)
            bullet.GetComponent<EnemyBullet>().Despawn();
        delayedDespawn = true;
        box.enabled = false;
        sprite.enabled = false;
        anim.Stop();
    }
}
