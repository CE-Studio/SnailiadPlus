extends VBoxContainer


const NEW_GAME_LAYER = "NewGame"
const LOAD_GAME_LAYER = "ProfileSpawn"
const BLINK_MULT = 4.8

var pro1_empty = true
var pro1_layer = NEW_GAME_LAYER
var pro2_empty = true
var pro2_layer = NEW_GAME_LAYER
var pro3_empty = true
var pro3_layer = NEW_GAME_LAYER

var button_selection:Vector2i = Vector2i.ZERO
var blink_timer:float = 0.0
var select_color:Color = Statics.get_color(Vector2i(1, 8))
var panel_up:bool = false
var last_focus:SnailyButton

var layer:MenuLayer

enum LayerState {
	NORMAL,
	COPY1,
	COPY2,
	ERASE,
}
var layer_state:LayerState = LayerState.NORMAL

@onready var btn_pro1:ContextSnailyButton = $"Profile1"
@onready var btn_pro2:ContextSnailyButton = $"Profile2"
@onready var btn_pro3:ContextSnailyButton = $"Profile3"
@onready var btn_copy:ActionSnailyButton = $"HBoxContainer/Copy"
@onready var btn_erase:ActionSnailyButton = $"HBoxContainer/Erase"
@onready var panel:ContextPanel = $"../ContextPanel"
@onready var sfx_beep:AudioStreamPlayer = $"../AudioGroup/Select"
@onready var sfx_copy:AudioStreamPlayer = $"../AudioGroup/Copy"
@onready var sfx_erase:AudioStreamPlayer = $"../AudioGroup/Erase"


func _ready() -> void:
	layer = get_parent()
	layer.meta_info.append(0)
	_update_profile_buttons()
	panel.modulate.a = 0
	panel.call_deferred("add_button",
		Statics.get_text("menu_add_generic_no"), _on_panel_no, false)
	panel.call_deferred("add_button",
		Statics.get_text("menu_add_generic_yes"), _on_panel_yes, false)


func _process(delta: float) -> void:
	blink_timer += delta * BLINK_MULT
	var this_color = Color.WHITE.lerp(select_color, abs(sin(blink_timer)))
	match layer_state:
		LayerState.NORMAL:
			btn_pro1.text.self_modulate = Color.WHITE
			btn_pro2.text.self_modulate = Color.WHITE
			btn_pro3.text.self_modulate = Color.WHITE
			btn_copy.text.self_modulate = Color.WHITE
			btn_erase.text.self_modulate = Color.WHITE
		LayerState.COPY1:
			btn_pro1.text.self_modulate = Color.WHITE
			btn_pro2.text.self_modulate = Color.WHITE
			btn_pro3.text.self_modulate = Color.WHITE
			btn_copy.text.self_modulate = this_color
		LayerState.COPY2:
			btn_copy.text.self_modulate = this_color
			match button_selection.x:
				1: btn_pro1.text.self_modulate = this_color
				2: btn_pro2.text.self_modulate = this_color
				3: btn_pro3.text.self_modulate = this_color
		LayerState.ERASE:
			btn_erase.text.self_modulate = this_color
	if panel_up:
		panel.modulate.a = lerpf(panel.modulate.a, 1.0, MenuLayer.MOVE_RATE * delta)
		panel.position.y = lerp(panel.position.y, 80.0, MenuLayer.MOVE_RATE * delta)
		layer.menu.selector_y_offset = 80.0 - panel.position.y
	else:
		panel.modulate.a = lerpf(panel.modulate.a, 0.0, MenuLayer.MOVE_RATE * delta)
		panel.position.y = lerp(panel.position.y, 240.0, MenuLayer.MOVE_RATE * delta)
		layer.menu.selector_y_offset = 0


func _update_profile_buttons() -> void:
	for i in range(3):
		var profile:Dictionary
		var button:ContextSnailyButton
		match i:
			0:
				profile = Statics.data_profile1
				pro1_empty = profile["is_empty"]
				pro1_layer = NEW_GAME_LAYER if pro1_empty else LOAD_GAME_LAYER
				button = $"Profile1"
			1:
				profile = Statics.data_profile2
				pro2_empty = profile["is_empty"]
				pro2_layer = NEW_GAME_LAYER if pro2_empty else LOAD_GAME_LAYER
				button = $"Profile2"
			2:
				profile = Statics.data_profile3
				pro3_empty = profile["is_empty"]
				pro3_layer = NEW_GAME_LAYER if pro3_empty else LOAD_GAME_LAYER
				button = $"Profile3"
		if not profile["is_empty"]:
			var _char = str(int(profile["character"]))
			button.set_text(Statics.get_text("char_" + _char) + " " + str(i + 1))
			var stats = Statics.get_text("difficulty_" + str(int(profile["difficulty"])))
			stats += " / " + Statics.format_game_time(profile["game_time"])
			stats += " / %.1f%%" % profile["item_rate"]
			if profile["r_shuffle_level"] >= 0:
				stats += " / %08d" % str(profile["r_seed"])
			button.set_subtext(stats)
		else:
			button.set_text(Statics.get_text("menu_option_profile_empty"))
			button.set_subtext(Statics.get_text("menu_option_profile_empty_context"))


