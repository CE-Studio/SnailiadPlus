using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Door))]
[CanEditMultipleObjects]
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
        EditorUtility.SetDirty(target);
        script.doorWeapon = EditorGUILayout.Popup("Required weapon: ", script.doorWeapon, new string[] { "Peashooter", "Boomerang", "Rainbow Wave", "Devastator" });
        script.locked = EditorGUILayout.Toggle("Is this door locked?", script.locked);
        script.bossLock = EditorGUILayout.Popup("Required boss: ", script.bossLock, new string[] { "Shellbreaker", "Stompy", "Space Box", "Moon Snail" });
        script.direction = EditorGUILayout.Popup("Direction: ", script.direction, new string[] { "Left", "Up", "Right", "Down" });

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
