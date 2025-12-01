extends Node


#region Variables
enum Inputs {
	LEFT,
	RIGHT,
	UP,
	DOWN,
	JUMP,
	SHOOT,
	STRAFE,
	SPEAK,
	GRAVITY,
	PAUSE,
	MAP,
	WEAPON0,
	WEAPON1,
	WEAPON2,
	WEAPON3,
	AIM_L,
	AIM_R,
	AIM_U,
	AIM_D,
	DEBUG,
	UI_CLICK,
	UI_ACCEPT,
	UI_BACK,
}

#region Static icon variables
var icon_left:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.LEFT)

var icon_right:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.RIGHT)

var icon_up:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.UP)

var icon_down:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.DOWN)

var icon_jump:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.JUMP)

var icon_shoot:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.SHOOT)

var icon_strafe:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.STRAFE)

var icon_speak:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.SPEAK)

var icon_gravity:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.GRAVITY)

var icon_pause:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.PAUSE)

var icon_map:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.MAP)

var icon_weapon0:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.WEAPON0)

var icon_weapon1:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.WEAPON1)

var icon_weapon2:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.WEAPON2)

var icon_weapon3:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.WEAPON3)

var icon_aiml:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.AIM_L)

var icon_aimr:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.AIM_R)

var icon_aimu:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.AIM_U)

var icon_aimd:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.AIM_D)

var icon_debug:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.DEBUG)

var icon_uiclick:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.UI_CLICK)

var icon_uiaccept:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.UI_ACCEPT)

var icon_uiback:String:
	set(_v): pass
	get(): return get_icon_as_bbcode(Inputs.UI_BACK)
#endregion

const INPUT_SLOTS:Array = [
	0b00001111, # Left
	0b00001111, # Right
	0b00001111, # Up
	0b00001111, # Down
	0b00001111, # Jump
	0b00001111, # Shoot
	0b00001111, # Strafe
	0b00001111, # Speak
	0b00001111, # Gravity
	0b00001010, # Pause
	0b00001010, # Map
	0b00001010, # Weapon 0
	0b00001010, # Weapon 1
	0b00001010, # Weapon 2
	0b00001010, # Weapon 3
	0b00000010, # Aim left
	0b00000010, # Aim right
	0b00000010, # Aim up
	0b00000010, # Aim down
	0b00001000, # Debug
	0b00001000, # UI click
	0b00001111, # UI accept
	0b00001111, # UI back
]

const CON_MAPPINGS:Array[Array] = [
	[ "FaceA", "FaceB", "FaceX", "FaceY", "XboxSel", "", "XboxStart" ],
	[ "FaceB", "FaceA", "FaceY", "FaceX", "NinSel", "", "NinStart" ],
	[ "PSX", "Circle", "Square", "Triangle", "PSSel", "", "PSStart" ],
	[ "FaceO", "FaceA", "FaceU", "FaceY", "Sel", "", "Start" ],
	[ "L3", "R3", "L1", "R1", "DpadUp", "DpadDown", "DpadLeft", "DpadRight" ]
]
const CON_TYPE_GENERAL:int = 7
const CON_TYPE_GENERAL_ARRAY:int = 4

const DEFAULTS:Array[Array] = [
	[ KEY_LEFT, KEY_A, Vector2i(0, -1), Vector2i(0, -1) ],
	[ KEY_RIGHT, KEY_D, Vector2i(0, 1), Vector2i(0, 1) ],
	[ KEY_UP, KEY_W, Vector2i(1, -1), Vector2i(1, -1) ],
	[ KEY_DOWN, KEY_S, Vector2i(1, 1), Vector2i(1, 1) ],
	[ KEY_Z, KEY_K, JOY_BUTTON_A, JOY_BUTTON_RIGHT_SHOULDER ],
	[ KEY_X, KEY_J, JOY_BUTTON_X, JOY_BUTTON_X ],
	[ KEY_C, KEY_H, JOY_BUTTON_B, JOY_BUTTON_B ],
	[ KEY_V, KEY_G, JOY_BUTTON_Y, JOY_BUTTON_Y ],
	[ KEY_SPACE, KEY_SPACE, JOY_BUTTON_LEFT_SHOULDER, JOY_BUTTON_LEFT_SHOULDER ],
	[ KEY_ESCAPE, KEY_ESCAPE, JOY_BUTTON_START, JOY_BUTTON_START ],
	[ KEY_M, KEY_M, JOY_BUTTON_BACK, JOY_BUTTON_BACK ],
	[ KEY_0, KEY_0, JOY_BUTTON_DPAD_LEFT, JOY_BUTTON_DPAD_LEFT ],
	[ KEY_1, KEY_1, JOY_BUTTON_DPAD_DOWN, JOY_BUTTON_DPAD_DOWN ],
	[ KEY_2, KEY_2, JOY_BUTTON_DPAD_UP, JOY_BUTTON_DPAD_UP ],
	[ KEY_3, KEY_3, JOY_BUTTON_DPAD_RIGHT, JOY_BUTTON_DPAD_RIGHT ],
	[ KEY_NONE, KEY_NONE, Vector2i(2, -1), Vector2i(2, -1) ],
	[ KEY_NONE, KEY_NONE, Vector2i(2, 1), Vector2i(2, 1) ],
	[ KEY_NONE, KEY_NONE, Vector2i(3, -1), Vector2i(3, -1) ],
	[ KEY_NONE, KEY_NONE, Vector2i(3, 1), Vector2i(3, 1) ],
	[ KEY_QUOTELEFT, KEY_QUOTELEFT, JOY_BUTTON_INVALID, JOY_BUTTON_INVALID ],
	[ KEY_NONE, KEY_NONE, JOY_BUTTON_INVALID, JOY_BUTTON_INVALID ],
	[ KEY_Z, KEY_ENTER, JOY_BUTTON_A, JOY_BUTTON_A ],
	[ KEY_X, KEY_ESCAPE, JOY_BUTTON_B, JOY_BUTTON_B ],
]

