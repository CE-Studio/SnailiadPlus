using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door:MonoBehaviour, IRoomObject {
    [SerializeField] private int doorWeapon;
    [SerializeField] private int bossLock;
    [SerializeField] private bool locked;
    [SerializeField] private int direction;
    private bool openAfterBossDefeat = false;
    private float bossUnlockDelay = 3.5f;
    private int[] flipStates;

    public AnimationModule anim;
    public SpriteRenderer sprite;
    public BoxCollider2D box;
    public GameObject player;

    public Sprite[] editorSprites;

    public Dictionary<string, object> resave() {
        return null;
    }

    public static readonly string myType = "Door";

    public string objType {
        get {
            return myType;
        }
    }

    public Dictionary<string, object> save() {
        Dictionary<string, object> content = new Dictionary<string, object>();
        content["doorWeapon"] = doorWeapon;
        content["bossLock"] = bossLock;
        content["locked"] = locked;
        content["direction"] = direction;
        return content;
    }

    public void load(Dictionary<string, object> content) {
        doorWeapon = (int)content["doorWeapon"];
        bossLock = (int)content["bossLock"];
        locked = (bool)content["locked"] && PlayState.IsBossAlive(bossLock);
        direction = (int)content["direction"];
        Spawn();
    }

    void Awake() {
        if (PlayState.gameState == PlayState.GameState.game) {
            anim = GetComponent<AnimationModule>();
            sprite = GetComponent<SpriteRenderer>();
            box = GetComponent<BoxCollider2D>();
            player = GameObject.FindWithTag("Player");

            string[] doorDirs = new string[] { "L", "R", "U", "D" };
            string[] doorColors = new string[] { "blue", "purple", "red", "green", "locked" };
            string[] doorStates = new string[] { "open", "holdOpen", "close", "holdClosed" };
            for (int i = 0; i < doorColors.Length; i++) {
                for (int j = 0; j < doorStates.Length; j++) {
                    for (int k = 0; k < doorDirs.Length; k++)
                        anim.Add("Door_" + doorColors[i] + "_" + doorStates[j] + "_" + doorDirs[k]);
                }
            }

            flipStates = PlayState.GetAnim("Door_data").frames;
        }
    }

    public void Spawn()
    {
        if (Vector2.Distance(transform.position, PlayState.player.transform.position) < 2)
            SetState1();
        else
            SetState2();

        if (direction == 1 || direction == 3 && flipStates[1] == 1)
        {
            box.size = new Vector2(3, 1);
            if (direction == 3)
            {
                sprite.flipY = true;
            }
        }
        else if (direction == 2 && flipStates[0] == 1)
        {
            sprite.flipX = true;
        }
    }

    private void Update()
    {
        if (locked && !PlayState.IsBossAlive(bossLock) && !openAfterBossDefeat)
        {
            bossUnlockDelay -= Time.deltaTime;
            if (bossUnlockDelay < 0)
            {
                SetState0();
                openAfterBossDefeat = true;
            }
            if (!box.enabled && !anim.isPlaying)
                anim.Play("holdOpen");
        }
    }

    public void SetClosedSprite() {
        PlayAnim("hold");
    }

    public void PlayAnim(string animType)
    {
        if (animType == "blank")
            sprite.enabled = false;
        else
        {
            sprite.enabled = true;
            string animToPlay = "Door_";
            if (locked)
                animToPlay += "locked_";
            else
            {
                animToPlay += doorWeapon switch
                {
                    0 => "blue_",
                    1 => "purple_",
                    2 => "red_",
                    _ => "green_"
                };
            }
            animToPlay += animType + "_" + direction switch
            {
                0 => "L",
                1 => "U",
                2 => "R",
                _ => "D"
            };
            anim.Play(animToPlay);
        }
    }

    // State 0 is for doors that are opened from being shot
    public void SetState0() {
        PlayAnim("open");
        PlayState.PlaySound("DoorOpen");
        box.enabled = false;
    }

    // State 1 is for doors that are opened for a limited time before closing or despawning as a result of being entered through
    public void SetState1() {
        sprite.enabled = false;
        box.enabled = false;
        StartCoroutine(nameof(WaitForClose));
    }

    // State 2 is for doors that are closed upon spawning
    public void SetState2() {
        sprite.enabled = true;
        PlayAnim("holdClosed");
        box.enabled = true;
    }

    // State 3 is for doors that are closed only after being entered through
    public void SetState3() {
        box.enabled = true;
        sprite.enabled = true;
        PlayAnim("close");
        PlayState.PlaySound("DoorClose");
    }

    public void SetStateDespawn() {
        gameObject.SetActive(false);
    }

    private IEnumerator WaitForClose() {
        while (Vector2.Distance(transform.position, player.transform.position) < 4 && gameObject.activeSelf && !PlayState.inBossFight) {
            yield return new WaitForEndOfFrame();
        }
        if (!gameObject.activeSelf) {
            SetStateDespawn();
        } else {
            SetState3();
        }
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("PlayerBullet")) {
            if (!locked && ((collision.GetComponent<Bullet>().bulletType > doorWeapon && doorWeapon != 3) || (collision.GetComponent<Bullet>().bulletType >= 4 && doorWeapon == 3))) {
                SetState0();
            }
        }
    }
}
