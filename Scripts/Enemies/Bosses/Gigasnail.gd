# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name Gigasnail
extends Boss


#region Variables
const GRAV_JUMP_TIMEOUT:float = 0.2
const START_ATTACK_TIME:float = 0.45
const JUMP_POWER:float = 360.0
const JUMP_TIMEOUT:float = 0.8
const GRAVITY:float = 900.0
const WALK_SPEED:float = 200.0
const WAVE_SPEED:float = 30.0
const WAVE_TIMEOUT:float = 0.9
const ZZZ_TIMEOUT:float = 0.3
const ZZZ_MAX:int = 3
const ZZZ_BUFFER:float = 24.0
const INTRO_DELAY:float = 1.25
const INTRO_END:float = 4.0
const STRAFE_TIMEOUT:float = 0.03
const STRAFE_SPEED:float = 400.0
const STRAFE_TARGET_PROX:float = 10.0
const SMASH_SPEED:float = 400.0
const STOMP_TIMEOUT:float = 0.25
const STOMP_TARGET_MIN_DIST:float = 130.0
const STOMP_TARGET_APPROACH_DIST:float = 10.0
const STOMP_MAX_ATTACKS_SINCE:int = 4
const STOMP_ALLOW_FLIPS:bool = false
const SLEEP_X_DIFF_FORGIVENESS:float = 40.0
const MODE_TIMEOUT_STOMP:float = 6.0
const MODE_TIMEOUT_STRAFE:float = 5.2
const MODE_TIMEOUT_SMASH:float = 6.0
const MODE_TIMEOUT_SLEEP:float = 6.2
const SHAKE_TIMELINE:Array[float] = [4.0, 0.7]
const BOX_SIZE_NORMAL:Vector2i = Vector2i(80, 44)
const BOX_SIZE_SHELL:Vector2i = Vector2i(44, 44)
const AREA_SIZE_NORMAL:Vector2i = Vector2i(80, 44)
const AREA_SIZE_SHELL:Vector2i = Vector2i(44, 44)
const AFTERIMAGE_MIN_DIST:float = 24.0
const DEATH_TIME:float = 8.0
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

enum BossMode {
	INTRO,
	STOMP,
	STRAFE,
	SMASH,
	SLEEP
}
var mode:BossMode = BossMode.INTRO
var last_mode:BossMode = BossMode.INTRO
var mode_elapsed:float = 0.0
var mode_initialized:bool = false
var mode_timeout:float = 0.0
var target:Vector2 = Vector2.ZERO

var last_hit_dir:Statics.DirsSurface = Statics.DirsSurface.NONE
var attacks_since_stomp:int = 0
var stomp_gravity:Statics.DirsSurface = Statics.DirsSurface.FLOOR
var strafe_spoke_count:int = 2
var last_smash_vel:Vector2 = Vector2.ZERO
var decision_table_index:int = 0
var shot_timeout:float = 0.0
var last_anim:String = ""
var last_state:String = "shell"
var strafe_theta:float = 0.0
var strafe_theta_vel:float = 0.0
var strafe_theta_accel:float = 0.0
var strafe_timeout:float = 0.0
var zzz_count:int = 0
var zzz_i:int = 0
var zzz_timeout:float = 0.0
var waiting_to_jump:bool = false
var stomp_timeout:float = 0.0
var wave_timeout:float = 0.0
var stomped:bool = false
var smash_dir:Vector2 = Vector2.RIGHT
var aimed:bool = false
var facing_left:bool = false
var grav_jump_timeout:float = 99999.0
var jump_timeout:float = 0.0
var boss_speed:float = 1.0
var last_afterimage_spawn:Vector2 = Vector2.ZERO

var update_if_shell:bool = false
var afterimage_y:Array = []

var boss_environment:GigaEnvironment

var body_rect:RectangleShape2D = null
var area_rect:RectangleShape2D = null

@export var stomp_targets:Array[EntityTarget]

@onready var sfx_spawn:AudioStreamPlayer = $"AudioGroup/Spawn"
@onready var sfx_jump:AudioStreamPlayer = $"AudioGroup/Jump"
@onready var sfx_gravjump:AudioStreamPlayer = $"AudioGroup/GravJump"
@onready var sfx_stomp:AudioStreamPlayer = $"AudioGroup/Stomp"
@onready var sfx_sleep:AudioStreamPlayer = $"AudioGroup/Sleep"
@onready var wave:PackedScene = preload("res://Scenes/Entities/Bullets/Enemy/EnemyBulletGigaWave.tscn")
@onready var pea:PackedScene = preload("res://Scenes/Entities/Bullets/Enemy/EnemyBulletGigaPea.tscn")
@onready var zzz:PackedScene = preload("res://Scenes/Entities/Bullets/Enemy/EnemyBulletZzz.tscn")
#endregion


