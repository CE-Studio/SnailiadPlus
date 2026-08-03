# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name SpeechBubble
extends Node2D


#region Variables
var direction:String = "D"
var text_delay:float = 0.375
var elapsed:float = 0.0
var shown:bool = false
var text_shown:bool = false

@export var sprite:SnailySprite2D
@export var text:SnailyText
#endregion


func _ready() -> void:
	sprite.visible = false
	text.visible = false


func _process(delta: float) -> void:
	elapsed += delta
	if elapsed >= text_delay and shown and not text_shown:
		text_shown = true
		text.visible = true
		_update_text()


func show_bubble() -> void:
	if shown:
		return
	shown = true
	sprite.visible = true
	text.visible = false
	sprite.play(direction + "_opening")
	sprite.autoplay_next = direction + "_open"
	elapsed = 0.0


func hide_bubble() -> void:
	if not shown:
		return
	shown = false
	text_shown = false
	text.visible = false
	sprite.play(direction + "_closing")
	sprite.autoplay_next = direction + "_closed"


func _update_text() -> void:
	text.set_snaily_text(SInput.get_icon_as_bbcode(SInput.Inputs.SPEAK))


func _on_sprite_anim_updated() -> void:
	if sprite.animation == direction + "_open":
		text.visible = true


func set_direction(new_dir:Statics.DirsSurface, distance:int = 24) -> void:
	match new_dir:
		Statics.DirsSurface.FLOOR:
			direction = "D"
			position = Vector2.UP * distance
		Statics.DirsSurface.LWALL:
			direction = "L"
			position = Vector2.RIGHT * distance
		Statics.DirsSurface.RWALL:
			direction = "R"
			position = Vector2.LEFT * distance
		Statics.DirsSurface.CEILING:
			direction = "U"
			position = Vector2.DOWN * distance
	if shown:
		sprite.play(direction + "_open")
