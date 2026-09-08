# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name Spacebox
extends Boss


#region Variables
const MAX_BABYBOXES:int = 8
const MAX_SHIELD_SLOTS:int = 36
const MAX_SHIELDS_ACTIVE:int = 26
const SHIELD_START_SLOT:int = 0
const SHIELD_DAMAGE_THRESHOLD:int = 100
const SHIELD_PERIOD:int = 4
const MODE_TIMEOUT:float = 0.6
const SPAWN_TIMEOUT:float = 2.5
const SPAWN_COUNTER:int = 8
const ACCEL:float = 210.0
const CLUSTER_TIMEOUT:float = 4.1
const SHOT_TIMEOUTS:Array[float] = [0.6, 0.2]
const SHAKE_TIMELINE:Array[float] = [4.0, 0.7]
const SHIELD_EXTENTS:Vector2 = Vector2(72, 72)
const DIAG_SLOPE:Vector2 = Vector2(22.5, 19.0)

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

var shields:Array[Enemy] = []
var babyboxes:Array[Enemy] = []
var shield_count:int = 0
var mode_timeout = MODE_TIMEOUT
var last_mode:Statics.DirsCompass = Statics.DirsCompass.W
var current_mode:Statics.DirsCompass = Statics.DirsCompass.NONE
var next_mode:Statics.DirsCompass = Statics.DirsCompass.NONE
var spawn_counter:int = MAX_BABYBOXES - 1
var boss_speed:float = 1.0
var decision_index:int = 0
var is_shooting:bool = false
var shot_max:int = 4
var shot_count:int = 0
var cluster_timeout:float = 0.0
var shot_timeout:float = 0.0
var accel_dir:Vector2 = Vector2.ZERO

@onready var sfx_move:AudioStreamPlayer = $"Move"
@onready var sfx_summon:AudioStreamPlayer = $"Summon"
@onready var sfx_stomp:AudioStreamPlayer = $"Stomp"
@onready var shield_layer:Node2D = $"ShieldLayer"
@onready var donut:PackedScene = load("uid://cpp1rm5lkd443")
@onready var shield_scn:PackedScene = load("uid://ka2xpbxbm32q")
@onready var babybox_scn:PackedScene = load("uid://4w6iyanqj1ks")
#endregion


func _ready() -> void:
	if Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS3) == true and not display_mode:
		queue_free()
		return
	
	my_type = EnemyTypes.SPACEBOX
	super.spawn()
	
	if display_mode:
		sprite.play("display")
		z_index = 0
		return
	else:
		if not Statics.is_in_boss_rush:
			GameCore.instance.music_manager.play_song(battle_music)
		health_bar = UICore.instance.show_boss_bar(self)
		can_damage = false
	if hard_mode:
		shot_max += 2
		boss_speed += 0.2
	
	nodes_to_wiggle.append(sprite)


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	if not intro_delay:
		check_mode(delta)
		check_shoot(delta)
		check_add_shields()
		update_shield_positions()
	body.velocity += accel_dir * ACCEL * delta
	var stomp_vel:Vector2 = body.velocity
	if stomp_vel != Vector2.ZERO and body.move_and_slide() and stomp_vel != body.velocity:
		stomp(stomp_vel)
	
	for i in range(babyboxes.size() - 1, -1, -1):
		if babyboxes[i] == null:
			babyboxes.remove_at(i)


func play_phase_anim(anim_name:String = "", set_as_current:bool = true) -> String:
	super.play_phase_anim(anim_name, set_as_current)
	return anim_name


func advance_phase(count:int = 1) -> void:
	super(count)
	if phase == 1:
		boss_speed += 0.5
	for babybox in babyboxes:
		if babybox and babybox is SpaceboxBabybox:
			babybox.play_phase_anim()


func enable_damage() -> void:
	can_damage = true


func _get_decision() -> float:
	decision_index = (decision_index + 1) % DECISION_TABLE.size()
	return DECISION_TABLE[decision_index]


