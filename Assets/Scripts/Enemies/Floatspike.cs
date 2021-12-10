using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floatspike : Enemy
{
    private float targetX;
    private float targetY;
    private Vector2 origin;
    private float time;
    
    void Start()
    {
        Begin();
        box.size = new Vector2(0.95f, 0.95f);
        attack = 2;
        defense = 59;
        letsPermeatingShotsBy = true;

        origin = new Vector2(transform.localPosition.x, transform.localPosition.y);
        targetX = origin.x;
        targetY = origin.y;

        resistances.Add(1);
        resistances.Add(2);
        resistances.Add(3);

        time = Random.Range(0, 100) * 0.01f;
    }

    void Update()
    {
        if (PlayState.gameState == "Game")
        {
            //if (transform.position.x > targetX - 0.0125f && transform.position.x < targetX + 0.0125f)
            //{
            //    targetX = Random.Range(origin.x - 0.09375f, origin.x + 0.09375f);
            //}
            //if (transform.position.y > targetY - 0.0125f && transform.position.y < targetY + 0.0125f)
            //{
            //    targetY = Random.Range(origin.y - 0.09375f, origin.y + 0.09375f);
            //}
            //float newX = Mathf.Lerp(transform.position.x, targetX, 0.005f);
            //float newY = Mathf.Lerp(transform.position.y, targetY, 0.005f);
            //transform.position = new Vector2(newX, newY);
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
