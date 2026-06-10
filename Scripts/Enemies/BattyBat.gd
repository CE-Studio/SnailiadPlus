# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name BattyBat
extends Enemy


#region Variables
const MOVE_TIMEOUT:float = 1.2
const REACT_DISTANCE:float = 110
const SPEED:float = 70

var velocity:Vector2 = Vector2.ZERO
var move_timeout:float = MOVE_TIMEOUT
var is_flying:bool = false
var facing_left:bool = false

@onready var sfx_fly:AudioStreamPlayer = $"Fly"
#endregion


func _ready() -> void:
	my_type = EnemyTypes.BATTYBAT
	hitbox = $"Area2D"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	if display_mode:
		facing_left = randf() < 0.5
		sprite.action = "fly_" + ("left" if facing_left else "right")
	else:
		facing_left = _get_left()
		sprite.action = "idle_" + ("left" if facing_left else "right")


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	move_timeout -= delta
	if (vis.is_on_screen() and move_timeout <= 0.0 and not is_flying
	and abs(GameCore.instance.player.position.x - position.x) < REACT_DISTANCE):
		facing_left = _get_left()
		velocity.x = SPEED * (-1 if facing_left else 1)
		velocity.y = sqrt(abs(GameCore.instance.player.position.y - position.y + 40)) * SPEED * 0.25
		sprite.action = "fly_" + ("left" if facing_left else "right")
		is_flying = true
		sfx_fly.play()
	elif not is_flying:
		var new_left = _get_left()
		if new_left != facing_left:
			facing_left = new_left
			sprite.action = "idle_" + ("left" if facing_left else "right")
	else:
		velocity.y -= SPEED * 2 * delta
	position += velocity * delta


func _get_left() -> bool:
	return GameCore.instance.player.position.x < position.x
