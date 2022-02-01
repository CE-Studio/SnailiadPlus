using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Door))]
public class EditorViewDoor : Editor
{
    Door script;
    SpriteRenderer sprite;

    private void OnEnable()
    {
        script = (Door)target;
        sprite = script.gameObject.GetComponent<SpriteRenderer>();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        int id = (script.locked ? 4 : script.doorWeapon) + ((script.direction == 1 || script.direction == 3) ? 5 : 0);
        sprite.sprite = script.editorSprites[id];
        sprite.flipX = false;
        sprite.flipY = false;
        if (script.direction == 2)
            sprite.flipX = true;
        else if (script.direction == 3)
            sprite.flipY = true;
    }
}
