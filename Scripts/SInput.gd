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

const STICK_DEADZONE_MOVE:float = 0.1
const STICK_DEADZONE_AIM:float = 0.2

const ICON_PATH:String = "res://Assets/Images/UI/ControlIcons/%s.png"

var last_input_was_con:bool = false
var last_ten_keys:Array = []
#endregion


func _ready() -> void:
	pass


func _input(event: InputEvent) -> void:
	if event is InputEventKey:
		last_input_was_con = false
		if event.pressed:
			last_ten_keys.append(event.keycode)
			if last_ten_keys.size() > 10:
				last_ten_keys.pop_front()
		#print(OS.get_keycode_string(event.physical_keycode))
	elif event is InputEventJoypadButton:
		last_input_was_con = true
		#print(event.button_index)
	elif event is InputEventJoypadMotion:
		last_input_was_con = true
		#print(event)


func pressed(action:String) -> bool:
	return Input.is_action_pressed(action)


func just_pressed(action:String) -> bool:
	return Input.is_action_just_pressed(action)


func check_input(action:Inputs, just:bool) -> bool:
	var this_action:String = get_input_str(action)
	if just:
		return just_pressed(this_action)
	return pressed(this_action)


func input_pressed(action:Inputs) -> bool:
	return check_input(action, false)


func input_just_pressed(action:Inputs) -> bool:
	return check_input(action, true)


func vector_move(raw:bool = false) -> Vector2:
	var vector:Vector2 = Input.get_vector("left", "right", "up", "down")
	vector *= 0.25
	if raw:
		return vector
	if vector.x < -STICK_DEADZONE_MOVE: vector.x = -1
	elif vector.x > STICK_DEADZONE_MOVE: vector.x = 1
	else: vector.x = 0
	if vector.y < -STICK_DEADZONE_MOVE: vector.y = -1
	elif vector.y > STICK_DEADZONE_MOVE: vector.y = 1
	else: vector.y = 0
	return vector


func vector_aim() -> Vector2:
	var vector:Vector2 = Input.get_vector("aimL", "aimR", "aimU", "aimD").normalized()
	if not ProjectSettings.get_setting("game/control/omni_stick_aim"):
		if vector.x < -STICK_DEADZONE_AIM: vector.x = -1
		elif vector.x > STICK_DEADZONE_AIM: vector.x = 1
		else: vector.x = 0
		if vector.y < -STICK_DEADZONE_AIM: vector.y = -1
		elif vector.y > STICK_DEADZONE_AIM: vector.y = 1
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


func rebind(action:Inputs, new_event:Variant, slot:int) -> void:
	var controls:Array = ProjectSettings.get_setting("game/control/controls")
	var old_action:Array = controls[action].duplicate()
	if slot < old_action.size() and slot >= 0:
		old_action[slot] = new_event
	controls[action] = old_action.duplicate()
	ProjectSettings.set_setting("game/control/controls", controls.duplicate())
