using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint:MonoBehaviour, IRoomObject
{
    public bool hasBeenActivated = false;

    public AnimationModule anim;

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
        return content;
    }

    public void load(Dictionary<string, object> content)
    {
        Spawn();
    }

    public void Spawn()
    {
        anim = GetComponent<AnimationModule>();
        anim.Add("Save_inactive");
        anim.Add("Save_active");
        anim.Add("Save_last");

        if (PlayState.currentProfile.saveCoords == new Vector2(transform.position.x, transform.position.y + 0.5f))
            anim.Play("Save_last");
        else
            anim.Play("Save_inactive");

        PlayState.globalFunctions.CreateLightMask(16, transform.position).transform.parent = transform;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!hasBeenActivated)
            {
                hasBeenActivated = true;
                PlayState.PlaySound("Save");
                anim.Play("Save_active");
                PlayState.globalFunctions.FlashHUDText(GlobalFunctions.TextTypes.save);
                PlayState.currentProfile.saveCoords = new Vector2(transform.position.x, transform.position.y + 0.5f);
                PlayState.positionOfLastSave = PlayState.positionOfLastRoom;
                PlayState.WriteSave(PlayState.currentProfileNumber, true);
            }
        }
    }
}
