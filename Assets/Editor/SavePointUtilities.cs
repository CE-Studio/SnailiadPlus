using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SavePoint))]
[CanEditMultipleObjects]
public class SavePointUtilities : Editor
{
    public SavePoint script;
    public SpriteRenderer sprite;

    public Sprite saveD;
    public Sprite saveL;
    public Sprite saveR;
    public Sprite saveU;

    SerializedProperty sSurface;
    SerializedProperty sExclusive;

    public void OnEnable()
    {
        script = (SavePoint)target;
        sprite = script.GetComponent<SpriteRenderer>();

        sSurface = serializedObject.FindProperty("surface");
        sExclusive = serializedObject.FindProperty("exclusiveSurface");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        sSurface.intValue = EditorGUILayout.Popup("Surface", sSurface.intValue, new string[] { "Floor", "Left Wall", "Right Wall", "Ceiling" });
        GUILayout.Label("Is this point exclusive to characters\nwho default to this surface?");
        sExclusive.boolValue = EditorGUILayout.Toggle(sExclusive.boolValue);

        serializedObject.ApplyModifiedProperties();
    }

    public void OnSceneGUI()
    {
        sprite.sprite = sSurface.intValue switch
        {
            1 => saveL,
            2 => saveR,
            3 => saveU,
            _ => saveD
        };
    }
}
