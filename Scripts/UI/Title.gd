class_name Title
extends Node2D


#region Variables
var title_string:String = "snailiad"
var letter:PackedScene = load("res://Scenes/UI/TitleLetter.tscn")

var valid_chars:Array = [
	"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
	"n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
]
#endregion


func _ready() -> void:
	title_string = title_string.to_lower()
	var spawn_pos = Vector2.ZERO
	for i in title_string.length():
		var char = title_string[i]
		match char:
			" ":
				spawn_pos.x += 24
			"+":
				pass
			_:
				if valid_chars.has(char):
					var new_letter = letter.instantiate()
					add_child(new_letter)
					new_letter.position = spawn_pos
					var advance_amount = new_letter.spawn(char, 0)
					spawn_pos.x += advance_amount + 4
					if i != title_string.length() - 1:
						position.x -= (advance_amount * 0.5) + 2
	position.x = roundi(position.x + 4)
