using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour, IRoomObject
{
    private const int MAX_TYPES = 2;
    private const int MAX_SIZES = 4;

    public int size = 2;
    public int type = 0;
    public float speed;
    private Vector2 lastPos;
    public string pathName;

    public Transform platObj;
    public BoxCollider2D box;
    public SpriteRenderer sprite;
    public AnimationModule anim;
    public Animator pathAnim;
    public Player player;

    public static readonly string myType = "Platform";

    public string objType { get { return myType; } }

    public Dictionary<string, object> save()
    {
        Dictionary<string, object> content = new Dictionary<string, object>
        {
            ["size"] = size,
            ["type"] = type,
            ["speed"] = speed,
            ["pathName"] = pathName
        };
        return content;
    }
    
    public Dictionary<string, object> resave()
    {
        return null;
    }

    public void load(Dictionary<string, object> content)
    {
        size = (int)content["size"];
        type = (int)content["type"];
        speed = (float)content["speed"];
        pathName = (string)content["pathName"];
        Spawn();
    }

    void Awake()
    {
        if (PlayState.gameState == PlayState.GameState.game)
        {
            platObj = transform.GetChild(0);
            box = platObj.GetComponent<BoxCollider2D>();
            sprite = platObj.GetComponent<SpriteRenderer>();
            anim = platObj.GetComponent<AnimationModule>();
            pathAnim = platObj.GetComponent<Animator>();
            player = GameObject.FindWithTag("Player").GetComponent<Player>();

            for (int i = 1; i <= MAX_TYPES; i++)
            {
                for (int j = 1; j <= MAX_SIZES; j++)
                {
                    anim.Add("Object_platform" + i + "_" + j + "_idle");
                    anim.Add("Object_platform" + i + "_" + j + "_up");
                    anim.Add("Object_platform" + i + "_" + j + "_down");
                    anim.Add("Object_platform" + i + "_" + j + "_left");
                    anim.Add("Object_platform" + i + "_" + j + "_right");
                }
            }
        }
    }

    void Spawn()
    {
        box.size = new Vector2(size, 1);
        PlayAnim();
        if (pathName != "None")
        {
            pathAnim.speed = speed;
            pathAnim.Play("Base Layer." + pathName);
        }
    }

    void Update()
    {
        if (PlayState.gameState != PlayState.GameState.game)
        {
            pathAnim.speed = 0;
            return;
        }
        pathAnim.speed = speed;

        if ((Vector2)transform.position == lastPos)
            PlayAnim();
        else
        {
            if (Mathf.Abs(transform.position.y) > Mathf.Abs(transform.position.x))
            {
                if (transform.position.y > 0)
                    PlayAnim("up");
                else
                    PlayAnim("down");
            }
            else
            {
                if (transform.position.x > 0)
                    PlayAnim("right");
                else
                    PlayAnim("left");
            }
        }

        if (player.currentCollision == box)
        {
            player.transform.position += transform.position - (Vector3)lastPos;
            PlayState.playerScript.CheckIntersectionAndCorrect();
        }

        lastPos = transform.position;
    }

    private void PlayAnim(string dir = "idle")
    {
        string animName = "Object_platform" + (type + 1) + "_" + size + "_" + dir;
        if (anim.currentAnimName != animName)
            anim.Play(animName);
    }

    private void EjectPlayerFromBox()
    {
        Vector2 playerSize = PlayState.playerScript.box.size * 0.5f;
        Vector2 radiiFromCenter = new Vector2(box.size.x + playerSize.x, box.size.y + playerSize.y) * 0.5f;
        Vector2 playerPos = PlayState.player.transform.position;
        if (playerPos.x > transform.position.x - radiiFromCenter.x && playerPos.x < transform.position.x)
        {
            PlayState.player.transform.position = new Vector2(transform.position.x - radiiFromCenter.x - PlayState.FRAC_128, playerPos.y);
            if (PlayState.playerScript.gravityDir == Player.DIR_WALL_RIGHT)
                PlayState.playerScript.grounded = true;
        }
        if (playerPos.x < transform.position.x + radiiFromCenter.x && playerPos.x > transform.position.x)
        {
            PlayState.player.transform.position = new Vector2(transform.position.x + radiiFromCenter.x + PlayState.FRAC_128, playerPos.y);
            if (PlayState.playerScript.gravityDir == Player.DIR_WALL_LEFT)
                PlayState.playerScript.grounded = true;
        }
        if (playerPos.y > transform.position.y - radiiFromCenter.y && playerPos.y < transform.position.y)
        {
            PlayState.player.transform.position = new Vector2(playerPos.x, transform.position.y - radiiFromCenter.y - PlayState.FRAC_128);
            if (PlayState.playerScript.gravityDir == Player.DIR_CEILING)
                PlayState.playerScript.grounded = true;
        }
        if (playerPos.y < transform.position.y + radiiFromCenter.y && playerPos.y > transform.position.y)
        {
            PlayState.player.transform.position = new Vector2(playerPos.x, transform.position.y + radiiFromCenter.y + PlayState.FRAC_128);
            if (PlayState.playerScript.gravityDir == Player.DIR_FLOOR)
                PlayState.playerScript.grounded = true;
        }
    }
}
