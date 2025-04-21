class_name MainMenu
extends Node2D

#region Variables
@export var is_main_menu:bool = false

var is_main_awaiting_input:bool = false
var input_delay_timer:float = -2.25
var click_play_text:SnailyText
#endregion


func _ready() -> void:
	if is_main_menu:
		is_main_awaiting_input = true
		click_play_text = $"ClickPlay"
		click_play_text.set_snaily_text(Statics.get_text("test"))
		click_play_text.add_border(1)
		click_play_text.add_shadow(2)
		
		var version_text = $"Version"
		var version_string = (Statics.get_text("menu_version_header") + "\n"
		+ Statics.parse_version_to_text_string(ProjectSettings.get_setting("application/config/version")))
		version_text.set_snaily_text(version_string)
		version_text.add_shadow(1)
		
		var version_panel:ContextPanel = $"VersionWarnPanel"
		version_panel.add_header(Statics.get_text("menu_olderVersion_header"), 2)
		version_panel.set_text(Statics.get_text("menu_olderVersion_body"), 1)


func _process(delta: float) -> void:
	if is_main_awaiting_input:
		input_delay_timer += delta
		click_play_text.set_visible_chars_ratio(input_delay_timer * 0.6)
		if input_delay_timer >= -1.5:
			if (Input.get_action_raw_strength("UIClick")
			or Input.get_action_raw_strength("Jump")):
				spawn_menu()
				is_main_awaiting_input = false


func spawn_menu() -> void:
	if is_main_menu:
		click_play_text.queue_free()
		$"VersionWarnPanel".queue_free()
