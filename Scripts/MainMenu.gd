class_name MainMenu
extends Node2D

#region Variables
const TITLE_REST_Y = 40
const TITLE_MOVE_RATE = 8
const LAYER_PATH = "res://Scenes/UI/MenuLayers/%s.tscn"
const SELECTOR_MOVE_RATE = 20
const SELECTOR_OFFSET = Vector2i(16, -2)

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

@onready var title:Node2D = $"Title"
@onready var layer_group:Node2D = $"LayerGroup"
@onready var selectors:Array = [ $"LeftSelector", $"RightSelector" ]
@onready var save_icon:JsonSprite2D = $"SaveIcon"
#endregion


func _ready() -> void:
	save_icon.visible = false
			
	var version_text = $"Version"
	var version_string = (Statics.get_text("menu_version_header") + "\n"
	+ Statics.parse_version_to_text_string(ProjectSettings.get_setting("application/config/version")))
	version_text.set_snaily_text(version_string)
	version_text.add_shadow(1)
	
	if is_main_menu:
		selectors[0].action = "left_0"
		selectors[1].action = "right_0"
		if not Statics.main_menu_booted_once:
			var saved_ver = Statics.parse_version_to_array(Statics.data_general["game_version"])
			var current_ver = Statics.parse_version_to_array(ProjectSettings.get_setting("application/config/version"))
			var ver_compare = Statics.compare_versions(saved_ver, current_ver)
			if ver_compare == 1:
				version_panel = $"VersionWarnPanel"
				version_panel.add_header(Statics.get_text("menu_olderVersion_header"), 2)
				version_panel.set_text(Statics.get_text("menu_olderVersion_body"), 1)
				version_panel.add_button(Statics.get_text("menu_olderVersion_confirm"), spawn_menu)
				version_panel.can_focus = true
				$"ColorCover".set_new_fade(Color(0.0, 0.0, 0.0, 1.0), Color(0.0, 0.0, 0.0, 0.4), 0.5)
			else:
				$"VersionWarnPanel".queue_free()
				is_main_awaiting_input = true
			
			click_play_text = $"ClickPlay"
			click_play_text.set_snaily_text(Statics.get_text("test"))
			click_play_text.add_border(1)
			click_play_text.add_shadow(2)
		else:
			title.position.y = TITLE_REST_Y
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
			if (Input.get_action_raw_strength("UIClick")
			or Input.get_action_raw_strength("Jump")) and spawn_buffer_frames <= 0:
				spawn_menu()
				is_main_awaiting_input = false
	if not is_main_awaiting_input:
		if is_main_menu:
			title.position.y = lerpf(title.position.y, TITLE_REST_Y, TITLE_MOVE_RATE * delta)
		if Input.is_action_just_pressed("Pause") and spawn_buffer_frames <= 0:
			selector_y_offset = 0
			if active_layers > 1 or not is_main_menu:
				clear_top_layer(0)
			else:
				create_layer("Quit")
	
	var focused_node = get_viewport().gui_get_focus_owner()
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
	
	if spawn_buffer_frames > 0:
		spawn_buffer_frames -= 1


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
		create_layer("Main")


func create_layer(_name:String) -> MenuLayer:
	active_layers += 1
	for this_layer in layer_group.get_children():
		this_layer.can_focus = false
	var layer_scene = load(LAYER_PATH % _name)
	var layer = layer_scene.instantiate()
	layer.menu = self
	layer_group.add_child(layer)
	layer.position = Vector2(0.0, 240.0)
	layer.layer_id = active_layers
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
