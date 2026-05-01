# Copyright 2026 CE-Studio: AGPL-3.0-only
@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name SnailyButton
extends PanelContainer

#region Variables
const COLOR_ENABLED = Color8(252, 252, 252)
const COLOR_DISABLED = Color8(200, 192, 192)
const REQ_LIFE_FRAMES = 2

@export var grab_focus_on_load:bool = false
@export var disabled = false
@export var hide_frame = false:
	set(value):
		hide_frame = value
		if frame:
			frame.self_modulate = Color(1.0, 1.0, 1.0, 0.0 if hide_frame else 1.0)

## Set to [code]true[/code] if this button currently has control focus
var focused:bool = false
## Set to [code]true[/code] if the mouse is currently hovering over this button
var mouse_over:bool = false
## The position that this button was spawned at
var origin:Vector2
## Controls whether or not this button is capable of gaining focus
var can_focus:bool = true:
	set(value):
		can_focus = value
		focus_mode = Control.FOCUS_ALL if value else Control.FOCUS_NONE
		if value:
			life_frames = 0
## The [MenuLayer] that this button is assigned to
var parent_layer:MenuLayer
## The [PanelContainer] the contents of this button are held within
var frame:PanelContainer = self
## How many frames this button has been instanced for
var life_frames:int = 0

## The sound played when this button gains focus
@onready var sfx_focus:AudioStreamPlayer
## The sound played when this button is selected
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


func _process(_delta: float) -> void:
	if not Engine.is_editor_hint():
		life_frames += 1


## Called to free this button from the tree in a way that passes any focus neighbors
## to the opposite neighbor to retain the structure of the menu
func free_safely() -> void:
	relinquish_focus_neighbors()
	free_roughly()


## Called to free this button without consideration for neighbors
func free_roughly() -> void:
	var index = parent_layer.buttons.find(self)
	parent_layer.buttons.remove_at(index)
	queue_free()


## Gets each focus neighbor in all six directions and passes the path to each to the opposite neighbor
## if it exists
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


## Called when the mouse rolls over this button
func _on_mouse_over() -> void:
	mouse_over = true
	if can_focus and not disabled:
		grab_focus()


## Called when the mouse leaves this button
func _on_mouse_exit() -> void:
	mouse_over = false


## Called when this button gains control focus
func _on_focus() -> void:
	if not Engine.is_editor_hint():
		if not focused:
			focused = true
			if grab_focus_on_load and (not parent_layer or parent_layer.first_beep):
				sfx_select.play()
				if parent_layer:
					parent_layer.first_beep = false
			else:
				sfx_focus.play()


## Called when this button loses control focus
func _on_exit_focus() -> void:
	if not Engine.is_editor_hint():
		if focused:
			focused = false
