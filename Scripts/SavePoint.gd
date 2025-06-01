@tool
@icon("res://Editor/ico/Interactable.svg")
class_name SavePoint
extends Node2D

#region Variables
@export_enum("Floor", "Left wall", "Right wall", "Ceiling") var surface:int = int(Statics.DirsSurface.FLOOR):
	set(value):
		surface = value
		_ready()

var activated:bool = false

@onready var sprite:JsonSprite2D = $"JsonSprite2D"
@onready var box:CollisionShape2D = $"Area2D/CollisionShape2D"
#endregion
#TODO make save points remember what room they're in


func _ready() -> void:
	if surface == Statics.DirsSurface.LWALL or surface == Statics.DirsSurface.RWALL:
		box.rotation_degrees = 90
	else:
		box.rotation_degrees = 0
	if Engine.is_editor_hint():
		var marker = $"MarkerSprite"
		match surface:
			Statics.DirsSurface.FLOOR: marker.frame = 0
			Statics.DirsSurface.LWALL: marker.frame = 4
			Statics.DirsSurface.RWALL: marker.frame = 8
			Statics.DirsSurface.CEILING: marker.frame = 12
