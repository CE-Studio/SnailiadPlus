using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floatspike : Enemy
{
    private float targetX;
    private float targetY;
    private Vector2 origin;
    
    void Start()
    {
        Begin();
        box.size = new Vector2(0.95f, 0.95f);
        attack = 2;
        defense = 59;
        letsPermeatingShotsBy = true;

        origin = new Vector2(transform.position.x, transform.position.y);
        targetX = origin.x;
        targetY = origin.y;

        resistances.Add("Rainbow Wave");
        resistances.Add("Charged Wave");
        resistances.Add("Paralaser");
        resistances.Add("Charged Laser");
        resistances.Add("Charged Rang");
        resistances.Add("Charged Flares");
    }

    void Update()
    {
        if (PlayState.gameState == "Game")
        {
            if (transform.position.x > targetX - 0.0125f && transform.position.x < targetX + 0.0125f)
            {
                targetX = Random.Range(origin.x - 0.09375f, origin.x + 0.09375f);
            }
            if (transform.position.y > targetY - 0.0125f && transform.position.y < targetY + 0.0125f)
            {
                targetY = Random.Range(origin.y - 0.09375f, origin.y + 0.09375f);
            }
            float newX = Mathf.Lerp(transform.position.x, targetX, 0.005f);
            float newY = Mathf.Lerp(transform.position.y, targetY, 0.005f);
            transform.position = new Vector2(newX, newY);
        }
    }
}
