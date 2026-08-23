# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name EndingStats
extends Node2D


#region Variables
const ITEM_TICK_TIME:float = 1.0
const TIME_TICK_TIME:float = 2.0

## Will be set if the stats screen animation has finished playing
var has_started:bool = false
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
## Will be set if the item percentage is lower than the lowest saved percentage or higher than the best saved
var is_new_best_items:int = 0
## Will be set if the clear time is lower than the best saved time
var is_new_best_time:bool = false
## Will ensure that the tick sound is played only every other frame
var tick_flag:bool = true
## The time ID that needs to be saved
var time_id:String = ""
## The time that needs to be saved
var time_to_save:Array = []
## The item rate that needs to be saved
var items_to_save:float = 0.0
## Time counter that controls the flashing of the continue prompt
var elapsed:float = -PI
## Will be set if the stats screen is currently fading out
var fading_out:bool = false

## Reference to the credits instance that spawns this results screen
var credits:EndingCredits

## The animation node that drives the entire stats screen
@export var anim:AnimationPlayer
## The background sprite
@export var end_bg:SnailySprite2D
## The snail sprite
@export var end_pic:SnailySprite2D
## The container that holds the header
@export var header_container:HBoxContainer
## The "congratulations!" header text
@export var header:SnailyText
## The container that holds the character and difficulty
@export var char_diff_container:HBoxContainer
## The text that displays the character and difficulty played with
@export var char_diff:SnailyText
## The container that holds all item text
@export var item_container:HBoxContainer
## The "items collected" header text
@export var item_header:SnailyText
## The percentage counter for item collection
@export var item_counter:SnailyText
## The label that shows when a new lowest item percentage is scored
@export var new_best_items:SnailyText
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
## The sound that plays when the stats screen is advanced
@export var sfx_continue:AudioStreamPlayer
## The label that prompts any input to advance out of the ending
@export var continue_prompt:SnailyText
## The sprite that covers the entire stats screen to fade it out
@export var fade_cover:Sprite2D
#endregion


func _ready() -> void:
	end_bg.modulate.a = 0.0
	end_pic.modulate.a = 0.0
	header.visible_ratio = 0.0
	header.set_snaily_text(tr(&"Congratulations!!"), true)
	header.custom_minimum_size.x = header.get_width()
	char_diff_container.position.y += 240
	char_diff.set_snaily_text(" - ".join([
		GlobalText.characters[Statics.current_profile["character"]][0],
		GlobalText.difficulties[Statics.current_profile["difficulty"]]
		]), true)
	char_diff.custom_minimum_size.x = char_diff.get_width()
	item_container.position.y += 240
	item_header.set_snaily_text(tr(&"Items collected:"), true)
	item_counter.set_snaily_text("0.0%", true)
	new_best_items.set_snaily_text(tr(&"New highest!!"))
	new_best_items.visible = false
	new_best_items.enable_rainbow()
	time_container.position.y += 240
	time_header.set_snaily_text(tr(&"Completion time:"), true)
	time_counter.set_snaily_text("0:00:00.00", true)
	new_best_time.set_snaily_text(tr(&"New best!!"))
	new_best_time.visible = false
	new_best_time.enable_rainbow()
	header.enable_rainbow_scroll()
	continue_prompt.modulate.a = 0.0
	continue_prompt.set_snaily_text(tr(&"Press anything to save and continue"))
	
	final_items = Statics.get_item_percentage()
	items_to_save = final_items
	var highest_items:float = Statics.get_highest_percent(
		Statics.current_profile["character"] as int, Statics.current_profile["difficulty"]
	)
	if highest_items != -1 and highest_items < final_items and absf(highest_items - final_items) >= 0.1:
		is_new_best_items = 1
	var lowest_items:float = Statics.get_lowest_percent(
		Statics.current_profile["character"] as int, Statics.current_profile["difficulty"]
	)
	if lowest_items != -1 and lowest_items > final_items and absf(lowest_items - final_items) >= 0.1:
		is_new_best_items = -1
		new_best_items.set_snaily_text(tr(&"New lowest!!"))
	
	var time:Array = Statics.current_profile["game_time"]
	time_to_save = time.duplicate()
	final_time = (time[0] * 60.0 * 60.0) + (time[1] * 60) + time[2]
	time_id = Statics.infer_time_id()
	if Statics.has_time(time_id) and Statics.compare_times(Statics.get_time(time_id), time) > 0:
		is_new_best_time = true
	
	if Statics.current_profile["difficulty"] == 2:
		end_bg.play("insane")
		end_pic.play("insane")
	elif Statics.compare_times(time, [0, 30, 0.0]) < 0:
		end_bg.play("sub30")
		end_pic.play("sub30")
	elif final_items >= 100.0:
		end_bg.play("100")
		end_pic.play("100")
	else:
		end_bg.play("normal")
		end_pic.play("normal")


func _process(delta: float) -> void:
	_tick_items(delta)
	_tick_time(delta)
	
	if has_started and not anim.is_playing():
		elapsed += delta * 4.0
		continue_prompt.modulate.a = cos(elapsed) + 1.0
		if SInput.check_any_button():
			_save_scores()
			fading_out = true
			sfx_continue.play()
	if fading_out:
		fade_cover.modulate.a += delta * 0.5
		credits.music_main.volume_linear = move_toward(
			credits.music_main.volume_linear, 0.0, delta * 0.5)
		credits.music_alt.volume_linear = credits.music_main.volume_linear
		if fade_cover.modulate.a > 1.25 and not credits.is_queued_for_deletion():
			credits.despawn()


## Starts the stats animation
func start_anim() -> void:
	anim.play(&"Ending")
	has_started = true


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
		if final_items >= 100.0 or is_new_best_items != 0:
			item_counter.enable_rainbow_scroll()
			sfx_best.play()
			played_sound = true
			if is_new_best_items != 0:
				new_best_items.visible = true
				new_best_items.position.x = (
					item_counter.global_position.x - new_best_items.size.x
					+ item_counter.size.x + 16
				)
				new_best_items.position.y = item_counter.global_position.y + 4
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


## Saves the best time and item rate to file if applicable
func _save_scores() -> void:
	if is_new_best_time or not Statics.has_time(time_id):
		Statics.save_time(time_id, time_to_save)
	var character:Player.Players = Player.instance.who_i_is
	var difficulty:int = Statics.current_profile["difficulty"]
	if is_new_best_items == 1 or Statics.get_highest_percent(character, difficulty) == -1:
		Statics.save_highest_percent(character, difficulty, items_to_save)
	if is_new_best_items == -1 or Statics.get_lowest_percent(character, difficulty) == -1:
		Statics.save_lowest_percent(character, difficulty, items_to_save)
	Statics.save_records()
