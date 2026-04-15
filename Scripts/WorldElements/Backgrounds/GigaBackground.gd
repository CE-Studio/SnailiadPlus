class_name GigaBackground
extends Node2D


#region Variables
var SPAWN_FADE_TIMEOUT:float = 3.25
var SPAWN_FADE_MULT:float = 0.5
var MAP_FADE_TIME:float = 2.0

var spr_intro:Array[JsonSprite2D] = []
var spr_stomp:Array[JsonSprite2D] = []
var spr_strafe:Array[JsonSprite2D] = []
var spr_smash:Array[JsonSprite2D] = []
var spr_sleep:Array[JsonSprite2D] = []
var spawn_fade_timeout:float = SPAWN_FADE_TIMEOUT
#var first_set:bool = false

@export var intro:Node2D
@export var stomp:Node2D
@export var smash:Node2D
@export var strafe:Node2D
@export var sleep:Node2D
@export var dim_and_hide_room:bool = true
#endregion


func _ready() -> void:
	if dim_and_hide_room and GameCore.instance.current_room:
		GameCore.instance.current_room.set_map_visible(Room.Layers.FG1, false)
	for child in intro.get_children():
		if child is JsonSprite2D:
			spr_intro.append(child)


func _process(delta: float) -> void:
	if spawn_fade_timeout > 0.0:
		spawn_fade_timeout -= delta
		var this_weight = clampf(spawn_fade_timeout * SPAWN_FADE_MULT, 0.0, 1.0)
		modulate = Color.WHITE.lerp(Color.BLACK, this_weight)
