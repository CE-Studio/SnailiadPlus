# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name Jellyfish
extends Enemy


const RADIUS:float = 30.0

var theta = 0.0


func _ready() -> void:
	my_type = EnemyTypes.JELLYFISH
	super.spawn()
	
	theta = position.x * 0.7 + position.y * 1.3
	_process(0.0)


func _process(delta: float) -> void:
	super(delta)
	if not ai_active or display_mode:
		return
	
	theta += delta
	position = Vector2(
		origin.x + RADIUS * sin(theta * 1.2) + sin(theta * 12.0) * 0.3,
		origin.y - RADIUS * cos(theta * 1.2) - cos(theta * 12.0)
	)
