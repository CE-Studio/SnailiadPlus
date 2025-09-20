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
	pass


func _process(_delta: float) -> void:
	if spawn_buffer_frames > 0:
		spawn_buffer_frames -= 1


func _on_player_enter(_area):
	if exit_room != "" and not_exiting and spawn_buffer_frames == 0:
		not_exiting = false
		GameCore.instance.player.set_box_disable_override(true)
		var offset = GameCore.instance.player.body.global_position - area.global_position
		GameCore.instance.call_deferred("spawn_room", exit_room, exit_transition, offset)
		UICore.instance.color_cover.set_new_fade(Color8(0, 0, 0, 255), Color8(0, 0, 0, 0), 0.25)
