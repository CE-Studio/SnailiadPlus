# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name ChirpyTough
extends Enemy


#region Variables
const WEAPON_SPEED = 90
const SHOT_TIMEOUT = 1.5
const SIN_ACCEL = 700
const MAX_OFF_SCREEN_TIME = 3.0
const DONUT:PackedScene = preload("uid://cr8jpfivtdpnw")

var velocity:Vector2 = Vector2.ZERO
var theta:float = 0.0
var theta_mult:float = 4.0
var fly_speed:float = 90.0
var fly_amplitude:float = 30.0
var going_up:bool = true
var facing_left:bool = false
var taken_off:bool = false
var off_screen_time:float = 0.0
var shot_timeout:float = SHOT_TIMEOUT

@export var sfx_chirp:AudioStreamPlayer
#endregion


func _ready() -> void:
	my_type = EnemyTypes.CHIRPY
	super.spawn()
	
	theta = position.x * position.x * 13.7
	theta_mult += sin(position.x * 1.732 - position.y * 3.2)
	fly_speed += sin(position.x * 2.332 - position.y * 1.9) * 10.0
	fly_amplitude += sin(position.x * 7.3 + position.y) * 5.0
	shot_timeout -= sin(position.x * 2.725 - position.y * 2.6) * 0.8
	
	if display_mode:
		taken_off = true
		facing_left = randf() < 0.5
		fly_amplitude = 16.0


func _process(delta: float) -> void:
	super(delta)
	if not ai_active and not display_mode:
		return
	
	if vis.is_on_screen() and not taken_off:
		get_going()
	
	if taken_off:
		theta += delta
		position += velocity * delta
		position.y = origin.y + sin(theta * theta_mult) * fly_amplitude
		if position.y > origin.y and not going_up:
			going_up = true
			_play_anim(true)
		elif position.y < origin.y and going_up:
			going_up = false
			_play_anim(false)
		if display_mode:
			return
		if vis.is_on_screen():
			off_screen_time = 0
			if hard_mode:
				shot_timeout -= delta
				if shot_timeout <= 0.0:
					shot_timeout = SHOT_TIMEOUT
					var player_pos = GameCore.instance.player.position
					var target = Vector2(player_pos - position).normalized()
					_shoot(DONUT, target, WEAPON_SPEED)
		else:
			off_screen_time += delta
			if off_screen_time >= MAX_OFF_SCREEN_TIME:
				queue_free()


func get_going() -> void:
	if taken_off:
		return
	taken_off = true
	sfx_chirp.play()
	facing_left = GameCore.instance.player.position.x < position.x
	velocity.x = fly_speed * (-1.0 if facing_left else 1.0)
	_play_anim(going_up)


func _play_anim(up:bool) -> void:
	var v:String = "up_" if up else "down_"
	var h:String = "left" if facing_left else "right"
	sprite.play(v + h)
	sprite.flip_h = not facing_left
