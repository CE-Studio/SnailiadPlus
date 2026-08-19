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

@export var sfx_fly:AudioStreamPlayer
#endregion


func _ready() -> void:
	my_type = EnemyTypes.BATTYBAT
	super.spawn()
	
	if display_mode:
		facing_left = randf() < 0.5
		sprite.play("fly_" + ("left" if facing_left else "right"))


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	move_timeout -= delta
	if (vis.is_on_screen() and move_timeout <= 0.0 and not is_flying
	and abs(Player.instance.position.x - position.x) < REACT_DISTANCE):
		facing_left = _get_left()
		velocity.x = SPEED * (-1 if facing_left else 1)
		velocity.y = sqrt(abs(Player.instance.position.y - position.y + 40)) * SPEED * 0.25
		sprite.play("fly_" + ("left" if facing_left else "right"))
		is_flying = true
		sfx_fly.play()
	elif is_flying:
		velocity.y -= SPEED * 2 * delta
	position += velocity * delta


func _get_left() -> bool:
	return Player.instance.position.x < position.x
