using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FakeRoomBorder))]
public class FakeBorderHighlighter : Editor
{
    FakeRoomBorder borderScript;
    GameObject borderObject;

    SerializedProperty sDir;
    SerializedProperty sWork;

    private const float WIDTH = 5;

    void OnEnable()
    {
        borderScript = (FakeRoomBorder)target;
        borderObject = borderScript.gameObject;

        sDir = serializedObject.FindProperty("direction");
        sWork = serializedObject.FindProperty("workingDirections");
    }

    void OnSceneGUI()
    {
        if (borderObject.transform.parent.CompareTag("RoomTrigger"))
        {
            Handles.color = Color.yellow;
            if (sDir.boolValue)
            {
                Handles.DrawLine(
                    new Vector2(borderObject.transform.position.x, borderObject.transform.position.y + (borderObject.transform.parent.GetComponent<BoxCollider2D>().size.y * 0.5f)),
                    new Vector2(borderObject.transform.position.x, borderObject.transform.position.y - (borderObject.transform.parent.GetComponent<BoxCollider2D>().size.y * 0.5f)),
                    WIDTH
                    );
                if (sWork.intValue >= 2)
                {
                    Handles.DrawLine(
                        new Vector2(borderObject.transform.position.x + 2.5f, borderObject.transform.position.y),
                        new Vector2(borderObject.transform.position.x + 1.25f, borderObject.transform.position.y + 3),
                        WIDTH
                        );
                    Handles.DrawLine(
                        new Vector2(borderObject.transform.position.x + 2.5f, borderObject.transform.position.y),
                        new Vector2(borderObject.transform.position.x + 1.25f, borderObject.transform.position.y - 3),
                        WIDTH
                        );
                }
                if (sWork.intValue == 1 || sWork.intValue == 3)
                {
                    Handles.DrawLine(
                        new Vector2(borderObject.transform.position.x - 2.5f, borderObject.transform.position.y),
                        new Vector2(borderObject.transform.position.x - 1.25f, borderObject.transform.position.y + 3),
                        WIDTH
                        );
                    Handles.DrawLine(
                        new Vector2(borderObject.transform.position.x - 2.5f, borderObject.transform.position.y),
                        new Vector2(borderObject.transform.position.x - 1.25f, borderObject.transform.position.y - 3),
                        WIDTH
                        );
                }
            }
            else
            {
                Handles.DrawLine(
                    new Vector2(borderObject.transform.position.x + (borderObject.transform.parent.GetComponent<BoxCollider2D>().size.x * 0.5f), borderObject.transform.position.y),
                    new Vector2(borderObject.transform.position.x - (borderObject.transform.parent.GetComponent<BoxCollider2D>().size.x * 0.5f), borderObject.transform.position.y),
                    WIDTH
                    );
                if (sWork.intValue >= 2)
                {
                    Handles.DrawLine(
                        new Vector2(borderObject.transform.position.x, borderObject.transform.position.y + 2.5f),
                        new Vector2(borderObject.transform.position.x + 3, borderObject.transform.position.y + 1.25f),
                        WIDTH
                        );
                    Handles.DrawLine(
                        new Vector2(borderObject.transform.position.x, borderObject.transform.position.y + 2.5f),
                        new Vector2(borderObject.transform.position.x - 3, borderObject.transform.position.y + 1.25f),
                        WIDTH
                        );
                }
                if (sWork.intValue == 1 || sWork.intValue == 3)
                {
                    Handles.DrawLine(
                        new Vector2(borderObject.transform.position.x, borderObject.transform.position.y - 2.5f),
                        new Vector2(borderObject.transform.position.x + 3, borderObject.transform.position.y - 1.25f),
                        WIDTH
                        );
                    Handles.DrawLine(
                        new Vector2(borderObject.transform.position.x, borderObject.transform.position.y - 2.5f),
                        new Vector2(borderObject.transform.position.x - 3, borderObject.transform.position.y - 1.25f),
                        WIDTH
                        );
                }
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        sDir.boolValue = EditorGUILayout.Popup("Direction", sDir.boolValue ? 1 : 0, new string[] { "Stop camera vertically", "Stop camera horizontally" }) == 1;
        sWork.intValue = EditorGUILayout.Popup("Function from", sWork.intValue - 1,
            new string[] { sDir.boolValue ? "Left of" : "Below", sDir.boolValue ? "Right of" : "Above", "Both directions" }) + 1;

        serializedObject.ApplyModifiedProperties();
    }
}
