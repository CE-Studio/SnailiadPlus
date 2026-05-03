# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name Boss
extends Enemy


#region Variables
const DEATH_WIGGLE_RANGE:float = 2.0
const DEATH_WIGGLE_TIME:float = 1.8

@export var battle_music:MusicManager.Loops = MusicManager.Loops.BOSS1
@export var phase_changes:Array[float] = [ 0 ]
@export var death_wiggle_mult:float = 1.0

## Delays this boss' normal actions for as long as it's set, in order to properly run an intro cutscene
var intro_delay:bool = true
## What "phase," or stage of health, the boss is currently sitting at. Will control attack patterns and speed
var phase:int = 0
## The currently-running animation
var current_anim:String = ""
## Will be set if the boss is currently playing its defeat animation
var in_death_anim:bool = false
## List of nodes that need to randomly shake during the defeat animation
var nodes_to_wiggle:Array = []
## How long the boss will remain in its defeat animation before freeing itself
var death_timer:float = 0.0
## All bullets that have been fired by this boss
var bullets:Array = []

## The connected health bar that will read this boss' health
var health_bar:BossHealthBar = null
#endregion


func _process(delta: float) -> void:
	if display_mode:
		return
	
	super(delta)

	while phase < phase_changes.size() and health < max_health * phase_changes[phase]:
		advance_phase()

	if in_death_anim:
		for node in nodes_to_wiggle:
			node.position = Vector2(
				randf_range(-DEATH_WIGGLE_RANGE, DEATH_WIGGLE_RANGE),
				randf_range(-DEATH_WIGGLE_RANGE, DEATH_WIGGLE_RANGE)
			) * death_wiggle_mult
		if death_timer <= 0.0:
			kill()
		death_timer -= delta


## Damages the boss and updates the health bar
func _damage(health_lost:int, sound:bool = true, allow_kill:bool = false) -> void:
	super(health_lost, sound, allow_kill)
	if health_bar:
		health_bar.update()


## Advances the boss' phase by the required amount
func advance_phase(count:int = 1) -> void:
	phase += count
	play_phase_anim()


## Plays the given animation with special formatting dependent on the boss' current phase
func play_phase_anim(anim_name:String = "", set_as_current:bool = true) -> String:
	if not sprite:
		return ""
	sprite.action = get_phase_anim(anim_name)
	if set_as_current and anim_name.strip_edges() != "":
		current_anim = anim_name
	return anim_name


## Returns the given animation with special formatting dependent on the boss' current phase
func get_phase_anim(anim_name:String = "", prefix:String = "") -> String:
	if anim_name.strip_edges() == "":
		anim_name = current_anim
	return "p%d_%s" % [ phase, prefix + anim_name ]


## Frees the boss in spectacular fashion. Must be called once to start the defeat animation,
## and a second time to properly free the boss
func kill() -> void:
	if in_death_anim:
		super()
	else:
		in_death_anim = true
		ai_active = false
		can_damage = false
		invulnerable = true
		death_timer = DEATH_WIGGLE_TIME
		for bullet in bullets:
			if bullet != null:
				if bullet is EnemyBullet:
					bullet._despawn()
				else:
					bullet.queue_free()
