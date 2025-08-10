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

const STICK_DEADZONE_MOVE:float = 0.1
const STICK_DEADZONE_AIM:float = 0.2
#endregion


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
