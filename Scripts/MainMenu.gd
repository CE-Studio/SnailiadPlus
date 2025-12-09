class_name MainMenu
extends Node2D

#region Variables
const TITLE_REST_Y = 40
const TITLE_MOVE_RATE = 8
const LAYER_PATH = "res://Scenes/UI/MenuLayers/%s.tscn"
const SELECTOR_MOVE_RATE = 20
const SELECTOR_OFFSET = Vector2i(16, -2)
const HIDE_FADE_RATE = 16

@export var is_main_menu:bool = false

var is_main_awaiting_input:bool = false
var version_panel_active:bool = false
var input_delay_timer:float = -2.25
var click_play_text:SnailyText
var version_panel:ContextPanel
var active_layers:int
var active_layer:MenuLayer
var selector_y_offset:float
var spawn_buffer_frames:int = 2
var read_inputs:bool = true:
	set(value):
		read_inputs = value
		if not value:
			read_esc = value
var read_esc:bool = false

@onready var title:Node2D = $"Title"
@onready var version_text:SnailyText = $"Version"
@onready var layer_group:Node2D = $"LayerGroup"
@onready var selectors:Array = [ $"LeftSelector", $"RightSelector" ]
@onready var save_icon:JsonSprite2D = $"SaveIcon"
@onready var color_cover:ColorCover = $"ColorCover"
#endregion


func _ready() -> void:
	save_icon.visible = false
	
	var number_str:String = Statics.parse_version_to_text_string(ProjectSettings.get_setting("application/config/version"))
	var version_str:String = tr(&"Engine version") + "\n" + number_str
	version_text.set_snaily_text(version_str)
	version_text.add_shadow(1)

	if is_main_menu:
		Statics.active_room = $"TitleRoom"
		Statics.active_room.spawn(true)
		get_tree().paused = false
		selectors[0].action = "left_0"
		selectors[1].action = "right_0"
		if not Statics.main_menu_booted_once:
			var saved_ver := Statics.parse_version_to_array(ProjectSettings.get_setting("application/config/old_version"))
			var current_ver := Statics.parse_version_to_array(ProjectSettings.get_setting("application/config/version"))
			var ver_compare := Statics.compare_versions(saved_ver, current_ver)
			if ver_compare == 1:
				version_panel = $"VersionWarnPanel"
				version_panel.add_header(tr(&"Woah there!!"), 2)
				version_panel.set_text(tr(&"Looks like your current save is from a newer version of the game! Are you sure you wanna continue playing this version? Some data might get erased!! I'd recommend backing it up before continuing!"), 1)
				version_panel.add_button(tr(&"Yeah, let me in!"), spawn_menu)
				version_panel.can_focus = true
				color_cover.set_new_fade(Color(0.0, 0.0, 0.0, 1.0), Color(0.0, 0.0, 0.0, 0.4), 0.5)
			else:
				$"VersionWarnPanel".queue_free()
				is_main_awaiting_input = true
			
			click_play_text = $"ClickPlay"
			click_play_text.set_snaily_text(tr(&"Click or press %s or %s to play!!") % [
				SInput.get_icon_as_bbcode(SInput.Inputs.UI_ACCEPT, "", 0), SInput.get_icon_as_bbcode(SInput.Inputs.UI_ACCEPT, "", 1)
			])
			click_play_text.add_border(1)
			click_play_text.add_shadow(2)
		else:
			title.position.y = TITLE_REST_Y
			$"ClickPlay".queue_free()
			$"VersionWarnPanel".queue_free()
			is_main_awaiting_input = false
			spawn_menu()

	else:
		title.position.y = TITLE_REST_Y
		selectors[0].action = "left_%d" % int(Statics.current_profile["character"])
		selectors[1].action = "right_%d" % int(Statics.current_profile["character"])
		create_layer("MainAlt")


