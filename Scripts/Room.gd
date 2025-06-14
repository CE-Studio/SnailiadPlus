@tool
@icon("res://Editor/ico/Room.svg")
class_name Room
extends Node2D


#region Variables
#region Export
@export_enum(
	"Snail Town", "Mare Carelia", "Spiralis Silere", "Amastrida Abyssus",
	"Lux Lirata", "Shrine of Iris", "Boss Rush", "None:-1"
	) var area_id:int
@export var subarea_id:int = 0
@export var is_bonus_room:bool = false
@export_range(0.0, 1.0) var darkness_level:float = 0.0
@export var cutscenes:Array[Cutscene]
@export var center_parallax_maps:bool = false
@export var minimap_offset:Vector2i = Vector2i.ZERO
@export var minimap_autofill:Array[Vector2i] = []
@export var song_change:MusicManager.Loops = MusicManager.Loops.None
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

var room_path:String

@onready var layer_entity:Node2D = $"EntityLayer"
@onready var map_entity:TileMapLayer = $"EntityLayer/Map"
@onready var layer_fg2:Parallax2D = $"FG2Layer"
@onready var map_fg2:TileMapLayer = $"FG2Layer/Map"
@onready var layer_fg1:Parallax2D = $"FG1Layer"
@onready var map_fg1:TileMapLayer = $"FG1Layer/Map"
@onready var layer_ground:Node2D = $"GroundLayer"
@onready var map_ground:TileMapLayer = $"GroundLayer/Map"
@onready var layer_bg1:Parallax2D = $"BG1Layer"
@onready var map_bg1:TileMapLayer = $"BG1Layer/Map"
@onready var layer_bg2:Parallax2D = $"BG2Layer"
@onready var map_bg2:TileMapLayer = $"BG2Layer/Map"
@onready var layer_sky:Parallax2D = $"SkyLayer"
@onready var map_sky:TileMapLayer = $"SkyLayer/Map"

@onready var bounds:CameraBorder = $"CameraBorder"

@onready var breakable_scene = load("res://Scenes/Entities/Breakable.tscn")
#endregion
#endregion


func spawn(_spawn_all:bool):
	if Statics.show_entity_layer:
		map_entity.modulate = Color(1, 1, 1, 0.5)
	else:
		map_entity.modulate = Color(1, 1, 1, 0)
	get_room_name_from_filename()
	
	# Get all entity tiles and spawn associated objects
	_spawn_entities_from_layer()
	
	# Properly spawn all objects in room
	if _spawn_all:
		var layer_array = [ layer_sky, layer_bg2, layer_bg1, layer_ground, layer_fg1, layer_fg2 ]
		for layer in layer_array:
			for child in layer.get_children():
				if (child is Door
				or child is NPC):
					child.spawn()
				if child is SavePoint:
					if child.check_character_spawnable():
						child.initialize_room_data(room_path)
		for fake_border in bounds.get_children():
			if fake_border is FakeCamBoundary:
				fake_border.call_deferred("instance")
	
	if song_change != MusicManager.Loops.None:
		GameCore.instance.music_manager.play_song(song_change)
	
	if center_parallax_maps:
		center_maps()
	
	UICore.instance.minimap.room_offset = minimap_offset
	for cell in minimap_autofill:
		UICore.instance.minimap.fill_cell(cell)


func get_room_name_from_filename() -> void:
	var trimmed_name = self.scene_file_path
	var path_parts = Statics.ROOM_PATH.split("%s")
	trimmed_name = trimmed_name.substr(path_parts[0].length())
	trimmed_name = trimmed_name.substr(0, trimmed_name.length() - path_parts[1].length())
	room_path = trimmed_name


func center_maps() -> void:
	var room_size = bounds.get_bound_size()
	for layer in [ Layers.SKY, Layers.BG2, Layers.BG1, Layers.FG1, Layers.FG2 ]:
		var this_layer:Parallax2D
		match layer:
			Layers.SKY:
				this_layer = layer_sky
			Layers.BG2:
				this_layer = layer_bg2
			Layers.BG1:
				this_layer = layer_bg1
			Layers.FG1:
				this_layer = layer_fg1
			Layers.FG2:
				this_layer = layer_fg2
		var scroll = this_layer.scroll_scale
		if scroll != Vector2(1.0, 1.0):
			var new_offset = Vector2(
				room_size.x - (room_size.x / scroll.x),
				room_size.y - (room_size.y / scroll.y)
			) * 0.5 * scroll
			this_layer.scroll_offset = new_offset


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
				grass.position = _tile_coords_to_vector_pos(tile)
				grass.spawn(Grass.GrassTypes.NORMAL, Statics.DirsSurface.FLOOR)
			
			Vector2i(14, 1): # Power grass
				var grass:Grass = load("res://Scenes/Entities/Grass.tscn").instantiate()
				layer_ground.add_child(grass)
				grass.position = _tile_coords_to_vector_pos(tile)
				grass.spawn(Grass.GrassTypes.POWER, Statics.DirsSurface.FLOOR)
			
			Vector2i(15, 1): # Smoke particle
				Statics.spawn_particle("Smoke", Room.Layers.BG1, _tile_coords_to_vector_pos(tile))
			
			Vector2i(8, 4): # Boomerang breakable
				var boom_tile:Breakable = breakable_scene.instantiate()
				layer_ground.add_child(boom_tile)
				layer_ground.move_child(boom_tile, 1)
				boom_tile.position = _tile_coords_to_vector_pos(tile)
				boom_tile.spawn(tile, Breakable.TileTypes.BOOMERANG, false)
			
			Vector2i(9, 4): # Rainbow Wave breakable
				var wave_tile:Breakable = breakable_scene.instantiate()
				layer_ground.add_child(wave_tile)
				layer_ground.move_child(wave_tile, 1)
				wave_tile.position = _tile_coords_to_vector_pos(tile)
				wave_tile.spawn(tile, Breakable.TileTypes.RAINBOW_WAVE, false)
			
			Vector2i(10, 4): # Devastator breakable
				var dev_tile:Breakable = breakable_scene.instantiate()
				layer_ground.add_child(dev_tile)
				layer_ground.move_child(dev_tile, 1)
				dev_tile.position = _tile_coords_to_vector_pos(tile)
				dev_tile.spawn(tile, Breakable.TileTypes.DEVASTATOR, false)
			
			Vector2i(1, 28): # Silent Devastator breakable
				var dev_tile:Breakable = breakable_scene.instantiate()
				layer_ground.add_child(dev_tile)
				layer_ground.move_child(dev_tile, 1)
				dev_tile.position = _tile_coords_to_vector_pos(tile)
				dev_tile.spawn(tile, Breakable.TileTypes.DEVASTATOR, true)
			
			Vector2i(12, 30): # Hanging grass
				var grass:Grass = load("res://Scenes/Entities/Grass.tscn").instantiate()
				layer_ground.add_child(grass)
				grass.position = _tile_coords_to_vector_pos(tile)
				grass.spawn(Grass.GrassTypes.NORMAL, Statics.DirsSurface.CEILING)
			
			Vector2i(13, 30): # Hanging power grass
				var grass:Grass = load("res://Scenes/Entities/Grass.tscn").instantiate()
				layer_ground.add_child(grass)
				grass.position = _tile_coords_to_vector_pos(tile)
				grass.spawn(Grass.GrassTypes.POWER, Statics.DirsSurface.CEILING)


func _tile_coords_to_vector_pos(coords:Vector2i) -> Vector2:
	return (coords * 16) + Vector2i(8, 8)
