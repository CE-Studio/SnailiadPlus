# Copyright 2026 CE-Studio: AGPL-3.0-only
@tool
@icon("res://Editor/ico/SnailyText.svg")
class_name SnailyText
extends RichTextLabel


@warning_ignore("unused_private_class_variable")
@export_tool_button("Force Update") var _blech:Callable = reset_label_size


#region Variables
## @deprecated
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
@export var line_separation:int = 0:
	set(value):
		line_separation = value
		add_theme_constant_override("line_separation", value)

const CONTROL_PATH:String = "[img]res://Assets/Images/UI/ControlIcons/%s.png[/img]"
const DEFAULT_TIMEOUT:float = 0.02

const RAINBOW_CYCLE_TIME:float = 0.0625
var rainbow_colors:Array[Color] = [
	Statics.get_color(Vector2i(0, 0))
]

## Quick reference to the font file
var font:FontFile = load("res://Resources/SnailplanesExtended.ttf")

## The color of the text shadow/border
var shadow_color:Color = Color(0.0, 0.0, 0.0)

## Internal copy of whatever [String] is being displayed, before any special parsing occurs
var internal_text:String = ""
## This label's parent label, if any exists. If none is set, this label will act as a standalone/parent.
## If it is set, this label will ignore outside influence and follow its parent's settings and position.
var parent_label:SnailyText = null

## Whether or not the text is cycling through rainbow colors
var rainbow_active:bool = false
## Whether or not the text is cycling through rainbow colors in a scrolling manner
var rainbow_scroll_active:bool = false
## How long it has been since the last rainbow color change
var rainbow_elapsed:float = 0.0
## The current color index that will be pulled next for the rainbow effect
var rainbow_index:int = 0

## Array containing any additional label components used for shadows or borders
@onready var sub_text:Array[RichTextLabel] = []
## Array containing the positional offsets for each child text node
@onready var sub_text_offsets:Array = []
#endregion


func _ready() -> void:
	bbcode_enabled = true
	fit_content = true
	scroll_active = false
	clip_contents = false
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	if not Engine.is_editor_hint():
		if quick_load_text.strip_edges() != "":
			set_snaily_text(quick_load_text.replace("\\n", "\n"))
		if shadow_scale > 0:
			add_shadow(shadow_scale)
		if border_scale > 0:
			add_border(border_scale)
	reset_rainbow()


func _process(delta: float) -> void:
	if parent_label:
		size = parent_label.size
		text = parent_label.text
		text_scale = parent_label.text_scale
		internal_text = parent_label.internal_text
		horizontal_alignment = parent_label.horizontal_alignment
		vertical_alignment = parent_label.vertical_alignment
		visible_ratio = parent_label.visible_ratio
	
	if rainbow_active or rainbow_scroll_active:
		rainbow_elapsed += delta
		if rainbow_elapsed >= RAINBOW_CYCLE_TIME:
			rainbow_elapsed -= RAINBOW_CYCLE_TIME
			rainbow_index = (rainbow_index + 1) % rainbow_colors.size()
			if rainbow_active:
				modulate = rainbow_colors[rainbow_index]
			elif rainbow_scroll_active:
				apply_rainbow_scroll()


## Sets this [SnailyText] up as a shadow/border child of another [SnailyText]
func setup_as_child(_parent:SnailyText, _offset:Vector2i) -> void:
	parent_label = _parent
	modulate = Color.BLACK
	show_behind_parent = true
	size = _parent.size
	text = _parent.text
	text_scale = _parent.text_scale
	internal_text = _parent.internal_text
	horizontal_alignment = _parent.horizontal_alignment
	vertical_alignment = _parent.vertical_alignment
	line_separation = _parent.line_separation
	parent_label.add_child(self)
	position = _offset


## Sets the displayed text to the new [String]
func set_snaily_text(_text:String, _rescale:bool = false) -> void:
	_text = format_extra_tags(_text)
	text = _text
	for sub_label in sub_text:
		sub_label.text = _text
	if _rescale:
		rescale_horizontal(true)


## Sets the text alignment
func set_alignment(horiz:int, vert:int) -> void:
	horizontal_alignment = horiz as HorizontalAlignment
	vertical_alignment = vert as VerticalAlignment
	for sub_label in sub_text:
		sub_label.horizontal_alignment = horiz as HorizontalAlignment
		sub_label.vertical_alignment = vert as VerticalAlignment


## Recalculates the label's size based on the set maximum size and text contained within
## @deprecated
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


## Recalculates the minimum size of the label to match the longest line of text
func rescale_horizontal(_limit:bool = false) -> void:
	var longest_line:int = get_width()
	var scale_mod:float = float(text_scale) * 0.5
	custom_maximum_size.x = ceili(longest_line * scale_mod)
	if _limit and custom_maximum_size.x > 400:
		custom_maximum_size.x = 400
	for _text in sub_text:
		_text.custom_maximum_size.x = custom_maximum_size.x


## Resets the minimum size of the label
func reset_rescale() -> void:
	custom_minimum_size.x = 0.0
	custom_maximum_size.x = -1.0
	for _text in sub_text:
		_text.custom_minimum_size.x = 0.0
		_text.custom_maximum_size.x = -1.0


