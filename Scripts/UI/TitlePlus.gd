class_name TitlePlus
extends Node2D

#region Variables
const START_DELAY = -2.0

var origin:Vector2
var life_time:float = 0.0

@onready var sprite:JsonSprite2D = $"JsonSprite2D"
#endregion


func spawn(delay:float) -> void:
	origin = position
	life_time = START_DELAY - delay


func _process(delta: float) -> void:
	if life_time >= 0.0 and sprite.action == "":
		sprite.action = "spawn"
	life_time += delta
