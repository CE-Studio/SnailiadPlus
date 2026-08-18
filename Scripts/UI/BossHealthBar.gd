# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name BossHealthBar
extends Node2D


#region Variables
const WIDTH:int = 250
const DAMAGE_UPDATE_TIMEOUT:float = 0.5
const DAMAGE_LERP_SPEED:float = 6.0
const SHAKE_LERP_SPEED:float = 12.0
const SHAKE_VARIANCE:float = 2.5
const INTRO_FILL_TIME:float = 1.5
const OUTRO_SHAKE:float = 2.0

## The boss that this health bar is connected to
var boss:Boss = null
## Delay timer that must zero out before the secondary bar starts to decrease
var damage_update_timeout:float = 0.0
## A timer that controls the fill-up animation played when the bar is first created
var intro_fill:float = 0.0
## If [code]true[/code], the bar will randomly bump its position by a random amount when damage is
## received. This value is controlled by the sprite's JSON and can be disabled there if the spritesheet
## contains a shake animation
var programmatic_shake:bool = true
## Will be set to [code]true[/code] if the bar is shaking as part of the boss' death animation
var outro_shake:bool = false
## The original position at creation of the health bar frame sprite
var frame_origin:Vector2 = Vector2.ZERO
## The original position at creation of the parent node of the "defeated!" text
var defeated_origin:Vector2 = Vector2.ZERO

## The main sprite representing the frame of the bar
@export var frame:SnailySprite2D
## The sprite representing the main bar, which always updates when damage is received
@export var main:SnailySprite2D
## The supplimentary sprite that masks the main bar out
@export var main_mask:SnailySprite2D
## The sprite representing the secondary damage bar, which hangs at the previous health value
## until enough time has passed
@export var damaged:SnailySprite2D
## The supplimentary sprite the masks the damage bar out
@export var damaged_mask:SnailySprite2D
## The text component that displays the boss' name
@export var boss_name:SnailyText
## The container for the boss name text
@export var boss_name_container:Node2D
## The text component that displays "defeated!"
@export var defeated:SnailyText
## The container for the defeated text
@export var defeated_container:Node2D
## The sound that plays when the bar is filling up initially
@export var sfx_beep:AudioStreamPlayer
## The sound that plays when the bar has finished filling up
@export var sfx_full:AudioStreamPlayer
## Drives the appear and defeat animations
@export var anim:AnimationPlayer
#endregion


func _ready() -> void:
	_update_main(_get_bar_pos_from_ratio(0))
	_update_damaged(_get_bar_pos_from_ratio(0))
	defeated_origin = defeated_container.position
	if frame.meta_info.size() > 0:
		var shake:Variant = frame.meta_info[0]
		if shake is bool:
			programmatic_shake = shake


## Connects an active [Boss] to this health bar and sets the displayed representation of their
## name to the [SnailyText]
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


## Hands control of this bar to a new [Boss] without needing to create an entirely new bar
func pass_control(new_boss:Boss) -> void:
	boss = new_boss
	boss.health_bar = self


func _process(delta: float) -> void:
	if damage_update_timeout <= 0.0:
		var lerp_amount = lerpf(damaged_mask.position.x, main_mask.position.x, DAMAGE_LERP_SPEED * delta)
		_update_damaged(lerp_amount)
	damage_update_timeout -= delta
	frame.position.y = lerpf(frame.position.y, frame_origin.y, SHAKE_LERP_SPEED * delta)
	
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
			frame.position = frame_origin + container_shake


#region AnimationPlayer functions
## Sets the intro fill timer. It will automatically begin to decrease and fill the bar
func _begin_intro_fill() -> void:
	intro_fill = INTRO_FILL_TIME


## Ends the intro fill animation
func _end_intro_fill() -> void:
	intro_fill = 0.0
	_update_main(0)
	_update_damaged(0)
	main.play("filled")
	main.autoplay_next = "default"
	sfx_full.play()
	frame_origin = frame.position


## Disables the intro state of the connected boss
func _enable_boss() -> void:
	if boss:
		boss.intro_delay = false


## Plays a lightened version of the fill animation if this bar was previously passed to a new [Boss]
func _refill() -> void:
	anim.play("Refill")


## Enables or disables the outro shake effect
func _toggle_outro_shake(lite:bool = false) -> void:
	outro_shake = not outro_shake
	if outro_shake:
		_update_main(_get_bar_pos_from_ratio(0))
		if not lite:
			anim.play("Defeated")
	else:
		boss_name_container.position = Vector2.ZERO
		defeated_container.position = defeated_origin


## Safely frees this bar and opens all boss doors
func _despawn() -> void:
	UICore.instance.clear_boss_bar()
	GameCore.instance.current_room.open_all_boss_doors()
#endregion


## Updates the position of the main and damage bars according to the health of the connected [Boss]
func update() -> void:
	_update_main()
	damage_update_timeout = DAMAGE_UPDATE_TIMEOUT
	if programmatic_shake:
		frame.position.y += randf_range(-SHAKE_VARIANCE, SHAKE_VARIANCE)
	#frame.action = "frame_damage"
	#main.action = "bar_main_damage"


## Plays the sound set when filling the bar
func play_beep() -> void:
	sfx_beep.play()


## Updates the position of the main bar
func _update_main(value:float = _get_bar_pos_from_health()) -> void:
	main_mask.position.x = value
	main.position.x = -value


## Updates the position of the damage bar
func _update_damaged(value:float) -> void:
	damaged_mask.position.x = value
	damaged.position.x = -value


## Returns an X position for the bar masks based on what the connected [Boss]'s health and max health are
func _get_bar_pos_from_health() -> float:
	if not boss:
		return WIDTH * 0.5
	var hp:float = boss.health
	var hp_max:float = boss.max_health
	var ratio:float = hp / hp_max
	return -WIDTH + ceili(ratio * WIDTH)


## Returns an X position for the bar masks based on a float value from 0.0 to 1.0
func _get_bar_pos_from_ratio(ratio:float) -> float:
	return -WIDTH + ceili(ratio * WIDTH)
