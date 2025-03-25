@tool
@icon("res://Editor/ico/Room.svg")
class_name Room
extends Node2D


#region Variables
#region Export
@export_group("General")
@export_enum(
	"Snail Town", "Mare Carelia", "Spiralis Silere", "Amastrida Abyssus",
	"Lux Lirata", "Shrine of Iris", "Boss Rush", "None:-1"
	) var area_id:int
@export var subarea_id:int = 0
@export var is_bonus_room:bool = false
@export_range(0.0, 1.0) var darkness_level:float = 0.0
@export var cutscenes:Array[Cutscene]


@export_group("Layers")
@export_subgroup("Parallax")
@export var fg2_parallax:Vector2
@export var fg1_parallax:Vector2
@export var bg1_parallax:Vector2
@export var bg2_parallax:Vector2
@export var sky_parallax:Vector2
@export_subgroup("Offsets")
@export var fg2_offset:Vector2
@export var fg1_offset:Vector2
@export var bg1_offset:Vector2
@export var bg2_offset:Vector2
@export var sky_offset:Vector2
#endregion


#region Internals
enum Layers {
	SKY,
	BG2,
	BG1,
	GROUND,
	FG1,
	FG2,
	ENTITY
}

@onready var layer_entity:Node2D = $"EntityLayer"
@onready var map_entity:TileMapLayer = $"EntityLayer/Map"
@onready var layer_fg2:Node2D = $"FG2Layer"
@onready var map_fg2:TileMapLayer = $"FG2Layer/Map"
@onready var layer_fg1:Node2D = $"FG1Layer"
@onready var map_fg1:TileMapLayer = $"FG1Layer/Map"
@onready var layer_ground:Node2D = $"GroundLayer"
@onready var map_ground:TileMapLayer = $"GroundLayer/Map"
@onready var layer_bg1:Node2D = $"BG1Layer"
@onready var map_bg1:TileMapLayer = $"BG1Layer/Map"
@onready var layer_bg2:Node2D = $"BG2Layer"
@onready var map_bg2:TileMapLayer = $"BG2Layer/Map"
@onready var layer_sky:Node2D = $"SkyLayer"
@onready var map_sky:TileMapLayer = $"SkyLayer/Map"

@onready var breakable_scene = load("res://Scenes/Entities/Breakable.tscn")
#endregion
#endregion


func spawn(spawn_all:bool):
	if Statics.show_entity_layer:
		map_entity.modulate = Color(1, 1, 1, 0.5)
	else:
		map_entity.modulate = Color(1, 1, 1, 0)
	
	# Get all entity tiles and spawn associated objects
	_spawn_entities_from_layer()
	
	# Properly spawn all objects in room
	if spawn_all:
		var layer_array = [ layer_sky, layer_bg2, layer_bg1, layer_ground, layer_fg1, layer_fg2 ]
		for layer in layer_array:
			for child in layer.get_children():
				if child is Door:
					child.spawn()


func _process(_delta):
	pass


func get_actors() -> Array[CutsceneControllable]:
	assert(Engine.is_editor_hint(), "Only intended to be used in the editor")
	var arr:Array[CutsceneControllable] = []
	_recur_extr(arr, self)
	return arr


func _recur_extr(arr:Array[CutsceneControllable], n:Node):
	for i in n.get_children():
		_recur_extr(arr, i)
	if n is CutsceneControllable:
		arr.append(n)


func _spawn_entities_from_layer() -> void:
	for tile in map_entity.get_used_cells():
		var tile_coords = map_entity.get_cell_atlas_coords(tile)
		match tile_coords:
			Vector2i(11, 1): # Grass
				var grass:Grass = load("res://Scenes/Entities/Grass.tscn").instantiate()
				layer_ground.add_child(grass)
				_set_entity_position(grass, tile)
				grass.spawn(Grass.GrassTypes.NORMAL, Statics.DirsSurface.FLOOR)
			
			Vector2i(14, 1): # Power grass
				var grass:Grass = load("res://Scenes/Entities/Grass.tscn").instantiate()
				layer_ground.add_child(grass)
				_set_entity_position(grass, tile)
				grass.spawn(Grass.GrassTypes.POWER, Statics.DirsSurface.FLOOR)
			
			Vector2i(8, 4): # Boomerang breakable
				var boom_tile:Breakable = breakable_scene.instantiate()
				layer_ground.add_child(boom_tile)
				_set_entity_position(boom_tile, tile)
				boom_tile.spawn(tile, Breakable.TileTypes.BOOMERANG, false)
			
			Vector2i(9, 4): # Rainbow Wave breakable
				var wave_tile:Breakable = breakable_scene.instantiate()
				layer_ground.add_child(wave_tile)
				_set_entity_position(wave_tile, tile)
				wave_tile.spawn(tile, Breakable.TileTypes.RAINBOW_WAVE, false)
			
			Vector2i(10, 4): # Devastator breakable
				var dev_tile:Breakable = breakable_scene.instantiate()
				layer_ground.add_child(dev_tile)
				_set_entity_position(dev_tile, tile)
				dev_tile.spawn(tile, Breakable.TileTypes.DEVASTATOR, false)
			
			Vector2i(1, 28): # Silent Devastator breakable
				var dev_tile:Breakable = breakable_scene.instantiate()
				layer_ground.add_child(dev_tile)
				_set_entity_position(dev_tile, tile)
				dev_tile.spawn(tile, Breakable.TileTypes.DEVASTATOR, true)
			
			Vector2i(12, 30): # Hanging grass
				var grass:Grass = load("res://Scenes/Entities/Grass.tscn").instantiate()
				layer_ground.add_child(grass)
				_set_entity_position(grass, tile)
				grass.spawn(Grass.GrassTypes.NORMAL, Statics.DirsSurface.CEILING)
			
			Vector2i(13, 30): # Hanging power grass
				var grass:Grass = load("res://Scenes/Entities/Grass.tscn").instantiate()
				layer_ground.add_child(grass)
				_set_entity_position(grass, tile)
				grass.spawn(Grass.GrassTypes.POWER, Statics.DirsSurface.CEILING)


func _set_entity_position(entity:Node, coords:Vector2i) -> void:
	entity.position = (coords * 16) + Vector2i(8, 8)
