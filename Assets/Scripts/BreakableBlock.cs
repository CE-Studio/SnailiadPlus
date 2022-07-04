using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    public int requiredWeapon;
    public bool isSilent;
    private bool hasBeenHit;
    public BoxCollider2D box;
    public SpriteRenderer[] sprites = new SpriteRenderer[] { };

    GameObject fg1Sprite;
    GameObject fg2Sprite;
    
    void Awake()
    {
        fg1Sprite = transform.GetChild(0).gameObject;
        fg2Sprite = transform.GetChild(1).gameObject;

        box = GetComponent<BoxCollider2D>();

        sprites = new SpriteRenderer[] { GetComponent<SpriteRenderer>(), fg1Sprite.GetComponent<SpriteRenderer>(), fg2Sprite.GetComponent<SpriteRenderer>() };
    }

    private void Update()
    {
        fg1Sprite.transform.localPosition = PlayState.fg1Layer.transform.position;
        fg2Sprite.transform.localPosition = PlayState.fg2Layer.transform.position;
    }

    public void Instantiate(PlayState.Breakable data)
    {
        transform.position = data.pos;
        requiredWeapon = data.weaponLevel;
        isSilent = data.isSilent;
        for (int i = 0; i < data.tiles.Length; i++)
            sprites[i].sprite = data.tiles[i] == -1 ? PlayState.BlankTexture() : PlayState.GetSprite("Tilesheet", data.tiles[i]);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet") && PlayState.OnScreen(transform.position, box))
        {
            if (collision.GetComponent<Bullet>().bulletType >= requiredWeapon)
            {
                if (!PlayState.explodePlayedThisFrame)
                {
                    PlayState.PlaySound("Explode" + Random.Range(1, 5));
                    PlayState.explodePlayedThisFrame = true;
                }
                for (int i = 0; i < 2; i++)
                    PlayState.RequestParticle(new Vector2(transform.position.x + Random.Range(-1f, 1f), transform.position.y + Random.Range(-1f, 1f)), "explosion", new float[] { 2 });
                Destroy(gameObject);
            }
            else
            {
                if (collision.GetComponent<Bullet>().bulletType != 1)
                {
                    if (!PlayState.armorPingPlayedThisFrame && !isSilent)
                    {
                        PlayState.PlaySound("Ping");
                        PlayState.armorPingPlayedThisFrame = true;
                    }
                    if ((PlayState.gameOptions[12] == 1 && !isSilent) || (PlayState.gameOptions[12] == 2 && isSilent))
                    {
                        if (!hasBeenHit)
                        {
                            foreach (SpriteRenderer sprite in sprites)
                            {
                                if (sprite.sprite != PlayState.BlankTexture())
                                    sprite.sprite = PlayState.GetSprite("Entities/BreakableIcons", requiredWeapon - 1);
                            }
                        }
                    }
                }
                else
                    collision.GetComponent<Bullet>().Despawn(PlayState.OnScreen(collision.transform.position, collision.GetComponent<BoxCollider2D>()));
            }
            hasBeenHit = true;
        }
    }
}
