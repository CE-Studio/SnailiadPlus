# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name Seahorse
extends Enemy


#region Variables
const OUT_OF_WATER_TIMEOUT:float = 1.5

enum MoveMode {
	WAIT,
	COS_UP_LEFT,
	COS_DOWN_LEFT,
	COS_UP_RIGHT,
	COS_DOWN_RIGHT,
	TURN_UP_LEFT,
	TURN_DOWN_LEFT,
	TURN_UP_RIGHT,
	TURN_DOWN_RIGHT,
}
var mode:MoveMode = MoveMode.WAIT

var facing_left:bool = false
var theta:float = 0.0
var elapsed:float = 0.0
var move_origin:Vector2 = Vector2.ZERO
var radius:Vector2 = Vector2(70, 20)
var move_time:float = 1.8
var in_water:bool = false
var out_of_water_timeout:float = 0.0
#endregion


func _ready() -> void:
	my_type = EnemyTypes.SEAHORSE
	hitbox = $"Area2D"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	if hard_mode:
		radius = Vector2(130, 40)
		move_time = 1.3
	elapsed = move_time
	
	sprite.action = "right_idle"
	if display_mode and randf() < 0.5:
		sprite.action = "left_idle"
	theta = position.x * position.x * 1.1 + position.y * 3.2 + 0.7


func _process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	var player_pos:Vector2 = GameCore.instance.player.position
	
	if not in_water and environment and environment is WaterArea:
		in_water = true
	if in_water and environment and not environment.point_in_bounds(position + Vector2(0, -16)) and out_of_water_timeout <= 0.0:
		out_of_water_timeout = OUT_OF_WATER_TIMEOUT
		elapsed = 0.0
		move_origin = position
		if player_pos.x < position.x:
			mode = MoveMode.COS_DOWN_LEFT if facing_left else MoveMode.TURN_DOWN_RIGHT
			sprite.action = "left_swim_down" if facing_left else "right_turn_down"
			facing_left = true
		else:
			mode = MoveMode.TURN_DOWN_LEFT if facing_left else MoveMode.COS_DOWN_RIGHT
			sprite.action = "left_turn_down" if facing_left else "right_move_down"
			facing_left = false
	out_of_water_timeout -= delta
	
	if vis.is_on_screen():
		elapsed += delta
		_update_position()
		if elapsed >= move_time:
			elapsed = 0.0
			move_origin = position
			if player_pos.x < position.x:
				if facing_left:
					mode = MoveMode.COS_UP_LEFT if player_pos.y < position.y else MoveMode.COS_DOWN_LEFT
					sprite.action = "left_swim_up" if player_pos.y < position.y else "left_swim_down"
				else:
					mode = MoveMode.TURN_UP_RIGHT if player_pos.y < position.y else MoveMode.TURN_DOWN_RIGHT
					sprite.action = "right_turn_up" if player_pos.y < position.y else "right_turn_down"
					facing_left = true
			else:
				if not facing_left:
					mode = MoveMode.COS_UP_RIGHT if player_pos.y < position.y else MoveMode.COS_DOWN_RIGHT
					sprite.action = "right_swim_up" if player_pos.y < position.y else "right_swim_down"
				else:
					mode = MoveMode.TURN_UP_LEFT if player_pos.y < position.y else MoveMode.TURN_DOWN_LEFT
					sprite.action = "left_turn_up" if player_pos.y < position.y else "left_turn_down"
					facing_left = false


func _update_position() -> void:
	if mode == MoveMode.WAIT:
		return
	var lerp_val:float = Statics.normalized_sigmoid(elapsed / move_time)
	var move:Vector2 = Vector2.ZERO
	match mode:
		MoveMode.COS_UP_LEFT:
			move = Vector2(
				-radius.x * lerp_val,
				-radius.y * (1.0 - cos(lerp_val * PI))
			)
		MoveMode.COS_DOWN_LEFT:
			move = Vector2(
				-radius.x * lerp_val,
				radius.y * (1.0 - cos(lerp_val * PI))
			)
		MoveMode.COS_UP_RIGHT:
			move = Vector2(
				radius.x * lerp_val,
				-radius.y * (1.0 - cos(lerp_val * PI))
			)
		MoveMode.COS_DOWN_RIGHT:
			move = Vector2(
				radius.x * lerp_val,
				radius.y * (1.0 - cos(lerp_val * PI))
			)
		MoveMode.TURN_UP_LEFT:
			move = Vector2(
				-radius.y * sin(lerp_val * PI),
				-radius.y * (1.0 - cos(lerp_val * PI))
			)
		MoveMode.TURN_DOWN_LEFT:
			move = Vector2(
				-radius.y * sin(lerp_val * PI),
				radius.y * (1.0 - cos(lerp_val * PI))
			)
		MoveMode.TURN_UP_RIGHT:
			move = Vector2(
				radius.y * sin(lerp_val * PI),
				-radius.y * (1.0 - cos(lerp_val * PI))
			)
		MoveMode.TURN_DOWN_RIGHT:
			move = Vector2(
				radius.y * sin(lerp_val * PI),
				radius.y * (1.0 - cos(lerp_val * PI))
			)
	position = move_origin + move
