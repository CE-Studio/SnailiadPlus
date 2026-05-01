# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name SpeechBubble
extends Node2D


#region Variables
var direction:String = "floor"
var text_delay:float = 0.375
var elapsed:float = 0.0
var shown:bool = false
var text_shown:bool = false

@onready var sprite:JsonSprite2D = $"JsonSprite2D"
@onready var text:SnailyText = $"SnailyText"
#endregion


func _ready() -> void:
	sprite.visible = false
	text.visible = false
	if sprite.meta.size() > 0:
		if sprite.meta.keys().has("icon_delay"):
			var new_delay = sprite.meta["icon_delay"]
			if Statics.is_number(new_delay, true):
				text_delay = clampf(new_delay, 0.0, 0.75)


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
	sprite.action = direction + "_open"
	elapsed = 0.0


func hide_bubble() -> void:
	if not shown:
		return
	shown = false
	text_shown = false
	text.visible = false
	sprite.action = direction + "_close"


func _update_text() -> void:
	#text.text = SInput.get_icon_as_bbcode(SInput.Inputs.SPEAK)
	text.set_snaily_text(SInput.get_icon_as_bbcode(SInput.Inputs.SPEAK))


func set_direction(new_dir:Statics.DirsSurface, distance:int = 24) -> void:
	match new_dir:
		Statics.DirsSurface.FLOOR:
			direction = "floor"
			position = Vector2.UP * distance
		Statics.DirsSurface.LWALL:
			direction = "lwall"
			position = Vector2.RIGHT * distance
		Statics.DirsSurface.RWALL:
			direction = "rwall"
			position = Vector2.LEFT * distance
		Statics.DirsSurface.CEILING:
			direction = "ceiling"
			position = Vector2.DOWN * distance
	if shown:
		sprite.action = direction + "_open"
