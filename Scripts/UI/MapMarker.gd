# Copyright 2026 CE-Studio: AGPL-3.0-only
@tool
@icon("res://Editor/ico/Minimap.svg")
class_name MapMarker
extends Node2D


#region Variables
const COL_CYCLE_TIME:float = 0.1

var cycle_col:float = 0.0
var ptr_col:int = 0
var p_col:Color = Color.TRANSPARENT
@export var type:Minimap.MarkerTypes = Minimap.MarkerTypes.SAVE
@export var data:Array = []

@onready var cell_pos:Vector2i = Vector2i(position)
@onready var sprite:SnailySprite2D = $"SnailySprite2D"
#endregion


func _ready() -> void:
	match type:
		Minimap.MarkerTypes.SAVE: sprite.play("save")
		Minimap.MarkerTypes.ITEM: sprite.play("item_normal")
		Minimap.MarkerTypes.ITEM_COLLECTED: sprite.play("item_collected")
		Minimap.MarkerTypes.BOSS: sprite.play("boss")
		Minimap.MarkerTypes.UNKNOWN: sprite.play("unknown")
		Minimap.MarkerTypes.P_MARKER: sprite.play("marker")


func _process(delta: float) -> void:
	if type == Minimap.MarkerTypes.P_MARKER:
		if p_col != Color.TRANSPARENT:
			modulate = p_col
		else:
			cycle_col += delta
			while cycle_col >= COL_CYCLE_TIME:
				cycle_col -= COL_CYCLE_TIME
				ptr_col = (ptr_col + 1) % 4
				match ptr_col:
					0: modulate = Statics.get_color(Vector2i(0, 1))
					1: modulate = Statics.get_color(Vector2i(3, 3))
					2: modulate = Statics.get_color(Vector2i(3, 13))
					3: modulate = Statics.get_color(Vector2i(3, 7))
					4: modulate = Statics.get_color(Vector2i(2, 2)) # Tried it and idk the loop looks better w/o the red
