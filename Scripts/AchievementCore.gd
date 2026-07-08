# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name AchievementCore
extends Node


#region Variables
enum Achievements {
	BEAT_SHELLBREAKER,
	BEAT_STOMPY,
	BEAT_SPACE_BOX,
	BEAT_MOON_SNAIL,
	BEAT_MOON_SNAIL_NO_ARMOR,
	MAP_100,
	SUN_SNAIL,
	ITEMS_100,
	WIN_SLUGGY,
	WIN_UPSIDE,
	WIN_LEGGY,
	WIN_BLOBBY,
	WIN_LEECHY,
	UNDER_30_MIN,
	BOSS_RUSH,
	FIND_SHRINE,
	SNELK_A,
	SNELK_B,
	SECRET_BOOMERANG,
	REMAKE_TEST_ROOMS,
	FILL_BESTIARY,
	FLASH_TEST_ROOMS,
	WIN_ABSURD,
	WIN_RANDOMIZER,
	GRAVITY_SHOCK
}

## Array of achievements that have been queued up to show as earned. As long as there are
## achievements in the queue, the popup will remain active
var queue:Array[int] = []
## Will be set to [code]true[/code] if the popup is currently visible
var currently_open:bool = false
## Will be set to [code]true[/code] if the icon of the earliest queued achievement is being displayed
var currently_displaying:bool = false
## Tracks how long the achievement popup has been open for
var time_open:float = 0.0

## The active instance of this script
static var instance:AchievementCore

## Main background component of the popup
@export var panel:SnailySprite2D
## Achievement icon component of the popup
@export var icon:Sprite2D
## Text component at the top of the popup
@export var header:SnailyText
## Sound played when the popup first appears
@export var jingle:AudioStreamPlayer
## Timer that controls how long achievements should remain visible in the popup
@export var timer:Timer
#endregion


func _ready() -> void:
	instance = self
	panel.visible = false
	icon.visible = false
	header.visible = false
	header.set_snaily_text(tr(&"Achievement!!"))
	header.set_default_flashy(2)
	header.enable_rainbow_scroll()


func _process(delta: float) -> void:
	if panel.animation == "default" and currently_open and not currently_displaying:
		currently_displaying = true
		icon.visible = true
		icon.frame = queue[0] + 1
		header.visible = true
		timer.start()
	if currently_displaying:
		time_open += delta
	if Input.is_key_pressed(KEY_P):
		_add_to_queue(randi_range(0, 24))


## Checks if the given achievement has been earned, and adds it to the queue if not
func check_add(id:Achievements) -> bool:
	if not Statics.check_achievement(id):
		Statics.add_achievement(id)
		_add_to_queue(id)
	return false


## Adds the given achievement to the queue
func _add_to_queue(id:Achievements) -> void:
	queue.append(id as int)
	if not currently_open:
		currently_open = true
		currently_displaying = false
		jingle.play()
		panel.visible = true
		panel.play("open")
		panel.autoplay_next = "default"
		time_open = 0.0


## Called when the icon display timer times out, and either restarts the timer with the next
## achievement in the queue or closes the popup if the queue is empty
func _on_timer_timeout() -> void:
	if time_open < 0.25:
		timer.start()
		return
	if queue.size() == 0 or not currently_open:
		return
	queue.remove_at(0)
	if queue.size() == 0:
		icon.visible = false
		header.visible = false
		panel.play("close")
		currently_displaying = false
		currently_open = false
		time_open = 0.0
	else:
		icon.frame = queue[0] + 1
		timer.start()
