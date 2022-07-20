using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomTrigger))]
[CanEditMultipleObjects]
public class RoomUtilities : Editor
{
    Vector2 size = Vector2.zero;
    const int WIDTH = 5;

    public override void OnInspectorGUI()
    {
        EditorUtility.SetDirty(target);
        RoomTrigger room = (RoomTrigger)target;
        Vector2 boxPos = room.transform.position;
        Vector2 boxSize = room.GetComponent<BoxCollider2D>().size;
        serializedObject.Update();

        room.areaID = EditorGUILayout.IntField("Area ID: ", room.areaID);
        room.areaSubzone = EditorGUILayout.IntField("Subzone ID:", room.areaSubzone);
        GUILayout.Space(15);

        if (GUILayout.Button("Snap position to nearest tile"))
        {
            boxPos = new Vector2(Mathf.Floor(boxPos.x) + 0.5f, Mathf.Floor(boxPos.y) + 0.5f);
        }
        if (GUILayout.Button("Snap position to room grid"))
        {
            boxPos = new Vector2((Mathf.Round(boxPos.x / 13) * 13) + 0.5f, (Mathf.Round(boxPos.y / 8) * 8) + 0.5f);
        }
        if (GUILayout.Button("Snap all children to tile grid"))
        {
            foreach (Transform obj in room.transform)
            {
                obj.position = new Vector2(Mathf.Round(obj.position.x * 4) * 0.25f, Mathf.Round(obj.position.y * 4) * 0.25f);
            }
        }
        GUILayout.Space(15);

        size = EditorGUILayout.Vector2Field("Size parameters:", size);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Set size in screens"))
        {
            boxSize = new Vector2(
                Mathf.RoundToInt(size.x * 25 + (1 * (Mathf.RoundToInt(size.x) - 1))) - 0.5f,
                Mathf.RoundToInt(size.y * 15 + (1 * (Mathf.RoundToInt(size.y) - 1))) - 0.5f
                );
        }
        if (GUILayout.Button("Set size in tiles"))
        {
            boxSize = new Vector2(Mathf.RoundToInt(size.x) - 0.5f, Mathf.RoundToInt(size.y) - 0.5f);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.Label("Fine tile count tweaking");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+1 X"))
        {
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x + 1, room.gameObject.GetComponent<BoxCollider2D>().size.y);
        }
        if (GUILayout.Button("+5 X"))
        {
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x + 5, room.gameObject.GetComponent<BoxCollider2D>().size.y);
        }
        if (GUILayout.Button("+10 X"))
        {
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x + 10, room.gameObject.GetComponent<BoxCollider2D>().size.y);
        }
        GUILayout.Space(3);
        if (GUILayout.Button("+1 Y"))
        {
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x, room.gameObject.GetComponent<BoxCollider2D>().size.y + 1);
        }
        if (GUILayout.Button("+5 Y"))
        {
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x, room.gameObject.GetComponent<BoxCollider2D>().size.y + 5);
        }
        if (GUILayout.Button("+10 Y"))
        {
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x, room.gameObject.GetComponent<BoxCollider2D>().size.y + 10);
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("-1 X"))
        {
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x - 1, room.gameObject.GetComponent<BoxCollider2D>().size.y);
        }
        if (GUILayout.Button("-5 X"))
        {
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x - 5, room.gameObject.GetComponent<BoxCollider2D>().size.y);
        }
        if (GUILayout.Button("-10 X"))
        {
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x - 10, room.gameObject.GetComponent<BoxCollider2D>().size.y);
        }
        GUILayout.Space(3);
        if (GUILayout.Button("-1 Y"))
        {
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x, room.gameObject.GetComponent<BoxCollider2D>().size.y - 1);
        }
        if (GUILayout.Button("-5 Y"))
        {
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x, room.gameObject.GetComponent<BoxCollider2D>().size.y - 5);
        }
        if (GUILayout.Button("-10 Y"))
        {
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x, room.gameObject.GetComponent<BoxCollider2D>().size.y - 10);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(3);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+26 X (screen width)"))
        {
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x + 26, room.gameObject.GetComponent<BoxCollider2D>().size.y);
        }
        if (GUILayout.Button("+16 Y (screen height)"))
        {
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x, room.gameObject.GetComponent<BoxCollider2D>().size.y + 16);
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("-26 X (screen width)"))
        {
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x - 26, room.gameObject.GetComponent<BoxCollider2D>().size.y);
        }
        if (GUILayout.Button("-16 Y (screen height)"))
        {
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x, room.gameObject.GetComponent<BoxCollider2D>().size.y - 16);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(8);
        if (GUILayout.Button("Reset size to one screen"))
        {
            boxSize = new Vector2(24.5f, 14.5f);
        }
        GUILayout.Space(15);
        room.GetComponent<BoxCollider2D>().size = boxSize;
        room.transform.position = boxPos;

        GUILayout.Label("Parallax modifiers");
        room.parallaxForeground2Modifier = EditorGUILayout.Vector2Field("Foreground 2: ", room.parallaxForeground2Modifier);
        room.parallaxForeground1Modifier = EditorGUILayout.Vector2Field("Foreground 1: ", room.parallaxForeground1Modifier);
        room.parallaxBackgroundModifier = EditorGUILayout.Vector2Field("Background: ", room.parallaxBackgroundModifier);
        room.parallaxSkyModifier = EditorGUILayout.Vector2Field("Sky: ", room.parallaxSkyModifier);
        GUILayout.Space(15);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("waterLevel"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("environmentalEffects"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("roomCommands"), true);

        serializedObject.ApplyModifiedProperties();
        //base.OnInspectorGUI();
    }

    private void OnSceneGUI()
    {
        RoomTrigger room = (RoomTrigger)target;

        Handles.color = Color.white;
        Vector2 currentSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x * 0.5f + 0.25f,
            room.gameObject.GetComponent<BoxCollider2D>().size.y * 0.5f + 0.25f);
        Vector2 roomPos = room.transform.position;
        Handles.DrawLine(new Vector3(roomPos.x + currentSize.x, roomPos.y + currentSize.y, 0), new Vector3(roomPos.x + currentSize.x, roomPos.y - currentSize.y, 0), WIDTH);
        Handles.DrawLine(new Vector3(roomPos.x + currentSize.x, roomPos.y - currentSize.y, 0), new Vector3(roomPos.x - currentSize.x, roomPos.y - currentSize.y, 0), WIDTH);
        Handles.DrawLine(new Vector3(roomPos.x - currentSize.x, roomPos.y - currentSize.y, 0), new Vector3(roomPos.x - currentSize.x, roomPos.y + currentSize.y, 0), WIDTH);
        Handles.DrawLine(new Vector3(roomPos.x - currentSize.x, roomPos.y + currentSize.y, 0), new Vector3(roomPos.x + currentSize.x, roomPos.y + currentSize.y, 0), WIDTH);

        if (room.waterLevel.Length != 0)
        {
            Handles.color = Color.cyan;
            Vector2 bottomLeftCorner = new Vector2(room.gameObject.transform.position.x - (room.gameObject.GetComponent<BoxCollider2D>().size.x * 0.5f) - 0.25f,
                room.gameObject.transform.position.y - (room.gameObject.GetComponent<BoxCollider2D>().size.y * 0.5f) - 0.25f);
            for (int i = 0; i < room.waterLevel.Length; i++)
            {
                if ((i != 0 && i == room.waterLevel.Length - 1) || (i == 0 && room.waterLevel.Length == 1))
                {
                    Handles.DrawLine(bottomLeftCorner + room.waterLevel[i], bottomLeftCorner + new Vector2(currentSize.x * 2, room.waterLevel[i].y), WIDTH);
                    if (i != 0)
                        Handles.DrawLine(bottomLeftCorner + room.waterLevel[i], bottomLeftCorner + new Vector2(room.waterLevel[i].x, room.waterLevel[i - 1].y), WIDTH);
                }
                else
                {
                    Handles.DrawLine(bottomLeftCorner + room.waterLevel[i], bottomLeftCorner + new Vector2(room.waterLevel[i + 1].x, room.waterLevel[i].y), WIDTH);
                    if (i != 0)
                        Handles.DrawLine(bottomLeftCorner + room.waterLevel[i], bottomLeftCorner + new Vector2(room.waterLevel[i].x, room.waterLevel[i - 1].y), WIDTH);
                }
            }
        }
    }
}
