@tool
@icon("res://Editor/ico/SnailyText.svg")
class_name SnailyText
extends RichTextLabel


@export_tool_button("Force Update") var _blech:Callable = reset_label_size


#region Variables
const MAGIC_VECTOR:Vector2 = Vector2(-9, 8)

@export var max_width:int = 0

var menu_theme:Theme = load("res://Resources/MenuTheme.tres")
var font:FontFile = load("res://Resources/SnailplanesExtended.ttf")

var shadow_color:Color = Color(0.0, 0.0, 0.0)
var align_horiz:HorizontalAlignment = HORIZONTAL_ALIGNMENT_LEFT
var align_vert:VerticalAlignment = VERTICAL_ALIGNMENT_TOP

@onready var sub_text:Array = []
@onready var sub_text_offsets:Array = []
#endregion


func set_snaily_text(_text:String) -> void:
	print(_text)
	text = _text
	for sub_label in sub_text:
		sub_label.text = _text
	reset_label_size.call_deferred()


func set_alignment(horiz:int, vert:int) -> void:
	align_horiz = horiz
	align_vert = vert
	horizontal_alignment = align_horiz
	vertical_alignment = align_vert
	for sub_label in sub_text:
		sub_label.horizontal_alignment = align_horiz
		sub_label.vertical_alignment = align_vert


func reset_label_size() -> void:
	#var string_size = font.get_string_size(text)
	#print(string_size.x)
	print("resize", text)
	var longest_line = 0
	var lines = text.split("\n")
	for line in lines:
		var line_length = font.get_string_size(line).x
		if longest_line < line_length:
			longest_line = line_length
	if max_width == 0 or longest_line < max_width:
		custom_minimum_size.x = longest_line
		print(custom_minimum_size.x)
		#size.y = 22 * len(lines)
	else:
		custom_minimum_size.x = max_width
		#size.y = 22 * get_line_count()
	#position.x = size.x * -0.5
	#print(position)
	for i in sub_text.size():
		#var sub_label = sub_text[i]
		#sub_text[i].size = size
		sub_text[i].custom_minimum_size.x = custom_minimum_size.x
		#sub_text[i].position = sub_text_offsets[i]
		#sub_text[i].position = position + sub_text_offsets[i]
		sub_text[i].position = sub_text_offsets[i] + MAGIC_VECTOR
		# I don't know why things don't line up without this magic vector. They should. Why don't they.
		#print(sub_text[i].position)


func clear_sub_text() -> void:
	for sub_label in sub_text:
		sub_label.queue_free()
	sub_text_offsets.clear()


func add_shadow(distance:int) -> void:
	var shadow = create_new_label()
	shadow.modulate = shadow_color
	sub_text.append(shadow)
	var offset = Vector2(distance, distance)
	#shadow.position = position + offset
	shadow.position = offset + MAGIC_VECTOR
	sub_text_offsets.append(offset)
	#shadow.position = Vector2(distance, distance)


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
		#border_part.position = position + offset
		#print(position)
		#print(offset)
		#print(border_part.position)
		border_part.position = offset + MAGIC_VECTOR
		sub_text_offsets.append(offset)


#func _input(event: InputEvent) -> void:
#	if not Engine.is_editor_hint():
#		if event is InputEventKey:
#			reset_label_size()


func create_new_label() -> RichTextLabel:
	print("create", text)
	var new_label = RichTextLabel.new()
	add_child(new_label)
	new_label.bbcode_enabled = true
	new_label.show_behind_parent = true
	new_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	new_label.fit_content = true
	new_label.set_anchors_preset(Control.PRESET_CENTER)
	new_label.horizontal_alignment = horizontal_alignment
	new_label.vertical_alignment = vertical_alignment
	new_label.size = size
	new_label.theme = theme
	new_label.text = text
	return new_label