func stomp(impact_vel:Vector2) -> void:
	var impact:bool = false
	if impact_vel.length() > 100.0:
		impact = true
		sfx_stomp.play()
	match current_mode:
		Statics.DirsCompass.N:
			if impact:
				UICore.instance.call_screen_shake_linear(SHAKE_TIMELINE, Vector2.UP, UICore.ShakeCallMode.OVERWRITE_ALL)
			play_phase_anim("U_land")
		Statics.DirsCompass.NE:
			if impact:
				UICore.instance.call_screen_shake_linear(SHAKE_TIMELINE, Vector2(1, -1).normalized(), UICore.ShakeCallMode.OVERWRITE_ALL)
			play_phase_anim("UR_land")
		Statics.DirsCompass.E:
			if impact:
				UICore.instance.call_screen_shake_linear(SHAKE_TIMELINE, Vector2.RIGHT, UICore.ShakeCallMode.OVERWRITE_ALL)
			play_phase_anim("R_land")
		Statics.DirsCompass.SE:
			if impact:
				UICore.instance.call_screen_shake_linear(SHAKE_TIMELINE, Vector2(1, 1).normalized(), UICore.ShakeCallMode.OVERWRITE_ALL)
			play_phase_anim("DR_land")
		Statics.DirsCompass.S:
			if impact:
				UICore.instance.call_screen_shake_linear(SHAKE_TIMELINE, Vector2.DOWN, UICore.ShakeCallMode.OVERWRITE_ALL)
			play_phase_anim("D_land")
		Statics.DirsCompass.SW:
			if impact:
				UICore.instance.call_screen_shake_linear(SHAKE_TIMELINE, Vector2(-1, 1).normalized(), UICore.ShakeCallMode.OVERWRITE_ALL)
			play_phase_anim("DL_land")
		Statics.DirsCompass.W:
			if impact:
				UICore.instance.call_screen_shake_linear(SHAKE_TIMELINE, Vector2.LEFT, UICore.ShakeCallMode.OVERWRITE_ALL)
			play_phase_anim("L_land")
		Statics.DirsCompass.NW:
			if impact:
				UICore.instance.call_screen_shake_linear(SHAKE_TIMELINE, Vector2(-1, -1).normalized(), UICore.ShakeCallMode.OVERWRITE_ALL)
			play_phase_anim("UL_land")
	position += impact_vel.normalized() * -0.25
	body.velocity = Vector2.ZERO
	accel_dir = Vector2.ZERO
	last_mode = current_mode
	current_mode = Statics.DirsCompass.NONE
	mode_timeout = MODE_TIMEOUT


func check_add_shields() -> void:
	var expected_count:int = floori((max_health - health) / SHIELD_DAMAGE_THRESHOLD)
	if expected_count > MAX_SHIELDS_ACTIVE:
		expected_count = MAX_SHIELDS_ACTIVE
	while expected_count > shield_count:
		var shield:Node2D = shield_scn.instantiate()
		shield_layer.add_child(shield)
		shields.append(shield)
		shield_count += 1


func update_shield_positions() -> void:
	var cycle_point:float
	var segment_point:float
	for i in range(shields.size()):
		cycle_point = fmod(fmod(lifetime / SHIELD_PERIOD, 1.0) * MAX_SHIELD_SLOTS + fmod(17.0 * (i + 8), MAX_SHIELDS_ACTIVE), MAX_SHIELD_SLOTS)
		segment_point = fmod(cycle_point, 9.0)
		if cycle_point < 9.0:
			shields[i].position.x = -SHIELD_EXTENTS.x + (segment_point * 16.0)
			shields[i].position.y = -SHIELD_EXTENTS.y
		elif cycle_point < 18.0:
			shields[i].position.x = SHIELD_EXTENTS.x
			shields[i].position.y = -SHIELD_EXTENTS.y + (segment_point * 16.0)
		elif cycle_point < 27.0:
			shields[i].position.x = SHIELD_EXTENTS.x - (segment_point * 16.0)
			shields[i].position.y = SHIELD_EXTENTS.y
		elif cycle_point < 36.0:
			shields[i].position.x = -SHIELD_EXTENTS.x
			shields[i].position.y = SHIELD_EXTENTS.y - (segment_point * 16.0)


func make_babyboxes() -> void:
	play_phase_anim("spawn")
	sfx_summon.play()
	current_mode = Statics.DirsCompass.NONE
	mode_timeout = SPAWN_TIMEOUT
	if babyboxes.size() < MAX_BABYBOXES:
		if phase == 0:
			spawn_new_babybox(Vector2.ZERO, true)
			spawn_new_babybox(Vector2.ZERO, false)
		else:
			spawn_new_babybox(Vector2(-32, -32), false)
			spawn_new_babybox(Vector2(32, -32), true)
			spawn_new_babybox(Vector2(32, 32), false)
			spawn_new_babybox(Vector2(-32, 32), true)


func spawn_new_babybox(_position:Vector2, axis:bool) -> void:
	var new_babybox:SpaceboxBabybox = babybox_scn.instantiate()
	new_babybox.boss = self
	new_babybox.last_mode = Statics.DirsCompass.N if axis else Statics.DirsCompass.W
	GameCore.instance.current_room.layer_ground.add_child(new_babybox)
	new_babybox.position = position + _position
	babyboxes.append(new_babybox)


