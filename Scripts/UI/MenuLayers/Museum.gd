# Copyright 2026 CE-Studio: AGPL-3.0-only
extends VBoxContainer


func _ready() -> void:
	if not Statics.has_times_for_character(Player.Players.SNAILY):
		$"Scores".quick_load_layer = ""
		$"Scores".set_text(tr(&"-- ??? --"))
		$"Gallery".quick_load_layer = ""
		$"Gallery".set_text(tr(&"-- ??? --"))
	if true:
		$"SoundTest".quick_load_layer = ""
		$"SoundTest".set_text(tr(&"-- ??? --"))
