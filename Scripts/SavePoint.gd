# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
@tool
@icon("res://Editor/ico/Interactable.svg")
class_name SavePoint
extends Node2D

#region Variables
@export_enum("Floor", "Left wall", "Right wall", "Ceiling") var surface:int = int(Statics.DirsSurface.FLOOR):
	set(value):
		surface = value
		_ready()
@export_flags("Snaily", "Sluggy", "Upside", "Leggy", "Blobby", "Leechy") var characters_who_can_use_me:int = 63

var activated:bool = false
var room_name:String
var surface_str:String = ""

@onready var sprite:SnailySprite2D = $"SnailySprite2D"
@onready var box:CollisionShape2D = $"Area2D/CollisionShape2D"
@onready var sfx:AudioStreamPlayer = $"Jingle"
#endregion


func _ready() -> void:
	if surface == Statics.DirsSurface.LWALL or surface == Statics.DirsSurface.RWALL:
		box.rotation_degrees = 90
	else:
		box.rotation_degrees = 0
	if Engine.is_editor_hint():
		sprite.visible = false
		var marker = $"MarkerSprite"
		match surface:
			Statics.DirsSurface.FLOOR: marker.frame = 0
			Statics.DirsSurface.LWALL: marker.frame = 4
			Statics.DirsSurface.RWALL: marker.frame = 8
			Statics.DirsSurface.CEILING: marker.frame = 12
	else:
		match surface:
			Statics.DirsSurface.FLOOR: surface_str = "_d"
			Statics.DirsSurface.LWALL: surface_str = "_l"
			Statics.DirsSurface.RWALL: surface_str = "_r"
			Statics.DirsSurface.CEILING: surface_str = "_u"
	
	if UICore.instance:
		UICore.instance.darkness_layer.add_source(self, 48)


func check_character_spawnable() -> bool:
	var char_flag:int = 1 << int(Statics.current_profile["character"])
	if characters_who_can_use_me & char_flag > 0:
		return true
	queue_free()
	return false


func initialize_room_data(_room_name:String) -> void:
	room_name = _room_name
	var saved_position = Statics.current_profile["save_coords"]
	if saved_position is String:
		saved_position = str_to_var("Vector2" + saved_position)
	if (Statics.current_profile["save_room"] == room_name
	and saved_position == global_position):
		sprite.play("last" + surface_str)
	else:
		sprite.play("inactive" + surface_str)


func _on_player_entered(_body) -> void:
	if not activated:
		activated = true
		Statics.current_profile["save_room"] = room_name
		Statics.current_profile["save_coords"] = global_position
		Statics.save_profile(Statics.current_profile_id)
		sfx.play()
		sprite.play("touched" + surface_str)
		sprite.autoplay_next = "active" + surface_str
		_spawn_save_particles()
		UICore.instance.play_save_anim()


func _spawn_save_particles() -> void:
	var particle_setting = ProjectSettings.get_setting("game/world/particles")
	if (particle_setting != Statics.ParticleOptions.ENTITIES_ALL
	and particle_setting != Statics.ParticleOptions.ALL):
		return
	
	var start_pos:Vector2
	var advance_dir:Vector2
	var float_dir:Vector2
	match surface:
		Statics.DirsSurface.FLOOR:
			start_pos = position + Vector2(-16, 8)
			advance_dir = Vector2.RIGHT
			float_dir = Vector2.UP
		Statics.DirsSurface.LWALL:
			start_pos = position + Vector2(-8, 16)
			advance_dir = Vector2.UP
			float_dir = Vector2.RIGHT
		Statics.DirsSurface.RWALL:
			start_pos = position + Vector2(8, 16)
			advance_dir = Vector2.UP
			float_dir = Vector2.LEFT
		Statics.DirsSurface.CEILING:
			start_pos = position + Vector2(-16, -8)
			advance_dir = Vector2.RIGHT
			float_dir = Vector2.DOWN
	for i in range(17):
		var spawn_pos = start_pos + (advance_dir * 2 * i)
		var new_particle = Statics.spawn_particle("DotGeneric", Room.Layers.GROUND, spawn_pos, [float_dir * randf_range(5.0, 30.0)])
		match randi() % 5:
			0: new_particle.sprite.modulate = Statics.get_color(Vector2i(3, 4))
			1: new_particle.sprite.modulate = Statics.get_color(Vector2i(3, 9))
			2: new_particle.sprite.modulate = Statics.get_color(Vector2i(2, 7))
			3: new_particle.sprite.modulate = Statics.get_color(Vector2i(0, 1))
			4: new_particle.sprite.modulate = Statics.get_color(Vector2i(2, 2))
