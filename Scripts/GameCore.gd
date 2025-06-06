class_name GameCore
extends Node2D


static var instance:GameCore


var player:Player
var cam_layer:UICore
var current_room:Node2D
var current_room_name:String
var sfx_group:Node


func _ready() -> void:
	instance = self
	sfx_group = $"SfxGroup"
	for child in get_children():
		if child is Player:
			player = child
	cam_layer = $"CameraLayer"
	cam_layer.instantiate()
	if current_room == null:
		spawn_room(Statics.load_room)
		player.reset_position(Statics.load_coords)
	cam_layer.cam.set_layer_position(player.position)


#func spawn_room_from_file(path:String, entrance:int = -1, offset:Vector2 = Vector2.ZERO) -> void:
#	var trimmed_path = path
#	var path_parts = Statics.ROOM_PATH.split("%s")
#	print(trimmed_path)
#	trimmed_path = trimmed_path.substr(path_parts[0].length())
#	print(trimmed_path)
#	trimmed_path = trimmed_path.substr(0, trimmed_path.length() - path_parts[1].length())
#	print(trimmed_path)
#	spawn_room(trimmed_path, path, entrance, offset)
#
#
#func spawn_room_from_name(path:String, entrance:int = -1, offset:Vector2 = Vector2.ZERO) -> void:
#	var full_path = Statics.ROOM_PATH % path
#	spawn_room(path, full_path, entrance, offset)


func spawn_room(path:String, entrance:int = -1, offset:Vector2 = Vector2.ZERO) -> void:
	if current_room != null:
		player.reparent(self)
		current_room.queue_free()
	var new_room:Room = load(path).instantiate() #TODO fix
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
