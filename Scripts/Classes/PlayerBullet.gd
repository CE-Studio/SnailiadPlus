@icon("res://Editor/ico/PlayerBullet.svg")
class_name PlayerBullet
extends Node2D


#region Variables
const TICKS_BETWEEN_AFTERIMAGES:int = 4

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
@export var ping_on_breakables:bool = true
@export var despawn_particle:String = "ExplosionSmall"
@export var despawn_offset:Vector2 = Vector2.ZERO
@export var light_radius:int = 0

var normalized_dir:Vector2 = Vector2.ZERO
var life_timer:float = 0.0
var velocity:float = 0.0
var velocity_init:float = 0.0
var single_frame_hit_flag:bool = false
var afterimage_tick:int = 0

@onready var sprite:JsonSprite2D = $"JsonSprite2D"
@onready var area:Area2D = $"Area2D"
@onready var box_normal:CollisionShape2D = $"Area2D/BoxNormal"
@onready var box_power:CollisionShape2D = $"Area2D/BoxPower"
@onready var sfx_shoot_normal:AudioStreamPlayer = $"AudioGroup/ShootNormal"
@onready var sfx_shoot_power:AudioStreamPlayer = $"AudioGroup/ShootPower"
@onready var vis:VisibleOnScreenNotifier2D = $"VisibleOnScreenNotifier2D"
@onready var sfx_despawn:AudioStream = preload("res://Assets/Sounds/Sfx/ShotHit.ogg")
@onready var afterimage:PackedScene = preload("res://Scenes/Entities/Bullets/Player/PlayerBulletAfterimage.tscn")
#endregion


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


func despawn(loudly:bool = false) -> void:
	if loudly and vis.is_on_screen():
		Statics.spawn_particle(despawn_particle, Room.Layers.FG1, Vector2(
			randf_range(-despawn_offset.x, despawn_offset.x),
			randf_range(-despawn_offset.y, despawn_offset.y)
		) + position)
		Statics.play_sfx_disconnected(sfx_despawn)
	queue_free()


func _spin_to_surface(surface:Statics.DirsSurface = Player.instance.gravity_dir) -> void:
	match surface:
		Statics.DirsSurface.FLOOR: area.rotation_degrees = 0.0
		Statics.DirsSurface.LWALL: area.rotation_degrees = 90.0
		Statics.DirsSurface.RWALL: area.rotation_degrees = -90.0
		Statics.DirsSurface.CEILING: area.rotation_degrees = 180.0


func _on_body_entered(body) -> void:
	if body is not Enemy and collide_with_wall:
		despawn(true)


func _can_create_afterimages() -> bool:
	if Statics.stack_weapons:
		return afterimages == 1 or afterimages == 3
	else:
		return afterimages >= 2


func _get_dir_string(dir:Statics.DirsSurface) -> String:
	match dir:
		Statics.DirsSurface.LWALL: return "left"
		Statics.DirsSurface.RWALL: return "right"
		Statics.DirsSurface.CEILING: return "up"
	return "down"


func _get_surface_string(dir:Statics.DirsSurface) -> String:
	match dir:
		Statics.DirsSurface.LWALL: return "lwall"
		Statics.DirsSurface.RWALL: return "rwall"
		Statics.DirsSurface.CEILING: return "ceiling"
	return "floor"
