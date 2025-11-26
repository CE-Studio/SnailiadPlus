class_name TurtleTough
extends Enemy


#region Variables
const JUMP_TIMEOUTS:Array[float] = [
	2.5, 2.3, 3.0, 2.1, 2.7, 2.6, 2.9, 2.1, 2.3, 3.1,3.3, 2.9,
	2.6, 2.4, 1.9, 3.1, 2.7, 3.9, 4.2, 1.8, 2.8, 3.1, 3.8, 2.8
]
const FLIP_TIMEOUT:float = 0.3
const JUMP_POWER:float = 500.0
const GRAVITY:float = 1200.0
const TURNAROUND_TIMEOUT:float = 1.8

@export var direction:Statics.DirsSurface = Statics.DirsSurface.FLOOR

var speed:float = 24.0
var jump_timeout:float = 0.0
var jump_index:int = 0
var flip_timeout:float = 99999999.0
var turn_timeout:float = 0.0
var facing_left:bool = false
#endregion


func _ready() -> void:
	my_type = EnemyTypes.TURTLE_TOUGH
	hitbox = $"Area2D"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	jump_index = absi(floori(fmod(position.y, 16.0))) % JUMP_TIMEOUTS.size()
	jump_timeout = JUMP_TIMEOUTS[jump_index]
	if hard_mode:
		speed *= 1.9
	
	if display_mode:
		direction = Statics.DirsSurface.RWALL
		facing_left = randf() <= 0.5
	else:
		var player:Player = GameCore.instance.player
		match direction:
			Statics.DirsSurface.FLOOR:
				facing_left = player.position.x < position.x
				velocity = Vector2(-speed if facing_left else speed, 0.0)
			Statics.DirsSurface.LWALL:
				facing_left = player.position.y < position.y
				velocity = Vector2(0.0, -speed if facing_left else speed)
			Statics.DirsSurface.RWALL:
				facing_left = player.position.y > position.y
				velocity = Vector2(0.0, speed if facing_left else -speed)
			Statics.DirsSurface.FLOOR:
				facing_left = player.position.x < position.x
				velocity = Vector2(speed if facing_left else -speed, 0.0)
	_play_anim("walk")


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active and not display_mode:
		return
	
	


func _play_anim(action:String) -> void:
	var anim_name:String = ""
	match direction:
		Statics.DirsSurface.FLOOR: anim_name = "floor_"
		Statics.DirsSurface.LWALL: anim_name = "lwall_"
		Statics.DirsSurface.RWALL: anim_name = "rwall_"
		Statics.DirsSurface.CEILING: anim_name = "ceiling_"
	anim_name += "left_" if facing_left else "right_"
	anim_name += action
	if sprite.action != anim_name:
		sprite.action = anim_name
