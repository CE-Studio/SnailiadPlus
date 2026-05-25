# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
@icon("res://Editor/ico/Minimap.svg")
class_name Minimap
extends Node2D


#region Variables
enum CellTypes {
	UNEXPLORED = 0,
	EXPLORED = 1,
	SECRET_UNEXPLORED = 2,
	SECRET_EXPLORED = 3,
	EMPTY = -1
}
enum MarkerTypes {
	NONE = -2,
	ITEM_COLLECTED = -1,
	SAVE,
	BOSS,
	ITEM,
	UNKNOWN,
	P_MARKER
}

const MAP_SIZE:Vector2i = Vector2i(26, 22)
const PANEL_SIZE:Vector2i = Vector2i(7, 5)
const EDGE_BUFFER:Vector2i = Vector2i(3, 2)
const SCREEN_SIZE:Vector2 = Vector2(26.0, 16.0)
const MAP_LAYER_MAX_BOUNDS:Vector2i = Vector2i(76, 68)
const TL_OFFSET:Vector2i = Vector2i(100, 84)
const MARKER_ZERO:Vector2i = Vector2i(-100, -84)
const ROOM_NAME_STRING:String = "room_%s"
const FADE_SPEED:float = 3.5
#const SUBSCREEN_MOVE_SPEED:float = 16.0
#const SUBSCREEN_MOD_TOLERANCE:float = Statics.FRAC_64
const P_MARKER_ID_OFFSET:int = 100

const DEFAULT_MAP:Array = [
#	 0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19  20  21  22  23  24  25
	-1,  0,  0, -1, -1,  2,  0, -1, -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, #   0   0
	-1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, #  26   1
	-1, -1,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, #  52   2
	-1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, #  78   3
	 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  2,  2,  0,  0,  0,  0,  0, -1, -1, -1, -1, # 104   4
	 0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  2,  2,  2,  0,  0,  0,  0,  0, -1, -1, -1, -1, # 130   5
	-1,  0,  0,  0,  0,  0,  0,  0, -1,  0,  2,  2,  2,  0,  2,  2,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, # 156   6
	-1,  0,  0,  0,  0,  0,  0, -1, -1,  0,  0,  0,  0,  0, -1,  0, -1,  0, -1, -1, -1, -1, -1, -1, -1, -1, # 182   7
	 0,  0,  0,  0,  0,  0,  0, -1,  2,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1, -1, -1, # 208   8
	 0,  0,  0,  0,  0,  0,  0,  2,  2,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1, -1, -1, # 234   9
	-1,  0,  0,  0,  0,  0,  2,  2,  2,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1, -1, -1, # 260  10
	-1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1, # 286  11
	-1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1,  0, -1, -1, -1, -1, -1, -1, # 312  12
	-1,  0,  2,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1,  0, -1, -1, -1, -1, -1, -1, # 338  13
	-1,  0,  0,  0, -1, -1, -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1, # 364  14
	 0,  0,  0,  0, -1, -1, -1, -1,  0,  0,  0,  0, -1, -1, -1, -1,  0,  0,  2,  0, -1, -1, -1, -1, -1, -1, # 390  15
	 0, -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1, # 416  16
	 0,  0, -1,  0, -1, -1, -1, -1, -1, -1, -1,  0, -1,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1, # 442  17
	 0, -1, -1,  0, -1, -1, -1, -1, -1, -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1, # 468  18
	 0, -1, -1,  0,  0,  0, -1, -1, -1, -1, -1, -1, -1, -1, -1,  0, -1, -1, -1,  0,  0, -1, -1, -1, -1, -1, # 494  19
	 0, -1,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  0, -1, -1, -1, -1, -1, # 520  20
	 0,  0,  0,  0,  0,  0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  0,  0, -1, -1, -1, -1, -1  # 546  21
	]

var mask:Image
var mask_texture:ImageTexture
var last_map_center:Vector2i = Vector2i(-1, -1)
var last_player_pos:Vector2i = Vector2i(-1, -1)
var last_drawn_cells:Array = []
var fade_override:float = 1.0
var update_player_flag:bool = false

var room_offset:Vector2i = Vector2i.ZERO