func _ready() -> void:
	my_type = EnemyTypes.GIGASNAIL
	col = $"BodyBox"
	sprite = $"JsonSprite2D"
	hitbox = $"Area2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	if display_mode:
		sprite.action = "p0_stomp_idle_floor_left" if randf() < 0.5 else "p0_stomp_idle_floor_right"
		return
	
	if col and col.shape and col.shape is RectangleShape2D:
		body_rect = col.shape
	var _hitbox:CollisionShape2D = $"Area2D/HitBox"
	if _hitbox and _hitbox.shape and _hitbox.shape is RectangleShape2D:
		area_rect = _hitbox.shape
	assert(body_rect, "Giga Snail requires a collision shape for wall interactions!")
	assert(area_rect, "Giga Snail requires a collision shape for his hitbox!")
	body_rect.size = BOX_SIZE_SHELL
	area_rect.size = AREA_SIZE_SHELL
	
	nodes_to_wiggle.append(sprite)
	
	if Statics.current_profile["difficulty"] == 2:
		boss_speed += 0.2
	
	decision_table_index = floori(Player.instance.position.x) % DECISION_TABLE.size()
	
	sfx_spawn.play()
	boss_environment = load("res://Scenes/Environments/GigaEnvironment.tscn").instantiate()
	GameCore.instance.current_room.layer_ground.add_child(boss_environment)
	boss_environment.connect_giga(self)
	
	update_if_shell = sprite.meta["change_anim_if_both_are_shell"]
	afterimage_y.append(sprite.meta["afterimage_y_stomp"])
	afterimage_y.append(sprite.meta["afterimage_y_smash"])
	afterimage_y.append(sprite.meta["afterimage_y_strafe"])
	afterimage_y.append(sprite.meta["afterimage_y_sleep"])
	sprite._process(0.0)
	if health_bar and health_bar.outro_shake:
		health_bar._toggle_outro_shake()


func _physics_process(delta:float) -> void:
	if Statics.show_invis_entites:
		queue_redraw()
	if Engine.is_editor_hint():
		return
	if not ai_active:
		return
	
	wave_timeout -= delta * boss_speed
	mode_timeout -= delta * boss_speed
	strafe_timeout -= delta * boss_speed
	stomp_timeout -= delta * boss_speed
	mode_elapsed += delta * boss_speed
	match mode:
		BossMode.INTRO:
			_update_intro()
		BossMode.STOMP:
			_update_stomp(delta)
		BossMode.STRAFE:
			_update_strafe(delta)
		BossMode.SMASH:
			_update_smash()
		BossMode.SLEEP:
			_update_sleep(delta)
	
	var real = self
	if real is CharacterBody2D:
		#region Move stomp
		if mode == BossMode.STOMP and not stomped and last_state == "stomp":
			var last_vel:Vector2 = real.velocity
			real.move_and_slide()
			match stomp_gravity:
				Statics.DirsSurface.FLOOR:
					real.velocity.y += GRAVITY * delta
					if real.is_on_wall():
						real.velocity.x = -last_vel.x
						position.x += 0.5 * sign(real.velocity.x)
				Statics.DirsSurface.LWALL:
					real.velocity.x -= GRAVITY * delta
					if real.is_on_wall():
						real.velocity.y = -last_vel.y
						position.y += 0.5 * sign(real.velocity.y)
				Statics.DirsSurface.RWALL:
					real.velocity.x += GRAVITY * delta
					if real.is_on_wall():
						real.velocity.y = -last_vel.y
						position.y += 0.5 * sign(real.velocity.y)
				Statics.DirsSurface.CEILING:
					real.velocity.y -= GRAVITY * delta
					if real.is_on_wall():
						real.velocity.x = -last_vel.x
						position.x += 0.5 * sign(real.velocity.x)
			if real.is_on_floor() and mode_elapsed > 0.0:
				_stomp()
		#endregion
		#region Move smash
		if mode == BossMode.SMASH:
			real.velocity += smash_dir * SMASH_SPEED * boss_speed * delta
			if real.move_and_slide():
				if real.is_on_floor():
					stomp_gravity = Statics.DirsSurface.FLOOR
					position.y -= 0.5
				elif real.is_on_ceiling():
					stomp_gravity = Statics.DirsSurface.CEILING
					position.y += 0.5
				elif real.get_last_slide_collision().get_position().x < position.x:
					stomp_gravity = Statics.DirsSurface.LWALL
					position.x += 0.5
				else:
					stomp_gravity = Statics.DirsSurface.RWALL
					position.x -= 0.5
				_stomp()
				real.velocity = Vector2.ZERO
				last_hit_dir = stomp_gravity
		#endregion
		#region Move sleep
		if mode == BossMode.SLEEP:
			if not stomped:
				real.velocity.y += GRAVITY * delta
				real.move_and_slide()
				if real.is_on_floor():
					_stomp()
					position.y += 4.0
		#endregion
	
	super._physics_process(delta)
	
	if (last_state != "stomp" and mode != BossMode.INTRO
	and position.distance_to(last_afterimage_spawn) >= AFTERIMAGE_MIN_DIST):
		_spawn_afterimage()


