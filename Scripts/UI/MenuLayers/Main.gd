# Copyright 2026 CE-Studio: AGPL-3.0-only
extends VBoxContainer


var layer:MenuLayer


func _ready() -> void:
	layer = get_parent()
	_check_remove_boss_rush.call_deferred()
	layer.layer_focused.connect(_on_layer_focused)


func _check_remove_boss_rush() -> void:
	if not Statics.can_unlock(Statics.Unlocks.BOSS_RUSH):
		$"BossRush".free_safely()


func _on_layer_focused() -> void:
	layer.menu.set_selector_anim(0)
