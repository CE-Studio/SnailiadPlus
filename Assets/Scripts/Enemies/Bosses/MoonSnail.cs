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

    private float donutTimeout = 0;
    private float releaseJumpTimeout;
    private int attackMode;
    private float modeElapsed;
    private float speed = 1f;
    private bool modeInitialized;
    private float actionTimeout;
    private float weaponTimeout;
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
    private bool isJumping = false;
    private bool grounded = false;
    private bool shelled = false;
    private int[] virtualInputs = new int[] { };
    private bool[] tappedInputs = new bool[] { };
    private int fallFrames;
    private float elapsed;
    private bool isAttacking = false;
    private Vector2 velocity;
    private bool justHitHeadOrWall;

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

            col.TryGetComponent(out box);
            boxSize = box.size;
        }
        else
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
        int pointID = Mathf.FloorToInt(GetDecision() * movePoints.Count);
        moveStart = transform.position;
        moveEnd = movePoints[pointID].pos;
        targetGravity = CompassToSurface(movePoints[pointID].directions[0]);
    }
    private void PickTeleTarget()
    {
        int pointID = Mathf.FloorToInt(GetDecision() * telePoints.Count);
        moveStart = transform.position;
        moveEnd = telePoints[pointID].pos;
        targetGravity = CompassToSurface(telePoints[pointID].directions[0]);
    }

    private void PressInput(Inputs input, bool tapped = false)
    {
        virtualInputs[(int)input] = 1;
        if (tapped)
            tappedInputs[(int)input] = true;
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

    private bool CheckForFreshInput(Inputs input)
    {
        return virtualInputs[(int)input] == 1;
    }

    private void UpdateAIMove()
    {
        if (!modeInitialized)
        {
            modeInitialized = true;
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
                if (animData[4] == 1)
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
            shadowTheta = PlayState.TAU * NormalizedSigmoid(modeElapsed / TELEPORT_TIME * 2);
        }
        for (int i = 0; i < shadowBalls.Count; i++)
        {
            shadowBalls[i].transform.position = new Vector2(
                teleStart.x * (1 - teleElapsed) + teleEnd.x * teleElapsed + Mathf.Cos(shadowTheta + PlayState.TAU / shadowBalls.Count * i) * shadowRadius,
                teleStart.y * (1 - teleElapsed) + teleEnd.y * teleElapsed + Mathf.Cos(shadowTheta + PlayState.TAU / shadowBalls.Count * i) * shadowRadius
                );
        }
        velocity = Vector2.zero;
        transform.position = teleEnd;
        ReleaseAllInputs();
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
                    if (distanceX < AIM_DETECT_RADIUS_HORIZONTAL && distanceY > AIM_DETECT_RADIUS_VERTICAL)
                        PressInput(Inputs.Up);
                    else
                        ReleaseInput(Inputs.Up);
                    break;
                case PlayState.EDirsSurface.WallL:
                    if (distanceY < AIM_DETECT_RADIUS_HORIZONTAL && distanceX > AIM_DETECT_RADIUS_VERTICAL)
                        PressInput(Inputs.Right);
                    else
                        ReleaseInput(Inputs.Right);
                    break;
                case PlayState.EDirsSurface.WallR:
                    if (distanceY < AIM_DETECT_RADIUS_HORIZONTAL && distanceX > AIM_DETECT_RADIUS_VERTICAL)
                        PressInput(Inputs.Left);
                    else
                        ReleaseInput(Inputs.Left);
                    break;
                case PlayState.EDirsSurface.Ceiling:
                    if (distanceX < AIM_DETECT_RADIUS_HORIZONTAL && distanceY > AIM_DETECT_RADIUS_VERTICAL)
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
        donutTimeout -= Time.fixedDeltaTime * speed;
        UntapInputs();
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
        if (releaseJumpTimeout <= 0 && virtualInputs[(int)Inputs.Jump] > 0)
        {
            ReleaseInput(Inputs.Jump);
            if (gravJumpDir != PlayState.EDirsSurface.None)
            {
                switch (gravJumpDir)
                {
                    default:
                    case PlayState.EDirsSurface.Floor:
                        PressInput(Inputs.Down, true);
                        AIJump(PlayState.EDirsSurface.None, 0.1f);
                        break;
                    case PlayState.EDirsSurface.WallL:
                        PressInput(Inputs.Left, true);
                        AIJump(PlayState.EDirsSurface.None, 0.1f);
                        break;
                    case PlayState.EDirsSurface.WallR:
                        PressInput(Inputs.Right, true);
                        AIJump(PlayState.EDirsSurface.None, 0.1f);
                        break;
                    case PlayState.EDirsSurface.Ceiling:
                        PressInput(Inputs.Up, true);
                        AIJump(PlayState.EDirsSurface.None, 0.1f);
                        break;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (!PlayState.paralyzed)
            UpdateAI();
        elapsed += Time.fixedDeltaTime;
        FixGravity();
        if (CheckForFreshInput(Inputs.Jump) && isJumping)
            PerformGravityJump();
        //CheckMoveInputs();
        if (CheckForFreshInput(Inputs.Jump))
        {
            //if (shelled)
            //    ToggleShell(false);
            if (!isJumping)
                Jump();
        }
        Attack();
        IncrementInputFrames();

        if (health <= maxHealth * 0.5f && attackPhase < 1)
        {
            speed *= 0.3f;
            attackPhase = 1;
            anim.Play(anim.currentAnimName.Replace('1', '2'));
        }
    }

    private void SetGravity(PlayState.EDirsSurface direction)
    {
        gravity = direction;
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
    }

    private void FixGravity()
    {
        switch (gravity)
        {
            default:
            case PlayState.EDirsSurface.Floor:
                if (!isJumping && velocity.y < 0 && CheckForFreshInput(Inputs.Down) && !justHitHeadOrWall)
                {
                    if (!facingLeft && CheckForFreshInput(Inputs.Right))
                    {
                        facingLeft = true;
                        velocity.x = -RUN_SPEED;
                    }
                    else if (facingLeft && CheckForFreshInput(Inputs.Left))
                    {
                        facingLeft = false;
                        velocity.x = RUN_SPEED;
                    }
                    break;
                }
                isJumping = velocity.y != 0;
                if (isJumping)
                    fallFrames++;
                else
                    fallFrames = 0;
                break;
            case PlayState.EDirsSurface.WallL:
                if (!isJumping && velocity.x < 0 && CheckForFreshInput(Inputs.Left) && !justHitHeadOrWall)
                {
                    if (!facingDown && CheckForFreshInput(Inputs.Up))
                    {
                        facingDown = true;
                        velocity.y = -RUN_SPEED;
                    }
                    else if (facingDown && CheckForFreshInput(Inputs.Down))
                    {
                        facingDown = false;
                        velocity.y = RUN_SPEED;
                    }
                    break;
                }
                isJumping = velocity.x != 0;
                if (isJumping)
                    fallFrames++;
                else
                    fallFrames = 0;
                break;
            case PlayState.EDirsSurface.WallR:
                if (!isJumping && velocity.x > 0 && CheckForFreshInput(Inputs.Right) && !justHitHeadOrWall)
                {
                    if (!facingDown && CheckForFreshInput(Inputs.Up))
                    {
                        facingDown = true;
                        velocity.y = -RUN_SPEED;
                    }
                    else if (facingDown && CheckForFreshInput(Inputs.Down))
                    {
                        facingDown = false;
                        velocity.y = RUN_SPEED;
                    }
                    break;
                }
                isJumping = velocity.x != 0;
                if (isJumping)
                    fallFrames++;
                else
                    fallFrames = 0;
                break;
            case PlayState.EDirsSurface.Ceiling:
                if (!isJumping && velocity.y > 0 && CheckForFreshInput(Inputs.Up) && !justHitHeadOrWall)
                {
                    if (!facingLeft && CheckForFreshInput(Inputs.Right))
                    {
                        facingLeft = true;
                        velocity.x = -RUN_SPEED;
                    }
                    else if (facingLeft && CheckForFreshInput(Inputs.Left))
                    {
                        facingLeft = false;
                        velocity.x = RUN_SPEED;
                    }
                    break;
                }
                isJumping = velocity.y != 0;
                if (isJumping)
                    fallFrames++;
                else
                    fallFrames = 0;
                break;
        }
    }

    private void Jump()
    {
        PlayState.PlaySound("Jump");
        GravityJump();
    }

    private void GravityJump()
    {
        switch (gravity)
        {
            case PlayState.EDirsSurface.Floor:
                velocity.y = -JUMP_POWER;
                break;
            case PlayState.EDirsSurface.WallL:
                velocity.x = JUMP_POWER;
                break;
            case PlayState.EDirsSurface.WallR:
                velocity.x = -JUMP_POWER;
                break;
            case PlayState.EDirsSurface.Ceiling:
                velocity.y = JUMP_POWER;
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

        Vector2 inputDir = new(0 + (virtualInputs[(int)Inputs.Right] > 0 ? 1 : 0) - (virtualInputs[(int)Inputs.Left] > 0 ? 1 : 0),
            0 + (virtualInputs[(int)Inputs.Up] > 0 ? 1 : 0) - (virtualInputs[(int)Inputs.Down] > 0 ? 1 : 0));
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
}
