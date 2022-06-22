using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    public int damage;
    public int protectionRequired;

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

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (protectionRequired == 0 || (protectionRequired != 0 &&
                !PlayState.CheckForItem(protectionRequired switch { 3 => "Full-Metal Snail", 2 => "Gravity Snail", _ => "Ice Snail" })))
                PlayState.playerScript.HitFor(damage);
        }
    }
}
