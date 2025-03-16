@tool
class_name Insocket
extends PanelContainer


const TEXTURES = [
	preload("uid://dggd412l2rf2a"), #bool
	preload("uid://dbmsm68yejkx6"), #str
	preload("uid://bskasrjl73ytt"), #ACTOR,
	preload("uid://dp8jjyaborgsx"), #INT,
	#FLOAT,
	#TEXTURE2D,
	#VECTOR2,
	#NODE2D,
]


var type:int = 0:
	set(value):
		type = value
		var texid = clampi(type, 0, TEXTURES.size() - 1)
		add_theme_stylebox_override(&"panel", TEXTURES[texid])


func _init() -> void:
	var texid = clampi(type, 0, TEXTURES.size() - 1)
	add_theme_stylebox_override(&"panel", TEXTURES[texid])
