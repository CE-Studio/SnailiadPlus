using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider1 : Enemy
{
    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(500, 3, 10, true);
    }
}
