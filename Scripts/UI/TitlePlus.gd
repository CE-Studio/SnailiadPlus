# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name TitlePlus
extends AnimatedSprite2D

#region Variables
const START_DELAY = -2.0

var origin:Vector2
var life_time:float = 0.0
var started_playing:bool = false

@onready var sprite:JsonSprite2D = $"JsonSprite2D"
#endregion


func spawn(delay:float) -> void:
	origin = position
	life_time = START_DELAY - delay


func _process(delta: float) -> void:
	#if life_time >= 0.0 and sprite.action == "":
	if life_time >= 0.0 and not is_playing() and not started_playing:
		#sprite.action = "spawn"
		play("appear")
		started_playing = true
	life_time += delta


func _on_anim_finish() -> void:
	play("loop")
