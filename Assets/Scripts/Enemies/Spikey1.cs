using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikey1 : Enemy
{
    public const int DIR_FLOOR = 0;
    public const int DIR_WALL_LEFT = 1;
    public const int DIR_WALL_RIGHT = 2;
    public const int DIR_CEILING = 3;
    public const float SPEED = 0.03f;

    public bool rotation; // Assuming the spikey is tracking the inner edge of a ring, false for clockwise and true for counter-clockwise
    public int direction;

    public Animator anim;
    
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
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SwapDir((int)spawnConditions[0]);
    }

    void FixedUpdate()
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
    }
}
