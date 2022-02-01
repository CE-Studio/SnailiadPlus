using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneViewMouse))]
public class EditorRoomGrid : Editor
{
    void OnSceneGUI()
    {
        Vector3 mousePos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;

        Handles.DrawLine(
            new Vector2(Mathf.Floor(mousePos.x / 26) * 26 + 0.5f, Mathf.Floor(mousePos.y / 16) * 16 + 0.5f),
            new Vector2(Mathf.Floor(mousePos.x / 26) * 26 + 0.5f, Mathf.Ceil(mousePos.y / 16) * 16 + 0.5f),
            4
            );
        Handles.DrawLine(
            new Vector2(Mathf.Floor(mousePos.x / 26) * 26 + 0.5f, Mathf.Ceil(mousePos.y / 16) * 16 + 0.5f),
            new Vector2(Mathf.Ceil(mousePos.x / 26) * 26 + 0.5f, Mathf.Ceil(mousePos.y / 16) * 16 + 0.5f),
            4
            );
        Handles.DrawLine(
            new Vector2(Mathf.Ceil(mousePos.x / 26) * 26 + 0.5f, Mathf.Ceil(mousePos.y / 16) * 16 + 0.5f),
            new Vector2(Mathf.Ceil(mousePos.x / 26) * 26 + 0.5f, Mathf.Floor(mousePos.y / 16) * 16 + 0.5f),
            4
            );
        Handles.DrawLine(
            new Vector2(Mathf.Ceil(mousePos.x / 26) * 26 + 0.5f, Mathf.Floor(mousePos.y / 16) * 16 + 0.5f),
            new Vector2(Mathf.Floor(mousePos.x / 26) * 26 + 0.5f, Mathf.Floor(mousePos.y / 16) * 16 + 0.5f),
            4
            );
    }
}
