using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceBox : Boss
{
    private const int MAX_BOXES = 8;
    private const int SHIELD_SLOTS = 36;
    private const int MAX_ACTIVE_SHIELDS = 26;
    private const int STARTING_SHIELDS = 0;
    private const int DAMAGE_TAKEN_FOR_SHIELD = 100;
    private const int SHIELD_PERIOD = 4;
    private const float MODE_TIMEOUT = 0.6f;
    private const float SPAWN_TIMEOUT = 2.5f;
    private const float STARTING_SPEED = 1f;
    private const int SPAWN_COUNTER = 8;
    private const int CAST_COUNT = 9;
    private const float SHAKE_STRENGTH = 0.6f;
    private const float SHAKE_TIME = 0.8f;

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
    private BossMode mode = BossMode.Wait;
    private BossMode lastMode = BossMode.Wait;
    private readonly BossMode[] introSteps = new BossMode[] { BossMode.Up, BossMode.Left };
    private int introStepID = 0;
    private bool introMovementsDone = false;
    private bool haltedIntroPlayerMovement = false;

    private Vector2 cornerUL;
    private Vector2 cornerUR;
    private Vector2 cornerDL;
    private Vector2 cornerDR;

    private enum ShootMode
    {
        Attack,
        WaitCluster
    };
    private ShootMode shootMode;

    public List<SpaceBoxShield> shields = new();
    public List<SpaceBoxBabybox> babyboxes = new();

    private float modeTimeout = MODE_TIMEOUT;
    private int spawnCounter = SPAWN_COUNTER;
    public int attackMode = 0;
    private float speed = STARTING_SPEED;
    private float acceleration = 0.21875f;
    private int decisionTableIndex = 1;
    private int SHOT_COUNT = 4;
    private float SHOT_TIMEOUT = 0.6f;
    private float CLUSTER_TIMEOUT = 4.1f;
    private int shotCount;
    private float shotTimeout;
    private float clusterTimeout;
    private Vector2 velocity = Vector2.zero;
    private string dirString = "";
    private float elapsed = 0;
    private readonly bool legacyCutscene = true;
    private readonly bool spawnAtCorner = true;
    private Vector2 shieldRange;
    private int activeShields = 0;
    private Vector2 halfBox;

    public GameObject shieldObj;
    public GameObject babyboxObj;

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (PlayState.IsBossAlive(2))
        {
            SpawnBoss(5100, 4, 9, true, 2, true);
            StartCoroutine(RunIntro(true, false));
            PlayState.playerScript.CorrectGravity(true);

            SHOT_COUNT = 4;
            if (PlayState.currentProfile.difficulty == 2)
            {
                SHOT_COUNT = 6;
                speed += 0.2f;
            }
            shotCount = SHOT_COUNT;
            shotTimeout = SHOT_TIMEOUT;
            clusterTimeout = CLUSTER_TIMEOUT;

            string[] bossAnimTypes = new string[] { "idle#", "charge#_$", "hit#_$", "waitSpawn#" };
            for (int i = 0; i < 2; i++)
            {
                foreach (string animType in bossAnimTypes)
                {
                    string formattedAnimType = animType.Replace("#", i.ToString());
                    if (formattedAnimType.Contains("$"))
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            string furtherFormattedAnimType = formattedAnimType.Replace("$", PlayState.DIRS_COMPASS[j]);
                            anim.Add("Boss_spaceBox_" + furtherFormattedAnimType);
                        }
                    }
                    else
                        anim.Add("Boss_spaceBox_" + formattedAnimType);
                }
            }

            float childCount = 0;
            while (childCount < SHIELD_SLOTS)
            {
                GameObject newShield = Instantiate(shieldObj, transform);
                SpaceBoxShield shieldComponent = newShield.GetComponent<SpaceBoxShield>();
                shields.Add(shieldComponent);
                shieldComponent.parentBoss = this;
                childCount++;
            }
            for (int i = 0; i < STARTING_SHIELDS; i++)
                shields[i].SetActive(true);

            col.TryGetComponent(out BoxCollider2D box);
            halfBox = box.size * 0.5f;
            shieldRange = halfBox + new Vector2(0.55f, 0.55f);

            if (spawnAtCorner)
                transform.position += new Vector3(3.5f, -3.5f, 0);

            PlayState.globalFunctions.CreateLightMask(23, transform.position).transform.parent = transform;
        }
        else
            Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (legacyCutscene && !introMovementsDone)
        {
            if (PlayState.currentProfile.difficulty == 2)
            {
                if (elapsed > 2.2f && elapsed < 2.9f)
                    Control.SetVirtual(Control.Keyboard.Right1, true);
                else if (elapsed >= 2.9f)
                    PlayState.paralyzed = false;
            }
            else
            {
                if (elapsed > 2.7f && elapsed < 3.4f)
                    Control.SetVirtual(Control.Keyboard.Right1, true);
                else if (elapsed >= 3.4f)
                    PlayState.paralyzed = false;
            }
        }
        else if (introMovementsDone && !haltedIntroPlayerMovement)
        {
            Control.ClearVirtual(true, true);
            haltedIntroPlayerMovement = true;
        }
        elapsed += Time.fixedDeltaTime;

        CheckMode();
        CheckShoot();
        AddNewShields();
        UpdateShieldPositions();

        float distance = GetDistance(mode, out float horizDis, out float vertDis);
        bool stopMoving = false;
        byte collisionTracker = 0;
        switch (mode)
        {
            default:
                velocity = Vector2.zero;
                break;
            case BossMode.Up:
                velocity.y += acceleration * speed * Time.fixedDeltaTime;
                if (distance < velocity.y)
                {
                    velocity.y = distance - PlayState.FRAC_32;
                    stopMoving = true;
                }
                break;
            case BossMode.Upright:
                velocity.x += acceleration * speed * Time.fixedDeltaTime;
                velocity.y += acceleration * speed * Time.fixedDeltaTime;
                if (distance < velocity.x)
                    collisionTracker += 1;
                if (distance < velocity.y)
                    collisionTracker += 2;
                switch (collisionTracker)
                {
                    default:
                        break;
                    case 1:
                    case 2:
                        velocity.x = distance - PlayState.FRAC_32;
                        velocity.y = distance - PlayState.FRAC_32;
                        stopMoving = true;
                        break;
                    case 3:
                        if (horizDis < vertDis)
                        {
                            velocity.x = horizDis - PlayState.FRAC_32;
                            velocity.y = horizDis - PlayState.FRAC_32;
                        }
                        else
                        {
                            velocity.x = vertDis - PlayState.FRAC_32;
                            velocity.y = vertDis - PlayState.FRAC_32;
                        }
                        stopMoving = true;
                        break;
                };
                break;
            case BossMode.Right:
                velocity.x += acceleration * speed * Time.fixedDeltaTime;
                if (distance < velocity.x)
                {
                    velocity.x = distance - PlayState.FRAC_32;
                    stopMoving = true;
                }
                break;
            case BossMode.Downright:
                velocity.x += acceleration * speed * Time.fixedDeltaTime;
                velocity.y -= acceleration * speed * Time.fixedDeltaTime;
                if (distance < velocity.x)
                    collisionTracker += 1;
                if (distance < -velocity.y)
                    collisionTracker += 2;
                switch (collisionTracker)
                {
                    default:
                        break;
                    case 1:
                    case 2:
                        velocity.x = distance - PlayState.FRAC_32;
                        velocity.y = -distance + PlayState.FRAC_32;
                        stopMoving = true;
                        break;
                    case 3:
                        if (horizDis < vertDis)
                        {
                            velocity.x = horizDis - PlayState.FRAC_32;
                            velocity.y = -horizDis + PlayState.FRAC_32;
                        }
                        else
                        {
                            velocity.x = vertDis - PlayState.FRAC_32;
                            velocity.y = -vertDis + PlayState.FRAC_32;
                        }
                        stopMoving = true;
                        break;
                };
                break;
            case BossMode.Down:
                velocity.y -= acceleration * speed * Time.fixedDeltaTime;
                if (distance < -velocity.y)
                {
                    velocity.y = -distance + PlayState.FRAC_32;
                    stopMoving = true;
                }
                break;
            case BossMode.Downleft:
                velocity.x -= acceleration * speed * Time.fixedDeltaTime;
                velocity.y -= acceleration * speed * Time.fixedDeltaTime;
                if (distance < -velocity.x)
                    collisionTracker += 1;
                if (distance < -velocity.y)
                    collisionTracker += 2;
                switch (collisionTracker)
                {
                    default:
                        break;
                    case 1:
                    case 2:
                        velocity.x = -distance + PlayState.FRAC_32;
                        velocity.y = -distance + PlayState.FRAC_32;
                        stopMoving = true;
                        break;
                    case 3:
                        if (horizDis < vertDis)
                        {
                            velocity.x = -horizDis + PlayState.FRAC_32;
                            velocity.y = -horizDis + PlayState.FRAC_32;
                        }
                        else
                        {
                            velocity.x = -vertDis + PlayState.FRAC_32;
                            velocity.y = -vertDis + PlayState.FRAC_32;
                        }
                        stopMoving = true;
                        break;
                };
                break;
            case BossMode.Left:
                velocity.x -= acceleration * speed * Time.fixedDeltaTime;
                if (distance < -velocity.x)
                {
                    velocity.x = -distance + PlayState.FRAC_32;
                    stopMoving = true;
                }
                break;
            case BossMode.Upleft:
                velocity.x -= acceleration * speed * Time.fixedDeltaTime;
                velocity.y += acceleration * speed * Time.fixedDeltaTime;
                if (distance < -velocity.x)
                    collisionTracker += 1;
                if (distance < velocity.y)
                    collisionTracker += 2;
                switch (collisionTracker)
                {
                    default:
                        break;
                    case 1:
                    case 2:
                        velocity.x = -distance + PlayState.FRAC_32;
                        velocity.y = distance - PlayState.FRAC_32;
                        stopMoving = true;
                        break;
                    case 3:
                        if (horizDis < vertDis)
                        {
                            velocity.x = -horizDis + PlayState.FRAC_32;
                            velocity.y = horizDis - PlayState.FRAC_32;
                        }
                        else
                        {
                            velocity.x = -vertDis + PlayState.FRAC_32;
                            velocity.y = vertDis - PlayState.FRAC_32;
                        }
                        stopMoving = true;
                        break;
                };
                break;
        }
        transform.position += (Vector3)velocity;
        if (stopMoving)
        {
            anim.Play("Boss_spaceBox_hit" + attackMode.ToString() + "_" + dirString);
            Stomp();
        }

        if (health <= maxHealth * 0.3f && attackMode < 1)
        {
            speed += 0.5f;
            attackMode = 1;
            foreach (SpaceBoxShield shield in shields)
                shield.TurnBlue();
            foreach (SpaceBoxBabybox babybox in babyboxes)
                babybox.TurnBlue();
            SHOT_COUNT = 6;
            CLUSTER_TIMEOUT = 4.1f;
            SHOT_TIMEOUT = 0.2f;
            anim.Play(anim.currentAnimName.Replace("0", "1"));
        }

        if (!anim.isPlaying)
            anim.Play("Boss_spaceBox_idle" + attackMode.ToString());
    }

    private float GetDecision()
    {
        decisionTableIndex = (decisionTableIndex + 1) % decisionTable.Length;
        return decisionTable[decisionTableIndex];
    }

    private void CheckMode()
    {
        modeTimeout -= Time.deltaTime * speed;
        if (modeTimeout <= 0)
        {
            if (mode == BossMode.Wait && !introMovementsDone)
            {
                if (introStepID < introSteps.Length)
                {
                    Charge(introSteps[introStepID]);
                    introStepID++;
                }
                else
                    introMovementsDone = true;
            }
            else if (mode == BossMode.Wait && introMovementsDone)
            {
                spawnCounter--;
                if (spawnCounter <= 0)
                {
                    spawnCounter = SPAWN_COUNTER;
                    MakeBoxes();
                }
                else
                {
                    float decisionValue = GetDecision();
                    switch (lastMode)
                    {
                        case BossMode.Intro:
                            Charge(BossMode.Up);
                            break;
                        case BossMode.Left:
                        case BossMode.Right:
                            if (PlayState.currentProfile.difficulty == 2)
                            {
                                if (decisionValue < 0.4f)
                                    ChargeVert();
                                else if (decisionValue < 0.75f)
                                    ChargeDiag();
                                else
                                    ChargeHoriz();
                            }
                            else
                            {
                                if (decisionValue < 0.75f)
                                    ChargeVert();
                                else
                                    ChargeHoriz();
                            }
                            break;
                        case BossMode.Up:
                        case BossMode.Down:
                            if (PlayState.currentProfile.difficulty == 2)
                            {
                                if (decisionValue < 0.4f)
                                    ChargeHoriz();
                                else if (decisionValue < 0.75f)
                                    ChargeDiag();
                                else
                                    ChargeVert();
                            }
                            else
                            {
                                if (decisionValue < 0.75f)
                                    ChargeHoriz();
                                else
                                    ChargeVert();
                            }
                            break;
                        case BossMode.Upleft:
                        case BossMode.Upright:
                        case BossMode.Downleft:
                        case BossMode.Downright:
                            if (decisionValue < 0.4f)
                                ChargeVert();
                            else if (decisionValue < 0.8f)
                                ChargeHoriz();
                            else
                                ChargeDiag();
                            break;
                    }
                }
            }
        }
    }

    private void CheckShoot()
    {
        if (maxHealth - health < 1500)
            return;

        switch (shootMode)
        {
            case ShootMode.WaitCluster:
                clusterTimeout -= Time.deltaTime * speed;
                if (clusterTimeout <= 0)
                {
                    shootMode = ShootMode.Attack;
                    shotCount = SHOT_COUNT;
                    shotTimeout = 0;
                }
                break;
            case ShootMode.Attack:
                shotTimeout -= Time.deltaTime * speed;
                if (shotTimeout <= 0)
                {
                    shotCount--;
                    if (shotCount <= 0)
                    {
                        shootMode = ShootMode.WaitCluster;
                        clusterTimeout = CLUSTER_TIMEOUT;
                    }
                    shotTimeout = SHOT_TIMEOUT;
                    Shoot();
                }
                break;
        }
    }

    private void AddNewShields()
    {
        int targetShieldCount = (maxHealth - health) / DAMAGE_TAKEN_FOR_SHIELD;
        if (targetShieldCount > MAX_ACTIVE_SHIELDS)
            targetShieldCount = MAX_ACTIVE_SHIELDS;
        while (targetShieldCount > activeShields)
        {
            shields[activeShields++].SetActive(true);
        }
    }

    private void UpdateShieldPositions()
    {
        int shieldID = 0;
        float timeValue;
        float moddedTime;
        while (shieldID < MAX_ACTIVE_SHIELDS)
        {
            timeValue = (elapsed / SHIELD_PERIOD % 1f * SHIELD_SLOTS + 17f * (shieldID + 8) % MAX_ACTIVE_SHIELDS) % SHIELD_SLOTS;
            moddedTime = timeValue % 9;
            if (timeValue < 9f)
            {
                shields[shieldID].transform.position = new Vector2(
                    transform.position.x - shieldRange.x + moddedTime,
                    transform.position.y + shieldRange.y
                    );
            }
            else if (timeValue < 18f)
            {
                shields[shieldID].transform.position = new Vector2(
                    transform.position.x + shieldRange.x,
                    transform.position.y + shieldRange.y - moddedTime
                    );
            }
            else if (timeValue < 27f)
            {
                shields[shieldID].transform.position = new Vector2(
                    transform.position.x + shieldRange.x - moddedTime,
                    transform.position.y - shieldRange.y
                    );
            }
            else
            {
                shields[shieldID].transform.position = new Vector2(
                    transform.position.x - shieldRange.x,
                    transform.position.y - shieldRange.y + moddedTime
                    );
            }
            shieldID++;
        }
    }

    private float GetDistance(BossMode dir, out float subDisHoriz, out float subDisVert)
    {
        cornerUL = (Vector2)transform.position + new Vector2(-halfBox.x, halfBox.y);
        cornerUR = (Vector2)transform.position + new Vector2(halfBox.x, halfBox.y);
        cornerDL = (Vector2)transform.position + new Vector2(-halfBox.x, -halfBox.y);
        cornerDR = (Vector2)transform.position + new Vector2(halfBox.x, -halfBox.y);

        subDisHoriz = 0;
        subDisVert = 0;

        float distance = 0;
        float subDis1;
        float subDis2;
        switch (dir)
        {
            default:
                break;
            case BossMode.Up:
                distance = PlayState.GetDistance(PlayState.EDirsCardinal.Up, cornerUL, cornerUR, CAST_COUNT, enemyCollide);
                break;
            case BossMode.Upright:
                subDis1 = PlayState.GetDistance(PlayState.EDirsCardinal.Up, cornerUL, cornerUR, CAST_COUNT, enemyCollide);
                subDis2 = PlayState.GetDistance(PlayState.EDirsCardinal.Right, cornerUR, cornerDR, CAST_COUNT, enemyCollide);
                if (subDis1 < subDis2)
                    distance = subDis1;
                else
                    distance = subDis2;
                subDisHoriz = subDis2;
                subDisVert = subDis1;
                break;
            case BossMode.Right:
                distance = PlayState.GetDistance(PlayState.EDirsCardinal.Right, cornerUR, cornerDR, CAST_COUNT, enemyCollide);
                break;
            case BossMode.Downright:
                subDis1 = PlayState.GetDistance(PlayState.EDirsCardinal.Right, cornerUR, cornerDR, CAST_COUNT, enemyCollide);
                subDis2 = PlayState.GetDistance(PlayState.EDirsCardinal.Down, cornerDL, cornerDR, CAST_COUNT, enemyCollide);
                if (subDis1 < subDis2)
                    distance = subDis1;
                else
                    distance = subDis2;
                subDisHoriz = subDis1;
                subDisVert = subDis2;
                break;
            case BossMode.Down:
                distance = PlayState.GetDistance(PlayState.EDirsCardinal.Down, cornerDL, cornerDR, CAST_COUNT, enemyCollide);
                break;
            case BossMode.Downleft:
                subDis1 = PlayState.GetDistance(PlayState.EDirsCardinal.Down, cornerDL, cornerDR, CAST_COUNT, enemyCollide);
                subDis2 = PlayState.GetDistance(PlayState.EDirsCardinal.Left, cornerDL, cornerUL, CAST_COUNT, enemyCollide);
                if (subDis1 < subDis2)
                    distance = subDis1;
                else
                    distance = subDis2;
                subDisHoriz = subDis2;
                subDisVert = subDis1;
                break;
            case BossMode.Left:
                distance = PlayState.GetDistance(PlayState.EDirsCardinal.Left, cornerDL, cornerUL, CAST_COUNT, enemyCollide);
                break;
            case BossMode.Upleft:
                subDis1 = PlayState.GetDistance(PlayState.EDirsCardinal.Left, cornerDL, cornerUL, CAST_COUNT, enemyCollide);
                subDis2 = PlayState.GetDistance(PlayState.EDirsCardinal.Up, cornerUL, cornerUR, CAST_COUNT, enemyCollide);
                if (subDis1 < subDis2)
                    distance = subDis1;
                else
                    distance = subDis2;
                subDisHoriz = subDis1;
                subDisVert = subDis2;
                break;
        }
        return distance;
    }

    private void Stomp()
    {
        lastMode = mode;
        mode = BossMode.Wait;
        modeTimeout = MODE_TIMEOUT;
        int shakeAngle = 0;
        switch (lastMode)
        {
            case BossMode.Left:
            case BossMode.Right:
                shakeAngle = 0;
                break;
            case BossMode.Upleft:
            case BossMode.Downright:
                shakeAngle = 45;
                break;
            case BossMode.Up:
            case BossMode.Down:
                shakeAngle = 90;
                break;
            case BossMode.Upright:
            case BossMode.Downleft:
                shakeAngle = 135;
                break;
        }
        if (Mathf.Abs(velocity.x) > 0.01f || Mathf.Abs(velocity.y) > 0.01f)
        {
            PlayState.globalFunctions.ScreenShake(new List<float> { SHAKE_STRENGTH, 0 }, new List<float> { SHAKE_TIME }, shakeAngle, 5f);
            PlayState.PlaySound("Stomp");
        }
    }

    private void ChargeHoriz()
    {
        if (PlayState.player.transform.position.x > transform.position.x)
            Charge(BossMode.Right);
        else
            Charge(BossMode.Left);
    }

    private void ChargeVert()
    {
        if (PlayState.player.transform.position.y > transform.position.y)
            Charge(BossMode.Up);
        else
            Charge(BossMode.Down);
    }

    private void ChargeDiag()
    {
        if (PlayState.player.transform.position.x > transform.position.x)
        {
            if (PlayState.player.transform.position.y > transform.position.y)
                Charge(BossMode.Upright);
            else
                Charge(BossMode.Downright);
        }
        else
        {
            if (PlayState.player.transform.position.y > transform.position.y)
                Charge(BossMode.Upleft);
            else
                Charge(BossMode.Downleft);
        }
    }

    private void Charge(BossMode dir)
    {
        lastMode = mode;
        mode = dir;
        dirString = dir switch
        {
            BossMode.Up => "N",
            BossMode.Upright => "NE",
            BossMode.Right => "E",
            BossMode.Downright => "SE",
            BossMode.Down => "S",
            BossMode.Downleft => "SW",
            BossMode.Left => "W",
            _ => "NW"
        };
        anim.Play("Boss_spaceBox_charge" + attackMode.ToString() + "_" + dirString);
    }

    private void Shoot()
    {
        PlayState.ShootEnemyBullet(this, transform.position, EnemyBullet.BulletType.donutRotary, new float[] { 3.75f, 4, PlayState.TAU / SHOT_COUNT * shotCount });
    }

    private void MakeBoxes()
    {
        anim.Play("Boss_spaceBox_waitSpawn" + attackMode.ToString());
        Transform parentRoom = transform.parent;
        if (babyboxes.Count < MAX_BOXES)
        {
            if (attackMode == 0)
            {
                GameObject box1 = Instantiate(babyboxObj, transform.position, Quaternion.identity, parentRoom);
                box1.GetComponent<SpaceBoxBabybox>().Activate(this, true);
                GameObject box2 = Instantiate(babyboxObj, transform.position, Quaternion.identity, parentRoom);
                box2.GetComponent<SpaceBoxBabybox>().Activate(this, false);
            }
            else
            {
                GameObject box1 = Instantiate(babyboxObj, new Vector2(transform.position.x - 2, transform.position.y + 2), Quaternion.identity, parentRoom);
                box1.GetComponent<SpaceBoxBabybox>().Activate(this, false);
                GameObject box2 = Instantiate(babyboxObj, new Vector2(transform.position.x + 2, transform.position.y + 2), Quaternion.identity, parentRoom);
                box2.GetComponent<SpaceBoxBabybox>().Activate(this, true);
                GameObject box3 = Instantiate(babyboxObj, new Vector2(transform.position.x - 2, transform.position.y - 2), Quaternion.identity, parentRoom);
                box3.GetComponent<SpaceBoxBabybox>().Activate(this, true);
                GameObject box4 = Instantiate(babyboxObj, new Vector2(transform.position.x + 2, transform.position.y - 2), Quaternion.identity, parentRoom);
                box4.GetComponent<SpaceBoxBabybox>().Activate(this, false);
            }
        }
        mode = BossMode.Wait;
        modeTimeout = SPAWN_TIMEOUT;
    }

    public override void Kill()
    {
        for (int i = babyboxes.Count - 1; i >= 0; i--)
            babyboxes[i].Kill();
        PlayState.QueueAchievementPopup(AchievementPanel.Achievements.BeatSpaceBox);
        base.Kill();
    }
}
