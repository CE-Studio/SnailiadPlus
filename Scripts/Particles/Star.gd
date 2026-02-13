extends Particle


const CENTER_MIN:float = 8.0

var speed:float = randf() * 100.0 + 10.0
var mod:float = 1.0
var direction:Vector2 = Vector2.LEFT
var border_mode:int = 0 # 0 - no special mode, 1 - inward from border, 2 - outward to border


func _spawn(_data:Array) -> void:
	var option:int = ProjectSettings.get_setting("game/world/particles")
	if not (option == Statics.ParticleOptions.ENVIRONMENTS
	or option == Statics.ParticleOptions.FLASH
	or option == Statics.ParticleOptions.ALL):
		queue_free()
	else:
		if _data.size() > 0 and _data[0] is int:
			match _data[0]:
				0: direction = Vector2.UP
				1: direction = Statics.VECTOR_DIAG * Vector2(1, -1)
				2: direction = Vector2.RIGHT
				3: direction = Statics.VECTOR_DIAG
				4: direction = Vector2.DOWN
				5: direction = Statics.VECTOR_DIAG * Vector2(-1, 1)
				6: direction = Vector2.LEFT
				7: direction = Statics.VECTOR_DIAG * -1
				8: border_mode = 1
				9:
					border_mode = 2
		if _data.size() > 1 and _data[1] is float:
			mod = _data[1]
		super(_data)


func _process(delta: float) -> void:
	var center:Vector2 = Statics.VECTOR_CENTER
	#if UICore.instance:
	#	center = UICore.instance.get_cam_center_pos()
	match border_mode:
		1:
			position = position.move_toward(center, speed * mod * delta)
			if position.distance_to(center) <= CENTER_MIN:
				var new_pos:Vector2 = Vector2(
					randf_range(-1.0, 1.0),
					-1 if randf() < 0.5 else 1
				)
				if randf() < 0.5:
					new_pos = Vector2(new_pos.y, new_pos.x)
				new_pos *= Vector2(216, 136)
				position = center + new_pos
		2:
			if absf(position.x - center.x) > 216 or absf(position.y - center.y) > 136:
				position = center
				var theta:float = randf() * TAU
				direction = Vector2(cos(theta), sin(theta))
			else:
				direction = position.direction_to(center) * -1
			position += direction * speed * mod * delta
		_:
			position += direction * speed * mod * delta
	#if UICore.instance:
	#	position += UICore.instance.get_cam_movement_this_tick()
