# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
extends EnemyBullet


func _spawn(dir:Vector2, speed:float, play_sound:bool = true) -> void:
	if dir == Vector2.ZERO:
		dir = Vector2.RIGHT
	super._spawn(dir, speed, play_sound)
	velocity = speed
	velocity_init = speed
	#region Direction animation
	var anim_name = ""
	if abs(dir.x) > abs(dir.y):
		anim_name = "R" if dir.x > 0.0 else "L"
	else:
		anim_name = "D" if dir.y > 0.0 else "U"
		box.rotation_degrees = 90.0
	sprite.action = anim_name
	sprite._process(0.0)
	#endregion


func _physics_process(delta: float) -> void:
	position += velocity * normalized_dir * delta
	velocity += velocity_init * 18.0 * delta
	super(delta)
