﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public const int DIR_FLOOR = 0;

    public const int DIR_WALL = 1;

    public const int DIR_CEILING = 2;

    public const float HITBOX_SIZEX = 1.467508f;

    public const float HITBOX_SIZEY = 0.8745056f;

    public const float HITBOX_SIZEX_SHELL = 0.8421245f;

    public const float HITBOX_OFFSETX = 0f;

    public const float HITBOX_OFFSETY = -0.560989f;

    public const float HITBOX_OFFSETX_SHELL = -0.186518f;

    public const float RUNSPEED_NORMAL = 8;

    public const float JUMPPOWER_NORMAL = 22;

    public const float GRAVITY = 1f;

    public const float TERMINAL_VELOCITY = -0.66f;


    private bool _inShell = false;

    private int _currentSurface = 0;

    private bool _facingLeft = false;

    private bool _facingUp = false;

    private bool _relativeLeft = false;

    private bool _relativeRight = false;

    private bool _relativeUp = false;

    private bool _relativeDown = false;

    private bool _justPressedLeft = false;

    private bool _justPressedRight = false;

    private bool _justPressedUp = false;

    private bool _justPressedDown = false;

    private bool _holdingLeft = false;

    private bool _holdingRight = false;

    private bool _holdingUp = false;

    private bool _holdingDown = false;

    private Vector2 _velocity = new Vector2(0, 0);

    private bool _onSurface = false;

    private bool _surfacedLastFrame = false;

    private bool _justJumped = false;

    private bool _holdingJump = false;

    private bool _justLeftShell = false;

    private float _lastVcheckHitX = 0f;

    private float _lastV2checkHitX = 0f;

    private bool _justGrabbedWall = false;

    private bool _readyToRoundCorner = false;


    public Animator anim;

    public SpriteRenderer sprite;

    public BoxCollider2D box;

    public AudioSource sfx;

    public AudioClip shell;

    public AudioClip jump;


    public LayerMask playerCollide;


    public GameObject debugUp;

    public GameObject debugDown;

    public GameObject debugLeft;

    public GameObject debugRight;

    public GameObject debugJump;

    public Sprite keyIdle;

    public Sprite keyPressed;

    public Sprite keyHeld;


    // Start() is called at the very beginning of the script's lifetime. It's used to initialize certain variables and states for components to be in.
    void Start()
    {
        // All this does is set Snaily's components to simpler variables that can be more easily called
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        sfx = GetComponent<AudioSource>();

        // This line here calls a separate script intended to handle much of the general heavy lifting of the program down the line. As of now, though, its only function is
        // to play the background music.
        PlayState.GetNewRoom("Test Zone");

        debugUp = GameObject.Find("View/Debug Keypress Indicators/Up");
        debugDown = GameObject.Find("View/Debug Keypress Indicators/Down");
        debugLeft = GameObject.Find("View/Debug Keypress Indicators/Left");
        debugRight = GameObject.Find("View/Debug Keypress Indicators/Right");
        debugJump = GameObject.Find("View/Debug Keypress Indicators/Jump");

        StartCoroutine("DebugKeys");
    }

    // FixedUpdate() is called repeatedly over a fixed duration of time (every 0.02 seconds)
    void FixedUpdate()
    {
        if (PlayState.gameState == "Game")
        {
            anim.speed = 1;
            // This is a boxcast meant to test the player's X movement
            RaycastHit2D checkHoriz = Physics2D.BoxCast(
                new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y),
                new Vector2(box.size.x - 0.125f, box.size.y - 0.125f),
                0,
                Vector2.right,
                _velocity.x * Time.fixedDeltaTime,
                playerCollide,
                Mathf.Infinity,
                Mathf.Infinity
                );
            // And this one tests their Y movement
            RaycastHit2D checkVert = Physics2D.BoxCast(
                new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y),
                new Vector2(box.size.x - 0.125f, box.size.y - 0.125f),
                0,
                Vector2.up,
                _velocity.y * Time.fixedDeltaTime,
                playerCollide,
                Mathf.Infinity,
                Mathf.Infinity
                );
            if (checkVert.point.x != 0)
            {
                _lastVcheckHitX = checkVert.point.x;
            }
            // This is just a secondary vertical boxcast, mainly used in the ground state to check for valid jumps
            RaycastHit2D checkVertSecondary = Physics2D.BoxCast(
                new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y),
                new Vector2(box.size.x - 0.125f, box.size.y - 0.125f),
                0,
                Vector2.up,
                Mathf.Infinity,
                playerCollide,
                Mathf.Infinity,
                Mathf.Infinity
                );
            //if (checkVert.point.x != 0)
            //{
            //    _lastV2checkHitX = checkVertSecondary.point.x;
            //}
            RaycastHit2D jumpChecker = Physics2D.BoxCast(
                new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y),
                new Vector2(box.size.x - 0.125f, box.size.y - 0.125f),
                0,
                Vector2.up,
                Mathf.Infinity,
                playerCollide,
                Mathf.Infinity,
                Mathf.Infinity
                );
            if (_currentSurface == 1 && _facingLeft)
            {
                jumpChecker = Physics2D.BoxCast(
                    new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y),
                    new Vector2(box.size.x - 0.125f, box.size.y - 0.125f),
                    0,
                    Vector2.right,
                    Mathf.Infinity,
                    playerCollide,
                    Mathf.Infinity,
                    Mathf.Infinity
                    );
            }
            else if (_currentSurface == 1 && !_facingLeft)
            {
                jumpChecker = Physics2D.BoxCast(
                    new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y),
                    new Vector2(box.size.x - 0.125f, box.size.y - 0.125f),
                    0,
                    -Vector2.right,
                    Mathf.Infinity,
                    playerCollide,
                    Mathf.Infinity,
                    Mathf.Infinity
                    );
            }
            else if (_currentSurface == 2)
            {
                jumpChecker = Physics2D.BoxCast(
                    new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y),
                    new Vector2(box.size.x - 0.125f, box.size.y - 0.125f),
                    0,
                    -Vector2.up,
                    Mathf.Infinity,
                    playerCollide,
                    Mathf.Infinity,
                    Mathf.Infinity
                    );
            }
            if (checkVert.point.x != 0)
            {
                _lastV2checkHitX = jumpChecker.point.x;
            }
            Debug.DrawLine(
                new Vector2(checkVert.point.x - 0.25f, checkVert.point.y - 0.25f),
                new Vector2(checkVert.point.x + 0.25f, checkVert.point.y + 0.25f),
                Color.red,
                0,
                false
                );
            Debug.DrawLine(
                new Vector2(checkVert.point.x + 0.25f, checkVert.point.y - 0.25f),
                new Vector2(checkVert.point.x - 0.25f, checkVert.point.y + 0.25f),
                Color.red,
                0,
                false
                );
            Debug.DrawLine(
                new Vector2(jumpChecker.point.x - 0.25f, jumpChecker.point.y - 0.25f),
                new Vector2(jumpChecker.point.x + 0.25f, jumpChecker.point.y + 0.25f),
                Color.yellow,
                0,
                false
                );
            Debug.DrawLine(
                new Vector2(jumpChecker.point.x + 0.25f, jumpChecker.point.y - 0.25f),
                new Vector2(jumpChecker.point.x - 0.25f, jumpChecker.point.y + 0.25f),
                Color.yellow,
                0,
                false
                );
            // This raycast, dependent on where exactly checkVert found a collider, returns the exact distance from the player to the ground
            RaycastHit2D distanceFromGround = Physics2D.Raycast(
                new Vector2(_lastVcheckHitX, transform.position.y + box.offset.y),
                new Vector2(0, -1),
                Mathf.Infinity,
                playerCollide,
                Mathf.Infinity,
                Mathf.Infinity
                );
            // Similarly, this one travels in both X directions to return the wall distance
            RaycastHit2D distanceFromWall;
            if (_facingLeft)
            {
                distanceFromWall = Physics2D.Raycast(
                    new Vector2(transform.position.x, transform.position.y + box.offset.y),
                    new Vector2(-1, 0),
                    1,
                    playerCollide,
                    Mathf.Infinity,
                    Mathf.Infinity
                    );
            }
            else
            {
                distanceFromWall = Physics2D.Raycast(
                    new Vector2(transform.position.x, transform.position.y + box.offset.y),
                    new Vector2(1, 0),
                    1,
                    playerCollide,
                    Mathf.Infinity,
                    Mathf.Infinity
                    );
            }
            // And this one checks the ceiling; essentially distanceFromFloor in reverse
            RaycastHit2D distanceFromCeiling = Physics2D.Raycast(
                new Vector2(_lastV2checkHitX, transform.position.y + box.offset.y),
                new Vector2(0, 1),
                Mathf.Infinity,
                playerCollide,
                Mathf.Infinity,
                Mathf.Infinity
                );
            Debug.DrawLine(
                new Vector2(_lastVcheckHitX, transform.position.y + box.offset.y),
                distanceFromGround.point,
                Color.red,
                0,
                false
                );
            Debug.DrawLine(
                new Vector2(_lastV2checkHitX, transform.position.y + box.offset.y),
                distanceFromCeiling.point,
                Color.yellow,
                0,
                false
                );

            // This switch statement essentially makes sure Snaily's hitbox is the right shape and positioned correctly based on the current surface and shell state
            switch (_currentSurface)
            {
                case 0:
                    if (_inShell)
                    {
                        box.size = new Vector2(HITBOX_SIZEX_SHELL, HITBOX_SIZEY);
                        box.offset = new Vector2(HITBOX_OFFSETX_SHELL, HITBOX_OFFSETY);
                        if (_facingLeft)
                        {
                            box.offset = new Vector2(-box.offset.x, box.offset.y);
                        }
                    }
                    else
                    {
                        box.size = new Vector2(HITBOX_SIZEX, HITBOX_SIZEY);
                        box.offset = new Vector2(HITBOX_OFFSETX, HITBOX_OFFSETY);

                        int tempCheckDir = 1;
                        if (!_facingLeft)
                        {
                            tempCheckDir = -1;
                        }
                        RaycastHit2D tempReverseWallDistance = Physics2D.Raycast(
                            new Vector2(transform.position.x, transform.position.y + box.offset.y),
                            new Vector2(1, 0),
                            tempCheckDir,
                            playerCollide,
                            Mathf.Infinity,
                            Mathf.Infinity
                            );
                        if (_justLeftShell && distanceFromWall.collider != null)
                        {
                            if (_facingLeft)
                            {
                                transform.position += new Vector3(0.920272f - distanceFromWall.distance, 0, 0);
                            }
                            else
                            {
                                transform.position -= new Vector3(0.920272f - distanceFromWall.distance, 0, 0);
                            }
                            _justLeftShell = false;
                        }
                        else if (_justLeftShell && tempReverseWallDistance.collider != null)
                        {
                            if (_facingLeft)
                            {
                                transform.position -= new Vector3(0.125f + tempReverseWallDistance.distance, 0, 0);
                            }
                            else
                            {
                                transform.position += new Vector3(0.125f + tempReverseWallDistance.distance, 0, 0);
                            }
                            _justLeftShell = false;
                        }
                    }
                    break;
                case 1:
                    if (_inShell)
                    {
                        box.size = new Vector2(HITBOX_SIZEY, HITBOX_SIZEX_SHELL);
                        box.offset = new Vector2(-HITBOX_OFFSETY, HITBOX_OFFSETX_SHELL);
                        if (_facingLeft)
                        {
                            box.offset = new Vector2(-box.offset.x, box.offset.y);
                        }
                        if (_facingUp)
                        {
                            box.offset = new Vector2(box.offset.x, -box.offset.y);
                        }
                    }
                    else
                    {
                        box.size = new Vector2(HITBOX_SIZEY, HITBOX_SIZEX);
                        box.offset = new Vector2(-HITBOX_OFFSETY, HITBOX_OFFSETX);
                        if (_facingLeft)
                        {
                            box.offset = new Vector2(-box.offset.x, box.offset.y);
                        }
                        if (_facingUp)
                        {
                            box.offset = new Vector2(box.offset.x, -box.offset.y);
                        }
                    }
                    break;
                case 2:
                    if (_inShell)
                    {
                        box.size = new Vector2(HITBOX_SIZEX_SHELL, HITBOX_SIZEY);
                        box.offset = new Vector2(HITBOX_OFFSETX_SHELL, -HITBOX_OFFSETY);
                        if (_facingLeft)
                        {
                            box.offset = new Vector2(-box.offset.x, box.offset.y);
                        }
                    }
                    else
                    {
                        box.size = new Vector2(HITBOX_SIZEX, HITBOX_SIZEY);
                        box.offset = new Vector2(HITBOX_OFFSETX, -HITBOX_OFFSETY);
                    }
                    break;
            }

            // Ensuring if the player is on the ground or not (Currently unused as it was previously causing problems)
            //if ((_currentSurface == 0 && distanceFromGround.distance <= 0.55f) || _currentSurface != 0)
            //{
            //    _onSurface = true;
            //}
            //else
            //{
            //    _onSurface = false;
            //}

            // Jump controls!
            //Debug.Log("Distance from ceiling: " + distanceFromCeiling.distance + ", on surface: " + _onSurface + ", just jumped: " + _justJumped + ", holding jump: " + _holdingJump);
            if (Input.GetAxisRaw("Jump") == 1 && _onSurface && !_justJumped && !_holdingJump)
            {
                // If we're on a wall/ceiling, disconnect from it and return to facing the ground
                if (_currentSurface != 0)
                {
                    Debug.DrawLine(
                        new Vector2(jumpChecker.point.x - 0.25f, jumpChecker.point.y - 0.25f),
                        new Vector2(jumpChecker.point.x + 0.25f, jumpChecker.point.y + 0.25f),
                        Color.green,
                        1,
                        false
                        );
                    Debug.DrawLine(
                        new Vector2(jumpChecker.point.x + 0.25f, jumpChecker.point.y - 0.25f),
                        new Vector2(jumpChecker.point.x - 0.25f, jumpChecker.point.y + 0.25f),
                        Color.green,
                        1,
                        false
                        );
                    if (((_currentSurface == 1 && Vector2.Distance(
                        new Vector2(transform.position.x + box.offset.x, jumpChecker.point.y),
                        jumpChecker.point
                        ) > 0.65f) || Vector2.Distance(
                            new Vector2(jumpChecker.point.x, transform.position.y + box.offset.y),
                            jumpChecker.point
                            ) > 0.65f) && !_holdingJump)
                    {
                        if (_facingLeft && _currentSurface == DIR_WALL)
                        {
                            transform.position += new Vector3(0.125f, 0, 0);
                        }
                        else if (!_facingLeft && _currentSurface == DIR_WALL)
                        {
                            transform.position += new Vector3(-0.125f, 0, 0);
                        }
                        else if (_currentSurface == DIR_CEILING)
                        {
                            transform.position += new Vector3(0, 1, 0);
                        }
                        box.size = new Vector2(HITBOX_SIZEX, HITBOX_SIZEY);
                        box.offset = new Vector2(HITBOX_OFFSETX, HITBOX_OFFSETY);
                        _currentSurface = 0;
                        _onSurface = false;
                        anim.SetInteger("currentSurface", 0);
                        sprite.flipY = false;
                        sfx.PlayOneShot(jump);
                        _justJumped = true;
                        _inShell = false;
                        anim.SetBool("inShell", false);
                    }
                }
                // If not, jump normally
                else
                {
                    if (((_currentSurface == 1 && Vector2.Distance(
                        new Vector2(transform.position.x + box.offset.x, jumpChecker.point.y),
                        jumpChecker.point
                        ) > 0.65f) || Vector2.Distance(
                            new Vector2(jumpChecker.point.x, transform.position.y + box.offset.y),
                            jumpChecker.point
                            ) > 0.65f) && !_holdingJump)
                    {
                        sfx.PlayOneShot(jump);
                        _justJumped = true;
                        _velocity.y = JUMPPOWER_NORMAL;
                        _onSurface = false;
                        transform.position += new Vector3(0, JUMPPOWER_NORMAL * Time.fixedDeltaTime, 0);
                        _inShell = false;
                        anim.SetBool("inShell", false);
                    }
                    else
                    {
                        _onSurface = true;
                        _justJumped = false;
                        _holdingJump = true;
                    }
                }
            }
            // If we just jumped and have left the ground, then tell the code we're holding the jump button
            if (_justJumped || (!_justJumped && !_holdingJump && Input.GetAxisRaw("Jump") == 1))
            {
                _justJumped = false;
                _holdingJump = true;
            }
            if (_holdingJump && Input.GetAxisRaw("Jump") != 1)
            {
                _holdingJump = false;
            }

            // Move controls!
            switch (_currentSurface)
            {
                // Floor
                case 0:
                    // First, we run horizontal checks
                    // If our wall-finding boxcast found something to run into, move forward the distance plus some offset
                    if (checkHoriz.collider != null)
                    {
                        if (_facingLeft)
                        {
                            transform.position += new Vector3(-checkHoriz.distance + 0.0625f, 0, 0);
                            // And don't forget to check to see if we want to climb a wall!
                            if (_relativeUp)
                            {
                                _currentSurface = DIR_WALL;
                                anim.SetInteger("currentSurface", 1);
                                box.size = new Vector2(HITBOX_SIZEY, HITBOX_SIZEX);
                                box.offset = new Vector2(HITBOX_OFFSETY, HITBOX_OFFSETX);
                                transform.position += new Vector3(0.25f, -0.25f, 0);
                                _justGrabbedWall = true;
                            }
                            else if (_relativeDown && !_onSurface)
                            {
                                _currentSurface = DIR_WALL;
                                anim.SetInteger("currentSurface", 1);
                                box.size = new Vector2(HITBOX_SIZEY, HITBOX_SIZEX);
                                box.offset = new Vector2(HITBOX_OFFSETY, HITBOX_OFFSETX);
                                transform.position += new Vector3(0.25f, -0.25f, 0);
                                _justGrabbedWall = true;
                            }
                        }
                        else
                        {
                            transform.position += new Vector3(checkHoriz.distance - 0.0625f, 0, 0);
                            if (_relativeUp)
                            {
                                _currentSurface = DIR_WALL;
                                anim.SetInteger("currentSurface", 1);
                                box.size = new Vector2(HITBOX_SIZEY, HITBOX_SIZEX);
                                box.offset = new Vector2(HITBOX_OFFSETY, HITBOX_OFFSETX);
                                transform.position += new Vector3(-0.25f, -0.25f, 0);
                                _justGrabbedWall = true;
                                _onSurface = true;
                            }
                            else if (_relativeDown && !_onSurface)
                            {
                                _currentSurface = DIR_WALL;
                                anim.SetInteger("currentSurface", 1);
                                box.size = new Vector2(HITBOX_SIZEY, HITBOX_SIZEX);
                                box.offset = new Vector2(HITBOX_OFFSETY, HITBOX_OFFSETX);
                                transform.position += new Vector3(-0.25f, -0.25f, 0);
                                _justGrabbedWall = true;
                                _onSurface = true;
                            }
                        }
                        _velocity.x = 0;
                    }
                    // If it didn't find anything, simply move however far the current runspeed will allow
                    else
                    {
                        transform.position += new Vector3(_velocity.x * Time.fixedDeltaTime, 0, 0);
                    }

                    // Now we make vertical checks
                    RaycastHit2D tempVCheck = Physics2D.BoxCast(
                        new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y),
                        new Vector2(box.size.x - 0.125f, box.size.y - 0.25f),
                        0,
                        new Vector2(0, -1),
                        1,
                        playerCollide,
                        Mathf.Infinity,
                        Mathf.Infinity
                        );
                    // Because on the ground vertical movement is different, being worked by gravity instead of direct movement control, we work off different
                    // conditions to find valid moves
                    // This first one assumes we're in the air and looking for walls in a direction based off our vertical velocity
                    if (tempVCheck.collider == null)
                    {
                        _onSurface = false;
                    }
                    if (checkVert.collider != null && !_onSurface)
                    {
                        if (Mathf.Sign(_velocity.y) == -1)
                        {
                            transform.position = new Vector3(transform.position.x, distanceFromGround.point.y + (box.size.y * 0.5f) - box.offset.y, 0);
                            _onSurface = true;
                        }
                        else
                        {
                            transform.position += new Vector3(0, checkVert.distance - 0.0625f, 0);
                            if (_relativeUp)
                            {
                                _currentSurface = DIR_CEILING;
                                sprite.flipY = true;
                                anim.SetInteger("currentSurface", 0);
                                transform.position += new Vector3(0, -1.125f, 0);
                                _justGrabbedWall = true;
                                _onSurface = true;
                                _holdingDown = true;
                            }
                        }
                        _velocity.y = 0;
                    }
                    // This next one ensures we don't fall through floors and stay grounded
                    else if (distanceFromGround.distance <= 0.55 && _onSurface)
                    {
                        transform.position = new Vector3(transform.position.x, distanceFromGround.point.y + (box.size.y * 0.5f) - box.offset.y, 0);
                        _velocity.y = 0;
                        _readyToRoundCorner = true;
                    }
                    // This pair is here to work crawling around ground corners
                    else if (!_facingLeft && _relativeDown && _relativeRight && (_onSurface || _surfacedLastFrame) && _readyToRoundCorner && !_justJumped && _velocity.y <= 0)
                    {
                        _currentSurface = DIR_WALL;
                        anim.SetInteger("currentSurface", 1);
                        box.size = new Vector2(HITBOX_SIZEY, HITBOX_SIZEX);
                        box.offset = new Vector2(HITBOX_OFFSETY, HITBOX_OFFSETX);
                        transform.position = new Vector3(Mathf.RoundToInt(transform.position.x + 0.25f), transform.position.y - 0.25f, 0);
                        _justGrabbedWall = true;
                        _onSurface = true;
                        _facingLeft = true;
                        _readyToRoundCorner = false;
                    }
                    else if (_facingLeft && _relativeDown && _relativeLeft && (_onSurface || _surfacedLastFrame) && _readyToRoundCorner && !_justJumped && _velocity.y <= 0)
                    {
                        _currentSurface = DIR_WALL;
                        anim.SetInteger("currentSurface", 1);
                        box.size = new Vector2(HITBOX_SIZEY, HITBOX_SIZEX);
                        box.offset = new Vector2(-HITBOX_OFFSETY, HITBOX_SIZEX);
                        transform.position = new Vector3(Mathf.RoundToInt(transform.position.x - 0.25f), transform.position.y - 0.25f, 0);
                        _justGrabbedWall = true;
                        _onSurface = true;
                        _facingLeft = false;
                        _readyToRoundCorner = false;
                    }
                    // This last one applies to unhindered movement through the air, moving based off our current velocity minus gravity
                    else
                    {
                        transform.position += new Vector3(0, Mathf.Clamp(_velocity.y * Time.fixedDeltaTime, TERMINAL_VELOCITY, Mathf.Infinity), 0);
                        // We get a higher jump by holding the jump button if we decrease the gravity scale on the upward half of a jump while the button is down!
                        if (_velocity.y > 0 && Input.GetAxisRaw("Jump") != 1)
                        {
                            _velocity.y -= GRAVITY * 2.5f;
                        }
                        else
                        {
                            _velocity.y -= GRAVITY;
                        }
                        _onSurface = false;
                    }
                    //Debug.Log(_onSurface);
                    if (_onSurface)
                    {
                        _surfacedLastFrame = true;
                    }
                    else
                    {
                        _surfacedLastFrame = false;
                    }
                    break;
                // Walls
                case 1:
                    _onSurface = true;
                    // This just makes sure our horizontal velocity is facing the current wall to check if we're on it; our horizontal position shouldn't be updated
                    // otherwise in this state
                    if (_facingLeft)
                    {
                        _velocity.x = -RUNSPEED_NORMAL;
                    }
                    else
                    {
                        _velocity.x = RUNSPEED_NORMAL;
                    }

                    // This checks for walls we might run into on our wall-crawling journey
                    if (checkVert.collider != null)
                    {
                        // In the case that we're going down, automatically enter the floor state with the required directional and hitbox adjustments
                        if (Mathf.Sign(_velocity.y) == -1)
                        {
                            transform.position += new Vector3(0, -checkVert.distance + 0.0625f, 0);
                            anim.SetInteger("currentSurface", 0);
                            _currentSurface = DIR_FLOOR;
                            box.size = new Vector2(HITBOX_SIZEX, HITBOX_SIZEY);
                            box.offset = new Vector2(HITBOX_OFFSETX, HITBOX_OFFSETY);
                            _holdingDown = true;
                            if (_facingLeft)
                            {
                                _facingLeft = false;
                                sprite.flipX = false;
                                transform.position += new Vector3(-0.25f, 0.25f, 0);
                            }
                            else
                            {
                                _facingLeft = true;
                                sprite.flipX = true;
                                transform.position += new Vector3(0.25f, 0.25f, 0);
                            }
                        }
                        // If we're going up, just move the required amount and stop
                        else
                        {
                            transform.position += new Vector3(0, checkVert.distance - 0.0625f, 0);
                            if (checkVert.distance == 0)
                            {
                                transform.position += new Vector3(0, -distanceFromCeiling.distance, 0);
                            }
                            // ...UNLESS the player specifically states they want to move to the ceiling
                            if (_facingLeft && _relativeUp)
                            {
                                anim.SetInteger("currentSurface", 0);
                                _currentSurface = DIR_CEILING;
                                _facingLeft = false;
                                sprite.flipX = false;
                                box.size = new Vector2(HITBOX_SIZEX, HITBOX_SIZEY);
                                box.offset = new Vector2(HITBOX_OFFSETX, -HITBOX_OFFSETY);
                                _holdingDown = true;
                                transform.position += new Vector3(-0.25f, -0.25f, 0);
                            }
                            else if (!_facingLeft && _relativeUp)
                            {
                                anim.SetInteger("currentSurface", 0);
                                _currentSurface = DIR_CEILING;
                                _facingLeft = true;
                                sprite.flipX = true;
                                box.size = new Vector2(HITBOX_SIZEX, HITBOX_SIZEY);
                                box.offset = new Vector2(HITBOX_OFFSETX, -HITBOX_OFFSETY);
                                _holdingDown = true;
                                transform.position += new Vector3(0.25f, -0.25f, 0);
                            }
                        }
                    }
                    // If no walls are found, move as much as you'd like. Or as much as the runspeed will allow
                    else
                    {
                        transform.position += new Vector3(0, _velocity.y * Time.fixedDeltaTime, 0);
                    }
                    // Now we have the horizontal check to do. This check is done to try and find any point when the player is no longer physically on any wall so as
                    // to adjust accordingly based on which direction the player's facing
                    if (checkHoriz.collider == null && !_justGrabbedWall)
                    {
                        _currentSurface = DIR_FLOOR;
                        anim.SetInteger("currentSurface", 0);
                        box.size = new Vector2(HITBOX_SIZEX, HITBOX_SIZEY);
                        box.offset = new Vector2(HITBOX_OFFSETX, HITBOX_OFFSETY);
                        if (_facingLeft)
                        {
                            transform.position += new Vector3(-0.375f, 0.125f, 0);
                        }
                        else
                        {
                            transform.position += new Vector3(0.375f, 0.125f, 0);
                        }
                        _holdingDown = true;
                        if (Mathf.Sign(_velocity.y) == 1)
                        {
                            _velocity.y = 0;
                        }
                    }
                    _justGrabbedWall = false;
                    break;
                case 2:
                    // The ceiling state here is just a dumbed-down version of the floor state
                    _velocity.y = 1;
                    if (checkHoriz.collider != null)
                    {
                        if (_facingLeft)
                        {
                            transform.position += new Vector3(-checkHoriz.distance + 0.0625f, 0, 0);
                            if (_relativeUp)
                            {
                                _currentSurface = DIR_WALL;
                                anim.SetInteger("currentSurface", 1);
                                box.size = new Vector2(HITBOX_SIZEY, HITBOX_SIZEX);
                                box.offset = new Vector2(-HITBOX_OFFSETY, HITBOX_OFFSETX);
                                transform.position += new Vector3(0.25f, 0.25f, 0);
                                _justGrabbedWall = true;
                                _onSurface = true;
                            }
                        }
                        else
                        {
                            transform.position += new Vector3(checkHoriz.distance - 0.0625f, 0, 0);
                            if (_relativeUp)
                            {
                                _currentSurface = DIR_WALL;
                                anim.SetInteger("currentSurface", 1);
                                box.size = new Vector2(HITBOX_SIZEY, HITBOX_SIZEX);
                                box.offset = new Vector2(-HITBOX_OFFSETY, HITBOX_OFFSETX);
                                transform.position += new Vector3(-0.25f, 0.25f, 0);
                                _justGrabbedWall = true;
                                _onSurface = true;
                            }
                        }
                        _velocity.x = 0;
                    }
                    else
                    {
                        transform.position += new Vector3(_velocity.x * Time.fixedDeltaTime, 0, 0);
                    }

                    if (checkVertSecondary.point.y > transform.position.y + 1.125f)
                    {
                        transform.position += new Vector3(0, 1.125f, 0);
                        box.offset = new Vector2(HITBOX_OFFSETX, HITBOX_OFFSETY);
                        _facingUp = false;
                        _currentSurface = DIR_FLOOR;
                        _onSurface = false;
                    }
                    break;
            }
        }
        else
        {
            anim.speed = 0;
        }
    }

    // Update(), called less frequently (every drawn frame), actually gets most of the inputs and converts them to what they should be given any current surface state
    void Update()
    {
        if (PlayState.gameState == "Game")
        {
            // First, get the inputs, converted to the right relative directions
            GetInputs();

            // Flip the sprite where it should be
            if ((_relativeLeft && _currentSurface == DIR_FLOOR) || (_relativeRight && _currentSurface == DIR_CEILING))
            {
                _facingLeft = true;
            }
            else if ((_relativeRight && _currentSurface == DIR_FLOOR) || (_relativeLeft && _currentSurface == DIR_CEILING))
            {
                _facingLeft = false;
            }
            if ((((_relativeLeft && _facingLeft) || (_relativeRight && !_facingLeft)) && _currentSurface == DIR_WALL) || _currentSurface == DIR_CEILING)
            {
                _facingUp = true;
            }
            else if ((((_relativeRight && _facingLeft) || (_relativeLeft && !_facingLeft)) && _currentSurface == DIR_WALL) || _currentSurface == DIR_FLOOR)
            {
                _facingUp = false;
            }
            // Here we control the player's shell state
            if ((_relativeRight || _relativeLeft || Input.GetAxisRaw("Jump") == 1) && _onSurface && _inShell)
            {
                _inShell = false;
                _justLeftShell = true;
            }
            if (_relativeDown && !_relativeLeft && !_relativeRight && _justPressedDown && !_inShell)
            {
                _inShell = true;
                sfx.PlayOneShot(shell);
                _justPressedDown = false;
                _holdingDown = true;
            }

            // This block actually performs the sprite flips
            if (_facingLeft)
            {
                sprite.flipX = true;
            }
            else
            {
                sprite.flipX = false;
            }
            if (_facingUp)
            {
                sprite.flipY = true;
            }
            else
            {
                sprite.flipY = false;
            }

            // Reset all the "just pressed this button" vars to false before the next method call
            _justPressedDown = false;
            _justPressedLeft = false;
            _justPressedRight = false;
            _justPressedUp = false;
            // Set the right animation for in shell vs out of shell
            if (_inShell)
            {
                anim.SetBool("inShell", true);
            }
            else
            {
                anim.SetBool("inShell", false);
            }

            // Get specifics on current inputs and set velocities before FixedUpdate handles them
            CheckMoveInput();
        }
    }

    // This function translates player inputs into directions relative to whatever surface Snaily is currently grabbing
    void GetInputs()
    {
        if (Input.GetAxisRaw("Horizontal") == 1)
        {
            switch (_currentSurface)
            {
                case 0:
                    _relativeLeft = false;
                    _relativeRight = true;
                    _justPressedRight = true;
                    break;
                case 1:
                    if (_facingLeft)
                    {
                        _relativeDown = false;
                        _relativeUp = true;
                        _justPressedUp = true;
                    }
                    else
                    {
                        _relativeUp = false;
                        _relativeDown = true;
                        if (!_holdingDown)
                        {
                            _justPressedDown = true;
                        }
                    }
                    break;
                case 2:
                    _relativeLeft = true;
                    _relativeRight = false;
                    _justPressedLeft = true;
                    break;
            }
        }
        else if (Input.GetAxisRaw("Horizontal") == -1)
        {
            switch (_currentSurface)
            {
                case 0:
                    _relativeLeft = true;
                    _relativeRight = false;
                    _justPressedLeft = true;
                    break;
                case 1:
                    if (_facingLeft)
                    {
                        _relativeDown = true;
                        _relativeUp = false;
                        if (!_holdingDown)
                        {
                            _justPressedDown = true;
                        }
                    }
                    else
                    {
                        _relativeUp = true;
                        _relativeDown = false;
                        _justPressedUp = true;
                    }
                    break;
                case 2:
                    _relativeLeft = false;
                    _relativeRight = true;
                    _justPressedRight = true;
                    break;
            }
        }
        else
        {
            if (_currentSurface == DIR_FLOOR || _currentSurface == DIR_CEILING)
            {
                _relativeLeft = false;
                _relativeRight = false;
            }
            else
            {
                _relativeDown = false;
                _relativeUp = false;
            }
        }
        if (Input.GetAxisRaw("Vertical") == 1)
        {
            switch (_currentSurface)
            {
                case 0:
                    _relativeDown = false;
                    _relativeUp = true;
                    _justPressedUp = true;
                    break;
                case 1:
                    if (_facingLeft)
                    {
                        _relativeRight = false;
                        _relativeLeft = true;
                        _justPressedLeft = true;
                    }
                    else
                    {
                        _relativeLeft = false;
                        _relativeRight = true;
                        _justPressedRight = true;
                    }
                    break;
                case 2:
                    _relativeUp = false;
                    _relativeDown = true;
                    if (!_holdingDown)
                    {
                        _justPressedDown = true;
                    }
                    break;
            }
        }
        else if (Input.GetAxisRaw("Vertical") == -1)
        {
            switch (_currentSurface)
            {
                case 0:
                    _relativeDown = true;
                    _relativeUp = false;
                    if (!_holdingDown)
                    {
                        _justPressedDown = true;
                    }
                    break;
                case 1:
                    if (_facingLeft)
                    {
                        _relativeRight = true;
                        _relativeLeft = false;
                        _justPressedRight = true;
                    }
                    else
                    {
                        _relativeLeft = true;
                        _relativeRight = false;
                        _justPressedLeft = true;
                    }
                    break;
                case 2:
                    _relativeUp = true;
                    _relativeDown = false;
                    _justPressedUp = true;
                    break;
            }
        }
        else
        {
            if (_currentSurface == DIR_FLOOR || _currentSurface == DIR_CEILING)
            {
                _relativeUp = false;
                _relativeDown = false;
            }
            else
            {
                _relativeLeft = false;
                _relativeRight = false;
            }
        }
    }

    // This function's main purpose is to assign the runspeed to any given direction the player can and wants to go
    void CheckMoveInput()
    {
        if (!_relativeDown && _holdingDown)
        {
            _holdingDown = false;
        }
        if (!_relativeLeft && _holdingLeft)
        {
            _holdingLeft = false;
        }
        if (!_relativeRight && _holdingRight)
        {
            _holdingRight = false;
        }
        if (!_relativeUp && _holdingUp)
        {
            _holdingUp = false;
        }

        switch (_currentSurface)
        {
            case 0:
                if (_relativeLeft)
                {
                    _velocity.x = -RUNSPEED_NORMAL;
                }
                else if (_relativeRight)
                {
                    _velocity.x = RUNSPEED_NORMAL;
                }
                else
                {
                    _velocity.x = 0;
                }
                break;
            case 1:
                if (_relativeLeft)
                {
                    if (_facingLeft)
                    {
                        _velocity.y = RUNSPEED_NORMAL;
                    }
                    else
                    {
                        _velocity.y = -RUNSPEED_NORMAL;
                    }
                }
                else if (_relativeRight)
                {
                    if (_facingLeft)
                    {
                        _velocity.y = -RUNSPEED_NORMAL;
                    }
                    else
                    {
                        _velocity.y = RUNSPEED_NORMAL;
                    }
                }
                else
                {
                    _velocity.y = 0;
                }
                break;
            case 2:
                if (_relativeLeft)
                {
                    _velocity.x = RUNSPEED_NORMAL;
                }
                else if (_relativeRight)
                {
                    _velocity.x = -RUNSPEED_NORMAL;
                }
                else
                {
                    _velocity.x = 0;
                }
                break;
        }
    }

    // This coroutine here is meant to display the keypress indicators intended for debugging purposes
    IEnumerator DebugKeys()
    {
        if (Input.GetAxisRaw("Horizontal") == 1)
        {
            debugLeft.GetComponent<SpriteRenderer>().sprite = keyIdle;
            debugRight.GetComponent<SpriteRenderer>().sprite = keyHeld;
        }
        else if (Input.GetAxisRaw("Horizontal") == -1)
        {
            debugLeft.GetComponent<SpriteRenderer>().sprite = keyHeld;
            debugRight.GetComponent<SpriteRenderer>().sprite = keyIdle;
        }
        else
        {
            debugLeft.GetComponent<SpriteRenderer>().sprite = keyIdle;
            debugRight.GetComponent<SpriteRenderer>().sprite = keyIdle;
        }

        if (Input.GetAxisRaw("Vertical") == 1)
        {
            debugDown.GetComponent<SpriteRenderer>().sprite = keyIdle;
            debugUp.GetComponent<SpriteRenderer>().sprite = keyHeld;
        }
        else if (Input.GetAxisRaw("Vertical") == -1)
        {
            debugDown.GetComponent<SpriteRenderer>().sprite = keyHeld;
            debugUp.GetComponent<SpriteRenderer>().sprite = keyIdle;
        }
        else
        {
            debugDown.GetComponent<SpriteRenderer>().sprite = keyIdle;
            debugUp.GetComponent<SpriteRenderer>().sprite = keyIdle;
        }

        if (Input.GetAxisRaw("Jump") == 1)
        {
            debugJump.GetComponent<SpriteRenderer>().sprite = keyHeld;
        }
        else
        {
            debugJump.GetComponent<SpriteRenderer>().sprite = keyIdle;
        }

        yield return new WaitForEndOfFrame();
        StartCoroutine("DebugKeys");
    }
}
