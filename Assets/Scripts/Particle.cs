using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    public bool isActive = false;
    public Animator anim;
    public SpriteRenderer sprite;
    public string type = "";
    public float extCounter = 0;
    private float intCounter = 0;

    public void Start()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    public void Update()
    {
        if (PlayState.gameState == "Game")
        {
            anim.speed = 1;
            switch (type)
            {
                default:
                    break;
                case "bubble":
                    break;
            }
        }
        else
            anim.speed = 0;
    }

    public void InitializeParticle()
    {
        switch (type)
        {
            default:
                break;
            case "bubble":
                break;
        }
    }

    public void ResetParticle()
    {
        transform.position = Vector2.zero;
        GetComponent<Animator>().Play("Blank", 0, 0);
        isActive = false;
        sprite.flipX = false;
        sprite.flipY = false;
    }
}
