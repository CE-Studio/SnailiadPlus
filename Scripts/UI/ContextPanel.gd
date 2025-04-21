class_name ContextPanel
extends PanelContainer


#region Variables
@onready var vbox:VBoxContainer = $"MarginContainer/VBoxContainer"
@onready var text:SnailyText = $"MarginContainer/VBoxContainer/Text"
@onready var header:SnailyText = null
@onready var text_scene = load("res://Scenes/internals/SnailyText.tscn")
@onready var button_scene = load("res://Scenes/UI/SnailyButton.tscn")
#endregion


func _ready() -> void:
	text.add_shadow(1)


func set_text(_text:String, _size:int) -> void:
	text.text_scale = _size
	text.set_snaily_text(_text)
	text.max_width = custom_minimum_size.x - 16


func add_header(_text:String, _size:int) -> void:
	if header == null:
		var header_text:SnailyText = text_scene.instantiate()
		var parent_box:VBoxContainer = VBoxContainer.new()
		vbox.add_child(header_text)
		vbox.move_child(header_text, 0)
		#header_text.set_alignment(HORIZONTAL_ALIGNMENT_CENTER, VERTICAL_ALIGNMENT_TOP)
		header_text.add_shadow(1)
		header = header_text
	header.text_scale = _size
	header.set_snaily_text(_text)


func center_on_screen() -> void:
	#var half_size = size * 0.5
	#var half_window_size = Statics.get_window_size() * 0.5
	#position = half_window_size - half_size
	#print(half_size)
	#print(half_window_size)
	#print(size)
	position = Vector2.ZERO
	#set_anchors_preset(Control.PRESET_CENTER)
