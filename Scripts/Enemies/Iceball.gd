# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name Iceball
extends Enemy


#region Variables
const SPEED = 60
const GRAVITY = 1200
const FALL_DIR = Vector2.DOWN
const CORNER_CHECK_EXTENT = 12

var sec_per_tick = 0.015
var elapsed:float = 0.0
var is_falling:bool = false
var grace_period = 0.4
var vel = 0

@export var direction:Statics.DirsSurface = Statics.DirsSurface.NONE
@export var ccw:bool # Assuming the iceball is tracking the inner edge of a ring, false for CW and true for CCW

@onready var box:CollisionShape2D = $"BodyBox"
@onready var cast_group:Node2D = $"CastGroup"
@onready var cast_cw_check:RayCast2D = $"CastGroup/CWCheck"
@onready var cast_ccw_check:RayCast2D = $"CastGroup/CCWCheck"
@onready var cast_cw_back:RayCast2D = $"CastGroup/CWBack"
@onready var cast_ccw_back:RayCast2D = $"CastGroup/CCWBack"
@onready var cast_center:RayCast2D = $"CastGroup/Center"
#endregion


func _ready() -> void:
	my_type = EnemyTypes.ICEBALL
	super.spawn()
	
	if hard_mode:
		sec_per_tick = 0.009
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
	play_anim()


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	if is_falling:
		vel += GRAVITY * delta
		body.velocity = FALL_DIR * vel
		body.move_and_slide()
		if body.is_on_floor():
			is_falling = false
			set_dir(Statics.DirsSurface.FLOOR)
			play_anim()
	else:
		elapsed += delta
		while elapsed > sec_per_tick:
			elapsed -= sec_per_tick
			vel = 0.0
			var front_cast = cast_ccw_check if ccw else cast_cw_check
			var back_cast = cast_ccw_back if ccw else cast_cw_back
			var turn_outer:bool = true
			if (front_cast.is_colliding() or back_cast.is_colliding() or cast_center.is_colliding()
			or grace_period > 0.0):
				turn_outer = false
				match direction:
					Statics.DirsSurface.FLOOR:
						body.velocity = (Vector2.RIGHT if ccw else Vector2.LEFT) * SPEED
					Statics.DirsSurface.LWALL:
						body.velocity = (Vector2.DOWN if ccw else Vector2.UP) * SPEED
					Statics.DirsSurface.RWALL:
						body.velocity = (Vector2.UP if ccw else Vector2.DOWN) * SPEED
					Statics.DirsSurface.CEILING:
						body.velocity = (Vector2.LEFT if ccw else Vector2.RIGHT) * SPEED
				body.move_and_slide()
				if body.is_on_wall():
					if cast_center.is_colliding():
						turn(ccw)
						play_anim()
					else:
						turn_outer = true
			if turn_outer:
				var turns = 0
				is_falling = true
				while turns < 4 and is_falling:
					turn(not ccw)
					if is_corner_solid():
						is_falling = false
						grace_period = 0.4
						play_anim()
						match direction:
							Statics.DirsSurface.FLOOR:
								position.y = roundi(position.y * 0.25) * 4.0
								position.y -= Statics.FRAC_8
							Statics.DirsSurface.LWALL:
								position.x = roundi(position.x * 0.25) * 4.0
								position.x += Statics.FRAC_8
							Statics.DirsSurface.RWALL:
								position.x = roundi(position.x * 0.25) * 4.0
								position.x -= Statics.FRAC_8
							Statics.DirsSurface.CEILING:
								position.y = roundi(position.y * 0.25) * 4.0
								position.y += Statics.FRAC_8
					turns += 1
				if is_falling:
					body.up_direction = Vector2.UP
	grace_period -= delta


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
			body.up_direction = Vector2.UP
			box.rotation_degrees = 0.0
		Statics.DirsSurface.LWALL:
			cast_group.rotation_degrees = 90.0
			body.up_direction = Vector2.RIGHT
			box.rotation_degrees = 90.0
		Statics.DirsSurface.RWALL:
			cast_group.rotation_degrees = -90.0
			body.up_direction = Vector2.LEFT
			box.rotation_degrees = 90.0
		Statics.DirsSurface.CEILING:
			cast_group.rotation_degrees = 180.0
			body.up_direction = Vector2.DOWN
			box.rotation_degrees = 0.0
	box.position = -body.up_direction


func play_anim() -> void:
	var anim:String = ""
	match direction:
		Statics.DirsSurface.FLOOR:
			anim = "floor_"
		Statics.DirsSurface.LWALL:
			anim = "lwall_"
		Statics.DirsSurface.RWALL:
			anim = "rwall_"
		Statics.DirsSurface.CEILING:
			anim = "ceiling_"
	anim += "ccw" if ccw else "cw"
	sprite.play(anim)
