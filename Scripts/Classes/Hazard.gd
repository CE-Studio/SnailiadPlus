@icon("res://Editor/ico/Hazard.svg")
class_name Hazard
extends Area2D


#region Variables
@export var damage:int = 0
@export var element:ElementTypes = ElementTypes.NONE
@export var ignore_defense:bool = false

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
