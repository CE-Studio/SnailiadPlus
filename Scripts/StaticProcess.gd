extends Node


var template_general
var template_profile
var template_records


signal cut_advance


# Initialize everything on load
func _ready() -> void:
	# Load all text from library
	Statics.text_lib = _load_json_to_dict("res://Resources/Text.json")

	# Load data. If no data exists, create it
	DirAccess.open("user://" + Statics.save_prefix)
	if DirAccess.get_open_error() != 0:
		DirAccess.make_dir_recursive_absolute("user://" + Statics.save_prefix)

	template_general = _load_json_to_dict("res://SaveTemplates/GeneralData.json")
	template_profile = _load_json_to_dict("res://SaveTemplates/ProfileData.json")
	template_records = _load_json_to_dict("res://SaveTemplates/RecordData.json")

	#Statics.data_general = _load_data_dict("GeneralData", template_general)
	Statics.data_profile1 = _load_data_dict("Profile1", template_profile)
	Statics.data_profile2 = _load_data_dict("Profile2", template_profile)
	Statics.data_profile3 = _load_data_dict("Profile3", template_profile)
	Statics.data_records = _load_data_dict("Records", template_records)

	# If control array is empty, set default controls
	if ProjectSettings.get_setting("game/control/controls").size() == 0:
		ProjectSettings.set_setting("game/control/controls", SInput.DEFAULTS.duplicate())

	# Set important game systems according to newly loaded data
	_set_game_settings()


func _set_game_settings() -> void:
	#region Sound volume
	var master_index = AudioServer.get_bus_index("Master")
	var master_vol = ProjectSettings.get_setting("audio/volume/master") / 20.0
	AudioServer.set_bus_volume_db(master_index, linear_to_db(master_vol))
	var sound_index = AudioServer.get_bus_index("Sfx")
	var sound_vol = ProjectSettings.get_setting("audio/volume/sound") / 20.0
	AudioServer.set_bus_volume_db(sound_index, linear_to_db(sound_vol))
	var music_index = AudioServer.get_bus_index("Music")
	var music_vol = ProjectSettings.get_setting("audio/volume/music") / 20.0
	AudioServer.set_bus_volume_db(music_index, linear_to_db(music_vol))
	#endregion

	#region Gameplay settings
	Engine.max_fps = Statics.TARGET_FRAMERATES[ProjectSettings.get_setting("game/visuals/frame_limit")]
	#endregion


func _load_data_dict(filename:String, template:Dictionary[String, Variant]) -> Dictionary[String, Variant]:
	var file_path := "user://%s/%s.json" % [ Statics.save_prefix, filename ]
	var loaded_dict:Dictionary[String, Variant] = {}
	if FileAccess.file_exists(file_path):
		loaded_dict = _load_json_to_dict(file_path)
		loaded_dict.merge(template)
	else:
		loaded_dict = template.duplicate()
	return loaded_dict


func _load_json_to_dict(path:String) -> Dictionary[String, Variant]:
	var j := JSON.new()
	var f := FileAccess.open(path, FileAccess.READ)
	var out_dict:Dictionary[String, Variant] = {}
	if j.parse(f.get_as_text()) != OK:
		f.close()
		assert(
			false,
			"JSON parsing failed! Line " + str(j.get_error_line()) +
			", Message: \"" + j.get_error_message() +
			"\", In file " + path
		)
	else:
		out_dict.merge(j.data, true)
	f.close()
	return out_dict


func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("debug") and not Statics.is_menu_open:
		Statics.noclip_mode = not Statics.noclip_mode
	if event.is_action_pressed(&"speak"):
		cut_advance.emit()
