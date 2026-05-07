# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name Angryblock
extends Enemy


#region Variables
const MIN_PHASE_TIME:float = 0.75
const MAX_PHASE_TIME:float = 2.0
const LONG_PHASE_ADD:float = 1.2
const MAX_PHASES_BETWEEN_IDLE:int = 4
const LERP_RATE:float = 12.5
const PLAYER_STARE_RADIUS:float = 96.0
const IDLE_PHASE_INC_MULT:float = 1.3
const LONG_PHASE_CHANCE:float = 0.35
const SHOT_COOLDOWN:float = 4.0
const SHOT_INIT_DELAY:float = 0.75
const SHOT_DELAY:float = 0.15
const BULLET_COUNT:int = 8
const BULLET_SPEED:float = 148
const BULLET_SPREAD:float = 0.135
const MIN_NOISE_TIME:float = 10.0
const MAX_NOISE_TIME:float = 30.0

var phase_time:float = LONG_PHASE_ADD
var face_player:bool = false
var phases_since_idle:int = MAX_PHASES_BETWEEN_IDLE
var face_radii:Vector2 = Vector2(8.0, 3.0)
var look_dir:Vector2 = Vector2.ZERO
var burst_cooldown:float = SHOT_COOLDOWN
var bullet_cooldown:float = SHOT_DELAY
var bullet_count:int = 0
var is_firing:bool = false
var body_anim_ptr:int = 0
var last_face_anim:String = ""
var time_until_noise:float = 0.0

@onready var face_spr:JsonSprite2D = $"Face"
@onready var donut:PackedScene = load("res://Scenes/Entities/Bullets/Enemy/EnemyBulletDonutLinear.tscn")
@onready var sfx_noise:AudioStreamPlayer = $"Noise"
#endregion


func _ready() -> void:
	my_type = EnemyTypes.ANGRYBLOCK
	hitbox = $"Area2D"
	sprite = $"Body"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	time_until_noise = randf_range(MIN_NOISE_TIME, MAX_NOISE_TIME)


func _process(delta: float) -> void:
	super(delta)
	if not ai_active and not display_mode:
		return
	
	if not display_mode:
		time_until_noise -= delta
		if time_until_noise <= 0.0:
			time_until_noise = randf_range(MIN_NOISE_TIME, MAX_NOISE_TIME)
			sfx_noise.play()
	
	phase_time -= delta
	var player_vector:Vector2 = Vector2.ZERO
	if not display_mode:
		var player_angle = atan2(
			position.y - GameCore.instance.player.position.y,
			position.x - GameCore.instance.player.position.x
		)
		player_vector = Vector2(-cos(player_angle), -sin(player_angle))
		face_player = position.distance_to(GameCore.instance.player.position) < PLAYER_STARE_RADIUS or is_firing
		if hard_mode and vis.is_on_screen():
			burst_cooldown -= delta
			if burst_cooldown <= 0.0:
				if not is_firing:
					bullet_count = 0
					bullet_cooldown = SHOT_INIT_DELAY
					look_dir = player_vector * face_radii
					_update_face_anim(true, "prep")
					is_firing = true
					face_player = true
				bullet_cooldown -= delta
				if bullet_cooldown <= 0:
					_update_face_anim(false, "fire")
					bullet_cooldown = SHOT_DELAY;
					var aim_vector = (player_vector + Vector2(
						randf_range(-BULLET_SPREAD, BULLET_SPREAD),
						randf_range(-BULLET_SPREAD, BULLET_SPREAD)
					)).normalized()
					_shoot(donut, aim_vector, BULLET_SPEED)
					bullet_count += 1
					if bullet_count >= BULLET_COUNT:
						is_firing = false
						face_player = false
						burst_cooldown = SHOT_COOLDOWN
						_update_face_anim(true)
	
	if phase_time <= 0.0 and not is_firing:
		if randf() > float(MAX_PHASES_BETWEEN_IDLE) / float(phases_since_idle):
			phases_since_idle = 0
			look_dir = Vector2.ZERO
			_update_face_anim(true)
		else:
			phases_since_idle += 1
			var new_angle:float = randf() * TAU
			var normal_dir:Vector2 = Vector2(-cos(new_angle), sin(new_angle))
			look_dir = normal_dir * face_radii
			_update_face_anim(true)
		phase_time = randf_range(MIN_PHASE_TIME, MAX_PHASE_TIME) * (IDLE_PHASE_INC_MULT if phases_since_idle == 0 else 1.0)
		if randf() < LONG_PHASE_CHANCE:
			phase_time += LONG_PHASE_ADD
	if face_player:
		phases_since_idle = MAX_PHASES_BETWEEN_IDLE
		look_dir = player_vector * face_radii
		if not is_firing:
			_update_face_anim(false)
	
	face_spr.position = face_spr.position.lerp(look_dir, LERP_RATE * delta)
	_update_body_anim()


func _update_body_anim() -> void:
	if body_anim_ptr >= sprite.meta.size() or display_mode:
		return
	var hp_ratio:int = ceili((float(health) / float(max_health)) * 100.0)
	if hp_ratio <= sprite.meta[str(body_anim_ptr)]:
		body_anim_ptr += 1
		sprite.action = str(body_anim_ptr)


func _update_face_anim(overwrite:bool, state:String = "idle") -> void:
	var anim_name:String = ""
	if look_dir == Vector2.ZERO:
		anim_name = "idle"
	else:
		var norm_dir = look_dir.normalized()
		if norm_dir.y < -0.3827:
			anim_name += "U"
		elif norm_dir.y > 0.3827:
			anim_name += "D"
		if norm_dir.x < -0.3827:
			anim_name += "L"
		elif norm_dir.x > 0.3827:
			anim_name += "R"
		anim_name = "_".join([anim_name, state])
	if overwrite or anim_name != last_face_anim:
		face_spr.action = anim_name
		last_face_anim = anim_name
