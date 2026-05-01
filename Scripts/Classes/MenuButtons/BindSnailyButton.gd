# Copyright 2026 CE-Studio: AGPL-3.0-only
@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name BindSnailyButton
extends SnailyButton


#region Variabes
#@export var bind:StringName = &""
@export var bind:SInput.Inputs = SInput.Inputs.LEFT

const SLOT_COUNT:int = 4
const BIND_STR:String = "menu_option_controls_%s"
const BIND_BBCODE:String = "ctrl__%s"
const FOCUS_FLASH_SPEED:float = 6.5

signal pressed(value:int)

## Byte representation of the active state of each bind cell in this button.[br]
## For easy readability, the byte is logged in reverse--1 (0001) represents the left cell
## and 8 (1000) represents the right cell
var active_buttons:int = 0
## String representation of the control being set. This is what's displayed on the button
var bind_str:String = ""
## The color that the currently selected cell will flash, assuming this button has focus
var focus_flash_color:Color = Color.WHITE
## The time since this button was created
var elapsed:float = 0.0
## Tracks how many cells the mouse is currently hovering over
var slots_moused_over:int = 0:
	set(value):
		slots_moused_over = value
		mouse_over = slots_moused_over > 0
## Tracks which horizontal input was last handled, to prevent incorrect repeated inputs on subsequent frames
var scroll_state:int = 0

## [SnailyText] component used to display which action this button is set to configure
@onready var text:SnailyText = $"PanelContainer/HBoxContainer/Label/MarginContainer/SnailyText"
## Reference array to the [PanelContainer] nodes for each bind cell
@onready var bind_frames:Array[PanelContainer] = [
	$"PanelContainer/HBoxContainer/PriKey",
	$"PanelContainer/HBoxContainer/SecKey",
	$"PanelContainer/HBoxContainer/PriCon",
	$"PanelContainer/HBoxContainer/SecCon",
]
## Reference array to the [SnailyText] nodes within each bind cell
@onready var bind_icons:Array[SnailyText] = [
	$"PanelContainer/HBoxContainer/PriKey/MarginContainer/SnailyText",
	$"PanelContainer/HBoxContainer/SecKey/MarginContainer/SnailyText",
	$"PanelContainer/HBoxContainer/PriCon/MarginContainer/SnailyText",
	$"PanelContainer/HBoxContainer/SecCon/MarginContainer/SnailyText",
]
#endregion


func _ready() -> void:
	super()
	bind_str = SInput.Inputs.keys()[bind]
	bind_str = bind_str.to_camel_case()

	if Engine.is_editor_hint():
		text.set_snaily_text(bind_str)
	else:
		text.set_snaily_text(SInput.get_input_tr_str(bind))
		setup_bind_icons()
		focus_flash_color = Statics.get_color(Vector2i(1, 8))


func _process(delta: float) -> void:
	if Engine.is_editor_hint():
		return

	if parent_layer.meta_info.size() > 0 and parent_layer.can_focus:
		if focused and parent_layer.meta_info[1] <= 0:
			if (SInput.check_input(SInput.Inputs.UI_ACCEPT, true)
			or (mouse_over and SInput.check_input(SInput.Inputs.UI_CLICK, true))):
				parent_layer.meta_info.append(self)
				pressed.emit(bind)
			else:
				if _check_left() and scroll_state != -1:
					sfx_focus.play()
					focus_left()
					scroll_state = -1
				if _check_right() and scroll_state != 1:
					sfx_focus.play()
					focus_right()
					scroll_state = 1
		if ((scroll_state == -1 and not Input.is_action_pressed("ui_left") and not SInput.input_pressed(SInput.Inputs.LEFT))
		or (scroll_state == 1 and not Input.is_action_pressed("ui_right") and not SInput.input_pressed(SInput.Inputs.RIGHT))):
			scroll_state = 0

		elapsed += delta * FOCUS_FLASH_SPEED
		for i in range(bind_icons.size()):
			bind_icons[i].modulate = Color.WHITE
			if i == parent_layer.meta_info[0] and focused:
				bind_icons[i].modulate = Color.WHITE.lerp(focus_flash_color, abs(sin(elapsed)))


## Returns [code]true[/code] if any left input has just been pressed
func _check_left() -> bool:
	var norm:bool = SInput.just_pressed("left", true)
	var ui:bool = SInput.just_pressed("ui_left", true)
	return norm or ui


