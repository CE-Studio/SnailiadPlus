using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Babyfish))]
public class BabyfishType : Editor
{
    Babyfish enemyScript;
    SpriteRenderer sprite;
    public Sprite green;
    public Sprite pink;

    private void OnEnable()
    {
        enemyScript = (Babyfish)target;
        sprite = enemyScript.gameObject.GetComponent<SpriteRenderer>();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        sprite.sprite = enemyScript.type switch { 1 => pink, _ => green };
    }
}
