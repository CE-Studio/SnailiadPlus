class_name MainMenu
extends Node2D

#region Variables
@export var is_main_menu:bool = false

var is_main_awaiting_input:bool = false
var version_panel_active:bool = false
var input_delay_timer:float = -2.25
var click_play_text:SnailyText
var version_panel:ContextPanel
#endregion


func _ready() -> void:
	if is_main_menu:
		if not Statics.main_menu_booted_once:
			var saved_ver = Statics.parse_version_to_array(Statics.data_general["game_version"])
			var current_ver = Statics.parse_version_to_array(ProjectSettings.get_setting("application/config/version"))
			var ver_compare = Statics.compare_versions(saved_ver, current_ver)
			if ver_compare == 1:
				version_panel = $"VersionWarnPanel"
				version_panel.add_header(Statics.get_text("menu_olderVersion_header"), 2)
				version_panel.set_text(Statics.get_text("menu_olderVersion_body"), 1)
				version_panel.add_button(Statics.get_text("menu_olderVersion_confirm"), spawn_menu)
				$"ColorCover".set_new_fade(Color(0.0, 0.0, 0.0, 1.0), Color(0.0, 0.0, 0.0, 0.4), 0.5)
			else:
				$"VersionWarnPanel".queue_free()
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


func _process(delta: float) -> void:
	if click_play_text:
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
		if click_play_text:
			click_play_text.queue_free()
		if version_panel:
			version_panel.queue_free()
			$"ColorCover".set_new_fade($"ColorCover".end_color, Color(0.0, 0.0, 0.0, 0.0), 0.25)
		Statics.main_menu_booted_once = true
		Statics.data_general["game_version"] = ProjectSettings.get_setting("application/config/version")
		Statics.save_general()
		Statics.current_profile = Statics.data_profile1
