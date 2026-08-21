# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name Stompy
extends Boss


#region Variables
const RAISED_Y:float = -109.0
const STOMP_Y:float = 111.0
const STOMP_VEL:float = 10.0
const MIN_DIST:float = 36.0
const SEC_PER_TICK:float = 0.01
const MIN_EYE_Y:float = -21.0
const SYNC_MODE_TIMEOUT:float = 3.0
const STEP_MODE_TIMEOUT:float = 9.0
const WAIT_RAISE_TIMEOUT:int = 50
const SYNC_STOMP_TIMEOUT:int = 10
const STEP_RADIUS:float = 40.0
const RADIUS_Y_MULT:float = 3.4
const NO_ORIGIN:int = -100000000
const TIMEOUTS:Array = [
	0.60153, 0.48509, 0.70037, 0.66276, 0.70802, 0.79541, 0.62043, 0.5796,  0.99605, 0.15058,
	0.72121, 0.86851, 0.64371, 0.76708, 0.89401, 0.52828, 0.72309, 0.15963, 0.15116, 0.1799,
	0.27829, 0.40878, 0.92538, 0.45074, 0.18865, 0.59797, 0.4318,  0.94098, 0.23463, 0.29221,
	0.59734, 0.34877, 0.81676, 0.57617, 0.14883, 0.16094, 0.14123, 0.57931, 0.85924, 0.22828,
	0.63834, 0.10387, 0.54746, 0.24897, 0.11105, 0.49748, 0.54746, 0.19405, 0.79792, 0.36023,
	0.53726, 0.78544, 0.60425, 0.83512, 0.01696, 0.10451, 0.01513, 0.78678, 0.51617, 0.24251
]
const OFFSET_FOOT_L:Vector2 = Vector2(-125, 0)
const OFFSET_FOOT_R:Vector2 = Vector2(125, 0)
const OFFSET_EYE_L:Vector2 = Vector2(52, -83)
const OFFSET_EYE_R:Vector2 = Vector2(-52, -83)
const SHAKE_TIMELINE:Array[float] = [5.0, 0.7]

enum BossMode {
	INTRO,
	MOVE_STOMP,
	HUNT,
	SYNC,
	STEP
}
enum FootMode {
	NONE,
	STOMP,
	WAIT_RAISE,
	RAISE,
	MOVE,
	SYNC,
	STEP,
	STEP_NOW,
	STEP_WAIT
}

var intro_step:int = 0
var intro_from_left:bool = false
var legacy_intro:bool = false
var set_intro:bool = false
var attack_mode:int = 0
var boss_speed:float = 0.6
var elapsed:float = 0.0
var boss_mode:BossMode = BossMode.INTRO
var next_step_is_left:bool = false
var step_dir_is_left:bool = false
var step_mode_timeout:float = 0.0
var cannons:Array[Enemy] = []

var pos_l:Vector2 = Vector2.ZERO
var mode_l:FootMode = FootMode.NONE
var vel_l:Vector2 = Vector2.ZERO
var target_l:Vector2 = Vector2.ZERO
var step_origin_l:Vector2 = Vector2.ZERO
var theta_l:float = 0.0
var step_theta_l:float = 0.0
var stomp_timeout_l:int = 0
var stomp_timeout_index_l:int = 23
var raise_timeout_l:int = 0
var played_fall_on_step_l:bool = false

var pos_r:Vector2 = Vector2.ZERO
var mode_r:FootMode = FootMode.NONE
var vel_r:Vector2 = Vector2.ZERO
var target_r:Vector2 = Vector2.ZERO
var step_origin_r:Vector2 = Vector2.ZERO
var theta_r:float = 0.0
var step_theta_r:float = 0.0
var stomp_timeout_r:int = 0
var stomp_timeout_index_r:int = 34
var raise_timeout_r:int = 0
var played_fall_on_step_r:bool = false

@onready var foot_l:StompyFoot = $"FootL"
@onready var foot_r:StompyFoot = $"FootR"
@onready var eye_l:StompyEye = $"EyeL"
@onready var eye_r:StompyEye = $"EyeR"
@onready var sfx_stomp:AudioStreamPlayer = $"Stomp"
@onready var debug_states:RichTextLabel = $"DebugFootState"
#endregion


