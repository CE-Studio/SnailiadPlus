# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
@tool
class_name Moonsnail
extends Boss


#region Variables
const RING_COUNT:int = 3
const RING_TIMEOUT:float = 1.7
const JUMP_LENGTH:float = 0.3
const TELEPORT_TIME:float = 1.4
const ATTACK_STOP_TIMEOUT:float = 0.9
const ATTACK_START_TIMEOUT:float = 0.45
const SHADOW_BALL_RADIUS:float = 80.0
const SHADOW_BALL_COUNT:int = 5
const DECISION_TABLE:Array[float] = [
	0.1640168826, 0.3892556902, 0.0336081053, 0.2246864975, 0.5434009453, 0.4227320437, 0.1017472328, 0.2041907897, 0.9950191347, 0.3634705228,
	0.0779175897, 0.384822732,  0.3284047846, 0.0951552057, 0.1941055446, 0.496359046,  0.2428007567, 0.8280672868, 0.852732986,  0.6928913176,
	0.2023843678, 0.7280045905, 0.4311591744, 0.796788024,  0.41191487,   0.7108575032, 0.1134556829, 0.6883870615, 0.8149317527, 0.8392490375,
	0.3647662453, 0.3487805783, 0.7900575239, 0.1670561498, 0.9810836953, 0.0097847681, 0.2244645569, 0.0842442402, 0.3263779227, 0.1481701068,
	0.6538572663, 0.2544128409, 0.1991950422, 0.541057099,  0.574700257,  0.5926224371, 0.310134571,  0.6104650203, 0.3545506087, 0.2313309166,
	0.3070387696, 0.0790505658, 0.9804949607, 0.7704714904, 0.7152660213, 0.8215058975, 0.9426850446, 0.7483973576, 0.7602092802, 0.881605898,
	0.5136580468, 0.0190696615, 0.28759162,   0.1565554394, 0.3664312259, 0.2586407176, 0.3185483313, 0.9837348993, 0.3330417452, 0.2801789805,
	0.3288621592, 0.0230039287, 0.303914672,  0.7212895333, 0.6296904139, 0.8659332532, 0.1715852607, 0.3900271956, 0.2824020982, 0.1624092775,
	0.7599701669, 0.6952292831, 0.2161165745, 0.9005386635, 0.3707154895, 0.6392742953, 0.452149187,  0.5595775233, 0.686286675,  0.7266258821,
	0.6904605229, 0.6808205255, 0.6856147591, 0.299675182,  0.8012191872, 0.804475971,  0.1926201715, 0.8868517061, 0.8347136807, 0.1512707539
]
const ACTION_TIMEOUT:float = 0.7
const JUMP_POWER:float = 428.0
const RUN_SPEED:float = 170.0
const MAX_SPEED:float = 600.0
const GRAVITY:float = 1200.0
const WEAPON_COOLDOWNS:Array[float] = [ 0.1, 0.3, 0.155 ]
const WEAPON_SPEED:Array[float] = [ 280.0, 330.0, 140.0 ]
const MOVE_SELECT_THRESHOLD:float = 60.0
const MOVE_END_THRESHOLD:float = 10.0
const MOVE_APPROACH_THRESHOLD:float = 40.0
const PROX_TELE_THRESHOLD:float = 60.0
const SIGMOID_MOD:float = 8.0
const DEATH_TIME:float = 4.5
const DEATH_MOVE_TIME:float = 3.25
const DEATH_FADE_START:float = 2.5
const DEATH_FADE_END:float = 4.5
const DEATH_FADE_COLOR:Color = Color("00c2f7")

enum BossMode {
	INTRO,
	ATTACK,
	MOVE,
	TELEPORT,
	STRAFE,
}

var mode:BossMode = BossMode.INTRO
var mode_elapsed:float = 0.0
var mode_initialized:bool = false
var ring_timeout:float = 0.0
var jumping:bool = false
var jump_release_timeout:float = 0.0
var gravity_jump_dir:Statics.DirsSurface = Statics.DirsSurface.NONE
var boss_speed:float = 1.0
var action_timeout:float = 0.0
var move_start:Vector2 = Vector2.ZERO
var move_end:Vector2 = Vector2.ZERO
var tele_start:Vector2 = Vector2.ZERO
var tele_end:Vector2 = Vector2.ZERO
var attacking:bool = false
var attack_start_timeout:float = 0.0
var attack_stop_timeout:float = 0.9
var weapon_cooldown:float = 0.0
var facing_left:bool = false
var gravity:Statics.DirsSurface = Statics.DirsSurface.FLOOR
var target_gravity:Statics.DirsSurface = Statics.DirsSurface.NONE
var decision_table_index:int = 0
var shadowballs:Array[JsonSprite2D] = []
var current_weapon:int = 2
var just_hit_surface:bool = false
var just_grav_jumped:bool = false
var fall_frames:int = 0
var last_anim:String = ""
var intro_done:bool = false
var death_start:Vector2 = Vector2.ZERO
var death_boom:Particle = null

