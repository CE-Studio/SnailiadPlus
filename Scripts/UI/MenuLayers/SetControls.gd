extends VBoxContainer


const SUPPRESS_FRAMES:int = 3

var panel_up:bool = false
var action_being_remapped:int = 0
var bind_buttons:Array[BindSnailyButton] = []
var suppress_input:int = 0
var queue_defocus:bool = true

@onready var layer:MenuLayer = get_parent()
@onready var panel:ContextPanel = $"../ContextPanel"


func _ready() -> void:
	if layer.meta_info.size() == 0:
		layer.meta_info.append(0)
	
	panel.call_deferred("add_header",
		"menu_option_controls_remap_header", 2)
	
	for child in Statics.get_all_children(self):
		if child is BindSnailyButton:
			child.connect("pressed", _on_button_pressed)
			bind_buttons.append(child)


func _process(delta: float) -> void:
	if panel_up:
		panel.modulate.a = lerpf(panel.modulate.a, 1.0, MenuLayer.MOVE_RATE * delta)
		panel.position.y = lerp(panel.position.y, 80.0, MenuLayer.MOVE_RATE * delta)
		layer.menu.selector_y_offset = 80.0 - panel.position.y
	else:
		panel.modulate.a = lerpf(panel.modulate.a, 0.0, MenuLayer.MOVE_RATE * delta)
		panel.position.y = lerp(panel.position.y, 240.0, MenuLayer.MOVE_RATE * delta)
		layer.menu.selector_y_offset = 0
	
	if suppress_input > 0 and not Input.is_anything_pressed() and SInput.vector_move(true) == Vector2.ZERO:
		suppress_input -= 1
		if queue_defocus and suppress_input == 0:
			_defocus_panel()
	


func _input(event: InputEvent) -> void:
	if not panel_up or event is InputEventMouse or (suppress_input > 0):
		return
	
	if event is InputEventKey and event.keycode == KEY_ESCAPE:
		_defocus_panel()
		return
	var controls:Array = ProjectSettings.get_setting("game/control/controls")
	var bind_slot:int = layer.meta_info[0]
	if ((bind_slot < 2 and event is InputEventKey)
	or (bind_slot >= 2 and (event is InputEventJoypadButton or event is InputEventJoypadMotion))):
		if event is InputEventKey:
			controls[action_being_remapped][bind_slot] = event.keycode
		elif event is InputEventJoypadMotion:
			controls[action_being_remapped][bind_slot] = Vector2(
				event.axis, -1 if event.axis_value < 0 else 1
			)
		elif event is InputEventJoypadButton:
			controls[action_being_remapped][bind_slot] = event.button_index
		rebind_action(SInput.get_input_str(action_being_remapped))
		for button in bind_buttons:
			if button.bind == action_being_remapped:
				button.setup_bind_icons()
		suppress_input = SUPPRESS_FRAMES
		queue_defocus = true


func rebind_action(action:String) -> void:
	var controls:Array = ProjectSettings.get_setting("game/control/controls")
	var actions_raw:Array = SInput.Inputs.keys()
	var actions:Array = []
	for act in actions_raw:
		actions.append(act.to_camel_case())
	var action_id:int = actions.find(action)
	
	InputMap.action_erase_events(action)
	for i in range(4):
		var new_event:InputEvent = null
		if i < 2:
			new_event = InputEventKey.new()
			new_event.keycode = controls[action_id][i]
		elif controls[action_id][i] is Vector2 or controls[action_id][i] is Vector2i:
			new_event = InputEventJoypadMotion.new()
			new_event.axis = controls[action_id][i].x
			new_event.axis_value = controls[action_id][i].y
		else:
			new_event = InputEventJoypadButton.new()
			new_event.button_index = controls[action_id][i]
		InputMap.action_add_event(action, new_event)


func rebind_all() -> void:
	var actions:Array = InputMap.get_actions()
	for action in actions:
		rebind_action(action)


func _on_button_pressed(bind:int) -> void:
	action_being_remapped = bind
	_focus_panel(bind)


func _focus_panel(bind:int) -> void:
	layer.can_focus = false
	layer.menu.read_inputs = false
	panel_up = true
	suppress_input = SUPPRESS_FRAMES
	queue_defocus = false
	var base_str:String = ""
	match layer.meta_info[0]:
		0: base_str = Statics.get_text("menu_option_controls_remap_priKey")
		1: base_str = Statics.get_text("menu_option_controls_remap_secKey")
		2: base_str = Statics.get_text("menu_option_controls_remap_priCon")
		3: base_str = Statics.get_text("menu_option_controls_remap_secCon")
	var bind_str = Statics.get_text("menu_option_controls_" + SInput.get_input_str(bind))
	panel.set_text(base_str % bind_str, 1)
	panel.can_focus = true
	layer.menu.selector_y_offset = 80.0 - panel.position.y


func _defocus_panel() -> void:
	panel_up = false
	panel.can_focus = false
	layer.can_focus = true
	layer.menu.set_deferred("read_inputs", true)
	if layer.meta_info.size() >= 2:
		layer.meta_info[1].grab_focus()
		layer.meta_info.remove_at(1)