var active_markers:Array[MapMarker] = []
var organized_markers:Dictionary = {
	"saves": [],
	"bosses": [],
	"items": [],
	"unknowns": [],
	"p_markers": []
}
static var unprocessed_marker_positions:Array = [] # Set up in Preloader.gd
static var empty_locations:Array[int] = [] # Set up in Preloader.gd
static var hide_empty_locations:bool = true
var marker_positions:Array = []
var player_marker_sprites:Array = []
@export var subscreen_mode:bool = false
#var subscreen_target:Vector2 = Vector2.ZERO
#var subscreen_target_mod:float = 0.0
#var edge_extension:int = 0

@onready var panel:JsonSprite2D = $"Panel"
@onready var panel_mask:Sprite2D = $"PanelMask"
@onready var map_group:Node2D = $"MapGroup"
@onready var cell_mask:Sprite2D = $"MapGroup/CellMask"
@onready var map:JsonSprite2D = $"MapGroup/CellMask/Map"
@onready var player_marker:JsonSprite2D = $"MapGroup/PlayerMarker"
@onready var marker_group:Node2D = $"MapGroup/MarkerGroup"
@onready var p_marker_group:Node2D = $"MapGroup/PlayerMarkerGroup"
@onready var marker_scene:PackedScene = preload("res://Scenes/UI/MapMarker.tscn")
@onready var name_text:SnailyText = $"SnailyText"
#endregion


func _ready() -> void:
	if Statics.current_profile["map_tiles"].size() == 0:
		Statics.current_profile["map_tiles"] = DEFAULT_MAP.duplicate()
	panel.action = "idle"
	map.action = "minimap"
	player_marker.action = "player_normal"
	create_cell_mask()
	log_markers()
	update_p_marker_layer()
	if subscreen_mode:
		map.action = "subscreen"
		panel.queue_free()
		name_text.queue_free()
		room_offset = UICore.instance.minimap.room_offset
		tick_minimap(1, true, true)


func update_visible_from_settings(target_fade:float = 1.0, quick_fade:bool = false) -> void:
	var mode = ProjectSettings.get_setting("game/ui/minimap")
	update_visible(mode, target_fade, quick_fade)


func update_visible(mode:int, target_fade:float = 1.0, quick_fade:bool = false) -> void:
	match mode:
		0:
			visible = false
		1:
			visible = true
			name_text.visible = false
		2:
			visible = true
			name_text.visible = true
	fade_override = target_fade
	if quick_fade:
		modulate.a = target_fade


func create_cell_mask() -> void:
	mask = Image.create_empty(MAP_SIZE.x, MAP_SIZE.y, false, Image.FORMAT_RGBA8)
	mask_texture = ImageTexture.create_from_image(mask)
	cell_mask.texture = mask_texture


func log_markers() -> void:
	marker_positions = unprocessed_marker_positions.duplicate()
	var screen_pos:Vector2i = Vector2i.ZERO
	for i in range(marker_positions.size()):
		if ((marker_positions[i] is Array and marker_positions[i][0] > MarkerTypes.NONE)
		or marker_positions[i] > MarkerTypes.NONE):
			var new_marker:MapMarker = marker_scene.instantiate()
			marker_group.add_child(new_marker)
			new_marker.position = screen_pos * 8
			if marker_positions[i] is Array:
				match marker_positions[i][0]:
					MarkerTypes.ITEM:
						new_marker.type = MarkerTypes.ITEM
						new_marker.data.append(marker_positions[i][1])
						new_marker.sprite.action = "item_normal"
						organized_markers["items"].append(new_marker)
			else:
				match marker_positions[i]:
					MarkerTypes.SAVE:
						new_marker.type = MarkerTypes.SAVE
						new_marker.sprite.action = "save"
						organized_markers["saves"].append(new_marker)
					MarkerTypes.BOSS:
						new_marker.type = MarkerTypes.BOSS
						new_marker.sprite.action = "boss"
						organized_markers["bosses"].append(new_marker)
					MarkerTypes.UNKNOWN:
						new_marker.type = MarkerTypes.UNKNOWN
						new_marker.sprite.action = "unknown"
						organized_markers["unknowns"].append(new_marker)
			marker_positions[i] = active_markers.size()
			active_markers.append(new_marker)
		else:
			marker_positions[i] = -1
		screen_pos.x += 1
		if screen_pos.x >= MAP_SIZE.x:
			screen_pos = Vector2i(0, screen_pos.y + 1)


