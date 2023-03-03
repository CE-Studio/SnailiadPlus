using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Platform))]
[CanEditMultipleObjects]
public class PlatformUtilities : Editor
{
    public Sprite p11;
    public Sprite p12;
    public Sprite p13;
    public Sprite p14;
    public Sprite p21;
    public Sprite p22;
    public Sprite p23;
    public Sprite p24;
    Sprite[] sprites;

    Platform plat;
    BoxCollider2D box;
    SpriteRenderer sprite;
    
    SerializedProperty sSize;
    SerializedProperty sType;
    SerializedProperty sSpeed;
    SerializedProperty sA;
    SerializedProperty sB;
    SerializedProperty sBrake;

    const int WIDTH = 2;

    public void OnEnable()
    {
        plat = (Platform)target;
        box = plat.GetComponent<BoxCollider2D>();
        sprite = plat.GetComponent<SpriteRenderer>();

        sSize = serializedObject.FindProperty("size");
        sType = serializedObject.FindProperty("type");
        sSpeed = serializedObject.FindProperty("topSpeed");
        sA = serializedObject.FindProperty("aRelative");
        sB = serializedObject.FindProperty("bRelative");
        sBrake = serializedObject.FindProperty("brakePercentage");

        sprites = new Sprite[] { p11, p12, p13, p14, p21, p22, p23, p24 };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUILayout.Label("Platform size (1 to 4 tiles inclusive)");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Decrease"))
        {
            if (sSize.intValue > 1)
            {
                Undo.RecordObject(box, "Decrease Platform Size");
                sSize.intValue--;
                box.size = new Vector2(sSize.intValue, 1);
                EditorUtility.SetDirty(target);
            }
        }
        if (GUILayout.Button("Increase"))
        {
            if (sSize.intValue < 4)
            {
                Undo.RecordObject(box, "Increase Platform Size");
                sSize.intValue++;
                box.size = new Vector2(sSize.intValue, 1);
                EditorUtility.SetDirty(target);
            }
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5);

        sType.intValue = EditorGUILayout.Popup("Platform Type", sType.intValue, new string[] { "Blue", "Pink" });
        GUILayout.Space(5);

        sSpeed.floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Speed", sSpeed.floatValue), 0, Mathf.Infinity);
        sBrake.floatValue = Mathf.Clamp(EditorGUILayout.FloatField("Slowdown range", sBrake.floatValue), 0, 0.5f);
        GUILayout.Label("Range value is a percentage of the total\nrange of the platform, capping at 50%");
        GUILayout.Space(5);

        sA.vector2Value = EditorGUILayout.Vector2Field("Point A", sA.vector2Value);
        sB.vector2Value = EditorGUILayout.Vector2Field("Point B", sB.vector2Value);

        serializedObject.ApplyModifiedProperties();
    }

    public void OnSceneGUI()
    {
        Vector2 a = sA.vector2Value;
        Vector2 b = sB.vector2Value;
        Vector2 size = new Vector2(sSize.intValue * 0.5f, 0.5f);
        Vector2 pos = plat.transform.position;

        Handles.DrawSolidRectangleWithOutline(new Rect((Vector2)plat.transform.position + a - size, size * 2), new Color(1f, 1f, 1f, 0.1f), Color.white);
        Handles.DrawSolidRectangleWithOutline(new Rect((Vector2)plat.transform.position + b - size, size * 2), new Color(1f, 1f, 1f, 0.1f), Color.white);
        Handles.color = new Color(0, 1, 1);
        Handles.DrawLine((Vector2)plat.transform.position + a, (Vector2)plat.transform.position + b, WIDTH);

        Handles.DrawSolidRectangleWithOutline(new Rect(Vector2.Lerp(pos + a, pos + b, sBrake.floatValue) - size, size * 2), Color.clear, Color.white);
        Handles.DrawSolidRectangleWithOutline(new Rect(Vector2.Lerp(pos + b, pos + a, sBrake.floatValue) - size, size * 2), Color.clear, Color.white);

        sprite.sprite = sprites[sSize.intValue + (sType.intValue * 4) - 1];
    }
}
