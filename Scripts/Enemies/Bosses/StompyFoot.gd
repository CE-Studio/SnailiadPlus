@tool
class_name StompyFoot
extends Enemy


@export var left:bool = false:
	set(value):
		left = value
		$"Area2D/HitBox".scale.x = -1 if value else 1
		if Engine.is_editor_hint():
			$"JsonSprite2D/MarkerSprite".flip_h = value


func _ready() -> void:
	if Engine.is_editor_hint():
		return
	
	my_type = EnemyTypes.NONE
	sprite = $"JsonSprite2D"
	hitbox = $"Area2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	sprite.action = "p0_left_up" if left else "p0_right_up"
