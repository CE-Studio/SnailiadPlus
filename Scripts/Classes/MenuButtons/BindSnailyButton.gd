@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name BindSnailyButton
extends SnailyButton


#region Variabes
#@export var bind:StringName = &""
@export var bind:SInput.Inputs = SInput.Inputs.LEFT

const BIND_STR:String = "menu_option_controls_%s"
const BIND_BBCODE:String = "[ctrl]%s[/ctrl]"
const FOCUS_FLASH_SPEED:float = 6.5

signal bind_changed

var active_buttons:int = 0
var bind_str:String = ""
var focus_flash_color:Color = Color.WHITE
var elapsed:float = 0.0

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
		_setup_bind_icons()
		focus_flash_color = Statics.get_color(Vector2i(1, 10))


func _process(delta: float) -> void:
	if Engine.is_editor_hint():
		return
	
	if parent_layer.meta_info.size() > 0:
		elapsed += delta * FOCUS_FLASH_SPEED
		for i in range(bind_icons.size()):
			bind_icons[i].modulate = Color.WHITE
			if i == parent_layer.meta_info[0] and focused:
				bind_icons[i].modulate = Color.WHITE.lerp(focus_flash_color, abs(sin(elapsed)))


func _setup_bind_icons() -> void:
	var events:Array = SInput.pull_action(bind)
	var event_ptr:int = 0
	var event_slots:int = SInput.INPUT_SLOTS[bind]
	
	for i in [ 8, 4, 2, 1 ]:
		if event_slots & i > 0:
			active_buttons += i
			var event = events[event_ptr]
			var event_str:String = ""
			if i > 2:
				event_str = SInput.get_key_icon(event)
			else:
				if event is Vector2i:
					event_str = SInput.get_axis_icon(event)
				else:
					event_str = SInput.get_button_icon(event)
			bind_icons[event_ptr].set_snaily_text(BIND_BBCODE % event_str)
		else:
			bind_frames[event_ptr].modulate.a = 0
		event_ptr += 1


func _on_exit_focus() -> void:
	super()
	if Engine.is_editor_hint():
		return
	
	for icon in bind_icons:
		icon.modulate = Color.WHITE
