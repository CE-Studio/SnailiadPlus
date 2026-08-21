# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name SpaceboxShield
extends Enemy


func _ready() -> void:
	my_type = EnemyTypes.NONE
	super.spawn()
