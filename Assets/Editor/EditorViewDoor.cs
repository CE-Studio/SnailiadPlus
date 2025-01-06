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
    private SerializedProperty sHelixLocked;
    private SerializedProperty sTargetSphere;
    private SerializedProperty sBoss;
    private SerializedProperty sDir;

    private void OnEnable()
    {
        script = (Door)target;
        sprite = script.gameObject.GetComponent<SpriteRenderer>();

        sWeapon = serializedObject.FindProperty("doorWeapon");
        sLocked = serializedObject.FindProperty("locked");
        sPermLocked = serializedObject.FindProperty("alwaysLocked");
        sRandoLocked = serializedObject.FindProperty("randoLocked");
        sHelixLocked = serializedObject.FindProperty("helixLocked");
        sTargetSphere = serializedObject.FindProperty("helixLockTargetSphere");
        sBoss = serializedObject.FindProperty("bossLock");
        sDir = serializedObject.FindProperty("direction");
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
            sHelixLocked.boolValue = EditorGUILayout.Toggle("   Helix-locked?", sHelixLocked.boolValue);
            if (sHelixLocked.boolValue)
            {
                GUILayout.Label("Item sphere to count fragments in\n" +
                    "   (This determines how many are needed\n   to open by counting all fragments\n   in this and all previous spheres)");
                sTargetSphere.intValue = EditorGUILayout.IntField(sTargetSphere.intValue);
            }
        }
        else
        {
            sPermLocked.boolValue = false;
            sRandoLocked.boolValue = false;
            sHelixLocked.boolValue = false;
        }
        if (sLocked.boolValue && !sPermLocked.boolValue && !sHelixLocked.boolValue)
            sBoss.intValue = EditorGUILayout.Popup("Required boss: ", sBoss.intValue, new string[] { "Shellbreaker", "Stompy", "Space Box", "Moon Snail" });
        sDir.intValue = EditorGUILayout.Popup("Direction: ", sDir.intValue, new string[] { "Left", "Up", "Right", "Down" });

        serializedObject.ApplyModifiedProperties();

        int id = ((sLocked.boolValue && !sRandoLocked.boolValue && !sHelixLocked.boolValue) ? 4 : sWeapon.intValue)
            + ((sDir.intValue == 1 || sDir.intValue == 3) ? 5 : 0);
        if (sLocked.boolValue && sHelixLocked.boolValue && !(id == 4 || id == 9))
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
