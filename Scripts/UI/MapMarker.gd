@tool
@icon("res://Editor/ico/Minimap.svg")
class_name MapMarker
extends Node2D


#region Variables
@export var type:Minimap.MarkerTypes = Minimap.MarkerTypes.SAVE
@export var data:Array = []

@onready var cell_pos:Vector2i = Vector2i(position)
@onready var sprite:JsonSprite2D = $"JsonSprite2D"
#endregion
