extends PlayerBullet


func _spawn(dir:Vector2, rapid_shot:float, power_shot:bool) -> float:
	if dir == Vector2.ZERO:
		dir = Vector2.RIGHT
	super._spawn(dir, rapid_shot, power_shot)
	velocity = 480 * rapid_shot
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
	position += velocity * normalized_dir * delta
	velocity -= velocity_init * 1.5 * delta
	super(delta)
