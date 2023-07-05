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
    private const float GRAVITY = 56.25f;

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

    private PlayState.EDirsCardinal lastHitDir;
    private int lastStomp;
    private int strafeCount;
    private Vector2 velocity;
    private Vector2 lastSmashVelocity = Vector2.zero;
    private Vector2 smashAccel = Vector2.zero;
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
    private int zzzNum;
    private string lastAnimState = "shell";
    private bool facingLeft = false;
    private bool flipHoriz = false;
    private bool facingDown = false;
    private bool flipVert = false;
    private PlayState.EDirsCardinal fallDir = PlayState.EDirsCardinal.None;

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
     *  7 - Frames into Stomp turnaround to flip sprite
     *  8 - Frames into horizontal Stomp gravity jump to flip sprite
     *  9 - Frames into vertical Stomp gravity jump to flip sprite
     * 10 - Update animation on collision during Smash phase
     * 11 - Update animation on landing during Sleep phase
    \*/

    private List<PlayState.TargetPoint> stompPoints = new();

    private GameObject zzzObj;

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

        decisionTableIndex = Mathf.Abs(Mathf.FloorToInt(PlayState.WORLD_ORIGIN.x - (PlayState.WORLD_SIZE.x * PlayState.ROOM_SIZE.x * 0.5f)
            - PlayState.player.transform.position.x)) % decisionTable.Length;

        if (PlayState.currentProfile.difficulty == 2)
            bossSpeed += 0.2f;

        animData = PlayState.GetAnim("Boss_gigaSnail_data").frames;
        if (animData[2] == 1)
            sprite.color = new Color32(255, 255, 255, 0);
        for (int i = 0; i < 2; i++)
        {

        }
        anim.Add("Boss_gigaSnail_intro");
        anim.Play("Boss_gigaSnail_intro");

        bgAnimData = PlayState.GetAnim("GigaBackground_data").frames;
        for (int i = 1; i <= 2; i++)
        {
            BGObj newBG = new() { obj = new GameObject("Giga Snail Background Layer " + i.ToString()) };
            newBG.obj.transform.parent = PlayState.cam.transform;

            newBG.sprite = newBG.obj.AddComponent<SpriteRenderer>();
            newBG.sprite.sortingOrder = -99;

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

        zzzObj = Resources.Load<GameObject>("Objects/Enemies/Bosses/Giga Zzz");

        CollectTargetPoints();
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

    private void PickStompTarget()
    {
        if (stompPoints.Count == 0)
            moveTarget = origin;
        else
        {
            int pointID = Mathf.FloorToInt(GetDecision() * stompPoints.Count);
            moveTarget = stompPoints[pointID].pos;
        }
    }

    private void UpdateAIIntro()
    {
        if (elapsed > 2f && elapsed < 3f && animData[2] == 1)
            sprite.color = new Color32(255, 255, 255, (byte)((elapsed - 2) * 255));
        else if (elapsed > 3 && introTimer == 0)
        {
            sprite.color = new Color32(255, 255, 255, 255);
            StartCoroutine(RunIntro(true, true, true));
        }
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
            //this.bg.setTargetRgb(176, 174, 0);
            //this.bg.bgColorSpeed = 3;
            modeTimeout = 6.2f;
            //this.playAnim("sleep");
            zzzNum = 0;
            zzzTimeout = ZZZ_TIMEOUT;
            if (PlayState.currentProfile.difficulty == 2)
            {
                ZZZ_COUNT = 5;
                modeTimeout *= 1.23f;
            }
            fallDir = PlayState.EDirsCardinal.Down;
            PlayState.PlaySound("Shell");
        }
        if (stomped)
        {
            zzzTimeout -= Time.fixedDeltaTime * bossSpeed;
            if (zzzTimeout <= 0 && zzzNum < ZZZ_COUNT)
            {
                Instantiate(zzzObj, new Vector2(transform.position.x + halfBox.x + 1.5f * zzzNum, transform.position.y), Quaternion.identity, transform);
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
        }
    }

    private void UpdateAIStomp()
    {
        if (!modeInitialized)
        {
            modeInitialized = true;
            //this.bg.setTargetRgb(0, 48, 0);
            //this.bg.bgColorSpeed = 3;
            modeTimeout = 6f;
            PickStompTarget();
        }
        if (lastAnimState == "shell")
        {
            if (Vector2.Distance(transform.position, moveTarget) < 0.625f)
            {
                if (GetDecision() > 0.5f)
                {
                    fallDir = PlayState.EDirsCardinal.Down;
                    //this.playAnim("floor");
                }
                else
                {
                    fallDir = PlayState.EDirsCardinal.Up;
                    //this.playAnim("ceil");
                }
                lastAnimState = "jump";
            }
            transform.position = new Vector2(PlayState.Integrate(transform.position.x, moveTarget.x, 0.7f, Time.fixedDeltaTime * bossSpeed),
                PlayState.Integrate(transform.position.y, moveTarget.y, 0.7f, Time.fixedDeltaTime * bossSpeed));
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
                    velocity = fallDir switch
                    {
                        PlayState.EDirsCardinal.Down => new Vector2(JUMP_POWER, WALK_SPEED * (facingLeft ? -1 : 1)),
                        PlayState.EDirsCardinal.Left => new Vector2(WALK_SPEED * (facingDown ? -1 : 1), JUMP_POWER),
                        PlayState.EDirsCardinal.Right => new Vector2(WALK_SPEED * (facingDown ? -1 : 1), -JUMP_POWER),
                        PlayState.EDirsCardinal.Up => new Vector2(-JUMP_POWER, WALK_SPEED * (facingLeft ? -1 : 1)),
                        _ => Vector2.zero
                    };
                    gravJumpTimeout = GRAV_JUMP_TIMEOUT;
                    jumpTimeout = 99999;
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
        if (modeInitialized)
        {
            modeInitialized = true;
            //if (attackPhase < 1)
            //{
            //    this.bg.setTargetRgb(48, 0, 48);
            //}
            //else
            //{
            //    this.bg.setTargetRgb(0, 48, 48);
            //}
            //this.bg.bgColorSpeed = 3;
            modeTimeout = 5.2f;
            //this.playAnim("shell");
            moveTarget = origin;
            aimed = false;
        }
        transform.position = new Vector2(PlayState.Integrate(transform.position.x, moveTarget.x, 1.7f, Time.fixedDeltaTime * bossSpeed),
            PlayState.Integrate(transform.position.y, moveTarget.y, 1.7f, Time.fixedDeltaTime * bossSpeed));
        if (modeElapsed > START_ATTACK_TIME && !aimed && Vector2.Distance(transform.position, moveTarget) < 0.625f)
        {
            AimStrafe();
            strafeThetaVel = Mathf.PI * 0.125f * (GetDecision() > 0.5f ? 1 : -1);
            aimed = true;
            if (PlayState.currentProfile.difficulty == 2)
                strafeThetaVel *= 1.6f;
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
            //this.bg.setTargetRgb(48, 0, 0);
            //this.bg.bgColorSpeed = 3;
            modeTimeout = 6f;
            //this.playAnim("shell");
            PickSmashDir();
        }
        if (stomped)
        {
            stomped = false;
            if (GetDecision() > 0.7 || attackPhase == 1)
                PickSmashDir(true);
            else if (velocity.y == 0)
                smashAccel.y *= -1;
            else
                smashAccel.x *= -1;
        }
        if (modeTimeout <= 0)
        {
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
    }

    private void AimStrafe()
    {
        float fireAngle = Mathf.Atan2(PlayState.player.transform.position.y - transform.position.y, PlayState.player.transform.position.x - transform.position.x);
        strafeCount = Mathf.Clamp(Mathf.RoundToInt(2.3f + 5f * (maxHealth - health) / maxHealth), 2, 7);
        strafeTheta = fireAngle - Mathf.PI / strafeCount;
    }

    private void StrafeSingle(float angle, bool playSound)
    {
        PlayState.ShootEnemyBullet(transform.position, EnemyBullet.BulletType.bigPea, new float[] { Mathf.Cos(angle), Mathf.Sin(angle), STRAFE_SPEED }, playSound);
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
        smashAccel = new Vector2(SMASH_SPEED * bossSpeed * Mathf.Cos(angle), SMASH_SPEED * bossSpeed * Mathf.Sin(angle));

        if ((lastHitDir == PlayState.EDirsCardinal.Right && smashAccel.x > 0) || (lastHitDir == PlayState.EDirsCardinal.Left && smashAccel.x < 0))
            smashAccel.x *= -1;
        if ((lastHitDir == PlayState.EDirsCardinal.Up && smashAccel.y > 0) || (lastHitDir == PlayState.EDirsCardinal.Down && smashAccel.y < 0))
            smashAccel.y *= -1;
    }

    private void FacePlayer()
    {
        if (fallDir == PlayState.EDirsCardinal.None)
            return;

        if (fallDir == PlayState.EDirsCardinal.Left || fallDir == PlayState.EDirsCardinal.Right)
        {
            bool targetState = PlayState.player.transform.position.y < transform.position.y;
            if (targetState != facingDown)
            {
                facingDown = targetState;
                if (animData[6] != 1)
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
                if (animData[6] != 1)
                    flipHoriz = targetState;
                else if (flipHoriz == targetState)
                    flipHoriz = !targetState;
            }
        }
    }

    private void CollectTargetPoints()
    {
        for (int i = 0; i < PlayState.activeTargets.Count; i++)
        {
            if (PlayState.activeTargets[i].type == PlayState.TargetTypes.GigaStomp)
                stompPoints.Add(PlayState.activeTargets[i]);
        }
    }
}
