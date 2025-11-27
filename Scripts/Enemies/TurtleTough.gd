class_name TurtleTough
extends Enemy


#region Variables
const JUMP_TIMEOUTS:Array[float] = [
	2.5, 2.3, 3.0, 2.1, 2.7, 2.6, 2.9, 2.1, 2.3, 3.1,3.3, 2.9,
	2.6, 2.4, 1.9, 3.1, 2.7, 3.9, 4.2, 1.8, 2.8, 3.1, 3.8, 2.8
]
const FLIP_TIMEOUT:float = 0.3
const JUMP_POWER:float = 500.0
const GRAVITY:float = 1200.0
const TURNAROUND_TIMEOUT:float = 1.8

@export var direction:Statics.DirsSurface = Statics.DirsSurface.FLOOR

var speed:float = 24.0
var jump_timeout:float = 0.0
var jump_index:int = 0
var flip_timeout:float = 99999999.0
var turn_timeout:float = 0.0
var facing_left:bool = false
var last_action:String = ""

@onready var sfx_jump:AudioStreamPlayer = $"Jump"
@onready var sfx_flip:AudioStreamPlayer = $"Flip"
#endregion


func _ready() -> void:
	my_type = EnemyTypes.TURTLE_TOUGH
	col = $"BodyBox"
	hitbox = $"Area2D"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	_set_gravity(direction)
	
	jump_index = absi(roundi(position.y / 16.0)) % JUMP_TIMEOUTS.size()
	jump_timeout = JUMP_TIMEOUTS[jump_index]
	if hard_mode:
		speed *= 1.9
	
	if display_mode:
		direction = Statics.DirsSurface.RWALL
		facing_left = randf() <= 0.5
	else:
		var player:Player = GameCore.instance.player
		match direction:
			Statics.DirsSurface.FLOOR:
				facing_left = player.position.x < position.x
				velocity = Vector2(-speed if facing_left else speed, 0.0)
			Statics.DirsSurface.LWALL:
				facing_left = player.position.y < position.y
				velocity = Vector2(0.0, -speed if facing_left else speed)
			Statics.DirsSurface.RWALL:
				facing_left = player.position.y > position.y
				velocity = Vector2(0.0, speed if facing_left else -speed)
			Statics.DirsSurface.FLOOR:
				facing_left = player.position.x < position.x
				velocity = Vector2(speed if facing_left else -speed, 0.0)
	_play_anim("walk")


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	var jump:bool = false
	var flip:bool = false
	var turn:bool = false
	if vis.is_on_screen():
		jump_timeout -= delta
		if jump_timeout <= 0.0:
			jump = true
			jump_index = (jump_index + 1) % JUMP_TIMEOUTS.size()
			jump_timeout = JUMP_TIMEOUTS[jump_index]
			flip_timeout = FLIP_TIMEOUT
			sfx_jump.play()
		flip_timeout -= delta
		if flip_timeout <= 0.0:
			flip_timeout = 99999999.0
			flip = true
			sfx_flip.play()
		turn_timeout -= delta
		if turn_timeout <= 0.0:
			turn_timeout = TURNAROUND_TIMEOUT
			turn = true
	
	var player:Player = GameCore.instance.player
	velocity += GRAVITY * delta * -up_direction
	match direction:
		Statics.DirsSurface.FLOOR:
			if vis.is_on_screen():
				if is_on_wall() or (turn and (
					(facing_left and player.position.x > position.x) or
					(not facing_left and player.position.x < position.x)
				)):
					facing_left = not facing_left
					_play_anim("turn" if is_on_floor() else "fall")
				velocity.x = -speed if facing_left else speed
				if jump:
					velocity.y = -JUMP_POWER
					_play_anim("jump")
				if flip:
					_set_gravity(Statics.DirsSurface.CEILING)
					facing_left = not facing_left
					_play_anim("flip")
					Statics.spawn_particle("GravWhooshGroup", Room.Layers.GROUND, position, [direction])
			else:
				velocity.x = 0.0
		Statics.DirsSurface.LWALL:
			if vis.is_on_screen():
				if is_on_wall() or (turn and (
					(facing_left and player.position.y > position.y) or
					(not facing_left and player.position.y < position.y)
				)):
					facing_left = not facing_left
					_play_anim("turn" if is_on_floor() else "fall")
				velocity.y = -speed if facing_left else speed
				if jump:
					velocity.x = JUMP_POWER
					_play_anim("jump")
				if flip:
					_set_gravity(Statics.DirsSurface.RWALL)
					facing_left = not facing_left
					_play_anim("flip")
					Statics.spawn_particle("GravWhooshGroup", Room.Layers.GROUND, position, [direction])
			else:
				velocity.y = 0.0
		Statics.DirsSurface.RWALL:
			if vis.is_on_screen():
				if is_on_wall() or (turn and (
					(facing_left and player.position.y < position.y) or
					(not facing_left and player.position.y > position.y)
				)):
					facing_left = not facing_left
					_play_anim("turn" if is_on_floor() else "fall")
				velocity.y = speed if facing_left else -speed
				if jump:
					velocity.x = -JUMP_POWER
					_play_anim("jump")
				if flip:
					_set_gravity(Statics.DirsSurface.LWALL)
					facing_left = not facing_left
					_play_anim("flip")
					Statics.spawn_particle("GravWhooshGroup", Room.Layers.GROUND, position, [direction])
			else:
				velocity.y = 0.0
		Statics.DirsSurface.CEILING:
			if vis.is_on_screen():
				if is_on_wall() or (turn and (
					(facing_left and player.position.x < position.x) or
					(not facing_left and player.position.x > position.x)
				)):
					facing_left = not facing_left
					_play_anim("turn" if is_on_floor() else "fall")
				velocity.x = speed if facing_left else -speed
				if jump:
					velocity.y = JUMP_POWER
					_play_anim("jump")
				if flip:
					_set_gravity(Statics.DirsSurface.FLOOR)
					facing_left = not facing_left
					_play_anim("flip")
					Statics.spawn_particle("GravWhooshGroup", Room.Layers.GROUND, position, [direction])
			else:
				velocity.x = 0.0
	move_and_slide()


func _set_gravity(new_dir:Statics.DirsSurface) -> void:
	direction = new_dir
	match new_dir:
		Statics.DirsSurface.FLOOR: up_direction = Vector2.UP
		Statics.DirsSurface.LWALL: up_direction = Vector2.RIGHT
		Statics.DirsSurface.RWALL: up_direction = Vector2.LEFT
		Statics.DirsSurface.CEILING: up_direction = Vector2.DOWN
	if new_dir == Statics.DirsSurface.LWALL or new_dir == Statics.DirsSurface.RWALL:
		col.rotation_degrees = 90
		hitbox.rotation_degrees = 90
	else:
		col.rotation_degrees = 0
		hitbox.rotation_degrees = 0


func _play_anim(action:String = "") -> void:
	var anim_name:String = ""
	match direction:
		Statics.DirsSurface.FLOOR: anim_name = "floor_"
		Statics.DirsSurface.LWALL: anim_name = "lwall_"
		Statics.DirsSurface.RWALL: anim_name = "rwall_"
		Statics.DirsSurface.CEILING: anim_name = "ceiling_"
	anim_name += "left_" if facing_left else "right_"
	anim_name += action if action.strip_edges() != "" else last_action
	if sprite.action != anim_name:
		sprite.action = anim_name
		last_action = action
