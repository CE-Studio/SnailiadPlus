# Copyright 2026 CE-Studio: AGPL-3.0-only
@icon("res://Editor/ico/RoomTransitionTrigger.svg")
class_name RoomTransitionTrigger
extends Node2D


@export var my_id:int
@export_file("*.tscn") var exit_room:String
@export var exit_transition:int

## The collidable area component of this trigger
@onready var area:Area2D = $"Area2D"
## The marker the player will spawn relative to if exiting a transition through this trigger
@onready var exit_marker:Marker2D = $"Marker2D"

## Set as long as the player has not entered this area. As soon as the player enters and the spawn
## buffer has elapsed, this is set to [code]false[/code]
var not_exiting:bool = true
## The amount of frames this trigger will wait before detecting any sort of player collision
var spawn_buffer_frames:int = 4


func _ready():
	if GameCore.instance and exit_room != "":
		GameCore.instance.room_loader.request_load(exit_room)
		if get_parent() is Room:
			get_parent().despawn.connect(_on_room_despawn)


func _process(_delta: float) -> void:
	if spawn_buffer_frames > 0:
		spawn_buffer_frames -= 1


## Called when the player enters this trigger's area
func _on_player_enter(_area):
	if exit_room != "" and not_exiting and spawn_buffer_frames == 0:
		not_exiting = false
		var offset = GameCore.instance.player.body.global_position - area.global_position
		UICore.instance.color_cover.call_thread_safe("set_color", Color8(0, 0, 0, 120))
		GameCore.instance.call_deferred("spawn_room", exit_room, exit_transition, offset)
		UICore.instance.color_cover.set_new_fade(Color8(0, 0, 0, 255), Color8(0, 0, 0, 0), 0.25)


## Called when this trigger's parent room is despawned
func _on_room_despawn() -> void:
	if GameCore.instance:
		GameCore.instance.room_loader.request_clear(exit_room)
