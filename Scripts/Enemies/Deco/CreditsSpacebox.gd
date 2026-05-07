# Copyright 2026 CE-Studio: AGPL-3.0-only
extends Node2D


const ANIM_START_THRESHOLD:float = 200.0

## Will be set once the babybox animation starts playing
var babybox_appeared:bool = false

@onready var spacebox:JsonSprite2D = $"Spacebox"
@onready var babybox:JsonSprite2D = $"Babybox"
@onready var anim:AnimationPlayer = $"AnimationPlayer"


func _process(_delta: float) -> void:
	if global_position.y <= ANIM_START_THRESHOLD and not babybox_appeared:
		babybox_appeared = true
		anim.play("Babybox")


func update_anims() -> void:
	spacebox.action = "display_smile"
	babybox.action = "display_hit"
