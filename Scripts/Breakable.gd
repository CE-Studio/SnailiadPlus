@icon("res://Editor/ico/Breakable.svg")
class_name Breakable
extends Node2D


#region Variables
enum TileTypes {
	PEASHOOTER,
	BOOMERANG,
	RAINBOW_WAVE,
	DEVASTATOR,
	MOON_REMOVE
}

var coords:Vector2i
var tile_data:Array
var type:int
var is_silent:bool
var layers:Array = [
	GameCore.instance.current_room.map_ground,
	GameCore.instance.current_room.map_fg1,
	]
#endregion


func spawn(tile_coords:Vector2i, tile_type:int, silent:bool):
	coords = tile_coords
	type = tile_type
	is_silent = silent
	for map in layers:
		tile_data.append(map.get_cell_atlas_coords(coords))


func _on_bullet_entered(area:Area2D) -> void:
	var bullet = area.get_parent()
	var hit_hard_enough:bool = false
	match type:
		TileTypes.PEASHOOTER:
			hit_hard_enough = true
		TileTypes.BOOMERANG:
			if bullet.type >= 4 or bullet.powered:
				hit_hard_enough = true
		TileTypes.RAINBOW_WAVE:
			if bullet.type >= 8 or bullet.powered:
				hit_hard_enough = true
		TileTypes.DEVASTATOR:
			if bullet.powered:
				hit_hard_enough = true
		TileTypes.MOON_REMOVE:
			hit_hard_enough = false
		_:
			hit_hard_enough = true
	if hit_hard_enough:
		for map in layers:
			map.set_cell(coords)
		queue_free()
