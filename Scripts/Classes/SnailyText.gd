@icon("res://Editor/ico/SnailyText.svg")
class_name SnailyText
extends Node2D


#region Variables
var theme:Theme = load("res://Resources/MenuTheme.tres")
var font:FontFile = load("res://Resources/SnailplanesExtended.ttf")

var shadow_color:Color = Color(0.0, 0.0, 0.0)
var align_horiz:HorizontalAlignment = HORIZONTAL_ALIGNMENT_LEFT
var align_vert:VerticalAlignment = VERTICAL_ALIGNMENT_TOP
var max_width:int = 0

@onready var text:RichTextLabel = $"MainText"
@onready var sub_text:Array = []
#endregion


func _ready() -> void:
	set_text("I... am Steve.")


func set_text(_text:String) -> void:
	text.text = _text
	for sub_label in sub_text:
		sub_label.text = _text
	reset_label_size()


func set_alignment(horiz:int, vert:int) -> void:
	align_horiz = horiz
	align_vert = vert
	text.horizontal_alignment = align_horiz
	text.vertical_alignment = align_vert


func reset_label_size() -> void:
	var string_size = font.get_string_size(text.text)
	if max_width == 0 or string_size.x < max_width:
		text.size.x = string_size.x
	else:
		text.size.x = max_width
	text.position.x = text.size.x * -0.5
