using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CreateAssetMenu(fileName = "Texture Library", menuName = "Scriptable Objects/Texture Library", order = 1)]
public class TextureLibrary : ScriptableObject
{
    public Sprite[][] library;

    public string[] referenceList = new string[]
    {
        "AchievementIcons",
        "Door",
        "MapTiles",
        "MenuPlus",
        "SavePoint",
        //"CEStudioLogo2022",
        //"Tilesheet",
        "TitleFont",

        "Bullets/Boomerang",
        "Bullets/RainbowWave",

        "Entities/BreakableIcons",
        "Entities/Floatspike1",
        "Entities/Grass",
        "Entities/PixelPeople",
        "Entities/PowerGrass",
        "Entities/PowerNom",
        "Entities/SnailNpc",
        "Entities/SnailNpcColor",
        "Entities/Spikey1",
        "Entities/TurtleNpc",

        "Items/Boomerang",
        "Items/HeartContainer",
        "Items/HelixFragment",
        "Items/RainbowWave",

        "Particles/Bubble",
        "Particles/Explosion",
        "Particles/Splash",
        "Particles/Star",

        "UI/AchievementPanel",
        "UI/DebugKey",
        "UI/DialogueBox",
        "UI/DialogueIcon2",
        "UI/DialoguePortrait",
        "UI/FontSprites",
        "UI/Heart",
        "UI/Minimap",
        "UI/MinimapIcons",
        "UI/MinimapMask",
        "UI/MinimapPanel",
        "UI/WeaponIcons"
    };

    public Sprite[] Unpack(Sprite texture, int sliceWidth, int sliceHeight)//, string spriteName)
    {
        Texture2D newTexture = new Texture2D((int)texture.rect.width, (int)texture.rect.height);
        newTexture.SetPixels(texture.texture.GetPixels((int)texture.textureRect.x, (int)texture.textureRect.y,
            (int)texture.textureRect.width, (int)texture.textureRect.height));
        newTexture.Apply();
        return Unpack(newTexture, sliceWidth, sliceHeight);//, spriteName);
    }

