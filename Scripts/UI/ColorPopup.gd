# Copyright 2026 CE-Studio: AGPL-3.0-only
extends Node2D


#region Variables
const TOTAL_FADE_IN_TIME:float = 2.5
const CHAR_FADE_IN_MULT:float = 3.0
#const COLOR_CYCLE_MULT:float = 6.0
const FADE_OUT_TIME:float = 0.7

var raw_text:String = ""
var life_timer:float = 0.0
var offset_base:float = 0.0
var max_lifetime:float = 4.5
var color_list:Array[Color] = []
var children:Array = []

@onready var text:SnailyText = $"SnailyText"
#endregion


func instance(new_text:String, new_list:Array[Color], lifetime:float = 4.5, text_scale:int = 2, delay:float = 0.0) -> void:
	raw_text = new_text
	color_list = new_list.duplicate()
	max_lifetime = lifetime
	offset_base = TOTAL_FADE_IN_TIME / raw_text.length()
	#text.set_alignment(HORIZONTAL_ALIGNMENT_CENTER, VERTICAL_ALIGNMENT_CENTER)
	text.text_scale = text_scale
	#text.add_border(1)
	text.set_snaily_text(new_text)
	#text.call_deferred("center_position")
	life_timer = -abs(delay)


func _process(delta: float) -> void:
	life_timer += delta
	var bb_text:String = ""
	var lerp_time = life_timer * CHAR_FADE_IN_MULT
	var this_pointer = floori(lerp_time)
	var lerp_weight = lerp_time - this_pointer
	var pointer_a = clampi(this_pointer - 1, 0, color_list.size() - 1)
	var pointer_b = clampi(this_pointer, 0, color_list.size() - 1)
	var this_color = color_list[pointer_a].lerp(color_list[pointer_b], lerp_weight)
	for i in raw_text.length():
		this_color.a = clampf((life_timer * CHAR_FADE_IN_MULT) - (offset_base * i), 0.0, 1.0)
		if life_timer >= max_lifetime - FADE_OUT_TIME:
			this_color.a = inverse_lerp(max_lifetime, max_lifetime - FADE_OUT_TIME, life_timer) * this_color.a
		bb_text += "[color=%08x]%s" % [this_color.to_rgba32(), raw_text[i]]
	text.set_snaily_text(bb_text)
	if life_timer >= max_lifetime - FADE_OUT_TIME:
		if children.size() == 0 and get_child_count() > 0:
			children.append_array(get_children())
		for child in children:
			child.modulate.a = inverse_lerp(max_lifetime, max_lifetime - FADE_OUT_TIME, life_timer)
	if life_timer >= max_lifetime:
		queue_free()
