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

    private void OnEnable()
    {
        script = (Item)target;
        sprite = script.GetComponent<SpriteRenderer>();
    }

    public override void OnInspectorGUI()
    {
        EditorUtility.SetDirty(target);
        script.itemID = EditorGUILayout.Popup("Item type: ", script.itemID == 24 ? 15 : script.itemID + 1, new string[]
        {
            "Select one",
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
        if (script.itemID == 14)
            script.itemID = 24;
        //GUILayout.Label("Item ID: " + script.itemID);
        if (script.itemID == 4)
            GUILayout.Label("Also corresponds to:\n- Wall Grab (Blobby)");
        if (script.itemID == 5)
            GUILayout.Label("Also corresponds to:\n- Shelmet (Blobby)");
        if (script.itemID == 6)
            GUILayout.Label("Also corresponds to:\n- Backfire (Leechy)");
        if (script.itemID == 8)
            GUILayout.Label("Also corresponds to:\n- Magnetic Foot (Upside)\n- Corkscrew Jump (Leggy)\n- Angel Jump (Blobby)");
        GUILayout.Space(15);

        GUILayout.Label("Is this item super unique?");
        script.isSuperUnique = EditorGUILayout.Toggle("", script.isSuperUnique);
        GUILayout.Label("Is this item counted toward percentage completion?");
        script.countedInPercentage = EditorGUILayout.Toggle("", script.countedInPercentage);
        GUILayout.Space(15);

        GUILayout.Label("Difficulties this item appears in");
        script.difficultiesPresentIn = new bool[]
        {
            EditorGUILayout.Toggle("Easy", script.difficultiesPresentIn[0]),
            EditorGUILayout.Toggle("Normal", script.difficultiesPresentIn[1]),
            EditorGUILayout.Toggle("Insane", script.difficultiesPresentIn[2])
        };
        GUILayout.Space(5);
        GUILayout.Label("Characters this item appears for");
        script.charactersPresentFor = new bool[]
        {
            EditorGUILayout.Toggle("Snaily", script.charactersPresentFor[0]),
            EditorGUILayout.Toggle("Sluggy", script.charactersPresentFor[1]),
            EditorGUILayout.Toggle("Upside", script.charactersPresentFor[2]),
            EditorGUILayout.Toggle("Leggy", script.charactersPresentFor[3]),
            EditorGUILayout.Toggle("Blobby", script.charactersPresentFor[4]),
            EditorGUILayout.Toggle("Leechy", script.charactersPresentFor[5])
        };
    }
}
