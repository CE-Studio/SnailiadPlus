@icon("res://Editor/ico/Interactable.svg")
class_name SavePoint
extends Node2D

#region Variables
@export_enum("Floor", "Left wall", "Right wall", "Ceiling") var surface:int = int(Statics.DirsSurface.FLOOR)

var activated:bool = false
#endregion