var frames_left:int = -1
var frames_right:int = -1
var frames_down:int = -1
var frames_up:int = -1
var frames_jump:int = -1
var tapped_left:bool = false
var tapped_right:bool = false
var tapped_down:bool = false
var tapped_up:bool = false
var most_recent_dir:Statics.DirsCardinal = Statics.DirsCardinal.NONE
var most_recent_horiz:Statics.DirsCardinal = Statics.DirsCardinal.NONE
var most_recent_vert:Statics.DirsCardinal = Statics.DirsCardinal.NONE

@export var move_targets:Array[EntityTarget] = []
@export var tele_targets:Array[EntityTarget] = []
@export var stomp_targets:Array[EntityTarget] = []
@export var giga_spawn_pos:Vector2i = Vector2i.ZERO

@onready var sfx_jump = $"AudioGroup/Jump"
@onready var sfx_jumpgrav = $"AudioGroup/JumpGrav"
@onready var sfx_shell = $"AudioGroup/Shell"
@onready var sfx_hurt = $"AudioGroup/Hurt"
@onready var sfx_parry = $"AudioGroup/Parry"
@onready var sfx_death = $"AudioGroup/Die"
@onready var sfx_shockcharge = $"AudioGroup/ShockCharge"
@onready var sfx_shocklaunch = $"AudioGroup/ShockLaunch"
@onready var sfx_shockland = $"AudioGroup/ShockLand"
@onready var sfx_teleport = $"AudioGroup/Teleport"
@onready var shadowball_group:Node2D = $"ShadowballGroup"
@onready var cast_group:Node2D = $"CastGroup"
@onready var cast0:RayCast2D = $"CastGroup/RayCast2D0"
@onready var cast1:RayCast2D = $"CastGroup/RayCast2D1"
@onready var cast2:RayCast2D = $"CastGroup/RayCast2D2"
@onready var boomerang:PackedScene = preload("res://Scenes/Entities/Bullets/Enemy/EnemyBulletBoomerangRed.tscn")
@onready var shadow_wave:PackedScene = preload("res://Scenes/Entities/Bullets/Enemy/EnemyBulletShadowWave.tscn")
@onready var donut:PackedScene = preload("res://Scenes/Entities/Bullets/Enemy/EnemyBulletDonutRotaryChaser.tscn")
@onready var debug_label:SnailyText = $"DebugLabel"
#endregion


func _ready() -> void:
	if Engine.is_editor_hint():
		return
	
	if Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS4) == true and not display_mode:
		queue_free()
		return
	
	my_type = EnemyTypes.MOONSNAIL
	col = $"BodyBox"
	sprite = $"JsonSprite2D"
	hitbox = $"Area2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	for child in shadowball_group.get_children():
		if child is JsonSprite2D:
			shadowballs.append(child)
	shadowball_group.visible = false
	
	invulnerable = true
	
	if display_mode:
		sprite.action = "p0_floor_left_idle" if randf() < 0.5 else "p0_floor_right_idle"
		return
	else:
		if not Statics.is_in_boss_rush:
			GameCore.instance.music_manager.play_song(battle_music)
		health_bar = UICore.instance.show_boss_bar(self)
	
	nodes_to_wiggle.append(sprite)
	
	if Statics.current_profile["difficulty"] == 2:
		boss_speed += 0.1


func _physics_process(delta: float) -> void:
	if Engine.is_editor_hint() or Statics.show_invis_entites:
		queue_redraw()
		if Engine.is_editor_hint():
			return
	if not ai_active:
		if in_death_anim:
			_tick_death(delta)
		return
	
	just_grav_jumped = false
	_update_ai(delta)
	_fix_gravity()
	if _pressed_jump(true) and jumping:
		_do_gravity_jump()
	_check_move_input(delta)
	if _pressed_jump(true) and not jumping:
		_do_jump()
	_attack(delta)
	move_and_slide()
	if is_on_ceiling() or is_on_wall():
		just_hit_surface = true
	if is_on_floor():
		just_hit_surface = false
		match gravity:
			Statics.DirsSurface.FLOOR: position.y -= 0.125
			Statics.DirsSurface.LWALL: position.x += 0.125
			Statics.DirsSurface.RWALL: position.x -= 0.125
			Statics.DirsSurface.CEILING: position.y += 0.125
	if frames_down > 0:
		frames_down += 1
	if frames_left > 0:
		frames_left += 1
	if frames_right > 0:
		frames_right += 1
	if frames_up > 0:
		frames_up += 1
	if frames_jump > 0:
		frames_jump += 1
	super._physics_process(delta)
	if debug_label.visible:
		debug_label.global_position = Vector2(16, 16)
		debug_label.set_snaily_text("\n".join([
			"Mode - " + str(BossMode.keys()[mode]),
			"Mode elapsed - " + str(mode_elapsed),
			"Gravity - " + str(Statics.DirsSurface.keys()[gravity]),
			"Target grav - " + str(Statics.DirsSurface.keys()[target_gravity]),
			"Left - " + str(facing_left),
			"Jumping - " + str(jumping),
			"Fall frames - " + str(fall_frames),
			"Move target - " + str(move_end),
			"Tele target - " + str(tele_end)
		]))


