# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name TallfishCommon
extends Enemy


#region Variables
const MOVE_TIMEOUT:float = 2.3
const SHOT_TIMEOUT:float = 0.25
const SHOT_COUNT:int = 3
const SHOT_SPEED:float = 100.0
const SPEED:float = 160.0
const DECEL:float = SPEED * 0.6
const SINE_AMPLITUDE:float = 4.0

var elapsed:float = 0.0
var velocity:Vector2 = Vector2.ZERO
var move_timeout:float = MOVE_TIMEOUT * 0.125
var shot_timeout:float = 0.0
var shot_num:int = 0
var facing_left:bool = false

@onready var donut:PackedScene = load("res://Scenes/Entities/Bullets/Enemy/EnemyBulletDonutLinear.tscn")
#endregion


func _ready() -> void:
	my_type = EnemyTypes.TALLFISH_COMMON
	hitbox = $"Area2D"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	facing_left = randf() < 0.5
	sprite.action = "idle_left" if facing_left else "idle_right"
	if display_mode:
		sprite.action = "idle_swim_left" if facing_left else "idle_swim_right"
		elapsed += randf() * TAU


func _process(delta: float) -> void:
	super(delta)
	if not ai_active and not display_mode:
		return
	
	elapsed += delta
	position.y = origin.y + SINE_AMPLITUDE * sin(elapsed * 2)
	if vis.is_on_screen() and not display_mode:
		move_timeout -= delta
		if move_timeout <= 0.0:
			if GameCore.instance.player.position.x < position.x:
				velocity.x = -SPEED
				facing_left = true
			else:
				velocity.x = SPEED
				facing_left = false
			shot_num = SHOT_COUNT
			shot_timeout = 0.0
			move_timeout = MOVE_TIMEOUT
			sprite.action = "swim_left" if facing_left else "swim_right"
		shot_timeout -= delta
		if shot_timeout <= 0.0 and shot_num > 0:
			var angle:float = atan2(
				GameCore.instance.player.position.y - position.y,
				GameCore.instance.player.position.x - position.x
			)
			_shoot(donut, Vector2(cos(angle), sin(angle)), SHOT_SPEED)
			shot_timeout = SHOT_TIMEOUT
			shot_num -= 1
		position += velocity * delta
		velocity.x = move_toward(velocity.x, 0.0, DECEL * delta)
