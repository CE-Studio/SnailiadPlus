using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bat : Enemy
{
    private const float REACT_DISTANCE = 6.875f;
    private const float SPEED = 4.375f;

    private bool active = false;
    private Vector2 velocity = Vector2.zero;

    private void Awake()
    {
        Spawn(120, 3, 10, true, new Vector2(1.95f, 0.95f));
        anim.Add("Enemy_bat_idle");
        anim.Add("Enemy_bat_fly");
        anim.Play("Enemy_bat_idle");
    }

    void Update()
    {
        if (PlayState.gameState != "Game")
            return;

        if (OnScreen() && !active && Mathf.Abs(PlayState.player.transform.position.x - transform.position.x) < REACT_DISTANCE)
        {
            active = true;
            anim.Play("Enemy_bat_fly");
            velocity = new Vector2(SPEED * (transform.position.x > PlayState.player.transform.position.x ? -1 : 1),
                -Mathf.Sqrt(Mathf.Abs((PlayState.player.transform.position.x - transform.position.x) * 16 + 40 + 
                    PlayState.playerScript.gravityDir switch { 0 => 8, 3 => -8, _ => 0 })) * (SPEED * 16) * 0.1875f * 0.0625f);
        }
        if (active)
        {
            transform.position = new Vector2(transform.position.x + velocity.x * Time.deltaTime, transform.position.y + velocity.y * Time.deltaTime);
            velocity.y += SPEED * 2 * Time.deltaTime;
            if (!OnScreen())
                Destroy(gameObject);
        }
    }
}
