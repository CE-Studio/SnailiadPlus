using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellbreakerHand : Enemy
{
    private void Awake()
    {
        Spawn(100, 1, 1000, false);
        makeSoundOnPing = false;
    }
}
