class_name SpikeyCommon
extends Enemy


#region Variables
const SPEED = 1
const GRAVITY = 1200
const FALL_DIR = Vector2.DOWN

var is_falling:bool = false
var grace_period = 0
var vel = 0
var initialized_rot:bool = false

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

@onready var cast_group:Node2D = $"CastGroup"
@onready var cast_cw_check:RayCast2D = $"CastGroup/CWCheck"
@onready var cast_ccw_check:RayCast2D = $"CastGroup/CCWCheck"
@onready var cast_cw_back:RayCast2D = $"CastGroup/CWBack"
@onready var cast_ccw_back:RayCast2D = $"CastGroup/CCWBack"
#endregion


func _ready() -> void:
	col = $"CharacterBody2D/CollisionShape2D"
	body = $"CharacterBody2D"
	hitbox = $"Area2D"
	sprite = $"JsonSprite2D"
	super.spawn(70, 2, 0, true, 2)
	
	if direction == Statics.DirsSurface.NONE:
		if Statics.solid_at_world_pos(Vector2i(position) + (Vector2i.DOWN * 16)):
			direction = Statics.DirsSurface.FLOOR
		elif Statics.solid_at_world_pos(Vector2i(position) + (Vector2i.RIGHT * 16)):
			direction = Statics.DirsSurface.RWALL
		elif Statics.solid_at_world_pos(Vector2i(position) + (Vector2i.UP * 16)):
			direction = Statics.DirsSurface.CEILING
		elif Statics.solid_at_world_pos(Vector2i(position) + (Vector2i.LEFT * 16)):
			direction = Statics.DirsSurface.LWALL
		else:
			direction = Statics.DirsSurface.FLOOR
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


func _process(delta: float) -> void:
	pass
