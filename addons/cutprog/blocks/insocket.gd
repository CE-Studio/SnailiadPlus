@tool
class_name Insocket
extends PanelContainer


const TEXTURES = [
	preload("uid://dggd412l2rf2a"), #bool
	preload("uid://dbmsm68yejkx6"), #str
]


var type:int = 0:
	set(value):
		type = value
		add_theme_stylebox_override(&"panel", TEXTURES[type])


func _init() -> void:
	add_theme_stylebox_override(&"panel", textures[type])
