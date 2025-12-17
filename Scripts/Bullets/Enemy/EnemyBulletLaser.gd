extends EnemyBullet


func _spawn(dir:Vector2, speed:float, play_sound:bool = true) -> void:
	if dir == Vector2.ZERO:
		dir = Vector2.RIGHT
	super._spawn(dir, speed, play_sound)
	velocity = speed
	velocity_init = speed
	#region Direction animation
	var anim_name:String = ""
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
	#region Rotate hitbox
	if anim_name == "UR" or anim_name == "DL":
		area.rotation_degrees = -45
	elif anim_name == "UL" or anim_name == "DR":
		area.rotation_degrees = 45
	elif anim_name == "U" or anim_name == "D":
		area.rotation_degrees = 90
	#endregion


func _process(delta: float) -> void:
	position += velocity * normalized_dir * delta
	super._process(delta)
