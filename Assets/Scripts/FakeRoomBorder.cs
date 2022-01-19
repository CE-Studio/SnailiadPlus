using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeRoomBorder : MonoBehaviour
{
    public bool direction = false; // Specifies the relative orientation of the border. False for horizontal border, true for vertical border
    public int workingDirections = 3; // Specifies what directions it will look for the player upon spawning. 1 for down/left only, 2 for up/right only, 3 for both
    
    private bool isActive = true;
    public Vector2 initialPosRelative = Vector2.zero;

    private const float BUFFER_HORIZ = 13;
    private const float BUFFER_VERT = 8;

    private void OnEnable()
    {
        isActive = true;
        initialPosRelative = new Vector2(PlayState.player.transform.position.x > transform.position.x ? 1 : -1,
            PlayState.player.transform.position.y > transform.position.y ? 1 : -1);
    }

    private void Update()
    {
        if (isActive)
        {
            if (direction)
            {
                if ((initialPosRelative.x == 1 && PlayState.player.transform.position.x < transform.position.x) ||
                    (initialPosRelative.x == -1 && PlayState.player.transform.position.x > transform.position.x))
                    isActive = false;
                else
                {
                    if (workingDirections >= 2 && initialPosRelative.x == 1)
                        PlayState.cam.transform.position = new Vector2(
                            Mathf.Clamp(PlayState.cam.transform.position.x, transform.position.x + BUFFER_HORIZ, Mathf.Infinity),
                            PlayState.cam.transform.position.y);
                    else if ((workingDirections == 1 || workingDirections == 3) && initialPosRelative.x == -1)
                        PlayState.cam.transform.position = new Vector2(
                            Mathf.Clamp(PlayState.cam.transform.position.x, -Mathf.Infinity, transform.position.x - BUFFER_HORIZ),
                            PlayState.cam.transform.position.y);
                }
            }
            else
            {
                if ((initialPosRelative.y == 1 && PlayState.player.transform.position.y < transform.position.y) ||
                    (initialPosRelative.y == -1 && PlayState.player.transform.position.y > transform.position.y))
                    isActive = false;
                else
                {
                    if (workingDirections >= 2 && initialPosRelative.y == 1)
                        PlayState.cam.transform.position = new Vector2(
                            PlayState.cam.transform.position.x,
                            Mathf.Clamp(PlayState.cam.transform.position.y, transform.position.y + BUFFER_VERT, Mathf.Infinity));
                    else if ((workingDirections == 1 || workingDirections == 3) && initialPosRelative.y == -1)
                        PlayState.cam.transform.position = new Vector2(
                            PlayState.cam.transform.position.x,
                            Mathf.Clamp(PlayState.cam.transform.position.y, -Mathf.Infinity, transform.position.y - BUFFER_VERT));
                }
            }
        }
    }
}
