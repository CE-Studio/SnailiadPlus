@tool
@icon("res://Editor/ico/SnailyText.svg")
class_name SnailyText
extends RichTextLabel


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

var menu_theme:Theme = load("res://Resources/MenuTheme.tres")
var font:FontFile = load("res://Resources/SnailplanesExtended.ttf")

var shadow_color:Color = Color(0.0, 0.0, 0.0)

@onready var sub_text:Array = []
@onready var sub_text_offsets:Array = []
#endregion


func _ready() -> void:
	if quick_load_text.strip_edges() != "" and not Engine.is_editor_hint():
		set_snaily_text(Statics.get_text(quick_load_text))
		if shadow_scale > 0:
			add_shadow(shadow_scale)
		if border_scale > 0:
			add_border(border_scale)


func set_snaily_text(_text:String) -> void:
	text = _text
	for sub_label in sub_text:
		sub_label.text = _text
	reset_label_size.call_deferred()


func set_alignment(horiz:int, vert:int) -> void:
	horizontal_alignment = horiz
	vertical_alignment = vert
	for sub_label in sub_text:
		sub_label.horizontal_alignment = horiz
		sub_label.vertical_alignment = vert


func reset_label_size() -> void:
	var longest_line = 0
	var lines = text.split("\n")
	var scale_mod = float(text_scale) * 0.5
	for line in lines:
		var line_length = font.get_string_size(line).x
		if longest_line < line_length:
			longest_line = line_length
	if max_width == 0 or longest_line < max_width:
		custom_minimum_size.x = longest_line * scale_mod
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
	return new_label


func set_visible_chars_count(count:int) -> void:
	visible_characters = count
	for sub_label in sub_text:
		sub_label.visible_characters = count


func set_visible_chars_ratio(ratio:float) -> void:
	visible_ratio = clamp(ratio, 0.0, 1.0)
	for sub_label in sub_text:
		sub_label.visible_ratio = clamp(ratio, 0.0, 1.0)
