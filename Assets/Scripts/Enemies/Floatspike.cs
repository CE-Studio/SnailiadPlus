using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floatspike : Enemy
{
    private float time;
    
    void Awake()
    {
        Begin();
        box.size = new Vector2(0.95f, 0.95f);
        attack = 2;
        defense = 59;
        maxHealth = 5;
        health = 5;
        letsPermeatingShotsBy = false;

        time = Random.Range(0, 100) * 0.01f;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        time = Random.Range(0, 100) * 0.01f;
    }

    void Update()
    {
        if (PlayState.gameState == "Game")
        {
            if (time >= 0.75f)
            {
                transform.localPosition = new Vector2(transform.localPosition.x, origin.y - 0.0625f);
            }
            else if (time >= 0.5f)
            {
                transform.localPosition = new Vector2(transform.localPosition.x,
                    Mathf.Lerp(origin.y + 0.125f, origin.y - 0.0625f, (time - 0.5f) * 4));
            }
            else if (time >= 0.25f)
            {
                transform.localPosition = new Vector2(transform.localPosition.x, origin.y + 0.125f);
            }
            else
            {
                transform.localPosition = new Vector2(transform.localPosition.x,
                    Mathf.Lerp(origin.y - 0.0625f, origin.y + 0.125f, time * 4));
            }
            time += Time.deltaTime * 0.25f;
            if (time >= 1)
                time = 0;
        }
    }
}
