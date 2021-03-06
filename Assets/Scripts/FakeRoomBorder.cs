using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeRoomBorder : MonoBehaviour
{
    public bool direction = false; // Specifies the relative orientation of the border. False for horizontal border, true for vertical border
    public int workingDirections = 3; // Specifies what directions it will look for the player upon spawning. 1 for down/left only, 2 for up/right only, 3 for both
    public string downLeftRoomName = "";
    public string upRightRoomName = "";
    
    private bool isActive = true;
    public Vector2 initialPosRelative = Vector2.zero;

    private const float BUFFER_HORIZ = 13;
    private const float BUFFER_VERT = 8;

    private TextMesh roomNameText;
    private TextMesh roomNameShadow;

    public void Awake()
    {
        roomNameText = GameObject.Find("View/Minimap Panel/Room Name Parent/Room Name Text").GetComponent<TextMesh>();
        roomNameShadow = GameObject.Find("View/Minimap Panel/Room Name Parent/Room Name Shadow").GetComponent<TextMesh>();
    }

    public void Spawn(int[] spawnData)
    {
        direction = spawnData[0] == 1;
        workingDirections = spawnData[1];

        isActive = true;
        initialPosRelative = new Vector2(PlayState.player.transform.position.x > transform.position.x ? 1 : -1,
            PlayState.player.transform.position.y > transform.position.y ? 1 : -1);
    }

    private void Update()
    {
        if (downLeftRoomName != "" || upRightRoomName != "")
        {
            if ((!direction && PlayState.player.transform.position.y > transform.position.y) ||
                (direction && PlayState.player.transform.position.x > transform.position.x))
            {
                roomNameText.text = upRightRoomName;
                roomNameShadow.text = upRightRoomName;
            }
            else
            {
                roomNameText.text = downLeftRoomName;
                roomNameShadow.text = downLeftRoomName;
            }
        }

        if (isActive)
        {
            if (direction)
            {
                if ((initialPosRelative.x == 1 && PlayState.player.transform.position.x < transform.position.x + 0.5f) ||
                    (initialPosRelative.x == -1 && PlayState.player.transform.position.x > transform.position.x - 0.5f))
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
                if ((initialPosRelative.y == 1 && PlayState.player.transform.position.y < transform.position.y + 0.5) ||
                    (initialPosRelative.y == -1 && PlayState.player.transform.position.y > transform.position.y - 0.5f))
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
