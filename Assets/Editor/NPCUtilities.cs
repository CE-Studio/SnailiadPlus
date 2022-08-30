using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPC))]
[CanEditMultipleObjects]
public class NPCUtilities : Editor
{
    NPC script;

    private void OnEnable()
    {
        script = (NPC)target;
    }

    public override void OnInspectorGUI()
    {
        EditorUtility.SetDirty(target);

        script.ID = EditorGUILayout.IntField("NPC ID: ", script.ID);
        script.upsideDown = EditorGUILayout.Toggle("NPC upside down? ", script.upsideDown);
    }
}
