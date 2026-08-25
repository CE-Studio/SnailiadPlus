# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
@icon("res://Editor/ico/EnemyBullet.svg")
class_name EnemyBullet
extends Node2D


#region Variables
const TICKS_BETWEEN_AFTERIMAGES:int = 4
const ANGLE_DEADZONE:float = 0.3827

@export var damage:int = 0
@export var rush_damage:int = 0
@export var parry_damage:int = 0
@export var max_life_time:float = 1.6
@export var rapid_mult:float = 1.0
@export var despawn_offscreen:bool = false
@export var collide_with_wall:bool = false
@export var single_hit:bool = false
@export var always_pierce:bool = false
@export var despawn_particle:String = "ExplosionSmall"
@export var despawn_offset:Vector2 = Vector2.ZERO
@export var light_radius:int = 0
@export var one_sound_per_frame:bool = false
@export var afterimages:bool = false
@export_group("Player bullet interactions")
@export var pbullet_interaction:PBulletInteractions = PBulletInteractions.ALWAYS_DESTROY
@export var pbullets_that_i_destroy:Array[int] = []
@export var pbullets_that_destroy_me:Array[int] = []

## The direction this bullet should travel
var normalized_dir:Vector2 = Vector2.ZERO
## The amount of time in seconds since this bullet was spawned
var life_timer:float = 0.0
## The speed at which this bullet should travel
var velocity:float = 0.0
## The speed set when this bullet was spawned
var velocity_init:float = 0.0
## The [Enemy] that spawned this bullet
var source_enemy:Enemy
## Will be set to [code]true[/code] if this bullet has been reflected by a Perfect Parry
var has_been_parried:bool = false
## Will be set to [code]true[/code] if this bullet is currently overlapping the player's hitbox
var intersecting_player:bool = false
## If this bullet is set to create afterimage hitboxes, this value tracks how many frames have passed
## since the last afterimage was spawned
var afterimage_tick:int = 0
## Will be set to [code]true[/code] if this bullet's animation has been previously inferred from
## its travel direction
var has_inferred_once:bool = false

## Determines what interactions with any set [PlayerBullet] nodes this bullet should have
enum PBulletInteractions {
	ALWAYS_DESTROY, ## Any collision angle will cause an interaction
	DESTROY_PARALLEL, ## Bullets must be traveling parallel to each other to cause an interaction
	DESTROY_PARALLEL_WIDE, ## Bullets must be traveling anywhere near parallel to cause an interaction
	DESTROY_PERPENDICULAR, ## Bullets must be traveling perpendicular to each other to cause an interaction
	DESTROY_PERPENDICULAR_WIDE ## Bullets must be traveling anywhere near perpendicular to cause an interaction
}

## The sprite component of this bullet
@onready var sprite:SnailySprite2D = $"SnailySprite2D"
## The bullet's hitbox for tracking entity collisions
@onready var area:Area2D = $"Area2D"
## The bullet's hitbox for tracking world collisions
@onready var box:CollisionShape2D = $"Area2D/Box"
## The sound that plays when this bullet is initially fired
@onready var sfx:AudioStreamPlayer = $"AudioGroup/Shoot"
## The area that is read to determine if the bullet is currently on-screen
@onready var vis:VisibleOnScreenNotifier2D = $"VisibleOnScreenNotifier2D"
## A persistent reference to the [EnemyBulletAfterimage] scene
@onready var afterimage:PackedScene = preload("uid://bcpnu7gti14ph")
#endregion


## Initializes this bullet with a direction and speed to travel in
func _spawn(dir:Vector2, speed:float, play_sound:bool = true) -> void:
	normalized_dir = dir
	velocity_init = speed
	if play_sound and (not one_sound_per_frame or not StaticProcess.check_sound_played_this_frame(sfx.stream.resource_path)):
		sfx.play()
	area.connect("area_entered", _on_pbullet_collision)
	area.connect("body_entered", _on_body_entered)
	area.connect("body_exited", _on_body_exited)
	if light_radius > 0:
		UICore.instance.darkness_layer.add_source(self, light_radius)


