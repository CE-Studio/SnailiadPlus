# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
extends Particle


func _process(delta: float) -> void:
	position += Vector2.UP * 0.3333
	super._process(delta)
