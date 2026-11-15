# Copyright 2026 CE-Studio: AGPL-3.0-only
@tool
@icon("res://Editor/ico/Minimap.svg")
class_name MapMarker
extends Node2D


#region Variables
const COL_CYCLE_TIME:float = 0.1

var cycle_col:float = 0.0
var ptr_col:int = 0
var p_col:Color = Color.TRANSPARENT
var player_should_highlight:bool = false
@export var type:Minimap.MarkerTypes = Minimap.MarkerTypes.SAVE
@export var data:Array = []
@export var item_location:int = -1
@export var save_location:int = -1
@export var boss_id:int = -1
@export var player_col:Color = Color.BLACK

@onready var cell_pos:Vector2i = Vector2i(position)
@onready var sprite:SnailySprite2D = $"SnailySprite2D"
#endregion


func _ready() -> void:
	match type:
		Minimap.MarkerTypes.SAVE: sprite.play("save")
		Minimap.MarkerTypes.ITEM: sprite.play("item_normal")
		Minimap.MarkerTypes.ITEM_COLLECTED: sprite.play("item_collected")
		Minimap.MarkerTypes.BOSS: sprite.play("boss")
		Minimap.MarkerTypes.UNKNOWN: sprite.play("unknown")
		Minimap.MarkerTypes.P_MARKER: sprite.play("marker")


func _process(delta: float) -> void:
	if type == Minimap.MarkerTypes.P_MARKER:
		cycle_col += delta
		while cycle_col >= COL_CYCLE_TIME:
			cycle_col -= COL_CYCLE_TIME
			ptr_col = (ptr_col + 1) % 4
		if player_col == Color.WHITE:
			match ptr_col:
				0: modulate = Statics.get_color(Vector2i(0, 1))
				1: modulate = Statics.get_color(Vector2i(3, 3))
				2: modulate = Statics.get_color(Vector2i(3, 13))
				3: modulate = Statics.get_color(Vector2i(3, 7))
				4: modulate = Statics.get_color(Vector2i(2, 2)) # Tried it and idk the loop looks better w/o the red
		else:
			match ptr_col:
				0: modulate = player_col
				1: modulate = player_col
				2: modulate = Color("fcfcfc")
				3: modulate = Color("fcfcfc")


# MARKER PRIORITY HIERARCHY:
# - Player-set markers (always show regardless of if the cell is filled or not
# - Saves
# - Bosses
# - Unknowns
# - Items
func update(tile_visible:bool) -> void:
	visible = true
	modulate = Color.WHITE
	player_should_highlight = true
	if player_col != Minimap.MARKER_NO_PMARKER:
		type = Minimap.MarkerTypes.P_MARKER
		sprite.play("marker")
		return
	if not tile_visible:
		visible = false
		player_should_highlight = false
		return
	if save_location != Minimap.MARKER_NO_SAVE:
		type = Minimap.MarkerTypes.SAVE
		sprite.play("save")
	elif boss_id != Minimap.MARKER_NO_BOSS:
		type = Minimap.MarkerTypes.BOSS
		sprite.play("boss")
		var defeated:bool = false
		match boss_id:
			1: defeated = Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS1)
			2: defeated = Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS2)
			3: defeated = Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS3)
			4: defeated = Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS4)
		if defeated:
			modulate = Statics.get_color(Vector2i(1, 1))
	elif item_location != Minimap.MARKER_NO_ITEM and not Minimap.empty_locations.has(item_location):
		var collected:bool = Statics.check_location_collected(item_location)
		if collected:
			type = Minimap.MarkerTypes.ITEM_COLLECTED
			sprite.play("item_collected")
			player_should_highlight = false
		else:
			type = Minimap.MarkerTypes.ITEM
			sprite.play("item_normal")
	else:
		visible = false


func copy_marker(source:MapMarker) -> void:
	data = source.data.duplicate()
	item_location = source.item_location
	save_location = source.save_location
	boss_id = source.boss_id
	player_col = source.player_col
	visible = source.visible
	modulate = source.modulate
