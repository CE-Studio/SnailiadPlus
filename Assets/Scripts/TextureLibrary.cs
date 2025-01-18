using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Texture Library", menuName = "Scriptable Objects/Texture Library", order = 1)]
public class TextureLibrary : ScriptableObject
{
    public Sprite[][] library;

    public string[] referenceList = new string[]
    {
        "AchievementIcons",
        "Door",
        "HelixLockedDoor",
        "MapTiles",
        "MenuPlus",
        "Player",
        "SavePoint",
        "Tilesheet",
        "TitleFont",

        "Backgrounds/EndingBackground",
        "Backgrounds/GigaBackground",
        "Backgrounds/IntroBackground",

        "Bullets/Boomerang",
        "Bullets/BoomerangDev",
        "Bullets/Broom",
        "Bullets/BroomDev",
        "Bullets/EnemyBoomerangBlue",
        "Bullets/EnemyBoomerangRed",
        "Bullets/EnemyDonut",
        "Bullets/EnemyGigaPeashooter",
        "Bullets/EnemyGigaWave",
        "Bullets/EnemyLaser",
        "Bullets/EnemyLightWave",
        "Bullets/EnemyShadowWave",
        "Bullets/EnemySpikeBall",
        "Bullets/Peashooter",
        "Bullets/PeashooterDev",
        "Bullets/RainbowWave",
        "Bullets/RainbowWaveDev",
        "Bullets/ShockBody",
        "Bullets/ShockWave",
        "Bullets/ShockWaveDev",

        "Endings/Ending100",
        "Endings/EndingBackground",
        "Endings/EndingBossRush",
        "Endings/EndingInsane",
        "Endings/EndingNormal",
        "Endings/EndingSub30",
        "Endings/TheEnd",

        "Entities/AngryBlock",
        "Entities/AngryBlockFace",
        "Entities/Babyfish",
        "Entities/BalloonBuster",
        "Entities/Bat",
        "Entities/Blob",
        "Entities/Boss1",
        "Entities/Boss1Eyes",
        "Entities/Boss1Hand",
        "Entities/Boss1Rush",
        "Entities/Boss1RushEyes",
        "Entities/Boss1RushHand",
        "Entities/Boss2Eye",
        "Entities/Boss2Eyelid",
        "Entities/Boss2Foot",
        "Entities/Boss2Pupil",
        "Entities/Boss2RushEye",
        "Entities/Boss2RushEyelid",
        "Entities/Boss2RushFoot",
        "Entities/Boss2RushPupil",
        "Entities/Boss3",
        "Entities/Boss3Babybox",
        "Entities/Boss3Shield",
        "Entities/Boss3Rush",
        "Entities/Boss3RushBabybox",
        "Entities/Boss3RushShield",
        "Entities/Boss4",
        "Entities/Boss4SecondForm",
        "Entities/Boss4ShadowBall",
        "Entities/Boss4Zzz",
        "Entities/Boss4Rush",
        "Entities/Boss4RushSecondForm",
        "Entities/Boss4RushShadowBall",
        "Entities/Boss4RushZzz",
        "Entities/BossBlocks",
        "Entities/BreakableIcons",
        "Entities/Cannon1",
        "Entities/Cannon1Base",
        "Entities/Cannon2",
        "Entities/Cannon2Base",
        "Entities/Chirpy1",
        "Entities/Chirpy2",
        "Entities/Dandelion",
        "Entities/Drone",
        "Entities/EndingMoonSnail",
        "Entities/ExtraPlatform",
        "Entities/Fire",
        "Entities/Fireball",
        "Entities/Fireball2",
        "Entities/Floatspike1",
        "Entities/Floatspike2",
        "Entities/ForTheFunny",
        "Entities/Gear",
        "Entities/GravTurtle1",
        "Entities/GravTurtle2",
        "Entities/Grass",
        "Entities/HealthOrb",
        "Entities/HelixLockPopup",
        "Entities/IceSpike",
        "Entities/Jellyfish",
        "Entities/Kitty",
        "Entities/Kitty2",
        "Entities/LightMask",
        "Entities/LightMaskLarge",
        "Entities/Muck",
        "Entities/Pincer",
        "Entities/PixelPeople",
        "Entities/Platform",
        "Entities/PowerGrass",
        "Entities/Seahorse",
        "Entities/SnailNpc",
        "Entities/SnailNpcColor",
        "Entities/Snake",
        "Entities/Snake2",
        "Entities/Snake3",
        "Entities/Snelk",
        "Entities/Spider1",
        "Entities/Spider2",
        "Entities/Spikey1",
        "Entities/Spikey2",
        "Entities/Tallfish1",
        "Entities/Tallfish2",
        "Entities/TurtleNpc",
        "Entities/Walleye",

        "Intro/Intro1",
        "Intro/Intro2",
        "Intro/Intro3A",
        "Intro/Intro3B",
        "Intro/Intro3C",
        "Intro/Intro4A",
        "Intro/Intro4B",
        "Intro/Intro4C",
        "Intro/Intro4D",
        "Intro/Intro4E",
        "Intro/Intro4F",

        "Items/AngelJump",
        "Items/APMajor",
        "Items/APMinor",
        "Items/Backfire",
        "Items/Boomerang",
        "Items/CorkscrewJump",
        "Items/Devastator",
        "Items/FullMetalSnail",
        "Items/GravityShock",
        "Items/GravitySnail",
        "Items/HeartContainer",
        "Items/HelixFragment",
        "Items/HighJump",
        "Items/IceSnail",
        "Items/MagneticFoot",
        "Items/MaskedItem",
        "Items/Peashooter",
        "Items/ProgressiveShell",
        "Items/ProgressiveWeapon",
        "Items/ProgressiveWeaponMod",
        "Items/RainbowWave",
        "Items/RapidFire",
        "Items/ShellShield",
        "Items/Shelmet",
        "Items/TrapItem",
        "Items/WallGrab",

        "Particles/AngelJumpEffect",
        "Particles/APItemDust",
        "Particles/APItemFlash",
        "Particles/Bubble",
        "Particles/Dot",
        "Particles/Dust",
        "Particles/EndingZzz",
        "Particles/Explosion",
        "Particles/Fog",
        "Particles/GigaTrail",
        "Particles/IntroBGPatterns",
        "Particles/Lightning",
        "Particles/Nom",
        "Particles/Parry",
        "Particles/Rain",
        "Particles/RushGigaTrail",
        "Particles/Shield",
        "Particles/ShockChar",
        "Particles/ShockCharge",
        "Particles/ShockLaunch",
        "Particles/Smoke",
        "Particles/Snow",
        "Particles/Splash",
        "Particles/Star",
        "Particles/Transformation",
        "Particles/Zzz",

        "UI/AchievementFrame",
        "UI/AchievementPanel",
        "UI/BossHealthBar",
        "UI/ControlHelp",
        "UI/ControlIcons",
        "UI/DebugIcons",
        "UI/DebugKey",
        "UI/DialogueBox",
        "UI/DialogueIcon2",
        "UI/DialoguePortrait",
        "UI/FontSprites",
        "UI/GeneratorLoading",
        "UI/GenericHighlightBox",
        "UI/Heart",
        "UI/LoadingIcon",
        "UI/Minimap",
        "UI/MinimapIcons",
        "UI/MinimapMask",
        "UI/MinimapPanel",
        "UI/SpeechBubble",
        "UI/Subscreen",
        "UI/TrapTimer",
        "UI/WeaponIcons"
    };

