@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name SnailyButton
extends PanelContainer

#region Variables
const COLOR_ENABLED = Color8(252, 252, 252)
const COLOR_DISABLED = Color8(200, 192, 192)

@export var grab_focus_on_load:bool = false
@export var disabled = false
@export var hide_frame = false:
	set(value):
		hide_frame = value
		frame.self_modulate = Color(1.0, 1.0, 1.0, 0.0 if hide_frame else 1.0)

var focused:bool = false
var mouse_over:bool = false
var origin:Vector2
var can_focus:bool = true:
	set(value):
		can_focus = value
		if value and grab_focus_on_load:
			has_played_focus_sound = false
		focus_mode = Control.FOCUS_ALL if value else Control.FOCUS_NONE
var has_played_focus_sound:bool = false
var parent_layer:MenuLayer
var frame:PanelContainer = self

@onready var sfx_focus:AudioStreamPlayer
@onready var sfx_select:AudioStreamPlayer
#endregion


func _ready() -> void:
	sfx_focus = $"AudioGroup/Focus"
	sfx_select = $"AudioGroup/Select"
	
	if not Engine.is_editor_hint():
		origin = position
		if can_focus and grab_focus_on_load and not disabled:
			grab_focus()
		hide_frame = hide_frame


func free_safely() -> void:
	relinquish_focus_neighbors()
	free_roughly()


func free_roughly() -> void:
	var index = parent_layer.buttons.find(self)
	parent_layer.buttons.remove_at(index)
	queue_free()


func relinquish_focus_neighbors() -> void:
	var bottom:Control = get_node(focus_neighbor_bottom)
	var left:Control = get_node(focus_neighbor_left)
	var right:Control = get_node(focus_neighbor_right)
	var top:Control = get_node(focus_neighbor_top)
	var next:Control = get_node(focus_next)
	var previous:Control = get_node(focus_previous)
	if bottom: if get_node(bottom.focus_neighbor_top) == self:
		bottom.focus_neighbor_top = focus_neighbor_top
	if left: if get_node(left.focus_neighbor_right) == self:
		left.focus_neighbor_right = focus_neighbor_right
	if right: if get_node(right.focus_neighbor_left) == self:
		right.focus_neighbor_left = focus_neighbor_left
	if top: if get_node(top.focus_neighbor_bottom) == self:
		top.focus_neighbor_bottom = focus_neighbor_bottom
	if next: if get_node(next.focus_previous) == self:
		next.focus_previous = focus_previous
	if previous: if get_node(previous.focus_next) == self:
		previous.focus_next = focus_next


func _on_mouse_over() -> void:
	mouse_over = true
	if can_focus and not disabled:
		grab_focus()


func _on_mouse_exit() -> void:
	mouse_over = false


func _on_focus() -> void:
	if not Engine.is_editor_hint():
		if not focused:
			focused = true
			if grab_focus_on_load and not has_played_focus_sound:
				sfx_select.play()
				has_played_focus_sound = true
			else:
				sfx_focus.play()


func _on_exit_focus() -> void:
	if not Engine.is_editor_hint():
		if focused:
			focused = false
