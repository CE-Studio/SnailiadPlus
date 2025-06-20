@icon("res://Editor/ico/PlayerBullet.svg")
class_name PlayerBullet
extends Node2D


#region Variables
var type:int = 0
var normalized_dir:Vector2 = Vector2.ZERO
var life_timer:float = 0.0
var velocity:float = 0.0
var velocity_init:float = 0.0
var damage:int = 0
var cooldown:float = 0.0
var rapid_mult:float = 0.0
var powered:bool = false
var despawn_offscreen:bool = false
var single_frame_hit_flag:bool = false

var collide_with_wall:bool
var single_hit:bool

@onready var sprite:JsonSprite2D = $"JsonSprite2D"
@onready var area:Area2D = $"Area2D"
@onready var box_normal:CollisionShape2D = $"Area2D/Normal"
@onready var box_power:CollisionShape2D = $"Area2D/Power"
@onready var sfx_normal:AudioStreamPlayer = $"AudioGroup/Normal"
@onready var sfx_power:AudioStreamPlayer = $"AudioGroup/Power"
#endregion


func _spawn(dir:Vector2, rapid_shot:float, power_shot:bool) -> float:
	normalized_dir = dir
	rapid_mult = rapid_shot
	powered = power_shot
	if power_shot:
		box_normal.disabled = true
		sfx_power.play()
	else:
		box_power.disabled = true
		sfx_normal.play()
	return cooldown


func _process(delta: float) -> void:
	life_timer += delta
	if ((life_timer > 3 or (despawn_offscreen and life_timer >= 0.25)) and
	not Statics.is_box_on_screen(box_power if powered else box_normal, position)):
		_despawn()


func _despawn(loudly:bool = false) -> void:
	queue_free()
