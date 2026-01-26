extends PlayerBullet


func _spawn(dir:Vector2, rapid_shot:float, power_shot:bool) -> float:
	if dir == Vector2.ZERO:
		dir = Vector2.RIGHT
	if power_shot:
		collide_with_wall = false
		single_hit = false
	super._spawn(dir, rapid_shot, power_shot)
	velocity = 370 * rapid_shot
	velocity_init = velocity
	cooldown /= rapid_shot
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
	if power_shot:
		anim_name += "_power"
	sprite.action = anim_name
	sprite._process(0.0)
	#endregion
	return cooldown


func _physics_process(delta: float) -> void:
	position += velocity * normalized_dir * delta * (1.5 if powered else 1.0)
	super(delta)
