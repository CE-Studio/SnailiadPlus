using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    public int[] damageValues = new int[] { };
    private bool intersectingPlayer = false;

    public SpriteRenderer sprite;
    public BoxCollider2D box;
    public AnimationModule anim;
    
    public void Spawn(int[] damage)
    {
        sprite = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        anim = GetComponent<AnimationModule>();

        damageValues = damage;
    }

    public virtual void FixedUpdate()
    {
        if (intersectingPlayer && !PlayState.playerScript.stunned)
        {
            int thisDamage = damageValues[0];
            if (PlayState.stackShells)
            {
                int shellLevel = PlayState.GetShellLevel();
                for (int i = 1; i <= shellLevel; i++)
                {
                    if (damageValues[i] < thisDamage)
                        thisDamage = damageValues[i];
                }
            }
            else
            {
                if (PlayState.CheckForItem(7) && damageValues[1] < thisDamage)
                    thisDamage = damageValues[1];
                if (PlayState.CheckForItem(8) && damageValues[2] < thisDamage)
                    thisDamage = damageValues[2];
                if (PlayState.CheckForItem(9) && damageValues[3] < thisDamage)
                    thisDamage = damageValues[3];
            }
            if (thisDamage > 0)
                PlayState.playerScript.HitFor(thisDamage);
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