    public Sprite[] Unpack(Texture2D texture, int sliceWidth, int sliceHeight, string name)
    {
        List<Sprite> unpackedArray = new();
        int counter = 0;
        if (texture.name == "Tilesheet")
        {
            Tile[] tiles = Resources.LoadAll<Tile>("Images/Tilesheet images");
            for (int i = 0; i < tiles.Length; i++)
            {
                Sprite oldSprite = Resources.Load<Tile>("Images/Tilesheet images/Tilesheet_" + i).sprite;
                Sprite newSprite = Sprite.Create(texture, oldSprite.textureRect, new Vector2(0.5f, 0.5f), 16);
                newSprite.name = oldSprite.name;
                unpackedArray.Add(newSprite);
                counter++;
            }
        }
        else
        {
            for (int i = texture.height - sliceHeight; i >= 0; i -= sliceHeight)
            {
                for (int j = 0; j < texture.width; j += sliceWidth)
                {
                    Sprite newSprite = Sprite.Create(texture, new Rect(j, i, sliceWidth, sliceHeight), new Vector2(0.5f, 0.5f), 16);
                    newSprite.name = name + " " + counter;
                    unpackedArray.Add(newSprite);
                    counter++;
                }
            }
        }
        Sprite[] finalArray = unpackedArray.ToArray();
        return finalArray;
    }

