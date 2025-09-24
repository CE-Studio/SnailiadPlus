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
var state:CamStates = CamStates.FOLLOW_FLASH
var target_point:Vector2 = Vector2.ZERO
var target_entity:Node2D
var player:Player
var ease_rate:float = 4.0
var offset:Vector2 = Vector2(200, 120)
var border:CameraBorder = null

#region New follow vars
const NF_OFFSET_MAX:Vector2 = Vector2(48.0, 32.0)
const NF_OFFSET_ADJUST_DELAY:float = 0.75
const NF_OFFSET_EASE_RATE:float = 32.0
const NF_OFFSET_MAX_FALL_MULT:Vector2 = Vector2(0.5, 2.0)
var nf_offset:Vector2 = Vector2.ZERO
var nf_delay_timer:float = 0.0
#endregion
#endregion


func instantiate() -> void:
	player = GameCore.instance.player


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
	if move_vector != Vector2.ZERO:
		nf_delay_timer += delta
	else:
		nf_delay_timer = 0.0
	
	var walled:bool = (
		player.gravity_dir == Statics.DirsSurface.LWALL or
		player.gravity_dir == Statics.DirsSurface.RWALL
	)
	if player.grounded:
		if nf_delay_timer >= NF_OFFSET_ADJUST_DELAY:
			if walled:
				nf_offset.y = clampf(
					nf_offset.y + (NF_OFFSET_EASE_RATE * delta * move_vector.y),
					-NF_OFFSET_MAX.y, NF_OFFSET_MAX.y
				)
			else:
				nf_offset.x = clampf(
					nf_offset.x + (NF_OFFSET_EASE_RATE * delta * move_vector.x),
					-NF_OFFSET_MAX.x, NF_OFFSET_MAX.x
				)
	else:
		match player.gravity_dir:
			Statics.DirsSurface.FLOOR:
				nf_offset.y = clampf(
					nf_offset.y + (NF_OFFSET_EASE_RATE * delta),
					-INF, NF_OFFSET_MAX.y * NF_OFFSET_MAX_FALL_MULT.y
				)
				nf_offset.x = NF_OFFSET_MAX.x * NF_OFFSET_MAX_FALL_MULT.x * move_vector.x
			Statics.DirsSurface.LWALL:
				nf_offset.x = clampf(
					nf_offset.x - (NF_OFFSET_EASE_RATE * delta),
					-INF, NF_OFFSET_MAX.x * NF_OFFSET_MAX_FALL_MULT.x
				)
				nf_offset.y = NF_OFFSET_MAX.y * NF_OFFSET_MAX_FALL_MULT.y * move_vector.y
			Statics.DirsSurface.RWALL:
				nf_offset.x = clampf(
					nf_offset.x + (NF_OFFSET_EASE_RATE * delta),
					-INF, NF_OFFSET_MAX.x * NF_OFFSET_MAX_FALL_MULT.x
				)
				nf_offset.y = NF_OFFSET_MAX.y * NF_OFFSET_MAX_FALL_MULT.y * move_vector.y
			Statics.DirsSurface.CEILING:
				nf_offset.y = clampf(
					nf_offset.y - (NF_OFFSET_EASE_RATE * delta),
					-INF, NF_OFFSET_MAX.y * NF_OFFSET_MAX_FALL_MULT.y
				)
				nf_offset.x = NF_OFFSET_MAX.x * NF_OFFSET_MAX_FALL_MULT.x * move_vector.x
	
	pos = pos.lerp(player.position - offset + nf_offset, ease_rate * delta)
	return pos


func set_layer_position(new_pos:Vector2):
	UICore.instance.position = new_pos - offset
