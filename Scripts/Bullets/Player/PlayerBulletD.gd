# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
extends PlayerBullet


func _spawn(dir:Vector2, rapid_shot:float, power_shot:bool) -> float:
	if dir == Vector2.ZERO:
		dir = Vector2.RIGHT
	super._spawn(dir, rapid_shot, power_shot)
	velocity = 270 * rapid_shot
	velocity_init = velocity
	cooldown /= rapid_shot
	_infer_direction_anim()
	return cooldown


func _physics_process(delta: float) -> void:
	position += velocity * normalized_dir * delta
	super(delta)
