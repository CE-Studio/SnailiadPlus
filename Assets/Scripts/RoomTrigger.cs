using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public BoxCollider2D box;
    
    void Start()
    {
        box = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
            PlayState.camCenter = new Vector2(transform.position.x, transform.position.y);
            PlayState.camBoundaryBuffers = new Vector2((box.size.x + 1) * 0.5f - 12.5f, (box.size.y + 1) * 0.5f - 7.5f);
            Debug.Log("(" + transform.position.x + ", " + transform.position.y + "), (" + PlayState.camBoundaryBuffers.x + ", " + PlayState.camBoundaryBuffers.y + ")");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}
