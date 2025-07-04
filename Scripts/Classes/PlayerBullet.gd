@icon("res://Editor/ico/PlayerBullet.svg")
class_name PlayerBullet
extends Node2D


#region Variables
@export_flags("Broom", "Peashooter", "Boomerang", "Rainbow Wave") var type:int = 0
@export var damage:int = 0
@export var cooldown:float = 0.0
@export var rapid_mult:float = 1.0
@export var powered:bool = false
@export var despawn_offscreen:bool = false
@export var collide_with_wall:bool
@export var single_hit:bool
@export var ping_on_breakables:bool = true
@export var despawn_particle:String = "ExplosionSmall"
@export var despawn_offset:Vector2 = Vector2.ZERO

var normalized_dir:Vector2 = Vector2.ZERO
var life_timer:float = 0.0
var velocity:float = 0.0
var velocity_init:float = 0.0
var single_frame_hit_flag:bool = false

@onready var sprite:JsonSprite2D = $"JsonSprite2D"
@onready var area:Area2D = $"Area2D"
@onready var box:CollisionShape2D = $"Area2D/Box"
@onready var sfx_shoot:AudioStreamPlayer = $"AudioGroup/Shoot"
@onready var vis:VisibleOnScreenNotifier2D = $"VisibleOnScreenNotifier2D"
@onready var sfx_despawn:AudioStream = preload("res://Assets/Sounds/Sfx/ShotHit.ogg")
#endregion


func _spawn(dir:Vector2, rapid_shot:float) -> float:
	normalized_dir = dir
	rapid_mult = rapid_shot
	sfx_shoot.play()
	area.connect("body_entered", _on_body_entered)
	return cooldown


func _physics_process(delta: float) -> void:
	life_timer += delta
	if ((life_timer > 3 or (despawn_offscreen and life_timer >= 0.25)) and
	not vis.is_on_screen()):
		despawn()


func despawn(loudly:bool = false) -> void:
	if loudly:
		Statics.spawn_particle(despawn_particle, Room.Layers.FG1, Vector2(
			randf_range(-despawn_offset.x, despawn_offset.x),
			randf_range(-despawn_offset.y, despawn_offset.y)
		) + position)
		Statics.play_sfx_disconnected(sfx_despawn)
	queue_free()


func _on_body_entered(body) -> void:
	if body is not Enemy and collide_with_wall:
		despawn(true)
