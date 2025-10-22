class_name Trigger
extends Area2D


@export var active:bool = true
@export var multi_trigger:bool = false


func _on_player_enter(_body:Node2D) -> void:
	if not multi_trigger:
		active = false


func _on_player_exit(_body:Node2D) -> void:
	pass
