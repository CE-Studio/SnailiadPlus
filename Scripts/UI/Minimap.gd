@icon("res://Editor/ico/Minimap.svg")
class_name Minimap
extends Node2D


#region Variables
enum CellTypes {
	UNEXPLORED,
	EXPLORED,
	SECRET_UNEXPLORED,
	SECRET_EXPLORED,
	EMPTY = -1
}
enum MarkerTypes {
	NONE = -2,
	ITEM_COLLECTED = -1,
	SAVE,
	BOSS,
	ITEM_UNCOLLECTED,
	UNKNOWN,
	P_MARKER
}

const MAP_SIZE:Vector2i = Vector2i(26, 22)
const PANEL_SIZE:Vector2i = Vector2i(7, 5)
const SCREEN_SIZE:Vector2 = Vector2(26.0, 16.0)
const MAP_LAYER_MAX_BOUNDS:Vector2i = Vector2i(76, 68)
const TL_OFFSET:Vector2 = Vector2(100, 84)
const MARKER_ZERO:Vector2 = Vector2(-100, -84)

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

var room_offset:Vector2 = Vector2.ZERO

var marker_data:Array = []

@onready var panel:JsonSprite2D = $"Panel"
@onready var panel_mask:Sprite2D = $"PanelMask"
@onready var map_group:Sprite2D = $"PanelMask/CellMask"
@onready var map:JsonSprite2D = $"PanelMask/CellMask/Map"
@onready var player_marker:JsonSprite2D = $"PanelMask/CellMask/PlayerMarker"
@onready var marker_group:Node2D = $"PanelMask/CellMask/MarkerGroup"
#endregion


func _ready() -> void:
	panel.action = "idle"
	map.action = "idle"
	player_marker.action = "player_normal"
	#map.region_rect = Rect2i(0, 0, PANEL_SIZE.x * 8, PANEL_SIZE.y * 8)
	create_cell_mask()


func create_cell_mask() -> void:
	mask = Image.create_empty(MAP_SIZE.x, MAP_SIZE.y, false, Image.FORMAT_RGBA8)
	for y in range(MAP_SIZE.y):
		for x in range(MAP_SIZE.x):
			if DEFAULT_MAP[x + (y * MAP_SIZE.x)] > -1:
				mask.set_pixel(x, y, Color.WHITE)
			else:
				mask.set_pixel(x, y, Color(1.0, 1.0, 1.0, 0.0))
	map_group.texture = ImageTexture.create_from_image(mask)


func _process(delta: float) -> void:
	var player = GameCore.instance.player
	var converted_player_pos = Vector2( # Converts the player position into coordinates on the "screen grid"
		floori((player.position.x - 0.5) / SCREEN_SIZE.x * Statics.FRAC_16),
		floori((player.position.y - 0.5) / SCREEN_SIZE.y * Statics.FRAC_16)
	)
	map_group.position = (converted_player_pos * -8) + TL_OFFSET + (room_offset * -8)
	map_group.position = Vector2(
		clampi(map_group.position.x, -MAP_LAYER_MAX_BOUNDS.x, MAP_LAYER_MAX_BOUNDS.x),
		clampi(map_group.position.y, -MAP_LAYER_MAX_BOUNDS.y, MAP_LAYER_MAX_BOUNDS.y)
	)
	#map.position = -map_group.position
	player_marker.position = MARKER_ZERO + (converted_player_pos * 8) + (room_offset * 8) #TODO configure cell mask to double as border mask
