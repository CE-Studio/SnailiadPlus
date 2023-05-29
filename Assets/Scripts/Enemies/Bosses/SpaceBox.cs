using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceBox : Boss
{
    private const int MAX_BOXES = 8;
    private const int SHIELD_SLOTS = 36;
    private const int MAX_ACTIVE_SHIELDS = 26;
    private const int SHIELD_START = 0;
    private const int DAMAGE_TAKEN_FOR_SHIELD = 100;
    private const int SHIELD_PERIOD = 4;
    private const float MODE_TIMEOUT = 0.6f;
    private const float SPAWN_TIMEOUT = 2.5f;
    private const float STARTING_SPEED = 1f;
    private const int SPAWN_COUNTER = 8;
    private const int SHOT_COUNT = 4;
    private const float SHOT_TIMEOUT = 0.6f;
    private const float CLUSTER_TIMEOUT = 4.1f;

    private float[] decisionTable = new float[]
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
        Idle,
        Up,
        Upright,
        Right,
        Downright,
        Down,
        Downleft,
        Left,
        Upleft,
        Wait
    };
    private BossMode mode = BossMode.Idle;
    private BossMode lastMode = BossMode.Right;
    private BossMode nextMode = BossMode.Up;

    private enum ShootMode
    {
        Attack,
        WaitCluster
    };
    private ShootMode shootMode;

    private List<GameObject> shields = new List<GameObject>();
    private List<GameObject> babyboxes = new List<GameObject>();

    private float modeTimeout = MODE_TIMEOUT;
    private int spawnCounter = SPAWN_COUNTER;
    private int attackMode = 0;
    private float speed = STARTING_SPEED;
    private float acceleration = 0.2625f;
    private int decisionTableIndex = 0;
    private int shotCount = SHOT_COUNT;
    private float shotTimeout = SHOT_TIMEOUT;
    private float clusterTimeout = CLUSTER_TIMEOUT;

    private void Awake()
    {
        if (PlayState.IsBossAlive(2))
        {
            SpawnBoss(5100, 4, 9, true, 2);


        }
    }
}
