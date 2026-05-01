# Copyright 2026 CE-Studio: AGPL-3.0-only
extends Particle


var direction:Vector2 = Vector2.ZERO


# Data accepted: [ Direction (Vector2), Size (int = -1), Speed (int = -1)
func _spawn(_data:Array) -> void:
	assert(_data.size() > 0, "DotGeneric spawn data array must contain data.")
	assert(_data[0] is Vector2, "DotGeneric spawn data array's first entry must be a Vector2.")
	
	direction = _data[0]
	var size:int = -1
	var speed:int = -1
	if _data.size() >= 2 and _data[1] is int:
		size = _data[1]
	if _data.size() >= 3 and _data[2] is int:
		speed = _data[2]
	if size == -1: size = randi() % 3
	if speed == -1: speed = randi() % 3
	var duration:float
	match size:
		0:
			anim_name = "small_"
			duration = 2
		1:
			anim_name = "med_"
			duration = 3
		2:
			anim_name = "big_"
			duration = 4
	match speed:
		0:
			anim_name += "slow"
			duration /= 1.5
		1:
			anim_name += "med"
			duration /= 1.85
		2:
			anim_name += "fast"
			duration /= 2.25
	timer.wait_time = duration
	super._spawn(_data)


func _process(delta: float) -> void:
	position += direction * delta
	super._process(delta)
