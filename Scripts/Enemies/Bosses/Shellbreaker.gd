class_name Shellbreaker
extends Boss


#region Variables
var HAND_COUNT:int = 3
var SHOT_COUNT:int = 5
var SHOT_DELAY:float = 0.8
const SHOT_DELAY_MULTS:Array = [ 0.6, 0.28, 0.1 ]
const PATTERN_DELAY:float = 3.0
const WEAPON_SPEED:float = 270.0
const PATH_RADIUS:Vector2 = Vector2(144, 112)
const PATH_RADIUS_CYCLE_MULT:float = 0.4286

var hand_theta:float = 0.0
var hand_speed:float = 0.0
var hand_radius:float = 0.0
var hand_radius_mult:float = 1.0
var hand_radius_target:float = 1.0
var elapsed:float = 0.0
var fire_pattern:int = 0
var fire_timeout:float = 0.0
var fire_pattern_timeout:float = 0.0
var is_firing:bool = false

@onready var eyes:JsonSprite2D = $"Eyes"
@onready var hand_group:Node2D = $"HandGroup"
#endregion


func _ready() -> void:
	my_type = EnemyTypes.SHELLBREAKER
	col = $"BodyBox"
	sprite = $"Body"
	hitbox = $"Area2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	if hard_mode:
		HAND_COUNT *= 4
		SHOT_COUNT *= 3
		SHOT_DELAY /= 3
	call_deferred("play_phase_anim", "idle")


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	elapsed += delta
	
	position = origin + (PATH_RADIUS * Vector2(
		cos(elapsed),
		sin(elapsed)
	) * sin(elapsed * PATH_RADIUS_CYCLE_MULT))
	var eye_pos_val:float = get_aim_dir()
	eyes.position = Vector2(
		cos(eye_pos_val),
		sin(eye_pos_val)
	) * -2.5
	try_fire()


func try_fire() -> void:
	if intro_delay:
		return
	var aim_dir:float = get_aim_dir()
	if fire_pattern_timeout <= 0:
		if not is_firing:
			is_firing = true
			play_phase_anim("shoot_start")
		hand_radius_target = 0.0
		if fire_timeout <= 0.0:
			fire_timeout = SHOT_DELAY * SHOT_DELAY_MULTS[phase]
			


func play_phase_anim(anim_name:String = "") -> String:
	anim_name = super.play_phase_anim(anim_name)
	eyes.action = anim_name
	return anim_name


func get_aim_dir() -> float:
	return atan2(
		position.y - GameCore.instance.player.position.y,
		position.x - GameCore.instance.player.position.x
	)
