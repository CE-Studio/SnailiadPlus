using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPC))]
public class ColorNpc : Editor
{
    NPC script;
    Transform npcObject;
    List<SpriteRenderer> npcParts = new List<SpriteRenderer>();

    private void OnEnable()
    {
        script = (NPC)target;
        npcObject = script.transform;
        npcParts.Add(npcObject.GetComponent<SpriteRenderer>());
        npcParts.Add(npcObject.GetChild(0).GetComponent<SpriteRenderer>());
        npcParts.Add(npcObject.GetChild(1).GetComponent<SpriteRenderer>());
        npcParts.Add(npcObject.GetChild(2).GetComponent<SpriteRenderer>());
        npcParts.Add(npcObject.GetChild(3).GetComponent<SpriteRenderer>());
    }

    //public override void OnInspectorGUI()
    //{
    //    DrawDefaultInspector();
    //    npcParts[0].color = script.colorTable.GetPixel(0, script.ID + 1);
    //    npcParts[1].color = script.colorTable.GetPixel(1, script.ID + 1);
    //    npcParts[2].color = script.colorTable.GetPixel(2, script.ID + 1);
    //    npcParts[3].color = script.colorTable.GetPixel(3, script.ID + 1);
    //    npcParts[4].color = script.colorTable.GetPixel(4, script.ID + 1);
    //}
}
