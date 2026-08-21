# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
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
var is_dying:bool = false
@export var clip_eye:bool = false

var boss:Stompy
var my_foot:StompyFoot
@export var spr_group:Node2D
@export var pupil:SnailySprite2D
@export var eyelid:SnailySprite2D
@onready var donut:PackedScene = load("uid://cr8jpfivtdpnw")
#endregion

func _ready() -> void:
	my_type = EnemyTypes.NONE
	super.spawn()
	
	sprite.play("p0_left" if left else "p0_right")
	pupil.play("p0_left" if left else "p0_right")
	eyelid.play("p0_left_open" if left else "p0_right_open")
	
	if clip_eye and not display_mode:
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
	if is_dying or boss.in_death_anim:
		invulnerable = true
		return
	
	if damaged_this_tick:
		var this_damage:int = max_health - health
		if not boss.in_death_anim:
			boss._damage(this_damage, false, true)
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
			eyelid.play(boss.get_phase_anim("left_blink" if left else "right_blink"))
		if will_close:
			close_timeout -= delta
			if close_timeout <= 0.0:
				will_close = false
				open = false
				invulnerable = true
				eyelid.play(boss.get_phase_anim("left_close" if left else "right_close"))
				open_timeout = OPEN_DELAY
	else:
		open_timeout -= delta
		if open_timeout <= 0.0:
			open = true
			invulnerable = false
			eyelid.play(boss.get_phase_anim("left_open" if left else "right_open"))


func update_phase() -> void:
	var dir:String = "left" if left else "right"
	sprite.play("p%d_%s" % [boss.phase, dir])
	pupil.play("p%d_%s" % [boss.phase, dir])
	eyelid.play("p%d_%s_%s" % [boss.phase, dir, "open" if open else "blink"])


func set_death_pose() -> void:
	if left:
		sprite.play("defeat_left")
		pupil.play("defeat_left")
		pupil.position = Vector2(cos(3.2), sin(3.2)) * PUPIL_RADII
		eyelid.play("defeat_left")
	else:
		sprite.play("defeat_right")
		pupil.play("defeat_right")
		pupil.position = Vector2(cos(0.85), sin(0.85)) * PUPIL_RADII
		eyelid.play("defeat_right")
	is_dying = true
