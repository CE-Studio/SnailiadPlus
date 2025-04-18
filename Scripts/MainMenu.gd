class_name MainMenu
extends Node2D

#region Variables
@export var is_main_menu:bool = false

var is_main_awaiting_input:bool = false
#endregion


func _ready() -> void:
	if is_main_menu:
		is_main_awaiting_input = true
		var click_play_text = $"ClickPlay"
		click_play_text.set_snaily_text(Statics.get_text("test"))
		click_play_text.add_border(1)
		click_play_text.add_shadow(2)
