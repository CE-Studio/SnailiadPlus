using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snakey1 : Enemy
{
    private const float MOVE_TIMEOUT = 1.2f;
    private const float REACT_DISTANCE = 6.25f;
    private const float SPEED = 11.25f;
    private const float DECELERATION = 0.47f;
    private const float ACCELERATION = 1.25f;

    private float moveTimeout = MOVE_TIMEOUT;
    private Vector2 velocity = Vector2.zero;
    private bool grounded = true;
    private BoxCollider2D box;
    private Vector2 halfBox;
    private float lastDistance;
    private float lastX;

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(250, 4, 10, true, 4);
        col.TryGetComponent(out box);
        halfBox = box.size * 0.5f;
        lastX = transform.position.x;

        anim.Add("Enemy_snakey_green_idle");
        anim.Add("Enemy_snakey_green_attack");
        anim.Play("Enemy_snakey_green_idle");
    }

    private void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (PlayState.OnScreen(transform.position, col))
        {
            moveTimeout -= Time.fixedDeltaTime;
            float playerDistance = PlayState.player.transform.position.x - transform.position.x;
            if (velocity.x != 0)
            {
                if (Mathf.Abs(velocity.x) < DECELERATION)
                    velocity.x = 0;
                else
                    velocity.x -= DECELERATION * Mathf.Sign(velocity.x);
            }
            if (!grounded)
                velocity.y -= ACCELERATION;
            if (moveTimeout <= 0 && Mathf.Abs(playerDistance) < REACT_DISTANCE)
            {
                moveTimeout = MOVE_TIMEOUT;
                velocity.x = SPEED * Mathf.Sign(playerDistance);
                anim.Play("Enemy_snakey_green_attack");
            }

            if (GetDistance(velocity.x < 0 ? PlayState.EDirsCardinal.Left : PlayState.EDirsCardinal.Right) < Mathf.Abs(velocity.x * Time.fixedDeltaTime))
            {
                transform.position += (lastDistance - PlayState.FRAC_32) * (velocity.x < 0 ? Vector3.left : Vector3.right);
                velocity.x *= -1;
            }
            GetDistance(PlayState.EDirsCardinal.Down);
            if (lastDistance < Mathf.Abs(velocity.y * Time.fixedDeltaTime) && !grounded)
            {
                transform.position += (lastDistance - PlayState.FRAC_32) * Vector3.down;
                velocity.y = 0;
                grounded = true;
                anim.Play("Enemy_snakey_green_idle");
            }
            else if (lastDistance > 0.25f && grounded)
                grounded = false;

            transform.position += (Vector3)velocity * Time.fixedDeltaTime;

            if (velocity.x != 0)
            {
                Vector2 testPoint = new Vector2(transform.position.x + (halfBox.x * (velocity.x > 0 ? 1 : -1)), transform.position.y - halfBox.y);
                if (PlayState.IsPointEnemyCollidable(testPoint))
                    transform.position = new Vector2(lastX, transform.position.y);
            }
            lastX = transform.position.x;
        }

        if (velocity.x != 0)
            sprite.flipX = velocity.x > 0;
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
        }
        lastDistance = PlayState.GetDistance(direction, a, b, 2, enemyCollide);
        return lastDistance;
    }
}
