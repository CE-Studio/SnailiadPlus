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
@export_range(0.0, 1.0) var darkness_level:float = 0.
@export var cutscene_script:DialogueResource
@export var cutscene_animator:AnimationPlayer
@export var center_parallax_maps:bool = false
@export var minimap_offset:Vector2i = Vector2i.ZERO
@export var minimap_autofill:Array[Vector2i] = []
@export var song_change:MusicManager.Loops = MusicManager.Loops.None
@export var play_song_on_enter:bool = true
@export var display_room:bool = false

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

static var instance:Room

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

static var sp_cache:Dictionary[StringName, PackedScene] = {}
var cells:Array[Vector2i] = []
var spawned_sp:int = 0
var last_spawned_layer:int = -1
var spawned_all:bool = false
var spawned_secondary:bool = false
var skipped_first_process_spawn:bool = false
var collision_enabled:bool = true
const MAX_SP_PER_LOOP:int = 48
const MAX_MS_PER_LOOP:int = 23
const BREAKABLE_IDS:Array[Vector2i] = [
	Vector2i(8, 4),
	Vector2i(9, 4),
	Vector2i(10, 4),
	Vector2i(1, 28),
	Vector2i(2, 31),
]

signal despawn

@onready var layer_entity:Node2D = $"EntityLayer"
@onready var map_entity1:TileMapLayer = $"EntityLayer/Map"
@onready var map_entity2:TileMapLayer = $"EntityLayer/Map2"
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
@onready var default_spawn:Marker2D = $"DefaultSpawn"

@onready var breakable_scene = preload("res://Scenes/Entities/Breakable.tscn")
@onready var special_collision_scene = preload("res://Scenes/Entities/SpecialCollision.tscn")
#endregion
#endregion


func _ready() -> void:
	if Engine.is_editor_hint():
		return
	instance = self
	if GameCore.instance == null and not display_room:
		Statics.current_profile = Statics.data_profile1
		Statics.current_profile_id = 1
		Statics.load_room = Statics.ROOM_PATH % (areas[area_id] + "/" + name if area_id != -1 else name)
		Statics.load_coords = default_spawn.position
		Statics.shortcut_load_game_scene = true
		get_tree().call_deferred("change_scene_to_file", "res://Scenes/PreloadScene.tscn")
	if UICore.instance:
		UICore.instance.darkness_layer.call_deferred("update_col", darkness_level)
	var ar:Array[CutsceneControllable] = get_actors()
	var iar:Array[StringName]
	for i in ar:
		if i.identifier in iar:
			push_error("Multiple actors are using the ID \"", i.identifier, "\"")
		else:
			iar.append(i.identifier)
	CutsceneControllable.actors = ar


func spawn(_spawn_all:bool) -> void:
	if Engine.is_editor_hint():
		return

	if Statics.show_entity_layer:
		map_entity1.modulate = Color(1, 1, 1, 0.5)
		map_entity2.modulate = Color(1, 1, 1, 0.5)
	else:
		map_entity1.modulate = Color(1, 1, 1, 0)
		map_entity2.modulate = Color(1, 1, 1, 0)
	get_room_name_from_filename()

	# Get all entity tiles and spawn associated objects
	_spawn_entities_from_layer(1)
	_spawn_entities_from_layer(0)

	# Properly spawn all objects in room
	if _spawn_all:
		var layer_array:Array[Node2D] = [ layer_sky, layer_bg2, layer_bg1, layer_ground, layer_fg1, layer_fg2, self ]
		for layer in layer_array:
			for child in layer.get_children():
				if (child is Door
				or child is NPC):
					child.spawn()
				if child is SavePoint:
					if child.check_character_spawnable():
						child.initialize_room_data(room_path)
				if child is ParticleLayer:
					child.spawn()
		for fake_border in bounds.get_children():
			if fake_border is FakeCamBoundary:
				fake_border.call_deferred("instance")
				fake_border.original_room_name = room_path

	if song_change != MusicManager.Loops.None and play_song_on_enter:
		GameCore.instance.music_manager.play_song(song_change)

	if center_parallax_maps:
		center_maps()

	if UICore.instance and not display_room:
		UICore.instance.minimap.room_offset = minimap_offset
		for cell in minimap_autofill:
			UICore.instance.minimap.fill_cell(cell)
		UICore.instance.minimap.set_room_name(room_path)


func _process(_delta: float) -> void:
	# Try to spawn objects if not all of them have been placed yet, but avoid first tick of _process
	if not spawned_all and not Engine.is_editor_hint() and skipped_first_process_spawn:
		_spawn_entities_from_layer(0)
	skipped_first_process_spawn = true
	pass