func update_p_marker_at_cell(cell:Vector2i) -> void:
	var index = (cell.y * MAP_SIZE.x) + cell.x
	if Statics.current_profile["map_tiles"][index] < P_MARKER_ID_OFFSET - 1:
		Statics.current_profile["map_tiles"][index] += P_MARKER_ID_OFFSET
	else:
		Statics.current_profile["map_tiles"][index] -= P_MARKER_ID_OFFSET
	update_p_marker_layer()
	if cell == last_player_pos:
		update_player()
		tick_minimap(0)


func update_p_marker_layer() -> void:
	while player_marker_sprites.size() < DEFAULT_MAP.size():
		player_marker_sprites.append(null)
	var cur_map:Array = Statics.current_profile["map_tiles"]
	var cell_coords:Vector2i = Vector2i.ZERO
	for i in cur_map.size():
		if cur_map[i] >= P_MARKER_ID_OFFSET - 1 and player_marker_sprites[i] == null:
			var new_marker:MapMarker = marker_scene.instantiate()
			p_marker_group.add_child(new_marker)
			new_marker.position = cell_coords * 8
			new_marker.type = MarkerTypes.P_MARKER
			new_marker.sprite.action = "marker"
			organized_markers["p_markers"].append(new_marker)
			player_marker_sprites[i] = new_marker
		elif cur_map[i] < P_MARKER_ID_OFFSET - 1 and player_marker_sprites[i] != null:
			var spr = player_marker_sprites[i]
			organized_markers["p_markers"].remove_at(organized_markers["p_markers"].find(spr))
			spr.queue_free()

		cell_coords.x += 1
		if cell_coords.x >= MAP_SIZE.x:
			cell_coords.x = 0
			cell_coords.y += 1


static func world_position_to_screen_coordinate(_position:Vector2) -> Vector2i:
	return Vector2i(
		floori((_position.x) / SCREEN_SIZE.x * Statics.FRAC_16),
		floori((_position.y) / SCREEN_SIZE.y * Statics.FRAC_16)
	)


func _process(delta: float) -> void:
	if not subscreen_mode:
		tick_minimap(2)

	#if subscreen_mode and subscreen_target_mod < 1.0:
	#	subscreen_target_mod = lerpf(subscreen_target_mod, 1.0, SUBSCREEN_MOVE_SPEED * delta)
	#	if 1.0 - subscreen_target_mod < SUBSCREEN_MOD_TOLERANCE:
	#		subscreen_target_mod = 1.0
	#elif not subscreen_mode and subscreen_target_mod > 0.0:
	#	subscreen_target_mod = lerpf(subscreen_target_mod, 0.0, SUBSCREEN_MOVE_SPEED * delta)
	#	if subscreen_target_mod < SUBSCREEN_MOD_TOLERANCE:
	#		subscreen_target_mod = 0.0
	#
	#if subscreen_target_mod > 0.0:
	#	subscreen_target = UICore.instance.pause_layer.subscreen.map_target.global_position
	#	subscreen_target -= UICore.instance.pause_layer.subscreen.position
	#	map_group.global_position = map_group.global_position.lerp(subscreen_target, subscreen_target_mod)
	#	if subscreen_mode and subscreen_target_mod == 1.0:
	#		if edge_extension < min(MAP_SIZE.x, MAP_SIZE.y):
	#			edge_extension += 1
	#			tick_minimap(false, false, true)
	#if not subscreen_mode and edge_extension > 0:
	#	edge_extension = clampi(edge_extension - 2, 0, 999)
	#	tick_minimap(false, false, true)

	var fade_d:float = delta * FADE_SPEED
	if modulate.a != fade_override and not subscreen_mode:
		modulate.a = move_toward(modulate.a, fade_override, fade_d)
	#var subscreen_target_a = 0.0 if subscreen_mode else 1.0
	#panel.modulate.a = move_toward(panel.modulate.a, subscreen_target_a, fade_d)
	#name_text.modulate.a = move_toward(name_text.modulate.a, subscreen_target_a, fade_d)


