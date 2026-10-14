class_name ControlTutorial
extends Node2D


const HALF_TIME:float = 4.0
const EASE:float = 0.4
const OUT_X:float = -300.0

var elapsed:float
var ease_out:bool = false
var origin_x:float = 0.0
@export var tut_start:Control
@export var tut_shoot:Control


func _ready() -> void:
	origin_x = position.x


func set_active_tutorial(id:int) -> void:
	if id != 0: tut_start.visible = false
	if id != 1: tut_shoot.visible = false


func _process(delta: float) -> void:
	var this_weight:float = clampf(elapsed, 0.0, 1.0)
	this_weight = ease(this_weight, EASE)
	position.x = lerpf(origin_x + OUT_X, origin_x, this_weight)
	if ease_out:
		elapsed -= delta
		if elapsed < 0.0:
			queue_free()
	else:
		elapsed += delta
		if elapsed >= HALF_TIME:
			ease_out = true
