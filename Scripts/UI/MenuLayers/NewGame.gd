# Copyright 2026 CE-Studio: AGPL-3.0-only
extends VBoxContainer


var layer:MenuLayer
var pro_id:int
var difficulty:int
var character:int


func _ready() -> void:
	layer = get_parent()
	pro_id = layer.menu.get_next_layer_up().meta_info[0]
	match pro_id:
		1: Statics.data_profile1 = StaticProcess._load_json_to_dict(
			"res://SaveTemplates/ProfileData.json")
		2: Statics.data_profile2 = StaticProcess._load_json_to_dict(
			"res://SaveTemplates/ProfileData.json")
		3: Statics.data_profile3 = StaticProcess._load_json_to_dict(
			"res://SaveTemplates/ProfileData.json")
	call_deferred("_clear_locked_buttons")


func _clear_locked_buttons() -> void:
	var cleared_mods:bool = false
	
	if (not Statics.has_unlock(Statics.Unlocks.ITEM_RANDO)
	and not Statics.has_unlock(Statics.Unlocks.OPEN_MAP)
	and not Statics.has_unlock(Statics.Unlocks.SIX_HUNDO)
	and not Statics.has_unlock(Statics.Unlocks.CHAOS_MODE)):
		cleared_mods = true
		$"Mods".free_roughly()
		$"HBoxContainer/Difficulty".focus_neighbor_bottom = NodePath("../../Start")
		$"HBoxContainer/Character".focus_neighbor_bottom = NodePath("../../Start")
		$"HBoxContainer/Character".focus_next = NodePath("../../Start")
		$"Start".focus_neighbor_top = NodePath("../HBoxContainer/Difficulty")
		$"Start".focus_previous = NodePath("../HBoxContainer/Difficulty")
	
	if not Statics.has_unlock(Statics.Unlocks.CHAR_SEL):
		$"HBoxContainer/Character".free_roughly()
		$"HBoxContainer/Difficulty".focus_neighbor_left = NodePath(".")
		$"HBoxContainer/Difficulty".focus_neighbor_right = NodePath(".")
		$"HBoxContainer/Difficulty".auto_select_mode = true
		if cleared_mods:
			$"HBoxContainer/Difficulty".focus_next = NodePath("../../Start")
			$"Start".focus_previous = NodePath("../HBoxContainer/Difficulty")
		else:
			$"HBoxContainer/Difficulty".focus_next = NodePath("../../Mods")
			$"Mods".focus_previous = NodePath("../HBoxContainer/Difficulty")
	
	if not Statics.has_unlock(Statics.Unlocks.ABSURD_DIFF):
		$"HBoxContainer/Difficulty".cycle_options.pop_back()


func _on_difficulty_changed(new_diff:int) -> void:
	match pro_id:
		1: Statics.data_profile1["difficulty"] = new_diff
		2: Statics.data_profile2["difficulty"] = new_diff
		3: Statics.data_profile3["difficulty"] = new_diff
	difficulty = new_diff


func _on_character_changed(new_char:int) -> void:
	match pro_id:
		1: Statics.data_profile1["character"] = new_char
		2: Statics.data_profile2["character"] = new_char
		3: Statics.data_profile3["character"] = new_char
	character = new_char


func _on_start_pressed(_value) -> void:
	var world_spawn = Statics.WORLD_SPAWN[character]
	Statics.current_profile_id = pro_id
	match pro_id:
		1: Statics.current_profile = Statics.data_profile1.duplicate()
		2: Statics.current_profile = Statics.data_profile2.duplicate()
		3: Statics.current_profile = Statics.data_profile3.duplicate()
	Statics.current_profile["is_empty"] = false
	Statics.current_profile["save_room"] = world_spawn[0]
	Statics.current_profile["save_coords"] = Vector2i(world_spawn[1], world_spawn[2])
	Statics.current_profile["difficulty"] = difficulty
	Statics.current_profile["character"] = character
	Statics.current_profile["map_tiles"] = Minimap.DEFAULT_MAP.duplicate()
	layer.menu.save_profile(pro_id)
	Statics.load_room = Statics.ROOM_PATH % world_spawn[0]
	Statics.load_coords = Vector2i(world_spawn[1], world_spawn[2])
	Statics.increment_igt = true
	var flags:Array[StringName] = []
	flags.assign(Statics.current_profile["cutscene_flags"])
	CutsceneController.load_flags(flags)
	if _value:
		get_tree().change_scene_to_file("uid://ltxtlsrku2k0")
	else:
		var intro:IntroCinematic = load("uid://lubilnhf0dul").instantiate()
		intro.menu = layer.menu
		layer.menu.add_child(intro)
		intro.setup_player(character as Player.Players)
		layer.can_focus = false
		layer.menu.read_inputs = false
