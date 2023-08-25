using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionTile : MonoBehaviour
{
    public PlayState.EDirsSurface dir = PlayState.EDirsSurface.Floor;
    
    public enum Types
    {
        QuarterStep,
        HalfStep,
        ThreeQuarterStep,
        FullStep,
        Slab,
        Corner,
        MidSlab,
        DualFullStep
    };
    private Types type;

    private BoxCollider2D box;

    void Awake()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        box = GetComponent<BoxCollider2D>();
    }

    public void Spawn(PlayState.EDirsSurface newDir, Types newType)
    {
        dir = newDir;
        type = newType;
        switch (type)
        {
            default:
                break;
            case Types.QuarterStep:
                box.size = new Vector2(1, 0.25f);
                box.offset = new Vector2(0, -0.38f);
                break;
            case Types.HalfStep:
                box.size = new Vector2(1, 0.5f);
                box.offset = new Vector2(0, -0.25f);
                break;
            case Types.ThreeQuarterStep:
                box.size = new Vector2(1, 0.75f);
                box.offset = new Vector2(0, -0.13f);
                break;
            case Types.FullStep:
                box.size = new Vector2(1, 1);
                box.offset = new Vector2(0, 0);
                break;
        }
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            BoxCollider2D playerBox = collision.GetComponent<BoxCollider2D>();
            collision.transform.position = new Vector2(collision.transform.position.x, transform.position.y + box.offset.y + (box.size.y * 0.5f) +
                (playerBox.size.y * 0.5f) - playerBox.offset.y + PlayState.FRAC_32);
            Player playerScript = collision.GetComponent<Player>();
            if (playerScript.gravityDir == Player.Dirs.Floor)
                playerScript.grounded = true;
            else if (playerScript.gravityDir != Player.Dirs.Ceiling && Control.AxisY() == -1)
                collision.transform.position += PlayState.FRAC_32 * Vector3.up;
        }
    }
}
