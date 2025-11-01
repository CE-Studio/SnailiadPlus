class_name SpikeyTough
extends Enemy


#region Variables
const SPEED = 60
const GRAVITY = 1200
const FALL_DIR = Vector2.DOWN
const CORNER_CHECK_EXTENT = 12
const SEC_PER_TICK_SLOW = 0.022
const SEC_PER_TICK_FAST = 0.019
const STOP_TIMEOUT_SLOW = 5.0
const START_TIMEOUT_SLOW = 1.0
const STOP_TIMEOUT_FAST = 3.0
const START_TIMEOUT_FAST = 0.2
const PEA_SPEED = 80.0
const PEA_MOD = 2.3

var elapsed:float = 0.0
var is_falling:bool = false
var grace_period = 4
var vel = 0
var stop_timeout:float
var start_timeout:float
var stopped:bool = false

@export var direction:Statics.DirsSurface = Statics.DirsSurface.NONE:
	set(value):
		direction = value
		if Engine.is_editor_hint():
			match value:
				Statics.DirsSurface.FLOOR or Statics.DirsSurface.NONE:
					$"JsonSprite2D/MarkerSprite".frame = 0
				Statics.DirsSurface.LWALL:
					$"JsonSprite2D/MarkerSprite".frame = 12
				Statics.DirsSurface.RWALL:
					$"JsonSprite2D/MarkerSprite".frame = 4
				Statics.DirsSurface.CEILING:
					$"JsonSprite2D/MarkerSprite".frame = 8
@export var ccw:bool # Assuming the spikey is tracking the inner edge of a ring, false for CW and true for CCW

@onready var box:CollisionShape2D = $"BodyBox"
@onready var cast_group:Node2D = $"CastGroup"
@onready var cast_cw_check:RayCast2D = $"CastGroup/CWCheck"
@onready var cast_ccw_check:RayCast2D = $"CastGroup/CCWCheck"
@onready var cast_cw_back:RayCast2D = $"CastGroup/CWBack"
@onready var cast_ccw_back:RayCast2D = $"CastGroup/CCWBack"
@onready var cast_center:RayCast2D = $"CastGroup/Center"
@onready var pea:PackedScene = preload("res://Scenes/Entities/Bullets/Enemy/EnemyBulletPea.tscn")
#endregion


