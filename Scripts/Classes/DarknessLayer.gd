# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name DarknessLayer
extends Node2D


#region Variables
const MAX_SOURCES:int = 128
const NULL_POS:Vector3 = Vector3(-128, -128, 0)
const MAX_DARKNESS_COL:Color = Color(0.0, 0.0, 0.0, 1.0)
const HALO_SCALE:float = 16.0
const HALO_AMPLITUDE:float = 2.0
const HALO_CYCLE_MULT:float = 2.5
const BUFFER:Vector2 = Vector2(48, 48)

## Array of every node set to emit light. Used to determine where to draw a light source
var sources:Array[Node2D] = []
## Array of how large to draw each light source. Should remain of equal size to the node array
var source_radii:Array[int] = []
## Array of light source data inferred from the node and radius arrays before being passed to the shader
var array_state:Array[Vector3] = []
## Timer used to calculate the oscillation of the outer edge of each light
var halo_cycle:float = 0.0
## The base light level of the last room that called this object. Used to lighten the effect in
## accordance with the darkness setting
var last_room_level:float = 0.0

## The shader material
@onready var mat:ShaderMaterial = material
#endregion


func _ready() -> void:
	visible = true
	mat.set_shader_parameter("lights", MAX_SOURCES)
	for i in range(MAX_SOURCES):
		sources.append(null)
		source_radii.append(0)
		array_state.append(NULL_POS)
	_update_light_positions()


func _process(delta: float) -> void:
	halo_cycle += delta * HALO_CYCLE_MULT
	mat.set_shader_parameter("halo_thickness", HALO_SCALE + (sin(halo_cycle) * HALO_AMPLITUDE))
	
	for i in range(MAX_SOURCES):
		if sources[i] == null:
			array_state[i] = NULL_POS
		else:
			var ui_pos:Vector2 = UICore.instance.position
			var new_pos:Vector3 = Vector3(
				sources[i].global_position.x - ui_pos.x + BUFFER.x,
				sources[i].global_position.y - ui_pos.y + BUFFER.y,
				source_radii[i]
			)
			array_state[i] = new_pos
	_update_light_positions()


## Adds a node as a light source and how big the light emitted should be
func add_source(source:Node2D, radius:int) -> void:
	_check_clear_null_sources()
	if sources.has(source):
		return
	for i in range(MAX_SOURCES):
		if sources[i] == null:
			sources[i] = source
			source_radii[i] = radius
			break


## Checks the node array and removes any references to previously freed nodes
func _check_clear_null_sources() -> void:
	for i in range(MAX_SOURCES):
		if not sources[i]:
			sources[i] = null


## Updates the color and alpha of the darkness layer
func update_col(room_level:float = -1, color:Color = MAX_DARKNESS_COL) -> void:
	var lerp_val:float
	match ProjectSettings.get_setting("game/visuals/darkness"):
		0: lerp_val = 0.0
		1: lerp_val = 0.2
		2: lerp_val = 0.5
		3: lerp_val = 1.0
	if room_level != -1:
		lerp_val = lerpf(0.0, lerp_val, room_level)
		last_room_level = room_level
	else:
		lerp_val = lerpf(0.0, lerp_val, last_room_level)
	
	color.a = lerpf(0.0, color.a, lerp_val)
	mat.set_shader_parameter("bacgroun_color", color)
	mat.set_shader_parameter("ring_color", color)
	mat.set_shader_parameter("main_color", Color(1.0, 1.0, 1.0, 0.0))


## Updates the size of the light emitted by a given node, assuming said node is currently in the array
func update_radius(source:Node2D, new_radius:int) -> void:
	if sources.has(source):
		var i = sources.find(source)
		source_radii[i] = new_radius


## Updates the positions of each light source in the shader proper
func _update_light_positions() -> void:
	mat.set_shader_parameter("lights", MAX_SOURCES)
	mat.set_shader_parameter("light_pos", array_state)
