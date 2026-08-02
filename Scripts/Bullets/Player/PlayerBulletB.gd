# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
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
	_infer_direction_anim()
	return cooldown


func _physics_process(delta: float) -> void:
	position += velocity * normalized_dir * delta * (1.5 if powered else 1.0)
	super(delta)
