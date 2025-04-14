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
@onready var sub_text_offsets:Array = []
#endregion


func _ready() -> void:
	set_text("Awesome text")
	add_shadow(2)
	add_border(1)
	set_text("Come on, kid. Let's go save Snailiad... 2!")
	#TODO: fix multiple lines causing issues


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
	for i in len(sub_text):
		var sub_label = sub_text[i]
		sub_label.size.x = text.size.x
		sub_label.position = text.position + sub_text_offsets[i]


func clear_sub_text() -> void:
	for sub_label in sub_text:
		sub_label.queue_free()
	sub_text_offsets.clear()


func add_shadow(distance:int) -> void:
	var shadow = create_new_label()
	shadow.modulate = shadow_color
	sub_text.append(shadow)
	var offset = Vector2(distance, distance)
	shadow.position = text.position + offset
	sub_text_offsets.append(offset)


func add_border(distance:int) -> void:
	for i in range(4):
		var border_part = create_new_label()
		border_part.modulate = shadow_color
		sub_text.append(border_part)
		var offset:Vector2
		match i:
			0: offset = Vector2(0, -distance)
			1: offset = Vector2(distance, 0)
			2: offset = Vector2(0, distance)
			3: offset = Vector2(-distance, 0)
		border_part.position = text.position + offset
		sub_text_offsets.append(offset)


func create_new_label() -> RichTextLabel:
	var new_label = RichTextLabel.new()
	add_child(new_label)
	move_child(new_label, 0)
	#new_label.push_font(font)
	new_label.set_anchors_preset(Control.PRESET_CENTER)
	new_label.size = text.size
	new_label.theme = theme
	new_label.text = text.text
	return new_label
