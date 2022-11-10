using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, ICutsceneObject {
    #region vars
    public const int DIR_FLOOR = 0;
    public const int DIR_WALL_LEFT = 1;
    public const int DIR_WALL_RIGHT = 2;
    public const int DIR_CEILING = 3;
    public int currentSurface = 0;
    public bool facingLeft = false;
    public bool facingDown = false;
    public int selectedWeapon = 0;
    public bool armed;
    public int health = 12;
    public int maxHealth = 12;
    public bool stunned = false;
    public bool inDeathCutscene = false;
    public int gravityDir = 0;
    public bool underwater = false;
    public Vector2 velocity = Vector2.zero;
    public bool grounded;
    public bool shelled;

    public float[] WEAPON_COOLDOWNS = new float[] { 0.085f, 0.3f, 0.17f, 0.0425f, 0.15f, 0.085f };

    private float speedMod = 1;
    private float jumpMod = 1;
    private float gravityMod = 1;
    public bool holdingJump = false;
    private bool holdingShell = false;
    private bool axisFlag = false;
    private bool againstWallFlag = false;
    private float fireCooldown = 0;
    private int bulletID = 0;
    private float sleepTimer = 30f;
    private bool isSleeping = false;

    public AnimationModule anim;
    public SpriteRenderer sprite;
    public BoxCollider2D box;
    public Rigidbody2D rb;
    public GameObject bulletPool;
    public Sprite blank;
    public Sprite smallBlank;
    public Sprite missing;

    private RaycastHit2D boxL;
    private RaycastHit2D boxR;
    private RaycastHit2D boxU;
    private RaycastHit2D boxD;
    private RaycastHit2D boxCorner;
    private Vector2 lastPosition;
    private Vector2 lastSize;

    public LayerMask playerCollide;

    public Snaily playerScriptSnaily;

    // Movement control vars
    private int defaultGravityDir = DIR_FLOOR;       // Determines the default direction gravity pulls the player
    private int shellable = -1;                      // Determines if the player can retract into a shell. -1 = always, -2 = never, any item ID = item-bound
    private int hopWhileMoving = 0;                  // Determines if the player bounces along the ground when they move. 0 = no hops, any positive int = hop power
    private bool canRoundInnerCorners = true;        // Determines if the player can round inside corners in terrain, switching their gravity state
    private bool canRoundOuterCorners = true;        // Determines if the player can round outside corners in terrain, switching their gravity state
    private float[] runSpeed = new float[4];         // Contains the speed at which the player moves with each shell upgrade
    private float[] jumpPower = new float[8];        // Contains the player's jump power with each shell upgrade. The second half of the array assumes High Jump
    private float[] gravity = new float[4];          // Contains the gravity scale with each shell upgrade
    private float[] terminalVelocity = new float[4]; // Contains the player's terminal velocity with each shell upgrade
    private float idleTimer = 30;                    // Determines how long the player must remain idle before playing an idle animation
    private List<Particle> idleParticles;            // Contains every particle used in the player's idle animation so that they can be despawned easily
    private Vector2 hitboxSize_normal;               // The size of the player's hitbox
    private Vector2 hitboxSize_shell;                // The size of the player's hitbox while in their shell
    private Vector2 hitboxOffset_normal;             // The offset of the player's hitbox
    private Vector2 hitboxOffset_shell;              // the offset of the player's hitbox while in their shell
    #endregion vars

    #region cutscene
    public void cutRegister() {
        CutsceneManager.declare("PLAYER", new CutsceneManager.Unit("dict", new Dictionary<string, CutsceneManager.Unit> {
            {"setFreeze", new CutsceneManager.Unit(setFreeze, 2, "none")},
        }));
    }

    public void cutStart() {

    }

    public void cutEnd() {

    }

    public object[] setFreeze(object[] inputs) {
        bool mode = (bool)inputs[0];
        int condition = (int)inputs[1];
        bool success = false;
        if (condition == 0) {
            PlayState.paralyzed = mode;
            success = true;
        } else if (condition == 1) {
            if (grounded) {
                PlayState.paralyzed = mode;
                success = true;
            }
        }
        return new object[1] { success };
    }

    public object[] impulse(object[] inputs) {
        Vector3 vel = new Vector3((float)inputs[0], (float)inputs[1], (float)inputs[2]);
        int condition = (int)inputs[3];
        bool success = false;
        return new object[1] { success };
    }
    #endregion cutscene

    // Start() is called at the very beginning of the script's lifetime. It's used to initialize certain variables and states for components to be in.
    void Start()
    {
        // All this does is set Snaily's components to simpler variables that can be more easily called
        anim = GetComponent<AnimationModule>();
        sprite = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        PlayState.globalFunctions.RenderNewHearts();
        PlayState.globalFunctions.UpdateHearts();

        PlayState.globalFunctions.RunDebugKeys();

        PlayState.globalFunctions.UpdateMusic(-1, -1, 3);
    }

    // Update(), called less frequently (every drawn frame), actually gets most of the inputs and converts them to what they should be given any current surface state
    void Update()
    {
        if (PlayState.gameState == PlayState.GameState.game)
        {
            rb.WakeUp();

            // Making sure we have weapons
            int[] weaponIDs = new int[] { 0, 1, 2, 11, 12 };
            armed = false;
            foreach (int weapon in weaponIDs)
                if (PlayState.CheckForItem(weapon))
                    armed = true;

            // Noclip!!!
            if (PlayState.noclipMode)
            {
                if (Control.ShootHold())
                {
                    if (Control.UpPress())
                        transform.position = new Vector2(transform.position.x, transform.position.y + 16);
                    if (Control.DownPress())
                        transform.position = new Vector2(transform.position.x, transform.position.y - 16);
                    if (Control.LeftPress())
                        transform.position = new Vector2(transform.position.x - 26, transform.position.y);
                    if (Control.RightPress())
                        transform.position = new Vector2(transform.position.x + 26, transform.position.y);
                }
                else
                    transform.position = new Vector2(transform.position.x + (10 * Control.AxisX() * (Control.JumpHold() ? 2.5f : 1) * Time.deltaTime),
                        transform.position.y + (10 * Control.AxisY() * (Control.JumpHold() ? 2.5f : 1) * Time.deltaTime));
                box.enabled = false;
            }
            else if (!inDeathCutscene)
                box.enabled = true;

            // Marking the "has jumped" flag for Snail NPC 01's dialogue
            if (Control.JumpHold())
                PlayState.hasJumped = true;

            // Weapon swapping
            if (Control.Weapon1() && PlayState.CheckForItem(0))
                PlayState.globalFunctions.ChangeActiveWeapon(0);
            if (Control.Weapon2() && (PlayState.CheckForItem(1) || PlayState.CheckForItem(11)))
                PlayState.globalFunctions.ChangeActiveWeapon(1);
            if (Control.Weapon3() && (PlayState.CheckForItem(2) || PlayState.CheckForItem(12)))
                PlayState.globalFunctions.ChangeActiveWeapon(2);

            // Sleep code! Don't do anything for thirty seconds and Snaily takes a nap!
            if (PlayState.gameState == PlayState.GameState.game)
            {
                if (Control.AxisX() != 0 || Control.AxisY() != 0 || Control.JumpHold() || Control.ShootHold() || Control.StrafeHold() || Control.SpeakHold() || stunned)
                {
                    sleepTimer = idleTimer;
                    isSleeping = false;
                    //if (zzz != null)
                    //{
                    //    zzz.ResetParticle();
                    //    zzz = null;
                    //}
                    foreach (Particle particle in idleParticles)
                        particle.ResetParticle();
                    idleParticles.Clear();
                }
                else
                {
                    sleepTimer = Mathf.Clamp(sleepTimer - Time.deltaTime, 0, idleTimer);
                    if (sleepTimer == 0 && !isSleeping)
                    {
                        if (!shelled)
                            ToggleShell();
                        idleParticles.Add(PlayState.RequestParticle(new Vector2(transform.position.x + 0.75f, transform.position.y), "zzz"));
                        isSleeping = true;
                    }
                }
                if (idleParticles.Count > 0)
                    idleParticles[0].transform.position = new Vector2(transform.position.x + 0.75f + ((gravityDir == DIR_FLOOR || gravityDir == DIR_CEILING) && facingLeft ? 0.25f : 0),
                        transform.position.y + ((gravityDir == DIR_WALL_LEFT || gravityDir == DIR_WALL_RIGHT) && facingDown ? 0.25f : 0));
            }
        }
    }

    // This function is called once every 0.02 seconds (50 time a second) regardless of framerate. Unity requires all physics calculations to be
    // run in this function, so it's where I put movement code as it utilizes boxcasts
    public virtual void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game || PlayState.noclipMode)
            return;
    
        // To start things off, we mark our current position as the last position we took. Same with our hitbox size
        // Among other things, this is used to test for ground when we're airborne
        lastPosition = new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y);
        lastSize = box.size;
        // We also update all our boxcasts, both for the corner and in case they're misaligned with our current gravity state
        UpdateBoxcasts();
        // Next, we decrease the fire cooldown
        fireCooldown = Mathf.Clamp(fireCooldown - Time.fixedDeltaTime, 0, Mathf.Infinity);
        // Then, we reset the flag marking if Snaily is airborne and shoving their face into a wall
        againstWallFlag = false;
    
        // Next, we run different blocks of movement code based on our gravity state. They're largely the same, but are kept separate
        // so that things can stay different between them if needed, like Snaily falling off walls and ceilings without Gravity Snail
        if (!inDeathCutscene)
        {
            int readIDSpeed = PlayState.CheckForItem(9) ? 3 : (PlayState.CheckForItem(8) ? 2 : (PlayState.CheckForItem(7) ? 1 : 0));
            int readIDJump = readIDSpeed + (PlayState.CheckForItem(4) ? 4 : 0);
    
            switch (gravityDir)
            {
                case DIR_FLOOR:
                    // This if block's purpose is so that you can click that minus button on the left and hide it from view, just so that you don't have
                    // to scroll quite as much if you don't want to. Cleanup, basically
                    if (true)
                    {
                        // We start by zeroing our relative vertical velocity if we're grounded, and our relative horizontal velocity no matter what
                        velocity.x = 0;
                        if (grounded)
                            velocity.y = 0;
    
                        // From here, we perform relatively horizontal movement checks to move, stop if we hit a wall, and allow for climbing
                        if (Control.AxisX() != 0 && !Control.StrafeHold() && !PlayState.paralyzed)
                        {
                            if (shelled)
                            {
                                if (Control.AxisX() == (facingLeft ? 1 : -1))
                                    transform.position = new Vector2(transform.position.x + (0.1667f * (facingLeft ? 1 : -1)), transform.position.y);
                                if (grounded)
                                    ToggleShell();
                                float distance = Vector2.Distance(boxL.point, new Vector2(transform.position.x, boxL.point.y));
                                if (distance < box.size.x * 0.5f)
                                {
                                    transform.position = new Vector2(transform.position.x + ((box.size.x * 0.675f) - distance) *
                                        (boxL.point.x < transform.position.x ? 1 : -1), transform.position.y);
                                    UpdateBoxcasts();
                                }
                            }
                            SwapDir(Control.RightHold() ? DIR_WALL_RIGHT : DIR_WALL_LEFT);
                            float runSpeedValue = runSpeed[readIDSpeed] * speedMod * Time.fixedDeltaTime;
                            if ((facingLeft ? boxL : boxR).distance < runSpeedValue)
                            {
                                againstWallFlag = true;
                                velocity.x = facingLeft ? -runSpeedValue + (runSpeedValue - boxL.distance) + 0.0078125f :
                                    runSpeedValue - (runSpeedValue - boxR.distance) - 0.0078125f;
                                // In case the player happens to be holding the relative up/down button while the character runs face-first into a wall,
                                // we check to see if climbing is possible in either direction and switch the character's gravity state
                                if ((boxD.distance + boxU.distance) >= 1)
                                {
                                    if (!stunned)
                                    {
                                        if (Control.UpHold() || (Control.DownHold() && !grounded))
                                        {
                                            if (shelled)
                                            {
                                                if (boxL.distance > 0.4f && boxR.distance < 0.4f)
                                                    transform.position = new Vector2(transform.position.x - (0.58125f - boxR.distance - (facingLeft ? 0.25f : 0)),
                                                        transform.position.y);
                                                else if (boxL.distance < 0.4f && boxR.distance > 0.4f)
                                                    transform.position = new Vector2(transform.position.x + (0.58125f - boxL.distance - (facingLeft ? 0 : 0.25f)),
                                                        transform.position.y);
                                                ToggleShell();
                                            }
    
                                            transform.position = new Vector2(transform.position.x + velocity.x, transform.position.y);
                                            float boxCorrection = (box.size.y - box.size.x) * 0.5f;
                                            float ceilDis = boxU.distance - boxCorrection;
                                            float floorDis = boxD.distance - boxCorrection;
                                            SwitchSurfaceAxis();
                                            float adjustment = 0;
                                            if (grounded)
                                                adjustment = boxCorrection;
                                            else
                                            {
                                                if (ceilDis < floorDis && ceilDis < box.size.y * 0.5f)
                                                    adjustment = -(ceilDis - (box.size.y * 0.5f));
                                                else if (floorDis < ceilDis && floorDis < box.size.y * 0.5f)
                                                    adjustment = floorDis - (box.size.y * 0.5f);
                                            }
                                            transform.position = new Vector2(
                                                transform.position.x + (facingLeft ? boxCorrection : -boxCorrection),
                                                transform.position.y - adjustment
                                                );
                                            SwapDir(Control.UpHold() ? DIR_CEILING : DIR_FLOOR);
                                            gravityDir = facingLeft ? DIR_WALL_LEFT : DIR_WALL_RIGHT;
                                            grounded = true;
                                            transform.position = new Vector2(Mathf.Round(transform.position.x * 2) * 0.5f + (facingLeft ? -0.01f : 0.01f), transform.position.y);
                                            UpdateBoxcasts();
                                            return;
                                        }
                                    }
                                }
                            }
                            else
                                velocity.x = facingLeft ? -runSpeedValue : runSpeedValue;
                            transform.position = new Vector2(transform.position.x + velocity.x, transform.position.y);
                            UpdateBoxcasts();
                        }
    
                        // Now, we perform relatively vertical checks. This mainly involves jumping and falling
                        if (!grounded)
                        {
                            bool pokedCeiling = false;
                            velocity.y -= gravity[readIDSpeed] * gravityMod * Time.fixedDeltaTime;
                            if (velocity.y > 0 && !holdingJump)
                                velocity.y = PlayState.Integrate(velocity.y, 0, 4, Time.fixedDeltaTime);
                            velocity.y = Mathf.Clamp(velocity.y, terminalVelocity[readIDSpeed], Mathf.Infinity);
                            if (boxD.distance != 0 && boxU.distance != 0)
                            {
                                if (boxD.distance < -velocity.y && Mathf.Sign(velocity.y) == -1)
                                {
                                    velocity.y = -boxD.distance;
                                    grounded = true;
                                }
                                else if (boxU.distance < velocity.y && Mathf.Sign(velocity.y) == 1)
                                {
                                    velocity.y = boxU.distance;
                                    pokedCeiling = true;
                                }
                            }
                            if (!againstWallFlag)
                            {
                                transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
                            }
                            else
                            {
                                // This entire block here covers the specific case of slipping into a one-tall tunnel in a wall while midair
                                for (int i = 0; i < 8; i++)
                                {
                                    transform.position = new Vector2(transform.position.x, transform.position.y + (velocity.y * 0.125f));
                                    RaycastHit2D tunnelCheckUpper = Physics2D.Raycast(
                                        new Vector2(transform.position.x, transform.position.y + 0.375f),
                                        facingLeft ? Vector2.left : Vector2.right,
                                        Mathf.Infinity,
                                        playerCollide,
                                        Mathf.Infinity,
                                        Mathf.Infinity
                                        );
                                    RaycastHit2D tunnelCheckLower = Physics2D.Raycast(
                                        new Vector2(transform.position.x, transform.position.y - 0.375f),
                                        facingLeft ? Vector2.left : Vector2.right,
                                        Mathf.Infinity,
                                        playerCollide,
                                        Mathf.Infinity,
                                        Mathf.Infinity
                                        );
                                    if (tunnelCheckUpper.distance >= 1.5f && tunnelCheckLower.distance >= 1.5f)
                                    {
                                        transform.position = new Vector2(
                                            transform.position.x + ((facingLeft ? -runSpeed[readIDSpeed] : runSpeed[readIDSpeed]) * speedMod * Time.fixedDeltaTime),
                                            Mathf.Floor(transform.position.y) + 0.5f);
                                        i = 8;
                                    }
                                }
                            }
                            UpdateBoxcasts();
                            if (pokedCeiling)
                            {
                                velocity.y = 0;
                                if (Control.UpHold())
                                {
                                    gravityDir = DIR_CEILING;
                                    SwapDir(DIR_CEILING);
                                    grounded = true;
                                    holdingShell = true;
                                    return;
                                }
                            }
                        }
                        else
                        {
                            if (boxD.distance > 0.0125f)
                            {
                                if (boxCorner.distance <= 0.0125f)
                                {
                                    if (Control.DownHold() && Control.AxisX() == (facingLeft ? -1 : 1) && !stunned)
                                    {
                                        SwapDir(facingLeft ? DIR_WALL_RIGHT : DIR_WALL_LEFT);
                                        SwitchSurfaceAxis();
                                        RaycastHit2D wallTester = Physics2D.Raycast(
                                            new Vector2(transform.position.x + (facingLeft ? -box.size.x * 0.5f : box.size.x * 0.5f), transform.position.y - 0.75f),
                                            facingLeft ? Vector2.left : Vector2.right,
                                            Mathf.Infinity,
                                            playerCollide,
                                            Mathf.Infinity,
                                            Mathf.Infinity
                                            );
                                        transform.position = new Vector2(
                                            transform.position.x + (facingLeft ? -wallTester.distance : wallTester.distance),
                                            transform.position.y
                                            );
                                        gravityDir = facingLeft ? DIR_WALL_LEFT : DIR_WALL_RIGHT;
                                        return;
                                    }
                                }
                                else
                                    grounded = false;
                            }
                        }
    
                        // Now, let's see if we can jump
                        if (boxD.distance == 0)
                        {
                            transform.position = new Vector2(transform.position.x, transform.position.y + 0.01f);
                            UpdateBoxcasts();
                        }
                        if (Control.JumpHold() && grounded && !holdingJump && boxU.distance > 0.95f && !PlayState.paralyzed)
                        {
                            if (shelled)
                            {
                                if (boxL.distance < 0.4f && boxR.distance < 0.4f)
                                    break;
                                if (boxL.distance > 0.4f && boxR.distance < 0.4f)
                                    transform.position = new Vector2(transform.position.x - (0.675f - boxR.distance - (facingLeft ? 0.25f : 0)),
                                        transform.position.y);
                                else if (boxL.distance < 0.4f && boxR.distance > 0.4f)
                                    transform.position = new Vector2(transform.position.x + (0.675f - boxL.distance - (facingLeft ? 0 : 0.25f)),
                                        transform.position.y);
                                ToggleShell();
                            }
                            grounded = false;
                            velocity.y = jumpPower[readIDJump] * jumpMod * Time.deltaTime;
                            PlayState.PlaySound("Jump");
                        }
                        if (Control.JumpHold() && !holdingJump)
                            holdingJump = true;
                        else if (!Control.JumpHold() && holdingJump)
                            holdingJump = false;
    
                        // Finally, we check to see if we can shell
                        if (Control.DownHold() &&
                            Control.AxisX() == 0 &&
                            !Control.JumpHold() &&
                            !Control.ShootHold() &&
                            !Control.StrafeHold() &&
                            !holdingShell && !PlayState.paralyzed)
                        {
                            if (!shelled)
                                ToggleShell();
                            else
                            {
                                if (boxL.distance < 0.4f && boxR.distance < 0.4f)
                                    break;
                                if (boxL.distance > 0.4f && boxR.distance < 0.4f)
                                    transform.position = new Vector2(transform.position.x - (0.675f - boxR.distance - (facingLeft ? 0.25f : 0)),
                                        transform.position.y);
                                else if (boxL.distance < 0.4f && boxR.distance > 0.4f)
                                    transform.position = new Vector2(transform.position.x + (0.675f - boxL.distance - (facingLeft ? 0 : 0.25f)),
                                        transform.position.y);
                                ToggleShell();
                            }
                            holdingShell = true;
                        }
                        else if (!holdingShell && Control.DownHold())
                            holdingShell = true;
                        if (holdingShell && !Control.DownHold())
                            holdingShell = false;
                        if (PlayState.IsTileSolid(transform.position))
                            transform.position = new Vector2(transform.position.x, transform.position.y + 1);
                    }
                    break;
                case DIR_WALL_LEFT:
                    // This if block's purpose is so that you can click that minus button on the left and hide it from view, just so that you don't have
                    // to scroll quite as much if you don't want to. Cleanup, basically
                    if (true)
                    {
                        // We start by zeroing our relative vertical velocity if we're grounded, and our relative horizontal velocity no matter what
                        velocity.y = 0;
                        if (grounded)
                            velocity.x = 0;
    
                        // From here, we perform relatively horizontal movement checks to move, stop if we hit a wall, and allow for climbing
                        if (Control.AxisY() != 0 && !Control.StrafeHold() && !PlayState.paralyzed)
                        {
                            if (shelled)
                            {
                                if (Control.AxisX() == (facingDown ? 1 : -1))
                                    transform.position = new Vector2(transform.position.x, transform.position.y + (0.1667f * (facingDown ? 1 : -1)));
                                if (grounded)
                                    ToggleShell();
                                float distance = Vector2.Distance(boxD.point, new Vector2(boxD.point.x, transform.position.y));
                                if (distance < box.size.y * 0.5f)
                                {
                                    transform.position = new Vector2(transform.position.x, transform.position.y + ((box.size.y * 0.675f) - distance) *
                                        (boxD.point.y < transform.position.y ? 1 : -1));
                                    UpdateBoxcasts();
                                }
                            }
                            SwapDir(Control.UpHold() ? DIR_CEILING : DIR_FLOOR);
                            float runSpeedValue = runSpeed[readIDSpeed] * speedMod * Time.fixedDeltaTime;
                            if ((facingDown ? boxD : boxU).distance < runSpeedValue)
                            {
                                againstWallFlag = true;
                                velocity.y = facingDown ? -runSpeedValue + (runSpeedValue - boxD.distance) + 0.0078125f :
                                    runSpeedValue - (runSpeedValue - boxU.distance) - 0.0078125f;
                                // In case the player happens to be holding the relative up/down button while the character runs face-first into a wall,
                                // we check to see if climbing is possible in either direction and switch the character's gravity state
                                if ((boxL.distance + boxR.distance) >= 1)
                                {
                                    if (!stunned)
                                    {
                                        if (Control.RightHold() || (Control.LeftHold() && !grounded))
                                        {
                                            if (shelled)
                                            {
                                                if (boxD.distance > 0.4f && boxU.distance < 0.4f)
                                                    transform.position = new Vector2(transform.position.x, transform.position.y -
                                                        (0.675f - boxU.distance - (facingLeft ? 0.25f : 0)));
                                                else if (boxD.distance < 0.4f && boxU.distance > 0.4f)
                                                    transform.position = new Vector2(transform.position.x, transform.position.y +
                                                        (0.675f - boxD.distance - (facingLeft ? 0 : 0.25f)));
                                                ToggleShell();
                                            }
    
                                            transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
                                            float boxCorrection = (box.size.x - box.size.y) * 0.5f;
                                            float ceilDis = boxR.distance - boxCorrection;
                                            float floorDis = boxL.distance - boxCorrection;
                                            SwitchSurfaceAxis();
                                            float adjustment = 0;
                                            if (grounded)
                                                adjustment = boxCorrection;
                                            else
                                            {
                                                if (ceilDis < floorDis && ceilDis < box.size.y * 0.5f)
                                                    adjustment = -(ceilDis - (box.size.y * 0.5f));
                                                else if (floorDis < ceilDis && floorDis < box.size.y * 0.5f)
                                                    adjustment = floorDis - (box.size.y * 0.5f);
                                            }
                                            transform.position = new Vector2(
                                                transform.position.x - adjustment,
                                                transform.position.y + (facingDown ? boxCorrection : -boxCorrection)
                                                );
                                            SwapDir((Control.RightHold()) ? DIR_WALL_RIGHT : DIR_WALL_LEFT);
                                            gravityDir = facingDown ? DIR_FLOOR : DIR_CEILING;
                                            grounded = true;
                                            transform.position = new Vector2(transform.position.x, Mathf.Round(transform.position.y * 2) * 0.5f + (facingDown ? -0.01f : 0.01f));
                                            UpdateBoxcasts();
                                            return;
                                        }
                                    }
                                }
                            }
                            else
                                velocity.y = facingDown ? -runSpeedValue : runSpeedValue;
                            transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
                            UpdateBoxcasts();
                        }
    
                        // Now, we perform relatively vertical checks. This mainly involves jumping and falling
                        if (!grounded)
                        {
                            if (!PlayState.CheckForItem("Gravity Snail"))
                            {
                                transform.position = new Vector2(transform.position.x + 0.0625f + (box.size.y - box.size.x) * 0.5f, transform.position.y);
                                SwapDir(DIR_FLOOR);
                                SwitchSurfaceAxis();
                                gravityDir = DIR_FLOOR;
                                if (Control.DownHold())
                                    holdingShell = true;
                            }
                            else
                            {
                                bool pokedCeiling = false;
                                velocity.x -= gravity[readIDSpeed] * gravityMod * Time.fixedDeltaTime;
                                if (velocity.x > 0 && !holdingJump)
                                    velocity.x = PlayState.Integrate(velocity.y, 0, 4, Time.fixedDeltaTime);
                                velocity.x = Mathf.Clamp(velocity.x, terminalVelocity[readIDSpeed], Mathf.Infinity);
                                if (boxL.distance != 0 && boxR.distance != 0)
                                {
                                    if (boxL.distance < -velocity.x && Mathf.Sign(velocity.x) == -1)
                                    {
                                        velocity.x = -boxL.distance;
                                        grounded = true;
                                    }
                                    else if (boxR.distance < velocity.x && Mathf.Sign(velocity.x) == 1)
                                    {
                                        velocity.x = boxR.distance;
                                        pokedCeiling = true;
                                    }
                                }
                                if (!againstWallFlag)
                                {
                                    transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
                                }
                                else
                                {
                                    // This entire block here covers the specific case of slipping into a one-tall tunnel in a wall while midair
                                    for (int i = 0; i < 8; i++)
                                    {
                                        transform.position = new Vector2(transform.position.x + (velocity.x * 0.125f), transform.position.y);
                                        RaycastHit2D tunnelCheckUpper = Physics2D.Raycast(
                                            new Vector2(transform.position.x + 0.375f, transform.position.y),
                                            facingDown ? Vector2.down : Vector2.up,
                                            Mathf.Infinity,
                                            playerCollide,
                                            Mathf.Infinity,
                                            Mathf.Infinity
                                            );
                                        RaycastHit2D tunnelCheckLower = Physics2D.Raycast(
                                            new Vector2(transform.position.x - 0.375f, transform.position.y),
                                            facingDown ? Vector2.down : Vector2.up,
                                            Mathf.Infinity,
                                            playerCollide,
                                            Mathf.Infinity,
                                            Mathf.Infinity
                                            );
                                        if (tunnelCheckUpper.distance >= 1.5f && tunnelCheckLower.distance >= 1.5f)
                                        {
                                            transform.position = new Vector2(
                                                Mathf.Floor(transform.position.x) + 0.5f,
                                                transform.position.y + ((facingDown ? -runSpeed[readIDSpeed] : runSpeed[readIDSpeed]) * speedMod * Time.fixedDeltaTime));
                                            i = 8;
                                        }
                                    }
                                }
                                UpdateBoxcasts();
                                if (pokedCeiling)
                                {
                                    velocity.x = 0;
                                    if (Control.RightHold())
                                    {
                                        gravityDir = DIR_WALL_RIGHT;
                                        SwapDir(DIR_WALL_RIGHT);
                                        grounded = true;
                                        holdingShell = true;
                                        return;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (boxL.distance > 0.0125f)
                            {
                                if (boxCorner.distance <= 0.0125f)
                                {
                                    if (!PlayState.CheckForItem("Gravity Snail"))
                                    {
                                        SwapDir(DIR_FLOOR);
                                        SwitchSurfaceAxis();
                                        gravityDir = DIR_FLOOR;
                                        if (Control.LeftHold() || Control.DownHold())
                                            holdingShell = true;
                                        transform.position = new Vector2(transform.position.x + 0.0625f, transform.position.y);
                                        if (boxL.distance == 0)
                                            transform.position = new Vector2(transform.position.x - 0.3125f, transform.position.y);
                                        return;
                                    }
                                    else if (Control.LeftHold() && Control.AxisY() == (facingDown ? -1 : 1) && !stunned)
                                    {
                                        SwapDir(facingDown ? DIR_CEILING : DIR_FLOOR);
                                        SwitchSurfaceAxis();
                                        RaycastHit2D wallTester = Physics2D.Raycast(
                                            new Vector2(transform.position.x - 0.75f, transform.position.y + (facingDown ? -box.size.y * 0.5f : box.size.y * 0.5f)),
                                            facingDown ? Vector2.down : Vector2.up,
                                            Mathf.Infinity,
                                            playerCollide,
                                            Mathf.Infinity,
                                            Mathf.Infinity
                                            );
                                        transform.position = new Vector2(
                                            transform.position.x,
                                            transform.position.y + (facingDown ? -wallTester.distance : wallTester.distance)
                                            );
                                        gravityDir = facingDown ? DIR_FLOOR : DIR_CEILING;
                                        return;
                                    }
                                }
                                else
                                    grounded = false;
                            }
                        }
    
                        // Now, let's see if we can jump
                        if (boxL.distance == 0)
                        {
                            transform.position = new Vector2(transform.position.x + 0.01f, transform.position.y);
                            UpdateBoxcasts();
                        }
                        if (Control.JumpHold() && grounded && !holdingJump && boxR.distance > 0.95f && !PlayState.paralyzed)
                        {
                            if (shelled)
                            {
                                if (boxD.distance < 0.4f && boxU.distance < 0.4f)
                                    break;
                                if (boxD.distance > 0.4f && boxU.distance < 0.4f)
                                    transform.position = new Vector2(transform.position.x, transform.position.y -
                                        (0.675f - boxU.distance - (facingLeft ? 0.25f : 0)));
                                else if (boxD.distance < 0.4f && boxU.distance > 0.4f)
                                    transform.position = new Vector2(transform.position.x, transform.position.y +
                                        (0.675f - boxD.distance - (facingLeft ? 0 : 0.25f)));
                                ToggleShell();
                            }
                            grounded = false;
                            if (PlayState.CheckForItem("Gravity Snail"))
                                velocity.x = jumpPower[readIDJump] * jumpMod * Time.deltaTime;
                            else
                            {
                                transform.position = new Vector2(transform.position.x + 0.0625f + (box.size.y - box.size.x) * 0.5f, transform.position.y);
                                SwapDir(DIR_FLOOR);
                                SwitchSurfaceAxis();
                                gravityDir = DIR_FLOOR;
                                if (Control.DownHold())
                                    holdingShell = true;
                            }
                            PlayState.PlaySound("Jump");
                        }
                        if (Control.JumpHold() && !holdingJump)
                            holdingJump = true;
                        else if (!Control.JumpHold() && holdingJump)
                            holdingJump = false;
    
                        // Finally, we check to see if we can shell
                        if (Control.AxisY() == 0 &&
                            Control.LeftHold() &&
                            !Control.JumpHold() &&
                            !Control.ShootHold() &&
                            !Control.StrafeHold() &&
                            !holdingShell && !PlayState.paralyzed)
                        {
                            if (!shelled)
                                ToggleShell();
                            else
                            {
                                if (boxD.distance < 0.4f && boxU.distance < 0.4f)
                                    break;
                                if (boxD.distance > 0.4f && boxU.distance < 0.4f)
                                    transform.position = new Vector2(transform.position.x, transform.position.y -
                                        (0.675f - boxU.distance - (facingLeft ? 0.25f : 0)));
                                else if (boxD.distance < 0.4f && boxU.distance > 0.4f)
                                    transform.position = new Vector2(transform.position.x, transform.position.y +
                                        (0.675f - boxD.distance - (facingLeft ? 0 : 0.25f)));
                                ToggleShell();
                            }
                            holdingShell = true;
                        }
                        else if (!holdingShell && Control.LeftHold())
                            holdingShell = true;
                        if (holdingShell && !Control.LeftHold())
                            holdingShell = false;
                        if (PlayState.IsTileSolid(transform.position))
                            transform.position = new Vector2(transform.position.x - 1, transform.position.y);
                    }
                    break;
                case DIR_WALL_RIGHT:
                    // This if block's purpose is so that you can click that minus button on the left and hide it from view, just so that you don't have
                    // to scroll quite as much if you don't want to. Cleanup, basically
                    if (true)
                    {
                        // We start by zeroing our relative vertical velocity if we're grounded, and our relative horizontal velocity no matter what
                        velocity.y = 0;
                        if (grounded)
                            velocity.x = 0;
    
                        // From here, we perform relatively horizontal movement checks to move, stop if we hit a wall, and allow for climbing
                        if (Control.AxisY() != 0 && !Control.StrafeHold() && !PlayState.paralyzed)
                        {
                            if (shelled)
                            {
                                if (Control.AxisX() == (facingDown ? 1 : -1))
                                    transform.position = new Vector2(transform.position.x, transform.position.y + (0.1667f * (facingDown ? 1 : -1)));
                                if (grounded)
                                    ToggleShell();
                                float distance = Vector2.Distance(boxD.point, new Vector2(boxD.point.x, transform.position.y));
                                if (distance < box.size.y * 0.5f)
                                {
                                    transform.position = new Vector2(transform.position.x, transform.position.y + ((box.size.y * 0.675f) - distance) *
                                        (boxD.point.y < transform.position.y ? 1 : -1));
                                    UpdateBoxcasts();
                                }
                            }
                            SwapDir(Control.UpHold() ? DIR_CEILING : DIR_FLOOR);
                            float runSpeedValue = runSpeed[readIDSpeed] * speedMod * Time.fixedDeltaTime;
                            if ((facingDown ? boxD : boxU).distance < runSpeedValue)
                            {
                                againstWallFlag = true;
                                velocity.y = facingDown ? -runSpeedValue + (runSpeedValue - boxD.distance) + 0.0078125f :
                                    runSpeedValue - (runSpeedValue - boxU.distance) - 0.0078125f;
                                // In case the player happens to be holding the relative up/down button while the character runs face-first into a wall,
                                // we check to see if climbing is possible in either direction and switch the character's gravity state
                                if ((boxL.distance + boxR.distance) >= 1)
                                {
                                    if (!stunned)
                                    {
                                        if (Control.LeftHold() || (Control.RightHold() && !grounded))
                                        {
                                            if (shelled)
                                            {
                                                if (boxD.distance > 0.4f && boxU.distance < 0.4f)
                                                    transform.position = new Vector2(transform.position.x, transform.position.y -
                                                        (0.675f - boxU.distance - (facingLeft ? 0.25f : 0)));
                                                else if (boxD.distance < 0.4f && boxU.distance > 0.4f)
                                                    transform.position = new Vector2(transform.position.x, transform.position.y +
                                                        (0.675f - boxD.distance - (facingLeft ? 0 : 0.25f)));
                                                ToggleShell();
                                            }
    
                                            transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
                                            float boxCorrection = (box.size.x - box.size.y) * 0.5f;
                                            float ceilDis = boxL.distance - boxCorrection;
                                            float floorDis = boxR.distance - boxCorrection;
                                            SwitchSurfaceAxis();
                                            float adjustment = 0;
                                            if (grounded)
                                                adjustment = -boxCorrection;
                                            else
                                            {
                                                if (ceilDis < floorDis && ceilDis < box.size.x * 0.5f)
                                                    adjustment = ceilDis - (box.size.x * 0.5f);
                                                else if (floorDis < ceilDis && floorDis < box.size.x * 0.5f)
                                                    adjustment = -(floorDis - (box.size.x * 0.5f));
                                            }
                                            transform.position = new Vector2(
                                                transform.position.x - adjustment,
                                                transform.position.y + (facingDown ? boxCorrection : -boxCorrection)
                                                );
                                            SwapDir(Control.RightHold() ? DIR_WALL_RIGHT : DIR_WALL_LEFT);
                                            gravityDir = facingDown ? DIR_FLOOR : DIR_CEILING;
                                            grounded = true;
                                            transform.position = new Vector2(transform.position.x, Mathf.Round(transform.position.y * 2) * 0.5f + (facingDown ? -0.01f : 0.01f));
                                            UpdateBoxcasts();
                                            return;
                                        }
                                    }
                                }
                            }
                            else
                                velocity.y = facingDown ? -runSpeedValue : runSpeedValue;
                            transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
                            UpdateBoxcasts();
                        }
    
                        // Now, we perform relatively vertical checks. This mainly involves jumping and falling
                        if (!grounded)
                        {
                            if (!PlayState.CheckForItem("Gravity Snail"))
                            {
                                transform.position = new Vector2(transform.position.x - 0.0625f - (box.size.y - box.size.x) * 0.5f, transform.position.y);
                                SwapDir(DIR_FLOOR);
                                SwitchSurfaceAxis();
                                gravityDir = DIR_FLOOR;
                                if (Control.DownHold())
                                    holdingShell = true;
                            }
                            else
                            {
                                bool pokedCeiling = false;
                                velocity.x += gravity[readIDSpeed] * gravityMod * Time.fixedDeltaTime;
                                if (velocity.x < 0 && !holdingJump)
                                    velocity.x = PlayState.Integrate(velocity.y, 0, 4, Time.fixedDeltaTime);
                                velocity.x = Mathf.Clamp(velocity.x, -Mathf.Infinity, -terminalVelocity[readIDSpeed]);
                                if (boxL.distance != 0 && boxR.distance != 0)
                                {
                                    if (boxL.distance < -velocity.x && Mathf.Sign(velocity.x) == -1)
                                    {
                                        velocity.x = -boxL.distance;
                                        pokedCeiling = true;
                                    }
                                    else if (boxR.distance < velocity.x && Mathf.Sign(velocity.x) == 1)
                                    {
                                        velocity.x = boxR.distance;
                                        grounded = true;
                                    }
                                }
                                if (!againstWallFlag)
                                {
                                    transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
                                }
                                else
                                {
                                    // This entire block here covers the specific case of slipping into a one-tall tunnel in a wall while midair
                                    for (int i = 0; i < 8; i++)
                                    {
                                        transform.position = new Vector2(transform.position.x + (velocity.x * 0.125f), transform.position.y);
                                        RaycastHit2D tunnelCheckUpper = Physics2D.Raycast(
                                            new Vector2(transform.position.x + 0.375f, transform.position.y),
                                            facingDown ? Vector2.down : Vector2.up,
                                            Mathf.Infinity,
                                            playerCollide,
                                            Mathf.Infinity,
                                            Mathf.Infinity
                                            );
                                        RaycastHit2D tunnelCheckLower = Physics2D.Raycast(
                                            new Vector2(transform.position.x - 0.375f, transform.position.y),
                                            facingDown ? Vector2.down : Vector2.up,
                                            Mathf.Infinity,
                                            playerCollide,
                                            Mathf.Infinity,
                                            Mathf.Infinity
                                            );
                                        if (tunnelCheckUpper.distance >= 1.5f && tunnelCheckLower.distance >= 1.5f)
                                        {
                                            transform.position = new Vector2(
                                                Mathf.Floor(transform.position.x) + 0.5f,
                                                transform.position.y + ((facingDown ? -runSpeed[readIDSpeed] : runSpeed[readIDSpeed]) * speedMod * Time.fixedDeltaTime));
                                            i = 8;
                                        }
                                    }
                                }
                                UpdateBoxcasts();
                                if (pokedCeiling)
                                {
                                    velocity.x = 0;
                                    if (Control.LeftHold())
                                    {
                                        gravityDir = DIR_WALL_LEFT;
                                        SwapDir(DIR_WALL_LEFT);
                                        grounded = true;
                                        holdingShell = true;
                                        return;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (boxR.distance > 0.0125f)
                            {
                                if (!PlayState.CheckForItem("Gravity Snail"))
                                {
                                    SwapDir(DIR_FLOOR);
                                    SwitchSurfaceAxis();
                                    gravityDir = DIR_FLOOR;
                                    if (Control.RightHold() || Control.DownHold())
                                        holdingShell = true;
                                    transform.position = new Vector2(transform.position.x - 0.0625f, transform.position.y);
                                    if (boxL.distance == 0)
                                        transform.position = new Vector2(transform.position.x + 0.3125f, transform.position.y);
                                    return;
                                }
                                else if (boxCorner.distance <= 0.0125f)
                                {
                                    if (Control.RightHold() && Control.AxisY() == (facingDown ? -1 : 1) && !stunned)
                                    {
                                        SwapDir(facingDown ? DIR_CEILING : DIR_FLOOR);
                                        SwitchSurfaceAxis();
                                        RaycastHit2D wallTester = Physics2D.Raycast(
                                            new Vector2(transform.position.x + 0.75f, transform.position.y + (facingDown ? -box.size.y * 0.5f : box.size.y * 0.5f)),
                                            facingDown ? Vector2.down : Vector2.up,
                                            Mathf.Infinity,
                                            playerCollide,
                                            Mathf.Infinity,
                                            Mathf.Infinity
                                            );
                                        transform.position = new Vector2(
                                            transform.position.x,
                                            transform.position.y + (facingDown ? -wallTester.distance : wallTester.distance)
                                            );
                                        gravityDir = facingDown ? DIR_FLOOR : DIR_CEILING;
                                        return;
                                    }
                                }
                                else
                                    grounded = false;
                            }
                        }
    
                        // Now, let's see if we can jump
                        if (boxR.distance == 0)
                        {
                            transform.position = new Vector2(transform.position.x - 0.01f, transform.position.y);
                            UpdateBoxcasts();
                        }
                        if (Control.JumpHold() && grounded && !holdingJump && boxL.distance > 0.95f && !PlayState.paralyzed)
                        {
                            if (shelled)
                            {
                                if (boxD.distance < 0.4f && boxU.distance < 0.4f)
                                    break;
                                if (boxD.distance > 0.4f && boxU.distance < 0.4f)
                                    transform.position = new Vector2(transform.position.x, transform.position.y -
                                        (0.675f - boxU.distance - (facingLeft ? 0.25f : 0)));
                                else if (boxD.distance < 0.4f && boxU.distance > 0.4f)
                                    transform.position = new Vector2(transform.position.x, transform.position.y +
                                        (0.675f - boxD.distance - (facingLeft ? 0 : 0.25f)));
                                ToggleShell();
                            }
                            grounded = false;
                            if (PlayState.CheckForItem("Gravity Snail"))
                                velocity.x = -jumpPower[readIDJump] * jumpMod * Time.deltaTime;
                            else
                            {
                                transform.position = new Vector2(transform.position.x - 0.0625f - (box.size.y - box.size.x) * 0.5f, transform.position.y);
                                SwapDir(DIR_FLOOR);
                                SwitchSurfaceAxis();
                                gravityDir = DIR_FLOOR;
                                if (Control.DownHold())
                                    holdingShell = true;
                            }
                            PlayState.PlaySound("Jump");
                        }
                        if (Control.JumpHold() && !holdingJump)
                            holdingJump = true;
                        else if (!Control.JumpHold() && holdingJump)
                            holdingJump = false;
    
                        // Finally, we check to see if we can shell
                        if (Control.AxisY() == 0 &&
                            Control.RightHold() &&
                            !Control.JumpHold() &&
                            !Control.ShootHold() &&
                            !Control.StrafeHold() &&
                            !holdingShell && !PlayState.paralyzed)
                        {
                            if (!shelled)
                                ToggleShell();
                            else
                            {
                                if (boxD.distance < 0.4f && boxU.distance < 0.4f)
                                    break;
                                if (boxD.distance > 0.4f && boxU.distance < 0.4f)
                                    transform.position = new Vector2(transform.position.x, transform.position.y -
                                        (0.675f - boxU.distance - (facingLeft ? 0.25f : 0)));
                                else if (boxD.distance < 0.4f && boxU.distance > 0.4f)
                                    transform.position = new Vector2(transform.position.x, transform.position.y +
                                        (0.675f - boxD.distance - (facingLeft ? 0 : 0.25f)));
                                ToggleShell();
                            }
                            holdingShell = true;
                        }
                        else if (!holdingShell && Control.RightHold())
                            holdingShell = true;
                        if (holdingShell && !Control.RightHold())
                            holdingShell = false;
                        if (PlayState.IsTileSolid(transform.position))
                            transform.position = new Vector2(transform.position.x + 1, transform.position.y);
                    }
                    break;
                case DIR_CEILING:
                    // This if block's purpose is so that you can click that minus button on the left and hide it from view, just so that you don't have
                    // to scroll quite as much if you don't want to. Cleanup, basically
                    if (true)
                    {
                        // We start by zeroing our relative vertical velocity if we're grounded, and our relative horizontal velocity no matter what
                        velocity.x = 0;
                        if (grounded)
                            velocity.y = 0;
    
                        // From here, we perform relatively horizontal movement checks to move, stop if we hit a wall, and allow for climbing
                        if (Control.AxisX() != 0 && !Control.StrafeHold() && !PlayState.paralyzed)
                        {
                            if (shelled)
                            {
                                if (Control.AxisX() == (facingLeft ? 1 : -1))
                                    transform.position = new Vector2(transform.position.x + (0.1667f * (facingLeft ? 1 : -1)), transform.position.y);
                                if (grounded)
                                    ToggleShell();
                                float distance = Vector2.Distance(boxL.point, new Vector2(transform.position.x, boxL.point.y));
                                if (distance < box.size.x * 0.5f)
                                {
                                    transform.position = new Vector2(transform.position.x + ((box.size.x * 0.675f) - distance) *
                                        (boxL.point.x < transform.position.x ? 1 : -1), transform.position.y);
                                    UpdateBoxcasts();
                                }
                            }
                            SwapDir(Control.RightHold() ? DIR_WALL_RIGHT : DIR_WALL_LEFT);
                            float runSpeedValue = runSpeed[readIDSpeed] * speedMod * Time.fixedDeltaTime;
                            if ((facingLeft ? boxL : boxR).distance < runSpeedValue)
                            {
                                againstWallFlag = true;
                                velocity.x = facingLeft ? -runSpeedValue + (runSpeedValue - boxL.distance) + 0.0078125f :
                                    runSpeedValue - (runSpeedValue - boxR.distance) - 0.0078125f;
                                // In case the player happens to be holding the relative up/down button while the character runs face-first into a wall,
                                // we check to see if climbing is possible in either direction and switch the character's gravity state
                                if ((boxD.distance + boxU.distance) >= 1)
                                {
                                    if (!stunned)
                                    {
                                        if (Control.DownHold() || (Control.UpHold() && !grounded))
                                        {
                                            if (shelled)
                                            {
                                                if (boxL.distance > 0.4f && boxR.distance < 0.4f)
                                                    transform.position = new Vector2(transform.position.x - (0.58125f - boxR.distance - (facingLeft ? 0.25f : 0)),
                                                        transform.position.y);
                                                else if (boxL.distance < 0.4f && boxR.distance > 0.4f)
                                                    transform.position = new Vector2(transform.position.x + (0.58125f - boxL.distance - (facingLeft ? 0 : 0.25f)),
                                                        transform.position.y);
                                                ToggleShell();
                                            }
    
                                            transform.position = new Vector2(transform.position.x + velocity.x, transform.position.y);
                                            float boxCorrection = (box.size.y - box.size.x) * 0.5f;
                                            float ceilDis = boxD.distance - boxCorrection;
                                            float floorDis = boxU.distance - boxCorrection;
                                            SwitchSurfaceAxis();
                                            float adjustment = 0;
                                            if (grounded)
                                                adjustment = -boxCorrection;
                                            else
                                            {
                                                if (ceilDis < floorDis && ceilDis < box.size.y * 0.5f)
                                                    adjustment = ceilDis - (box.size.y * 0.5f);
                                                else if (floorDis < ceilDis && floorDis < box.size.y * 0.5f)
                                                    adjustment = -(floorDis - (box.size.y * 0.5f));
                                            }
                                            transform.position = new Vector2(
                                                transform.position.x + (facingLeft ? boxCorrection : -boxCorrection),
                                                transform.position.y - adjustment
                                                );
                                            SwapDir(Control.UpHold() ? DIR_CEILING : DIR_FLOOR);
                                            gravityDir = facingLeft ? DIR_WALL_LEFT : DIR_WALL_RIGHT;
                                            grounded = true;
                                            transform.position = new Vector2(Mathf.Round(transform.position.x * 2) * 0.5f + (facingLeft ? -0.01f : 0.01f), transform.position.y);
                                            UpdateBoxcasts();
                                            return;
                                        }
                                    }
                                }
                            }
                            else
                                velocity.x = facingLeft ? -runSpeedValue : runSpeedValue;
                            transform.position = new Vector2(transform.position.x + velocity.x, transform.position.y);
                            UpdateBoxcasts();
                        }
    
                        // Now, we perform relatively vertical checks. This mainly involves jumping and falling
                        if (!grounded)
                        {
                            if (!PlayState.CheckForItem("Gravity Snail"))
                            {
                                SwapDir(DIR_FLOOR);
                                gravityDir = DIR_FLOOR;
                                if (Control.DownHold())
                                    holdingShell = true;
                            }
                            else
                            {
                                bool pokedCeiling = false;
                                velocity.y += gravity[readIDSpeed] * gravityMod * Time.fixedDeltaTime;
                                if (velocity.y < 0 && !holdingJump)
                                    velocity.y = PlayState.Integrate(velocity.y, 0, 4, Time.fixedDeltaTime);
                                velocity.y = Mathf.Clamp(velocity.x, -Mathf.Infinity, -terminalVelocity[readIDSpeed]);
                                if (boxD.distance != 0 && boxU.distance != 0)
                                {
                                    if (boxD.distance < -velocity.y && Mathf.Sign(velocity.y) == -1)
                                    {
                                        velocity.y = -boxD.distance;
                                        pokedCeiling = true;
                                    }
                                    else if (boxU.distance < velocity.y && Mathf.Sign(velocity.y) == 1)
                                    {
                                        velocity.y = boxU.distance;
                                        grounded = true;
                                    }
                                }
                                if (!againstWallFlag)
                                {
                                    transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y);
                                }
                                else
                                {
                                    // This entire block here covers the specific case of slipping into a one-tall tunnel in a wall while midair
                                    for (int i = 0; i < 8; i++)
                                    {
                                        transform.position = new Vector2(transform.position.x, transform.position.y + (velocity.y * 0.125f));
                                        RaycastHit2D tunnelCheckUpper = Physics2D.Raycast(
                                            new Vector2(transform.position.x, transform.position.y + 0.375f),
                                            facingLeft ? Vector2.left : Vector2.right,
                                            Mathf.Infinity,
                                            playerCollide,
                                            Mathf.Infinity,
                                            Mathf.Infinity
                                            );
                                        RaycastHit2D tunnelCheckLower = Physics2D.Raycast(
                                            new Vector2(transform.position.x, transform.position.y - 0.375f),
                                            facingLeft ? Vector2.left : Vector2.right,
                                            Mathf.Infinity,
                                            playerCollide,
                                            Mathf.Infinity,
                                            Mathf.Infinity
                                            );
                                        if (tunnelCheckUpper.distance >= 1.5f && tunnelCheckLower.distance >= 1.5f)
                                        {
                                            transform.position = new Vector2(
                                                transform.position.x + ((facingLeft ? -runSpeed[readIDSpeed] : runSpeed[readIDSpeed]) * speedMod * Time.fixedDeltaTime),
                                                Mathf.Floor(transform.position.y) + 0.5f);
                                            i = 8;
                                        }
                                    }
                                }
                                UpdateBoxcasts();
                                if (pokedCeiling)
                                {
                                    velocity.y = 0;
                                    if (Control.DownHold())
                                    {
                                        gravityDir = DIR_FLOOR;
                                        SwapDir(DIR_FLOOR);
                                        grounded = true;
                                        holdingShell = true;
                                        return;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (boxU.distance > 0.0125f)
                            {
                                if (boxCorner.distance <= 0.0125f)
                                {
                                    if (!PlayState.CheckForItem("Gravity Snail"))
                                    {
                                        SwapDir(DIR_FLOOR);
                                        gravityDir = DIR_FLOOR;
                                        if (Control.DownHold())
                                            holdingShell = true;
                                    }
                                    else if (Control.UpHold() && Control.AxisX() == (facingLeft ? -1 : 1) && !stunned)
                                    {
                                        SwapDir(facingLeft ? DIR_WALL_RIGHT : DIR_WALL_LEFT);
                                        SwitchSurfaceAxis();
                                        RaycastHit2D wallTester = Physics2D.Raycast(
                                            new Vector2(transform.position.x + (facingLeft ? -box.size.x * 0.5f : box.size.x * 0.5f), transform.position.y + 0.75f),
                                            facingLeft ? Vector2.left : Vector2.right,
                                            Mathf.Infinity,
                                            playerCollide,
                                            Mathf.Infinity,
                                            Mathf.Infinity
                                            );
                                        transform.position = new Vector2(
                                            transform.position.x + (facingLeft ? -wallTester.distance : wallTester.distance),
                                            transform.position.y
                                            );
                                        gravityDir = facingLeft ? DIR_WALL_LEFT : DIR_WALL_RIGHT;
                                        return;
                                    }
                                }
                                else
                                    grounded = false;
                            }
                        }
    
                        // Now, let's see if we can jump
                        if (boxU.distance == 0)
                        {
                            transform.position = new Vector2(transform.position.x, transform.position.y - 0.01f);
                            UpdateBoxcasts();
                        }
                        if (Control.JumpHold() && grounded && !holdingJump && boxD.distance > 0.95f && !PlayState.paralyzed)
                        {
                            if (shelled)
                            {
                                if (boxL.distance < 0.4f && boxR.distance < 0.4f)
                                    break;
                                if (boxL.distance > 0.4f && boxR.distance < 0.4f)
                                    transform.position = new Vector2(transform.position.x - (0.675f - boxR.distance - (facingLeft ? 0.25f : 0)),
                                        transform.position.y);
                                else if (boxL.distance < 0.4f && boxR.distance > 0.4f)
                                    transform.position = new Vector2(transform.position.x + (0.675f - boxL.distance - (facingLeft ? 0 : 0.25f)),
                                        transform.position.y);
                                ToggleShell();
                            }
                            grounded = false;
                            if (PlayState.CheckForItem("Gravity Snail"))
                                velocity.y = -jumpPower[readIDJump] * jumpMod * Time.deltaTime;
                            else
                            {
                                SwapDir(DIR_FLOOR);
                                gravityDir = DIR_FLOOR;
                            }
                            PlayState.PlaySound("Jump");
                        }
                        if (Control.JumpHold() && !holdingJump)
                            holdingJump = true;
                        else if (!Control.JumpHold() && holdingJump)
                            holdingJump = false;
    
                        // Finally, we check to see if we can shell
                        if (Control.UpHold() &&
                            Control.AxisX() == 0 &&
                            !Control.JumpHold() &&
                            !Control.ShootHold() &&
                            !Control.StrafeHold() &&
                            !holdingShell && !PlayState.paralyzed)
                        {
                            if (!shelled)
                                ToggleShell();
                            else
                            {
                                if (boxL.distance < 0.4f && boxR.distance < 0.4f)
                                    break;
                                if (boxL.distance > 0.4f && boxR.distance < 0.4f)
                                    transform.position = new Vector2(transform.position.x - (0.675f - boxR.distance - (facingLeft ? 0.25f : 0)),
                                        transform.position.y);
                                else if (boxL.distance < 0.4f && boxR.distance > 0.4f)
                                    transform.position = new Vector2(transform.position.x + (0.675f - boxL.distance - (facingLeft ? 0 : 0.25f)),
                                        transform.position.y);
                                ToggleShell();
                            }
                            holdingShell = true;
                        }
                        else if (!holdingShell && Control.UpHold())
                            holdingShell = true;
                        if (holdingShell && !Control.UpHold())
                            holdingShell = false;
                        if (PlayState.IsTileSolid(transform.position))
                            transform.position = new Vector2(transform.position.x, transform.position.y - 1);
                    }
                    break;
            }
    
            if ((Control.ShootHold() || Control.StrafeHold()) && !PlayState.paralyzed)
            {
                if (shelled)
                {
                    if (gravityDir == DIR_WALL_LEFT || gravityDir == DIR_WALL_RIGHT)
                    {
                        if (!(boxD.distance < 0.4f && boxU.distance < 0.4f))
                        {
                            if (boxD.distance > 0.4f && boxU.distance < 0.4f)
                                transform.position = new Vector2(transform.position.x, transform.position.y -
                                    (0.675f - boxU.distance - (facingLeft ? 0.25f : 0)));
                            else if (boxD.distance < 0.4f && boxU.distance > 0.4f)
                                transform.position = new Vector2(transform.position.x, transform.position.y +
                                    (0.675f - boxD.distance - (facingLeft ? 0 : 0.25f)));
                        }
                    }
                    else
                    {
                        if (!(boxL.distance < 0.4f && boxR.distance < 0.4f))
                        {
                            if (boxL.distance > 0.4f && boxR.distance < 0.4f)
                                transform.position = new Vector2(transform.position.x - (0.675f - boxR.distance - (facingLeft ? 0.25f : 0)),
                                    transform.position.y);
                            else if (boxL.distance < 0.4f && boxR.distance > 0.4f)
                                transform.position = new Vector2(transform.position.x + (0.675f - boxL.distance - (facingLeft ? 0 : 0.25f)),
                                    transform.position.y);
                        }
                    }
                    ToggleShell();
                }
                Shoot();
            }
        }
    }

    // This function is used to reset all five boxcasts the player character uses for ground checks. It's called once per
    // FixedUpdate() call automatically plus any additional resets needed, for instance, after a gravity change
    private void UpdateBoxcasts()
    {
        boxL = Physics2D.BoxCast(
            new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y),
            new Vector2(box.size.x, box.size.y - 0.015625f),
            0,
            Vector2.left,
            Mathf.Infinity,
            playerCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );
        boxR = Physics2D.BoxCast(
            new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y),
            new Vector2(box.size.x, box.size.y - 0.015625f),
            0,
            Vector2.right,
            Mathf.Infinity,
            playerCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );
        boxU = Physics2D.BoxCast(
            new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y),
            new Vector2(box.size.x - 0.015625f, box.size.y),
            0,
            Vector2.up,
            Mathf.Infinity,
            playerCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );
        boxD = Physics2D.BoxCast(
            new Vector2(transform.position.x + box.offset.x, transform.position.y + box.offset.y),
            new Vector2(box.size.x - 0.015625f, box.size.y),
            0,
            Vector2.down,
            Mathf.Infinity,
            playerCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );

        Vector2 cornerTestDir;
        if (gravityDir == DIR_CEILING)
            cornerTestDir = Vector2.up;
        else if (gravityDir == DIR_WALL_LEFT)
            cornerTestDir = Vector2.left;
        else if (gravityDir == DIR_WALL_RIGHT)
            cornerTestDir = Vector2.right;
        else
            cornerTestDir = Vector2.down;
        boxCorner = Physics2D.BoxCast(
            lastPosition,
            lastSize,
            0,
            cornerTestDir,
            Mathf.Infinity,
            playerCollide,
            Mathf.Infinity,
            Mathf.Infinity
            );
    }

    // This function is called to reorient the player character in any way necessary
    // Note: this only accounts for four directions in either the ground/ceiling state or the wall state, never both. A call to
    // SwitchSurfaceAxis() is necessary for that
    private void SwapDir(int dirToFace)
    {
        switch (dirToFace)
        {
            case DIR_FLOOR:
                facingDown = true;
                break;
            case DIR_WALL_LEFT:
                facingLeft = true;
                break;
            case DIR_WALL_RIGHT:
                facingLeft = false;
                break;
            case DIR_CEILING:
                facingDown = false;
                break;
        }
    }

    // This function is used to swap the player character between the ground/ceiling state and the wall state and vice versa
    private void SwitchSurfaceAxis()
    {
        axisFlag = !axisFlag;
        box.size = new Vector2(box.size.y, box.size.x);
        box.offset = new Vector2(Mathf.Abs(box.offset.y) * (facingLeft ? 1 : -1), Mathf.Abs(box.offset.x) * (facingDown ? 1 : -1));
        UpdateBoxcasts();
    }

    // This function is called whenever a shelled character asks to enter/exit their shell
    public void ToggleShell()
    {
        if (stunned && !shelled)
            return;
        //if (shelled)
        //{
        //    box.offset = Vector2.zero;
        //    if (gravityDir == DIR_WALL_LEFT || gravityDir == DIR_WALL_RIGHT)
        //        box.size = new Vector2(hitboxSize_normal.y, hitboxSize_normal.x);
        //    else
        //        box.size = hitboxSize_normal;
        //}
        //else
        //{
        //    if (gravityDir == DIR_WALL_LEFT || gravityDir == DIR_WALL_RIGHT)
        //    {
        //        //if (facingDown)
        //        //    box.offset = new Vector2(hitboxOffset_shell.y, hitboxOffset_shell.x);
        //        //else
        //        //    box.offset = new Vector2(0, -hitboxOffset_shell.x);
        //        box.size = new Vector2(hitboxSize_shell.y, hitboxSize_shell.x);
        //    }
        //    else
        //    {
        //        if (facingLeft)
        //            box.offset = new Vector2(HITBOX_SHELL_OFFSET, 0);
        //        else
        //            box.offset = new Vector2(-HITBOX_SHELL_OFFSET, 0);
        //        box.size = new Vector2(HITBOX_SHELL_X, HITBOX_SHELL_Y);
        //    }
        //    PlayState.PlaySound("Shell");
        //}
        if (shelled)
        {
            if (gravityDir == DIR_WALL_LEFT || gravityDir == DIR_WALL_RIGHT)
            {
                box.offset = new Vector2(hitboxOffset_normal.y * (facingLeft ? -1 : 1), hitboxOffset_normal.x * (facingDown ? -1 : 1));
                box.size = new Vector2(hitboxSize_normal.y, hitboxSize_normal.x);
            }
            else
            {
                box.offset = new Vector2(hitboxOffset_normal.x * (facingLeft ? -1 : 1), hitboxOffset_normal.y * (facingDown ? -1 : 1));
                box.size = hitboxSize_normal;
            }
        }
        else
        {
            if (gravityDir == DIR_WALL_LEFT || gravityDir == DIR_WALL_RIGHT)
            {
                box.offset = new Vector2(hitboxOffset_shell.y * (facingLeft ? -1 : 1), hitboxOffset_shell.x * (facingDown ? -1 : 1));
                box.size = new Vector2(hitboxSize_shell.y, hitboxSize_shell.x);
            }
            else
            {
                box.offset = new Vector2(hitboxOffset_shell.x * (facingLeft ? -1 : 1), hitboxOffset_shell.y * (facingDown ? -1 : 1));
                box.size = hitboxSize_shell;
            }
            PlayState.PlaySound("Shell");
        }
        shelled = !shelled;
        UpdateBoxcasts();
    }

    // This function handles activation of projectiles when the player presses either shoot button
    private void Shoot()
    {
        if (fireCooldown == 0 && armed && !PlayState.paralyzed)
        {
            Vector2 inputDir = new Vector2(Control.AxisX(), Control.AxisY());
            int type = selectedWeapon + (PlayState.CheckForItem("Devastator") ? 3 : 0);
            int dir = 0;
            switch (inputDir.x + "" + inputDir.y)
            {
                case "-11":
                    dir = 0;
                    break;
                case "01":
                    dir = 1;
                    break;
                case "11":
                    dir = 2;
                    break;
                case "-10":
                    dir = 3;
                    break;
                case "10":
                    dir = 4;
                    break;
                case "-1-1":
                    dir = 5;
                    break;
                case "0-1":
                    dir = 6;
                    break;
                case "1-1":
                    dir = 7;
                    break;
                case "00":
                    dir = -1;
                    break;
            }

            if (type == 1 && grounded)
            {
                if (gravityDir == DIR_FLOOR && (dir == 5 || dir == 6 || dir == 7))
                    dir = facingLeft ? 3 : 4;
                else if (gravityDir == DIR_WALL_LEFT && (dir == 0 || dir == 3 || dir == 5))
                    dir = facingDown ? 6 : 1;
                else if (gravityDir == DIR_WALL_RIGHT && (dir == 2 || dir == 4 || dir == 7))
                    dir = facingDown ? 6 : 1;
                else if (gravityDir == DIR_CEILING && (dir == 0 || dir == 1 || dir == 2))
                    dir = facingLeft ? 3 : 4;
            }
            if (dir == -1)
            {
                if (gravityDir == DIR_FLOOR && dir == -1)
                    dir = facingLeft ? 3 : 4;
                else if (gravityDir == DIR_WALL_LEFT && dir == -1)
                    dir = facingDown ? 6 : 1;
                else if (gravityDir == DIR_WALL_RIGHT && dir == -1)
                    dir = facingDown ? 6 : 1;
                else if (gravityDir == DIR_CEILING && dir == -1)
                    dir = facingLeft ? 3 : 4;
            }
            if (!bulletPool.transform.GetChild(bulletID).GetComponent<Bullet>().isActive)
            {
                bulletPool.transform.GetChild(bulletID).GetComponent<Bullet>().Shoot(type, dir);
                bulletID++;
                if (bulletID >= bulletPool.transform.childCount)
                    bulletID = 0;
                fireCooldown = WEAPON_COOLDOWNS[type - 1];
                PlayState.PlaySound(type switch { 2 => "ShotBoomerang", 3 => "ShotRainbow", 4 => "ShotRainbow", 5 => "ShotRainbow", 6 => "ShotRainbow", _ => "ShotPeashooter", });
            }
        }
    }

    public void HitFor(int damage)
    {
        if (stunned || inDeathCutscene)
            return;

        if (damage < 0)
        {
            health = Mathf.Clamp(health - damage, 0, maxHealth);
            PlayState.globalFunctions.UpdateHearts();
        }
        else
        {
            if (health - damage <= 0)
                StartCoroutine(nameof(DieAndRespawn));
            else
                StartCoroutine(StunTimer(damage));
        }
    }

    public IEnumerator StunTimer(int damage)
    {
        if (shelled && PlayState.CheckForItem("Shell Shield"))
            PlayState.PlaySound("Ping");
        else
        {
            health = Mathf.RoundToInt(Mathf.Clamp(health - damage, 0, Mathf.Infinity));
            PlayState.globalFunctions.UpdateHearts();
            PlayState.PlaySound("Hurt");
        }
        stunned = true;
        float timer = 0;
        while (timer < 1)
        {
            if (PlayState.gameState == PlayState.GameState.game)
            {
                sprite.enabled = !sprite.enabled;
                timer += Time.deltaTime;
            }
            else if (PlayState.gameState == PlayState.GameState.menu)
                timer = 1;
            yield return new WaitForEndOfFrame();
        }
        if (PlayState.gameState != PlayState.GameState.menu)
        {
            sprite.enabled = true;
            stunned = false;
        }
    }

    IEnumerator DieAndRespawn()
    {
        if (shelled)
            ToggleShell();
        health = 0;
        PlayState.globalFunctions.UpdateHearts();
        inDeathCutscene = true;
        box.enabled = false;
        PlayState.paralyzed = true;
        PlayState.PlaySound("Death");
        float timer = 0;
        bool hasStartedTransition = false;
        Vector3 fallDir = new Vector3(0.125f, 0.35f, 0);
        if (!facingLeft)
            fallDir = new Vector3(-0.125f, 0.35f, 0);
        while ((timer < 1.6f && PlayState.quickDeathTransition) || (timer < 2 && !PlayState.quickDeathTransition))
        {
            transform.position += fallDir;
            fallDir = new Vector3(fallDir.x, Mathf.Clamp(fallDir.y - 0.025f, -0.5f, Mathf.Infinity), 0);
            yield return new WaitForFixedUpdate();
            timer += Time.fixedDeltaTime;
            if (timer > 1 && !hasStartedTransition)
            {
                hasStartedTransition = true;
                PlayState.ScreenFlash("Death Transition");
            }
        }
        yield return new WaitForEndOfFrame();
        if (PlayState.positionOfLastRoom == PlayState.positionOfLastSave)
        {
            Transform deathLocation = PlayState.roomTriggerParent.transform.GetChild((int)PlayState.positionOfLastRoom.x).GetChild((int)PlayState.positionOfLastRoom.y);
            deathLocation.GetComponent<Collider2D>().enabled = true;
            deathLocation.GetComponent<RoomTrigger>().active = true;
            deathLocation.GetComponent<RoomTrigger>().DespawnEverything();
        }
        PlayState.ToggleBossfightState(false, 0, true);
        transform.position = PlayState.respawnCoords;
        inDeathCutscene = false;
        box.enabled = true;
        PlayState.paralyzed = false;
        health = maxHealth;
        PlayState.globalFunctions.UpdateHearts();
        yield return new WaitForEndOfFrame();
        PlayState.ScreenFlash("Room Transition");
    }

    public void RemoteJump(float jumpPower)
    {
        //switch (PlayState.currentCharacter)
        //{
        //    default:
        //    case "Snaily":
        //        playerScriptSnaily.RemoteJump(jumpPower);
        //        break;
        //    case "Sluggy":
        //        break;
        //    case "Upside":
        //        break;
        //    case "Leggy":
        //        break;
        //    case "Blobby":
        //        break;
        //    case "Leechy":
        //        break;
        //}
    }

    public void RemoteGravity(int direction)
    {
        //switch (PlayState.currentCharacter)
        //{
        //    default:
        //    case "Snaily":
        //        playerScriptSnaily.RemoteGravity(direction);
        //        break;
        //    case "Sluggy":
        //        break;
        //    case "Upside":
        //        break;
        //    case "Leggy":
        //        break;
        //    case "Blobby":
        //        break;
        //    case "Leechy":
        //        break;
        //}
    }
}
