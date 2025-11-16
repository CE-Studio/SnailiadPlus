class_name GameCore
extends Node2D


static var instance:GameCore


var player:Player
var cam_layer:UICore
var current_room:Room
var current_room_name:String
var current_area:int = -1
var sfx_group:Node
var music_manager:MusicManager
var lim_sfx_handler:LimitedSoundHandler


func _ready() -> void:
	instance = self
	sfx_group = $"SfxGroup"
	for child in get_children():
		if child is Player:
			player = child
	cam_layer = $"CameraLayer"
	cam_layer.instantiate()
	music_manager = $"MusicManager"
	lim_sfx_handler = $"LimitedSoundHandler"
	if current_room == null:
		spawn_room(Statics.load_room)
		player.reset_position(Statics.load_coords)
	player.selected_weapon = Statics.current_profile["equipped_weapons"]
	cam_layer.cam.set_layer_position(player.position)
	cam_layer.update_weapon_icons(false)
	cam_layer.configure_for_aspect_ratio(ProjectSettings.get_setting("display/window/size/aspect_ratio"))
	cam_layer.shake_setting = ProjectSettings.get_setting("game/visuals/screen_shake")


func _process(delta: float) -> void:
	inc_game_time(delta)
	handle_cheats()


func inc_game_time(delta:float) -> void:
	if get_tree().paused:
		return
	
	var cur_time = Statics.current_profile["game_time"].duplicate()
	cur_time[2] += delta
	if cur_time[2] >= 60.0:
		cur_time[2] -= 60.0
		cur_time[1] += 1
	if cur_time[1] >= 60:
		cur_time[1] -= 60
		cur_time[0] += 1
	Statics.current_profile["game_time"] = cur_time.duplicate()
	
	if UICore.instance:
		var time_str:String = Statics.get_igt_str()
		UICore.instance.igt_text.set_snaily_text_raw(time_str)


func spawn_room(path:String, entrance:int = -1, offset:Vector2 = Vector2.ZERO) -> void:
	player.set_box_disable_override(true)
	if current_room != null:
		player.environment_exit_override = 2
		player.reparent(self)
		despawn_room(current_room)
	player.reset_position(Vector2(-999999, -999999))
	
	var new_room:Room = load(path).instantiate()
	add_child(new_room)
	move_child(new_room, 0)
	current_room = new_room
	Statics.active_room = current_room
	player.reparent(new_room.layer_ground)
	new_room.layer_ground.move_child(player, 1)
	
	player.reset_position(new_room.default_spawn.position + offset)
	if entrance != -1:
		for child in new_room.get_children():
			if child is RoomTransitionTrigger:
				if child.my_id == entrance:
					player.reset_position(child.exit_marker.global_position + offset)
	cam_layer.cam.set_layer_position(player.position)
	
	new_room.spawn(true)
	player.set_box_disable_override.call_deferred(false)
	if new_room.area_id != current_area:
		UICore.instance.show_area_text(new_room.area_id)
		current_area = new_room.area_id


func despawn_room(room:Room) -> void:
	for child in Statics.get_all_children(room):
		if child is EnvironmentArea:
			child.read_interactions = false
	room.queue_free()


func handle_cheats() -> void:
	var running_check:Array = SInput.last_ten_keys.duplicate()
	var cheat_executed:bool = false
	while running_check.size() > 0:
		match running_check.size():
			7:
				if running_check == [ KEY_S, KEY_K, KEY_Y, KEY_F, KEY_I, KEY_S, KEY_H ]:
					Statics.play_sfx_disconnected(load("res://Assets/Sounds/Sfx/CheatSkyfish.ogg"))
					Statics.set_world_flag(Statics.WorldFlags.DEFEATED_BOSS1, false)
					Statics.set_world_flag(Statics.WorldFlags.DEFEATED_BOSS2, false)
					Statics.set_world_flag(Statics.WorldFlags.DEFEATED_BOSS3, false)
					Statics.set_world_flag(Statics.WorldFlags.DEFEATED_BOSS4, false)
					UICore.instance.show_flashy_popup(Statics.get_text("cheat_skyfish"))
					cheat_executed = true
			_:
				pass
		running_check.pop_front()
		if cheat_executed:
			running_check.clear()
			SInput.last_ten_keys.clear()
