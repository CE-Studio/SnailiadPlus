# Copyright 2026 CE-Studio: AGPL-3.0-only
@icon("res://Editor/ico/RoomTransitionTrigger.svg")
class_name RoomTransitionTrigger
extends Node2D


@export var my_id:int
@export_file("*.tscn") var exit_room:String
@export var exit_transition:int


@onready var area:Area2D = $"Area2D"
@onready var exit_marker:Marker2D = $"Marker2D"


var not_exiting := true
var spawn_buffer_frames:int = 4


func _ready():
	if GameCore.instance and exit_room != "":
		GameCore.instance.room_loader.request_load(exit_room)
		if get_parent() is Room:
			get_parent().despawn.connect(_on_room_despawn)


func _process(_delta: float) -> void:
	if spawn_buffer_frames > 0:
		spawn_buffer_frames -= 1


func _on_player_enter(_area):
	if exit_room != "" and not_exiting and spawn_buffer_frames == 0:
		not_exiting = false
		var offset = GameCore.instance.player.body.global_position - area.global_position
		UICore.instance.color_cover.call_thread_safe("set_color", Color8(0, 0, 0, 120))
		GameCore.instance.call_deferred("spawn_room", exit_room, exit_transition, offset)
		UICore.instance.color_cover.set_new_fade(Color8(0, 0, 0, 255), Color8(0, 0, 0, 0), 0.25)


func _on_room_despawn() -> void:
	if GameCore.instance:
		GameCore.instance.room_loader.request_clear(exit_room)
