@tool
class_name Outsocket
extends TextureRect


const TEXTURES = [
	preload("uid://dhuss2ltx0bac"), #bool
	preload("uid://cad6u5u1gf3hw"), #str
	preload("uid://wipjg26ehwi5"), #ACTOR,
	preload("uid://d2nab8bc05grq"), #INT,
	#FLOAT,
	#TEXTURE2D,
	#VECTOR2,
	#NODE2D,
]


var type:int = 0:
	set(value):
		type = value
		var texid = clampi(type, 0, TEXTURES.size() - 1)
		texture = TEXTURES[texid]


func _init() -> void:
	var texid = clampi(type, 0, TEXTURES.size() - 1)
	texture = TEXTURES[texid]
