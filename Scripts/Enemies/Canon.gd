# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name Canon
extends Enemy


#region Variables
const AIM_TIMEOUT:float = 0.25
const SHOT_TIMEOUT:float = 4.0
const SHOT_SPEED:float = 140.0
const TICK_MULT:float = 0.6

var aim_timeout:float = 0.0
var shot_timeout:float = 0.0
var base_dir:Statics.DirsSurface = Statics.DirsSurface.FLOOR
var vel:Vector2 = Vector2.ZERO
var current_dir:String = ""

@onready var base_spr:SnailySprite2D = $"Base"
@onready var spikeball:PackedScene = load("uid://nu02ausampyq")
#endregion


func _ready() -> void:
	my_type = EnemyTypes.CANON
	super.spawn()
	
	if display_mode:
		sprite.play("UL_idle")
		base_spr.play("floor_idle")
	else:
		match base_dir:
			Statics.DirsSurface.FLOOR:
				current_dir = "U"
				sprite.play("U_idle")
				base_spr.play("floor_idle")
			Statics.DirsSurface.LWALL:
				current_dir = "R"
				sprite.play("R_idle")
				base_spr.play("lwall_idle")
			Statics.DirsSurface.RWALL:
				current_dir = "L"
				sprite.play("L_idle")
				base_spr.play("rwall_idle")
			Statics.DirsSurface.CEILING:
				current_dir = "D"
				sprite.play("D_idle")
				base_spr.play("ceiling_idle")
		aim_timeout = fmod(position.x / 38.2, 0.25)
		shot_timeout = SHOT_TIMEOUT + fmod(position.x / 96.0, 6.0)


func _process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	aim_timeout -= delta
	if aim_timeout <= 0.0:
		aim_timeout = AIM_TIMEOUT
		aim()
	if vis.is_on_screen():
		shot_timeout -= delta * TICK_MULT
		if shot_timeout <= 0.0:
			shot_timeout = SHOT_TIMEOUT
			_shoot(spikeball, vel.normalized(), SHOT_SPEED)
			sprite.play(current_dir + "_fire")
			sprite.autoplay_next = current_dir + "_idle"


func aim() -> void:
	var player_angle:float = atan2(
		GameCore.instance.player.position.y - position.y,
		GameCore.instance.player.position.x - position.x
	)
	player_angle = floori((player_angle + PI / 8.0) * 4.0 / PI) / 4.0 * PI
	vel = Vector2(cos(player_angle), sin(player_angle)) * SHOT_SPEED
	var anim_name:String = ""
	if vel.y < -0.1:
		anim_name += "U"
	elif vel.y > 0.1:
		anim_name += "D"
	if vel.x < -0.1:
		anim_name += "L"
	elif vel.x > 0.1:
		anim_name += "R"
	current_dir = anim_name
	sprite.play(anim_name + "_idle")


func kill() -> void:
	if invulnerable:
		return
	Statics.play_sfx_disconnected(SFX_KILL)
	if my_type != EnemyTypes.NONE:
		Statics.add_bestiary_entry(my_type)
	for i in range(kill_particle_count):
		var p_range = kill_particle_range
		var pos = Vector2(randi_range(-p_range.x, p_range.x), randi_range(-p_range.y, p_range.y))
		var part = kill_particle_types[randi() % kill_particle_types.size()]
		Statics.spawn_particle(part, Room.Layers.FG1, position + pos)
	if Statics.current_profile["character"] == Player.Players.LEECHY:
		spawn_health_orbs()
	environment = null
	ai_active = false
	sprite.visible = false
	invulnerable = true
	match base_dir:
		Statics.DirsSurface.FLOOR:
			base_spr.play("floor_destroyed")
		Statics.DirsSurface.LWALL:
			base_spr.play("lwall_destroyed")
		Statics.DirsSurface.RWALL:
			base_spr.play("rwall_destroyed")
		Statics.DirsSurface.CEILING:
			base_spr.play("ceiling_destroyed")
