using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonSnail : Boss
{
    private const float DONUT_TIMEOUT = 1.7f;
    private const int DONUT_COUNT = 3;
    private const float JUMP_LENGTH = 0.3f;
    private const float TELEPORT_TIME = 1.4f;
    private const float ATTACK_START_TIMEOUT = 0.45f;
    private const float ATTACK_STOP_TIMEOUT = 0.9f;
    private const float RUN_SPEED = 11;
    private const float JUMP_POWER = 25.25f;
    private const float GRAVITY = 1.25f;
    private const float TERMINAL_VELOCITY = -0.5208f;
    private const float ACTION_TIMEOUT = 0.7f;
    private const float SHADOW_BALL_RADIUS = 5f;
    private const int SHADOW_BALL_COUNT = 5;
    private const float MOVE_TARGET_MARGIN = 0.625f;
    private const float TARGET_ATTACK_DISTANCE = 2.5f;
    private const float PLAYER_TOO_CLOSE_RADIUS = 3.75f;
    private const float AIM_DETECT_RADIUS_HORIZONTAL = 3.125f;
    private const float AIM_DETECT_RADIUS_VERTICAL = 12.5f;
    private const int CAST_COUNT = 4;

    private readonly float[] weaponTimeouts = new float[] { 0.05f, 0.15f, 0.0775f };
    private readonly float[] weaponSpeeds = new float[] { 0.925f, 16.875f, 0.15f };

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

    private float donutTimeout = 0;
    private float releaseJumpTimeout;
    private float modeElapsed;
    private float speed = 1f;
    private bool modeInitialized;
    private float actionTimeout;
    private float weaponTimeout;
    private Vector2 moveEnd = Vector2.zero;
    private Vector2 teleStart = Vector2.zero;
    private Vector2 teleEnd = Vector2.zero;
    private float attackStartTimeout = 0;
    private float attackStopTimeout = ATTACK_STOP_TIMEOUT;
    private int attackPhase = 0;
    private int decisionTableIndex = 0;
    private int currentWeapon = 2;
    private PlayState.EDirsSurface gravity;
    private PlayState.EDirsSurface targetGravity;
    private PlayState.EDirsSurface gravJumpDir;
    private PlayState.EDirsSurface mostRecentDir;
    private bool facingLeft = false;
    private bool facingDown = true;
    private bool isJumping = false;
    private bool grounded = false;
    private int[] virtualInputs = new int[] { };
    private bool[] tappedInputs = new bool[] { };
    private bool isAttacking = false;
    private Vector2 velocity;

    private enum Inputs { Up, Down, Left, Right, Jump, Shoot, Shell };

    private List<GameObject> shadowBalls = new();
    private List<AnimationModule> shadowBallAnims = new();

    private List<PlayState.TargetPoint> telePoints = new();
    private List<PlayState.TargetPoint> movePoints = new();
    private PlayState.TargetPoint gigaSpawnPoint;

    private BoxCollider2D box;
    private Vector2 boxSize;

    private int[] animData;
    /*\
     *   ANIMATION DATA
     * 0 - Flip sprite horizontally
     * 1 - Flip sprite vertically
     * 2 - Update animation on move
     * 3 - Update animation when off the ground
     * 4 - Update animation when shooting
     * 5 - Restart shadow ball animations on teleport
    \*/

    private readonly bool debugSkipToGiga = false;

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (PlayState.IsBossAlive(3))
        {
            SpawnBoss(7000, 0, 30, true, 3, true);
            if (debugSkipToGiga)
                StartCoroutine(DelayedSkipToGiga());
            else
            {
                StartCoroutine(RunIntro());
                PlayState.playerScript.CorrectGravity(true);
                PlayState.playerScript.ZeroWalkVelocity();
                gigaSpawnPoint = new PlayState.TargetPoint { pos = (Vector2)transform.parent.position + origin };

                int inputCount = System.Enum.GetValues(typeof(Inputs)).Length;
                virtualInputs = new int[inputCount];
                tappedInputs = new bool[inputCount];

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
                            anim.Add("Boss_moonSnail_" + states[j] + (i + 1).ToString() + "_" + directions[k]);
                    }
                }
                animData = PlayState.GetAnim("Boss_moonSnail_data").frames;

                col.TryGetComponent(out box);
                boxSize = box.size;
            }
        }
        else
            Destroy(gameObject);
    }

    private IEnumerator DelayedSkipToGiga()
    {
        while (gigaSpawnPoint.pos == Vector2.zero)
        {
            CollectTargetPoints();
            yield return new WaitForEndOfFrame();
        }
        Instantiate(Resources.Load<GameObject>("Objects/Enemies/Bosses/Giga Snail"), gigaSpawnPoint.pos, Quaternion.identity, transform.parent);
        PlayState.ToggleBossfightState(false, -1);
        Destroy(gameObject);
    }

    private void SetMode(BossMode newMode, bool shootDonuts = false)
    {
        mode = newMode;
        modeElapsed = 0;
        modeInitialized = false;
        isAttacking = false;
        actionTimeout = ACTION_TIMEOUT;
        for (int i = 0; i < shadowBalls.Count; i++)
            shadowBalls[i].SetActive(false);
        sprite.enabled = true;
        box.enabled = true;
        ReleaseAllInputs();
        attackStopTimeout = ATTACK_STOP_TIMEOUT;
        attackStartTimeout = ATTACK_START_TIMEOUT;
        if (shootDonuts)
            CheckFireDonuts();
    }

    private void CheckFireDonuts()
    {
        if (donutTimeout <= 0)
        {
            donutTimeout = DONUT_TIMEOUT;
            float angle = Mathf.Atan2(transform.position.y - PlayState.player.transform.position.y,
                transform.position.x - PlayState.player.transform.position.x);
            for (int i = 0; i < DONUT_COUNT; i++)
                PlayState.ShootEnemyBullet(transform.position, EnemyBullet.BulletType.donutHybrid, new float[]
                    { 0, angle, 0.2f, 1f, 2.2f, PlayState.TAU / DONUT_COUNT * i }, i == 0);
        }
    }

    private void PickMoveTarget()
    {
        if (movePoints.Count == 0)
        {
            moveEnd = origin;
            targetGravity = PlayState.EDirsSurface.Floor;
        }
        else
        {
            int pointID = Mathf.FloorToInt(GetDecision() * movePoints.Count);
            moveEnd = movePoints[pointID].pos;
            targetGravity = CompassToSurface(movePoints[pointID].directions[0]);
        }
    }
    private void PickTeleTarget()
    {
        teleStart = transform.position;
        if (telePoints.Count == 0)
        {
            teleEnd = origin;
            SetGravity(PlayState.EDirsSurface.Floor);
        }
        else
        {
            int pointID = Mathf.FloorToInt(GetDecision() * telePoints.Count);
            teleEnd = telePoints[pointID].pos;
            SetGravity(CompassToSurface(telePoints[pointID].directions[0]));
        }
    }

    private void PressInput(Inputs input, bool tapped = false)
    {
        if (virtualInputs[(int)input] == 0)
            virtualInputs[(int)input] = 1;
        if (tapped)
            tappedInputs[(int)input] = true;
        if ((int)input < (int)Inputs.Jump)
        {
            mostRecentDir = input switch
            {
                Inputs.Down => PlayState.EDirsSurface.Floor,
                Inputs.Left => PlayState.EDirsSurface.WallL,
                Inputs.Right => PlayState.EDirsSurface.WallR,
                _ => PlayState.EDirsSurface.Ceiling
            };
        }
    }

    private void ReleaseInput(Inputs input)
    {
        virtualInputs[(int)input] = 0;
    }

    private void ReleaseAllInputs()
    {
        for (int i = 0; i < virtualInputs.Length; i++)
            virtualInputs[i] = 0;
    }

    private void UntapInputs()
    {
        for (int i = 0; i < virtualInputs.Length; i++)
        {
            if (tappedInputs[i])
            {
                tappedInputs[i] = false;
                virtualInputs[i] = 0;
            }
        }
    }

    private void IncrementInputFrames()
    {
        for (int i = 0; i < virtualInputs.Length; i++)
        {
            if (virtualInputs[i] > 0)
                virtualInputs[i]++;
        }
    }

    private int GetInput(Inputs input)
    {
        return virtualInputs[(int)input];
    }

    private bool CheckForFreshInput(Inputs input)
    {
        return virtualInputs[(int)input] == 1;
    }

    private void UpdateAIMove()
    {
        if (!modeInitialized)
        {
            modeInitialized = true;
            currentWeapon = 1;
            PickMoveTarget();
            ReleaseAllInputs();
        }
        if (attackPhase >= 1 || PlayState.currentProfile.difficulty == 2)
            isAttacking = true;
        if ((gravity == PlayState.EDirsSurface.Floor || gravity == PlayState.EDirsSurface.Ceiling) && Mathf.Abs(moveEnd.x - transform.position.x) > MOVE_TARGET_MARGIN)
            PressInput(moveEnd.x > transform.position.x ? Inputs.Right : Inputs.Left);
        else
        {
            ReleaseInput(Inputs.Left);
            ReleaseInput(Inputs.Right);
        }
        if ((gravity == PlayState.EDirsSurface.WallL || gravity == PlayState.EDirsSurface.WallR) && Mathf.Abs(moveEnd.y - transform.position.y) > MOVE_TARGET_MARGIN)
            PressInput(moveEnd.y > transform.position.y ? Inputs.Up : Inputs.Down);
        else
        {
            ReleaseInput(Inputs.Up);
            ReleaseInput(Inputs.Down);
        }
        if (!isJumping)
        {
            if (gravity != targetGravity)
                AIJump(targetGravity);
            else
                AIJump();
        }
        CheckFireDonuts();
        if (Vector2.Distance(transform.position, moveEnd) < TARGET_ATTACK_DISTANCE)
            SetMode(BossMode.Attack);
    }

    private void UpdateAIIntro()
    {
        if (telePoints.Count == 0 && movePoints.Count == 0)
            CollectTargetPoints();
        if (modeElapsed > 0.3f)
            SetMode(BossMode.Attack);
    }

    private void UpdateAITeleport()
    {
        float shadowRadius;
        float shadowTheta;
        if (!modeInitialized)
        {
            modeInitialized = true;
            PickTeleTarget();
            sprite.enabled = false;
            box.enabled = false;
            for (int i = 0; i < shadowBalls.Count; i++)
            {
                shadowBalls[i].SetActive(true);
                shadowBalls[i].transform.localPosition = Vector2.zero;
                if (animData[5] == 1)
                    shadowBallAnims[i].Play("Boss_moonSnail_shadowBall" + (attackPhase + 1).ToString());
            }
        }
        float teleElapsed = NormalizedSigmoid(modeElapsed / TELEPORT_TIME);
        if (teleElapsed <= 0.5)
        {
            shadowRadius = SHADOW_BALL_RADIUS * NormalizedSigmoid(modeElapsed / TELEPORT_TIME * 2);
            shadowTheta = PlayState.TAU * NormalizedSigmoid(modeElapsed / TELEPORT_TIME * 2);
        }
        else
        {
            shadowRadius = SHADOW_BALL_RADIUS * NormalizedSigmoid((1 - modeElapsed / TELEPORT_TIME) * 2);
            shadowTheta = PlayState.TAU * NormalizedSigmoid((1 - modeElapsed / TELEPORT_TIME) * 2);
        }
        for (int i = 0; i < shadowBalls.Count; i++)
        {
            shadowBalls[i].transform.position = new Vector2(
                teleStart.x * (1 - teleElapsed) + teleEnd.x * teleElapsed + Mathf.Cos(shadowTheta + PlayState.TAU / shadowBalls.Count * i) * shadowRadius,
                teleStart.y * (1 - teleElapsed) + teleEnd.y * teleElapsed + Mathf.Sin(shadowTheta + PlayState.TAU / shadowBalls.Count * i) * shadowRadius
                );
        }
        velocity = Vector2.zero;
        transform.position = teleEnd;
        ReleaseAllInputs();
        grounded = false;
        if (modeElapsed / TELEPORT_TIME >= 1)
            SetMode(BossMode.Attack, true);
    }

    private void UpdateAIAttack()
    {
        FacePlayer();
        if (!isAttacking)
        {
            attackStartTimeout -= Time.fixedDeltaTime * speed;
            if (attackStartTimeout <= 0)
            {
                attackStopTimeout = ATTACK_STOP_TIMEOUT;
                isAttacking = true;
            }
        }
        else
        {
            attackStopTimeout -= Time.fixedDeltaTime * speed;
            if (attackStopTimeout <= 0)
            {
                attackStartTimeout = ATTACK_START_TIMEOUT;
                isAttacking = false;
            }
        }
        currentWeapon = 2;
        if (actionTimeout <= 0)
        {
            actionTimeout = ACTION_TIMEOUT;
            if (GetDecision() < 0.2f)
            {
                switch (gravity)
                {
                    default:
                    case PlayState.EDirsSurface.Floor:
                        AIJump(PlayState.EDirsSurface.Ceiling);
                        break;
                    case PlayState.EDirsSurface.WallL:
                        AIJump(PlayState.EDirsSurface.WallR);
                        break;
                    case PlayState.EDirsSurface.WallR:
                        AIJump(PlayState.EDirsSurface.WallL);
                        break;
                    case PlayState.EDirsSurface.Ceiling:
                        AIJump(PlayState.EDirsSurface.Floor);
                        break;
                }
            }
            else if (GetDecision() < 0.4f)
                AIJump();
            else if (GetDecision() < 0.4f)
                SetMode(BossMode.Teleport);
            else
                SetMode(BossMode.Move);
        }
        if (Vector2.Distance(transform.position, PlayState.player.transform.position) < PLAYER_TOO_CLOSE_RADIUS)
            SetMode(BossMode.Teleport);
        else
        {
            float distanceX = Mathf.Abs(PlayState.player.transform.position.x - transform.position.x);
            float distanceY = Mathf.Abs(PlayState.player.transform.position.y - transform.position.y);
            switch (gravity)
            {
                default:
                case PlayState.EDirsSurface.Floor:
                    if (distanceX < AIM_DETECT_RADIUS_HORIZONTAL || distanceY > AIM_DETECT_RADIUS_VERTICAL)
                        PressInput(Inputs.Up);
                    else
                        ReleaseInput(Inputs.Up);
                    break;
                case PlayState.EDirsSurface.WallL:
                    if (distanceY < AIM_DETECT_RADIUS_HORIZONTAL || distanceX > AIM_DETECT_RADIUS_VERTICAL)
                        PressInput(Inputs.Right);
                    else
                        ReleaseInput(Inputs.Right);
                    break;
                case PlayState.EDirsSurface.WallR:
                    if (distanceY < AIM_DETECT_RADIUS_HORIZONTAL || distanceX > AIM_DETECT_RADIUS_VERTICAL)
                        PressInput(Inputs.Left);
                    else
                        ReleaseInput(Inputs.Left);
                    break;
                case PlayState.EDirsSurface.Ceiling:
                    if (distanceX < AIM_DETECT_RADIUS_HORIZONTAL || distanceY > AIM_DETECT_RADIUS_VERTICAL)
                        PressInput(Inputs.Down);
                    else
                        ReleaseInput(Inputs.Down);
                    break;
            }
        }
    }

    private void AIJump(PlayState.EDirsSurface gravityJumpDirection = PlayState.EDirsSurface.None, float releaseTimeout = 0.3f)
    {
        if (releaseJumpTimeout > 0)
            return;
        PressInput(Inputs.Jump);
        releaseJumpTimeout = releaseTimeout;
        gravJumpDir = gravityJumpDirection;
    }

    private void FacePlayer()
    {
        switch (gravity)
        {
            default:
            case PlayState.EDirsSurface.Floor:
            case PlayState.EDirsSurface.Ceiling:
                PressInput(PlayState.player.transform.position.x < transform.position.x ? Inputs.Left : Inputs.Right, true);
                break;
            case PlayState.EDirsSurface.WallL:
            case PlayState.EDirsSurface.WallR:
                PressInput(PlayState.player.transform.position.y < transform.position.y ? Inputs.Down : Inputs.Up, true);
                break;
        }
    }

    private void UpdateAI()
    {
        UntapInputs();
        donutTimeout -= Time.fixedDeltaTime * speed;
        modeElapsed += Time.fixedDeltaTime * speed;
        actionTimeout -= Time.fixedDeltaTime * speed;
        switch (mode)
        {
            default:
            case BossMode.Intro:
                UpdateAIIntro();
                break;
            case BossMode.Move:
                UpdateAIMove();
                break;
            case BossMode.Attack:
                UpdateAIAttack();
                break;
            case BossMode.Teleport:
                UpdateAITeleport();
                break;
            case BossMode.Strafe:
                break;
        }
        releaseJumpTimeout -= Time.fixedDeltaTime * speed;
        if (releaseJumpTimeout <= 0 && GetInput(Inputs.Jump) > 0)
        {
            ReleaseInput(Inputs.Jump);
            if (gravJumpDir != PlayState.EDirsSurface.None)
            {
                switch (gravJumpDir)
                {
                    default:
                    case PlayState.EDirsSurface.Floor:
                        PressInput(Inputs.Down, true);
                        SetGravity(PlayState.EDirsSurface.Floor, true);
                        break;
                    case PlayState.EDirsSurface.WallL:
                        PressInput(Inputs.Left, true);
                        SetGravity(PlayState.EDirsSurface.WallL, true);
                        break;
                    case PlayState.EDirsSurface.WallR:
                        PressInput(Inputs.Right, true);
                        SetGravity(PlayState.EDirsSurface.WallR, true);
                        break;
                    case PlayState.EDirsSurface.Ceiling:
                        PressInput(Inputs.Up, true);
                        SetGravity(PlayState.EDirsSurface.Ceiling, true);
                        break;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game || debugSkipToGiga)
            return;

        if (!PlayState.paralyzed)
            UpdateAI();
        if (CheckForFreshInput(Inputs.Jump) && isJumping)
            PerformGravityJump();
        if (CheckForFreshInput(Inputs.Jump))
        {
            if (!isJumping && grounded)
                Jump();
        }
        Attack();

        float disFloor;
        float disCeil;
        float disWalls;
        switch (gravity)
        {
            default:
            case PlayState.EDirsSurface.Floor:
                disFloor = GetDistance(PlayState.EDirsSurface.Floor);
                if (grounded)
                {
                    velocity.y = 0;
                    if (disFloor < 0.25f)
                        grounded = false;
                }
                else
                {
                    velocity.y = Mathf.Clamp(velocity.y - GRAVITY * Time.fixedDeltaTime, TERMINAL_VELOCITY, Mathf.Infinity);
                    if (disFloor < Mathf.Abs(velocity.y) && velocity.y < 0)
                    {
                        transform.position += (disFloor - PlayState.FRAC_32) * Vector3.down;
                        velocity.y = 0;
                        grounded = true;
                    }
                }
                if (velocity.y > 0)
                {
                    disCeil = GetDistance(PlayState.EDirsSurface.Ceiling);
                    if (disCeil < Mathf.Abs(velocity.y))
                    {
                        transform.position += (disCeil - PlayState.FRAC_32) * Vector3.up;
                        velocity.y = 0;
                    }
                }
                facingLeft = GetInput(Inputs.Right) <= 0 && (GetInput(Inputs.Left) > 0 || facingLeft);
                velocity.x = RUN_SPEED * Time.fixedDeltaTime * (0 + (GetInput(Inputs.Right) > 1 ? 1 : 0) - (GetInput(Inputs.Left) > 1 ? 1 : 0));
                if (velocity.x != 0)
                {
                    facingLeft = velocity.x < 0;
                    disWalls = GetDistance(facingLeft ? PlayState.EDirsSurface.WallL : PlayState.EDirsSurface.WallR);
                    if (disWalls < Mathf.Abs(velocity.x))
                        velocity.x = (disWalls - PlayState.FRAC_32) * (facingLeft ? -1 : 1);
                }
                break;
            case PlayState.EDirsSurface.WallL:
                disFloor = GetDistance(PlayState.EDirsSurface.WallL);
                if (grounded)
                {
                    velocity.x = 0;
                    if (disFloor < 0.25f)
                        grounded = false;
                }
                else
                {
                    velocity.x = Mathf.Clamp(velocity.x - GRAVITY * Time.fixedDeltaTime, TERMINAL_VELOCITY, Mathf.Infinity);
                    if (disFloor < Mathf.Abs(velocity.x) && velocity.x < 0)
                    {
                        transform.position += (disFloor - PlayState.FRAC_32) * Vector3.left;
                        velocity.x = 0;
                        grounded = true;
                    }
                }
                if (velocity.x > 0)
                {
                    disCeil = GetDistance(PlayState.EDirsSurface.WallR);
                    if (disCeil < Mathf.Abs(velocity.x))
                    {
                        transform.position += (disCeil - PlayState.FRAC_32) * Vector3.right;
                        velocity.x = 0;
                    }
                }
                facingDown = GetInput(Inputs.Up) <= 0 && (GetInput(Inputs.Down) > 0 || facingDown);
                velocity.y = RUN_SPEED * Time.fixedDeltaTime * (0 + (GetInput(Inputs.Up) > 1 ? 1 : 0) - (GetInput(Inputs.Down) > 1 ? 1 : 0));
                if (velocity.y != 0)
                {
                    facingDown = velocity.y < 0;
                    disWalls = GetDistance(facingDown ? PlayState.EDirsSurface.Floor : PlayState.EDirsSurface.Ceiling);
                    if (disWalls < Mathf.Abs(velocity.y))
                        velocity.y = (disWalls - PlayState.FRAC_32) * (facingDown ? -1 : 1);
                }
                break;
            case PlayState.EDirsSurface.WallR:
                disFloor = GetDistance(PlayState.EDirsSurface.WallR);
                if (grounded)
                {
                    velocity.x = 0;
                    if (disFloor < 0.25f)
                        grounded = false;
                }
                else
                {
                    velocity.x = Mathf.Clamp(velocity.x + GRAVITY * Time.fixedDeltaTime, -Mathf.Infinity, -TERMINAL_VELOCITY);
                    if (disFloor < Mathf.Abs(velocity.x) && velocity.x > 0)
                    {
                        transform.position += (disFloor - PlayState.FRAC_32) * Vector3.right;
                        velocity.x = 0;
                        grounded = true;
                    }
                }
                if (velocity.x < 0)
                {
                    disCeil = GetDistance(PlayState.EDirsSurface.WallL);
                    if (disCeil < Mathf.Abs(velocity.x))
                    {
                        transform.position += (disCeil - PlayState.FRAC_32) * Vector3.left;
                        velocity.x = 0;
                    }
                }
                facingDown = GetInput(Inputs.Up) <= 0 && (GetInput(Inputs.Down) > 0 || facingDown);
                velocity.y = RUN_SPEED * Time.fixedDeltaTime * (0 + (GetInput(Inputs.Up) > 1 ? 1 : 0) - (GetInput(Inputs.Down) > 1 ? 1 : 0));
                if (velocity.y != 0)
                {
                    facingDown = velocity.y < 0;
                    disWalls = GetDistance(facingDown ? PlayState.EDirsSurface.Floor : PlayState.EDirsSurface.Ceiling);
                    if (disWalls < Mathf.Abs(velocity.y))
                        velocity.y = (disWalls - PlayState.FRAC_32) * (facingDown ? -1 : 1);
                }
                break;
            case PlayState.EDirsSurface.Ceiling:
                disFloor = GetDistance(PlayState.EDirsSurface.Ceiling);
                if (grounded)
                {
                    velocity.y = 0;
                    if (disFloor < 0.25f)
                        grounded = false;
                }
                else
                {
                    velocity.y = Mathf.Clamp(velocity.y + GRAVITY * Time.fixedDeltaTime, -Mathf.Infinity, -TERMINAL_VELOCITY);
                    if (disFloor < Mathf.Abs(velocity.y) && velocity.y > 0)
                    {
                        transform.position += (disFloor - PlayState.FRAC_32) * Vector3.up;
                        velocity.y = 0;
                        grounded = true;
                    }
                }
                if (velocity.y < 0)
                {
                    disCeil = GetDistance(PlayState.EDirsSurface.Floor);
                    if (disCeil < Mathf.Abs(velocity.y))
                    {
                        transform.position += (disCeil - PlayState.FRAC_32) * Vector3.down;
                        velocity.y = 0;
                    }
                }
                facingLeft = GetInput(Inputs.Right) <= 0 && (GetInput(Inputs.Left) > 0 || facingLeft);
                velocity.x = RUN_SPEED * Time.fixedDeltaTime * (0 + (GetInput(Inputs.Right) > 1 ? 1 : 0) - (GetInput(Inputs.Left) > 1 ? 1 : 0));
                if (velocity.x != 0)
                {
                    facingLeft = velocity.x < 0;
                    disWalls = GetDistance(facingLeft ? PlayState.EDirsSurface.WallL : PlayState.EDirsSurface.WallR);
                    if (disWalls < Mathf.Abs(velocity.x))
                        velocity.x = (disWalls - PlayState.FRAC_32) * (facingLeft ? -1 : 1);
                }
                break;
        }
        transform.position += (Vector3)velocity;

        bool onWall = gravity == PlayState.EDirsSurface.WallL || gravity == PlayState.EDirsSurface.WallR;
        string expectedAnim = "Boss_moonSnail";
        if (animData[4] == 1 && weaponTimeout == weaponTimeouts[currentWeapon])
            expectedAnim += "_shoot";
        else if (animData[3] == 1 && !grounded)
            expectedAnim += "_jump";
        else
        {
            if (animData[2] == 1 && (onWall ? velocity.y : velocity.x) != 0)
                expectedAnim += "_move";
            else
                expectedAnim += "_idle";
        }
        expectedAnim += (attackPhase + 1).ToString();
        expectedAnim += gravity switch
        {
            PlayState.EDirsSurface.Floor => "_floor_",
            PlayState.EDirsSurface.WallL => "_wallL_",
            PlayState.EDirsSurface.WallR => "_wallR_",
            _ => "_ceiling_"
        };
        if (onWall)
            expectedAnim += facingDown ? "D" : "U";
        else
            expectedAnim += facingLeft ? "L" : "R";
        if (anim.currentAnimName != expectedAnim)
            anim.Play(expectedAnim);
        sprite.flipX = facingLeft && animData[0] == 1;
        sprite.flipY = !facingDown && animData[1] == 1;

        if (health <= maxHealth * 0.5f && attackPhase < 1)
        {
            speed += 0.3f;
            attackPhase = 1;
            anim.Play(anim.currentAnimName.Replace('1', '2'));
        }

        IncrementInputFrames();
    }

    private void SetGravity(PlayState.EDirsSurface direction, bool unground = false)
    {
        PlayState.EDirsSurface originalGravity = gravity;
        Vector3 gravVector = gravity switch
        {
            PlayState.EDirsSurface.Floor => Vector3.up,
            PlayState.EDirsSurface.WallL => Vector3.right,
            PlayState.EDirsSurface.WallR => Vector3.left,
            _ => Vector3.down
        };
        gravity = direction;
        float wallEject = (boxSize.x > boxSize.y) ? boxSize.x - boxSize.y : boxSize.y - boxSize.x;
        if (GetDistance(originalGravity) < wallEject)
            transform.position += wallEject * gravVector;
        switch (gravity)
        {
            default:
            case PlayState.EDirsSurface.Floor:
                box.size = boxSize;
                facingDown = true;
                break;
            case PlayState.EDirsSurface.WallL:
                box.size = new Vector2(boxSize.y, boxSize.x);
                facingLeft = true;
                break;
            case PlayState.EDirsSurface.WallR:
                box.size = new Vector2(boxSize.y, boxSize.x);
                facingLeft = false;
                break;
            case PlayState.EDirsSurface.Ceiling:
                box.size = boxSize;
                facingDown = false;
                break;
        }
        if (unground)
            grounded = false;
    }

    private void Jump()
    {
        PlayState.PlaySound("Jump");
        GravityJump();
        grounded = false;
    }

    private void GravityJump()
    {
        switch (gravity)
        {
            case PlayState.EDirsSurface.Floor:
                velocity.y = JUMP_POWER * Time.fixedDeltaTime;
                break;
            case PlayState.EDirsSurface.WallL:
                velocity.x = JUMP_POWER * Time.fixedDeltaTime;
                break;
            case PlayState.EDirsSurface.WallR:
                velocity.x = -JUMP_POWER * Time.fixedDeltaTime;
                break;
            case PlayState.EDirsSurface.Ceiling:
                velocity.y = -JUMP_POWER * Time.fixedDeltaTime;
                break;
        }
    }

    private void Attack()
    {
        if (!isAttacking)
            return;
        weaponTimeout -= Time.fixedDeltaTime;
        if (weaponTimeout > 0)
            return;
        weaponTimeout = weaponTimeouts[currentWeapon];

        Vector2 inputDir = new(0 + (GetInput(Inputs.Right) > 1 ? 1 : 0) - (GetInput(Inputs.Left) > 1 ? 1 : 0),
            0 + (GetInput(Inputs.Up) > 1 ? 1 : 0) - (GetInput(Inputs.Down) > 1 ? 1 : 0));
        int type = currentWeapon;
        int dir = 0;
        switch (inputDir.x + "" + inputDir.y)
        {
            case "-11":
                dir = 0;
                break;
            case "01":
                dir = 1;
                break;
            case "11":
                dir = 2;
                break;
            case "-10":
                dir = 3;
                break;
            case "10":
                dir = 4;
                break;
            case "-1-1":
                dir = 5;
                break;
            case "0-1":
                dir = 6;
                break;
            case "1-1":
                dir = 7;
                break;
            case "00":
                dir = -1;
                break;
        }

        if (type == 1 && grounded)
        {
            if (gravity == PlayState.EDirsSurface.Floor && (dir == 5 || dir == 6 || dir == 7))
                dir = facingLeft ? 3 : 4;
            else if (gravity== PlayState.EDirsSurface.WallL && (dir == 0 || dir == 3 || dir == 5))
                dir = facingDown ? 6 : 1;
            else if (gravity == PlayState.EDirsSurface.WallR && (dir == 2 || dir == 4 || dir == 7))
                dir = facingDown ? 6 : 1;
            else if (gravity == PlayState.EDirsSurface.Ceiling && (dir == 0 || dir == 1 || dir == 2))
                dir = facingLeft ? 3 : 4;
        }
        if (dir == -1)
        {
            if (gravity == PlayState.EDirsSurface.Floor && dir == -1)
                dir = facingLeft ? 3 : 4;
            else if (gravity == PlayState.EDirsSurface.WallL && dir == -1)
                dir = facingDown ? 6 : 1;
            else if (gravity == PlayState.EDirsSurface.WallR && dir == -1)
                dir = facingDown ? 6 : 1;
            else if (gravity == PlayState.EDirsSurface.Ceiling && dir == -1)
                dir = facingLeft ? 3 : 4;
        }
        Vector2 fireAngle = dir switch
        {
            0 => new(-PlayState.ANGLE_DIAG.x, PlayState.ANGLE_DIAG.y),
            1 => Vector2.up,
            2 => PlayState.ANGLE_DIAG,
            3 => Vector2.left,
            4 => Vector2.right,
            5 => -PlayState.ANGLE_DIAG,
            6 => Vector2.down,
            7 => new(PlayState.ANGLE_DIAG.x, -PlayState.ANGLE_DIAG.y),
            _ => Vector2.right
        };
        switch (currentWeapon)
        {
            case 0:
                PlayState.ShootEnemyBullet(transform.position, EnemyBullet.BulletType.bigPea, new float[] { weaponSpeeds[0], fireAngle.x, fireAngle.y });
                break;
            case 1:
                PlayState.ShootEnemyBullet(transform.position, EnemyBullet.BulletType.boomRed, new float[] { weaponSpeeds[1], fireAngle.x, fireAngle.y });
                break;
            default:
            case 2:
                PlayState.ShootEnemyBullet(transform.position, EnemyBullet.BulletType.shadowWave, new float[] { weaponSpeeds[2], fireAngle.x, fireAngle.y });
                break;
        }
    }

    private void PerformGravityJump()
    {
        switch (gravity)
        {
            default:
            case PlayState.EDirsSurface.Floor:
                if (((GetInput(Inputs.Left) < 1 && GetInput(Inputs.Right) < 1) || mostRecentDir == PlayState.EDirsSurface.Ceiling) && GetInput(Inputs.Up) > 0)
                {
                    SetGravity(PlayState.EDirsSurface.Ceiling, true);
                }
                else if (((GetInput(Inputs.Down) < 1 && GetInput(Inputs.Up) < 1) || mostRecentDir == PlayState.EDirsSurface.WallR) && GetInput(Inputs.Right) > 0)
                {
                    SetGravity(PlayState.EDirsSurface.WallR, true);
                }
                else if (((GetInput(Inputs.Down) < 1 && GetInput(Inputs.Up) < 1) || mostRecentDir == PlayState.EDirsSurface.WallL) && GetInput(Inputs.Left) > 0)
                {
                    SetGravity(PlayState.EDirsSurface.WallL, true);
                }
                break;
            case PlayState.EDirsSurface.WallL:
                if (((GetInput(Inputs.Down) < 1 && GetInput(Inputs.Up) < 1) || mostRecentDir == PlayState.EDirsSurface.WallR) && GetInput(Inputs.Right) > 0)
                {
                    SetGravity(PlayState.EDirsSurface.WallR, true);
                }
                else if (((GetInput(Inputs.Left) < 1 && GetInput(Inputs.Right) < 1) || mostRecentDir == PlayState.EDirsSurface.Ceiling) && GetInput(Inputs.Up) > 0)
                {
                    SetGravity(PlayState.EDirsSurface.Ceiling, true);
                }
                else if (((GetInput(Inputs.Left) < 1 && GetInput(Inputs.Right) < 1) || mostRecentDir == PlayState.EDirsSurface.Floor) && GetInput(Inputs.Down) > 0)
                {
                    SetGravity(PlayState.EDirsSurface.Floor, true);
                }
                break;
            case PlayState.EDirsSurface.WallR:
                if (((GetInput(Inputs.Down) < 1 && GetInput(Inputs.Up) < 1) || mostRecentDir == PlayState.EDirsSurface.WallL) && GetInput(Inputs.Left) > 0)
                {
                    SetGravity(PlayState.EDirsSurface.WallL, true);
                }
                else if (((GetInput(Inputs.Left) < 1 && GetInput(Inputs.Right) < 1) || mostRecentDir == PlayState.EDirsSurface.Ceiling) && GetInput(Inputs.Up) > 0)
                {
                    SetGravity(PlayState.EDirsSurface.Ceiling, true);
                }
                else if (((GetInput(Inputs.Left) < 1 && GetInput(Inputs.Right) < 1) || mostRecentDir == PlayState.EDirsSurface.Floor) && GetInput(Inputs.Down) > 0)
                {
                    SetGravity(PlayState.EDirsSurface.Floor, true);
                }
                break;
            case PlayState.EDirsSurface.Ceiling:
                if (((GetInput(Inputs.Left) < 1 && GetInput(Inputs.Right) < 1) || mostRecentDir == PlayState.EDirsSurface.Floor) && GetInput(Inputs.Down) > 0)
                {
                    SetGravity(PlayState.EDirsSurface.Floor, true);
                }
                else if (((GetInput(Inputs.Down) < 1 && GetInput(Inputs.Up) < 1) || mostRecentDir == PlayState.EDirsSurface.WallR) && GetInput(Inputs.Right) > 0)
                {
                    SetGravity(PlayState.EDirsSurface.WallR, true);
                }
                else if (((GetInput(Inputs.Down) < 1 && GetInput(Inputs.Up) < 1) || mostRecentDir == PlayState.EDirsSurface.WallL) && GetInput(Inputs.Left) > 0)
                {
                    SetGravity(PlayState.EDirsSurface.WallL, true);
                }
                break;
        }
    }

    private float GetDecision()
    {
        decisionTableIndex = ++decisionTableIndex % decisionTable.Length;
        return decisionTable[decisionTableIndex];
    }

    private float NormalizedSigmoid(float input)
    {
        return 1 / (1 + Mathf.Exp(-(input * 8 - 4)));
    }

    private void CollectTargetPoints()
    {
        for (int i = 0; i < PlayState.activeTargets.Count; i++)
        {
            if (PlayState.activeTargets[i].type == PlayState.TargetTypes.MoonTele)
                telePoints.Add(PlayState.activeTargets[i]);
            if (PlayState.activeTargets[i].type == PlayState.TargetTypes.MoonMove)
                movePoints.Add(PlayState.activeTargets[i]);
            if (PlayState.activeTargets[i].type == PlayState.TargetTypes.GigaSpawn)
                gigaSpawnPoint = PlayState.activeTargets[i];
        }
    }

    private PlayState.EDirsSurface CompassToSurface(PlayState.EDirsCompass input)
    {
        return input switch
        {
            PlayState.EDirsCompass.N => PlayState.EDirsSurface.Ceiling,
            PlayState.EDirsCompass.W => PlayState.EDirsSurface.WallL,
            PlayState.EDirsCompass.E => PlayState.EDirsSurface.WallR,
            _ => PlayState.EDirsSurface.Floor
        };
    }

    private float GetDistance(PlayState.EDirsSurface direction)
    {
        Vector2 halfBox = box.size * 0.5f;
        Vector2 ul = new(transform.position.x - halfBox.x, transform.position.y + halfBox.y);
        Vector2 dl = (Vector2)transform.position - halfBox;
        Vector2 dr = new(transform.position.x + halfBox.x, transform.position.y - halfBox.y);
        Vector2 ur = (Vector2)transform.position + halfBox;

        return direction switch
        {
            PlayState.EDirsSurface.Floor => PlayState.GetDistance(direction, dl, dr, CAST_COUNT, enemyCollide),
            PlayState.EDirsSurface.WallL => PlayState.GetDistance(direction, dl, ul, CAST_COUNT, enemyCollide),
            PlayState.EDirsSurface.WallR => PlayState.GetDistance(direction, dr, ur, CAST_COUNT, enemyCollide),
            _ => PlayState.GetDistance(direction, ul, ur, CAST_COUNT, enemyCollide)
        };
    }

    public override void Kill()
    {
        PlayState.globalFunctions.RequestQueuedExplosion(transform.position, 2.7f, 0, true);
        PlayState.ToggleBossfightState(false, -1);
        PlayState.TogglableHUDElements[0].SetActive(false);
        foreach (Transform bullet in PlayState.enemyBulletPool.transform)
            bullet.GetComponent<EnemyBullet>().Despawn();
        Instantiate(Resources.Load<GameObject>("Objects/Enemies/Bosses/Giga Snail"), gigaSpawnPoint.pos, Quaternion.identity, transform.parent);
        Destroy(gameObject);
    }
}
