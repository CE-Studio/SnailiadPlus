using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableBlock : MonoBehaviour
{
    public int requiredWeapon;
    public bool isSilent;
    private bool hasBeenHit;
    public BoxCollider2D box;
    public SpriteRenderer sprite;

    public TileBase gTile = null;
    public TileBase fgTile = null;
    public TileBase fg2Tile = null;
    public List<Tilemap> maps = new List<Tilemap>();
    public Vector3Int tilePos;
    
    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();

        sprite.sprite = PlayState.BlankTexture();

        maps.Add(GameObject.Find("Grid/Ground").GetComponent<Tilemap>());
        maps.Add(GameObject.Find("Grid/Foreground").GetComponent<Tilemap>());
        maps.Add(GameObject.Find("Grid/Foreground 2").GetComponent<Tilemap>());

        tilePos = new Vector3Int((int)Mathf.Round(transform.position.x - 0.5f), (int)Mathf.Round(transform.position.y - 0.5f), 0);
        if (maps[0].GetTile(tilePos) != null)
            gTile = maps[0].GetTile(tilePos);
        if (maps[1].GetTile(tilePos) != null)
            fgTile = maps[1].GetTile(tilePos);
        if (maps[2].GetTile(tilePos) != null)
            fg2Tile = maps[2].GetTile(tilePos);
    }

    private void Update()
    {
        if (transform.position.x > PlayState.cam.transform.position.x - 12.5f - (box.size.x * 0.5f) &&
            transform.position.x < PlayState.cam.transform.position.x + 12.5f + (box.size.x * 0.5f) &&
            transform.position.y > PlayState.cam.transform.position.y - 7.5f - (box.size.y * 0.5f) &&
            transform.position.y < PlayState.cam.transform.position.y + 7.5f + (box.size.y * 0.5f) && !hasBeenHit)
            box.enabled = true;
        else
            box.enabled = false;
    }

    public void Instantiate(int type, bool silent)
    {
        requiredWeapon = type;
        isSilent = silent;
        sprite.sprite = PlayState.BlankTexture();
    }

    public void Despawn()
    {
        tilePos = new Vector3Int((int)Mathf.Round(transform.position.x - 0.5f), (int)Mathf.Round(transform.position.y - 0.5f), 0);
        if (gTile != null)
            maps[0].SetTile(tilePos, gTile);
        if (fgTile != null)
            maps[1].SetTile(tilePos, fgTile);
        if (fg2Tile != null)
            maps[2].SetTile(tilePos, fg2Tile);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PlayerBullet"))
        {
            if (collision.GetComponent<Bullet>().bulletType >= requiredWeapon)
            {
                maps[0].SetTile(tilePos, null);
                maps[1].SetTile(tilePos, null);
                maps[2].SetTile(tilePos, null);
                box.enabled = false;
                sprite.sprite = PlayState.BlankTexture();
                hasBeenHit = true;
                if (!PlayState.explodePlayedThisFrame)
                {
                    PlayState.PlaySound("Explode" + Random.Range(1, 5));
                    PlayState.explodePlayedThisFrame = true;
                }
                for (int i = 0; i < 2; i++)
                    PlayState.RequestParticle(new Vector2(transform.position.x + Random.Range(-1f, 1f), transform.position.y + Random.Range(-1f, 1f)), "explosion", new float[] { 2 });
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
                        if (sprite.sprite == PlayState.BlankTexture())
                            sprite.sprite = PlayState.GetSprite("Entities/BreakableIcons", requiredWeapon - 1);
                    }
                }
            }
        }
    }
}
