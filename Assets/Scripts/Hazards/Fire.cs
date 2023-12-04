using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : Hazard
{
    public void Awake()
    {
        Spawn(new int[] { 4, 1, 4, 0 });

        string newAnim = "Hazard_fire" + (Mathf.Abs(Mathf.Floor(transform.position.x) % 4) + 1);
        anim.Add(newAnim);
        anim.Play(newAnim);

        PlayState.globalFunctions.CreateLightMask(15, transform);
    }
}