func exit_intro() -> void:
	intro_done = true
	mode_elapsed = 0.0
	invulnerable = false


func _tick_death(delta:float) -> void:
	mode_elapsed += delta
	sprite.fps_mult = 1.0 + mode_elapsed
	var lerp_prog:float = inverse_lerp(DEATH_FADE_START, DEATH_FADE_END, mode_elapsed)
	flash_color = Color.BLACK.lerp(DEATH_FADE_COLOR, clampf(lerp_prog, 0.0, 1.0))
	var progress:float = Statics.normalized_sigmoid(mode_elapsed / DEATH_MOVE_TIME, SIGMOID_MOD)
	progress = clampf(progress, 0.0, 1.0)
	position = death_start.lerp(giga_spawn_pos, progress)
	if death_boom != null:
		death_boom.position = position


func kill() -> void:
	var set_timer:bool = false
	if not in_death_anim:
		if health_bar:
			health_bar._toggle_outro_shake(true)
		set_timer = true
		sprite.action = "defeat"
		death_start = position
		death_boom = Statics.spawn_particle("ExplosionBossDefeat",
			Room.Layers.GROUND, position, [true, DEATH_TIME, true])
		mode_elapsed = 0.0
		GameCore.instance.music_manager.stop_all(true)
		Player.instance.do_moon_snail_heal()
	else:
		var giga:Gigasnail = load("res://Scenes/Entities/Enemies/Bosses/Gigasnail.tscn").instantiate()
		giga.position = giga_spawn_pos
		health_bar.pass_control(giga)
		giga.stomp_targets.append_array(stomp_targets)
		GameCore.instance.current_room.layer_ground.add_child(giga)
	super()
	if set_timer:
		death_timer = DEATH_TIME


#region General utility
func _get_decision() -> float:
	decision_table_index = (decision_table_index + 1) % DECISION_TABLE.size()
	return DECISION_TABLE[decision_table_index]


func _set_mode(new_mode:BossMode, try_shoot:bool = false) -> void:
	mode = new_mode
	mode_elapsed = 0.0
	mode_initialized = false
	attacking = false
	action_timeout = ACTION_TIMEOUT
	shadowball_group.visible = false
	_release_all()
	attack_start_timeout = ATTACK_START_TIMEOUT
	attack_stop_timeout = ATTACK_STOP_TIMEOUT
	if try_shoot:
		_check_shoot_donuts()


func _check_shoot_donuts() -> void:
	if ring_timeout <= 0.0:
		ring_timeout = RING_TIMEOUT
		bullets.append_array(_shoot_360_cluster_rotary(donut, Vector2(2.2, 0), 16.0, RING_COUNT))


