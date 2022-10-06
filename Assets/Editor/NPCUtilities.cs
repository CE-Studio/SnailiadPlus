using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPC))]
[CanEditMultipleObjects]
public class NPCUtilities:Editor {
    NPC script;
    private static readonly string[] looklist = new string[] { "Look at Player", "Left", "Right" };

    private void OnEnable() {
        script = (NPC)target;
    }

    public override void OnInspectorGUI() {
        EditorUtility.SetDirty(target);

        script.ID = EditorGUILayout.IntField("NPC ID: ", script.ID);
        script.nameID = EditorGUILayout.TextField("NPC Cutscene Name:", script.nameID);
        script.upsideDown = EditorGUILayout.Toggle("NPC upside down? ", script.upsideDown);
        script.lookMode = EditorGUILayout.Popup("Look Behavior: ", script.lookMode, looklist);
    }
}