func get_room_name_from_filename() -> void:
	var trimmed_name := self.scene_file_path
	var path_parts := Statics.ROOM_PATH.split("%s")
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
			this_layer.scroll_offset += new_offset


func get_actors() -> Array[CutsceneControllable]:
	var arr:Array[CutsceneControllable] = []
	_recur_extr(arr, self)
	return arr


func _recur_extr(arr:Array[CutsceneControllable], n:Node) -> void:
	for i in n.get_children():
		_recur_extr(arr, i)
	if n is CutsceneControllable:
		arr.append(n)


#region Cell sorting
func _sort_tl(a:Vector2i, b:Vector2i) -> bool:
	if a.x == b.x:
		return a.y < b.y
	return a.x < b.x


func _sort_tr(a:Vector2i, b:Vector2i) -> bool:
	if a.x == b.x:
		return a.y < b.y
	return a.x > b.x


func _sort_bl(a:Vector2i, b:Vector2i) -> bool:
	if a.x == b.x:
		return a.y > b.y
	return a.x < b.x


func _sort_br(a:Vector2i, b:Vector2i) -> bool:
	if a.x == b.x:
		return a.y > b.y
	return a.x > b.x
#endregion


func _spawn_entities_from_layer(layer:int) -> void:
	var map:TileMapLayer = map_entity1 if layer == 0 else map_entity2
	if cells.is_empty() or last_spawned_layer != layer:
		if last_spawned_layer != layer:
			spawned_sp = 0
		last_spawned_layer = layer
		cells = map.get_used_cells()
		if layer == 1 and cells.is_empty():
			spawned_secondary = true
			return
		var center:Vector2 = bounds.get_center()
		if GameCore.instance:
			var player:Vector2 = GameCore.instance.player.position
			if player.y > center.y:
				if player.x > center.x:
					cells.sort_custom(_sort_br)
				else:
					cells.sort_custom(_sort_bl)
			elif player.x > center.x:
				cells.sort_custom(_sort_tr)
			else:
				cells.sort_custom(_sort_tl)
	#print("%d - %d/%d" % [layer, spawned_sp, cells.size()])
	var running_count:int = 0
	if layer == 1:
		running_count = MAX_SP_PER_LOOP - cells.size()
	var tpf:Array[int] = []
	var fts := Time.get_ticks_msec()
	while ((spawned_sp < cells.size()) and (running_count < MAX_SP_PER_LOOP)
	and ((Time.get_ticks_msec() - fts) < MAX_MS_PER_LOOP)):
		var ts := Time.get_ticks_msec()
		var tile := cells[spawned_sp]
		var tile_coords := map.get_cell_atlas_coords(tile)
		spawned_sp += 1
		running_count += 1

		match tile_coords:
			Vector2i(4, 0): # Blob
				var blob:BlobCommon = _load(&"res://Scenes/Entities/Enemies/BlobCommon.tscn").instantiate()
				blob.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(blob)

			Vector2i(5, 0): # Blub
				var blob:BlobTough = _load(&"res://Scenes/Entities/Enemies/BlobTough.tscn").instantiate()
				blob.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(blob)

			Vector2i(6, 0): # Devilblob
				var blob:BlobDevil = _load(&"res://Scenes/Entities/Enemies/BlobDevil.tscn").instantiate()
				blob.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(blob)

			Vector2i(7, 0): # Blue chirpy
				var chirpy:ChirpyCommon = _load(&"res://Scenes/Entities/Enemies/ChirpyCommon.tscn").instantiate()
				chirpy.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(chirpy)

			Vector2i(8, 0): # Gray kitty
				var kitty:KittyCommon = _load(&"res://Scenes/Entities/Enemies/KittyCommon.tscn").instantiate()
				kitty.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(kitty)

			Vector2i(9, 0): # Orange kitty
				var kitty:KittyTough = _load(&"res://Scenes/Entities/Enemies/KittyTough.tscn").instantiate()
				kitty.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(kitty)

			Vector2i(10, 0): # Blue chirpy generator
				var gen:GeneratorChirpyCommon = _load(&"res://Scenes/Entities/Enemies/Generators/GeneratorChirpyCommon.tscn").instantiate()
				gen.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(gen)

			Vector2i(11, 0): # Blue spikey (CW)
				var spikey:SpikeyCommon = _load(&"res://Scenes/Entities/Enemies/SpikeyCommon.tscn").instantiate()
				spikey.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(spikey)

			Vector2i(12, 0): # Blue spikey (CCW)
				var spikey:SpikeyCommon = _load(&"res://Scenes/Entities/Enemies/SpikeyCommon.tscn").instantiate()
				spikey.position = _tile_coords_to_vector_pos(tile)
				spikey.ccw = true
				layer_ground.add_child(spikey)

			Vector2i(13, 0): # Orange spikey (CW)
				var spikey:SpikeyTough = _load(&"res://Scenes/Entities/Enemies/SpikeyTough.tscn").instantiate()
				spikey.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(spikey)

			Vector2i(14, 0): # Orange spikey (CCW)
				var spikey:SpikeyTough = _load(&"res://Scenes/Entities/Enemies/SpikeyTough.tscn").instantiate()
				spikey.position = _tile_coords_to_vector_pos(tile)
				spikey.ccw = true
				layer_ground.add_child(spikey)

			Vector2i(15, 0): # Fireball (CW)
				var fireball:Fireball = _load(&"res://Scenes/Entities/Enemies/Fireball.tscn").instantiate()
				fireball.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(fireball)

			Vector2i(0, 1): # Fireball (CCW)
				var fireball:Fireball = _load(&"res://Scenes/Entities/Enemies/Fireball.tscn").instantiate()
				fireball.position = _tile_coords_to_vector_pos(tile)
				fireball.ccw = true
				layer_ground.add_child(fireball)

			Vector2i(1, 1): # Iceball (CW)
				var iceball:Iceball = _load(&"res://Scenes/Entities/Enemies/Iceball.tscn").instantiate()
				iceball.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(iceball)

			Vector2i(2, 1): # Iceball (CCW)
				var iceball:Iceball = _load(&"res://Scenes/Entities/Enemies/Iceball.tscn").instantiate()
				iceball.position = _tile_coords_to_vector_pos(tile)
				iceball.ccw = true
				layer_ground.add_child(iceball)

			Vector2i(3, 1): # Ghost dandelion generator
				var gen:GeneratorGhostball = _load(&"res://Scenes/Entities/Enemies/Generators/GeneratorGhostball.tscn").instantiate()
				gen.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(gen)

			Vector2i(7, 1): # Shellbreaker
				var shellbreaker:Shellbreaker = _load(&"res://Scenes/Entities/Enemies/Bosses/Shellbreaker.tscn").instantiate()
				shellbreaker.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(shellbreaker)

			Vector2i(8, 1): # Stompy
				var stompy:Stompy = _load(&"res://Scenes/Entities/Enemies/Bosses/Stompy.tscn").instantiate()
				stompy.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(stompy)

			Vector2i(11, 1): # Grass
				var grass:Grass = _load(&"res://Scenes/Entities/Grass.tscn").instantiate()
				grass.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(grass)
				grass.spawn(Grass.GrassTypes.NORMAL, Statics.DirsSurface.FLOOR)

			Vector2i(14, 1): # Power grass
				var grass:Grass = _load(&"res://Scenes/Entities/Grass.tscn").instantiate()
				grass.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(grass)
				grass.spawn(Grass.GrassTypes.POWER, Statics.DirsSurface.FLOOR)

			Vector2i(15, 1): # Smoke effect tile
				var smoke_tile:JsonSprite2D = _load(&"res://Scenes/Environments/SmokeTile.tscn").instantiate()
				smoke_tile.position = _tile_coords_to_vector_pos(tile)
				layer_fg2.add_child(smoke_tile)

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
				var spike:IceSpike = _load(&"res://Scenes/Entities/Hazards/IceSpike.tscn").instantiate()
				spike.position = _tile_coords_to_vector_pos(tile)
				spike.direction = Statics.DirsSurface.FLOOR
				layer_ground.add_child(spike)

			Vector2i(4, 24): # Ice spike (ceiling)
				var spike:IceSpike = _load(&"res://Scenes/Entities/Hazards/IceSpike.tscn").instantiate()
				spike.position = _tile_coords_to_vector_pos(tile)
				spike.direction = Statics.DirsSurface.CEILING
				layer_ground.add_child(spike)

			Vector2i(5, 24): # Ice spike (left wall)
				var spike:IceSpike = _load(&"res://Scenes/Entities/Hazards/IceSpike.tscn").instantiate()
				spike.position = _tile_coords_to_vector_pos(tile)
				spike.direction = Statics.DirsSurface.LWALL
				layer_ground.add_child(spike)

			Vector2i(6, 24): # Ice spike (right wall)
				var spike:IceSpike = _load(&"res://Scenes/Entities/Hazards/IceSpike.tscn").instantiate()
				spike.position = _tile_coords_to_vector_pos(tile)
				spike.direction = Statics.DirsSurface.RWALL
				layer_ground.add_child(spike)

			Vector2i(7, 24): # Canon (floor)
				var canon:Canon = _load(&"res://Scenes/Entities/Enemies/Canon.tscn").instantiate()
				canon.position = _tile_coords_to_vector_pos(tile)
				canon.base_dir = Statics.DirsSurface.FLOOR
				layer_ground.add_child(canon)

			Vector2i(8, 24): # Canon (left wall)
				var canon:Canon = _load(&"res://Scenes/Entities/Enemies/Canon.tscn").instantiate()
				canon.position = _tile_coords_to_vector_pos(tile)
				canon.base_dir = Statics.DirsSurface.LWALL
				layer_ground.add_child(canon)

			Vector2i(9, 24): # Canon (right wall)
				var canon:Canon = _load(&"res://Scenes/Entities/Enemies/Canon.tscn").instantiate()
				canon.position = _tile_coords_to_vector_pos(tile)
				canon.base_dir = Statics.DirsSurface.RWALL
				layer_ground.add_child(canon)

			Vector2i(10, 24): # Canon (ceiling)
				var canon:Canon = _load(&"res://Scenes/Entities/Enemies/Canon.tscn").instantiate()
				canon.position = _tile_coords_to_vector_pos(tile)
				canon.base_dir = Statics.DirsSurface.CEILING
				layer_ground.add_child(canon)

			Vector2i(12, 24): # Muck
				var muck:Muck = _load(&"res://Scenes/Entities/Hazards/Muck.tscn").instantiate()
				muck.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(muck)

			Vector2i(13, 24): # Ghost dandelion
				var ghostball:Ghostball = _load(&"res://Scenes/Entities/Enemies/Ghostball.tscn").instantiate()
				ghostball.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(ghostball)

			Vector2i(15, 24): # Black floatspike
				var floatspike:FloatspikeCommon = _load(&"res://Scenes/Entities/Enemies/FloatspikeCommon.tscn").instantiate()
				floatspike.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(floatspike)

			Vector2i(3, 25): # Spinnygear (up)
				var gear:GearCommon = _load(&"res://Scenes/Entities/Enemies/GearCommon.tscn").instantiate()
				gear.position = _tile_coords_to_vector_pos(tile) + Vector2(8, 8)
				gear.direction = Statics.DirsCardinal.UP
				layer_ground.add_child(gear)

			Vector2i(4, 25): # Spinnygear (down)
				var gear:GearCommon = _load(&"res://Scenes/Entities/Enemies/GearCommon.tscn").instantiate()
				gear.position = _tile_coords_to_vector_pos(tile) + Vector2(8, 8)
				gear.direction = Statics.DirsCardinal.DOWN
				layer_ground.add_child(gear)

			Vector2i(5, 25): # Spinnygear (right)
				var gear:GearCommon = _load(&"res://Scenes/Entities/Enemies/GearCommon.tscn").instantiate()
				gear.position = _tile_coords_to_vector_pos(tile) + Vector2(8, 8)
				gear.direction = Statics.DirsCardinal.RIGHT
				layer_ground.add_child(gear)

			Vector2i(6, 25): # Spinnygear (left)
				var gear:GearCommon = _load(&"res://Scenes/Entities/Enemies/GearCommon.tscn").instantiate()
				gear.position = _tile_coords_to_vector_pos(tile) + Vector2(8, 8)
				gear.direction = Statics.DirsCardinal.LEFT
				layer_ground.add_child(gear)

			Vector2i(7, 25): # Aqua chirpy
				var chirpy:ChirpyTough = _load(&"res://Scenes/Entities/Enemies/ChirpyTough.tscn").instantiate()
				chirpy.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(chirpy)

			Vector2i(8, 25): # Snakey (common)
				var snakey:SnakeyCommon = _load(&"res://Scenes/Entities/Enemies/SnakeyCommon.tscn").instantiate()
				snakey.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(snakey)

			Vector2i(9, 25): # Pincer
				var pincer:Pincer = _load(&"res://Scenes/Entities/Enemies/Pincer.tscn").instantiate()
				pincer.position = _tile_coords_to_vector_pos(tile)
				pincer.direction = Statics.DirsSurface.FLOOR
				layer_ground.add_child(pincer)

			Vector2i(10, 25): # Sky Pincer
				var pincer:Pincer = _load(&"res://Scenes/Entities/Enemies/Pincer.tscn").instantiate()
				pincer.position = _tile_coords_to_vector_pos(tile)
				pincer.direction = Statics.DirsSurface.CEILING
				layer_ground.add_child(pincer)

			Vector2i(11, 25): # Jellyfish
				var jellyfish:Jellyfish = _load(&"res://Scenes/Entities/Enemies/Jellyfish.tscn").instantiate()
				jellyfish.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(jellyfish)

			Vector2i(12, 25): # Syngnathida
				var seahorse:Seahorse = _load(&"res://Scenes/Entities/Enemies/Seahorse.tscn").instantiate()
				seahorse.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(seahorse)

			Vector2i(13, 25): # Tallfish
				var tallfish:TallfishCommon = _load(&"res://Scenes/Entities/Enemies/TallfishCommon.tscn").instantiate()
				tallfish.position = _tile_coords_to_vector_pos(tile) + Vector2(8, 16)
				layer_ground.add_child(tallfish)

			Vector2i(14, 25): # Federation drone
				var drone:Drone = _load(&"res://Scenes/Entities/Enemies/Drone.tscn").instantiate()
				drone.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(drone)

			Vector2i(15, 25): # Walleye (right)
				var walleye:Walleye = _load(&"res://Scenes/Entities/Enemies/Walleye.tscn").instantiate()
				walleye.position = _tile_coords_to_vector_pos(tile)
				walleye.direction = 2
				layer_ground.add_child(walleye)

			Vector2i(0, 26): # Spider
				var spider:SpiderCommon = _load(&"res://Scenes/Entities/Enemies/SpiderCommon.tscn").instantiate()
				spider.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(spider)

			Vector2i(1, 26): # Spider mama
				var spider:SpiderTough = _load(&"res://Scenes/Entities/Enemies/SpiderTough.tscn").instantiate()
				spider.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(spider)

			Vector2i(2, 26): # Turtle (right)
				var turtle:TurtleCommon = _load(&"res://Scenes/Entities/Enemies/TurtleCommon.tscn").instantiate()
				turtle.position = _tile_coords_to_vector_pos(tile) + Vector2(0, 8)
				turtle.direction = Statics.DirsSurface.RWALL
				layer_ground.add_child(turtle)

			Vector2i(3, 26): # Turtle (left)
				var turtle:TurtleCommon = _load(&"res://Scenes/Entities/Enemies/TurtleCommon.tscn").instantiate()
				turtle.position = _tile_coords_to_vector_pos(tile) + Vector2(0, 8)
				turtle.direction = Statics.DirsSurface.LWALL
				layer_ground.add_child(turtle)

			Vector2i(4, 26): # Snakey (tough)
				var snakey:SnakeyTough = _load(&"res://Scenes/Entities/Enemies/SnakeyTough.tscn").instantiate()
				snakey.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(snakey)

			Vector2i(5, 26): # Cherry turtle (right)
				var turtle:TurtleTough = _load(&"res://Scenes/Entities/Enemies/TurtleTough.tscn").instantiate()
				turtle.position = _tile_coords_to_vector_pos(tile) + Vector2(8, 0)
				turtle.direction = Statics.DirsSurface.RWALL
				layer_ground.add_child(turtle)

			Vector2i(6, 26): # Cherry turtle (left)
				var turtle:TurtleTough = _load(&"res://Scenes/Entities/Enemies/TurtleTough.tscn").instantiate()
				turtle.position = _tile_coords_to_vector_pos(tile) + Vector2(8, 0)
				turtle.direction = Statics.DirsSurface.LWALL
				layer_ground.add_child(turtle)

			Vector2i(7, 26): # Balloon buster
				var balloon:Balloon = _load(&"res://Scenes/Entities/Enemies/Balloon.tscn").instantiate()
				balloon.position = _tile_coords_to_vector_pos(tile) + Vector2(8, 0)
				layer_ground.add_child(balloon)

			Vector2i(8, 26): # Batty bat
				var bat:BattyBat = _load(&"res://Scenes/Entities/Enemies/Battybat.tscn").instantiate()
				bat.position = _tile_coords_to_vector_pos(tile) + Vector2(8, 0)
				layer_ground.add_child(bat)

			Vector2i(9, 26): # Angelblob
				var blob:BlobAngel = _load(&"res://Scenes/Entities/Enemies/BlobAngel.tscn").instantiate()
				blob.position = _tile_coords_to_vector_pos(tile) + Vector2(8, 0)
				layer_ground.add_child(blob)

			Vector2i(10, 26): # Angry tallfish
				var tallfish:TallfishTough = _load(&"res://Scenes/Entities/Enemies/TallfishTough.tscn").instantiate()
				tallfish.position = _tile_coords_to_vector_pos(tile) + Vector2(8, 16)
				layer_ground.add_child(tallfish)

			Vector2i(11, 26): # Walleye (left)
				var walleye:Walleye = _load(&"res://Scenes/Entities/Enemies/Walleye.tscn").instantiate()
				walleye.position = _tile_coords_to_vector_pos(tile)
				walleye.direction = 6
				layer_ground.add_child(walleye)

			Vector2i(12, 26): # Blue floatspike
				var floatspike:FloatspikeTough = _load(&"res://Scenes/Entities/Enemies/FloatspikeTough.tscn").instantiate()
				floatspike.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(floatspike)

			Vector2i(13, 26): # Sky viper
				var snakey:Skyviper = _load(&"res://Scenes/Entities/Enemies/Skyviper.tscn").instantiate()
				snakey.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(snakey)

			Vector2i(14, 26): # Non-canon (floor)
				var noncanon:Noncanon = _load(&"res://Scenes/Entities/Enemies/Noncanon.tscn").instantiate()
				noncanon.position = _tile_coords_to_vector_pos(tile)
				noncanon.base_dir = Statics.DirsSurface.FLOOR
				layer_ground.add_child(noncanon)

			Vector2i(15, 26): # Non-Canon (left wall)
				var noncanon:Noncanon = _load(&"res://Scenes/Entities/Enemies/Noncanon.tscn").instantiate()
				noncanon.position = _tile_coords_to_vector_pos(tile)
				noncanon.base_dir = Statics.DirsSurface.LWALL
				layer_ground.add_child(noncanon)

			Vector2i(0, 27): # Non-Canon (right wall)
				var noncanon:Noncanon = _load(&"res://Scenes/Entities/Enemies/Noncanon.tscn").instantiate()
				noncanon.position = _tile_coords_to_vector_pos(tile)
				noncanon.base_dir = Statics.DirsSurface.RWALL
				layer_ground.add_child(noncanon)

			Vector2i(1, 27): # Non-Canon (ceiling)
				var noncanon:Noncanon = _load(&"res://Scenes/Entities/Enemies/Noncanon.tscn").instantiate()
				noncanon.position = _tile_coords_to_vector_pos(tile)
				noncanon.base_dir = Statics.DirsSurface.CEILING
				layer_ground.add_child(noncanon)

			Vector2i(2, 27): # Snelk
				var snelk:Snelk = _load(&"res://Scenes/Entities/Enemies/Snelk.tscn").instantiate()
				snelk.position = _tile_coords_to_vector_pos(tile) + Vector2(8, 0)
				layer_ground.add_child(snelk)

			Vector2i(3, 27): # Snelk (panicked)
				var snelk:Snelk = _load(&"res://Scenes/Entities/Enemies/Snelk.tscn").instantiate()
				snelk.position = _tile_coords_to_vector_pos(tile) + Vector2(8, 0)
				snelk.state = Snelk.States.RUN
				layer_ground.add_child(snelk)

			Vector2i(1, 28): # Silent Devastator breakable
				var dev_tile:Breakable = breakable_scene.instantiate()
				dev_tile.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(dev_tile)
				layer_ground.move_child(dev_tile, 1)
				dev_tile.spawn(tile, Breakable.TileTypes.DEVASTATOR, true)

			Vector2i(7, 28): # Green babyfish
				var fish:Babyfish1 = _load(&"res://Scenes/Entities/Enemies/Babyfish1.tscn").instantiate()
				fish.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(fish)

			Vector2i(8, 28): # Pink babyfish
				var fish:Babyfish2 = _load(&"res://Scenes/Entities/Enemies/Babyfish2.tscn").instantiate()
				fish.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(fish)

			Vector2i(13, 28): # Aqua chirpy generator
				var gen:GeneratorChirpyTough = _load(&"res://Scenes/Entities/Enemies/Generators/GeneratorChirpyTough.tscn").instantiate()
				gen.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(gen)

			Vector2i(14, 28): # Snelk (sleeping)
				var snelk:Snelk = _load(&"res://Scenes/Entities/Enemies/Snelk.tscn").instantiate()
				snelk.position = _tile_coords_to_vector_pos(tile) + Vector2(8, 0)
				snelk.state = Snelk.States.SLEEP
				layer_ground.add_child(snelk)

			Vector2i(15, 28): # Turtle (floor)
				var turtle:TurtleCommon = _load(&"res://Scenes/Entities/Enemies/TurtleCommon.tscn").instantiate()
				turtle.position = _tile_coords_to_vector_pos(tile) + Vector2(8, 0)
				turtle.direction = Statics.DirsSurface.FLOOR
				layer_ground.add_child(turtle)

			Vector2i(0, 29): # Turtle (ceiling)
				var turtle:TurtleCommon = _load(&"res://Scenes/Entities/Enemies/TurtleCommon.tscn").instantiate()
				turtle.position = _tile_coords_to_vector_pos(tile) + Vector2(8, 0)
				turtle.direction = Statics.DirsSurface.CEILING
				layer_ground.add_child(turtle)

			Vector2i(1, 29): # Cherry turtle (floor)
				var turtle:TurtleTough = _load(&"res://Scenes/Entities/Enemies/TurtleTough.tscn").instantiate()
				turtle.position = _tile_coords_to_vector_pos(tile) + Vector2(8, 0)
				turtle.direction = Statics.DirsSurface.FLOOR
				layer_ground.add_child(turtle)

			Vector2i(2, 29): # Cherry turtle (ceiling)
				var turtle:TurtleTough = _load(&"res://Scenes/Entities/Enemies/TurtleTough.tscn").instantiate()
				turtle.position = _tile_coords_to_vector_pos(tile) + Vector2(8, 0)
				turtle.direction = Statics.DirsSurface.CEILING
				layer_ground.add_child(turtle)

			Vector2i(3, 29): # Balloon buster generator
				var gen:GeneratorBalloon = _load(&"res://Scenes/Entities/Enemies/Generators/GeneratorBalloon.tscn").instantiate()
				gen.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(gen)

			Vector2i(4, 29): # Pouncer (left)
				var pincer:Pincer = _load(&"res://Scenes/Entities/Enemies/Pincer.tscn").instantiate()
				pincer.position = _tile_coords_to_vector_pos(tile)
				pincer.direction = Statics.DirsSurface.LWALL
				layer_ground.add_child(pincer)

			Vector2i(5, 29): # Pouncer (right)
				var pincer:Pincer = _load(&"res://Scenes/Entities/Enemies/Pincer.tscn").instantiate()
				pincer.position = _tile_coords_to_vector_pos(tile)
				pincer.direction = Statics.DirsSurface.RWALL
				layer_ground.add_child(pincer)

			Vector2i(11, 30): # Angry block
				var block:Angryblock = _load(&"res://Scenes/Entities/Enemies/Angryblock.tscn").instantiate()
				block.position = _tile_coords_to_vector_pos(tile) + Vector2(40, 24)
				layer_ground.add_child(block)

			Vector2i(12, 30): # Hanging grass
				var grass:Grass = _load(&"res://Scenes/Entities/Grass.tscn").instantiate()
				grass.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(grass)
				grass.spawn(Grass.GrassTypes.NORMAL, Statics.DirsSurface.CEILING)

			Vector2i(13, 30): # Hanging power grass
				var grass:Grass = _load(&"res://Scenes/Entities/Grass.tscn").instantiate()
				grass.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(grass)
				grass.spawn(Grass.GrassTypes.POWER, Statics.DirsSurface.CEILING)

			Vector2i(2, 31): # Peashooter breakable
				var pea_tile:Breakable = breakable_scene.instantiate()
				pea_tile.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(pea_tile)
				layer_ground.move_child(pea_tile, 1)
				pea_tile.spawn(tile, Breakable.TileTypes.PEASHOOTER, false)

			Vector2i(3, 31): # Water surface effect tile
				var surface_tile:JsonSprite2D = _load(&"res://Scenes/Environments/WaterSurfaceTile.tscn").instantiate()
				surface_tile.position = _tile_coords_to_vector_pos(tile) + Vector2(0, -1)
				layer_fg2.add_child(surface_tile)

			Vector2i(4, 31): # Walleye (up right)
				var walleye:Walleye = _load(&"res://Scenes/Entities/Enemies/Walleye.tscn").instantiate()
				walleye.position = _tile_coords_to_vector_pos(tile)
				walleye.direction = 1
				layer_ground.add_child(walleye)

			Vector2i(5, 31): # Walleye (up)
				var walleye:Walleye = _load(&"res://Scenes/Entities/Enemies/Walleye.tscn").instantiate()
				walleye.position = _tile_coords_to_vector_pos(tile)
				walleye.direction = 0
				layer_ground.add_child(walleye)

			Vector2i(6, 31): # Walleye (up left)
				var walleye:Walleye = _load(&"res://Scenes/Entities/Enemies/Walleye.tscn").instantiate()
				walleye.position = _tile_coords_to_vector_pos(tile)
				walleye.direction = 7
				layer_ground.add_child(walleye)

			Vector2i(7, 31): # Walleye (down right)
				var walleye:Walleye = _load(&"res://Scenes/Entities/Enemies/Walleye.tscn").instantiate()
				walleye.position = _tile_coords_to_vector_pos(tile)
				walleye.direction = 3
				layer_ground.add_child(walleye)

			Vector2i(8, 31): # Walleye (down)
				var walleye:Walleye = _load(&"res://Scenes/Entities/Enemies/Walleye.tscn").instantiate()
				walleye.position = _tile_coords_to_vector_pos(tile)
				walleye.direction = 4
				layer_ground.add_child(walleye)

			Vector2i(9, 31): # Walleye (down left)
				var walleye:Walleye = _load(&"res://Scenes/Entities/Enemies/Walleye.tscn").instantiate()
				walleye.position = _tile_coords_to_vector_pos(tile)
				walleye.direction = 5
				layer_ground.add_child(walleye)

			Vector2i(12, 70): # Fire
				var fire:Fire = _load(&"res://Scenes/Entities/Hazards/Fire.tscn").instantiate()
				fire.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(fire)

			Vector2i(13, 70): # Fire
				var fire:Fire = _load(&"res://Scenes/Entities/Hazards/Fire.tscn").instantiate()
				fire.position = _tile_coords_to_vector_pos(tile)
				layer_ground.add_child(fire)

		tpf.append(Time.get_ticks_msec() - ts)
	print_verbose(tpf)
	if spawned_sp >= cells.size():
		cells.clear()
		spawned_sp = 0
		if layer == 0:
			spawned_all = true
		elif layer == 1:
			spawned_secondary = true


