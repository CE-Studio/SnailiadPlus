using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chirpy1 : Enemy
{
    private float speed = 8.75f;
    private float amplitude = 2.5f;
    private float thetaMultiplier = 5;
    private float theta = 0;
    private bool goingUp;
    private bool goingRight;
    private float originY;
    public bool spawnedViaGenerator = false;
    private bool active = false;
    private float lifeTimer = 10;

    public void Awake()
    {
        Spawn(1, 2, 0, true, new Vector2(0.95f, 0.95f));
        anim.Add("Enemy_chirpy1_up");
        anim.Add("Enemy_chirpy1_down");
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
        if (PlayState.gameState != "Game")
            return;
        if ((OnScreen() || spawnedViaGenerator) && !active)
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
                anim.Play("Enemy_chirpy1_down");
            }
            else if (Mathf.Cos(theta * thetaMultiplier) > 0 && !goingUp)
            {
                goingUp = true;
                anim.Play("Enemy_chirpy1_up");
            }
        }
        if (lifeTimer < 0 && !OnScreen())
            Destroy(gameObject);
    }
}