func kill() -> void:
	var set_timer:bool = false
	if not in_death_anim:
		if health_bar:
			health_bar._toggle_outro_shake(true)
		sprite.action = "defeat"
		for bullet in bullets:
			bullet._despawn()
		set_timer = true
		Statics.set_world_flag(Statics.WorldFlags.DEFEATED_BOSS4, true)
		Statics.spawn_particle("ExplosionBossDefeat",
			Room.Layers.FG1, position, [true, DEATH_TIME, true, true])
		UICore.instance.call_screen_shake_radial([2.0, 4.0, 2.0, 4.0, 0.0], UICore.ShakeCallMode.OVERWRITE_ALL)
		GameCore.instance.music_manager.stop_all(true)
		#SInput.read_inputs = false
		PauseLayer.suppress_menuing = true
		Statics.increment_igt = false
		var fade:EndingFade = load("res://Scenes/UI/EndingComponents/EndingFade.tscn").instantiate()
		fade.explosion_point = position
		GameCore.instance.add_child(fade)
		#region Achievements
		AchievementCore.instance.check_add(AchievementCore.Achievements.BEAT_MOON_SNAIL)
		if not Statics.check_item(Item.ItemTypes.METAL_SHELL):
			AchievementCore.instance.check_add(AchievementCore.Achievements.BEAT_MOON_SNAIL_NO_ARMOR)
		if Statics.current_profile["difficulty"] == 2:
			AchievementCore.instance.check_add(AchievementCore.Achievements.WIN_ABSURD)
		if Statics.is_random_game:
			AchievementCore.instance.check_add(AchievementCore.Achievements.WIN_RANDOMIZER)
		match Statics.current_profile["character"]:
			Player.Players.SLUGGY:
				AchievementCore.instance.check_add(AchievementCore.Achievements.WIN_SLUGGY)
			Player.Players.UPSIDE:
				AchievementCore.instance.check_add(AchievementCore.Achievements.WIN_UPSIDE)
			Player.Players.LEGGY:
				AchievementCore.instance.check_add(AchievementCore.Achievements.WIN_LEGGY)
			Player.Players.BLOBBY:
				AchievementCore.instance.check_add(AchievementCore.Achievements.WIN_BLOBBY)
			Player.Players.LEECHY:
				AchievementCore.instance.check_add(AchievementCore.Achievements.WIN_LEECHY)
		if Statics.check_item(Item.ItemTypes.HELIX_FRAGMENT) >= 30:
			AchievementCore.instance.check_add(AchievementCore.Achievements.SUN_SNAIL)
		if Statics.compare_times(Statics.current_profile["game_time"], [0, 30, 0.0]) < 0:
			AchievementCore.instance.check_add(AchievementCore.Achievements.UNDER_30_MIN)
			Statics.add_unlock_condition(Statics.Unlocks.ABSURD_DIFF)
		Statics.add_unlock_condition(Statics.Unlocks.BOSS_RUSH)
		#endregion
	else:
		GameCore.instance.current_room.set_ground_collision(true)
		boss_environment.despawn()
		UICore.instance.clear_boss_bar()
	super()
	if set_timer:
		death_timer = DEATH_TIME


