# Copyright 2026 CE-Studio: AGPL-3.0-only
extends VBoxContainer


func _ready() -> void:
	if not Statics.has_unlock(Statics.Unlocks.BOSS_RUSH):
		$"Times".quick_load_layer = ""
		$"Times".set_text(tr(&"-- ??? --"))
		$"Gallery".quick_load_layer = ""
		$"Gallery".set_text(tr(&"-- ??? --"))
		$"SoundTest".quick_load_layer = ""
		$"SoundTest".set_text(tr(&"-- ??? --"))