    public Sprite[] Unpack(Texture2D texture, int sliceWidth, int sliceHeight)//, string spriteName)
    {
        //Debug.Log(texture);
        List<Sprite> unpackedArray = new List<Sprite>();
        int counter = 0;
        for (int i = texture.height - sliceHeight; i >= 0; i -= sliceHeight)
        {
            for (int j = 0; j < texture.width; j += sliceWidth)
            {
                Sprite newSprite = Sprite.Create(texture, new Rect(j, i, sliceWidth, sliceHeight), new Vector2(0.5f, 0.5f), 16);
                //newSprite.name = spriteName + (spriteName[spriteName.Length - 1] == '_' ? "" : "_") + counter;
                unpackedArray.Add(newSprite);
                counter++;
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
        //library = new Sprite[][]
        //{
        //    Unpack((Texture2D)Resources.Load("Images/AchievementIcons"), 50, 50, "General_AchievementIcons_"),
        //    Unpack((Texture2D)Resources.Load("Images/Door"), 48, 48, "General_Door_"),
        //    Unpack((Texture2D)Resources.Load("Images/MapTiles"), 8, 8, "General_MapTiles_"),
        //    Unpack((Texture2D)Resources.Load("Images/MenuPlus"), 32, 32, "General_MenuPlus_"),
        //    Unpack((Texture2D)Resources.Load("Images/SavePoint"), 32, 48, "General_SavePoint_"),
        //    //Unpack((Texture2D)Resources.Load("Images/CEStudioLogo2022"), 594, 1188, "General_StudioLogo_"),
        //    //Unpack((Texture2D)Resources.Load("Images/Tilesheet"), 16, 16, "General_Tileset_"),
        //    Unpack((Texture2D)Resources.Load("Images/TitleFont"), 32, 56, "General_TitleFont_"),
        //
        //    Unpack((Texture2D)Resources.Load("Images/Bullets/Boomerang"), 16, 16, "Bullet_Boomerang_"),
        //    Unpack((Texture2D)Resources.Load("Images/Bullets/RainbowWave"), 32, 32, "Bullet_RainbowWave_"),
        //
        //    Unpack((Texture2D)Resources.Load("Images/Entities/BreakableIcons"), 16, 16, "Entity_BreakableIcons_"),
        //    Unpack((Texture2D)Resources.Load("Images/Entities/Floatspike1"), 16, 16, "Entity_Floatspike1_"),
        //    Unpack((Texture2D)Resources.Load("Images/Entities/Grass"), 16, 16, "Entity_Grass_"),
        //    Unpack((Texture2D)Resources.Load("Images/Entities/PixelPeople"), 18, 32, "Entity_PixelPeople_"),
        //    Unpack((Texture2D)Resources.Load("Images/Entities/PowerGrass"), 16, 16, "Entity_PowerGrass_"),
        //    Unpack((Texture2D)Resources.Load("Images/Entities/PowerNom"), 16, 4, "Entity_PowerNom_"),
        //    Unpack((Texture2D)Resources.Load("Images/Entities/SnailNpc"), 32, 18, "Entity_SnailNpc_"),
        //    Unpack((Texture2D)Resources.Load("Images/Entities/SnailNpcColor"), 7, 52, "Entity_SnailNpcColor_"),
        //    Unpack((Texture2D)Resources.Load("Images/Entities/Spikey1"), 16, 16, "Entity_Spikey1_"),
        //    Unpack((Texture2D)Resources.Load("Images/Entities/TurtleNpc"), 32, 16, "Entity_TurtleNpc_"),
        //    
        //    Unpack((Texture2D)Resources.Load("Images/Items/Boomerang"), 32, 32, "Item_Boomerang_"),
        //    Unpack((Texture2D)Resources.Load("Images/Items/HeartContainer"), 32, 32, "Item_RainbowWave_"),
        //    Unpack((Texture2D)Resources.Load("Images/Items/HelixFragment"), 16, 16, "Item_HelixFragment_"),
        //    Unpack((Texture2D)Resources.Load("Images/Items/RainbowWave"), 32, 32, "Item_RainbowWave_"),
        //    
        //    Unpack((Texture2D)Resources.Load("Images/Particles/Bubble"), 8, 8, "Particle_Bubble_"),
        //    Unpack((Texture2D)Resources.Load("Images/Particles/Explosion"), 64, 64, "Particle_Explosion_"),
        //    Unpack((Texture2D)Resources.Load("Images/Particles/Splash"), 16, 16, "Particle_Splash_"),
        //    Unpack((Texture2D)Resources.Load("Images/Particles/Star"), 16, 16, "Particle_Star_"),
        //    
        //    Unpack((Texture2D)Resources.Load("Images/UI/AchievementPanel"), 128, 64, "UI_AchievementPanel_"),
        //    Unpack((Texture2D)Resources.Load("Images/UI/DebugKey"), 8, 8, "UI_DebugKeys_"),
        //    Unpack((Texture2D)Resources.Load("Images/UI/DialogueBox"), 368, 64, "UI_DialogueBox_"),
        //    Unpack((Texture2D)Resources.Load("Images/UI/DialogueIcon2"), 16, 8, "UI_DialogueIcon_"),
        //    Unpack((Texture2D)Resources.Load("Images/UI/DialoguePortrait"), 32, 32, "UI_DialoguePortraits_"),
        //    Unpack((Texture2D)Resources.Load("Images/UI/Heart"), 8, 8, "UI_Heart_"),
        //    Unpack((Texture2D)Resources.Load("Images/UI/Minimap"), 208, 176, "UI_Minimap_"),
        //    Unpack((Texture2D)Resources.Load("Images/UI/MinimapIcons"), 8, 8, "UI_MinimapIcons_"),
        //    Unpack((Texture2D)Resources.Load("Images/UI/MinimapMask"), 400, 240, "UI_MinimapMask_"),
        //    Unpack((Texture2D)Resources.Load("Images/UI/MinimapPanel"), 56, 40, "UI_MinimapPanel_"),
        //    Unpack((Texture2D)Resources.Load("Images/UI/WeaponIcons"), 8, 8, "UI_WeaponIcons_")
        //};
        List<Sprite[]> newLibrary = new List<Sprite[]>();
        for (int i = 0; i < referenceList.Length; i++)
        {
            Vector2 thisSize = GetSpriteSize(referenceList[i]);
            if (thisSize == Vector2.zero)
                thisSize = new Vector2(16, 16);
            newLibrary.Add(Unpack((Texture2D)Resources.Load("Images/" + referenceList[i]), (int)thisSize.x, (int)thisSize.y));
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

    public void BuildLibrary(string folderPath = null)
    {
        BuildDefaultLibrary();
        if (folderPath != null)
        {

        }
        GetNewTextWidths();
    }

    public void BuildAnimationLibrary(string dataPath = null)
    {
        BuildDefaultAnimLibrary();
        if (dataPath != null)
        {
            PlayState.LoadNewAnimationLibrary(dataPath);
        }
    }

    public void BuildSpriteSizeLibrary(string dataPath = null)
    {
        BuildDefaultSpriteSizeLibrary();
        if (dataPath != null)
        {
            PlayState.LoadNewSpriteSizeLibrary(dataPath);
        }
    }

    public void GetNewTextWidths()
    {
        List<int> newWidths = new List<int>();
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
}
