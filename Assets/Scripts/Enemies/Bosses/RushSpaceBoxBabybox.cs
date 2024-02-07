using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RushSpaceBoxBabybox : Enemy
{
    public RushSpaceBox parentBoss;

    private const float ACCEL_NORMAL = 0.416f;
    private const float ACCEL_BLUE = 0.52f;
    private const float MODE_TIMEOUT = 0.6f;
    private const float SHAKE_STRENGTH = 0.4f;
    private const float SHAKE_TIME = 0.6f;
    private const int CAST_COUNT = 5;

    private enum BoxMode
    {
        Up,
        Right,
        Down,
        Left,
        Wait
    };
    private BoxMode mode = BoxMode.Wait;
    private BoxMode lastMode = BoxMode.Wait;

    private Vector2 cornerUL;
    private Vector2 cornerUR;
    private Vector2 cornerDL;
    private Vector2 cornerDR;

    private float acceleration;
    private float modeTimeout;
    private Vector2 velocity;
    private Vector2 halfBox;

    private void Awake()
    {
        Spawn(450, 2, 9, true, 4);

        anim.Add("RushBoss_spaceBox_babybox0_spawn");
        anim.Add("RushBoss_spaceBox_babybox1_spawn");
        anim.Add("RushBoss_spaceBox_babybox0_hit");
        anim.Add("RushBoss_spaceBox_babybox1_hit");
        anim.Add("RushBoss_spaceBox_babybox0_turnBlue");

        modeTimeout = MODE_TIMEOUT;
        col.TryGetComponent(out BoxCollider2D box);
        halfBox = box.size * 0.5f;
    }

    private void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        CheckMode();

        float distance = GetDistance(mode);
        bool stopMoving = false;
        switch (mode)
        {
            default:
                velocity = Vector2.zero;
                break;
            case BoxMode.Up:
                velocity.y += acceleration * Time.fixedDeltaTime;
                if (distance < velocity.y)
                {
                    velocity.y = distance - PlayState.FRAC_32;
                    stopMoving = true;
                }
                break;
            case BoxMode.Right:
                velocity.x += acceleration * Time.fixedDeltaTime;
                if (distance < velocity.x)
                {
                    velocity.x = distance - PlayState.FRAC_32;
                    stopMoving = true;
                }
                break;
            case BoxMode.Down:
                velocity.y -= acceleration * Time.fixedDeltaTime;
                if (distance < -velocity.y)
                {
                    velocity.y = -distance + PlayState.FRAC_32;
                    stopMoving = true;
                }
                break;
            case BoxMode.Left:
                velocity.x -= acceleration * Time.fixedDeltaTime;
                if (distance < -velocity.x)
                {
                    velocity.x = -distance + PlayState.FRAC_32;
                    stopMoving = true;
                }
                break;
        }
        transform.position += (Vector3)velocity;
        if (stopMoving)
            Stomp();
    }

    public void Activate(RushSpaceBox parent, bool startingOrientation)
    {
        parentBoss = parent;
        parentBoss.babyboxes.Add(this);
        anim.Play("RushBoss_spaceBox_babybox" + parentBoss.attackMode.ToString() + "_spawn");
        acceleration = (parentBoss.attackMode == 1 ? ACCEL_BLUE : ACCEL_NORMAL) * (PlayState.currentProfile.difficulty == 2 ? 1.3f : 1f);
        lastMode = startingOrientation ? BoxMode.Up : BoxMode.Left;
    }

    public void TurnBlue()
    {
        anim.Play("RushBoss_spaceBox_babybox0_turnBlue");
        acceleration = ACCEL_BLUE * (PlayState.currentProfile.difficulty == 2 ? 1.3f : 1f);
    }

    private float GetDistance(BoxMode dir)
    {
        cornerUL = (Vector2)transform.position + new Vector2(-halfBox.x, halfBox.y);
        cornerUR = (Vector2)transform.position + new Vector2(halfBox.x, halfBox.y);
        cornerDL = (Vector2)transform.position + new Vector2(-halfBox.x, -halfBox.y);
        cornerDR = (Vector2)transform.position + new Vector2(halfBox.x, -halfBox.y);

        float distance = 0;
        switch (dir)
        {
            default:
                break;
            case BoxMode.Up:
                distance = PlayState.GetDistance(PlayState.EDirsCardinal.Up, cornerUL, cornerUR, CAST_COUNT, enemyCollide);
                break;
            case BoxMode.Right:
                distance = PlayState.GetDistance(PlayState.EDirsCardinal.Right, cornerUR, cornerDR, CAST_COUNT, enemyCollide);
                break;
            case BoxMode.Down:
                distance = PlayState.GetDistance(PlayState.EDirsCardinal.Down, cornerDL, cornerDR, CAST_COUNT, enemyCollide);
                break;
            case BoxMode.Left:
                distance = PlayState.GetDistance(PlayState.EDirsCardinal.Left, cornerDL, cornerUL, CAST_COUNT, enemyCollide);
                break;
        }
        return distance;
    }

    private void CheckMode()
    {
        modeTimeout -= Time.deltaTime;
        if (mode == BoxMode.Wait && modeTimeout < 0)
        {
            switch (lastMode)
            {
                case BoxMode.Wait:
                case BoxMode.Right:
                case BoxMode.Left:
                    if (PlayState.player.transform.position.y > transform.position.y)
                        Charge(BoxMode.Up);
                    else
                        Charge(BoxMode.Down);
                    break;
                case BoxMode.Up:
                case BoxMode.Down:
                    if (PlayState.player.transform.position.x > transform.position.x)
                        Charge(BoxMode.Right);
                    else
                        Charge(BoxMode.Left);
                    break;
            }
        }
    }
    private void Charge(BoxMode dir)
    {
        lastMode = mode;
        mode = dir;
    }

    private void Stomp()
    {
        lastMode = mode;
        mode = BoxMode.Wait;
        modeTimeout = MODE_TIMEOUT;
        int shakeAngle = 0;
        switch (lastMode)
        {
            case BoxMode.Left:
            case BoxMode.Right:
                shakeAngle = 0;
                break;
            case BoxMode.Up:
            case BoxMode.Down:
                shakeAngle = 90;
                break;
        }
        if (Mathf.Abs(velocity.x) > 0.01f || Mathf.Abs(velocity.y) > 0.01f)
        {
            PlayState.globalFunctions.ScreenShake(new List<float> { SHAKE_STRENGTH, 0 }, new List<float> { SHAKE_TIME }, shakeAngle, 2f);
            PlayState.PlaySound("Stomp");
        }
    }

    public override void Kill()
    {
        parentBoss.babyboxes.Remove(this);
        base.Kill();
    }
}
