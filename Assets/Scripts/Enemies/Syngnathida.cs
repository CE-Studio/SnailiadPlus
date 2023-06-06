using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Syngnathida : Enemy
{
    private  float X_RADIUS = 4.375f;
    private  float Y_RADIUS = 1.25f;
    private float MOVE_TIME = 1.8f;
    private const float OUT_OF_WATER_TIMEOUT = 1.5f;
    private const float FALL_SPEED = 0.5625f;

    private enum MoveMode
    {
        Wait,
        Cos_UL,
        Cos_DL,
        Cos_UR,
        Cos_DR,
        Semicircle_UL,
        Semicircle_DL,
        Semicircle_UR,
        Semicircle_DR
    };
    private MoveMode mode = MoveMode.Wait;

    private Vector2 moveOrigin;
    private float outOfWaterTimeout = OUT_OF_WATER_TIMEOUT;
    private bool outOfWater = false;
    private float fallSpeed = 0;
    private float elapsed;
    private bool facingLeft;
    private float halfBox;

    private int[] flipMode;
    // >= 0 - how many frames into the current animation the sprite will flip
    // -1 - flip halfway through the curve
    // -2 - flip at the end of the animation

    private RoomTrigger parentRoom;

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(250, 2, 10, true);
        if (PlayState.currentProfile.difficulty == 2)
        {
            MOVE_TIME = 1.3f;
            X_RADIUS = 8.125f;
            Y_RADIUS = 2.5f;
        }
        else if (PlayState.currentProfile.character == "Sluggy")
        {
            MOVE_TIME = 1.9f;
            X_RADIUS = 8.75f;
            Y_RADIUS = 3.125f;
        }
        elapsed = MOVE_TIME;

        anim.Add("Enemy_syngnathida_forward");
        anim.Add("Enemy_syngnathida_turn_up");
        anim.Add("Enemy_syngnathida_turn_down");
        flipMode = PlayState.GetAnim("Enemy_syngnathida_data").frames;

        parentRoom = transform.parent.GetComponent<RoomTrigger>();

        col.TryGetComponent(out BoxCollider2D box);
        halfBox = box.size.y * 0.5f;
    }

    private void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (PlayState.OnScreen(transform.position, col))
        {
            elapsed += Time.deltaTime;
            UpdatePosition();
            HandleOutOfWater();
            if (elapsed >= MOVE_TIME)
            {
                elapsed = 0;
                moveOrigin = transform.position;
                if (PlayState.player.transform.position.x < transform.position.x)
                {
                    if (facingLeft)
                    {
                        if (PlayState.player.transform.position.y > transform.position.y)
                            mode = MoveMode.Cos_UL;
                        else
                            mode = MoveMode.Cos_DL;
                    }
                    else if (PlayState.player.transform.position.y > transform.position.y)
                        mode = MoveMode.Semicircle_UR;
                    else
                        mode = MoveMode.Semicircle_DR;
                }
                else
                {
                    if (!facingLeft)
                    {
                        if (PlayState.player.transform.position.y > transform.position.y)
                            mode = MoveMode.Cos_UR;
                        else
                            mode = MoveMode.Cos_DR;
                    }
                    else if (PlayState.player.transform.position.y > transform.position.y)
                        mode = MoveMode.Semicircle_UL;
                    else
                        mode = MoveMode.Semicircle_DL;
                }
            }
        }
    }

    private float NormalizedSigmoid(float input)
    {
        return 1f / (1f + Mathf.Exp(-(input * 12f - 6f)));
    }

    private void UpdatePosition()
    {
        if (mode == MoveMode.Wait || outOfWater)
            return;

        float lerpValue = NormalizedSigmoid(elapsed / MOVE_TIME);
        switch (mode)
        {
            case MoveMode.Cos_UL:
                transform.position = new Vector2(moveOrigin.x - (X_RADIUS * lerpValue), moveOrigin.y + (Y_RADIUS * (1f - Mathf.Cos(lerpValue * Mathf.PI))));
                break;
            case MoveMode.Cos_DL:
                transform.position = new Vector2(moveOrigin.x - (X_RADIUS * lerpValue), moveOrigin.y - (Y_RADIUS * (1f - Mathf.Cos(lerpValue * Mathf.PI))));
                break;
            case MoveMode.Cos_UR:
                transform.position = new Vector2(moveOrigin.x + (X_RADIUS * lerpValue), moveOrigin.y + (Y_RADIUS * (1f - Mathf.Cos(lerpValue * Mathf.PI))));
                break;
            case MoveMode.Cos_DR:
                transform.position = new Vector2(moveOrigin.x + (X_RADIUS * lerpValue), moveOrigin.y - (Y_RADIUS * (1f - Mathf.Cos(lerpValue * Mathf.PI))));
                break;
            case MoveMode.Semicircle_UL:
                transform.position = new Vector2(moveOrigin.x - (Y_RADIUS * Mathf.Sin(lerpValue * Mathf.PI)),
                    moveOrigin.y + (Y_RADIUS * (1f - Mathf.Cos(lerpValue * Mathf.PI))));
                if (facingLeft)
                {
                    if ((flipMode[1] == -2 && anim.GetCurrentFrame() == anim.currentAnim.frames.Length) ||
                        (flipMode[1] == -1 && lerpValue >= 0.5f) ||
                        (flipMode[1] >= 0 && anim.GetCurrentFrame() >= flipMode[1]))
                    {
                        facingLeft = false;
                        sprite.flipX = false;
                    }
                }
                break;
            case MoveMode.Semicircle_DL:
                transform.position = new Vector2(moveOrigin.x - (Y_RADIUS * Mathf.Sin(lerpValue * Mathf.PI)),
                    moveOrigin.y - (Y_RADIUS * (1f - Mathf.Cos(lerpValue * Mathf.PI))));
                if (facingLeft)
                {
                    if ((flipMode[1] == -2 && anim.GetCurrentFrame() == anim.currentAnim.frames.Length) ||
                        (flipMode[1] == -1 && lerpValue >= 0.5f) ||
                        (flipMode[1] >= 0 && anim.GetCurrentFrame() >= flipMode[1]))
                    {
                        facingLeft = false;
                        sprite.flipX = false;
                    }
                }
                break;
            case MoveMode.Semicircle_UR:
                transform.position = new Vector2(moveOrigin.x + (Y_RADIUS * Mathf.Sin(lerpValue * Mathf.PI)),
                    moveOrigin.y + (Y_RADIUS * (1f - Mathf.Cos(lerpValue * Mathf.PI))));
                if (!facingLeft)
                {
                    if ((flipMode[0] == -2 && anim.GetCurrentFrame() == anim.currentAnim.frames.Length) ||
                        (flipMode[0] == -1 && lerpValue >= 0.5f) ||
                        (flipMode[0] >= 0 && anim.GetCurrentFrame() >= flipMode[0]))
                    {
                        facingLeft = true;
                        sprite.flipX = true;
                    }
                }
                break;
            case MoveMode.Semicircle_DR:
                transform.position = new Vector2(moveOrigin.x + (Y_RADIUS * Mathf.Sin(lerpValue * Mathf.PI)),
                    moveOrigin.y - (Y_RADIUS * (1f - Mathf.Cos(lerpValue * Mathf.PI))));
                if (!facingLeft)
                {
                    if ((flipMode[0] == -2 && anim.GetCurrentFrame() == anim.currentAnim.frames.Length) ||
                        (flipMode[0] == -1 && lerpValue >= 0.5f) ||
                        (flipMode[0] >= 0 && anim.GetCurrentFrame() >= flipMode[0]))
                    {
                        facingLeft = true;
                        sprite.flipX = true;
                    }
                }
                break;
        }
    }

    private void HandleOutOfWater()
    {
        if (transform.position.y + halfBox > parentRoom.GetWaterLevelAt(transform.position.x) && !outOfWater)
        {
            mode = MoveMode.Wait;
            outOfWater = true;
            fallSpeed = 0;
        }
        if (!outOfWater)
            return;
        if (transform.position.y + halfBox > parentRoom.GetWaterLevelAt(transform.position.x) && outOfWater)
        {
            transform.position += fallSpeed * Time.deltaTime * Vector3.down;
            fallSpeed += FALL_SPEED;
            if (transform.position.y + halfBox > parentRoom.GetWaterLevelAt(transform.position.x))
                outOfWaterTimeout = OUT_OF_WATER_TIMEOUT;
        }
        outOfWaterTimeout -= elapsed;
        if (outOfWaterTimeout > 0)
            return;
        outOfWaterTimeout = OUT_OF_WATER_TIMEOUT;
        elapsed = 0;
        moveOrigin = transform.position;
        if (PlayState.player.transform.position.x < transform.position.x)
        {
            if (facingLeft)
                mode = MoveMode.Cos_DL;
            else
                mode = MoveMode.Semicircle_DR;
        }
        else if (facingLeft)
            mode = MoveMode.Semicircle_DL;
        else
            mode = MoveMode.Cos_DR;
        outOfWater = false;
    }
}
