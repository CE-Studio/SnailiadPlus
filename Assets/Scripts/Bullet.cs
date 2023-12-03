using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int bulletType;
    public int direction;
    public bool isActive;
    private float lifeTimer;
    private float velocity;
    private float initialVelocity;
    public int damage;
    public float rapidMult;
    private bool despawnOffScreen = true;

    public SpriteRenderer sprite;
    public AnimationModule anim;
    public BoxCollider2D box;
    public GameObject lightMask;

    public GameObject player;
    public GameObject cam;

    private readonly Vector2 diagonalAim = PlayState.ANGLE_DIAG;
    
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        sprite.enabled = false;
        anim = GetComponent<AnimationModule>();
        box = GetComponent<BoxCollider2D>();
        box.enabled = false;

        player = GameObject.FindWithTag("Player");
        cam = GameObject.Find("View");

        string[] bulletTypes = new string[] { "peashooter", "boomerang", "rainbowWave", "peashooterDev", "boomerangDev", "rainbowWaveDev" };
        for (int i = 0; i < bulletTypes.Length; i++)
        {
            for (int j = 0; j < PlayState.DIRS_COMPASS.Length; j++)
                anim.Add("Bullet_" + bulletTypes[i] + "_" + PlayState.DIRS_COMPASS[j]);
        }
        string[] shockChars = new string[] { "snaily", "sluggy", "upside", "leggy", "blobby", "leechy" };
        string[] shockDirs = new string[] { "N", "E", "S", "W" };
        for (int i = 0; i < shockChars.Length; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < shockDirs.Length; k++)
                    anim.Add(string.Format("Bullet_gravShock_{0}{1}_{2}", shockChars[i], j.ToString(), shockDirs[k]));
            }
        }
        string[] shockWaveDirs = new string[] { "floor_L", "floor_R", "wallL_D", "wallL_U", "wallR_D", "wallR_U", "ceiling_L", "ceiling_R" };
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < shockWaveDirs.Length; j++)
                anim.Add(string.Format("Bullet_shockWave{0}_{1}", i == 0 ? "" : "Dev", shockWaveDirs[j]));
        }
    }

    void FixedUpdate()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;
        if (isActive)
        {
            lifeTimer += Time.fixedDeltaTime;
            switch (bulletType)
            {
                case 1:
                case 4:
                    break;
                case 2:
                case 5:
                    velocity -= initialVelocity * 1.5f * Time.fixedDeltaTime * rapidMult;
                    break;
                case 3:
                case 6:
                    velocity += initialVelocity * 18f * Time.fixedDeltaTime;
                    break;
                case 7:
                case 8:
                    break;
                case 9:
                case 10:
                    velocity += initialVelocity * 15f * Time.fixedDeltaTime;
                    break;
                default:
                    velocity += 0.03f;
                    break;
            }
            switch (direction)
            {
                case 0:
                    MoveNW();
                    break;
                case 1:
                    MoveN();
                    break;
                case 2:
                    MoveNE();
                    break;
                case 3:
                    MoveW();
                    break;
                case 4:
                    MoveE();
                    break;
                case 5:
                    MoveSW();
                    break;
                case 6:
                    MoveS();
                    break;
                case 7:
                    MoveSE();
                    break;
            }
        }
        if ((lifeTimer > 3 || despawnOffScreen) && !PlayState.OnScreen(transform.position, box) && bulletType < 7)
            Despawn();
        if ((bulletType == 1 || bulletType == 9 || bulletType == 10) && PlayState.IsTileSolid(transform.position))
            Despawn(PlayState.OnScreen(transform.position, box));
    }

    public void Shoot(int type, int dir, bool applyRapidMult, float posOverrideX = Mathf.Infinity, float posOverrideY = Mathf.Infinity)
    {
        isActive = true;
        sprite.enabled = true;
        box.enabled = true;
        if (!(posOverrideX == Mathf.Infinity && posOverrideY == Mathf.Infinity))
            transform.position = new Vector2(posOverrideX, posOverrideY);
        else
        {
            transform.position = new Vector2(
                player.transform.position.x + PlayState.playerScript.box.offset.x,
                player.transform.position.y + PlayState.playerScript.box.offset.y) + PlayState.playerScript.gravityDir switch
                {
                    Player.Dirs.WallL => new Vector2(-0.0625f, 0),
                    Player.Dirs.WallR => new Vector2(0.0625f, 0),
                    Player.Dirs.Ceiling => new Vector2(0, 0.0625f),
                    _ => new Vector2(0, -0.0625f)
                };
            switch (dir)
            {
                case 0:
                    transform.position = new Vector2(transform.position.x - 0.4f, transform.position.y + 0.2f);
                    break;
                case 1:
                    transform.position = new Vector2(transform.position.x, transform.position.y + 0.2f);
                    break;
                case 2:
                    transform.position = new Vector2(transform.position.x + 0.4f, transform.position.y + 0.2f);
                    break;
                case 3:
                    transform.position = new Vector2(transform.position.x - 0.4f, transform.position.y);
                    break;
                case 4:
                    transform.position = new Vector2(transform.position.x + 0.4f, transform.position.y);
                    break;
                case 5:
                    transform.position = new Vector2(transform.position.x - 0.4f, transform.position.y - 0.2f);
                    break;
                case 6:
                    transform.position = new Vector2(transform.position.x, transform.position.y - 0.2f);
                    break;
                case 7:
                    transform.position = new Vector2(transform.position.x + 0.4f, transform.position.y - 0.2f);
                    break;
            }
        }
        bulletType = type;
        int lightSize = -1;
        switch (type)
        {
            case 1: // Peashooter
                box.size = new Vector2(0.25f, 0.25f);
                velocity = 0.4625f;
                damage = 10;
                rapidMult = 2f;
                break;
            case 2: // Boomerang
                box.size = new Vector2(0.9f, 0.9f);
                velocity = 0.4125f;
                damage = 20;
                rapidMult = 2f;
                despawnOffScreen = false;
                break;
            default:
            case 3: // Rainbow Wave
                box.size = new Vector2(1.9f, 1.9f);
                velocity = 0.075f;
                damage = 30;
                rapidMult = 2f;
                lightSize = 13;
                break;
            case 4: // Devastator Peashooter
                box.size = new Vector2(1.4f, 1.4f);
                velocity = 0.4625f;
                damage = 45;
                rapidMult = 2f;
                break;
            case 5: // Devastator Boomerang
                box.size = new Vector2(1.4f, 1.4f);
                velocity = 0.4125f;
                damage = 50;
                rapidMult = 2f;
                despawnOffScreen = false;
                break;
            case 6: // Devastator Rainbow Wave
                box.size = new Vector2(2.4f, 2.4f);
                velocity = 0.075f;
                damage = 68;
                rapidMult = 2f;
                lightSize = 17;
                break;
            case 7: // Gravity Shock
                box.size = new Vector2(2.75f, 2.75f);
                velocity = 0;
                damage = 300;
                rapidMult = 1f;
                despawnOffScreen = false;
                break;
            case 8: // Full-Metal Gravity Shock
                box.size = new Vector2(3f, 3f);
                velocity = 0;
                damage = 650;
                rapidMult = 1f;
                despawnOffScreen = false;
                break;
            case 9: // Shockwave
                box.size = new Vector2(0.45f, 0.45f);
                velocity = 0.05f;
                damage = 68;
                rapidMult = 1f;
                lightSize = 9;
                break;
            case 10: // Devastator Shockwave
                box.size = new Vector2(0.95f, 0.95f);
                velocity = 0.085f;
                damage = 108;
                rapidMult = 1f;
                lightSize = 11;
                break;
        }
        direction = dir;
        initialVelocity = velocity;
        if (!((PlayState.CheckForItem("Rapid Fire") || (PlayState.CheckForItem("Devastator") && PlayState.stackWeaponMods)) && applyRapidMult))
            rapidMult = 1f;
        if (PlayState.damageMult)
            damage *= 10;
        velocity *= rapidMult;
        PlayAnim();
        if (lightSize != -1 && lightMask == null)
        {
            lightMask = PlayState.globalFunctions.CreateLightMask(lightSize, transform.position);
            lightMask.transform.parent = transform;
        }
    }

    void PlayAnim()
    {
        string animToPlay = "Bullet_";
        animToPlay += bulletType switch
        {
            1 => "peashooter_",
            2 => "boomerang_",
            3 => "rainbowWave_",
            4 => "peashooterDev_",
            5 => "boomerangDev_",
            6 => "rainbowWaveDev_",
            7 => "gravShock_" + PlayState.currentProfile.character.ToLower() + "0_",
            8 => "gravShock_" + PlayState.currentProfile.character.ToLower() + "1_",
            9 => ShockWaveAnimSubroutine(false),
            10 => ShockWaveAnimSubroutine(true),
            _ => "rainbowWave_",
        };
        if (bulletType != 9 && bulletType != 10)
        {
            animToPlay += direction switch
            {
                1 => "N",
                2 => "NE",
                3 => "W",
                4 => "E",
                5 => "SW",
                6 => "S",
                7 => "SE",
                _ => "NW"
            };
        }
        anim.Play(animToPlay);
    }

    private string ShockWaveAnimSubroutine(bool devVariant)
    {
        string output = string.Format("shockWave{0}_", devVariant ? "Dev" : "");
        string var1;
        string var2;
        switch (direction)
        {
            default:
            case 5: // SW
                var1 = "floor_L";
                var2 = "wallL_D";
                break;
            case 7: // SE
                var1 = "floor_R";
                var2 = "wallR_D";
                break;
            case 8: // NW
                var1 = "ceiling_L";
                var2 = "wallL_U";
                break;
            case 2: // NE
                var1 = "ceiling_R";
                var2 = "wallR_U";
                break;
        }
        if (PlayState.playerScript.gravityDir == Player.Dirs.Floor || PlayState.playerScript.gravityDir == Player.Dirs.Ceiling)
        {
            output += var1;
            direction = direction switch
            {
                5 => 3,
                8 => 3,
                _ => 4
            };
        }
        else
        {
            output += var2;
            direction = direction switch
            {
                5 => 6,
                7 => 6,
                _ => 1
            };
        }
        return output;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerCollide") && bulletType == 1)
        {
            Despawn(true);
        }
    }

    public void Despawn(bool loudly = false)
    {
        if (isActive)
        {
            if (loudly)
            {
                PlayState.PlaySound("ShotHit");
                switch (bulletType)
                {
                    case 1:
                        CallExplosion(0.25f, 1, 1);
                        break;
                    case 2:
                        CallExplosion(0.5f, 5, 1);
                        break;
                    case 3:
                        CallExplosion(0.5f, 7, 1);
                        break;
                    case 4:
                        CallExplosion(0.5f, 5, 4);
                        break;
                    case 5:
                        CallExplosion(0.5f, 6, 1);
                        break;
                    case 6:
                        CallExplosion(0.5f, 8, 1);
                        break;
                    case 7:
                        CallExplosion(1.25f, 2, 3);
                        break;
                    case 8:
                        CallExplosion(1.25f, 2, 2);
                        CallExplosion(1.25f, 3, 2);
                        break;
                    case 9:
                    case 10:
                        CallExplosion(0.5f, 2, 1);
                        break;
                    default:
                        break;
                }
            }
            isActive = false;
            sprite.enabled = false;
            box.enabled = false;
            despawnOffScreen = true;
            lifeTimer = 0;
            transform.position = Vector2.zero;
            if (lightMask != null)
                Destroy(lightMask);
        }
    }

    private void MoveNW()
    {
        transform.position = new Vector2(transform.position.x + (-diagonalAim.x * velocity), transform.position.y + (diagonalAim.y * velocity));
    }

    private void MoveN()
    {
        transform.position = new Vector2(transform.position.x, transform.position.y + (1 * velocity));
    }

    private void MoveNE()
    {
        transform.position = new Vector2(transform.position.x + (diagonalAim.x * velocity), transform.position.y + (diagonalAim.y * velocity));
    }

    private void MoveW()
    {
        transform.position = new Vector2(transform.position.x + (-1 * velocity), transform.position.y);
    }

    private void MoveE()
    {
        transform.position = new Vector2(transform.position.x + (1 * velocity), transform.position.y);
    }

    private void MoveSW()
    {
        transform.position = new Vector2(transform.position.x + (-diagonalAim.x * velocity), transform.position.y + (-diagonalAim.y * velocity));
    }

    private void MoveS()
    {
        transform.position = new Vector2(transform.position.x, transform.position.y + (-1 * velocity));
    }

    private void MoveSE()
    {
        transform.position = new Vector2(transform.position.x + (diagonalAim.x * velocity), transform.position.y + (-diagonalAim.y * velocity));
    }

    public Vector2 Vector2Direction()
    {
        return direction switch
        {
            0 => new(-PlayState.ANGLE_DIAG.x, PlayState.ANGLE_DIAG.y),
            1 => Vector2.up,
            2 => PlayState.ANGLE_DIAG,
            3 => Vector2.left,
            4 => Vector2.right,
            5 => -PlayState.ANGLE_DIAG,
            6 => Vector2.down,
            7 => new(PlayState.ANGLE_DIAG.x, -PlayState.ANGLE_DIAG.y),
            _ => Vector2.right
        };
    }

    private void CallExplosion(float buffer, int type, int count)
    {
        for (int i = 0; i < count; i++)
            PlayState.RequestParticle(new Vector2(Random.Range(transform.position.x - buffer, transform.position.x + buffer),
                Random.Range(transform.position.y - buffer, transform.position.y + buffer)), "explosion", new float[] { type });
    }
}
