@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name ScrollingSnailyButton
extends Control

#region Variables
const COLOR_ENABLED = Color8(252, 252, 252)
const COLOR_DISABLED = Color8(200, 192, 192)
const HOVER_ARROW_MAX_ALPHA = 0.5
const HOVER_ARROW_CYCLE_SPEED = 8.0

@export var header_id:String = ""
@export var cycle_options:Array[String] = []
@export var focus_option:int = 0
@export var loop:bool = true
@export var grab_focus_on_load:bool = false
@export var emit_signal_on_load:bool = false
@export var disabled = false
@export var minimum_x:int:
	set(value):
		minimum_x = value
		custom_minimum_size.x = minimum_x

var focused:bool = false
var mouse_over:bool = false
var origin:Vector2
var can_focus:bool = true:
	set(value):
		can_focus = value
		if not selected:
			focus_mode = Control.FOCUS_ALL if value else Control.FOCUS_NONE
var has_played_focus_sound:bool = false
var selected_option:int
var arrow_flash_cycle:float
var arrow_hover_state:Array[bool] = [ false, false ]

var selected:bool = false
var parent_layer:MenuLayer

signal option_cycled(value)

@onready var header:SnailyText
@onready var option:SnailyText
@onready var tex_left:TextureRect
@onready var tex_right:TextureRect
@onready var sfx_focus:AudioStreamPlayer
@onready var sfx_select:AudioStreamPlayer
#endregion

func _ready() -> void:
	header = $"Main/TopText/SnailyText"
	option = $"Main/Scroller/Frame/HBoxContainer/MarginContainer/SnailyText"
	tex_left = $"Main/Scroller/Left"
	tex_right = $"Main/Scroller/Right"
	sfx_focus = $"AudioGroup/Focus"
	sfx_select = $"AudioGroup/Select"
	
	header.set_alignment(HORIZONTAL_ALIGNMENT_CENTER, VERTICAL_ALIGNMENT_TOP)
	header.add_shadow(1)
	option.add_shadow(1)
	if not Engine.is_editor_hint():
		origin = position
		if header_id.strip_edges() == "":
			header.set_snaily_text("Text!!")
		else:
			header.set_snaily_text(Statics.get_text(header_id))
		if cycle_options.size() == 0:
			option.set_snaily_text(Statics.get_text("menu_option_scroller_none"))
			selected_option = -1
			disabled = true
		else:
			while focus_option < 0:
				focus_option += cycle_options.size()
			selected_option = focus_option % cycle_options.size()
			option.set_snaily_text(Statics.get_text(cycle_options[selected_option]))
			if emit_signal_on_load:
				option_cycled.emit(selected_option)
		if can_focus and grab_focus_on_load and not disabled:
			grab_focus()
		header.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED
		option.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED
		tex_left.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED
		tex_right.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED


func _process(delta: float) -> void:
	if not Engine.is_editor_hint():
		if (focused and not disabled) or selected:
			var suppress_deselect:bool = selected and (arrow_hover_state[0] or arrow_hover_state[1])
			if (mouse_over and (Input.is_action_just_pressed("UIClick") and not suppress_deselect)
			or Input.is_action_just_pressed("Jump")):
				if selected:
					deselect()
				else:
					set_selected()
		if selected:
			var cycled:bool = false
			if (Input.is_action_just_pressed("Left")
			or (Input.is_action_just_pressed("UIClick") and arrow_hover_state[0])):
				selected_option -= 1
				if selected_option < 0:
					selected_option = cycle_options.size() - 1 if loop else 0
				cycled = true
			if (Input.is_action_just_pressed("Right")
			or (Input.is_action_just_pressed("UIClick") and arrow_hover_state[1])):
				selected_option += 1
				if selected_option >= cycle_options.size():
					selected_option = 0 if loop else cycle_options.size() - 1
				cycled = true
			if cycled:
				sfx_focus.play()
				option.call_deferred("set_snaily_text", Statics.get_text(cycle_options[selected_option]))
				option_cycled.emit(selected_option)
		
		arrow_flash_cycle += delta * HOVER_ARROW_CYCLE_SPEED
		var alpha = 0.0
		if selected:
			alpha = 1.0
		elif focused:
			var cycle = inverse_lerp(-1.0, 1.0, sin(arrow_flash_cycle)) * HOVER_ARROW_MAX_ALPHA
			alpha = cycle
		tex_left.modulate.a = alpha
		tex_right.modulate.a = alpha


func set_header(_text:String) -> void:
	header.set_snaily_text(_text)


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


func set_selected() -> void:
	selected = true
	sfx_select.play()
	parent_layer.can_focus = false


func deselect() -> void:
	selected = false
	sfx_select.play()
	parent_layer.can_focus = true


func _on_left_mouse_entered() -> void:
	arrow_hover_state[0] = true


func _on_left_mouse_exited() -> void:
	arrow_hover_state[0] = false


func _on_right_mouse_entered() -> void:
	arrow_hover_state[1] = true


func _on_right_mouse_exited() -> void:
	arrow_hover_state[1] = false
