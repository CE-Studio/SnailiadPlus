using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinnygear : Enemy
{
    private const float DETECT_DISTANCE = 5.625f;
    private const float START_DISTANCE = 5f;

    private readonly Vector2 chargeSpeed = new Vector2(31.25f, 25.625f);
    private Vector2 velocity = Vector2.zero;
    private PlayState.EDirsCardinal direction;
    private bool isActive = false;
    private bool canAttack = true;

    private bool alignCornerToTile = true;

    private BoxCollider2D box;
    private Vector2 halfBox;

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(500, 4, 1, true);
        col.TryGetComponent(out box);
        halfBox = box.size * 0.5f;

        if (alignCornerToTile)
        {
            transform.position += new Vector3(0.5f, -0.5f, 0);
            origin += new Vector2(0.5f, -0.5f);
        }

        anim.Add("Enemy_gear_idle");
        anim.Add("Enemy_gear_down");
        anim.Add("Enemy_gear_left");
        anim.Add("Enemy_gear_right");
        anim.Add("Enemy_gear_up");
        anim.Play("Enemy_gear_idle");
    }

    public void SetDirection(PlayState.EDirsCardinal newDir)
    {
        direction = newDir;
        transform.position += START_DISTANCE * newDir switch
        {
            PlayState.EDirsCardinal.Down => Vector3.up,
            PlayState.EDirsCardinal.Left => Vector3.right,
            PlayState.EDirsCardinal.Right => Vector3.left,
            _ => Vector3.down
        };
    }

    private void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        if (isActive)
        {
            switch (direction)
            {
                default:
                case PlayState.EDirsCardinal.Down:
                    velocity.y += chargeSpeed.y * Time.deltaTime;
                    if (transform.localPosition.y > origin.y + START_DISTANCE)
                    {
                        velocity.y = 0;
                        transform.localPosition = origin + (START_DISTANCE * Vector2.up);
                        isActive = false;
                        anim.Play("Enemy_gear_idle");
                        if (PlayState.currentProfile.difficulty == 2)
                            canAttack = true;
                    }
                    break;
                case PlayState.EDirsCardinal.Left:
                    velocity.x += chargeSpeed.x * Time.deltaTime;
                    if (transform.localPosition.x > origin.x + START_DISTANCE)
                    {
                        velocity.x = 0;
                        transform.localPosition = origin + (START_DISTANCE * Vector2.right);
                        isActive = false;
                        anim.Play("Enemy_gear_idle");
                        if (PlayState.currentProfile.difficulty == 2)
                            canAttack = true;
                    }
                    break;
                case PlayState.EDirsCardinal.Right:
                    velocity.x -= chargeSpeed.x * Time.deltaTime;
                    if (transform.localPosition.x < origin.x - START_DISTANCE)
                    {
                        velocity.x = 0;
                        transform.localPosition = origin + (START_DISTANCE * Vector2.left);
                        isActive = false;
                        anim.Play("Enemy_gear_idle");
                        if (PlayState.currentProfile.difficulty == 2)
                            canAttack = true;
                    }
                    break;
                case PlayState.EDirsCardinal.Up:
                    velocity.y -= chargeSpeed.y * Time.deltaTime;
                    if (transform.localPosition.y < origin.y - START_DISTANCE)
                    {
                        velocity.y = 0;
                        transform.localPosition = origin + (START_DISTANCE * Vector2.down);
                        isActive = false;
                        anim.Play("Enemy_gear_idle");
                        if (PlayState.currentProfile.difficulty == 2)
                            canAttack = true;
                    }
                    break;
            }
            transform.position += (Vector3)velocity * Time.deltaTime;
        }
        else if (canAttack)
        {
            if (direction == PlayState.EDirsCardinal.Up || direction == PlayState.EDirsCardinal.Down)
            {
                if (Mathf.Abs(PlayState.player.transform.position.x - transform.position.x - halfBox.x) < DETECT_DISTANCE)
                {
                    isActive = true;
                    canAttack = false;
                    velocity.y = chargeSpeed.y * (direction == PlayState.EDirsCardinal.Up ? 1 : -1);
                    anim.Play(direction == PlayState.EDirsCardinal.Up ? "Enemy_gear_up" : "Enemy_gear_down");
                }
            }
            else
            {
                if (Mathf.Abs(PlayState.player.transform.position.y - transform.position.y + halfBox.y) < DETECT_DISTANCE)
                {
                    isActive = true;
                    canAttack = false;
                    velocity.x = chargeSpeed.x * (direction == PlayState.EDirsCardinal.Right ? 1 : -1);
                    anim.Play(direction == PlayState.EDirsCardinal.Right ? "Enemy_gear_right" : "Enemy_gear_left");
                }
            }
        }
    }
}
