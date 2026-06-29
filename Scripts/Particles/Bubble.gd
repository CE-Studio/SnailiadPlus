# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
extends Particle


#region Variables
const INIT_VEL_DECREASE:float = 1.25
const BOUND_BUFFER:float = 4.0
const FAST_MULT:float = 4.0

var theta:float = 0.0
var up_speed:float = 0.0
var center_x:float = 0.0
var initial_vel:Vector2 = Vector2.ZERO
var despawn_at_home_top:bool = false
var home:CollisionShape2D = null
var fast:bool = false
#endregion

func _spawn(_data:Array) -> void:
	super(_data)
	up_speed = 4 + randf() * 16
	center_x = position.x
	if _data.size() != 0:
		initial_vel = _data[0]
		if _data.size() >= 2:
			despawn_at_home_top = _data[1]
		if _data.size() >= 3:
			fast = _data[2]
			if fast:
				up_speed *= FAST_MULT


func _process(delta: float) -> void:
	super(delta)
	theta += delta * (FAST_MULT if fast else 1.0)
	position = Vector2(center_x + (2.0 * sin(theta / 1.2)), position.y - (up_speed * delta))
	if home:
		var box_size = home.shape.size * 0.5
		if position.y < home.global_position.y - box_size.y + BOUND_BUFFER:
			if despawn_at_home_top:
				queue_free()
			else:
				position = Vector2(
					randf_range(-box_size.x, box_size.x),
					box_size.y - BOUND_BUFFER
				) + home.global_position
				center_x = position.x
				up_speed = 4 + randf() * 16
				if fast:
					up_speed *= FAST_MULT
	center_x += initial_vel.x
	position.y += initial_vel.y
	initial_vel = initial_vel.lerp(Vector2.ZERO, INIT_VEL_DECREASE * delta)
