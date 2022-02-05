using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerGrass : MonoBehaviour
{
    public int totalBites = 12;
    public int healthPerBite = 3;
    public float biteTimeout = 0.17f;

    public int bitesRemaining;
    public bool active = false;
    public float timer = 0;

    public SpriteRenderer sprite;
    public BoxCollider2D box;
    public AudioSource sfx;
    public AudioClip bite;

    public Sprite nom0;
    public Sprite nom1;
    public Sprite nom2;
    public Sprite nom3;

    public GameObject player;

    public void Start()
    {
        bitesRemaining = totalBites;
        sprite = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        sfx = GetComponent<AudioSource>();
        bite = (AudioClip)Resources.Load("Sounds/Sfx/EatPowerGrass");

        Physics2D.IgnoreCollision(transform.parent.GetComponent<Collider2D>(), GetComponent<Collider2D>());

        player = GameObject.Find("Player");
    }

    void Update()
    {
        if (PlayState.gameState == "Game")
        {
            sfx.volume = PlayState.gameOptions[0] * 0.1f;
            timer = Mathf.Clamp(timer - Time.deltaTime, 0, Mathf.Infinity);
        }
        //if (sprite.enabled && timer == 0)
        //{
        //    box.enabled = true;
        //    player.transform.position = new Vector2(player.transform.position.x, player.transform.position.y + 0.0001f);
        //    // Because otherwise Snaily stops eating after three or four bites unless you move
        //}
    }

    public void ToggleActive(bool state)
    {
        active = state;
    }

    public void Spawn()
    {
        bitesRemaining = totalBites;
        sprite.enabled = true;
        box.enabled = true;
        sfx.enabled = true;
        timer = 0;
        if (transform.childCount > 0)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        ToggleActive(true);
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (timer == 0)
            {
                sfx.PlayOneShot(bite);
                bitesRemaining--;
                if (bitesRemaining == 0)
                {
                    sprite.enabled = false;
                    box.enabled = false;
                }
                else
                    timer = biteTimeout;
                collision.GetComponent<Player>().health = Mathf.Clamp(collision.GetComponent<Player>().health + healthPerBite, 0, collision.GetComponent<Player>().maxHealth);
                collision.GetComponent<Player>().UpdateHearts();
                StartCoroutine(nameof(NomText));
            }
        }
    }

    public IEnumerator NomText()
    {
        GameObject Nom = new GameObject();
        Nom.transform.parent = transform;
        Nom.transform.position = new Vector2(transform.position.x, transform.position.y + 0.25f);
        Nom.AddComponent<SpriteRenderer>();
        Nom.GetComponent<SpriteRenderer>().sortingOrder = -49;
        float nomTimer = 0;
        while (nomTimer < 0.8f)
        {
            if (PlayState.gameState == "Game")
            {
                nomTimer += Time.fixedDeltaTime;
                Nom.transform.position = new Vector2(transform.position.x, Mathf.Lerp(transform.position.y + 0.25f, transform.position.y + 1.5f, nomTimer * 1.2f));
                if (nomTimer < 0.2f)
                {
                    Nom.GetComponent<SpriteRenderer>().sprite = nom0;
                }
                else if (nomTimer < 0.4f)
                {
                    Nom.GetComponent<SpriteRenderer>().sprite = nom1;
                }
                else if (nomTimer < 0.6f)
                {
                    Nom.GetComponent<SpriteRenderer>().sprite = nom2;
                }
                else
                {
                    Nom.GetComponent<SpriteRenderer>().sprite = nom3;
                }
            }
            if (!active)
            {
                break;
            }
            yield return new WaitForFixedUpdate();
        }
        Destroy(Nom);
    }
}