func _ready() -> void:
	if Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS2) == true and not display_mode:
		queue_free()
		return
	
	my_type = EnemyTypes.STOMPY
	super.spawn()
	
	foot_l.boss = self
	foot_l.display_mode = display_mode
	foot_r.boss = self
	foot_r.display_mode = display_mode
	eye_l.boss = self
	eye_l.my_foot = foot_l
	eye_l.display_mode = display_mode
	eye_r.boss = self
	eye_r.my_foot = foot_r
	eye_r.display_mode = display_mode
	
	nodes_to_wiggle.append(foot_l.sprite)
	nodes_to_wiggle.append(foot_r.sprite)
	nodes_to_wiggle.append(eye_l.spr_group)
	nodes_to_wiggle.append(eye_r.spr_group)
	
	if hard_mode:
		boss_speed = 1.0
	
	if display_mode:
		z_index = 0
		foot_l.position = Vector2i(-101, 76)
		foot_r.position = Vector2i(101, 76)
		eye_l.position = Vector2i(-48, 0)
		eye_r.position = Vector2i(48, 0)
	else:
		pos_l.y = RAISED_Y
		pos_r.y = RAISED_Y
		_tick_parts()
		_spawn_cannons.call_deferred()
		foot_l.can_damage = false
		foot_r.can_damage = false


func intro_step_left() -> void:
	mode_l = FootMode.STOMP


func intro_step_right() -> void:
	mode_r = FootMode.STOMP


func intro_step_bar() -> void:
	if not Statics.is_in_boss_rush:
		GameCore.instance.music_manager.play_song(battle_music)
	health_bar = UICore.instance.show_boss_bar(self)


func _process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	if not set_intro:
		intro_from_left = GameCore.instance.player.position.x < position.x
		set_intro = true
	
	elapsed += delta * boss_speed
	while elapsed > SEC_PER_TICK:
		elapsed -= SEC_PER_TICK
		_tick_parts()
	var player_pos:Vector2 = GameCore.instance.player.position
	if intro_delay:
	#region Intro
		if legacy_intro:
			match intro_step:
				0:
					if intro_from_left and player_pos.x > position.x + 73.0:
						intro_step_right()
						if player_pos.x > position.x + 93.0:
							intro_step += 1
					elif not intro_from_left and player_pos.x < position.x - 73.0:
						intro_step_left()
						if player_pos.x < position.x - 93.0:
							intro_step += 1
				1:
					var mark_complete:bool = false
					if intro_from_left and player_pos.x < position.x + 30.0:
						intro_step_left()
						if player_pos.x < position.x + 38.0:
							mark_complete = true
					elif not intro_from_left and player_pos.x > position.x - 30.0:
						intro_step_right()
						if player_pos.x > position.x - 38.0:
							mark_complete = true
					if mark_complete:
						intro_step_bar()
						intro_step += 1
				_:
					pass
	#endregion
	
	else:
		match boss_mode:
			BossMode.INTRO:
				if not intro_delay:
					foot_l.can_damage = true
					foot_r.can_damage = true
					boss_mode = BossMode.MOVE_STOMP
					step_mode_timeout = STEP_MODE_TIMEOUT
			
			BossMode.MOVE_STOMP:
				step_mode_timeout -= delta * boss_speed
				if step_mode_timeout <= 0.0:
					boss_mode = BossMode.HUNT
			
			BossMode.HUNT:
				if pos_r.x - pos_l.x <= MIN_DIST + 2:
					step_mode_timeout = SYNC_MODE_TIMEOUT
					boss_mode = BossMode.SYNC
			
			BossMode.SYNC:
				if mode_l == FootMode.MOVE and mode_r == FootMode.MOVE:
					if abs(player_pos.x - pos_l.x) < abs(player_pos.x - pos_r.x):
						mode_l = FootMode.STOMP
						vel_l.y = STOMP_VEL
					else:
						mode_r = FootMode.STOMP
						vel_r.y = STOMP_VEL
				elif mode_l == FootMode.MOVE and mode_r == FootMode.RAISE:
					mode_l = FootMode.STOMP
					vel_l.y = STOMP_VEL
				elif mode_l == FootMode.RAISE and mode_r == FootMode.MOVE:
					mode_r = FootMode.STOMP
					vel_r.y = STOMP_VEL
				step_mode_timeout -= delta * boss_speed
				if step_mode_timeout <= 0.0:
					step_mode_timeout = STEP_MODE_TIMEOUT
					boss_mode = BossMode.STEP
			
			BossMode.STEP:
				if mode_l == FootMode.MOVE:
					mode_l = FootMode.STOMP
					vel_l.y = STOMP_VEL
				if mode_r == FootMode.MOVE:
					mode_r = FootMode.STOMP
					vel_r.y = STOMP_VEL
				step_mode_timeout -= delta * boss_speed
				if step_mode_timeout <= 0.0:
					step_mode_timeout = STEP_MODE_TIMEOUT
					boss_mode = BossMode.MOVE_STOMP
					mode_l = FootMode.RAISE
					mode_r = FootMode.RAISE
					raise_timeout_l = 0
					raise_timeout_r = 0
					stomp_timeout_l = WAIT_RAISE_TIMEOUT
					stomp_timeout_r = WAIT_RAISE_TIMEOUT
	
	if debug_states.visible:
		debug_states.text = "Main - %s\nLeft - %s\nRight - %s" % [
			BossMode.keys()[boss_mode],
			FootMode.keys()[mode_l],
			FootMode.keys()[mode_r]
		]


