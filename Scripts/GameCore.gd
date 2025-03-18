class_name GameCore
extends Node2D


var player:Player
var cam_layer:Node2D
var current_room:Node2D
var current_room_name:String
var sfx_group:Node


static var instance:GameCore


func _ready() -> void:
	instance = self
	sfx_group = $"SfxGroup"
	for child in get_children():
		if child is Player:
			player = child
	cam_layer = $"CameraLayer"
	cam_layer.player = player
	if current_room == null:
		spawn_room("res://Scenes/Rooms/SnailTown/TestRoom1.tscn")


func spawn_room(path:String, entrance:int = -1, offset:Vector2 = Vector2.ZERO) -> void:
	if current_room != null:
		player.reparent(self)
		current_room.queue_free()
	var new_room:Room = load(path).instantiate()
	add_child(new_room)
	current_room = new_room
	player.reparent(new_room.layer_ground)
	new_room.instance()
	if entrance != -1:
		for child in new_room.get_children():
			if child is RoomTransitionTrigger:
				if child.my_id == entrance:
					player.reset_position(child.exit_marker.global_position + offset)
					cam_layer.set_layer_position(player.position)
	player.set_box_disable_override(false)
