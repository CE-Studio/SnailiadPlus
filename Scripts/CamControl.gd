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
			pass
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


func set_layer_position(new_pos:Vector2):
	UICore.instance.position = new_pos - offset
