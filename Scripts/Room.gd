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
@export_file("*.txt") var cutscene_script:String = "res://CutsceneScripts/test.txt"
@export var cutscene_animator:AnimationPlayer
@export var center_parallax_maps:bool = false
@export var minimap_offset:Vector2i = Vector2i.ZERO
@export var minimap_autofill:Array[Vector2i] = []
@export var song_change:MusicManager.Loops = MusicManager.Loops.None

@export_group("Tiled importing")
@export_file("*.tmx") var tiled_path:String = "res://Resources/map.tmx"
@export var tiled_corner:Vector2i = Vector2.ZERO
@export var tiled_range:Vector2i = Vector2.ZERO
@export var tiled_layers:Array[String] = [ "sky", "", "", "bg", "fg", "", "sp" ]
@export_tool_button("Import tile data") var import_button:Callable = _import_from_tiled
#endregion


#region Internals
static var areas:Array = [
	"SnailTown",
	"MareCarelia",
	"SpiralisSilere",
	"AmastridaAbyssus",
	"LuxLirata",
	"ShrineOfIris",
	"BossRush"
]

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

@onready var breakable_scene = preload("res://Scenes/Entities/Breakable.tscn")
@onready var special_collision_scene = preload("res://Scenes/Entities/SpecialCollision.tscn")
#endregion
#endregion


func spawn(_spawn_all:bool) -> void:
	if Statics.show_entity_layer:
		map_entity.modulate = Color(1, 1, 1, 0.5)
	else:
		map_entity.modulate = Color(1, 1, 1, 0)
	get_room_name_from_filename()
	
	# Get all entity tiles and spawn associated objects
	_spawn_entities_from_layer()
	
	# Properly spawn all objects in room
	if _spawn_all:
		var layer_array:Array[Node2D] = [ layer_sky, layer_bg2, layer_bg1, layer_ground, layer_fg1, layer_fg2 ]
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
				fake_border.original_room_name = room_path
	
	if song_change != MusicManager.Loops.None:
		GameCore.instance.music_manager.play_song(song_change)
	
	if center_parallax_maps:
		center_maps()
	
	UICore.instance.minimap.room_offset = minimap_offset
	for cell in minimap_autofill:
		UICore.instance.minimap.fill_cell(cell)
	UICore.instance.minimap.set_room_name(room_path)


func get_room_name_from_filename() -> void:
	var trimmed_name := self.scene_file_path
	var path_parts: = Statics.ROOM_PATH.split("%s")
	trimmed_name = trimmed_name.substr(path_parts[0].length())
	trimmed_name = trimmed_name.substr(0, trimmed_name.length() - path_parts[1].length())
	room_path = trimmed_name


func center_maps() -> void:
	var room_size := bounds.get_bound_size()
	for layer:int in [ Layers.SKY, Layers.BG2, Layers.BG1, Layers.FG1, Layers.FG2 ]:
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
		var scroll := this_layer.scroll_scale
		if scroll != Vector2(1.0, 1.0):
			var new_offset := Vector2(
				room_size.x - (room_size.x / scroll.x),
				room_size.y - (room_size.y / scroll.y)
			) * 0.5 * scroll
			this_layer.scroll_offset = new_offset


func get_actors() -> Array[CutsceneControllable]:
	assert(Engine.is_editor_hint(), "Only intended to be used in the editor")
	var arr:Array[CutsceneControllable] = []
	_recur_extr(arr, self)
	return arr


func _recur_extr(arr:Array[CutsceneControllable], n:Node) -> void:
	for i in n.get_children():
		_recur_extr(arr, i)
	if n is CutsceneControllable:
		arr.append(n)


