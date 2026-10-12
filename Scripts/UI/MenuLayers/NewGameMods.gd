# Copyright 2026 CE-Studio: AGPL-3.0-only
extends VBoxContainer


@onready var open_map:ToggleSnailyButton = $"OpenMap"


func _ready() -> void:
	if Statics.has_unlock(Statics.Unlocks.OPEN_MAP):
		open_map.toggled_on = NewGameData.instance.open_map
	else:
		open_map.disabled = true
		open_map.set_text("???")


func _on_open_map_toggled(value:bool) -> void:
	NewGameData.instance.open_map = value
