# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name ContextPanel
extends PanelContainer


#region Variables
var can_focus:bool = false:
	set(value):
		can_focus = value
		for button in buttons:
			button.can_focus = value

@onready var vbox:VBoxContainer = $"MarginContainer/VBoxContainer"
@onready var buttonbox:HBoxContainer = null
@onready var text:SnailyText = $"MarginContainer/VBoxContainer/Text"
@onready var header:SnailyText = null
@onready var buttons:Array[SnailyButton] = []
@onready var text_scene = load("res://Scenes/internals/SnailyText.tscn")
@onready var button_scene = load("res://Scenes/UI/ActionSnailyButton.tscn")
#endregion


func _ready() -> void:
	text.add_shadow(1)


func set_text(_text:String, _size:int) -> void:
	text.text_scale = _size
	text.set_snaily_text(_text)
	text.max_width = int(custom_minimum_size.x) - 16


func add_header(_text:String, _size:int) -> void:
	if header == null:
		var header_text:SnailyText = text_scene.instantiate()
		#var parent_box:VBoxContainer = VBoxContainer.new()
		vbox.add_child(header_text)
		vbox.move_child(header_text, 0)
		header_text.add_shadow(1)
		header = header_text
	header.text_scale = _size
	header.set_snaily_text(_text)


func add_button(_text:String, target_function:Callable, focus:bool = true) -> void:
	if buttonbox == null:
		var new_hbox = HBoxContainer.new()
		new_hbox.alignment = BoxContainer.ALIGNMENT_CENTER
		new_hbox.add_theme_constant_override("separation", 32)
		vbox.add_child(new_hbox)
		buttonbox = new_hbox
	var new_button:SnailyButton = button_scene.instantiate()
	buttonbox.add_child(new_button)
	new_button.set_text(_text)
	new_button.can_focus = can_focus
	if focus:
		new_button.grab_focus()
	new_button.button_pressed.connect(target_function)
	buttons.append(new_button)


func focus_button(i:int) -> void:
	if i < 0:
		i = 0
	if buttons.size() > 0:
		buttons[i % buttons.size()].grab_focus()


func center_on_screen() -> void:
	#var half_size = size * 0.5
	#var half_window_size = Statics.get_window_size() * 0.5
	#position = half_window_size - half_size
	#print(half_size)
	#print(half_window_size)
	#print(size)
	position = Vector2.ZERO
	#set_anchors_preset(Control.PRESET_CENTER)
