# Copyright 2026 CE-Studio: AGPL-3.0-only
@icon("res://Editor/ico/Hazard.svg")
class_name Hazard
extends Area2D


#region Variables
@export var damage:int = 0
@export var element:ElementTypes = ElementTypes.NONE
@export var ignore_defense:bool = false
@export_range(0, 256, 1) var light_radius:int = 0

enum ElementTypes {
	ICE,
	FIRE,
	NONE = -1
}

var intersecting_player:bool = false
#endregion


func _ready() -> void:
	connect("body_entered", _on_player_entered)
	connect("body_exited", _on_player_exited)
	if light_radius > 0:
		UICore.instance.darkness_layer.add_source(self, light_radius)


func _physics_process(_delta: float) -> void:
	if intersecting_player and not GameCore.instance.player.stunned:
		var can_hit = true
		match element:
			ElementTypes.ICE:
				can_hit = not Statics.has_shell(1)
			ElementTypes.FIRE:
				can_hit = not Statics.has_shell(3)
		if can_hit:
			GameCore.instance.player.adjust_health(-damage, ignore_defense)


func _on_player_entered(_body) -> void:
	intersecting_player = true


func _on_player_exited(_body) -> void:
	intersecting_player = false
