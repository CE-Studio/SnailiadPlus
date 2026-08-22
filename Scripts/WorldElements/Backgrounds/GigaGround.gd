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

var elapsed:float = 0.0
var effect_dir:Vector2 = Vector2.DOWN
var anim_prefix:String = "floor"
var anim_path:String = ""
var environment:GigaEnvironment
var env_linked:bool = false
var do_fade_effects:bool = false
var do_flash_effects:bool = false
var sprites_close_enough:Array[bool] = []
var use_grid:bool = false
var extend_dir:Vector2 = Vector2.RIGHT

@export var segments:int = 1
@export var surface:Statics.DirsSurface

@onready var sprites:Array[SnailySprite2D] = []
@onready var circle:PackedScene = preload("uid://c2d3jobu0nn2a")
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
	
	if use_grid:
		_spawn_grid()
	else:
		_spawn_circles()
		match surface:
			Statics.DirsSurface.FLOOR: position += Vector2(0, -8)
			Statics.DirsSurface.LWALL: position += Vector2(8, 0)
			Statics.DirsSurface.RWALL: position += Vector2(-8, 0)
			Statics.DirsSurface.CEILING: position += Vector2(0, 8)


func _spawn_grid() -> void:
	anim_path = sprites[0].texture_path
	
	segments -= 1
	sprites[0].position -= extend_dir * segments * (SPR_SIZE * 0.5)
	var origin:Vector2 = sprites[0].position
	for i in range(segments):
		var j:int = i + 1
		var new_main:JsonSprite2D = JsonSprite2D.new()
		new_main.texture_path = anim_path
		add_child(new_main)
		sprites.append(new_main)
		new_main.position = origin + extend_dir * j * SPR_SIZE
	for spr in sprites:
		spr.action = "_".join([anim_prefix, "intro"])
		spr._process(0.0)
		sprites_close_enough.append(false)


func _spawn_circles() -> void:
	#sprites[0].queue_free()
	#sprites.clear()
	
	var length:int = 0
	var goal:int = (segments - 1) * SPR_SIZE
	var start:Vector2 = extend_dir * goal * -0.5
	var placed_all:bool = false
	var shimmer_state:bool = true
	while not placed_all:
		var new_spr:SnailySprite2D = circle.instantiate()
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


func _process(_delta: float) -> void:
	if Player.instance and (do_fade_effects or do_flash_effects) and use_grid:
		var p_pos:Vector2 = Player.instance.position
		var g_pos:Vector2 = Vector2(9999, 9999)
		if env_linked:
			g_pos = environment.giga.position
		var i:int = 0
		for spr in sprites:
			var p_dist:float = spr.global_position.distance_to(p_pos)
			var g_dist:float = spr.global_position.distance_to(g_pos)
			var dist:float = minf(p_dist, g_dist)
			if do_fade_effects:
				spr.modulate.a = FADE_ALPHA
				if dist < FADE_DISTANCE:
					var weight:float = inverse_lerp(FADE_MIN, FADE_DISTANCE, dist)
					spr.modulate.a = lerpf(1.0, FADE_ALPHA, clampf(weight, 0.0, 1.0))
				if env_linked:
					spr.modulate.a += environment.ground_glow
					spr.modulate.a += environment.get_stripe_glow_at_y(spr.global_position.y)
			if do_flash_effects and environment.state != "intro":
				if dist <= FLASH_PROXIMITY and not sprites_close_enough[i]:
					sprites_close_enough[i] = true
					update_one_anim(i)
				elif dist > FLASH_PROXIMITY and sprites_close_enough[i]:
					sprites_close_enough[i] = false
			i += 1


func update_all_anim() -> void:
	for i in range(sprites.size()):
		update_one_anim(i)


func update_one_anim(i:int) -> void:
	if use_grid:
		sprites[i].action = "_".join([anim_prefix, environment.state, environment.phase])
	else:
		sprites[i].update_state(environment.state, environment.phase)


func impact_ground(impact_point:Vector2) -> void:
	for part in sprites:
		part.set_impact_flash(impact_point)
