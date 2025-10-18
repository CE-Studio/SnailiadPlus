extends VBoxContainer


const SUPPRESS_FRAMES:int = 3
const BIND_TIME:float = 2.0

var panel_up:bool = false
var action_being_remapped:int = 0
var bind_buttons:Array[BindSnailyButton] = []
var suppress_input:int = 0
var queue_defocus:bool = true
var bind_time:float = 0.0
var rebind_buffer:InputEvent = null

@onready var layer:MenuLayer = get_parent()
@onready var panel:ContextPanel = $"../ContextPanel"


func _ready() -> void:
	if layer.meta_info.size() == 0:
		layer.meta_info.append(0)
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
		bind_time -= delta
		if bind_time <= 0.0:
			_defocus_panel()
	else:
		panel.modulate.a = lerpf(panel.modulate.a, 0.0, MenuLayer.MOVE_RATE * delta)
		panel.position.y = lerp(panel.position.y, 240.0, MenuLayer.MOVE_RATE * delta)
		layer.menu.selector_y_offset = 0
	
	if rebind_buffer != null and not Input.is_anything_pressed():
		_rebind_from_buffered()
		rebind_buffer = null
	
	if suppress_input > 0 and not Input.is_anything_pressed() and SInput.vector_move(true) == Vector2.ZERO:
		suppress_input -= 1
	layer.meta_info[1] = suppress_input
	if queue_defocus:
		_defocus_panel()
	


func _input(event: InputEvent) -> void:
	if not panel_up or event is InputEventMouse or (suppress_input > 0):
		return
	
	var bind_slot:int = layer.meta_info[0]
	if ((bind_slot < 2 and event is InputEventKey)
	or (bind_slot >= 2 and (event is InputEventJoypadButton or event is InputEventJoypadMotion))):
		rebind_buffer = event
		suppress_input = SUPPRESS_FRAMES
		queue_defocus = true


func _rebind_from_buffered() -> void:
	var bind_slot:int = layer.meta_info[0]
	#var controls:Array = ProjectSettings.get_setting("game/control/controls")
	#if rebind_buffer is InputEventKey:
	#	controls[action_being_remapped][bind_slot] = rebind_buffer.keycode
	#elif rebind_buffer is InputEventJoypadMotion:
	#	controls[action_being_remapped][bind_slot] = Vector2(
	#		rebind_buffer.axis, -1 if rebind_buffer.axis_value < 0 else 1
	#	)
	#elif rebind_buffer is InputEventJoypadButton:
	#	controls[action_being_remapped][bind_slot] = rebind_buffer.button_index
	SInput.rebind_ctrl(action_being_remapped, rebind_buffer, bind_slot)
	SInput.rebind_action.call_deferred(SInput.get_input_str(action_being_remapped))
	for button in bind_buttons:
		if button.bind == action_being_remapped:
			button.setup_bind_icons.call_deferred()


func _on_button_pressed(bind:int) -> void:
	action_being_remapped = bind
	_focus_panel(bind)
	bind_time = BIND_TIME


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
	if layer.meta_info.size() >= 3:
		layer.meta_info[2].grab_focus()
		layer.meta_info.remove_at(2)
