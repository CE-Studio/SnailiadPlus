using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayState
{
    public static string gameState = "Game"; // Can be "Game", "Pause", or "Dialogue" as of now
    
    public static AudioSource mus = GameObject.Find("View/Music").GetComponent<AudioSource>();
    public static int musicVol = 1;
    public static float playbackTime;
    public static string area;
    public static AudioClip areaMus;

    public static AudioClip snailTown = (AudioClip)Resources.Load("Sounds/Music/SnailTown");
    public static AudioClip majorItemJingle = (AudioClip)Resources.Load("Sounds/Music/MajorItemJingle");

    public static GameObject cam = GameObject.Find("View");

    public static bool hasRainbowWave = false;

    public static void GetNewRoom(string intendedArea)
    {
        area = intendedArea;
        switch (intendedArea)
        {
            case "Test Zone":
                mus.clip = snailTown;
                break;
            default:
                mus.clip = snailTown;
                break;
        }
        mus.Play();
    }

    public static void RunItemPopup(string item)
    {
        playbackTime = mus.time;
        mus.Stop();
        areaMus = mus.clip;
        mus.PlayOneShot(majorItemJingle);
        List<string> text = new List<string>();
        switch (item)
        {
            case "Rainbow Wave":
                text.Add("Rainbow Wave acquired!!");
                //text.Add("Your strongest weapon, reduced\nto a peashooter by Iris\' need\nto sustain herself");
                //text.Add("Hang on... don\'t you already\nhave this?  Well, uh... welcome\nto the testing zone!!  _@_V");
                text.Add("There's other text here, but it\ncontains some spoilers that I\nwanted to hide. Sorry!!");
                break;
        }
        gameState = "Dialogue";
        cam.transform.Find("Dialogue Box").GetComponent<DialogueBox>().RunBox(1, 0, text);
    }

    public static void OpenDialogue(int type, int speaker, List<string> text)
    {
        cam.transform.Find("Dialogue Box").GetComponent<DialogueBox>().RunBox(type, speaker, text);
    }

    public static void CloseDialogue()
    {
        cam.transform.Find("Dialogue Box").GetComponent<DialogueBox>().CloseBox();
    }
}
