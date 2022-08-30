using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CutsceneController))]
[CanEditMultipleObjects]
public class CutsceneUtilities : Editor
{
    CutsceneController script;
    BoxCollider2D box;
    string validationOutput = "";

    readonly string[] GENERAL_COMMANDS = new string[] { "move", "paralyze", "unparalyze", "face", "flip", "show", "hide", "toggle", "animate", "dialogue",
        "fade", "cam", "if", "else", "endif", "loop", "endloop", "wait", "create", "particle", "shake", "sound", "music", "settile" };

    private void OnEnable()
    {
        script = (CutsceneController)target;
        box = script.GetComponent<BoxCollider2D>();
    }

    public override void OnInspectorGUI()
    {
        EditorUtility.SetDirty(target);
        box.size = EditorGUILayout.Vector2Field("Trigger size", box.size);
        GUILayout.Space(15);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("actors"), true);
        GUILayout.Label("The player is automatically counted as an actor");
        GUILayout.Space(15);

        GUILayout.Label("Cutscene script");
        script.sceneScript = EditorGUILayout.TextArea(script.sceneScript);
        GUILayout.Space(10);
        if (GUILayout.Button("Validate script"))
        {
            validationOutput = "This script is valid";
            int thisChar = 0;
            int thisLineNum = 0;
            string thisLine = "";
            string[] tokens;

            while (thisChar < script.sceneScript.Length)
            {
                if (script.sceneScript[thisChar] == '\n' || thisChar == script.sceneScript.Length - 1)
                {
                    thisLine.Trim().ToLower();
                    tokens = thisLine.Contains(" ") ? thisLine.Split(' ') : new string[] { thisLine };

                    if (thisLineNum == 0 && (tokens[0] == "with" || tokens[0] == "after"))
                    {
                        validationOutput = "First line must not start with \'with\' or \'after\'!";
                        break;
                    }

                    int tokenID = 0;
                    foreach (string token in tokens)
                    {


                        tokenID++;
                    }

                    thisLineNum++;
                    thisLine = "";
                }
                else
                {
                    thisLine += script.sceneScript[thisChar];
                }

                thisChar++;
            }

            Debug.Log("Cutscene script validation result: " + validationOutput);
        }
    }
}
