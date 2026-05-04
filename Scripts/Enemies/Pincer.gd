# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name Pincer
extends Enemy


#region Variables
const MOVE_TIMEOUTS:Array = [ 0.4, 0.3, 0.4, 0.2, 0.4, 0.3, 0.4, 0.2, 0.4, 0.3, 0.4, 0.2, 0.2, 0.2, 0.2, 0.1, 0.4 ]
const JUMP_HEIGHTS:Array = [ 1.0, 1.0, 1.0, 1.0, 2.0, 1.0, 1.0, 1.0, 2.0, 1.0, 1.0, 1.0, 0.5, 0.5, 0.5, 0.0, 2.5 ]
const GRAVITY:float = 1200.0
const VEL_X:float = 200.0
const DECEL:float = 400.0
const JUMP_VEL_BASE:float = -130
const REACT_DISTANCE:float = 540.0
const SHOT_TIMEOUT:float = 0.4
const WEAPON_SPEED:float = 80.0
const JUMP_CHANCE:float = 0.9
const RAND_DIR_CHANCE:float = 0.77

var facing_right:bool = false
var timeout_ptr:int = 0
var move_timeout:float = 0.0
var shot_timeout:float = SHOT_TIMEOUT
var pouncing:bool = false
var grounded:bool = true
@export var direction:Statics.DirsSurface = Statics.DirsSurface.FLOOR
var rel_velocity:Vector2 = Vector2.ZERO
var last_nonzero_x:float = 0.0
var wall:bool = false

@onready var sfx_jump:AudioStreamPlayer = $"Jump"
@onready var donut:PackedScene = load("res://Scenes/Entities/Bullets/Enemy/EnemyBulletDonutLinear.tscn")
#endregion


func _ready() -> void:
	match direction:
		Statics.DirsSurface.FLOOR: my_type = EnemyTypes.PINCER_FLOOR
		Statics.DirsSurface.CEILING: my_type = EnemyTypes.PINCER_CEILING
		_:
			my_type = EnemyTypes.PINCER_WALL
			wall = true
	col = $"BodyBox"
	hitbox = $"Area2D"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	timeout_ptr = int(position.x / 16 + position.y / 8) % MOVE_TIMEOUTS.size()
	move_timeout = MOVE_TIMEOUTS[timeout_ptr]
	if not display_mode:
		var player:Player = GameCore.instance.player
		match direction:
			Statics.DirsSurface.FLOOR:
				facing_right = player.position.x > position.x
				up_direction = Vector2.UP
			Statics.DirsSurface.LWALL:
				facing_right = player.position.y > position.y
				up_direction = Vector2.RIGHT
			Statics.DirsSurface.RWALL:
				facing_right = player.position.y < position.y
				up_direction = Vector2.LEFT
			Statics.DirsSurface.CEILING:
				facing_right = player.position.x < position.x
				up_direction = Vector2.DOWN
	else:
		facing_right = randf() >= 0.5
	play_anim("idle")


func configure_display_mode(_credits:bool = false, _data:int = 0) -> void:
	if _credits:
		if _data == 0:
			direction = Statics.DirsSurface.FLOOR
		elif _data == 1:
			direction = Statics.DirsSurface.CEILING
		else:
			direction = Statics.DirsSurface.LWALL
		play_anim("idle")


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	if vis.is_on_screen():
		move_timeout -= delta
		if move_timeout <= 0.0 and _player_close():
			timeout_ptr = (timeout_ptr + 1) % MOVE_TIMEOUTS.size()
			move_timeout = MOVE_TIMEOUTS[timeout_ptr]
			_setup_move()
	if rel_velocity.x != 0.0:
		last_nonzero_x = rel_velocity.x
	_relative_to_actual()
	move_and_slide()
	_actual_to_relative()
	rel_velocity.y += GRAVITY * delta
	rel_velocity.x = move_toward(rel_velocity.x, 0.0, DECEL * delta)
	
	if is_on_wall():
		if (facing_right and last_nonzero_x > 0.0) or (not facing_right and last_nonzero_x < 0.0):
			facing_right = not facing_right
			rel_velocity.x = -last_nonzero_x
			play_anim("idle" if not grounded else "pounce")
	
	if is_on_floor() and rel_velocity.y >= 0.0 and not grounded:
		grounded = true
		play_anim("idle")
		rel_velocity.y *= -0.1
	
	if hard_mode:
		shot_timeout -= delta
		if shot_timeout <= 0.0:
			shot_timeout = SHOT_TIMEOUT
			var aim:float = atan2(
				GameCore.instance.player.position.y - position.y,
				GameCore.instance.player.position.x - position.x
			)
			_shoot(donut, Vector2(sin(aim), cos(aim)), WEAPON_SPEED)


func play_anim(modifier:String = "") -> void:
	var dir:String = ""
	match direction:
		Statics.DirsSurface.FLOOR: dir = "floor"
		Statics.DirsSurface.LWALL: dir = "lwall"
		Statics.DirsSurface.RWALL: dir = "rwall"
		Statics.DirsSurface.CEILING: dir = "ceiling"
	var facing:String = "r" if facing_right else "l"
	sprite.action = "_".join([dir, facing, modifier])


func _player_close() -> bool:
	if wall:
		return abs(GameCore.instance.player.position.y - position.y) < REACT_DISTANCE
	return abs(GameCore.instance.player.position.x - position.x) < REACT_DISTANCE


func _setup_move() -> void:
	if randf() > JUMP_CHANCE and grounded:
		rel_velocity.y = JUMP_VEL_BASE * JUMP_HEIGHTS[timeout_ptr]
		grounded = false
		sfx_jump.play()
	
	rel_velocity.x = VEL_X
	if randf() > RAND_DIR_CHANCE:
		rel_velocity.x *= -1.0 if randf() < 0.5 else 1.0
	else:
		var player_pos:Vector2 = GameCore.instance.player.position
		match direction:
			Statics.DirsSurface.FLOOR:
				rel_velocity.x *= -1.0 if player_pos.x < position.x else 1.0
			Statics.DirsSurface.LWALL:
				rel_velocity.x *= -1.0 if player_pos.y < position.y else 1.0
			Statics.DirsSurface.RWALL:
				rel_velocity.x *= 1.0 if player_pos.y < position.y else -1.0
			Statics.DirsSurface.CEILING:
				rel_velocity.x *= 1.0 if player_pos.x < position.x else -1.0
	facing_right = rel_velocity.x > 0.0
	play_anim("pounce")


func _relative_to_actual() -> void:
	match direction:
		Statics.DirsSurface.FLOOR: velocity = rel_velocity
		Statics.DirsSurface.LWALL: velocity = Vector2(-rel_velocity.y, rel_velocity.x)
		Statics.DirsSurface.RWALL: velocity = Vector2(rel_velocity.y, -rel_velocity.x)
		Statics.DirsSurface.CEILING: velocity = -rel_velocity


func _actual_to_relative() -> void:
	match direction:
		Statics.DirsSurface.FLOOR: rel_velocity = velocity
		Statics.DirsSurface.LWALL: rel_velocity = Vector2(velocity.y, -velocity.x)
		Statics.DirsSurface.LWALL: rel_velocity = Vector2(velocity.y, -velocity.x)
		Statics.DirsSurface.CEILING: rel_velocity = -velocity
