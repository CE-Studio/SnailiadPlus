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
const ROOM_NAME_STRING = "room_%s"

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

var room_offset:Vector2i = Vector2i.ZERO

var active_markers:Array[MapMarker] = []
var organized_markers:Dictionary = {
	"saves": [],
	"bosses": [],
	"items": [],
	"unknowns": [],
	"p_markers": []
}
static var marker_positions:Array = []

@onready var panel:JsonSprite2D = $"Panel"
@onready var panel_mask:Sprite2D = $"PanelMask"
@onready var map_group:Node2D = $"MapGroup"
@onready var cell_mask:Sprite2D = $"MapGroup/CellMask"
@onready var map:JsonSprite2D = $"MapGroup/CellMask/Map"
@onready var player_marker:JsonSprite2D = $"MapGroup/PlayerMarker"
@onready var marker_group:Node2D = $"MapGroup/MarkerGroup"
@onready var marker_scene:PackedScene = preload("res://Scenes/UI/MapMarker.tscn")
@onready var name_text:SnailyText = $"SnailyText"
#endregion


func _ready() -> void:
	if Statics.current_profile["map_tiles"].size() == 0:
		Statics.current_profile["map_tiles"] = DEFAULT_MAP.duplicate()
	panel.action = "idle"
	map.action = "idle"
	player_marker.action = "player_normal"
	marker_group.position
	create_cell_mask()
	log_markers()


func create_cell_mask() -> void:
	mask = Image.create_empty(MAP_SIZE.x, MAP_SIZE.y, false, Image.FORMAT_RGBA8)
	mask_texture = ImageTexture.create_from_image(mask)
	cell_mask.texture = mask_texture


func log_markers() -> void:
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


static func world_position_to_screen_coordinate(position:Vector2) -> Vector2i:
	return Vector2i(
		floori((position.x) / SCREEN_SIZE.x * Statics.FRAC_16),
		floori((position.y) / SCREEN_SIZE.y * Statics.FRAC_16)
	)


func _process(delta: float) -> void:
	var player = GameCore.instance.player
	# Converts the player position into coordinates on the "screen grid"
	var converted_player_pos = world_position_to_screen_coordinate(player.position + Vector2(8, 8))
	map_group.position = (converted_player_pos * -8) + TL_OFFSET + (room_offset * -8)
	map_group.position = Vector2(
		clampi(map_group.position.x, -MAP_LAYER_MAX_BOUNDS.x, MAP_LAYER_MAX_BOUNDS.x),
		clampi(map_group.position.y, -MAP_LAYER_MAX_BOUNDS.y, MAP_LAYER_MAX_BOUNDS.y)
	)
	player_marker.position = MARKER_ZERO + (converted_player_pos * 8) + (room_offset * 8)
	
	var update_map:bool = false
	
	var map_local_center = Vector2i(
		clampi(converted_player_pos.x + room_offset.x, EDGE_BUFFER.x, MAP_SIZE.x - EDGE_BUFFER.x),
		clampi(converted_player_pos.y + room_offset.y, EDGE_BUFFER.y, MAP_SIZE.y - EDGE_BUFFER.y)
	)
	if last_map_center != map_local_center:
		update_map = true
		last_map_center = map_local_center
		
	var player_cell = Vector2i(converted_player_pos + room_offset)
	if player_cell != last_player_pos:
		var array_pos = vector_to_array_index(player_cell)
		var cell = Statics.current_profile["map_tiles"][array_pos]
		if cell == CellTypes.UNEXPLORED:
			Statics.current_profile["map_tiles"][array_pos] = CellTypes.EXPLORED
		if cell == CellTypes.SECRET_UNEXPLORED:
			Statics.current_profile["map_tiles"][array_pos] = CellTypes.SECRET_EXPLORED
		update_map = true
		last_player_pos = player_cell
		
		if marker_positions[array_pos] >= 0 and player_marker.action == "player_normal":
			var marker = active_markers[marker_positions[array_pos]]
			if (marker.type != MarkerTypes.ITEM
			or (marker.type == MarkerTypes.ITEM and not Statics.check_location_collected(marker.data[0]))):
				player_marker.action = "player_highlight"
			elif player_marker.action == "player_highlight":
				player_marker.action = "player_normal"
		elif marker_positions[array_pos] < 0 and player_marker.action == "player_highlight":
			player_marker.action = "player_normal"
		
	if update_map:
		update_cell_mask(map_local_center)
		update_markers(last_drawn_cells)


func update_cell_mask(center:Vector2i, extents:Vector2i = EDGE_BUFFER) -> void:
	mask.fill(Color(1.0, 1.0, 1.0, 0.0))
	var new_marker_cells:Array = []
	for y in range(center.y - extents.y, center.y + extents.y + 1):
		for x in range(center.x - extents.x, center.x + extents.x + 1):
			if x >= 0 and x < MAP_SIZE.x and y >= 0 and y < MAP_SIZE.y:
				var cell = Statics.current_profile["map_tiles"][x + (y * MAP_SIZE.x)]
				if (cell == CellTypes.EXPLORED or
				(cell == CellTypes.SECRET_EXPLORED and Statics.data_general["secret_tile_toggle"])):
					mask.set_pixel(x, y, Color.WHITE)
				new_marker_cells.append(x + (y * MAP_SIZE.x))
	mask_texture.update(mask)
	last_drawn_cells = new_marker_cells


func vector_to_array_index(coords:Vector2) -> int:
	return coords.x + (coords.y * MAP_SIZE.x)


func fill_cell(coords:Vector2i) -> void:
	var cell_index = vector_to_array_index(coords)
	var cell = Statics.current_profile["map_tiles"][cell_index]
	if cell == CellTypes.UNEXPLORED:
		Statics.current_profile["map_tiles"][cell_index] = CellTypes.EXPLORED
	if cell == CellTypes.SECRET_UNEXPLORED:
		Statics.current_profile["map_tiles"][cell_index] = CellTypes.SECRET_EXPLORED


func update_markers(target_cells:Array = []) -> void:
	if target_cells.is_empty():
		for i in range(marker_positions.size()):
			target_cells.append(i)
	for i in range(marker_positions.size()):
		var cell_pos = Vector2i.ZERO
		var working_i = i
		while working_i >= MAP_SIZE.x:
			cell_pos.y += 1
			working_i -= MAP_SIZE.x
		cell_pos.x = working_i
		var array_i = marker_positions[i]
		if array_i != MarkerTypes.NONE:
			var marker = active_markers[array_i]
			var tile = Statics.current_profile["map_tiles"][i]
			
			marker.visible = false
			if target_cells.has(i):
				if (tile == CellTypes.EXPLORED
				or (Statics.data_general["secret_tile_toggle"] and tile == CellTypes.SECRET_EXPLORED)):
					marker.visible = true
				
			if marker.type == MarkerTypes.ITEM:
				var collected:bool = Statics.check_location_collected(marker.data[0])
				if marker.sprite.action == "item_normal" and collected:
					marker.sprite.action = "item_collected"
				elif marker.sprite.action == "item_collected" and not collected:
					marker.sprite.action = "item_normal"


func set_room_name(_name:String):
	name_text.set_snaily_text(Statics.get_text(ROOM_NAME_STRING % _name))
