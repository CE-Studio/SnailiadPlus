using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Music library", menuName = "Scriptable Objects/Music Library", order = 1)]
public class MusicLibrary : ScriptableObject
{
    public AudioClip[][] library;

    public string[][] referenceList = new string[][]
    {
        new string[]
        {
            "TitleSong",
            "MinorItemJingle",
            "MajorItemJingle",
            "AchievementJingle"
        },
        new string[]
        {
            "SnailTown",
            "TestZone"
        },
        new string[]
        {
            "MareCarelia"
        }
    };

    public void BuildDefaultLibrary()
    {
        List<AudioClip[]> newLibrary = new List<AudioClip[]>();
        for (int i = 0; i < referenceList.Length; i++)
        {
            List<AudioClip> newList = new List<AudioClip>();
            for (int j = 0; j < referenceList[i].Length; j++)
                newList.Add((AudioClip)Resources.Load("Sounds/Music/" + referenceList[i][j]));
            newLibrary.Add(newList.ToArray());
        }
        library = newLibrary.ToArray();
    }

    public void BuildDefaultOffsetLibrary()
    {
        TextAsset offsetJson = Resources.Load<TextAsset>("MusicLoopOffsets");
        PlayState.MusicOffsetLibrary newLibrary = JsonUtility.FromJson<PlayState.MusicOffsetLibrary>(offsetJson.text);
        PlayState.musicLoopOffsetLibrary = newLibrary.offsetArray;
    }

    public void BuildOffsetLibrary(string dataPath = null)
    {
        BuildDefaultOffsetLibrary();
        if (dataPath != null)
        {
            PlayState.LoadNewMusicOffsetLibrary(dataPath);
        }
    }
}
