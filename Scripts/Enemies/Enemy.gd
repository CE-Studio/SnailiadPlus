# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
@icon("res://Editor/ico/Enemy.svg")
class_name Enemy
extends Node2D


#region Vaariables
const DAMAGE_TIMEOUT:float = 0.025
const DAMAGE_FLASH_COLOR:Color = Color(0.9, 0.9, 0.9)
const DAMAGE_FLASH_STRENGTH:float = 0.9
const DAMAGE_FADE_DECAY:float = 10.0
const PARRY_DAMAGE_MULT:float = 8.0

@export var max_health:int
@export var max_health_easy:int
@export var max_health_hard:int
@export var attack:int
@export var defense:int
@export var weaknesses:Array[int] = []  # Enemies take double damage from bullet types in this list
@export var resistances:Array[int] = [] # Enemies take half damage from bullet types in this list
@export var immunities:Array[int] = []  # Enemies resist all damage from bullet types in this list
@export_range(0, 1, 0.01) var parry_resist:float = 0.0
@export var can_be_pierced:bool = true
@export var make_sound_on_ping:bool = true
@export var invulnerable:bool = false
@export var can_damage:bool = true
@export var shield_entity:bool = false
@export var interact_with_environments:bool = true
@export var health_orb_value:int = 0
@export_range(0, 256, 1) var light_radius:int = 0
@export var my_element:ElementTypes = ElementTypes.NONE
@export var kill_particle_range:Vector2i = Vector2i(8, 8)
@export var kill_particle_types:Array[String] = [ "ExplosionSmall" ]
@export var kill_particle_count:int = 4
@export var grant_bestiary_without_defeat:bool = false
@export var display_mode:bool = false

var health:int
var parry_damage:int = 0
var damage_timeout:float = 0.0
var stun_invul:bool = false
var ping_played:bool = false
var sent_entry_once:bool = false
var damaged_this_tick:bool = false
var lifetime:float = 0.0

var flash_mat:Material = preload("uid://bj1jqi3180tg5")

enum ElementTypes {
	ICE,
	FIRE,
	NONE = -1
}

@export_group("Components")
@export var col:CollisionShape2D
@export var body:CharacterBody2D
@export var hitbox:Area2D
@export var sprite:SnailySprite2D
@export var vis:VisibleOnScreenNotifier2D
@export var environment:EnvironmentArea

var spawn_conditions:Array[float] = []
var origin:Vector2
var intersecting_player:bool = false
var intersecting_pbullets:Array[PlayerBullet] = []
var intersecting_ebullets:Array[EnemyBullet] = []
var last_damage_num:DamageNumber = null
var ai_active:bool = true
var easy_mode:bool = false
var hard_mode:bool = false

var flash_color:Color = Color.BLACK
var flash_strength:float = 0.0

@onready var sfx_ping:AudioStream = preload("uid://da0tclibjxftr")
@onready var sfx_kill:AudioStream = preload("uid://bt37e4rs2jw1g")
@onready var sfx_hit1:AudioStream = preload("uid://br8snm0iqts4g")
@onready var sfx_hit2:AudioStream = preload("uid://fxp56us7em3w")
@onready var sfx_hit3:AudioStream = preload("uid://cbkqdrvlo8gbo")
@onready var sfx_hit4:AudioStream = preload("uid://brxaghtx4yim")
@onready var hit_sounds:Array = [ sfx_hit1, sfx_hit2, sfx_hit3, sfx_hit4 ]