func _tick_parts() -> void:
	var player_pos:Vector2 = GameCore.instance.player.position
	
	#region Stomp left
	if mode_l == FootMode.STOMP:
		vel_l.y += 0.2
		pos_l.y += vel_l.y
		if pos_l.y > STOMP_Y:
			pos_l.y = STOMP_Y
			mode_l = FootMode.STEP if boss_mode == BossMode.STEP else FootMode.WAIT_RAISE
			vel_l.y = 0.0
			raise_timeout_l = WAIT_RAISE_TIMEOUT
			stomp_timeout_l = 1000000
			_shake()
			foot_l.play_phase_anim(phase, "down")
	
	elif boss_mode != BossMode.INTRO and mode_l == FootMode.WAIT_RAISE:
		raise_timeout_l -= 1
		if raise_timeout_l <= 0:
			mode_l = FootMode.STEP if mode_l == FootMode.STEP else FootMode.RAISE
		if mode_l == FootMode.RAISE:
			foot_l.play_phase_anim(phase, "raise")
	
	if boss_mode != BossMode.INTRO and mode_l == FootMode.RAISE:
		vel_l.y -= 0.2
		pos_l.y += vel_l.y
		if pos_l.y < RAISED_Y:
			pos_l.y = RAISED_Y
			mode_l = FootMode.MOVE
			eye_l.can_attack = true
			raise_timeout_l = 1000000
			stomp_timeout_index_l = (stomp_timeout_index_l + 1) % TIMEOUTS.size()
			stomp_timeout_l = int(TIMEOUTS[stomp_timeout_index_l] * 360) + 60
			if boss_mode == BossMode.SYNC:
				stomp_timeout_l = SYNC_STOMP_TIMEOUT
			vel_l.y = 0.0
			foot_l.play_phase_anim(phase, "up")
	#endregion
	
	#region Stomp right
	if mode_r == FootMode.STOMP:
		vel_r.y += 0.2
		pos_r.y += vel_r.y
		if pos_r.y > STOMP_Y:
			pos_r.y = STOMP_Y
			mode_r = FootMode.STEP if boss_mode == BossMode.STEP else FootMode.WAIT_RAISE
			vel_r.y = 0.0
			raise_timeout_r = WAIT_RAISE_TIMEOUT
			stomp_timeout_r = 1000000
			_shake()
			foot_r.play_phase_anim(phase, "down")
	
	elif boss_mode != BossMode.INTRO and mode_r == FootMode.WAIT_RAISE: 
		raise_timeout_r -= 1
		if raise_timeout_r <= 0:
			mode_r = FootMode.STEP if mode_r == FootMode.STEP else FootMode.RAISE
		if mode_r == FootMode.RAISE:
			foot_r.play_phase_anim(phase, "raise")
	
	if boss_mode != BossMode.INTRO and mode_r == FootMode.RAISE:
		vel_r.y -= 0.2
		pos_r.y += vel_r.y
		if pos_r.y < RAISED_Y:
			pos_r.y = RAISED_Y
			mode_r = FootMode.MOVE
			eye_r.can_attack = true
			raise_timeout_r = 1000000
			stomp_timeout_index_l = (stomp_timeout_index_l + 1) % TIMEOUTS.size()
			stomp_timeout_r = int(TIMEOUTS[stomp_timeout_index_l] * 360) + 60
			if boss_mode == BossMode.SYNC:
				stomp_timeout_r = SYNC_STOMP_TIMEOUT
			vel_r.y = 0.0
			foot_r.play_phase_anim(phase, "up")
	#endregion
	
	#region Move left
	if boss_mode != BossMode.INTRO and mode_l == FootMode.MOVE:
		theta_l += 0.2
		target_l.x = player_pos.x - position.x if boss_mode == BossMode.HUNT else sin(theta_l / 15) * 160
		if pos_r.x - target_l.x < MIN_DIST:
			target_l.x = pos_r.x - MIN_DIST
		if player_pos.x - position.x < -320.0:
			target_l.x = player_pos.x - position.x
		stomp_timeout_l -= 1
		vel_l.x = target_l.x - pos_l.x
		pos_l.x += vel_l.x * 0.1
		if stomp_timeout_l <= 0.0 and pos_l.y <= RAISED_Y + 10.0 and vel_l.y < 1.0:
			mode_l = FootMode.STOMP
			vel_l.y = STOMP_VEL
			eye_l.can_attack = false
			foot_l.play_phase_anim(phase, "fall")
	#endregion
	
	#region Move right
	if boss_mode != BossMode.INTRO and mode_r == FootMode.MOVE:
		theta_r += 0.2
		target_r.x = player_pos.x - position.x if boss_mode == BossMode.HUNT else sin(theta_r / 15 + PI / 3) * 160
		if target_r.x - pos_l.x < MIN_DIST:
			target_r.x = pos_l.x + MIN_DIST
		if player_pos.x - position.x > 302.0:
			target_r.x = player_pos.x - position.x
		stomp_timeout_r -= 1
		vel_r.x = target_r.x - pos_r.x
		pos_r.x += vel_r.x * 0.1
		if stomp_timeout_r <= 0.0 and pos_r.y <= RAISED_Y + 10.0 and vel_r.y < 1.0:
			mode_r = FootMode.STOMP
			vel_r.y = STOMP_VEL
			eye_r.can_attack = false
			foot_r.play_phase_anim(phase, "fall")
	#endregion
	
	#region Step both
	if mode_l == FootMode.STEP and mode_r == FootMode.STEP:
		theta_l = 0.0
		theta_r = 0.0
		if pos_l.x < position.x:
			step_dir_is_left = true
			mode_l = FootMode.STEP_NOW
			mode_r = FootMode.STEP_WAIT
			played_fall_on_step_l = false
			step_origin_l = Vector2(NO_ORIGIN, pos_l.y)
		else:
			step_dir_is_left = false
			mode_l = FootMode.STEP_WAIT
			mode_r = FootMode.STEP_NOW
			played_fall_on_step_r = false
			step_origin_r = Vector2(NO_ORIGIN, pos_r.y)
	#endregion
	
	#region Step now left
	if mode_l == FootMode.STEP_NOW:
		if step_dir_is_left and theta_l == 0.0 and pos_l.x < -165.0:
			step_dir_is_left = false
			mode_l = FootMode.STEP_WAIT
			mode_r = FootMode.STEP_NOW
			theta_r = 0
			played_fall_on_step_r = false
			step_origin_r = Vector2(NO_ORIGIN, pos_r.y)
		else:
			theta_l += 0.05
			if theta_l >= PI * 0.5 and not played_fall_on_step_l:
				played_fall_on_step_l = true
				foot_l.play_phase_anim(phase, "fall")
			if theta_l >= PI:
				theta_l = PI
				_shake()
				foot_l.play_phase_anim(phase, "down")
				mode_l = FootMode.STEP_WAIT
				mode_r = FootMode.STEP_NOW
				foot_r.play_phase_anim(phase, "raise")
				theta_r = 0
				step_origin_r = Vector2(NO_ORIGIN, pos_r.y)
			if step_dir_is_left:
				if step_origin_l.x == NO_ORIGIN:
					step_origin_l.x = pos_l.x - STEP_RADIUS
				pos_l = Vector2(
					step_origin_l.x + cos(theta_l) * STEP_RADIUS,
					step_origin_l.y - sin(theta_l) * STEP_RADIUS * RADIUS_Y_MULT
				)
			else:
				if step_origin_l.x == NO_ORIGIN:
					step_origin_l.x = pos_l.x + STEP_RADIUS
				pos_l = Vector2(
					step_origin_l.x - cos(theta_l) * STEP_RADIUS,
					step_origin_l.y - sin(theta_l) * STEP_RADIUS * RADIUS_Y_MULT
				)
	#endregion
	
	#region Step now right
	elif mode_r == FootMode.STEP_NOW:
		if not step_dir_is_left and theta_r == 0.0 and pos_r.x > 165.0:
			step_dir_is_left = true
			mode_r = FootMode.STEP_WAIT
			mode_l = FootMode.STEP_NOW
			theta_l = 0
			played_fall_on_step_l = false
			step_origin_l = Vector2(NO_ORIGIN, pos_l.y)
		else:
			theta_r += 0.05
			if theta_r >= PI * 0.5 and not played_fall_on_step_r:
				played_fall_on_step_r = true
				foot_r.play_phase_anim(phase, "fall")
			if theta_r >= PI:
				theta_r = PI
				_shake()
				foot_r.play_phase_anim(phase, "down")
				mode_r = FootMode.STEP_WAIT
				mode_l = FootMode.STEP_NOW
				foot_l.play_phase_anim(phase, "raise")
				theta_l = 0
				step_origin_l = Vector2(NO_ORIGIN, pos_l.y)
			if step_dir_is_left:
				if step_origin_r.x == NO_ORIGIN:
					step_origin_r.x = pos_r.x - STEP_RADIUS
				pos_r = Vector2(
					step_origin_r.x + cos(theta_r) * STEP_RADIUS,
					step_origin_r.y - sin(theta_r) * STEP_RADIUS * RADIUS_Y_MULT
				)
			else:
				if step_origin_r.x == NO_ORIGIN:
					step_origin_r.x = pos_r.x + STEP_RADIUS
				pos_r = Vector2(
					step_origin_r.x - cos(theta_r) * STEP_RADIUS,
					step_origin_r.y - sin(theta_r) * STEP_RADIUS * RADIUS_Y_MULT
				)
	#endregion
	
	#region Final cleanup
	if pos_r.x - pos_l.x < MIN_DIST:
		var difference:float = MIN_DIST - (pos_r.x - pos_l.x)
		if mode_l == FootMode.MOVE and mode_r == FootMode.STOMP:
			pos_l.x -= difference
		elif mode_r == FootMode.MOVE and mode_l == FootMode.STOMP:
			pos_r.x += difference
		else:
			pos_l.x -= difference * 0.5
			pos_r.x += difference * 0.5
	foot_l.position = pos_l + OFFSET_FOOT_L
	foot_r.position = pos_r + OFFSET_FOOT_R
	eye_l.position = foot_l.position + OFFSET_EYE_L
	eye_r.position = foot_r.position + OFFSET_EYE_R
	if eye_l.position.y <= MIN_EYE_Y:
		eye_l.position.y = MIN_EYE_Y
		eye_l.invulnerable = true
	else:
		eye_l.invulnerable = not eye_l.open or boss_mode == BossMode.INTRO
	if eye_r.position.y <= MIN_EYE_Y:
		eye_r.position.y = MIN_EYE_Y
		eye_r.invulnerable = true
	else:
		eye_r.invulnerable = not eye_r.open or boss_mode == BossMode.INTRO
	#endregion