func focus_panel() -> void:
	layer.can_focus = false
	layer.menu.read_inputs = false
	panel_up = true
	if layer_state == LayerState.COPY2:
		var copy_str = Statics.get_text("menu_option_copyGame_popup")
		copy_str = copy_str % [ button_selection.x, button_selection.y ]
		panel.set_text(copy_str, 2)
	elif layer_state == LayerState.ERASE:
		var erase_str = Statics.get_text("menu_option_eraseGame_popup")
		erase_str = erase_str % button_selection.x
		panel.set_text(erase_str, 2)
	panel.can_focus = true
	panel.focus_button(0)
	layer.menu.selector_y_offset = 80.0 - panel.position.y


func defocus_panel() -> void:
	panel_up = false
	panel.can_focus = false
	layer.can_focus = true
	layer.menu.set_deferred("read_inputs", true)
	last_focus.grab_focus()


func _on_profile1_selected(_value) -> void:
	match layer_state:
		LayerState.NORMAL:
			layer.meta_info[0] = 1
			layer.remote_create_layer(pro1_layer)
		LayerState.COPY1:
			if not pro1_empty:
				button_selection.x = 1
				layer_state = LayerState.COPY2
				sfx_beep.play()
		LayerState.COPY2:
			if button_selection.x == 1:
				button_selection.x = 0
				layer_state = LayerState.COPY1
			else:
				button_selection.y = 1
				last_focus = btn_pro1
				focus_panel()
		LayerState.ERASE:
			if not pro1_empty:
				button_selection.x = 1
				last_focus = btn_pro1
				focus_panel()


func _on_profile2_selected(_value) -> void:
	match layer_state:
		LayerState.NORMAL:
			layer.meta_info[0] = 2
			layer.remote_create_layer(pro2_layer)
		LayerState.COPY1:
			if not pro2_empty:
				button_selection.x = 2
				layer_state = LayerState.COPY2
				sfx_beep.play()
		LayerState.COPY2:
			if button_selection.x == 2:
				button_selection.x = 0
				layer_state = LayerState.COPY1
			else:
				button_selection.y = 2
				last_focus = btn_pro2
				focus_panel()
		LayerState.ERASE:
			if not pro2_empty:
				button_selection.x = 2
				last_focus = btn_pro2
				focus_panel()


func _on_profile3_selected(_value) -> void:
	match layer_state:
		LayerState.NORMAL:
			layer.meta_info[0] = 3
			layer.remote_create_layer(pro3_layer)
		LayerState.COPY1:
			if not pro3_empty:
				button_selection.x = 3
				layer_state = LayerState.COPY2
				sfx_beep.play()
		LayerState.COPY2:
			if button_selection.x == 3:
				button_selection.x = 0
				layer_state = LayerState.COPY1
			else:
				button_selection.y = 3
				last_focus = btn_pro3
				focus_panel()
		LayerState.ERASE:
			if not pro3_empty:
				button_selection.x = 3
				last_focus = btn_pro3
				focus_panel()


func _on_copy_selected(_value) -> void:
	match layer_state:
		LayerState.NORMAL:
			layer_state = LayerState.COPY1
		LayerState.COPY1:
			layer_state = LayerState.NORMAL
			button_selection = Vector2i.ZERO
		LayerState.COPY2:
			layer_state = LayerState.NORMAL
			button_selection = Vector2i.ZERO
	sfx_beep.play()


func _on_erase_selected(_value) -> void:
	match layer_state:
		LayerState.NORMAL:
			layer_state = LayerState.ERASE
		LayerState.ERASE:
			layer_state = LayerState.NORMAL
			button_selection = Vector2i.ZERO
	sfx_beep.play()


func _on_panel_no(_value) -> void:
	defocus_panel()


func _on_panel_yes(_value) -> void:
	if layer_state == LayerState.COPY2:
		var file_to_copy:Dictionary
		match button_selection.x:
			1:
				file_to_copy = Statics.data_profile1
			2:
				file_to_copy = Statics.data_profile2
			3:
				file_to_copy = Statics.data_profile3
		match button_selection.y:
			1:
				Statics.data_profile1 = file_to_copy.duplicate()
			2:
				Statics.data_profile2 = file_to_copy.duplicate()
			3:
				Statics.data_profile3 = file_to_copy.duplicate()
		layer.menu.save_profile(button_selection.y)
		sfx_copy.play()
	elif layer_state == LayerState.ERASE:
		Statics.delete_profile(button_selection.x)
		match  button_selection.x:
			1:
				Statics.data_profile1 = StaticProcess.template_profile.duplicate()
			2:
				Statics.data_profile2 = StaticProcess.template_profile.duplicate()
			3:
				Statics.data_profile3 = StaticProcess.template_profile.duplicate()
		layer.menu.play_save_anim()
		sfx_erase.play()
	defocus_panel()
	layer_state = LayerState.NORMAL
	button_selection = Vector2.ZERO
	_update_profile_buttons()
