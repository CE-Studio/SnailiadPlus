using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int bulletType;
    public int direction;
    public bool isActive;
    private float lifeTimer;
    private float velocity;
    public int damage;

    public SpriteRenderer sprite;
    public AnimationModule anim;
    public BoxCollider2D box;

    public GameObject player;
    public GameObject cam;

    private readonly float diagOffsetX = 0.75f;
    private readonly float diagOffsetY = 0.65f;
    
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        sprite.enabled = false;
        anim = GetComponent<AnimationModule>();
        box = GetComponent<BoxCollider2D>();
        box.enabled = false;

        player = GameObject.FindWithTag("Player");
        cam = GameObject.Find("View");

        string[] bulletTypes = new string[] { "boomerang", "rainbowWave" };
        string[] directions = new string[] { "NW", "N", "NE", "E", "SE", "S", "SW", "W" };
        for (int i = 0; i < bulletTypes.Length; i++)
        {
            for (int j = 0; j < directions.Length; j++)
                anim.Add("Bullet_" + bulletTypes[i] + "_" + directions[j]);
        }
    }

    void FixedUpdate()
    {
        if (PlayState.gameState == "Game")
        {
            if (isActive)
            {
                lifeTimer += Time.fixedDeltaTime;
                switch (bulletType)
                {
                    case 2:
                        velocity -= 0.0125f;
                        break;
                    case 3:
                        velocity += 0.03f;
                        break;
                    default:
                        velocity += 0.03f;
                        break;
                }
                switch (direction)
                {
                    case 0:
                        MoveNW();
                        break;
                    case 1:
                        MoveN();
                        break;
                    case 2:
                        MoveNE();
                        break;
                    case 3:
                        MoveW();
                        break;
                    case 4:
                        MoveE();
                        break;
                    case 5:
                        MoveSW();
                        break;
                    case 6:
                        MoveS();
                        break;
                    case 7:
                        MoveSE();
                        break;
                }
            }
            if (lifeTimer > 3)
            {
                Despawn();
            }
            else
            {
                if (transform.position.x > cam.transform.position.x - 12.5f - (box.size.x * 0.5f) &&
                    transform.position.x < cam.transform.position.x + 12.5f + (box.size.x * 0.5f) &&
                    transform.position.y > cam.transform.position.y - 7.5f - (box.size.y * 0.5f) &&
                    transform.position.y < cam.transform.position.y + 7.5f + (box.size.y * 0.5f))
                    box.enabled = true;
                else
                    box.enabled = false;
            }
        }
    }

    public void Shoot(int type, int dir)
    {
        isActive = true;
        sprite.enabled = true;
        box.enabled = true;
        transform.position = new Vector2(
            player.transform.position.x + player.GetComponent<Player>().box.offset.x,
            player.transform.position.y + player.GetComponent<Player>().box.offset.y);
        switch (dir)
        {
            case 0:
                transform.position = new Vector2(transform.position.x - 0.4f, transform.position.y + 0.2f);
                break;
            case 1:
                transform.position = new Vector2(transform.position.x, transform.position.y + 0.2f);
                break;
            case 2:
                transform.position = new Vector2(transform.position.x + 0.4f, transform.position.y + 0.2f);
                break;
            case 3:
                transform.position = new Vector2(transform.position.x - 0.4f, transform.position.y);
                break;
            case 4:
                transform.position = new Vector2(transform.position.x + 0.4f, transform.position.y);
                break;
            case 5:
                transform.position = new Vector2(transform.position.x - 0.4f, transform.position.y - 0.2f);
                break;
            case 6:
                transform.position = new Vector2(transform.position.x, transform.position.y - 0.2f);
                break;
            case 7:
                transform.position = new Vector2(transform.position.x + 0.4f, transform.position.y - 0.2f);
                break;
        }
        bulletType = type;
        switch (type)
        {
            case 2:
                box.size = new Vector2(0.9f, 0.9f);
                velocity = 0.415f;
                damage = 20;
                break;
            case 3:
                box.size = new Vector2(1.9f, 1.9f);
                velocity = 0.025f;
                damage = 30;
                break;
        }
        direction = dir;
        PlayAnim();
    }

    void PlayAnim()
    {
        string animToPlay = "Bullet_";
        switch (bulletType)
        {
            case 2:
                animToPlay += "boomerang_";
                break;
            case 3:
                animToPlay += "rainbowWave_";
                break;
            default:
                animToPlay += "rainbowWave_";
                break;
        }
        switch (direction)
        {
            case 0:
                animToPlay += "NW";
                break;
            case 1:
                animToPlay += "N";
                break;
            case 2:
                animToPlay += "NE";
                break;
            case 3:
                animToPlay += "W";
                break;
            case 4:
                animToPlay += "E";
                break;
            case 5:
                animToPlay += "SW";
                break;
            case 6:
                animToPlay += "S";
                break;
            case 7:
                animToPlay += "SE";
                break;
        }
        anim.Play(animToPlay);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.CompareTag("PlayerCollide") || collision.CompareTag("EnemyCollide")) && bulletType == 1)
        {
            Despawn();
        }
    }

    public void Despawn()
    {
        isActive = false;
        sprite.enabled = false;
        box.enabled = false;
        lifeTimer = 0;
        transform.position = Vector2.zero;
    }

    private void MoveNW()
    {
        transform.position = new Vector2(transform.position.x + (-diagOffsetX * velocity), transform.position.y + (diagOffsetY * velocity));
    }

    private void MoveN()
    {
        transform.position = new Vector2(transform.position.x, transform.position.y + (1 * velocity));
    }

    private void MoveNE()
    {
        transform.position = new Vector2(transform.position.x + (diagOffsetX * velocity), transform.position.y + (diagOffsetY * velocity));
    }

    private void MoveW()
    {
        transform.position = new Vector2(transform.position.x + (-1 * velocity), transform.position.y);
    }

    private void MoveE()
    {
        transform.position = new Vector2(transform.position.x + (1 * velocity), transform.position.y);
    }

    private void MoveSW()
    {
        transform.position = new Vector2(transform.position.x + (-diagOffsetX * velocity), transform.position.y + (-diagOffsetY * velocity));
    }

    private void MoveS()
    {
        transform.position = new Vector2(transform.position.x, transform.position.y + (-1 * velocity));
    }

    private void MoveSE()
    {
        transform.position = new Vector2(transform.position.x + (diagOffsetX * velocity), transform.position.y + (-diagOffsetY * velocity));
    }
}