func play_phase_anim(anim_name:String = "", set_as_current:bool = true) -> String:
	super.play_phase_anim(anim_name, set_as_current)
	return anim_name


func advance_phase(count:int = 1) -> void:
	super(count)
	foot_l.update_phase()
	foot_r.update_phase()
	eye_l.update_phase()
	eye_r.update_phase()
	if phase == 1:
		boss_speed += 0.2
	elif phase == 2:
		boss_speed += 0.3


func _shake() -> void:
	sfx_stomp.play()
	UICore.instance.call_screen_shake_linear(SHAKE_TIMELINE, Vector2.DOWN, UICore.ShakeCallMode.OVERWRITE_ALL)


func _spawn_cannons() -> void:
	var y:float = 16.0
	var target_dir:Statics.DirsSurface = Statics.DirsSurface.CEILING
	var cannon:PackedScene = load("res://Scenes/Entities/Enemies/Canon.tscn")
	if Statics.current_profile["character"] == Player.Players.UPSIDE:
		y = 160.0
		target_dir = Statics.DirsSurface.FLOOR
	for x in [ -224.0, -128.0, 128.0, 224.0 ]:
		var new_cannon:Canon = cannon.instantiate()
		new_cannon.base_dir = target_dir
		new_cannon.position = position + Vector2(x, y)
		GameCore.instance.current_room.layer_ground.add_child(new_cannon)
		cannons.append(new_cannon)


func kill() -> void:
	if not in_death_anim:
		UICore.instance.achievement_core.check_add(AchievementCore.Achievements.BEAT_STOMPY)
		if health_bar:
			health_bar._toggle_outro_shake()
		foot_l.sprite.play("defeat_left")
		foot_r.sprite.play("defeat_right")
		eye_l.set_death_pose.call_deferred()
		eye_r.set_death_pose.call_deferred()
		for _cannon in cannons:
			_cannon.kill()
		Statics.spawn_particle("ExplosionBossDefeat", Room.Layers.GROUND, position + foot_l.position)
		Statics.spawn_particle("ExplosionBossDefeat", Room.Layers.GROUND, position + foot_r.position, [false])
		GameCore.instance.music_manager.stop_all(true)
		Statics.set_world_flag(Statics.WorldFlags.DEFEATED_BOSS2, true)
	else:
		GameCore.instance.music_manager.play_song(GameCore.instance.current_room.song_change)
	super()
