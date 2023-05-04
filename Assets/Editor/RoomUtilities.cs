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

    RoomTrigger room;
    Vector2 bosPos;
    Vector2 boxSize;

    SerializedProperty sID;
    SerializedProperty sSubID;
    SerializedProperty sParFG2;
    SerializedProperty sParFG1;
    SerializedProperty sParBG1;
    SerializedProperty sParBG2;
    SerializedProperty sOffFG2;
    SerializedProperty sOffFG1;
    SerializedProperty sOffBG1;
    SerializedProperty sOffBG2;

    public void OnEnable()
    {
        room = (RoomTrigger)target;

        sID = serializedObject.FindProperty("areaID");
        sSubID = serializedObject.FindProperty("areaSubzone");
        sParFG2 = serializedObject.FindProperty("parallaxForeground2Modifier");
        sParFG1 = serializedObject.FindProperty("parallaxForeground1Modifier");
        sParBG1 = serializedObject.FindProperty("parallaxBackgroundModifier");
        sParBG2 = serializedObject.FindProperty("parallaxSkyModifier");
        sOffFG2 = serializedObject.FindProperty("offsetForeground2");
        sOffFG1 = serializedObject.FindProperty("offsetForeground1");
        sOffBG1 = serializedObject.FindProperty("offsetBackground");
        sOffBG2 = serializedObject.FindProperty("offsetSky");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Vector2 boxPos = room.transform.position;
        Vector2 boxSize = room.GetComponent<BoxCollider2D>().size;

        sID.intValue = EditorGUILayout.IntField("Area ID", sID.intValue);
        sSubID.intValue = EditorGUILayout.IntField("Subzone ID", sSubID.intValue);
        GUILayout.Space(15);

        if (GUILayout.Button("Snap position to nearest tile"))
        {
            Undo.RecordObject(target, "Move Room");
            boxPos = new Vector2(Mathf.Floor(boxPos.x) + 0.5f, Mathf.Floor(boxPos.y) + 0.5f);
        }
        if (GUILayout.Button("Snap position to room grid"))
        {
            Undo.RecordObject(target, "Move Room");
            boxPos = new Vector2((Mathf.Round(boxPos.x / 13) * 13) + 0.5f, (Mathf.Round(boxPos.y / 8) * 8) + 0.5f);
        }
        if (GUILayout.Button("Snap all children to tile grid"))
        {
            Undo.RecordObject(target, "Move Room Assets");
            foreach (Transform obj in room.transform)
            {
                obj.position = new Vector2(Mathf.Round(obj.position.x * 4) * 0.25f, Mathf.Round(obj.position.y * 4) * 0.25f);
            }
        }
        GUILayout.Space(15);

        size = EditorGUILayout.Vector2Field("Size parameters", size);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Set size in screens"))
        {
            Undo.RecordObject(target, "Resize Room");
            boxSize = new Vector2(
                Mathf.RoundToInt(size.x * 25 + (1 * (Mathf.RoundToInt(size.x) - 1))) - 0.5f,
                Mathf.RoundToInt(size.y * 15 + (1 * (Mathf.RoundToInt(size.y) - 1))) - 0.5f
                );
        }
        if (GUILayout.Button("Set size in tiles"))
        {
            Undo.RecordObject(target, "Resize Room");
            boxSize = new Vector2(Mathf.RoundToInt(size.x) - 0.5f, Mathf.RoundToInt(size.y) - 0.5f);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.Label("Fine tile count tweaking");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+1 X"))
        {
            Undo.RecordObject(target, "Resize Room");
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x + 1, room.gameObject.GetComponent<BoxCollider2D>().size.y);
        }
        if (GUILayout.Button("+5 X"))
        {
            Undo.RecordObject(target, "Resize Room");
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x + 5, room.gameObject.GetComponent<BoxCollider2D>().size.y);
        }
        if (GUILayout.Button("+10 X"))
        {
            Undo.RecordObject(target, "Resize Room");
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x + 10, room.gameObject.GetComponent<BoxCollider2D>().size.y);
        }
        GUILayout.Space(3);
        if (GUILayout.Button("+1 Y"))
        {
            Undo.RecordObject(target, "Resize Room");
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x, room.gameObject.GetComponent<BoxCollider2D>().size.y + 1);
        }
        if (GUILayout.Button("+5 Y"))
        {
            Undo.RecordObject(target, "Resize Room");
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x, room.gameObject.GetComponent<BoxCollider2D>().size.y + 5);
        }
        if (GUILayout.Button("+10 Y"))
        {
            Undo.RecordObject(target, "Resize Room");
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x, room.gameObject.GetComponent<BoxCollider2D>().size.y + 10);
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("-1 X"))
        {
            Undo.RecordObject(target, "Resize Room");
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x - 1, room.gameObject.GetComponent<BoxCollider2D>().size.y);
        }
        if (GUILayout.Button("-5 X"))
        {
            Undo.RecordObject(target, "Resize Room");
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x - 5, room.gameObject.GetComponent<BoxCollider2D>().size.y);
        }
        if (GUILayout.Button("-10 X"))
        {
            Undo.RecordObject(target, "Resize Room");
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x - 10, room.gameObject.GetComponent<BoxCollider2D>().size.y);
        }
        GUILayout.Space(3);
        if (GUILayout.Button("-1 Y"))
        {
            Undo.RecordObject(target, "Resize Room");
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x, room.gameObject.GetComponent<BoxCollider2D>().size.y - 1);
        }
        if (GUILayout.Button("-5 Y"))
        {
            Undo.RecordObject(target, "Resize Room");
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x, room.gameObject.GetComponent<BoxCollider2D>().size.y - 5);
        }
        if (GUILayout.Button("-10 Y"))
        {
            Undo.RecordObject(target, "Resize Room");
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x, room.gameObject.GetComponent<BoxCollider2D>().size.y - 10);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(3);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+26 X (screen width)"))
        {
            Undo.RecordObject(target, "Resize Room");
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x + 26, room.gameObject.GetComponent<BoxCollider2D>().size.y);
        }
        if (GUILayout.Button("+16 Y (screen height)"))
        {
            Undo.RecordObject(target, "Resize Room");
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x, room.gameObject.GetComponent<BoxCollider2D>().size.y + 16);
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("-26 X (screen width)"))
        {
            Undo.RecordObject(target, "Resize Room");
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x - 26, room.gameObject.GetComponent<BoxCollider2D>().size.y);
        }
        if (GUILayout.Button("-16 Y (screen height)"))
        {
            Undo.RecordObject(target, "Resize Room");
            boxSize = new Vector2(room.gameObject.GetComponent<BoxCollider2D>().size.x, room.gameObject.GetComponent<BoxCollider2D>().size.y - 16);
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(8);
        if (GUILayout.Button("Reset size to one screen"))
        {
            Undo.RecordObject(target, "Resize Room");
            boxSize = new Vector2(24.5f, 14.5f);
        }
        GUILayout.Space(15);

        if (room.GetComponent<BoxCollider2D>().size != boxSize)
        {
            Undo.RecordObject(room.GetComponent<BoxCollider2D>(), "Resize Room");
            room.GetComponent<BoxCollider2D>().size = boxSize;
            EditorUtility.SetDirty(target);
        }
        if ((Vector2)room.transform.position != boxPos)
        {
            Undo.RecordObject(room.transform, "Move Room");
            room.transform.position = boxPos;
            EditorUtility.SetDirty(target);
        }

        GUILayout.Label("Parallax modifiers");
        sParFG2.vector2Value = EditorGUILayout.Vector2Field("Foreground 2", sParFG2.vector2Value);
        sOffFG2.vector2Value = EditorGUILayout.Vector2Field("        Offset", sOffFG2.vector2Value);
        sParFG1.vector2Value = EditorGUILayout.Vector2Field("Foreground 1", sParFG1.vector2Value);
        sOffFG1.vector2Value = EditorGUILayout.Vector2Field("        Offset", sOffFG1.vector2Value);
        sParBG1.vector2Value = EditorGUILayout.Vector2Field("Background", sParBG1.vector2Value);
        sOffBG1.vector2Value = EditorGUILayout.Vector2Field("        Offset", sOffBG1.vector2Value);
        sParBG2.vector2Value = EditorGUILayout.Vector2Field("Sky", sParBG2.vector2Value);
        sOffBG2.vector2Value = EditorGUILayout.Vector2Field("        Offset", sOffBG2.vector2Value);
        GUILayout.Space(15);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("waterLevel"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("environmentalEffects"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("roomCommands"), true);

        serializedObject.ApplyModifiedProperties();
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