## Moves the text to be centered horizontally on its previous position
func center_position() -> void:
	position.x = custom_minimum_size.x * -0.5


## Frees all child text nodes, removing active shadows and borders
func clear_sub_text() -> void:
	for sub_label in sub_text:
		sub_label.queue_free()
	sub_text_offsets.clear()


## Adds a shadow, offset down-right from the main text by the given number of pixels
func add_shadow(distance:int) -> void:
	var offset:Vector2i = Vector2i(distance, distance)
	sub_text.append(create_child_text(offset))
	sub_text_offsets.append(offset)


## Adds a border made of four chid text nodes offset in each cardinal direction from the main text
## by the given number of pixels
func add_border(distance:int) -> void:
	var offset:Vector2i = Vector2i(0, -distance)
	for i in range(4):
		match i:
			1: offset = Vector2i(distance, 0)
			2: offset = Vector2i(0, distance)
			3: offset = Vector2i(-distance, 0)
		sub_text.append(create_child_text(offset))
		sub_text_offsets.append(offset)


## Instances and sets up a new [RichTextLabel] for use as a child
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
	new_label.add_theme_constant_override("line_separation", line_separation)
	new_label.add_theme_font_size_override("normal_font_size", 8 * text_scale)
	new_label.clip_contents = false
	new_label.size = size
	new_label.theme = theme
	new_label.text = text
	new_label.autowrap_mode = self.autowrap_mode
	return new_label


## Instances and sets up a new [SnailyText] for use as a child
func create_child_text(_offset:Vector2i) -> SnailyText:
	var new_text:SnailyText = SnailyText.new()
	new_text.setup_as_child(self, _offset)
	return new_text


## Sets the number of visible characters this text can display
func set_visible_chars_count(count:int) -> void:
	visible_characters = count
	for sub_label in sub_text:
		sub_label.visible_characters = count


## Sets the number of visible characters this text can display based on a ratio from 0.0 to 1.0
func set_visible_chars_ratio(ratio:float) -> void:
	visible_ratio = clamp(ratio, 0.0, 1.0)
	for sub_label in sub_text:
		sub_label.visible_ratio = clamp(ratio, 0.0, 1.0)


## Returns the maximum width of this text object, inferred from the longest line of text
func get_width(_text:String = text) -> int:
	var longest_line = 0
	var lines = internal_text.split("\n")
	for line in lines:
		var line_length = font.get_string_size(line).x
		if longest_line < line_length:
			longest_line = line_length
	return longest_line


## Parses custom tags for things like control inputs, converting them into a format that can be
## read and handled by BBcode
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


## Sets the text color
func set_color(col:Color) -> void:
	modulate = col
	rainbow_active = false
	rainbow_scroll_active = false

## Sets the text color using a given palette ID
func set_color_from_id(id:Vector2i) -> void:
	modulate = Statics.get_color(id)
	rainbow_active = false
	rainbow_scroll_active = false


## Enables the flashing rainbow effect
func enable_rainbow() -> void:
	if rainbow_colors.size() == 0:
		return
	rainbow_active = true
	rainbow_scroll_active = false
	rainbow_elapsed = 0.0
	rainbow_index = 0
	modulate = rainbow_colors[0]


## Enables the scrolling rainbow effect
func enable_rainbow_scroll() -> void:
	if rainbow_colors.size() == 0:
		return
	rainbow_scroll_active = true
	rainbow_active = false
	rainbow_elapsed = 0.0
	rainbow_index = 0
	apply_rainbow_scroll()


## Applies a new color array to rainbow mode
func overwrite_rainbow(new_list:Array[Color]) -> void:
	rainbow_colors = new_list.duplicate()


## Resets the rainbow to its initial true rainbow state
func reset_rainbow() -> void:
	rainbow_colors = [
		Statics.get_color(Vector2i(1, 2)),
		Statics.get_color(Vector2i(2, 2)),
		Statics.get_color(Vector2i(2, 3)),
		Statics.get_color(Vector2i(2, 4)),
		Statics.get_color(Vector2i(3, 4)),
		Statics.get_color(Vector2i(2, 5)),
		Statics.get_color(Vector2i(2, 7)),
		Statics.get_color(Vector2i(2, 6)),
		Statics.get_color(Vector2i(2, 8)),
		Statics.get_color(Vector2i(2, 9)),
		Statics.get_color(Vector2i(1, 9)),
		Statics.get_color(Vector2i(0, 9)),
		Statics.get_color(Vector2i(0, 11)),
		Statics.get_color(Vector2i(0, 12)),
		Statics.get_color(Vector2i(1, 12)),
		Statics.get_color(Vector2i(1, 13)),
	]


## Applies a rainbow scroll to the visible text
func apply_rainbow_scroll() -> void:
	var this_i:int = rainbow_index
	var in_str:String = internal_text
	var out_str:String = ""
	for chr in in_str:
		out_str += "[color=%s]%s" % [rainbow_colors[this_i].to_html(), chr]
		this_i = (this_i + 1) % rainbow_colors.size()
	text = out_str
