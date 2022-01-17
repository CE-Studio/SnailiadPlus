using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public const int DIR_FLOOR = 0;
    //private bool inShell = false;
    public int currentSurface = 0;
    public bool facingLeft = false;
    public bool facingDown = false;
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
    public AudioClip hurt;
    public AudioClip die;
    public GameObject bulletPool;
    public Sprite blank;
    public Sprite iconPeaDeselected;
    public Sprite iconPeaSelected;
    public Sprite iconBoomDeselected;
    public Sprite iconBoomSelected;
    public Sprite iconWaveDeselected;
    public Sprite iconWaveSelected;
    public GameObject hearts;
    public Sprite heart0;
    public Sprite heart1;
    public Sprite heart2;
    public Sprite heart3;
    public Sprite heart4;
    public GameObject itemTextGroup;
    public GameObject itemPercentageGroup;
    public GameObject gameSaveGroup;

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

    public GameObject weaponIcon1;
    public GameObject weaponIcon2;
    public GameObject weaponIcon3;
    public SpriteRenderer[] weaponIcons;

    public Snaily playerScriptSnaily;

    public double nextLoopEvent;

    // FPS stuff
    int frameCount = 0;
    float dt = 0f;
    float fps = 0f;
    float updateRate = 4;

    // Global sound flag stuff
    int pingTimer = 0;
    int explodeTimer = 0;

    // Start() is called at the very beginning of the script's lifetime. It's used to initialize certain variables and states for components to be in.
    void Start()
    {
        // All this does is set Snaily's components to simpler variables that can be more easily called
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        sfx = GetComponent<AudioSource>();

        weaponIcons = new SpriteRenderer[]
        {
            weaponIcon1.GetComponent<SpriteRenderer>(),
            weaponIcon2.GetComponent<SpriteRenderer>(),
            weaponIcon3.GetComponent<SpriteRenderer>()
        };
        foreach (SpriteRenderer sprite in weaponIcons)
            sprite.enabled = false;
        weaponIcons[0].sprite = iconPeaDeselected;
        weaponIcons[1].sprite = iconBoomDeselected;
        weaponIcons[2].sprite = iconWaveDeselected;

        RenderNewHearts();
        UpdateHearts();

        PlayState.AssignProperCollectibleIDs();

        StartCoroutine("DebugKeys");
    }

    // Update(), called less frequently (every drawn frame), actually gets most of the inputs and converts them to what they should be given any current surface state
    void Update()
    {
        if (PlayState.gameState == "Game")
        {
            // These are only here to make sure they're called once, before anything else that needs it
            if (PlayState.armorPingPlayedThisFrame)
            {
                pingTimer++;
                if (pingTimer >= 7)
                {
                    pingTimer = 0;
                    PlayState.armorPingPlayedThisFrame = false;
                }
            }
            if (PlayState.explodePlayedThisFrame)
            {
                explodeTimer++;
                if (explodeTimer >= 7)
                {
                    explodeTimer = 0;
                    PlayState.explodePlayedThisFrame = false;
                }
            }

            // Marking the "has jumped" flag for Snail NPC 01's dialogue
            if (Input.GetAxisRaw("Jump") == 1)
                PlayState.hasJumped = true;

            // Weapon swapping
            if (Input.GetKeyDown(KeyCode.Alpha1) && PlayState.CheckForItem(0))
                ChangeActiveWeapon(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2) && (PlayState.CheckForItem(1) || PlayState.CheckForItem(11)))
                ChangeActiveWeapon(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3) && (PlayState.CheckForItem(2) || PlayState.CheckForItem(12)))
                ChangeActiveWeapon(2);

            // Music looping
            if (!PlayState.playingMusic)
                return;

            double time = AudioSettings.dspTime;

            if (time + 1 > nextLoopEvent)
            {
                for (int i = 0 + (PlayState.musFlag ? 0 : 1); i < PlayState.musicSourceArray.Count; i += 2)
                {
                    PlayState.musicSourceArray[i].clip = PlayState.areaMusic[PlayState.currentArea][(int)Mathf.Floor(i * 0.5f)];
                    PlayState.musicSourceArray[i].time = PlayState.musicLoopOffsets[PlayState.currentArea][0];
                    PlayState.musicSourceArray[i].PlayScheduled(nextLoopEvent);
                }
                nextLoopEvent += PlayState.musicLoopOffsets[PlayState.currentArea][1];
                PlayState.musFlag = !PlayState.musFlag;
            }
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

        // Game time counter
        if (PlayState.gameState == "Game")
            PlayState.currentTime[2] += Time.deltaTime;
        if (PlayState.currentTime[2] >= 60)
        {
            PlayState.currentTime[2] -= 60;
            PlayState.currentTime[1] += 1;
        }
        if (PlayState.currentTime[1] >= 60)
        {
            PlayState.currentTime[1] -= 60;
            PlayState.currentTime[1] += 1;
        }
    }

    public void UpdateMusic(int area, int subzone, bool resetAudioSources = false)
    {
        if (resetAudioSources)
        {
            while (PlayState.musicParent.childCount > 0)
                Destroy(PlayState.musicParent.GetChild(0));

            for (int i = 0; i < PlayState.areaMusic[area].Length; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    GameObject newSource = new GameObject();
                    newSource.transform.parent = PlayState.musicParent;
                    newSource.name = "Music Source " + (j + 1) + " (Subzone " + i + ")";
                    newSource.AddComponent<AudioSource>();
                    PlayState.musicSourceArray.Add(newSource.GetComponent<AudioSource>());
                    if (j == 0)
                    {
                        newSource.GetComponent<AudioSource>().clip = PlayState.areaMusic[area][i];
                        newSource.GetComponent<AudioSource>().Play();
                    }
                }
            }

            nextLoopEvent = AudioSettings.dspTime + PlayState.musicLoopOffsets[area][1];
        }
        for (int i = 0; i * 2 < PlayState.musicParent.childCount; i++)
        {
            if (i == subzone)
            {
                PlayState.musicSourceArray[i * 2].volume = PlayState.musicVol;
                PlayState.musicSourceArray[i * 2 + 1].volume = PlayState.musicVol;
            }
            else
            {
                PlayState.musicSourceArray[i * 2].volume = 0;
                PlayState.musicSourceArray[i * 2 + 1].volume = 0;
            }
        }
        PlayState.playingMusic = true;
    }

    public void ChangeActiveWeapon(int weaponID, bool activateThisWeapon = false)
    {
        weaponIcons[0].sprite = iconPeaDeselected;
        weaponIcons[1].sprite = iconBoomDeselected;
        weaponIcons[2].sprite = iconWaveDeselected;
        selectedWeapon = weaponID + 1;
        if (activateThisWeapon)
            weaponIcons[weaponID].enabled = true;
        if (weaponID == 2)
            weaponIcons[2].sprite = iconWaveSelected;
        else if (weaponID == 1)
            weaponIcons[1].sprite = iconBoomSelected;
        else
            weaponIcons[0].sprite = iconPeaSelected;
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
        currentSurface = DIR_FLOOR;
        ExitShell();
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

    public void FlashSaveText()
    {
        StartCoroutine(FlashText("save"));
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
                SetTextDisplayed("collection", "Item collection " + PlayState.GetItemPercentage() + "% complete!  Game saved.");
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
            case "save":
                SetTextAlpha("save", 255);
                while (timer < 2.5f)
                {
                    if (timer > 2)
                    {
                        SetTextAlpha("save", Mathf.RoundToInt(Mathf.Lerp(255, 0, (timer - 2) * 1.5f)));
                    }
                    yield return new WaitForFixedUpdate();
                    timer += Time.deltaTime;
                }
                SetTextAlpha("save", 0);
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
            case "save":
                foreach (Transform textObj in gameSaveGroup.transform)
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
            case "save":
                foreach (Transform textObj in gameSaveGroup.transform)
                    textObj.GetComponent<TextMesh>().text = textToDisplay;
                break;
        }
    }

    IEnumerator DieAndRespawn()
    {
        ExitShell();
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
        transform.position = PlayState.respawnCoords;
        inDeathCutscene = false;
        box.enabled = true;
        PlayAnim("idle");
        PlayState.paralyzed = false;
        health = maxHealth;
        UpdateHearts();
        yield return new WaitForEndOfFrame();
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
            if (currentSurface == 1)
                newAnim += "wall ";
            else
                newAnim += "floor ";
        }
        newAnim += state;
        if (newAnim != currentAnim)
        {
            currentAnim = newAnim;
            switch (PlayState.currentCharacter)
            {
                case "Snaily":
                    playerScriptSnaily.anim.Play(newAnim, 0, 0);
                    break;
            }
        }
    }

    public void ExitShell()
    {
        switch (PlayState.currentCharacter)
        {
            case "Snaily":
                if (playerScriptSnaily.shelled)
                    playerScriptSnaily.ToggleShell();
                break;
        }
    }
}
