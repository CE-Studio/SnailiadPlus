@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name ScrollingSnailyButton
extends SnailyButton


#region Variables
const HOVER_ARROW_MAX_ALPHA = 0.5
const HOVER_ARROW_CYCLE_SPEED = 8.0

@export var header_id:String = ""
@export var auto_select_mode:bool = false # Automatically enable cycling when button is focused, and enable emitting of button_pressed
@export var cycle_options:Array[String] = []
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
var scroll_state:int = 0

var selected:bool = false

signal option_cycled(value)
signal cycled_left(value)
signal cycled_right(value)
signal button_pressed(value)

@onready var header:SnailyText
@onready var scroller:Control
@onready var option:SnailyText
@onready var tex_left:TextureRect
@onready var tex_right:TextureRect
#endregion


func _ready() -> void:
	frame = $"Main/Scroller/Frame"
	super._ready()
	header = $"Main/TopText/SnailyText"
	scroller = $"Main/Scroller"
	option = $"Main/Scroller/Frame/HBoxContainer/MarginContainer/SnailyText"
	tex_left = $"Main/Scroller/Left"
	tex_right = $"Main/Scroller/Right"
	
	header.set_alignment(HORIZONTAL_ALIGNMENT_CENTER, VERTICAL_ALIGNMENT_TOP)
	header.add_shadow(1)
	option.add_shadow(1)
	if not Engine.is_editor_hint():
		if header_id.strip_edges() == "":
			header.set_snaily_text_raw("Text!!")
		else:
			header.set_snaily_text(header_id)
		if cycle_options.size() == 0:
			option.set_snaily_text("menu_option_scroller_none")
			selected_option = -1
			disabled = true
		else:
			while focus_option < 0:
				focus_option += cycle_options.size()
			selected_option = focus_option % cycle_options.size()
			option.set_snaily_text(cycle_options[selected_option])
			if emit_signal_on_load:
				option_cycled.emit(selected_option)
		header.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED
		option.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED
		tex_left.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED
		tex_right.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED


func _process(delta: float) -> void:
	if not Engine.is_editor_hint():
		if (focused and not disabled and life_frames >= REQ_LIFE_FRAMES) or selected:
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
		if (selected or (focused and auto_select_mode)) and not disabled:
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
				option.call_deferred("set_snaily_text", cycle_options[selected_option])
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


func _check_left() -> bool:
	var norm:bool = SInput.just_pressed("left", true)
	var ui:bool = SInput.just_pressed("ui_left", true)
	var mouse:bool = SInput.input_just_pressed(SInput.Inputs.UI_CLICK) and arrow_hover_state[0]
	return norm or ui or mouse


func _check_right() -> bool:
	var norm:bool = SInput.just_pressed("right", true)
	var ui:bool = SInput.just_pressed("ui_right", true)
	var mouse:bool = SInput.input_just_pressed(SInput.Inputs.UI_CLICK) and arrow_hover_state[1]
	return norm or ui or mouse


func set_header(_text:String) -> void:
	header.set_snaily_text_raw(_text)


func remote_set_option(value:int) -> void:
	while value < 0:
		value += cycle_options.size()
	selected_option = value % cycle_options.size()
	option.set_snaily_text(cycle_options[selected_option])


func remote_import_new_options(new_array:Array[String]) -> void:
	cycle_options = new_array.duplicate()
	selected_option %= cycle_options.size()
	option.set_snaily_text(cycle_options[selected_option])


func set_selected() -> void:
	selected = true
	sfx_select.play()
	parent_layer.can_focus = false
	parent_layer.menu.read_inputs = false


func deselect() -> void:
	selected = false
	sfx_select.play()
	parent_layer.can_focus = true
	parent_layer.menu.set_deferred("read_inputs", true)


func _on_left_mouse_entered() -> void:
	arrow_hover_state[0] = true


func _on_left_mouse_exited() -> void:
	arrow_hover_state[0] = false


func _on_right_mouse_entered() -> void:
	arrow_hover_state[1] = true


func _on_right_mouse_exited() -> void:
	arrow_hover_state[1] = false
