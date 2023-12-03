using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightMask : MonoBehaviour
{
    private bool flipX;
    private bool flipY;

    private void Start()
    {
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        GetComponent<AnimationModule>().AddMask(GetComponent<SpriteMask>());
        PlayState.AnimationData data = PlayState.GetAnim("LightMask_data");
        flipX = data.frames[0] == 1;
        flipY = data.frames[1] == 1;
    }

    private void Update()
    {
        Vector2 parsedPos = new(Mathf.RoundToInt(transform.position.x * 16f), Mathf.RoundToInt(transform.position.y * 16));
        bool flipStateX = flipX && Mathf.Abs(parsedPos.x) % 2 == 1;
        bool flipStateY = flipY && Mathf.Abs(parsedPos.y) % 2 == 1;
        transform.localScale = new Vector2(flipStateX ? -1 : 1, flipStateY ? -1 : 1);
    }
}
