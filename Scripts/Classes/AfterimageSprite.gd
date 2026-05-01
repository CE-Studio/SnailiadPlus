# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name AfterimageSprite
extends Sprite2D


## The alpha (from 0.0 to 1.0) that this sprite starts at
var start_a:float = 1.0
## The alpha (from 0.0 to 1.0) that this sprite should approach over its lifetime
var end_a:float = 0.0
## How long it should take in seconds for this sprite to fade to its target alpha
var fade_time:float = 1.0

## How much time in seconds has passed since this sprite was spawned
var elapsed:float = 0.0


## Properly initializes this sprite with whatever alpha, fade time, and other configurations
## are required
func setup(_source_sprite:JsonSprite2D, _frame_coords:Vector2i, _start_a:float, _end_a:float, _fade_time:float, _z:int) -> void:
	texture = _source_sprite.texture
	hframes = _source_sprite.hframes
	vframes = _source_sprite.vframes
	frame_coords = _frame_coords
	start_a = _start_a
	end_a = _end_a
	fade_time = _fade_time
	modulate.a = _start_a
	z_index = _z
	global_position = _source_sprite.global_position


func _process(delta: float) -> void:
	if texture:
		var weight:float = inverse_lerp(0.0, fade_time, elapsed)
		modulate.a = lerp(start_a, end_a, weight)
		if elapsed > fade_time:
			queue_free()
		elapsed += delta
