class_name BlobTough
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
	my_type = EnemyTypes.BLOB_TOUGH
	col = $"BodyBox"
	hitbox = $"Area2D"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	hop_ptr = int(position.x) % HOP_HEIGHTS.size()
	hop_timeout = HOP_TIMEOUTS[hop_ptr] * (0.5 if hard_mode else 1)
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
			hop_timeout = HOP_TIMEOUTS[hop_ptr] * (0.5 if hard_mode else 1.0)
			play_anim("jump")
			sfx_jump.play()
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
	var anim_name = "tough_"
	anim_name += modifier
	anim_name += "_right" if facing_right else "_left"
	sprite.action = anim_name
