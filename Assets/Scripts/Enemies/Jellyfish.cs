using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jellyfish : Enemy
{
    private const float RADIUS = 1.875f;

    private float theta;
    private bool useDiagonalAnims = false;

    private void Awake()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        Spawn(350, 3, 50, true, 6);
        theta = transform.position.x * 0.7f + transform.position.y * 1.3f;

        anim.Add("Enemy_jellyfish_normal");
        anim.Add("Enemy_jellyfish_upRight");
        anim.Add("Enemy_jellyfish_downRight");
        anim.Add("Enemy_jellyfish_downLeft");
        anim.Add("Enemy_jellyfish_upLeft");
        anim.Play("Enemy_jellyfish_normal");

        int animData = PlayState.GetAnim("Enemy_jellyfish_data").frames[0];
        useDiagonalAnims = animData == 1;
    }

    private void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
            return;

        theta += Time.deltaTime;
        transform.localPosition = new Vector2(origin.x + RADIUS * Mathf.Sin(theta * 1.2f) + Mathf.Sin(theta * 12f) * 0.3f * PlayState.FRAC_8,
            origin.y + RADIUS * Mathf.Cos(theta * 1.2f) + Mathf.Cos(theta * 12f) * 0.3f * PlayState.FRAC_8);

        if (useDiagonalAnims)
        {
            string expectedAnimName = "Enemy_jellyfish_";
            if (transform.localPosition.x > origin.x)
                expectedAnimName += "down";
            else
                expectedAnimName += "up";
            if (transform.localPosition.y > origin.y)
                expectedAnimName += "Right";
            else
                expectedAnimName += "Left";

            if (anim.currentAnimName != expectedAnimName)
                anim.Play(expectedAnimName);
        }
    }
}
