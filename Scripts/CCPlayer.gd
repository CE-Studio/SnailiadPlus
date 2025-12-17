class_name CCPlayer
extends Node2D


var active:bool = false
var last_position:Vector2 = Vector2.ZERO


func _ready() -> void:
	last_position = position


func _physics_process(_delta: float) -> void:
	if not active:
		return
	if last_position != position:
		Player.instance.body.position = position
		last_position = position


func set_active() -> void:
	active = true


func set_inactive() -> void:
	active = false
