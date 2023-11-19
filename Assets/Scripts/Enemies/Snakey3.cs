using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snakey3 : Enemy
{
    private const float REACT_DISTANCE = 24.375f;
    private const float WEAPON_SPEED = 5f;
    private const float SHOT_TIMEOUT = 1.2f;
    private const float CONVERSION_MULTIPLIER = 180f / Mathf.PI;

    private readonly float[] thetaOffsets = new float[]
    {
        0,
        Mathf.PI,
        0,
        Mathf.PI / 2,
        0,
        0,
        -Mathf.PI / 2,
        0,
        0,
        -Mathf.PI / 4,
        Mathf.PI / 4,
        0
    };

    private readonly float[] moveTimeouts = new float[]
    {
        1.2f, 1.3f, 1.4f, 1.1f, 1.6f, 1f, 1.8f, 0.9f, 1.9f, 2.1f, 0.9f, 1.3f, 1.7f, 1.4f, 2.1f, 1.2f, 0.9f, 0.8f,
        1.2f, 1.3f, 1.4f, 0.2f, 1.6f, 1f, 1.8f, 0.4f, 1.9f, 2.1f, 0.9f, 0.7f, 1.7f, 1.2f, 2.3f, 1.1f, 0.9f, 0.8f
    };

    private readonly Vector2 chargeSpeed = new Vector2(15f, 11.875f);
    private readonly Vector2 deceleration = new Vector2(0.21f, 0.1425f);

    private float moveTimeout = 0;
    private float shotTimeout = SHOT_TIMEOUT;
    private int thetaOffsetIndex = 0;
    private int moveTimeoutIndex = 0;
    private Vector2 velocity = Vector2.zero;
    private Vector2 halfBox;
    private float lastDistance;
    private Vector2 lastPoint;

    private bool facingRight;
    private bool flipOnRight;

    public void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(450, 4, 10, true);

        int indexID = Mathf.RoundToInt((transform.position.x * 0.0625f) + (transform.position.y * 0.25f));
        thetaOffsetIndex = indexID % thetaOffsets.Length;
        moveTimeoutIndex = indexID % moveTimeouts.Length;
        moveTimeout = moveTimeouts[moveTimeoutIndex] * 0.25f;

        col.TryGetComponent(out BoxCollider2D box);
        halfBox = box.size * 0.5f;
        lastPoint = transform.position;
        
        anim.Add("Enemy_skyViper_idleL");
        anim.Add("Enemy_skyViper_idleR");
        foreach (string direction in PlayState.DIRS_COMPASS)
            anim.Add("Enemy_skyViper_attack" + direction.ToUpper());

        flipOnRight = PlayState.GetAnim("Enemy_skyViper_data").frames[0] == 1;
    }

    public void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (PlayState.OnScreen(transform.position, col))
        {
            moveTimeout -= Time.fixedDeltaTime;
            if (moveTimeout <= 0 && Mathf.Abs(PlayState.player.transform.position.x - transform.position.x) < REACT_DISTANCE &&
                Mathf.Abs(PlayState.player.transform.position.y - transform.position.y) < REACT_DISTANCE)
            {
                moveTimeoutIndex = ++moveTimeoutIndex % moveTimeouts.Length;
                moveTimeout = moveTimeouts[moveTimeoutIndex];

                float angle = GetAngle() + thetaOffsets[thetaOffsetIndex];
                velocity = new Vector2(Mathf.Cos(angle) * chargeSpeed.x, Mathf.Sin(angle) * chargeSpeed.y);
                PlayAttackAnim(angle, true);
                facingRight = velocity.x > 0;

                thetaOffsetIndex = ++thetaOffsetIndex % thetaOffsets.Length;
            }

            if (velocity.x != 0)
            {
                if (Mathf.Abs(velocity.x) < PlayState.FRAC_64)
                    velocity.x = 0;
                else
                    velocity.x += deceleration.x * (velocity.x < 0 ? 1 : -1);
            }
            if (velocity.y != 0)
            {
                if (Mathf.Abs(velocity.y) < PlayState.FRAC_64)
                    velocity.y = 0;
                else
                    velocity.y += deceleration.y * (velocity.y < 0 ? 1 : -1);
            }

            GetDistance(velocity.x < 0 ? PlayState.EDirsCardinal.Left : PlayState.EDirsCardinal.Right);
            if (lastDistance < Mathf.Abs(velocity.x) * Time.fixedDeltaTime)
            {
                transform.position += (lastDistance - PlayState.FRAC_32) * (velocity.x < 0 ? Vector3.left : Vector3.right);
                velocity.x *= -1;
                facingRight = !facingRight;
                PlayAttackAnim(Vector2.Angle(Vector2.right, velocity), false);
            }
            GetDistance(velocity.y < 0 ? PlayState.EDirsCardinal.Down : PlayState.EDirsCardinal.Up);
            if (lastDistance < Mathf.Abs(velocity.y) * Time.fixedDeltaTime)
            {
                transform.position += (lastDistance - PlayState.FRAC_32) * (velocity.y < 0 ? Vector3.down : Vector3.up);
                velocity.y *= -1;
                PlayAttackAnim(Vector2.Angle(Vector2.right, velocity), false);
            }
            transform.position += (Vector3)velocity * Time.fixedDeltaTime;

            Vector2 testPt = (Vector2)transform.position + new Vector2(velocity.x < 0 ? -halfBox.x : halfBox.x, velocity.y < 0 ? -halfBox.y : halfBox.y);
            if (PlayState.IsPointEnemyCollidable(testPt))
            {
                if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
                    transform.position = new Vector2(transform.position.x, lastPoint.y);
                else
                    transform.position = new Vector2(lastPoint.x, transform.position.y);
            }
            lastPoint = transform.position;

            shotTimeout -= Time.fixedDeltaTime;
            if (shotTimeout <= 0)
            {
                shotTimeout = SHOT_TIMEOUT;
                Shoot(GetAngle());
            }

            if (!anim.isPlaying)
                anim.Play(facingRight ? "Enemy_skyViper_idleR" : "Enemy_skyViper_idleL");
        }

        sprite.flipX = facingRight && flipOnRight;
    }

    private float GetAngle()
    {
        return Mathf.Atan2(PlayState.player.transform.position.y - transform.position.y, PlayState.player.transform.position.x - transform.position.x);
    }

    private void Shoot(float angle)
    {
        if (PlayState.currentProfile.difficulty == 2)
        {
            float angleX = -Mathf.Cos(angle);
            float angleY = Mathf.Sin(angle);
            PlayState.ShootEnemyBullet(this, transform.position, EnemyBullet.BulletType.donutLinear, new float[] { WEAPON_SPEED, angleX, angleY });
        }
    }

    private float GetDistance(PlayState.EDirsCardinal direction)
    {
        Vector2 a;
        Vector2 b;
        switch (direction)
        {
            default:
            case PlayState.EDirsCardinal.Down:
                a = (Vector2)transform.position + new Vector2(-halfBox.x, -halfBox.y);
                b = (Vector2)transform.position + new Vector2(halfBox.x, -halfBox.y);
                break;
            case PlayState.EDirsCardinal.Left:
                a = (Vector2)transform.position + new Vector2(-halfBox.x, -halfBox.y);
                b = (Vector2)transform.position + new Vector2(-halfBox.x, halfBox.y);
                break;
            case PlayState.EDirsCardinal.Right:
                a = (Vector2)transform.position + new Vector2(halfBox.x, -halfBox.y);
                b = (Vector2)transform.position + new Vector2(halfBox.x, halfBox.y);
                break;
            case PlayState.EDirsCardinal.Up:
                a = (Vector2)transform.position + new Vector2(-halfBox.x, halfBox.y);
                b = (Vector2)transform.position + new Vector2(halfBox.x, halfBox.y);
                break;
        }
        lastDistance = PlayState.GetDistance(direction, a, b, 2, enemyCollide);
        return lastDistance;
    }
    private void PlayAttackAnim(float angle, bool convertFirst)
    {
        if (convertFirst)
            angle *= CONVERSION_MULTIPLIER;

        while (angle > 180f)
            angle -= 360f;
        while (angle < -180f)
            angle += 360f;

        string expectedAnimName = "Enemy_skyViper_attack";
        if (angle > 157.5f)
            expectedAnimName += "W";
        else if (angle > 112.5f)
            expectedAnimName += "NW";
        else if (angle > 67.5f)
            expectedAnimName += "N";
        else if (angle > 22.5f)
            expectedAnimName += "NE";
        else if (angle > -22.5f)
            expectedAnimName += "E";
        else if (angle > -67.5f)
            expectedAnimName += "SE";
        else if (angle > -112.5f)
            expectedAnimName += "S";
        else if (angle > -157.5f)
            expectedAnimName += "SW";
        else
            expectedAnimName += "W";
        if (anim.currentAnimName != expectedAnimName)
            anim.Play(expectedAnimName);
    }
}
