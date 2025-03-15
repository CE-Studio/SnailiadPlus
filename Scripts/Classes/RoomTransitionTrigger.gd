@icon("res://Editor/ico/RoomTransitionTrigger.svg")
extends Node2D
class_name RoomTransitionTrigger


@onready var area:Area2D = $"Area2D"
@onready var exit_marker:Marker2D = $"Marker2D"
@onready var core:GameCore = $"/root/GameScene"

@export var my_id:int
@export var exit_room:Resource
@export var exit_transition:int


func _ready():
	pass


func _on_player_enter(area):
	if exit_room != null:
		core.player._set_box_disable_override(true)
		var offset = core.player.global_position - area.global_position
		#core.spawn_room(exit_room, exit_transition, offset)
		core.call_deferred("spawn_room", exit_room, exit_transition, offset)
