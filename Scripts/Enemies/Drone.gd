class_name Drone
extends Enemy


#region Variables
const SHOT_TIMEOUT:float = 0.08
const SHOT_SPEED:float = 700.0
const SHOT_COUNT:int = 4
const DONUT_COUNT:int = 3

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
	ATTACK,
}
var mode:MoveMode = MoveMode.WAIT

var facing_left:bool = false
var theta:float = 0.0
var elapsed:float = 0.0
var move_origin:Vector2 = Vector2.ZERO
var radius:Vector2 = Vector2(130, 40)
var move_time:float = 2.2
var shot_timeout:float = 0.0
var shot_count:int = 0

@onready var laser:PackedScene = load("res://Scenes/Entities/Bullets/Enemy/EnemyBulletLaser.tscn")
@onready var donut:PackedScene = load("res://Scenes/Entities/Bullets/Enemy/EnemyBulletDonutRotary.tscn")
#endregion


func _ready() -> void:
	my_type = EnemyTypes.DRONE
	hitbox = $"Area2D"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	if hard_mode:
		radius = Vector2(110, 60)
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
	
	if vis.is_on_screen():
		if player_pos.x < position.x and not facing_left:
			facing_left = true
			sprite.action = "right_turn_up" if player_pos.y < position.y else "right_turn_down"
		elif player_pos.x > position.x and facing_left:
			facing_left = false
			sprite.action = "left_turn_up" if player_pos.y < position.y else "left_turn_down"
		
		if mode == MoveMode.ATTACK:
			shot_timeout -= delta
			if shot_timeout <= 0.0:
				shot_timeout = SHOT_TIMEOUT
				shot_count -= 1
				if shot_count <= 0:
					mode = MoveMode.WAIT
					_shoot_360_cluster_rotary(donut, Vector2(4.0, 0.0), 60.0, DONUT_COUNT)
				_shoot(laser, Vector2.LEFT if facing_left else Vector2.RIGHT, SHOT_SPEED)
			return
		
		elapsed += delta
		_update_position()
		if elapsed >= move_time and mode != MoveMode.WAIT:
			mode = MoveMode.ATTACK
			shot_count = SHOT_COUNT
		elif mode == MoveMode.WAIT:
			elapsed = 0.0
			move_origin = position
			if player_pos.x < position.x:
				if facing_left:
					mode = MoveMode.COS_UP_LEFT if player_pos.y < position.y else MoveMode.COS_DOWN_LEFT
					sprite.action = "left_fly_up" if player_pos.y < position.y else "left_fly_down"
				else:
					mode = MoveMode.TURN_UP_RIGHT if player_pos.y < position.y else MoveMode.TURN_DOWN_RIGHT
					sprite.action = "right_turn_up" if player_pos.y < position.y else "right_turn_down"
					facing_left = true
			else:
				if not facing_left:
					mode = MoveMode.COS_UP_RIGHT if player_pos.y < position.y else MoveMode.COS_DOWN_RIGHT
					sprite.action = "right_fly_up" if player_pos.y < position.y else "right_fly_down"
				else:
					mode = MoveMode.TURN_UP_LEFT if player_pos.y < position.y else MoveMode.TURN_DOWN_LEFT
					sprite.action = "left_turn_up" if player_pos.y < position.y else "left_turn_down"
					facing_left = false


func _update_position() -> void:
	if mode == MoveMode.WAIT or mode == MoveMode.ATTACK:
		return
	var lerp_val:float = _normalized_sigmoid(elapsed / move_time)
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


func _normalized_sigmoid(val:float) -> float:
	return 1.0 / (1.0 + exp(-(val * 12.0 - 6.0)))
