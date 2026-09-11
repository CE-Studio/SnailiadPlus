# Copyright 2026 CE-Studio: AGPL-3.0-only
@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name HeaderlessScrollingSnailyButton
extends SnailyButton


#region Variables
const HOVER_ARROW_MAX_ALPHA = 0.5
const HOVER_ARROW_CYCLE_SPEED = 8.0

@export var cycle_options:Array[String] = []
@export var cycle_option_text:String = ""
@export var auto_select_mode:bool = false # Automatically enable cycling when button is focused, and enable emitting of button_pressed
@export var focus_option:int = 0
@export var loop:bool = true
@export var emit_signal_on_load:bool = false
@export var minimum_x:int:
	set(value):
		minimum_x = value
		custom_minimum_size.x = minimum_x

## Index into the option array representing the current selection
var selected_option:int
## Used to calculate and apply color oscillation to the button's arrows when hovering focus on the button
var arrow_flash_cycle:float
## Used to track if the mouse is currently rolled over either arrow, in order to handle mouse input to scroll
var arrow_hover_state:Array[bool] = [ false, false ]
## Tracks which horizontal input was last handled, to prevent incorrect repeated inputs on subsequent frames
var scroll_state:int = 0

## Set to [code]true[/code] if this button is currently in a state where options can be scrolled
var selected:bool = false

signal option_cycled(value)
signal cycled_left(value)
signal cycled_right(value)
signal button_pressed(value)

## Main body of the button
@onready var scroller:Control
## [SnailyText] label that displays the currently selected option
@onready var option:SnailyText
## Left arrow texture, used to indicate scrolling and handle mouse input
@onready var tex_left:TextureRect
## Right arrow texture, used to indicate scrolling and handle mouse input
@onready var tex_right:TextureRect
#endregion


func _ready() -> void:
	frame = $"Scroller/Frame"
	super._ready()
	scroller = $"Scroller"
	option = $"Scroller/Frame/HBoxContainer/MarginContainer/SnailyText"
	tex_left = $"Scroller/Left"
	tex_right = $"Scroller/Right"
	
	option.add_shadow(1)
	if not Engine.is_editor_hint():
		if cycle_option_text.strip_edges() != "":
			var options:PackedStringArray = cycle_option_text.split("|")
			cycle_options.append_array(options)
			for i in cycle_options.size():
				cycle_options[i] = cycle_options[i].replace("\\n", "\n")
		if cycle_options.size() == 0:
			option.set_snaily_text(tr(&"[Empty array!]"), true)
			selected_option = -1
			disabled = true
		else:
			while focus_option < 0:
				focus_option += cycle_options.size()
			selected_option = focus_option % cycle_options.size()
			option.set_snaily_text(cycle_options[selected_option], true)
			if emit_signal_on_load:
				option_cycled.emit(selected_option)
		option.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED
		tex_left.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED
		tex_right.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED


func _process(delta: float) -> void:
	if not Engine.is_editor_hint():
		if (((focused and not disabled and life_frames >= REQ_LIFE_FRAMES) or selected)
		and parent_layer.menu.read_inputs):
			var suppress_deselect:bool = selected and (arrow_hover_state[0] or arrow_hover_state[1])
			if (mouse_over and (SInput.input_just_pressed(SInput.Inputs.UI_CLICK) and not suppress_deselect)
			or SInput.input_just_pressed(SInput.Inputs.UI_ACCEPT)
			or (selected and SInput.input_just_pressed(SInput.Inputs.UI_BACK))):
				if auto_select_mode:
					button_pressed.emit(selected_option)
				elif selected:
					deselect()
				else:
					set_selected()
		if selected or (focused and auto_select_mode):
			var cycled:bool = false
			if _check_left() and (scroll_state != -1 or SInput.send_con_as_echo):
				selected_option -= 1
				if selected_option < 0:
					selected_option = cycle_options.size() - 1 if loop else 0
				cycled = true
				scroll_state = -1
				cycled_left.emit(selected_option)
			if _check_right() and (scroll_state != 1 or SInput.send_con_as_echo):
				selected_option += 1
				if selected_option >= cycle_options.size():
					selected_option = 0 if loop else cycle_options.size() - 1
				cycled = true
				scroll_state = 1
				cycled_right.emit(selected_option)
			if cycled:
				sfx_focus.play()
				option.call_deferred("set_snaily_text", cycle_options[selected_option], true)
				option_cycled.emit(selected_option)
			if ((scroll_state == -1 and not Input.is_action_pressed("ui_left") and not SInput.input_pressed(SInput.Inputs.LEFT))
			or (scroll_state == 1 and not Input.is_action_pressed("ui_right") and not SInput.input_pressed(SInput.Inputs.RIGHT))):
				scroll_state = 0
		
		arrow_flash_cycle += delta * HOVER_ARROW_CYCLE_SPEED
		var alpha = 0.0
		if selected or (focused and auto_select_mode):
			alpha = 1.0
		elif focused:
			var cycle = inverse_lerp(-1.0, 1.0, sin(arrow_flash_cycle)) * HOVER_ARROW_MAX_ALPHA
			alpha = cycle
		tex_left.modulate.a = alpha
		tex_right.modulate.a = alpha
	super._process(delta)


## Returns [code]true[/code] if any valid left input has been handled
func _check_left() -> bool:
	var norm:bool = SInput.just_pressed("left", true)
	var ui:bool = SInput.just_pressed("ui_left", true)
	var mouse:bool = SInput.input_just_pressed(SInput.Inputs.UI_CLICK) and arrow_hover_state[0]
	return norm or ui or mouse


## Returns [code]true[/code] if any valid right input has been handled
func _check_right() -> bool:
	var norm:bool = SInput.just_pressed("right", true)
	var ui:bool = SInput.just_pressed("ui_right", true)
	var mouse:bool = SInput.input_just_pressed(SInput.Inputs.UI_CLICK) and arrow_hover_state[1]
	return norm or ui or mouse


## Can be called externally to force select an option. Usually called by the parent
## [MenuLayer] on spawn to set the option in accordance with the last saved state of
## the corresponding setting.
func remote_set_option(value:int) -> void:
	while value < 0:
		value += cycle_options.size()
	selected_option = value % cycle_options.size()
	option.set_snaily_text(cycle_options[selected_option], true)


## Can be called externally to overwrite this button's existing list of options with a new one
func remote_import_new_options(new_array:Array[String]) -> void:
	cycle_options = new_array.duplicate()
	selected_option %= cycle_options.size()
	option.set_snaily_text(cycle_options[selected_option], true)


## Sets this button to be in a selected state, where options can be scrolled through and
## focus will not be shifted to horizontally adjacent [Control] nodes
func set_selected() -> void:
	selected = true
	sfx_select.play()
	parent_layer.can_focus = false


## Sets this button to no longer be in a selected state, where options will not be scrolled
## and neighboring nodes can gain focus again
func deselect() -> void:
	selected = false
	sfx_select.play()
	parent_layer.can_focus = true


## Alerts the button when the mouse rolls over the left arrow
func _on_left_mouse_entered() -> void:
	arrow_hover_state[0] = true


## Alerts the button when the mouse leaves the left arrow
func _on_left_mouse_exited() -> void:
	arrow_hover_state[0] = false


## Alerts the button when the mouse rolls over the right arrow
func _on_right_mouse_entered() -> void:
	arrow_hover_state[1] = true


## Alerts the button when the mouse leaves the right arrow
func _on_right_mouse_exited() -> void:
	arrow_hover_state[1] = false
