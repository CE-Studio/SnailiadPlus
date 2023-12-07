using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ExtraPlatform))]
public class ExtraPlatformUtilities : Editor
{
    ExtraPlatform script;
    SpriteRenderer sprite;

    public Sprite plat0;
    public Sprite plat1;
    public Sprite plat2;
    public Sprite plat3;
    public Sprite plat4;
    public Sprite plat5;
    public Sprite plat6;
    public Sprite plat7;
    public Sprite plat8;
    public Sprite plat9;
    public Sprite plat10;
    public Sprite plat11;
    public Sprite plat12;
    public Sprite plat13;
    public Sprite plat14;
    public Sprite plat15;
    public Sprite plat16;
    public Sprite plat17;
    public Sprite plat18;

    SerializedProperty sCharStates;
    SerializedProperty sType;

    private void OnEnable()
    {
        script = (ExtraPlatform)target;
        sprite = script.GetComponent<SpriteRenderer>();

        sCharStates = serializedObject.FindProperty("appearToChars");
        sType = serializedObject.FindProperty("type");
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
        sType.intValue = EditorGUILayout.IntField("Platform type", Mathf.Clamp(sType.intValue, 0, 18));

        serializedObject.ApplyModifiedProperties();

        if (sType.intValue >= 0 && sType.intValue <= 18)
            sprite.sprite = new Sprite[]
            {
                plat0, plat1, plat2, plat3, plat4, plat5, plat6, plat7, plat8, plat9,
                plat10, plat11, plat12, plat13, plat14, plat15, plat16, plat17, plat18
            }[sType.intValue];
    }
}
