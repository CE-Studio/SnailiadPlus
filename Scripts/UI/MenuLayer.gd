@icon("res://Editor/ico/MenuLayer.svg")
class_name MenuLayer
extends Node2D

#region Variables
@export var save_general_on_close:bool = false
@export var hide_global_menu_assets:bool = false
@export_range(0.0, 1.0, 0.01) var selector_opacity:float = 1.0

const MIN_SEPARATION = 4.0
const MAX_SEPARATION = 4.0
const MOVE_RATE = 20.0

var menu:MainMenu
var meta_info:Array = []

var buttons:Array
var can_focus:bool = true:
	set(value):
		can_focus = value
		for button in buttons:
			var can_scroll := button is ScrollingSnailyButton or button is HeaderlessScrollingSnailyButton
			if (can_scroll and not button.selected) or not can_scroll:
				button.can_focus = value
var layer_id:int = 0
var total_layer_count:int = 0
var separation_float:float = MAX_SEPARATION
var first_beep:bool = false

@onready var main_vbox:VBoxContainer = self.get_node_or_null("MainVBox")
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
		position = position.lerp(Vector2(0.0, -300.0), MOVE_RATE * delta)
	else:
		position = position.lerp(Vector2.ZERO, MOVE_RATE * delta)
	if separation_float > MIN_SEPARATION and main_vbox:
		main_vbox.add_theme_constant_override("separation", int(separation_float))
		separation_float = lerpf(separation_float, MIN_SEPARATION, MOVE_RATE * delta)


func get_vbox_pos() -> Vector2:
	if main_vbox:
		return main_vbox.position
	return Vector2.ZERO


func remote_create_layer(_layer:String) -> MenuLayer:
	return menu.create_layer(_layer)
