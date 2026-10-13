# Copyright 2026 CE-Studio: AGPL-3.0-only
extends VBoxContainer


const ITEM_SPRITESHEET:Texture2D = preload("uid://14gb1mahe3ip")
const ITEM_SPACING:float = 10.0

var item_frame_size:Vector2i

@export var header_easy:SnailyText
@export var header_normal:SnailyText
@export var header_absurd:SnailyText
@export var header_rush:SnailyText
@export var stats_easy:SnailyText
@export var stats_normal:SnailyText
@export var stats_absurd:SnailyText
@export var stats_rush:SnailyText
@export var rush_item_group:Marker2D
@export var item_spritesheet:Texture2D


func _ready() -> void:
	header_easy.set_snaily_text(tr(&"-- Easy mode --"))
	header_normal.set_snaily_text(tr(&"-- Normal mode --"))
	header_absurd.set_snaily_text(tr(&"-- Absurd mode --") if Statics.has_times_for_mode("insane") else "-- ??? --")
	header_rush.set_snaily_text(tr(&"-- Boss rush --"))
	item_frame_size = Vector2i(
		ITEM_SPRITESHEET.get_width() / 8,
		ITEM_SPRITESHEET.get_height() / 8
	)
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
	var items_low_rush:Array = Statics.data_records["lowest_rush_inv"][chr_enum as int]
	
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
		tr(&"Fewest items:") + "\n" + _setup_rush_inv(items_low_rush)
	)


func _time_str(time:Array) -> String:
	if time[0] == 0.0 and time[1] == 0.0 and time[2] == 0.0:
		return "--:--:--"
	return Statics.format_game_time(time)


func _rate_str(rate:float) -> String:
	if rate == -1.0:
		return "--%"
	return "%.1f%%" % rate


func _setup_rush_inv(inventory:Array) -> String:
	rush_item_group.position.x = 0.0
	for child in rush_item_group.get_children():
		child.queue_free()
	if inventory.is_empty():
		return "[color=#777f83]-NA-"
	var inv_to_add:Array[int] = []
	for i in range(inventory.size()):
		for j in range(inventory[i]):
			var this_i = roundi(i)
			if this_i == 13: inv_to_add.append(11)
			else: inv_to_add.append(i)
	var spr_pos:Vector2 = Vector2.ZERO
	for i in range(inv_to_add.size()):
		var new_spr:Sprite2D = Sprite2D.new()
		new_spr.texture = ITEM_SPRITESHEET
		new_spr.hframes = item_frame_size.x
		new_spr.vframes = item_frame_size.y
		new_spr.frame = inv_to_add[i]
		rush_item_group.add_child(new_spr)
		new_spr.position = spr_pos
		spr_pos.x += ITEM_SPACING
		if i > 0:
			rush_item_group.position.x -= ITEM_SPACING * 0.5
	return ""


func _on_char_cycled(value:int) -> void:
	var ch_str:String = "snaily"
	match value:
		1: ch_str = "sluggy"
		2: ch_str = "upside"
		3: ch_str = "leggy"
		4: ch_str = "blobby"
		5: ch_str = "leechy"
	_print_info(ch_str, value as Player.Players)
