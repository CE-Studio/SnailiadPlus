# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
@tool
@icon("res://Editor/ico/Door.svg")
class_name Door
extends Node2D


#region Variables
enum LockTypes {
	ALWAYS_LOCKED,
	LOCKED_BY_BOSS,
	LOCKED_BY_BOSS_IN_RANDOMIZER,
	LOCKED_BY_FRAGMENTS_IN_RANDOMIZER,
	UNLOCKED = -1,
}

const SPAWN_OPEN_RADIUS = 40
const SPAWN_CLOSE_RADIUS = 70

@export var direction:Statics.DirsCardinal = Statics.DirsCardinal.LEFT:
	set(value):
		direction = value
		_set_editor_marker()
@export_enum(
	"Peashooter", "Boomerang", "Rainbow Wave", "Devastator"
) var door_type:int:
	set(value):
		door_type = value
		_set_editor_marker()
@export var lock_type:LockTypes = LockTypes.UNLOCKED:
	set(value):
		lock_type = value
		_set_editor_marker()
@export_enum(
	"Shellbreaker", "Stompy", "Space Box", "Moon Snail"
) var required_boss:int
@export_range(0, 3) var rando_sphere:int

var is_open:bool = false
var is_locked:bool = false
var spawned_open:bool = false
var anim_prefix:String = ""

@export_group("Components")
@export var vis:VisibleOnScreenNotifier2D
@export var box_group:Node2D
@export var body:StaticBody2D
@export var box:CollisionShape2D
@export var sprite:SnailySprite2D
@export var sfx_open:AudioStreamPlayer
@export var sfx_close:AudioStreamPlayer
@export var sfx_ping:AudioStreamPlayer
#endregion


func _ready() -> void:
	_set_editor_marker()
	if direction == Statics.DirsCardinal.RIGHT:
		sprite.flip_h = true
	if direction == Statics.DirsCardinal.DOWN:
		sprite.flip_v = true


func spawn() -> void:
	#Temp
	if _check_boss_locked() or lock_type == LockTypes.ALWAYS_LOCKED:
		anim_prefix = "locked_"
		is_locked = true
	else:
		match door_type:
			0: anim_prefix = "blue_"
			1: anim_prefix = "purple_"
			2: anim_prefix = "red_"
			3: anim_prefix = "green_"
	match direction:
		Statics.DirsCardinal.LEFT: anim_prefix += "left_"
		Statics.DirsCardinal.RIGHT: anim_prefix += "right_"
		Statics.DirsCardinal.UP: anim_prefix += "up_"
		Statics.DirsCardinal.DOWN: anim_prefix += "down_"
	
	if position.distance_to(GameCore.instance.player.position) <= SPAWN_OPEN_RADIUS:
		is_open = true
		spawned_open = true
		sprite.play(anim_prefix + "open")
	else:
		box.disabled = false
		sprite.play(anim_prefix + "closed")
	
	if direction == Statics.DirsCardinal.DOWN or direction == Statics.DirsCardinal.UP:
		box_group.rotation_degrees = 90.0
	
	UICore.instance.darkness_layer.add_source(self, 40)


func _check_boss_locked() -> bool:
	if (lock_type == LockTypes.LOCKED_BY_BOSS
	or (Statics.is_random_game and lock_type == LockTypes.LOCKED_BY_BOSS_IN_RANDOMIZER)):
		match required_boss:
			0:
				if Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS1) != true:
					return true
			1:
				if Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS2) != true:
					return true
			2:
				if Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS3) != true:
					return true
			3:
				if Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS4) != true:
					return true
	return false


func _process(_delta: float) -> void:
	if spawned_open and is_open:
		if position.distance_to(GameCore.instance.player.position) > SPAWN_CLOSE_RADIUS:
			close()


func _set_editor_marker():
	if not Engine.is_editor_hint() or box_group == null:
		return
	sprite.visible = false
	var target_frame
	var marker = $"MarkerSprite"
	if direction == Statics.DirsCardinal.UP or direction == Statics.DirsCardinal.DOWN:
		box_group.rotation_degrees = 90.0
		target_frame = 9
	else:
		box_group.rotation_degrees = 0.0
		target_frame = 0
	marker.flip_h = direction == Statics.DirsCardinal.RIGHT
	marker.flip_v = direction == Statics.DirsCardinal.DOWN
	if lock_type == LockTypes.LOCKED_BY_FRAGMENTS_IN_RANDOMIZER:
		target_frame += (door_type * 18) + 7
	elif lock_type != LockTypes.UNLOCKED:
		target_frame += 72
	else:
		target_frame += door_type * 18
	marker.frame = target_frame


func _on_bullet_entered(area:Area2D) -> void:
	if not vis.is_on_screen() or not GameCore.instance.current_room.collision_enabled:
		return
	var bullet = area.get_parent()
	if is_locked:
		sfx_ping.play()
	elif not is_open:
		var hit_hard_enough:bool = false
		match door_type:
			1:
				if bullet.type >= 4 or bullet.powered:
					hit_hard_enough = true
			2:
				if bullet.type >= 8 or bullet.powered:
					hit_hard_enough = true
			3:
				if bullet.powered:
					hit_hard_enough = true
			_:
				hit_hard_enough = true
		if hit_hard_enough:
			open()
		else:
			sfx_ping.play()


func open() -> void:
	sprite.play(anim_prefix + "opening")
	sprite.autoplay_next = anim_prefix + "open"
	box.set_deferred("disabled", true)
	is_open = true
	spawned_open = false
	sfx_open.play()


func close() -> void:
	sprite.play(anim_prefix + "closing")
	sprite.autoplay_next = anim_prefix + "closed"
	box.set_deferred("disabled", false)
	is_open = false
	sfx_close.play()
