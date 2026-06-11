# Copyright 2026 CE-Studio: AGPL-3.0-only
extends VBoxContainer


#region Variables
var pic_states:Array[bool] = [ true, false, false, false, false ]

@export var bg:Sprite2D
@export var pic:Sprite2D
@export var main_scroller:HeaderlessScrollingSnailyButton
@export var pic_normal:Texture2D
@export var pic_rush:Texture2D
@export var pic_100:Texture2D
@export var pic_sub30:Texture2D
@export var pic_absurd:Texture2D
@export var pic_locked:Texture2D
#endregion


func _ready() -> void:
	var new_option_array:Array[String] = [ tr(&"Normal clear") ]
	if Statics.has_times_for_mode("rush"):
		pic_states[1] = true
		new_option_array.append(tr(&"Boss Rush clear"))
	else: new_option_array.append(tr(&"???"))
	if Statics.can_unlock(Statics.Unlocks.ITEM_RANDO):
		pic_states[2] = true
		new_option_array.append(tr(&"100% clear"))
	else: new_option_array.append(tr(&"???"))
	if Statics.can_unlock(Statics.Unlocks.ABSURD_DIFF):
		pic_states[3] = true
		new_option_array.append(tr(&"<30 min clear"))
	else: new_option_array.append(tr(&"???"))
	if Statics.has_times_for_mode("absurd"):
		pic_states[4] = true
		new_option_array.append(tr(&"Absurd Mode clear"))
	else: new_option_array.append(tr(&"???"))
	main_scroller.remote_import_new_options(new_option_array)


func _on_option_cycled(value) -> void:
	if pic_states[value]:
		match value:
			0: pic.texture = pic_normal
			1: pic.texture = pic_rush
			2: pic.texture = pic_100
			3: pic.texture = pic_sub30
			4: pic.texture = pic_absurd
	else:
		pic.texture = pic_locked
