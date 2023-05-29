using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    public bool isActive = false;
    public AnimationModule anim;
    public SpriteRenderer sprite;
    public string type = "";
    public float[] vars = new float[] { 0, 0, 0, 0, 0, 0 };
    private float[] internalVars = new float[] { 0, 0, 0, 0, 0, 0 };
    public ParticleSpriteCollection sprites;

    public void Start()
    {
        anim = GetComponent<AnimationModule>();
        sprite = GetComponent<SpriteRenderer>();
        anim.blankOnNonLoopEnd = true;
        gameObject.SetActive(false);

        anim.Add("Bubble1");
        anim.Add("Bubble2");
        anim.Add("Bubble3");
        anim.Add("Bubble4");
        anim.Add("Dot_heat_tiny");
        anim.Add("Dot_heat_small");
        anim.Add("Dot_heat_medium");
        anim.Add("Dust");
        anim.Add("Explosion_tiny");
        anim.Add("Explosion_small");
        anim.Add("Explosion_big");
        anim.Add("Explosion_huge");
        anim.Add("Explosion_white_small");
        anim.Add("Explosion_white_big");
        anim.Add("Explosion_rainbow_small");
        anim.Add("Explosion_rainbow_big");
        anim.Add("Nom");
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
    }

    public void Update()
    {
        if (PlayState.gameState == PlayState.GameState.game)
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
                    case "heat":
                        vars[0] += Time.deltaTime * vars[4];
                        transform.position = new Vector2(vars[3] + Mathf.Sin(vars[0]) * vars[4], transform.position.y + vars[1] * Time.deltaTime);
                        vars[1] -= vars[2] * Time.deltaTime;
                        while (transform.position.x < PlayState.cam.transform.position.x - 13)
                            transform.position = new Vector2(transform.position.x + 26, transform.position.y);
                        while (transform.position.x > PlayState.cam.transform.position.x + 13)
                            transform.position = new Vector2(transform.position.x - 26, transform.position.y);
                        break;
                    case "nom":
                        internalVars[0] += Time.deltaTime;
                        transform.position = new Vector2(transform.position.x, Mathf.Lerp(vars[0], vars[0] + 1.25f, internalVars[0] * 1.2f));
                        break;
                    case "snow":
                        transform.position = new Vector2(transform.position.x + (Mathf.Sin(vars[1] * 4) - 1) * 2.5f * Time.deltaTime,
                            transform.position.y - vars[0] * Time.deltaTime);
                        vars[1] += Time.deltaTime;
                        if (vars[1] > Mathf.PI * 2)
                            vars[1] -= Mathf.PI * 2;
                        while (transform.position.x < PlayState.cam.transform.position.x - 13)
                            transform.position = new Vector2(transform.position.x + 26, transform.position.y);
                        while (transform.position.x > PlayState.cam.transform.position.x + 13)
                            transform.position = new Vector2(transform.position.x - 26, transform.position.y);
                        while (transform.position.y < PlayState.cam.transform.position.y - 8)
                            transform.position = new Vector2(transform.position.x, transform.position.y + 16);
                        while (transform.position.y > PlayState.cam.transform.position.y + 8)
                            transform.position = new Vector2(transform.position.x, transform.position.y - 16);
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
                        transform.position += new Vector3(internalVars[0], internalVars[1]);
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
                                transform.position = new Vector2(transform.position.x + 26, transform.position.y);
                            while (transform.position.x > PlayState.cam.transform.position.x + 13)
                                transform.position = new Vector2(transform.position.x - 26, transform.position.y);
                            while (transform.position.y < PlayState.cam.transform.position.y - 8)
                                transform.position = new Vector2(transform.position.x, transform.position.y + 16);
                            while (transform.position.y > PlayState.cam.transform.position.y + 8)
                                transform.position = new Vector2(transform.position.x, transform.position.y - 16);
                        }
                        MoveToCamSynced();
                        break;
                }

                if (!anim.isPlaying && isActive && !(type == "bubble"))
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
            case "heat":
                anim.Play(Random.Range(0, 3) switch { 1 => "Dot_heat_small", 2 => "Dot_heat_medium", _ => "Dot_heat_tiny" });
                break;
            case "nom":
                anim.Play("Nom");
                break;
            case "smoke":
                anim.Play("Smoke");
                break;
            case "snow":
                anim.Play("Snow" + Random.Range(1, 5).ToString());
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
            _ => -15
        };
        sprite.color = animType switch
        {
            "heat" => PlayState.GetColor(Random.Range(0, 7) switch { 0 => "0209", 1 => "0210", 2 => "0211", 3=> "0309", 4 => "0310", 5 => "0311", _ => "0312"}),
            _ => Color.white
        };
        anim.SetSpeed(animType switch {
            "heat" => Random.Range(0.5f, 1.5f),
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
        anim.Stop(true);
        sprite.sprite = sprites.blank;
        sprite.flipX = false;
        sprite.flipY = false;
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
