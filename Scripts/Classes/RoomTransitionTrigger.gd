@icon("res://Editor/ico/RoomTransitionTrigger.svg")
class_name RoomTransitionTrigger
extends Node2D


@export var my_id:int
@export_file("*.tscn") var exit_room:String
@export var exit_transition:int


@onready var area:Area2D = $"Area2D"
@onready var exit_marker:Marker2D = $"Marker2D"
@onready var core:GameCore = $"/root/GameScene"


var not_exiting := true


func _ready():
	pass


func _on_player_enter(area):
	if exit_room != "" && not_exiting:
		not_exiting = false
		core.player.set_box_disable_override(true)
		var offset = core.player.global_position - area.global_position
		#core.spawn_room(exit_room, exit_transition, offset)
		core.call_deferred("spawn_room", exit_room, exit_transition, offset)
