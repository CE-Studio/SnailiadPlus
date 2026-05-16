# Copyright 2026 CE-Studio: AGPL-3.0-only
extends VBoxContainer


@export var header_easy:SnailyText
@export var header_normal:SnailyText
@export var header_absurd:SnailyText
@export var header_rush:SnailyText
@export var stats_easy:SnailyText
@export var stats_normal:SnailyText
@export var stats_absurd:SnailyText
@export var stats_rush:SnailyText


func _ready() -> void:
	header_easy.set_snaily_text(tr(&"-- Easy mode --"))
	header_normal.set_snaily_text(tr(&"-- Normal mode --"))
	header_absurd.set_snaily_text(tr(&"-- Absurd mode --") if Statics.has_times_for_mode("insane") else "-- ??? --")
	header_rush.set_snaily_text(tr(&"-- Boss rush --"))
	_print_info("snaily", Player.Players.SNAILY)


func _print_info(ch_str:String, chr_enum:Player.Players) -> void:
	var time_easy:Array = Statics.get_time(ch_str + "_easy")
	var time_normal:Array = Statics.get_time(ch_str + "_normal")
	var time_absurd:Array = Statics.get_time(ch_str + "_insane")
	var time_rush:Array = Statics.get_time(ch_str + "_rush")
	var items_high_easy:float = Statics.get_highest_percent(chr_enum, 0)
	var items_high_normal:float = Statics.get_highest_percent(chr_enum, 1)
	var items_high_absurd:float = Statics.get_highest_percent(chr_enum, 2)
	var items_low_easy:float = Statics.get_lowest_percent(chr_enum, 0)
	var items_low_normal:float = Statics.get_lowest_percent(chr_enum, 1)
	var items_low_absurd:float = Statics.get_lowest_percent(chr_enum, 2)
	
	stats_easy.set_snaily_text(
		tr(&"Best time:") + "   " + _time_str(time_easy) + "\n" +
		tr(&"Best item rate:") + "   " + _rate_str(items_high_easy) + "\n" +
		tr(&"Lowest item rate:") + "   " + _rate_str(items_low_easy)
	)
	stats_normal.set_snaily_text(
		tr(&"Best time:") + "   " + _time_str(time_normal) + "\n" +
		tr(&"Best item rate:") + "   " + _rate_str(items_high_normal) + "\n" +
		tr(&"Lowest item rate:") + "   " + _rate_str(items_low_normal)
	)
	stats_absurd.set_snaily_text(
		tr(&"Best time:") + "   " + _time_str(time_absurd) + "\n" +
		tr(&"Best item rate:") + "   " + _rate_str(items_high_absurd) + "\n" +
		tr(&"Lowest item rate:") + "   " + _rate_str(items_low_absurd)
	)
	stats_rush.set_snaily_text(
		tr(&"Best time:") + "   " + _time_str(time_rush) + "\n" +
		tr(&"Fewest items:") + "\n" + _rush_inv_str()
	)


func _time_str(time:Array) -> String:
	if time[0] == 0.0 and time[1] == 0.0 and time[2] == 0.0:
		return "--:--:--"
	return Statics.format_game_time(time)


func _rate_str(rate:float) -> String:
	if rate == -1.0:
		return "--%"
	return "%.1f%%" % rate


func _rush_inv_str() -> String:
	return "[color=#777f83]-NA-"