var my_type:EnemyTypes
enum EnemyTypes {
	SPIKEY_COMMON,     # Blue spikey
	SPIKEY_TOUGH,      # Orange spikey
	SPIKEY_ABSURD,     # Pink spikey
	BABYFISH,          # Green and pink babyfish
	FLOATSPIKE,        # Floatspike
	BLOB_COMMON,       # Blob
	BLOB_TOUGH,        # Blub
	BLOB_ANGEL,        # Angelblob
	BLOB_DEVIL,        # Devilblob
	CHIRPY,            # Chirpy
	BATTYBAT,          # Batty bat
	FIREBALL,          # Fireball
	ICEBALL,           # Iceball
	GHOSTBALL,         # Ghost dandelion
	SNELK,             # Secret snelk
	KITTY,             # Kitty
	CANON,             # Canon (red)
	NONCANON,          # Non-canon (blue)
	FANON,             # Fanon (green)
	SNAKEY,            # Snakey
	MISSING0,          # Unused slot
	SKYVIPER,          # Sky viper
	SPIDER_COMMON,     # Spider
	SPIDER_TOUGH,      # Spider mama
	TURTLE_COMMON,     # Gravity turtle
	TURTLE_TOUGH,      # Cherry red gravity turtle
	JELLYFISH,         # Jellyfish
	SEAHORSE,          # Syngnathida
	TALLFISH_COMMON,   # Tallfish
	TALLFISH_TOUGH,    # Angry tallfish
	WALLEYE,           # Walleye
	PINCER_FLOOR,      # Pincer
	PINCER_WALL,       # Pouncer
	PINCER_CEILING,    # Sky pincer
	GEAR_COMMON,       # Gray spinnygear
	GEAR_TOUGH,        # Red spinnygear
	DRONE,             # Federation drone
	BALLOON,           # Balloon buster
	SHELLBREAKER,      # Shellbreaker
	STOMPY,            # Stompy
	SPACEBOX,          # Space Box
	SPACEBOX_BABYBOX,  # Baby Box
	MOONSNAIL,         # Moon Snail
	GIGASNAIL,         # Giga Moon Snail
	COSMICSNAIL,       # Cosmic Moon Snail
	SHELLBREAKER_RUSH, # Super Shellbreaker
	STOMPY_RUSH,       # Vis Vires
	SPACEBOX_RUSH,     # Time Cube
	MOONSNAIL_RUSH,    # Sun Snail
	GIGASNAIL_RUSH,    # Giga Sun Snail
	ANGRYBLOCK,        # Angry Block
	NONE = -1,
}
#endregion


func spawn(active:bool = true) -> void:
	if display_mode:
		ai_active = false
		can_damage = false
		invulnerable = true
		can_be_pierced = true
		make_sound_on_ping = false
		configure_display_mode()
		return
	if not GameCore.instance:
		return

	origin = position
	ai_active = active
	easy_mode = Statics.current_profile["difficulty"] == 0
	if easy_mode and max_health_easy != 0:
		max_health = max_health_easy
	hard_mode = Statics.current_profile["difficulty"] == 2
	if hard_mode and max_health_hard != 0:
		max_health = max_health_hard
	health = max_health
	if sprite and sprite.material == null:
		sprite.material = flash_mat

	if hitbox:
		hitbox.connect("area_entered", _on_bullet_entered)
		hitbox.connect("area_exited", _on_bullet_exited)
		hitbox.connect("body_entered", _on_player_entered)
		hitbox.connect("body_exited", _on_player_exited)

	if light_radius > 0:
		UICore.instance.darkness_layer.add_source(self, light_radius)


func configure_display_mode(_credits:bool = false, _data:int = 0) -> void:
	pass


func _process(delta: float) -> void:
	if display_mode:
		return

	if vis and vis.is_on_screen() and grant_bestiary_without_defeat and not sent_entry_once:
		sent_entry_once = true
		Statics.add_bestiary_entry(my_type)

	if sprite and not shield_entity:
		sprite.material.set("shader_parameter/flash_color", Color.BLACK + flash_color)
		flash_color = flash_color.lerp(Color.BLACK, DAMAGE_FADE_DECAY * delta)

	lifetime += delta


