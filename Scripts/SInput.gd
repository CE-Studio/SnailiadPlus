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
}
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


func input_pressed(action:Inputs) -> bool:
	var this_action:String = Inputs.keys()[action]
	this_action = this_action.to_camel_case()
	var current_checks:Array[bool] = [true, true, true, true]
	match action:
		_:
			pass
	return false
	#temp
