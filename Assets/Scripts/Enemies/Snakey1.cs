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

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(250, 4, 10, true);
        col.TryGetComponent(out box);

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
            Vector2 pos = transform.position;

            RaycastHit2D wallHit = Physics2D.BoxCast(transform.position, new Vector2(box.size.x, box.size.y * 0.5f), 0,
                Vector2.right * Mathf.Sign(velocity.x), velocity.x * Time.fixedDeltaTime, enemyCollide);
            RaycastHit2D floorLeft = Physics2D.Raycast(new Vector2(pos.x - (box.size.x * 0.5f), pos.y - (box.size.y * 0.5f)), Vector2.down, 20, enemyCollide);
            RaycastHit2D floorRight = Physics2D.Raycast(new Vector2(pos.x + (box.size.x * 0.5f), pos.y - (box.size.y * 0.5f)), Vector2.down, 20, enemyCollide);

            if (wallHit.collider != null)
                velocity.x *= -1;
            if ((floorLeft.distance < -(velocity.y * Time.fixedDeltaTime) || floorRight.distance < -(velocity.y * Time.fixedDeltaTime)) && !grounded)
            {
                transform.position = new Vector2(transform.position.x,
                    (floorLeft.point.y > floorRight.point.y ? floorLeft.point.y : floorRight.point.y) + (box.size.y * 0.5f) + 0.05f);
                grounded = true;
                velocity.y = 0;
                anim.Play("Enemy_snakey_green_idle");
            }
            else if (floorLeft.distance > 0.25f && floorRight.distance > 0.25f && grounded)
                grounded = false;
            transform.position += (Vector3)velocity * Time.fixedDeltaTime;
        }

        if (velocity.x != 0)
            sprite.flipX = velocity.x > 0;
    }
}
