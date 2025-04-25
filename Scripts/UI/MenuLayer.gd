@icon("res://Editor/ico/MenuLayer.svg")
class_name MenuLayer
extends Node2D

#region Variables
var buttons:Array
var can_focus:bool = true:
	set(value):
		can_focus = value
		for button in buttons:
			button.can_focus = value
#endregion


func _ready() -> void:
	for child in get_children():
		if child is SnailyButton:
			buttons.append(child)
