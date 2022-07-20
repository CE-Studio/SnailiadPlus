using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Muck : Hazard
{
    public void Start()
    {
        Spawn(3, 3);

        string newAnim = "Hazard_muck_" + (Mathf.FloorToInt(transform.position.x) % 4).ToString();
        anim.Add(newAnim);
        anim.Play(newAnim);
    }
}