## Returns [code]true[/code] if any right input has just been pressed
func _check_right() -> bool:
	var norm:bool = SInput.just_pressed("right", true)
	var ui:bool = SInput.just_pressed("ui_right", true)
	return norm or ui


## Initialized each bind cell in accordance with the input this button has been set to configure
func setup_bind_icons() -> void:
	var events:Array = SInput.pull_action(bind)
	var event_ptr:int = 0
	var event_slots:int = SInput.INPUT_SLOTS[bind]

	active_buttons = 0
	for i in [ 8, 4, 2, 1 ]:
		if event_slots & i > 0:
			active_buttons += i
			var event = events[event_ptr]
			var event_str:String = ""
			if i > 2:
				event_str = SInput.get_key_icon(event)
			else:
				if event is Vector2 or event is Vector2i:
					event_str = SInput.get_axis_icon(event)
				else:
					event_str = SInput.get_button_icon(event)
			#bind_icons[event_ptr].set_snaily_text(BIND_BBCODE % event_str)
			bind_icons[event_ptr].set_snaily_text(SInput.get_icon_as_bbcode_from_string(event_str))
		else:
			bind_frames[event_ptr].modulate.a = 0
		event_ptr += 1


## Returns [code]true[/code] if the queried bind cell is selectable on this button
func check_slot_active(slot_id:int) -> bool:
	var bitwise_id = 1 << abs(slot_id - SLOT_COUNT + 1)
	return bitwise_id & active_buttons > 0


## Shifts focus to the next available leftward cell. If none are available, focus wraps
func focus_left(iterations:int = 0) -> void:
	parent_layer.meta_info[0] -= 1
	if parent_layer.meta_info[0] < 0:
		parent_layer.meta_info[0] = SLOT_COUNT - 1
	if not check_slot_active(parent_layer.meta_info[0]) and iterations < SLOT_COUNT:
		focus_left(iterations + 1)


## Shifts focus to the next available rightward cell. If none are available, focus wraps
func focus_right(iterations:int = 0) -> void:
	parent_layer.meta_info[0] += 1
	if parent_layer.meta_info[0] >= SLOT_COUNT:
		parent_layer.meta_info[0] = 0
	if not check_slot_active(parent_layer.meta_info[0]) and iterations < SLOT_COUNT:
		focus_right(iterations + 1)


## Ensures focus properly lands on the nearest available cell when this button gains focus
func _on_focus() -> void:
	super()
	if Engine.is_editor_hint() or not parent_layer:
		return

	if not check_slot_active(parent_layer.meta_info[0]):
		if parent_layer.meta_info[0] == 0:
			focus_right()
		else:
			focus_left()


## Ensures the highlight flash returns to white when this button loses focus
func _on_exit_focus() -> void:
	super()
	if Engine.is_editor_hint():
		return

	for icon in bind_icons:
		icon.modulate = Color.WHITE


## Called when the mouse rolls over slot 0 (left) and alerts the button and parent layer
func _on_mouse_entered_0() -> void:
	if check_slot_active(0):
		parent_layer.meta_info[0] = 0
		if focused:
			sfx_focus.play()
		else:
			grab_focus()
	slots_moused_over += 1


## Called when the mouse rolls over slot 1 (middle-left) and alerts the button and parent layer
func _on_mouse_entered_1() -> void:
	if check_slot_active(1):
		parent_layer.meta_info[0] = 1
		if focused:
			sfx_focus.play()
		else:
			grab_focus()
	slots_moused_over += 1


## Called when the mouse rolls over slot 2 (middle-right) and alerts the button and parent layer
func _on_mouse_entered_2() -> void:
	if check_slot_active(2):
		parent_layer.meta_info[0] = 2
		if focused:
			sfx_focus.play()
		else:
			grab_focus()
	slots_moused_over += 1


## Called when the mouse rolls over slot 3 (right) and alerts the button and parent layer
func _on_mouse_entered_3() -> void:
	if check_slot_active(3):
		parent_layer.meta_info[0] = 3
		if focused:
			sfx_focus.play()
		else:
			grab_focus()
	slots_moused_over += 1


## Called when the mouse exits any cell
func _on_mouse_exit_slot() -> void:
	slots_moused_over -= 1
