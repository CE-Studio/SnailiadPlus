extends Node2D


@export var anim:AnimationPlayer
@export var stars:StarLayer
@export var cover:Sprite2D
@export var first_moon:JsonSprite2D
@export var spotlight:JsonSprite2D


func _ready() -> void:
	cover.modulate.a = 1.0
	first_moon.modulate.a = 0.0
	spotlight.modulate.a = 0.0
	stars.force_process()
	if Statics.get_item_percentage() >= 100:
		anim.play(&"Sun")
	else:
		anim.play(&"Moon")
