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
	current_room_name = "SnailTown/TestRoom2"
	_spawn_room(current_room_name)


func _spawn_room(path:String) -> void:
	if current_room != null:
		player.reparent(self)
		current_room.queue_free()
	var room_scene = load("res://Scenes/Rooms/" + path + ".tscn")
	var new_room = room_scene.instantiate()
	add_child(new_room)
	player.reparent(new_room.layer_ground)
	new_room.instance()
