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
	#region Rotate hitbox
	if sprite.animation == "UR" or sprite.animation == "DL":
		area.rotation_degrees = -45
	elif sprite.animation == "UL" or sprite.animation == "DR":
		area.rotation_degrees = 45
	elif sprite.animation == "U" or sprite.animation == "D":
		area.rotation_degrees = 90
	#endregion


func _physics_process(delta: float) -> void:
	position += velocity * normalized_dir * delta
	super(delta)


func _flip_sprite_from_dir(_angle:Vector2 = normalized_dir) -> void:
	sprite.flip_x = _angle.x < 0.0
	sprite.flip_y = _angle.y < 0.0
