# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name MarkerSprite
extends Sprite2D
## A temporary sprite that removes itself on scene load.


func _ready() -> void:
	queue_free()
