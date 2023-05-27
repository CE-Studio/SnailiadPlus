using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider2 : Enemy
{
    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(2100, 4, 10, true);
    }
}