func _physics_process(delta) -> void:
	if display_mode:
		return

	if damage_timeout > 0.0:
		damage_timeout -= delta
	if not intersecting_player and intersecting_pbullets.is_empty() and intersecting_ebullets.is_empty():
		return

	if intersecting_player and not GameCore.instance.player.stunned and can_damage and ai_active and not CutsceneController.running:
		var can_hit = true
		match my_element:
			ElementTypes.ICE:
				can_hit = not Statics.has_shell(1)
			ElementTypes.FIRE:
				can_hit = not Statics.has_shell(3)
		if can_hit and attack > 0:
			if GameCore.instance.player.adjust_health(-attack):
				parry_damage = floori(attack * PARRY_DAMAGE_MULT)
				parry_damage *= floori(1.0 + (Statics.get_shell_level() * 0.2))
				parry_damage = roundi(lerpf(parry_damage, 0, parry_resist))

	damaged_this_tick = false
	if not stun_invul and (not vis or vis.is_on_screen()) and not invulnerable:
		var pbullets_to_despawn:Array = []
		var ebullets_to_despawn:Array = []
		var kill_flag:bool = false
		var max_damage:int = parry_damage
		var max_color:Color = Statics.get_color(Vector2i(3, 1))
		var was_hit:bool = false
		for bullet in intersecting_pbullets:
			was_hit = true
			var bullet_damage = bullet.damage_powered if bullet.powered else bullet.damage
			if Statics.damage_mult:
				bullet_damage *= Statics.DAMAGE_MULT
			var this_damage:int = bullet_damage
			var this_color:Color = Statics.get_color(Vector2i(3, 1))
			#gravity shock critical damage mult (1.35)
			if not immunities.has(bullet.type) and bullet_damage - defense > 0:
				this_damage = floori(bullet_damage - defense)
				if weaknesses.has(bullet.type):
					this_damage *= 2
					this_color = Statics.get_color(Vector2i(2, 3))
				if resistances.has(bullet.type):
					this_damage = floori(this_damage * 0.5)
					this_color = Statics.get_color(Vector2i(2, 10))
				if this_damage > max_damage:
					max_damage = this_damage
					max_color = this_color
			else:
				if make_sound_on_ping and not ping_played and not bullet is PlayerBulletAfterimage:
					Statics.play_sfx_disconnected(sfx_ping)
				ping_played  = true
				if max_damage == 0:
					max_color = Statics.get_color(Vector2i(3, 0))
			if (not can_be_pierced and not bullet.always_pierce) or bullet.single_hit:
				pbullets_to_despawn.append(bullet)
		for bullet in intersecting_ebullets:
			if bullet.has_been_parried:
				if bullet.parry_damage - defense > 0:
					var this_damage = floori(bullet.parry_damage - defense)
					if this_damage > max_damage:
						max_damage = this_damage
				else:
					if make_sound_on_ping and not ping_played:
						Statics.play_sfx_disconnected(sfx_ping)
					ping_played  = true
				if (not can_be_pierced and not bullet.always_pierce) or bullet.single_hit:
					ebullets_to_despawn.append(bullet)
		if was_hit:
			_spawn_damage_num(max_damage, max_color)
		if max_damage > 0 and not shield_entity:
			if health - max_damage <= 0:
				kill_flag = true
			else:
				_damage(max_damage)
		for bullet in pbullets_to_despawn:
			bullet.despawn(true)
		for bullet in ebullets_to_despawn:
			bullet.queue_free()
		pbullets_to_despawn.clear()
		ebullets_to_despawn.clear()
		if kill_flag:
			kill()
	parry_damage = 0
	ping_played = false


func _on_player_entered(_body) -> void:
	intersecting_player = true


func _on_bullet_entered(_area) -> void:
	var bullet = _area.get_parent()
	if bullet is PlayerBullet:
		intersecting_pbullets.append(bullet)
	elif bullet is EnemyBullet:
		intersecting_ebullets.append(bullet)


func _on_player_exited(_body) -> void:
	intersecting_player = false


func _on_bullet_exited(_area) -> void:
	var bullet = _area.get_parent()
	if bullet is PlayerBullet:
		intersecting_pbullets.remove_at(intersecting_pbullets.find(bullet))
	elif bullet is EnemyBullet:
		intersecting_ebullets.remove_at(intersecting_ebullets.find(bullet))


