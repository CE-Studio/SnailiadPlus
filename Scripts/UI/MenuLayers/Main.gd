# Copyright 2026 CE-Studio: AGPL-3.0-only
extends VBoxContainer


func _ready() -> void:
	_check_remove_boss_rush.call_deferred()


func _check_remove_boss_rush() -> void:
	if not Statics.can_unlock(Statics.Unlocks.BOSS_RUSH):
		$"BossRush".free_safely()
