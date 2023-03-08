using System;
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
    SerializedProperty sPath;

    const int WIDTH = 2;

    public void OnEnable()
    {
        plat = (Platform)target;
        box = plat.transform.GetChild(0).GetComponent<BoxCollider2D>();
        sprite = plat.transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (plat.pathAnim == null)
            plat.pathAnim = plat.transform.GetChild(0).GetComponent<Animator>();

        sSize = serializedObject.FindProperty("size");
        sType = serializedObject.FindProperty("type");
        sSpeed = serializedObject.FindProperty("speed");
        sPath = serializedObject.FindProperty("pathName");

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

        sSpeed.floatValue = EditorGUILayout.FloatField("Speed", sSpeed.floatValue);
        GUILayout.Space(5);

        AnimationClip[] paths = plat.pathAnim.runtimeAnimatorController.animationClips;
        string[] pathNames = new string[paths.Length + 1];
        pathNames[0] = "None";
        for (int i = 0; i < paths.Length; i++)
            pathNames[i + 1] = paths[i].name;
        int index = Mathf.Clamp(Array.IndexOf(pathNames, sPath.stringValue), 0, paths.Length);
        sPath.stringValue = pathNames[EditorGUILayout.Popup("Path", index, pathNames)];
        GUILayout.Label("Make sure that any new paths you've created\nmove the child object with the sprite and\nhitbox, not the origin object!");

        serializedObject.ApplyModifiedProperties();
    }

    public void OnSceneGUI()
    {
        sprite.sprite = sprites[sSize.intValue + (sType.intValue * 4) - 1];
    }
}
