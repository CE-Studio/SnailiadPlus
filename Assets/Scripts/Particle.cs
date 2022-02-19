using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    public bool isActive = false;
    public Animator anim;
    public SpriteRenderer sprite;
    public string type = "";
    public float[] vars = new float[] { 0, 0, 0, 0, 0 };
    public ParticleSpriteCollection sprites;

    public void Start()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        anim.enabled = false;
        gameObject.SetActive(false);
    }

    public void Update()
    {
        if (PlayState.gameState == "Game")
        {
            anim.speed = 1;
            if (gameObject.activeSelf)
            {
                switch (type)
                {
                    default:
                        break;
                    case "bubble":
                        vars[0] += Time.deltaTime;
                        transform.position = new Vector2(vars[1] + 2 * Mathf.Sin(vars[0] / 1.2f) * 0.0625f, transform.position.y + vars[3] * Time.deltaTime * 0.25f);
                        if (transform.position.y > vars[2])
                            ResetParticle();
                        break;
                }
            }
        }
        else
            anim.speed = 0;
    }

    public void ResetParticle()
    {
        transform.position = Vector2.zero;
        anim.enabled = false;
        sprite.sprite = sprites.blank;
        sprite.flipX = false;
        sprite.flipY = false;
        for (int i = 0; i < vars.Length; i++)
            vars[i] = 0;
        gameObject.SetActive(false);
    }
}