    public Vector2 GetSpriteSize(string name)
    {
        Vector2 size = Vector2.zero;
        int i = 0;
        while (i < PlayState.spriteSizeLibrary.Length && size == Vector2.zero)
        {
            if (PlayState.spriteSizeLibrary[i].name == name)
                size = new Vector2(PlayState.spriteSizeLibrary[i].width, PlayState.spriteSizeLibrary[i].height);
            i++;
        }
        return size;
    }

    public void BuildDefaultLibrary()
    {
        BuildDefaultSpriteSizeLibrary();
        List<Sprite[]> newLibrary = new();
        for (int i = 0; i < referenceList.Length; i++)
        {
            Vector2 thisSize = GetSpriteSize(referenceList[i]);
            if (thisSize == Vector2.zero || thisSize.x < 0 || thisSize.y < 0)
                thisSize = new Vector2(16, 16);
            newLibrary.Add(Unpack((Texture2D)Resources.Load("Images/" + referenceList[i]), (int)thisSize.x, (int)thisSize.y, referenceList[i]));
        }
        library = newLibrary.ToArray();
        GetNewTextWidths();
    }

    public void BuildDefaultAnimLibrary()
    {
        TextAsset animJson = Resources.Load<TextAsset>("Animations");
        PlayState.AnimationLibrary newLibrary = JsonUtility.FromJson<PlayState.AnimationLibrary>(animJson.text);
        PlayState.animationLibrary = newLibrary.animArray;
    }

    public void BuildDefaultSpriteSizeLibrary()
    {
        TextAsset sizeJson = Resources.Load<TextAsset>("SpriteSizes");
        PlayState.SpriteSizeLibrary newLibrary = JsonUtility.FromJson<PlayState.SpriteSizeLibrary>(sizeJson.text);
        PlayState.spriteSizeLibrary = newLibrary.sizeArray;
    }

    public void BuildTilemap()
    {
        Sprite tilemap = PlayState.GetSprite("Tilesheet");
        Transform grid = GameObject.Find("Grid").transform;
        List<Tilemap> maps = new();
        List<int> swappedIDs = new();
        foreach (Transform layer in grid)
        {
            if (layer.name != "Special")
                maps.Add(layer.GetComponent<Tilemap>());
        }
        foreach (Tilemap map in maps)
        {
            for (int y = 0; y < map.size.y; y++)
            {
                for (int x = 0; x < map.size.x; x++)
                {
                    Vector3Int worldPos = new(Mathf.RoundToInt(map.origin.x - (map.size.x * 0.5f) + x), Mathf.RoundToInt(map.origin.y - (map.size.y * 0.5f) + y), 0);
                    if (map.GetSprite(worldPos) != null)
                    {
                        Sprite tileSprite = map.GetSprite(worldPos);
                        int spriteID = int.Parse(tileSprite.name.Split('_', ' ')[1]);
                        if (!swappedIDs.Contains(spriteID))
                        {
                            TileBase tile = map.GetTile(worldPos);
                            Tile newTile = CreateInstance<Tile>();
                            Sprite newSprite = Sprite.Create(tilemap.texture, tileSprite.rect, new(0.5f, 0.5f), 16);
                            newSprite.OverridePhysicsShape(new List<Vector2[]> {
                                new Vector2[] { new Vector2(0, 0), new Vector2(0, 16), new Vector2(16, 16), new Vector2(16, 0) }
                                });
                            newSprite.name = "Tilesheet_" + spriteID;
                            newTile.sprite = newSprite;
                            newTile.name = "Tilesheet_" + spriteID;
                            foreach (Tilemap thisMap in maps)
                                thisMap.SwapTile(tile, newTile);
                            swappedIDs.Add(spriteID);
                        }
                    }
                }
            }
        }
    }

