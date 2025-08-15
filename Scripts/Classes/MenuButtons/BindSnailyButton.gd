@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name BindSnailyButton
extends SnailyButton


#region Variabes
@export var bind:StringName = &""

const BIND_STR:String = "menu_option_controls_%s"
const BIND_BBCODE:String = "[ctrl]%s[/ctrl]"

signal bind_changed

var active_buttons:int = 0

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
	
	if Engine.is_editor_hint():
		text.set_snaily_text(bind)
	else:
		text.set_snaily_text(BIND_STR % bind)
		_setup_bind_icons()


func _setup_bind_icons() -> void:
	var event:InputEvent
	
	if InputMap.has_action(bind) or InputMap.has_action(bind + "K1"):
		active_buttons += 1
		if InputMap.has_action(bind):
			event = InputMap.action_get_events(bind)[0].duplicate()
		else:
			event = InputMap.action_get_events(bind + "K1")[0].duplicate()
		bind_icons[0].set_snaily_text(BIND_BBCODE % SInput.get_input_icon(event))
	
	if InputMap.has_action(bind + "K2"):
		active_buttons += 2
		event = InputMap.action_get_events(bind + "K2")[0].duplicate()
		bind_icons[1].set_snaily_text(BIND_BBCODE % SInput.get_input_icon(event))
	else:
		bind_frames[1].modulate.a = 0
	
	if InputMap.has_action(bind + "C1"):
		active_buttons += 4
		#print(bind + "C1")
		#print(InputMap.action_get_events(bind + "C1"))
	else:
		bind_frames[2].modulate.a = 0
	
	if InputMap.has_action(bind + "C2"):
		active_buttons += 8
		#print(bind + "C2")
		#print(InputMap.action_get_events(bind + "C2"))
	else:
		bind_frames[3].modulate.a = 0
