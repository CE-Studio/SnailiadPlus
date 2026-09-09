# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name GigaGround
extends Node2D


#region Variables
const SPR_SIZE:int = 16
const MIN_LENGTH_ADV:int = 2
const MAX_LENGTH_ADV:int = 12
const FADE_ALPHA:float = 0.25
const FADE_MIN:float = 32.0
const FADE_DISTANCE:float = 64.0
const FLASH_PROXIMITY:float = 20.0
const CIRCLE:PackedScene = preload("uid://c2d3jobu0nn2a")

var effect_dir:Vector2 = Vector2.DOWN
var anim_prefix:String = "floor"
var environment:GigaEnvironment
var env_linked:bool = false
var extend_dir:Vector2 = Vector2.RIGHT

@export var segments:int = 1
@export var surface:Statics.DirsSurface

@onready var sprites:Array[SnailySprite2D] = []
#endregion


func _ready() -> void:
	if segments < 1:
		segments = 1
	
	match surface:
		Statics.DirsSurface.LWALL:
			effect_dir = Vector2.LEFT
			extend_dir = Vector2.DOWN
			anim_prefix = "lwall"
		Statics.DirsSurface.RWALL:
			effect_dir = Vector2.RIGHT
			extend_dir = Vector2.DOWN
			anim_prefix = "rwall"
		Statics.DirsSurface.CEILING:
			effect_dir = Vector2.UP
			anim_prefix = "ceiling"
	
	_spawn_circles()
	match surface:
		Statics.DirsSurface.FLOOR: position += Vector2(0, -8)
		Statics.DirsSurface.LWALL: position += Vector2(8, 0)
		Statics.DirsSurface.RWALL: position += Vector2(-8, 0)
		Statics.DirsSurface.CEILING: position += Vector2(0, 8)


func _spawn_circles() -> void:
	var length:int = 0
	var goal:int = (segments - 1) * SPR_SIZE
	var start:Vector2 = extend_dir * goal * -0.5
	var placed_all:bool = false
	var shimmer_state:bool = true
	while not placed_all:
		var new_spr:SnailySprite2D = CIRCLE.instantiate()
		new_spr.position = start + (length * extend_dir)
		new_spr.play(anim_prefix + str(randi_range(0, 4)))
		new_spr.shimmer = shimmer_state
		sprites.append(new_spr)
		add_child(new_spr)
		new_spr._process(0.0)
		if length == goal:
			placed_all = true
		else:
			length += randi_range(MIN_LENGTH_ADV, MAX_LENGTH_ADV)
			length = clampi(length, 0, goal)
		shimmer_state = not shimmer_state


func update_all_anim() -> void:
	for i in range(sprites.size()):
		update_one_anim(i)


func update_one_anim(i:int) -> void:
	sprites[i].update_state(environment.state, environment.phase)


func impact_ground(impact_point:Vector2) -> void:
	for part in sprites:
		part.set_impact_flash(impact_point)
