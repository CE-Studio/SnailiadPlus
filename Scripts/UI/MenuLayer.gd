@icon("res://Editor/ico/MenuLayer.svg")
class_name MenuLayer
extends Node2D

#region Variables
@export var save_general_on_close:bool = false

const MIN_SEPARATION = 4.0
const MAX_SEPARATION = 4.0
const MOVE_RATE = 12.0

var menu:MainMenu
var meta_info:Array = []

var buttons:Array
var can_focus:bool = true:
	set(value):
		can_focus = value
		for button in buttons:
			if (button is ScrollingSnailyButton and not button.selected) or button is not ScrollingSnailyButton:
				button.can_focus = value
var layer_id:int = 0
var total_layer_count:int = 0
var separation_float:float = MAX_SEPARATION

@onready var main_vbox:VBoxContainer = $"MainVBox"
#endregion


func _ready() -> void:
	for child in Statics.get_all_children(self):
		if child is SnailyButton:
			buttons.append(child)
			child.parent_layer = self


func _process(delta: float) -> void:
	if layer_id == -1:
		position = position.lerp(Vector2(0.0, 480.0), MOVE_RATE * delta)
		if position.y > 479.0:
			queue_free()
	elif total_layer_count > layer_id:
		position = position.lerp(Vector2(0.0, -240.0), MOVE_RATE * delta)
	else:
		position = position.lerp(Vector2.ZERO, MOVE_RATE * delta)
	if separation_float > MIN_SEPARATION:
		main_vbox.add_theme_constant_override("separation", int(separation_float))
		separation_float = lerpf(separation_float, MIN_SEPARATION, MOVE_RATE * delta)


func get_vbox_pos() -> Vector2:
	return main_vbox.position


func remote_create_layer(_layer:String) -> MenuLayer:
	return menu.create_layer(_layer)
