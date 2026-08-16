# Copyright 2026 CE-Studio: AGPL-3.0-only
extends SnailySprite2D


var player:Player
var p_spr:SnailySprite2D


func _ready() -> void:
	if not GameCore.instance or not Player.instance:
		queue_free()
		return
	assert(GameCore.instance.player, "No Player set in GameCore! A Player must exist for the PlayerSilhouette to work.")
	player = Player.instance
	p_spr = player.sprite
	sprite_frames = p_spr.sprite_frames
	p_spr.animation_changed.connect(_on_p_anim_changed)
	_on_p_anim_changed()


func _process(_delta:float) -> void:
	if not GameCore.instance or not Player.instance:
		return
	global_position = player.global_position


func _on_p_anim_changed() -> void:
	play(p_spr.animation)
	frame = p_spr.frame
	flip_h = p_spr.flip_h
	flip_v = p_spr.flip_v