func tick_minimap(move_group_mode:int, tick_player:bool = true, force_update:bool = false) -> void:
	var player = GameCore.instance.player
	# Converts the player position into coordinates on the "screen grid"
	var converted_player_pos = world_position_to_screen_coordinate(player.position + Vector2(8, 8))
	if move_group_mode > 0:
		if move_group_mode > 1:
			map_group.position = (converted_player_pos * -8) + TL_OFFSET + (room_offset * -8)
			map_group.position = Vector2(
				int(clampf(map_group.position.x, -MAP_LAYER_MAX_BOUNDS.x, MAP_LAYER_MAX_BOUNDS.x)),
				int(clampf(map_group.position.y, -MAP_LAYER_MAX_BOUNDS.y, MAP_LAYER_MAX_BOUNDS.y))
			)
		player_marker.position = MARKER_ZERO + (converted_player_pos * 8) + (room_offset * 8)

	var update_map:bool = false

	var map_local_center = Vector2i(
		clampi(converted_player_pos.x + room_offset.x, EDGE_BUFFER.x, MAP_SIZE.x - EDGE_BUFFER.x - 1),
		clampi(converted_player_pos.y + room_offset.y, EDGE_BUFFER.y, MAP_SIZE.y - EDGE_BUFFER.y - 1)
	)
	if last_map_center != map_local_center and not subscreen_mode:
		update_map = true
		last_map_center = map_local_center

	if tick_player:
		var player_cell = Vector2i(converted_player_pos + room_offset)
		if player_cell != last_player_pos or update_player_flag:
			update_player_flag = false
			var array_pos = vector_to_array_index(player_cell)
			#var cell = Statics.current_profile["map_tiles"][array_pos]
			#var p_marked_cell:bool = false
			#if cell >= P_MARKER_ID_OFFSET - 1:
			#	cell -= P_MARKER_ID_OFFSET
			#	p_marked_cell = true
			#if cell == CellTypes.UNEXPLORED:
			#	Statics.current_profile["map_tiles"][array_pos] = CellTypes.EXPLORED
			#if cell == CellTypes.SECRET_UNEXPLORED:
			#	Statics.current_profile["map_tiles"][array_pos] = CellTypes.SECRET_EXPLORED
			var cell = fill_cell(player_cell)
			var p_marked_cell = cell > P_MARKER_ID_OFFSET - 1
			if not subscreen_mode:
				update_cell_mask(map_local_center)
				update_markers(last_drawn_cells)
			update_map = false
			last_player_pos = player_cell

			if marker_positions.size() > 0:
				var highlight:bool = false

				if p_marked_cell:
					highlight = true
				elif marker_positions[array_pos] >= 0:
					var marker = active_markers[marker_positions[array_pos]]
					if (marker.type != MarkerTypes.ITEM
					or (marker.type == MarkerTypes.ITEM and not Statics.check_location_collected(marker.data[0]) and
					(not empty_locations.has(marker.data[0]) or not hide_empty_locations))):
						highlight = true

				if highlight and player_marker.action == "player_normal":
					player_marker.action = "player_highlight"
				elif not highlight and player_marker.action == "player_highlight":
					player_marker.action = "player_normal"

	if update_map or force_update:
		if subscreen_mode:
			update_cell_mask(Vector2i.ZERO, MAP_SIZE)
		else:
			update_cell_mask(map_local_center, EDGE_BUFFER)
		update_markers(last_drawn_cells)


func update_cell_mask(center:Vector2i, extents:Vector2i = EDGE_BUFFER) -> void:
	mask.fill(Color(1.0, 1.0, 1.0, 0.0))
	var new_marker_cells:Array = []
	for y in range(center.y - extents.y, center.y + extents.y + 1):
		for x in range(center.x - extents.x, center.x + extents.x + 1):
			if x >= 0 and x < MAP_SIZE.x and y >= 0 and y < MAP_SIZE.y:
				var cell = Statics.current_profile["map_tiles"][x + (y * MAP_SIZE.x)]
				if cell >= P_MARKER_ID_OFFSET - 1:
					cell -= P_MARKER_ID_OFFSET
				if (cell == CellTypes.EXPLORED or
				(cell == CellTypes.SECRET_EXPLORED and ProjectSettings.get_setting("game/ui/secret_map_tiles"))):
					mask.set_pixel(x, y, Color.WHITE)
				new_marker_cells.append(x + (y * MAP_SIZE.x))
	mask_texture.update(mask)
	last_drawn_cells = new_marker_cells


