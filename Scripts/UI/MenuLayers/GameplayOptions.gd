extends VBoxContainer


var layer:MenuLayer


func _ready() -> void:
	layer = get_parent()
	$"ScrollContainer/VBoxContainer/DynamicCamera".toggled_on = ProjectSettings.get_setting("game/visuals/dynamic_camera")
	$"ScrollContainer/VBoxContainer/ShootMode".remote_set_option(ProjectSettings.get_setting("game/control/toggle_shoot"))
	$"ScrollContainer/VBoxContainer/Breakables".remote_set_option(ProjectSettings.get_setting("game/world/breakables"))
	$"ScrollContainer/VBoxContainer/SecretMap".toggled_on = ProjectSettings.get_setting("game/ui/secret_map_tiles")
	$"ScrollContainer/VBoxContainer/TargetFPS".remote_set_option(ProjectSettings.get_setting("game/visuals/frame_limit"))
	$"ScrollContainer/VBoxContainer/ScreenShake".remote_set_option(ProjectSettings.get_setting("game/visuals/screen_shake"))
	$"ScrollContainer/VBoxContainer/DamageNumbers".toggled_on = ProjectSettings.get_setting("game/world/damage_numbers")
	$"ScrollContainer/VBoxContainer/StickAimMode".remote_set_option(ProjectSettings.get_setting("game/control/omni_stick_aim"))
	$"ScrollContainer/VBoxContainer/GravSwap".remote_set_option(ProjectSettings.get_setting("game/control/gravity_swap"))
	$"ScrollContainer/VBoxContainer/GravKeep".remote_set_option(ProjectSettings.get_setting("game/control/gravity_keep"))


func _on_dynamic_camera_toggled(value) -> void:
	ProjectSettings.set_setting("game/visuals/dynamic_camera", value)


func _on_shoot_mode_cycled(value) -> void:
	ProjectSettings.set_setting("game/control/toggle_shoot", value)


func _on_breakables_cycled(value) -> void:
	ProjectSettings.set_setting("game/world/breakables", value)


func _on_secret_map_tiles_toggled(value) -> void:
	ProjectSettings.set_setting("game/ui/secret_map_tiles", value)


func _on_target_fps_cycled(value) -> void:
	Engine.max_fps = Statics.TARGET_FRAMERATES[value]
	ProjectSettings.set_setting("game/visuals/frame_limit", value)


func _on_screen_shake_cycled(value) -> void:
	ProjectSettings.set_setting("game/visuals/screen_shake", value)


func _on_damage_numbers_toggled(value) -> void:
	ProjectSettings.set_setting("game/world/damage_numbers", value)


func _on_stick_aim_mode_cycled(value) -> void:
	ProjectSettings.set_setting("game/control/omni_stick_aim", value)


func _on_grav_swap_cycled(value) -> void:
	ProjectSettings.set_setting("game/control/gravity_swap", value)


func _on_grav_keep_cycled(value) -> void:
	ProjectSettings.set_setting("game/control/gravity_keep", value)
