class_name DarknessLayer
extends Node2D


#region Variables
const MAX_SOURCES:int = 64
const NULL_POS:Vector3 = Vector3(-32, -32, 0)
const MAX_DARKNESS_COL:Color = Color(0.0, 0.0, 0.0, 0.8)
const HALO_SCALE:float = 2.0
const HALO_AMPLITUDE:float = 2.0
const HALO_CYCLE_MULT:float = 2.5

var sources:Array[Node2D] = []
var source_radii:Array[int] = []
var array_state:Array[Vector3] = []
var halo_cycle:float = 0.0

@onready var mat:ShaderMaterial = material
#endregion


func _ready() -> void:
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
			var new_pos:Vector3 = Vector3(
				sources[i].position.x,
				sources[i].position.y,
				source_radii[i]
			)
			array_state[i] = new_pos
	_update_light_positions()


func add_source(source:Node2D, radius:int) -> void:
	_check_clear_null_sources()
	for i in range(MAX_SOURCES):
		if sources[i] == null:
			sources[i] = source
			source_radii[i] = radius
			return


func _check_clear_null_sources() -> void:
	for i in range(MAX_SOURCES):
		if not sources[i]:
			sources[i] = null


func update_col() -> void:
	var lerp_val:float
	match ProjectSettings.get_setting("game/visuals/darkness"):
		0: lerp_val = 0.0
		1: lerp_val = 0.2
		2: lerp_val = 0.5
		3: lerp_val = 1.0
	var new_col:Color = MAX_DARKNESS_COL
	new_col.a = lerpf(0.0, new_col.a, lerp_val)
	mat.set_shader_parameter("bacgroun_color", new_col)
	mat.set_shader_parameter("ring_color", new_col)
	mat.set_shader_parameter("main_color", Color(1.0, 1.0, 1.0, 0.0))


func _update_light_positions() -> void:
	mat.set_shader_parameter("light_pos", array_state)
