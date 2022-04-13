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
        anim.Play("Save_inactive");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
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
        if (hasBeenActivated)
            anim.Play("Save_inactive");
        else
        {
            PlayState.PlaySound("Save");
            anim.Play("Save_active");
        }
        hasBeenActivated = !hasBeenActivated;
    }
}
