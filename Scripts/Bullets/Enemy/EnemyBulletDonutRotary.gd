# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
extends EnemyBullet


#region Variables
## How much time has passed since this bullet was spawned
var elapsed:float = 0.0
## The position at which this bullet was spawned
var origin:Vector2 = Vector2.ZERO
## The speed at which this bullet travels outward from the origin
var vel_radius:float = 0.0
## The speed at which this bullet rotates around the origin
var vel_theta:float = 0.0
## The initial offset applied to the theta at spawn
var theta_offset:float = 0.0
#endregion


func _spawn(dir:Vector2, speed:float, play_sound:bool = true) -> void:
	if dir == Vector2.ZERO:
		dir = Vector2.RIGHT
	super._spawn(dir, speed, play_sound)
	origin = position
	vel_radius = speed
	vel_theta = dir.x
	theta_offset = dir.y
	#region Direction animation
	var angle_vector:Vector2 = Vector2(cos(dir.y), sin(dir.y))
	var anim_name = ""
	if angle_vector.y < -0.3827:
		anim_name += "U"
	elif angle_vector.y > 0.3827:
		anim_name += "D"
	if angle_vector.x < -0.3827:
		anim_name += "L"
	elif angle_vector.x > 0.3827:
		anim_name += "R"
	anim_name += "_rotary"
	sprite.action = anim_name
	sprite._process(0.0)
	#endregion


func _physics_process(delta: float) -> void:
	elapsed += delta
	var last_pos:Vector2 = position
	position = origin + vel_radius * elapsed * Vector2(
		cos(elapsed * vel_theta + theta_offset),
		sin(elapsed * vel_theta + theta_offset)
	)
	_update_anim(last_pos)
	super(delta)


func _update_anim(last_pos:Vector2) -> void:
	var dir = last_pos.direction_to(position)
	var anim_name = ""
	if dir.y < -0.3827:
		anim_name += "U"
	elif dir.y > 0.3827:
		anim_name += "D"
	if dir.x < -0.3827:
		anim_name += "L"
	elif dir.x > 0.3827:
		anim_name += "R"
	anim_name += "_rotary"
	if sprite.action != anim_name:
		sprite.action = anim_name
