using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Spikey1))]
public class ArrowSpikey1 : Editor
{
    Spikey1 enemyScript;
    SpriteRenderer sprite;

    private void OnEnable()
    {
        enemyScript = (Spikey1)target;
        sprite = enemyScript.gameObject.GetComponent<SpriteRenderer>();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (enemyScript.rotation)
            sprite.sprite = enemyScript.spritwCCW;
        else
            sprite.sprite = enemyScript.spriteCW;
    }
}
