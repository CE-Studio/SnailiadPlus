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
    private float initialVelocity;
    public int damage;

    public SpriteRenderer sprite;
    public AnimationModule anim;
    public BoxCollider2D box;

    public GameObject player;
    public GameObject cam;

    private readonly Vector2 diagonalAim = new Vector2(Mathf.Cos(40 * Mathf.Deg2Rad), Mathf.Sin(40 * Mathf.Deg2Rad));
    
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        sprite.enabled = false;
        anim = GetComponent<AnimationModule>();
        box = GetComponent<BoxCollider2D>();
        box.enabled = false;

        player = GameObject.FindWithTag("Player");
        cam = GameObject.Find("View");

        string[] bulletTypes = new string[] { "peashooter", "boomerang", "rainbowWave" };
        string[] directions = new string[] { "NW", "N", "NE", "E", "SE", "S", "SW", "W" };
        for (int i = 0; i < bulletTypes.Length; i++)
        {
            for (int j = 0; j < directions.Length; j++)
                anim.Add("Bullet_" + bulletTypes[i] + "_" + directions[j]);
        }
    }

    void FixedUpdate()
    {
        if (PlayState.gameState != "Game")
            return;
        if (isActive)
        {
            lifeTimer += Time.fixedDeltaTime;
            switch (bulletType)
            {
                case 1:
                    break;
                case 2:
                    velocity -= initialVelocity * 1.5f * Time.fixedDeltaTime;
                    break;
                case 3:
                    velocity += initialVelocity * 18f * Time.fixedDeltaTime;
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
        if (lifeTimer > 3 || !(transform.position.x > cam.transform.position.x - 12.5f - (box.size.x * 0.5f) &&
                transform.position.x < cam.transform.position.x + 12.5f + (box.size.x * 0.5f) &&
                transform.position.y > cam.transform.position.y - 7.5f - (box.size.y * 0.5f) &&
                transform.position.y < cam.transform.position.y + 7.5f + (box.size.y * 0.5f)))
            Despawn();
        if (bulletType == 1 && PlayState.IsTileSolid(transform.position))
            Despawn(true);
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
            case 1:
                box.size = new Vector2(0.25f, 0.25f);
                velocity = 0.3855f;
                damage = 10;
                break;
            case 2:
                box.size = new Vector2(0.9f, 0.9f);
                velocity = 0.34375f;
                damage = 20;
                break;
            case 3:
                box.size = new Vector2(1.9f, 1.9f);
                velocity = 0.0625f;
                damage = 30;
                break;
        }
        direction = dir;
        initialVelocity = velocity;
        PlayAnim();
    }

    void PlayAnim()
    {
        string animToPlay = "Bullet_";
        animToPlay += bulletType switch
        {
            1 => "peashooter_",
            2 => "boomerang_",
            3 => "rainbowWave_",
            _ => "rainbowWave_",
        };
        animToPlay += direction switch
        {
            1 => "N",
            2 => "NE",
            3 => "W",
            4 => "E",
            5 => "SW",
            6 => "S",
            7 => "SE",
            _ => "NW"
        };
        anim.Play(animToPlay);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerCollide") && bulletType == 1)
        {
            Despawn(true);
        }
    }

    public void Despawn(bool loudly = false)
    {
        if (isActive)
        {
            if (loudly)
            {
                PlayState.PlaySound("ShotHit");
                switch (bulletType)
                {
                    case 1:
                        PlayState.RequestParticle(new Vector2(Random.Range(transform.position.x - 0.25f, transform.position.x + 0.25f),
                            Random.Range(transform.position.y - 0.25f, transform.position.y + 0.25f)), "explosion", new float[] { 1 });
                        break;
                    case 2:
                        PlayState.RequestParticle(new Vector2(Random.Range(transform.position.x - 0.5f, transform.position.x + 0.5f),
                            Random.Range(transform.position.y - 0.5f, transform.position.y + 0.5f)), "explosion", new float[] { 5 });
                        break;
                    case 3:
                        PlayState.RequestParticle(new Vector2(Random.Range(transform.position.x - 0.5f, transform.position.x + 0.5f),
                            Random.Range(transform.position.y - 0.5f, transform.position.y + 0.5f)), "explosion", new float[] { 7 });
                        break;
                    case 4:
                        PlayState.RequestParticle(new Vector2(Random.Range(transform.position.x - 0.5f, transform.position.x + 0.5f),
                            Random.Range(transform.position.y - 0.5f, transform.position.y + 0.5f)), "explosion", new float[] { 5 });
                        PlayState.RequestParticle(new Vector2(Random.Range(transform.position.x - 0.5f, transform.position.x + 0.5f),
                            Random.Range(transform.position.y - 0.5f, transform.position.y + 0.5f)), "explosion", new float[] { 5 });
                        PlayState.RequestParticle(new Vector2(Random.Range(transform.position.x - 0.5f, transform.position.x + 0.5f),
                            Random.Range(transform.position.y - 0.5f, transform.position.y + 0.5f)), "explosion", new float[] { 5 });
                        PlayState.RequestParticle(new Vector2(Random.Range(transform.position.x - 0.5f, transform.position.x + 0.5f),
                            Random.Range(transform.position.y - 0.5f, transform.position.y + 0.5f)), "explosion", new float[] { 5 });
                        break;
                    case 5:
                        PlayState.RequestParticle(new Vector2(Random.Range(transform.position.x - 0.5f, transform.position.x + 0.5f),
                            Random.Range(transform.position.y - 0.5f, transform.position.y + 0.5f)), "explosion", new float[] { 6 });
                        break;
                    case 6:
                        PlayState.RequestParticle(new Vector2(Random.Range(transform.position.x - 0.5f, transform.position.x + 0.5f),
                            Random.Range(transform.position.y - 0.5f, transform.position.y + 0.5f)), "explosion", new float[] { 8 });
                        break;
                }
            }
            isActive = false;
            sprite.enabled = false;
            box.enabled = false;
            lifeTimer = 0;
            transform.position = Vector2.zero;
        }
    }

    private void MoveNW()
    {
        transform.position = new Vector2(transform.position.x + (-diagonalAim.x * velocity), transform.position.y + (diagonalAim.y * velocity));
    }

    private void MoveN()
    {
        transform.position = new Vector2(transform.position.x, transform.position.y + (1 * velocity));
    }

    private void MoveNE()
    {
        transform.position = new Vector2(transform.position.x + (diagonalAim.x * velocity), transform.position.y + (diagonalAim.y * velocity));
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
        transform.position = new Vector2(transform.position.x + (-diagonalAim.x * velocity), transform.position.y + (-diagonalAim.y * velocity));
    }

    private void MoveS()
    {
        transform.position = new Vector2(transform.position.x, transform.position.y + (-1 * velocity));
    }

    private void MoveSE()
    {
        transform.position = new Vector2(transform.position.x + (diagonalAim.x * velocity), transform.position.y + (-diagonalAim.y * velocity));
    }
}
