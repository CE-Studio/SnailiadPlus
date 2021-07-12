using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    public const int totalBites = 3;
    public const int healthPerBite = 1;
    public const float biteTimeout = 0.25f;
    public const float regrowTimeout = 15f;

    private int bitesRemaining;
    public bool active = false;
    private float timer = 0;

    public SpriteRenderer sprite;
    public BoxCollider2D box;
    public AudioSource sfx;
    public AudioClip bite;
    public AudioClip regrow;
    
    void Start()
    {
        bitesRemaining = totalBites;
        sprite = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        sfx = GetComponent<AudioSource>();
        bite = (AudioClip)Resources.Load("Sounds/Sfx/EatGrass");
        regrow = (AudioClip)Resources.Load("Sounds/Sfx/GrassGrow");

        Transform targetObj1 = transform;
        Transform targetObj2;
        while (targetObj1.parent != null)
        {
            targetObj2 = targetObj1;
            targetObj1 = targetObj1.parent;
            if (targetObj1.name == "Room Triggers")
            {
                Physics2D.IgnoreCollision(targetObj2.GetComponent<Collider2D>(), GetComponent<Collider2D>());
            }
        }
    }

    void Update()
    {
        if (PlayState.gameState == "Game")
        {
            timer = Mathf.Clamp(timer - Time.deltaTime, 0, Mathf.Infinity);
        }
        if (active && timer == 0)
        {
            box.enabled = true;
            if (!sprite.enabled)
            {
                sfx.PlayOneShot(regrow);
                sprite.enabled = true;
                bitesRemaining = totalBites;
            }
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
        sfx.enabled = true;
        timer = 0;
        ToggleActive(true);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            box.enabled = false;
            sfx.PlayOneShot(bite);
            bitesRemaining--;
            if (bitesRemaining == 0)
            {
                timer = regrowTimeout;
                sprite.enabled = false;
            }
            else
            {
                timer = biteTimeout;
            }
        }
    }
}
