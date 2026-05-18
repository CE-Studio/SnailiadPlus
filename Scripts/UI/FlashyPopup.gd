# Copyright 2026 CE-Studio: AGPL-3.0-only
extends Node2D


#region Variables
const TOTAL_FADE_IN_TIME:float = 1.0
const CHAR_FADE_IN_MULT:float = 3.25
const COLOR_CYCLE_MULT:float = 6.0
const FADE_OUT_TIME:float = 0.7

var raw_text:String = ""
var life_timer:float = 0.0
var offset_base:float = 0.0
var max_lifetime:float = 3.5

@onready var text:SnailyText = $"SnailyText"
#endregion


func instance(new_text:String, lifetime:float = 3.5, text_scale:int = 2, delay:float = 0.0) -> void:
	raw_text = new_text
	max_lifetime = lifetime
	offset_base = TOTAL_FADE_IN_TIME / raw_text.length()
	text.text_scale = text_scale
	text.set_snaily_text(new_text)
	life_timer = -abs(delay)


func _process(delta: float) -> void:
	life_timer += delta
	var bb_text:String = ""
	for i in raw_text.length():
		var this_color:Color = Color.WHITE
		var color_pointer = floori((life_timer - (offset_base * i) + ceili(TOTAL_FADE_IN_TIME)) * COLOR_CYCLE_MULT) % 4
		match color_pointer:
			0: this_color = Statics.get_color(Vector2i(0, 1))
			1: this_color = Statics.get_color(Vector2i(3, 3))
			2: this_color = Statics.get_color(Vector2i(3, 13))
			3: this_color = Statics.get_color(Vector2i(3, 7))
		this_color.a = clampf((life_timer * CHAR_FADE_IN_MULT) - (offset_base * i), 0.0, 1.0)
		if life_timer >= max_lifetime - FADE_OUT_TIME:
			this_color.a = inverse_lerp(max_lifetime, max_lifetime - FADE_OUT_TIME, life_timer) * this_color.a
		bb_text += "[color=%08x]%s" % [this_color.to_rgba32(), raw_text[i]]
	text.set_snaily_text(bb_text)
	if life_timer >= max_lifetime:
		queue_free()
