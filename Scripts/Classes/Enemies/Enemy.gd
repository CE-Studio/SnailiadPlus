@icon("res://Editor/ico/Enemy.svg")
class_name Enemy
extends Node2D


#region Vaariables
var health:int
var max_health:int
var attack:int
var defense:int
var weaknesses:Array[int] = []  # Enemies take double damage from bullet types in this list
var resistances:Array[int] = [] # Enemies take half damage from bullet types in this list
var immunities:Array[int] = []  # Enemies resist all damage from bullet types in this list
var lets_permeating_shots_by:bool
var stun_invul:bool = false
var make_sound_on_ping:bool = true
var invulnerable:bool = false
var can_damage:bool = true
var shield_entity:bool = false
var parry_damage:int = 0
var health_orb_value:int = 0

enum ElementTypes {
	ICE,
	FIRE,
	NONE = -1
}
var my_element:ElementTypes = ElementTypes.NONE

#public Collider2D col;
#public Rigidbody2D rb;
#public SpriteRenderer sprite;
#public AnimationModule anim;
#public SpriteMask mask;

var col:CollisionShape2D
var hitbox:Area2D
var sprite:JsonSprite2D

var ping_player:int = 0

var spawn_conditions:Array[float] = []
var origin:Vector2
var intersecting_player:bool = false
var intersecting_bullets:Array[PlayerBullet] = []
var intersecting_enemy_bullets:Array = [] #TODO: mark this as EnemyBullet array when class implemented
var kill_particles:Array[String] = [ "ExplosionBig" ]

@onready var sfx_ping:AudioStream = preload("res://Assets/Sounds/Sfx/Ping.ogg")
@onready var sfx_kill:AudioStream = preload("res://Assets/Sounds/Sfx/EnemyKilled1.ogg")
#endregion


func spawn(hp:int, atk:int, def:int, piercable:bool, orb_value:int, wea:Array[int] = [], res:Array[int] = [], imm:Array[int] = []) -> void:
	origin = position
	health = hp
	max_health = hp
	attack = atk
	defense = def
	weaknesses = wea.duplicate()
	resistances = res.duplicate()
	immunities = imm.duplicate()
	lets_permeating_shots_by = piercable
	health_orb_value = orb_value


func _process(delta) -> void:
	if intersecting_player and not GameCore.instance.player.stunned and can_damage:
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
				if not lets_permeating_shots_by or bullet.single_hit:
					pbullets_to_despawn.append(bullet)
#			foreach (EnemyBullet bullet in intersectingEnemyBullets)
#			{
#				if (bullet.hasBeenParried)
#				{
#					if (bullet.damage - defense > 0)
#					{
#						int thisDamage = Mathf.FloorToInt(bullet.damage - defense);
#						if (thisDamage > maxDamage)
#							maxDamage = thisDamage;
#					}
#					else
#					{
#						if (!PlayState.armorPingPlayedThisFrame && makeSoundOnPing)
#						{
#							PlayState.armorPingPlayedThisFrame = true;
#							PlayState.PlaySound("Ping");
#						}
#						pingPlayer -= 1;
#					}
#					if (!letsPermeatingShotsBy || bullet.bulletType == EnemyBullet.BulletType.pea || !bullet.isActive)
#						enemyBulletsToDespawn.Add(bullet);
#				}
#			}
			if max_damage > 0 and not shield_entity:
				health -= max_damage
				if health <= 0:
					kill_flag = true
				else:
					pass #flash
			parry_damage = 0
			for bullet in pbullets_to_despawn:
				bullet.queue_free()
			for bullet in ebullets_to_despawn:
				bullet.queue_free()
			pbullets_to_despawn.clear()
			ebullets_to_despawn.clear()
			if kill_flag:
				pass #kill


func _on_player_entered(_body) -> void:
	intersecting_player = true

func _on_bullet_entered(_area) -> void:
	if _area is PlayerBullet:
		intersecting_bullets.append(_area.get_parent())
	#elif _area is EnemyBullet:


func _on_player_exited(_body) -> void:
	intersecting_player = false

func _on_bullet_exited(_area) -> void:
	if _area is PlayerBullet:
		intersecting_bullets.remove_at(intersecting_bullets.find(_area))


#	public virtual IEnumerator Flash(bool playSound = true)
#	{
#		mask.enabled = true;
#		stunInvulnerability = true;
#		if (playSound)
#			PlayState.PlaySound("Explode" + Random.Range(1, 5));
#		yield return new WaitForFixedUpdate();
#		mask.enabled = false;
#		yield return new WaitForFixedUpdate();
#		stunInvulnerability = false;
#	}


func kill() -> void:
	Statics.play_sfx_disconnected(sfx_kill)
	for i in range(4):
		var pos = Vector2(randi_range(-16, 16), randi_range(-16, 16))
		var part = kill_particles[randi() % kill_particles.size()]
		Statics.spawn_particle("ExplosionBig", Room.Layers.GROUND, position + pos)
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
