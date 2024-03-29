using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floatspike2 : Enemy
{
    private float theta;

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(5, 6, 250, true, 2);

        theta = origin.x * origin.x * 1.1f + origin.y * 3.2f + 0.7f;

        anim.Add("Enemy_floatspike_blue");
        anim.Play("Enemy_floatspike_blue");
    }

    void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        theta += Time.deltaTime;
        transform.localPosition = new Vector2(origin.x, origin.y + (Mathf.Sin(theta) * 1.8f * PlayState.FRAC_16) + PlayState.FRAC_32);
    }
}
