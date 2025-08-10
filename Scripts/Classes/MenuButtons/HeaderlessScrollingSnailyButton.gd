@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name HeaderlessScrollingSnailyButton
extends SnailyButton


#region Variables
const HOVER_ARROW_MAX_ALPHA = 0.5
const HOVER_ARROW_CYCLE_SPEED = 8.0

@export var cycle_options:Array[String] = []
@export var auto_select_mode:bool = false # Automatically enable cycling when button is focused, and enable emitting of button_pressed
@export var focus_option:int = 0
@export var loop:bool = true
@export var emit_signal_on_load:bool = false
@export var minimum_x:int:
	set(value):
		minimum_x = value
		custom_minimum_size.x = minimum_x

var selected_option:int
var arrow_flash_cycle:float
var arrow_hover_state:Array[bool] = [ false, false ]

var selected:bool = false

signal option_cycled(value)
signal cycled_left(value)
signal cycled_right(value)
signal button_pressed(value)

@onready var scroller:Control
@onready var option:SnailyText
@onready var tex_left:TextureRect
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
		option.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED
		tex_left.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED
		tex_right.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED


func _process(delta: float) -> void:
	if not Engine.is_editor_hint():
		if (focused and not disabled and life_frames >= REQ_LIFE_FRAMES) or selected:
			var suppress_deselect:bool = selected and (arrow_hover_state[0] or arrow_hover_state[1])
			if (mouse_over and (SInput.input_just_pressed(SInput.Inputs.UI_CLICK) and not suppress_deselect)
			or SInput.input_just_pressed(SInput.Inputs.JUMP)):
				if auto_select_mode:
					button_pressed.emit(selected_option)
				elif selected:
					deselect()
				else:
					set_selected()
		if selected or auto_select_mode:
			var cycled:bool = false
			if (SInput.input_just_pressed(SInput.Inputs.LEFT)
			or (SInput.input_just_pressed(SInput.Inputs.UI_CLICK) and arrow_hover_state[0])):
				selected_option -= 1
				if selected_option < 0:
					selected_option = cycle_options.size() - 1 if loop else 0
				cycled = true
				cycled_left.emit(selected_option)
			if (SInput.input_just_pressed(SInput.Inputs.RIGHT)
			or (SInput.input_just_pressed(SInput.Inputs.UI_CLICK) and arrow_hover_state[1])):
				selected_option += 1
				if selected_option >= cycle_options.size():
					selected_option = 0 if loop else cycle_options.size() - 1
				cycled = true
				cycled_right.emit(selected_option)
			if cycled:
				sfx_focus.play()
				option.call_deferred("set_snaily_text", Statics.get_text(cycle_options[selected_option]))
				option_cycled.emit(selected_option)
		
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


func remote_set_option(value:int) -> void:
	while value < 0:
		value += cycle_options.size()
	selected_option = value % cycle_options.size()
	option.set_snaily_text(Statics.get_text(cycle_options[selected_option]))


func remote_import_new_options(new_array:Array[String]) -> void:
	cycle_options = new_array.duplicate()
	selected_option %= cycle_options.size()
	option.set_snaily_text(Statics.get_text(cycle_options[selected_option]))


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
