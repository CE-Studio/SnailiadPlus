# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name BlobAngel
extends Enemy


#region Variables
const HOP_TIMEOUTS:Array = [ 0.4, 0.5, 1.6, 0.4, 0.9, 1.1, 0.9, 0.5, 0.9 ]
const HOP_HEIGHTS:Array = [ 0.2, 0.3, 3.0, 0.2, 1.6, 0.4, 2.5, 2.7, 0.5 ]
const GRAVITY:float = 1200.0
const VEL_X:float = 100.0
const JUMP_VEL_BASE:float = -200
const QUIVER_THRESHOLD:float = 0.35
const QUIVER_RANGE:float = 1.5
const SHOT_TIMEOUT:float = 4.0
const SHOT_COUNT:int = 4

var facing_right:bool = false
var hop_ptr:int = 0
var hop_timeout:float = 0.0
var shot_timeout:float = SHOT_TIMEOUT
var grounded:bool = false

@onready var sfx_jump:AudioStreamPlayer = $"Jump"
@onready var ceil_ray:RayCast2D = $"CeilCheck"
@onready var donut:PackedScene = load("uid://cpp1rm5lkd443")
#endregion


func _ready() -> void:
	my_type = EnemyTypes.BLOB_ANGEL
	super.spawn()
	
	hop_ptr = int(position.x) % HOP_HEIGHTS.size()
	hop_timeout = HOP_TIMEOUTS[hop_ptr]
	if not display_mode:
		facing_right = GameCore.instance.player.position.x > position.x
	else:
		facing_right = randf() >= 0.5
	play_anim("idle")


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	if vis.is_on_screen():
		hop_timeout -= delta
		if hop_timeout <= QUIVER_THRESHOLD:
			sprite.position.x = randf_range(-QUIVER_RANGE, QUIVER_RANGE)
			play_anim("charge")
		if hop_timeout <= 0.0:
			sprite.position.x = 0.0
			facing_right = GameCore.instance.player.position.x > position.x
			body.velocity = Vector2(
				VEL_X * (1 if facing_right else -1),
				JUMP_VEL_BASE * HOP_HEIGHTS[hop_ptr]
			)
			hop_ptr = (hop_ptr + 1) % HOP_HEIGHTS.size()
			hop_timeout = HOP_TIMEOUTS[hop_ptr]
			play_anim("jump")
			grounded = false
			if not ceil_ray.is_colliding():
				sfx_jump.play()
		shot_timeout -= delta
		if shot_timeout <= 0.0:
			shot_timeout = SHOT_TIMEOUT
			_shoot_360_cluster_rotary(donut, Vector2(4.0, 0.0), 60.0, SHOT_COUNT)
	body.move_and_slide()
	body.velocity.y += GRAVITY * delta
	
	if body.is_on_wall() and not grounded:
		facing_right = not facing_right
		body.velocity.x = VEL_X * (1 if facing_right else -1)
		play_anim("reflect")
	
	if body.is_on_floor():
		if body.velocity.x != 0.0:
			play_anim("quiver")
		body.velocity.x = 0.0
		body.velocity.y *= -0.1
		grounded = true
	
	if body.is_on_ceiling() and body.velocity.y < 0.0:
		body.velocity.y = 0.0


func play_anim(state:String = "") -> void:
	state += "_right" if facing_right else "_left"
	sprite.play(state)
	sprite.flip_h = facing_right
