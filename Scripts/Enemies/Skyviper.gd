# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name Skyviper
extends Enemy


#region Variables
const MOVE_TIMEOUTS:Array[float] = [
	1.2, 1.3, 1.4, 1.1, 1.6, 1.0, 1.8, 0.9, 1.9, 2.1, 0.9, 1.3,
	1.7, 1.4, 2.1, 1.2, 0.9, 0.8, 1.2, 1.3, 1.4, 0.2, 1.6, 1.0,
	1.8, 0.4, 1.9, 2.1, 0.9, 0.7, 1.7, 1.2, 2.3, 1.1, 0.9, 0.8
]
const THETA_OFFSETS:Array[float] = [
	0.0, PI, 0.0, PI * 0.5, 0.0, 0.0, PI * -0.5,
	0.0, 0.0, PI * -0.25, PI * 0.25, 0.0
]
const REACT_DISTANCE:float = 390.0
const SHOT_TIMEOUT:float = 1.2
const WEAPON_SPEED:float = 80.0
const RETURN_SPEED:float = 20.0

var SPEED:Vector2 = Vector2(240.0, 190.0)
var DECEL:Vector2 = Vector2.ZERO
var move_timeout:float = 0.0
var move_timeout_index:int = 0
var theta_offset_index:int = 0
var shot_timeout:float = SHOT_TIMEOUT
var facing_left:bool = false

@onready var sfx_move:AudioStreamPlayer = $"Move"
@onready var donut:PackedScene = load("res://Scenes/Entities/Bullets/Enemy/EnemyBulletDonutLinear.tscn")
#endregion


func _ready() -> void:
	my_type = EnemyTypes.SKYVIPER
	col = $"BodyBox"
	hitbox = $"Area2D"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	if easy_mode:
		SPEED.y = 120.0
	DECEL = SPEED * Vector2(0.7, 0.6)
	theta_offset_index = absi(roundi(position.x / 16.0 + position.y / 4.0)) % THETA_OFFSETS.size()
	move_timeout_index = absi(roundi(position.x / 16.0 + position.y / 4.0)) % MOVE_TIMEOUTS.size()
	move_timeout = MOVE_TIMEOUTS[move_timeout_index] * 0.25
	
	facing_left = randf() <= 0.5
	_play_anim()


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	if vis.is_on_screen():
		move_timeout -= delta
		shot_timeout -= delta
		var player_pos:Vector2 = GameCore.instance.player.position
		if abs(player_pos.x - position.x) <= REACT_DISTANCE and abs(player_pos.y - position.y) <= REACT_DISTANCE:
			if move_timeout <= 0.0:
				var aim:float = atan2(player_pos.y - position.y, player_pos.x - position.x)
				aim += THETA_OFFSETS[theta_offset_index]
				theta_offset_index = (theta_offset_index + 1) % THETA_OFFSETS.size()
				velocity = Vector2(cos(aim) * SPEED.x, sin(aim) * SPEED.y)
				move_timeout_index = (move_timeout_index + 1) % MOVE_TIMEOUTS.size()
				move_timeout = MOVE_TIMEOUTS[move_timeout_index]
				facing_left = velocity.x < 0.0
				_play_anim(true)
				sfx_move.play()
			if shot_timeout <= 0.0 and hard_mode:
				shot_timeout = SHOT_TIMEOUT
				var aim:float = atan2(player_pos.y - position.y, player_pos.x - position.x)
				_shoot(donut, Vector2(cos(aim), sin(aim)), WEAPON_SPEED)
	
	var last_vel:Vector2 = velocity
	move_and_slide()
	if is_on_wall() and last_vel.x != 0.0:
		velocity.x = -last_vel.x
		facing_left = not facing_left
		_play_anim(true)
	if (is_on_floor() or is_on_ceiling()) and last_vel.y != 0.0:
		velocity.y = -last_vel.y
	
	velocity = Vector2(
		move_toward(velocity.x, 0.0, DECEL.x * delta),
		move_toward(velocity.y, 0.0, DECEL.y * delta)
	)
	if abs(velocity.x) < RETURN_SPEED:
		_play_anim(false)


func _play_anim(mode:int = 0) -> void:
	var anim_name:String = "left_" if facing_left else "right_"
	#anim_name += "move" if moving else "idle"
	if mode > 0:
		anim_name += "move_high"
	elif mode < 0:
		anim_name += "move_low"
	else:
		anim_name += "idle"
	if sprite.action != anim_name:
		sprite.action = anim_name
