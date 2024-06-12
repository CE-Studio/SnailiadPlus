using System.Collections;
using System.Collections.Generic;
using System.IO;
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
            "AchievementJingle",
            "EndingIntro",
            "MultiItemJingle",
            "APItemJingle"
        },
        new string[]
        {
            "EndingCredits"
        },
        new string[]
        {
            "Snelk"
        },
        new string[]
        {
            "Boss1"
        },
        new string[]
        {
            "Boss2"
        },
        new string[]
        {
            "Boss3"
        },
        new string[]
        {
            "Boss4"
        },
        new string[]
        {
            "Boss4b"
        },
        new string[]
        {
            "SnailTown",
            "TestZone"
        },
        new string[]
        {
            "MareCarelia"
        },
        new string[]
        {
            "SpiralisSilere"
        },
        new string[]
        {
            "AmastridaAbyssus"
        },
        new string[]
        {
            "LuxLirata"
        },
        new string[]
        {
            "ShrineOfIris",
            "UnknownArea"
        },
        new string[]
        {
            "BossRush"
        }
    };
    public int areaThemeOffset = 8;

    public void BuildDefaultLibrary()
    {
        List<AudioClip[]> newLibrary = new();
        for (int i = 0; i < referenceList.Length; i++)
        {
            List<AudioClip> newList = new();
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

    public void BuildLibrary(string folderPath = null)
    {
        BuildDefaultLibrary();
        if (folderPath != null)
        {
            string[] tempArray = Directory.GetDirectories(folderPath);
            string[] directories = new string[tempArray.Length + 1];
            directories[0] = folderPath;
            for (int i = 0; i < tempArray.Length; i++)
                directories[i + 1] = tempArray[i].Replace('\\', '/');

            foreach (string directory in directories)
            {
                string[] spriteFiles = Directory.GetFiles(directory);
                foreach (string file in spriteFiles)
                {
                    if (file.Substring(file.Length - 3, 3).ToLower() == "ogg")
                    {
                        string[] fileParts = file.Replace('\\', '/').Split('/');
                        string fileName = fileParts[fileParts.Length - 1].Split('.')[0];
                        if (InReferenceList(fileName))
                        {
                            PlayState.importJobs++;
                            PlayState.globalFunctions.WaitForImportJobCompletion(true);
                            PlayState.globalFunctions.LoadClip(file.Replace('\\', '/'), fileName, GetPosInList(fileName));
                        }
                    }
                }
            }
        }
    }

    private bool InReferenceList(string input)
    {
        Vector2 index = Vector2.zero;
        bool found = false;
        while (index.x < referenceList.Length && !found)
        {
            if (referenceList[(int)index.x][(int)index.y] == input)
                found = true;
            index.y++;
            if (index.y >= referenceList[(int)index.x].Length)
            {
                index.y = 0;
                index.x++;
            }    
        }
        return found;
    }

    private Vector2 GetPosInList(string input)
    {
        Vector2 index = Vector2.zero;
        bool found = false;
        while (index.x < referenceList.Length && !found)
        {
            while (index.y < referenceList[(int)index.x].Length && !found)
            {
                if (referenceList[(int)index.x][(int)index.y] == input)
                    found = true;
                else
                    index.y++;
            }
            if (!found)
            {
                index.y = 0;
                index.x++;
            }
        }
        if (found)
            return index;
        else
            return new Vector2(-1, -1);
    }
}
