using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightMask : MonoBehaviour
{
    public SpriteRenderer sprite;
    public AnimationModule anim;

    private bool flipX;
    private bool flipY;
    public bool isActive = true;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        sprite.color = new Color(1, 1, 1, 0);
        anim = GetComponent<AnimationModule>();
        anim.AddMask(GetComponent<SpriteMask>());
        PlayState.AnimationData data = PlayState.GetAnim("LightMask_data");
        flipX = data.frames[0] == 1;
        flipY = data.frames[1] == 1;
    }

    private void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game || !isActive)
            return;

        Vector2 parsedPos = new(Mathf.RoundToInt(transform.position.x * 16f), Mathf.RoundToInt(transform.position.y * 16));
        bool flipStateX = flipX && Mathf.Abs(parsedPos.x) % 2 == 1;
        bool flipStateY = flipY && Mathf.Abs(parsedPos.y) % 2 == 1;
        transform.localScale = new Vector2(flipStateX ? -1 : 1, flipStateY ? -1 : 1);
    }

    public void SetSize(int newSize)
    {
        anim.ClearList();
        if (newSize == -1)
        {
            isActive = false;
            anim.Stop();
            return;
        }
        isActive = true;
        newSize = Mathf.Clamp(newSize, 0, 24);
        string animName = "LightMask_" + newSize.ToString();
        anim.AddAndPlay(animName);
    }

    public void Instance(int newSize, Transform parent)
    {
        Instance(newSize, parent.position);
        transform.parent = parent;
    }
    public void Instance(int newSize, Vector2 newPos)
    {
        SetSize(newSize);
        transform.position = newPos;
    }
}
