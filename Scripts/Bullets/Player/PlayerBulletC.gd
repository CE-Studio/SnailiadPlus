# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
extends PlayerBullet


const RETURN_THRESHOLD:float = 1.5
## Set to true when this bullet starts to return to the player
var is_returning:bool = false
## Player
var player:Player


func _spawn(dir:Vector2, rapid_shot:float, power_shot:bool) -> float:
	if dir == Vector2.ZERO:
		dir = Vector2.RIGHT
	super._spawn(dir, rapid_shot, power_shot)
	velocity = 330 * rapid_shot
	velocity_init = velocity
	cooldown /= rapid_shot
	_infer_direction_anim()
	player = GameCore.instance.player
	player.return_bullet = self
	return cooldown


func _physics_process(delta: float) -> void:
	if is_returning:
		position = position.move_toward(player.position, velocity * delta)
		velocity += velocity_init * 1.5 * delta
		if position.distance_to(player.position) <= RETURN_THRESHOLD:
			player.return_bullet = null
			despawn()
	else:
		position += velocity * normalized_dir * delta
		velocity -= velocity_init * 1.5 * delta
		if velocity <= 0.0:
			is_returning = true
			velocity *= -1.0
	super(delta)