func _ready() -> void:
	my_type = EnemyTypes.SPIKEY_TOUGH
	col = $"BodyBox"
	hitbox = $"Area2D"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	if hard_mode:
		max_health = 720
	
	if direction == Statics.DirsSurface.NONE:
		if Statics.solid_at_world_pos(position + (Vector2.DOWN * 16), true):
			set_dir(Statics.DirsSurface.FLOOR)
			position.y -= Statics.FRAC_8
		elif Statics.solid_at_world_pos(position + (Vector2.RIGHT * 16), true):
			set_dir(Statics.DirsSurface.RWALL)
			position.x -= Statics.FRAC_8
		elif Statics.solid_at_world_pos(position + (Vector2.UP * 16), true):
			set_dir(Statics.DirsSurface.CEILING)
			position.y += Statics.FRAC_8
		elif Statics.solid_at_world_pos(position + (Vector2.LEFT * 16), true):
			set_dir(Statics.DirsSurface.LWALL)
			position.x += Statics.FRAC_8
		else:
			set_dir(Statics.DirsSurface.FLOOR)
			is_falling = true
	var dir = "ccw" if ccw else "cw"
	match direction:
		Statics.DirsSurface.FLOOR:
			sprite.action = "floor_" + dir
		Statics.DirsSurface.LWALL:
			sprite.action = "lwall_" + dir
			cast_group.rotation_degrees = 90.0
		Statics.DirsSurface.RWALL:
			sprite.action = "rwall_" + dir
			cast_group.rotation_degrees = -90.0
		Statics.DirsSurface.CEILING:
			sprite.action = "ceiling_" + dir
			cast_group.rotation_degrees = 180.0
	sprite._process(0.0)
	
	stop_timeout = fmod(2.0 + (position.x * 0.13 + position.y * 0.7), 2.94)
	start_timeout = stop_timeout 


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	stop_timeout -= delta
	start_timeout -= delta
	var this_stop = STOP_TIMEOUT_FAST if hard_mode else STOP_TIMEOUT_SLOW
	var this_start = START_TIMEOUT_FAST if hard_mode else START_TIMEOUT_SLOW
	if stop_timeout < 0 and not stopped:
		stop_timeout = this_stop
		start_timeout = this_start
		stopped = true
		play_anim("_stop")
	elif start_timeout < 0 and stopped:
		stop_timeout = this_stop
		start_timeout = this_start + this_stop
		stopped = false
		play_anim()
		if vis.is_on_screen():
			var target = Vector2(GameCore.instance.player.position - position).normalized()
			var speed = PEA_SPEED * (PEA_MOD if hard_mode else 1.0)
			_shoot(pea, target, speed)
	
	if is_falling:
		vel += GRAVITY * delta
		velocity = FALL_DIR * vel
		move_and_slide()
		if is_on_floor():
			is_falling = false
			set_dir(Statics.DirsSurface.FLOOR)
			play_anim()
	elif not stopped:
		elapsed += delta
		var this_tick_speed = SEC_PER_TICK_FAST if hard_mode else SEC_PER_TICK_SLOW
		while elapsed > this_tick_speed:
			elapsed -= this_tick_speed
			vel = 0.0
			var front_cast = cast_ccw_check if ccw else cast_cw_check
			var back_cast = cast_ccw_back if ccw else cast_cw_back
			var turn_outer:bool = true
			if (front_cast.is_colliding() or back_cast.is_colliding() or cast_center.is_colliding()
			or grace_period > 0.0):
				turn_outer = false
				match direction:
					Statics.DirsSurface.FLOOR:
						velocity = (Vector2.RIGHT if ccw else Vector2.LEFT) * SPEED
					Statics.DirsSurface.LWALL:
						velocity = (Vector2.DOWN if ccw else Vector2.UP) * SPEED
					Statics.DirsSurface.RWALL:
						velocity = (Vector2.UP if ccw else Vector2.DOWN) * SPEED
					Statics.DirsSurface.CEILING:
						velocity = (Vector2.LEFT if ccw else Vector2.RIGHT) * SPEED
				move_and_slide()
				if is_on_wall():
					if cast_center.is_colliding():
						turn(ccw)
						play_anim("_turnto_inner")
					else:
						turn_outer = true
			if turn_outer:
				var turns = 0
				is_falling = true
				while turns < 4 and is_falling:
					turn(not ccw)
					if is_corner_solid():
						is_falling = false
						grace_period = 4
						play_anim("_turnto_outer")
						match direction:
							Statics.DirsSurface.FLOOR:
								position.y = roundi(position.y * 0.25) * 4.0
								position.y -= Statics.FRAC_16
							Statics.DirsSurface.LWALL:
								position.x = roundi(position.x * 0.25) * 4.0
								position.x += Statics.FRAC_16
							Statics.DirsSurface.RWALL:
								position.x = roundi(position.x * 0.25) * 4.0
								position.x -= Statics.FRAC_16
							Statics.DirsSurface.CEILING:
								position.y = roundi(position.y * 0.25) * 4.0
								position.y += Statics.FRAC_16
					turns += 1
				if is_falling:
					up_direction = Vector2.UP
	grace_period -= 1


func turn(_ccw:bool) -> void:
	set_dir(Statics.spin_surface(direction, _ccw))


func is_corner_solid() -> bool:
	var check_pos
	match direction:
		Statics.DirsSurface.FLOOR:
			check_pos = Vector2(1 if ccw else -1, 1)
		Statics.DirsSurface.LWALL:
			check_pos = Vector2(-1, 1 if ccw else -1)
		Statics.DirsSurface.RWALL:
			check_pos = Vector2(1, -1 if ccw else 1)
		Statics.DirsSurface.CEILING:
			check_pos = Vector2(-1 if ccw else 1, -1)
	check_pos *= CORNER_CHECK_EXTENT
	return Statics.solid_at_world_pos(position + check_pos, true)


func set_dir(new_dir:Statics.DirsSurface) -> void:
	direction = new_dir
	match new_dir:
		Statics.DirsSurface.FLOOR:
			cast_group.rotation_degrees = 0.0
			up_direction = Vector2.UP
			box.rotation_degrees = 0.0
		Statics.DirsSurface.LWALL:
			cast_group.rotation_degrees = 90.0
			up_direction = Vector2.RIGHT
			box.rotation_degrees = 90.0
		Statics.DirsSurface.RWALL:
			cast_group.rotation_degrees = -90.0
			up_direction = Vector2.LEFT
			box.rotation_degrees = 90.0
		Statics.DirsSurface.CEILING:
			cast_group.rotation_degrees = 180.0
			up_direction = Vector2.DOWN
			box.rotation_degrees = 0.0
	box.position = -up_direction


func play_anim(modifier:String = "") -> void:
	var new_action = ""
	match direction:
		Statics.DirsSurface.FLOOR:
			new_action = "floor_"
		Statics.DirsSurface.LWALL:
			new_action = "lwall_"
		Statics.DirsSurface.RWALL:
			new_action = "rwall_"
		Statics.DirsSurface.CEILING:
			new_action = "ceiling_"
	new_action += "ccw" if ccw else "cw"
	new_action += modifier
	sprite.action = new_action
