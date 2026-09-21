# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name GigaEnvironment
extends Node2D


#region Variables
var giga:Boss
var state:String = "intro"
var phase:int = 0

@export var bg:GigaBackground
@export var ground:Array[GigaGround] = []
@export var stars:GigaStarLayer
#endregion


func connect_giga(_giga:Boss) -> void:
	giga = _giga
	for surface in ground:
		surface.environment = self
		surface.env_linked = true
	bg.environment = self
	stars.environment = self
	stars.spawn()
	for star in stars.active_particles:
		star.boss = giga


func set_state(_state:String) -> void:
	state = _state
	phase = giga.phase
	for surface in ground:
		surface.update_all_anim()
	bg.update_visible()
	stars.update_mode(_state)


func impact_ground(impact_point:Vector2) -> void:
	for surface in ground:
		surface.impact_ground(impact_point)


func despawn() -> void:
	stars.despawn()
	queue_free()
