class_name BossHealthBar
extends Node2D


#region Variables
const WIDTH:int = 250
const DAMAGE_UPDATE_TIMEOUT:float = 0.5
const DAMAGE_LERP_SPEED:float = 6.0
const SHAKE_LERP_SPEED:float = 10.0
const SHAKE_VARIANCE:float = 6.0
const INTRO_FILL_TIME:float = 1.5
const NAME_SHAKE_RADIUS:float = 4.0
const NAME_SHAKE_TIME:float = 0.25

var boss:Boss = null
var damage_update_timeout:float = 0.0
var intro_fill:float = 0.0
var name_shake_time:float = 0.0

@onready var frame:JsonSprite2D = $"Frame"
@onready var main:JsonSprite2D = $"Frame/BarMainMask/BarMain"
@onready var main_mask:JsonSprite2D = $"Frame/BarMainMask"
@onready var damaged:JsonSprite2D = $"Frame/BarDamagedMask/BarDamaged"
@onready var damaged_mask:JsonSprite2D = $"Frame/BarDamagedMask"
@onready var boss_name:SnailyText = $"BossName/HBox/SnailyText"
@onready var boss_name_container:Node2D = $"BossName"
@onready var sfx_beep:AudioStreamPlayer = $"AudioGroup/Beep"
#endregion


func _ready() -> void:
	frame.action = "frame_spawn"
	main.action = "bar_main_idle"
	main_mask.action = "bar_main_mask"
	damaged.action = "bar_damaged_idle"
	damaged_mask.action = "bar_damaged_mask"
	_update_main(_get_bar_pos_from_ratio(0))
	_update_damaged(_get_bar_pos_from_ratio(0))


func _process(delta: float) -> void:
	if damage_update_timeout <= 0.0:
		var lerp_amount = lerpf(damaged_mask.position.x, main_mask.position.x, DAMAGE_LERP_SPEED * delta)
		_update_damaged(lerp_amount)
	damage_update_timeout -= delta
	frame.position.x = lerpf(frame.position.x, 0.0, SHAKE_LERP_SPEED * delta)
	
	if intro_fill > 0.0:
		var current_ratio = abs((intro_fill / INTRO_FILL_TIME) - 1)
		var current_pos = _get_bar_pos_from_ratio(current_ratio)
		_update_main(current_pos)
		_update_damaged(current_pos)
		intro_fill -= delta
	if name_shake_time > 0.0:
		boss_name_container.position = Vector2(
			randf_range(-NAME_SHAKE_RADIUS, NAME_SHAKE_RADIUS),
			randf_range(-NAME_SHAKE_RADIUS, NAME_SHAKE_RADIUS)
		) * inverse_lerp(0.0, NAME_SHAKE_TIME, name_shake_time)
		name_shake_time -= delta
		if name_shake_time <= 0.0:
			boss_name_container.position = Vector2.ZERO


func _begin_intro_fill() -> void:
	intro_fill = INTRO_FILL_TIME


func _end_intro_fill() -> void:
	intro_fill = 0.0
	name_shake_time = NAME_SHAKE_TIME
	_update_main(0)
	_update_damaged(0)
	main.action = "bar_main_filled"


func update() -> void:
	_update_main()
	damage_update_timeout = DAMAGE_UPDATE_TIMEOUT
	if frame.meta["programmatic_shake"] == true:
		frame.position.x += randf_range(-SHAKE_VARIANCE, SHAKE_VARIANCE)
	frame.action = "frame_damage"
	main.action = "bar_main_damage"


func play_beep() -> void:
	sfx_beep.play()


func _update_main(value:float = _get_bar_pos_from_health()) -> void:
	main_mask.position.x = value
	main.position.x = -value


func _update_damaged(value:float) -> void:
	damaged_mask.position.x = value
	damaged.position.x = -value


func _get_bar_pos_from_health() -> float:
	if not boss:
		return WIDTH * 0.5
	var hp:float = boss.health
	var hp_max:float = boss.max_health
	var ratio:float = hp / hp_max
	return -WIDTH + ceili(ratio * WIDTH)


func _get_bar_pos_from_ratio(ratio:float) -> float:
	return -WIDTH + ceili(ratio * WIDTH)
