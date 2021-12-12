﻿using System.Collections;
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

    public static int currentProfile = 1;
    public static int currentDifficulty = 1; // 1 = Easy, 2 = Normal, 3 = Insane
    public static string currentCharacter = "";
    public static float[] currentTime = new float[] { 0, 0, 0 };

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

    public const byte OFFSET_HEARTS = 12;
    public const byte OFFSET_FRAGMENTS = 23;

    public bool hasSeenIris;
    public bool talkedToCaveSnail;

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

    public static byte TranslateItemNameToID(string itemName)
    {
        byte id = 0;
        if (itemName.Contains("Heart Container"))
        {
            id = byte.Parse(itemName.Substring(15, itemName.Length));
        }
        else if (itemName.Contains("Helix Fragment"))
        {
            id = byte.Parse(itemName.Substring(14, itemName.Length));
        }
        else
        {
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
        }
        return id;
    }

    public static string TranslateIDToItemName(int itemID)
    {
        string name = "";
        if (itemID >= 23)
            name = "Helix Fragment " + (itemID - 23);
        else if (itemID >= 12)
            name = "Heart Container " + (itemID - 12);
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
            }
        }
        return name;
    }

    public void WriteSave(string dataType)
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
            string gameData = "/";

            // Profile
            gameData += currentProfile + "/";

            // Difficulty
            gameData += currentDifficulty + "/";

            // Time
            gameData += currentTime[0] + "," + currentTime[1] + "," + currentTime[2] + "/";

            // Save position
            gameData += respawnCoords.x + "/" + respawnCoords.y + "/";

            // Character
            gameData += currentCharacter + "/";

            // Items
            int index = 0;
            while (index < itemCollection.Length)
            {
                gameData += itemCollection[index] + (index == itemCollection.Length - 1 ? "/" : ",");
                index++;
            }

            // Selected weapon
            gameData += player.GetComponent<Player>().selectedWeapon + "/";

            // Boss states
            index = 0;
            while (index < bossStates.Length)
            {
                gameData += bossStates[index] + (index == itemCollection.Length - 1 ? "/" : ",");
                index++;
            }

            // NPC vars
            gameData += (hasSeenIris ? "1," : "0,") + (talkedToCaveSnail ? "1/" : "0/");

            PlayerPrefs.SetString("SaveGameData" + currentProfile, gameData);
        }
        else if (dataType == "options")
        {

        }
        else
        {
            Debug.Log("Invalid save type!");
        }
    }
}
