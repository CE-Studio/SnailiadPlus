# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name FloatspikeCommon
extends Enemy


var theta = 0.0


func _ready() -> void:
	my_type = EnemyTypes.FLOATSPIKE
	hitbox = $"Area2D"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	sprite.action = "idle"
	theta = position.x * position.x * 1.1 + position.y * 3.2 + 0.7


func _process(delta: float) -> void:
	super(delta)
	if not ai_active and not display_mode:
		return
	
	theta += delta
	position.y = origin.y + sin(theta) * 1.8
