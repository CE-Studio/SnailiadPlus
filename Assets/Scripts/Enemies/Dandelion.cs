using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dandelion : Enemy
{
    private const float WAVE_DISTANCE = 3f;
    private const float WAVE_SPEED = 1.2f;
    private const float RISE_SPEED = 0.75f;

    private float theta = 0f;
    private float originX = 0f;

    private void Awake()
    {
        Spawn(50, 3, 1, true, new Vector2(0.95f, 0.95f));
        originX = transform.position.x;
        anim.Add("Enemy_dandelion_left");
        anim.Add("Enemy_dandelion_right");
    }

    private void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game || box == null)
            return;

        theta += Time.deltaTime;
        transform.position = new Vector2(originX + WAVE_DISTANCE * Mathf.Sin(theta * WAVE_SPEED), transform.position.y + RISE_SPEED * Time.deltaTime);
        if (theta > PlayState.PI_OVER_TWO && theta <= PlayState.THREE_PI_OVER_TWO && anim.currentAnimName != "Enemy_dandelion_right")
            anim.Play("Enemy_dandelion_right");
        else if (anim.currentAnimName != "Enemy_dandelion_left")
            anim.Play("Enemy_dandelion_left");
    }
}
