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

        if (PlayState.currentProfile.saveCoords == (Vector2)transform.position + DirToVector())
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
                SpawnParticles();
                PlayState.globalFunctions.FlashHUDText(GlobalFunctions.TextTypes.save);
                PlayState.currentProfile.saveCoords = (Vector2)transform.position + DirToVector();
                PlayState.positionOfLastSave = PlayState.positionOfLastRoom;
                PlayState.WriteSave(PlayState.currentProfileNumber, true);
            }
        }
    }

    private Vector2 DirToVector()
    {
        return surface switch
        {
            (int)Player.Dirs.Floor => Vector2.up,
            (int)Player.Dirs.WallL => Vector2.right,
            (int)Player.Dirs.WallR => Vector2.left,
            _ => Vector2.down
        } * 0.5f;
    }

    private void SpawnParticles()
    {
        Vector2 aimDir = DirToVector();
        bool horizontal = aimDir.y == 0;
        int[] colors = new int[] { 309, 304, 206, 12 };
        float topSpeed = 5f;
        for (int i = 0; i < 32; i++)
        {
            if (horizontal)
            {
                Vector2 spawnPos = new(transform.position.x + (aimDir.x * -0.5f), transform.position.y + ((Random.value * 2f) - 1f));
                int thisColor = colors[Mathf.FloorToInt(Random.value * colors.Length)];
                PlayState.RequestParticle(spawnPos, "tintedSparkle", new float[] { thisColor, Random.value * topSpeed * Mathf.Sign(aimDir.x), 0f });
            }
            else
            {
                Vector2 spawnPos = new(transform.position.x + ((Random.value * 2f) - 1f), transform.position.y + (aimDir.y * -0.5f));
                int thisColor = colors[Mathf.FloorToInt(Random.value * colors.Length)];
                PlayState.RequestParticle(spawnPos, "tintedSparkle", new float[] { thisColor, 0f, Random.value * topSpeed * Mathf.Sign(aimDir.y) });
            }
        }
    }
}
