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
const ACCEL:float = 210.0
const CLUSTER_TIMEOUT:float = 4.1
const SHOT_TIMEOUTS:Array[float] = [0.6, 0.2]
const SHAKE_TIMELINE:Array[float] = [5.0, 0.7]
const SHIELD_EXTENTS:Vector2 = Vector2(72, 72)

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
var elapsed:float = 0.0
var mode_timeout = MODE_TIMEOUT
var last_mode:Statics.DirsCompass = Statics.DirsCompass.NONE
var current_mode:Statics.DirsCompass = Statics.DirsCompass.NONE
var next_mode:Statics.DirsCompass = Statics.DirsCompass.NONE
var spawn_counter:int = MAX_BABYBOXES
var phase_speed:float = 1.0
var decision_index:int = 0
var is_shooting:bool = false
var shot_max:int = 4
var shot_count:int = 0
var cluster_timeout:float = 0.0
var shot_timeout:float = 0.0

@onready var sfx_summon:AudioStreamPlayer = $"Summon"
@onready var sfx_stomp:AudioStreamPlayer = $"Stomp"
@onready var shield_layer:Node2D = $"ShieldLayer"
@onready var donut:PackedScene = preload("res://Scenes/Entities/Bullets/Enemy/EnemyBulletDonutRotary.tscn")
@onready var shield_scn:PackedScene = preload("res://Scenes/Entities/Enemies/Bosses/SpaceboxShield.tscn")
#endregion


func _ready() -> void:
	if Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS3) == true and not display_mode:
		queue_free()
		return
	
	my_type = EnemyTypes.SPACEBOX
	col = $"BodyBox"
	sprite = $"JsonSprite2D"
	hitbox = $"Area2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	if display_mode:
		sprite.action = "display"
		return
	if hard_mode:
		shot_max += 2
		phase_speed += 0.2
	
	nodes_to_wiggle.append(sprite)


func _physics_process(delta: float) -> void:
	super(delta)
	
	elapsed += delta
	#check_mode
	#check_shoot
	check_add_shields()
	update_shield_positions()


func play_phase_anim(anim_name:String = "", set_as_current:bool = true) -> String:
	super.play_phase_anim(anim_name, set_as_current)
	return anim_name


func advance_phase(count:int = 1) -> void:
	super(count)
	if phase == 1:
		phase_speed += 0.5


func _get_decision() -> float:
	decision_index = (decision_index + 1) % DECISION_TABLE.size()
	return DECISION_TABLE[decision_index]


func stomp() -> void:
	if velocity.length() > 100.0:
		sfx_stomp.play()
		match current_mode:
			Statics.DirsCompass.N or Statics.DirsCompass.S:
				UICore.instance.call_screen_shake_linear(SHAKE_TIMELINE, Vector2.UP, UICore.ShakeCallMode.OVERWRITE_ALL)
			Statics.DirsCompass.NE or Statics.DirsCompass.SW:
				UICore.instance.call_screen_shake_linear(SHAKE_TIMELINE, Vector2(1, -1).normalized(), UICore.ShakeCallMode.OVERWRITE_ALL)
			Statics.DirsCompass.E or Statics.DirsCompass.W:
				UICore.instance.call_screen_shake_linear(SHAKE_TIMELINE, Vector2.RIGHT, UICore.ShakeCallMode.OVERWRITE_ALL)
			Statics.DirsCompass.SE or Statics.DirsCompass.NW:
				UICore.instance.call_screen_shake_linear(SHAKE_TIMELINE, Vector2(1, 1).normalized(), UICore.ShakeCallMode.OVERWRITE_ALL)
	velocity = Vector2.ZERO
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
		cycle_point = fmod(fmod(elapsed / SHIELD_PERIOD, 1.0) * MAX_SHIELD_SLOTS + fmod(17.0 * (i + 8), MAX_SHIELDS_ACTIVE), MAX_SHIELD_SLOTS)
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
