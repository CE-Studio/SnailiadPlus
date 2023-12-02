using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    public int type;
    public bool isSilent;
    private bool hasBeenHit;
    public BoxCollider2D box;
    public SpriteRenderer[] sprites = new SpriteRenderer[] { };

    GameObject fg1Sprite;
    GameObject fg2Sprite;

    private Vector2 worldPos;

    private readonly List<List<int>> bulletsThatBreakMe = new()
    {
        new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 },      // Peashooter
        new List<int> { 2, 3, 4, 5, 6, 7, 8 },                // Boomerang
        new List<int> { 3, 4, 5, 6, 7, 8 },                   // Rainbow Wave
        new List<int> { 4, 5, 6, 8 }                          // Devastator
    };
    private readonly List<List<int>> bulletsIShouldIgnore = new()
    {
        new List<int> { },
        new List<int> { 1 },
        new List<int> { 1 },
        new List<int> { 1 }
    };
    
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
        for (int i = 0; i < sprites.Length; i++)
            sprites[i].color = PlayState.entityColor;
    }

    public void Instantiate(PlayState.Breakable data, bool isFinalBossTile = false)
    {
        transform.position = data.pos;
        type = data.blockType;
        isSilent = data.isSilent;
        worldPos = new Vector2(Mathf.Floor(transform.position.x), Mathf.Floor(transform.position.y));
        for (int i = 0; i < data.tiles.Length; i++)
            sprites[i].sprite = data.tiles[i] == -1 ? PlayState.BlankTexture() : PlayState.GetSprite("Tilesheet", data.tiles[i]);
        if (data.tiles[0] == -1)
            gameObject.layer = 10;
        else
            PlayState.breakablePositions.Add(worldPos);
        if (isFinalBossTile)
            PlayState.finalBossTiles.Add(gameObject);
    }

    public void ToggleActive(bool state)
    {
        for (int i = 0; i < sprites.Length; i++)
            sprites[i].enabled = state;
        box.enabled = state;
        if (state)
            PlayState.breakablePositions.Add(worldPos);
        else
            PlayState.breakablePositions.Remove(worldPos);
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (type == -1)
            return;

        if (collision.CompareTag("PlayerBullet") && PlayState.OnScreen(transform.position, box))
        {
            int thisWeaponType = collision.GetComponent<Bullet>().bulletType;
            if (bulletsThatBreakMe[type].Contains(thisWeaponType))
            {
                if (!PlayState.explodePlayedThisFrame)
                {
                    PlayState.PlaySound("Explode" + Random.Range(1, 5));
                    PlayState.explodePlayedThisFrame = true;
                }
                for (int i = 0; i < 2; i++)
                    PlayState.RequestParticle(new Vector2(transform.position.x + Random.Range(-1f, 1f), transform.position.y + Random.Range(-1f, 1f)), "explosion", new float[] { 2 });
                PlayState.breakablePositions.Remove(worldPos);
                Destroy(gameObject);
            }
            else if (!bulletsIShouldIgnore[type].Contains(thisWeaponType))
            {
                if (!PlayState.armorPingPlayedThisFrame && !isSilent)
                {
                    PlayState.PlaySound("Ping");
                    PlayState.armorPingPlayedThisFrame = true;
                }
                if ((PlayState.generalData.breakableState == 1 && !isSilent) || (PlayState.generalData.breakableState == 2 && isSilent))
                {
                    if (!hasBeenHit)
                    {
                        foreach (SpriteRenderer sprite in sprites)
                        {
                            if (sprite.sprite != PlayState.BlankTexture())
                                sprite.sprite = PlayState.GetSprite("Entities/BreakableIcons", type);
                        }
                    }
                }
            }
            hasBeenHit = true;
        }
    }
}
