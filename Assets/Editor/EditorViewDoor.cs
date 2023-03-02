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

    private SerializedProperty sWeapon;
    private SerializedProperty sLocked;
    private SerializedProperty sBoss;
    private SerializedProperty sDir;

    private void OnEnable()
    {
        script = (Door)target;
        sprite = script.gameObject.GetComponent<SpriteRenderer>();

        sWeapon = serializedObject.FindProperty("doorWeapon");
        sLocked = serializedObject.FindProperty("locked");
        sBoss = serializedObject.FindProperty("bossLock");
        sDir = serializedObject.FindProperty("direction");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        sWeapon.intValue = EditorGUILayout.Popup("Required weapon: ", sWeapon.intValue, new string[] { "Peashooter", "Boomerang", "Rainbow Wave", "Devastator" });
        sLocked.boolValue = EditorGUILayout.Toggle("Is this door locked?", sLocked.boolValue);
        sBoss.intValue = EditorGUILayout.Popup("Required boss: ", sBoss.intValue, new string[] { "Shellbreaker", "Stompy", "Space Box", "Moon Snail" });
        sDir.intValue = EditorGUILayout.Popup("Direction: ", sDir.intValue, new string[] { "Left", "Up", "Right", "Down" });

        serializedObject.ApplyModifiedProperties();

        int id = (sLocked.boolValue ? 4 : sWeapon.intValue) + ((sDir.intValue == 1 || sDir.intValue == 3) ? 5 : 0);
        sprite.sprite = script.editorSprites[id];
        sprite.flipX = false;
        sprite.flipY = false;
        if (sDir.intValue == 2)
            sprite.flipX = true;
        else if (sDir.intValue == 3)
            sprite.flipY = true;
    }
}
