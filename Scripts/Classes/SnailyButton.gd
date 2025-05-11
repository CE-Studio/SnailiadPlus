@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name SnailyButton
extends PanelContainer

#region Variables
const COLOR_ENABLED = Color8(252, 252, 252)
const COLOR_DISABLED = Color8(200, 192, 192)

@export var text_id:String = ""
@export var quick_load_layer:String = ""
@export var back_one_layer:bool = false
@export var grab_focus_on_load:bool = false
@export var disabled = false
@export var hide_frame = false:
	set(value):
		hide_frame = value
		self_modulate = Color(1.0, 1.0, 1.0, 0.0 if hide_frame else 1.0)

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

signal button_pressed(value)

@onready var text:SnailyText
@onready var sfx_focus:AudioStreamPlayer
@onready var sfx_select:AudioStreamPlayer
#endregion


func _ready() -> void:
	text = $"MarginContainer/SnailyText"
	sfx_focus = $"AudioGroup/Focus"
	sfx_select = $"AudioGroup/Select"
	
	text.set_alignment(HORIZONTAL_ALIGNMENT_CENTER, VERTICAL_ALIGNMENT_TOP)
	text.add_shadow(1)
	if not Engine.is_editor_hint():
		origin = position
		if text_id.strip_edges() == "":
			text.set_snaily_text("Text!!")
		else:
			text.set_snaily_text(Statics.get_text(text_id))
		if can_focus and grab_focus_on_load and not disabled:
			grab_focus()
		text.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED
		hide_frame = hide_frame


func _process(delta: float) -> void:
	if focused and not disabled:
		if (mouse_over and (Input.is_action_just_pressed("UIClick"))
		or Input.is_action_just_pressed("Jump")):
			if quick_load_layer.strip_edges() != "":
				button_pressed.emit(quick_load_layer)
			else:
				button_pressed.emit()


func set_text(_text:String) -> void:
	text.set_snaily_text(_text)


func free_safely() -> void:
	relinquish_focus_neighbors()
	queue_free()


func relinquish_focus_neighbors() -> void:
	var bottom:Control = get_node(focus_neighbor_bottom)
	var left:Control = get_node(focus_neighbor_left)
	var right:Control = get_node(focus_neighbor_right)
	var top:Control = get_node(focus_neighbor_top)
	var next:Control = get_node(focus_next)
	var previous:Control = get_node(focus_previous)
	bottom.focus_neighbor_top = focus_neighbor_top
	top.focus_neighbor_bottom = focus_neighbor_bottom
	left.focus_neighbor_right = focus_neighbor_right
	right.focus_neighbor_left = focus_neighbor_left
	next.focus_previous = focus_previous
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
