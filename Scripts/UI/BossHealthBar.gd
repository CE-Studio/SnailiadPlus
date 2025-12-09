class_name BossHealthBar
extends Node2D


#region Variables
const WIDTH:int = 250
const DAMAGE_UPDATE_TIMEOUT:float = 0.5
const DAMAGE_LERP_SPEED:float = 6.0
const SHAKE_LERP_SPEED:float = 10.0
const SHAKE_VARIANCE:float = 2.0
const INTRO_FILL_TIME:float = 1.5
const OUTRO_SHAKE:float = 2.0

var boss:Boss = null
var damage_update_timeout:float = 0.0
var intro_fill:float = 0.0
var name_shake_time:float = 0.0
var programmatic_shake:bool = true
var outro_shake:bool = false
var defeated_origin:Vector2 = Vector2.ZERO

@onready var frame:JsonSprite2D = $"Frame"
@onready var main:JsonSprite2D = $"Frame/BarMainMask/BarMain"
@onready var main_mask:JsonSprite2D = $"Frame/BarMainMask"
@onready var damaged:JsonSprite2D = $"Frame/BarDamagedMask/BarDamaged"
@onready var damaged_mask:JsonSprite2D = $"Frame/BarDamagedMask"
@onready var boss_name:SnailyText = $"BossName/HBox/SnailyText"
@onready var boss_name_container:Node2D = $"BossName"
@onready var defeated:SnailyText = $"Defeated/HBox/SnailyText"
@onready var defeated_container:Node2D = $"Defeated"
@onready var sfx_beep:AudioStreamPlayer = $"AudioGroup/Beep"
@onready var sfx_full:AudioStreamPlayer = $"AudioGroup/Full"
@onready var anim:AnimationPlayer = $"AnimationPlayer"
#endregion


func _ready() -> void:
	frame.action = "frame_spawn"
	main.action = "bar_main_idle"
	main_mask.action = "bar_main_mask"
	damaged.action = "bar_damaged_idle"
	damaged_mask.action = "bar_damaged_mask"
	_update_main(_get_bar_pos_from_ratio(0))
	_update_damaged(_get_bar_pos_from_ratio(0))
	defeated_origin = defeated_container.position
	if frame.meta.size() > 0 and frame.meta.keys().has("programmatic_shake"):
		var shake = frame.meta["programmatic_shake"]
		if shake is bool:
			programmatic_shake = shake


func instance(_boss:Boss) -> void:
	boss = _boss
	var _name:String = Enemy.EnemyTypes.keys()[_boss.my_type]
	_name = _name.to_camel_case()
	match _name:
		"shellbreaker": boss_name.set_snaily_text(tr(&"Shellbreaker"))
		"shellbreakerRush": boss_name.set_snaily_text(tr(&"Super Shellbreaker"))
		"stompy": boss_name.set_snaily_text(tr(&"Stompy"))
		"stompyRush": boss_name.set_snaily_text(tr(&"Vis Vires"))
		"spacebox": boss_name.set_snaily_text(tr(&"Space Box"))
		"spaceboxRush": boss_name.set_snaily_text(tr(&"Time Cube"))
		"moonsnail": boss_name.set_snaily_text(tr(&"Moon Snail"))
		"moonsnailRush": boss_name.set_snaily_text(tr(&"Sun Snail"))
		"gigasnail": boss_name.set_snaily_text(tr(&"Giga Snail"))
		"gigasnailRush": boss_name.set_snaily_text(tr(&"Giga Sun Snail"))
		"cosmicsnail": boss_name.set_snaily_text(tr(&"Cosmic Snail"))
		"cosmicsnailRush": boss_name.set_snaily_text(tr(&"Cosmic Sun Snail"))
	defeated.set_snaily_text(tr(&"Defeated!!"))


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
	if outro_shake:
		var container_shake = Vector2(
			randf_range(-OUTRO_SHAKE, OUTRO_SHAKE),
			randf_range(-OUTRO_SHAKE, OUTRO_SHAKE)
		)
		boss_name_container.position = container_shake
		defeated_container.position = container_shake + defeated_origin
		if programmatic_shake:
			frame.position.x += randf_range(-OUTRO_SHAKE, OUTRO_SHAKE)


#region AnimationPlayer functions
func _begin_intro_fill() -> void:
	intro_fill = INTRO_FILL_TIME


func _end_intro_fill() -> void:
	intro_fill = 0.0
	_update_main(0)
	_update_damaged(0)
	main.action = "bar_main_filled"
	sfx_full.play()


func _enable_boss() -> void:
	if boss:
		boss.intro_delay = false


func _toggle_outro_shake() -> void:
	outro_shake = not outro_shake
	if outro_shake:
		_update_main(_get_bar_pos_from_ratio(0))
		anim.play("Defeated")
	else:
		boss_name_container.position = Vector2.ZERO
		defeated_container.position = defeated_origin


func _despawn() -> void:
	UICore.instance.clear_boss_bar()
	GameCore.instance.current_room.open_all_boss_doors()
#endregion


func update() -> void:
	_update_main()
	damage_update_timeout = DAMAGE_UPDATE_TIMEOUT
	if programmatic_shake:
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
