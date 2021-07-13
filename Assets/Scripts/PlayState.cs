using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayState
{
    public static string gameState = "Game"; // Can be "Game", "Pause", or "Dialogue" as of now
    
    public static AudioSource mus = GameObject.Find("View/Music").GetComponent<AudioSource>();
    public static int musicVol = 1;
    public static float playbackTime;
    public static string area;
    public static AudioClip areaMus;
    public static bool colorblindMode = false;

    public static AudioClip snailTown = (AudioClip)Resources.Load("Sounds/Music/SnailTown");
    public static AudioClip majorItemJingle = (AudioClip)Resources.Load("Sounds/Music/MajorItemJingle");

    public static GameObject player = GameObject.Find("Player");
    public static GameObject cam = GameObject.Find("View");
    public static GameObject screenCover = GameObject.Find("View/Cover");

    public static bool paralyzed = false;
    public static bool isArmed = false;
    public static bool hasRainbowWave = false;

    public static Vector2 camCenter;
    public static Vector2 camBoundaryBuffers;

    public static Vector2 respawnCoords = new Vector2(-7, 0);
    public static Scene respawnScene = SceneManager.GetActiveScene();

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
        List<Color32> colors = new List<Color32>();
        switch (item)
        {
            case "Rainbow Wave":
                text.Add("Rainbow Wave acquired!!");
                //text.Add("Your strongest weapon, reduced\nto a peashooter by Iris\' need\nto sustain herself");
                //text.Add("Hang on... don\'t you already\nhave this?  Well, uh... welcome\nto the testing zone!!  _@_V");
                text.Add("There's other text here, but it\ncontains some spoilers that I\nwanted to hide. Sorry!!");
                break;
        }
        for (int i = 0; i < text.Count; i++)
        {
            colors.Add(new Color32(0, 0, 0, 0));
            colors.Add(new Color32(0, 0, 0, 0));
            colors.Add(new Color32(0, 0, 0, 0));
        }
        gameState = "Dialogue";
        cam.transform.Find("Dialogue Box").GetComponent<DialogueBox>().RunBox(1, 0, text, colors);
    }

    public static void OpenDialogue(int type, int speaker, List<string> text, List<Color32> colors)
    {
        cam.transform.Find("Dialogue Box").GetComponent<DialogueBox>().RunBox(type, speaker, text, colors);
    }

    public static void CloseDialogue()
    {
        cam.transform.Find("Dialogue Box").GetComponent<DialogueBox>().CloseBox();
    }

    public static void ScreenFlash(string type, int red, int green, int blue, int alpha)
    {
        switch (type)
        {
            default:
                screenCover.GetComponent<SpriteRenderer>().color = new Color32((byte)red, (byte)green, (byte)blue, (byte)alpha);
                break;
            case "Room Transition":
                screenCover.GetComponent<SpriteRenderer>().color = new Color32(0, 0, 0, 200);
                player.GetComponent<Player>().ExecuteCoverCommand(type);
                break;
        }
    }
}
