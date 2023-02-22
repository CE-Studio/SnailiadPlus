using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerGrass : MonoBehaviour
{
    public int totalBites = 12;
    public int healthPerBite = 3;
    public float biteTimeout = 0.17f;

    public int bitesRemaining;
    public bool active = false;
    public float timer = 0;

    public SpriteRenderer sprite;
    public BoxCollider2D box;
    public AnimationModule anim;

    public GameObject player;

    public void Start()
    {
        if (PlayState.gameState == PlayState.GameState.game)
        {
            bitesRemaining = totalBites;
            sprite = GetComponent<SpriteRenderer>();
            box = GetComponent<BoxCollider2D>();
            anim = GetComponent<AnimationModule>();
            anim.Add("PowerGrass_idle");
            anim.Add("PowerGrass_eaten");
            anim.Play("PowerGrass_idle");

            Physics2D.IgnoreCollision(transform.parent.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }
    }

    void Update()
    {
        if (PlayState.gameState == PlayState.GameState.game)
        {
            timer = Mathf.Clamp(timer - Time.deltaTime, 0, Mathf.Infinity);
        }
    }

    public void ToggleActive(bool state)
    {
        active = state;
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !PlayState.paralyzed)
        {
            if (timer == 0)
            {
                PlayState.PlaySound("EatPowerGrass");
                bitesRemaining--;
                if (bitesRemaining == 0)
                {
                    box.enabled = false;
                    anim.Play("PowerGrass_eaten");
                }
                else
                    timer = biteTimeout;
                PlayState.playerScript.HitFor(-healthPerBite);
                if (PlayState.gameOptions[11] > 1)
                    PlayState.RequestParticle(new Vector2(transform.position.x, transform.position.y + 0.25f), "nom");
            }
        }
    }
}
