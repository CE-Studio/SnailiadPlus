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

    private const int NO_ORIGIN = -999999999;
    private const float RAISED_Y = 13.75f;
    private const float STOMP_Y = 0f;
    private const float MIN_DIST = 2.25f;
    private const float SYNC_MODE_TIMEOUT = 3f;
    private const float STEP_MODE_TIMEOUT = 9f;
    private const float SEC_PER_TICK = 0.01f;
    private const float SIXTEENTH = 0.0625f;

    private readonly Vector2 offsetEyeL = new Vector2(6.0625f, 1.5625f);
    private readonly Vector2 offsetEyeR = new Vector2(-0.5625f, 1.5625f);
    private readonly Vector2 offsetFootL = new Vector2(-15.625f, 0f);
    private readonly Vector2 offsetFootR = new Vector2(0f, 0f);

    private int attackMode = 0;
    private float bossSpeed = 0.6f;

    private GameObject footL;
    private GameObject footR;
    private GameObject eyeL;
    private GameObject eyeR;
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

    private float elapsed;

    private bool eyesOnFeet = true;
    private bool beingKilled = false;
    private bool nextStepIsLeft = true;
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
            footL = transform.Find("Foot L").gameObject;
            footR = transform.Find("Foot R").gameObject;
            eyeL = transform.Find("Eye L").gameObject;
            eyeR = transform.Find("Eye R").gameObject;
            spriteData = PlayState.GetAnim("Boss_stompy_data").frames;
        }
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        elapsed += Time.deltaTime * bossSpeed;
        if (elapsed > SEC_PER_TICK)
        {
            elapsed -= SEC_PER_TICK;
            MoveChildren();
        }

    }

    private void MoveChildren()
    {
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
                PlayState.globalFunctions.ScreenShake(new List<float> { 0.5f, 0 }, new List<float> { 0.3f });
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
            PlayState.globalFunctions.ScreenShake(new List<float> { 0.5f, 0 }, new List<float> { 0.3f });
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

        }
        if (modeMain > BossMode.intro3 && modeR == FootMode.move)
        {

        }
        if (modeL == FootMode.step && modeR == FootMode.step)
        {

        }
        if (modeL == FootMode.stepNow)
        {

        }
        else if (modeR == FootMode.stepNow)
        {

        }
        if (footRPos.x - footLPos.x < MIN_DIST)
        {

        }
    }
}
