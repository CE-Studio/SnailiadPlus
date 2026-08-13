# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name FloatspikeCommon
extends Enemy


var theta = 0.0
var blink_timeout:float = randf_range(1.2, 6.0)


func _ready() -> void:
	my_type = EnemyTypes.FLOATSPIKE
	hitbox = $"Area2D"
	sprite = $"SnailySprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	if display_mode:
		theta = randf() * TAU
	else:
		theta = position.x * position.x * 1.1 + position.y * 3.2 + 0.7


func _process(delta: float) -> void:
	super(delta)
	if not ai_active and not display_mode:
		return
	
	theta += delta
	sprite.position.y = sin(theta) * 1.8
	blink_timeout -= delta
	if blink_timeout <= 0:
		blink_timeout = randf_range(1.2, 6.0)
		sprite.play("blink")
		sprite.autoplay_next = "idle"
