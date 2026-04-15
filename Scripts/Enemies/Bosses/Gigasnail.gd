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
const INTRO_DELAY:float = 1.25
const INTRO_END:float = 4.0
const STRAFE_TIMEOUT:float = 0.03
const STRAFE_SPEED:float = 400.0
const SMASH_SPEED:float = 400.0
const STOMP_TIMEOUT:float = 0.25
const STOMP_TARGET_MIN_DIST:float = 130.0
const STOMP_TARGET_APPROACH_DIST:float = 10.0
const STOMP_MAX_ATTACKS_SINCE:int = 4
const SHAKE_TIMELINE:Array[float] = [4.0, 0.7]
const BOX_SIZE_NORMAL:Vector2i = Vector2i(80, 44)
const BOX_SIZE_SHELL:Vector2i = Vector2i(44, 44)
const AREA_SIZE_NORMAL:Vector2i = Vector2i(80, 44)
const AREA_SIZE_SHELL:Vector2i = Vector2i(44, 44)
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
var waiting_to_jump:bool = false
var stomp_timeout:float = 0.0
var wave_timeout:float = 0.0
var stomped:bool = false
var aimed:bool = false
var facing_left:bool = false
var grav_jump_timeout:float = 99999.0
var jump_timeout:float = 0.0
var boss_speed:float = 1.0

var update_if_shell:bool = false
var afterimage_y:Array = []

var boss_environment:GigaEnvironment

var body_rect:RectangleShape2D = null
var area_rect:RectangleShape2D = null

@export var stomp_targets:Array[EntityTarget]

@onready var sfx_jump:AudioStreamPlayer = $"AudioGroup/Jump"
@onready var sfx_gravjump:AudioStreamPlayer = $"AudioGroup/GravJump"
@onready var sfx_stomp:AudioStreamPlayer = $"AudioGroup/Stomp"
@onready var sfx_sleep:AudioStreamPlayer = $"AudioGroup/Sleep"
@onready var wave:PackedScene = preload("res://Scenes/Entities/Bullets/Enemy/EnemyBulletGigaWave.tscn")
#endregion


func _ready() -> void:
	my_type = EnemyTypes.GIGASNAIL
	col = $"BodyBox"
	sprite = $"JsonSprite2D"
	hitbox = $"Area2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	if display_mode:
		#sprite.action = "p0_floor_left_idle" if randf() < 0.5 else "p0_floor_right_idle"
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
	
	if Statics.current_profile["difficulty"] == 2:
		boss_speed += 0.2
	
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


func _physics_process(delta) -> void:
	if Statics.show_invis_entites:
		queue_redraw()
	if Engine.is_editor_hint():
		return
	if not ai_active:
		#if in_death_anim:
		#	_tick_death(delta)
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
			_update_strafe()
		BossMode.SMASH:
			_update_smash()
		BossMode.SLEEP:
			_update_sleep()
	
	if mode == BossMode.STOMP and not stomped and last_state != "shell":
		var last_vel:Vector2 = velocity
		move_and_slide()
		match stomp_gravity:
			Statics.DirsSurface.FLOOR:
				velocity.y += GRAVITY * delta
				if is_on_wall():
					velocity.x = -last_vel.x
					position.x += 0.5 * sign(velocity.x)
			Statics.DirsSurface.LWALL:
				velocity.x -= GRAVITY * delta
				if is_on_wall():
					velocity.y = -last_vel.y
					position.y += 0.5 * sign(velocity.y)
			Statics.DirsSurface.RWALL:
				velocity.x += GRAVITY * delta
				if is_on_wall():
					velocity.y = -last_vel.y
					position.y += 0.5 * sign(velocity.y)
			Statics.DirsSurface.FLOOR:
				velocity.y -= GRAVITY * delta
				if is_on_wall():
					velocity.x = -last_vel.x
					position.x += 0.5 * sign(velocity.x)
		if is_on_floor():
			_stomp()
	
	super._physics_process(delta)


#region General utility
func _get_decision() -> float:
	decision_table_index = (decision_table_index + 1) % DECISION_TABLE.size()
	return DECISION_TABLE[decision_table_index]


func _set_mode(new_mode:BossMode) -> void:
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
	velocity = Vector2.ZERO
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