#region General utility
func _get_decision() -> float:
	decision_table_index = (decision_table_index + 1) % DECISION_TABLE.size()
	return DECISION_TABLE[decision_table_index]


func _set_mode(new_mode:BossMode) -> void:
	var real = self
	if real is CharacterBody2D:
		if new_mode == BossMode.STOMP:
			attacks_since_stomp = 0
		else:
			attacks_since_stomp += 1
		if attacks_since_stomp >= STOMP_MAX_ATTACKS_SINCE:
			new_mode = BossMode.STOMP
			attacks_since_stomp = 0
		
		last_mode = mode
		mode = new_mode
		mode_initialized = false
		stomped = false
		real.velocity = Vector2.ZERO
		mode_elapsed = 0.0
		waiting_to_jump = false
		_set_hitboxes(0)
		
		match new_mode:
			BossMode.STOMP: boss_environment.set_state("stomp")
			BossMode.STRAFE: boss_environment.set_state("strafe")
			BossMode.SMASH: boss_environment.set_state("smash")
			BossMode.SLEEP: boss_environment.set_state("sleep")


func _shoot_wave() -> void:
	if wave_timeout > 0.0:
		return
	wave_timeout = WAVE_TIMEOUT
	var dir:Vector2 = Vector2.RIGHT
	match stomp_gravity:
		Statics.DirsSurface.FLOOR:
			dir = Vector2.LEFT if facing_left else Vector2.RIGHT
		Statics.DirsSurface.LWALL:
			dir = Vector2.UP if facing_left else Vector2.DOWN
		Statics.DirsSurface.RWALL:
			dir = Vector2.DOWN if facing_left else Vector2.UP
		Statics.DirsSurface.CEILING:
			dir = Vector2.RIGHT if facing_left else Vector2.LEFT
	_shoot(wave, dir, WAVE_SPEED)


func _pick_stomp_target() -> void:
	target = position
	var gravity_options:int = 0
	while position.distance_to(target) < STOMP_TARGET_MIN_DIST:
		var i:int = floori(_get_decision() * stomp_targets.size())
		var _target:EntityTarget = stomp_targets[i]
		target = _target.position
		if not _target.global_space:
			target += position
		gravity_options = _target.data
	var available_grav:Array[Statics.DirsSurface]
	if gravity_options & 1 > 0:
		available_grav.append(Statics.DirsSurface.FLOOR)
	if gravity_options & 2 > 0:
		available_grav.append(Statics.DirsSurface.LWALL)
	if gravity_options & 4 > 0:
		available_grav.append(Statics.DirsSurface.CEILING)
	if gravity_options & 8 > 0:
		available_grav.append(Statics.DirsSurface.RWALL)
	if available_grav.size() > 0:
		var i:int = floori(_get_decision() * available_grav.size())
		stomp_gravity = available_grav[i]


func _pick_smash_target(force_target_player:bool) -> void:
	var angle:float = randf() * TAU
	if _get_decision() > 0.5 or force_target_player:
		angle = atan2(
			Player.instance.position.y - position.y,
			Player.instance.position.x - position.x
		)
	smash_dir = Vector2(cos(angle), sin(angle))
	if ((last_hit_dir == Statics.DirsSurface.FLOOR and smash_dir.y > 0.0)
	or (last_hit_dir == Statics.DirsSurface.CEILING and smash_dir.y < 0.0)):
		smash_dir.y *= -1.0
	if ((last_hit_dir == Statics.DirsSurface.RWALL and smash_dir.x > 0.0)
	or (last_hit_dir == Statics.DirsSurface.LWALL and smash_dir.x < 0.0)):
		smash_dir.x *= -1.0


func _play_anim(action:String, state:String, dir_target:Vector2 = Vector2.ZERO) -> void:
	if last_state == "shell" and state == "shell" and not update_if_shell:
		return
	last_state = state
	
	var full_action:String = action
	if state == "stomp":
		var _surface:String = "floor"
		match stomp_gravity:
			Statics.DirsSurface.LWALL:
				_surface = "lwall"
			Statics.DirsSurface.RWALL:
				_surface = "rwall"
			Statics.DirsSurface.CEILING:
				_surface = "ceiling"
		var _dir:String = "left" if facing_left else "right"
		full_action = "_".join([action, _surface, _dir])
	
	if dir_target != Vector2.ZERO:
		var dir_vector:Vector2 = position.direction_to(dir_target)
		var dir_string:String = "_"
		if dir_vector.y < -0.3827:
			dir_string += "n"
		elif dir_vector.y > 0.3827:
			dir_string += "s"
		if dir_vector.x < -0.3827:
			dir_string += "w"
		elif dir_vector.x > 0.3827:
			dir_string += "e"
		full_action += dir_string
	
	if full_action != last_anim:
		play_phase_anim(full_action)
		sprite._process(0.0)
		last_anim = full_action


