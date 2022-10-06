using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint:MonoBehaviour, IRoomObject {
    public bool hasBeenActivated = false;

    public AnimationModule anim;

    public void Spawn() {
        anim = GetComponent<AnimationModule>();
        anim.Add("Save_inactive");
        anim.Add("Save_active");
        anim.Add("Save_last");

        if (PlayState.respawnCoords == new Vector2(transform.position.x, transform.position.y + 0.5f))
            anim.Play("Save_last");
        else
            anim.Play("Save_inactive");
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            if (!hasBeenActivated) {
                hasBeenActivated = true;
                PlayState.PlaySound("Save");
                anim.Play("Save_active");
                PlayState.FlashSaveText();
                PlayState.respawnCoords = new Vector2(transform.position.x, transform.position.y + 0.5f);
                PlayState.positionOfLastSave = PlayState.positionOfLastRoom;
                PlayState.WriteSave("game");
                PlayState.WriteSave("records");
            }
        }
    }
}