func _infer_direction_anim(_angle:Vector2 = normalized_dir) -> void:
	has_inferred_once = true
	var anim_name = ""
	if _angle.y < -ANGLE_DEADZONE:
		anim_name += "U"
	elif _angle.y > ANGLE_DEADZONE:
		anim_name += "D"
	if _angle.x < -ANGLE_DEADZONE:
		anim_name += "L"
	elif _angle.x > ANGLE_DEADZONE:
		anim_name += "R"
	sprite.play(anim_name)
	_flip_sprite_from_dir(_angle)


func _flip_sprite_from_dir(_angle:Vector2 = normalized_dir) -> void:
	pass


func _physics_process(delta: float) -> void:
	if intersecting_player and not GameCore.instance.player.stunned and not has_been_parried and not CutsceneController.running:
		var this_damage:int = damage
		if Statics.is_in_boss_rush and rush_damage != 0:
			this_damage = rush_damage
		if GameCore.instance.player.adjust_health(-this_damage, false, true):
			parry_reshoot()
		elif single_hit:
			_despawn()
	
	life_timer += delta
	if (life_timer > max_life_time
	or (despawn_offscreen and life_timer >= 0.25 and not vis.is_on_screen())):
		_despawn()
	
	if afterimages:
		afterimage_tick += 1
		if afterimage_tick >= TICKS_BETWEEN_AFTERIMAGES:
			var new_afterimage:EnemyBulletAfterimage = afterimage.instantiate()
			GameCore.instance.current_room.layer_ground.add_child(new_afterimage)
			new_afterimage.position = position
			new_afterimage._spawn_afterimage(self)
			afterimage_tick -= TICKS_BETWEEN_AFTERIMAGES


## Performs a secondary fire after being reflected by a Perfect Parry
func parry_reshoot() -> void:
	has_been_parried = true
	if source_enemy:
		normalized_dir = Vector2(source_enemy.global_position - global_position).normalized()
	else:
		normalized_dir *= -1.0
	life_timer = 0.0
	velocity = velocity_init
	parry_damage *= floori(1.0 + (Statics.get_shell_level() * 0.2))
	if has_inferred_once:
		_infer_direction_anim()


## Called whenever this bullet intersects with another body
func _on_body_entered(_body) -> void:
	if _body.get_parent() is Player:
		intersecting_player = true
	elif collide_with_wall:
		_despawn(true)


## Called whenever this bullet exits another body
func _on_body_exited(_body) -> void:
	if _body.get_parent() is Player:
		intersecting_player = false


## Called whenever this bullet intersects with a [PlayerBullet] in order to handle collision interactions
func _on_pbullet_collision(_area:Area2D) -> void:
	if has_been_parried:
		return
	var bullet = _area.get_parent()
	if bullet is PlayerBullet and not bullet is PlayerBulletAfterimage:
		var i_destroy:bool = pbullets_that_i_destroy.has(bullet.type)
		var destroys_me:bool = pbullets_that_destroy_me.has(bullet.type)
		var angle = rad_to_deg(normalized_dir.angle_to(bullet.normalized_dir))
		angle = abs(angle)
		var destroy_flag
		match pbullet_interaction:
			PBulletInteractions.ALWAYS_DESTROY:
				destroy_flag = true
			PBulletInteractions.DESTROY_PARALLEL:
				destroy_flag = angle <= 22.5 or angle >= 157.5
			PBulletInteractions.DESTROY_PARALLEL_WIDE:
				destroy_flag = angle <= 67.5 or angle >= 112.5
			PBulletInteractions.DESTROY_PERPENDICULAR:
				destroy_flag = angle >= 67.5 and angle <= 112.5
			PBulletInteractions.DESTROY_PERPENDICULAR_WIDE:
				destroy_flag = angle >= 22.5 and angle <= 157.5
		if destroy_flag:
			if i_destroy and not bullet.immune_to_bullet_collisions:
				bullet.despawn(true)
			if destroys_me:
				_despawn(true)


## Will free this bullet with any required particles and sounds
func _despawn(_loudly:bool = false) -> void:
	if _loudly and vis.is_on_screen() and despawn_particle.strip_edges() != "":
		Statics.spawn_particle(despawn_particle, Room.Layers.FG1, Vector2(
			randf_range(-despawn_offset.x, despawn_offset.x),
			randf_range(-despawn_offset.y, despawn_offset.y)
		) + position)
	queue_free()
