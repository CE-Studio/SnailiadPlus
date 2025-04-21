class_name ColorCover
extends Sprite2D


#region Variables
@export var start_color:Color = Color(0.0, 0.0, 0.0, 1.0)
@export var end_color:Color = Color(0.0, 0.0, 0.0, 0.0)
@export_range(0.0, 10.0, 0.01) var fade_time:float = 0.5

var current_time:float = 0.0
#endregion


func _ready() -> void:
	modulate = start_color


func _process(delta: float) -> void:
	if current_time > fade_time:
		modulate = end_color
	else:
		modulate = start_color.lerp(end_color, inverse_lerp(0.0, fade_time, current_time))
		current_time += delta


func set_new_fade(start:Color, end:Color, _fade_time:float) -> void:
	start_color = start
	end_color = end
	fade_time = _fade_time
	current_time = 0.0
	modulate = start