func vector_to_array_index(coords:Vector2) -> int:
	return int(coords.x + (coords.y * MAP_SIZE.x))


func fill_cell(coords:Vector2i) -> int:
	var cell_index:int = vector_to_array_index(coords)
	var true_cell:int = Statics.current_profile["map_tiles"][cell_index]
	var cell:int = true_cell
	var player_marked:bool = false
	if cell >= P_MARKER_ID_OFFSET - 1:
		cell -= P_MARKER_ID_OFFSET
		player_marked = true
	
	if cell == CellTypes.UNEXPLORED:
		Statics.current_profile["map_tiles"][cell_index] = CellTypes.EXPLORED
		if player_marked:
			Statics.current_profile["map_tiles"][cell_index] += P_MARKER_ID_OFFSET
	if cell == CellTypes.SECRET_UNEXPLORED:
		Statics.current_profile["map_tiles"][cell_index] = CellTypes.SECRET_EXPLORED
		if player_marked:
			Statics.current_profile["map_tiles"][cell_index] += P_MARKER_ID_OFFSET
	
	if get_map_rate() >= 100.0:
		if not Statics.add_achievement(AchievementCore.Achievements.MAP_100):
			Statics.add_unlock_condition(Statics.Unlocks.OPEN_MAP)
	
	return true_cell


func update_markers(target_cells:Array = []) -> void:
	if target_cells.is_empty():
		for i in range(marker_positions.size()):
			target_cells.append(i)

	for i in range(marker_positions.size()):
		var array_i = marker_positions[i]
		var tile = Statics.current_profile["map_tiles"][i]
		var player_tile:bool = false
		if tile >= P_MARKER_ID_OFFSET - 1:
			tile -= P_MARKER_ID_OFFSET
			player_tile = true
		var tile_visible:bool = (tile == CellTypes.EXPLORED
			or (ProjectSettings.get_setting("game/ui/secret_map_tiles")
			and tile == CellTypes.SECRET_EXPLORED))

		if array_i >= 0:
			var cell_pos = Vector2i.ZERO
			var working_i = i
			while working_i >= MAP_SIZE.x:
				cell_pos.y += 1
				working_i -= MAP_SIZE.x
			cell_pos.x = working_i
			var marker = active_markers[array_i]

			marker.modulate.a = 0.0
			if target_cells.has(i) and tile_visible:
				marker.modulate.a = 1.0

			if marker.type == MarkerTypes.ITEM:
				marker.visible = not empty_locations.has(marker.data[0]) or not hide_empty_locations
				var collected:bool = Statics.check_location_collected(marker.data[0])
				if marker.sprite.action == "item_normal" and collected:
					marker.sprite.action = "item_collected"
				elif marker.sprite.action == "item_collected" and not collected:
					marker.sprite.action = "item_normal"

		if player_marker_sprites[i]:
			player_marker_sprites[i].modulate.a = 0.0
			if target_cells.has(i) and player_tile:
				player_marker_sprites[i].modulate.a = 1.0


func set_room_name(_name:String):
	if not subscreen_mode:
		if GlobalText.room_names.keys().has(_name):
			name_text.set_snaily_text(GlobalText.room_names[_name])


func update_player() -> void:
	update_player_flag = true


static func get_map_rate() -> float:
	var max_cell_count:int = DEFAULT_MAP.count(CellTypes.UNEXPLORED)
	var cur_map:Array = Statics.current_profile["map_tiles"].duplicate()
	for i in cur_map.size():
		cur_map[i] = cur_map[i] as int
	var cur_cell_count:int = cur_map.count(CellTypes.EXPLORED)
	cur_cell_count += cur_map.count(CellTypes.EXPLORED + P_MARKER_ID_OFFSET)
	var output:float = (float(cur_cell_count) / float(max_cell_count)) * 100.0
	return output
