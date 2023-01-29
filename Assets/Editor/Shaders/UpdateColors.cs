using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Assets.Scripts.Cam.Effects.RetroPixelMax))]
public class UpdateColors : Editor
{
    Assets.Scripts.Cam.Effects.RetroPixelMax script;

    private void OnEnable()
    {
        script = (Assets.Scripts.Cam.Effects.RetroPixelMax)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Update colors"))
        {
            script.colors = script.palette.texture.GetPixels();
        }
    }
}
