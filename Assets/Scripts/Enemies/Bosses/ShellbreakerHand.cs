using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellbreakerHand : Enemy
{
    private void Awake()
    {
        Spawn(100, 1, 1000, false, new Vector2(1.45f, 1.45f));
        makeSoundOnPing = false;
    }
}
