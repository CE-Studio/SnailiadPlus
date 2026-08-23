# Copyright 2026 CE-Studio: AGPL-3.0-only
extends Node2D


const ANIM_START_THRESHOLD:float = 200.0

## Will be set once the babybox animation starts playing
var babybox_appeared:bool = false

@export var spacebox:SnailySprite2D
@export var babybox:SnailySprite2D
@export var anim:AnimationPlayer


func _process(_delta: float) -> void:
	if global_position.y <= ANIM_START_THRESHOLD and not babybox_appeared:
		babybox_appeared = true
		anim.play("Babybox")


func update_anims() -> void:
	spacebox.play("smile")
	babybox.play("hit")
