@tool
@icon("res://Editor/ico/SnailyText.svg")
class_name SnailyText
extends RichTextLabel


@warning_ignore("unused_private_class_variable")
@export_tool_button("Force Update") var _blech:Callable = reset_label_size


#region Variables
@export var max_width:int = 0:
	set(value):
		max_width = value
		custom_minimum_size.x = value
@export var text_scale:int = 2:
	set(value):
		text_scale = value
		add_theme_font_size_override("normal_font_size", 8 * value)
		for sub_label in sub_text:
			sub_label.add_theme_font_size_override("normal_font_size", 8 * value)
@export var quick_load_text:String = ""
@export var shadow_scale:int = 0
@export var border_scale:int = 0

const CONTROL_PATH:String = "[img]res://Assets/Images/UI/ControlIcons/%s.png[/img]"
const DEFAULT_TIMEOUT:float = 0.02

var menu_theme:Theme = load("res://Resources/MenuTheme.tres")
var font:FontFile = load("res://Resources/SnailplanesExtended.ttf")

var shadow_color:Color = Color(0.0, 0.0, 0.0)

var char_timeouts:Array[float] = []
var internal_text:String = ""

@onready var sub_text:Array[RichTextLabel] = []
@onready var sub_text_offsets:Array = []
#endregion


func _ready() -> void:
	if not Engine.is_editor_hint():
		if quick_load_text.strip_edges() != "":
			set_snaily_text(quick_load_text)
		if shadow_scale > 0:
			add_shadow(shadow_scale)
		if border_scale > 0:
			add_border(border_scale)


func set_snaily_text(_text:String) -> void:
	set_snaily_text_raw(Statics.get_text(_text))


func set_snaily_text_raw(_text:String) -> void:
	_text = format_extra_tags(_text)
	text = _text
	for sub_label in sub_text:
		sub_label.text = _text
	reset_label_size.call_deferred()


func set_alignment(horiz:int, vert:int) -> void:
	horizontal_alignment = horiz as HorizontalAlignment
	vertical_alignment = vert as VerticalAlignment
	for sub_label in sub_text:
		sub_label.horizontal_alignment = horiz as HorizontalAlignment
		sub_label.vertical_alignment = vert as VerticalAlignment


func reset_label_size() -> void:
	var longest_line = get_width()
	var scale_mod = float(text_scale) * 0.5
	if max_width == 0 or longest_line < max_width:
		custom_minimum_size.x = ceil(longest_line * scale_mod)
	else:
		custom_minimum_size.x = max_width
	size.x = custom_minimum_size.x
	for i in sub_text.size():
		sub_text[i].custom_minimum_size.x = custom_minimum_size.x
		sub_text[i].size.x = sub_text[i].custom_minimum_size.x
		sub_text[i].position = sub_text_offsets[i]


func center_position() -> void:
	position.x = custom_minimum_size.x * -0.5


func clear_sub_text() -> void:
	for sub_label in sub_text:
		sub_label.queue_free()
	sub_text_offsets.clear()


func add_shadow(distance:int) -> void:
	var shadow = create_new_label()
	shadow.modulate = shadow_color
	sub_text.append(shadow)
	var offset = Vector2(distance, distance)
	shadow.position = offset
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
		border_part.position = offset
		sub_text_offsets.append(offset)


func create_new_label() -> RichTextLabel:
	var new_label = RichTextLabel.new()
	add_child(new_label)
	new_label.bbcode_enabled = true
	new_label.show_behind_parent = true
	new_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	new_label.fit_content = true
	new_label.set_anchors_preset(Control.PRESET_TOP_LEFT)
	new_label.horizontal_alignment = horizontal_alignment
	new_label.vertical_alignment = vertical_alignment
	new_label.add_theme_constant_override("line_separation", get_theme_constant("line_separation"))
	new_label.add_theme_font_size_override("normal_font_size", 8 * text_scale)
	new_label.clip_contents = false
	new_label.size = size
	new_label.theme = theme
	new_label.text = text
	new_label.autowrap_mode = self.autowrap_mode
	return new_label


func set_visible_chars_count(count:int) -> void:
	visible_characters = count
	for sub_label in sub_text:
		sub_label.visible_characters = count


func set_visible_chars_ratio(ratio:float) -> void:
	visible_ratio = clamp(ratio, 0.0, 1.0)
	for sub_label in sub_text:
		sub_label.visible_ratio = clamp(ratio, 0.0, 1.0)


func get_width(_text:String = text) -> int:
	var longest_line = 0
	var lines = internal_text.split("\n")
	for line in lines:
		var line_length = font.get_string_size(line).x
		if longest_line < line_length:
			longest_line = line_length
	return longest_line


func format_extra_tags(_text:String) -> String:
	var split_words:PackedStringArray = _text.split(" ")
	var reassembled_str:PackedStringArray = []
	var internal_reassembled_str:PackedStringArray = []
	for word in split_words:
		if word.contains("__"):
			var parts = word.split("__")
			match parts[0]:
				"ctrl":
					reassembled_str.append(CONTROL_PATH % parts[1])
					internal_reassembled_str.append("LL")
				"bind":
					var icon:String = SInput.get_icon_from_enum_str(parts[1])
					reassembled_str.append(CONTROL_PATH % icon)
					internal_reassembled_str.append("LL")
				_:
					reassembled_str.append(word)
					internal_reassembled_str.append(word)
		else:
			reassembled_str.append(word)
			internal_reassembled_str.append(word)

	internal_text = " ".join(internal_reassembled_str)
	return " ".join(reassembled_str)
	#var parsed_text:String = ""
	#var parsed_tag:String = ""
	#var current_timeout:float = DEFAULT_TIMEOUT
	#var open_tag:bool = false
	#for char in _text:
	#	match char:
	#		"[":
	#			pass
	#		"]":
	#			pass
	#		_:
	#			if open_tag:
	#				parsed_tag += char
	#			else:
	#				parsed_text += char
	#				char_timeouts.append(current_timeout)
	#return parsed_text
