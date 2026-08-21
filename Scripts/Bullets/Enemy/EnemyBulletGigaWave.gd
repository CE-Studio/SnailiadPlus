# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
extends EnemyBullet


func _spawn(dir:Vector2, speed:float, play_sound:bool = true) -> void:
	if dir == Vector2.ZERO:
		dir = Vector2.RIGHT
	super._spawn(dir, speed, play_sound)
	velocity = speed
	velocity_init = speed
	_infer_direction_anim()


func _physics_process(delta: float) -> void:
	position += velocity * normalized_dir * delta
	velocity += velocity_init * 18.0 * delta
	super(delta)


func _infer_direction_anim(_angle:Vector2 = normalized_dir) -> void:
	if abs(_angle.x) > abs(_angle.y):
		sprite.play("R" if _angle.x > 0.0 else "L")
		box.rotation_degrees = 0.0
	else:
		sprite.play("D" if _angle.y > 0.0 else "U")
		box.rotation_degrees = 90.0
	_flip_sprite_from_dir(_angle)


func _flip_sprite_from_dir(_angle:Vector2 = normalized_dir) -> void:
	sprite.flip_h = _angle.x < 0.0
	sprite.flip_v = _angle.y > 0.0
