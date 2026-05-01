# Copyright 2026 CE-Studio: AGPL-3.0-only
extends VBoxContainer


func _ready() -> void:
	$"ButtonStyle".remote_set_option(ProjectSettings.get_setting("game/control/controller_type"))
	$"MoveDeadzone".remote_set_option(roundi(ProjectSettings.get_setting("game/control/deadzone_move") * 20.0))
	$"AimDeadzone".remote_set_option(roundi(ProjectSettings.get_setting("game/control/deadzone_aim") * 20.0))


func _on_face_type_cycled(value:int) -> void:
	ProjectSettings.set_setting("game/control/controller_type", value)


func _on_move_dz_cycled(value:int) -> void:
	ProjectSettings.set_setting("game/control/deadzone_move", value * 0.05)


func _on_aim_dz_cycled(value:int) -> void:
	ProjectSettings.set_setting("game/control/deadzone_aim", value * 0.05)
