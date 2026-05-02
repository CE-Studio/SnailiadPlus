# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name CamControl
extends Node2D


#region Variables
const EASE_RATE:float = 4.25

enum CamStates {
	FOLLOW_FLASH,
	FOLLOW_NEW,
	TARGET_POINT,
	TARGET_ENTITY,
	NONE = -1,
}
## The follow state of the camera
var state:CamStates = CamStates.FOLLOW_NEW
## The current position of the camera
var pos:Vector2 = Vector2.ZERO
## The target point for the camera to follow
var target_point:Vector2 = Vector2.ZERO
## The target node for the camera to follow, if set
var target_entity:Node2D
## The player
var player:Player
## The positional difference between the center of the screen and the top-left corner of the viewport
var offset:Vector2 = Statics.VECTOR_CENTER
## The currently active [CameraBorder] that should constrict the camera's movement
var border:CameraBorder = null
## An offset applied after the camera's base position is moved in order to apply screen shake
var shake_offset:Vector2 = Vector2.ZERO
## Controls whether or not the camera should shake at all
var do_screen_shake:bool = true
## If [code]true[/code], only the camera node will shake when screen shake is applied.[br]
## If [code]false[/code], the entire UI parent will shake, bringing the HUD along for the ride.
var shake_cam_only:bool = false
## The position of the camera on the last move tick
var last_pos:Vector2 = Vector2.ZERO
## The position of the camera on the last move tick, excluding ticks focused on the default "static" position
var last_non_static_pos:Vector2 = Vector2.ZERO

#region New follow vars
const NF_OFFSET_MAX:Vector2 = Vector2(40.0, 32.0)
const NF_OFFSET_MAX_FALL:Vector2 = Vector2(128.0, 80.0)
const NF_OFFSET_ADJUST_DELAY:float = 0.5
const NF_OFFSET_RETURN_DELAY:float = 1.25
const NF_OFFSET_EASE_RATE:float = 128.0
const NF_OFFSET_RETURN_EASE_RATE:float = 64.0
const NF_OFFSET_FALL_EASE_RATE:float = 192.0
const NF_OFFSET_LAND_EASE_RATE:float = 512.0
const NF_OFFSET_MAX_FALL_MULT:float = 2.5
## Any sort of lookahead offset applied by the new follow system
var nf_offset:Vector2 = Vector2.ZERO
## A timer that delays any sort of lookahead from being applied
var nf_delay_timer:float = 0.0
## A timer that delays when the camera returns to focusing on the player after they stop moving
var nf_return_timer:float = 0.0
#endregion
#endregion


## Properly initializes the camera and connects the player and UI parent
func instantiate() -> void:
	player = GameCore.instance.player
	pos = UICore.instance.position
	set_cam_mode()


## Sets the camera's current mode, along with any target it needs to focus on
func set_cam_mode(_state:CamStates = CamStates.NONE, _target:Variant = null) -> void:
	if _state == CamStates.NONE:
		if ProjectSettings.get_setting("game/visuals/dynamic_camera"):
			state = CamStates.FOLLOW_NEW
		else:
			state = CamStates.FOLLOW_FLASH
	else:
		state = _state
		if _target:
			if _state == CamStates.TARGET_POINT and _target is Vector2:
				target_point = _target
			if _state == CamStates.TARGET_ENTITY and _target is Node2D:
				target_entity = _target


func _process(delta):
	last_pos = pos
	match state:
		CamStates.FOLLOW_FLASH:
			if player != null:
				pos = pos.lerp(player.position - offset, EASE_RATE * delta)
		CamStates.FOLLOW_NEW:
			pos = _tick_new_follow(pos, delta)
		CamStates.TARGET_POINT:
			pos = pos.lerp(target_point - offset, EASE_RATE * delta)
		CamStates.TARGET_ENTITY:
			if target_entity:
				pos = pos.lerp(target_entity.position - offset, EASE_RATE * delta)
			else:
				target_point = pos - offset
				state = CamStates.TARGET_POINT
		_:
			pass
	if border != null:
		pos = border.get_closest_point_to(pos + offset) - offset
		#region Fake cam boundaries
		for child in border.get_children():
			if child is FakeCamBoundary:
				if child.active:
					var buffer = child.get_cam_buffer()
					if child.initial_relative_pos == Statics.DirsCardinal.LEFT and child.stop_from & 1 > 0:
						pos.x = clampf(pos.x, -INF, child.position.x + buffer)
					if child.initial_relative_pos == Statics.DirsCardinal.RIGHT and child.stop_from & 2 > 0:
						pos.x = clampf(pos.x, child.position.x + buffer, INF)
					if child.initial_relative_pos == Statics.DirsCardinal.DOWN and child.stop_from & 2 > 0:
						pos.y = clampf(pos.y, child.position.y + buffer, INF)
					if child.initial_relative_pos == Statics.DirsCardinal.UP and child.stop_from & 1 > 0:
						pos.y = clampf(pos.y, -INF, child.position.y + buffer)
		#endregion
	UICore.instance.position = pos
	UICore.instance.border.position = Statics.VECTOR_CENTER
	position = offset
	if do_screen_shake:
		if shake_cam_only:
			position += shake_offset
		else:
			UICore.instance.position += shake_offset
			var global_center:Vector2 = UICore.instance.position + Statics.VECTOR_CENTER
			if border.get_closest_point_to(global_center) != global_center:
				UICore.instance.border.position -= shake_offset