func _attack(delta:float) -> void:
	weapon_cooldown -= delta
	if weapon_cooldown > 0.0 or not attacking:
		return
	
	var aim:Statics.DirsCompass = Statics.DirsCompass.NONE
	if _pressed_up(false):
		if _pressed_left(false):
			aim = Statics.DirsCompass.NW
		elif _pressed_right(false):
			aim = Statics.DirsCompass.NE
		else:
			aim = Statics.DirsCompass.N
	elif _pressed_down(false):
		if _pressed_left(false):
			aim = Statics.DirsCompass.SW
		elif _pressed_right(false):
			aim = Statics.DirsCompass.SE
		else:
			aim = Statics.DirsCompass.S
	elif _pressed_left(false):
		aim = Statics.DirsCompass.W
	elif _pressed_right(false):
		aim = Statics.DirsCompass.E
	else:
		match gravity:
			Statics.DirsSurface.FLOOR:
				aim = Statics.DirsCompass.W if facing_left else Statics.DirsCompass.E
			Statics.DirsSurface.LWALL:
				aim = Statics.DirsCompass.N if facing_left else Statics.DirsCompass.S
			Statics.DirsSurface.RWALL:
				aim = Statics.DirsCompass.S if facing_left else Statics.DirsCompass.N
			Statics.DirsSurface.CEILING:
				aim = Statics.DirsCompass.E if facing_left else Statics.DirsCompass.W
	
	var aim_vector:Vector2 = Vector2.ZERO
	match aim:
		Statics.DirsCompass.N: aim_vector = Vector2.UP
		Statics.DirsCompass.NE: aim_vector = Vector2(Statics.VECTOR_DIAG.x, -Statics.VECTOR_DIAG.y)
		Statics.DirsCompass.E: aim_vector = Vector2.RIGHT
		Statics.DirsCompass.SE: aim_vector = Statics.VECTOR_DIAG
		Statics.DirsCompass.S: aim_vector = Vector2.DOWN
		Statics.DirsCompass.SW: aim_vector = Vector2(-Statics.VECTOR_DIAG.x, Statics.VECTOR_DIAG.y)
		Statics.DirsCompass.W: aim_vector = Vector2.LEFT
		Statics.DirsCompass.NW: aim_vector = -Statics.VECTOR_DIAG
	match current_weapon:
		1:
			bullets.append(_shoot(boomerang, aim_vector, WEAPON_SPEED[1]))
		2:
			bullets.append(_shoot(shadow_wave, aim_vector, WEAPON_SPEED[2]))
	weapon_cooldown = WEAPON_COOLDOWNS[current_weapon] * 0.5


func _pick_move_target() -> void:
	move_start = position
	move_end = position
	if move_targets.size() > 0:
		while move_start.distance_to(move_end) < MOVE_SELECT_THRESHOLD:
			var i:int = floori(_get_decision() * move_targets.size())
			var target:EntityTarget = move_targets[i]
			move_end = move_targets[i].position
			if not target.global_space:
				move_end += move_start
			target_gravity = (absi(target.data) % 4) as Statics.DirsSurface


func _pick_tele_target() -> void:
	tele_start = position
	tele_end = position
	if tele_targets.size() > 0:
		while tele_start.distance_to(tele_end) < MOVE_SELECT_THRESHOLD:
			var i:int = floori(_get_decision() * tele_targets.size())
			var target:EntityTarget = tele_targets[i]
			tele_end = tele_targets[i].position
			if not target.global_space:
				tele_end += tele_start
			target_gravity = (absi(target.data) % 4) as Statics.DirsSurface


func _play_anim(action:String) -> void:
	var _surface:String = "floor"
	match gravity:
		Statics.DirsSurface.LWALL:
			_surface = "lwall"
		Statics.DirsSurface.RWALL:
			_surface = "rwall"
		Statics.DirsSurface.CEILING:
			_surface = "ceiling"
	var _dir:String = "left" if facing_left else "right"
	var full_action:String = "_".join([_surface, _dir, action])
	if full_action != last_anim:
		play_phase_anim(full_action)
		last_anim = full_action


func _casts_colliding() -> bool:
	return cast0.is_colliding() or cast1.is_colliding() or cast2.is_colliding()


func advance_phase(count:int = 1) -> void:
	super(count)
	if phase == 1:
		boss_speed += 0.3
		for shadowball in shadowballs:
			shadowball.action = "anim_panic"


func _draw() -> void:
	if Engine.is_editor_hint() or Statics.show_invis_entites:
		for tgt in move_targets:
			var pos:Vector2 = tgt.position
			if tgt.global_space:
				pos -= position
			draw_circle(pos, 8, Color(0.1, 0.6, 0.75, 0.3), true)
		for tgt in tele_targets:
			var pos:Vector2 = tgt.position
			if tgt.global_space:
				pos -= position
			draw_circle(pos, 6, Color(0.6, 0.6, 0.6, 0.3), true)
	if Engine.is_editor_hint():
		for tgt in stomp_targets:
			var pos:Vector2 = tgt.position
			if tgt.global_space:
				pos -= position
			draw_circle(pos, 7, Color(0.1, 0.75, 0.4, 0.3), true)
			if tgt.data >= 1 and tgt.data <= 15:
				if tgt.data & 1 > 0:
					draw_line(pos, pos + Vector2(0, 8), Color(0.1, 0.85, 0.25, 0.7), 1.0)
				if tgt.data & 2 > 0:
					draw_line(pos, pos + Vector2(-8, 0), Color(0.1, 0.85, 0.25, 0.7), 1.0)
				if tgt.data & 4 > 0:
					draw_line(pos, pos + Vector2(0, -8), Color(0.1, 0.85, 0.25, 0.7), 1.0)
				if tgt.data & 8 > 0:
					draw_line(pos, pos + Vector2(8, 0), Color(0.1, 0.85, 0.25, 0.7), 1.0)
		draw_circle(giga_spawn_pos - Vector2i(position), 8, Color(0.95, 0.1, 0.5, 0.3), true)
