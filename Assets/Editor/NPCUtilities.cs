using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPC))]
[CanEditMultipleObjects]
public class NPCUtilities:Editor {
    NPC script;
    private static readonly string[] looklist = new string[] { "Look at Player", "Left", "Right" };

    SerializedProperty sID;
    SerializedProperty sName;
    SerializedProperty sUpside;
    SerializedProperty sLook;

    public void OnEnable() {
        script = (NPC)target;

        sID = serializedObject.FindProperty("ID");
        sName = serializedObject.FindProperty("nameID");
        sUpside = serializedObject.FindProperty("upsideDown");
        sLook = serializedObject.FindProperty("lookMode");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        sID.intValue = EditorGUILayout.IntField("NPC ID: ", sID.intValue);
        sName.stringValue = EditorGUILayout.TextField("NPC Cutscene Name:", sName.stringValue);
        sUpside.boolValue = EditorGUILayout.Toggle("NPC upside down? ", sUpside.boolValue);
        sLook.intValue = EditorGUILayout.Popup("Look Behavior: ", sLook.intValue, looklist);

        serializedObject.ApplyModifiedProperties();
    }
}
