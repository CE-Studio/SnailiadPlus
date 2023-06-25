using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonSnail : Boss
{
    private const float RING_TIMEOUT = 1.7f;
    private const float JUMP_LENGTH = 0.3f;
    private const float TELEPORT_TIME = 1.4f;
    private const float ATTACK_START_TIMEOUT = 0.45f;
    private const float ATTACK_STOP_TIMEOUT = 0.9f;
    private const float JUMP_POWER_NORMAL = 25.25f;
    private const float JUMP_POWER_HIGH = 31.125f;
    private const float ACTION_TIMEOUT = 0.7f;
    private const float SHADOW_BALL_RADIUS = 5f;
    private const int SHADOW_BALL_COUNT = 5;

    private readonly float[] weaponTimeouts = new float[] { 0.1f, 0.3f, 0.155f };
    private readonly float[] weaponSpeeds = new float[] { 0.4625f, 0.4125f, 0.075f };

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
        Attack,
        Move,
        Teleport,
        Strafe
    };
    private BossMode mode;

    private float ringTimeout;
    private float releaseJumpTimeout;
    private int attackMode;
    private float modeElapsed;
    private float speed = 1f;
    private bool modeInitialized;
    private float actionTimeout;
    private Vector2 moveStart = Vector2.zero;
    private Vector2 moveEnd = Vector2.zero;
    private Vector2 teleStart = Vector2.zero;
    private Vector2 teleEnd = Vector2.zero;
    private float attackStartTimeout = 0;
    private float attackStopTimeout = ATTACK_STOP_TIMEOUT;
    private int attackPhase = 0;
    private int decisionTableIndex = 0;
    private int currentWeapon = 2;
    private float nextMove = 0;
    private PlayState.EDirsSurface gravity;
    private PlayState.EDirsSurface targetGravity;
    private PlayState.EDirsSurface desiredGravity;
    private PlayState.EDirsSurface gravJumpDir;
    private bool facingLeft = false;
    private bool facingDown = true;
    private bool grounded = false;
    private bool[] virtualInputs = new bool[] { false, false, false, false, false, false };
    private float elapsed;
    private bool isAttacking = false;

    private List<GameObject> shadowBalls = new List<GameObject>();
    private List<AnimationModule> shadowBallAnims = new List<AnimationModule>();

    private int[] animData;
    /*\
     *   ANIMATION DATA
     * 0 - Flip sprite horizontally
     * 1 - Flip sprite vertically
     * 2 - Update animation on move
     * 3 - Update animation when off the ground
     * 4 - Restart shadow ball animations on teleport
    \*/

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (PlayState.IsBossAlive(3))
        {
            SpawnBoss(7000, 0, 30, true, 3, true);
            StartCoroutine(RunIntro());
            PlayState.playerScript.CorrectGravity(true);

            decisionTableIndex = (PlayState.playerScript.maxHealth + PlayState.GetItemPercentage()) % decisionTable.Length;

            if (PlayState.currentProfile.difficulty == 2)
                speed += 0.1f;

            for (int i = 0; i < SHADOW_BALL_COUNT; i++)
            {
                GameObject newShadowBall = new("Shadow Ball " + i.ToString());
                newShadowBall.transform.parent = transform;
                SpriteRenderer ballSprite = newShadowBall.AddComponent<SpriteRenderer>();
                ballSprite.sortingOrder = -49;
                AnimationModule ballAnim = newShadowBall.AddComponent<AnimationModule>();
                ballAnim.Add("Boss_moonSnail_shadowBall1");
                ballAnim.Add("Boss_moonSnail_shadowBall2");
                ballAnim.Play("Boss_moonSnail_shadowBall1");
                shadowBalls.Add(newShadowBall);
                shadowBallAnims.Add(ballAnim);
                newShadowBall.SetActive(false);
            }

            string[] states = new string[] { "idle", "move", "jump", "shoot", "shell" };
            string[] directions = new string[] { "floor_L", "floor_R", "wallL_D", "wallL_U", "wallR_D", "wallR_U", "ceiling_L", "ceiling_R" };
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < states.Length; j++)
                {
                    for (int k = 0; k < directions.Length; k++)
                        anim.Add("Boss_moonSnail_" + states[j] + i.ToString() + "_" + directions[k]);
                }
            }
        }
        else
            Destroy(gameObject);
    }
}
