class_name PlayerSpawner
extends Node2D


var snaily:PackedScene = preload("uid://dgd76ibvdfk3c")
var sluggy:PackedScene = preload("uid://dt2f017mhof4h")

@export var gamecore:GameCore


func spawn() -> Player:
	var player_id:int = 0
	if not Statics.current_profile.is_empty():
		player_id = Statics.current_profile["character"]
	var player:Player
	match player_id:
		1: player = sluggy.instantiate()
		_: player = snaily.instantiate()
	add_child(player)
	player.position = position
	gamecore.player = player
	return player
