using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtraPlatform : MonoBehaviour, IRoomObject
{
    public bool[] appearToChars = new bool[6];
    public int type;

    public static readonly string myType = "Extra Platform";

    public string objType
    {
        get
        {
            return myType;
        }
    }

    public Dictionary<string, object> resave()
    {
        return null;
    }

    public Dictionary<string, object> save()
    {
        Dictionary<string, object> content = new();
        content["appearToChars"] = appearToChars.Clone();
        content["type"] = type;
        return content;
    }

    public void load(Dictionary<string, object> content)
    {
        appearToChars = (bool[])((bool[])content["appearToChars"]).Clone();
        type = (int)content["type"];
        Spawn();
    }

    void Spawn()
    {
        if (!appearToChars[PlayState.mainMenu.CharacterNameToID(PlayState.currentProfile.character)])
            Destroy(gameObject);
        else
            GetComponent<AnimationModule>().AddAndPlay("Object_extraPlatform_" + type.ToString());
    }
}
