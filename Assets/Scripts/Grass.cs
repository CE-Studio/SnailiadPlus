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

    public bool isCeilingGrass = false;
    
    public void Start()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        totalBites = new int[] { 6, 3, 1 };

        sprite = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        anim = GetComponent<AnimationModule>();
        anim.Add(string.Format("Grass_{0}_eaten", isCeilingGrass ? "ceiling" : "floor"));
        anim.AddAndPlay(string.Format("Grass_{0}_idle", isCeilingGrass ? "ceiling" : "floor"));

        Physics2D.IgnoreCollision(transform.parent.GetComponent<Collider2D>(), GetComponent<Collider2D>());

        bitesRemaining = totalBites[PlayState.currentProfile.difficulty];
        timer = 0;
    }

    public void Update()
    {
        if (PlayState.gameState == PlayState.GameState.game)
            timer = Mathf.Clamp(timer - Time.deltaTime, 0, Mathf.Infinity);

        if (timer == 0 && bitesRemaining == 0)
        {
            PlayState.PlaySound("GrassGrow");
            anim.Play(string.Format("Grass_{0}_idle", isCeilingGrass ? "ceiling" : "floor"));
            bitesRemaining = totalBites[PlayState.currentProfile.difficulty];
        }
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !PlayState.paralyzed)
        {
            if (timer == 0)
            {
                PlayState.PlaySound("EatGrass");
                bitesRemaining--;
                if (bitesRemaining == 0)
                {
                    timer = regrowTimeout;
                    anim.Play(string.Format("Grass_{0}_eaten", isCeilingGrass ? "ceiling" : "floor"));
                }
                else
                    timer = biteTimeout;
                PlayState.playerScript.HitFor(-healthPerBite);
                if (PlayState.generalData.particleState > 1)
                    PlayState.RequestParticle(new Vector2(transform.position.x, transform.position.y + 0.25f), "nom");
            }
        }
    }
}
