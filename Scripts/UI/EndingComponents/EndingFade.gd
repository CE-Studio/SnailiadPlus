class_name EndingFade
extends Node2D


const FADE_START_TIME:float = 2.0
const FADE_END_TIME:float = 6.5
const END_SPAWN_TIME:float = 10.0

static var instance:EndingFade

var elapsed:float = 0.0
var started_ui_fade:bool = false
var fade_in:bool = true


func _ready() -> void:
	instance = self
	modulate.a = 0.0


func _process(delta: float) -> void:
	if fade_in:
		modulate.a = inverse_lerp(FADE_START_TIME, FADE_END_TIME, elapsed)
		if elapsed >= FADE_START_TIME and not started_ui_fade:
			UICore.instance.fade_ui(0.0, FADE_END_TIME - FADE_START_TIME)
			started_ui_fade = true
		elapsed += delta
		if elapsed > END_SPAWN_TIME:
			fade_in = false
			elapsed = 1.0
			GameCore.instance.add_child(load("res://Scenes/UI/EndingComponents/EndingCutscene.tscn").instantiate())
			get_tree().paused = true
