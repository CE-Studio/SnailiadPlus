@icon("res://Editor/ico/MenuLayer.svg")
class_name MenuLayer
extends Node2D

#region Variables
const MIN_SEPARATION = 4.0
const MAX_SEPARATION = 64.0
const MOVE_RATE = 8.0

var buttons:Array
var can_focus:bool = true:
	set(value):
		can_focus = value
		for button in buttons:
			button.can_focus = value
var layer_id:int = 0
var total_layer_count:int = 0
var separation_float:float = MAX_SEPARATION

@onready var main_vbox:VBoxContainer = $"MainVBox"
#endregion


func _ready() -> void:
	for child in get_children():
		if child is SnailyButton:
			buttons.append(child)


func _process(delta: float) -> void:
	if total_layer_count > layer_id:
		position = position.lerp(Vector2(0.0, -240.0), MOVE_RATE * delta)
	elif layer_id == -1:
		position = position.lerp(Vector2(0.0, 480.0), MOVE_RATE * delta)
		if position.y > 479.0:
			queue_free()
	else:
		position = position.lerp(Vector2.ZERO, MOVE_RATE * delta)
	if separation_float > MIN_SEPARATION:
		main_vbox.add_theme_constant_override("separation", int(separation_float))
		separation_float = lerpf(separation_float, MIN_SEPARATION, MOVE_RATE * delta)
