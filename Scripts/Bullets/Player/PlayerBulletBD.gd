extends PlayerBullet


func _spawn(dir:Vector2, rapid_shot:float, power_shot:bool) -> float:
	if dir == Vector2.ZERO:
		dir = Vector2.RIGHT
	super._spawn(dir, rapid_shot, power_shot)
	type = 10
	velocity = 1.2
	velocity_init = 1.2
	damage = 68 if power_shot else 30
	cooldown = 0.085 if (rapid_shot > 1.0) else 0.17
	despawn_offscreen = true
	#region Direction animation
	var anim_name = "power_" if power_shot else "normal_"
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
	return cooldown


func _process(delta: float) -> void:
	position += velocity * normalized_dir
	velocity += velocity_init * 18.0 * delta
	super._process(delta)
