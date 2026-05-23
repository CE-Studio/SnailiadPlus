# Copyright 2026 CE-Studio: AGPL-3.0-only
extends Particle


const MODE_WEIGHTS:Array = [0, 1, 1, 2, 2, 2, 2, 3, 3, 4, 4]

@export var spr:Sprite2D

var mode:int = -1
var mode_initialized:bool = false
var mode_time:float = 0.0
var visible_time:float = 0.0
var tile_world_coords:Vector2i = Vector2i.ZERO
var tile_atlas_coords:Vector2i = Vector2i.ZERO


func _ready() -> void:
	mode_time = randf_range(-0.5, 1.5)


func _process(delta: float) -> void:
	mode_time -= delta
	visible_time -= delta
	if visible_time <= 0.0:
		spr.visible = false
	if mode_time <= 0.0:
		mode_initialized = false
		spr.visible = true
		spr.region_rect.size = Vector2i(16, 16)
		mode = MODE_WEIGHTS.pick_random()
		mode_time = randf_range(0.5, 3.0)
		_set_pos()
	match mode:
		0: # Random entity tile
			if not mode_initialized or randf() <= Statics.FRAC_64:
				spr.region_rect.position = Vector2(
					randi_range(0, 15),
					randi_range(0, 4)
				) * 16
			if not mode_initialized:
				visible_time = randf_range(0.125, 0.625)
				mode_initialized = true
		1: # Offset room tile
			if not mode_initialized:
				spr.region_rect.position = Vector2(tile_atlas_coords * 16)
				position += Vector2(randi_range(-4, 4), randi_range(-4, 4))
				visible_time = randf_range(0.125, 0.75)
				mode_initialized = true
		2: # Duplicated tile corner
			if not mode_initialized:
				spr.region_rect.position = Vector2(tile_atlas_coords * 16)
				visible_time = randf_range(0.3, 1.5)
				mode_time += visible_time - randf_range(0.1, 0.3)
				spr.region_rect.size = Vector2i(8, 8)
				position -= Vector2(4, 4)
				var atlas_corner:int = randi_range(0, 3)
				var tile_corner:int = atlas_corner
				while tile_corner == atlas_corner:
					tile_corner = randi_range(0, 3)
				if atlas_corner & 1 > 0: spr.region_rect.position.x += 8
				if atlas_corner & 2 > 0: spr.region_rect.position.y += 8
				if tile_corner & 1 > 0: position.x += 8
				if tile_corner & 2 > 0: position.y += 8
				mode_initialized = true
		3: # UV shift
			if not mode_initialized or randf() <= Statics.FRAC_64:
				spr.region_rect.position += Vector2(
					randi_range(-2, 2),
					randi_range(-2, 2)
				) * 8
			if not mode_initialized:
				spr.region_rect.position = Vector2(tile_atlas_coords * 16)
				visible_time = randf_range(0.1, 1.5)
				mode_time += randf_range(0.0, 1.0)
				mode_initialized = true
		4: # Tile shift
			if not mode_initialized:
				spr.region_rect.position = Vector2(tile_atlas_coords * 16)
				visible_time = randf_range(0.3, 0.9)
				mode_time += randf_range(0.0, 0.5)
				z_index += 300
				match randi_range(0, 3):
					0: position += Vector2.LEFT * 16
					1: position += Vector2.RIGHT * 16
					2: position += Vector2.UP * 16
					3: position += Vector2.DOWN * 16
				mode_initialized = true


func _set_pos() -> void:
	tile_world_coords = Vector2i(
		randi_range(-3, 27),
		randi_range(-3, 17),
	)
	position = tile_world_coords * 16
	var cam_pos:Vector2 = UICore.instance.get_cam_center_pos()
	tile_atlas_coords = Vector2i(-1, -1)
	
	while absf(cam_pos.x - position.x) > 200 + (16 * 3):
		if position.x < cam_pos.x:
			position.x += 400 + (16 * 6)
			tile_world_coords.x += 25 + 6
		else:
			position.x -= 400 + (16 * 6)
			tile_world_coords.x -= 25 + 6
	while absf(cam_pos.y - position.y) > 120 + (16 * 3):
		if position.y < cam_pos.y:
			position.y += 240 + (16 * 6)
			tile_world_coords.y += 15 + 6
		else:
			position.y -= 240 + (16 * 6)
			tile_world_coords.y -= 15 + 6
	
	var room:Room = GameCore.instance.current_room
	tile_atlas_coords = room.map_fg2.get_cell_atlas_coords(tile_world_coords)
	z_index = 201
	if tile_atlas_coords == Vector2i(-1, -1):
		tile_atlas_coords = room.map_fg1.get_cell_atlas_coords(tile_world_coords)
		z_index = 101
	if tile_atlas_coords == Vector2i(-1, -1):
		tile_atlas_coords = room.map_ground.get_cell_atlas_coords(tile_world_coords)
		z_index = 1
	if tile_atlas_coords == Vector2i(-1, -1):
		tile_atlas_coords = room.map_bg1.get_cell_atlas_coords(tile_world_coords)
		z_index = -99
	if tile_atlas_coords == Vector2i(-1, -1):
		tile_atlas_coords = room.map_bg2.get_cell_atlas_coords(tile_world_coords)
		z_index = -199
	if tile_atlas_coords == Vector2i(-1, -1):
		tile_atlas_coords = room.map_sky.get_cell_atlas_coords(tile_world_coords)
		z_index = -299
