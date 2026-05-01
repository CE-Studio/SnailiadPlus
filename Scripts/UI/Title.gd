# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name Title
extends Node2D


#region Variables
const LETTER_SPACING:int = 4
const LETTER_DELAY:float = PI / 11.0
const RARE_CHANCE:float = 0.005

var title_string:String = ""
var letter:PackedScene = load("res://Scenes/UI/TitleLetter.tscn")
var plus:PackedScene = load("res://Scenes/UI/TitlePlus.tscn")

var valid_chars:Array = [
	"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
	"n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
]
#endregion


func _ready() -> void:
	title_string = tr(&"snailiad+")
	if randf() <= RARE_CHANCE:
		match floori(randf() * 4):
			0: title_string = tr(&"snaliad+")
			1: title_string = tr(&"snailaid+")
			2: title_string = tr(&"snailidad+")
			3: title_string = tr(&"snailad+")
	title_string = title_string.to_lower()
	var spawn_pos = Vector2.ZERO
	var spawn_delay = 0.0
	for i in title_string.length():
		var this_char = title_string[i]
		match this_char:
			" ":
				spawn_pos.x += 24
			"+":
				var new_plus = plus.instantiate()
				add_child(new_plus)
				new_plus.position = spawn_pos
				new_plus.position.x -= 4
				new_plus.spawn(spawn_delay)
				if i != title_string.length() - 1:
					spawn_pos.x += 32 + LETTER_SPACING - 8
				spawn_delay += LETTER_DELAY
			_:
				if valid_chars.has(this_char):
					var new_letter = letter.instantiate()
					add_child(new_letter)
					new_letter.position = spawn_pos
					var advance_amount = new_letter.spawn(this_char, spawn_delay)
					if i != title_string.length() - 1:
						spawn_pos.x += advance_amount + LETTER_SPACING
					spawn_delay += LETTER_DELAY
	position.x = roundi(position.x - (spawn_pos.x * 0.5) + LETTER_SPACING)