#endregion


#region AI updating
func _update_intro() -> void:
	if mode_elapsed > 0.3 and intro_done:
		_set_mode(BossMode.ATTACK)


func _update_move() -> void:
	if not mode_initialized:
		mode_initialized = true
		_pick_move_target()
		current_weapon = 1
		_release_all()
	if phase >= 1 or Statics.current_profile["difficulty"] == 2:
		attacking = true
	if ((gravity == Statics.DirsSurface.FLOOR or gravity == Statics.DirsSurface.CEILING)
	and absf(move_end.x - position.x) > MOVE_END_THRESHOLD):
		if move_end.x > position.x:
			_press_right()
		else:
			_press_left()
	else:
		_release_right() 
		_release_left()
	if ((gravity == Statics.DirsSurface.LWALL or gravity == Statics.DirsSurface.RWALL)
	and absf(move_end.y - position.y) > MOVE_END_THRESHOLD):
		if move_end.y > position.y:
			_press_down()
		else:
			_press_up()
	else:
		_release_down() 
		_release_up()
	if not jumping:
		if gravity != target_gravity:
			_prep_jump(target_gravity)
		else:
			_prep_jump()
	_check_shoot_donuts()
	if position.distance_to(move_end) < MOVE_APPROACH_THRESHOLD:
		_set_mode(BossMode.ATTACK)


func _update_teleport() -> void:
	if not mode_initialized:
		mode_initialized = true
		_pick_tele_target()
		sprite.modulate.a = 0.0
		can_damage = false
		invulnerable = true
		for ball in shadowballs:
			ball.global_position = tele_start
		shadowball_group.visible = true
		sfx_teleport.play()
		var particle_state:int = ProjectSettings.get_setting("game/world/particles")
		if particle_state == Statics.ParticleOptions.ENTITIES_ALL or particle_state == Statics.ParticleOptions.ALL:
			Statics.spawn_particle("MoonTeleport", Room.Layers.GROUND, tele_start)
	var progress:float = Statics.normalized_sigmoid(mode_elapsed / TELEPORT_TIME, SIGMOID_MOD)
	var this_sigmoid:float = 0.0
	if progress <= 0.5:
		this_sigmoid = Statics.normalized_sigmoid(mode_elapsed / TELEPORT_TIME * 2, SIGMOID_MOD)
	else:
		this_sigmoid = Statics.normalized_sigmoid((1 - mode_elapsed / TELEPORT_TIME) * 2, SIGMOID_MOD)
	var ball_radius:float = SHADOW_BALL_RADIUS * this_sigmoid
	var ball_theta:float = TAU * this_sigmoid
	var i:int = 0
	while i < shadowballs.size():
		shadowballs[i].global_position = Vector2(
			tele_start.x * (1.0 - progress) + tele_end.x * progress + cos(
				ball_theta + TAU / shadowballs.size() * i
			) * ball_radius,
			tele_start.y * (1.0 - progress) + tele_end.y * progress + sin(
				ball_theta + TAU / shadowballs.size() * i
			) * ball_radius
		)
		i += 1
	velocity = Vector2.ZERO
	_release_jump()
	if mode_elapsed / TELEPORT_TIME >= 1.0:
		position = tele_end
		_set_mode(BossMode.ATTACK, true)
		_set_dir(target_gravity, facing_left)
		_face_player()
		sprite.modulate.a = 1.0
		can_damage = true
		invulnerable = false