func _shoot(_scene:PackedScene, _direction:Vector2, _speed:float, _play_sound:bool = true) -> EnemyBullet:
	var bullet:EnemyBullet = _scene.instantiate()
	bullet.global_position = global_position
	Statics.active_room.layer_ground.add_child(bullet)
	bullet._spawn(_direction, _speed, _play_sound)
	bullet.source_enemy = self
	return bullet


func _shoot_360_cluster(_scene:PackedScene, _init_angle:float, _speed:float, _count:int) -> Array[EnemyBullet]:
	var return_bullets:Array[EnemyBullet] = []
	var inc_amount:float = TAU / _count
	for i in range(_count):
		var direction:Vector2 = Vector2(cos(_init_angle), sin(_init_angle))
		var bullet = _shoot(_scene, direction, _speed, i == 0)
		_init_angle += inc_amount
		return_bullets.append(bullet)
	return return_bullets


func _shoot_360_cluster_rotary(_scene:PackedScene, _direction:Vector2, _speed:float, _count:int) -> Array[EnemyBullet]:
	var return_bullets:Array[EnemyBullet] = []
	var inc_amount:float = TAU / _count
	for i in range(_count):
		var bullet = _shoot(_scene, _direction, _speed, i == 0)
		_direction.y += inc_amount
		return_bullets.append(bullet)
	return return_bullets


func _damage(health_lost:int, sound:bool = true, allow_kill:bool = false) -> void:
	if damage_timeout > 0 or GameCore.instance.player.in_death_cutscene:
		return
	health -= health_lost
	damage_timeout = DAMAGE_TIMEOUT
	flash_color = DAMAGE_FLASH_COLOR
	damaged_this_tick = true
	if sound and health > 0:
		Statics.play_sfx_disconnected(hit_sounds[randi_range(0, 3)])
	if allow_kill and health <= 0:
		kill()


func _spawn_damage_num(num:int, color:Color) -> void:
	if not ProjectSettings.get_setting("game/world/damage_numbers"):
		return
	if (last_damage_num and last_damage_num.is_same_color(color)
	and last_damage_num.is_below_hit_threshold()):
		last_damage_num.add(num)
		return
	var new_num:DamageNumber = Statics.damage_number.instantiate()
	GameCore.instance.current_room.layer_ground.add_child(new_num)
	new_num.position = position + Vector2(0, -8)
	new_num.instance(num, color)
	last_damage_num = new_num


func kill() -> void:
	Statics.play_sfx_disconnected(sfx_kill)
	if my_type != EnemyTypes.NONE:
		Statics.add_bestiary_entry(my_type)
	for i in range(kill_particle_count):
		var p_range = kill_particle_range
		var pos = Vector2(randi_range(-p_range.x, p_range.x), randi_range(-p_range.y, p_range.y))
		var part = kill_particle_types[randi() % kill_particle_types.size()]
		Statics.spawn_particle(part, Room.Layers.FG1, global_position + pos)
	if Statics.current_profile["character"] == Player.Players.LEECHY:
		spawn_health_orbs()
	environment = null
	queue_free()


func spawn_health_orbs() -> void:
	pass
#	protected void SpawnHealthOrbs()
#	{
#		healthOrbValue = Mathf.CeilToInt(healthOrbValue * PlayState.HEALTH_ORB_MULTS[PlayState.currentProfile.difficulty]);
#		while (healthOrbValue > 0)
#		{
#			int randRange = 0;
#			if (healthOrbValue >= PlayState.HEALTH_ORB_VALUES[2])
#				randRange = 3;
#			else if (healthOrbValue >= PlayState.HEALTH_ORB_VALUES[1])
#				randRange = 2;
#			else if (healthOrbValue >= PlayState.HEALTH_ORB_VALUES[0])
#				randRange = 1;
#			if (randRange == 0)
#				healthOrbValue = 0;
#			else
#			{
#				int orbSize = Random.Range(0, randRange);
#				PlayState.GetHealthOrb(orbSize, transform.position);
#				healthOrbValue -= PlayState.HEALTH_ORB_VALUES[orbSize];
#			}
#		}
#	}
