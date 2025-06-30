@icon("res://Editor/ico/EnvironmentArea.svg")
class_name EnvironmentArea
extends Area2D

#region Variables
var contained_bodies:Array = []
#endregion


func _on_body_enter(body) -> void:
	contained_bodies.append(body)


func _on_body_exit(body) -> void:
	contained_bodies.remove_at(contained_bodies.find(body))
