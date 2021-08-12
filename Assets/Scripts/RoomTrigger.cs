using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public BoxCollider2D box;
    public bool active = true;

    public float parallaxBackgroundModifier = 0;
    
    void Start()
    {
        box = GetComponent<BoxCollider2D>();
        DespawnEverything();
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && active)
        {
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

            foreach (Transform trigger in transform.parent)
            {
                if (!trigger.GetComponent<Collider2D>().enabled && trigger.name != transform.name)
                {
                    trigger.GetComponent<Collider2D>().enabled = true;
                    trigger.GetComponent<RoomTrigger>().active = true;
                    trigger.GetComponent<RoomTrigger>().DespawnEverything();
                }
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
}
