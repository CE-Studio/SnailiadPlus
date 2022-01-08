using System;
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
    public static AudioClip[][] areaMusic = new AudioClip[][]
    {
        new AudioClip[]
        {
            (AudioClip)Resources.Load("Sounds/Music/SnailTown"),
            (AudioClip)Resources.Load("Sounds/Music/TestZone")
        }
    };
    public static int currentArea = -1;
    public static int currentSubzone = -1;

    public static GameObject player = GameObject.Find("Player");
    public static Player playerScript = player.GetComponent<Player>();
    public static GameObject cam = GameObject.Find("View");
    public static GameObject screenCover = GameObject.Find("View/Cover");
    public static GameObject fg2Layer = GameObject.Find("Grid/Foreground 2");
    public static GameObject fg1Layer = GameObject.Find("Grid/Foreground");
    public static GameObject bgLayer = GameObject.Find("Grid/Background");
    public static GameObject skyLayer = GameObject.Find("Grid/Sky");
    public static GameObject minimap = GameObject.Find("View/Minimap Panel/Minimap");
    public static GameObject achievement = GameObject.Find("View/Achievement Panel");
    public static GameObject explosionPool = GameObject.Find("Explosion Pool");

    public static bool paralyzed = false;
    public static bool isArmed = false;
    public static bool hasRainbowWave = false;
    public static bool hasGravitySnail = false;

    public static Vector2 camCenter;
    public static Vector2 camBoundaryBuffers;
    public static Vector2 camTempBuffers;
    public static Vector2 camTempBuffersX;
    public static Vector2 camTempBuffersY;
    public static Vector2 posRelativeToTempBuffers;
    public static Vector2 camTempBufferTruePos;

    public static Vector2 respawnCoords = new Vector2(6, 8.5f);
    public static Scene respawnScene = SceneManager.GetActiveScene();

    public static TextMesh fpsText = GameObject.Find("View/FPS Text").GetComponent<TextMesh>();
    public static TextMesh fpsShadow = GameObject.Find("View/FPS Shadow").GetComponent<TextMesh>();

    public static int currentProfile = 1;
    public static int currentDifficulty = 1; // 1 = Easy, 2 = Normal, 3 = Insane
    public static string currentCharacter = "";
    public static float[] currentTime = new float[] { 0, 0, 0 };

    public static int helixCount;
    public static int heartCount;
    public static int itemPercentage;

    public static int[] defaultMinimapState = new int[]
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
        0,  // Debug Rainbow Wave
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // Heart Containers
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0  // Helix Fragments
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
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    };

    public const byte OFFSET_HEARTS = 13;
    public const byte OFFSET_FRAGMENTS = 24;

    public static bool hasSeenIris;
    public static bool talkedToCaveSnail;

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
        public int[] achievements;
    }

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

    public static void PlayAreaSong(int area, int subzone)
    {
        if (area == currentArea && subzone != currentSubzone)
        {
            float playTime = mus.time;
            mus.clip = areaMusic[area][subzone];
            mus.Play();
            mus.time = playTime;
        }
        else if (area != currentArea)
        {
            mus.clip = areaMusic[area][subzone];
            mus.Play();
        }
        currentArea = area;
        currentSubzone = subzone;
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

    public static bool CheckBossState(int bossID)
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

    public static int GetItemPercentage()
    {
        int itemsFound = 0;
        foreach (int itemStatus in itemCollection)
        {
            if (itemStatus == 1)
                itemsFound++;
        }
        return Mathf.FloorToInt(((float)itemsFound / (float)itemCollection.Length) * 100);
    }

    public static void WriteSave(string dataType)
    {
        if (dataType == "game")
        {
            // Save data is stored in the following order:
            // - Profile beign played on
            // - Difficulty being played on
            // - Game time, saved as hours, minutes, and seconds + hundredths
            // - World position of the player's last save point, individually saved as two floats
            // - Current campaign character
            // - Item list
            // - Selected weapon
            // - Boss states
            // - NPC variables
            GameSaveData data = new GameSaveData();
            data.profile = currentProfile;
            data.difficulty = currentDifficulty;
            data.gameTime = currentTime;
            data.saveCoords = respawnCoords;
            data.character = currentCharacter;
            data.items = itemCollection;
            data.weapon = player.GetComponent<Player>().selectedWeapon;
            data.bossStates = bossStates;
            data.NPCVars = new int[]
            {
                hasSeenIris ? 1 : 0,
                talkedToCaveSnail ? 1 : 0
            };
            data.achievements = achievementStates;
            string saveData = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("SaveGameData" + currentProfile, saveData);
            PlayerPrefs.Save();
        }
        else if (dataType == "options")
        {

        }
        else
        {
            Debug.Log("Invalid save type!");
        }
    }

    public static void QueueAchievementPopup(string achID)
    {
        achievement.GetComponent<AchievementPanel>().popupQueue.Add(achID);
    }
}
