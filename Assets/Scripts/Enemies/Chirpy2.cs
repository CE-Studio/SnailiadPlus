using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chirpy2 : Enemy
{
    private float speed = 5.625f;
    private float amplitude = 1.875f;
    private float thetaMultiplier = 4;
    private float theta = 0;
    private bool goingUp;
    private bool goingRight;
    private float originY;
    public bool spawnedViaGenerator = false;
    private bool active = false;
    private float lifeTimer = 10;

    public void Awake()
    {
        Spawn(1, 3, 0, true);
        anim.Add("Enemy_chirpy2_up");
        anim.Add("Enemy_chirpy2_down");
        originY = transform.position.y;
        theta = (transform.position.x + transform.position.y * 13.7f) % (Mathf.PI * 2);
        thetaMultiplier += Mathf.Sin(transform.position.x * 1.732f - transform.position.y * 3.2f);
        speed += (Mathf.Sin(transform.position.x * 2.332f - transform.position.y * 1.9f) * 10) * 0.0625f;
        amplitude += (Mathf.Sin(transform.position.x * 7.3f - transform.position.y) * 5) * 0.0625f;
        goingRight = transform.position.x < PlayState.player.transform.position.x;
        sprite.flipX = goingRight;
    }

    void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;
        if ((PlayState.OnScreen(transform.position, col) || spawnedViaGenerator) && !active)
        {
            active = true;
            PlayState.PlaySound("Chirp");
        }
        theta += Time.deltaTime;
        if (active)
        {
            lifeTimer -= Time.deltaTime;
            transform.position = new Vector2(transform.position.x + (speed * (goingRight ? 1 : -1) * Time.deltaTime), originY + Mathf.Sin(theta * thetaMultiplier) * amplitude);
            if (Mathf.Cos(theta * thetaMultiplier) < 0 && goingUp)
            {
                goingUp = false;
                anim.Play("Enemy_chirpy2_down");
            }
            else if (Mathf.Cos(theta * thetaMultiplier) > 0 && !goingUp)
            {
                goingUp = true;
                anim.Play("Enemy_chirpy2_up");
            }
        }
        if (lifeTimer < 0 && !PlayState.OnScreen(transform.position, col))
            Destroy(gameObject);
    }
}
