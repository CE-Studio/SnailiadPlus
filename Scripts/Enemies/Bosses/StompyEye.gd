class_name StompyEye
extends Enemy


#region Variables
const CLUSTER_TIMEOUT:float = 7.2
var SHOT_TIMEOUT:float = 0.6
var SHOT_COUNT:int = 2
const SHOT_SPEED:float = 40.0
const PUPIL_RADII:Vector2 = Vector2(20.0, 10.0)
const CLOSE_DELAY:float = 0.3
const OPEN_DELAY:float = 0.8

@export var left:bool = false
var open:bool = false
var will_close:bool = false
var blink_timeout:float = 4.0
var open_timeout:float = 0.0
var close_timeout:float = 0.0
var cluster_timeout:float = CLUSTER_TIMEOUT
var shot_timeout:float = SHOT_TIMEOUT
var shot_count:int = 0
var is_shooting:bool = false
var can_attack:bool = false

var boss:Stompy
var my_foot:StompyFoot
@onready var pupil:JsonSprite2D = $"Pupil"
@onready var eyelid:JsonSprite2D = $"Eyelid"
@onready var donut:PackedScene = load("res://Scenes/Entities/Bullets/Enemy/EnemyDonutLinear.tscn")
#endregion

func _ready() -> void:
	my_type = EnemyTypes.NONE
	sprite = $"Eye"
	hitbox = $"Area2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	sprite.action = "p0_left" if left else "p0_right"
	pupil.action = "p0_left" if left else "p0_right"
	eyelid.action = "p0_left_open" if left else "p0_right_open"
	
	if pupil.meta["clip"]:
		pupil.reparent(sprite)
	
	if hard_mode:
		SHOT_TIMEOUT *= 0.5
		SHOT_COUNT += 2


func _physics_process(delta) -> void:
	super(delta)
	if not Engine.is_editor_hint():
		pupil.material.set("shader_parameter/flash_color", Color.BLACK + flash_color)
		eyelid.material.set("shader_parameter/flash_color", Color.BLACK + flash_color)
		my_foot.sprite.material.set("shader_parameter/flash_color", Color.BLACK + flash_color)
	if damaged_this_tick:
		var this_damage:int = max_health - health
		boss._damage(this_damage, false)
		health = max_health
		if not will_close:
			close_timeout = CLOSE_DELAY
			will_close = true
	
	if not display_mode:
		var player_dir:float = atan2(
			GameCore.instance.player.position.y - global_position.y,
			GameCore.instance.player.position.x - global_position.x
		)
		pupil.position = Vector2(cos(player_dir), sin(player_dir)) * PUPIL_RADII
	
	if can_attack:
		cluster_timeout -= delta
		if cluster_timeout <= 0.0:
			cluster_timeout = CLUSTER_TIMEOUT
			shot_timeout = SHOT_TIMEOUT
			shot_count = SHOT_COUNT
			is_shooting = true
		
		if is_shooting:
			shot_timeout -= delta
			if shot_timeout <= 0.0:
				shot_timeout = SHOT_TIMEOUT
				shot_count -= 1
				if shot_count == 0:
					is_shooting = false
				var fire_angle:float = -PI / SHOT_COUNT
				fire_angle *= shot_count if left else (SHOT_COUNT - shot_count)
				var _donut:EnemyBullet = _shoot(donut, Vector2(-cos(fire_angle), -sin(fire_angle)), SHOT_SPEED)
				_donut.global_position = pupil.global_position
				boss.bullets.append(_donut)
	
	if open:
		blink_timeout -= delta
		if blink_timeout <= 0.0:
			blink_timeout = randf() * 8.0 + 1.0
			eyelid.action = boss.get_phase_anim("left_blink" if left else "right_blink")
		if will_close:
			close_timeout -= delta
			if close_timeout <= 0.0:
				will_close = false
				open = false
				invulnerable = true
				eyelid.action = boss.get_phase_anim("left_close" if left else "right_close")
				open_timeout = OPEN_DELAY
	else:
		open_timeout -= delta
		if open_timeout <= 0.0:
			open = true
			invulnerable = false
			eyelid.action = boss.get_phase_anim("left_open" if left else "right_open")


func update_phase() -> void:
	var dir:String = "left" if left else "right"
	sprite.action = "p%d_%s" % [boss.phase, dir]
	pupil.action = "p%d_%s" % [boss.phase, dir]
	eyelid.action = "p%d_%s_%s" % [boss.phase, dir, "open" if open else "blink"]
