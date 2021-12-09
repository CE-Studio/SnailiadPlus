using System.Collections;
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
    public const float RAINBOW_WAVE_COOLDOWN = 0.15f;

    private bool _inShell = false;
    public int _currentSurface = 0;
    public bool _facingLeft = false;
    public bool _facingUp = false;
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
    private int bulletPointer = 0;
    private float fireCooldown = 0;
    public int selectedWeapon = 0;
    public int health = 12;
    public int maxHealth = 12;
    public bool stunned = false;
    private string currentAnim = "";
    public bool inDeathCutscene = false;
    public int gravityDir = 0;

    public Animator anim;
    public SpriteRenderer sprite;
    public BoxCollider2D box;
    public AudioSource sfx;
    public AudioClip shell;
    public AudioClip jump;
    public AudioClip hurt;
    public AudioClip die;
    public AudioClip shootRWave;
    public GameObject bulletPool;
    public GameObject iconRWave;
    public Sprite blank;
    public Sprite iconRWaveDeselected;
    public Sprite iconRWaveSelected;
    public GameObject hearts;
    public Sprite heart0;
    public Sprite heart1;
    public Sprite heart2;
    public Sprite heart3;
    public Sprite heart4;
    public GameObject itemTextGroup;
    public GameObject itemPercentageGroup;

    public LayerMask playerCollide;

    public GameObject debugUp;
    public GameObject debugDown;
    public GameObject debugLeft;
    public GameObject debugRight;
    public GameObject debugJump;
    public GameObject debugShoot;
    public GameObject debugStrafe;
    public Sprite keyIdle;
    public Sprite keyPressed;
    public Sprite keyHeld;

    // FPS stuff
    int frameCount = 0;
    float dt = 0f;
    float fps = 0f;
    float updateRate = 4;

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

        bulletPool = GameObject.Find("Player Bullet Pool");

        hearts = GameObject.Find("View/Hearts");
        iconRWave = GameObject.Find("View/Weapon Icons/Rainbow Wave");

        debugUp = GameObject.Find("View/Debug Keypress Indicators/Up");
        debugDown = GameObject.Find("View/Debug Keypress Indicators/Down");
        debugLeft = GameObject.Find("View/Debug Keypress Indicators/Left");
        debugRight = GameObject.Find("View/Debug Keypress Indicators/Right");
        debugJump = GameObject.Find("View/Debug Keypress Indicators/Jump");
        debugShoot = GameObject.Find("View/Debug Keypress Indicators/Shoot");
        debugStrafe = GameObject.Find("View/Debug Keypress Indicators/Strafe");

        itemTextGroup = GameObject.Find("View/Item Get Text");
        itemPercentageGroup = GameObject.Find("View/Item Percentage Text");

        RenderNewHearts();
        UpdateHearts();

        StartCoroutine("DebugKeys");
    }

    // Update(), called less frequently (every drawn frame), actually gets most of the inputs and converts them to what they should be given any current surface state
    void Update()
    {
        if (PlayState.gameState == "Game")
        {
            // These are only here to make sure they're called once, before anything else that needs it
            PlayState.armorPingPlayedThisFrame = false;
            PlayState.explodePlayedThisFrame = false;
        }
    }

    void LateUpdate()
    {
        PlayState.fg2Layer.transform.localPosition = new Vector2(
            Mathf.Round((PlayState.cam.transform.position.x - PlayState.camCenter.x) * PlayState.parallaxFg2Mod * 16) * 0.0625f,
            Mathf.Round((PlayState.cam.transform.position.y - PlayState.camCenter.y) * PlayState.parallaxFg2Mod * 16) * 0.0625f
            );
        PlayState.fg1Layer.transform.localPosition = new Vector2(
            Mathf.Round((PlayState.cam.transform.position.x - PlayState.camCenter.x) * PlayState.parallaxFg1Mod * 16) * 0.0625f,
            Mathf.Round((PlayState.cam.transform.position.y - PlayState.camCenter.y) * PlayState.parallaxFg1Mod * 16) * 0.0625f
            );
        PlayState.bgLayer.transform.localPosition = new Vector2(
            Mathf.Round((PlayState.cam.transform.position.x - PlayState.camCenter.x) * PlayState.parallaxBgMod * 16) * 0.0625f,
            Mathf.Round((PlayState.cam.transform.position.y - PlayState.camCenter.y) * PlayState.parallaxBgMod * 16) * 0.0625f
            );
        PlayState.skyLayer.transform.localPosition = new Vector2(
            Mathf.Round((PlayState.cam.transform.position.x - PlayState.camCenter.x) * PlayState.parallaxSkyMod * 16) * 0.0625f,
            Mathf.Round((PlayState.cam.transform.position.y - PlayState.camCenter.y) * PlayState.parallaxSkyMod * 16) * 0.0625f
            );

        // FPS calculator
        frameCount++;
        dt += Time.deltaTime;
        if (dt > 1 / updateRate)
        {
            fps = frameCount / dt;
            frameCount = 0;
            dt -= 1 / updateRate;
        }
        PlayState.fpsText.text = "" + Mathf.Round(fps) + "FPS";
        PlayState.fpsShadow.text = "" + Mathf.Round(fps) + "FPS";
    }

    // This coroutine here is meant to display the keypress indicators intended for debugging purposes
    IEnumerator DebugKeys()
    {
        while (true)
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
            if (Input.GetAxisRaw("Shoot") == 1)
            {
                debugShoot.GetComponent<SpriteRenderer>().sprite = keyHeld;
            }
            else
            {
                debugShoot.GetComponent<SpriteRenderer>().sprite = keyIdle;
            }
            if (Input.GetAxisRaw("Strafe") == 1)
            {
                debugStrafe.GetComponent<SpriteRenderer>().sprite = keyHeld;
            }
            else
            {
                debugStrafe.GetComponent<SpriteRenderer>().sprite = keyIdle;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public void ExecuteCoverCommand(string type)
    {
        switch (type)
        {
            case "Room Transition":
                StartCoroutine(nameof(CoverRoomTransition));
                break;
            case "Death Transition":
                StartCoroutine(nameof(CoverDeathTransition));
                break;
        }
    }
    public IEnumerator CoverRoomTransition()
    {
        SpriteRenderer sprite = PlayState.screenCover.GetComponent<SpriteRenderer>();
        while (sprite.color.a > 0)
        {
            yield return new WaitForFixedUpdate();
            sprite.color = new Color32(0, 0, 0, (byte)Mathf.Clamp((sprite.color.a * 255) - 15, 0, Mathf.Infinity));
        }
    }

    public IEnumerator CoverDeathTransition()
    {
        SpriteRenderer sprite = PlayState.screenCover.GetComponent<SpriteRenderer>();
        float timer = 0;
        while (sprite.color.a < 1)
        {
            yield return new WaitForFixedUpdate();
            sprite.color = new Color32(0, 64, 127, (byte)Mathf.Lerp(0, 255, timer * 2));
            timer += Time.fixedDeltaTime;
        }
    }

    public void RenderNewHearts()
    {
        if (hearts.transform.childCount != 0)
        {
            for (int i = hearts.transform.childCount; i > -1; i--)
            {
                Destroy(hearts.transform.GetChild(i));
            }
        }
        for (int i = 0; i < maxHealth * 0.25f; i++)
        {
            GameObject NewHeart = new GameObject();
            NewHeart.transform.parent = hearts.transform;
            NewHeart.transform.localPosition = new Vector3(-12 + (0.5f * (i % 7)), 7 - (0.5f * ((i / 7) % 7)), 0);
            NewHeart.AddComponent<SpriteRenderer>();
            NewHeart.GetComponent<SpriteRenderer>().sprite = heart4;
            NewHeart.GetComponent<SpriteRenderer>().sortingOrder = -1;
            NewHeart.name = "Heart " + (i + 1) + " (HP " + (i * 4) + "-" + (i * 4 + 4) + ")";
        }
    }

    public void UpdateHearts()
    {
        if (hearts.transform.childCount != 0)
        {
            int totalOfPreviousHearts = 0;
            for (int i = 0; i < hearts.transform.childCount; i++)
            {
                switch (health - totalOfPreviousHearts)
                {
                    case 1:
                        hearts.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = heart1;
                        break;
                    case 2:
                        hearts.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = heart2;
                        break;
                    case 3:
                        hearts.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = heart3;
                        break;
                    default:
                        if (Mathf.Sign(health - totalOfPreviousHearts) == 1 && (health - totalOfPreviousHearts) != 0)
                        {
                            hearts.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = heart4;
                        }
                        else
                        {
                            hearts.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = heart0;
                        }
                        break;
                }
                totalOfPreviousHearts += 4;
            }
        }
    }

    public IEnumerator StunTimer()
    {
        stunned = true;
        sfx.PlayOneShot(hurt);
        UpdateHearts();
        _currentSurface = DIR_FLOOR;
        _inShell = false;
        float timer = 0;
        while (timer < 1)
        {
            sprite.enabled = !sprite.enabled;
            timer += 0.02f;
            yield return new WaitForFixedUpdate();
        }
        sprite.enabled = true;
        stunned = false;
    }

    public void BecomeStunned()
    {
        StartCoroutine(nameof(StunTimer));
    }

    public void FlashItemText(string itemName)
    {
        StartCoroutine(FlashText("item", itemName));
    }

    public void FlashCollectionText()
    {
        StartCoroutine(FlashText("collection"));
    }

    public IEnumerator FlashText(string textType, string itemName = "No item")
    {
        float timer = 0;
        int colorPointer = 0;
        int colorCooldown = 0;
        switch (textType)
        {
            default:
                yield return new WaitForEndOfFrame();
                break;
            case "item":
                SetTextAlpha("item", 255);
                SetTextDisplayed("item", itemName);
                while (timer < 3)
                {
                    if (colorCooldown <= 0)
                    {
                        switch (colorPointer)
                        {
                            case 0:
                                itemTextGroup.transform.GetChild(0).GetComponent<TextMesh>().color = new Color32(189, 191, 198, 255);
                                break;
                            case 1:
                                itemTextGroup.transform.GetChild(0).GetComponent<TextMesh>().color = new Color32(247, 196, 223, 255);
                                break;
                            case 2:
                                itemTextGroup.transform.GetChild(0).GetComponent<TextMesh>().color = new Color32(252, 214, 136, 255);
                                break;
                            case 3:
                                itemTextGroup.transform.GetChild(0).GetComponent<TextMesh>().color = new Color32(170, 229, 214, 255);
                                break;
                        }
                        colorPointer++;
                        if (colorPointer >= 4)
                        {
                            colorPointer = 0;
                        }
                        colorCooldown = 2;
                    }
                    else
                        colorCooldown--;

                    if (timer > 2.5f)
                    {
                        SetTextAlpha("item", Mathf.RoundToInt(Mathf.Lerp(255, 0, (timer - 2.5f) * 2)));
                    }
                    yield return new WaitForFixedUpdate();
                    timer += Time.deltaTime;
                }
                SetTextAlpha("item", 0);
                break;
            case "collection":
                SetTextAlpha("collection", 255);
                SetTextDisplayed("collection", "Item collection ??% complete!  Saving not yet implemented.");
                while (timer < 2)
                {
                    if (colorCooldown <= 0)
                    {
                        switch (colorPointer)
                        {
                            case 0:
                                itemPercentageGroup.transform.GetChild(0).GetComponent<TextMesh>().color = new Color32(189, 191, 198, 255);
                                break;
                            case 1:
                                itemPercentageGroup.transform.GetChild(0).GetComponent<TextMesh>().color = new Color32(247, 196, 223, 255);
                                break;
                            case 2:
                                itemPercentageGroup.transform.GetChild(0).GetComponent<TextMesh>().color = new Color32(252, 214, 136, 255);
                                break;
                            case 3:
                                itemPercentageGroup.transform.GetChild(0).GetComponent<TextMesh>().color = new Color32(170, 229, 214, 255);
                                break;
                        }
                        colorPointer++;
                        if (colorPointer >= 4)
                        {
                            colorPointer = 0;
                        }
                        colorCooldown = 2;
                    }
                    else
                        colorCooldown--;

                    if (timer > 1.5f)
                    {
                        SetTextAlpha("collection", Mathf.RoundToInt(Mathf.Lerp(255, 0, (timer - 1.5f) * 2)));
                    }
                    yield return new WaitForFixedUpdate();
                    timer += Time.deltaTime;
                }
                SetTextAlpha("collection", 0);
                break;
        }
        yield return new WaitForEndOfFrame();
    }

    void SetTextAlpha(string textGroup, int alpha)
    {
        switch (textGroup)
        {
            case "item":
                foreach (Transform textObj in itemTextGroup.transform)
                {
                    textObj.GetComponent<TextMesh>().color = new Color32(
                        (byte)(textObj.GetComponent<TextMesh>().color.r * 255),
                        (byte)(textObj.GetComponent<TextMesh>().color.g * 255),
                        (byte)(textObj.GetComponent<TextMesh>().color.b * 255),
                        (byte)alpha
                        );
                }
                break;
            case "collection":
                foreach (Transform textObj in itemPercentageGroup.transform)
                {
                    textObj.GetComponent<TextMesh>().color = new Color32(
                        (byte)(textObj.GetComponent<TextMesh>().color.r * 255),
                        (byte)(textObj.GetComponent<TextMesh>().color.g * 255),
                        (byte)(textObj.GetComponent<TextMesh>().color.b * 255),
                        (byte)alpha
                        );
                }
                break;
        }
    }

    void SetTextDisplayed(string textGroup, string textToDisplay)
    {
        switch (textGroup)
        {
            case "item":
                foreach (Transform textObj in itemTextGroup.transform)
                    textObj.GetComponent<TextMesh>().text = textToDisplay;
                break;
            case "collection":
                foreach (Transform textObj in itemPercentageGroup.transform)
                    textObj.GetComponent<TextMesh>().text = textToDisplay;
                break;
        }
    }

    IEnumerator DieAndRespawn()
    {
        health = 0;
        UpdateHearts();
        inDeathCutscene = true;
        box.enabled = false;
        PlayState.paralyzed = true;
        sfx.PlayOneShot(die);
        PlayAnim("die");
        float timer = 0;
        bool hasStartedTransition = false;
        Vector3 fallDir = new Vector3(0.125f, 0.35f, 0);
        if (!_facingLeft)
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
        //GameObject roomArray = GameObject.Find("Room Triggers");
        //for (int i = 0; i < roomArray.transform.childCount; i++)
        //{
        //    if (!roomArray.transform.GetChild(i).GetComponent<BoxCollider2D>().enabled)
        //        roomArray.transform.GetChild(i).GetComponent<BoxCollider2D>().enabled = true;
        //}
        yield return new WaitForEndOfFrame();
        transform.position = PlayState.respawnCoords;
        inDeathCutscene = false;
        box.enabled = true;
        PlayAnim("idle");
        PlayState.paralyzed = false;
        health = maxHealth;
        UpdateHearts();
        PlayState.ScreenFlash("Room Transition");
    }

    public void Die()
    {
        StartCoroutine(nameof(DieAndRespawn));
    }

    public void PlayAnim(string state)
    {
        string newAnim = "Normal ";
        if (state != "die")
        {
            if (_currentSurface == 1)
                newAnim += "wall ";
            else
                newAnim += "floor ";
        }
        newAnim += state;
        if (newAnim != currentAnim)
        {
            currentAnim = newAnim;
            anim.Play(newAnim, 0, 0);
        }
    }
}
