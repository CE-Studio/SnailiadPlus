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
        bitesRemaining = totalBites;
        sprite = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        anim = GetComponent<AnimationModule>();
        anim.Add("PowerGrass_idle");
        anim.Add("PowerGrass_eaten");

        Physics2D.IgnoreCollision(transform.parent.GetComponent<Collider2D>(), GetComponent<Collider2D>());
    }

    void Update()
    {
        if (PlayState.gameState == "Game")
        {
            timer = Mathf.Clamp(timer - Time.deltaTime, 0, Mathf.Infinity);
        }
    }

    public void ToggleActive(bool state)
    {
        active = state;
    }

    public void Spawn()
    {
        bitesRemaining = totalBites;
        sprite.enabled = true;
        box.enabled = true;
        timer = 0;
        if (transform.childCount > 0)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        ToggleActive(true);
        anim.Play("PowerGrass_idle");
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
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
                collision.GetComponent<Player>().health = Mathf.Clamp(collision.GetComponent<Player>().health + healthPerBite, 0, collision.GetComponent<Player>().maxHealth);
                collision.GetComponent<Player>().UpdateHearts();
                if (PlayState.gameOptions[11] > 1)
                    PlayState.RequestParticle(new Vector2(transform.position.x, transform.position.y + 0.25f), "nom");
            }
        }
    }
}
