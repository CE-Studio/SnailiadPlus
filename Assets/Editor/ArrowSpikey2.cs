using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Spikey2))]
public class ArrowSpikey2 : Editor
{
    Spikey2 enemyScript;
    SpriteRenderer sprite;
    public Sprite CW;
    public Sprite CCW;

    private void OnEnable()
    {
        enemyScript = (Spikey2)target;
        sprite = enemyScript.gameObject.GetComponent<SpriteRenderer>();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        sprite.sprite = enemyScript.rotation ? CCW : CW;
    }
}
