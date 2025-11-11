extends EnemyBullet


func _spawn(dir:Vector2, speed:float, play_sound:bool = true) -> void:
	if dir == Vector2.ZERO:
		dir = Vector2.RIGHT
	super._spawn(dir, speed, play_sound)
	velocity = speed
	velocity_init = speed
	#region Direction animation
	var anim_name = ""
	if dir.y < -0.3827:
		anim_name += "U"
	elif dir.y > 0.3827:
		anim_name += "D"
	if dir.x < -0.3827:
		anim_name += "L"
	elif dir.x > 0.3827:
		anim_name += "R"
	sprite.action = anim_name
	sprite._process(0.0)
	#endregion


func _process(delta: float) -> void:
	position += velocity * normalized_dir * delta
	super._process(delta)