const ICON_PATH:String = "res://Assets/Images/UI/ControlIcons/%s.png"

const ECHO_DELAY_INITIAL:float = 0.6
const ECHO_DELAY_REPEAT:float = 0.04
const DEBUG_PRINT_INPUTS:bool = false

var read_inputs:bool = true
var cutscene_has_control:bool = false
var last_input_was_con:bool = false
var last_ten_keys:Array = []
var ui_echo_delay:float = ECHO_DELAY_INITIAL
var send_con_as_echo:bool = false
var inputs_down:int = 0
#endregion


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS


func _process(delta: float) -> void:
	var any_down:bool = Input.is_anything_pressed()
	send_con_as_echo = false
	if last_input_was_con and any_down:
		ui_echo_delay -= delta
		if ui_echo_delay <= 0.0:
			ui_echo_delay += ECHO_DELAY_REPEAT
			send_con_as_echo = true
	else:
		ui_echo_delay = ECHO_DELAY_INITIAL


func _input(event: InputEvent) -> void:
	if event is InputEventKey:
		if DEBUG_PRINT_INPUTS:
			(OS.get_keycode_string(event.physical_keycode))

		last_input_was_con = false
		if event.pressed:
			last_ten_keys.append(event.keycode)
			if last_ten_keys.size() > 10:
				last_ten_keys.pop_front()
	elif event is InputEventJoypadButton:
		if DEBUG_PRINT_INPUTS:
			print(event.button_index)
		last_input_was_con = true
	elif event is InputEventJoypadMotion:
		if DEBUG_PRINT_INPUTS:
			print(event)
		last_input_was_con = true


func pressed(action:String) -> bool:
	if not accepting_input():
		return false
	return Input.is_action_pressed(action)


func just_pressed(action:String, accept_con_echo:bool = false) -> bool:
	if not accepting_input():
		return false
	if accept_con_echo:
		return (Input.is_action_just_pressed(action) or
		Input.is_action_pressed(action) and send_con_as_echo)
	return Input.is_action_just_pressed(action)


func just_pressed_as_echo(action:String) -> bool:
	if just_pressed(action) or not accepting_input():
		return false
	return just_pressed(action, true)


func check_input(action:Inputs, just:bool, accept_con_echo:bool = false) -> bool:
	if not accepting_input():
		return false
	var this_action:String = get_input_str(action)
	if just:
		return just_pressed(this_action, accept_con_echo)
	return pressed(this_action)


func check_input_as_echo(action:Inputs) -> bool:
	var this_action:String = get_input_str(action)
	if just_pressed(this_action) or not accepting_input():
		return false
	return just_pressed(this_action, true)


func input_pressed(action:Inputs) -> bool:
	if not accepting_input():
		return false
	return check_input(action, false)


func input_just_pressed(action:Inputs) -> bool:
	if not accepting_input():
		return false
	return check_input(action, true)


func vector_move(raw:bool = false) -> Vector2:
	if not accepting_input():
		return Vector2.ZERO
	var deadzone:float = ProjectSettings.get_setting("game/control/deadzone_move")
	var vector:Vector2 = Input.get_vector("left", "right", "up", "down", deadzone)
	if raw:
		return vector
	if vector.x < -deadzone: vector.x = -1
	elif vector.x > deadzone: vector.x = 1
	else: vector.x = 0
	if vector.y < -deadzone: vector.y = -1
	elif vector.y > deadzone: vector.y = 1
	else: vector.y = 0
	return vector


