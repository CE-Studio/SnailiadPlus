# Copyright 2026 CE-Studio: AGPL-3.0-only
extends Particle


func _spawn(_data:Array) -> void:
	super(_data)
	sprite.play(_data[0])
