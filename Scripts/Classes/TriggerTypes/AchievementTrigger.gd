# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name AchievementTrigger
extends Trigger


@export var achievement:AchievementCore.Achievements


func _on_player_enter(_body:Node2D) -> void:
	if active:
		UICore.instance.achievement_core.check_add(achievement)
	super(_body)
