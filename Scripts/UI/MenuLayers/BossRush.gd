# Copyright 2026 CE-Studio: AGPL-3.0-only
extends VBoxContainer


@onready var layer:MenuLayer = get_parent()

@export var anim:AnimationPlayer
@export var sfx_select:AudioStreamPlayer
@export var sfx_start:AudioStreamPlayer


func on_yes_pressed(_value) -> void:
	layer.can_focus = false
	layer.menu.read_inputs = false
	var tween:Tween = get_tree().create_tween()
	tween.tween_property(layer.menu.music, "volume_linear", 0.0, 1.5)
	anim.play("fade")
	sfx_select.play()
	sfx_start.play()
