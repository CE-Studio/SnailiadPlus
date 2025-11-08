class_name Stompy
extends Boss


#region Variables
const RAISED_Y:float = -220.0
const STOMP_Y:float = 0.0
const MIN_DIST:float = 36.0
const SEC_PER_TICK:float = 0.01
const MIN_EYE_Y:float = 0.0
const SYNC_MODE_TIMEOUT:float = 3.0
const STEP_MODE_TIMEOUT:float = 9.0
const STEP_RADIUS:float = 40.0
const TIMEOUTS:Array = [
	0.60153, 0.48509, 0.70037, 0.66276, 0.70802, 0.79541, 0.62043, 0.5796,  0.99605, 0.15058,
	0.72121, 0.86851, 0.64371, 0.76708, 0.89401, 0.52828, 0.72309, 0.15963, 0.15116, 0.1799,
	0.27829, 0.40878, 0.92538, 0.45074, 0.18865, 0.59797, 0.4318,  0.94098, 0.23463, 0.29221,
	0.59734, 0.34877, 0.81676, 0.57617, 0.14883, 0.16094, 0.14123, 0.57931, 0.85924, 0.22828,
	0.63834, 0.10387, 0.54746, 0.24897, 0.11105, 0.49748, 0.54746, 0.19405, 0.79792, 0.36023,
	0.53726, 0.78544, 0.60425, 0.83512, 0.01696, 0.10451, 0.01513, 0.78678, 0.51617, 0.24251
]

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
var attack_mode:int = 0
var boss_speed:float = 0.6
var elapsed:float = 0.0
var boss_mode:BossMode = BossMode.INTRO
var next_step_is_left:bool = false
var step_dir_is_left:bool = false
var step_mode_timeout:float = 0.0

var mode_l:FootMode = FootMode.NONE
var vel_l:Vector2 = Vector2.ZERO
var target_l:Vector2 = Vector2.ZERO
var step_origin_l:Vector2 = Vector2.ZERO
var theta_l:float = 0.0
var step_theta_l:float = 0.0
var stomp_timeout_l:float = 0.0
var stomp_timeout_index_l:int = 23
var raise_timeout_l:float = 0.0

var mode_r:FootMode = FootMode.NONE
var vel_r:Vector2 = Vector2.ZERO
var target_r:Vector2 = Vector2.ZERO
var step_origin_r:Vector2 = Vector2.ZERO
var theta_r:float = 0.0
var step_theta_r:float = 0.0
var stomp_timeout_r:float = 0.0
var stomp_timeout_index_r:int = 34
var raise_timeout_r:float = 0.0

@onready var foot_l:StompyFoot = $"FootL"
@onready var foot_r:StompyFoot = $"FootR"
@onready var eye_l:StompyEye = $"EyeL"
@onready var eye_r:StompyEye = $"EyeR"
#endregion


func _ready() -> void:
	my_type = EnemyTypes.STOMPY
	super.spawn()
	
	eye_l.boss = self
	eye_l.my_foot = foot_l
	eye_r.boss = self
	eye_r.my_foot = foot_r
