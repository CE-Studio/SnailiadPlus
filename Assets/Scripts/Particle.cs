using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    public bool isActive = false;
    public AnimationModule anim;
    public SpriteRenderer sprite;
    public string type = "";
    public float[] vars = new float[] { 0, 0, 0, 0, 0, 0, 0, 0 };
    private float[] internalVars = new float[] { 0, 0, 0, 0, 0, 0, 0, 0 };
    private bool runInMenu = false;
    public ParticleSpriteCollection sprites;

    public void Awake()
    {
        anim = GetComponent<AnimationModule>();
        sprite = GetComponent<SpriteRenderer>();
        anim.blankOnNonLoopEnd = true;
        gameObject.SetActive(false);
    }

    public void Start()
    {
        if (anim.animList.Count == 0)
            AddAnims();
    }

    public void AddAnims()
    {
        anim.Add("Bubble1");
        anim.Add("Bubble2");
        anim.Add("Bubble3");
        anim.Add("Bubble4");
        anim.Add("Dot_heat_tiny");
        anim.Add("Dot_heat_small");
        anim.Add("Dot_heat_medium");
        anim.Add("Dot_sparkle_short");
        anim.Add("Dot_sparkle_medium");
        anim.Add("Dot_sparkle_long");
        anim.Add("Dust");
        anim.Add("Explosion_tiny");
        anim.Add("Explosion_small");
        anim.Add("Explosion_big");
        anim.Add("Explosion_huge");
        anim.Add("Explosion_white_small");
        anim.Add("Explosion_white_big");
        anim.Add("Explosion_rainbow_small");
        anim.Add("Explosion_rainbow_big");
        anim.Add("GravShock_char_base1");
        anim.Add("GravShock_char_base2");
        anim.Add("GravShock_char_base3");
        anim.Add("GravShock_charge");
        anim.Add("GravShock_launch_up");
        anim.Add("GravShock_launch_left");
        anim.Add("GravShock_launch_down");
        anim.Add("GravShock_launch_right");
        anim.Add("IntroPattern_1");
        anim.Add("IntroPattern_2");
        anim.Add("IntroPattern_3");
        anim.Add("IntroPattern_4");
        anim.Add("IntroPattern_5");
        anim.Add("IntroPattern_6");
        anim.Add("IntroPattern_7");
        anim.Add("IntroPattern_8");
        anim.Add("Nom");
        anim.Add("Shield");
        anim.Add("Smoke");
        anim.Add("Snow1");
        anim.Add("Snow2");
        anim.Add("Snow3");
        anim.Add("Snow4");
        anim.Add("Splash");
        anim.Add("Star1");
        anim.Add("Star2");
        anim.Add("Star3");
        anim.Add("Star4");
        anim.Add("Transformation_ice");
        anim.Add("Transformation_gravity");
        anim.Add("Transformation_fullMetal");
        anim.Add("Transformation_magnet");
        anim.Add("Transformation_corkscrew");
        anim.Add("Transformation_angel");
        anim.Add("Zzz");

        foreach (string character in new string[] { "snaily", "sluggy" })
        {
            for (int i = 0; i <= 1; i++)
                foreach (string dir in new string[] { "down", "up", "left", "right" })
                    anim.Add(string.Format("GravShock_char_{0}{1}_{2}", character, i.ToString(), dir));
        }
    }

    public void Update()
    {
        if (PlayState.gameState == PlayState.GameState.game || PlayState.gameState == PlayState.GameState.menu ||
            PlayState.gameState == PlayState.GameState.credits || runInMenu)
        {
            if (!anim.isPlaying)
                anim.Resume();
            if (gameObject.activeSelf)
            {
                switch (type)
                {
                    default:
                        break;
                    case "bubble":
                        if (vars[4] == 1)
                        {
                            if (internalVars[0] == 0 && internalVars[1] == 0)
                            {
                                internalVars[0] = Random.Range(-4f, 4f);
                                internalVars[1] = Random.Range(-12f, -3f) * Mathf.Abs(PlayState.playerScript.velocity.y);
                            }
                            transform.position = new Vector2(transform.position.x, transform.position.y + (internalVars[1] * Time.deltaTime));
                            vars[1] += internalVars[0] * Time.deltaTime;
                            internalVars[0] = Mathf.Lerp(internalVars[0], 0, 2f * Time.deltaTime);
                            internalVars[1] = Mathf.Lerp(internalVars[1], 0, 2f * Time.deltaTime);
                        }
                        vars[0] += Time.deltaTime;
                        transform.position = new Vector2(vars[1] + 2 * Mathf.Sin(vars[0] / 1.2f) * 0.0625f, transform.position.y + vars[3] * Time.deltaTime * 0.25f);
                        if (transform.position.y > vars[2] - 0.25f)
                            ResetParticle();
                        break;
                    case "gigastar":
                        for (int i = 2; i < 7; i++)
                        {
                            if (i == vars[0] + 2)
                                internalVars[i] = Mathf.Clamp(internalVars[i] + Time.deltaTime, 0f, 1f);
                            else
                                internalVars[i] = Mathf.Clamp(internalVars[i] - Time.deltaTime, 0f, 1f);
                        }

                        float linearMod = 2.5f;
                        float gigaCenterDis = Vector2.Distance(transform.position, PlayState.cam.transform.position);
                        internalVars[0] = 0;
                        internalVars[1] = 0;
                        // Intro - to center
                        Vector2 gigaToCenter = (Vector2)(PlayState.cam.transform.position - transform.position).normalized * vars[2];
                        internalVars[0] += gigaToCenter.x * internalVars[2];
                        internalVars[1] += gigaToCenter.y * internalVars[2];
                        // Stomp - N
                        internalVars[1] += Mathf.Abs(vars[1]) * internalVars[3] * linearMod;
                        // Strafe - to border
                        Vector2 gigaFromCenter = (Vector2)(PlayState.cam.transform.position - transform.position).normalized * -vars[2];
                        internalVars[0] += gigaFromCenter.x * internalVars[4];
                        internalVars[1] += gigaFromCenter.y * internalVars[4];
                        // Smash - SE
                        internalVars[0] += Mathf.Abs(PlayState.ANGLE_DIAG.x * vars[1]) * internalVars[5] * linearMod;
                        internalVars[1] += -Mathf.Abs(PlayState.ANGLE_DIAG.y * vars[1]) * internalVars[5] * linearMod;
                        // Sleep - don't move at all
                        transform.position += new Vector3(internalVars[0], internalVars[1]);
                        if (internalVars[2] > 0.25f)
                        {
                            if (gigaCenterDis < Vector2.Distance(transform.position, PlayState.cam.transform.position))
                            {
                                if (Mathf.Round(Random.Range(0f, 1f)) == 1)
                                {
                                    transform.position = (Vector2)PlayState.cam.transform.position +
                                        new Vector2(Mathf.Round(Random.Range(0f, 1f)) == 1 ? 13 : -13, Random.Range(-8, 8));
                                }
                                else
                                {
                                    transform.position = (Vector2)PlayState.cam.transform.position +
                                        new Vector2(Random.Range(-13, 13), Mathf.Round(Random.Range(0f, 1f)) == 1 ? 8 : -8);
                                }
                                internalVars[0] = 0;
                                internalVars[1] = 0;
                            }
                        }
                        else if (internalVars[4] > 0.25f)
                        {
                            if (transform.position.x < PlayState.cam.transform.position.x - 13 ||
                                transform.position.x > PlayState.cam.transform.position.x + 13 ||
                                transform.position.y < PlayState.cam.transform.position.y - 8 ||
                                transform.position.y > PlayState.cam.transform.position.y + 8)
                            {
                                transform.position = (Vector2)PlayState.cam.transform.position +
                                    new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
                                internalVars[0] = 0;
                                internalVars[1] = 0;
                            }
                        }
                        else
                        {
                            while (transform.position.x < PlayState.cam.transform.position.x - 13)
                                transform.position = new(transform.position.x + 26, transform.position.y);
                            while (transform.position.x > PlayState.cam.transform.position.x + 13)
                                transform.position = new(transform.position.x - 26, transform.position.y);
                            while (transform.position.y < PlayState.cam.transform.position.y - 8)
                                transform.position = new(transform.position.x, transform.position.y + 16);
                            while (transform.position.y > PlayState.cam.transform.position.y + 8)
                                transform.position = new(transform.position.x, transform.position.y - 16);
                        }
                        break;
                    case "gigatrail":
                        sprite.color = new Color(1, 1, 1, sprite.color.a - Time.deltaTime * 2f);
                        if (sprite.color.a <= 0)
                            ResetParticle();
                        break;
                    case "heat":
                        vars[0] += Time.deltaTime * vars[4];
                        transform.position = new(vars[3] + Mathf.Sin(vars[0]) * vars[4], transform.position.y + vars[1] * Time.deltaTime);
                        vars[1] -= vars[2] * Time.deltaTime;
                        while (transform.position.x < PlayState.cam.transform.position.x - 13)
                            transform.position = new(transform.position.x + 26, transform.position.y);
                        while (transform.position.x > PlayState.cam.transform.position.x + 13)
                            transform.position = new(transform.position.x - 26, transform.position.y);
                        break;
                    case "intropattern":
                        transform.position += new Vector3(-0.0425f, 0.0425f);
                        if (transform.position.x < PlayState.cam.transform.position.x - 14)
                            transform.position += 30 * Vector3.right;
                        if (transform.position.y > PlayState.cam.transform.position.y + 9)
                            transform.position += 18 * Vector3.down;
                        break;
                    case "nom":
                        internalVars[0] += Time.deltaTime;
                        transform.position = new(transform.position.x, Mathf.Lerp(vars[0], vars[0] + 1.25f, internalVars[0] * 1.2f));
                        break;
                    case "shockcharmain":
                        PlayState.RequestParticle(transform.position, "shockcharsub", new float[] { vars[0], vars[1], vars[2], anim.GetCurrentFrameValue() });
                        break;
                    case "snow":
                        transform.position = new(transform.position.x + (Mathf.Sin(vars[1] * 4) - 1) * 2.5f * Time.deltaTime,
                            transform.position.y - vars[0] * Time.deltaTime);
                        vars[1] += Time.deltaTime;
                        if (vars[1] > Mathf.PI * 2)
                            vars[1] -= Mathf.PI * 2;
                        while (transform.position.x < PlayState.cam.transform.position.x - 13)
                            transform.position = new(transform.position.x + 26, transform.position.y);
                        while (transform.position.x > PlayState.cam.transform.position.x + 13)
                            transform.position = new(transform.position.x - 26, transform.position.y);
                        while (transform.position.y < PlayState.cam.transform.position.y - 8)
                            transform.position = new(transform.position.x, transform.position.y + 16);
                        while (transform.position.y > PlayState.cam.transform.position.y + 8)
                            transform.position = new(transform.position.x, transform.position.y - 16);
                        break;
                    case "sparkle":
                        transform.position += new Vector3(vars[0] * Time.deltaTime, vars[1] * Time.deltaTime, 0);
                        break;
                    case "star":
                        float centerDis = Vector2.Distance(transform.position, PlayState.cam.transform.position);
                        if (internalVars[0] == 0 && internalVars[1] == 0)
                        {
                            switch (vars[0])
                            {
                                case 0:
                                    internalVars[0] = 0;
                                    internalVars[1] = Mathf.Abs(vars[1]);
                                    break;
                                case 1:
                                    internalVars[0] = Mathf.Abs(PlayState.ANGLE_DIAG.x * vars[1]);
                                    internalVars[1] = Mathf.Abs(PlayState.ANGLE_DIAG.y * vars[1]);
                                    break;
                                case 2:
                                    internalVars[0] = Mathf.Abs(vars[1]);
                                    internalVars[1] = 0;
                                    break;
                                case 3:
                                    internalVars[0] = Mathf.Abs(PlayState.ANGLE_DIAG.x * vars[1]);
                                    internalVars[1] = -Mathf.Abs(PlayState.ANGLE_DIAG.y * vars[1]);
                                    break;
                                case 4:
                                    internalVars[0] = 0;
                                    internalVars[1] = -Mathf.Abs(vars[1]);
                                    break;
                                case 5:
                                    internalVars[0] = -Mathf.Abs(PlayState.ANGLE_DIAG.x * vars[1]);
                                    internalVars[1] = -Mathf.Abs(PlayState.ANGLE_DIAG.y * vars[1]);
                                    break;
                                default:
                                case 6:
                                    internalVars[0] = -Mathf.Abs(vars[1]);
                                    internalVars[1] = 0;
                                    break;
                                case 7:
                                    internalVars[0] = -Mathf.Abs(PlayState.ANGLE_DIAG.x * vars[1]);
                                    internalVars[1] = Mathf.Abs(PlayState.ANGLE_DIAG.y * vars[1]);
                                    break;
                                case 8:
                                    Vector2 toCenter = (Vector2)(PlayState.cam.transform.position - transform.position).normalized * vars[2];
                                    internalVars[0] = toCenter.x;
                                    internalVars[1] = toCenter.y;
                                    break;
                                case 9:
                                    Vector2 fromCenter = (Vector2)(PlayState.cam.transform.position - transform.position).normalized * -vars[2];
                                    internalVars[0] = fromCenter.x;
                                    internalVars[1] = fromCenter.y;
                                    break;
                            }
                        }
                        transform.position += Time.deltaTime * new Vector3(internalVars[0], internalVars[1]);
                        if (vars[0] == 8)
                        {
                            if (centerDis < Vector2.Distance(transform.position, PlayState.cam.transform.position))
                            {
                                if (Mathf.Round(Random.Range(0f, 1f)) == 1)
                                {
                                    transform.position = (Vector2)PlayState.cam.transform.position +
                                        new Vector2(Mathf.Round(Random.Range(0f, 1f)) == 1 ? 13 : -13, Random.Range(-8, 8));
                                }
                                else
                                {
                                    transform.position = (Vector2)PlayState.cam.transform.position +
                                        new Vector2(Random.Range(-13, 13), Mathf.Round(Random.Range(0f, 1f)) == 1 ? 8 : -8);
                                }
                                internalVars[0] = 0;
                                internalVars[1] = 0;
                            }
                        }
                        else if (vars[0] == 9)
                        {
                            if (transform.position.x < PlayState.cam.transform.position.x - 13 ||
                                transform.position.x > PlayState.cam.transform.position.x + 13 ||
                                transform.position.y < PlayState.cam.transform.position.y - 8 ||
                                transform.position.y > PlayState.cam.transform.position.y + 8)
                            {
                                transform.position = (Vector2)PlayState.cam.transform.position +
                                    new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
                                internalVars[0] = 0;
                                internalVars[1] = 0;
                            }
                        }
                        else
                        {
                            while (transform.position.x < PlayState.cam.transform.position.x - 13)
                                transform.position = new(transform.position.x + 26, transform.position.y);
                            while (transform.position.x > PlayState.cam.transform.position.x + 13)
                                transform.position = new(transform.position.x - 26, transform.position.y);
                            while (transform.position.y < PlayState.cam.transform.position.y - 8)
                                transform.position = new(transform.position.x, transform.position.y + 16);
                            while (transform.position.y > PlayState.cam.transform.position.y + 8)
                                transform.position = new(transform.position.x, transform.position.y - 16);
                        }
                        MoveToCamSynced();
                        break;
                }

                if (!anim.isPlaying && isActive && !(type == "bubble" || type == "gigatrail"))
                    ResetParticle();
            }
        }
        else
        {
            if (anim.isPlaying)
                anim.Pause();
        }
    }

    public void SetAnim(string animType)
    {
        if (anim.animList.Count == 0)
            AddAnims();
        isActive = true;
        switch (animType)
        {
            default:
                break;
            case "bubble":
                anim.Play("Bubble" + Random.Range(1, 5).ToString());
                break;
            case "dust":
                anim.Play("Dust");
                break;
            case "explosion":
                anim.Play("Explosion_" + vars[0] switch
                {
                    1 => "tiny",
                    2 => "small",
                    3 => "big",
                    4 => "huge",
                    5 => "white_small",
                    6 => "white_big",
                    7 => "rainbow_small",
                    8 => "rainbow_big",
                    _ => "small"
                });
                break;
            case "gigastar":
                anim.Play("Star" + Random.Range(1, 5).ToString());
                break;
            case "gigatrail":
                sprite.sprite = PlayState.GetSprite("Particles/GigaTrail", (int)vars[0]);
                break;
            case "heat":
                anim.Play(Random.Range(0, 3) switch { 1 => "Dot_heat_small", 2 => "Dot_heat_medium", _ => "Dot_heat_tiny" });
                break;
            case "intropattern":
                anim.Play("IntroPattern_" + (vars[0] == 1 ? Random.Range(5, 9) : Random.Range(1, 5)));
                break;
            case "nom":
                anim.Play("Nom");
                break;
            case "shield":
                anim.Play("Shield");
                break;
            case "shockcharge":
                anim.Play("GravShock_charge");
                break;
            case "shockcharmain":
                sprite.enabled = false;
                anim.Play(string.Format(
                    "GravShock_char_{0}{1}_{2}",
                    vars[0] switch { 0 => "snaily", 1 => "sluggy", 2 => "upside", 3 => "leggy", 4 => "blobby", 5 => "leechy", _ => "snaily" },
                    vars[1], vars[2] switch { 0 => "down", 1 => "left", 2 => "right", 3 => "up", _ => "down" }));
                break;
            case "shockcharsub":
                anim.Play("GravShock_char_base" + Random.Range(1, 4).ToString(), 1f, (int)vars[3]);
                break;
            case "shocklaunch":
                anim.Play("GravShock_launch_" + (vars[0] switch { 0 => "down", 1 => "left", 2 => "right", _ => "up" }));
                break;
            case "smoke":
                anim.Play("Smoke");
                break;
            case "snow":
                anim.Play("Snow" + Random.Range(1, 5).ToString());
                break;
            case "sparkle":
                anim.Play(Random.Range(0, 3) switch { 1 => "Dot_sparkle_medium", 2 => "Dot_sparkle_long", _ => "Dot_sparkle_short" });
                anim.pauseOnMenu = false;
                runInMenu = true;
                break;
            case "splash":
                anim.Play("Splash");
                break;
            case "star":
                anim.Play("Star" + Random.Range(1, 5).ToString());
                break;
            case "transformation":
                anim.Play("Transformation_" + vars[0] switch
                {
                    1 => "ice",
                    2 => "gravity",
                    3 => "fullMetal",
                    4 => "magnet",
                    5 => "corkscrew",
                    6 => "angel",
                    _ => "ice"
                });
                break;
            case "zzz":
                anim.Play("Zzz");
                break;
        }
        sprite.sortingOrder = animType switch
        {
            "transformation" => -51,
            "heat" => Random.Range(0, 4) switch { 0 => -124, 1 => -110, 2 => -24, _ => -1 },
            "star" => -115,
            "gigastar" => -96,
            "gigatrail" => -51,
            "intropattern" => 1002,
            "shocklaunch" => -10,
            "shockcharsub" => -14,
            "sparkle" => 10,
            _ => -15
        };
        sprite.color = animType switch
        {
            "heat" => PlayState.GetColor(Random.Range(0, 7) switch { 0 => "0209", 1 => "0210", 2 => "0211", 3=> "0309", 4 => "0310", 5 => "0311", _ => "0312"}),
            "sparkle" => PlayState.GetColor(Random.Range(0, 4) switch { 0 => "0304", 1 => "0206", 2 => "0309", _ => "0312" }),
            _ => Color.white
        };
        anim.SetSpeed(animType switch {
            "heat" => Random.Range(0.5f, 1.5f),
            "sparkle" => Random.Range(0.5f, 1.5f),
            _ => 1f
        });
    }

    public void PlaySound()
    {
        switch (type)
        {
            default:
                break;
            case "explosion":
                PlayState.PlaySound("Explode" + Random.Range(1, 5));
                break;
            case "splash":
                PlayState.PlaySound("Splash");
                break;
        }
    }

    public void ResetParticle()
    {
        isActive = false;
        transform.position = Vector2.zero;
        type = "";
        anim.Stop(true);
        anim.pauseOnMenu = true;
        runInMenu = true;
        sprite.enabled = true;
        sprite.sprite = sprites.blank;
        sprite.flipX = false;
        sprite.flipY = false;
        sprite.color = new Color(1, 1, 1, 1);
        for (int i = 0; i < vars.Length; i++)
            vars[i] = 0;
        for (int i = 0; i < internalVars.Length; i++)
            internalVars[i] = 0;
        MoveToMainPool();
        gameObject.SetActive(false);
    }

    public void MoveToCamSynced()
    {
        if (transform.parent != PlayState.camParticlePool.transform)
            transform.parent = PlayState.camParticlePool.transform;
    }

    public void MoveToMainPool()
    {
        if (transform.parent != PlayState.particlePool.transform)
            transform.parent = PlayState.particlePool.transform;
    }
}