func _load(path:StringName) -> PackedScene:
	if not sp_cache.has(path):
		sp_cache[path] = load(path)
		print_verbose("cache miss: ", path)
	return sp_cache[path]


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
										map_entity1.set_cell(map_index - tiled_corner, source, tile_coords)


#region Runtime functions
func open_all_boss_doors() -> void:
	var children:Array = Statics.get_all_children(self)
	for child in children:
		if child is Door:
			var boss_locked:bool = (
				(child.lock_type == Door.LockTypes.LOCKED_BY_BOSS
				or child.lock_type == Door.LockTypes.LOCKED_BY_BOSS_IN_RANDOMIZER)
			)
			if boss_locked:
				match child.required_boss:
					0:
						if Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS1) == true:
							child.open()
					1:
						if Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS2) == true:
							child.open()
					2:
						if Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS3) == true:
							child.open()
					3:
						if Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS4) == true:
							child.open()


func set_environment_visibility() -> void:
	for child in get_children():
		if child is EnvironmentArea:
			child.update_shader_visibility()


func despawn_room() -> void:
	despawn.emit()
	queue_free()


func fade_layer(_layer:Layers, _color:Color, _time:float) -> void:
	var tween:Tween = create_tween()
	var layer:Node2D = layer_ground
	match _layer:
		Layers.SKY: layer = layer_sky
		Layers.BG2: layer = layer_bg2
		Layers.BG1: layer = layer_bg1
		Layers.FG1: layer = layer_fg1
		Layers.FG2: layer = layer_fg2
		Layers.ENTITY: layer = layer_entity
	tween.tween_property(layer, "modulate", _color, _time)


