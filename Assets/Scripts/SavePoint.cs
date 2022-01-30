using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    public bool hasBeenActivated = false;

    public Animator anim;
    public AudioSource sfx;
    public AudioClip saveSfx;

    public 

    void Start()
    {
        anim = GetComponent<Animator>();
        sfx = GetComponent<AudioSource>();
        saveSfx = (AudioClip)Resources.Load("Sounds/Sfx/Save");
    }

    void Update()
    {
        if (PlayState.gameState == "Game")
            anim.speed = 1;
        else
            anim.speed = 0;
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
            }
        }
    }

    public void ToggleActiveState()
    {
        if (hasBeenActivated)
        {
            anim.Play("Save inactive", 0, 0);
        }
        else
        {
            sfx.PlayOneShot(saveSfx);
            anim.Play("Save active", 0, 0);
        }
        hasBeenActivated = !hasBeenActivated;
    }
}
