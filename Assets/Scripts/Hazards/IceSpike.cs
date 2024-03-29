using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceSpike : Hazard
{
    public int direction = 0;

    public void Spawn(int direction)
    {
        Spawn(new int[] { 2, 0, 2, 2 });

        this.direction = direction;
        string newAnim = "Hazard_iceSpike_" + (direction switch { 1 => "left", 2 => "up", 3 => "right", _ => "down" });
        anim.Add(newAnim);
        anim.Play(newAnim);
    }
}
