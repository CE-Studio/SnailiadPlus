using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    public bool hasBeenActivated = false;

    public AnimationModule anim;

    void Start()
    {
        anim = GetComponent<AnimationModule>();
        anim.Add("Save_inactive");
        anim.Add("Save_active");
        anim.Add("Save_last");
    }

    public void Spawn()
    {
        if (PlayState.respawnCoords == new Vector2(transform.position.x, transform.position.y))
            anim.Play("Save_last");
        else
            anim.Play("Save_inactive");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!hasBeenActivated)
            {
                ToggleActiveState();
                PlayState.FlashSaveText();
                PlayState.respawnCoords = transform.position;
                PlayState.WriteSave("game");
                PlayState.WriteSave("records");
            }
        }
    }

    public void ToggleActiveState()
    {
        if (!hasBeenActivated)
        {
            PlayState.PlaySound("Save");
            anim.Play("Save_active");
        }
        hasBeenActivated = !hasBeenActivated;
    }
}
