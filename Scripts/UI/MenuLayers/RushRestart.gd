# Copyright 2026 CE-Studio: AGPL-3.0-only
extends VBoxContainer


@onready var layer:MenuLayer = get_parent()
@export var anim:AnimationPlayer


func on_quit_button_pressed(_value) -> void:
	layer.can_focus = false
	layer.menu.read_inputs = false
	SInput.read_inputs = false
	GameCore.instance.music_manager.set_fade(0.0, 1.8)
	anim.play("fade")


func _reload_boss_rush() -> void:
	Statics.setup_bossrush_profile(Statics.current_profile["character"])
	Statics.load_room = Statics.ROOM_PATH % Statics.RUSH_SPAWN[0]
	Statics.load_coords = Vector2(Statics.RUSH_SPAWN[1], Statics.RUSH_SPAWN[2])
	get_tree().change_scene_to_file("uid://ltxtlsrku2k0")
