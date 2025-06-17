class_name GameCore
extends Node2D


static var instance:GameCore


var player:Player
var cam_layer:UICore
var current_room:Node2D
var current_room_name:String
var sfx_group:Node
var music_manager:MusicManager


func _ready() -> void:
	instance = self
	sfx_group = $"SfxGroup"
	for child in get_children():
		if child is Player:
			player = child
	cam_layer = $"CameraLayer"
	cam_layer.instantiate()
	music_manager = $"MusicManager"
	if current_room == null:
		spawn_room(Statics.load_room)
		player.reset_position(Statics.load_coords)
	player.selected_weapon = Statics.current_profile["equipped_weapons"]
	cam_layer.cam.set_layer_position(player.position)
	cam_layer.update_weapon_icons()
	cam_layer.configure_for_aspect_ratio(int(Statics.data_general["aspect_ratio"]))


func spawn_room(path:String, entrance:int = -1, offset:Vector2 = Vector2.ZERO) -> void:
	if current_room != null:
		player.reparent(self)
		current_room.queue_free()
	var new_room:Room = load(path).instantiate()
	add_child(new_room)
	move_child(new_room, 0)
	current_room = new_room
	Statics.active_room = current_room
	player.reparent(new_room.layer_ground)
	new_room.layer_ground.move_child(player, 1)
	if entrance != -1:
		for child in new_room.get_children():
			if child is RoomTransitionTrigger:
				if child.my_id == entrance:
					player.reset_position(child.exit_marker.global_position + offset)
					cam_layer.cam.set_layer_position(player.position)
	new_room.spawn(true)
	player.set_box_disable_override(false)
