# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name BlobDevil
extends Enemy


#region Variables
const HOP_TIMEOUTS:Array = [ 0.4, 0.5, 1.6, 0.4, 0.9, 1.1, 0.9, 0.5, 0.9 ]
const HOP_HEIGHTS:Array = [ 0.2, 0.3, 3.0, 0.2, 1.6, 0.4, 2.5, 2.7, 0.5 ]
const GRAVITY:float = 1200.0
const VEL_X:float = 280.0
const JUMP_VEL_BASE:float = -320
const SHOT_TIMEOUT:float = 0.7
const SHOT_COUNT:int = 4
const CLUSTER_COUNT:int = 1

var facing_right:bool = false
var hop_ptr:int = 0
var hop_timeout:float = 0.0
var shot_timeout:float = 0.0

@onready var sfx_jump:AudioStreamPlayer = $"Jump"
@onready var donut:PackedScene = load("res://Scenes/Entities/Bullets/Enemy/EnemyBulletDonutRotary.tscn")
#endregion


func _ready() -> void:
	my_type = EnemyTypes.BLOB_DEVIL
	col = $"BodyBox"
	hitbox = $"Area2D"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	hop_ptr = int(position.x) % HOP_HEIGHTS.size()
	hop_timeout = HOP_TIMEOUTS[hop_ptr]
	facing_right = randf() >= 0.5
	play_anim("idle")


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	if vis.is_on_screen():
		hop_timeout -= delta
		if hop_timeout <= 0.0:
			facing_right = GameCore.instance.player.position.x > position.x
			velocity = Vector2(
				VEL_X * (1 if facing_right else -1),
				JUMP_VEL_BASE * HOP_HEIGHTS[hop_ptr]
			)
			hop_ptr = (hop_ptr + 1) % HOP_HEIGHTS.size()
			hop_timeout = HOP_TIMEOUTS[hop_ptr]
			play_anim("jump")
			sfx_jump.play()
		if hard_mode:
			shot_timeout -= delta
			if shot_timeout <= 0.0:
				shot_timeout = SHOT_TIMEOUT
				_shoot_360_cluster_rotary(donut, Vector2(4.0, 0.0), 60.0, SHOT_COUNT)
	move_and_slide()
	velocity.y += GRAVITY * delta
	
	if is_on_wall():
		facing_right = not facing_right
		velocity.x = VEL_X * (1 if facing_right else -1)
		play_anim("reflect")
	
	if is_on_floor():
		if velocity.x != 0.0:
			play_anim("quiver")
		velocity.x = 0.0
		velocity.y *= -0.1


func play_anim(modifier:String = "") -> void:
	var anim_name = "devil_"
	anim_name += modifier
	anim_name += "_right" if facing_right else "_left"
	sprite.action = anim_name