    public void BuildLibrary(string folderPath = null)
    {
        BuildDefaultLibrary();
        BuildSpriteSizeLibrary(folderPath + "/SpriteSizes.json");
        BuildAnimationLibrary(folderPath + "/Animations.json");
        if (folderPath != null && Directory.Exists(folderPath))
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
                    if (file.Substring(file.Length - 3, 3).ToLower() == "png")
                    {
                        string fileName = file.Replace('\\', '/').Substring(folderPath.Length + 1, file.Length - folderPath.Length - 1).Split('.')[0];
                        if (InReferenceList(fileName))
                        {
                            byte[] rawSpriteData = File.ReadAllBytes(file);
                            Texture2D newTexture = new(128, 1);
                            newTexture.LoadImage(rawSpriteData);
                            Vector2 thisSize = GetSpriteSize(fileName);
                            if (thisSize == Vector2.zero || thisSize.x < 0 || thisSize.y < 0)
                                thisSize = new Vector2(16, 16);
                            library[Array.IndexOf(referenceList, fileName)] = Unpack(newTexture, (int)thisSize.x, (int)thisSize.y, fileName);
                        }
                    }
                }
            }
        }
        GetNewTextWidths();
    }

    public void BuildAnimationLibrary(string dataPath = null)
    {
        BuildDefaultAnimLibrary();
        if (dataPath != null && File.Exists(dataPath))
        {
            PlayState.LoadNewAnimationLibrary(dataPath);
        }
    }

    public void BuildSpriteSizeLibrary(string dataPath = null)
    {
        BuildDefaultSpriteSizeLibrary();
        if (dataPath != null && File.Exists(dataPath))
        {
            PlayState.LoadNewSpriteSizeLibrary(dataPath);
        }
    }

    public void GetNewTextWidths()
    {
        List<int> newWidths = new();
        for (int i = 0; i < 94; i++)
        {
            Sprite letter = PlayState.GetSprite("UI/FontSprites", i);
            int totalWidth = 0;
            int emptySpaceFound = 0;
            for (int x = 0; x < letter.rect.width; x++)
            {
                bool found = false;
                for (int y = 0; y < letter.rect.height; y++)
                {
                    if (letter.texture.GetPixel(x, y) != new Color32(0, 0, 0, 0))
                        found = true;
                }
                if (!found && totalWidth != 0)
                    emptySpaceFound++;
                else if (found)
                {
                    totalWidth++;
                    while (emptySpaceFound > 0)
                    {
                        totalWidth++;
                        emptySpaceFound--;
                    }
                }
            }
            newWidths.Add(totalWidth);
        }
        newWidths.Add(10);
        PlayState.charWidths = newWidths.ToArray();
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

    public void BuildTemplatePack()
    {
        string mainPath = Application.persistentDataPath + "/TexturePacks";
        string templatePath = mainPath + "/Template (" + PlayState.GetCurrentVersion() + ")";
        Directory.CreateDirectory(templatePath);
        for (int i = 0; i < referenceList.Length; i++)
        {
            string spritePath = referenceList[i];
            string[] pathParts = spritePath.Split('/');
            Texture2D sprite = (Texture2D)Resources.Load("Images/" + spritePath);
            if (sprite.isReadable)
            {
                string directory = templatePath;
                if (pathParts.Length > 1)
                {
                    for (int j = 0; j < pathParts.Length - 1; j++)
                        directory += "/" + pathParts[j];
                }
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                byte[] spriteBytes = sprite.EncodeToPNG();
                File.WriteAllBytes(directory + "/" + pathParts[pathParts.Length - 1] + ".png", spriteBytes);
            }
        }
        TextAsset animJson = Resources.Load<TextAsset>("Animations");
        File.WriteAllText(templatePath + "/Animations.json", animJson.text);
        TextAsset sizeJson = Resources.Load<TextAsset>("SpriteSizes");
        File.WriteAllText(templatePath + "/SpriteSizes.json", sizeJson.text);
        TextAsset infoText = new(
            "AUTHOR (write your name here!)\n" +
            "Unknown\n" +
            "PACK VERSION\n" +
            "0.0\n" +
            "GAME VERSION\n" +
            PlayState.GetCurrentVersion());
        File.WriteAllText(templatePath + "/Info.txt", infoText.text);
    }
}
