using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stompy : Boss
{
    private enum BossMode
    {
        intro1,
        intro2,
        intro3,
        moveStomp,
        hunt,
        sync,
        step
    };
    private enum FootMode
    {
        none,
        stomp,
        waitRaise,
        raise,
        move,
        sync,
        step,
        stepNow,
        stepWait
    };
    private BossMode modeMain;
    private FootMode modeL;
    private FootMode modeR;

    private enum Parts { foot, eye, pupil, eyelid };
    private float lastFootLY;
    private float lastFootRY;
    private string lastFootLState;
    private string lastFootRState;

    private const int NO_ORIGIN = -999999999;
    private const float RAISED_Y = 13.75f;
    private const float STOMP_Y = 0f;
    private const float MIN_DIST = 2.25f;
    private const float SYNC_MODE_TIMEOUT = 3f;
    private const float STEP_MODE_TIMEOUT = 9f;
    private const float SEC_PER_TICK = 0.01f;
    private const float SIXTEENTH = 0.0625f;
    private const float SHAKE_STRENGTH = 0.35f;
    private const float SHAKE_TIME = 0.6f;

    private readonly Vector2 offsetEyeL = new Vector2(6.0625f, 1.5625f);
    private readonly Vector2 offsetEyeR = new Vector2(-0.5625f, 1.5625f);
    private readonly Vector2 offsetFootL = new Vector2(-15.625f, 0f);
    private readonly Vector2 offsetFootR = new Vector2(0f, 0f);

    private int attackMode = 0;
    private float bossSpeed = 0.6f;

    private StompyFoot footL;
    private StompyFoot footR;
    private StompyEye eyeL;
    private StompyEye eyeR;
    private int[] spriteData;
    /*\
     *   ANIMATION DATA VALUES
     * 0 - Mirror foot on left side
     * 1 - Mirror eye on left side
     * 2 - Mirror pupil on left side
     * 3 - Mirror eyelid on left side
     * 4 - Mask left pupil to only appear over eye
     * 5 - Mask right pupil to only appear over eye
    \*/
    private List<AnimationModule> anims = new List<AnimationModule>();

    private Vector2 footLPos;
    private Vector2 footRPos;
    private Vector2 footLTarget;
    private Vector2 footRTarget;
    private Vector2 footLVel;
    private Vector2 footRVel;

    private Vector2 stepLOrigin;
    private Vector2 stepROrigin;
    private float stepRadius = 2.5f;
    private float stepThetaL;
    private float stepThetaR;
    private float thetaL;
    private float thetaR;
    private float minEyeY;

    private readonly float[] timeouts = new float[]
    {
        0.60153f, 0.48509f, 0.70037f, 0.66276f, 0.70802f, 0.79541f, 0.62043f, 0.5796f, 0.99605f, 0.15058f,
        0.72121f, 0.86851f, 0.64371f, 0.76708f, 0.89401f, 0.52828f, 0.72309f, 0.15963f, 0.15116f, 0.1799f,
        0.27829f, 0.40878f, 0.92538f, 0.45074f, 0.18865f, 0.59797f, 0.4318f, 0.94098f, 0.23463f, 0.29221f,
        0.59734f, 0.34877f, 0.81676f, 0.57617f, 0.14883f, 0.16094f, 0.14123f, 0.57931f, 0.85924f, 0.22828f,
        0.63834f, 0.10387f, 0.54746f, 0.24897f, 0.11105f, 0.49748f, 0.54746f, 0.19405f, 0.79792f, 0.36023f,
        0.53726f, 0.78544f, 0.60425f, 0.83512f, 0.01696f, 0.10451f, 0.01513f, 0.78678f, 0.51617f, 0.24251f
    };
    private float footLTimeoutStomp;
    private float footLTimeoutRaise;
    private int footLTimeoutIndex = 23;
    private float footRTimeoutStomp;
    private float footRTimeoutRaise;
    private int footRTimeoutIndex = 34;
    private float stepModeTimeout;

    private float elapsed;
    private bool hasIntroRun = false;

    private bool eyesOnFeet = true;
    private bool beingKilled = false;
    private bool stepDirIsLeft = true;

    private List<GameObject> cannons = new List<GameObject>();

    private void Awake()
    {
        if (PlayState.IsBossAlive(1))
        {
            SpawnBoss(Mathf.FloorToInt(2000 * PlayState.currentDifficulty == 2 ? 2 : (PlayState.currentCharacter == "Sluggy" ? 1.4f : 1)),
                2, 5, true, 1);
            if (PlayState.currentDifficulty == 2)
                bossSpeed = 1f;
            else if (PlayState.currentCharacter == "Sluggy")
                bossSpeed = 0.9f;
            transform.position -= Vector3.up * 2.5625f;
            minEyeY = transform.position.y + 5f;
            footLPos.y = RAISED_Y;
            footRPos.y = RAISED_Y;
            footL = transform.Find("Foot L").GetComponent<StompyFoot>();
            footR = transform.Find("Foot R").GetComponent<StompyFoot>();
            eyeL = transform.Find("Eye L").GetComponent<StompyEye>();
            eyeR = transform.Find("Eye R").GetComponent<StompyEye>();
            InitializeAnims();
            MoveChildren();
            modeMain = BossMode.intro1;
        }
        else
            Destroy(gameObject);
    }

    private void InitializeAnims()
    {
        spriteData = PlayState.GetAnim("Boss_stompy_data").frames;

        footL.GetComponent<SpriteRenderer>().flipX = spriteData[0] == 1;
        eyeL.GetComponent<SpriteRenderer>().flipX = spriteData[1] == 1;
        eyeL.transform.Find("Pupil").GetComponent<SpriteRenderer>().flipX = spriteData[2] == 1;
        eyeL.transform.Find("Eyelid").GetComponent<SpriteRenderer>().flipX = spriteData[3] == 1;
        eyeL.transform.Find("Pupil").GetComponent<SpriteRenderer>().maskInteraction = spriteData[4] == 1 ? SpriteMaskInteraction.VisibleInsideMask : SpriteMaskInteraction.None;
        eyeR.transform.Find("Pupil").GetComponent<SpriteRenderer>().maskInteraction = spriteData[5] == 1 ? SpriteMaskInteraction.VisibleInsideMask : SpriteMaskInteraction.None;

        anims.Add(footL.GetComponent<AnimationModule>());
        anims.Add(footR.GetComponent<AnimationModule>());
        anims.Add(eyeL.GetComponent<AnimationModule>());
        anims.Add(eyeR.GetComponent<AnimationModule>());
        anims.Add(eyeL.transform.Find("Pupil").GetComponent<AnimationModule>());
        anims.Add(eyeR.transform.Find("Pupil").GetComponent<AnimationModule>());
        anims.Add(eyeL.transform.Find("Eyelid").GetComponent<AnimationModule>());
        anims.Add(eyeR.transform.Find("Eyelid").GetComponent<AnimationModule>());

        anims[4].AddMask(eyeL.transform.Find("Mask").GetComponent<SpriteMask>());
        anims[5].AddMask(eyeR.transform.Find("Mask").GetComponent<SpriteMask>());

        for (int i = 0; i <= 2; i++)
        {
            anims[0].Add("Boss_stompy_footL" + i + "_ground");
            anims[0].Add("Boss_stompy_footL" + i + "_rise");
            anims[0].Add("Boss_stompy_footL" + i + "_fall");
            anims[1].Add("Boss_stompy_footR" + i + "_ground");
            anims[1].Add("Boss_stompy_footR" + i + "_rise");
            anims[1].Add("Boss_stompy_footR" + i + "_fall");
            anims[2].Add("Boss_stompy_eyeL" + i);
            anims[3].Add("Boss_stompy_eyeR" + i);
            anims[4].Add("Boss_stompy_pupilL" + i);
            anims[5].Add("Boss_stompy_pupilR" + i);
            anims[6].Add("Boss_stompy_eyelidL" + i + "_open");
            anims[6].Add("Boss_stompy_eyelidL" + i + "_close");
            anims[6].Add("Boss_stompy_eyelidL" + i + "_blink");
            anims[7].Add("Boss_stompy_eyelidR" + i + "_open");
            anims[7].Add("Boss_stompy_eyelidR" + i + "_close");
            anims[7].Add("Boss_stompy_eyelidR" + i + "_blink");
        }

        anims[0].Play("Boss_stompy_footL0_ground");
        anims[1].Play("Boss_stompy_footR0_ground");
        anims[2].Play("Boss_stompy_eyeL0");
        anims[3].Play("Boss_stompy_eyeR0");
        anims[4].Play("Boss_stompy_pupilL0");
        anims[5].Play("Boss_stompy_pupilR0");
        anims[6].Play("Boss_stompy_eyelidL0_open");
        anims[7].Play("Boss_stompy_eyelidR0_open");
    }

    private void PlayAnim(Parts part, bool leftSide, string mode = "")
    {
        string lOrR = leftSide ? "L" : "R";
        switch (part)
        {
            default:
            case Parts.foot:
                anims[leftSide ? 0 : 1].Play("Boss_stompy_foot" + lOrR + attackMode + "_" + mode);
                break;
            case Parts.eye:
                anims[leftSide ? 2 : 3].Play("Boss_stompy_eye" + lOrR + attackMode);
                break;
            case Parts.pupil:
                anims[leftSide ? 4 : 5].Play("Boss_stompy_pupil" + lOrR + attackMode);
                break;
            case Parts.eyelid:
                anims[leftSide ? 6 : 7].Play("Boss_stompy_eyelid" + lOrR + attackMode + "_" + mode);
                break;
        }
    }

    private void UpdateAnimLifeState()
    {
        foreach (AnimationModule anim in anims)
        {
            string oldName = anim.currentAnimName;
            string[] nameParts;
            if (attackMode == 2)
                nameParts = oldName.Split('1');
            else
                nameParts = oldName.Split('0');
            anim.Play(nameParts[0] + attackMode + nameParts[1]);
        }
    }

    private void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        elapsed += Time.deltaTime * bossSpeed * 2f;
        while (elapsed > SEC_PER_TICK)
        {
            elapsed -= SEC_PER_TICK;
            MoveChildren();
        }

        if (!hasIntroRun)
        {
            switch (modeMain)
            {
                case BossMode.intro1:
                    if (PlayState.player.transform.position.x < transform.position.x - 5.5625f)
                        modeL = FootMode.stomp;
                    if (PlayState.player.transform.position.x < transform.position.x - 6.8125f)
                    {
                        //PlayState.player.velocity.x = PlayState.player._runSpeed.value;
                        //PlayState.player.setFaceDir(Player.FACE_FLOOR_RIGHT, true);
                        modeMain = BossMode.intro2;
                    }
                    //else
                    //{
                    //    PlayState.player.velocity.x = -PlayState.player._runSpeed.value;
                    //}
                    break;
                case BossMode.intro2:
                    if (PlayState.player.transform.position.x > transform.position.x - 2.875f)
                        modeR = FootMode.stomp;
                    if (PlayState.player.transform.position.x > transform.position.x - 2.375f)
                    {
                        //PlayState.player.setFaceDir(Player.FACE_FLOOR_LEFT, true);
                        //PlayState.player.velocity.x = 0;
                        modeMain = BossMode.intro3;
                        hasIntroRun = true;
                        //resetStartTime();
                        //PlayState.hud.bossBarHud.makeBar(_hp);
                        //PlayState.miniMap.setMapLittle();
                        //PlayState.showBossName(2);
                        //Music.playBoss1();
                        RunIntro(false);
                    }
                    //else
                    //{
                    //    PlayState.player.velocity.x = PlayState.player._runSpeed.value;
                    //}
                    break;
            }
        }
        else
        {
            switch (modeMain)
            {
                case BossMode.intro3:
                    if (hasIntroRun)
                    {
                        modeMain = BossMode.moveStomp;
                        stepModeTimeout = STEP_MODE_TIMEOUT;
                    }
                    break;
                case BossMode.moveStomp:
                    stepModeTimeout -= elapsed * bossSpeed;
                    if (stepModeTimeout < 0)
                        modeMain = BossMode.hunt;
                    break;
                case BossMode.hunt:
                    if (footRPos.x - footLPos.x <= MIN_DIST + 0.125f)
                    {
                        stepModeTimeout = SYNC_MODE_TIMEOUT;
                        modeMain = BossMode.sync;
                    }
                    break;
                case BossMode.sync:
                    if (modeL == FootMode.move && modeR == FootMode.move)
                    {
                        modeL = FootMode.stomp;
                        modeR = FootMode.stomp;
                    }
                    stepModeTimeout -= elapsed * bossSpeed;
                    if (stepModeTimeout < 0)
                    {
                        stepModeTimeout = STEP_MODE_TIMEOUT;
                        modeMain = BossMode.step;
                    }
                    break;
                case BossMode.step:
                    if (modeL == FootMode.move)
                        modeL = FootMode.stomp;
                    if (modeR == FootMode.move)
                        modeR = FootMode.stomp;
                    stepModeTimeout -= elapsed * bossSpeed;
                    if (stepModeTimeout < 0)
                    {
                        stepModeTimeout = STEP_MODE_TIMEOUT;
                        modeMain = BossMode.moveStomp;
                        modeL = FootMode.raise;
                        modeR = FootMode.raise;
                        footLTimeoutRaise = 0f;
                        footRTimeoutRaise = 0f;
                        footLTimeoutStomp = 50f;
                        footRTimeoutStomp = 50f;
                    }
                    break;
            }
        }
    }

    public override void LateUpdate() { }

    private void MoveChildren()
    {
        lastFootLY = footLPos.y;
        lastFootRY = footRPos.y;

        if (modeL == FootMode.stomp)
        {
            footLVel.y -= 0.2f;
            footLPos = new Vector2(footLPos.x, footLPos.y + footLVel.y * SIXTEENTH);
            if (footLPos.y < 0)
            {
                footLPos.y = 0;
                modeL = FootMode.waitRaise;
                if (modeMain == BossMode.step)
                    modeL = FootMode.step;
                footLVel.y = 0;
                footLTimeoutRaise = 50;
                footLTimeoutStomp = 1000000;
                PlayState.PlaySound("Stomp");
                PlayState.globalFunctions.ScreenShake(new List<float> { SHAKE_STRENGTH, 0 }, new List<float> { SHAKE_TIME });
            }
        }
        else if (modeMain > BossMode.intro3 && modeL == FootMode.waitRaise && --footLTimeoutRaise <= 0)
        {
            if (modeMain != BossMode.step)
                modeL = FootMode.raise;
            else
                modeL = FootMode.step;
        }
        if (modeMain > BossMode.intro3 && modeL == FootMode.raise)
        {
            footLVel.y += 0.2f;
            footLPos.y += footLVel.y * SIXTEENTH;
            if (footLPos.y > RAISED_Y)
            {
                footLPos.y = RAISED_Y;
                modeL = FootMode.move;
                //this.leye.shouldAttack = true;
                footLTimeoutRaise = 1000000;
                footLTimeoutStomp = timeouts[++footLTimeoutIndex % timeouts.Length] * 360 + 60;
                if (modeMain == BossMode.sync)
                    footLTimeoutStomp = 10;
            }
        }
        if (modeR == FootMode.stomp)
        {
            footRVel.y -= 0.2f;
            footRPos.y += footRVel.y * SIXTEENTH;
            if (modeMain == BossMode.step)
                modeR = FootMode.step;
            footRVel.y = 0;
            footRTimeoutRaise = 50;
            footRTimeoutStomp = 1000000;
            PlayState.PlaySound("Stomp");
            PlayState.globalFunctions.ScreenShake(new List<float> { SHAKE_STRENGTH, 0 }, new List<float> { SHAKE_TIME });
        }
        else if (modeMain > BossMode.intro3 && modeR == FootMode.waitRaise && --footRTimeoutRaise <= 0)
        {
            if (modeMain != BossMode.step)
                modeR = FootMode.raise;
            else
                modeR = FootMode.step;
        }
        if (modeMain > BossMode.intro3 && modeR == FootMode.raise)
        {
            footRVel.y += 0.2f;
            footRPos.y += footRVel.y * SIXTEENTH;
            if (footRPos.y > RAISED_Y)
            {
                footRPos.y = RAISED_Y;
                modeR = FootMode.move;
                //this.reye.shouldAttack = true;
                footRTimeoutRaise = 1000000;
                footRTimeoutStomp = timeouts[++footRTimeoutIndex % timeouts.Length] * 360 + 60;
                if (modeMain == BossMode.sync)
                    footRTimeoutStomp = 10;
            }
        }
        if (modeMain > BossMode.intro3 && modeL == FootMode.move)
        {
            thetaL += 0.2f;
            if (modeMain == BossMode.hunt)
                footLTarget.x = PlayState.player.transform.position.x - transform.position.x;
            else
                footLTarget.x = Mathf.Sin(thetaL / 15) * 160;
            if (footRPos.x - footLTarget.x < MIN_DIST)
                footLTarget.x = footRPos.x - MIN_DIST;
            if (PlayState.player.transform.position.x - transform.position.x < -20)
                footLTarget.x = PlayState.player.transform.position.x - transform.position.x + 6.25f;
            footLTimeoutStomp--;
            footLVel.x = footLTarget.x - footLPos.x;
            footLPos.x += footLVel.x * 0.1f;
            if (footLTimeoutStomp <= 0 && footLPos.y >= RAISED_Y - 0.625f && footLVel.y > 1)
            {
                modeL = FootMode.stomp;
                footLVel.y = -10f;
                //this.leye.shouldAttack = false;
            }
        }
        if (modeMain > BossMode.intro3 && modeR == FootMode.move)
        {
            thetaR += 0.2f;
            if (modeMain == BossMode.hunt)
                footRTarget.x = PlayState.player.transform.position.x - transform.position.x;
            else
                footRTarget.x = Mathf.Sin(thetaR / 15.5f + Mathf.PI / 3f) * 160;
            if (footRTarget.x - footLPos.x < MIN_DIST)
                footRTarget.x = footLPos.x + MIN_DIST;
            if (PlayState.player.transform.position.x - transform.position.x > 18.875f)
                footRTarget.x = PlayState.player.transform.position.x - transform.position.x - 2.5f;
            footRVel.x = footRTarget.x - footRPos.x;
            footRPos.x += footRVel.x * 0.1f;
            footRTimeoutStomp--;
            if (footRTimeoutStomp <= 0 && footRPos.y >= RAISED_Y - 10 && footRVel.y > 1)
            {
                modeR = FootMode.stomp;
                footRVel.y = -10f;
                //this.reye.shouldAttack = false;
            }
        }
        if (modeL == FootMode.step && modeR == FootMode.step)
        {
            if (footLPos.x < transform.position.x)
            {
                stepDirIsLeft = true;
                modeL = FootMode.stepNow;
                modeR = FootMode.stepWait;
                stepThetaL = 0f;
                stepThetaR = 0f;
                stepLOrigin = new Vector2(NO_ORIGIN, footLPos.y);
            }
            else
            {
                stepDirIsLeft = false;
                modeL = FootMode.stepWait;
                modeR = FootMode.stepNow;
                stepThetaL = 0f;
                stepThetaR = 0f;
                stepROrigin = new Vector2(NO_ORIGIN, footRPos.y);
            }
        }
        if (modeL == FootMode.stepNow)
        {
            if (stepDirIsLeft && stepThetaL == 0f && footLPos.x < -10.625f)
            {
                stepDirIsLeft = false;
                modeL = FootMode.stepWait;
                modeR = FootMode.stepNow;
                stepThetaR = 0f;
                stepROrigin = new Vector2(NO_ORIGIN, footRPos.y);
            }
            else
            {
                stepThetaL += 0.05f;
                if (stepThetaL >= Mathf.PI)
                {
                    stepThetaL = Mathf.PI;
                    PlayState.PlaySound("Stomp");
                    PlayState.globalFunctions.ScreenShake(new List<float> { SHAKE_STRENGTH, 0 }, new List<float> { SHAKE_TIME });
                    modeL = FootMode.stepWait;
                    modeR = FootMode.stepNow;
                    stepThetaR = 0f;
                    stepROrigin = new Vector2(NO_ORIGIN, footRPos.y);
                }
                if (stepDirIsLeft)
                {
                    if (stepLOrigin.x == NO_ORIGIN)
                        stepLOrigin.x = footLPos.x - stepRadius;
                    footLPos.x = stepLOrigin.x + Mathf.Cos(stepThetaL) * stepRadius;
                    footLPos.y = stepLOrigin.y + Mathf.Sin(stepThetaL) * stepRadius * 3.4f;
                }
                else
                {
                    if (stepLOrigin.x == NO_ORIGIN)
                        stepLOrigin.x = footLPos.x + stepRadius;
                    footLPos.x = stepLOrigin.x - Mathf.Cos(stepThetaL) * stepRadius;
                    footLPos.y = stepLOrigin.y + Mathf.Sin(stepThetaL) * stepRadius * 3.4f;
                }
            }
        }
        else if (modeR == FootMode.stepNow)
        {
            if (!stepDirIsLeft && stepThetaR == 0f && footRPos.x > 10.625f)
            {
                stepDirIsLeft = true;
                modeR = FootMode.stepWait;
                modeL = FootMode.stepNow;
                stepThetaL = 0f;
                stepLOrigin = new Vector2(NO_ORIGIN, footLPos.y);
            }
            else
            {
                stepThetaR += 0.05f;
                if (stepThetaR >= Mathf.PI)
                {
                    stepThetaR = Mathf.PI;
                    PlayState.PlaySound("Stomp");
                    PlayState.globalFunctions.ScreenShake(new List<float> { SHAKE_STRENGTH, 0 }, new List<float> { SHAKE_TIME });
                    modeL = FootMode.stepNow;
                    modeR = FootMode.stepWait;
                    stepLOrigin = new Vector2(NO_ORIGIN, footLPos.y);
                    stepThetaL = 0f;
                }
                if (stepDirIsLeft)
                {
                    if (stepROrigin.x == NO_ORIGIN)
                        stepROrigin.x = footRPos.x - stepRadius;
                    footRPos.x = stepROrigin.x + Mathf.Cos(stepThetaR) * stepRadius;
                    footRPos.y = stepROrigin.y + Mathf.Sin(stepThetaR) * stepRadius * 3.4f;
                }
                else
                {
                    if (stepROrigin.x == NO_ORIGIN)
                        stepROrigin.x = footRPos.x + stepRadius;
                    footRPos.x = stepROrigin.x - Mathf.Cos(stepThetaR) * stepRadius;
                    footRPos.y = stepROrigin.y + Mathf.Sin(stepThetaR) * stepRadius * 3.4f;
                }
            }
        }
        if (footRPos.x - footLPos.x < MIN_DIST)
        {
            float min = MIN_DIST - (footRPos.x - footLPos.x);
            if (modeL == FootMode.move && modeR == FootMode.stomp)
                footLPos.x -= min;
            else if (modeR == FootMode.move && modeL == FootMode.stomp)
                footRPos.x += min;
            else
            {
                footLPos.x -= min * 0.5f;
                footRPos.x += min * 0.5f;
            }
        }
        footL.transform.position = (Vector2)transform.position + offsetFootL + footLPos;
        footR.transform.position = (Vector2)transform.position + offsetFootR + footRPos;
        if (eyesOnFeet)
        {
            eyeL.transform.position = (Vector2)footL.transform.position + offsetEyeL;
            eyeR.transform.position = (Vector2)footR.transform.position + offsetEyeR;
        }
        if (eyeL.transform.position.y >= minEyeY)
        {
            eyeL.transform.position = new Vector2(eyeL.transform.position.x, minEyeY);
            //eyeL.solid = false;
        }
        //else
        //{
        //    this.leye.solid = true;
        //}
        if (eyeR.transform.position.y >= minEyeY)
        {
            eyeR.transform.position = new Vector2(eyeR.transform.position.x, minEyeY);
            //this.reye.solid = false;
        }
        //else
        //{
        //    this.reye.solid = true;
        //}

        if (footLPos.y > lastFootLY && lastFootLState != "rise")
        {
            PlayAnim(Parts.foot, true, "rise");
            lastFootLState = "rise";
        }
        if (footLPos.y < lastFootLY && lastFootLState != "fall")
        {
            PlayAnim(Parts.foot, true, "fall");
            lastFootLState = "fall";
        }
        if (footLPos.y == lastFootLY && lastFootLState != "ground")
        {
            PlayAnim(Parts.foot, true, "ground");
            lastFootLState = "ground";
        }
        if (footRPos.y > lastFootRY && lastFootRState != "rise")
        {
            PlayAnim(Parts.foot, false, "rise");
            lastFootLState = "rise";
        }
        if (footRPos.y < lastFootRY && lastFootRState != "fall")
        {
            PlayAnim(Parts.foot, false, "fall");
            lastFootLState = "fall";
        }
        if (footRPos.y == lastFootRY && lastFootRState != "ground")
        {
            PlayAnim(Parts.foot, false, "ground");
            lastFootLState = "ground";
        }
    }

    public override void OnTriggerEnter2D(Collider2D collision) { }

    public override void OnTriggerExit2D(Collider2D collision) { }
}