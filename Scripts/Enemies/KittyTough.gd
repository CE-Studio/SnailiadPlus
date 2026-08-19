# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name KittyTough
extends Enemy


#region Variables
const HOP_TIMEOUTS:Array = [ 0.7, 0.8, 0.6, 0.7, 0.8, 0.6, 0.7, 0.8, 0.6 ]
const HOP_HEIGHTS:Array = [ 1.0, 1.0, 1.0, 1.2, 1.3, 1.0, 1.2, 1.0, 0.9 ]
const GRAVITY:float = 1200.0
const VEL_X:float = 120.0
const JUMP_VEL_BASE:float = -310
const MAX_SHOTS:int = 10
const SHOT_SPEED:float = 80.0
const SHOT_TIMEOUT:float = 0.08
const JUMPS_BEFORE_ATTACK:int = 3

var facing_right:bool = false
var hop_ptr:int = 0
var hop_timeout:float = 0.0
var next_attack:int = 2
var is_attacking:bool = false
var shot_counter:int = 0
var shot_timeout:float = 0.0
var fall_flag:bool = false

@onready var sfx_jump:AudioStreamPlayer = $"Jump"
@onready var bullet:PackedScene = load("res://Scenes/Entities/Bullets/Enemy/EnemyBulletPea.tscn")
#endregion


func _ready() -> void:
	my_type = EnemyTypes.KITTY
	super.spawn()
	
	hop_ptr = int(position.x) % HOP_TIMEOUTS.size()
	hop_timeout = HOP_TIMEOUTS[hop_ptr]
	if display_mode:
		facing_right = randf() > 0.5
	else:
		facing_right = position.x < GameCore.instance.player.position.x
	play_anim("idle")


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	if vis.is_on_screen():
		body.velocity.y += GRAVITY * delta
		shot_timeout -= delta
		if is_attacking and shot_timeout <= 0.0 and shot_counter > 0:
			shot_counter -= 1
			shot_timeout = SHOT_TIMEOUT
			shoot(PI + (PI * 0.6 * shot_counter / MAX_SHOTS))
		if shot_counter <= 0:
			hop_timeout -= delta
			if hop_timeout <= 0.0:
				if not is_attacking and next_attack <= 0:
					is_attacking = true
					shot_counter = MAX_SHOTS
					shot_timeout = SHOT_TIMEOUT
					next_attack = JUMPS_BEFORE_ATTACK
				else:
					next_attack -= 1
					is_attacking = false
					if position.x > GameCore.instance.player.position.x:
						body.velocity.x = -VEL_X
						facing_right = false
					else:
						body.velocity.x = VEL_X
						facing_right = true
					body.velocity.y = JUMP_VEL_BASE * HOP_HEIGHTS[hop_ptr]
					play_anim("jump")
					sfx_jump.play()
				hop_ptr = (hop_ptr + 1) % HOP_HEIGHTS.size()
				hop_timeout = HOP_TIMEOUTS[hop_ptr]
				fall_flag = false
		var air_flag:bool = not body.is_on_floor()
		body.move_and_slide()
		if body.is_on_wall():
			facing_right = not facing_right
			body.velocity.x = VEL_X if facing_right else -VEL_X
			play_anim("fall" if fall_flag else "jump")
		if body.is_on_floor() and air_flag:
			play_anim("idle")
			body.velocity.x = 0
		elif not body.is_on_floor() and body.velocity.y > 0 and not fall_flag:
			fall_flag = true
			play_anim("fall")


func shoot(angle:float) -> void:
	if not facing_right:
		angle = PI - angle
	var aim_vector:Vector2 = Vector2(-cos(angle), sin(angle))
	_shoot(bullet, aim_vector, SHOT_SPEED)


func play_anim(anim:String) -> void:
	anim += "_right" if facing_right else "_left"
	sprite.flip_h = facing_right
	sprite.play(anim)