func _update_attack(delta:float) -> void:
	_face_player()
	if not attacking:
		attack_start_timeout -= delta * boss_speed
		if attack_start_timeout <= 0.0:
			attack_stop_timeout = ATTACK_STOP_TIMEOUT
			attacking = true
	else:
		attack_stop_timeout -= delta * boss_speed
		if attack_stop_timeout <= 0.0:
			attack_start_timeout = ATTACK_START_TIMEOUT
			attacking = false
	current_weapon = 2
	if action_timeout <= 0.0:
		action_timeout = ACTION_TIMEOUT
		if _get_decision() < 0.2:
			match gravity:
				Statics.DirsSurface.FLOOR:
					_prep_jump(Statics.DirsSurface.CEILING)
				Statics.DirsSurface.LWALL:
					_prep_jump(Statics.DirsSurface.RWALL)
				Statics.DirsSurface.RWALL:
					_prep_jump(Statics.DirsSurface.LWALL)
				Statics.DirsSurface.CEILING:
					_prep_jump(Statics.DirsSurface.FLOOR)
		elif _get_decision() < 0.4:
			_prep_jump()
		elif _get_decision() < 0.4:
			_set_mode(BossMode.TELEPORT)
		else:
			_set_mode(BossMode.MOVE)
	if position.distance_to(Player.instance.position) < PROX_TELE_THRESHOLD:
		_set_mode(BossMode.TELEPORT)
	else:
		var rel_horiz_threshold:float = 50.0
		var rel_vert_threshold:float = 200.0
		var x_difference:float = abs(position.x - Player.instance.position.x)
		var y_difference:float = abs(position.y - Player.instance.position.y)
		match gravity:
			Statics.DirsSurface.FLOOR:
				if x_difference < rel_horiz_threshold or y_difference > rel_vert_threshold:
					_press_up()
				else:
					_release_up()
			Statics.DirsSurface.LWALL:
				if y_difference < rel_horiz_threshold or x_difference > rel_vert_threshold:
					_press_right()
				else:
					_release_right()
			Statics.DirsSurface.RWALL:
				if y_difference < rel_horiz_threshold or x_difference > rel_vert_threshold:
					_press_left()
				else:
					_release_left()
			Statics.DirsSurface.CEILING:
				if x_difference < rel_horiz_threshold or y_difference > rel_vert_threshold:
					_press_down()
				else:
					_release_down()


func _update_ai(delta:float) -> void:
	ring_timeout -= delta * boss_speed
	if tapped_left:
		tapped_left = false
		_release_left()
	if tapped_right:
		tapped_right = false
		_release_right()
	if tapped_up:
		tapped_up = false
		_release_up()
	if tapped_down:
		tapped_down = false
		_release_down()
	mode_elapsed += delta * boss_speed
	action_timeout -= delta * boss_speed
	match mode:
		BossMode.INTRO:
			_update_intro()
		BossMode.MOVE:
			_update_move()
		BossMode.ATTACK:
			_update_attack(delta)
		BossMode.TELEPORT:
			_update_teleport()
		BossMode.STRAFE:
			pass
	jump_release_timeout -= delta
	if jump_release_timeout <= 0.0 and _pressed_jump(false):
		_release_jump()
		if gravity_jump_dir != Statics.DirsSurface.NONE:
			match gravity_jump_dir:
				Statics.DirsSurface.FLOOR:
					_tap_down()
				Statics.DirsSurface.LWALL:
					_tap_left()
				Statics.DirsSurface.RWALL:
					_tap_right()
				Statics.DirsSurface.CEILING:
					_tap_up()
			_prep_jump(Statics.DirsSurface.NONE, 0.1)
#endregion


#region Movement handling
func _set_dir(dir:Statics.DirsSurface, left:bool) -> void:
	gravity = dir
	facing_left = left
	var rot_deg:float = 0.0
	up_direction = Vector2.UP
	match dir:
		Statics.DirsSurface.LWALL:
			rot_deg = 90.0
			up_direction = Vector2.RIGHT
		Statics.DirsSurface.RWALL:
			rot_deg = -90.0
			up_direction = Vector2.LEFT
		Statics.DirsSurface.CEILING:
			rot_deg = 180.0
			up_direction = Vector2.DOWN
	col.rotation_degrees = rot_deg
	hitbox.rotation_degrees = rot_deg
	cast_group.rotation_degrees = rot_deg


func _face_player() -> void:
	var p_pos:Vector2 = Player.instance.position
	match gravity:
		Statics.DirsSurface.FLOOR:
			if p_pos.x > position.x and facing_left:
				_tap_right()
			elif p_pos.x < position.x and not facing_left:
				_tap_left()
		Statics.DirsSurface.LWALL:
			if p_pos.y > position.y and facing_left:
				_tap_down()
			elif p_pos.y < position.y and not facing_left:
				_tap_up()
		Statics.DirsSurface.RWALL:
			if p_pos.y < position.y and facing_left:
				_tap_up()
			elif p_pos.y > position.y and not facing_left:
				_tap_down()
		Statics.DirsSurface.CEILING:
			if p_pos.x < position.x and facing_left:
				_tap_left()
			elif p_pos.x > position.x and not facing_left:
				_tap_right()


func _prep_jump(grav_jump_target:Statics.DirsSurface = Statics.DirsSurface.NONE, release_timeout:float = 0.3) -> void:
	if jump_release_timeout > 0:
		return
	_press_jump()
	jump_release_timeout = release_timeout
	gravity_jump_dir = grav_jump_target


