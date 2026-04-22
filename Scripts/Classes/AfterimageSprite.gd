class_name AfterimageSprite
extends Sprite2D


var start_a:float = 1.0
var end_a:float = 0.0
var fade_time:float = 1.0

var elapsed:float = 0.0


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
