@icon("res://Editor/ico/Enemy.svg")
class_name Enemy
extends CharacterBody2D


#region Vaariables
const DAMAGE_TIMEOUT:float = 0.025

@export var max_health:int
@export var attack:int
@export var defense:int
@export var weaknesses:Array[int] = []  # Enemies take double damage from bullet types in this list
@export var resistances:Array[int] = [] # Enemies take half damage from bullet types in this list
@export var immunities:Array[int] = []  # Enemies resist all damage from bullet types in this list
@export var can_be_pierced:bool
@export var make_sound_on_ping:bool = true
@export var invulnerable:bool = false
@export var can_damage:bool = true
@export var shield_entity:bool = false
@export var health_orb_value:int = 0
@export var my_element:ElementTypes = ElementTypes.NONE
@export var kill_particle_range:Vector2i = Vector2i(8, 8)
@export var kill_particle_types:Array[String] = [ "ExplosionSmall" ]
@export var kill_particle_count:int = 4

var health:int
var parry_damage:int = 0
var damage_timeout:float = 0.0
var stun_invul:bool = false

enum ElementTypes {
	ICE,
	FIRE,
	NONE = -1
}

var col:CollisionShape2D
var hitbox:Area2D
var sprite:JsonSprite2D

var ping_player:int = 0

var spawn_conditions:Array[float] = []
var origin:Vector2
var intersecting_player:bool = false
var intersecting_bullets:Array[PlayerBullet] = []
var intersecting_enemy_bullets:Array = [] #TODO: mark this as EnemyBullet array when class implemented
var ai_active:bool = true

@onready var sfx_ping:AudioStream = preload("res://Assets/Sounds/Sfx/Ping.ogg")
@onready var sfx_kill:AudioStream = preload("res://Assets/Sounds/Sfx/EnemyKilled1.ogg")
@onready var sfx_hit1:AudioStream = preload("res://Assets/Sounds/Sfx/Explode1.ogg")
@onready var sfx_hit2:AudioStream = preload("res://Assets/Sounds/Sfx/Explode2.ogg")
@onready var sfx_hit3:AudioStream = preload("res://Assets/Sounds/Sfx/Explode3.ogg")
@onready var sfx_hit4:AudioStream = preload("res://Assets/Sounds/Sfx/Explode4.ogg")
@onready var hit_sounds:Array = [ sfx_hit1, sfx_hit2, sfx_hit3, sfx_hit4 ]

var my_type:EnemyTypes
enum EnemyTypes {
	SPIKEY_COMMON,     # Blue spikey
	SPIKEY_TOUGH,      # Orange spikey
	SPIKEY_ABSURD,     # Pink spikey
	BABYFISH_1,        # Green babyfish
	BABYFISH_2,        # Pink babyfish
	FLOATSPIKE_COMMON, # Black floatspike
	FLOATSPIKE_TOUGH,  # Blue floatspike
	BLOB_COMMON,       # Blob
	BLOB_TOUGH,        # Blub
	BLOB_ANGEL,        # Angelblob
	BLOB_DEVIL,        # Devilblov
	CHIRPY_COMMON,     # Blue chirpy
	CHIRPY_TOUGH,      # Light-blue chirpy
	BATTYBAT,          # Batty bat
	FIREBALL,          # Fireball
	ICEBALL,           # Iceball
	GHOSTBALL,         # Ghost dandelion
	SNELK,             # Secret snelk
	KITTY_COMMON,      # Gray kitty
	KITTY_TOUGH,       # Orange kitty
	CANON,             # Canon (red)
	NONCANON,          # Non-canon (blue)
	SNAKEY_COMMON,     # Green snakey
	SNAKEY_TOUGH,      # Blue snakey
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
	PINCER,            # Pincer, sky pincer, and pouncer
	GEAR_COMMON,       # Gray spinnygear
	GEAR_TOUGH,        # Red spinnygear
	DRONE,             # Federation drone
	BALLOON,           # Balloon buster
}
#endregion


func spawn(active:bool = true) -> void:
	origin = position
	health = max_health
	ai_active = active
	
	if hitbox:
		hitbox.connect("area_entered", _on_bullet_entered)
		hitbox.connect("area_exited", _on_bullet_exited)
		hitbox.connect("body_entered", _on_player_entered)
		hitbox.connect("body_exited", _on_player_entered)


