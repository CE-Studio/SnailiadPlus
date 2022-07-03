using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TextObjectEditorPointer))]
[CanEditMultipleObjects]
public class EditorTextObjectUtilities : Editor
{
    Transform parent;
    TextMesh text;
    TextMesh shadow;
    string textString = "";
    int currentTextSize = 1;

    public void OnEnable()
    {
        TextObjectEditorPointer script = (TextObjectEditorPointer)target;
        parent = script.gameObject.transform;
        text = script.gameObject.transform.GetChild(0).GetComponent<TextMesh>();
        shadow = script.gameObject.transform.GetChild(1).GetComponent<TextMesh>();
        currentTextSize = Mathf.RoundToInt(text.characterSize * 16);
    }

    public override void OnInspectorGUI()
    {
        textString = EditorGUILayout.TextField("Text", textString);
        if (GUILayout.Button("Set text"))
            SetText(textString);
        GUILayout.Space(10);

        GUILayout.Label("Screen pixels per character pixel");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("-1"))
        {
            if (currentTextSize != 1)
            {
                currentTextSize--;
                SetSize(currentTextSize * 0.0625f);
            }
        }
        if (GUILayout.Button("+1"))
        {
            currentTextSize++;
            SetSize(currentTextSize * 0.0625f);
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);

        if (GUILayout.Button("Snap to nearest pixel"))
        {
            float yAdjust = 0;
            if (text.anchor == TextAnchor.UpperLeft || text.anchor == TextAnchor.UpperCenter || text.anchor == TextAnchor.UpperRight)
                yAdjust = 0.025f;
            else if (text.anchor == TextAnchor.MiddleLeft || text.anchor == TextAnchor.MiddleCenter || text.anchor == TextAnchor.MiddleRight)
                yAdjust = -0.0075f;
            parent.transform.position = new Vector2(Mathf.Round(parent.transform.position.x * 16) * 0.0625f,
                Mathf.Round(parent.transform.position.y * 16) * 0.0625f + yAdjust);
        }
        GUILayout.Space(10);

        GUILayout.Label("Set text anchor");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Top left"))
            SetAnchor(TextAnchor.UpperLeft);
        if (GUILayout.Button("Top center"))
            SetAnchor(TextAnchor.UpperCenter);
        if (GUILayout.Button("Top right"))
            SetAnchor(TextAnchor.UpperRight);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Middle left"))
            SetAnchor(TextAnchor.MiddleLeft);
        if (GUILayout.Button("Middle center"))
            SetAnchor(TextAnchor.MiddleCenter);
        if (GUILayout.Button("Middle right"))
            SetAnchor(TextAnchor.MiddleRight);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Bottom left"))
            SetAnchor(TextAnchor.LowerLeft);
        if (GUILayout.Button("Bottom center"))
            SetAnchor(TextAnchor.LowerCenter);
        if (GUILayout.Button("Bottom right"))
            SetAnchor(TextAnchor.LowerRight);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);

        GUILayout.Label("Set text alignment");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Left"))
            SetAlignment(TextAlignment.Left);
        if (GUILayout.Button("Center"))
            SetAlignment(TextAlignment.Center);
        if (GUILayout.Button("Right"))
            SetAlignment(TextAlignment.Right);
        EditorGUILayout.EndHorizontal();

        //base.OnInspectorGUI();
    }

    private void SetText(string input)
    {
        string newText = input.Replace("\\n", "\n");
        text.text = newText;
        shadow.text = newText;
    }

    private void SetSize(float size)
    {
        text.characterSize = size;
        shadow.characterSize = size;
    }

    private void SetAnchor(TextAnchor anchor)
    {
        text.anchor = anchor;
        shadow.anchor = anchor;
    }

    private void SetAlignment(TextAlignment alignment)
    {
        text.alignment = alignment;
        shadow.alignment = alignment;
    }
}
