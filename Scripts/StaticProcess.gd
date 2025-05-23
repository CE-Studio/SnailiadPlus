extends Node


# Initialize everything on load
func _ready() -> void:
	# Load all text from library
	Statics.text_lib = _load_json_to_dict("res://Resources/Text.json")
	
	# Load data. If no data exists, create it
	DirAccess.open("user://" + Statics.save_prefix)
	if DirAccess.get_open_error() != 0:
		DirAccess.make_dir_recursive_absolute("user://" + Statics.save_prefix)
	
	var template_general = _load_json_to_dict("res://SaveTemplates/GeneralData.json")
	var template_profile = _load_json_to_dict("res://SaveTemplates/ProfileData.json")
	var template_records = _load_json_to_dict("res://SaveTemplates/RecordData.json")
	
	Statics.data_general = _load_data_dict("GeneralData", template_general)
	Statics.data_profile1 = _load_data_dict("Profile1", template_profile)
	Statics.data_profile2 = _load_data_dict("Profile2", template_profile)
	Statics.data_profile3 = _load_data_dict("Profile3", template_profile)
	Statics.data_records = _load_data_dict("Records", template_records)
	
	# Set important game systems according to newly loaded data
	_set_game_settings()


func _set_game_settings() -> void:
	#region Sound volume
	var master_index = AudioServer.get_bus_index("Master")
	var master_vol = float(Statics.data_general["master_volume"]) / 20.0
	AudioServer.set_bus_volume_db(master_index, linear_to_db(master_vol))
	var sound_index = AudioServer.get_bus_index("Sfx")
	var sound_vol = float(Statics.data_general["sound_volume"]) / 20.0
	AudioServer.set_bus_volume_db(sound_index, linear_to_db(sound_vol))
	var music_index = AudioServer.get_bus_index("Music")
	var music_vol = float(Statics.data_general["music_volume"]) / 20.0
	AudioServer.set_bus_volume_db(music_index, linear_to_db(music_vol))
	#endregion
	
	#region Display settings
	var window = get_window()
	var window_scale = Statics.data_general["window_scale"] + 1
	var aspect_ratio = Statics.ASPECT_RATIOS[Statics.data_general["aspect_ratio"]]
	var old_size = window.size
	var old_position = window.position
	window.size = aspect_ratio * window_scale
	window.content_scale_factor = window_scale
	var new_size = window.size
	var difference = new_size - old_size
	window.position = old_position - Vector2i(difference * 0.5)
	#endregion


func _load_data_dict(filename:String, template:Dictionary) -> Dictionary:
	var file_path = "user://%s/%s.json" % [ Statics.save_prefix, filename ]
	var loaded_dict:Dictionary = Dictionary()
	if FileAccess.file_exists(file_path):
		loaded_dict = _load_json_to_dict(file_path)
		loaded_dict.merge(template)
	else:
		loaded_dict = template.duplicate()
	return loaded_dict


func _load_json_to_dict(path:String) -> Dictionary:
	var j := JSON.new()
	var f := FileAccess.open(path, FileAccess.READ)
	var out_dict:Dictionary = { }
	if j.parse(f.get_as_text()) != OK:
		f.close()
		assert(
			false,
			"JSON parsing failed! Line " + str(j.get_error_line()) +
			", Message: \"" + j.get_error_message() +
			"\", In file " + path
		)
	else:
		out_dict = j.data
	f.close()
	return out_dict


func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("Debug") and not Statics.is_menu_open:
		Statics.noclip_mode = not Statics.noclip_mode
