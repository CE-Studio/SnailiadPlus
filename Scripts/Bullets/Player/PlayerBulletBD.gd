extends PlayerBullet


func _spawn(dir:Vector2, rapid_shot:float, power_shot:bool) -> float:
	super._spawn(dir, rapid_shot, power_shot)
	velocity = 1.2
	velocity_init = 1.2
	damage = 68 if power_shot else 30
	cooldown = 0.085 if (rapid_shot > 1.0) else 0.17
	return cooldown


func _process(delta: float) -> void:
	pass
