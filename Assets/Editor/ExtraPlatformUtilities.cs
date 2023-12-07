using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ExtraPlatform))]
public class ExtraPlatformUtilities : Editor
{
    ExtraPlatform script;
    SpriteRenderer sprite;

    public Sprite[] platSprites;

    SerializedProperty sCharStates;
    SerializedProperty sType;

    private void OnEnable()
    {
        script = (ExtraPlatform)target;
        sprite = script.GetComponent<SpriteRenderer>();

        sCharStates = serializedObject.FindProperty("appearToChars");
        sType = serializedObject.FindProperty("type");

        platSprites = Resources.LoadAll<Sprite>("Images/Entities/ExtraPlatform");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUILayout.Label("This platform will appear for...");
        sCharStates.GetArrayElementAtIndex(0).boolValue = EditorGUILayout.Toggle("Snaily", sCharStates.GetArrayElementAtIndex(0).boolValue);
        sCharStates.GetArrayElementAtIndex(1).boolValue = EditorGUILayout.Toggle("Sluggy", sCharStates.GetArrayElementAtIndex(1).boolValue);
        sCharStates.GetArrayElementAtIndex(2).boolValue = EditorGUILayout.Toggle("Upside", sCharStates.GetArrayElementAtIndex(2).boolValue);
        sCharStates.GetArrayElementAtIndex(3).boolValue = EditorGUILayout.Toggle("Leggy", sCharStates.GetArrayElementAtIndex(3).boolValue);
        sCharStates.GetArrayElementAtIndex(4).boolValue = EditorGUILayout.Toggle("Blobby", sCharStates.GetArrayElementAtIndex(4).boolValue);
        sCharStates.GetArrayElementAtIndex(5).boolValue = EditorGUILayout.Toggle("Leechy", sCharStates.GetArrayElementAtIndex(5).boolValue);

        GUILayout.Space(5);
        sType.intValue = EditorGUILayout.IntField("Platform type", Mathf.Clamp(sType.intValue, 0, platSprites.Length - 1));

        serializedObject.ApplyModifiedProperties();

        if (sType.intValue >= 0 && sType.intValue < platSprites.Length)
            sprite.sprite = platSprites[sType.intValue];
    }
}
