class_name CamControl
extends Node2D


#region Variables
enum CamStates {
	FOLLOW_FLASH,
	FOLLOW_NEW,
	TARGET_POINT,
	TARGET_ENTITY,
	NONE = -1,
}
var state:CamStates = CamStates.FOLLOW_NEW
var target_point:Vector2 = Vector2.ZERO
var target_entity:Node2D
var player:Player
var ease_rate:float = 4.25
var offset:Vector2 = Vector2(200, 120)
var border:CameraBorder = null

#region New follow vars
const NF_OFFSET_MAX:Vector2 = Vector2(64.0, 48.0)
const NF_OFFSET_MAX_FALL:Vector2 = Vector2(128.0, 80.0)
const NF_OFFSET_ADJUST_DELAY:float = 0.5
const NF_OFFSET_EASE_RATE:float = 128.0
const NF_OFFSET_FALL_EASE_RATE:float = 192.0
const NF_OFFSET_LAND_EASE_RATE:float = 512.0
const NF_OFFSET_MAX_FALL_MULT:float = 2.5
var nf_offset:Vector2 = Vector2.ZERO
var nf_delay_timer:float = 0.0
#endregion
#endregion


func instantiate() -> void:
	player = GameCore.instance.player


func set_cam_mode(_state:CamStates = CamStates.NONE) -> void:
	if _state == CamStates.NONE:
		if ProjectSettings.get_setting("game/visuals/dynamic_camera"):
			state = CamStates.FOLLOW_NEW
		else:
			state = CamStates.FOLLOW_FLASH
	else:
		state = _state


func _process(delta):
	var pos = UICore.instance.position
	match state:
		CamStates.FOLLOW_FLASH:
			if player != null:
				pos = pos.lerp(player.position - offset, ease_rate * delta)
		CamStates.FOLLOW_NEW:
			pos = _tick_new_follow(pos, delta)
		CamStates.TARGET_POINT:
			pass
		CamStates.TARGET_ENTITY:
			pass
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


func _tick_new_follow(pos:Vector2, delta:float) -> Vector2:
	var move_vector:Vector2 = SInput.vector_move()
	var walled:bool = (
		player.gravity_dir == Statics.DirsSurface.LWALL or
		player.gravity_dir == Statics.DirsSurface.RWALL
	)
	
	if ((walled and move_vector.y != 0.0) or
	(not walled and move_vector.x != 0.0)):
		nf_delay_timer += delta
	else:
		nf_delay_timer = 0.0
	
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
	
	pos = pos.lerp(player.position - offset + nf_offset, ease_rate * delta)
	return pos


func set_layer_position(new_pos:Vector2):
	UICore.instance.position = new_pos - offset
