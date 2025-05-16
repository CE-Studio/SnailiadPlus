class_name ContextPanel
extends PanelContainer


#region Variables
@onready var vbox:VBoxContainer = $"MarginContainer/VBoxContainer"
@onready var text:SnailyText = $"MarginContainer/VBoxContainer/Text"
@onready var header:SnailyText = null
@onready var button:SnailyButton = null
@onready var text_scene = load("res://Scenes/internals/SnailyText.tscn")
@onready var button_scene = load("res://Scenes/UI/SnailyButton.tscn")
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
		var parent_box:VBoxContainer = VBoxContainer.new()
		vbox.add_child(header_text)
		vbox.move_child(header_text, 0)
		header_text.add_shadow(1)
		header = header_text
	header.text_scale = _size
	header.set_snaily_text(_text)


func add_button(_text:String, target_function:Callable, focus:bool = true) -> void:
	if button == null:
		var new_button:SnailyButton = button_scene.instantiate()
		vbox.add_child(new_button)
		new_button.set_text(_text)
		if focus:
			new_button.grab_focus()
		new_button.button_pressed.connect(target_function)
		button = new_button


func center_on_screen() -> void:
	#var half_size = size * 0.5
	#var half_window_size = Statics.get_window_size() * 0.5
	#position = half_window_size - half_size
	#print(half_size)
	#print(half_window_size)
	#print(size)
	position = Vector2.ZERO
	#set_anchors_preset(Control.PRESET_CENTER)
