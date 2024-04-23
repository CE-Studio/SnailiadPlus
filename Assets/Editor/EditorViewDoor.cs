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
    private SerializedProperty sPermLocked;
    private SerializedProperty sBoss;
    private SerializedProperty sDir;
    private SerializedProperty sFragments;

    private void OnEnable()
    {
        script = (Door)target;
        sprite = script.gameObject.GetComponent<SpriteRenderer>();

        sWeapon = serializedObject.FindProperty("doorWeapon");
        sLocked = serializedObject.FindProperty("locked");
        sPermLocked = serializedObject.FindProperty("alwaysLocked");
        sBoss = serializedObject.FindProperty("bossLock");
        sDir = serializedObject.FindProperty("direction");
        sFragments = serializedObject.FindProperty("requiredFragments");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        sWeapon.intValue = EditorGUILayout.Popup("Required weapon: ", sWeapon.intValue, new string[] { "Peashooter", "Boomerang", "Rainbow Wave", "Devastator" });
        sLocked.boolValue = EditorGUILayout.Toggle("Is this door locked?", sLocked.boolValue);
        if (sLocked.boolValue)
            sPermLocked.boolValue = EditorGUILayout.Toggle("Permanently locked?", sPermLocked.boolValue);
        else
            sPermLocked.boolValue = false;
        sBoss.intValue = EditorGUILayout.Popup("Required boss: ", sBoss.intValue, new string[] { "Shellbreaker", "Stompy", "Space Box", "Moon Snail" });
        sDir.intValue = EditorGUILayout.Popup("Direction: ", sDir.intValue, new string[] { "Left", "Up", "Right", "Down" });
        GUILayout.Space(5);
        GUILayout.Label("Required Helix Fragments to open in\nrandomizer (when the option is turned on):");
        sFragments.intValue = EditorGUILayout.IntField(sFragments.intValue);

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
