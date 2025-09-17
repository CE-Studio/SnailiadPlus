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
	MAZE_BIRDS,
	FLASH_TEST_ROOMS,
	WIN_ABSURD,
	WIN_RANDOMIZER,
	GRAVITY_SHOCK
}

var queue:Array[String] = []
var currently_open:bool = false
var currently_displaying:bool = false

@onready var panel:JsonSprite2D = $"Panel"
@onready var icon:JsonSprite2D = $"Icon"
@onready var header:SnailyText = $"Header"
@onready var jingle:AudioStreamPlayer = $"Jingle"
@onready var timer:Timer = $"Timer"
#endregion


func _ready() -> void:
	panel.visible = false
	icon.visible = false
	header.visible = false


func _process(_delta: float) -> void:
	if panel.action == "idle" and currently_open and not currently_displaying:
		currently_displaying = true
		icon.visible = true
		icon.action = queue[0]
		header.visible = true
		timer.start()


func check_add(id:Achievements) -> void:
	if not Statics.check_achievement(id):
		Statics.add_achievement(id)
		_add_to_queue(id)
		Statics.save_records()


func _add_to_queue(id:Achievements) -> void:
	var ach_str:String = Achievements.keys()[id]
	ach_str = ach_str.to_camel_case()
	queue.append(ach_str)
	if not currently_open:
		currently_open = true
		jingle.play()
		panel.visible = true
		panel.action = "open"
		header.set_snaily_text("hud_achievement")


func _on_timer_timeout() -> void:
	if queue.size() == 0:
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
