using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(IceSpike))]
public class RotateIceSpike : Editor
{
    IceSpike script;
    SpriteRenderer sprite;
    public Sprite down;
    public Sprite left;
    public Sprite up;
    public Sprite right;

    private void OnEnable()
    {
        script = (IceSpike)target;
        sprite = script.gameObject.GetComponent<SpriteRenderer>();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        sprite.sprite = script.direction == 3 ? right : (script.direction == 2 ? up : (script.direction == 1 ? left : down));
    }
}
