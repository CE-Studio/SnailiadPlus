using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "Text library", menuName = "Scriptable Objects/Text Library", order = 1)]
public class TextLibrary : ScriptableObject
{
    public PlayState.TextDict[] library;

    public struct TextCollective
    {
        public PlayState.TextDict[] textArray;
    }
    public void BuildDefaultLibrary()
    {
        TextAsset textJson = Resources.Load<TextAsset>("Text");
        TextCollective newDict = JsonUtility.FromJson<TextCollective>(textJson.text);
        library = newDict.textArray;
    }

    public void BuildLibrary(string folderPath = null)
    {
        if (folderPath != null && File.Exists(folderPath))
        {
            string fileText = File.ReadAllText(folderPath);
            TextCollective newDict = JsonUtility.FromJson<TextCollective>(fileText);
            library = newDict.textArray;
        }
    }
}