func _spawn_afterimage() -> void:
	var frame_coords:Vector2i = sprite.frame_coords
	match mode:
		BossMode.STOMP: frame_coords.y = afterimage_y[0][phase]
		BossMode.SMASH: frame_coords.y = afterimage_y[1][phase]
		BossMode.STRAFE: frame_coords.y = afterimage_y[2][phase]
		BossMode.SLEEP: frame_coords.y = afterimage_y[3][phase]
	sprite.call_deferred("create_afterimage", 0.8, 0.0, 0.75, z_index - 1, frame_coords)
	last_afterimage_spawn = position


func _set_hitboxes(state:int) -> void:
	match state:
		0:
			body_rect.size = BOX_SIZE_SHELL
			area_rect.size = AREA_SIZE_SHELL
			invulnerable = true
		1:
			body_rect.size = BOX_SIZE_NORMAL
			area_rect.size = AREA_SIZE_NORMAL
			invulnerable = false
		-1:
			body_rect.size = Vector2(BOX_SIZE_NORMAL.y, BOX_SIZE_NORMAL.x)
			area_rect.size = Vector2(AREA_SIZE_NORMAL.y, AREA_SIZE_NORMAL.x)
			invulnerable = false


func advance_phase(count:int = 1) -> void:
	super(count)
	if phase == 1:
		boss_speed += 0.5


func _draw() -> void:
	if Engine.is_editor_hint() or Statics.show_invis_entites:
		for tgt in stomp_targets:
			var pos:Vector2 = tgt.position
			if tgt.global_space:
				pos -= position
			draw_circle(pos, 8, Color(0.1, 0.75, 0.4, 0.3), true)
			if tgt.data >= 1 and tgt.data <= 15:
				if tgt.data & 1 > 0:
					draw_line(pos, pos + Vector2(0, 8), Color(0.1, 0.85, 0.25, 0.7), 1.0)
				if tgt.data & 2 > 0:
					draw_line(pos, pos + Vector2(-8, 0), Color(0.1, 0.85, 0.25, 0.7), 1.0)
				if tgt.data & 4 > 0:
					draw_line(pos, pos + Vector2(0, -8), Color(0.1, 0.85, 0.25, 0.7), 1.0)
				if tgt.data & 8 > 0:
					draw_line(pos, pos + Vector2(8, 0), Color(0.1, 0.85, 0.25, 0.7), 1.0)
#endregion


#region AI updating
func _update_intro() -> void:
	if mode_elapsed > INTRO_DELAY and not mode_initialized:
		mode_initialized = true
		GameCore.instance.music_manager.play_song(battle_music)
		health_bar._refill()
		GameCore.instance.current_room.set_ground_collision(false)
	if mode_elapsed > INTRO_END:
		can_damage = true
		_set_mode(BossMode.STOMP)


