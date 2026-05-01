# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
extends Particle


var speed:Vector2 = Vector2(
	40.0,
	30.0 + randf() * 60.0
)
var elapsed:float = randf() * TAU


func _ready() -> void:
	var option:int = ProjectSettings.get_setting("game/world/particles")
	if not (option == Statics.ParticleOptions.ENVIRONMENTS
	or option == Statics.ParticleOptions.FLASH
	or option == Statics.ParticleOptions.ALL):
		queue_free()


func _process(delta: float) -> void:
	elapsed += delta
	position += Vector2(
		(sin(elapsed * 4.0) - 1.0) * speed.x,
		speed.y
	) * delta
