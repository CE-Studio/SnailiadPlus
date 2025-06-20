@tool
@icon("res://Editor/ico/FakeCamBoundary.svg")
class_name FakeCamBoundary
extends Node2D


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
@export var aspect_offset:Vector2i = Vector2i.ZERO:
	set(value):
		aspect_offset = Vector2i(
			clampi(value.x, -1, 1),
			clampi(value.y, -1, 1)
		)

const BUFFER_HORIZ:float = 12.5 * 16.0
const BUFFER_VERT:float = 7.5 * 16.0

var active:bool = true
var initial_relative_pos:Statics.DirsCardinal
var original_room_name:String = ""
var last_pos_neg_position:int = 0

@onready var origin:Vector2 = position
#endregion


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
	offset_position_for_ratio()


func offset_position_for_ratio() -> void:
	var current_ratio = int(Statics.data_general["aspect_ratio"])
	var offset = Statics.ASPECT_RATIO_OFFSETS[current_ratio]
	position = origin + (offset * 0.5)


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
		var player_pos = GameCore.instance.player.position
		if axis == 0:
			if abs(player_pos.x - position.x) <= 8.0:
				active = false
		else:
			if abs(player_pos.y - position.y) <= 8.0:
				active = false
		#region Set room name on either side where applicable
		if down_right_room_name_override != "" or up_left_room_name_override != "":
			var this_pos_neg_position:int = 0
			if ((axis == 0 and player_pos.x > position.x)
			or (axis == 1 and player_pos.y > position.y)):
				this_pos_neg_position = 1
			else:
				this_pos_neg_position = -1
			if this_pos_neg_position != last_pos_neg_position:
				if this_pos_neg_position == 1:
					if down_right_room_name_override == "":
						UICore.instance.minimap.set_room_name(original_room_name)
					else:
						UICore.instance.minimap.set_room_name(down_right_room_name_override)
				else:
					if up_left_room_name_override == "":
						UICore.instance.minimap.set_room_name(original_room_name)
					else:
						UICore.instance.minimap.set_room_name(up_left_room_name_override)
				last_pos_neg_position = this_pos_neg_position
		#endregion


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
