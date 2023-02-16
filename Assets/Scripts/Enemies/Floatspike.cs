using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floatspike : Enemy
{
    private float time;
    
    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;
        
        Spawn(5, 2, 59, true);

        time = Random.Range(0, 100) * 0.01f;

        anim.Add("Enemy_floatspike_black");
        anim.Play("Enemy_floatspike_black");
    }

    void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

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
