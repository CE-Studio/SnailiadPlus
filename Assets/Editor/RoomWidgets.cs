using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoomTrigger))]
public class RoomWidgets : Editor
{
    RoomTrigger room;
    Vector2 bottomLeftCorner;
    Vector2 roomSize;
    const int WIDTH = 5;

    private void OnEnable()
    {
        room = (RoomTrigger)target;
        bottomLeftCorner = new Vector2(room.gameObject.transform.position.x - (room.gameObject.GetComponent<BoxCollider2D>().size.x * 0.5f) - 0.25f,
            room.gameObject.transform.position.y - (room.gameObject.GetComponent<BoxCollider2D>().size.y * 0.5f) - 0.25f);
        BoxCollider2D box = room.gameObject.GetComponent<BoxCollider2D>();
        roomSize = new Vector2(box.size.x + 0.5f, box.size.y + 0.5f);
    }

    public void OnSceneGUI()
    {
        Handles.color = Color.cyan;
        if (room.waterLevel.Length != 0)
        {
            for (int i = 0; i < room.waterLevel.Length; i++)
            {
                if ((i != 0 && i == room.waterLevel.Length - 1) || (i == 0 && room.waterLevel.Length == 1))
                {
                    Handles.DrawLine(bottomLeftCorner + room.waterLevel[i], bottomLeftCorner + new Vector2(roomSize.x, room.waterLevel[i].y), WIDTH);
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
