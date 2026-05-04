# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name Walleye
extends Enemy


#region Variables
const SHOT_TIMEOUT:float = 0.08
const SHOT_SPEED:float = 700.0

@export_enum(
	"Up", "Up right", "Right", "Down right",
	"Down", "Down left", "Left", "Up left"
) var direction:int = 2

var shot_timeout:float = 0.0
var fire_dir:Vector2 = Vector2.ZERO

@onready var line_of_sight:Node2D = $"LineOfSight"
@onready var cast1:RayCast2D = $"LineOfSight/RayCast2D"
@onready var cast2:RayCast2D = $"LineOfSight/RayCast2D2"
@onready var laser:PackedScene = load("res://Scenes/Entities/Bullets/Enemy/EnemyBulletLaser.tscn")
#endregion


func _ready() -> void:
	my_type = EnemyTypes.WALLEYE
	hitbox = $"Area2D"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	_play_anim(false)
	line_of_sight.rotation_degrees = 45 * direction
	fire_dir = Vector2(
		sin(deg_to_rad(line_of_sight.rotation_degrees)),
		-cos(deg_to_rad(line_of_sight.rotation_degrees))
	)


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	shot_timeout -= delta
	if vis.is_on_screen() and cast1.is_colliding() and cast2.is_colliding():
		if shot_timeout <= 0.0:
			shot_timeout = SHOT_TIMEOUT
			_shoot(laser, fire_dir, SHOT_SPEED)


func _play_anim(shoot:bool) -> void:
	var angle:String = ""
	match direction:
		0: angle = "U"
		1: angle = "UR"
		2: angle = "R"
		3: angle = "DR"
		4: angle = "D"
		5: angle = "DL"
		6: angle = "L"
		7: angle = "UL"
	var mode:String = "fire" if shoot else "idle"
	sprite.action = "_".join([angle, mode])
