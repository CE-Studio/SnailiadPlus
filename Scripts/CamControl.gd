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


func _process(delta):
	match state:
		CamStates.FOLLOW_FLASH:
			if player != null:
				position = position.lerp(player.position - offset, ease_rate * delta)
		CamStates.FOLLOW_NEW:
			pass
		CamStates.TARGET_POINT:
			pass
		CamStates.TARGET_ENTITY:
			pass
		_:
			pass
	if border != null:
		position = border.get_closest_point_to(position + offset) - offset


func set_layer_position(new_pos:Vector2):
	position = new_pos - offset