func _update_stomp(delta:float) -> void:
	if not mode_initialized:
		mode_initialized = true
		mode_timeout = MODE_TIMEOUT_STOMP
		_pick_stomp_target()
		_play_anim("shell_stomp_move", "shell", target)
		_set_hitboxes(0)
		_face_player(false)
		_spawn_afterimage()
	if last_state == "shell":
		if position.distance_to(target) < STOMP_TARGET_APPROACH_DIST:
			_face_player(true, true)
			if stomp_gravity == Statics.DirsSurface.LWALL or stomp_gravity == Statics.DirsSurface.RWALL:
				_set_hitboxes(-1)
			else:
				_set_hitboxes(1)
		position = Vector2(
			Statics.integrate(position.x, target.x, 0.7, delta * boss_speed),
			Statics.integrate(position.y, target.y, 0.7, delta * boss_speed)
		)
	else:
		_face_player(true)
		if stomped:
			if not waiting_to_jump:
				waiting_to_jump = true
				jump_timeout = JUMP_TIMEOUT
			jump_timeout -= delta * boss_speed
			if waiting_to_jump and jump_timeout <= 0.0:
				waiting_to_jump = false
				stomped = false
				var real = self
				if real is CharacterBody2D:
					match stomp_gravity:
						Statics.DirsSurface.FLOOR:
							real.velocity.y = -JUMP_POWER
							real.velocity.x = -WALK_SPEED if facing_left else WALK_SPEED
						Statics.DirsSurface.LWALL:
							real.velocity.x = JUMP_POWER
							real.velocity.y = -WALK_SPEED if facing_left else WALK_SPEED
						Statics.DirsSurface.RWALL:
							real.velocity.x = -JUMP_POWER
							real.velocity.y = WALK_SPEED if facing_left else -WALK_SPEED
						Statics.DirsSurface.CEILING:
							real.velocity.y = JUMP_POWER
							real.velocity.x = WALK_SPEED if facing_left else -WALK_SPEED
				grav_jump_timeout = GRAV_JUMP_TIMEOUT
				jump_timeout = 99999.0
				_play_anim("stomp_jump", "stomp")
				sfx_jump.play()
		elif not stomped and phase > 0 and STOMP_ALLOW_FLIPS:
			grav_jump_timeout -= delta
			if grav_jump_timeout <= 0.0:
				if _get_decision() > 0.66:
					match stomp_gravity:
						Statics.DirsSurface.FLOOR:
							stomp_gravity = Statics.DirsSurface.CEILING
						Statics.DirsSurface.LWALL:
							stomp_gravity = Statics.DirsSurface.RWALL
						Statics.DirsSurface.RWALL:
							stomp_gravity = Statics.DirsSurface.LWALL
						Statics.DirsSurface.CEILING:
							stomp_gravity = Statics.DirsSurface.FLOOR
					_play_anim("stomp_flip", "stomp")
					sfx_gravjump.play()
				grav_jump_timeout = 99999.0
	if ((phase == 0 and mode_elapsed > START_ATTACK_TIME * 2.5)
	or (phase > 0 and mode_elapsed > START_ATTACK_TIME * 3.2)):
		_shoot_wave()
	if mode_timeout <= 0.0:
		if phase > 0 and _get_decision() > 0.7:
			_set_mode(BossMode.SLEEP)
		elif _get_decision() > 0.7:
			_set_mode(BossMode.SMASH)
		elif _get_decision() > 0.8:
			_set_mode(BossMode.STOMP)
		else:
			_set_mode(BossMode.STRAFE)


func _update_strafe(delta:float) -> void:
	if not mode_initialized:
		mode_initialized = true
		mode_timeout = MODE_TIMEOUT_STRAFE
		_play_anim("shell_strafe_charge", "shell")
		_set_hitboxes(0)
		target = origin
		aimed = false
		_spawn_afterimage()
	position = Vector2(
		Statics.integrate(position.x, target.x, 1.7, delta * boss_speed),
		Statics.integrate(position.y, target.y, 1.7, delta * boss_speed)
	)
	if (mode_elapsed > START_ATTACK_TIME and not aimed
	and position.distance_to(target) < STRAFE_TARGET_PROX):
		var aim_angle:float = atan2(
			Player.instance.position.y - position.y,
			Player.instance.position.x - position.x
		)
		strafe_spoke_count = roundi(2.3 + 5.0 * (max_health - health) / max_health)
		strafe_spoke_count = clampi(strafe_spoke_count, 2, 7)
		strafe_theta = aim_angle - PI / strafe_spoke_count
		if _get_decision() > 0.5:
			strafe_theta_vel = PI / 8.0
		else:
			strafe_theta_vel = -PI / 8.0
		aimed = true
		if Statics.current_profile["difficulty"] == 2:
			strafe_theta_vel *= 1.6
	strafe_theta += strafe_theta_vel * delta * boss_speed
	strafe_theta_vel += strafe_theta_accel * delta * boss_speed
	if mode_elapsed > START_ATTACK_TIME and aimed and strafe_timeout <= 0.0:
		strafe_timeout = STRAFE_TIMEOUT
		_shoot_360_cluster(pea, strafe_theta, STRAFE_SPEED, strafe_spoke_count)
	if mode_timeout < 0.0:
		if phase > 0 and _get_decision() > 0.74:
			_set_mode(BossMode.SLEEP)
		elif _get_decision() < 0.77:
			_set_mode(BossMode.STOMP)
		else:
			_set_mode(BossMode.SMASH)


