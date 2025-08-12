extends EnemyBullet


func _spawn(dir:Vector2, speed:float) -> void:
	if dir == Vector2.ZERO:
		dir = Vector2.RIGHT
	super._spawn(dir, speed)
	velocity = speed
	velocity_init = speed
	damage = 2
	max_life_time = 3.6
	despawn_offscreen = true
	collide_with_world = true
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
