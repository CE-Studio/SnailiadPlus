using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrintMapPointer))]
public class PrintDefaultMapState : Editor
{
    public Sprite map;

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Print current map state"))
        {
            string output = "";
            for (int y = (int)PlayState.WORLD_SIZE.y - 1; y >= 0; y--)
            {
                for (int x = 0; x < PlayState.WORLD_SIZE.x; x++)
                {
                    Color thisTile = map.texture.GetPixel(x * 8 + 4, y * 8 + 4);
                    if (thisTile.a != 1)
                        output += "-1";
                    else if (thisTile.r == 0 && thisTile.g == 0 && thisTile.b == 0)
                        output += " 2";
                    else
                        output += " 0";
                    if (!(x == PlayState.WORLD_SIZE.x - 1 && y == 0))
                    {
                        output += ",";
                        if (x != PlayState.WORLD_SIZE.x - 1)
                            output += " ";
                    }
                    if (x == PlayState.WORLD_SIZE.x - 1)
                        output += "\n";
                }
            }
            Debug.Log(output);
        }
    }
}