func vector_aim() -> Vector2:
	if not accepting_input():
		return Vector2.ZERO
	var deadzone:float = ProjectSettings.get_setting("game/control/deadzone_aim")
	var vector:Vector2 = Input.get_vector("aimL", "aimR", "aimU", "aimD", deadzone).normalized()
	if not ProjectSettings.get_setting("game/control/omni_stick_aim"):
		if vector.x < -deadzone: vector.x = -1
		elif vector.x > deadzone: vector.x = 1
		else: vector.x = 0
		if vector.y < -deadzone: vector.y = -1
		elif vector.y > deadzone: vector.y = 1
		else: vector.y = 0
	return vector


func get_icon_from_enum_str(string:String) -> String:
	string = string.to_upper()
	var keys:Array = Inputs.keys()
	assert(keys.has(string), "'%s' is not a valid Inputs value!" % string)
	var index:int = keys.find(string)
	return get_icon_from_enum(index as Inputs)


func get_icon_from_enum(input:Inputs) -> String:
	var action = pull_action(input)
	if last_input_was_con:
		if action[2] is Vector2i:
			return get_axis_icon(action[2])
		return get_button_icon(action[2])
	return get_key_icon(action[0])


func get_icon_as_bbcode(input:Inputs) -> String:
	return "[img=top,top]" + (ICON_PATH % get_icon_from_enum(input)) + "[/img]"


func get_input_icon(event:InputEvent) -> String:
	if event is InputEventKey:
		return get_key_icon(event.physical_keycode)
	if event is InputEventJoypadMotion:
		return get_axis_icon(Vector2i(
			event.axis,
			-1 if event.axis_value < 0 else 1
		))
	if event is InputEventJoypadButton:
		return get_button_icon(event.button_index)
	return "Unknown"


func get_input_str(input:Inputs) -> StringName:
	var string:String = Inputs.keys()[input]
	return string.to_camel_case()


func _check_icon_exists(key:String) -> String:
	if ResourceLoader.exists(ICON_PATH % key):
		return key
	return "Unknown"


func get_key_icon(key:int) -> String:
	return _check_icon_exists(OS.get_keycode_string(key))


func get_button_icon(button:int) -> String:
	var key:String = ""
	if button < CON_TYPE_GENERAL:
		var con_type = ProjectSettings.get_setting("game/control/controller_type")
		key = CON_MAPPINGS[con_type][button]
	else:
		key = CON_MAPPINGS[CON_TYPE_GENERAL_ARRAY][button - CON_TYPE_GENERAL]
	return _check_icon_exists(key)


func get_axis_icon(axis:Vector2i) -> String:
	var key:String = ""
	match axis.x:
		0: key = "LLeft" if axis.y < 0 else "LRight"
		1: key = "LUp" if axis.y < 0 else "LDown"
		2: key = "RLeft" if axis.y < 0 else "RRight"
		3: key = "RUp" if axis.y < 0 else "RDown"
		4: key = "L2"
		5: key = "R2"
		_: key = "Unknown"
	return _check_icon_exists(key)


func pull_action(input:Inputs) -> Array:
	var action:Array = ProjectSettings.get_setting("game/control/controls")[input].duplicate()
	for i in range(action.size()):
		var event = action[i]
		if event is String and event.contains("("):
			action[i] = str_to_var("Vector2i" + event)
	return action


func rebind_ctrl(action:Inputs, new_event:InputEvent, slot:int) -> void:
	var controls:Array = ProjectSettings.get_setting("game/control/controls")
	var old_action:Array = controls[action].duplicate()
	var slot_state:int = INPUT_SLOTS[action as int]

	var event_data
	if new_event is InputEventKey:
		event_data = new_event.keycode
	elif new_event is InputEventJoypadButton:
		event_data = new_event.button_index
	elif new_event is InputEventJoypadMotion:
		event_data = Vector2(new_event.axis, -1 if new_event.axis_value < 0 else 1)

	old_action[slot] = event_data
	# Bitwise op: check slot to the right of current slot by indexing slot_state where slot = 0b----0123
	var state_index:int = 2 - slot # slot (range 0-3) is flipped (3 - slot), then subtract 1 to bump index right
	if ((slot == 0 or slot == 2) and ((1 << state_index) & slot_state) == 0):
		old_action[slot + 1] = event_data

	controls[action] = old_action.duplicate()
	ProjectSettings.set_setting("game/control/controls", controls.duplicate())


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
	var keys:Array = Inputs.keys()
	for i in range(keys.size() - 1):
		keys[i] = keys[i].to_camel_case()

	var actions:Array = InputMap.get_actions()
	for action in actions:
		if keys.has(action) and action != "uiClick":
			rebind_action(action)


func load_from_project_settings() -> void:
	pass


func accepting_input() -> bool:
	return read_inputs and not cutscene_has_control
