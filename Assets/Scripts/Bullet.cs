using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public string bulletType;
    public int bulletTypeInt;
    public int direction;
    public bool isActive;
    private float lifeTimer;
    private float velocity;
    public float damage;

    public SpriteRenderer sprite;
    public Animator anim;
    public BoxCollider2D box;

    public GameObject player;
    
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        sprite.enabled = false;
        anim = GetComponent<Animator>();
        box = GetComponent<BoxCollider2D>();
        box.enabled = false;

        player = GameObject.FindWithTag("Player");
    }

    void FixedUpdate()
    {
        anim.speed = 1;
        if (PlayState.gameState == "Game")
        {
            if (isActive)
            {
                lifeTimer += Time.fixedDeltaTime;
                switch (bulletType)
                {
                    case "Rainbow Wave":
                        velocity = Mathf.Clamp(velocity + 0.04f, 0, 0.75f);
                        break;
                    default:
                        velocity = Mathf.Clamp(velocity + 0.04f, 0, 0.75f);
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
        }
        else
        {
            anim.speed = 0;
        }
    }

    public void Shoot(string type, int dir)
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
            case "Rainbow Wave":
                bulletTypeInt = 2;
                box.size = new Vector2(1.9f, 1.9f);
                break;
        }
        direction = dir;
        switch (bulletType)
        {
            case "Rainbow Wave":
                velocity = 0;
                break;
        }
        PlayAnim();
    }

    void PlayAnim()
    {
        string animToPlay = "";
        switch (bulletType)
        {
            case "Rainbow Wave":
                animToPlay += "Rainbow Wave ";
                break;
            default:
                animToPlay += "Rainbow Wave ";
                break;
        }
        switch (direction)
        {
            case 0:
                animToPlay += "diagonal";
                sprite.flipX = true;
                sprite.flipY = false;
                break;
            case 1:
                animToPlay += "vertical";
                sprite.flipX = false;
                sprite.flipY = false;
                break;
            case 2:
                animToPlay += "diagonal";
                sprite.flipX = false;
                sprite.flipY = false;
                break;
            case 3:
                animToPlay += "horizontal";
                sprite.flipX = true;
                sprite.flipY = false;
                break;
            case 4:
                animToPlay += "horizontal";
                sprite.flipX = false;
                sprite.flipY = false;
                break;
            case 5:
                animToPlay += "diagonal";
                sprite.flipX = true;
                sprite.flipY = true;
                break;
            case 6:
                animToPlay += "vertical";
                sprite.flipX = false;
                sprite.flipY = true;
                break;
            case 7:
                animToPlay += "diagonal";
                sprite.flipX = false;
                sprite.flipY = true;
                break;
        }
        float startTime = 0;
        switch (bulletType)
        {
            case "Rainbow Wave":
                startTime = Random.Range(1, 7) * 0.16f;
                break;
        }
        anim.Play(animToPlay, 0, startTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.CompareTag("PlayerCollide") || collision.CompareTag("EnemyCollide")) && bulletType == "Peashooter")
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
        transform.position = new Vector2(transform.position.x + (-0.75f * velocity), transform.position.y + (0.7f * velocity));
    }

    private void MoveN()
    {
        transform.position = new Vector2(transform.position.x, transform.position.y + (1 * velocity));
    }

    private void MoveNE()
    {
        transform.position = new Vector2(transform.position.x + (0.75f * velocity), transform.position.y + (0.7f * velocity));
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
        transform.position = new Vector2(transform.position.x + (-0.75f * velocity), transform.position.y + (-0.7f * velocity));
    }

    private void MoveS()
    {
        transform.position = new Vector2(transform.position.x, transform.position.y + (-1 * velocity));
    }

    private void MoveSE()
    {
        transform.position = new Vector2(transform.position.x + (0.75f * velocity), transform.position.y + (-0.7f * velocity));
    }
}
