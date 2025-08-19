@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name BindSnailyButton
extends SnailyButton


#region Variabes
#@export var bind:StringName = &""
@export var bind:SInput.Inputs = SInput.Inputs.LEFT

const SLOT_COUNT:int = 4
const BIND_STR:String = "menu_option_controls_%s"
const BIND_BBCODE:String = "[ctrl]%s[/ctrl]"
const FOCUS_FLASH_SPEED:float = 6.5

signal pressed(value:int)

var active_buttons:int = 0
var bind_str:String = ""
var focus_flash_color:Color = Color.WHITE
var elapsed:float = 0.0
var slots_moused_over:int = 0:
	set(value):
		slots_moused_over = value
		mouse_over = slots_moused_over > 0

@onready var text:SnailyText = $"PanelContainer/HBoxContainer/Label/MarginContainer/SnailyText"
@onready var bind_frames:Array[PanelContainer] = [
	$"PanelContainer/HBoxContainer/PriKey",
	$"PanelContainer/HBoxContainer/SecKey",
	$"PanelContainer/HBoxContainer/PriCon",
	$"PanelContainer/HBoxContainer/SecCon",
]
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
		text.set_snaily_text(BIND_STR % bind_str)
		setup_bind_icons()
		focus_flash_color = Statics.get_color(Vector2i(1, 8))


func _process(delta: float) -> void:
	if Engine.is_editor_hint():
		return
	
	if parent_layer.meta_info.size() > 0 and parent_layer.can_focus:
		if focused:
			if (SInput.check_input(SInput.Inputs.UI_ACCEPT, true)
			or (mouse_over and SInput.check_input(SInput.Inputs.UI_CLICK, true))):
				parent_layer.meta_info.append(self)
				pressed.emit(bind)
			else:
				if SInput.check_input(SInput.Inputs.LEFT, true):
					sfx_focus.play()
					focus_left()
				if SInput.check_input(SInput.Inputs.RIGHT, true):
					sfx_focus.play()
					focus_right()
		
		elapsed += delta * FOCUS_FLASH_SPEED
		for i in range(bind_icons.size()):
			bind_icons[i].modulate = Color.WHITE
			if i == parent_layer.meta_info[0] and focused:
				bind_icons[i].modulate = Color.WHITE.lerp(focus_flash_color, abs(sin(elapsed)))


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
			bind_icons[event_ptr].set_snaily_text(BIND_BBCODE % event_str)
		else:
			bind_frames[event_ptr].modulate.a = 0
		event_ptr += 1


func check_slot_active(slot_id:int) -> bool:
	var bitwise_id = 1 << abs(slot_id - SLOT_COUNT + 1)
	return bitwise_id & active_buttons > 0


func focus_left(iterations:int = 0) -> void:
	parent_layer.meta_info[0] -= 1
	if parent_layer.meta_info[0] < 0:
		parent_layer.meta_info[0] = SLOT_COUNT - 1
	if not check_slot_active(parent_layer.meta_info[0]) and iterations < SLOT_COUNT:
		focus_left(iterations + 1)


func focus_right(iterations:int = 0) -> void:
	parent_layer.meta_info[0] += 1
	if parent_layer.meta_info[0] >= SLOT_COUNT:
		parent_layer.meta_info[0] = 0
	if not check_slot_active(parent_layer.meta_info[0]) and iterations < SLOT_COUNT:
		focus_right(iterations + 1)


func _on_focus() -> void:
	super()
	if Engine.is_editor_hint() or not parent_layer:
		return
	
	if not check_slot_active(parent_layer.meta_info[0]):
		if parent_layer.meta_info[0] == 0:
			focus_right()
		else:
			focus_left()


func _on_exit_focus() -> void:
	super()
	if Engine.is_editor_hint():
		return
	
	for icon in bind_icons:
		icon.modulate = Color.WHITE


func _on_mouse_entered_0() -> void:
	if check_slot_active(0):
		parent_layer.meta_info[0] = 0
		if focused:
			sfx_focus.play()
		else:
			grab_focus()
	slots_moused_over += 1


func _on_mouse_entered_1() -> void:
	if check_slot_active(1):
		parent_layer.meta_info[0] = 1
		if focused:
			sfx_focus.play()
		else:
			grab_focus()
	slots_moused_over += 1


func _on_mouse_entered_2() -> void:
	if check_slot_active(2):
		parent_layer.meta_info[0] = 2
		if focused:
			sfx_focus.play()
		else:
			grab_focus()
	slots_moused_over += 1


func _on_mouse_entered_3() -> void:
	if check_slot_active(3):
		parent_layer.meta_info[0] = 3
		if focused:
			sfx_focus.play()
		else:
			grab_focus()
	slots_moused_over += 1


func _on_mouse_exit_slot() -> void:
	slots_moused_over -= 1
