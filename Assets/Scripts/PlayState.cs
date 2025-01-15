using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PlayState
{
    public const float TAU = Mathf.PI * 2;
    public const float PI_OVER_EIGHT = Mathf.PI * 0.125f;
    public const float PI_OVER_FOUR = Mathf.PI * 0.25f;
    public const float PI_OVER_THREE = Mathf.PI * 0.3333f;
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

    public static readonly int[] HEALTH_ORB_VALUES = new int[] { 1, 2, 4 };
    public static readonly float[] HEALTH_ORB_MULTS = new float[] { 1.25f, 0.6f, 0.125f };

    public enum GameState { preload, game, menu, pause, map, debug, dialogue, error, credits }
    public static GameState gameState = GameState.preload;

    public static bool isMenuOpen = false;
    public static bool isInBossRush = false;
    public static bool incrementRushTimer = false;
    public enum CreditsStates { none, fadeIn, startDelay, moonScene, fadeToCredits, credits, fadeToTime, time, overwriteOldTime, fadeOut };
    public static CreditsStates creditsState = CreditsStates.none;
    public static bool isRandomGame = false;

    public static bool noclipMode = false;
    public static bool damageMult = false;
    public static bool showSpLayer = false;

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

    public static TextureLibrary textureLibrary;
    public static AnimationData[] animationLibrary = new AnimationData[0];
    public static SpriteFrameSize[] spriteSizeLibrary = new SpriteFrameSize[0];
    public static SoundLibrary soundLibrary;
    public static MusicLibrary musicLibrary;
    public static MusicLoopOffset[] musicLoopOffsetLibrary = new MusicLoopOffset[0];
    public static TextLibrary textLibrary;

    public static int[] charWidths;

    public static Transform musicParent;
    public static List<AudioSource> musicSourceArray = new();
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
    public static float sfxFader = 1.0f;
    public static string area;
    public static AudioClip areaMus;
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
    public static List<Vector2> breakablePositions = new();
    public static List<int> tempTiles = new(); // x, y, layer, original tile ID
    public static bool dialogueOpen = false;
    public static bool achievementOpen = false;
    public static bool cutsceneActive = false;
    public static int lastLoadedWeapon = 0;
    public static bool stackShells = true;
    public static bool stackWeaponMods = true;
    public static bool suppressPause = false;
    public static bool resetInducingFadeActive = false;
    public static int areaOfDeath = -1;
    public static bool lastRoomWasSnelk = false;
    public static int healthOrbPointer = 0;

    public static int importJobs = 0;

    public static Texture2D palette;

    public static int currentArea = -1;
    public static int currentSubzone = -1;

    public static GameObject player;
    public static Player playerScript;
    public static GameObject cam;
    public static GameObject camObj;
    public static GameObject camBorder;
    public static Camera mainCam;
    public static CamMovement camScript;
    public static SpriteRenderer screenCover;
    public static Tilemap groundLayer;
    public static Tilemap fg2Layer;
    public static Tilemap fg1Layer;
    public static Tilemap bgLayer;
    public static Tilemap skyLayer;
    public static Tilemap specialLayer;
    public static GameObject minimap;
    public static Minimap minimapScript;
    public static GameObject achievement;
    public static GameObject particlePool;
    public static GameObject camParticlePool;
    public static GameObject roomTriggerParent;
    public static RoomTrigger currentRoom;
    public static MainMenu mainMenu;
    public static Credits credits;
    public static GameObject loadingIcon;
    public static GameObject enemyBulletPool;
    public static GameObject subscreen;
    public static Subscreen subscreenScript;
    public static GameObject dialogueBox;
    public static DialogueBox dialogueScript;
    public static GameObject titleParent;
    public static SpriteRenderer darknessLayer;
    public static GameObject healthOrbPool;
    public static TrapManager trapManager;

    public static RoomTrigger titleRoom;
    public static RoomTrigger moonCutsceneRoom;
    public static RoomTrigger creditsRoom;

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
        public int blockType;
        public bool isSilent;
    }

    public static GameObject[] TogglableHUDElements;

    public static bool paralyzed = false;
    public static bool overrideParalysisInvulnerability = false;
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

    public static readonly Vector2 WORLD_ORIGIN = new(0.5f, 0.5f); // The exact center of the chartable map
    public static readonly Vector2 WORLD_SIZE = new(26, 22); // The number of screens wide and tall the world is
    public static readonly Vector2 WORLD_SPAWN = new(-37, 10.5f); // Use (-37, 10.5f) for Snail Town spawn, (84, 88.5f) for debug room spawn
    public static readonly Vector2 ROOM_SIZE = new(26, 16); // The number of tiles wide and tall each screen is, counting the buffer space that makes up room borders
    public static readonly Vector2 BOSS_RUSH_SPAWN = new(163, 41.5f); // Where the player starts in Boss Rush
    public static Vector2 respawnCoords = WORLD_SPAWN;
    public static Scene respawnScene;

    public static readonly Vector2[] PLAYER_SPAWNS = new Vector2[]
    {
        new(-37, 10.5f),    // Snaily
        new(-37, 10.5f),    // Sluggy
        new(-19f, 38.5f),   // Upside
        new(-37, 10.5f),    // Leggy
        new(-37, 10.5f),    // Blobby
        new(-37, 10.5f)     // Leechy
    };

    public static TextObject hudFps;
    public static TextObject hudTime;
    public static TextObject hudPause;
    public static TextObject hudMap;
    public static TextObject hudRoomName;
    public static TextObject hudRushTime;

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

    public static List<GameObject> gigaBGLayers = new();

    public static int currentProfileNumber = 0;

    public static readonly int[] defaultMinimapState = new int[]
    {
        -1,  0,  0, -1, -1,  2,  0, -1, -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
        -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
        -1, -1,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1,
        -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1,
         0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  2,  2,  0,  0,  0,  0,  0, -1, -1, -1, -1,
         0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  2,  2,  2,  0,  0,  0,  0,  0, -1, -1, -1, -1,
        -1,  0,  0,  0,  0,  0,  0,  0, -1,  0,  2,  2,  2,  0,  2,  2,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1,
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
    public static List<int> saveLocations = new();
    public static List<int> bossLocations = new();
    public static Dictionary<int, int> itemLocations = new();
    public static Dictionary<int, string> playerMarkerLocations = new();
    public static List<List<int>> itemAreas = new();
    public static List<int> totaItemsPerArea = new();

    public static bool[][] itemData = new bool[][] { };
    public static bool[] countedItems = new bool[] { };
    public static int totalCountedItems = 0;

    public static int npcCount = 0;
    
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

    public static readonly int[] timeVersionsDefault = new int[]
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

    public enum TimeIndeces
    {
        snailyNormal, snailyInsane, snaily100, snailyRush,
        sluggyNormal, sluggyInsane, sluggy100, sluggyRush,
        upsideNormal, upsideInsane, upside100, upsideRush,
        leggyNormal, leggyInsane, leggy100, leggyRush,
        blobbyNormal, blobbyInsane, blobby100, blobbyRush,
        leechyNormal, leechyInsane, leechy100, leechyRush,
        none
    }

    public enum TimeCategories
    {
        normal, insane, hundo, rush
    }
    
    public const byte MAX_HEARTS = 11;
    public const byte MAX_FRAGMENTS = 30;

    public static int[] NPCvarDefault = new int[] { 0, 0, 0 };

    public static List<string[]> cutsceneData = new();
    public static List<int> cutscenesToNotSpawn = new();

    public const string SAVE_FILE_PREFIX = "SnailySave";

    public enum NPCVarIDs
    {
        HasSeenIris,
        TalkedToCaveSnail,
        SeenSunEnding
    };

    public enum Items
    {
        Peashooter,
        Boomerang,
        RainbowWave,
        Devastator,
        HighJump,
        ShellShield,
        RapidFire,
        IceShell,
        FlyShell,
        MetalShell,
        GravShock,
        SSBoom,
        DebugRW,
        Heart,
        Fragment,
        RadarShell,
        None
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
        public int[] locations;       // Similar to item table, but denotes the actual item locations instead (0 = uncollected, 1 = collected)
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
     |   0 - - Peashooter
     |   1 - - Boomerang
     |   2 - - Rainbow Wave
     |   3 - - Devastator
     |   4 - - High Jump          Wall Grab
     |   5 - - Shell Shield       Shelmet
     |   6 - - Rapid Fire         Backfire
     |   7 - - Ice Snail
     |   8 - - Gravity Snail      Magnetic Foot      Corkscrew Jump       Angel Jump
     |   9 - - Full-Metal Snail
     |  10 - - Gravity Shock
     |  11 - - Super Secret Boomerang
     |  12 - - Debug Rainbow Wave
     |  13 - - Heart Containers
     |  14 - - Helix Fragments
     |  15 - - Radar Shell
     |  16 - - None
     |  100  - Weapon Lock
     |  101  - Gravity Lock
     |  102  - Lullaby Trap
     |  103  - Spider Trap
     |  104  - Warp Trap
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
    public struct ProfileRandoData
    {
        public int randoLevel;         // Whether or not this profile is even randomized, and to what extent
        public int seed;               // Eight-digit seed that all randomization was based on
        public int[] itemLocations;    // All item IDs, in order of location appearance in the hierarchy
        public bool progressivesOn;    // Whether or not progressive items are turned on
        public bool broomStart;        // Whether or not the player starts with a Broom
        public bool trapsActive;       // Whether or not trap items have been shuffled into the pool
        public bool maskedItems;       // Whether or not items are masked as the same sprite
        public bool openAreas;         // Whether or not all areas are open from the start
        public bool bossesLocked;      // Whether or not Helix Locks have been applied to boss doors
        public int[] helixesRequired;  // The required number of Helix Framgents need to open the door to each boss
        public int musicShuffled;      // Whether or not music should be shuffled, and to what extent
        public int[] musicList;        // All music IDs, in order relative to the actual music ID list
        public bool npcTextShuffled;   // Whether or not NPC dialogue should be randomized
        public int[] npcTextIndeces;   // Pointers for each snail NPC into a bonus text table
        public int[] npcHintData;      // Any item hints that get generated. Values come in groups of four, ordered: [NPC ID, hint text ID, item ID, area ID]
    }

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
        public int gravSwapType;            // Method of swapping gravity (0 = hold dir mid-air and jump, 1 = hold jump mid-air and tap dir, 2 = double-tap dir)
        public int gravKeepType;            // How gravity state is retained (0 = swap fall dir on any grav change, 1 = swap fall dir on deliberate gravity jump)
        public int darknessLevel;           // The level of which darkness is induced (0 = none, 1 = minor, 2 = half, 3 = full)
        public KeyCode[] keyboardInputs;
        public Control.ControllerBinds[] controllerInputs;

        // RECORDS
        public bool[] achievements;
        public float[] times;
        public int[] timeVers;
    }

    public static readonly ProfileData blankProfile = new()
    {
        isEmpty = true,
        difficulty = 0,
        gameTime = new float[] { 0f, 0f, 0f },
        saveCoords = WORLD_SPAWN,
        character = "Snaily",
        items = new int[16],
        locations = new int[57],
        weapon = -1,
        bossStates = new int[] { 1, 1, 1, 1 },
        NPCVars = new int[] { 0, 0, 0 },
        percentage = 0,
        exploredMap = defaultMinimapState,
        cutsceneStates = new int[] { }
    };

    public static readonly ProfileRandoData blankRando = new()
    {
        randoLevel = 0,
        seed = 0,
        itemLocations = new int[] { },
        progressivesOn = false,
        broomStart = false,
        trapsActive = false,
        maskedItems = false,
        openAreas = false,
        bossesLocked = false,
        helixesRequired = new int[] { },
        musicShuffled = 0,
        musicList = new int[] { },
        npcTextShuffled = false,
        npcTextIndeces = new int[] { },
        npcHintData = new int[] { }
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
        screenShake = 4,
        paletteFilterState = false,
        controllerFaceType = 0,
        gravSwapType = 0,
        gravKeepType = 0,
        darknessLevel = 3,
        keyboardInputs = (KeyCode[])Control.defaultKeyboardInputs.Clone(),
        controllerInputs = (Control.ControllerBinds[])Control.defaultControllerInputs.Clone(),
        achievements = new bool[Enum.GetNames(typeof(AchievementPanel.Achievements)).Length],
        times = (float[])timeDefault.Clone(),
        timeVers = (int[])timeVersionsDefault.Clone()
    };

    public static ProfileData profile1 = blankProfile;
    public static ProfileData profile2 = blankProfile;
    public static ProfileData profile3 = blankProfile;
    public static ProfileData currentProfile = blankProfile;
    public static ProfileRandoData rando1 = blankRando;
    public static ProfileRandoData rando2 = blankRando;
    public static ProfileRandoData rando3 = blankRando;
    public static ProfileRandoData currentRando = blankRando;
    public static GeneralData generalData = blankData;

    public static List<int> baseItemLocations = new();
    public static List<int> rushItemLocations = new();

    public enum Areas
    {
        SnailTown,
        MareCarelia,
        SpiralisSilere,
        AmastridaAbyssus,
        LuxLirata,
        ShrineOfIris,
        BossRush
    }

    public struct BossRushData
    {
        public float ssbTime;
        public float visTime;
        public float cubeTime;
        public float sunTime;
        public float gigaTime;
        public int peasFired;
        public int boomsFired;
        public int wavesFired;
        public int parries;
        public int shocksFired;
        public int healthLost;
        public bool[] itemStates;
    }

    public static readonly BossRushData defaultRushData = new()
    {
        ssbTime = 0,
        visTime = 0,
        cubeTime = 0,
        sunTime = 0,
        gigaTime = 0,
        peasFired = 0,
        boomsFired = 0,
        wavesFired = 0,
        parries = 0,
        shocksFired = 0,
        healthLost = 0,
        itemStates = new bool[0]
    };

    public static BossRushData activeRushData;

    public static Color entityColor = Color.white;

    public static float currentDarkness = 0f;

    public static Sprite BlankTexture(bool useSmallBlank = false) {
        return useSmallBlank ? globalFunctions.blankSmall : globalFunctions.blank;
    }

    public static Sprite MissingTexture() {
        return globalFunctions.missing;
    }

    public static int[] ParseVersion(string version)
    {
        if (version.Contains(' '))
            version = version.Split(' ')[1];
        string[] parts = version.Split('.');
        int major = int.Parse(parts[0]);
        int minor = int.Parse(parts[1]);
        int patch = int.Parse(parts[2]);
        return new int[] { major, minor, patch };
    }

    public static int CompareVersions(string ver1, string ver2)
    {
        int[] newVer1 = ParseVersion(ver1);
        int[] newVer2 = ParseVersion(ver2);
        return CompareVersions(newVer1, newVer2);
    }
    public static int CompareVersions(int[] ver1, int[] ver2)
    {
        if (ver1[0] > ver2[0])
            return 1;
        else if (ver1[0] < ver2[0])
            return -1;
        else
        {
            if (ver1[1] > ver2[1])
                return 1;
            else if (ver1[1] < ver2[1])
                return -1;
            else
            {
                if (ver1[2] > ver2[2])
                    return 1;
                else if (ver1[2] < ver2[2])
                    return -1;
                else
                    return 0;
            }
        }
    }

    public static ProfileData BlankProfile()
    {
        return new ProfileData
        {
            isEmpty = blankProfile.isEmpty,
            difficulty = blankProfile.difficulty,
            gameTime = (float[])blankProfile.gameTime.Clone(),
            saveCoords = blankProfile.saveCoords,
            character = blankProfile.character,
            items = (int[])blankProfile.items.Clone(),
            locations = (int[])blankProfile.locations.Clone(),
            weapon = blankProfile.weapon,
            bossStates = (int[])blankProfile.bossStates.Clone(),
            NPCVars = (int[])blankProfile.NPCVars.Clone(),
            percentage = blankProfile.percentage,
            exploredMap = (int[])defaultMinimapState.Clone(),
            cutsceneStates = (int[])blankProfile.cutsceneStates.Clone()
        };
    }

    public static ProfileRandoData BlankRando()
    {
        return new ProfileRandoData
        {
            randoLevel = blankRando.randoLevel,
            seed = blankRando.seed,
            itemLocations = (int[])blankRando.itemLocations.Clone(),
            progressivesOn = blankRando.progressivesOn,
            broomStart = blankRando.broomStart,
            trapsActive = blankRando.trapsActive,
            maskedItems = blankRando.maskedItems,
            openAreas = blankRando.openAreas,
            bossesLocked = blankRando.bossesLocked,
            helixesRequired = (int[])blankRando.helixesRequired.Clone(),
            musicShuffled = blankRando.musicShuffled,
            musicList = (int[])blankRando.musicList.Clone(),
            npcTextShuffled = blankRando.npcTextShuffled,
            npcTextIndeces = (int[])blankRando.npcTextIndeces.Clone(),
            npcHintData = (int[])blankRando.npcHintData.Clone()
        };
    }

    public static AnimationData GetAnim(string name) {
        AnimationData foundData = new() {
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
        foreach (Transform obj in healthOrbPool.transform)
            obj.GetComponent<AnimationModule>().ReloadList();
    }

    public static AnimationData GetAnim(int ID) {
        return animationLibrary[ID];
    }

    public static int GetAnimID(string name) {
        AnimationData foundData = new() {
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

    public static Color32 GetColor(string ID)
    {
        if (ID.Length == 2)
        {
            ID = ID.ToLower();
            int x = ID[0] switch { 'a' => 10, 'b' => 11, 'c' => 12, 'd' => 13, 'e' => 14, 'f' => 15, _ => int.Parse(ID.Substring(0, 1)) };
            int y = ID[1] switch { 'a' => 10, 'b' => 11, 'c' => 12, 'd' => 13, 'e' => 14, 'f' => 15, _ => int.Parse(ID.Substring(1, 1)) };
            return palette.GetPixel(x % 4, y % 14);
        }
        else
            return palette.GetPixel(int.Parse(ID.Substring(0, 2)) % 4, int.Parse(ID.Substring(2, 2)) % 14);
    }
    public static Color32 GetColor(Vector2 ID)
    {
        return palette.GetPixel((int)ID.x % 4, (int)ID.y % 14);
    }
    public static Color32 GetColor(int ID)
    {
        string idStr = ID.ToString();
        while (idStr.Length < 4)
            idStr = string.Concat("0", idStr);
        return GetColor(idStr);
    }
    public static Color32 GetColor(float ID)
    {
        return GetColor(Mathf.RoundToInt(ID));
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
        Dictionary<Color32, int> referenceColors = new();
        for (int i = 0; i < colorTable.width; i++) {
            referenceColors.Add(colorTable.GetPixel(i, 0), i);
        }

        Sprite oldSprite = GetSprite(sprite, spriteNum);
        Texture2D newSprite = new((int)oldSprite.rect.width, (int)oldSprite.rect.height);
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
        Vector2 topLeftCorner = new(WORLD_ORIGIN.x - (WORLD_SIZE.x * ROOM_SIZE.x * 0.5f), WORLD_ORIGIN.y + (WORLD_SIZE.y * ROOM_SIZE.y * 0.5f));
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
        int itemLocationID = 0;
        foreach (Transform area in roomTriggerParent.transform) {
            if (!area.name.ToLower().Contains("boss rush"))
            {
                foreach (Transform room in area)
                {
                    foreach (Transform entity in room)
                    {
                        if (entity.CompareTag("SavePoint"))
                            saveLocations.Add(WorldPosToMapGridID(entity.transform.position));
                        if (entity.CompareTag("Item"))
                            itemLocations.Add(WorldPosToMapGridID(entity.transform.position), itemLocationID++);
                        if (entity.CompareTag("NPC"))
                            npcCount++;
                    }
                }
            }
        }

        Tilemap spMap = specialLayer.GetComponent<Tilemap>();
        List<int> bossTileIDs = new() { 23, 24, 25, 26 };
        for (int y = 0; y < spMap.size.y; y++) {
            for (int x = 0; x < spMap.size.x; x++) {
                Vector3Int worldPos = new(Mathf.RoundToInt(spMap.origin.x + x), Mathf.RoundToInt(spMap.origin.y + y), 0);
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
            Vector3Int position = new(tempTiles[0], tempTiles[1], 0);
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

    public static void PlayAreaSong(int area, int subzone, bool isSnelk = false)
    {
        if (areaOfDeath != area)
        {
            if (isSnelk && !lastRoomWasSnelk)
            {
                lastRoomWasSnelk = true;
                globalFunctions.UpdateMusic(2 - musicLibrary.areaThemeOffset, 0, 1);
            }
            else
            {
                if (area == currentArea && subzone != currentSubzone)
                    globalFunctions.UpdateMusic(area, subzone);
                else if (area != currentArea || lastRoomWasSnelk)
                    globalFunctions.UpdateMusic(area, subzone, 1);
                lastRoomWasSnelk = false;
            }
        }
        currentArea = area;
        currentSubzone = subzone;
        areaOfDeath = -1;
    }

    public static bool IsTileSolid(Vector2 tilePos, bool checkForEnemyCollide = false) {
        if (breakablePositions.Contains(new Vector2(Mathf.Floor(tilePos.x), Mathf.Floor(tilePos.y))))
            return true;
        else {
            Vector2 gridPos = new(Mathf.Floor(tilePos.x), Mathf.Floor(tilePos.y));
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
        List<Transform> platforms = new();
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

    public static RoomTrigger LastRoom()
    {
        return roomTriggerParent.transform.GetChild((int)positionOfLastRoom.x).GetChild((int)positionOfLastRoom.y).GetComponent<RoomTrigger>();
    }

    public static void ToggleHUD(bool state)
    {
        for (int i = 0; i < TogglableHUDElements.Length; i++)
        {
            GameObject thisElement = TogglableHUDElements[i];
            switch (i)
            {
                case 0: // Minimap
                    thisElement.SetActive(state && generalData.minimapState > 0 && !inBossFight && !isInBossRush);
                    hudRoomName.SetColor(generalData.minimapState == 2 ? Color.white : new Color(1, 1, 1, 0));
                    break;
                case 1: // Hearts
                    thisElement.SetActive(state);
                    break;
                case 2: // Input display
                    thisElement.SetActive(state && generalData.keymapState);
                    break;
                case 3: // Weapon icons
                    thisElement.SetActive(state && generalData.bottomKeyState > 0);
                    break;
                case 4: // Saving text
                    thisElement.SetActive(state);
                    break;
                case 5: // Area name text
                    thisElement.SetActive(state);
                    break;
                case 6: // Item name text
                    thisElement.SetActive(state);
                    break;
                case 7: // Completion percentage text
                    thisElement.SetActive(state);
                    break;
                case 8: // Framerate display
                    thisElement.SetActive(state && generalData.FPSState);
                    break;
                case 9: // In-game time display
                    thisElement.SetActive(state && generalData.timeState);
                    break;
                case 10: // Dialogue box
                    thisElement.SetActive(state);
                    break;
                case 11: // Bottom keys
                    thisElement.SetActive(state && generalData.bottomKeyState == 2);
                    break;
                case 12: // Boss health bar
                    thisElement.SetActive(state);
                    break;
                case 13: // Radar
                    thisElement.SetActive(state);
                    break;
                case 14: // Best time text
                    thisElement.SetActive(state);
                    break;
                case 15: // Mode unlock text
                    thisElement.SetActive(state);
                    break;
                case 16: // Item completion text
                    thisElement.SetActive(state);
                    break;
                case 17: // Control guide
                    thisElement.SetActive(state);
                    break;
                case 18: // Boss Rush timer
                    thisElement.SetActive(state && isInBossRush);
                    break;
            }
        }
    }

    public static void RunItemPopup(string item) {
        playbackTime = activeMus.time;
        mus1.Stop();
        areaMus = activeMus.clip;
        PlayMusic(0, 2);
        List<string> text = new();
        List<Color32> colors = new();
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

    public static void ScreenFlash(string type, int red = 0, int green = 0, int blue = 0, int alpha = 0, float maxTime = 0, float delay = 0,
        int sortingOrder = 1001)
    {
        switch (type)
        {
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
                globalFunctions.ExecuteCoverCommand(type, (byte)red, (byte)green, (byte)blue, (byte)alpha, maxTime, delay, sortingOrder);
                break;
        }
    }

    public static void SetDarkness(float newValue)
    {
        newValue *= generalData.darknessLevel switch { 0 => 0f, 1 => 0.2f, 2 => 0.5f, _ => 1f };
        currentDarkness = newValue;
        darknessLayer.color = new Color(0, 0, 0, newValue);
    }

    public static void SetTempDarkness(float newValue)
    {
        newValue *= generalData.darknessLevel switch { 0 => 0f, 1 => 0.2f, 2 => 0.5f, _ => 1f };
        darknessLayer.color = new Color(0, 0, 0, newValue);
    }

    public static void ResetTilemapColors()
    {
        skyLayer.color = Color.white;
        bgLayer.color = Color.white;
        groundLayer.color = Color.white;
        fg1Layer.color = Color.white;
        fg2Layer.color = Color.white;
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
        thisParticleID %= particlePool.transform.childCount;
        if (particlePool.transform.GetChild(thisParticleID).gameObject.activeSelf) {
            int i = 0;
            while (i < particlePool.transform.childCount - 1 && !found) {
                thisParticleID = (thisParticleID + 1) % particlePool.transform.childCount;
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
                case "angeljumpeffect":
                    if (generalData.particleState == 3 || generalData.particleState == 5)
                    {
                        activateParticle = true;
                        particleScript.vars[0] = CheckForItem(Items.MetalShell) ? 1 : 0;
                    }
                    break;
                case "apitemdust":
                    // Values:
                    // 0 = Color

                    if (generalData.particleState == 3 || generalData.particleState == 5)
                    {
                        activateParticle = true;
                        particleScript.vars[0] = values[0];  // Color
                    }
                    break;
                case "apitemflash":
                    if (generalData.particleState == 3 || generalData.particleState == 5)
                        activateParticle = true;
                    break;
                case "bubble":
                    // Values:
                    // 0 = Water level
                    // 1 = Boolean to initialize particle with random velocity or not

                    if (generalData.particleState == 1 || generalData.particleState >= 4)
                    {
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

                    if ((generalData.particleState > 1 && values[0] <= 4) || ((generalData.particleState == 3 || generalData.particleState == 5) && values[0] > 4))
                    {
                        activateParticle = true;
                        particleScript.vars[0] = values[0];
                    }
                    break;
                case "fog":
                    if (generalData.particleState == 1 || generalData.particleState == 5)
                        activateParticle = true;
                    break;
                case "gigastar":
                    if (generalData.particleState == 4 || generalData.particleState == 1 || generalData.particleState == 5)
                    {
                        activateParticle = true;
                        particleScript.vars[0] = 0;
                        particleScript.vars[1] = UnityEngine.Random.Range(0f, 1f) * 5f - 0.5f;
                        particleScript.vars[2] = UnityEngine.Random.Range(0f, 1f) * 6f + 3f;
                    }
                    break;
                case "gigatrail":
                    // Values:
                    // 0 = sprite ID
                    // 1 = flip sprite X
                    // 2 = flip sprite Y
                    
                    if (generalData.particleState == 3 || generalData.particleState == 5)
                    {
                        activateParticle = true;
                        particleScript.vars[0] = values[0];
                        particleScript.vars[1] = values[1];
                        particleScript.vars[2] = values[2];
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
                case "intropattern":
                    // Values:
                    // 0 = if the tile should play anim 1, 2, 3, or 4 VS anim 5, 6, 7, or 8

                    if (generalData.particleState != 0)
                    {
                        activateParticle = true;
                        particleScript.vars[0] = values[0];
                    }
                    break;
                case "lightning":
                    if (generalData.particleState == 1 || generalData.particleState == 5)
                        activateParticle = true;
                    break;
                case "nom":
                    // Values:
                    // 0 = Start Y

                    if (generalData.particleState > 1)
                    {
                        activateParticle = true;
                        particleScript.vars[0] = position.y;
                    }
                    break;
                case "parry":
                    if (generalData.particleState == 3 || generalData.particleState == 5)
                        activateParticle = true;
                    break;
                case "radarsparkle":
                    if (generalData.particleState == 3 || generalData.particleState == 5)
                    {
                        activateParticle = true;
                        particleScript.vars[0] = UnityEngine.Random.Range(-0.25f, 0.25f);
                        particleScript.vars[1] = UnityEngine.Random.Range(-0.25f, 0.25f);
                    }
                    break;
                case "rain":
                    if (generalData.particleState == 1 || generalData.particleState == 5)
                    {
                        activateParticle = true;
                        particleScript.vars[0] = 28.75f + UnityEngine.Random.Range(0f, 1f) * 12f;  // Downward velocity
                        particleScript.vars[1] = 14f + UnityEngine.Random.Range(0f, 1f) * 6f;   // Leftward velocity
                    }
                    break;
                case "rushgigatrail":
                    // Values:
                    // 0 = sprite ID
                    // 1 = flip sprite X
                    // 2 = flip sprite Y

                    if (generalData.particleState == 3 || generalData.particleState == 5)
                    {
                        activateParticle = true;
                        particleScript.vars[0] = values[0];
                        particleScript.vars[1] = values[1];
                        particleScript.vars[2] = values[2];
                    }
                    break;
                case "shield":
                    if (generalData.particleState == 3 || generalData.particleState == 5)
                        activateParticle = true;
                    break;
                case "shockcharge":
                    if (generalData.particleState == 3 || generalData.particleState == 5)
                        activateParticle = true;
                    break;
                case "shockcharmain":
                    // Values:
                    // 0 = the current player
                    // 1 = the shell state as considered by Gravity Shock
                    // 2 = the direction, according to EDirsSurface

                    if (generalData.particleState == 3 || generalData.particleState == 5)
                    {
                        activateParticle = true;
                        particleScript.vars[0] = values[0];
                        particleScript.vars[1] = values[1];
                        particleScript.vars[2] = values[2];
                    }
                    break;
                case "shockcharsub":
                    // Values:
                    // 0 = the current player
                    // 1 = the shell state as considered by Gravity Shock
                    // 2 = the direction, according to EDirsSurface
                    // 3 = the frame index offset

                    if (generalData.particleState == 3 || generalData.particleState == 5)
                    {
                        activateParticle = true;
                        particleScript.vars[0] = values[0];
                        particleScript.vars[1] = values[1];
                        particleScript.vars[2] = values[2];
                        particleScript.vars[3] = values[3];
                    }
                    break;
                case "shocklaunch":
                    // Values:
                    // 0 = the direction, according to EDirsSurface

                    if (generalData.particleState == 3 || generalData.particleState == 5)
                    {
                        activateParticle = true;
                        particleScript.vars[0] = values[0];
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
                        particleScript.vars[1] = UnityEngine.Random.Range(0f, 1f) * 5f - 0.5f;
                        particleScript.vars[2] = UnityEngine.Random.Range(0f, 1f) * 6f + 3f;
                    }
                    break;
                case "tintedsparkle":
                    // Values:
                    // 0 = Color
                    // 1 = X velocity
                    // 2 = Y velocity

                    if (generalData.particleState == 3 || generalData.particleState == 5)
                    {
                        activateParticle = true;
                        particleScript.vars[0] = UnityEngine.Random.Range(-0.25f, 0.25f);
                        particleScript.vars[1] = UnityEngine.Random.Range(-0.25f, 0.25f);
                        particleScript.vars[2] = values[0];
                        particleScript.vars[3] = values[1];
                        particleScript.vars[4] = values[2];
                    }
                    break;
                case "transformation":
                    // Values:
                    // 0 = Type

                    if (generalData.particleState == 3 || generalData.particleState == 5)
                    {
                        activateParticle = true;
                        particleScript.vars[0] = values[0];
                    }
                    break;
                case "warpsparkle":
                    if (generalData.particleState == 3 || generalData.particleState == 5)
                    {
                        activateParticle = true;
                        particleScript.vars[0] = UnityEngine.Random.Range(0f, TAU);
                        particleScript.vars[1] = UnityEngine.Random.Range(0.25f, 2f);
                        particleScript.vars[2] = 3f;
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
                particleScript.type = type.ToLower();
                particleScript.SetAnim(type.ToLower());
                if (playSound)
                    particleScript.PlaySound();
                thisParticleID = (thisParticleID + 1) % particlePool.transform.childCount;
            }
        }
        return selectedParticle;
    }

    public static void ResetAllParticles()
    {
        for (int i = camParticlePool.transform.childCount - 1; i >= 0; i--)
            camParticlePool.transform.GetChild(i).parent = particlePool.transform;
        foreach (Transform particle in particlePool.transform)
        {
            Particle particleScript = particle.GetComponent<Particle>();
            if (particle.gameObject.activeSelf && particleScript.type != "shockcharmain")
                particleScript.ResetParticle();
        }
    }

    public static void GetHealthOrb(int size, Vector2 position)
    {
        bool found = false;
        int attemptsMade = 0;
        while (!found && attemptsMade < healthOrbPool.transform.childCount)
        {
            HealthOrb thisOrb = healthOrbPool.transform.GetChild(healthOrbPointer).GetComponent<HealthOrb>();
            if (!thisOrb.isActive)
            {
                found = true;
                thisOrb.Activate(size);
                thisOrb.transform.position = position;
            }
            healthOrbPointer++;
            if (healthOrbPointer >= healthOrbPool.transform.childCount)
                healthOrbPointer -= healthOrbPool.transform.childCount;
        }
    }

    public static bool IsBossAlive(int bossID)
    {
        return currentProfile.bossStates[bossID] == 1;
    }

    //public static bool CheckForItem(int itemID)
    //{
    //    return currentProfile.items[itemID] == 1;
    //}
    //
    //public static bool CheckForItem(string itemName)
    //{
    //    return currentProfile.items[TranslateItemNameToID(itemName)] == 1;
    //}

    public static bool CheckForItem(Items item)
    {
        return currentProfile.items[(int)item] > 0;
    }

    public static bool CheckShellLevel(int level)
    {
        bool meetsLevel;
        if (stackShells)
        {
            meetsLevel = level switch
            {
                2 => CheckForItem(Items.FlyShell) || CheckForItem(Items.MetalShell),
                3 => CheckForItem(Items.MetalShell),
                _ => CheckForItem(Items.IceShell) || CheckForItem(Items.FlyShell) || CheckForItem(Items.MetalShell),
            };
        }
        else
        {
            meetsLevel = level switch
            {
                2 => CheckForItem(Items.FlyShell),
                3 => CheckForItem(Items.MetalShell),
                _ => CheckForItem(Items.IceShell),
            };
        }
        return meetsLevel;
    }

    public static int GetShellLevel()
    {
        return CheckForItem(Items.MetalShell) ? 3 : (CheckForItem(Items.FlyShell) ? 2 : (CheckForItem(Items.IceShell) ? 1 : 0));
    }

    public static void SetPlayer(string newPlayer)
    {
        player.GetComponent<Snaily>().enabled = newPlayer == "Snaily";
        player.GetComponent<Sluggy>().enabled = newPlayer == "Sluggy";
        player.GetComponent<Upside>().enabled = newPlayer == "Upside";
        player.GetComponent<Leggy>().enabled = newPlayer == "Leggy";
        player.GetComponent<Blobby>().enabled = newPlayer == "Blobby";
        player.GetComponent<Leechy>().enabled = newPlayer == "Leechy";
        currentProfile.character = newPlayer;
        playerScript = newPlayer switch
        {
            "Sluggy" => player.GetComponent<Sluggy>(),
            "Upside" => player.GetComponent<Upside>(),
            "Leggy" => player.GetComponent<Leggy>(),
            "Blobby" => player.GetComponent<Blobby>(),
            "Leechy" => player.GetComponent<Leechy>(),
            _ => player.GetComponent<Snaily>()
        };
    }

    public static void AddItem(Items itemID)
    {
        if ((int)itemID < currentProfile.items.Length)
            currentProfile.items[(int)itemID]++;
    }

    public static int GetMapPercentage() {
        int explored = 0;
        int total = 0;
        foreach (int i in currentProfile.exploredMap) {
            if (i != -1 && i != 9) {
                if (i != 2 && i != 3 && i != 12 && i != 13)
                    total++;
                if (i == 1 || i == 11)
                    explored++;
            }
        }
        return Mathf.FloorToInt(((float)explored / (float)total) * 100);
    }

    public static int GetItemPercentage(int profileID = 0, bool isRando = false)
    {
        int itemsFound = 0;
        int totalCount = totalCountedItems;
        ProfileData targetProfile = profileID switch { 1 => profile1, 2 => profile2, 3 => profile3, _ => currentProfile };
        ProfileRandoData targetRando = profileID switch { 1 => rando1, 2 => rando2, 3 => rando3, _ => currentRando };
        int charCheck = targetProfile.character switch { "Snaily" => 3, "Sluggy" => 4, "Upside" => 5, "Leggy" => 6, "Blobby" => 7, "Leechy" => 8, _ => 3 };
        if (isRandomGame)
        {
            totalCount = 0;
            for (int i = 0; i < targetProfile.locations.Length; i++)
                totalCount += targetRando.itemLocations[i] > -1 ? 1 : 0;
        }
        for (int i = 0; i < targetProfile.items.Length; i++)
        {
            if (countedItems[i])
            {
                if (itemData[i] != null)
                {
                    if (itemData[i][targetProfile.difficulty] && itemData[i][charCheck])
                    {
                        totalCount++;
                        itemsFound += targetProfile.items[i];
                    }
                }
                else
                    totalCount++;
            }
        }
        return Mathf.FloorToInt(((float)itemsFound / (float)totalCount) * 100);
    }

    public static bool GetItemAvailabilityThisDifficulty(int itemID)
    {
        if (itemID == -1)
            return false;
        return itemData[itemID][currentProfile.difficulty];
    }
    public static bool GetItemAvailabilityThisDifficulty(int itemID, int difficulty)
    {
        if (itemID == -1)
            return false;
        return itemData[itemID][difficulty];
    }

    public static bool GetItemAvailabilityThisCharacter(int itemID)
    {
        if (itemID == -1)
            return false;
        int charCheck = currentProfile.character switch { "Snaily" => 3, "Sluggy" => 4, "Upside" => 5, "Leggy" => 6, "Blobby" => 7, "Leechy" => 8, _ => 3 };
        return itemData[itemID][charCheck];
    }
    public static bool GetItemAvailabilityThisCharacter(int itemID, string character)
    {
        if (itemID == -1)
            return false;
        int charCheck = character switch { "Snaily" => 3, "Sluggy" => 4, "Upside" => 5, "Leggy" => 6, "Blobby" => 7, "Leechy" => 8, _ => 3 };
        return itemData[itemID][charCheck];
    }

    public static string GetTimeString(bool trimBlankHours = true, bool dropSecondDecimal = false)
    {
        return GetTimeString(currentProfile.gameTime, trimBlankHours, dropSecondDecimal);
    }
    public static string GetTimeString(TimeIndeces target, bool trimBlankHours = true, bool dropSecondDecimal = false)
    {
        return GetTimeString(GetTime(target), trimBlankHours, dropSecondDecimal);
    }
    public static string GetTimeString(int profile, bool trimBlankHours = true, bool dropSecondDecimal = false)
    {
        return GetTimeString(profile switch { 2 => profile2.gameTime, 3 => profile3.gameTime, _ => profile1.gameTime }, trimBlankHours, dropSecondDecimal);
    }
    public static string GetTimeString(float[] time, bool trimBlankHours = true, bool dropSecondDecimal = false)
    {
        int hours = (int)time[0];
        int minutesI = (int)time[1];
        float secondsF = Mathf.RoundToInt(time[2] * 100) * 0.01f;
        if (dropSecondDecimal)
            secondsF = (int)secondsF;

        string minutes = minutesI.ToString();
        if (minutesI < 10 && !trimBlankHours)
            minutes = "0" + minutes;
        string seconds = secondsF.ToString();
        if (secondsF < 10f)
            seconds = "0" + seconds;
        if (!dropSecondDecimal)
        {
            seconds += seconds.Length switch
            {
                2 => ".00",
                4 => "0",
                _ => ""
            };
            if (seconds.Length > 5)
                seconds = seconds.Substring(0, 5);
        }

        if (trimBlankHours && hours == 0)
            return string.Format("{0}:{1}", minutes, seconds);
        else
            return string.Format("{0}:{1}:{2}", hours, minutes, seconds);
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
        return currentProfile.items[(int)Items.Heart];
    }

    public static int CountFragments()
    {
        return currentProfile.items[(int)Items.Fragment];
    }

    public static int[] GetAreaItemRate(int areaID)
    {
        int collectedItems = 0;
        int totalItems = 0;
        int totalCounted = 0;
        int collectedUncounted = 0;
        int target = Math.Clamp(areaID, 0, 6);
        bool hint = false;
        for (int i = 0; i < itemAreas[target].Count; i++)
        {
            if (GetItemAvailabilityThisDifficulty(itemAreas[target][i]) && GetItemAvailabilityThisCharacter(itemAreas[target][i]))
            {
                bool isCounted = countedItems[itemAreas[target][i]];
                bool isCollected = currentProfile.items[itemAreas[target][i]] == 1;
                if (isCounted)
                {
                    totalCounted++;
                    if (isCollected)
                        collectedItems++;
                }
                else if (isCollected && !isCounted)
                {
                    collectedItems++;
                    collectedUncounted++;
                }
                totalItems++;
            }
        }
        if (totalCounted != totalItems)
        {
            if ((collectedItems == totalCounted && collectedItems < totalItems) || (collectedItems < totalItems && collectedUncounted > 0))
                hint = true;
        }
        return new int[] { collectedItems, totalCounted + collectedUncounted, hint ? 1 : 0, collectedItems == totalItems ? 1 : 0 };
    }

    public static int GetNPCVar(NPCVarIDs ID)
    {
        return currentProfile.NPCVars[(int)ID];
    }

    public static void SetNPCVar(NPCVarIDs ID, int value)
    {
        currentProfile.NPCVars[(int)ID] = value;
    }

    public static void WriteSave(int profileID, bool saveGeneral)
    {
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

    public static void SaveRando(int profileID)
    {
        switch (profileID)
        {
            case 1:
                rando1 = currentRando;
                File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_RandoData1.json", JsonUtility.ToJson(rando1));
                break;
            case 2:
                rando2 = currentRando;
                File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_RandoData2.json", JsonUtility.ToJson(rando2));
                break;
            case 3:
                rando3 = currentRando;
                File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_RandoData3.json", JsonUtility.ToJson(rando3));
                break;
        }
    }

    public static void CopySave(int copiedDataID, int destinationID)
    {
        if (copiedDataID > 0 && copiedDataID <= 3)
        {
            File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_Profile" + destinationID + ".json",
                JsonUtility.ToJson(copiedDataID switch { 1 => profile1, 2 => profile2, _ => profile3 }));
            File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_RandoData" + destinationID + ".json",
                JsonUtility.ToJson(copiedDataID switch { 1 => rando1, 2 => rando2, _ => rando3 }));
        }
    }

    public static ProfileData LoadGame(int profile, bool setAsCurrent)
    {
        ProfileData thisProfile = blankProfile;
        string gamePath = Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_Profile" + profile + ".json";
        if (File.Exists(gamePath))
        {
            thisProfile = JsonUtility.FromJson<ProfileData>(File.ReadAllText(gamePath));
            int[] itemArray = (int[])thisProfile.items.Clone();
            if (thisProfile.locations == null)
            {
                thisProfile.locations = new int[blankProfile.locations.Length];
                for (int i = 0; i < itemArray.Length; i++)
                    thisProfile.locations[i] = itemArray[i];
            }
            if (itemArray.Length == 54) // The length of the item array up to v0.3.2 before the overhaul
            {
                int legacyOffsetHearts = 13;
                int legacyMaxHearts = 11;
                int legacyOffsetFragments = 24;
                int legacyMaxFragments = 30;
                int[] newItemArray = new int[16];
                for (int i = 0; i < legacyOffsetHearts; i++)
                    newItemArray[i] = itemArray[i];
                for (int i = 0; i < legacyMaxHearts; i++)
                    newItemArray[legacyOffsetHearts] += itemArray[legacyOffsetHearts + i];
                for (int i = 0; i < legacyMaxFragments; i++)
                    newItemArray[legacyMaxHearts + 1] += itemArray[legacyOffsetFragments + i];
                thisProfile.items = newItemArray;
            }
            if (setAsCurrent)
                currentProfile = thisProfile;
        }

        ProfileRandoData thisRandoData = blankRando;
        string randoPath = Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_RandoData" + profile + ".json";
        if (File.Exists(randoPath))
        {
            thisRandoData = JsonUtility.FromJson<ProfileRandoData>(File.ReadAllText(randoPath));
            if (setAsCurrent)
                currentRando = thisRandoData;
        }

        switch (profile)
        {
            case 1:
                profile1 = thisProfile;
                rando1 = thisRandoData;
                break;
            case 2:
                profile2 = thisProfile;
                rando2 = thisRandoData;
                break;
            case 3:
                profile3 = thisProfile;
                rando3 = thisRandoData;
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

        generalData = blankData;
        string path = Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_OptionsAndRecords.json";
        if (File.Exists(path))
        {
            GeneralData newData = JsonUtility.FromJson<GeneralData>(File.ReadAllText(path));
            if (newData.gameVersion != null)
                generalData.gameVersion = newData.gameVersion;
            generalData.soundVolume = newData.soundVolume;
            generalData.musicVolume = newData.musicVolume;
            generalData.windowSize = newData.windowSize;
            generalData.minimapState = newData.minimapState;
            generalData.bottomKeyState = newData.bottomKeyState;
            generalData.keymapState = newData.keymapState;
            generalData.timeState = newData.timeState;
            generalData.FPSState = newData.FPSState;
            generalData.shootMode = newData.shootMode;
            if (newData.texturePackID != null)
                generalData.texturePackID = newData.texturePackID;
            if (newData.musicPackID != null)
                generalData.musicPackID = newData.musicPackID;
            if (newData.soundPackID != null)
                generalData.soundPackID = newData.soundPackID;
            if (newData.textPackID != null)
                generalData.textPackID = newData.textPackID;
            generalData.particleState = newData.particleState;
            generalData.breakableState = newData.breakableState;
            generalData.secretMapTilesVisible = newData.secretMapTilesVisible;
            generalData.frameLimiter = newData.frameLimiter;
            generalData.screenShake = newData.screenShake;
            generalData.paletteFilterState = newData.paletteFilterState;
            generalData.controllerFaceType = newData.controllerFaceType;
            generalData.gravSwapType = newData.gravSwapType;
            generalData.gravKeepType = newData.gravKeepType;
            generalData.darknessLevel = newData.darknessLevel;
            if (newData.keyboardInputs != null)
                generalData.keyboardInputs = (KeyCode[])newData.keyboardInputs.Clone();
            if (newData.controllerInputs != null)
                generalData.controllerInputs = (Control.ControllerBinds[])newData.controllerInputs.Clone();
            if (newData.achievements != null)
                generalData.achievements = (bool[])newData.achievements.Clone();
            if (newData.times != null)
                generalData.times = (float[])newData.times.Clone();
            if (newData.timeVers != null)
                generalData.timeVers = (int[])newData.timeVers.Clone();
        }

        int currentLength = generalData.achievements.Length;
        int intendedLength = Enum.GetNames(typeof(AchievementPanel.Achievements)).Length;
        if (currentLength < intendedLength)
        {
            List<bool> newList = new();
            for (int i = 0; i < currentLength - 1; i++)
                newList.Add(generalData.achievements[i]);
            while (newList.Count < intendedLength)
                newList.Add(false);
            generalData.achievements = newList.ToArray();
        }

        if (Control.keyboardInputs.Length != Control.defaultKeyboardInputs.Length)
        {
            Control.keyboardInputs = (KeyCode[])Control.defaultKeyboardInputs.Clone();
            generalData.keyboardInputs = (KeyCode[])Control.defaultKeyboardInputs.Clone();
        }
        if (Control.controllerInputs.Length != Control.defaultControllerInputs.Length)
        {
            Control.controllerInputs = (Control.ControllerBinds[])Control.defaultControllerInputs.Clone();
            generalData.controllerInputs = (Control.ControllerBinds[])Control.defaultControllerInputs.Clone();
        }

        SaveAll();
    }

    public static void EraseGame(int profile) {
        if (currentProfileNumber == profile)
        {
            currentProfileNumber = 0;
            currentProfile = BlankProfile();
            currentRando = BlankRando();
        }    
        switch (profile)
        {
            case 1:
                profile1 = BlankProfile();
                rando1 = BlankRando();
                File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_Profile1.json", JsonUtility.ToJson(profile1));
                File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_RandoData1.json", JsonUtility.ToJson(rando1));
                break;
            case 2:
                profile2 = BlankProfile();
                rando2 = BlankRando();
                File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_Profile2.json", JsonUtility.ToJson(profile2));
                File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_RandoData2.json", JsonUtility.ToJson(rando2));
                break;
            case 3:
                profile3 = BlankProfile();
                rando3 = BlankRando();
                File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_Profile3.json", JsonUtility.ToJson(profile3));
                File.WriteAllText(Application.persistentDataPath + "/Saves/" + SAVE_FILE_PREFIX + "_RandoData3.json", JsonUtility.ToJson(rando3));
                break;
        }
    }

    public static void LoadPacks()
    {
        string[] packNames = new string[] { generalData.texturePackID, generalData.soundPackID, generalData.musicPackID, generalData.textPackID };
        for (int i = 0; i < 4; i++)
        {
            string thisPackName = packNames[i];
            string packType = i switch { 1 => "Sound", 2 => "Music", 3 => "Text", _ => "Texture" };
            string packPath = string.Format("{0}/{1}Packs/{2}", Application.persistentDataPath, packType, thisPackName);
            bool usePack = thisPackName != "DEFAULT" && Directory.Exists(packPath);

            switch (i)
            {
                case 0:
                    if (usePack)
                        textureLibrary.BuildLibrary(packPath);
                    else
                    {
                        textureLibrary.BuildDefaultSpriteSizeLibrary();
                        textureLibrary.BuildDefaultAnimLibrary();
                        textureLibrary.BuildDefaultLibrary();
                    }
                    textureLibrary.BuildTilemap();
                    break;
                case 1:
                    if (usePack)
                        soundLibrary.BuildLibrary(packPath);
                    else
                        soundLibrary.BuildDefaultLibrary();
                    break;
                case 2:
                    if (usePack)
                    {
                        musicLibrary.BuildOffsetLibrary(packPath + "/MusicLoopOffsets.json");
                        musicLibrary.BuildLibrary(packPath);
                    }
                    else
                    {
                        musicLibrary.BuildDefaultLibrary();
                        musicLibrary.BuildDefaultOffsetLibrary();
                    }
                    break;
                case 3:
                    if (usePack)
                        textLibrary.BuildLibrary(packPath + "/Text.json");
                    else
                        textLibrary.BuildDefaultLibrary();
                    break;
            }
        }
        //for (int i = 0; i < 4; i++)
        //{
        //    string packType = i switch { 1 => "Sound", 2 => "Music", 3 => "Text", _ => "Texture" };
        //    string[] packNames = new string[] { generalData.texturePackID, generalData.soundPackID, generalData.musicPackID, generalData.textPackID };
        //    bool loadDefault = false;
        //
        //    if (packNames[i] != "DEFAULT")
        //    {
        //        string path = Application.persistentDataPath + "/" + packType + "Packs/" + packNames[i];
        //        if (Directory.Exists(path))
        //        {
        //            switch (packType)
        //            {
        //                case "Texture":
        //                    textureLibrary.BuildSpriteSizeLibrary(path + "/SpriteSizes.json");
        //                    textureLibrary.BuildAnimationLibrary(path + "/Animations.json");
        //                    textureLibrary.BuildLibrary(path);
        //                    textureLibrary.BuildTilemap();
        //                    break;
        //                case "Sound":
        //                    soundLibrary.BuildLibrary(path);
        //                    break;
        //                case "Music":
        //                    musicLibrary.BuildOffsetLibrary(path + "/MusicLoopOffsets.json");
        //                    musicLibrary.BuildLibrary(path);
        //                    break;
        //                case "Text":
        //                    textLibrary.BuildLibrary(path + "/Text.json");
        //                    break;
        //            }
        //        }
        //        else
        //            loadDefault = true;
        //    }
        //    else
        //        loadDefault = true;
        //
        //    if (loadDefault)
        //    {
        //        switch (packType)
        //        {
        //            case "Texture":
        //                textureLibrary.BuildDefaultSpriteSizeLibrary();
        //                textureLibrary.BuildDefaultLibrary();
        //                textureLibrary.BuildDefaultAnimLibrary();
        //                textureLibrary.BuildTilemap();
        //                break;
        //            case "Sound":
        //                soundLibrary.BuildDefaultLibrary();
        //                break;
        //            case "Music":
        //                musicLibrary.BuildDefaultLibrary();
        //                musicLibrary.BuildDefaultOffsetLibrary();
        //                break;
        //            case "Text":
        //                textLibrary.BuildDefaultLibrary();
        //                break;
        //        }
        //    }
        //}
    }

    public static void CheckControlsAreUpToDate() {
        if (generalData.keyboardInputs.Length != Enum.GetNames(typeof(Control.Keyboard)).Length)
            generalData.keyboardInputs = Control.defaultKeyboardInputs;
        if (generalData.controllerInputs.Length != Enum.GetNames(typeof(Control.Controller)).Length)
            generalData.controllerInputs = Control.defaultControllerInputs;
    }

    public static void SetTime(TimeIndeces target, float[] newTime)
    {
        if (newTime.Length != 3)
            return;
        int startID = (int)target * 3;
        generalData.times[startID] = newTime[0];
        generalData.times[startID + 1] = newTime[1];
        generalData.times[startID + 2] = newTime[2];
        SetTimeVersion(target, Application.version, true);
    }

    public static void SetTimeVersion(TimeIndeces target, string newVer, bool save = false)
    {
        if (newVer.Contains(' '))
            newVer = newVer.Split(' ')[1];
        string[] verParts = newVer.Split('.');
        SetTimeVersion(target, new int[] { int.Parse(verParts[0]), int.Parse(verParts[1]), int.Parse(verParts[2]) }, save);
    }
    public static void SetTimeVersion(TimeIndeces target, int[] newVer, bool save = false)
    {
        int startID = (int)target * 3;
        generalData.timeVers[startID] = newVer[0];
        generalData.timeVers[startID + 1] = newVer[1];
        generalData.timeVers[startID + 2] = newVer[2];
        if (save)
            WriteSave(0, true);
    }

    public static float[] GetTime(TimeIndeces target)
    {
        int startID = (int)target * 3;
        return new float[] { generalData.times[startID], generalData.times[startID + 1], generalData.times[startID + 2] };
    }

    public static string GetTimeVersion(TimeIndeces target)
    {
        int startID = (int)target * 3;
        return string.Format("{0}.{1}.{2}", generalData.timeVers[startID], generalData.timeVers[startID + 1], generalData.timeVers[startID + 2]);
    }

    public static string GetCurrentVersion()
    {
        return Application.version.Split(' ')[1];
    }

    public static bool HasTime(TimeIndeces ID = TimeIndeces.none)
    {
        int rowCount = generalData.times.Length / 3;

        if (ID == TimeIndeces.none)
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
            int intID = (int)ID * 3;
            if (generalData.times[intID] == 0 && generalData.times[intID + 1] == 0 && generalData.times[intID + 2] == 0)
                return false;
            return true;
        }
    }
    public static bool HasTime(TimeCategories ID)
    {
        return ID switch
        {
            TimeCategories.insane => HasTime(TimeIndeces.snailyInsane) || HasTime(TimeIndeces.sluggyInsane) || HasTime(TimeIndeces.upsideInsane) ||
                HasTime(TimeIndeces.leggyInsane) || HasTime(TimeIndeces.blobbyInsane) || HasTime(TimeIndeces.leechyInsane),
            TimeCategories.hundo => HasTime(TimeIndeces.snaily100) || HasTime(TimeIndeces.sluggy100) || HasTime(TimeIndeces.upside100) ||
                HasTime(TimeIndeces.leggy100) || HasTime(TimeIndeces.blobby100) || HasTime(TimeIndeces.leechy100),
            TimeCategories.rush => HasTime(TimeIndeces.snailyRush) || HasTime(TimeIndeces.sluggyRush) || HasTime(TimeIndeces.upsideRush) ||
                HasTime(TimeIndeces.leggyRush) || HasTime(TimeIndeces.blobbyRush) || HasTime(TimeIndeces.leechyRush),
            _ => HasTime(TimeIndeces.snailyNormal) || HasTime(TimeIndeces.sluggyNormal) || HasTime(TimeIndeces.upsideNormal) ||
                HasTime(TimeIndeces.leggyNormal) || HasTime(TimeIndeces.blobbyNormal) || HasTime(TimeIndeces.leechyNormal)
        };
    }

    // Returns
    // -1, if time A is less than time B
    // 0, if the two times are equal
    // 1, if time A is greater than time B
    public static int CompareTimes(TimeIndeces time1, TimeIndeces time2)
    {
        return CompareTimes(GetTime(time1), GetTime(time2));
    }
    public static int CompareTimes(TimeIndeces indexTime, float[] rawTime)
    {
        return CompareTimes(GetTime(indexTime), rawTime);
    }
    public static int CompareTimes(float[] rawTime, TimeIndeces indexTime)
    {
        return CompareTimes(rawTime, GetTime(indexTime));
    }
    public static int CompareTimes(float[] time1, float[] time2)
    {
        if (time1[0] < time2[0])
            return -1;
        else if (time1[0] > time2[0])
            return 1;
        else
        {
            if (time1[1] < time2[1])
                return -1;
            else if (time1[1] > time2[1])
                return 1;
            else
            {
                if (time1[2] < time2[2])
                    return -1;
                else if (time1[2] > time2[2])
                    return 1;
                else
                    return 0;
            }
        }
    }

    public static void QueueAchievementPopup(AchievementPanel.Achievements achID)
    {
        if (!generalData.achievements[(int)achID])
        {
            achievement.GetComponent<AchievementPanel>().popupQueue.Add(achID);
            generalData.achievements[(int)achID] = true;
        }
    }

    public static bool HasAchievemements()
    {
        bool found = false;
        foreach (bool achState in generalData.achievements)
            found = found || achState;
        return found;
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

    public static bool ShootEnemyBullet(Enemy sourceEnemy, Vector2 newOrigin, EnemyBullet.BulletType type, float angle, float newSpeed, bool playSound = true) {
        Vector2 newAngle = Quaternion.Euler(0, 0, angle) * Vector2.up;
        return ShootEnemyBullet(sourceEnemy, newOrigin, type, new float[] { newSpeed, newAngle.x, newAngle.y }, playSound);
    }
    public static bool ShootEnemyBullet(Enemy sourceEnemy, Vector2 newOrigin, EnemyBullet.BulletType type, float[] dirVelVars, bool playSound = true) {
        bool hasShot = false;
        if (!enemyBulletPool.transform.GetChild(enemyBulletPointer).GetComponent<EnemyBullet>().isActive) {
            enemyBulletPool.transform.GetChild(enemyBulletPointer).GetComponent<EnemyBullet>().Shoot(sourceEnemy, newOrigin, type, dirVelVars, playSound);
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

    public static void ToggleBossfightState(bool state, int musicID, bool snapDespawnBar = false)
    {
        inBossFight = state;
        TogglableHUDElements[0].SetActive(!state && !isInBossRush);
        if (!state)
        {
            if (snapDespawnBar)
            {
                TogglableHUDElements[12].GetComponent<SpriteRenderer>().enabled = false;
                TogglableHUDElements[12].transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                globalFunctions.displayDefeatText = false;
                if (!isInBossRush)
                    globalFunctions.RemoveGigaBackgroundLayers();
            }
            else
            {
                TogglableHUDElements[12].GetComponent<AnimationModule>().Play("BossBar_frame_despawn");
                TogglableHUDElements[12].transform.GetChild(0).GetComponent<AnimationModule>().Play("BossBar_bar_despawn");
                globalFunctions.displayDefeatText = true;
            }
        }
        if (currentArea != (int)Areas.BossRush) {
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

    public static string VectorToCompass(Vector2 dir)
    {
        float angle = Vector2.SignedAngle(Vector2.up, dir);
        while (angle < 0)
            angle += 360;
        while (angle > 360)
            angle -= 360;
        if (angle > 337.5f)
            return "N";
        else if (angle > 292.5f)
            return "NE";
        else if (angle > 247.5f)
            return "E";
        else if (angle > 202.5f)
            return "SE";
        else if (angle > 157.5f)
            return "S";
        else if (angle > 112.5f)
            return "SW";
        else if (angle > 67.5f)
            return "W";
        else if (angle > 22.5f)
            return "NW";
        else
            return "N";
    }

    public static string VectorToCardinal(Vector2 dir)
    {
        float angle = Vector2.SignedAngle(Vector2.up, dir);
        while (angle < 0)
            angle += 360;
        while (angle > 360)
            angle -= 360;
        if (angle > 315f)
            return "up";
        else if (angle > 225f)
            return "right";
        else if (angle > 135f)
            return "down";
        else if (angle > 45f)
            return "left";
        else
            return "up";
    }

    public static bool IsControllerConnected()
    {
        string[] controllers = Input.GetJoystickNames();
        if (controllers.Length == 1)
        {
            if (controllers[0] == "" || controllers[0] == " ")
                return false;
        }
        return Input.GetJoystickNames().Length > 0;
    }
}
