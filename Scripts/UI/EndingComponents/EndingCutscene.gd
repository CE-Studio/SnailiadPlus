# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
extends Node2D


const CHAR_TIMEOUT:float = 0.067
const DIALOGUE_FADE_RATE:float = 0.9

var dialogue_moon:PackedStringArray = [
	tr(&"    And so..."),
	tr(&"    Defeated, Moon Snail\n        lost his powers"),
	tr(&"    But could he ever become\n        Sun Snail once more?"),
]
var dialogue_sun:PackedStringArray = [
	tr(&"    And so..."),
	tr(&"    Moon Snail once again\n        regained his light"),
	tr(&"    And became the\n        legendary Sun Snail"),
]

var is_sun:bool = false
var target_str:String = ""
var current_char:int = 0
var char_timeout:float = 0.0
var dialogue_visible:bool = false

@export var anim:AnimationPlayer
@export var stars:StarLayer
@export var cover:Sprite2D
@export var first_moon:JsonSprite2D
@export var spotlight:JsonSprite2D
@export var dialogue:SnailyText
@export var sfx_dialogue:AudioStreamPlayer


func _ready() -> void:
	UICore.instance.cam.set_to_static_pos()
	GameCore.instance.music_manager.stop_all()
	cover.modulate.a = 1.0
	first_moon.modulate.a = 0.0
	spotlight.modulate.a = 0.0
	stars.spawn()
	if Statics.get_item_percentage() >= 100:
		is_sun = true
		anim.play(&"Sun")
	else:
		anim.play(&"Moon")
	dialogue.set_snaily_text("")


func _process(delta: float) -> void:
	if not dialogue_visible and dialogue.modulate.a > 0.0:
		dialogue.modulate.a -= delta * DIALOGUE_FADE_RATE
	if current_char < target_str.length():
		if char_timeout <= 0.0:
			char_timeout = CHAR_TIMEOUT
			var this_char:String = " "
			while (this_char == " " or this_char == "\n") and current_char < target_str.length():
				this_char = target_str[current_char]
				dialogue.set_snaily_text(dialogue.text + this_char)
				current_char += 1
			sfx_dialogue.play()
		else:
			char_timeout -= delta


func set_text(id:int) -> void:
	dialogue_visible = true
	dialogue.modulate.a = 1.0
	dialogue.set_snaily_text("")
	char_timeout = 0.0
	current_char = 0
	if is_sun:
		target_str = dialogue_sun[id]
	else:
		target_str = dialogue_moon[id]


func fade_text() -> void:
	dialogue_visible = false
