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
    private SerializedProperty sRandoLocked;
    private SerializedProperty sBoss;
    private SerializedProperty sDir;
    private SerializedProperty sFragmentsMin;
    private SerializedProperty sFragmentsMaj;

    private void OnEnable()
    {
        script = (Door)target;
        sprite = script.gameObject.GetComponent<SpriteRenderer>();

        sWeapon = serializedObject.FindProperty("doorWeapon");
        sLocked = serializedObject.FindProperty("locked");
        sPermLocked = serializedObject.FindProperty("alwaysLocked");
        sRandoLocked = serializedObject.FindProperty("randoLocked");
        sBoss = serializedObject.FindProperty("bossLock");
        sDir = serializedObject.FindProperty("direction");
        sFragmentsMin = serializedObject.FindProperty("requiredFragmentsMin");
        sFragmentsMaj = serializedObject.FindProperty("requiredFragmentsMaj");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        sWeapon.intValue = EditorGUILayout.Popup("Required weapon: ", sWeapon.intValue, new string[] { "Peashooter", "Boomerang", "Rainbow Wave", "Devastator" });
        sLocked.boolValue = EditorGUILayout.Toggle("Is this door locked?", sLocked.boolValue);
        if (sLocked.boolValue)
        {
            sPermLocked.boolValue = EditorGUILayout.Toggle("   Permanently?", sPermLocked.boolValue);
            sRandoLocked.boolValue = EditorGUILayout.Toggle("   Only in rando?", sRandoLocked.boolValue);
        }
        else
        {
            sPermLocked.boolValue = false;
            sRandoLocked.boolValue = false;
        }
        sBoss.intValue = EditorGUILayout.Popup("Required boss: ", sBoss.intValue, new string[] { "Shellbreaker", "Stompy", "Space Box", "Moon Snail" });
        sDir.intValue = EditorGUILayout.Popup("Direction: ", sDir.intValue, new string[] { "Left", "Up", "Right", "Down" });
        GUILayout.Space(5);
        GUILayout.Label("Required Helix Fragments to open in\nrandomizer (when the option is turned on):");
        sFragmentsMin.intValue = EditorGUILayout.IntField("Set area prog.", sFragmentsMin.intValue);
        sFragmentsMaj.intValue = EditorGUILayout.IntField("Open area prog.", sFragmentsMaj.intValue);

        serializedObject.ApplyModifiedProperties();

        int id = ((sLocked.boolValue && !sRandoLocked.boolValue) ? 4 : sWeapon.intValue) + ((sDir.intValue == 1 || sDir.intValue == 3) ? 5 : 0);
        if (sLocked.boolValue && sRandoLocked.boolValue && !(id == 4 || id == 9))
            id += 10;
        sprite.sprite = script.editorSprites[id];
        sprite.flipX = false;
        sprite.flipY = false;
        if (sDir.intValue == 2)
            sprite.flipX = true;
        else if (sDir.intValue == 3)
            sprite.flipY = true;
    }
}
