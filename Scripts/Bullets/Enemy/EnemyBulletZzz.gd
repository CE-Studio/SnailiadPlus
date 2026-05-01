# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
extends EnemyBullet


const ACCEL:float = 2200.0
const THETA_CYCLE:float = TAU
const THETA_AMPLITUDE:float = 14.0
const WIGGLE_THRESHOLD:float = 0.5
const WIGGLE_SCALE:float = 3.0

## The position this bullet is set to rest before properly firing at the player
var target_position:Vector2 = Vector2.ZERO
## How long it takes for the bullet to target and move toward the player
var fire_delay:float = 3.0
## Used to calculate and apply vertical oscillation before being fired
var theta:float = 0.0
## Whether or not this bullet has "fired," or targeted and started pursuing the player
var fired:bool = false
## Used as a substitute for this bullet's Y position when moving toward the target, to negate vertical oscillation
var base_y:float = 0.0
## Additional [AudioStreamPlayer] played when this bullet fires
@onready var sfx_fire:AudioStreamPlayer = $"AudioGroup/ActuallyShoot"


func _spawn(dir:Vector2, speed:float, play_sound:bool = true) -> void:
	super._spawn(dir, speed, play_sound)
	target_position = dir
	fire_delay = speed
	max_life_time += fire_delay
	base_y = position.y


func _physics_process(delta: float) -> void:
	if fired:
		velocity += ACCEL * delta
		position += velocity * normalized_dir * delta
	else:
		theta += delta
		position.x = Statics.integrate(position.x, target_position.x, 2.0, delta)
		base_y = Statics.integrate(base_y, target_position.y, 2.0, delta)
		position.y = base_y + THETA_AMPLITUDE * sin(theta * THETA_CYCLE)
		fire_delay -= delta
		if fire_delay <= 0.0:
			sfx_fire.play()
			sprite.position = Vector2.ZERO
			fired = true
			normalized_dir = position.direction_to(Player.instance.position)
		elif fire_delay <= WIGGLE_THRESHOLD:
			sprite.position = Vector2(
				randf_range(-WIGGLE_SCALE, WIGGLE_SCALE),
				randf_range(-WIGGLE_SCALE, WIGGLE_SCALE)
			)
	super(delta)
