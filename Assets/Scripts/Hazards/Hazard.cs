using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    public int damage;
    public int protectionRequired;
    private bool intersectingPlayer = false;

    public SpriteRenderer sprite;
    public BoxCollider2D box;
    public AnimationModule anim;
    
    public void Spawn(int newDamage, int newProtection)
    {
        sprite = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        anim = GetComponent<AnimationModule>();

        damage = newDamage;
        protectionRequired = newProtection;
    }

    public virtual void FixedUpdate()
    {
        if (intersectingPlayer && !PlayState.playerScript.stunned)
        {
            if (protectionRequired == 0 || (protectionRequired == 1 && PlayState.currentDifficulty == 2) || (protectionRequired != 0 &&
                !PlayState.CheckForItem(protectionRequired switch { 3 => "Full-Metal Snail", 2 => "Gravity Snail", _ => "Ice Snail" })))
                PlayState.playerScript.HitFor(damage);
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            intersectingPlayer = true;
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            intersectingPlayer = false;
    }
}
