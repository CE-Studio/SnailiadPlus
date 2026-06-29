# Copyright 2026 CE-Studio: AGPL-3.0-only
extends Particle


#region Variables
const SPEED:Vector2 = Vector2(14.0, 8.0)
const TURNAROUND_SPEED:float = 12.0
const MAX_FLAPS:int = 12
const MAX_GLIDE_TIME:float = 4.0
const MIN_TURNAROUND_TIME:float = 6.0
const MAX_TURNAROUND_TIME:float = 20.0
const MAX_DIR_CHANGE_TIME:float = 5.0
const RISE_FALL_MOD:float = 4.0

var origin:Vector2 = Vector2.ZERO
var bounds:Vector2i = Vector2i(77, 47)
var velocity:Vector2 = Vector2.ZERO
var flaps:int = 0
var glide_timeout:float = 0.0
var turnaround_timeout:float = 0.0
var dir_change_timeout:float = 0.0
var left:bool = false
var y_cycle:float = 0.0
#endregion


func _spawn(_data:Array) -> void:
	super(_data)
	if _data.size() > 0:
		bounds = _data[0]
		origin = position
		if _data.size() > 1:
			position = _data[1]
	glide_timeout = randf() * MAX_GLIDE_TIME
	turnaround_timeout = randf_range(MIN_TURNAROUND_TIME, MAX_TURNAROUND_TIME)
	left = randf() > 0.5
	sprite.animation_finished.connect(_on_sprite_anim_finished)
	_play_anim("default")
	velocity = Vector2(SPEED.x * (-1 if left else 1), 0.0)
	dir_change_timeout = randf() * MAX_DIR_CHANGE_TIME
	y_cycle = randf() * TAU


func _process(delta: float) -> void:
	var bound_state:Vector2i = _in_bounds()
	if bound_state.x == 0:
		turnaround_timeout -= delta
		if turnaround_timeout <= 0.0:
			turnaround_timeout = randf_range(MIN_TURNAROUND_TIME, MAX_TURNAROUND_TIME)
			left = not left
			_play_anim("turn")
	else:
		left = false if bound_state.x == -1 else true
	
	var set_flaps:bool = false
	if bound_state.y == 0:
		dir_change_timeout -= delta
		if dir_change_timeout <= 0.0:
			dir_change_timeout = randf() * MAX_DIR_CHANGE_TIME
			if velocity.y == 0.0:
				velocity.y = roundi(randf_range(-1.25, 1.25)) * SPEED.y
			else:
				velocity.y = 0.0
			set_flaps = true
	else:
		velocity.y = (1 if bound_state.y == -1 else -1)
		set_flaps = true
	if set_flaps:
		if velocity.y < 0:
			flaps = 99
			_play_anim("flap")
		else:
			flaps = 0
	
	if (velocity.y == 0.0
	and bound_state == Vector2i.ZERO
	and flaps == 0
	and abs(velocity.x) == SPEED.x):
		glide_timeout -= delta
		if glide_timeout <= 0.0:
			glide_timeout = randf() * MAX_GLIDE_TIME
			flaps = randi_range(1, MAX_FLAPS)
			_play_anim("flap")
	
	velocity.x = clampf(
		velocity.x + (TURNAROUND_SPEED * (-1 if left else 1) * delta),
		-SPEED.x, SPEED.x
	)
	position += velocity * delta
	sprite.position.y = sin(y_cycle) * 3
	y_cycle += delta


func _in_bounds() -> Vector2i:
	var output:Vector2i = Vector2i.ZERO
	if position.x < origin.x - bounds.x:
		output.x = -1
	elif position.x > origin.x + bounds.x:
		output.x = 1
	if position.y < origin.y - bounds.y:
		output.y = -1
	elif position.y > origin.y + bounds.y:
		output.y = 1
	return output


func _play_anim(anim:String) -> void:
	sprite.play(anim)
	sprite.flip_h = left


func _on_sprite_anim_finished() -> void:
	if flaps > 0:
		_play_anim("flap")
		flaps -= 1
	else:
		_play_anim("default")
