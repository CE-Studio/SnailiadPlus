using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Snelk))]
[CanEditMultipleObjects]
public class SnelkUtilities : Editor
{
    SerializedProperty sState;
    SerializedProperty sChance;
    SerializedProperty sFacing;

    public void OnEnable()
    {
        sState = serializedObject.FindProperty("state");
        sChance = serializedObject.FindProperty("spawnChance");
        sFacing = serializedObject.FindProperty("facingState");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        sState.intValue = EditorGUILayout.Popup("Initial state", sState.intValue, new string[] { "Hop around at random", "Run away from the player", "Sleeping" });
        sChance.floatValue = Mathf.Clamp(EditorGUILayout.FloatField("% chance to spawn", sChance.floatValue * 100f) * 0.01f, 0f, 1f);
        sFacing.intValue = EditorGUILayout.Popup("Initial direction", sFacing.intValue, new string[]
        {
            "Right",
            "Left",
            "Either, at random",
            "Facing the player",
            "Facing away from the player"
        });

        serializedObject.ApplyModifiedProperties();
    }
}
