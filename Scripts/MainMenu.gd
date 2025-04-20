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
		
		var version_text = $"Version"
		var version_string = (Statics.get_text("menu_version_header") + "\n"
		+ Statics.parse_version_to_text_string(ProjectSettings.get_setting("application/config/version")))
		version_text.set_snaily_text(version_string)
		version_text.add_shadow(1)
		
		var version_panel:ContextPanel = $"ContextPanel"
		version_panel.add_header(Statics.get_text("menu_olderVersion_header"), 2)
		version_panel.set_text(Statics.get_text("menu_olderVersion_body"), 1, 260)
