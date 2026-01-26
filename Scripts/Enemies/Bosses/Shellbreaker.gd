class_name Shellbreaker
extends Boss


#region Variables
var HAND_COUNT:int = 3
var SHOT_COUNT:Array[int] = [ 5, 14, 41 ]
var SHOT_DELAY:float = 0.8
const SHOT_DELAY_MULTS:Array[float] = [ 0.6, 0.28, 0.1 ]
const PATTERN_DELAY:float = 3.0
const WEAPON_SPEED:float = 270.0
const PATH_RADIUS:Vector2 = Vector2(144, 112)
const PATH_RADIUS_CYCLE_MULT:float = 0.4286
const PATTERN_COUNT:int = 4
const HAND_RADIUS_MIN:float = 50.0
const HAND_RADIUS_BASE:float = 40.0
const HAND_RADIUS_MOD:float = 90.0
const BLINK_TIMEOUT_MAX:float = 3.0

var hands:Array[Enemy] = []
var hand_thetas:Array[float] = []
var hand_theta_speeds:Array[float] = []
var hand_speed:float = 0.0
var hand_radius:float = 0.0
var hand_radius_mult:float = 1.0
var hand_radius_target:float = 1.0
var shot_pattern:int = 0
var shot_timeout:float = 0.0
var shot_pattern_timeout:float = 0.0
var shot_count:int = 0
var is_firing:bool = false
var blink_timeout:float = 0.0

@onready var eyes:JsonSprite2D = $"Eyes"
@onready var hand:PackedScene = preload("res://Scenes/Entities/Enemies/Bosses/ShellbreakerHand.tscn")
@onready var hand_group:Node2D = $"HandGroup"
@onready var boomerang:PackedScene = preload("res://Scenes/Entities/Bullets/Enemy/EnemyBulletBoomerangBlue.tscn")
#endregion


func _ready() -> void:
	if Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS1) == true and not display_mode:
		queue_free()
		return
	
	my_type = EnemyTypes.SHELLBREAKER
	col = $"BodyBox"
	sprite = $"Body"
	hitbox = $"Area2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	if display_mode:
		HAND_COUNT = 6
	elif hard_mode:
		HAND_COUNT *= 4
		SHOT_DELAY /= 3
		for i in SHOT_COUNT.size():
			SHOT_COUNT[i] *= 3
	call_deferred("play_phase_anim", "idle")
	for i in HAND_COUNT:
		var new_hand:Node2D = hand.instantiate()
		hands.append(new_hand)
		hand_thetas.append(0.0)
		hand_theta_speeds.append(2.5 + i * 0.75)
		hand_group.add_child(new_hand)
	blink_timeout = randf() * BLINK_TIMEOUT_MAX
	
	if display_mode:
		z_index = 0
		for _hand in hands:
			_hand.z_index = 0
	else:
		if not Statics.is_in_boss_rush:
			GameCore.instance.music_manager.play_song(battle_music)
		health_bar = UICore.instance.show_boss_bar(self)
	
	nodes_to_wiggle.append(sprite)
	nodes_to_wiggle.append(eyes)
	
	eyes.material = sprite.material


func _process(delta: float) -> void:
	super(delta)
	if not Engine.is_editor_hint():
		eyes.material.set("shader_parameter/flash_color", Color.BLACK + flash_color)
	
	if not is_firing and not in_death_anim:
		blink_timeout -= delta
	if blink_timeout <= 0.0:
		blink_timeout = randf() * BLINK_TIMEOUT_MAX
		play_phase_anim("blink", false)
		
	if not ai_active and not display_mode:
		return
	
	lifetime += delta
	shot_timeout -= delta
	shot_pattern_timeout -= delta
	
	if display_mode:
		for i in hands.size():
			var this_theta:float = hand_thetas[0] + 2 * (PI / 6 * i)
			hands[i].position = Vector2(
				cos(this_theta), sin(this_theta)
			) * 48
		hand_thetas[0] += delta * 4.0
		return
	
	position = origin + (PATH_RADIUS * Vector2(
		cos(lifetime),
		sin(lifetime)
	) * sin(lifetime * PATH_RADIUS_CYCLE_MULT))
	var eye_pos_val:float = get_aim_dir()
	eyes.position = Vector2(
		cos(eye_pos_val),
		sin(eye_pos_val)
	) * -2.5
	
	try_shoot()
	
	hand_radius = HAND_RADIUS_BASE + (HAND_RADIUS_MOD * sin(sin(lifetime * 5 / 3)))
	hand_radius = clampf(hand_radius, HAND_RADIUS_MIN, INF) * hand_radius_mult
	hand_radius_mult = hand_radius_mult * 0.9 + hand_radius_target * 0.1
	for i in hands.size():
		hand_thetas[i] += hand_theta_speeds[i] * delta * (1.0 + sin(lifetime * 5 / 4)) * 1.2
		hands[i].position = Vector2(
			-sin(hand_thetas[i]),
			cos(hand_thetas[i])
		) * hand_radius
		hands[i].invulnerable = hand_radius_target == 0.0


func try_shoot() -> void:
	if intro_delay:
		return
	var aim_dir:float = get_aim_dir()
	if shot_pattern_timeout <= 0:
		if not is_firing:
			is_firing = true
			play_phase_anim("shoot_start")
		hand_radius_target = 0.0
		if shot_timeout <= 0.0:
			shot_timeout = SHOT_DELAY * SHOT_DELAY_MULTS[phase]
			shot_count += 1
			match shot_pattern:
				0:
					shoot(aim_dir)
				1:
					shoot(aim_dir + (PI / SHOT_COUNT[phase] - PI * 0.5) * shot_count / 12)
				2:
					shoot(aim_dir + PI)
				3:
					shoot(aim_dir - (PI / SHOT_COUNT[phase] - PI * 0.5) * shot_count / 12)
			if shot_count >= SHOT_COUNT[phase]:
				shot_pattern = (shot_pattern + 1) % PATTERN_COUNT
				shot_pattern_timeout = PATTERN_DELAY
				hand_radius_target = 1.0
				shot_count = 0
				is_firing = false
				play_phase_anim("shoot_end")


func shoot(angle:float) -> void:
	var direction:Vector2 = Vector2(cos(angle), sin(angle))
	bullets.append(_shoot(boomerang, direction, WEAPON_SPEED))


func play_phase_anim(anim_name:String = "", set_as_current:bool = true) -> String:
	super.play_phase_anim(anim_name, set_as_current)
	eyes.action = get_phase_anim(anim_name, "eyes_")
	return anim_name


func advance_phase(count:int = 1) -> void:
	super(count)


func get_aim_dir() -> float:
	return atan2(
		position.y - GameCore.instance.player.position.y,
		position.x - GameCore.instance.player.position.x
	)


func kill() -> void:
	if not in_death_anim:
		UICore.instance.achievement_core.check_add(AchievementCore.Achievements.BEAT_SHELLBREAKER)
		if health_bar:
			health_bar._toggle_outro_shake()
		sprite.action = "defeat"
		eyes.action = "eyes_defeat"
		for _hand in hands:
			_hand.kill()
		hands.clear()
		Statics.spawn_particle("ExplosionBossDefeat", Room.Layers.GROUND, position)
		GameCore.instance.music_manager.stop_all(true)
		Statics.set_world_flag(Statics.WorldFlags.DEFEATED_BOSS1, true)
	else:
		GameCore.instance.music_manager.play_song(GameCore.instance.current_room.song_change)
	super()