func _spawn_entities_from_layer() -> void:
	for tile in map_entity.get_used_cells():
		var tile_coords := map_entity.get_cell_atlas_coords(tile)
		match tile_coords:
			Vector2i(4, 0): # Blob
				var blob:BlobCommon = load("res://Scenes/Entities/Enemies/BlobCommon.tscn").instantiate()
				blob.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(blob)
			
			Vector2i(5, 0): # Blub
				var blob:BlobTough = load("res://Scenes/Entities/Enemies/BlobTough.tscn").instantiate()
				blob.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(blob)
			
			Vector2i(7, 0): # Blue chirpy
				var chirpy:ChirpyCommon = load("res://Scenes/Entities/Enemies/ChirpyCommon.tscn").instantiate()
				chirpy.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(chirpy)
			
			Vector2i(10, 0): # Blue chirpy generator
				var gen:GeneratorChirpyCommon = load("res://Scenes/Entities/Enemies/Generators/GeneratorChirpyCommon.tscn").instantiate()
				gen.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(gen)
			
			Vector2i(11, 0): # Blue spikey (CW)
				var spikey:SpikeyCommon = load("res://Scenes/Entities/Enemies/SpikeyCommon.tscn").instantiate()
				spikey.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(spikey)
			
			Vector2i(12, 0): # Blue spikey (CCW)
				var spikey:SpikeyCommon = load("res://Scenes/Entities/Enemies/SpikeyCommon.tscn").instantiate()
				spikey.position = _tile_coords_to_vector_pos(tile)
				spikey.ccw = true
				layer_ground.add_child(spikey)
			
			Vector2i(13, 0): # Orange spikey (CW)
				var spikey:SpikeyTough = load("res://Scenes/Entities/Enemies/SpikeyTough.tscn").instantiate()
				spikey.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(spikey)
			
			Vector2i(14, 0): # Orange spikey (CCW)
				var spikey:SpikeyTough = load("res://Scenes/Entities/Enemies/SpikeyTough.tscn").instantiate()
				spikey.position = _tile_coords_to_vector_pos(tile)
				spikey.ccw = true
				layer_ground.add_child(spikey)
			
			Vector2i(11, 1): # Grass
				var grass:Grass = load("res://Scenes/Entities/Grass.tscn").instantiate()
				grass.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(grass)
				grass.spawn(Grass.GrassTypes.NORMAL, Statics.DirsSurface.FLOOR)
			
			Vector2i(14, 1): # Power grass
				var grass:Grass = load("res://Scenes/Entities/Grass.tscn").instantiate()
				grass.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(grass)
				grass.spawn(Grass.GrassTypes.POWER, Statics.DirsSurface.FLOOR)
			
			Vector2i(15, 1): # Smoke particle
				Statics.spawn_particle("Smoke", Room.Layers.BG1, _tile_coords_to_vector_pos(tile))
			
			Vector2i(8, 4): # Boomerang breakable
				var boom_tile:Breakable = breakable_scene.instantiate()
				boom_tile.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(boom_tile)
				layer_ground.move_child(boom_tile, 1)
				boom_tile.spawn(tile, Breakable.TileTypes.BOOMERANG, false)
			
			Vector2i(9, 4): # Rainbow Wave breakable
				var wave_tile:Breakable = breakable_scene.instantiate()
				wave_tile.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(wave_tile)
				layer_ground.move_child(wave_tile, 1)
				wave_tile.spawn(tile, Breakable.TileTypes.RAINBOW_WAVE, false)
			
			Vector2i(10, 4): # Devastator breakable
				var dev_tile:Breakable = breakable_scene.instantiate()
				dev_tile.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(dev_tile)
				layer_ground.move_child(dev_tile, 1)
				dev_tile.spawn(tile, Breakable.TileTypes.DEVASTATOR, false)
			
			Vector2i(2, 24): # Enemy solid tile
				var enemy_tile:SpecialCollision = special_collision_scene.instantiate()
				enemy_tile.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(enemy_tile)
				layer_ground.move_child(enemy_tile, 1)
				enemy_tile.set_full()
				enemy_tile.set_collision_enemy()
			
			Vector2i(3, 24): # Ice spike (floor)
				var spike:IceSpike = load("res://Scenes/Entities/Hazards/IceSpike.tscn").instantiate()
				spike.position = _tile_coords_to_vector_pos(tile)
				spike.direction = Statics.DirsSurface.FLOOR
				layer_ground.add_child(spike)
			
			Vector2i(4, 24): # Ice spike (ceiling)
				var spike:IceSpike = load("res://Scenes/Entities/Hazards/IceSpike.tscn").instantiate()
				spike.position = _tile_coords_to_vector_pos(tile)
				spike.direction = Statics.DirsSurface.CEILING
				layer_ground.add_child(spike)
			
			Vector2i(5, 24): # Ice spike (left wall)
				var spike:IceSpike = load("res://Scenes/Entities/Hazards/IceSpike.tscn").instantiate()
				spike.position = _tile_coords_to_vector_pos(tile)
				spike.direction = Statics.DirsSurface.LWALL
				layer_ground.add_child(spike)
			
			Vector2i(6, 24): # Ice spike (right wall)
				var spike:IceSpike = load("res://Scenes/Entities/Hazards/IceSpike.tscn").instantiate()
				spike.position = _tile_coords_to_vector_pos(tile)
				spike.direction = Statics.DirsSurface.RWALL
				layer_ground.add_child(spike)
			
			Vector2i(15, 24): # Black floatspike
				var floatspike:FloatspikeCommon = load("res://Scenes/Entities/Enemies/FloatspikeCommon.tscn").instantiate()
				floatspike.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(floatspike)
			
			Vector2i(7, 25): # Aqua chirpy
				var chirpy:ChirpyTough = load("res://Scenes/Entities/Enemies/ChirpyTough.tscn").instantiate()
				chirpy.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(chirpy)
			
			Vector2i(8, 26): # Batty bat
				var bat:BattyBat = load("res://Scenes/Entities/Enemies/BattyBat.tscn").instantiate()
				bat.position = _tile_coords_to_vector_pos(tile) + Vector2(8, 0)
				layer_ground.add_child(bat)
			
			Vector2i(1, 28): # Silent Devastator breakable
				var dev_tile:Breakable = breakable_scene.instantiate()
				dev_tile.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(dev_tile)
				layer_ground.move_child(dev_tile, 1)
				dev_tile.spawn(tile, Breakable.TileTypes.DEVASTATOR, true)
			
			Vector2i(7, 28): # Green babyfish
				var fish:Babyfish1 = load("res://Scenes/Entities/Enemies/Babyfish1.tscn").instantiate()
				fish.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(fish)
			
			Vector2i(8, 28): # Pink babyfish
				var fish:Babyfish2 = load("res://Scenes/Entities/Enemies/Babyfish2.tscn").instantiate()
				fish.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(fish)
			
			Vector2i(13, 28): # Aqua chirpy generator
				var gen:GeneratorChirpyTough = load("res://Scenes/Entities/Enemies/Generators/GeneratorChirpyTough.tscn").instantiate()
				gen.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(gen)
			
			Vector2i(12, 30): # Hanging grass
				var grass:Grass = load("res://Scenes/Entities/Grass.tscn").instantiate()
				grass.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(grass)
				grass.spawn(Grass.GrassTypes.NORMAL, Statics.DirsSurface.CEILING)
			
			Vector2i(13, 30): # Hanging power grass
				var grass:Grass = load("res://Scenes/Entities/Grass.tscn").instantiate()
				grass.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(grass)
				grass.spawn(Grass.GrassTypes.POWER, Statics.DirsSurface.CEILING)
			
			Vector2i(2, 31): # Peashooter breakable
				var pea_tile:Breakable = breakable_scene.instantiate()
				pea_tile.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(pea_tile)
				layer_ground.move_child(pea_tile, 1)
				pea_tile.spawn(tile, Breakable.TileTypes.PEASHOOTER, false)


