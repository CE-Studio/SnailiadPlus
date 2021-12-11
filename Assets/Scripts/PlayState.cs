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
    public static bool colorblindMode = true;
    public static bool quickDeathTransition = false;
    public static bool armorPingPlayedThisFrame = false;
    public static bool explodePlayedThisFrame = false;
    public static float parallaxFg2Mod = 0;
    public static float parallaxFg1Mod = 0;
    public static float parallaxBgMod = 0;
    public static float parallaxSkyMod = 0;
    public static int thisExplosionID = 0;

    public static AudioClip snailTown = (AudioClip)Resources.Load("Sounds/Music/SnailTown");
    public static AudioClip majorItemJingle = (AudioClip)Resources.Load("Sounds/Music/MajorItemJingle");

    public static GameObject player = GameObject.Find("Player");
    public static GameObject cam = GameObject.Find("View");
    public static GameObject screenCover = GameObject.Find("View/Cover");
    public static GameObject fg2Layer = GameObject.Find("Grid/Foreground 2");
    public static GameObject fg1Layer = GameObject.Find("Grid/Foreground");
    public static GameObject bgLayer = GameObject.Find("Grid/Background");
    public static GameObject skyLayer = GameObject.Find("Grid/Sky");
    public static GameObject minimap = GameObject.Find("View/Minimap Panel/Minimap");
    public static GameObject explosionPool = GameObject.Find("Explosion Pool");

    public static bool paralyzed = false;
    public static bool isArmed = false;
    public static bool hasRainbowWave = false;
    public static bool hasGravitySnail = false;

    public static Vector2 camCenter;
    public static Vector2 camBoundaryBuffers;

    public static Vector2 respawnCoords = new Vector2(-7, 0);
    public static Scene respawnScene = SceneManager.GetActiveScene();

    public static TextMesh fpsText = GameObject.Find("View/FPS Text").GetComponent<TextMesh>();
    public static TextMesh fpsShadow = GameObject.Find("View/FPS Shadow").GetComponent<TextMesh>();

    public static int[] defaultMinimapState = new int []
    {
        0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    };

    public static int[] itemCollection = new int[]
    {
        0,  // Peashooter
        0,  // Boomerang
        0,  // Rainbow Wave
        0,  // Devastator
        0,  // High Jump          Wall Grab
        0,  // Shell Shield       Shelmet
        0,  // Rapid Fire         Backfire
        0,  // Ice Snail
        0,  // Gravity Snail      Magnetic Foot      Corkscrew Jump       Angel Jump
        0,  // Full-Metal Snail
        0,  // Gravity Shock
        0,  // Super Secret Boomerang
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // Heart Containers
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0  // Helix Fragments
    };
    public const byte OFFSET_HEARTS = 12;
    public const byte OFFSET_FRAGMENTS = 23;

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

    public static void OpenDialogue(int type, int speaker, List<string> text, List<Color32> colors = null, List<int> stateList = null, bool facingLeft = false)
    {
        cam.transform.Find("Dialogue Box").GetComponent<DialogueBox>().RunBox(type, speaker, text, colors, stateList, facingLeft);
    }

    public static void CloseDialogue()
    {
        cam.transform.Find("Dialogue Box").GetComponent<DialogueBox>().CloseBox();
    }

    public static void ScreenFlash(string type, int red = 0, int green = 0, int blue = 0, int alpha = 0)
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
            case "Death Transition":
                player.GetComponent<Player>().ExecuteCoverCommand(type);
                break;
        }
    }

    public static void FlashItemText(string item)
    {
        player.GetComponent<Player>().FlashItemText(item);
    }

    public static void FlashCollectionText()
    {
        player.GetComponent<Player>().FlashCollectionText();
    }

    public static void FlashSaveText()
    {
        player.GetComponent<Player>().FlashSaveText();
    }

    public static void RequestExplosion(int size, Vector2 position)
    {
        if (!explosionPool.transform.GetChild(thisExplosionID).GetComponent<Explosion>().isActive)
        {
            explosionPool.transform.GetChild(thisExplosionID).GetComponent<Explosion>().isActive = true;
            explosionPool.transform.GetChild(thisExplosionID).position = position;
            switch (size)
            {
                case 1:
                    explosionPool.transform.GetChild(thisExplosionID).GetComponent<Animator>().Play("Explosion tiny", 0, 0);
                    break;
                case 2:
                    explosionPool.transform.GetChild(thisExplosionID).GetComponent<Animator>().Play("Explosion small", 0, 0);
                    break;
                case 3:
                    explosionPool.transform.GetChild(thisExplosionID).GetComponent<Animator>().Play("Explosion big", 0, 0);
                    break;
                case 4:
                    explosionPool.transform.GetChild(thisExplosionID).GetComponent<Animator>().Play("Explosion huge", 0, 0);
                    break;
            }
            thisExplosionID++;
            if (thisExplosionID >= explosionPool.transform.childCount)
                thisExplosionID = 0;
        }
    }

    public static bool CheckForItem(string itemName)
    {
        return itemCollection[TranslateItemNameToID(itemName)] == 1;
    }

    public static void AddItem(string itemName)
    {
        itemCollection[TranslateItemNameToID(itemName)] = 1;
    }

    private static byte TranslateItemNameToID(string itemName)
    {
        byte id = 0;
        switch (itemName)
        {
            case "Peashooter":
                id = 0;
                break;
            case "Boomerang":
                id = 1;
                break;
            case "Rainbow Wave":
                id = 2;
                break;
            case "Devastator":
                id = 3;
                break;
            case "High Jump":
            case "Wall Grab":
                id = 4;
                break;
            case "Shell Shield":
            case "Shelmet":
                id = 5;
                break;
            case "Rapid Fire":
            case "Backfire":
                id = 6;
                break;
            case "Ice Snail":
                id = 7;
                break;
            case "Gravity Snail":
            case "Magnetic Foot":
            case "Corkscrew Jump":
            case "Angel Jump":
                id = 8;
                break;
            case "Full-Metal Snail":
                id = 9;
                break;
            case "Gravity Shock":
                id = 10;
                break;
            case "Super Secret Boomerang":
                id = 11;
                break;
        }
        return id;
    }
}
