@tool
@icon("res://Editor/ico/FakeCamBoundary.svg")
class_name FakeCamBoundary
extends Node2D

#public class FakeRoomBorder:MonoBehaviour, IRoomObject {
#    [SerializeField] private bool direction = false; // Specifies the relative orientation of the border. False for horizontal border, true for vertical border
#    [SerializeField] private int workingDirections = 3; // Specifies what directions it will look for the player upon spawning. 1 for down/left only, 2 for up/right only, 3 for both
#    public string downLeftRoomName = "";
#    private string rawDownLeftRoomName = "";
#    public string upRightRoomName = "";
#    private string rawUpRightRoomName = "";
#
#    private bool isActive = true;
#    public Vector2 initialPosRelative = Vector2.zero;
#
#    private const float BUFFER_HORIZ = 13;
#    private const float BUFFER_VERT = 8;
#
#    public Dictionary<string, object> resave() {
#        return null;
#    }

#region Variables
@export_enum("Horizontal", "Vertical") var axis:int = 0:
	set(value):
		axis = value
		if Engine.is_editor_hint():
			update_marker()
@export_flags("Left/Top", "Right/Bottom") var stop_from:int = 3:
	set(value):
		stop_from = value
		if Engine.is_editor_hint():
			update_marker()
@export var cover_full_tile:bool = false:
	set(value):
		cover_full_tile = value
		if Engine.is_editor_hint():
			update_marker()
@export var up_left_room_name_override:String = ""
@export var down_right_room_name_override:String = ""

const BUFFER_HORIZ:float = 12.5 * 16.0
const BUFFER_VERT:float = 7.5 * 16.0

var active:bool = true
var initial_relative_pos:Statics.DirsCardinal
var original_room_name:String = ""
#endregion


#    public void Spawn()
#    {
#        isActive = true;
#        initialPosRelative = new Vector2(PlayState.player.transform.position.x > transform.position.x ? 1 : -1,
#            PlayState.player.transform.position.y > transform.position.y ? 1 : -1);
#        if (workingDirections != 3)
#        {
#            if (direction)
#            {
#                if ((initialPosRelative.x < 0 && workingDirections == 2) || (initialPosRelative.x > 0 && workingDirections == 1))
#                    isActive = false;
#            }
#            else
#            {
#                if ((initialPosRelative.y < 0 && workingDirections == 2) || (initialPosRelative.y > 0 && workingDirections == 1))
#                    isActive = false;
#            }
#        }
#
#        string roomName = transform.parent.name;
#        if (roomName.Contains("/"))
#        {
#            string[] nameParts = roomName.Split('/');
#            int areaID = transform.parent.GetComponent<RoomTrigger>().areaID;
#            rawDownLeftRoomName = "room_" + (areaID < 10 ? "0" : "") + areaID + "_" + nameParts[0];
#            foreach (char character in PlayState.GetText(rawDownLeftRoomName))
#            {
#                if (character == '|')
#                    downLeftRoomName += "\n";
#                else
#                    downLeftRoomName += character;
#            }
#            rawUpRightRoomName = "room_" + (areaID < 10 ? "0" : "") + areaID + "_" + nameParts[1];
#            foreach (char character in PlayState.GetText(rawUpRightRoomName))
#            {
#                if (character == '|')
#                    upRightRoomName += "\n";
#                else
#                    upRightRoomName += character;
#            }
#        }
#    }
func instance() -> void:
	if axis == 0:
		if GameCore.instance.player.position.x > position.x:
			initial_relative_pos = Statics.DirsCardinal.RIGHT
			if stop_from & 2 == 0:
				active = false
		else:
			initial_relative_pos = Statics.DirsCardinal.LEFT
			if stop_from & 1 == 0:
				active = false
	else:
		if GameCore.instance.player.position.y > position.y:
			initial_relative_pos = Statics.DirsCardinal.DOWN
			if stop_from & 2 == 0:
				active = false
		else:
			initial_relative_pos = Statics.DirsCardinal.UP
			if stop_from & 1 == 0:
				active = false


