class_name SnakeyCommon
extends Enemy


#region Variables
const MOVE_TIMEOUT:float = 1.2
const REACT_DISTANCE:float = 100.0
const SPEED:float = 180.0
const DECEL:float = 360.0
const GRAVITY:float = 1200.0
const RETURN_SPEED:float = 20.0

var move_timeout:float = MOVE_TIMEOUT
var facing_left:bool = false

@onready var sfx_move:AudioStreamPlayer = $"Move"
#endregion


func _ready() -> void:
	my_type = EnemyTypes.SNAKEY
	col = $"BodyBox"
	hitbox = $"Area2D"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	facing_left = randf() <= 0.5
	_play_anim(false)


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	if vis.is_on_screen():
		move_timeout -= delta
		var player_x:float = GameCore.instance.player.position.x
		if move_timeout <= 0.0 and abs(player_x - position.x) <= REACT_DISTANCE:
			if player_x < position.x:
				facing_left = true
				velocity.x = -SPEED
			else:
				facing_left = false
				velocity.x = SPEED
			_play_anim(true)
			move_timeout = MOVE_TIMEOUT
			sfx_move.play()
	
	var last_vel:Vector2 = velocity
	var last_grounded:bool = is_on_floor()
	move_and_slide()
	if is_on_wall() and last_vel.x != 0.0:
		velocity.x = -last_vel.x
		facing_left = not facing_left
		_play_anim(true)
	if is_on_floor() and last_grounded:
		velocity.y = last_vel.y * -0.1
	
	if not is_on_floor():
		velocity.y += GRAVITY * delta
	velocity.x = move_toward(velocity.x, 0.0, DECEL * delta)
	if abs(velocity.x) < RETURN_SPEED:
		_play_anim(false)


func _play_anim(moving:bool) -> void:
	var anim_name:String = "left_" if facing_left else "right_"
	anim_name += "move" if moving else "idle"
	if sprite.action != anim_name:
		sprite.action = anim_name
