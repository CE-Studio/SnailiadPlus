# Copyright 2026 CE-Studio: AGPL-3.0-only
extends VBoxContainer


var layer:MenuLayer
var pro_id:int
var fade_color:Color
var fade_color_a:Color


func _ready() -> void:
	layer = get_parent()
	pro_id = Statics.current_profile_id
	var save_room_name:String
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
	fade_color = Statics.get_color(Vector2i(1, 10))
	fade_color_a = fade_color
	fade_color_a.a = 0


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
	layer.can_focus = false
	layer.menu.read_inputs = false
	layer.menu.color_cover.set_new_fade(fade_color_a, fade_color, 0.5)
	GameCore.instance.music_manager.set_fade(0.0, 1.25)
	$"Timer".start()


func _on_world_spawn_pressed(_value) -> void:
	match pro_id:
		1: Statics.current_profile = Statics.data_profile1
		2: Statics.current_profile = Statics.data_profile2
		3: Statics.current_profile = Statics.data_profile3
	Statics.current_profile_id = pro_id
	var world_spawn = Statics.WORLD_SPAWN[Statics.current_profile["character"]]
	Statics.load_room = Statics.ROOM_PATH % world_spawn[0]
	Statics.load_coords = Vector2i(world_spawn[1], world_spawn[2])
	layer.can_focus = false
	layer.menu.read_inputs = false
	layer.menu.color_cover.set_new_fade(fade_color_a, fade_color, 0.5)
	GameCore.instance.music_manager.set_fade(0.0, 1.25)
	$"Timer".start()

func _on_timer_timeout() -> void:
	GameCore.instance.music_manager.stop_all()
	GameCore.instance.music_manager.set_global_volume(1.0)
	UICore.instance.pause_layer.unpause_fade_out()
	UICore.instance.color_cover.set_new_fade(fade_color, fade_color_a, 0.25)
	GameCore.instance.spawn_room(Statics.load_room)
	GameCore.instance.player.reset_position(Statics.load_coords, true)
	GameCore.instance.player.adjust_health(999999)
	UICore.instance.cam.set_layer_position(Statics.load_coords)
	UICore.instance.clear_area_text()
	UICore.instance.clear_boss_bar()
	get_tree().paused = false
	layer.menu.queue_free()
	Statics.increment_igt = true
