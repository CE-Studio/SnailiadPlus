extends Node


# Initialize everything on load
func _ready() -> void:
	# Load data. If no data exists, create it
	DirAccess.open("user://" + Statics.SAVE_PREFIX)
	if DirAccess.get_open_error() != 0:
		DirAccess.make_dir_recursive_absolute("user://" + Statics.SAVE_PREFIX)
	if FileAccess.file_exists("user://" + Statics.SAVE_PREFIX + "/GeneralData.json"):
		Statics.data_general = _load_json_to_dict("user://" + Statics.SAVE_PREFIX + "/GeneralData.json")
	else:
		Statics.data_general = _load_json_to_dict("res://SaveTemplates/GeneralData.json")
	if FileAccess.file_exists("user://" + Statics.SAVE_PREFIX + "/Profile1.json"):
		Statics.data_profile1 = _load_json_to_dict("user://" + Statics.SAVE_PREFIX + "/Profile1.json")
	else:
		Statics.data_profile1 = _load_json_to_dict("res://SaveTemplates/ProfileData.json")
	if FileAccess.file_exists("user://" + Statics.SAVE_PREFIX + "/Profile2.json"):
		Statics.data_profile2 = _load_json_to_dict("user://" + Statics.SAVE_PREFIX + "/Profile2.json")
	else:
		Statics.data_profile2 = _load_json_to_dict("res://SaveTemplates/ProfileData.json")
	if FileAccess.file_exists("user://" + Statics.SAVE_PREFIX + "/Profile3.json"):
		Statics.data_profile3 = _load_json_to_dict("user://" + Statics.SAVE_PREFIX + "/Profile3.json")
	else:
		Statics.data_profile3 = _load_json_to_dict("res://SaveTemplates/ProfileData.json")
	if FileAccess.file_exists("user://" + Statics.SAVE_PREFIX + "/Records.json"):
		Statics.data_records = _load_json_to_dict("user://" + Statics.SAVE_PREFIX + "/Records.json")
	else:
		Statics.data_records = _load_json_to_dict("res://SaveTemplates/RecordData.json")
	Statics.save_general()


func _load_json_to_dict(path:String) -> Dictionary:
	var j := JSON.new()
	print(path)
	var f := FileAccess.open(path, FileAccess.READ)
	print(FileAccess.get_open_error())
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