func update_marker() -> void:
	assert(Engine.is_editor_hint(), "Fake boundary marker updates should only happen in the editor.")
	var marker = $"MarkerSprite"
	marker.flip_h = false
	marker.flip_v = false
	marker.modulate = Color(1.0, 1.0, 1.0, 1.0)
	if axis == 0:
		match stop_from:
			0:
				marker.frame = 1
				marker.modulate = Color(1.0, 1.0, 1.0, 0.5)
			1:
				marker.frame = 0
				marker.flip_h = true
			2:
				marker.frame = 0
			3:
				marker.frame = 1
	else:
		match stop_from:
			0:
				marker.frame = 3
				marker.modulate = Color(1.0, 1.0, 1.0, 0.5)
			1:
				marker.frame = 2
				marker.flip_v = true
			2:
				marker.frame = 2
			3:
				marker.frame = 3
	if cover_full_tile:
		marker.frame += 4


func _process(delta: float) -> void:
	if not Engine.is_editor_hint():
		if axis == 0:
			if abs(GameCore.instance.player.position.x - position.x) <= 8.0:
				active = false
		else:
			if abs(GameCore.instance.player.position.y - position.y) <= 8.0:
				active = false
#    public void Update()
#    {
#        if ((rawDownLeftRoomName.Contains("ALT") || rawUpRightRoomName.Contains("ALT")) && workingDirections != 3)
#        {
#            string tempName = downLeftRoomName;
#            string trueName = downLeftRoomName;
#            if (rawDownLeftRoomName.Contains("ALT"))
#                trueName = upRightRoomName;
#            else
#                tempName = upRightRoomName;
#            PlayState.hudRoomName.SetText(isActive ? tempName : trueName);
#        }
#        else if (downLeftRoomName != "" || upRightRoomName != "")
#        {
#            if ((!direction && PlayState.player.transform.position.y > transform.position.y) ||
#                (direction && PlayState.player.transform.position.x > transform.position.x))
#                PlayState.hudRoomName.SetText(upRightRoomName);
#            else
#                PlayState.hudRoomName.SetText(downLeftRoomName);
#        }
#
#        if (isActive)
#        {
#            if (direction)
#            {
#                if ((initialPosRelative.x == 1 && PlayState.player.transform.position.x < transform.position.x + 0.5f) ||
#                    (initialPosRelative.x == -1 && PlayState.player.transform.position.x > transform.position.x - 0.5f))
#                    isActive = false;
#                else
#                {
#                    if (workingDirections >= 2 && initialPosRelative.x == 1)
#                        PlayState.cam.transform.position = new Vector2(
#                            Mathf.Clamp(PlayState.cam.transform.position.x, transform.position.x + BUFFER_HORIZ, Mathf.Infinity),
#                            PlayState.cam.transform.position.y);
#                    else if ((workingDirections == 1 || workingDirections == 3) && initialPosRelative.x == -1)
#                        PlayState.cam.transform.position = new Vector2(
#                            Mathf.Clamp(PlayState.cam.transform.position.x, -Mathf.Infinity, transform.position.x - BUFFER_HORIZ),
#                            PlayState.cam.transform.position.y);
#                }
#            }
#            else
#            {
#                if ((initialPosRelative.y == 1 && PlayState.player.transform.position.y < transform.position.y + 0.5) ||
#                    (initialPosRelative.y == -1 && PlayState.player.transform.position.y > transform.position.y - 0.5f))
#                    isActive = false;
#                else
#                {
#                    if (workingDirections >= 2 && initialPosRelative.y == 1)
#                        PlayState.cam.transform.position = new Vector2(
#                            PlayState.cam.transform.position.x,
#                            Mathf.Clamp(PlayState.cam.transform.position.y, transform.position.y + BUFFER_VERT, Mathf.Infinity));
#                    else if ((workingDirections == 1 || workingDirections == 3) && initialPosRelative.y == -1)
#                        PlayState.cam.transform.position = new Vector2(
#                            PlayState.cam.transform.position.x,
#                            Mathf.Clamp(PlayState.cam.transform.position.y, -Mathf.Infinity, transform.position.y - BUFFER_VERT));
#                }
#            }
#        }
#    }
#}


func get_cam_buffer() -> float:
	var buffer:float = 0.0
	match initial_relative_pos:
		Statics.DirsCardinal.LEFT:
			buffer = -400
			if cover_full_tile: buffer -= 8
		Statics.DirsCardinal.RIGHT:
			if cover_full_tile: buffer += 8
		Statics.DirsCardinal.DOWN:
			if cover_full_tile: buffer += 8
		Statics.DirsCardinal.UP:
			buffer = -240
			if cover_full_tile: buffer -= 8
	return buffer