func _tile_coords_to_vector_pos(coords:Vector2i) -> Vector2:
	return (coords * 16) + Vector2i(8, 8)


func _import_from_tiled():
	assert(Engine.is_editor_hint(), "_import_from_tiled() must only be called in the editor.")
	if not Engine.is_editor_hint():
		return
	
	var target_layer:int = -1
	var layer_size:Vector2i = Vector2i.ZERO
	var tilesheet_size:Vector2i = Vector2i.ZERO
	var parser := XMLParser.new()
	
	parser.open(tiled_path)
	while parser.read() != ERR_FILE_EOF:
		if parser.get_node_type() == XMLParser.NODE_ELEMENT:
			var node_name := parser.get_node_name()
			match node_name:
				"layer":
					var layer_name := parser.get_attribute_value(1)
					target_layer = -1
					if tiled_layers.has(layer_name):
						target_layer = tiled_layers.find(layer_name)
						layer_size = Vector2(
							int(parser.get_attribute_value(2)),
							int(parser.get_attribute_value(3))
						)
					#print(layer_name)
					#print(layer_size)
				"image":
					tilesheet_size = Vector2(
							int(parser.get_attribute_value(1)),
							int(parser.get_attribute_value(2))
						) / 16
		elif parser.get_node_type() == XMLParser.NODE_TEXT:
			if target_layer != -1:
				var data := parser.get_node_data()
				data.replace(" ", "")
				data.replace("\n", "")
				var this_line := parser.get_node_data().split(",")
				if this_line.size() > 1:
					for y in range(tiled_corner.y, tiled_corner.y + tiled_range.y):
						for x in range(tiled_corner.x, tiled_corner.x + tiled_range.x):
							var array_i := (y * layer_size.x) + x
							var tile := this_line[array_i]
							tile = tile.strip_edges()
							var tile_id:int = 0
							if tile.is_valid_int():
								tile_id = int(tile)
							tile_id -= 1
							if tile_id >= -1:
								var tile_coords := Vector2i(tile_id, 0)
								var source:int = 0
								if tile_id == -1:
									tile_coords = Vector2i.ZERO
									source = -1
								while tile_coords.x >= tilesheet_size.x:
									tile_coords.x -= tilesheet_size.x
									tile_coords.y += 1
								var map_index := Vector2i(x, y)
								print("Placing %s at %s" % [ tile_coords, map_index - tiled_corner ])
								match target_layer:
									Layers.SKY:
										map_sky.set_cell(map_index - tiled_corner, source, tile_coords)
									Layers.BG2:
										map_bg2.set_cell(map_index - tiled_corner, source, tile_coords)
									Layers.BG1:
										map_bg1.set_cell(map_index - tiled_corner, source, tile_coords)
									Layers.GROUND:
										map_ground.set_cell(map_index - tiled_corner, source, tile_coords)
									Layers.FG1:
										map_fg1.set_cell(map_index - tiled_corner, source, tile_coords)
									Layers.FG2:
										map_fg2.set_cell(map_index - tiled_corner, source, tile_coords)
									Layers.ENTITY:
										map_entity.set_cell(map_index - tiled_corner, source, tile_coords)