func _update_smash() -> void:
	var real = self
	if real is CharacterBody2D and not mode_initialized:
		mode_initialized = true
		mode_timeout = MODE_TIMEOUT_SMASH
		_pick_smash_target(false)
		_play_anim("shell_smash_move", "shell", position + smash_dir)
		real.velocity = Vector2.ZERO
		real.up_direction = Vector2.UP
		_spawn_afterimage()
	if stomped:
		stomped = false
		if _get_decision() > 0.7 or phase > 0:
			_pick_smash_target(true)
		elif last_hit_dir == Statics.DirsSurface.FLOOR or last_hit_dir == Statics.DirsSurface.CEILING:
			smash_dir.y *= -1.0
		else:
			smash_dir.x *= -1.0
		_play_anim("shell_smash_move", "shell", position + smash_dir)
	if mode_timeout <= 0.0:
		if _get_decision() > 0.5:
			_set_mode(BossMode.STOMP)
		else:
			_set_mode(BossMode.STRAFE)


func _update_sleep(delta:float) -> void:
	var real = self
	if real is CharacterBody2D and not mode_initialized:
		if (position.x - Player.instance.position.x < SLEEP_X_DIFF_FORGIVENESS
		and Player.instance.position.y > position.y):
			_set_mode(BossMode.STOMP)
			return
		mode_initialized = true
		mode_timeout = MODE_TIMEOUT_SLEEP
		_play_anim("shell_sleep_fall", "sleep")
		zzz_i = 0
		zzz_count = ZZZ_MAX
		zzz_timeout = 0.0
		if Statics.current_profile["difficulty"] == 2:
			zzz_count += 2
			mode_timeout *= 1.23
		real.velocity = Vector2.ZERO
		sfx_sleep.play()
		_set_hitboxes(0)
		real.up_direction = Vector2.UP
		_spawn_afterimage()
	if stomped:
		zzz_timeout -= delta * boss_speed
		if zzz_timeout <= 0.0 and zzz_count > 0:
			_shoot(zzz, Vector2(position.x + 40.0 + ZZZ_BUFFER * zzz_i, position.y), 3.0)
			zzz_timeout = ZZZ_TIMEOUT
			zzz_count -= 1
			zzz_i += 1
	if mode_timeout <= 0.0:
		if _get_decision() > 0.5:
			_set_mode(BossMode.STOMP)
		else:
			_set_mode(BossMode.STRAFE)
#endregion



#region Movement handling
func _face_player(play_anim:bool, unshell:bool = false) -> void:
	var real = self
	if real is CharacterBody2D:
		var old_left:bool = facing_left
		match stomp_gravity:
			Statics.DirsSurface.FLOOR:
				facing_left = Player.instance.position.x < position.x
				real.up_direction = Vector2.UP
			Statics.DirsSurface.LWALL:
				facing_left = Player.instance.position.y < position.y
				real.up_direction = Vector2.RIGHT
			Statics.DirsSurface.RWALL:
				facing_left = Player.instance.position.y > position.y
				real.up_direction = Vector2.LEFT
			Statics.DirsSurface.CEILING:
				facing_left = Player.instance.position.x > position.x
				real.up_direction = Vector2.DOWN
		if play_anim:
			if unshell:
				_play_anim("stomp_unshell", "stomp")
			elif facing_left != old_left:
				_play_anim("stomp_turnground" if stomped else "stomp_turnair", "stomp")


func _stomp() -> void:
	var real = self
	if real is CharacterBody2D:
		real.velocity = Vector2.ZERO
		if stomp_timeout <= 0.0:
			var stomp_dir:Vector2 = Vector2.DOWN
			match stomp_gravity:
				Statics.DirsSurface.LWALL:
					stomp_dir = Vector2.LEFT
				Statics.DirsSurface.RWALL:
					stomp_dir = Vector2.RIGHT
				Statics.DirsSurface.CEILING:
					stomp_dir = Vector2.UP
			UICore.instance.call_screen_shake_linear(
				SHAKE_TIMELINE, stomp_dir, UICore.ShakeCallMode.OVERWRITE_ALL)
			sfx_stomp.play()
			stomp_timeout = STOMP_TIMEOUT
			boss_environment.impact_ground(position)
		if mode == BossMode.STOMP:
			_play_anim("stomp_land", "stomp")
		stomped = true
		grav_jump_timeout = 99999.0
#endregion
