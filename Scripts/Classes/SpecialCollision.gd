# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name SpecialCollision
extends StaticBody2D


#region Variables
## The fully square hitbox
@onready var box_main:CollisionShape2D = $"Full"
## The half-size hitbox aligned to an edge
@onready var box_half_edge:CollisionShape2D = $"HalfEdge"
## The centered half-size hitbox
@onready var box_half_center:CollisionShape2D = $"HalfCenter"
## The quarter-size hitbox aligned to one corner of the main square
@onready var box_corner:CollisionShape2D = $"Corner"
#endregion


## Disables all hitboxes
func disable_all() -> void:
	box_main.disabled = true
	box_half_edge.disabled = true
	box_half_center.disabled = true
	box_corner.disabled = true


## Sets the main hitbox as active
func set_full() -> void:
	disable_all()
	box_main.disabled = false


## Sets the edge half hitbox as active
func set_half_edge() -> void:
	disable_all()
	box_half_edge.disabled = false


## Sets the centered half hitbox as active
func set_half_center() -> void:
	disable_all()
	box_half_center.disabled = false


## Sets the corner hitbox as active
func set_corner() -> void:
	disable_all()
	box_corner.disabled = false


## Sets the collision layer of this tile to be that of general world collision
func set_collision_world() -> void:
	collision_layer = 1


## Sets the collision layer of this tile to be that of enemy world collision
func set_collision_enemy() -> void:
	collision_layer = 2
