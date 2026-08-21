# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name SpaceboxBabybox
extends Enemy


#region Variables
const MODE_TIMEOUT:float = 0.6
const ACCEL:Array[float] = [400.0, 500.0]

var mode_timeout:float = MODE_TIMEOUT
var last_mode:Statics.DirsCompass = Statics.DirsCompass.NONE
var current_mode:Statics.DirsCompass = Statics.DirsCompass.NONE
var boss:Spacebox = null
var last_action:String = ""
var accel_dir:Vector2 = Vector2.ZERO

@onready var sfx_stomp:AudioStreamPlayer = $"Stomp"
#endregion


func _ready() -> void:
	my_type = EnemyTypes.SPACEBOX_BABYBOX
	super.spawn()
	
	if boss:
		play_phase_anim("spawn")
	if display_mode:
		sprite.play("display")


func _physics_process(delta) -> void:
	super(delta)
	if not ai_active:
		return
	
	var real = self
	if real is CharacterBody2D:
		check_mode(delta)
		real.velocity += accel_dir * (ACCEL[1] if boss and boss.phase > 0 else ACCEL[0]) * delta
		var stomp_vel:Vector2 = real.velocity
		if stomp_vel != Vector2.ZERO and real.move_and_slide() and stomp_vel != real.velocity:
			stomp(stomp_vel)


func play_phase_anim(anim:String = last_action) -> void:
	last_action = anim
	sprite.play("p%d_%s" % [boss.phase, anim])


func stomp(impact_vel:Vector2) -> void:
	var real = self
	if real is CharacterBody2D:
		if impact_vel.length() > 100.0:
			sfx_stomp.play()
		match current_mode:
			Statics.DirsCompass.N:
				play_phase_anim("U_land")
			Statics.DirsCompass.E:
				play_phase_anim("R_land")
			Statics.DirsCompass.S:
				play_phase_anim("D_land")
			Statics.DirsCompass.W:
				play_phase_anim("L_land")
		position += impact_vel.normalized() * -0.25
		real.velocity = Vector2.ZERO
		accel_dir = Vector2.ZERO
		last_mode = current_mode
		current_mode = Statics.DirsCompass.NONE
		mode_timeout = MODE_TIMEOUT


func check_mode(delta:float) -> void:
	mode_timeout -= delta
	if current_mode == Statics.DirsCompass.NONE and mode_timeout <= 0.0:
		if last_mode == Statics.DirsCompass.N or last_mode == Statics.DirsCompass.S:
			if GameCore.instance.player.position.x < position.x:
				charge_w()
			else:
				charge_e()
		elif last_mode == Statics.DirsCompass.W or last_mode == Statics.DirsCompass.E:
			if GameCore.instance.player.position.y < position.y:
				charge_n()
			else:
				charge_s()


func charge_n() -> void:
	last_mode = current_mode
	current_mode = Statics.DirsCompass.N
	play_phase_anim("U_move")
	accel_dir = Vector2.UP


func charge_e() -> void:
	last_mode = current_mode
	current_mode = Statics.DirsCompass.E
	play_phase_anim("R_move")
	accel_dir = Vector2.RIGHT


func charge_s() -> void:
	last_mode = current_mode
	current_mode = Statics.DirsCompass.S
	play_phase_anim("D_move")
	accel_dir = Vector2.DOWN


func charge_w() -> void:
	last_mode = current_mode
	current_mode = Statics.DirsCompass.W
	play_phase_anim("L_move")
	accel_dir = Vector2.LEFT


#func kill() -> void:
#	if boss:
#		boss.babyboxes.remove_at(boss.babyboxes.find(self))
#	super()
