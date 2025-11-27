class_name SnakeyTough
extends Enemy


#region Variables
const MOVE_TIMEOUT:float = 1.2
const REACT_DISTANCE:float = 240.0
const SPEED:float = 240.0
const DECEL:float = 360.0
const GRAVITY:float = 1200.0
const RETURN_SPEED:float = 20.0
const SHOT_TIMEOUT:float = 1.2
const SHOT_SPEED:float = 80.0

var move_timeout:float = MOVE_TIMEOUT
var shot_timeout:float = SHOT_TIMEOUT
var facing_left:bool = false

@onready var sfx_move:AudioStreamPlayer = $"Move"
@onready var donut:PackedScene = load("res://Scenes/Entities/Bullets/Enemy/EnemyBulletDonutLinear.tscn")
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
		if hard_mode:
			shot_timeout -= delta
			if shot_timeout <= 0.0:
				shot_timeout = SHOT_TIMEOUT
				var angle = atan2(
					position.y - GameCore.instance.player.position.y,
					position.x - GameCore.instance.player.position.x
				)
				_shoot(donut, Vector2(-cos(angle), -sin(angle)), SHOT_SPEED)
	move_and_slide()
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
