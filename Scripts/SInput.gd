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

const DEFAULTS:Dictionary = {
	"leftK1": KEY_LEFT,
	"leftK2": KEY_A,
}

const ICONS_KEY:Dictionary = {
	KEY_0: "0",
	KEY_1: "1",
	KEY_2: "2",
	KEY_3: "3",
	KEY_4: "4",
	KEY_5: "5",
	KEY_6: "6",
	KEY_7: "7",
	KEY_8: "8",
	KEY_9: "9",
	KEY_A: "A",
	KEY_ALT: "Alt",
	KEY_B: "B",
	KEY_QUOTELEFT: "Backquote",
	KEY_BACKSLASH: "Backslash",
	KEY_BACKSPACE: "Backspace",
	KEY_C: "C",
	KEY_BRACKETRIGHT: "CloseBracket",
	KEY_COLON: "Colon",
	KEY_COMMA: "Comma",
	KEY_CTRL: "Ctrl",
	KEY_D: "D",
	KEY_DELETE: "Del",
	KEY_QUOTEDBL: "DoubleQuote",
	KEY_DOWN: "Down",
	KEY_E: "E",
	KEY_ENTER: "Enter",
	KEY_EQUAL: "Equal",
	KEY_ESCAPE: "Esc",
	KEY_F: "F",
	KEY_F1: "F1",
	KEY_F2: "F2",
	KEY_F3: "F3",
	KEY_F4: "F4",
	KEY_F5: "F5",
	KEY_F6: "F6",
	KEY_F7: "F7",
	KEY_F8: "F8",
	KEY_F9: "F9",
	KEY_F10: "F10",
	KEY_F11: "F11",
	KEY_F12: "F12",
	KEY_G: "G",
	KEY_H: "H",
	KEY_I: "I",
	KEY_J: "J",
	KEY_K: "K",
	KEY_L: "L",
	KEY_LEFT: "Left",
	KEY_META: "Logo",
	KEY_M: "M",
	KEY_MINUS: "Minus",
	KEY_N: "N",
	KEY_KP_0: "Num0",
	KEY_KP_1: "Num1",
	KEY_KP_2: "Num2",
	KEY_KP_3: "Num3",
	KEY_KP_4: "Num4",
	KEY_KP_5: "Num5",
	KEY_KP_6: "Num6",
	KEY_KP_7: "Num7",
	KEY_KP_8: "Num8",
	KEY_KP_9: "Num9",
	KEY_KP_DIVIDE: "NumDivide",
	KEY_KP_ENTER: "NumEnter",
	KEY_KP_SUBTRACT: "NumMinus",
	KEY_KP_MULTIPLY: "NumMultiply",
	KEY_KP_ADD: "NumPlus",
	KEY_O: "O",
	KEY_BRACKETLEFT: "OpenBracket",
	KEY_P: "P",
	KEY_PERIOD: "Period",
	KEY_Q: "Q",
	KEY_R: "R",
	KEY_RIGHT: "Right",
	KEY_S: "S",
	KEY_SEMICOLON: "Semicolon",
	KEY_SHIFT: "Shift",
	KEY_SLASH: "Slash",
	KEY_SPACE: "Space",
	KEY_T: "T",
	KEY_TAB: "Tab",
	KEY_U: "U",
	KEY_UP: "Up",
	KEY_V: "V",
	KEY_W: "W",
	KEY_X: "X",
	KEY_Y: "Y",
	KEY_Z: "Z",
}

const STICK_DEADZONE_MOVE:float = 0.1
const STICK_DEADZONE_AIM:float = 0.2
#endregion


func _ready() -> void:
	pass


func _input(event: InputEvent) -> void:
	pass


func pressed(action:String) -> bool:
	return Input.is_action_pressed(action)


func just_pressed(action:String) -> bool:
	return Input.is_action_just_pressed(action)


func batch_pressed(action:String, pri_key:bool, sec_key:bool, pri_con:bool, sec_con:bool) -> bool:
	return (
		(pri_key and pressed(action + "K1")) or
		(sec_key and pressed(action + "K2")) or
		(pri_con and pressed(action + "C1")) or
		(sec_con and pressed(action + "C2"))
	)


func batch_just_pressed(action:String, pri_key:bool, sec_key:bool, pri_con:bool, sec_con:bool) -> bool:
	return (
		(pri_key and just_pressed(action + "K1")) or
		(sec_key and just_pressed(action + "K2")) or
		(pri_con and just_pressed(action + "C1")) or
		(sec_con and just_pressed(action + "C2"))
	)


func check_input(action:Inputs, just:bool) -> bool:
	#print(Inputs.keys()[action])
	var this_action:String = Inputs.keys()[action]
	this_action = this_action.to_camel_case()
	#match action:
	#	(Inputs.WEAPON0 or Inputs.WEAPON1 or Inputs.WEAPON2 or Inputs.WEAPON3
	#	or Inputs.PAUSE or Inputs.MAP):
	#		if just:
	#			return batch_just_pressed(this_action, true, false, true, false)
	#		return batch_pressed(this_action, true, false, true, false)
	#	Inputs.DEBUG or Inputs.UI_CLICK:
	#		if just:
	#			return just_pressed(this_action)
	#		return pressed(this_action)
	#	_:
	#		if just:
	#			return batch_just_pressed(this_action, true, true, true, true)
	#		return batch_pressed(this_action, true, true, true, true)
	
	if [ Inputs.WEAPON0, Inputs.WEAPON1, Inputs.WEAPON2, Inputs.WEAPON3,
	Inputs.PAUSE, Inputs.MAP, Inputs.UI_ACCEPT, Inputs.UI_BACK ].has(action):
		if just: return batch_just_pressed(this_action, true, false, true, false)
		return batch_pressed(this_action, true, false, true, false)
		
	elif [ Inputs.DEBUG, Inputs.UI_CLICK ].has(action):
		if just: return just_pressed(this_action)
		return pressed(this_action)
		
	else:
		if just: return batch_just_pressed(this_action, true, true, true, true)
		return batch_pressed(this_action, true, true, true, true)


func input_pressed(action:Inputs) -> bool:
	return check_input(action, false)


func input_just_pressed(action:Inputs) -> bool:
	return check_input(action, true)


func vector_move(raw:bool = false) -> Vector2:
	var vector:Vector2 = (Input.get_vector("leftK1", "rightK1", "upK1", "downK1")
	+ Input.get_vector("leftK2", "rightK2", "upK2", "downK2")
	+ Input.get_vector("leftC1", "rightC1", "upC1", "downC1")
	+ Input.get_vector("leftC2", "rightC2", "upC2", "downC2"))
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
	if Statics.data_general["stick_aim_mode"] == 0:
		if vector.x < -STICK_DEADZONE_AIM: vector.x = -1
		elif vector.x > STICK_DEADZONE_AIM: vector.x = 1
		else: vector.x = 0
		if vector.y < -STICK_DEADZONE_AIM: vector.y = -1
		elif vector.y > STICK_DEADZONE_AIM: vector.y = 1
		else: vector.y = 0
	return vector


func get_input_icon(event:InputEvent) -> String:
	if event is InputEventKey:
		var key = OS.get_keycode_string(event.physical_keycode)
		print(key)
		if ICONS_KEY.has(key):
			return ICONS_KEY[key]
		else:
			return "Unknown"
	return "Unknown"
