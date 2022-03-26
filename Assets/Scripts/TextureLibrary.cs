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

    public Sprite[] Unpack(Sprite texture, int sliceWidth, int sliceHeight, string name)
    {
        Texture2D newTexture = new Texture2D((int)texture.rect.width, (int)texture.rect.height);
        newTexture.SetPixels(texture.texture.GetPixels((int)texture.textureRect.x, (int)texture.textureRect.y,
            (int)texture.textureRect.width, (int)texture.textureRect.height));
        newTexture.Apply();
        return Unpack(newTexture, sliceWidth, sliceHeight, name);
    }

    public Sprite[] Unpack(Texture2D texture, int sliceWidth, int sliceHeight, string name)
    {
        //Debug.Log(texture);
        List<Sprite> unpackedArray = new List<Sprite>();
        int counter = 0;
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
        List<Sprite[]> newLibrary = new List<Sprite[]>();
        for (int i = 0; i < referenceList.Length; i++)
        {
            Vector2 thisSize = GetSpriteSize(referenceList[i]);
            if (thisSize == Vector2.zero)
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