func _do_jump() -> void:
	if not just_grav_jumped:
		sfx_jump.play()
	match gravity:
		Statics.DirsSurface.FLOOR:
			velocity.y = -JUMP_POWER
		Statics.DirsSurface.LWALL:
			velocity.x = JUMP_POWER
		Statics.DirsSurface.RWALL:
			velocity.x = -JUMP_POWER
		Statics.DirsSurface.CEILING:
			velocity.y = JUMP_POWER


func _do_gravity_jump() -> void:
	var no_horiz:bool = not _pressed_left(false) and not _pressed_right(false)
	var no_vert:bool = not _pressed_up(false) and not _pressed_down(false)
	if no_horiz and no_vert:
		return
	var down:bool = (no_horiz or most_recent_dir == Statics.DirsCardinal.DOWN) and _pressed_down(false)
	var left:bool = (no_vert or most_recent_dir == Statics.DirsCardinal.LEFT) and _pressed_left(false)
	var right:bool = (no_vert or most_recent_dir == Statics.DirsCardinal.RIGHT) and _pressed_right(false)
	var up:bool = (no_horiz or most_recent_dir == Statics.DirsCardinal.UP) and _pressed_up(false)
	match gravity:
		Statics.DirsSurface.FLOOR:
			if up:
				_set_dir(Statics.DirsSurface.CEILING, not facing_left)
				target_gravity = Statics.DirsSurface.CEILING
			elif right:
				_set_dir(Statics.DirsSurface.RWALL, false)
				target_gravity = Statics.DirsSurface.RWALL
			elif left:
				_set_dir(Statics.DirsSurface.LWALL, false)
				target_gravity = Statics.DirsSurface.LWALL
		Statics.DirsSurface.LWALL:
			if right:
				_set_dir(Statics.DirsSurface.RWALL, not facing_left)
				target_gravity = Statics.DirsSurface.RWALL
			elif down:
				_set_dir(Statics.DirsSurface.FLOOR, false)
				target_gravity = Statics.DirsSurface.FLOOR
			elif up:
				_set_dir(Statics.DirsSurface.CEILING, false)
				target_gravity = Statics.DirsSurface.CEILING
		Statics.DirsSurface.RWALL:
			if left:
				_set_dir(Statics.DirsSurface.LWALL, not facing_left)
				target_gravity = Statics.DirsSurface.LWALL
			elif down:
				_set_dir(Statics.DirsSurface.FLOOR, false)
				target_gravity = Statics.DirsSurface.FLOOR
			elif up:
				_set_dir(Statics.DirsSurface.CEILING, false)
				target_gravity = Statics.DirsSurface.CEILING
		Statics.DirsSurface.CEILING:
			if down:
				_set_dir(Statics.DirsSurface.FLOOR, not facing_left)
				target_gravity = Statics.DirsSurface.FLOOR
			elif right:
				_set_dir(Statics.DirsSurface.RWALL, false)
				target_gravity = Statics.DirsSurface.RWALL
			elif left:
				_set_dir(Statics.DirsSurface.LWALL, false)
				target_gravity = Statics.DirsSurface.LWALL
	sfx_jumpgrav.play()
	just_grav_jumped = true
	Statics.spawn_particle("GravWhooshGroup", Room.Layers.GROUND, position, [gravity, false])


func _check_move_input(delta:float) -> void:
	if _pressed_up(true):
		most_recent_dir = Statics.DirsCardinal.UP
		most_recent_vert = Statics.DirsCardinal.UP
	if _pressed_down(true):
		most_recent_dir = Statics.DirsCardinal.DOWN
		most_recent_vert = Statics.DirsCardinal.DOWN
	if _pressed_left(true):
		most_recent_dir = Statics.DirsCardinal.LEFT
		most_recent_horiz = Statics.DirsCardinal.LEFT
	if _pressed_right(true):
		most_recent_dir = Statics.DirsCardinal.RIGHT
		most_recent_horiz = Statics.DirsCardinal.RIGHT
	var jump_state:int = 0
	var turned:bool = false
	var moving:bool = false
	
	if not jumping and not _casts_colliding():
		jumping = true
	
	match gravity:
		Statics.DirsSurface.FLOOR:
			velocity.x = 0.0
			if jumping:
				velocity.y += GRAVITY * delta
				jump_state = 1 if velocity.y < 0 else 2
			if _pressed_left(false):
				if not facing_left:
					turned = true
				facing_left = true
				velocity.x = -RUN_SPEED
			elif _pressed_right(false):
				if facing_left:
					turned = true
				facing_left = false
				velocity.x = RUN_SPEED
			moving = velocity.x != 0.0
		Statics.DirsSurface.LWALL:
			velocity.y = 0.0
			if jumping:
				velocity.x -= GRAVITY * delta
				jump_state = 1 if velocity.x > 0 else 2
			if _pressed_up(false):
				if not facing_left:
					turned = true
				facing_left = true
				velocity.y = -RUN_SPEED
			elif _pressed_down(false):
				if facing_left:
					turned = true
				facing_left = false
				velocity.y = RUN_SPEED
			moving = velocity.y != 0.0
		Statics.DirsSurface.RWALL:
			velocity.y = 0.0
			if jumping:
				velocity.x += GRAVITY * delta
				jump_state = 1 if velocity.x < 0 else 2
			if _pressed_down(false):
				if not facing_left:
					turned = true
				facing_left = true
				velocity.y = RUN_SPEED
			elif _pressed_up(false):
				if facing_left:
					turned = true
				facing_left = false
				velocity.y = -RUN_SPEED
			moving = velocity.y != 0.0
		Statics.DirsSurface.CEILING:
			velocity.x = 0.0
			if jumping:
				velocity.y -= GRAVITY * delta
				jump_state = 1 if velocity.y > 0 else 2
			if _pressed_right(false):
				if not facing_left:
					turned = true
				facing_left = true
				velocity.x = RUN_SPEED
			elif _pressed_left(false):
				if facing_left:
					turned = true
				facing_left = false
				velocity.x = -RUN_SPEED
			moving = velocity.x != 0.0
	
	if jumping:
		if turned:
			_play_anim("turnjump" if jump_state == 1 else "turnfall")
		else:
			_play_anim("jump" if jump_state == 1 else "fall")
	elif moving:
		_play_anim("turnground" if turned else "walk")
	else:
		_play_anim("turnground" if turned else "idle")


