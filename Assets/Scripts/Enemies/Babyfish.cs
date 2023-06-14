using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Babyfish : Enemy
{
    public int type = 0;

    private readonly float[] MOVE_TIMEOUT = { 1.3f, 1.7f };
    private RaycastHit2D wallCheck;
    private float moveTimeout;
    private bool facingLeft;
    private float sinTimer = 0;
    private float velocity = 0;
    private bool readyToMove = false;

    private void Awake()
    {
        Spawn(40, 0, 0, true);
        invulnerable = true;
        canDamage = false;

        wallCheck = Physics2D.Raycast(
            transform.position,
            Vector2.right,
            0.5f,
            enemyCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );
    }

    public void AssignType(int newType)
    {
        type = newType;

        anim.Add("Enemy_babyfish" + (type + 1) + "_normal");
        anim.Add("Enemy_babyfish" + (type + 1) + "_swim");
        anim.Play("Enemy_babyfish" + (type + 1) + "_swim");

        moveTimeout = MOVE_TIMEOUT[type] / type switch { 1 => 4, _ => 8 };

        readyToMove = true;
    }

    private void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game || !readyToMove)
            return;

        wallCheck = Physics2D.Raycast(
            transform.position,
            facingLeft ? Vector2.left : Vector2.right,
            0.5f,
            enemyCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );

        if (PlayState.OnScreen(transform.position, col))
        {
            moveTimeout -= Time.deltaTime;
            if (moveTimeout < 0)
            {
                velocity = 3.75f;
                moveTimeout = MOVE_TIMEOUT[type];
                anim.Play("Enemy_babyfish" + (type + 1) + "_normal");
                anim.Play("Enemy_babyfish" + (type + 1) + "_swim");
                facingLeft = PlayState.player.transform.position.x < transform.position.x;
                if (Random.Range(0f, 1f) > 0.8f)
                    facingLeft = !facingLeft;
            }

            if (wallCheck.collider != null)
                facingLeft = !facingLeft;

            transform.position = new Vector2(transform.position.x + (velocity * (facingLeft ? -1 : 1) * Time.deltaTime), origin.y + 0.25f * Mathf.Sin(sinTimer * 2));
            velocity = Mathf.Lerp(velocity, 0, Time.deltaTime);
        }
        sinTimer = (sinTimer + Time.deltaTime) % (Mathf.PI * 2);

        sprite.flipX = facingLeft;
    }
}
