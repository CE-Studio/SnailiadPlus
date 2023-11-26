using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeRoomBorder:MonoBehaviour, IRoomObject {
    [SerializeField] private bool direction = false; // Specifies the relative orientation of the border. False for horizontal border, true for vertical border
    [SerializeField] private int workingDirections = 3; // Specifies what directions it will look for the player upon spawning. 1 for down/left only, 2 for up/right only, 3 for both
    public string downLeftRoomName = "";
    private string rawDownLeftRoomName = "";
    public string upRightRoomName = "";
    private string rawUpRightRoomName = "";

    private bool isActive = true;
    public Vector2 initialPosRelative = Vector2.zero;

    private const float BUFFER_HORIZ = 13;
    private const float BUFFER_VERT = 8;

    public Dictionary<string, object> resave() {
        return null;
    }

    public static readonly string myType = "Fake Boundary";

    public string objType {
        get {
            return myType;
        }
    }

    public Dictionary<string, object> save() {
        Dictionary<string, object> content = new();
        content["direction"] = direction;
        content["workingDirections"] = workingDirections;
        return content;
    }

    public void load(Dictionary<string, object> content) {
        direction = (bool)content["direction"];
        workingDirections = (int)content["workingDirections"];
        Spawn();
    }

    public void Spawn()
    {
        isActive = true;
        initialPosRelative = new Vector2(PlayState.player.transform.position.x > transform.position.x ? 1 : -1,
            PlayState.player.transform.position.y > transform.position.y ? 1 : -1);
        if (workingDirections != 3)
        {
            if (direction)
            {
                if ((initialPosRelative.x < 0 && workingDirections == 2) || (initialPosRelative.x > 0 && workingDirections == 1))
                    isActive = false;
            }
            else
            {
                if ((initialPosRelative.y < 0 && workingDirections == 2) || (initialPosRelative.y > 0 && workingDirections == 1))
                    isActive = false;
            }
        }

        string roomName = transform.parent.name;
        if (roomName.Contains("/"))
        {
            string[] nameParts = roomName.Split('/');
            int areaID = transform.parent.GetComponent<RoomTrigger>().areaID;
            rawDownLeftRoomName = "room_" + (areaID < 10 ? "0" : "") + areaID + "_" + nameParts[0];
            foreach (char character in PlayState.GetText(rawDownLeftRoomName))
            {
                if (character == '|')
                    downLeftRoomName += "\n";
                else
                    downLeftRoomName += character;
            }
            rawUpRightRoomName = "room_" + (areaID < 10 ? "0" : "") + areaID + "_" + nameParts[1];
            foreach (char character in PlayState.GetText(rawUpRightRoomName))
            {
                if (character == '|')
                    upRightRoomName += "\n";
                else
                    upRightRoomName += character;
            }
        }
    }

    public void Update()
    {
        if ((rawDownLeftRoomName.Contains("ALT") || rawUpRightRoomName.Contains("ALT")) && workingDirections != 3)
        {
            string tempName = downLeftRoomName;
            string trueName = downLeftRoomName;
            if (rawDownLeftRoomName.Contains("ALT"))
                trueName = upRightRoomName;
            else
                tempName = upRightRoomName;
            PlayState.hudRoomName.SetText(isActive ? tempName : trueName);
        }
        else if (downLeftRoomName != "" || upRightRoomName != "")
        {
            if ((!direction && PlayState.player.transform.position.y > transform.position.y) ||
                (direction && PlayState.player.transform.position.x > transform.position.x))
                PlayState.hudRoomName.SetText(upRightRoomName);
            else
                PlayState.hudRoomName.SetText(downLeftRoomName);
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
