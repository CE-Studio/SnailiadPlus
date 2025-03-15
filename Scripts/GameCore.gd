extends Node2D
class_name GameCore


var player:Node2D
var cam_layer:Node2D
var current_room:Node2D
var current_room_name:String


func _ready() -> void:
	for child in get_children():
		if child is Player:
			player = child
	cam_layer = $"CameraLayer"
	cam_layer.player = player
	if current_room == null:
		current_room_name = "SnailTown/TestRoom2"
		spawn_room_from_path(current_room_name)


func spawn_room_from_path(path:String, entrance:int = -1, offset:Vector2 = Vector2.ZERO) -> void:
	if current_room != null:
		player.reparent(self)
		current_room.queue_free()
	var room_scene = load("res://Scenes/Rooms/" + path + ".tscn")
	var new_room = room_scene.instantiate()
	add_child(new_room)
	current_room = new_room
	player.reparent(new_room.layer_ground)
	new_room.instance()
	if entrance != -1:
		for child in new_room.get_children():
			if child is RoomTransitionTrigger:
				if child.my_id == entrance:
					player.global_position = child.exit_marker.global_position + offset
	player._set_box_disable_override(false)


func spawn_room(room:Resource, entrance:int = -1, offset:Vector2 = Vector2.ZERO) -> void:
	if current_room != null:
		player.reparent(self)
		current_room.queue_free()
	var new_room = room.instantiate()
	add_child(new_room)
	current_room = new_room
	player.reparent(new_room.layer_ground)
	new_room.instance()
	if entrance != -1:
		for child in new_room.get_children():
			if child is RoomTransitionTrigger:
				if child.my_id == entrance:
					player.global_position = child.exit_marker.global_position + offset
	player._set_box_disable_override(false)
