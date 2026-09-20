# Copyright 2026 CE-Studio: AGPL-3.0-only
extends Particle


const CENTER_MIN:float = 8.0

var speed:float = randf() * 35.0 + 10.0
var mod:float = 1.0
var direction:Vector2 = Vector2.LEFT


func _spawn(_data:Array) -> void:
	var option:int = ProjectSettings.get_setting("game/world/particles")
	if not (option == Statics.ParticleOptions.ENVIRONMENTS
	or option == Statics.ParticleOptions.ALL):
		queue_free()
	else:
		if _data.size() > 0 and _data[0] is Vector2:
			direction = _data[0]
		if _data.size() > 1 and _data[1] is float:
			mod = _data[1]
		if abs(direction.x) > abs(direction.y):
			sprite.play("horizontal")
		else:
			sprite.play("vertical")
		super(_data)


func _process(delta: float) -> void:
	position += direction * speed * mod * delta
