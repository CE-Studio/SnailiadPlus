using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableBlock : MonoBehaviour
{
    public int requiredWeapon;
    public bool isSilent;
    public SpriteRenderer sprite;
    public BoxCollider2D box;
    public AudioSource sfx;
    public AudioClip expl1;
    public AudioClip expl2;
    public AudioClip expl3;
    public AudioClip expl4;

    public Sprite coverSprite;
    public TileBase originalTile;
    public Sprite blankSprite;
    public Tilemap homeMap;
    
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        sfx = GetComponent<AudioSource>();
        sprite.sprite = coverSprite;

        expl1 = (AudioClip)Resources.Load("Sounds/Sfx/Explode1");
        expl2 = (AudioClip)Resources.Load("Sounds/Sfx/Explode2");
        expl3 = (AudioClip)Resources.Load("Sounds/Sfx/Explode3");
        expl4 = (AudioClip)Resources.Load("Sounds/Sfx/Explode4");
    }

    public void Instantiate(int type, bool silent)
    {
        requiredWeapon = type;
        isSilent = silent;
    }

    public void Despawn()
    {
        homeMap.SetTile(new Vector3Int((int)Mathf.Round(transform.position.x - 0.5f), (int)Mathf.Round(transform.position.y - 0.5f), 0), originalTile);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
        {
            if (collision.GetComponent<Bullet>().bulletTypeInt >= requiredWeapon)
            {
                box.enabled = false;
                sprite.sprite = blankSprite;
                if (!PlayState.explodePlayedThisFrame)
                {
                    int i = Random.Range(1, 4);
                    switch (i)
                    {
                        case 1:
                            sfx.PlayOneShot(expl1);
                            break;
                        case 2:
                            sfx.PlayOneShot(expl2);
                            break;
                        case 3:
                            sfx.PlayOneShot(expl3);
                            break;
                        case 4:
                            sfx.PlayOneShot(expl4);
                            break;
                    }
                    PlayState.explodePlayedThisFrame = true;
                }
            }
        }
    }
}
