# Copyright 2026 CE-Studio: AGPL-3.0-only
extends Particle


var direction:Vector2 = Vector2.ZERO


# Data accepted: [ Direction (Vector2), Size (int = -1)
func _spawn(_data:Array) -> void:
	assert(_data.size() > 0, "DotGeneric spawn data array must contain data.")
	assert(_data[0] is Vector2, "DotGeneric spawn data array's first entry must be a Vector2.")
	
	direction = _data[0]
	var size:int = -1
	if _data.size() >= 2 and _data[1] is int:
		size = _data[1]
	if size == -1: size = randi() % 3
	match size:
		0: sprite.play("fade_small")
		1: sprite.play("fade_med")
		2: sprite.play("fade_big")
	super._spawn(_data)


func _process(delta: float) -> void:
	position += direction * delta
	super._process(delta)
