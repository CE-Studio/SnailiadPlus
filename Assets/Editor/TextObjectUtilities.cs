using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TextObject))]
[CanEditMultipleObjects]
public class TextObjectUtilities : Editor
{
    TextObject script;
    string textToAssign = "";
    const float PIXEL = 0.0625f;

    private void OnEnable()
    {
        script = (TextObject)target;
        script.Initialize();
    }

    public override void OnInspectorGUI()
    {
        textToAssign = EditorGUILayout.TextField("New Text", textToAssign);
        if (GUILayout.Button("Assign Text"))
            script.SetText(textToAssign);
        
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Decrease Size"))
            script.SetSize(script.size - 1);
        if (GUILayout.Button("Increase Size"))
            script.SetSize(script.size + 1);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        if (GUILayout.Button("Clear Formatting"))
            script.ClearChildText();
        if (GUILayout.Button("Add Shadow"))
            script.CreateShadow();
        if (GUILayout.Button("Add Outline"))
            script.CreateOutline();

        GUILayout.Space(10);
        GUILayout.Label("Text Alignment");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Left"))
            script.SetAlignment("left");
        if (GUILayout.Button("Center"))
            script.SetAlignment("center");
        if (GUILayout.Button("Right"))
            script.SetAlignment("right");
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("Set Position in Pixels");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Up-Left"))
            script.transform.position += new Vector3(-PIXEL, PIXEL, 0);
        if (GUILayout.Button("Up"))
            script.transform.position += new Vector3(0, PIXEL, 0);
        if (GUILayout.Button("Up-Right"))
            script.transform.position += new Vector3(PIXEL, PIXEL, 0);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Left"))
            script.transform.position += new Vector3(-PIXEL, 0, 0);
        if (GUILayout.Button("Snap to Grid"))
            script.transform.position = new Vector3(Mathf.Round(script.transform.position.x * 16) * PIXEL, Mathf.Round(script.transform.position.y * 16) * PIXEL, 0);
        if (GUILayout.Button("Right"))
            script.transform.position += new Vector3(PIXEL, 0, 0);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Down-Left"))
            script.transform.position += new Vector3(-PIXEL, -PIXEL, 0);
        if (GUILayout.Button("Down"))
            script.transform.position += new Vector3(0, -PIXEL, 0);
        if (GUILayout.Button("Down-Right"))
            script.transform.position += new Vector3(PIXEL, -PIXEL, 0);
        GUILayout.EndHorizontal();
    }
}