func _process(delta: float) -> void:
	if click_play_text:
		if is_main_awaiting_input:
			input_delay_timer += delta
		click_play_text.set_visible_chars_ratio(input_delay_timer * 0.6)
		if input_delay_timer >= -1.5:
			if ((SInput.input_pressed(SInput.Inputs.UI_CLICK)
			or SInput.input_pressed(SInput.Inputs.UI_ACCEPT))
			and spawn_buffer_frames <= 0 and read_inputs):
				spawn_menu()
				is_main_awaiting_input = false
	if not is_main_awaiting_input:
		if is_main_menu:
			title.position.y = lerpf(title.position.y, TITLE_REST_Y, TITLE_MOVE_RATE * delta)
		if ((SInput.input_just_pressed(SInput.Inputs.PAUSE) or SInput.input_just_pressed(SInput.Inputs.UI_BACK))
		and spawn_buffer_frames <= 0 and read_esc):
			selector_y_offset = 0
			if active_layers > 1 or not is_main_menu:
				clear_top_layer(0)
			else:
				create_layer("Quit")

	var focused_node:Control = get_viewport().gui_get_focus_owner()
	if focused_node != null:
		if focused_node is ScrollingSnailyButton:
			focused_node = focused_node.scroller
		for i in selectors.size():
			var selector_pos:Vector2 = selectors[i].global_position
			var destination = focused_node.global_position
			if destination.x > -230: # I hate that I have to do this to fix the weird left jump on first load
				destination.y += (focused_node.size.y * 0.5) + SELECTOR_OFFSET.y
				destination.y -= active_layer.position.y
				destination.y += selector_y_offset
				match i:
					0: destination.x -= SELECTOR_OFFSET.x
					1: destination.x += focused_node.size.x + SELECTOR_OFFSET.x
				var new_pos = selector_pos.lerp(destination, SELECTOR_MOVE_RATE * delta)
				selectors[i].global_position = new_pos

		if SInput.just_pressed_as_echo("left") or SInput.just_pressed_as_echo("ui_left"):
			if focused_node.focus_neighbor_left != ^"":
				var left:Control = focused_node.get_node(focused_node.focus_neighbor_left)
				left.grab_focus()
		if SInput.just_pressed_as_echo("right") or SInput.just_pressed_as_echo("ui_right"):
			if focused_node.focus_neighbor_right != ^"":
				var right:Control = focused_node.get_node(focused_node.focus_neighbor_right)
				right.grab_focus()
		if SInput.just_pressed_as_echo("up") or SInput.just_pressed_as_echo("ui_up"):
			if focused_node.focus_neighbor_top != ^"":
				var up:Control = focused_node.get_node(focused_node.focus_neighbor_top)
				up.grab_focus()
		if SInput.just_pressed_as_echo("down") or SInput.just_pressed_as_echo("ui_down"):
			if focused_node.focus_neighbor_bottom != ^"":
				var down:Control = focused_node.get_node(focused_node.focus_neighbor_bottom)
				down.grab_focus()

	if spawn_buffer_frames > 0:
		spawn_buffer_frames -= 1

	if not is_main_awaiting_input:
		var rate_delta := HIDE_FADE_RATE * delta
		var asset_a = title.modulate.a
		var selector_a = selectors[0].modulate.a
		var selector_target_a = active_layer.selector_opacity
		if active_layer.hide_global_menu_assets:
			title.modulate.a = lerp(asset_a, 0.0, rate_delta)
			version_text.modulate.a = lerp(asset_a, 0.0, rate_delta)
		else:
			title.modulate.a = lerp(asset_a, 1.0, rate_delta)
			version_text.modulate.a = lerp(asset_a, 1.0, rate_delta)
		selectors[0].modulate.a = lerp(selector_a, selector_target_a, rate_delta)
		selectors[1].modulate.a = lerp(selector_a, selector_target_a, rate_delta)

	if read_inputs and not read_esc:
		read_esc = true


func spawn_menu() -> void:
	if is_main_menu:
		if click_play_text:
			click_play_text.queue_free()
		if version_panel:
			version_panel.queue_free()
			$"ColorCover".set_new_fade($"ColorCover".end_color, Color(0.0, 0.0, 0.0, 0.0), 0.25)
		if not Statics.main_menu_booted_once:
			Statics.save_general()
			Statics.current_profile = Statics.data_profile1
		Statics.main_menu_booted_once = true
		create_layer("Main")


func create_layer(_name:String) -> MenuLayer:
	active_layers += 1
	for this_layer in layer_group.get_children():
		this_layer.can_focus = false
	var layer_scene = load(LAYER_PATH % _name)
	var layer = layer_scene.instantiate()
	layer.menu = self
	layer.position = Vector2(0.0, 240.0)
	layer.layer_id = active_layers
	layer_group.add_child(layer)
	for this_layer in layer_group.get_children():
		this_layer.total_layer_count = active_layers
	for button in Statics.get_all_children(layer):
		if button is ActionSnailyButton or button is ContextSnailyButton:
			if button.back_one_layer:
				button.button_pressed.connect(clear_top_layer)
			elif button.quick_load_layer.strip_edges() != "":
				button.button_pressed.connect(create_layer)
	active_layer = layer
	return layer


func clear_top_layer(_value) -> MenuLayer:
	if active_layers > 1:
		active_layers -= 1
		var top_layer:MenuLayer = null
		var second_top_layer:MenuLayer
		for this_layer in layer_group.get_children():
			if this_layer.layer_id != -1:
				this_layer.total_layer_count -= 1
				second_top_layer = top_layer
				top_layer = this_layer
		top_layer.layer_id = -1
		top_layer.can_focus = false
		if top_layer.save_general_on_close:
			save_general()
		second_top_layer.can_focus = true
		second_top_layer.first_beep = true
		#region Find new focus
		var buttons = Statics.get_all_children(second_top_layer)
		var found_focus:bool = false
		var focus_button:SnailyButton = null
		for button in buttons:
			if not found_focus:
				if button is SnailyButton:
					if focus_button == null:
						focus_button = button
					if button.grab_focus_on_load:
						focus_button = button
						found_focus = true
		focus_button.grab_focus()
		#endregion
		active_layer = second_top_layer
		return second_top_layer
	elif active_layers == 1 and not is_main_menu:
		UICore.instance.pause_layer.unpause_fade_out()
		queue_free()
	return null


func connect_button_to_layer(button:SnailyButton) -> void:
	button.button_pressed.connect(create_layer)


func get_next_layer_up() -> MenuLayer:
	var layer_count = layer_group.get_child_count()
	if layer_count > 1:
		return layer_group.get_child(layer_count - 2)
	return null


func save_general() -> void:
	Statics.save_general()
	play_save_anim()


func save_profile(id:int) -> void:
	Statics.save_profile(id)
	play_save_anim()


func play_save_anim() -> void:
	save_icon.visible = true
	save_icon.action = "anim"
