@icon("res://Editor/ico/Breakable.svg")
class_name Breakable
extends Node2D


#region Variables
enum TileTypes {
	PEASHOOTER,
	BOOMERANG,
	RAINBOW_WAVE,
	DEVASTATOR,
	MOON_REMOVE,
	ENEMY_COLLIDE
}

@onready var area:Area2D = $"Area2D"
@onready var box:CollisionShape2D = $"Area2D/CollisionShape2D"
@onready var sprite:JsonSprite2D = $"JsonSprite2D"
@onready var vis:VisibleOnScreenNotifier2D = $"VisibleOnScreenNotifier2D"

var explode1:AudioStream = load("res://Assets/Sounds/Sfx/Explode1.ogg")
var explode2:AudioStream = load("res://Assets/Sounds/Sfx/Explode2.ogg")
var explode3:AudioStream = load("res://Assets/Sounds/Sfx/Explode3.ogg")
var explode4:AudioStream = load("res://Assets/Sounds/Sfx/Explode4.ogg")

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
	if type == TileTypes.ENEMY_COLLIDE:
		box.collision_layer = 2
	sprite.visible = false


func _on_bullet_entered(_area:Area2D) -> void:
	if vis.is_on_screen():
		var bullet = _area.get_parent()
		var hit_hard_enough:bool = false
		var icon_anim:String = ""
		match type:
			TileTypes.PEASHOOTER:
				icon_anim = "peashooter"
				hit_hard_enough = true
			TileTypes.BOOMERANG:
				icon_anim = "boomerang"
				if bullet.type >= 4 or bullet.powered:
					hit_hard_enough = true
			TileTypes.RAINBOW_WAVE:
				icon_anim = "rainbow_wave"
				if bullet.type >= 8 or bullet.powered:
					hit_hard_enough = true
			TileTypes.DEVASTATOR:
				icon_anim = "devastator"
				if bullet.powered:
					hit_hard_enough = true
			_:
				hit_hard_enough = false
		if hit_hard_enough:
			sprite.visible = false
			for map in layers:
				map.set_cell(coords)
			for i in range(2):
				var pos = Vector2(randi_range(-16, 16), randi_range(-16, 16))
				Statics.spawn_particle("ExplosionBig", Room.Layers.FG1, position + pos)
			match randi_range(1, 4):
				1: Statics.play_sfx_disconnected(explode1)
				2: Statics.play_sfx_disconnected(explode2)
				3: Statics.play_sfx_disconnected(explode3)
				4: Statics.play_sfx_disconnected(explode4)
			queue_free()
		elif icon_anim != "" and not is_silent:
			sprite.visible = true
			sprite.action = icon_anim
