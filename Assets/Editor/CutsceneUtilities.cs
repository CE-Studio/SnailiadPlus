using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CutsceneController))]
[CanEditMultipleObjects]
public class CutsceneUtilities : Editor
{
    CutsceneController script;
    BoxCollider2D box;

    private void OnEnable()
    {
        script = (CutsceneController)target;
        box = script.GetComponent<BoxCollider2D>();
    }

    public override void OnInspectorGUI()
    {
        EditorUtility.SetDirty(target);
        script.triggerActive = EditorGUILayout.Toggle("Trigger active?", script.triggerActive);
        box.size = EditorGUILayout.Vector2Field("Trigger size", box.size);
        GUILayout.Space(15);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("actors"), true);
        GUILayout.Label("The player is automatically counted as an actor");
        GUILayout.Space(15);

        GUILayout.Label("Cutscene script");
        script.sceneScript = EditorGUILayout.TextArea(script.sceneScript);
    }
}
