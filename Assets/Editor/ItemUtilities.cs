using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Item))]
[CanEditMultipleObjects]
public class ItemUtilities : Editor
{
    Item script;
    SpriteRenderer sprite;

    SerializedProperty sID;
    SerializedProperty sUnique;
    SerializedProperty sCounted;
    SerializedProperty sDiffs;
    SerializedProperty sChars;

    private void OnEnable()
    {
        script = (Item)target;
        sprite = script.GetComponent<SpriteRenderer>();

        sID = serializedObject.FindProperty("itemID");
        sUnique = serializedObject.FindProperty("isSuperUnique");
        sCounted = serializedObject.FindProperty("countedInPercentage");
        sDiffs = serializedObject.FindProperty("difficultiesPresentIn");
        sChars = serializedObject.FindProperty("charactersPresentFor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        sID.intValue = EditorGUILayout.Popup("Item type: ", sID.intValue == 24 ? 15 : sID.intValue + 1, new string[]
        {
            "Nothing",
            "Peashooter",
            "Boomerang",
            "Rainbow Wave",
            "Devastator",
            "High Jump",
            "Shell Shield",
            "Rapid Fire",
            "Ice Snail",
            "Gravity Snail",
            "Full-Metal Snail",
            "Gravity Shock",
            "Super Secret Boomerang",
            "Debug Rainbow Wave",
            "Heart Container",
            "Helix Fragment"
        }) - 1;
        if (sID.intValue == 14)
            sID.intValue = 24;
        //GUILayout.Label("Item ID: " + script.itemID);
        if (sID.intValue == 4)
            GUILayout.Label("Also corresponds to:\n- Wall Grab (Blobby)");
        if (sID.intValue == 5)
            GUILayout.Label("Also corresponds to:\n- Shelmet (Blobby)");
        if (sID.intValue == 6)
            GUILayout.Label("Also corresponds to:\n- Backfire (Leechy)");
        if (sID.intValue == 8)
            GUILayout.Label("Also corresponds to:\n- Magnetic Foot (Upside)\n- Corkscrew Jump (Leggy)\n- Angel Jump (Blobby)");
        GUILayout.Space(15);

        GUILayout.Label("Is this item super unique?");
        sUnique.boolValue = EditorGUILayout.Toggle("", sUnique.boolValue);
        GUILayout.Label("Is this item counted toward percentage completion?");
        sCounted.boolValue = EditorGUILayout.Toggle("", sCounted.boolValue);
        GUILayout.Space(15);

        GUILayout.Label("Difficulties this item appears in");
        sDiffs.GetArrayElementAtIndex(0).boolValue = EditorGUILayout.Toggle("Easy", sDiffs.GetArrayElementAtIndex(0).boolValue);
        sDiffs.GetArrayElementAtIndex(1).boolValue = EditorGUILayout.Toggle("Normal", sDiffs.GetArrayElementAtIndex(1).boolValue);
        sDiffs.GetArrayElementAtIndex(2).boolValue = EditorGUILayout.Toggle("Insane", sDiffs.GetArrayElementAtIndex(2).boolValue);
        GUILayout.Space(5);
        GUILayout.Label("Characters this item appears for");
        sChars.GetArrayElementAtIndex(0).boolValue = EditorGUILayout.Toggle("Snaily", sChars.GetArrayElementAtIndex(0).boolValue);
        sChars.GetArrayElementAtIndex(1).boolValue = EditorGUILayout.Toggle("Sluggy", sChars.GetArrayElementAtIndex(1).boolValue);
        sChars.GetArrayElementAtIndex(2).boolValue = EditorGUILayout.Toggle("Upside", sChars.GetArrayElementAtIndex(2).boolValue);
        sChars.GetArrayElementAtIndex(3).boolValue = EditorGUILayout.Toggle("Leggy", sChars.GetArrayElementAtIndex(3).boolValue);
        sChars.GetArrayElementAtIndex(4).boolValue = EditorGUILayout.Toggle("Blobby", sChars.GetArrayElementAtIndex(4).boolValue);
        sChars.GetArrayElementAtIndex(5).boolValue = EditorGUILayout.Toggle("Leechy", sChars.GetArrayElementAtIndex(5).boolValue);

        serializedObject.ApplyModifiedProperties();
    }
}
