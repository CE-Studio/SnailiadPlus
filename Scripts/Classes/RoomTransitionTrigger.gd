@icon("res://Editor/ico/RoomTransitionTrigger.svg")
class_name RoomTransitionTrigger
extends Node2D


@export var my_id:int
@export_file("*.tscn") var exit_room:String
@export var exit_transition:int


@onready var area:Area2D = $"Area2D"
@onready var exit_marker:Marker2D = $"Marker2D"


var not_exiting := true


func _ready():
	pass


func _on_player_enter(_area):
	if exit_room != "" && not_exiting:
		not_exiting = false
		GameCore.instance.player.set_box_disable_override(true)
		var offset = GameCore.instance.player.body.global_position - area.global_position
		GameCore.instance.call_deferred("spawn_room", exit_room, exit_transition, offset)
