# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name ShellbreakerHand
extends Enemy


func _ready() -> void:
	my_type = EnemyTypes.NONE
	col = $"BodyBox"
	sprite = $"Body"
	hitbox = $"Area2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	sprite.action = "idle"