## Standalone function to process the new follow system, implementing lookahead
func _tick_new_follow(_pos:Vector2, delta:float) -> Vector2:
	var move_vector:Vector2 = SInput.vector_move()
	var walled:bool = (
		player.gravity_dir == Statics.DirsSurface.LWALL or
		player.gravity_dir == Statics.DirsSurface.RWALL
	)
	
	if ((walled and move_vector.y != 0.0) or
	(not walled and move_vector.x != 0.0)):
		nf_delay_timer += delta
		nf_return_timer = 0.0
	else:
		nf_delay_timer = 0.0
		nf_return_timer += delta
	
	if nf_delay_timer >= NF_OFFSET_ADJUST_DELAY:
		if walled:
			nf_offset.y = move_toward(nf_offset.y,
				NF_OFFSET_MAX.y * move_vector.y,
				NF_OFFSET_EASE_RATE * delta
			)
		else:
			nf_offset.x = move_toward(nf_offset.x,
				NF_OFFSET_MAX.x * move_vector.x,
				NF_OFFSET_EASE_RATE * delta
			)
	if nf_return_timer >= NF_OFFSET_RETURN_DELAY:
		nf_offset = nf_offset.move_toward(Vector2.ZERO, NF_OFFSET_RETURN_EASE_RATE * delta)
	
	var tick_land:bool = true
	if not player.grounded:
		match player.gravity_dir:
			Statics.DirsSurface.FLOOR:
				if player.grounded_last_frame:
					nf_offset.y = 0.0
				if player.body.velocity.y > 0.0:
					nf_offset.y = move_toward(nf_offset.y,
						NF_OFFSET_MAX_FALL.y,
						NF_OFFSET_FALL_EASE_RATE * delta
					)
					tick_land = false
			Statics.DirsSurface.LWALL:
				if player.grounded_last_frame:
					nf_offset.x = 0.0
				if player.body.velocity.x < 0.0:
					nf_offset.x = move_toward(nf_offset.x,
						-NF_OFFSET_MAX_FALL.x,
						NF_OFFSET_FALL_EASE_RATE * delta
					)
					tick_land = false
			Statics.DirsSurface.RWALL:
				if player.grounded_last_frame:
					nf_offset.x = 0.0
				if player.body.velocity.x > 0.0:
					nf_offset.x = move_toward(nf_offset.x,
						NF_OFFSET_MAX_FALL.x,
						NF_OFFSET_FALL_EASE_RATE * delta
					)
					tick_land = false
			Statics.DirsSurface.CEILING:
				if player.grounded_last_frame:
					nf_offset.y = 0.0
				if player.body.velocity.y < 0.0:
					nf_offset.y = move_toward(nf_offset.y,
						-NF_OFFSET_MAX_FALL.y,
						NF_OFFSET_FALL_EASE_RATE * delta
					)
					tick_land = false
	if tick_land:
		if walled:
			nf_offset.x = move_toward(nf_offset.x, 0.0, NF_OFFSET_LAND_EASE_RATE * delta)
			#nf_offset.x = 0.0
		else:
			nf_offset.y = move_toward(nf_offset.y, 0.0, NF_OFFSET_LAND_EASE_RATE * delta)
			#nf_offset.y = 0.0
	
	_pos = _pos.lerp(player.position - offset + nf_offset, EASE_RATE * delta)
	return _pos


## Sets the position of the UI layer
func set_layer_position(new_pos:Vector2):
	UICore.instance.position = new_pos - offset
	pos = UICore.instance.position


## Resets the relevant timers in the new follow functionality
func reset_new_follow() -> void:
	nf_delay_timer = 0
	nf_return_timer = NF_OFFSET_RETURN_DELAY
	#nf_offset = Vector2.ZERO


## Sets the camera to the "static" position, where the top-left corner of the viewport sits at (0, 0)
func set_to_static_pos() -> void:
	last_non_static_pos = UICore.instance.position
	UICore.instance.position = Vector2.ZERO


## Returns the camera to its previous position from before it was sent to the "static" position
func reset_from_static_pos() -> void:
	UICore.instance.position = last_non_static_pos
