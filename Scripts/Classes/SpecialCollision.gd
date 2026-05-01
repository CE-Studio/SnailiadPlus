# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name SpecialCollision
extends StaticBody2D


#region Variables
@onready var box_main:CollisionShape2D = $"Full"
@onready var box_half_edge:CollisionShape2D = $"HalfEdge"
@onready var box_half_center:CollisionShape2D = $"HalfCenter"
@onready var box_corner:CollisionShape2D = $"Corner"
#endregion

func disable_all() -> void:
	box_main.disabled = true
	box_half_edge.disabled = true
	box_half_center.disabled = true
	box_corner.disabled = true


func set_full() -> void:
	disable_all()
	box_main.disabled = false


func set_half_edge() -> void:
	disable_all()
	box_half_edge.disabled = false


func set_half_center() -> void:
	disable_all()
	box_half_center.disabled = false


func set_corner() -> void:
	disable_all()
	box_corner.disabled = false


func set_collision_world() -> void:
	collision_layer = 1


func set_collision_enemy() -> void:
	collision_layer = 2
