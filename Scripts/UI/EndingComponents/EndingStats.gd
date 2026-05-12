# Copyright 2026 CE-Studio: AGPL-3.0-only
extends Node2D


#region Variables
const ITEM_TICK_TIME:float = 1.0
const TIME_TICK_TIME:float = 2.0

## Will be set if the item percentage is currently ticking up
var ticking_items:bool = false
## The target item percentage to count toward
var final_items:float = 0.0
## The current progress of the item percentage counter, from 0.0 to 1.0
var progress_items:float = 0.0
## Will be set if the clear time is currently ticking up
var ticking_time:bool = false
## The target clear time to count toward
var final_time:float = 0.0
## The current progress of the clear time counter, from 0.0 to 1.0
var progress_time:float = 0.0
## Will be set if the item percentage is lower than the lowest saved percentage
var is_new_lowest_items:bool = false
## Will be set if the clear time is lower than the best saved time
var is_new_best_time:bool = false
## Will ensure that the tick sound is played only every other frame
var tick_flag:bool = true
## The time ID that needs to be saved
var time_id:String = ""

## The animation node that drives the entire stats screen
@export var anim:AnimationPlayer
## The background sprite
@export var end_bg:JsonSprite2D
## The snail sprite
@export var end_pic:JsonSprite2D
## The "congratulations!" header text
@export var header:SnailyText
## The text that displays the character and difficulty played with
@export var char_diff:SnailyText
## The container that holds all item text
@export var item_container:HBoxContainer
## The "items collected" header text
@export var item_header:SnailyText
## The percentage counter for item collection
@export var item_counter:SnailyText
## The label that shows when a new lowest item percentage is scored
@export var new_lowest_items:SnailyText
## The container that holds all time text
@export var time_container:HBoxContainer
## The "completion time" text
@export var time_header:SnailyText
## The counter for time taken to beat the game
@export var time_counter:SnailyText
## The label that shows when a new lowest clear time is scored
@export var new_best_time:SnailyText
## The sound that plays when a text counter is ticked up
@export var sfx_tick:AudioStreamPlayer
## The sound that plays when a new best is recorded on a counter
@export var sfx_best:AudioStreamPlayer
#endregion


func _ready() -> void:
	end_bg.modulate.a = 0.0
	end_pic.modulate.a = 0.0
	header.visible_ratio = 0.0
	header.set_snaily_text(tr(&"Congratulations!!"), true)
	char_diff.position.y += 240
	char_diff.set_snaily_text(" - ".join([
		GlobalText.characters[Statics.current_profile["character"]][0],
		GlobalText.difficulties[Statics.current_profile["difficulty"]]
		]), true)
	item_container.position.y += 240
	item_header.set_snaily_text(tr(&"Items collected:"), true)
	item_counter.set_snaily_text("0.0%", true)
	new_lowest_items.set_snaily_text("New lowest!!")
	new_lowest_items.visible = false
	new_lowest_items.enable_rainbow()
	time_container.position.y += 240
	time_header.set_snaily_text(tr(&"Completion time:"), true)
	time_counter.set_snaily_text("0:00:00.00", true)
	new_best_time.set_snaily_text("New best!!")
	new_best_time.visible = false
	new_best_time.enable_rainbow()
	header.enable_rainbow_scroll()
	
	final_items = Statics.get_item_percentage()
	var lowest_items:float = Statics.get_lowest_percent(
		Statics.current_profile["character"] as int, Statics.current_profile["difficulty"]
	)
	if lowest_items != -1 and lowest_items > final_items:
		is_new_lowest_items = true
	
	var time:Array = Statics.current_profile["game_time"]
	final_time = (time[0] * 60.0 * 60.0) + (time[1] * 60) + time[2]
	time_id = Statics.infer_time_id()
	if Statics.has_time(time_id) and Statics.compare_times(Statics.get_time(time_id), time) > 0:
		is_new_best_time = true


func _process(delta: float) -> void:
	_tick_items(delta)
	_tick_time(delta)


## Starts the stats animation
func start_anim() -> void:
	anim.play(&"Ending")


## Ticks up the item percentage counter
func _tick_items(delta:float) -> void:
	if not ticking_items:
		return
	progress_items = move_toward(progress_items, 1.0, delta / ITEM_TICK_TIME)
	var counter_state = lerpf(0.0, final_items, progress_items)
	item_counter.set_snaily_text("%.1f%%" % counter_state, true)
	var played_sound:bool = false
	if progress_items == 1.0:
		ticking_items = false
		if final_items >= 100.0 or is_new_lowest_items:
			item_counter.enable_rainbow_scroll()
			sfx_best.play()
			played_sound = true
			if is_new_lowest_items:
				new_lowest_items.visible = true
				new_lowest_items.position.x = (
					item_counter.global_position.x - new_lowest_items.size.x
					+ item_counter.size.x + 16
				)
				new_lowest_items.position.y = item_counter.global_position.y + 4
	if not played_sound:
		if tick_flag:
			sfx_tick.play()
		tick_flag = not tick_flag

## Starts ticking the item rate
func start_ticking_items() -> void:
	ticking_items = true


## Ticks up the item percentage counter
func _tick_time(delta:float) -> void:
	if not ticking_time:
		return
	progress_time = move_toward(progress_time, 1.0, delta / TIME_TICK_TIME)
	var counter_state = lerpf(0.0, final_time, progress_time)
	var seconds:float = counter_state
	var minutes:int = 0
	var hours:int = 0
	while seconds >= 60.0:
		seconds -= 60.0
		minutes += 1
	while minutes >= 60:
		minutes -= 60
		hours += 1
	time_counter.set_snaily_text(Statics.format_game_time([hours, minutes, seconds]))
	var played_sound:bool = false
	if progress_time == 1.0:
		ticking_time = false
		if is_new_best_time:
			time_counter.enable_rainbow_scroll()
			sfx_best.play()
			played_sound = true
			new_best_time.visible = true
			new_best_time.position.x = (
				time_counter.global_position.x - new_best_time.size.x
				+ time_counter.size.x + 16
			)
			new_best_time.position.y = time_counter.global_position.y + 4
	if not played_sound:
		if tick_flag:
			sfx_tick.play()
		tick_flag = not tick_flag

## Starts ticking the item rate
func start_ticking_time() -> void:
	ticking_time = true
