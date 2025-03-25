@icon("res://Editor/ico/Particle.svg")
class_name Particle
extends Node2D


#region Variables
@export var sprite:JsonSprite2D
@export var anim_name:String
@export var sound:AudioStreamPlayer
@export var timer:Timer
#endregion


func _spawn(_data:Array) -> void:
	if sprite != null and anim_name != "":
		sprite.action = anim_name
	if sound != null:
		sound.play()
	if timer != null:
		timer.start()


func _process(_delta: float) -> void:
	if timer != null:
		if timer.is_stopped():
			queue_free()
