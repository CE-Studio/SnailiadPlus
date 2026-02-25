extends Boss


#region Variables
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
const JUMP_NORMAL:float = 428.0
const JUMP_HIGH:float = 920.0
const RUN_SPEED:float = 370.0
const MAX_SPEED:float = 600.0
const GRAVITY:float = 1200.0
const WEAPON_COOLDOWNS:Array[float] = [ 0.1, 0.3, 0.155 ]
const WEAPON_SPEED:Array[float] = [ 280.0, 330.0, 140.0 ]
const MOVE_TARGET_THRESHOLD:float = 60.0
const MOVE_END_THRESHOLD:float = 10.0

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
var jump_release_timeout:float = 0.0
var boss_speed:float = 1.0
var action_timeout:float = 0.0
var move_start:Vector2 = Vector2.ZERO
var move_end:Vector2 = Vector2.ZERO
var tele_start:Vector2 = Vector2.ZERO
var tele_end:Vector2 = Vector2.ZERO
var attacking:bool = false
var attack_start_timeout:float = 0.0
var attack_stop_timeout:float = 0.9
var gravity:Statics.DirsSurface = Statics.DirsSurface.FLOOR
var target_gravity:Statics.DirsSurface = Statics.DirsSurface.NONE
var decision_table_index:int = 0
var shadowballs:Array[JsonSprite2D] = []

var frames_left:int = -1
var frames_right:int = -1
var frames_down:int = -1
var frames_up:int = -1
var frames_jump:int = -1
var tapped_left:bool = false
var tapped_right:bool = false
var tapped_down:bool = false
var tapped_up:bool = false

@export var move_targets:Array[EntityTarget] = []
@export var tele_targets:Array[EntityTarget] = []

@onready var sfx_jump = $"AudioGroup/Jump"
@onready var sfx_jumpgrav = $"AudioGroup/JumpGrav"
@onready var sfx_shell = $"AudioGroup/Shell"
@onready var sfx_hurt = $"AudioGroup/Hurt"
@onready var sfx_parry = $"AudioGroup/Parry"
@onready var sfx_death = $"AudioGroup/Die"
@onready var sfx_shockcharge = $"AudioGroup/ShockCharge"
@onready var sfx_shocklaunch = $"AudioGroup/ShockLaunch"
@onready var sfx_shockland = $"AudioGroup/ShockLand"
#endregion


func _ready() -> void:
	if Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS4) == true and not display_mode:
		queue_free()
		return
	
	my_type = EnemyTypes.MOONSNAIL
	col = $"BodyBox"
	sprite = $"JsonSprite2D"
	hitbox = $"Area2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	if Statics.current_profile["difficulty"] == 2:
		boss_speed += 0.1
	for child in $"ShadowballGroup".get_children():
		if child is JsonSprite2D:
			shadowballs.append(child)


#region General utility
func _get_decision() -> float:
	decision_table_index = (decision_table_index + 1) % DECISION_TABLE.size()
	return DECISION_TABLE[decision_table_index]


func _set_mode(new_mode:BossMode, try_shoot:bool) -> void:
	mode = new_mode
	mode_elapsed = 0.0
	mode_initialized = false
	attacking = false
	action_timeout = ACTION_TIMEOUT
	for ball in shadowballs:
		ball.visible = true
	#Release everything
	attack_start_timeout = ATTACK_START_TIMEOUT
	attack_stop_timeout = ATTACK_STOP_TIMEOUT
	if try_shoot:
		_check_shoot_donuts()


func _check_shoot_donuts() -> void:
	if ring_timeout <= 0.0:
		ring_timeout = RING_TIMEOUT
		pass # Shoot donuts


func _pick_move_target() -> void:
	move_start = position
	move_end = position
	if move_targets.size() > 0:
		while move_start.distance_to(move_end) < MOVE_TARGET_THRESHOLD:
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
		while tele_start.distance_to(tele_end) < MOVE_TARGET_THRESHOLD:
			var i:int = floori(_get_decision() * tele_targets.size())
			var target:EntityTarget = tele_targets[i]
			tele_end = tele_targets[i].position
			if not target.global_space:
				tele_end += tele_start
			target_gravity = (absi(target.data) % 4) as Statics.DirsSurface
#endregion


#region AI updating
func _update_move() -> void:
	if not mode_initialized:
		mode_initialized = true
		_pick_move_target()
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


func _press_right() -> void:
	frames_right = 1
	frames_left = -1


func _release_right() -> void:
	frames_right = -1


func _tap_right() -> void:
	_press_right()
	tapped_right = true


func _press_up() -> void:
	frames_up = 1
	frames_down = -1


func _release_up() -> void:
	frames_up = -1


func _tap_up() -> void:
	_press_up()
	tapped_up = true


func _press_down() -> void:
	frames_down = 1
	frames_up = -1


func _release_down() -> void:
	frames_down = -1


func _tap_down() -> void:
	_press_down()
	tapped_down = true


func _press_jump() -> void:
	frames_jump = 1


func _release_jump() -> void:
	frames_jump = -1


func _release_all() -> void:
	_release_left()
	_release_right()
	_release_up()
	_release_down()
#endregion
