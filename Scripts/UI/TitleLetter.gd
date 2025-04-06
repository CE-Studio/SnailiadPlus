class_name TitleLetter
extends Node2D


#region Variables
var letter:String
var origin:Vector2
var life_time:float

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
	origin = position
	life_time = -delay
	var letter_id = letter_ids[letter]
	return sprite.meta["widths"][letter_id]
