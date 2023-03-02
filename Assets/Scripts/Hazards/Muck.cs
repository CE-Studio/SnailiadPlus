using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Muck : Hazard
{
    public void Awake()
    {
        Spawn(new int[] { 3, 3, 3, 0 });

        string newAnim = "Hazard_muck_" + (Mathf.FloorToInt(transform.position.x) % 4).ToString();
        anim.Add(newAnim);
        anim.Play(newAnim);
    }
}
