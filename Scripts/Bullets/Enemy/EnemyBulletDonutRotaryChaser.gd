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

## The rate at which this bullet's travel angle changes
var TURN_SPEED:float = 0.2
## The rate at which this bullet's travel speed increases
var ACCELERATION:float = 150.0
## The current travel speed
var current_speed:float = 0.0
## The current travel angle
var move_theta:float = 0.0
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
	
	if Statics.current_profile["difficulty"] == 2:
		TURN_SPEED = 0.4
		ACCELERATION = 170.0
	move_theta = atan2(
		Player.instance.position.y - origin.y,
		Player.instance.position.x - origin.x
	)


func _physics_process(delta: float) -> void:
	elapsed += delta
	var last_pos:Vector2 = position - origin
	_update_origin(delta)
	position = origin + vel_radius * elapsed * Vector2(
		cos(elapsed * PI * vel_theta + theta_offset),
		sin(elapsed * PI * vel_theta + theta_offset)
	)
	_update_anim(last_pos + origin)
	super(delta)


func parry_reshoot() -> void:
	origin = position
	elapsed = 0.0
	current_speed = 0.0
	move_theta = 0.0
	super()


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


func _update_origin(delta:float) -> void:
	var target:Vector2 = source_enemy.position if has_been_parried else Player.instance.position
	var this_angle:float = atan2(
		target.y - origin.y,
		target.x - origin.x
	)
	var difference = this_angle - move_theta
	while difference > PI:
		difference -= TAU
	while difference < -PI:
		difference += TAU
	move_theta += PI * delta * TURN_SPEED * (-1.0 if difference < 0.0 else 1.0)
	
	current_speed += ACCELERATION * delta
	origin += current_speed * delta * Vector2(cos(move_theta), sin(move_theta))
