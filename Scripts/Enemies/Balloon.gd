class_name Balloon
extends Enemy


const CYCLE_SPEED:float = 1.2
const CYCLE_AMPLITUDE:float = 96.0
const RISE_SPEED:float = 24.0
const SPAWN_DRIFT_DIST:float = 48.0
const SPAWN_DRIFT_TIME:float = 4.0
const WEAPON_SPEED:float = 60.0
const SHOT_TIMEOUT:float = 2.4
const SHOT_SPEED:float = 1.6

var origin_x:float = 0.0
var theta:float = 0.0
var going_left:bool = false
var spawn_drift_amount:float = 0.0
var on_screen_once:bool = false
var shot_timeout:float = SHOT_TIMEOUT

@onready var donut:PackedScene = load("res://Scenes/Entities/Bullets/Enemy/EnemyBulletDonutLinear.tscn")


func _ready() -> void:
	my_type = EnemyTypes.BALLOON
	hitbox = $"Area2D"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	origin_x = position.x
	sprite.action = "right"


func _process(delta) -> void:
	super(delta)
	if not ai_active and not display_mode:
		return
	
	if vis.is_on_screen():
		on_screen_once = true
	if on_screen_once and not vis.is_on_screen():
		queue_free()
	
	theta += delta
	if display_mode:
		position.x = origin_x + 48.0 * sin(theta * CYCLE_SPEED)
		return
	var before_x:float = position.x
	position.x = origin_x + CYCLE_AMPLITUDE * sin(theta * CYCLE_SPEED)
	if before_x > position.x and not going_left:
		sprite.action = "left"
		going_left = true
	elif before_x < position.x and going_left:
		sprite.action = "right"
		going_left = false
	spawn_drift_amount = lerpf(spawn_drift_amount, 0.0, SPAWN_DRIFT_TIME * delta)
	position.y -= (RISE_SPEED + spawn_drift_amount) * delta
	
	if vis.is_on_screen():
		shot_timeout -= delta
		if shot_timeout <= 0.0:
			shot_timeout = SHOT_TIMEOUT
			var aim:float = atan2(
				position.y - GameCore.instance.player.position.y,
				position.x - GameCore.instance.player.position.x
			)
			_shoot(donut, Vector2(-cos(aim), -sin(aim)), WEAPON_SPEED)
