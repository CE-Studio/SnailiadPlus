using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball1 : Enemy
{
    public const int DIR_FLOOR = 0;
    public const int DIR_WALL_LEFT = 1;
    public const int DIR_WALL_RIGHT = 2;
    public const int DIR_CEILING = 3;
    public float speed = 0.06f;
    private const float GRAVITY = 1.35f;
    private const float TERMINAL_VELOCITY = -0.66f;

    public bool rotation; // Assuming the fireball is tracking the inner edge of a ring, false for clockwise and true for counter-clockwise
    public int direction;
    public bool isFalling = false;
    private float gracePeriod = 0;
    private float velocity = 0;
    private RaycastHit2D hCast;
    private RaycastHit2D vCast;
    private RaycastHit2D groundCheck;
    private bool updateAnimOnTurn;
    private bool initializedRotation = false;

    private BoxCollider2D box;

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(400, 3, 0, true);
        col.TryGetComponent(out box);
        speed = PlayState.currentProfile.difficulty == 2 ? 0.12f : 0.06f;
        elementType = "fire";

        anim.Add("Enemy_fireball1_down");
        anim.Add("Enemy_fireball1_right");
        anim.Add("Enemy_fireball1_up");
        anim.Add("Enemy_fireball1_left");

        groundCheck = Physics2D.BoxCast(
            transform.position,
            box.size,
            0,
            Vector2.down,
            Mathf.Infinity,
            LayerMask.GetMask("EnemyCollide"),
            Mathf.Infinity,
            Mathf.Infinity
            );

        updateAnimOnTurn = PlayState.GetAnim("Enemy_fireball1_data").frames[0] == 1;

        PlayState.globalFunctions.CreateLightMask(15, transform.position).transform.parent = transform;
    }

    void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (!initializedRotation)
        {
            if (PlayState.IsTileSolid(new Vector2(transform.position.x, transform.position.y - 1), true))
            {
                direction = DIR_FLOOR;
                anim.Play("Enemy_fireball1_down");
            }
            else if (PlayState.IsTileSolid(new Vector2(transform.position.x + 1, transform.position.y), true))
            {
                direction = DIR_WALL_RIGHT;
                anim.Play("Enemy_fireball1_right");
            }
            else if (PlayState.IsTileSolid(new Vector2(transform.position.x, transform.position.y + 1), true))
            {
                direction = DIR_CEILING;
                anim.Play("Enemy_fireball1_up");
            }
            else
            {
                direction = DIR_WALL_LEFT;
                anim.Play("Enemy_fireball1_left");
            }
            AdjustGroundChecks();
            initializedRotation = true;
        }

        if (gracePeriod != 0)
            gracePeriod = Mathf.Clamp(gracePeriod - speed, 0, 1);

        if (isFalling)
        {
            if (groundCheck.collider == null)
            {
                velocity = Mathf.Clamp(velocity - GRAVITY * Time.fixedDeltaTime, TERMINAL_VELOCITY, Mathf.Infinity);
                transform.position = new Vector2(transform.position.x, transform.position.y + velocity);
            }
            else
            {
                velocity = 0;
                transform.position = new Vector2(transform.position.x, Mathf.Floor(transform.position.y - groundCheck.distance + (box.size.y * 0.5f)) + 0.5f);
                SwapDir(DIR_FLOOR);
                isFalling = false;
            }
        }
        else if (vCast.collider == null && gracePeriod == 0)
        {
            transform.position = new Vector2(Mathf.Floor(transform.position.x) + 0.5f, Mathf.Floor(transform.position.y) + 0.5f);
            Turn(!rotation);
            if (!CheckFrontBottomCorner())
                isFalling = true;
        }
        else if (hCast.collider != null)
        {
            transform.position = new Vector2(Mathf.Floor(transform.position.x) + 0.5f, Mathf.Floor(transform.position.y) + 0.5f);
            Turn(rotation);
        }
        else if (vCast.collider != null || (gracePeriod != 0 && CheckFrontBottomCorner()))
        {
            Vector2 dirToMove = Vector2.zero;
            switch (direction)
            {
                case DIR_FLOOR:
                    dirToMove = rotation ? Vector2.right : Vector2.left;
                    break;
                case DIR_WALL_RIGHT:
                    dirToMove = rotation ? Vector2.up : Vector2.down;
                    break;
                case DIR_CEILING:
                    dirToMove = rotation ? Vector2.left : Vector2.right;
                    break;
                case DIR_WALL_LEFT:
                    dirToMove = rotation ? Vector2.down : Vector2.up;
                    break;
            }
            transform.position = new Vector2(transform.position.x + (dirToMove.x * speed), transform.position.y + (dirToMove.y * speed));
        }
        else
        {
            isFalling = true;
        }
        AdjustGroundChecks();
    }

    private void SwapDir(int dir)
    {
        switch (dir)
        {
            case DIR_FLOOR:
                direction = DIR_FLOOR;
                if (updateAnimOnTurn)
                    anim.Play("Enemy_spikey_blue_down");
                break;
            case DIR_WALL_LEFT:
                direction = DIR_WALL_LEFT;
                if (updateAnimOnTurn)
                    anim.Play("Enemy_spikey_blue_left");
                break;
            case DIR_WALL_RIGHT:
                direction = DIR_WALL_RIGHT;
                if (updateAnimOnTurn)
                    anim.Play("Enemy_spikey_blue_right");
                break;
            case DIR_CEILING:
                direction = DIR_CEILING;
                if (updateAnimOnTurn)
                    anim.Play("Enemy_spikey_blue_up");
                break;
        }
        AdjustGroundChecks();
    }

    // Each spikey checks the tile directly in front of it and the relative ground tile diagonally backward
    private void AdjustGroundChecks()
    {
        switch (direction)
        {
            case DIR_FLOOR:
                hCast = Physics2D.BoxCast(
                    transform.position,
                    box.size,
                    0,
                    rotation ? Vector2.right : Vector2.left,
                    speed,
                    enemyCollide,
                    Mathf.Infinity,
                    Mathf.Infinity
                    );
                vCast = Physics2D.BoxCast(
                    transform.position,
                    box.size,
                    0,
                    Vector2.down,
                    1,
                    enemyCollide,
                    Mathf.Infinity,
                    Mathf.Infinity
                    );
                break;
            case DIR_WALL_LEFT:
                hCast = Physics2D.BoxCast(
                    transform.position,
                    box.size,
                    0,
                    rotation ? Vector2.down : Vector2.up,
                    speed,
                    enemyCollide,
                    Mathf.Infinity,
                    Mathf.Infinity
                    );
                vCast = Physics2D.BoxCast(
                    transform.position,
                    box.size,
                    0,
                    Vector2.left,
                    1,
                    enemyCollide,
                    Mathf.Infinity,
                    Mathf.Infinity
                    );
                break;
            case DIR_WALL_RIGHT:
                hCast = Physics2D.BoxCast(
                    transform.position,
                    box.size,
                    0,
                    rotation ? Vector2.up : Vector2.down,
                    speed,
                    enemyCollide,
                    Mathf.Infinity,
                    Mathf.Infinity
                    );
                vCast = Physics2D.BoxCast(
                    transform.position,
                    box.size,
                    0,
                    Vector2.right,
                    1,
                    enemyCollide,
                    Mathf.Infinity,
                    Mathf.Infinity
                    );
                break;
            case DIR_CEILING:
                hCast = Physics2D.BoxCast(
                    transform.position,
                    box.size,
                    0,
                    rotation ? Vector2.left : Vector2.right,
                    speed,
                    enemyCollide,
                    Mathf.Infinity,
                    Mathf.Infinity
                    );
                vCast = Physics2D.BoxCast(
                    transform.position,
                    box.size,
                    0,
                    Vector2.up,
                    1,
                    enemyCollide,
                    Mathf.Infinity,
                    Mathf.Infinity
                    );
                break;
        }
        groundCheck = Physics2D.BoxCast(
            transform.position,
            box.size,
            0,
            Vector2.down,
            velocity,
            enemyCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );
    }

    private void Turn(bool ccw)
    {
        switch (direction)
        {
            case DIR_FLOOR:
                if (ccw)
                    SwapDir(DIR_WALL_RIGHT);
                else
                    SwapDir(DIR_WALL_LEFT);
                break;
            case DIR_WALL_LEFT:
                if (ccw)
                    SwapDir(DIR_FLOOR);
                else
                    SwapDir(DIR_CEILING);
                break;
            case DIR_WALL_RIGHT:
                if (ccw)
                    SwapDir(DIR_CEILING);
                else
                    SwapDir(DIR_FLOOR);
                break;
            case DIR_CEILING:
                if (ccw)
                    SwapDir(DIR_WALL_LEFT);
                else
                    SwapDir(DIR_WALL_RIGHT);
                break;
        }
        gracePeriod = 1;
    }

    private bool CheckFrontBottomCorner()
    {
        if ((direction == DIR_FLOOR && rotation) || (direction == DIR_WALL_RIGHT && !rotation))
            return PlayState.IsTileSolid(new Vector2(transform.position.x + 1, transform.position.y - 1), true);
        else if ((direction == DIR_FLOOR && !rotation) || (direction == DIR_WALL_LEFT && rotation))
            return PlayState.IsTileSolid(new Vector2(transform.position.x - 1, transform.position.y - 1), true);
        else if ((direction == DIR_CEILING && rotation) || (direction == DIR_WALL_LEFT && !rotation))
            return PlayState.IsTileSolid(new Vector2(transform.position.x - 1, transform.position.y + 1), true);
        else
            return PlayState.IsTileSolid(new Vector2(transform.position.x + 1, transform.position.y + 1), true);
    }
}
