# Copyright 2026 CE-Studio: AGPL-3.0-only
extends Node2D


#region Variables
## The animation node that drives the entire stats screen
@export var anim:AnimationPlayer
## The background sprite
@export var end_bg:JsonSprite2D
## The snail sprite
@export var end_pic:JsonSprite2D
## The "congratulations!" header text
@export var header:SnailyText
## The text that displays the character and difficulty played with
@export var char_diff:SnailyText
## The container that holds all item text
@export var item_container:HBoxContainer
## The "items collected" header text
@export var item_header:SnailyText
## The percentage counter for item collection
@export var item_counter:SnailyText
## The container that holds all time text
@export var time_container:HBoxContainer
## The "completion time" text
@export var time_header:SnailyText
## The counter for time taken to beat the game
@export var time_counter:SnailyText
#endregion


func _ready() -> void:
	end_bg.modulate.a = 0.0
	end_pic.modulate.a = 0.0
	header.visible_ratio = 0.0
	header.set_snaily_text(tr(&"Congratulations!!"))
	char_diff.set_snaily_text(" - ".join([
		GlobalText.characters[Statics.current_profile["character"][0]],
		GlobalText.difficulties[Statics.current_profile["difficulty"]]
		]))
	item_header.set_snaily_text(tr(&"Items collected:"))
	time_header.set_snaily_text(tr(&"Completion time:"))


func start_anim() -> void:
	anim.play(&"Ending")
