using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[CreateAssetMenu(fileName = "Sound Library", menuName = "Scriptable Objects/Sound Library", order = 1)]
public class SoundLibrary : ScriptableObject
{
    public AudioClip[] library;

    public Dictionary<string, int> soundDict = new Dictionary<string, int>();

    public string[] referenceList = new string[]
    {
        "AngelJump",
        "BossHPBleep",
        "Cannon",
        "CheatSkyfish",
        "Chirp",
        "Death",
        "Dialogue0",
        "Dialogue1",
        "Dialogue2",
        "Dialogue3",
        "Dialogue4",
        "DoorClose",
        "DoorOpen",
        "EatGrass",
        "EatHealthOrb",
        "EatPowerGrass",
        "EnemyKilled1",
        "EnemyKilled2",
        "EnemyKilled3",
        "Explode1",
        "Explode2",
        "Explode3",
        "Explode4",
        "GrassGrow",
        "Hurt",
        "Jump",
        "MenuBeep1",
        "MenuBeep2",
        "Parry",
        "Ping",
        "Save",
        "Shell",
        "ShockCharge",
        "ShockLaunch",
        "ShotBoomerang",
        "ShotBoomerangDev",
        "ShotEnemyDonut",
        "ShotEnemyGigaWave",
        "ShotEnemyLaser",
        "ShotHit",
        "ShotPeashooter",
        "ShotPeashooterDev",
        "ShotRainbow",
        "ShotRainbowDev",
        "Snelk",
        "Splash",
        "Stomp",
        "Thunder",
        "Transformation"
    };

    public void BuildDictionary()
    {
        if (soundDict.Count == 0)
        {
            for (int i = 0; i < referenceList.Length; i++)
                soundDict.Add(referenceList[i], i);
        }
    }

    public void BuildDefaultLibrary()
    {
        BuildDictionary();
        List<AudioClip> newLibrary = new List<AudioClip>();
        for (int i = 0; i < referenceList.Length; i++)
            newLibrary.Add(Resources.Load<AudioClip>("Sounds/Sfx/" + referenceList[i]));
        library = newLibrary.ToArray();
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
                        string fileName = file.Replace('\\', '/').Substring(folderPath.Length + 1, file.Length - folderPath.Length - 1).Split('.')[0];
                        if (InReferenceList(fileName))
                        {
                            PlayState.importJobs++;
                            PlayState.globalFunctions.WaitForImportJobCompletion();
                            PlayState.globalFunctions.LoadClip(file.Replace('\\', '/'), fileName, new Vector2(-1, -1));
                        }
                    }
                }
            }
        }
    }

    private bool InReferenceList(string input)
    {
        int index = 0;
        bool found = false;
        while (index < referenceList.Length && !found)
        {
            if (referenceList[index] == input)
                found = true;
            index++;
        }
        return found;
    }
}