func _play_anim(action:String, shell:bool, dir_target:Vector2 = Vector2.ZERO) -> void:
	if last_state == "shell" and shell and not update_if_shell:
		return
	last_state = "shell" if shell else "stomp"
	
	var full_action:String = action
	if not shell:
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
		last_anim = full_action


func _set_hitboxes(state:int) -> void:
	match state:
		0:
			body_rect.size = BOX_SIZE_SHELL
			area_rect.size = AREA_SIZE_SHELL
		1:
			body_rect.size = BOX_SIZE_NORMAL
			area_rect.size = AREA_SIZE_NORMAL
		-1:
			body_rect.size = Vector2(BOX_SIZE_NORMAL.y, BOX_SIZE_NORMAL.x)
			area_rect.size = Vector2(AREA_SIZE_NORMAL.y, AREA_SIZE_NORMAL.x)


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
		_set_mode(BossMode.STOMP)


func _update_stomp(delta:float) -> void:
	if not mode_initialized:
		mode_initialized = true
		mode_timeout = 6.0
		_pick_stomp_target()
		_play_anim("shell_stomp_move", true, target)
		_set_hitboxes(0)
	if last_state == "shell":
		if position.distance_to(target) < STOMP_TARGET_APPROACH_DIST:
			_face_player(true)
			if stomp_gravity == Statics.DirsSurface.LWALL or stomp_gravity == Statics.DirsSurface.RWALL:
				_set_hitboxes(-1)
			else:
				_set_hitboxes(1)
		position = Vector2(
			Statics.integrate(position.x, target.x, 0.7, delta * boss_speed),
			Statics.integrate(position.y, target.y, 0.7, delta * boss_speed)
		)
	else:
		_face_player()
		if stomped:
			if not waiting_to_jump:
				waiting_to_jump = true
				jump_timeout = JUMP_TIMEOUT
			jump_timeout -= delta * boss_speed
			if waiting_to_jump and jump_timeout <= 0.0:
				waiting_to_jump = false
				stomped = false
				match stomp_gravity:
					Statics.DirsSurface.FLOOR:
						velocity.y = -JUMP_POWER
						velocity.x = -WALK_SPEED if facing_left else WALK_SPEED
					Statics.DirsSurface.LWALL:
						velocity.x = JUMP_POWER
						velocity.y = -WALK_SPEED if facing_left else WALK_SPEED
					Statics.DirsSurface.RWALL:
						velocity.x = -JUMP_POWER
						velocity.y = WALK_SPEED if facing_left else -WALK_SPEED
					Statics.DirsSurface.CEILING:
						velocity.y = JUMP_POWER
						velocity.x = WALK_SPEED if facing_left else -WALK_SPEED
				grav_jump_timeout = GRAV_JUMP_TIMEOUT
				jump_timeout = 99999.0
				_play_anim("stomp_jump", false)
				sfx_jump.play()
		elif not stomped and phase > 0:
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
				grav_jump_timeout = 99999.0
				_play_anim("stomp_flip", false)
				sfx_gravjump.play()
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


func _update_strafe() -> void:
	pass


func _update_smash() -> void:
	pass


func _update_sleep() -> void:
	pass
#endregion



#region Movement handling
func _face_player(unshell:bool = false) -> void:
	var old_left:bool = facing_left
	match stomp_gravity:
		Statics.DirsSurface.FLOOR:
			facing_left = Player.instance.position.x < position.x
			up_direction = Vector2.UP
		Statics.DirsSurface.LWALL:
			facing_left = Player.instance.position.y < position.y
			up_direction = Vector2.RIGHT
		Statics.DirsSurface.RWALL:
			facing_left = Player.instance.position.y > position.y
			up_direction = Vector2.LEFT
		Statics.DirsSurface.CEILING:
			facing_left = Player.instance.position.x > position.x
			up_direction = Vector2.DOWN
	if unshell:
		_play_anim("stomp_unshell", false)
	elif facing_left != old_left:
		_play_anim("stomp_turnground" if stomped else "stomp_turnair", false)


func _stomp() -> void:
	velocity = Vector2.ZERO
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
	_play_anim("stomp_land", false)
	stomped = true
	grav_jump_timeout = 99999.0
#endregion
