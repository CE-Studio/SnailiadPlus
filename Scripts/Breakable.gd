# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
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

@export var area:Area2D
@export var box:CollisionShape2D
@export var sprite:SnailySprite2D

var explode1:AudioStream = load("uid://br8snm0iqts4g")
var explode2:AudioStream = load("uid://fxp56us7em3w")
var explode3:AudioStream = load("uid://cbkqdrvlo8gbo")
var explode4:AudioStream = load("uid://brxaghtx4yim")
var ping:AudioStream = load("uid://da0tclibjxftr")

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
	if _area.get_parent() is PlayerBulletAfterimage:
		return
	var bullet:PlayerBullet = _area.get_parent()
	var hit_hard_enough:bool = false
	var icon_anim:String = "locked"
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
			Statics.spawn_particle("ExplosionSmall", Room.Layers.FG1, position + pos)
		if not StaticProcess.check_sound_played_this_frame("BreakableExplode"):
			match randi_range(1, 4):
				1: Statics.play_sfx_limited(explode1, "Explode1", 0.65)
				2: Statics.play_sfx_limited(explode2, "Explode2", 0.65)
				3: Statics.play_sfx_limited(explode3, "Explode3", 0.65)
				4: Statics.play_sfx_limited(explode4, "Explode4", 0.65)
		queue_free()
	else:
		if not is_silent and bullet.ping_on_breakables:
			Statics.play_sfx_disconnected(ping)
		if icon_anim != "":
			var show_option = ProjectSettings.get_setting("game/world/breakables")
			var can_show:bool = false
			match show_option:
				0: # Don't show
					can_show = false
				1: # Obvious, any shot
					can_show = not is_silent
				2: # All, any shot
					can_show = true
				3: # Obvious, percing shot
					can_show = not is_silent and not bullet.collide_with_wall
				4: # All, piercing shot
					can_show = not bullet.collide_with_wall
			if can_show and not sprite.visible:
				sprite.visible = true
				sprite.play(icon_anim)
