# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name BlobCommon
extends Enemy


#region Variables
const HOP_TIMEOUTS:Array = [ 2.4, 3.5, 2.2, 1.6, 3.9, 3.5, 2.9, 3.1, 1.8 ]
const HOP_HEIGHTS:Array = [ 1.0, 1.0, 1.0, 1.2, 2.0, 1.0, 1.2, 1.0, 2.0 ]
const GRAVITY:float = 1200.0
const VEL_X:float = 100.0
const JUMP_VEL_BASE:float = -240

var facing_right:bool = false
var hop_ptr:int = 0
var hop_timeout:float = 0.0

@onready var sfx_jump:AudioStreamPlayer = $"Jump"
#endregion


func _ready() -> void:
	my_type = EnemyTypes.BLOB_COMMON
	col = $"BodyBox"
	hitbox = $"Area2D"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	hop_ptr = int(position.x) % HOP_HEIGHTS.size()
	hop_timeout = HOP_TIMEOUTS[hop_ptr] * 0.3333
	if not display_mode:
		facing_right = GameCore.instance.player.position.x > position.x
	else:
		facing_right = randf() >= 0.5
	play_anim("idle")


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	var real = self
	if real is CharacterBody2D:
		if vis.is_on_screen():
			hop_timeout -= delta
			if hop_timeout <= 0.0:
				facing_right = GameCore.instance.player.position.x > position.x
				real.velocity = Vector2(
					VEL_X * (1 if facing_right else -1),
					JUMP_VEL_BASE * HOP_HEIGHTS[hop_ptr]
				)
				hop_ptr = (hop_ptr + 1) % HOP_HEIGHTS.size()
				hop_timeout = HOP_TIMEOUTS[hop_ptr] * (0.5 if hard_mode else 1.0)
				play_anim("jump")
				sfx_jump.play()
		real.move_and_slide()
		real.velocity.y += GRAVITY * delta
		
		if real.is_on_wall():
			facing_right = not facing_right
			real.velocity.x = VEL_X * (1 if facing_right else -1)
			play_anim("reflect")
		
		if real.is_on_floor():
			if real.velocity.x != 0.0:
				play_anim("quiver")
			real.velocity.x = 0.0
			real.velocity.y *= -0.1


func play_anim(modifier:String = "") -> void:
	var anim_name = "common_"
	anim_name += modifier
	anim_name += "_right" if facing_right else "_left"
	sprite.action = anim_name