func fade_all_layers(_color:Color, _time:float) -> void:
	fade_layer(Layers.SKY, _color, _time)
	fade_layer(Layers.BG2, _color, _time)
	fade_layer(Layers.BG1, _color, _time)
	fade_layer(Layers.GROUND, _color, _time)
	fade_layer(Layers.FG1, _color, _time)
	fade_layer(Layers.FG2, _color, _time)
	fade_layer(Layers.ENTITY, _color, _time)


func fade_map(_layer:Layers, _color:Color, _time:float) -> void:
	var tween:Tween = create_tween()
	var layer:Node2D = map_ground
	match _layer:
		Layers.SKY: layer = map_sky
		Layers.BG2: layer = map_bg2
		Layers.BG1: layer = map_bg1
		Layers.FG1: layer = map_fg1
		Layers.FG2: layer = map_fg2
		Layers.ENTITY: layer = map_entity1
	tween.tween_property(layer, "modulate", _color, _time)
	


func fade_all_maps(_color:Color, _time:float) -> void:
	fade_map(Layers.SKY, _color, _time)
	fade_map(Layers.BG2, _color, _time)
	fade_map(Layers.BG1, _color, _time)
	fade_map(Layers.GROUND, _color, _time)
	fade_map(Layers.FG1, _color, _time)
	fade_map(Layers.FG2, _color, _time)
	fade_map(Layers.ENTITY, _color, _time)


func set_map_visible(_layer:Layers, _visible:bool) -> void:
	var layer:Node2D = map_ground
	match _layer:
		Layers.SKY: layer = map_sky
		Layers.BG2: layer = map_bg2
		Layers.BG1: layer = map_bg1
		Layers.FG1: layer = map_fg1
		Layers.FG2: layer = map_fg2
		Layers.ENTITY: layer = map_entity1
	layer.visible = _visible
	if _layer == Layers.ENTITY:
		map_entity2.visible = _visible


func set_all_maps_visible(_visible:bool) -> void:
	set_map_visible(Layers.SKY, _visible)
	set_map_visible(Layers.BG2, _visible)
	set_map_visible(Layers.BG1, _visible)
	set_map_visible(Layers.GROUND, _visible)
	set_map_visible(Layers.FG1, _visible)
	set_map_visible(Layers.FG2, _visible)
	set_map_visible(Layers.ENTITY, _visible)


func set_ground_collision(_enabled:bool) -> void:
	map_ground.collision_enabled = _enabled
	collision_enabled = _enabled
#endregion


func start_cutscene(initiator:CutsceneControllable) -> void:
	CutsceneController.start(cutscene_script, cutscene_animator, initiator.identifier)
