using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FakeBorderComponent))]
public class FakeBorderHighlighter : Editor
{
    FakeBorderComponent border;
    GameObject borderObject;
    FakeRoomBorder borderScript;

    void OnEnable()
    {
        border = (FakeBorderComponent)target;
        borderObject = border.gameObject;
        borderScript = borderObject.GetComponent<FakeRoomBorder>();
    }

    void OnSceneGUI()
    {
        if (borderObject.transform.parent.tag == "RoomTrigger")
        {
            Handles.color = Color.yellow;
            if (borderScript.direction)
            {
                Handles.DrawLine(
                    new Vector2(borderObject.transform.position.x, borderObject.transform.position.y + (borderObject.transform.parent.GetComponent<BoxCollider2D>().size.y * 0.5f)),
                    new Vector2(borderObject.transform.position.x, borderObject.transform.position.y - (borderObject.transform.parent.GetComponent<BoxCollider2D>().size.y * 0.5f)),
                    3
                    );
                if (borderScript.workingDirections >= 2)
                {
                    Handles.DrawLine(
                        new Vector2(borderObject.transform.position.x + 2.5f, borderObject.transform.position.y),
                        new Vector2(borderObject.transform.position.x + 1.25f, borderObject.transform.position.y + 3),
                        3
                        );
                    Handles.DrawLine(
                        new Vector2(borderObject.transform.position.x + 2.5f, borderObject.transform.position.y),
                        new Vector2(borderObject.transform.position.x + 1.25f, borderObject.transform.position.y - 3),
                        3
                        );
                }
                if (borderScript.workingDirections == 1 || borderScript.workingDirections == 3)
                {
                    Handles.DrawLine(
                        new Vector2(borderObject.transform.position.x - 2.5f, borderObject.transform.position.y),
                        new Vector2(borderObject.transform.position.x - 1.25f, borderObject.transform.position.y + 3),
                        3
                        );
                    Handles.DrawLine(
                        new Vector2(borderObject.transform.position.x - 2.5f, borderObject.transform.position.y),
                        new Vector2(borderObject.transform.position.x - 1.25f, borderObject.transform.position.y - 3),
                        3
                        );
                }
            }
            else
            {
                Handles.DrawLine(
                    new Vector2(borderObject.transform.position.x + (borderObject.transform.parent.GetComponent<BoxCollider2D>().size.x * 0.5f), borderObject.transform.position.y),
                    new Vector2(borderObject.transform.position.x - (borderObject.transform.parent.GetComponent<BoxCollider2D>().size.x * 0.5f), borderObject.transform.position.y),
                    3
                    );
                if (borderScript.workingDirections >= 2)
                {
                    Handles.DrawLine(
                        new Vector2(borderObject.transform.position.x, borderObject.transform.position.y + 2.5f),
                        new Vector2(borderObject.transform.position.x + 3, borderObject.transform.position.y + 1.25f),
                        3
                        );
                    Handles.DrawLine(
                        new Vector2(borderObject.transform.position.x, borderObject.transform.position.y - 2.5f),
                        new Vector2(borderObject.transform.position.x + 3, borderObject.transform.position.y - 1.25f),
                        3
                        );
                }
                if (borderScript.workingDirections == 1 || borderScript.workingDirections == 3)
                {
                    Handles.DrawLine(
                        new Vector2(borderObject.transform.position.x, borderObject.transform.position.y + 2.5f),
                        new Vector2(borderObject.transform.position.x - 3, borderObject.transform.position.y + 1.25f),
                        3
                        );
                    Handles.DrawLine(
                        new Vector2(borderObject.transform.position.x, borderObject.transform.position.y - 2.5f),
                        new Vector2(borderObject.transform.position.x - 3, borderObject.transform.position.y - 1.25f),
                        3
                        );
                }
            }
        }
    }
}
