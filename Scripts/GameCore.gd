extends Node2D
class_name GameCore


var player:Node2D
var current_room:Node2D
var current_room_name:String


func _ready() -> void:
	current_room_name = "TestRoom2"
	_spawn_room(current_room_name)


func _spawn_room(path:String) -> void:
	if current_room != null:
		player.reparent(self)
		current_room.queue_free()
	var new_room = load("res://Scenes/Rooms/" + path + ".tscn").instantiate()
	add_child(new_room)
