using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeRoomBorder : MonoBehaviour
{
    public bool direction = false; // Specifies the relative orientation of the border. False for horizontal border, true for vertical border
    public int workingDirections = 3; // Specifies what directions it will look for the player upon spawning. 1 for down/left only, 2 for up/right only, 3 for both

    private void Update()
    {
        if (direction && Mathf.Sign(PlayState.posRelativeToTempBuffers.x) != Mathf.Sign(PlayState.player.transform.position.x - transform.position.x))
            PlayState.camTempBuffersX = Vector2.zero;
        if (!direction && Mathf.Sign(PlayState.posRelativeToTempBuffers.y) != Mathf.Sign(PlayState.player.transform.position.y - transform.position.y))
            PlayState.camTempBuffersY = Vector2.zero;
    }
}
