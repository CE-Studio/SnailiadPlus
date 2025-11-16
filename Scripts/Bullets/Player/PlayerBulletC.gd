extends PlayerBullet


const RETURN_THRESHOLD:float = 1.5
var is_returning:bool = false
var player:Player


func _spawn(dir:Vector2, rapid_shot:float) -> float:
	if dir == Vector2.ZERO:
		dir = Vector2.RIGHT
	super._spawn(dir, rapid_shot)
	velocity = 330
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
	sprite.action = anim_name
	sprite._process(0.0)
	#endregion
	player = GameCore.instance.player
	return 9999


func _physics_process(delta: float) -> void:
	if is_returning:
		position = position.move_toward(player.position, velocity * delta)
		velocity += velocity_init * 1.5 * delta
		if position.distance_to(player.position) <= RETURN_THRESHOLD:
			player.fire_cooldown = cooldown
			despawn()
	else:
		position += velocity * normalized_dir * delta
		velocity -= velocity_init * 1.5 * delta
		if velocity <= 0.0:
			is_returning = true
			velocity *= -1.0
	super(delta)