func _fix_gravity() -> void:
	match gravity:
		Statics.DirsSurface.FLOOR:
			if not jumping and velocity.y > 0 and _pressed_down(false) and not just_hit_surface:
				if not facing_left and _pressed_right(false):
					facing_left = true
				elif facing_left and _pressed_left(false):
					facing_left = false
				_play_anim("walk")
			jumping = velocity.y != 0.0
		Statics.DirsSurface.LWALL:
			if not jumping and velocity.x < 0 and _pressed_left(false) and not just_hit_surface:
				if not facing_left and _pressed_down(false):
					facing_left = true
				elif facing_left and _pressed_up(false):
					facing_left = false
				_play_anim("walk")
			jumping = velocity.x != 0.0
		Statics.DirsSurface.RWALL:
			if not jumping and velocity.x > 0 and _pressed_right(false) and not just_hit_surface:
				if not facing_left and _pressed_up(false):
					facing_left = true
				elif facing_left and _pressed_down(false):
					facing_left = false
				_play_anim("walk")
			jumping = velocity.x != 0.0
		Statics.DirsSurface.CEILING:
			if not jumping and velocity.y < 0 and _pressed_up(false) and not just_hit_surface:
				if not facing_left and _pressed_left(false):
					facing_left = true
				elif facing_left and _pressed_right(false):
					facing_left = false
				_play_anim("walk")
			jumping = velocity.y != 0.0
	if jumping:
		fall_frames += 1
	else:
		fall_frames = 0
	if fall_frames == 1:
		_play_anim("jump")
#endregion


#region Simulated control
func _press_left() -> void:
	frames_left = 1
	frames_right = -1


func _release_left() -> void:
	frames_left = -1


func _tap_left() -> void:
	_press_left()
	tapped_left = true


func _pressed_left(just:bool) -> bool:
	if just:
		return frames_left == 1
	return frames_left > 0


func _press_right() -> void:
	frames_right = 1
	frames_left = -1


func _release_right() -> void:
	frames_right = -1


func _tap_right() -> void:
	_press_right()
	tapped_right = true


func _pressed_right(just:bool) -> bool:
	if just:
		return frames_right == 1
	return frames_right > 0


func _press_up() -> void:
	frames_up = 1
	frames_down = -1


func _release_up() -> void:
	frames_up = -1


func _tap_up() -> void:
	_press_up()
	tapped_up = true


func _pressed_up(just:bool) -> bool:
	if just:
		return frames_up == 1
	return frames_up > 0


func _press_down() -> void:
	frames_down = 1
	frames_up = -1


func _release_down() -> void:
	frames_down = -1


func _tap_down() -> void:
	_press_down()
	tapped_down = true


func _pressed_down(just:bool) -> bool:
	if just:
		return frames_down == 1
	return frames_down > 0


func _press_jump() -> void:
	frames_jump = 1


func _release_jump() -> void:
	frames_jump = -1


func _pressed_jump(just:bool) -> bool:
	if just:
		return frames_jump == 1
	return frames_jump > 0


func _release_all() -> void:
	_release_left()
	_release_right()
	_release_up()
	_release_down()
#endregion
