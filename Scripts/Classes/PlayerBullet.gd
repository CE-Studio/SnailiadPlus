# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
@icon("res://Editor/ico/PlayerBullet.svg")
class_name PlayerBullet
extends Node2D


#region Variables
const TICKS_BETWEEN_AFTERIMAGES:int = 4
const ANGLE_DEADZONE:float = 0.3827

@export_flags("Broom", "Peashooter", "Boomerang", "Rainbow Wave") var type:int = 0
@export var damage:int = 0
@export var damage_powered:int = 0
@export var cooldown:float = 0.0
@export var rapid_mult:float = 1.0
@export var powered:bool = false
@export_enum(
	"Don't create", "Create if stacked",
	"Create if unstacked", "Always create") var afterimages:int = 0
@export var despawn_offscreen:bool = false
@export var collide_with_wall:bool
@export var single_hit:bool
@export var always_pierce:bool
@export var ping_on_breakables:bool = true
@export var immune_to_bullet_collisions:bool = false
@export var despawn_particle:String = "ExplosionSmall"
@export var despawn_offset:Vector2 = Vector2.ZERO
@export var light_radius:int = 0

## The direction in which this bullet travels
var normalized_dir:Vector2 = Vector2.ZERO
## The time in seconds since this bullet was spawned
var life_timer:float = 0.0
## The speed at which this bullet is currently traveling
var velocity:float = 0.0
## The initial speed assigned to this bullet when it was fired
var velocity_init:float = 0.0
## If this bullet is set to create afterimage hitboxes, this value tracks how many frames have passed
## since the last one was spawned
var afterimage_tick:int = 0

## The sprite component of the bullet
@onready var sprite:SnailySprite2D = $"SnailySprite2D"
## The main area component of the bullet
@onready var area:Area2D = $"Area2D"
## The hitbox shape used for normal bullets
@onready var box_normal:CollisionShape2D = $"Area2D/BoxNormal"
## The hitbox shape used for bullets marked as powered
@onready var box_power:CollisionShape2D = $"Area2D/BoxPower"
## The sound played when a normal bullet fires
@onready var sfx_shoot_normal:AudioStreamPlayer = $"AudioGroup/ShootNormal"
## The sound played when a powered bullet fires
@onready var sfx_shoot_power:AudioStreamPlayer = $"AudioGroup/ShootPower"
## The area used to detect if this bullet is on-screen
@onready var vis:VisibleOnScreenNotifier2D = $"VisibleOnScreenNotifier2D"
## The sound played when this bullet despawns after a collision with an entity or world geometry
@onready var sfx_despawn:AudioStream = preload("uid://jy74nugtx5w1")
## Persistent reference to the [PlayerBulletAfterimage] scene
@onready var afterimage:PackedScene = preload("uid://dpwnugjso664")
#endregion


## Initializes this bullet with the desired direction and other configurations
func _spawn(dir:Vector2, rapid_shot:float, power_shot:bool) -> float:
	normalized_dir = dir
	rapid_mult = rapid_shot
	powered = power_shot
	if power_shot:
		box_normal.disabled = true
		box_power.disabled = false
		sfx_shoot_power.play()
	else:
		sfx_shoot_normal.play()
	area.connect("body_entered", _on_body_entered)
	if light_radius > 0:
		UICore.instance.darkness_layer.add_source(self, light_radius)
	afterimage_tick = randi_range(0, TICKS_BETWEEN_AFTERIMAGES - 1)
	return cooldown / rapid_mult


func _infer_direction_anim() -> void:
	var anim_name = ""
	if normalized_dir.y < -ANGLE_DEADZONE:
		anim_name += "U"
	elif normalized_dir.y > ANGLE_DEADZONE:
		anim_name += "D"
	if normalized_dir.x < -ANGLE_DEADZONE:
		anim_name += "L"
	elif normalized_dir.x > ANGLE_DEADZONE:
		anim_name += "R"
	if powered:
		anim_name += "_power"
	sprite.play(anim_name)


func _physics_process(delta: float) -> void:
	life_timer += delta
	if ((life_timer > 3 or (despawn_offscreen and life_timer >= 0.25)) and
	not vis.is_on_screen()):
		despawn()
	elif _can_create_afterimages():
		afterimage_tick += 1
		if afterimage_tick >= TICKS_BETWEEN_AFTERIMAGES:
			var new_afterimage:PlayerBulletAfterimage = afterimage.instantiate()
			GameCore.instance.current_room.layer_ground.add_child(new_afterimage)
			new_afterimage.position = position
			new_afterimage._spawn_afterimage(self)
			afterimage_tick -= TICKS_BETWEEN_AFTERIMAGES


## Frees the bullet with any necessary particles and sounds
func despawn(loudly:bool = false) -> void:
	if loudly and vis.is_on_screen():
		Statics.spawn_particle(despawn_particle, Room.Layers.FG1, Vector2(
			randf_range(-despawn_offset.x, despawn_offset.x),
			randf_range(-despawn_offset.y, despawn_offset.y)
		) + position)
		Statics.play_sfx_disconnected(sfx_despawn)
	queue_free()

## Rotates the bullet's hitbox to match the given direction, usually in accordance with
## the player's gravity
func _spin_to_surface(surface:Statics.DirsSurface = Player.instance.gravity_dir) -> void:
	match surface:
		Statics.DirsSurface.FLOOR: area.rotation_degrees = 0.0
		Statics.DirsSurface.LWALL: area.rotation_degrees = 90.0
		Statics.DirsSurface.RWALL: area.rotation_degrees = -90.0
		Statics.DirsSurface.CEILING: area.rotation_degrees = 180.0


## Called when the bullet intersects a body
func _on_body_entered(body) -> void:
	if body is not Enemy and collide_with_wall:
		despawn(true)


## Returns [code]true[/code] if this bullet is capable of creating afterimage hitboxes
func _can_create_afterimages() -> bool:
	if Statics.stack_weapons:
		return afterimages == 1 or afterimages == 3
	else:
		return afterimages >= 2


## Returns the string representation of a given direction enum for use in animations
func _get_dir_string(dir:Statics.DirsSurface) -> String:
	match dir:
		Statics.DirsSurface.LWALL: return "left"
		Statics.DirsSurface.RWALL: return "right"
		Statics.DirsSurface.CEILING: return "up"
	return "down"


## Returns the string representation of a given surface enum for use in animations
func _get_surface_string(dir:Statics.DirsSurface) -> String:
	match dir:
		Statics.DirsSurface.LWALL: return "lwall"
		Statics.DirsSurface.RWALL: return "rwall"
		Statics.DirsSurface.CEILING: return "ceiling"
	return "floor"
