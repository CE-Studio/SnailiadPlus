# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name RemoteFullHeal
extends Node2D


#region Variables
const PITCH_MIN:float = 1.0
const PITCH_MAX:float = 1.4

var steps:int = 0
var steps_done:int = 0
var duration:float = 0.0
var elapsed:float = 0.0
var amount_per_step:int = 0
var ease_mode:int = 0
var running:bool = false

@export var sfx_heal:AudioStreamPlayer

signal heal(amount:int)
#endregion


func _ready() -> void:
	assert(sfx_heal, "RemoteFullHeal requires an AudioStreamPlayer to be connected")


func _process(delta: float) -> void:
	if running:
		elapsed += delta
		var weight:float = inverse_lerp(0.0, duration, elapsed)
		weight = _get_ease_weight(weight)
		while weight * steps >= steps_done:
			sfx_heal.pitch_scale = lerp(PITCH_MIN, PITCH_MAX, weight)
			sfx_heal.play()
			heal.emit(amount_per_step)
			steps_done += 1
		if elapsed > duration:
			running = false


func setup_heal(_amount:int, _amount_per_step:int, _duration:float, _easing:int, _delay:float = 0.0) -> void:
	if _amount <= 0:
		return
	amount_per_step = _amount_per_step
	steps = floori(float(_amount) / float(_amount_per_step))
	steps_done = 0
	duration = _duration
	elapsed = -abs(_delay)
	ease_mode = _easing
	running = true


func _get_ease_weight(current_weight:float) -> float:
	var _sign:float = sign(current_weight)
	var out_weight:float = current_weight
	match ease_mode:
		-2: # Cubic in
			out_weight = current_weight * current_weight * current_weight
		-1: # Quad in
			out_weight = current_weight * current_weight
		0: # Linear
			out_weight = current_weight # Linear
		1: # Quad out
			out_weight = 1.0 - (1.0 - current_weight) * (1.0 - current_weight)
		2: # Cubic out
			out_weight = 1.0 - pow(1.0 - current_weight, 3.0)
	return abs(out_weight) * _sign
