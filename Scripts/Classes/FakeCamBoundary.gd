# Copyright 2026 CE-Studio: AGPL-3.0-only
@tool
@icon("res://Editor/ico/FakeCamBoundary.svg")
class_name FakeCamBoundary
extends Node2D


#region Variables
## Tracks whether this border is treated as a horizontal or vertical stop.[br]
## Will be set to 0 for horizontal and 1 for vertical
@export_enum("Horizontal", "Vertical") var axis:int = 0:
	set(value):
		axis = value
		if Engine.is_editor_hint():
			update_marker()
## Determins which directions this border should be considered in by the camera.[br]
## Will be set to 1 to stop the camera from the top/left, 2 to stop from the bottom/right, or 3 for all directions
@export_flags("Left/Top", "Right/Bottom") var stop_from:int = 3:
	set(value):
		stop_from = value
		if Engine.is_editor_hint():
			update_marker()
## Determins whether or not this border should cover a full tile's width instead of a single point
@export var cover_full_tile:bool = false:
	set(value):
		cover_full_tile = value
		if Engine.is_editor_hint():
			update_marker()
## If set, will override the displayed room name below the minimap if the camera is
## above or to the left of this border
@export var up_left_room_name_override:String = ""
## If set, will override the displayed room name below the minimap if the camera is
## below or to the right of this border
@export var down_right_room_name_override:String = ""
## If set, will restore the original room name once this border has been crossed
@export var restore_name_if_crossed:bool = false
## Will determine a direction to push this border in if an aspect ratio requires the camera borders
## to be expanded
@export var aspect_offset:Vector2i = Vector2i.ZERO:
	set(value):
		aspect_offset = Vector2i(
			clampi(value.x, -1, 1),
			clampi(value.y, -1, 1)
		)
## If set, will disconnect the actual aspect ratio offset from where the player needs to cross to
## disable this border, keeping the cross threshold where it normally is
@export var offset_dir_from_entry_dir:bool = false

const BUFFER_HORIZ:float = 12.5 * 16.0
const BUFFER_VERT:float = 7.5 * 16.0

## If [code]true[/code], this boundary will restrict the camera's movement. Else, it will allow
## the camera to pass by
var active:bool = true
## The direction the camera starts relative to this boundary, based on where the player entered the room
var initial_relative_pos:Statics.DirsCardinal
## The unaltered [String] name of this room
var original_room_name:String = ""
## Tracks where the player is relative to this border's center.[br]
## Will be set to 1 if the player is below/to the right, and -1 if the player is above/to the left
var last_pos_neg_position:int = 0
## Will be set to [code]true[/code] if the room's original name has been restored to the map, assuming
## it was previously changed by an overwrite while this border was active
var restored_name:bool = false

## The point in world space where this border was spawned
@onready var origin:Vector2 = position
#endregion


## Instantiates this border on spawn. If this border operates one-way and the player starts behind
## it, it immediately disables itself
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


## Adjusts the position of this border based on any active aspect ratio if necessary
func offset_position_for_ratio() -> void:
	var current_ratio = ProjectSettings.get_setting("display/window/size/aspect_ratio")
	var offset = Statics.ASPECT_RATIO_OFFSETS[current_ratio]
	var a_offset:Vector2i = aspect_offset
	if offset_dir_from_entry_dir:
		if axis:
			a_offset = Vector2i.DOWN if GameCore.instance.player.position.y > position.y else Vector2i.UP
		else:
			a_offset = Vector2i.LEFT if GameCore.instance.player.position.x > position.x else Vector2i.RIGHT
	position = origin + (offset * a_offset * 0.5)


## Editor function, called to update the sprite drawn to indicate the current state of the border
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


func _process(_delta: float) -> void:
	if not Engine.is_editor_hint():
		var player_pos = GameCore.instance.player.position
		if axis == 0:
			if abs(player_pos.x - position.x) <= 8.0:
				active = false
		else:
			if abs(player_pos.y - position.y) <= 8.0:
				active = false
		#region Set room name on either side where applicable
		if ((down_right_room_name_override != "" or up_left_room_name_override != "")
		and ((restore_name_if_crossed and active) or not restore_name_if_crossed)):
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
		elif restore_name_if_crossed and not active and not restored_name:
			UICore.instance.minimap.set_room_name(original_room_name)
			restored_name = true
		#endregion


## Returns a point in world space to be read by the camera, in order to properly get stopped
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
