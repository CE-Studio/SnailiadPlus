using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikey1 : Enemy
{
    public const int DIR_FLOOR = 0;
    public const int DIR_WALL_LEFT = 1;
    public const int DIR_WALL_RIGHT = 2;
    public const int DIR_CEILING = 3;
    public const float SPEED = 0.04f;
    private const float GRAVITY = 1.35f;
    private const float TERMINAL_VELOCITY = -0.66f;

    public bool rotation; // Assuming the spikey is tracking the inner edge of a ring, false for clockwise and true for counter-clockwise
    public int direction;
    public bool isFalling = false;
    private float gracePeriod = 0;
    private float velocity = 0;
    private RaycastHit2D hCast;
    private RaycastHit2D vCast;
    private RaycastHit2D groundCheck;

    public Animator anim;
    public Sprite spriteCW;
    public Sprite spritwCCW;
    
    void Awake()
    {
        Begin();
        box.size = new Vector2(0.95f, 0.95f);
        attack = 2;
        defense = 0;
        maxHealth = 70;
        health = 70;
        letsPermeatingShotsBy = true;

        anim = GetComponent<Animator>();

        if (PlayState.IsTileSolid(new Vector2(transform.position.x, transform.position.y - 1), true))
            spawnConditions.Add(DIR_FLOOR);
        else if (PlayState.IsTileSolid(new Vector2(transform.position.x + 1, transform.position.y), true))
            spawnConditions.Add(DIR_WALL_RIGHT);
        else if (PlayState.IsTileSolid(new Vector2(transform.position.x, transform.position.y + 1), true))
            spawnConditions.Add(DIR_CEILING);
        else
            spawnConditions.Add(DIR_WALL_LEFT);
        AdjustGroundChecks();

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
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SwapDir((int)spawnConditions[0]);
        AdjustGroundChecks();
    }

    void FixedUpdate()
    {
        if (gracePeriod != 0)
            gracePeriod = Mathf.Clamp(gracePeriod - SPEED, 0, 1);

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
            transform.position = new Vector2(transform.position.x + (dirToMove.x * SPEED), transform.position.y + (dirToMove.y * SPEED));
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
                anim.Play("Spikey1 down", 0, 0);
                break;
            case DIR_WALL_LEFT:
                direction = DIR_WALL_LEFT;
                anim.Play("Spikey1 left", 0, 0);
                break;
            case DIR_WALL_RIGHT:
                direction = DIR_WALL_RIGHT;
                anim.Play("Spikey1 right", 0, 0);
                break;
            case DIR_CEILING:
                direction = DIR_CEILING;
                anim.Play("Spikey1 up", 0, 0);
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
                    SPEED,
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
                    SPEED,
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
                    SPEED,
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
                    SPEED,
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
