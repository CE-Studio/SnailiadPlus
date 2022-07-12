using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    public bool isActive = false;
    public AnimationModule anim;
    public SpriteRenderer sprite;
    public string type = "";
    public float[] vars = new float[] { 0, 0, 0, 0, 0 };
    private float[] internalVars = new float[] { 0, 0, 0, 0, 0 };
    public ParticleSpriteCollection sprites;

    public void Start()
    {
        anim = GetComponent<AnimationModule>();
        sprite = GetComponent<SpriteRenderer>();
        anim.blankOnNonLoopEnd = true;
        gameObject.SetActive(false);

        anim.Add("Bubble");
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
        anim.Add("Splash");
    }

    public void Update()
    {
        if (PlayState.gameState == "Game")
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
                    case "nom":
                        internalVars[0] += Time.deltaTime;
                        transform.position = new Vector2(transform.position.x, Mathf.Lerp(vars[0], vars[0] + 1.25f, internalVars[0] * 1.2f));
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
                anim.Play("Bubble");
                break;
            case "explosion":
                var suffix = vars[0] switch
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
                };
                anim.Play("Explosion_" + suffix);
                break;
            case "nom":
                anim.Play("Nom");
                break;
            case "smoke":
                anim.Play("Smoke");
                break;
            case "splash":
                anim.Play("Splash");
                break;
        }
        sprite.sortingOrder = animType switch
        {
            _ => -15
        };
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
        gameObject.SetActive(false);
    }
}
