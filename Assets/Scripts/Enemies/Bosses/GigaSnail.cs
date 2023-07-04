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
    private BossMode lastMode = BossMode.Intro;

    private PlayState.EDirsCardinal gravity;

    private int lastHitDir;
    private int lastStomp;
    private int strafeCount;
    private Vector2 velocity;
    private Vector2 lastSmashVelocity = Vector2.zero;
    private int decisionTableIndex;
    private float shotTimeout;
    private float bossSpeed;
    private int attackPhase;
    private float elapsed;
    private float modeElapsed;
    private bool modeInitialized;
    private float modeTimeout;
    private Vector2 moveOrigin = Vector2.zero;
    private Vector2 moveTarget = Vector2.zero;
    private float strafeTheta;
    private float strafeThetaVel;
    private float strafeThetaAccel;
    private bool waitingToJump;
    private float strafeTimeout;
    private float stompTimeout;
    private float waveTimeout;
    private bool stomped;
    private bool aimed;
    private float gravJumpTimeout;
    private float jumpTimeout;
    private float zzzTimeout;

    private int[] animData;
    /*\
     *   ANIMATION DATA
     *  0 - Allow horizontal sprite flip
     *  1 - Allow vertical sprite flip
     *  2 - Fade sprite in on spawn
     *  3 - Update animation on phase change
     *  4 - Update animation on jump during Stomp phase
     *  5 - Update animation on gravity jump during Stomp phase
     *  6 - Update animation on turnaround during Stomp phase
     *  7 - Frames into Stomp turnaround to flip sprite
     *  8 - Frames into horizontal Stomp gravity jump to flip sprite
     *  9 - Frames into vertical Stomp gravity jump to flip sprite
     * 10 - Update animation on collision during Smash phase
     * 11 - Update animation on landing during Sleep phase
    \*/

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
     * 1 - crossfade attack backgrounds
     * 2 - display stars over the background
    \*/

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

        if (PlayState.currentProfile.difficulty == 2)
            bossSpeed += 0.2f;

        animData = PlayState.GetAnim("Boss_gigaSnail_data").frames;
        if (animData[2] == 1)
            sprite.color = new Color32(255, 255, 255, 0);

        bgAnimData = PlayState.GetAnim("GigaBackground_data").frames;
        for (int i = 1; i <= 2; i++)
        {
            BGObj newBG = new() { obj = new GameObject("Giga Snail Background Layer " + i.ToString()) };
            newBG.obj.transform.parent = PlayState.cam.transform;

            newBG.sprite = newBG.obj.AddComponent<SpriteRenderer>();
            newBG.sprite.sortingOrder = -124;

            newBG.anim = newBG.obj.AddComponent<AnimationModule>();
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
        }
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

        PlayState.ShootEnemyBullet(transform.position, EnemyBullet.BulletType.gigaWave, new float[] { WAVE_SPEED, direction.x, direction.y });
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

        lastMode = mode;
        mode = newMode;
        modeInitialized = false;
        stomped = false;
        velocity = Vector2.zero;
        modeElapsed = 0;
        waitingToJump = false;
    }

    private void UpdateAIIntro()
    {
        if (elapsed > 2f && elapsed < 3f && animData[2] == 1)
            sprite.color = new Color32(255, 255, 255, (byte)((elapsed - 2) * 255));
        else if (elapsed > 3)
            StartCoroutine(RunIntro(true, true, true));
        if (introDone)
        {
            PlayState.ToggleGigaTiles(false);
            SetMode(BossMode.Stomp);
            box.enabled = true;
        }
    }
}
