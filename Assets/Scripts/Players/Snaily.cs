using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snaily : MonoBehaviour
{
    public const float RUNSPEED_NORMAL = 8;
    public const float JUMPPOWER_NORMAL = 22;
    public const float GRAVITY = 1;
    public const float TERMINAL_VELOCITY = -0.66f;
    public const float HITBOX_X = 1.467508f;
    public const float HITBOX_Y = 0.8745056f;
    public const float HITBOX_SHELL_X = 0.8745056f;
    public const float HITBOX_SHELL_Y = 0.8745056f;
    
    public float[] WEAPON_COOLDOWNS = new float[8];

    private Vector2 velocity = new Vector2(0, 0);
    private Vector2 boxDistances = new Vector2(0, 0);

    private RaycastHit2D boxUp;
    private RaycastHit2D boxDown;
    private RaycastHit2D boxLeft;
    private RaycastHit2D boxRight;

    private RaycastHit2D boxHoriz;
    private RaycastHit2D boxVert;

    public BoxCollider2D box;

    public LayerMask playerCollide;

    void Start()
    {
        box = GetComponent<BoxCollider2D>();

        // Weapon cooldowns; first four are without Devastator, last four are with
        WEAPON_COOLDOWNS[0] = 0f;
        WEAPON_COOLDOWNS[1] = 0f;
        WEAPON_COOLDOWNS[2] = 0.15f;
        WEAPON_COOLDOWNS[3] = 0f;
        WEAPON_COOLDOWNS[4] = 0f;
        WEAPON_COOLDOWNS[5] = 0f;
        WEAPON_COOLDOWNS[6] = 0f;
        WEAPON_COOLDOWNS[7] = 0f;

        // Setting bases for move-checking boxcasts
        boxHoriz = Physics2D.BoxCast(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(box.size.x, box.size.y),
            0,
            Vector2.right,
            boxDistances.x,
            playerCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );
        boxVert = Physics2D.BoxCast(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(box.size.x, box.size.y),
            0,
            Vector2.up,
            boxDistances.y,
            playerCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        
    }
}