func check_shoot(delta:float) -> void:
	if max_health - health < 1500:
		return
	
	if not is_shooting:
		cluster_timeout -= delta * boss_speed
		if cluster_timeout <= 0.0:
			is_shooting = true
			shot_count = shot_max
			shot_timeout = 0.0
	else:
		shot_timeout -= delta * boss_speed
		if shot_timeout <= 0.0:
			shot_count -= 1
			if shot_count <= 0:
				is_shooting = false
				cluster_timeout = CLUSTER_TIMEOUT
			shot_timeout = SHOT_TIMEOUTS[phase]
			bullets.append(_shoot(donut, Vector2(4.0, TAU / shot_max * shot_count), 60.0))


func check_mode(delta:float) -> void:
	mode_timeout -= delta * boss_speed
	if current_mode == Statics.DirsCompass.NONE and mode_timeout <= 0.0:
		spawn_counter -= 1
		if spawn_counter < 0.0:
			spawn_counter = SPAWN_COUNTER
			make_babyboxes()
		else:
			sfx_move.play()
			var decision:float = _get_decision()
			if last_mode == Statics.DirsCompass.N or last_mode == Statics.DirsCompass.S:
				if hard_mode:
					if decision < 0.2:
						charge_diag()
					elif decision < 0.8:
						charge_horiz()
					else:
						charge_vert()
				else:
					if decision < 0.75:
						charge_horiz()
					else:
						charge_vert()
			elif last_mode == Statics.DirsCompass.W or last_mode == Statics.DirsCompass.E:
				if hard_mode:
					if decision < 0.2:
						charge_diag()
					elif decision < 0.8:
						charge_vert()
					else:
						charge_horiz()
				else:
					if decision < 0.75:
						charge_vert()
					else:
						charge_horiz()
			elif last_mode != Statics.DirsCompass.NONE:
				if decision < 0.5:
					charge_horiz()
				else:
					charge_vert()


#region Charge functions
func charge_n() -> void:
	last_mode = current_mode
	current_mode = Statics.DirsCompass.N
	play_phase_anim("U_move")
	accel_dir = Vector2.UP


func charge_ne() -> void:
	last_mode = current_mode
	current_mode = Statics.DirsCompass.NE
	play_phase_anim("UR_move")
	accel_dir = Vector2(DIAG_SLOPE.x, -DIAG_SLOPE.y).normalized()


func charge_e() -> void:
	last_mode = current_mode
	current_mode = Statics.DirsCompass.E
	play_phase_anim("R_move")
	accel_dir = Vector2.RIGHT


func charge_se() -> void:
	last_mode = current_mode
	current_mode = Statics.DirsCompass.SE
	play_phase_anim("DR_move")
	accel_dir = DIAG_SLOPE.normalized()


func charge_s() -> void:
	last_mode = current_mode
	current_mode = Statics.DirsCompass.S
	play_phase_anim("D_move")
	accel_dir = Vector2.DOWN


func charge_sw() -> void:
	last_mode = current_mode
	current_mode = Statics.DirsCompass.SW
	play_phase_anim("DL_move")
	accel_dir = Vector2(-DIAG_SLOPE.x, DIAG_SLOPE.y).normalized()


func charge_w() -> void:
	last_mode = current_mode
	current_mode = Statics.DirsCompass.W
	play_phase_anim("L_move")
	accel_dir = Vector2.LEFT


func charge_nw() -> void:
	last_mode = current_mode
	current_mode = Statics.DirsCompass.NW
	play_phase_anim("UL_move")
	accel_dir = -DIAG_SLOPE.normalized()


func charge_horiz() -> void:
	if GameCore.instance.player.position.x < position.x:
		charge_w()
	else:
		charge_e()


func charge_vert() -> void:
	if GameCore.instance.player.position.y < position.y:
		charge_n()
	else:
		charge_s()


func charge_diag() -> void:
	var player:Player = GameCore.instance.player
	if player.position.x < position.x:
		if player.position.y < position.y:
			charge_nw()
		else:
			charge_sw()
	else:
		if player.position.y < position.y:
			charge_ne()
		else:
			charge_se()
#endregion


func kill() -> void:
	if not in_death_anim:
		UICore.instance.achievement_core.check_add(AchievementCore.Achievements.BEAT_SPACE_BOX)
		if health_bar:
			health_bar._toggle_outro_shake()
		sprite.play("defeat")
		for _shield in shields:
			_shield.kill()
		shields.clear()
		for _babybox in babyboxes:
			if _babybox:
				_babybox.kill()
		babyboxes.clear()
		Statics.spawn_particle("ExplosionBossDefeat", Room.Layers.GROUND, position)
		GameCore.instance.music_manager.stop_all(true)
		Statics.set_world_flag(Statics.WorldFlags.DEFEATED_BOSS3, true)
	else:
		GameCore.instance.music_manager.play_song(GameCore.instance.current_room.song_change)
	super()
