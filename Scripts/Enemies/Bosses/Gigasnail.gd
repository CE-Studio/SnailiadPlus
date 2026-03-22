class_name Gigasnail
extends Boss


#region Variables
const GRAV_JUMP_TIMEOUT:float = 0.2
const START_ATTACK_TIME:float = 0.45
const JUMP_POWER:float = 360.0
const JUMP_TIMEOUT:float = 0.8
const WALK_SPEED:float = 200.0
const WAVE_SPEED:float = 30.0
const WAVE_TIMEOUT:float = 0.9
const ZZZ_TIMEOUT:float = 0.3
const ZZZ_MAX:int = 3
const STRAFE_TIMEOUT:float = 0.03
const STRAFE_SPEED:float = 400.0
const SMASH_SPEED:float = 400.0
const STOMP_TIMEOUT:float = 0.25
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

var last_hit_dir:Statics.DirsSurface = Statics.DirsSurface.NONE
var stomps_this_cycle:int = 0
var strafe_spoke_count:int = 2
var last_smash_vel:Vector2 = Vector2.ZERO
var decision_table_index:int = 0
var shot_timeout:float = 0.0
var last_anim:String = ""
var strafe_theta:float = 0.0
var strafe_theta_vel:float = 0.0
var strafe_theta_accel:float = 0.0
var strafe_timeout:float = 0.0
var waiting_to_jump:bool = false
var stomp_timeout:float = 0.0
var wave_timeout:float = 0.0
var stomped:bool = false
var aimed:bool = false
var grav_jump_timeout:float = 99999.0
var jump_timeout:float = 0.0

var body_rect:RectangleShape2D
var area_rect:RectangleShape2D
#endregion


func _ready() -> void:
	my_type = EnemyTypes.GIGASNAIL
	col = $"BodyBox"
	sprite = $"JsonSprite2D"
	hitbox = $"Area2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	if col and col.shape and col.shape is RectangleShape2D:
		body_rect = col.shape
	var _hitbox:CollisionShape2D = $"Area2D/HitBox"
	if _hitbox and _hitbox.shape and _hitbox.shape is RectangleShape2D:
		area_rect = _hitbox.shape
	
	GameCore.instance.current_room.layer_ground.add_child(
		load("res://Scenes/Environments/Backgrounds/GigaBackground.tscn").instantiate()
	)