func _process(delta) -> void:
	if intersecting_player and not GameCore.instance.player.stunned and can_damage and ai_active:
		var can_hit = true
		match my_element:
			ElementTypes.ICE:
				can_hit = not Statics.has_shell(1)
			ElementTypes.FIRE:
				can_hit = not Statics.has_shell(3)
		if can_hit:
			GameCore.instance.player.adjust_health(-attack)
		
	if not stun_invul and Statics.is_box_on_screen(col, position) and not invulnerable:
		var pbullets_to_despawn:Array = []
		var ebullets_to_despawn:Array = []
		var kill_flag:bool = false
		var max_damage:int = parry_damage
		for bullet in intersecting_bullets:
			var this_damage = bullet.damage
			#gravity shock critical damage mult (1.35)
			if not immunities.has(bullet.type) and bullet.damage - defense > 0:
				this_damage = floori(bullet.damage - defense)
				if weaknesses.has(bullet.type):
					this_damage *= 2
				if resistances.has(bullet.type):
					this_damage = floori(this_damage * 0.5)
				if this_damage > max_damage:
					max_damage = this_damage
			else:
				if make_sound_on_ping:
					Statics.play_sfx_disconnected(sfx_ping)
				ping_player -= 1
			if not can_be_pierced or bullet.single_hit:
				pbullets_to_despawn.append(bullet)
#		foreach (EnemyBullet bullet in intersectingEnemyBullets)
#		{
#			if (bullet.hasBeenParried)
#			{
#				if (bullet.damage - defense > 0)
#				{
#					int thisDamage = Mathf.FloorToInt(bullet.damage - defense);
#					if (thisDamage > maxDamage)
#						maxDamage = thisDamage;
#				}
#				else
#				{
#					if (!PlayState.armorPingPlayedThisFrame && makeSoundOnPing)
#					{
#						PlayState.armorPingPlayedThisFrame = true;
#						PlayState.PlaySound("Ping");
#					}
#					pingPlayer -= 1;
#				}
#				if (!letsPermeatingShotsBy || bullet.bulletType == EnemyBullet.BulletType.pea || !bullet.isActive)
#					enemyBulletsToDespawn.Add(bullet);
#			}
#		}
		if max_damage > 0 and not shield_entity:
			if health <= 0:
				kill_flag = true
			else:
				_damage(max_damage)
		parry_damage = 0
		for bullet in pbullets_to_despawn:
			bullet.queue_free()
		for bullet in ebullets_to_despawn:
			bullet.queue_free()
		pbullets_to_despawn.clear()
		ebullets_to_despawn.clear()
		if kill_flag:
			kill()
	if damage_timeout > 0.0:
		damage_timeout -= delta


func _on_player_entered(_body) -> void:
	intersecting_player = true

func _on_bullet_entered(_area) -> void:
	var bullet = _area.get_parent()
	if bullet is PlayerBullet:
		intersecting_bullets.append(bullet)
	#elif _area is EnemyBullet:


func _on_player_exited(_body) -> void:
	intersecting_player = false

func _on_bullet_exited(_area) -> void:
	var bullet = _area.get_parent()
	if bullet is PlayerBullet:
		intersecting_bullets.remove_at(intersecting_bullets.find(bullet))


func _damage(health_lost:int, sound:bool = true) -> void:
	if damage_timeout > 0:
		return
	if sound:
		Statics.play_sfx_disconnected(hit_sounds[randi_range(0, 3)])
	health -= health_lost


func kill() -> void:
	Statics.play_sfx_disconnected(sfx_kill)
	for i in range(kill_particle_count):
		var range = kill_particle_range
		var pos = Vector2(randi_range(-range.x, range.x), randi_range(-range.y, range.y))
		var part = kill_particle_types[randi() % kill_particle_types.size()]
		Statics.spawn_particle(part, Room.Layers.GROUND, position + pos)
	if Statics.current_profile["character"] == Player.Players.LEECHY:
		pass #SpawnHealthOrbs
	queue_free()


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
