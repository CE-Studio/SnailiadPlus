# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name TitleLetter
extends Node2D


#region Variables
const POS_SCALE:int = 80
const START_TIME:float = -2.5

var letter:String
var origin:Vector2
var life_time:float
var intro_anim_complete:bool = false
var y_loop:float = 0.0

var letter_ids:Dictionary = {
	"a": 0,
	"b": 1,
	"c": 2,
	"d": 3,
	"e": 4,
	"f": 5,
	"g": 6,
	"h": 7,
	"i": 8,
	"j": 9,
	"k": 10,
	"l": 11,
	"m": 12,
	"n": 13,
	"o": 14,
	"p": 15,
	"q": 16,
	"r": 17,
	"s": 18,
	"t": 19,
	"u": 20,
	"v": 21,
	"w": 22,
	"x": 23,
	"y": 24,
	"z": 25
}

@onready var sprite:JsonSprite2D = $"JsonSprite2D"
#endregion


func spawn(new_letter:String, delay:float) -> int:
	letter = new_letter
	sprite.action = letter + ".spawn"
	sprite.modulate.a = 0.0
	origin = position
	life_time = START_TIME - delay
	var letter_id = letter_ids[letter]
	y_loop = randf_range(-0.5, 0.5)
	
	var out_width:int = 32
	if sprite.meta.size() > 0 and sprite.meta.keys().has("widths"):
		if sprite.meta["widths"].size() > letter_id:
			var width = sprite.meta["widths"][letter_id]
			if Statics.is_number(width):
				out_width = clampi(width, 0, 128)
	return out_width


func  _process(delta: float) -> void:
	if life_time < START_TIME:
		pass
	elif life_time < 0.0:
		sprite.modulate.a = 1.0
		position.x = origin.x - sin(-life_time * PI) * life_time * POS_SCALE
		#position.y = origin.y - cos(-life_time * PI) * life_time * POS_SCALE * y_loop
	elif not intro_anim_complete:
		intro_anim_complete = true
		sprite.action = letter + ".idle"
		position = origin
	life_time += delta
