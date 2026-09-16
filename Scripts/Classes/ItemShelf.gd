# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name ItemShelf
extends Node2D


#region Variables
const LERP_RATE:float = 12.0

var x_pos:Array[float] = []
var target_pos:Array[Vector2] = []
var items:Array[Item] = []
var last_player_x:float = 0.0
var nearest_item:int = 0
var do_lerp:bool = false
var first_frame:bool = true

@export_range(1.0, 40.0) var x_buffer:float = 14.0
@export_range(1, 12) var x_buffer_fade:int = 6
@export_range(0.0, 48.0) var y_retract:float = 24.0
@export_range(0.0, 32.0) var y_retract_mod:float = 8.0
#endregion


func _ready() -> void:
	for child in get_children():
		if child is Item:
			items.append(child)
			x_pos.append(child.position.x)
			target_pos.append(Vector2(child.position.x, 0.0))
	last_player_x = Player.instance.position.x
	nearest_item = _find_nearest()


func _process(delta: float) -> void:
	var this_nearest:int = _find_nearest()
	if this_nearest != nearest_item:
		nearest_item = this_nearest
		if not first_frame:
			do_lerp = true
			_calculate_new_targets()
		first_frame = false
	
	if do_lerp:
		for i in range(x_pos.size()):
			if items[i]:
				items[i].position = items[i].position.lerp(target_pos[i], LERP_RATE * delta)


func _find_nearest() -> int:
	var player_x:float = Player.instance.position.x - position.x
	var last_difference:float = 99999.0
	var nearest_id:int = 0
	for i in range(x_pos.size()):
		var this_difference:float = abs(x_pos[i] - player_x)
		if this_difference < last_difference:
			last_difference = this_difference
			nearest_id = i
	return nearest_id


func _calculate_new_targets() -> void:
	for i in range(x_pos.size()):
		target_pos[i] = Vector2(x_pos[i], 0.0)
		if i == nearest_item:
			continue
		var abs_id:int = absi(i - nearest_item)
		var x_weight:float = clampf(inverse_lerp(x_buffer_fade, 0, abs_id), 0.0, 1.0)
		x_weight = ease(x_weight, 4.0)
		var x_sign:int = -1 if i < nearest_item else 1
		target_pos[i].x += lerpf(0.0, x_buffer, x_weight) * x_sign
		target_pos[i].y -= y_retract
		target_pos[i].y += lerpf(0.0, y_retract_mod, x_weight)
