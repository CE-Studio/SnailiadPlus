class_name Snelk
extends Enemy


#region Variables
const HOP_HEIGHTS:Array = [2.0, 1.0, 1.9, 1.2, 2.0, 0.4, 1.2, 2.0, 0.3]
const GRAVITY:float = 1200.0
const SPEED_NORMAL:float = 100.0
const SPEED_PANIC:float = 140.0
const JUMP_POWER:float = -240.0
const WAKE_RANGE:float = 40.0
const SFX_CHANCE:float = 0.6
const NORMAL_TURN_CHANCE:float = 0.2

enum States {
	NORMAL,
	RUN,
	SLEEP
}

@export var state:States = States.NORMAL
@export_range(0.0, 1.0, 0.01) var spawn_chance:float = 1.0

var hop_num:int = 0
var hop_timeout:float = 0.0
var facing_left:bool = false
var first_jump:bool = false

@onready var sfx:AudioStreamPlayer = $"Sfx"
@onready var sound:AudioStream = load("res://Assets/Sounds/Sfx/Enemy/Snelk.ogg")
#endregion


func _ready() -> void:
	if randf() > spawn_chance:
		queue_free()
		return
	
	my_type = EnemyTypes.SNELK
	col = $"BodyBox"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	if state == States.SLEEP or display_mode:
		facing_left = randf() < 0.5
	else:
		facing_left = position.x > GameCore.instance.player.position.x
	hop_num = abs(roundi(position.x)) % HOP_HEIGHTS.size()
	_play_anim()


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	if state == States.SLEEP:
		if position.distance_to(GameCore.instance.player.position) < WAKE_RANGE:
			_play_sound()
			state = States.RUN
	else:
		var jump:bool = false
		var set_vel:bool = false
		if is_on_floor():
			jump = true
			if vis and vis.is_on_screen() and randf() <= SFX_CHANCE:
				_play_sound()
		if is_on_wall():
			facing_left = not facing_left
			_play_anim()
			set_vel = true
		
		if jump:
			if state == States.NORMAL:
				facing_left = position.x > GameCore.instance.player.position.x
				if randf() <= NORMAL_TURN_CHANCE:
					facing_left = not facing_left
			elif state == States.RUN:
				facing_left = position.x < GameCore.instance.player.position.x
			velocity.y = JUMP_POWER * HOP_HEIGHTS[hop_num]
			hop_num = (hop_num + 1) % HOP_HEIGHTS.size()
			first_jump = true
			_play_anim()
			set_vel = true
		
		if set_vel:
			velocity.x = (SPEED_PANIC if state == States.RUN else SPEED_NORMAL)
			velocity.x *= (-1.0 if facing_left else 1.0)
	
	move_and_slide()
	
	velocity.y += GRAVITY * delta


func _play_anim() -> void:
	var dir:String = "_left" if facing_left else "_right"
	var action = ""
	if display_mode:
		if randf() < 0.02:
			action = "sleep"
		else:
			action = "idle"
	else:
		match state:
			States.NORMAL:
				action = "jump"
				if not first_jump:
					action = "idle"
			States.RUN:
				action = "jump_panic"
				if not first_jump:
					action = "idle"
			States.SLEEP:
				action = "sleep"
	sprite.action = action + dir


func _play_sound() -> void:
	#sfx.play()
	Statics.play_sfx_limited(sound, "Snelk")
