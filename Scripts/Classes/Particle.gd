@icon("res://Editor/ico/Particle.svg")
class_name Particle
extends Node2D


#region Variables
@export var sprite:JsonSprite2D
@export var anim_name:String
@export var sound:AudioStreamPlayer
@export var timer:Timer
@export_range(0.1, 8.0, 0.01) var anim_speed_min:float = 1.0
@export_range(0.1, 8.0, 0.01) var anim_speed_max:float = 1.0
@export var follow_center:Vector2 = Vector2(200, 120)
#endregion


func _spawn(_data:Array) -> void:
	if sprite != null:
		sprite.fps_mult = randf_range(anim_speed_min, anim_speed_max)
		if anim_name != "":
			sprite.action = anim_name
	if sound != null:
		sound.play()
	if timer != null:
		timer.start()


func _process(_delta: float) -> void:
	if timer != null:
		if timer.is_stopped():
			queue_free()
