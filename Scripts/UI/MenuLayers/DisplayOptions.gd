extends VBoxContainer


func _ready() -> void:
	$"ScrollContainer/VBoxContainer/Scale".remote_set_option(int(ProjectSettings.get_setting("display/window/stretch/scale") - 1))
	$"ScrollContainer/VBoxContainer/Ratio".remote_set_option(ProjectSettings.get_setting("display/window/size/aspect_ratio"))
	$"ScrollContainer/VBoxContainer/Minimap".remote_set_option(ProjectSettings.get_setting("game/ui/minimap"))
	$"ScrollContainer/VBoxContainer/BottomKeys".toggled_on = ProjectSettings.get_setting("game/ui/bottom_keys")
	$"ScrollContainer/VBoxContainer/InputMap".toggled_on = ProjectSettings.get_setting("game/ui/keymap")
	$"ScrollContainer/VBoxContainer/InGameTime".toggled_on = ProjectSettings.get_setting("game/ui/in_game_time")
	$"ScrollContainer/VBoxContainer/Framerate".toggled_on = ProjectSettings.get_setting("game/ui/fps_counter")
	$"ScrollContainer/VBoxContainer/Particles".remote_set_option(ProjectSettings.get_setting("game/world/particles"))
	$"ScrollContainer/VBoxContainer/Darkness".remote_set_option(ProjectSettings.get_setting("game/visuals/darkness"))
	$"ScrollContainer/VBoxContainer/PaletteShader".toggled_on = ProjectSettings.get_setting("game/visuals/palette_shader")
	$"ScrollContainer/VBoxContainer/DistortShader".toggled_on = ProjectSettings.get_setting("game/visuals/distortion_shader")
	$"ScrollContainer/VBoxContainer/OpaqueAfterimages".toggled_on = ProjectSettings.get_setting("game/visuals/opaque_afterimages")


func on_scale_cycled(value) -> void:
	set_window_size(value, Statics.ASPECT_RATIOS[ProjectSettings.get_setting("display/window/size/aspect_ratio")])


func on_ratio_cycled(value) -> void:
	var this_ratio = Statics.ASPECT_RATIOS[value]
	set_window_size(ProjectSettings.get_setting("display/window/stretch/scale") - 1, this_ratio)
	ProjectSettings.set_setting("display/window/size/aspect_ratio", value)
	if UICore.instance != null:
		UICore.instance.configure_for_aspect_ratio(value)
		GameCore.instance.current_room.bounds.replot_points(Statics.ASPECT_RATIO_OFFSETS[value])
		UICore.instance.cam._process(0.0)


func on_minimap_cycled(value) -> void:
	ProjectSettings.set_setting("game/ui/minimap", value)


func on_bottom_keys_toggled(value) -> void:
	ProjectSettings.set_setting("game/ui/bottom_keys", value)


func on_input_map_toggled(value) -> void:
	ProjectSettings.set_setting("game/ui/keymap", value)


func on_igt_toggled(value) -> void:
	ProjectSettings.set_setting("game/ui/in_game_time", value)

func on_fullscreen_toggled(value) -> void:
	var window = get_window()
	
	if value:
		window.mode = Window.MODE_FULLSCREEN
	else:
		window.mode = Window.MODE_WINDOWED
		
		# Restore window size
		var ratio_idx = ProjectSettings.get("display/window/size/aspect_ratio")
		var ratio = Statics.ASPECT_RATIOS[ratio_idx]
		
		var window_scale = ProjectSettings.get("display/window/stretch/scale") - 1
		
		set_window_size(window_scale, ratio)

func on_fps_toggled(value) -> void:
	ProjectSettings.set_setting("game/ui/fps_counter", value)


func on_particles_cycled(value) -> void:
	ProjectSettings.set_setting("game/world/particles", value)


func on_darkness_cycled(value) -> void:
	ProjectSettings.set_setting("game/visuals/darkness", value)


func on_pshader_toggled(value) -> void:
	ProjectSettings.set_setting("game/visuals/palette_shader", value)


func on_dshader_toggled(value) -> void:
	ProjectSettings.set_setting("game/visuals/distortion_shader", value)


func on_opaque_afterimages_toggled(value) -> void:
	ProjectSettings.set_setting("game/visuals/opaque_afterimages", value)


func set_window_size(_scale:int, _ratio:Vector2i) -> void:
	var new_size:Vector2i = _ratio * (_scale + 1)
	var window = get_window()
	ProjectSettings.set_setting("display/window/size/viewport_width", new_size.x)
	ProjectSettings.set_setting("display/window/size/viewport_height", new_size.y)
	ProjectSettings.set_setting("display/window/size/window_width_override", new_size.x)
	ProjectSettings.set_setting("display/window/size/window_height_override", new_size.y)
	ProjectSettings.set_setting("display/window/stretch/scale", _scale + 1)
	window.size = new_size
	window.content_scale_factor = _scale + 1
	window.content_scale_size = new_size
	
	if GameCore.instance:
		UICore.instance.configure_for_aspect_ratio(ProjectSettings.get_setting("display/window/size/aspect_ratio"))
