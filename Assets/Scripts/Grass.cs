using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    public int[] totalBites;
    public int healthPerBite = 1;
    public float biteTimeout = 0.22f;
    public float regrowTimeout = 15f;

    public int bitesRemaining;
    public float timer = 0;

    public SpriteRenderer sprite;
    public BoxCollider2D box;
    public AnimationModule anim;
    
    public void Start()
    {
        if (PlayState.gameState != "Game")
            return;

        totalBites = new int[] { 6, 3, 1 };

        sprite = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        anim = GetComponent<AnimationModule>();
        anim.Add("Grass_idle");
        anim.Add("Grass_eaten");
        anim.Play("Grass_idle");

        Physics2D.IgnoreCollision(transform.parent.GetComponent<Collider2D>(), GetComponent<Collider2D>());

        bitesRemaining = totalBites[PlayState.currentDifficulty];
        timer = 0;
    }

    public void Update()
    {
        if (PlayState.gameState == "Game")
            timer = Mathf.Clamp(timer - Time.deltaTime, 0, Mathf.Infinity);

        if (timer == 0 && bitesRemaining == 0)
        {
            PlayState.PlaySound("GrassGrow");
            anim.Play("Grass_idle");
            bitesRemaining = totalBites[PlayState.currentDifficulty];
        }
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (timer == 0)
            {
                PlayState.PlaySound("EatGrass");
                bitesRemaining--;
                if (bitesRemaining == 0)
                {
                    timer = regrowTimeout;
                    anim.Play("Grass_eaten");
                }
                else
                    timer = biteTimeout;
                collision.GetComponent<Player>().health = Mathf.Clamp(collision.GetComponent<Player>().health + healthPerBite, 0, collision.GetComponent<Player>().maxHealth);
                collision.GetComponent<Player>().UpdateHearts();
                if (PlayState.gameOptions[11] > 1)
                    PlayState.RequestParticle(new Vector2(transform.position.x, transform.position.y + 0.25f), "nom");
            }
        }
    }
}
