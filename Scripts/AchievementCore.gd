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
var queue:Array[String] = []
## Will be set to [code]true[/code] if the popup is currently visible
var currently_open:bool = false
## Will be set to [code]true[/code] if the icon of the earliest queued achievement is being displayed
var currently_displaying:bool = false

## Main background component of the popup
@onready var panel:JsonSprite2D = $"Panel"
## Achievement icon component of the popup
@onready var icon:JsonSprite2D = $"Icon"
## Text component at the top of the popup
@onready var header:SnailyText = $"Header"
## Sound played when the popup first appears
@onready var jingle:AudioStreamPlayer = $"Jingle"
## Timer that controls how long achievements should remain visible in the popup
@onready var timer:Timer = $"Timer"
#endregion


func _ready() -> void:
	panel.visible = false
	icon.visible = false
	header.visible = false
	header.set_snaily_text(&"Achievement!!")


func _process(_delta: float) -> void:
	if panel.action == "idle" and currently_open and not currently_displaying:
		currently_displaying = true
		icon.visible = true
		icon.action = queue[0]
		header.visible = true
		timer.start()


## Checks if the given achievement has been earned, and adds it to the queue if not
func check_add(id:Achievements) -> void:
	if not Statics.check_achievement(id):
		Statics.add_achievement(id)
		_add_to_queue(id)
		Statics.save_records()


## Adds the given achievement to the queue
func _add_to_queue(id:Achievements) -> void:
	var ach_str:String = Achievements.keys()[id]
	ach_str = ach_str.to_camel_case()
	queue.append(ach_str)
	if not currently_open:
		currently_open = true
		jingle.play()
		panel.visible = true
		panel.action = "open"
		header.set_snaily_text(tr("Achievement!!"))


## Called when the icon display timer times out, and either restarts the timer with the next
## achievement in the queue or closes the popup if the queue is empty
func _on_timer_timeout() -> void:
	if queue.size() == 0 or not currently_open:
		return
	queue.remove_at(0)
	if queue.size() == 0:
		icon.visible = false
		header.visible = false
		panel.action = "close"
		currently_displaying = false
		currently_open = false
	else:
		icon.action = queue[0]
		timer.start()
