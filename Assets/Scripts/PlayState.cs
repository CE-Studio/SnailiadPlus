using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PlayState {
    public const float TAU = Mathf.PI * 2;
    public const float PI_OVER_EIGHT = Mathf.PI * 0.125f;
    public const float PI_OVER_FOUR = Mathf.PI * 0.25f;
    public const float PI_OVER_TWO = Mathf.PI * 0.5f;
    public const float THREE_PI_OVER_TWO = TAU - PI_OVER_TWO;
    public const float FRAC_8 = 0.125f;
    public const float FRAC_16 = 0.0625f;
    public const float FRAC_32 = 0.03125f;
    public const float FRAC_64 = 0.015625f;
    public const float FRAC_128 = 0.0078125f;
    public static readonly Vector2 ANGLE_DIAG = new(Mathf.Cos(40 * Mathf.Deg2Rad), Mathf.Sin(40 * Mathf.Deg2Rad));

    public static readonly string[] DIRS_COMPASS = new string[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
    public static readonly string[] DIRS_CARDINAL = new string[] { "up", "down", "left", "right" };
    public static readonly string[] DIRS_SURFACE = new string[] { "floor", "wallL", "wallR", "ceiling" };
    public enum EDirsCompass { N, NE, E, SE, S, SW, W, NW, None };
    public enum EDirsCardinal { Up, Down, Left, Right, None };
    public enum EDirsSurface { Floor, WallL, WallR, Ceiling, None };

    public enum GameState { game, menu, pause, map, debug, dialogue, error }
    public static GameState gameState = GameState.menu;

    public static bool isMenuOpen = false;

    public static bool noclipMode = false;

    [Serializable]
    public struct AnimationData {
        public string name;
        public string spriteName;
        public float framerate;
        public int[] frames;
        public bool loop;
        public int loopStartFrame;
        public bool randomizeStartFrame;
    }

    [Serializable]
    public struct SpriteFrameSize {
        public string name;
        public int width;
        public int height;
    }

    [Serializable]
    public struct MusicLoopOffset {
        public string name;
        public float offset;
    }

    [Serializable]
    public struct TextDict {
        public string name;
        public string text;
        public int value;
    }

    public static TextureLibrary textureLibrary;
    public static AnimationData[] animationLibrary = new AnimationData[0];
    public static SpriteFrameSize[] spriteSizeLibrary = new SpriteFrameSize[0];
    public static SoundLibrary soundLibrary;
    public static MusicLibrary musicLibrary;
    public static MusicLoopOffset[] musicLoopOffsetLibrary = new MusicLoopOffset[0];
    public static TextLibrary textLibrary;

    public static int[] charWidths;

    public static Transform musicParent;
    public static List<AudioSource> musicSourceArray = new List<AudioSource>();
    public static AudioSource mus1; //= GameObject.Find("View/Music1").GetComponent<AudioSource>();
    public static AudioSource mus2; //= GameObject.Find("View/Music2").GetComponent<AudioSource>();
    public static AudioSource activeMus; //= mus1;
    public static AudioSource globalSFX;
    public static AudioSource globalMusic;
    public static bool musFlag = false;
    public static bool playingMusic = false;
    public static int musicVol = 1;
    public static float playbackTime;
    public static float fader = 1.0f;
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
    public static Vector2 fg2Offset = Vector2.zero;
    public static Vector2 fg1Offset = Vector2.zero;
    public static Vector2 bgOffset = Vector2.zero;
    public static Vector2 skyOffset = Vector2.zero;
    public static int thisParticleID = 0;
    public static bool isTalking = false;
    public static bool hasJumped = false;
    public static Vector2 positionOfLastRoom = Vector2.zero;
    public static Vector2 positionOfLastSave = Vector2.zero;
    public static int enemyBulletPointer = 0;
    public static int enemyGlobalMoveIndex = 0;
    public static List<Vector2> breakablePositions = new List<Vector2>();
    public static List<int> tempTiles = new List<int>(); // x, y, layer, original tile ID
    public static bool dialogueOpen = false;
    public static bool cutsceneActive = false;
    public static int lastLoadedWeapon = 0;
    public static bool stackShells = true;
    public static bool stackWeaponMods = true;

    public static int importJobs = 0;

    public static Texture2D palette;

    public static int currentArea = -1;
    public static int currentSubzone = -1;

    public static GameObject player;
    public static Player playerScript;
    public static GameObject cam;
    public static GameObject camObj;
    public static CamMovement camScript;
    public static SpriteRenderer screenCover;
    public static GameObject groundLayer;
    public static GameObject fg2Layer;
    public static GameObject fg1Layer;
    public static GameObject bgLayer;
    public static GameObject skyLayer;
    public static GameObject specialLayer;
    public static GameObject minimap;
    public static Minimap minimapScript;
    public static GameObject achievement;
    public static GameObject particlePool;
    public static GameObject camParticlePool;
    public static GameObject roomTriggerParent;
    public static GameObject mainMenu;
    public static GameObject loadingIcon;
    public static GameObject enemyBulletPool;
    public static GameObject subscreen;
    public static Subscreen subscreenScript;
    public static GameObject dialogueBox;
    public static DialogueBox dialogueScript;
    public static GameObject titleParent;

    public static GlobalFunctions globalFunctions;

    //Replaced with IRoomObject
    //public struct RoomEntity
    //{
    //    public string name;
    //    public string tag;
    //    public Vector2 pos;
    //    public int[] spawnData;
    //}
    public struct Breakable {
        public Vector2 pos;
        public int[] tiles;
        public int weaponLevel;
        public bool isSilent;
    }

    public static GameObject[] TogglableHUDElements;

    public static bool paralyzed = false;
    public static bool overrideParalysisInvulnerability = false;
    public static bool isArmed = false;
    public static bool inBossFight = false;
    public static bool finishedFinalBoss = false;

    public static Vector2 camCenter;
    public static Vector2 camBoundaryBuffers;
    public static Vector2 camTempBuffers;
    public static Vector2 camTempBuffersX;
    public static Vector2 camTempBuffersY;
    public static Vector2 posRelativeToTempBuffers;
    public static Vector2 camTempBufferTruePos;
    public static Vector2 camCutsceneOffset;
    public static Vector2 camShakeOffset;

    public static readonly Vector2 WORLD_ORIGIN = new Vector2(0.5f, 0.5f); // The exact center of the chartable map
    public static readonly Vector2 WORLD_SIZE = new Vector2(26, 22); // The number of screens wide and tall the world is
    public static readonly Vector2 WORLD_SPAWN = new Vector2(-37, 10.5f); // Use (-37, 10.5f) for Snail Town spawn, (84, 88.5f) for debug room spawn
    public static readonly Vector2 ROOM_SIZE = new Vector2(26, 16); // The number of tiles wide and tall each screen is, counting the buffer space that makes up room borders
    public static Vector2 respawnCoords = WORLD_SPAWN;
    public static Scene respawnScene;

    public static TextMesh fpsText;
    public static TextMesh fpsShadow;
    public static TextMesh timeText;
    public static TextMesh timeShadow;
    public static TextMesh pauseText;
    public static TextMesh pauseShadow;
    public static TextMesh mapText;
    public static TextMesh mapShadow;

    public enum TargetTypes
    {
        MoonTele,
        MoonMove,
        GigaSpawn,
        GigaStomp
    };
    public struct TargetPoint
    {
        public TargetTypes type;
        public Vector2 pos;
        public EDirsCompass[] directions;
    }
    public static List<TargetPoint> activeTargets = new();

    public static List<GameObject> finalBossTiles = new();

    public static int currentProfileNumber = 0;

    public static readonly int[] defaultMinimapState = new int[]
    {
        -1,  0,  0, -1, -1,  2,  0, -1, -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
        -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
        -1, -1,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1,
        -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1,
         0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  2,  2,  0,  0,  0,  0,  0, -1, -1, -1, -1,
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

    public static bool[][] itemData = new bool[][] { };
    
    public static readonly float[] timeDefault = new float[]
    {
        0, 0, 0,  // Snaily Normal
        0, 0, 0,  // Snaily Insane
        0, 0, 0,  // Snaily 100%
        0, 0, 0,  // Snaily Boss Rush
        0, 0, 0,  // Sluggy Normal
        0, 0, 0,  // Sluggy Insane
        0, 0, 0,  // Sluggy 100%
        0, 0, 0,  // Sluggy Boss Rush
        0, 0, 0,  // Upside Normal
        0, 0, 0,  // Upside Insane
        0, 0, 0,  // Upside 100%
        0, 0, 0,  // Upside Boss Rush
        0, 0, 0,  // Leggy  Normal
        0, 0, 0,  // Leggy  Insane
        0, 0, 0,  // Leggy  100%
        0, 0, 0,  // Leggy  Boss Rush
        0, 0, 0,  // Blobby Normal
        0, 0, 0,  // Blobby Insane
        0, 0, 0,  // Blobby 100%
        0, 0, 0,  // Blobby Boss Rush
        0, 0, 0,  // Leechy Normal
        0, 0, 0,  // Leechy Insane
        0, 0, 0,  // Leechy 100%
        0, 0, 0   // Leechy Boss Rush
    };
    
    public const byte OFFSET_HEARTS = 13;
    public const byte MAX_HEARTS = 11;
    public const byte OFFSET_FRAGMENTS = 24;
    public const byte MAX_FRAGMENTS = 30;

    public static int[] NPCvarDefault = new int[] { 0, 0 };

    public static List<string[]> cutsceneData = new List<string[]>();
    public static List<int> cutscenesToNotSpawn = new List<int> { };

    public const string SAVE_FILE_PREFIX = "SnailySave";

    public enum NPCVarIDs
    {
        HasSeenIris,
        TalkedToCaveSnail
    };

    [Serializable]
    public struct ProfileData
    {
        public bool isEmpty;          // Controls if this profile is considered empty on the profile selection screen
        public int difficulty;        // 0 = Easy, 1 = Normal, 2 = Insane
        public float[] gameTime;      // Length-3 array tracking hours, minutes, and seconds
        public Vector2 saveCoords;    // X and Y coordinates of the last save point the player touched (or the start of the campaign if none has been touched)
        public string character;      // Snaily, Sluggy, Upside, Leggy, Blobby, and Leechy (case-sensitive)
        public int[] items;           // See below item table (0 = uncollected, 1 = collected)
        public int weapon;            // Last selected weapon ID
        public int[] bossStates;      // Length-4 array tracking which bosses have been defeated (0 = defeated, 1 = alive)
        public int[] NPCVars;         // Variable-length array tracking certain variables NPC read and write, such as Cave Snail and Iris
        public int percentage;        // Item percentage
        public int[] exploredMap;     // Length-(26x22) array tracking the individual states of all map cells (see below map table for legend)
        public int[] cutsceneStates;  // Variable-length array tracking certain variables that cutscenes can control
    }

    /*\
     |  Item Table
     |
     |   0  - - Peashooter
     |   1  - - Boomerang
     |   2  - - Rainbow Wave
     |   3  - - Devastator
     |   4  - - High Jump          Wall Grab
     |   5  - - Shell Shield       Shelmet
     |   6  - - Rapid Fire         Backfire
     |   7  - - Ice Snail
     |   8  - - Gravity Snail      Magnetic Foot      Corkscrew Jump       Angel Jump
     |   9  - - Full-Metal Snail
     |  10  - - Gravity Shock
     |  11  - - Super Secret Boomerang
     |  12  - - Debug Rainbow Wave
     |  13-23 - Heart Containers
     |  24-53 - Helix Fragments
    \*/

    /*\
     |  Map legend
     | 
     |  -1 - Blank tile
     |   0 - Unexplored tile
     |   1 - Explored tile
     |   2 - Unexplored secret tile
     |   3 - Explored secret tile
     |  10 - Unexplored marked tile
     |  11 - Explored marked tile
     |  12 - Unexplored marked secret tile
     |  13 - Explored marked secret tile
    \*/

    [Serializable]
    public struct GeneralData
    {
        public string gameVersion;

        // OPTIONS
        public int soundVolume;             // Sound volume (0-10)
        public int musicVolume;             // Music volume (0-10)
        public int windowSize;              // Window resolution (0, 1, 2, or 3 (plus 1) for each zoom level)
        public int minimapState;            // Minimap display (0 = hidden, 1 = only minimap, 2 = minimap and room names)
        public int bottomKeyState;          // Display bottom keys (0 = hidden, 1 = only weapon icons, 2 = all)
        public bool keymapState;            // Display keymap
        public bool timeState;              // Time display
        public bool FPSState;               // FPS counter
        public bool shootMode;              // Shoot mode (Normal VS Toggle)
        public string texturePackID;        // Folder name of the currently-used texture pack (if none, set to DEFAULT)
        public string musicPackID;          // Folder name of the currently-used music pack (if none, set to DEFAULT)
        public string soundPackID;          // Folder name of the currently-used sound pack (if none, set to DEFAULT)
        public string textPackID;           // Folder name of the currently-used text pack (if none, set to DEFAULT)
        public int particleState;           // Particle settings (0 = none, 1 = environments only, 2 = Flash entities, 3 = all entities, 4 = Flash, 5 = all)
        public int breakableState;          // Breakable block reveal settings (0 = off, 1 = obvious, 2 = all)
        public bool secretMapTilesVisible;  // Secret map tile visibility
        public int frameLimiter;            // Frame limiter (0 = unlimited, 1 = 30fps, 2 = 60fps, 3 = 120fps)
        public int screenShake;             // Screen shake (0 = off, 1 = minimal, 2 = full, 3 = minimal w/ no HUD shake, 4 = full w/ no HUD shake)
        public bool paletteFilterState;     // Palette filter
        public int controllerFaceType;      // Controller face button type (0 = Xbox, 1 = Nintendo, 2 = PlayStation, 3 = Ouya)
        public KeyCode[] keyboardInputs;
        public KeyCode[] controllerInputs;

        // RECORDS
        public bool[] achievements;
        public float[] times;
    }

    public static readonly ProfileData blankProfile = new()
    {
        isEmpty = true,
        difficulty = 0,
        gameTime = new float[] { 0f, 0f, 0f },
        saveCoords = WORLD_SPAWN,
        character = "Snaily",
        items = new int[54],
        weapon = -1,
        bossStates = new int[] { 1, 1, 1, 1 },
        NPCVars = new int[] { 0, 0 },
        percentage = 0,
        exploredMap = defaultMinimapState,
        cutsceneStates = new int[] { }
    };

    public static GeneralData blankData = new()
    {
        soundVolume = 10,
        musicVolume = 10,
        windowSize = 2,
        minimapState = 2,
        bottomKeyState = 2,
        keymapState = false,
        timeState = false,
        FPSState = false,
        shootMode = false,
        texturePackID = "DEFAULT",
        musicPackID = "DEFAULT",
        soundPackID = "DEFAULT",
        textPackID = "DEFAULT",
        particleState = 5,
        breakableState = 0,
        secretMapTilesVisible = false,
        frameLimiter = 2,
        screenShake = 2,
        paletteFilterState = false,
        controllerFaceType = 0,
        keyboardInputs = Control.defaultKeyboardInputs,
        controllerInputs = Control.defaultControllerInputs,
        achievements = new bool[Enum.GetNames(typeof(AchievementPanel.Achievements)).Length],
        times = (float[])timeDefault.Clone()
    };

    public static ProfileData profile1 = blankProfile;
    public static ProfileData profile2 = blankProfile;
    public static ProfileData profile3 = blankProfile;
    public static ProfileData currentProfile = blankProfile;
    public static GeneralData generalData = blankData;

    public static Sprite BlankTexture(bool useSmallBlank = false) {
        return useSmallBlank ? globalFunctions.blankSmall : globalFunctions.blank;
    }

    public static Sprite MissingTexture() {
        return globalFunctions.missing;
    }

    public static AnimationData GetAnim(string name) {
        AnimationData foundData = new AnimationData {
            name = "NoAnim"
        };
        int i = 0;
        while (foundData.name == "NoAnim" && i < animationLibrary.Length) {
            if (animationLibrary[i].name == name)
                foundData = animationLibrary[i];
            i++;
        }
        return foundData;
    }

    public static void PrintAllAnims() {
        string output = "";
        for (int i = 0; i < animationLibrary.Length; i++)
            output += animationLibrary[i].name + "\n";
        Debug.Log(output);
    }

    public static void RefreshPoolAnims() {
        foreach (Transform obj in particlePool.transform)
            obj.GetComponent<AnimationModule>().ReloadList();
        foreach (Transform obj in globalFunctions.playerBulletPool.transform)
            obj.GetComponent<AnimationModule>().ReloadList();
        foreach (Transform obj in enemyBulletPool.transform)
            obj.GetComponent<AnimationModule>().ReloadList();
    }

    public static AnimationData GetAnim(int ID) {
        return animationLibrary[ID];
    }

    public static int GetAnimID(string name) {
        AnimationData foundData = new AnimationData {
            name = "NoAnim"
        };
        int i = 0;
        while (foundData.name == "NoAnim" && i < animationLibrary.Length) {
            if (animationLibrary[i].name == name)
                foundData = animationLibrary[i];
            else
                i++;
        }
        return i;
    }

    public static Sprite GetSprite(string name, int ID = 0) {
        Sprite newSprite = MissingTexture();
        int i = 0;
        bool found = false;
        while (i < textureLibrary.referenceList.Length && !found) {
            if (textureLibrary.referenceList[i] == name)
                found = true;
            i++;
        }
        if (found) {
            if (ID < textureLibrary.library[Array.IndexOf(textureLibrary.referenceList, name)].Length)
                newSprite = textureLibrary.library[Array.IndexOf(textureLibrary.referenceList, name)][ID];
        }
        return newSprite;
    }

    public static AudioClip GetSound(string name) {
        return soundLibrary.library[soundLibrary.soundDict[name]];
    }

    public static AudioClip GetMusic(int groupIndex, string name) {
        return musicLibrary.library[groupIndex][Array.IndexOf(musicLibrary.library[groupIndex], name)];
    }
    public static AudioClip GetMusic(int groupIndex, int songIndex) {
        return musicLibrary.library[groupIndex][songIndex];
    }

    public static void PlaySound(string name) {
        if (GetSound(name) != null)
            globalSFX.PlayOneShot(GetSound(name));
        else
            Debug.LogWarning("Audioclip \"" + name + "\" does not exist!");
    }
    public static void PlaySound(AudioClip clip) {
        globalSFX.PlayOneShot(clip);
    }

    public static void PlayMusic(int groupIndex, string name) {
        if (GetMusic(groupIndex, name) != null)
            globalMusic.PlayOneShot(GetMusic(groupIndex, name));
        else
            Debug.LogWarning("Audipclip \"" + name + "\" does not exist!");
    }
    public static void PlayMusic(int groupIndex, int songIndex) {
        globalMusic.PlayOneShot(GetMusic(groupIndex, songIndex));
    }

    public static void MuteMusic() {
        globalFunctions.musicMuted = true;
    }

    public static void FadeMusicBackIn() {
        globalFunctions.musicMuted = false;
    }

    public static Color32 GetColor(string ID) {
        return palette.GetPixel(int.Parse(ID.Substring(0, 2)) % 4, int.Parse(ID.Substring(2, 2)) % 14);
    }

    public static string GetText(string ID) {
        int i = 0;
        bool found = false;
        while (i < textLibrary.library.Length && !found) {
            if (textLibrary.library[i].name == ID)
                return textLibrary.library[i].text;
            i++;
        }
        return ID;
    }

    public static TextDict GetTextInfo(string ID) {
        int i = 0;
        bool found = false;
        while (i < textLibrary.library.Length && !found) {
            if (textLibrary.library[i].name == ID)
                return textLibrary.library[i];
            i++;
        }
        return new TextDict {
            name = "missing",
            text = "Text with ID \"" + ID + "\" not found",
            value = 0
        };
    }

    public static string GetIDFromText(string text) {
        int i = 0;
        bool found = false;
        while (i < textLibrary.library.Length && !found) {
            if (textLibrary.library[i].text == text)
                return textLibrary.library[i].name;
            i++;
        }
        return "ID with text \"" + text + "\" not found";
    }

    public static Sprite Colorize(string sprite, int spriteNum, string table, int tableValue) {
        Texture2D colorTable = GetSprite(table).texture;
        Dictionary<Color32, int> referenceColors = new Dictionary<Color32, int>();
        for (int i = 0; i < colorTable.width; i++) {
            referenceColors.Add(colorTable.GetPixel(i, 0), i);
        }

        Sprite oldSprite = GetSprite(sprite, spriteNum);
        Texture2D newSprite = new Texture2D((int)oldSprite.rect.width, (int)oldSprite.rect.height);
        Color[] pixels = oldSprite.texture.GetPixels((int)oldSprite.textureRect.x,
            (int)oldSprite.textureRect.y,
            (int)oldSprite.textureRect.width,
            (int)oldSprite.textureRect.height);
        for (int j = 0; j < pixels.Length; j++) {
            if (pixels[j].r == 0.9960785f && pixels[j].g == 0.9960785f && pixels[j].b == 0.9960785f)
                pixels[j] = new Color(0, 0, 0, 0);
            else if (referenceColors.ContainsKey(pixels[j]))
                pixels[j] = colorTable.GetPixel(referenceColors[pixels[j]], tableValue + 1);
        }
        newSprite.SetPixels(pixels);
        newSprite.Apply();

        return Sprite.Create(newSprite, new Rect(0, 0, newSprite.width, newSprite.height), new Vector2(0.5f, 0.5f), 16);
    }

    public static string ParseColorCodeToString(int colorData) {
        return "" + (colorData < 1000 ? "0" : "") + (colorData < 100 ? "0" : "") + (colorData < 100 ? "0" : "") + colorData.ToString();
    }

    public static Vector2 WorldPosToMapPos(Vector2 worldPos) {
        Vector2 topLeftCorner = new Vector2(WORLD_ORIGIN.x - (WORLD_SIZE.x * ROOM_SIZE.x * 0.5f), WORLD_ORIGIN.y + (WORLD_SIZE.y * ROOM_SIZE.y * 0.5f));
        return new Vector2(Mathf.Floor(Mathf.Abs(topLeftCorner.x - worldPos.x) / ROOM_SIZE.x), Mathf.Floor(Mathf.Abs(topLeftCorner.y - worldPos.y) / ROOM_SIZE.y));
    }

    public static int WorldPosToMapGridID(Vector3 worldPos) {
        return WorldPosToMapGridID(new Vector2(worldPos.x, worldPos.y));
    }
    public static int WorldPosToMapGridID(Vector3Int worldPos) {
        return WorldPosToMapGridID(new Vector2(worldPos.x, worldPos.y));
    }
    public static int WorldPosToMapGridID(Vector2 worldPos) {
        Vector2 mapPos = WorldPosToMapPos(worldPos);
        return Mathf.RoundToInt(mapPos.y * WORLD_SIZE.x + mapPos.x);
    }

    public static void BuildMapMarkerArrays() {
        saveLocations.Clear();
        bossLocations.Clear();
        itemLocations.Clear();
        foreach (Transform area in roomTriggerParent.transform) {
            foreach (Transform room in area) {
                foreach (Transform entity in room) {
                    if (entity.CompareTag("SavePoint"))
                        saveLocations.Add(WorldPosToMapGridID(entity.transform.position));
                    if (entity.CompareTag("Item"))
                        itemLocations.Add(WorldPosToMapGridID(entity.transform.position), entity.GetComponent<Item>().itemID);
                }
            }
        }

        Tilemap spMap = specialLayer.GetComponent<Tilemap>();
        for (int y = 0; y < spMap.size.y; y++) {
            for (int x = 0; x < spMap.size.x; x++) {
                List<int> bossTileIDs = new List<int> { 23, 24, 25, 26 };
                Vector3Int worldPos = new Vector3Int(Mathf.RoundToInt(spMap.origin.x - (spMap.size.x * 0.5f) + x), Mathf.RoundToInt(spMap.origin.y - (spMap.size.y * 0.5f) + y), 0);
                Sprite tileSprite = spMap.GetSprite(worldPos);
                if (tileSprite != null) {
                    int spriteID = int.Parse(tileSprite.name.Split('_', ' ')[1]);
                    if (bossTileIDs.Contains(spriteID))
                        bossLocations.Add(WorldPosToMapGridID(worldPos));
                }
            }
        }
    }

    public static void BuildPlayerMarkerArray() {
        playerMarkerLocations.Clear();
        for (int i = 0; i < currentProfile.exploredMap.Length; i++)
            if (currentProfile.exploredMap[i] >= 10)
                playerMarkerLocations.Add(i, "placeholder for multiplayer name");
    }

    public static void ReplaceAllTempTiles() // x, y, layer, original tile ID
    {
        while (tempTiles.Count > 0) {
            Vector3Int position = new Vector3Int(tempTiles[0], tempTiles[1], 0);
            Tilemap map = tempTiles[2] switch {
                0 => specialLayer.GetComponent<Tilemap>(),
                1 => fg2Layer.GetComponent<Tilemap>(),
                2 => fg1Layer.GetComponent<Tilemap>(),
                3 => groundLayer.GetComponent<Tilemap>(),
                4 => bgLayer.GetComponent<Tilemap>(),
                5 => skyLayer.GetComponent<Tilemap>(),
                _ => specialLayer.GetComponent<Tilemap>()
            };
            if (tempTiles[3] == -1)
                map.SetTile(position, null);
            else {
                Tile newTile = ScriptableObject.CreateInstance<Tile>();
                Sprite newSprite = PlayState.GetSprite("Tilesheet", tempTiles[3]);
                newSprite.OverridePhysicsShape(new List<Vector2[]> {
                    new Vector2[] { new Vector2(0, 0), new Vector2(0, 16), new Vector2(16, 16), new Vector2(16, 0) }
                    });
                newTile.sprite = newSprite;
                newTile.name = "Tilesheet_" + tempTiles[3];
                map.SetTile(position, newTile);
            }
        }
    }

    public static void PlayAreaSong(int area, int subzone) {
        if (area == currentArea && subzone != currentSubzone) {
            globalFunctions.UpdateMusic(area, subzone);
        } else if (area != currentArea) {
            globalFunctions.UpdateMusic(area, subzone, 1);
        }
        currentArea = area;
        currentSubzone = subzone;
    }

    public static bool IsTileSolid(Vector2 tilePos, bool checkForEnemyCollide = false) {
        if (breakablePositions.Contains(new Vector2(Mathf.Floor(tilePos.x), Mathf.Floor(tilePos.y))))
            return true;
        else {
            Vector2 gridPos = new Vector2(Mathf.Floor(tilePos.x), Mathf.Floor(tilePos.y));
            bool result = groundLayer.GetComponent<Tilemap>().GetTile(new Vector3Int((int)gridPos.x, (int)gridPos.y, 0)) != null;
            if (!result && checkForEnemyCollide) {
                TileBase spTile = specialLayer.GetComponent<Tilemap>().GetTile(new Vector3Int((int)gridPos.x, (int)gridPos.y, 0));
                if (spTile != null)
                    if (spTile.name == "Tilesheet_376")
                        result = true;
            }
            return result;
        }
    }

    public static bool IsPointPlayerCollidable(Vector2 pos)
    {
        if (IsTileSolid(pos))
            return true;
        List<Transform> platforms = new List<Transform>();
        Transform room = roomTriggerParent.transform.GetChild((int)positionOfLastRoom.x).GetChild((int)positionOfLastRoom.y);
        for (int i = 0; i < room.childCount; i++)
        {
            if (room.GetChild(i).gameObject.CompareTag("Platform"))
                platforms.Add(room.GetChild(i));
        }
        foreach (Transform platform in platforms)
        {
            Transform platObj = platform.GetChild(0);
            BoxCollider2D platBox = platObj.GetComponent<BoxCollider2D>();
            Vector2 a = (Vector2)platObj.position - (platBox.size * 0.5f);
            Vector2 b = (Vector2)platObj.position + (platBox.size * 0.5f);
            if (pos.x > a.x && pos.x < b.x && pos.y > a.y && pos.y < b.y)
                return true;
        }
        return false;
    }

    public static bool IsPointEnemyCollidable(Vector2 pos)
    {
        if (IsTileSolid(pos, true) || IsPointPlayerCollidable(pos))
            return true;
        return false;
    }

    public static void ToggleHUD(bool state) {
        foreach (GameObject element in TogglableHUDElements) {
            element.SetActive(state);
            if (state) {
                if (element.name == "Minimap Panel")
                    element.SetActive(!inBossFight);
                if (element.name == "Boss Health Bar")
                    element.SetActive(true);
            }
        }
    }

    public static void RunItemPopup(string item) {
        playbackTime = activeMus.time;
        mus1.Stop();
        areaMus = activeMus.clip;
        PlayMusic(0, 2);
        List<string> text = new List<string>();
        List<Color32> colors = new List<Color32>();
        switch (item) {
            default:
                text.Add("skibidi bop mm dada");
                break;
        }
        for (int i = 0; i < text.Count; i++) {
            colors.Add(new Color32(0, 0, 0, 0));
            colors.Add(new Color32(0, 0, 0, 0));
            colors.Add(new Color32(0, 0, 0, 0));
        }
        gameState = GameState.dialogue;
        dialogueScript.RunBox(1, 0, text, 0, "0005");
    }

    public static void OpenDialogue(int type, int speaker, List<string> text, int shape, string boxColor = "0005", List<int> stateList = null, bool facingLeft = false) {
        dialogueScript.RunBox(type, speaker, text, shape, boxColor, stateList, facingLeft);
    }

    public static void CloseDialogue() {
        isTalking = false;
        dialogueScript.CloseBox();
    }

    public static void StallDialogueContinuous(CutsceneController cutscene, float lingerTime) {
        dialogueScript.StallCutsceneDialogue(cutscene, lingerTime);
    }

    public static void StallDialoguePrompted(CutsceneController cutscene) {
        dialogueScript.StallCutsceneDialoguePrompted(cutscene);
    }

    public static void ScreenFlash(string type, int red = 0, int green = 0, int blue = 0, int alpha = 0, float maxTime = 0, int sortingOrder = 1001) {
        switch (type) {
            default:
            case "Solid Color":
                screenCover.color = new Color32((byte)red, (byte)green, (byte)blue, (byte)alpha);
                break;
            case "Room Transition":
                screenCover.color = new Color32(0, 0, 0, 200);
                globalFunctions.ExecuteCoverCommand(type);
                break;
            case "Death Transition":
                globalFunctions.ExecuteCoverCommand(type);
                break;
            case "Custom Fade":
                globalFunctions.ExecuteCoverCommand(type, (byte)red, (byte)green, (byte)blue, (byte)alpha, maxTime, sortingOrder);
                break;
        }
    }

    public static Particle RequestParticle(Vector2 position, string type) {
        return RequestParticle(position, type, new float[] { 0 }, false);
    }
    public static Particle RequestParticle(Vector2 position, string type, float[] values) {
        return RequestParticle(position, type, values, false);
    }
    public static Particle RequestParticle(Vector2 position, string type, bool playSound) {
        return RequestParticle(position, type, new float[] { 0 }, playSound);
    }

    public static Particle RequestParticle(Vector2 position, string type, float[] values, bool playSound) {
        Particle selectedParticle = null;
        bool found = false;
        if (particlePool.transform.GetChild(thisParticleID).gameObject.activeSelf) {
            int i = 0;
            while (i < particlePool.transform.childCount - 1 && !found) {
                thisParticleID++;
                if (thisParticleID >= particlePool.transform.childCount)
                    thisParticleID = 0;
                if (!particlePool.transform.GetChild(thisParticleID).gameObject.activeSelf)
                    found = true;
                i++;
            }
        } else
            found = true;

        if (found) {
            Transform particleObject = particlePool.transform.GetChild(thisParticleID);
            particleObject.gameObject.SetActive(true);
            Particle particleScript = particleObject.GetComponent<Particle>();

            bool activateParticle = false;

            switch (type.ToLower()) { // Particle settings - 0 = none, 1 = environments only, 2 = Flash entities, 3 = all entities, 4 = Flash, 5 = all
                default:
                    break;
                case "bubble":
                    // Values:
                    // 0 = Water level
                    // 1 = Boolean to initialize particle with random velocity or not

                    if (generalData.particleState == 1 || generalData.particleState >= 4) {
                        activateParticle = true;
                        particleScript.vars[0] = UnityEngine.Random.Range(0, 2 * Mathf.PI);       // Animation cycle
                        particleScript.vars[1] = position.x;                                      // Origin X
                        particleScript.vars[2] = values[0] - 0.25f;                               // Water level above the bubble's spawn
                        particleScript.vars[3] = 4 + UnityEngine.Random.Range(0f, 1f) * 0.0625f;  // Rise speed
                        particleScript.vars[4] = values[1];                                       // Randomize initial velocity
                    }
                    break;
                case "dust":
                    if (generalData.particleState > 1)
                        activateParticle = true;
                    break;
                case "explosion":
                    // Values:
                    // 0 = Size

                    if ((generalData.particleState > 1 && values[0] <= 4) || ((generalData.particleState == 3 || generalData.particleState == 5) && values[0] > 4)) {
                        activateParticle = true;
                        particleScript.vars[0] = values[0];
                    }
                    break;
                case "heat":
                    if (generalData.particleState == 1 || generalData.particleState == 5)
                    {
                        activateParticle = true;
                        particleScript.vars[0] = UnityEngine.Random.Range(0, 2 * Mathf.PI);  // Animation cycle
                        particleScript.vars[1] = UnityEngine.Random.Range(3f, 7f);           // Initial velocity
                        particleScript.vars[2] = UnityEngine.Random.Range(0.5f, 3f);         // Deceleration
                        particleScript.vars[3] = position.x;                                 // Origin X
                        particleScript.vars[4] = UnityEngine.Random.Range(0.5f, 1.25f);      // Sine amplitude
                    }
                    break;
                case "nom":
                    // Values:
                    // 0 = Start Y

                    if (generalData.particleState > 1) {
                        activateParticle = true;
                        particleScript.vars[0] = position.y;
                    }
                    break;
                case "smoke":
                    if (generalData.particleState == 1 || generalData.particleState >= 4)
                        activateParticle = true;
                    break;
                case "snow":
                    if (generalData.particleState == 1 || generalData.particleState >= 4) {
                        activateParticle = true;
                        particleScript.vars[0] = 1.875f + UnityEngine.Random.Range(0f, 1f) * 3.75f;  // Downward velocity
                        particleScript.vars[1] = UnityEngine.Random.Range(0f, 1f) * Mathf.PI * 2;    // Sine loop start
                    }
                    break;
                case "splash":
                    if (generalData.particleState == 1 || generalData.particleState == 3 || generalData.particleState == 5)
                        activateParticle = true;
                    break;
                case "star":
                    if ((generalData.particleState == 4 && values[0] == 6) || generalData.particleState == 1 || generalData.particleState == 5)
                    {
                        activateParticle = true;
                        particleScript.vars[0] = values[0];
                        particleScript.vars[1] = (UnityEngine.Random.Range(0f, 1f) * 5f - 0.5f) * Time.fixedDeltaTime;
                        particleScript.vars[2] = (UnityEngine.Random.Range(0f, 1f) * 6f + 3f) * Time.fixedDeltaTime;
                    }
                    break;
                case "transformation":
                    // Values:
                    // 0 = Type

                    if (generalData.particleState == 3 || generalData.particleState == 5) {
                        activateParticle = true;
                        particleScript.vars[0] = values[0];
                    }
                    break;
                case "zzz":
                    if (generalData.particleState > 1)
                        activateParticle = true;
                    break;
            } // Particle settings - 0 = none, 1 = environments only, 2 = Flash entities, 3 = all entities, 4 = Flash, 5 = all

            if (activateParticle) {
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

    public static void ResetAllParticles()
    {
        foreach (Transform particle in particlePool.transform)
        {
            Particle particleScript = particle.GetComponent<Particle>();
            if (particle.gameObject.activeSelf)
                particleScript.ResetParticle();
        }
        for (int i = camParticlePool.transform.childCount - 1; i >= 0; i--)
        {
            Transform particle = camParticlePool.transform.GetChild(i);
            if (particle.gameObject.activeSelf)
                particle.GetComponent<Particle>().ResetParticle();
        }
    }

    public static bool IsBossAlive(int bossID) {
        return currentProfile.bossStates[bossID] == 1;
    }

    public static bool CheckForItem(int itemID) {
        return currentProfile.items[itemID] == 1;
    }

    public static bool CheckForItem(string itemName) {
        return currentProfile.items[TranslateItemNameToID(itemName)] == 1;
    }

    public static bool CheckShellLevel(int level) {
        bool meetsLevel;
        if (currentProfile.difficulty == 2) {
            meetsLevel = level switch {
                2 => CheckForItem(8),
                3 => CheckForItem(9),
                _ => CheckForItem(7),
            };
        } else {
            meetsLevel = level switch {
                2 => CheckForItem(8) || CheckForItem(9),
                3 => CheckForItem(9),
                _ => CheckForItem(7) || CheckForItem(8) || CheckForItem(9),
            };
        }
        return meetsLevel;
    }

    public static int GetShellLevel() {
        return CheckForItem(9) ? 3 : (CheckForItem(8) ? 2 : (CheckForItem(7) ? 1 : 0));
    }

    public static void SetPlayer(string newPlayer)
    {
        playerScript.GetComponent<Snaily>().enabled = newPlayer == "Snaily";
        //playerScript.GetComponent<Sluggy>().enabled = newPlayer == "Sluggy";
        //playerScript.GetComponent<Upside>().enabled = newPlayer == "Upside";
        //playerScript.GetComponent<Leggy>().enabled = newPlayer == "Leggy";
        //playerScript.GetComponent<Blobby>().enabled = newPlayer == "Blobby";
        //playerScript.GetComponent<Leechy>().enabled = newPlayer == "Leechy";
        currentProfile.character = newPlayer;
        playerScript = player.GetComponent<Player>();
    }

    public static void AddItem(int itemID) {
        currentProfile.items[itemID] = 1;
    }

    public static void AddItem(string itemName) {
        currentProfile.items[TranslateItemNameToID(itemName)] = 1;
    }

    public static void AssignProperCollectibleIDs() {
        Transform roomTriggerArray = GameObject.Find("Room Triggers").transform;
        int currentHelixCount = 0;
        int currentHeartCount = 0;

        foreach (Transform area in roomTriggerArray) {
            foreach (Transform room in area) {
                foreach (Transform entity in room) {
                    if (entity.name == "Item") {
                        if (entity.GetComponent<Item>().itemID >= OFFSET_FRAGMENTS) {
                            entity.GetComponent<Item>().itemID = OFFSET_FRAGMENTS + currentHelixCount;
                            currentHelixCount++;
                        } else if (entity.GetComponent<Item>().itemID >= OFFSET_HEARTS) {
                            entity.GetComponent<Item>().itemID = OFFSET_HEARTS + currentHeartCount;
                            currentHeartCount++;
                        }
                    }
                }
            }
        }
    }

    public static byte TranslateItemNameToID(string itemName) {
        byte id = 0;
        switch (itemName) {
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

    public static string TranslateIDToItemName(int itemID) {
        string name = "";
        if (itemID >= OFFSET_FRAGMENTS)
            name = "Helix Fragment";
        else if (itemID >= OFFSET_HEARTS)
            name = "Heart Container";
        else {
            switch (itemID) {
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
                    if (currentProfile.character == "Blobby")
                        name = "Wall Grab";
                    else
                        name = "High Jump";
                    break;
                case 5:
                    if (currentProfile.character == "Blobby")
                        name = "Shelmet";
                    else
                        name = "Shell Shield";
                    break;
                case 6:
                    if (currentProfile.character == "Leechy")
                        name = "Backfire";
                    else
                        name = "Rapid Fire";
                    break;
                case 7:
                    name = "Ice Snail";
                    break;
                case 8:
                    if (currentProfile.character == "Upside")
                        name = "Magnetic Foot";
                    else if (currentProfile.character == "Leggy")
                        name = "Corkscrew Jump";
                    else if (currentProfile.character == "Blobby")
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

    public static int GetMapPercentage() {
        int explored = 0;
        int total = 0;
        foreach (int i in currentProfile.exploredMap) {
            if (i != -1) {
                total++;
                if (i == 1)
                    explored++;
            }
        }
        return Mathf.FloorToInt(((float)explored / (float)total) * 100);
    }

    public static int GetItemPercentage() {
        int itemsFound = 0;
        int totalCount = 0;
        int charCheck = currentProfile.character switch { "Snaily" => 3, "Sluggy" => 4, "Upside" => 5, "Leggy" => 6, "Blobby" => 7, "Leechy" => 8, _ => 3 };
        for (int i = 0; i < currentProfile.items.Length; i++) {
            if (itemData[i] != null) {
                if (itemData[i][currentProfile.difficulty] && itemData[i][charCheck]) {
                    totalCount++;
                    itemsFound += currentProfile.items[i] == 1 ? 1 : 0;
                }
            } else
                totalCount++;
        }
        return Mathf.FloorToInt(((float)itemsFound / (float)totalCount) * 100);
    }

    public static string GetTimeString() {
        string hourInt = currentProfile.gameTime[0] < 10 ? "0" + currentProfile.gameTime[0] : (currentProfile.gameTime[0] == 0 ? "00" : currentProfile.gameTime[0].ToString());
        string minuteInt = currentProfile.gameTime[1] < 10 ? "0" + currentProfile.gameTime[1] : (currentProfile.gameTime[1] == 0 ? "00" : currentProfile.gameTime[1].ToString());
        string secondsInt = (Mathf.RoundToInt(currentProfile.gameTime[2] * 100) + 10000).ToString();
        return hourInt + ":" + minuteInt + ":" + secondsInt.Substring(1, 2) + "." + secondsInt.Substring(3, 2);
    }

    public static void SetMapTile(Vector2 pos, bool state) {
        int cellID = Mathf.RoundToInt((WORLD_SIZE.x * pos.y) + pos.x);
        int currentCellState = currentProfile.exploredMap[cellID];
        bool marked = false;
        if (currentCellState >= 10) {
            currentCellState -= 10;
            marked = true;
        }
        currentProfile.exploredMap[cellID] = currentCellState > 1 ? (state ? 3 : 2) : (state ? 1 : 0);
        if (marked)
            currentProfile.exploredMap[cellID] += 10;
        minimapScript.RefreshMap();
    }

    public static int CountHearts()
    {
        int count = 0;
        for (int i = OFFSET_HEARTS; i < OFFSET_HEARTS + MAX_HEARTS; i++)
            if (currentProfile.items[i] == 1)
                count++;
        return count;
    }

    public static int CountFragments()
    {
        int count = 0;
        for (int i = OFFSET_FRAGMENTS; i < OFFSET_FRAGMENTS + MAX_FRAGMENTS; i++)
            if (currentProfile.items[i] == 1)
                count++;
        return count;
    }

    public static int GetNPCVar(NPCVarIDs ID)
    {
        return currentProfile.NPCVars[(int)ID];
    }

    public static void SetNPCVar(NPCVarIDs ID, int value)
    {
        currentProfile.NPCVars[(int)ID] = value;
    }

    public static void WriteSave(int profileID, bool saveGeneral) {
        switch (profileID)
        {
            case 1:
                profile1 = currentProfile;
                File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_Profile1.json", JsonUtility.ToJson(profile1));
                break;
            case 2:
                profile2 = currentProfile;
                File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_Profile2.json", JsonUtility.ToJson(profile2));
                break;
            case 3:
                profile3 = currentProfile;
                File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_Profile3.json", JsonUtility.ToJson(profile3));
                break;
        }
        if (saveGeneral)
            File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_OptionsAndRecords.json", JsonUtility.ToJson(generalData));
    }

    public static void SaveAll()
    {
        File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_Profile1.json", JsonUtility.ToJson(profile1));
        File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_Profile2.json", JsonUtility.ToJson(profile2));
        File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_Profile3.json", JsonUtility.ToJson(profile3));
        File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_OptionsAndRecords.json", JsonUtility.ToJson(generalData));
    }

    public static void CopySave(int copiedDataID, int destinationID)
    {
        if (copiedDataID > 0 && copiedDataID <= 3)
            File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_Profile" + destinationID + ".json",
                JsonUtility.ToJson(copiedDataID switch { 1 => profile1, 2 => profile2, _ => profile3 }));
    }

    public static ProfileData LoadGame(int profile, bool setAsCurrent)
    {
        ProfileData thisProfile = blankProfile;
        string path = Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_Profile" + profile + ".json";
        if (File.Exists(path))
        {
            thisProfile = JsonUtility.FromJson<ProfileData>(File.ReadAllText(path));
            if (setAsCurrent)
                currentProfile = thisProfile;
        }
        switch (profile)
        {
            case 1:
                profile1 = thisProfile;
                break;
            case 2:
                profile2 = thisProfile;
                break;
            case 3:
                profile3 = thisProfile;
                break;
        }
        return thisProfile;
    }

    public static void LoadAllProfiles()
    {
        LoadGame(1, false);
        LoadGame(2, false);
        LoadGame(3, false);
    }

    public static void LoadAllMainData()
    {
        currentProfile = blankProfile;

        LoadAllProfiles();

        string path = Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_OptionsAndRecords.json";
        if (File.Exists(path))
            generalData = JsonUtility.FromJson<GeneralData>(File.ReadAllText(path));
        else
            generalData = blankData;
    }

    public static void EraseGame(int profile) {
        if (currentProfileNumber == profile)
        {
            currentProfileNumber = 0;
            currentProfile = blankProfile;
        }    
        switch (profile)
        {
            case 1:
                profile1 = blankProfile;
                File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_Profile1.json", JsonUtility.ToJson(profile1));
                break;
            case 2:
                profile2 = blankProfile;
                File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_Profile2.json", JsonUtility.ToJson(profile2));
                break;
            case 3:
                profile3 = blankProfile;
                File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_Profile3.json", JsonUtility.ToJson(profile3));
                break;
        }
    }

    public static void LoadPacks() {
        for (int i = 0; i < 4; i++)
        {
            string packType = i switch { 1 => "Sound", 2 => "Music", 3 => "Text", _ => "Texture" };
            string[] packNames = new string[] { generalData.texturePackID, generalData.soundPackID, generalData.musicPackID, generalData.textPackID };
            bool loadDefault = false;

            if (packNames[i] != "DEFAULT")
            {
                string path = Application.persistentDataPath + "/" + packType + "Packs/" + packNames[i];
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
                else
                    loadDefault = true;
            }
            else
                loadDefault = true;

            if (loadDefault)
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

    public static void CheckControlsAreUpToDate() {
        if (generalData.keyboardInputs.Length != Enum.GetNames(typeof(Control.Keyboard)).Length)
            generalData.keyboardInputs = Control.defaultKeyboardInputs;
        if (generalData.controllerInputs.Length != Enum.GetNames(typeof(Control.Controller)).Length)
            generalData.controllerInputs = Control.defaultControllerInputs;
    }

    public static bool HasTime(int ID = -1) {
        int rowCount = generalData.times.Length / 3;

        if (ID == -1)
        {
            for (int i = 0; i < rowCount; i++)
            {
                int foundZeroes = 0;
                for (int j = 0; j < 3; j++)
                {
                    if (generalData.times[(i * j) + j] == 0)
                        foundZeroes++;
                }
                if (foundZeroes < 3)
                    return true;
            }
            return false;
        }
        else
        {
            if (generalData.times[ID * 3] == 0 && generalData.times[(ID * 3) + 1] == 0 && generalData.times[(ID * 3) + 2] == 0)
                return false;
            return true;
        }
    }

    public static void QueueAchievementPopup(AchievementPanel.Achievements achID) {
        achievement.GetComponent<AchievementPanel>().popupQueue.Add(achID);
    }

    public struct AnimationLibrary {
        public AnimationData[] animArray;
    }
    public static void LoadNewAnimationLibrary(string path) {
        AnimationLibrary newLibrary = JsonUtility.FromJson<AnimationLibrary>(File.ReadAllText(path));
        animationLibrary = newLibrary.animArray;
    }

    public struct SpriteSizeLibrary {
        public SpriteFrameSize[] sizeArray;
    }
    public static void LoadNewSpriteSizeLibrary(string path) {
        SpriteSizeLibrary newLibrary = JsonUtility.FromJson<SpriteSizeLibrary>(File.ReadAllText(path));
        spriteSizeLibrary = newLibrary.sizeArray;
    }

    public struct MusicOffsetLibrary {
        public MusicLoopOffset[] offsetArray;
    }
    public static void LoadNewMusicOffsetLibrary(string path) {
        MusicOffsetLibrary newLibrary = JsonUtility.FromJson<MusicOffsetLibrary>(File.ReadAllText(path));
        musicLoopOffsetLibrary = newLibrary.offsetArray;
    }

    public static void AdjustHUDText() {
        TogglableHUDElements[4].transform.GetChild(0).GetComponent<TextMesh>().text = GetText("hud_gameSaved");
        TogglableHUDElements[4].transform.GetChild(1).GetComponent<TextMesh>().text = GetText("hud_gameSaved");
    }

    public static void ToggleLoadingIcon(bool state) {
        if (state) {
            loadingIcon.SetActive(true);
            loadingIcon.GetComponent<AnimationModule>().Play("Loading");
        } else {
            loadingIcon.GetComponent<AnimationModule>().Stop();
            loadingIcon.SetActive(false);
        }
    }

    public static bool ShootEnemyBullet(Vector2 newOrigin, EnemyBullet.BulletType type, float angle, float newSpeed, bool playSound = true) {
        Vector2 newAngle = Quaternion.Euler(0, 0, angle) * Vector2.up;
        return ShootEnemyBullet(newOrigin, type, new float[] { newSpeed, newAngle.x, newAngle.y }, playSound);
    }
    public static bool ShootEnemyBullet(Vector2 newOrigin, EnemyBullet.BulletType type, float[] dirVelVars, bool playSound = true) {
        bool hasShot = false;
        if (!enemyBulletPool.transform.GetChild(enemyBulletPointer).GetComponent<EnemyBullet>().isActive) {
            enemyBulletPool.transform.GetChild(enemyBulletPointer).GetComponent<EnemyBullet>().Shoot(newOrigin, type, dirVelVars, playSound);
            enemyBulletPointer = (enemyBulletPointer + 1) % enemyBulletPool.transform.childCount;
            hasShot = true;
        }
        return hasShot;
    }

    public static Vector2 DirectionBetween(Vector2 a, Vector2 b) {
        return (b - a).normalized;
    }

    public static void SetCamFocus(Transform point = null) {
        camScript.focusPoint = point;
    }

    public static void SetCamSpeed(float speed = 0.1f) {
        camScript.camSpeed = speed;
    }

    public static void ToggleBossfightState(bool state, int musicID, bool snapDespawnBar = false) {
        inBossFight = state;
        TogglableHUDElements[0].SetActive(!state);
        if (!state && snapDespawnBar) {
            TogglableHUDElements[12].GetComponent<SpriteRenderer>().enabled = false;
            TogglableHUDElements[12].transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
            globalFunctions.displayDefeatText = false;
        } else if (!state && !snapDespawnBar) {
            TogglableHUDElements[12].GetComponent<AnimationModule>().Play("BossBar_frame_despawn");
            TogglableHUDElements[12].transform.GetChild(0).GetComponent<AnimationModule>().Play("BossBar_bar_despawn");
            globalFunctions.displayDefeatText = true;
        }
        if (currentArea != 7) {
            if (state)
                globalFunctions.UpdateMusic(musicID, 0, 1);
            else if (musicID == -1)
                globalFunctions.UpdateMusic(-1, -1, 4);
            else
                globalFunctions.UpdateMusic(currentArea, currentSubzone, 1);
        }
    }

    public static void ToggleGigaTiles(bool state)
    {
        for (int i = 0; i < finalBossTiles.Count; i++)
            finalBossTiles[i].GetComponent<BreakableBlock>().ToggleActive(state);
    }

    public static bool OnScreen(Vector2 position, Collider2D col) {
        if (col == null)
            return false;

        string colType = "";
        if (col.TryGetComponent(out BoxCollider2D box))
            colType = "box";
        if (col.TryGetComponent(out CircleCollider2D circ))
            colType = "circ";
        if (col.TryGetComponent(out PolygonCollider2D poly))
            colType = "poly";
        if (col.TryGetComponent(out CapsuleCollider2D cap))
            colType = "cap";

        Vector2 halfBoxSize;
        switch (colType)
        {
            default:
            case "box":
                halfBoxSize = new Vector2(box.size.x * 0.5f, box.size.y * 0.5f);
                break;
            case "circ":
                halfBoxSize = new Vector2(circ.radius, circ.radius);
                break;
            case "poly":
                float left = 99f;
                float right = -99f;
                float up = -99f;
                float down = 99f;
                foreach (Vector2 point in poly.points)
                {
                    if (point.x < left)
                        left = point.x;
                    if (point.x > right)
                        right = point.x;
                    if (point.y < down)
                        down = point.y;
                    if (point.y > up)
                        up = point.y;
                }
                halfBoxSize = new Vector2(Mathf.Abs(right - left) * 0.5f, Mathf.Abs(up - down) * 0.5f);
                break;
            case "cap":
                halfBoxSize = new Vector2(cap.size.x * 0.5f, cap.size.y * 0.5f);
                break;
        }

        return Vector2.Distance(new Vector2(position.x, 0), new Vector2(cam.transform.position.x, 0)) - halfBoxSize.x < 12.5f &&
            Vector2.Distance(new Vector2(0, position.y), new Vector2(0, cam.transform.position.y)) - halfBoxSize.y < 7.5f;
    }

    public static float GetDistance(EDirsSurface dir, Vector2 a, Vector2 b, int castCount, LayerMask layerMask, bool drawRays = false)
    {
        EDirsCardinal newDir = dir switch
        {
            EDirsSurface.Floor => EDirsCardinal.Down,
            EDirsSurface.WallL => EDirsCardinal.Left,
            EDirsSurface.WallR => EDirsCardinal.Right,
            EDirsSurface.Ceiling => EDirsCardinal.Up,
            _ => EDirsCardinal.None
        };
        return GetDistance(newDir, a, b, castCount, layerMask, drawRays);
    }
    public static float GetDistance(EDirsCardinal dir, Vector2 a, Vector2 b, int castCount, LayerMask layerMask, bool drawRays = false)
    {
        float shortestDis = Mathf.Infinity;
        Vector2 origin;
        RaycastHit2D hit;
        LayerMask playerCollide = LayerMask.GetMask("PlayerCollide");
        LayerMask enemyCollide = LayerMask.GetMask("PlayerCollide", "EnemyCollide");
        for (int i = 0; i < castCount; i++)
        {
            float t = (float)i / (float)(castCount - 1);
            switch (dir)
            {
                default:
                case EDirsCardinal.Down:
                    origin = Vector2.Lerp(a, new Vector2(b.x, a.y), t);
                    hit = Physics2D.Raycast(origin, Vector2.down, Mathf.Infinity, layerMask);
                    break;
                case EDirsCardinal.Left:
                    origin = Vector2.Lerp(a, new Vector2(a.x, b.y), t);
                    hit = Physics2D.Raycast(origin, Vector2.left, Mathf.Infinity, layerMask);
                    break;
                case EDirsCardinal.Right:
                    origin = Vector2.Lerp(new Vector2(b.x, a.y), b, t);
                    hit = Physics2D.Raycast(origin, Vector2.right, Mathf.Infinity, layerMask);
                    break;
                case EDirsCardinal.Up:
                    origin = Vector2.Lerp(new Vector2(a.x, b.y), b, t);
                    hit = Physics2D.Raycast(origin, Vector2.up, Mathf.Infinity, layerMask);
                    break;
            }
            if (layerMask == playerCollide && IsPointPlayerCollidable(origin))
                shortestDis = 0;
            else if (layerMask == enemyCollide && IsPointEnemyCollidable(origin))
                shortestDis = 0;
            else if (hit.collider != null)
            {
                if (shortestDis > hit.distance)
                    shortestDis = hit.distance;
                if (drawRays)
                    Debug.DrawLine(origin, hit.point);
            }
        }
        return shortestDis;
    }

    public static float Integrate(float num, float target, float speed, float elapsed, float threshold = 0.1f) {
        float scale = Mathf.Pow(0.1f, speed);
        num = num * Mathf.Pow(scale, elapsed) + target * (1 - Mathf.Pow(scale, elapsed));
        if (Mathf.Abs(num - target) < threshold)
            num = target;
        return num;
    }

    public static bool IsControllerConnected()
    {
        return Input.GetJoystickNames().Length > 0;
    }
}
