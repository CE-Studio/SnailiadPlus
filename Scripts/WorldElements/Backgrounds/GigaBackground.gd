# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name GigaBackground
extends Node2D


#region Variables
var SPAWN_FADE_TIMEOUT:float = 3.25
var SPAWN_FADE_MULT:float = 0.5
var MAP_FADE_TIME:float = 2.0
var BG_FADE_RATE:float = 1.5

var spawn_fade_timeout:float = SPAWN_FADE_TIMEOUT
var first_set:bool = false
var active:String = "intro"
var environment:GigaEnvironment

@export var intro:Node2D
@export var intro_sprites:Array[SnailySprite2D] = []
@export var stomp:SnailySprite2D
@export var smash:SnailySprite2D
@export var strafe:SnailySprite2D
@export var sleep:SnailySprite2D
@export var dim_and_hide_room:bool = true
#endregion


func _ready() -> void:
	if dim_and_hide_room and GameCore.instance.current_room:
		GameCore.instance.current_room.set_map_visible(Room.Layers.FG1, false)
	stomp.modulate.a = 0.0
	smash.modulate.a = 0.0
	strafe.modulate.a = 0.0
	sleep.modulate.a = 0.0


func _process(delta: float) -> void:
	if spawn_fade_timeout > 0.0:
		spawn_fade_timeout -= delta
		var this_weight:float = clampf(spawn_fade_timeout * SPAWN_FADE_MULT, 0.0, 1.0)
		modulate = Color.WHITE.lerp(Color.BLACK, this_weight)
	
	var fade_rate:float = BG_FADE_RATE * delta
	stomp.modulate.a = move_toward(stomp.modulate.a, 1.0 if active == "stomp" else 0.0, fade_rate)
	smash.modulate.a = move_toward(smash.modulate.a, 1.0 if active == "smash" else 0.0, fade_rate)
	strafe.modulate.a = move_toward(strafe.modulate.a, 1.0 if active == "strafe" else 0.0, fade_rate)
	sleep.modulate.a = move_toward(sleep.modulate.a, 1.0 if active == "sleep" else 0.0, fade_rate)


func hide_intro() -> void:
	if not first_set:
		first_set = true
		return
	intro.modulate = Color.BLACK


func update_visible() -> void:
	hide_intro()
	active = environment.state
	match active:
		"stomp": stomp.play(str(environment.phase))
		"strafe": strafe.play(str(environment.phase))
		"smash": smash.play(str(environment.phase))
		"sleep": sleep.play(str(environment.phase))
