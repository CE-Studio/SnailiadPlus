# Copyright 2026 CE-Studio: AGPL-3.0-only
extends VBoxContainer


var layer:MenuLayer
var pro_id:int


func _ready() -> void:
	layer = get_parent()
	pro_id = layer.menu.get_next_layer_up().meta_info[0]
	var save_room_name
	match pro_id:
		1: save_room_name = Statics.data_profile1["save_room"]
		2: save_room_name = Statics.data_profile2["save_room"]
		3: save_room_name = Statics.data_profile3["save_room"]
	var save_room_parts:PackedStringArray = save_room_name.split("/")
	var save_area:String = GlobalText.areas[save_room_parts[0]].strip_edges()
	if save_room_parts.size() > 1:
		var save_room:String = GlobalText.room_names[save_room_name].strip_edges()
		$"SavePoint".set_subtext("%s - %s" % [save_area, save_room])
	else:
		$"SavePoint".set_subtext(save_area)


func _on_save_spawn_pressed(_value) -> void:
	match pro_id:
		1: Statics.current_profile = Statics.data_profile1
		2: Statics.current_profile = Statics.data_profile2
		3: Statics.current_profile = Statics.data_profile3
	Statics.current_profile_id = pro_id
	var load_pos = Statics.current_profile["save_coords"]
	if load_pos is String:
		load_pos = str_to_var("Vector2i" + load_pos)
	Statics.load_room = Statics.ROOM_PATH % str(Statics.current_profile["save_room"])
	Statics.load_coords = load_pos
	var flags:Array[StringName] = []
	flags.assign(Statics.current_profile["cutscene_flags"])
	CutsceneController.load_flags(flags)
	get_tree().change_scene_to_file("res://Scenes/GameScene.tscn")


func _on_world_spawn_pressed(_value) -> void:
	match pro_id:
		1: Statics.current_profile = Statics.data_profile1
		2: Statics.current_profile = Statics.data_profile2
		3: Statics.current_profile = Statics.data_profile3
	Statics.current_profile_id = pro_id
	var world_spawn = Statics.WORLD_SPAWN[Statics.current_profile["character"]]
	Statics.load_room = Statics.ROOM_PATH % world_spawn[0]
	Statics.load_coords = Vector2i(world_spawn[1], world_spawn[2])
	var flags:Array[StringName] = []
	flags.assign(Statics.current_profile["cutscene_flags"])
	CutsceneController.load_flags(flags)
	get_tree().change_scene_to_file("res://Scenes/GameScene.tscn")
	Statics.increment_igt = true
