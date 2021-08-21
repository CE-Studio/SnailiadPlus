﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomTrigger : MonoBehaviour
{
    public BoxCollider2D box;
    public bool active = true;

    public float parallaxBackgroundModifier = 0;

    public TextMesh roomNameText;
    public TextMesh roomNameShadow;

    public Tilemap bg;
    public Tilemap breakableMap;
    public GameObject breakableBlock;
    
    void Start()
    {
        box = GetComponent<BoxCollider2D>();
        roomNameText = GameObject.Find("View/Minimap Panel/Room Name Text").GetComponent<TextMesh>();
        roomNameShadow = GameObject.Find("View/Minimap Panel/Room Name Shadow").GetComponent<TextMesh>();
        bg = GameObject.Find("Grid/Ground").GetComponent<Tilemap>();
        breakableMap = GameObject.Find("Grid/Breakables").GetComponent<Tilemap>();
        breakableBlock = (GameObject)Resources.Load("Objects/Breakable Block");
        DespawnEverything();
        breakableMap.color = new Color32(255, 255, 255, 0);
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && active)
        {
            foreach (Transform trigger in transform.parent)
            {
                if (!trigger.GetComponent<Collider2D>().enabled && trigger.name != transform.name)
                {
                    trigger.GetComponent<Collider2D>().enabled = true;
                    trigger.GetComponent<RoomTrigger>().active = true;
                    trigger.GetComponent<RoomTrigger>().DespawnEverything();
                }
            }

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
                if (child.name.Contains("Door"))
                {
                    if (Vector2.Distance(collision.transform.position, child.transform.position) < 2)
                    {
                        child.GetComponent<Door>().SetState1();
                    }
                    else
                    {
                        child.GetComponent<Door>().SetState2();
                    }
                }
                else if (child.name.Contains("Grass"))
                {
                    switch (child.name)
                    {
                        default:
                            break;
                        case "Grass":
                            child.GetComponent<Grass>().Spawn();
                            break;
                    }
                }
            }
            PlayState.camCenter = new Vector2(transform.position.x, transform.position.y);
            PlayState.camBoundaryBuffers = new Vector2((box.size.x + 0.5f) * 0.5f - 12.5f, (box.size.y + 0.5f) * 0.5f - 7.5f);
            PlayState.ScreenFlash("Room Transition", 0, 0, 0, 0);
            PlayState.parallaxMod = parallaxBackgroundModifier;

            string newRoomName = "";
            foreach (char character in transform.name)
            {
                if (character == '|')
                    newRoomName += "\n";
                else
                    newRoomName += character;
            }
            roomNameText.text = newRoomName;
            roomNameShadow.text = newRoomName;

            CheckForBreakables(collision.transform);

            if (PlayState.player.GetComponent<Player>()._currentSurface == 1 && PlayState.player.GetComponent<Player>()._facingUp)
            {
                PlayState.player.transform.position = new Vector2(PlayState.player.transform.position.x, PlayState.player.transform.position.y + 0.125f);
            }
            else if (PlayState.player.GetComponent<Player>()._currentSurface == 1)
            {
                PlayState.player.transform.position = new Vector2(PlayState.player.transform.position.x, PlayState.player.transform.position.y - 0.125f);
            }
            else if (PlayState.player.GetComponent<Player>()._facingLeft)
            {
                PlayState.player.transform.position = new Vector2(PlayState.player.transform.position.x - 0.125f, PlayState.player.transform.position.y);
            }
            else
            {
                PlayState.player.transform.position = new Vector2(PlayState.player.transform.position.x + 0.125f, PlayState.player.transform.position.y);
            }

            box.enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            active = false;
        }
    }

    private void DespawnEverything()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
            if (child.name == "Door")
            {
                child.GetComponent<Door>().SetStateDespawn();
            }
            else if (child.name.Contains("Grass"))
            {
                switch (child.name)
                {
                    default:
                        break;
                    case "Grass":
                        child.GetComponent<Grass>().ToggleActive(false);
                        break;
                }
            }
            else if (child.name.Contains("Breakable Block"))
            {
                child.GetComponent<BreakableBlock>().Despawn();
            }
        }
        GameObject pool = GameObject.Find("Player Bullet Pool");
        for (int i = 0; i < pool.transform.childCount; i++)
        {
            if (pool.transform.GetChild(i).transform.GetComponent<Bullet>().isActive)
            {
                pool.transform.GetChild(i).transform.GetComponent<Bullet>().Despawn();
            }
            pool.transform.GetChild(i).transform.position = Vector2.zero;
        }
    }

    private void CheckForBreakables(Transform player)
    {
        int limitX = (int)Mathf.Round((box.size.x + 0.5f) * 0.5f + 1);
        int limitY = (int)Mathf.Round((box.size.y + 0.5f) * 0.5f + 1);
        for (int x = -limitX; x <= limitX; x++)
        {
            for (int y = -limitY; y <= limitY; y++)
            {
                Vector3Int tilePos = new Vector3Int((int)Mathf.Round(transform.position.x) + x, (int)Mathf.Round(transform.position.y) + y, 0);
                TileBase originalTile = bg.GetTile(tilePos);
                Sprite originalTileSprite = bg.GetSprite(tilePos);
                TileBase currentTile = breakableMap.GetTile(tilePos);
                Sprite currentTileSprite = breakableMap.GetSprite(tilePos);
                if (currentTile != null)
                {
                    int weaponType = 0;
                    bool isSilent = false;
                    switch (currentTileSprite.name)
                    {
                        case "Tilesheet_72":
                            weaponType = 1;
                            break;
                        case "Tilesheet_73":
                            weaponType = 2;
                            break;
                        case "Tilesheet_74":
                            weaponType = 3;
                            break;
                        case "Tilesheet_439":
                            weaponType = 3;
                            isSilent = true;
                            break;
                    }

                    GameObject Breakable = Instantiate(breakableBlock, new Vector2(tilePos.x + 0.5f, tilePos.y + 0.5f), Quaternion.identity);
                    Breakable.transform.parent = transform;
                    Breakable.GetComponent<BreakableBlock>().coverSprite = originalTileSprite;
                    Breakable.GetComponent<BreakableBlock>().originalTile = originalTile;
                    Breakable.GetComponent<BreakableBlock>().homeMap = bg;
                    Breakable.GetComponent<BreakableBlock>().Instantiate(weaponType, isSilent);

                    bg.SetTile(tilePos, null);
                }
            }
        }
    }
}
