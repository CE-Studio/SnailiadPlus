using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sound Library", menuName = "Scriptable Objects/Sound Library", order = 1)]
public class SoundLibrary : ScriptableObject
{
    public AudioClip[] library;

    public Dictionary<string, int> soundDict = new Dictionary<string, int>();

    public string[] referenceList = new string[]
    {
        "Death",
        "Dialogue0",
        "Dialogue1",
        "Dialogue2",
        "Dialogue3",
        "DoorClose",
        "DoorOpen",
        "EatGrass",
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
        "Ping",
        "Save",
        "Shell",
        "ShotBoomerang",
        "ShotRainbow",
        "Splash"
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
}
