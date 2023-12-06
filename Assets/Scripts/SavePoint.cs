using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint:MonoBehaviour, IRoomObject
{
    public bool hasBeenActivated = false;
    public int surface = (int)Player.Dirs.Floor;
    public bool exclusiveSurface = true;
    private string surfaceString;

    public AnimationModule anim;
    public BoxCollider2D box;

    public static readonly string myType = "Save Point";

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
        content["surface"] = surface;
        content["exclusiveSurface"] = exclusiveSurface;
        return content;
    }

    public void load(Dictionary<string, object> content)
    {
        surface = (int)content["surface"];
        exclusiveSurface = (bool)content["exclusiveSurface"];
        Spawn();
    }

    public void Spawn()
    {
        if (exclusiveSurface && surface != (int)PlayState.playerScript.defaultGravityDir)
        {
            Destroy(gameObject);
            return;
        }

        surfaceString = surface switch
        {
            (int)Player.Dirs.WallL => "wallL",
            (int)Player.Dirs.WallR => "wallR",
            (int)Player.Dirs.Ceiling => "ceiling",
            _ => "floor"
        };

        box = GetComponent<BoxCollider2D>();
        if ((Player.Dirs)surface == Player.Dirs.WallL || (Player.Dirs)surface == Player.Dirs.WallR)
            box.size = new(box.size.y, box.size.x);

        anim = GetComponent<AnimationModule>();
        anim.Add("Save_inactive_" + surfaceString);
        anim.Add("Save_active_" + surfaceString);
        anim.Add("Save_last_" + surfaceString);

        if (PlayState.currentProfile.saveCoords == new Vector2(transform.position.x, transform.position.y + 0.5f))
            anim.Play("Save_last_" + surfaceString);
        else
            anim.Play("Save_inactive_" + surfaceString);

        PlayState.globalFunctions.CreateLightMask(16, transform);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!hasBeenActivated)
            {
                hasBeenActivated = true;
                PlayState.PlaySound("Save");
                anim.Play("Save_active_" + surfaceString);
                PlayState.globalFunctions.FlashHUDText(GlobalFunctions.TextTypes.save);
                PlayState.currentProfile.saveCoords = new Vector2(transform.position.x, transform.position.y + 0.5f);
                PlayState.positionOfLastSave = PlayState.positionOfLastRoom;
                PlayState.WriteSave(PlayState.currentProfileNumber, true);
            }
        }
    }
}
