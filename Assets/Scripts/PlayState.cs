using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PlayState
{
    public static string gameState = "Menu"; // Can be "Game", "Menu", "Pause", "Map", "Debug", or "Dialogue" as of now

    public static bool isMenuOpen = false;

    public static bool noclipMode = false;

    [Serializable]
    public struct AnimationData
    {
        public string name;
        public string spriteName;
        public float framerate;
        public int[] frames;
        public bool loop;
        public int loopStartFrame;
        public bool randomizeStartFrame;
    }

    [Serializable]
    public struct SpriteFrameSize
    {
        public string name;
        public int width;
        public int height;
    }

    [Serializable]
    public struct MusicLoopOffset
    {
        public string name;
        public float offset;
    }

    [Serializable]
    public struct TextDict
    {
        public string name;
        public string text;
        public int value;
    }

    public static TextureLibrary textureLibrary = GameObject.Find("View").GetComponent<LibraryManager>().textureLibrary;
    public static AnimationData[] animationLibrary = new AnimationData[0];
    public static SpriteFrameSize[] spriteSizeLibrary = new SpriteFrameSize[0];
    public static SoundLibrary soundLibrary = GameObject.Find("View").GetComponent<LibraryManager>().soundLibrary;
    public static MusicLibrary musicLibrary = GameObject.Find("View").GetComponent<LibraryManager>().musicLibrary;
    public static MusicLoopOffset[] musicLoopOffsetLibrary = new MusicLoopOffset[0];
    public static TextLibrary textLibrary = GameObject.Find("View").GetComponent<LibraryManager>().textLibrary;

    public static int[] charWidths;

    public static Transform musicParent = GameObject.Find("View/Music Parent").transform;
    public static List<AudioSource> musicSourceArray = new List<AudioSource>();
    public static AudioSource mus1; //= GameObject.Find("View/Music1").GetComponent<AudioSource>();
    public static AudioSource mus2; //= GameObject.Find("View/Music2").GetComponent<AudioSource>();
    public static AudioSource activeMus; //= mus1;
    public static AudioSource globalSFX = GameObject.Find("View/Global SFX Source").GetComponent<AudioSource>();
    public static bool musFlag = false;
    public static bool playingMusic = false;
    public static int musicVol = 1;
    public static float playbackTime;
    public static string area;
    public static AudioClip areaMus;
    public static bool colorblindMode = true;
    public static bool quickDeathTransition = false;
    public static bool armorPingPlayedThisFrame = false;
    public static bool explodePlayedThisFrame = false;
    public static Vector2 parallaxFg2Mod = Vector2.zero;
    public static Vector2 parallaxFg1Mod = Vector2.zero;
    public static Vector2 parallaxBgMod = Vector2.zero;
    public static Vector2 parallaxSkyMod = Vector2.zero;
    public static int thisParticleID = 0;
    public static bool isTalking = false;
    public static bool hasJumped = false;
    public static Vector2 positionOfLastRoom = Vector2.zero;
    public static Vector2 positionOfLastSave = Vector2.zero;
    public static int enemyBulletPointer = 0;
    public static List<Vector2> breakablePositions = new List<Vector2>();

    public static int importJobs = 0;

    public static Texture2D palette = (Texture2D)Resources.Load("Images/Palette");

    public static AudioClip snailTown = (AudioClip)Resources.Load("Sounds/Music/SnailTown");
    public static AudioClip majorItemJingle = (AudioClip)Resources.Load("Sounds/Music/MajorItemJingle");
    public static AudioClip[][] areaMusic = new AudioClip[][]
    {
        new AudioClip[]
        {
            (AudioClip)Resources.Load("Sounds/Music/SnailTown"),
            (AudioClip)Resources.Load("Sounds/Music/TestZone")
        },
        new AudioClip[]
        {
            (AudioClip)Resources.Load("Sounds/Music/MareCarelia")
        }
    };
    public static int currentArea = -1;
    public static int currentSubzone = -1;

    public static GameObject player = GameObject.Find("Player");
    public static Player playerScript = player.GetComponent<Player>();
    public static GameObject cam = GameObject.Find("View");
    public static SpriteRenderer screenCover = GameObject.Find("View/Cover").GetComponent<SpriteRenderer>();
    public static GameObject groundLayer = GameObject.Find("Grid/Ground");
    public static GameObject fg2Layer = GameObject.Find("Grid/Foreground 2");
    public static GameObject fg1Layer = GameObject.Find("Grid/Foreground");
    public static GameObject bgLayer = GameObject.Find("Grid/Background");
    public static GameObject skyLayer = GameObject.Find("Grid/Sky");
    public static GameObject specialLayer = GameObject.Find("Grid/Special");
    public static GameObject minimap = GameObject.Find("View/Minimap Panel/Minimap");
    public static Minimap minimapScript = minimap.transform.parent.GetComponent<Minimap>();
    public static GameObject achievement = GameObject.Find("View/Achievement Panel");
    public static GameObject particlePool = GameObject.Find("Particle Pool");
    public static GameObject roomTriggerParent = GameObject.Find("Room Triggers");
    public static GameObject mainMenu = GameObject.Find("View/Menu Parent");
    public static GameObject loadingIcon = GameObject.Find("View/Loading Icon");
    public static GameObject enemyBulletPool = GameObject.Find("Enemy Bullet Pool");
    public static GameObject subscreen = GameObject.Find("View/Subscreen");
    public static Subscreen subscreenScript = subscreen.GetComponent<Subscreen>();

    public struct RoomEntity
    {
        public string name;
        public string tag;
        public Vector2 pos;
        public int[] spawnData;
    }
    public struct Breakable
    {
        public Vector2 pos;
        public int[] tiles;
        public int weaponLevel;
        public bool isSilent;
    }

    public static GameObject[] TogglableHUDElements = new GameObject[]
    {
        GameObject.Find("View/Minimap Panel"),             //  0
        GameObject.Find("View/Hearts"),                    //  1
        GameObject.Find("View/Debug Keypress Indicators"), //  2
        GameObject.Find("View/Weapon Icons"),              //  3
        GameObject.Find("View/Game Saved Text"),           //  4
        GameObject.Find("View/Area Name Text"),            //  5
        GameObject.Find("View/Item Get Text"),             //  6
        GameObject.Find("View/Item Percentage Text"),      //  7
        GameObject.Find("View/FPS Text"),                  //  8
        GameObject.Find("View/Time Text"),                 //  9
        GameObject.Find("View/Dialogue Box"),              // 10
        GameObject.Find("View/Bottom Keys"),               // 11
        GameObject.Find("View/Boss Health Bar")            // 12
    };

    public static bool paralyzed = false;
    public static bool isArmed = false;
    public static bool inBossFight = false;

    public static Vector2 camCenter;
    public static Vector2 camBoundaryBuffers;
    public static Vector2 camTempBuffers;
    public static Vector2 camTempBuffersX;
    public static Vector2 camTempBuffersY;
    public static Vector2 posRelativeToTempBuffers;
    public static Vector2 camTempBufferTruePos;

    public static readonly Vector2 WORLD_ORIGIN = new Vector2(0.5f, 0.5f); // The exact center of the chartable map
    public static readonly Vector2 WORLD_SIZE = new Vector2(26, 22); // The number of screens wide and tall the world is
    public static readonly Vector2 WORLD_SPAWN = new Vector2(-37, 10.5f); // Use (-37, 10.5f) for Snail Town spawn, (84, 88.5f) for debug room spawn
    public static readonly Vector2 ROOM_SIZE = new Vector2(26, 16); // The number of tiles wide and tall each screen is, counting the buffer space that makes up room borders
    public static Vector2 respawnCoords = WORLD_SPAWN;
    public static Scene respawnScene = SceneManager.GetActiveScene();

    public static TextMesh fpsText = GameObject.Find("View/FPS Text/Text").GetComponent<TextMesh>();
    public static TextMesh fpsShadow = GameObject.Find("View/FPS Text/Shadow").GetComponent<TextMesh>();
    public static TextMesh timeText = GameObject.Find("View/Time Text/Text").GetComponent<TextMesh>();
    public static TextMesh timeShadow = GameObject.Find("View/Time Text/Shadow").GetComponent<TextMesh>();
    public static TextMesh pauseText = GameObject.Find("View/Bottom Keys/Pause Key/Text").GetComponent<TextMesh>();
    public static TextMesh pauseShadow = GameObject.Find("View/Bottom Keys/Pause Key/Shadow").GetComponent<TextMesh>();
    public static TextMesh mapText = GameObject.Find("View/Bottom Keys/Map Key/Text").GetComponent<TextMesh>();
    public static TextMesh mapShadow = GameObject.Find("View/Bottom Keys/Map Key/Shadow").GetComponent<TextMesh>();

    public static int currentProfile = -1;
    public static int currentDifficulty = 1; // 0 = Easy, 1 = Normal, 2 = Insane
    public static string currentCharacter = "";
    public static float[] currentTime = new float[] { 0, 0, 0 };

    public static int helixCount;
    public static int heartCount;
    public static int itemPercentage;

    public static readonly int[] defaultMinimapState = new int[]
    {
        -1,  0,  0, -1, -1,  2,  0, -1, -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
        -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
        -1, -1,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1,
        -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1,
         0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1,  0,  0,  0,  0,  0, -1, -1, -1, -1,
         0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  2,  2,  2,  0,  0,  0,  0,  0, -1, -1, -1, -1,
        -1,  0,  0,  0,  0,  0,  0,  0, -1,  0, -1, -1, -1,  0,  2,  2,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1,
        -1,  0,  0,  0,  0,  0,  0, -1, -1,  0,  0,  0,  0,  0, -1,  0, -1,  0, -1, -1, -1, -1, -1, -1, -1, -1,
         0,  0,  0,  0,  0,  0,  0, -1,  2,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1, -1, -1,
         0,  0,  0,  0,  0,  0,  0,  2,  2,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1, -1, -1,
        -1,  0,  0,  0,  0,  0,  2,  2,  2,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1, -1, -1,
        -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1,
        -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1,  0, -1, -1, -1, -1, -1, -1,
        -1,  0,  2,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1,  0, -1, -1, -1, -1, -1, -1,
        -1,  0,  0,  0, -1, -1, -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1,
         0,  0,  0,  0, -1, -1, -1, -1,  0,  0,  0,  0, -1, -1, -1, -1,  0,  0,  2,  0, -1, -1, -1, -1, -1, -1,
         0, -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1,
         0,  0, -1,  0, -1, -1, -1, -1, -1, -1, -1,  0, -1,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1,
         0, -1, -1,  0, -1, -1, -1, -1, -1, -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1,
         0, -1, -1,  0,  0,  0, -1, -1, -1, -1, -1, -1, -1, -1, -1,  0, -1, -1, -1,  0,  0, -1, -1, -1, -1, -1,
         0, -1,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  0, -1, -1, -1, -1, -1,
         0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  0,  0, -1, -1, -1, -1, -1
    };
    public static List<int> saveLocations = new List<int>();
    public static List<int> bossLocations = new List<int>();
    public static Dictionary<int, int> itemLocations = new Dictionary<int, int>();
    public static Dictionary<int, string> playerMarkerLocations = new Dictionary<int, string>();

    public static int[] itemCollection = new int[]
    {
        0,  //  0 - Peashooter
        0,  //  1 - Boomerang
        0,  //  2 - Rainbow Wave
        0,  //  3 - Devastator
        0,  //  4 - High Jump          Wall Grab
        0,  //  5 - Shell Shield       Shelmet
        0,  //  6 - Rapid Fire         Backfire
        0,  //  7 - Ice Snail
        0,  //  8 - Gravity Snail      Magnetic Foot      Corkscrew Jump       Angel Jump
        0,  //  9 - Full-Metal Snail
        0,  // 10 - Gravity Shock
        0,  // 11 - Super Secret Boomerang
        0,  // 12 - Debug Rainbow Wave
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // 13-23 - Heart Containers
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0  // 24-53 - Helix Fragments
    };

    public static int[] bossStates = new int[]
    {
        1,  // Shellbreaker / Super Shellbreaker
        1,  // Stompy / Vis Vires
        1,  // Space Box / Time Cube
        1   // Moon Snail / Sun Snail
    };

    public static int[] achievementStates = new int[]
    {
        0, //  0 - First of Four
        0, //  1 - Stinky Toe
        0, //  2 - Gravity Battle
        0, //  3 - Victory
        0, //  4 - Scout
        0, //  5 - Explorer
        0, //  6 - Happy Ending
        0, //  7 - Treasure Hunter
        0, //  8 - Homeless
        0, //  9 - Top Floor
        0, // 10 - Mansion
        0, // 11 - Just Renting
        0, // 12 - Attic Dweller
        0, // 13 - Speedrunner
        0, // 14 - The Gauntlet
        0, // 15 - Pilgrim
        0, // 16 - Snelk Hunter A
        0, // 17 - Snelk Hunter B
        0, // 18 - Super Secret
        0, // 19 - Counter-Snail
        0, // 20 - Birds in the Maze Room
        0, // 21 - Where are we, Snaily?
        0, // 22 - Omega Snail
        0  // 23 - How did you get up here?
    };

    public static int[] achievementDefault = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    public static float[][] savedTimes = new float[][]
    {
        new float[] { 0, 0, 0 }, // Snaily Normal
        new float[] { 0, 0, 0 }, // Snaily Insane
        new float[] { 0, 0, 0 }, // Snaily 100%
        new float[] { 0, 0, 0 }, // Sluggy Normal
        new float[] { 0, 0, 0 }, // Sluggy Insane
        new float[] { 0, 0, 0 }, // Sluggy 100%
        new float[] { 0, 0, 0 }, // Upside Normal
        new float[] { 0, 0, 0 }, // Upside Insane
        new float[] { 0, 0, 0 }, // Upside 100%
        new float[] { 0, 0, 0 }, // Leggy  Normal
        new float[] { 0, 0, 0 }, // Leggy  Insane
        new float[] { 0, 0, 0 }, // Leggy  100%
        new float[] { 0, 0, 0 }, // Blobby Normal
        new float[] { 0, 0, 0 }, // Blobby Insane
        new float[] { 0, 0, 0 }, // Blobby 100%
        new float[] { 0, 0, 0 }, // Leechy Normal
        new float[] { 0, 0, 0 }, // Leechy Insane
        new float[] { 0, 0, 0 }, // Leechy 100%
        new float[] { 0, 0, 0 }  // Boss Rush
    };

    public static float[][] timeDefault = new float[][]
    {
        new float[] { 0, 0, 0 },
        new float[] { 0, 0, 0 },
        new float[] { 0, 0, 0 },
        new float[] { 0, 0, 0 },
        new float[] { 0, 0, 0 },
        new float[] { 0, 0, 0 },
        new float[] { 0, 0, 0 },
        new float[] { 0, 0, 0 },
        new float[] { 0, 0, 0 },
        new float[] { 0, 0, 0 },
        new float[] { 0, 0, 0 },
        new float[] { 0, 0, 0 },
        new float[] { 0, 0, 0 },
        new float[] { 0, 0, 0 },
        new float[] { 0, 0, 0 },
        new float[] { 0, 0, 0 },
        new float[] { 0, 0, 0 },
        new float[] { 0, 0, 0 },
        new float[] { 0, 0, 0 }
    };

    public static int[] gameOptions = new int[]
    {
        10, //  0 - Sound volume (0-10)
        10, //  1 - Music volume (0-10)
        1,  //  2 - Window resolution (0, 1, 2, or 3 (plus 1) for each zoom level)
        1,  //  3 - Minimap display (0 = hidden, 1 = only minimap, 2 = minimap and room names)
        2,  //  4 - Display bottom keys (0 = hidden, 1 = only weapon icons, 2 = all)
        0,  //  5 - Display keymap (boolean)
        0,  //  6 - Time display (boolean)
        0,  //  7 - FPS counter (boolean)
        0,  //  8 - Shoot mode (boolean)
        0,  //  9 - Texture pack ID (any positive int, 0 for default)
        0,  // 10 - Music pack ID (any positive int, 0 for default)
        5,  // 11 - Particle settings (0 = none, 1 = environments only, 2 = Flash entities, 3 = all entities, 4 = Flash, 5 = all)
        0,  // 12 - Breakable block reveal settings (0 = off, 1 = obvious, 2 = all)
        0,  // 13 - Secret tile visibility (boolean)
        2   // 14 - Frame limiter (0 = unlimited, 1 = 30fps, 2 = 60fps, 3 = 120fps)
    };

    public static int[] optionsDefault = new int[] { 10, 10, 1, 1, 2, 0, 0, 0, 0, 0, 0, 5, 0 };

    [Serializable]
    public struct OptionData
    {
        public int[] options;
    }

    public static string[] currentPacks = new string[]
    {
        "DEFAULT", // Texture
        "DEFAULT", // Sound
        "DEFAULT", // Music
        "DEFAULT"  // Text
    };

    [Serializable]
    public struct PackData
    {
        public string[] packs;
    }

    [Serializable]
    public struct ControlData
    {
        public KeyCode[] controls;
    }

    public const byte OFFSET_HEARTS = 13;
    public const byte OFFSET_FRAGMENTS = 24;

    public static bool hasSeenIris;
    public static bool talkedToCaveSnail;
    public static int[] NPCvarDefault = new int[] { 0, 0 };

    public const string SAVE_FILE_PREFIX = "SnailySave";

    [Serializable]
    public struct GameSaveData
    {

        public int profile;
        public int difficulty;
        public float[] gameTime;
        public Vector2 saveCoords;
        public string character;
        public int[] items;
        public int weapon;
        public int[] bossStates;
        public int[] NPCVars;
        public int percentage;
        public int[] exploredMap;
    }

    [Serializable]
    public struct RecordData
    {
        public int[] achievements;
        public float[][] times;
    }

    [Serializable]
    public struct CollectiveData
    {
        public string version;
        public GameSaveData profile1;
        public GameSaveData profile2;
        public GameSaveData profile3;
        public OptionData options;
        public PackData packs;
        public ControlData controls;
        public RecordData records;
    }
    public static CollectiveData gameData = new CollectiveData();

    public static Sprite BlankTexture(bool useSmallBlank = false)
    {
        return useSmallBlank ? playerScript.smallBlank : playerScript.blank;
    }

    public static Sprite MissingTexture()
    {
        return playerScript.missing;
    }

    public static AnimationData GetAnim(string name)
    {
        AnimationData foundData = new AnimationData
        {
            name = "NoAnim"
        };
        int i = 0;
        while (foundData.name == "NoAnim" && i < animationLibrary.Length)
        {
            if (animationLibrary[i].name == name)
                foundData = animationLibrary[i];
            i++;
        }
        return foundData;
    }

    public static void PrintAllAnims()
    {
        string output = "";
        for (int i = 0; i < animationLibrary.Length; i++)
            output += animationLibrary[i].name + "\n";
        Debug.Log(output);
    }

    public static void RefreshPoolAnims()
    {
        foreach (Transform obj in particlePool.transform)
            obj.GetComponent<AnimationModule>().ReloadList();
        foreach (Transform obj in playerScript.bulletPool.transform)
            obj.GetComponent<AnimationModule>().ReloadList();
        foreach (Transform obj in enemyBulletPool.transform)
            obj.GetComponent<AnimationModule>().ReloadList();
    }

    public static AnimationData GetAnim(int ID)
    {
        return animationLibrary[ID];
    }

    public static int GetAnimID(string name)
    {
        AnimationData foundData = new AnimationData
        {
            name = "NoAnim"
        };
        int i = 0;
        while (foundData.name == "NoAnim" && i < animationLibrary.Length)
        {
            if (animationLibrary[i].name == name)
                foundData = animationLibrary[i];
            else
                i++;
        }
        return i;
    }

    public static Sprite GetSprite(string name, int ID = 0)
    {
        Sprite newSprite = MissingTexture();
        int i = 0;
        bool found = false;
        while (i < textureLibrary.referenceList.Length && !found)
        {
            if (textureLibrary.referenceList[i] == name)
                found = true;
            i++;
        }
        if (found)
        {
            if (ID < textureLibrary.library[Array.IndexOf(textureLibrary.referenceList, name)].Length)
                newSprite = textureLibrary.library[Array.IndexOf(textureLibrary.referenceList, name)][ID];
        }
        return newSprite;
    }

    public static AudioClip GetSound(string name)
    {
        return soundLibrary.library[soundLibrary.soundDict[name]];
    }

    public static AudioClip GetMusic(int groupIndex, string name)
    {
        return musicLibrary.library[groupIndex][Array.IndexOf(musicLibrary.library[groupIndex], name)];
    }
    public static AudioClip GetMusic(int groupIndex, int songIndex)
    {
        return musicLibrary.library[groupIndex][songIndex];
    }

    public static void PlaySound(string name)
    {
        if (GetSound(name) != null)
            globalSFX.PlayOneShot(GetSound(name));
        else
            Debug.LogWarning("Audioclip \"" + name + "\" does not exist!");
    }
    public static void PlaySound(AudioClip clip)
    {
        globalSFX.PlayOneShot(clip);
    }

    public static void PlayMusic(int groupIndex, string name)
    {
        if (GetMusic(groupIndex, name) != null)
            globalSFX.PlayOneShot(GetMusic(groupIndex, name));
        else
            Debug.LogWarning("Audipclip \"" + name + "\" does not exist!");
    }
    public static void PlayMusic(int groupIndex, int songIndex)
    {
        globalSFX.PlayOneShot(GetMusic(groupIndex, songIndex));
    }

    public static Color32 GetColor(string ID)
    {
        return palette.GetPixel(int.Parse(ID.Substring(0, 2)) % 4, int.Parse(ID.Substring(2, 2)) % 14);
    }

    public static string GetText(string ID)
    {
        int i = 0;
        bool found = false;
        while (i < textLibrary.library.Length && !found)
        {
            if (textLibrary.library[i].name == ID)
                return textLibrary.library[i].text;
            i++;
        }
        return ID;
    }

    public static TextDict GetTextInfo(string ID)
    {
        int i = 0;
        bool found = false;
        while (i < textLibrary.library.Length && !found)
        {
            if (textLibrary.library[i].name == ID)
                return textLibrary.library[i];
            i++;
        }
        return new TextDict
        {
            name = "missing",
            text = "Text with ID \"" + ID + "\" not found",
            value = 0
        };
    }

    public static string GetIDFromText(string text)
    {
        int i = 0;
        bool found = false;
        while (i < textLibrary.library.Length && !found)
        {
            if (textLibrary.library[i].text == text)
                return textLibrary.library[i].name;
            i++;
        }
        return "ID with text \"" + text + "\" not found";
    }

    public static Sprite Colorize(string sprite, int spriteNum, string table, int tableValue)
    {
        Texture2D colorTable = GetSprite(table).texture;
        Dictionary<Color32, int> referenceColors = new Dictionary<Color32, int>();
        for (int i = 0; i < colorTable.width; i++)
        {
            referenceColors.Add(colorTable.GetPixel(i, 0), i);
        }

        Sprite oldSprite = GetSprite(sprite, spriteNum);
        Texture2D newSprite = new Texture2D((int)oldSprite.rect.width, (int)oldSprite.rect.height);
        Color[] pixels = oldSprite.texture.GetPixels((int)oldSprite.textureRect.x,
            (int)oldSprite.textureRect.y,
            (int)oldSprite.textureRect.width,
            (int)oldSprite.textureRect.height);
        for (int j = 0; j < pixels.Length; j++)
        {
            if (pixels[j].r == 0.9960785f && pixels[j].g == 0.9960785f && pixels[j].b == 0.9960785f)
                pixels[j] = new Color(0, 0, 0, 0);
            else if (referenceColors.ContainsKey(pixels[j]))
                pixels[j] = colorTable.GetPixel(referenceColors[pixels[j]], tableValue + 1);
        }
        newSprite.SetPixels(pixels);
        newSprite.Apply();

        return Sprite.Create(newSprite, new Rect(0, 0, newSprite.width, newSprite.height), new Vector2(0.5f, 0.5f), 16);
    }

    public static string ParseColorCodeToString(int colorData)
    {
        return "" + (colorData < 1000 ? "0" : "") + (colorData < 100 ? "0" : "") + (colorData < 100 ? "0" : "") + colorData.ToString();
    }

    public static Vector2 WorldPosToMapPos(Vector2 worldPos)
    {
        Vector2 topLeftCorner = new Vector2(WORLD_ORIGIN.x - (WORLD_SIZE.x * ROOM_SIZE.x * 0.5f), WORLD_ORIGIN.y + (WORLD_SIZE.y * ROOM_SIZE.y * 0.5f));
        return new Vector2(Mathf.Floor(Mathf.Abs(topLeftCorner.x - worldPos.x) / ROOM_SIZE.x), Mathf.Floor(Mathf.Abs(topLeftCorner.y - worldPos.y) / ROOM_SIZE.y));
    }

    public static int WorldPosToMapGridID(Vector3 worldPos)
    {
        return WorldPosToMapGridID(new Vector2(worldPos.x, worldPos.y));
    }
    public static int WorldPosToMapGridID(Vector3Int worldPos)
    {
        return WorldPosToMapGridID(new Vector2(worldPos.x, worldPos.y));
    }
    public static int WorldPosToMapGridID(Vector2 worldPos)
    {
        Vector2 mapPos = WorldPosToMapPos(worldPos);
        return Mathf.RoundToInt(mapPos.y * WORLD_SIZE.x + mapPos.x);
    }

    public static void BuildMapMarkerArrays()
    {
        saveLocations.Clear();
        bossLocations.Clear();
        itemLocations.Clear();
        foreach (Transform area in roomTriggerParent.transform)
        {
            foreach (Transform room in area)
            {
                foreach (Transform entity in room)
                {
                    if (entity.CompareTag("SavePoint"))
                        saveLocations.Add(WorldPosToMapGridID(entity.transform.position));
                    if (entity.CompareTag("Item"))
                        itemLocations.Add(WorldPosToMapGridID(entity.transform.position), entity.GetComponent<Item>().itemID);
                }
            }
        }

        Tilemap spMap = specialLayer.GetComponent<Tilemap>();
        for (int y = 0; y < spMap.size.y; y++)
        {
            for (int x = 0; x < spMap.size.x; x++)
            {
                List<int> bossTileIDs = new List<int> { 23, 24, 25, 26 };
                Vector3Int worldPos = new Vector3Int(Mathf.RoundToInt(spMap.origin.x - (spMap.size.x * 0.5f) + x), Mathf.RoundToInt(spMap.origin.y - (spMap.size.y * 0.5f) + y), 0);
                Sprite tileSprite = spMap.GetSprite(worldPos);
                if (tileSprite != null)
                {
                    int spriteID = int.Parse(tileSprite.name.Split('_', ' ')[1]);
                    if (bossTileIDs.Contains(spriteID))
                        bossLocations.Add(WorldPosToMapGridID(worldPos));
                }
            }
        }
    }

    public static void BuildPlayerMarkerArray()
    {
        playerMarkerLocations.Clear();
        for (int i = 0; i < minimapScript.currentMap.Length; i++)
            if (minimapScript.currentMap[i] >= 10)
                playerMarkerLocations.Add(i, "placeholder for multiplayer name");
    }

    public static void PlayAreaSong(int area, int subzone)
    {
        if (area == currentArea && subzone != currentSubzone)
        {
            playerScript.UpdateMusic(area, subzone);
        }
        else if (area != currentArea)
        {
            playerScript.UpdateMusic(area, subzone, 1);
        }
        currentArea = area;
        currentSubzone = subzone;
    }

    public static bool IsTileSolid(Vector2 tilePos, bool checkForEnemyCollide = false)
    {
        if (breakablePositions.Contains(new Vector2(Mathf.Floor(tilePos.x), Mathf.Floor(tilePos.y))))
            return true;
        else
        {
            Vector2 gridPos = new Vector2(Mathf.Floor(tilePos.x), Mathf.Floor(tilePos.y));
            bool result = groundLayer.GetComponent<Tilemap>().GetTile(new Vector3Int((int)gridPos.x, (int)gridPos.y, 0)) != null;
            if (!result && checkForEnemyCollide)
            {
                TileBase spTile = specialLayer.GetComponent<Tilemap>().GetTile(new Vector3Int((int)gridPos.x, (int)gridPos.y, 0));
                if (spTile != null)
                    if (spTile.name == "Tilesheet_376")
                        result = true;
            }
            return result;
        }
    }

    public static void ToggleHUD(bool state)
    {
        foreach (GameObject element in TogglableHUDElements)
        {
            element.SetActive(state);
            if (state)
            {
                if (element.name == "Weapon Icons")
                {
                    playerScript.ChangeWeaponIconSprite(0, !CheckForItem(0) ? 0 : (playerScript.selectedWeapon == 1 ? 2 : 1));
                    playerScript.ChangeWeaponIconSprite(1, !(CheckForItem(1) || CheckForItem(11)) ? 0 : (playerScript.selectedWeapon == 2 ? 2 : 1));
                    playerScript.ChangeWeaponIconSprite(2, !(CheckForItem(2) || CheckForItem(12)) ? 0 : (playerScript.selectedWeapon == 3 ? 2 : 1));
                }
                if (element.name == "Minimap Panel")
                    element.SetActive(!inBossFight);
                if (element.name == "Boss Health Bar")
                    element.SetActive(true);
            }
        }
    }

    public static void RunItemPopup(string item)
    {
        playbackTime = activeMus.time;
        mus1.Stop();
        areaMus = activeMus.clip;
        mus1.PlayOneShot(majorItemJingle);
        List<string> text = new List<string>();
        List<Color32> colors = new List<Color32>();
        switch (item)
        {
            default:
                text.Add("skibidi bop mm dada");
                break;
        }
        for (int i = 0; i < text.Count; i++)
        {
            colors.Add(new Color32(0, 0, 0, 0));
            colors.Add(new Color32(0, 0, 0, 0));
            colors.Add(new Color32(0, 0, 0, 0));
        }
        gameState = "Dialogue";
        cam.transform.Find("Dialogue Box").GetComponent<DialogueBox>().RunBox(1, 0, text, 0, "0005");
    }

    public static void OpenDialogue(int type, int speaker, List<string> text, int shape, string boxColor = "0005", List<int> stateList = null, bool facingLeft = false)
    {
        cam.transform.Find("Dialogue Box").GetComponent<DialogueBox>().RunBox(type, speaker, text, shape, boxColor, stateList, facingLeft);
    }

    public static void CloseDialogue()
    {
        isTalking = false;
        cam.transform.Find("Dialogue Box").GetComponent<DialogueBox>().CloseBox();
    }

    public static void ScreenFlash(string type, int red = 0, int green = 0, int blue = 0, int alpha = 0, float maxTime = 0)
    {
        switch (type)
        {
            default:
            case "Solid Color":
                screenCover.color = new Color32((byte)red, (byte)green, (byte)blue, (byte)alpha);
                break;
            case "Room Transition":
                screenCover.color = new Color32(0, 0, 0, 200);
                playerScript.ExecuteCoverCommand(type);
                break;
            case "Death Transition":
                playerScript.ExecuteCoverCommand(type);
                break;
            case "Custom Fade":
                playerScript.ExecuteCoverCommand(type, (byte)red, (byte)green, (byte)blue, (byte)alpha, maxTime);
                break;
        }
    }

    public static void FlashItemText(string item)
    {
        playerScript.FlashItemText(item);
    }

    public static void FlashCollectionText()
    {
        playerScript.FlashCollectionText();
    }

    public static void FlashSaveText()
    {
        playerScript.FlashSaveText();
    }

    public static Particle RequestParticle(Vector2 position, string type)
    {
        return RequestParticle(position, type, new float[] { 0 }, false);
    }
    public static Particle RequestParticle(Vector2 position, string type, float[] values)
    {
        return RequestParticle(position, type, values, false);
    }
    public static Particle RequestParticle(Vector2 position, string type, bool playSound)
    {
        return RequestParticle(position, type, new float[] { 0 }, playSound);
    }

    public static Particle RequestParticle(Vector2 position, string type, float[] values, bool playSound)
    {
        Particle selectedParticle = null;
        bool found = false;
        if (particlePool.transform.GetChild(thisParticleID).gameObject.activeSelf)
        {
            int i = 0;
            while (i < particlePool.transform.childCount - 1 && !found)
            {
                thisParticleID++;
                if (thisParticleID >= particlePool.transform.childCount)
                    thisParticleID = 0;
                if (!particlePool.transform.GetChild(thisParticleID).gameObject.activeSelf)
                    found = true;
                i++;
            }
        }
        else
            found = true;

        if (found)
        {
            Transform particleObject = particlePool.transform.GetChild(thisParticleID);
            particleObject.gameObject.SetActive(true);
            Particle particleScript = particleObject.GetComponent<Particle>();

            bool activateParticle = false;

            switch (type.ToLower())
            {
                default:
                    break;
                case "bubble":
                    // Values:
                    // 0 = Water level
                    // 1 = Boolean to initialize particle with random velocity or not

                    if (gameOptions[11] > 1)
                    {
                        activateParticle = true;
                        particleScript.vars[0] = UnityEngine.Random.Range(0, 2 * Mathf.PI);       // Animation cycle
                        particleScript.vars[1] = position.x;                                      // Origin X
                        particleScript.vars[2] = values[0] - 0.25f;                               // Water level above the bubble's spawn
                        particleScript.vars[3] = 4 + UnityEngine.Random.Range(0f, 1f) * 0.0625f;  // Rise speed
                        particleScript.vars[4] = values[1];                                       // Randomize initial velocity
                    }
                    break;
                case "explosion":
                    // Values:
                    // 0 = Size

                    if ((gameOptions[11] > 1 && values[0] <= 4) || ((gameOptions[11] == 3 || gameOptions[11] == 5) && values[0] > 4))
                    {
                        activateParticle = true;
                        particleScript.vars[0] = values[0];
                    }
                    break;
                case "nom":
                    // Values:
                    // 0 = Start Y

                    if (gameOptions[11] > 1)
                    {
                        activateParticle = true;
                        particleScript.vars[0] = position.y;
                    }
                    break;
                case "smoke":
                    if (gameOptions[11] > 1)
                        activateParticle = true;
                    break;
                case "splash":
                    if (gameOptions[11] == 1 || gameOptions[11] == 3 || gameOptions[11] == 5)
                        activateParticle = true;
                    break;
                case "zzz":
                    if (gameOptions[11] > 1)
                        activateParticle = true;
                    break;
            }

            if (activateParticle)
            {
                selectedParticle = particleScript;
                particleObject.position = position;
                particleScript.type = type;
                particleScript.SetAnim(type);
                if (playSound)
                    particleScript.PlaySound();
                thisParticleID++;
                if (thisParticleID >= particlePool.transform.childCount)
                    thisParticleID = 0;
            }
        }
        return selectedParticle;
    }

    public static void RequestQueuedExplosion(Vector2 pos, float lifeTime, int size, bool loudly)
    {
        playerScript.RequestQueuedExplosion(pos, lifeTime, size, loudly);
    }

    public static void ResetAllParticles()
    {
        foreach (Transform particle in particlePool.transform)
        {
            Particle particleScript = particle.GetComponent<Particle>();
            if (particle.gameObject.activeSelf)
                particleScript.ResetParticle();
        }
    }

    public static bool IsBossAlive(int bossID)
    {
        return bossStates[bossID] == 1;
    }

    public static bool CheckForItem(int itemID)
    {
        return itemCollection[itemID] == 1;
    }

    public static bool CheckForItem(string itemName)
    {
        return itemCollection[TranslateItemNameToID(itemName)] == 1;
    }

    public static bool CheckShellLevel(int level)
    {
        bool meetsLevel;
        if (currentDifficulty == 2)
        {
            meetsLevel = level switch
            {
                2 => CheckForItem(8),
                3 => CheckForItem(9),
                _ => CheckForItem(7),
            };
        }
        else
        {
            meetsLevel = level switch
            {
                2 => CheckForItem(8) || CheckForItem(9),
                3 => CheckForItem(9),
                _ => CheckForItem(7) || CheckForItem(8) || CheckForItem(9),
            };
        }
        return meetsLevel;
    }

    public static void AddItem(int itemID)
    {
        itemCollection[itemID] = 1;
    }

    public static void AddItem(string itemName)
    {
        itemCollection[TranslateItemNameToID(itemName)] = 1;
    }

    public static void AssignProperCollectibleIDs()
    {
        Transform roomTriggerArray = GameObject.Find("Room Triggers").transform;
        int currentHelixCount = 0;
        int currentHeartCount = 0;

        foreach (Transform area in roomTriggerArray)
        {
            foreach (Transform room in area)
            {
                foreach (Transform entity in room)
                {
                    if (entity.name == "Item")
                    {
                        if (entity.GetComponent<Item>().itemID >= OFFSET_FRAGMENTS)
                        {
                            entity.GetComponent<Item>().itemID = OFFSET_FRAGMENTS + currentHelixCount;
                            currentHelixCount++;
                        }
                        else if (entity.GetComponent<Item>().itemID >= OFFSET_HEARTS)
                        {
                            entity.GetComponent<Item>().itemID = OFFSET_HEARTS + currentHeartCount;
                            currentHeartCount++;
                        }
                    }
                }
            }
        }
    }

    public static byte TranslateItemNameToID(string itemName)
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
            case "Debug Rainbow Wave":
                id = 12;
                break;
            case "Heart Container":
                id = byte.Parse(itemName.Substring(15, itemName.Length));
                break;
            case "Helix Fragment":
                id = byte.Parse(itemName.Substring(14, itemName.Length));
                break;
        }
        return id;
    }

    public static string TranslateIDToItemName(int itemID)
    {
        string name = "";
        if (itemID >= OFFSET_FRAGMENTS)
            name = "Helix Fragment";
        else if (itemID >= OFFSET_HEARTS)
            name = "Heart Container";
        else
        {
            switch (itemID)
            {
                case 0:
                    name = "Peashooter";
                    break;
                case 1:
                    name = "Boomerang";
                    break;
                case 2:
                    name = "Rainbow Wave";
                    break;
                case 3:
                    name = "Devastator";
                    break;
                case 4:
                    if (currentCharacter == "Blobby")
                        name = "Wall Grab";
                    else
                        name = "High Jump";
                    break;
                case 5:
                    if (currentCharacter == "Blobby")
                        name = "Shelmet";
                    else
                        name = "Shell Shield";
                    break;
                case 6:
                    if (currentCharacter == "Leechy")
                        name = "Backfire";
                    else
                        name = "Rapid Fire";
                    break;
                case 7:
                    name = "Ice Snail";
                    break;
                case 8:
                    if (currentCharacter == "Upside")
                        name = "Magnetic Foot";
                    else if (currentCharacter == "Leggy")
                        name = "Corkscrew Jump";
                    else if (currentCharacter == "Blobby")
                        name = "Angel Jump";
                    else
                        name = "Gravity Snail";
                    break;
                case 9:
                    name = "Full-Metal Snail";
                    break;
                case 10:
                    name = "Gravity Shock";
                    break;
                case 11:
                    name = "Super Secret Boomerang";
                    break;
                case 12:
                    name = "Debug Rainbow Wave";
                    break;
            }
        }
        return name;
    }

    public static int GetMapPercentage()
    {
        int explored = 0;
        int total = 0;
        foreach (int i in minimapScript.currentMap)
        {
            if (i != -1)
            {
                total++;
                if (i == 1)
                    explored++;
            }
        }
        return Mathf.FloorToInt(((float)explored / (float)total) * 100);
    }

    public static int GetItemPercentage()
    {
        int itemsFound = 0;
        int totalCount = 0;
        for (int i = 0; i < itemCollection.Length; i++)
        {
            if (!((currentCharacter == "Sluggy" || currentCharacter == "Leechy") && i == 5))
            {
                totalCount++;
                itemsFound += itemCollection[i] == 1 ? 1 : 0;
            }
        }
        return Mathf.FloorToInt(((float)itemsFound / (float)totalCount) * 100);
    }

    public static string GetTimeString()
    {
        string hourInt = currentTime[0] < 10 ? "0" + currentTime[0] : (currentTime[0] == 0 ? "00" : currentTime[0].ToString());
        string minuteInt = currentTime[1] < 10 ? "0" + currentTime[1] : (currentTime[1] == 0 ? "00" : currentTime[1].ToString());
        string secondsInt = (Mathf.RoundToInt(currentTime[2] * 100) + 10000).ToString();
        return hourInt + ":" + minuteInt + ":" + secondsInt.Substring(1, 2) + "." + secondsInt.Substring(3, 2);
    }

    public static void SetMapTile(Vector2 pos, bool state)
    {
        int cellID = Mathf.RoundToInt((WORLD_SIZE.x * pos.y) + pos.x);
        int currentCellState = minimapScript.currentMap[cellID];
        bool marked = false;
        if (currentCellState >= 10)
        {
            currentCellState -= 10;
            marked = true;
        }
        minimapScript.currentMap[cellID] = currentCellState > 1 ? (state ? 3 : 2) : (state ? 1 : 0);
        if (marked)
            minimapScript.currentMap[cellID] += 10;
        minimapScript.RefreshMap();
    }

    public static void WriteSave(string dataType = "")
    {
        if (dataType == "game")
        {
            GameSaveData data = new GameSaveData
            {
                profile = currentProfile,
                difficulty = currentDifficulty,
                gameTime = (float[])currentTime.Clone(),
                saveCoords = respawnCoords,
                character = currentCharacter,
                items = (int[])itemCollection.Clone(),
                weapon = playerScript.selectedWeapon,
                bossStates = (int[])bossStates.Clone(),
                NPCVars = new int[]
                {
                    hasSeenIris ? 1 : 0,
                    talkedToCaveSnail ? 1 : 0
                },
                percentage = GetItemPercentage(),
                exploredMap = (int[])minimapScript.currentMap.Clone()
            };
            switch (currentProfile)
            {
                case 1:
                    gameData.profile1 = data;
                    break;
                case 2:
                    gameData.profile2 = data;
                    break;
                case 3:
                    gameData.profile3 = data;
                    break;
            }
        }
        else if (dataType == "options")
        {
            gameData.options = new OptionData
            {
                options = gameOptions
            };
        }
        else if (dataType == "packs")
        {
            gameData.packs = new PackData
            {
                packs = currentPacks
            };
        }
        else if (dataType == "records")
        {
            gameData.records = new RecordData
            {
                achievements = achievementStates,
                times = savedTimes
            };
        }
        else if (dataType == "controls")
        {
            gameData.controls = new ControlData
            {
                controls = Control.inputs
            };
        }
        else if (!(dataType == "" || dataType == " "))
        {
            Debug.LogWarning("Invalid save type \"" + dataType + "\"");
        }
        gameData.version = Application.version;
        File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_CurrentSave.json", JsonUtility.ToJson(gameData));
    }

    public static void WriteSave(GameSaveData copyData, int profileToCopyTo)
    {
        copyData.profile = profileToCopyTo;
        switch (profileToCopyTo)
        {
            case 1:
                gameData.profile1 = copyData;
                break;
            case 2:
                gameData.profile2 = copyData;
                break;
            case 3:
                gameData.profile3 = copyData;
                break;
        }
        WriteSave();
    }

    public static GameSaveData LoadGame(int profile, bool mode = false)
    {
        if ((profile == 1 && gameData.profile1.profile == 1) || (profile == 2 && gameData.profile2.profile == 2) || (profile == 3 && gameData.profile3.profile == 3))
        {
            GameSaveData loadedSave = profile == 1 ? gameData.profile1 : (profile == 2 ? gameData.profile2 : gameData.profile3);
            if (mode)
            {
                currentProfile = loadedSave.profile;
                currentDifficulty = loadedSave.difficulty;
                currentTime = (float[])loadedSave.gameTime.Clone();
                respawnCoords = loadedSave.saveCoords;
                currentCharacter = loadedSave.character;
                itemCollection = (int[])loadedSave.items.Clone();
                playerScript.selectedWeapon = loadedSave.weapon;
                bossStates = (int[])loadedSave.bossStates.Clone();
                hasSeenIris = loadedSave.NPCVars[0] == 1;
                talkedToCaveSnail = loadedSave.NPCVars[1] == 1;
                minimapScript.currentMap = (int[])loadedSave.exploredMap.Clone();
                playerScript.maxHealth = playerScript.hpPerHeart[currentDifficulty] * 3;
                helixCount = 0;
                heartCount = 0;
                for (int i = 0; i < loadedSave.items.Length; i++)
                {
                    if (loadedSave.items[i] == 1)
                    {
                        if (i >= OFFSET_FRAGMENTS)
                            helixCount++;
                        else if (i >= OFFSET_HEARTS)
                        {
                            heartCount++;
                            playerScript.maxHealth += playerScript.hpPerHeart[currentDifficulty];
                        }
                    }
                }
                playerScript.health = playerScript.maxHealth;
                playerScript.RenderNewHearts();
            }
            return loadedSave;
        }
        else
        {
            GameSaveData nullData = new GameSaveData
            {
                profile = -1
            };
            return nullData;
        }
    }

    public static void EraseGame(int profile)
    {
        switch (profile)
        {
            case 1:
                gameData.profile1 = new GameSaveData { profile = -1 };
                break;
            case 2:
                gameData.profile2 = new GameSaveData { profile = -1 };
                break;
            case 3:
                gameData.profile3 = new GameSaveData { profile = -1 };
                break;
        }
        WriteSave();
    }

    public static void LoadRecords()
    {
        //if (PlayerPrefs.HasKey("RecordData"))
        //{
        //    RecordData data = JsonUtility.FromJson<RecordData>(PlayerPrefs.GetString("RecordData"));
        //    for (int i = 0; i < data.achievements.Length; i++)
        //        achievementStates[i] = data.achievements[i];
        //    if (data.times != null)
        //    {
        //        for (int i = 0; i < data.times.Length; i++)
        //            savedTimes[i] = data.times[i];
        //    }
        //    else
        //    {
        //        savedTimes = new float[][]
        //        {
        //            new float[] { 0, 0, 0 },
        //            new float[] { 0, 0, 0 },
        //            new float[] { 0, 0, 0 },
        //            new float[] { 0, 0, 0 },
        //            new float[] { 0, 0, 0 },
        //            new float[] { 0, 0, 0 },
        //            new float[] { 0, 0, 0 },
        //            new float[] { 0, 0, 0 },
        //            new float[] { 0, 0, 0 },
        //            new float[] { 0, 0, 0 },
        //            new float[] { 0, 0, 0 },
        //            new float[] { 0, 0, 0 },
        //            new float[] { 0, 0, 0 },
        //            new float[] { 0, 0, 0 },
        //            new float[] { 0, 0, 0 },
        //            new float[] { 0, 0, 0 },
        //            new float[] { 0, 0, 0 },
        //            new float[] { 0, 0, 0 },
        //            new float[] { 0, 0, 0 }
        //        };
        //    }
        //}
        if (achievementStates.Length == gameData.records.achievements.Length)
            achievementStates = gameData.records.achievements;
        else
        {
            for (int i = 0; i < gameData.records.achievements.Length; i++)
                achievementStates[i] = gameData.records.achievements[i];
        }
        if (savedTimes.Length == gameData.records.times.Length)
            savedTimes = gameData.records.times;
        else
        {
            for (int i = 0; i < gameData.records.times.Length; i++)
                savedTimes[i] = gameData.records.times[i];
        }
    }

    public static void LoadOptions()
    {
        bool load = true;
        try
        {
            int i = gameData.options.options.Length;
        }
#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable CS0168 // Variable is declared but never used
        catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        {
            load = false;
        }

        if (load)
        {
            if (gameOptions.Length == gameData.options.options.Length)
                gameOptions = gameData.options.options;
            else
            {
                for (int i = 0; i < gameData.options.options.Length; i++)
                    gameOptions[i] = gameData.options.options[i];
            }
        }
    }

    public static void LoadPacks()
    {
        bool loadNewPacks = true;
        try
        {
            string pack1 = gameData.packs.packs[0];
            string pack2 = gameData.packs.packs[1];
            string pack3 = gameData.packs.packs[2];
            string pack4 = gameData.packs.packs[3];
        }
#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable CS0168 // Variable is declared but never used
        catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        {
            loadNewPacks = false;
        }

        if (currentPacks.Length == gameData.packs.packs.Length && loadNewPacks)
        {
            for (int i = 0; i < 4; i++)
            {
                string packType = i switch { 1 => "Sound", 2 => "Music", 3 => "Text", _ => "Texture" };
                if (gameData.packs.packs[i] != "DEFAULT")
                {
                    string path = Application.persistentDataPath + "/" + packType + "Packs/" + gameData.packs.packs[i];
                    if (Directory.Exists(path))
                    {
                        switch (packType)
                        {
                            case "Texture":
                                textureLibrary.BuildSpriteSizeLibrary(path + "/SpriteSizes.json");
                                textureLibrary.BuildAnimationLibrary(path + "/Animations.json");
                                textureLibrary.BuildLibrary(path);
                                textureLibrary.BuildTilemap();
                                break;
                            case "Sound":
                                soundLibrary.BuildLibrary(path);
                                break;
                            case "Music":
                                musicLibrary.BuildOffsetLibrary(path + "/MusicLoopOffsets.json");
                                musicLibrary.BuildLibrary(path);
                                break;
                            case "Text":
                                textLibrary.BuildLibrary(path + "/Text.json");
                                break;
                        }
                    }
                }
                else
                {
                    switch (packType)
                    {
                        case "Texture":
                            textureLibrary.BuildDefaultSpriteSizeLibrary();
                            textureLibrary.BuildDefaultLibrary();
                            textureLibrary.BuildDefaultAnimLibrary();
                            textureLibrary.BuildTilemap();
                            break;
                        case "Sound":
                            soundLibrary.BuildDefaultLibrary();
                            break;
                        case "Music":
                            musicLibrary.BuildDefaultLibrary();
                            musicLibrary.BuildDefaultOffsetLibrary();
                            break;
                        case "Text":
                            textLibrary.BuildDefaultLibrary();
                            break;
                    }
                }
            }
        }
        else
            currentPacks = new string[] { "DEFAULT", "DEFAULT", "DEFAULT", "DEFAULT" };
    }

    public static void LoadControls()
    {
        bool load = true;
        try
        {
            int i = gameData.controls.controls.Length;
        }
#pragma warning disable IDE0059 // Unnecessary assignment of a value
#pragma warning disable CS0168 // Variable is declared but never used
        catch (Exception e)
#pragma warning restore CS0168 // Variable is declared but never used
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        {
            load = false;
        }

        if (load)
        {
            if (Control.inputs.Length == gameData.controls.controls.Length)
                Control.inputs = gameData.controls.controls;
            else
            {
                for (int i = 0; i < gameData.controls.controls.Length; i++)
                    Control.inputs[i] = gameData.controls.controls[i];
            }
        }
    }

    public static bool HasTime(int ID = -1)
    {
        float[] blankTime = new float[] { 0, 0, 0 };
        bool foundTimes = false;
        if (ID == -1)
        {
            foreach (float[] selectedTime in savedTimes)
                foundTimes = selectedTime == blankTime;
            return foundTimes;
        }
        else
            return savedTimes[ID] != blankTime;
    }

    public static void QueueAchievementPopup(string achID)
    {
        achievement.GetComponent<AchievementPanel>().popupQueue.Add(achID);
    }

    public struct AnimationLibrary
    {
        public AnimationData[] animArray;
    }
    public static void LoadNewAnimationLibrary(string path)
    {
        AnimationLibrary newLibrary = JsonUtility.FromJson<AnimationLibrary>(File.ReadAllText(path));
        animationLibrary = newLibrary.animArray;
    }

    public struct SpriteSizeLibrary
    {
        public SpriteFrameSize[] sizeArray;
    }
    public static void LoadNewSpriteSizeLibrary(string path)
    {
        SpriteSizeLibrary newLibrary = JsonUtility.FromJson<SpriteSizeLibrary>(File.ReadAllText(path));
        spriteSizeLibrary = newLibrary.sizeArray;
    }

    public struct MusicOffsetLibrary
    {
        public MusicLoopOffset[] offsetArray;
    }
    public static void LoadNewMusicOffsetLibrary(string path)
    {
        MusicOffsetLibrary newLibrary = JsonUtility.FromJson<MusicOffsetLibrary>(File.ReadAllText(path));
        musicLoopOffsetLibrary = newLibrary.offsetArray;
    }

    public static void AdjustHUDText()
    {
        TogglableHUDElements[4].transform.GetChild(0).GetComponent<TextMesh>().text = GetText("hud_gameSaved");
        TogglableHUDElements[4].transform.GetChild(1).GetComponent<TextMesh>().text = GetText("hud_gameSaved");
    }

    public static void ToggleLoadingIcon(bool state)
    {
        if (state)
        {
            loadingIcon.SetActive(true);
            loadingIcon.GetComponent<AnimationModule>().Play("Loading");
        }
        else
        {
            loadingIcon.GetComponent<AnimationModule>().Stop();
            loadingIcon.SetActive(false);
        }
    }

    public static bool ShootEnemyBullet(Vector2 newOrigin, int type, float angle, float newSpeed)
    {
        return ShootEnemyBullet(newOrigin, type, Quaternion.Euler(0, 0, angle) * Vector2.up, newSpeed);
    }
    public static bool ShootEnemyBullet(Vector2 newOrigin, int type, Vector2 direction, float newSpeed)
    {
        bool hasShot = false;
        if (!enemyBulletPool.transform.GetChild(enemyBulletPointer).GetComponent<EnemyBullet>().isActive)
        {
            enemyBulletPool.transform.GetChild(enemyBulletPointer).GetComponent<EnemyBullet>().Shoot(newOrigin, type, direction, newSpeed);
            enemyBulletPointer = (enemyBulletPointer + 1) % enemyBulletPool.transform.childCount;
            hasShot = true;
        }
        return hasShot;
    }

    public static Vector2 DirectionBetween(Vector2 a, Vector2 b)
    {
        return (b - a).normalized;
    }

    public static void SetCamFocus(Transform point)
    {
        cam.GetComponent<CamMovement>().focusPoint = point;
    }

    public static void ToggleBossfightState(bool state, int musicID, bool snapDespawnBar = false)
    {
        inBossFight = state;
        TogglableHUDElements[0].SetActive(!state);
        if (!state && snapDespawnBar)
        {
            TogglableHUDElements[12].GetComponent<SpriteRenderer>().enabled = false;
            TogglableHUDElements[12].transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
            playerScript.displayDefeatText = false;
        }
        else if (!state && !snapDespawnBar)
        {
            TogglableHUDElements[12].GetComponent<AnimationModule>().Play("BossBar_frame_despawn");
            TogglableHUDElements[12].transform.GetChild(0).GetComponent<AnimationModule>().Play("BossBar_bar_despawn");
            playerScript.displayDefeatText = true;
        }
        if (currentArea != 7)
        {
            if (state)
                playerScript.UpdateMusic(musicID, 0, 1);
            else
                playerScript.UpdateMusic(currentArea, currentSubzone, 1);
        }
    }

    public static bool OnScreen(Vector2 position, BoxCollider2D box)
    {
        float boxAdjust = box != null ? box.size.x * 0.5f : 8;
        return Vector2.Distance(new Vector2(position.x, 0), new Vector2(cam.transform.position.x, 0)) - boxAdjust < 12.5f &&
            Vector2.Distance(new Vector2(0, position.y), new Vector2(0, cam.transform.position.y)) - boxAdjust < 7.5f;
    }
}
