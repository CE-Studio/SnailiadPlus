class_name Statics
extends RefCounted


#region Variables
#region Directional enums
enum DirsCompass {
	N,
	NE,
	E,
	SE,
	S,
	SW,
	W,
	NW,
	NONE = -1,
}
enum DirsCardinal {
	UP,
	DOWN,
	LEFT,
	RIGHT,
	NONE = -1,
}
enum DirsSurface {
	FLOOR,
	LWALL,
	RWALL,
	CEILING,
	NONE = -1,
}
#endregion


enum Items {
	PEASHOOTER,
	BOOMERANG,
	RAINBOWWAVE,
	DEVASTATOR,
	HIGHJUMP,
	SHELLSHIELD,
	RAPIDFIRE,
	ICESHELL,
	FLYSHELL,
	METALSHELL,
	GRAVSHOCK,
	SSBOOM,
	DEBUGRW,
	HEART,
	FRAGMENT,
	RADARSHELL,
	NONE = -1,
}


const PI_OVER_EIGHT:float = PI * 0.125
const PI_OVER_FOUR:float = PI * 0.25
const PI_OVER_THREE:float = PI * 0.3333
const PI_OVER_TWO:float = PI * 0.5
const THREE_PI_OVER_TWO:float = TAU - PI_OVER_TWO
const FRAC_8:float = 0.125
const FRAC_16:float = 0.0625
const FRAC_32:float = 0.03125
const FRAC_64:float = 0.015625
const FRAC_128:float = 0.0078125
const VECTOR_DIAG:Vector2 = Vector2(cos(deg_to_rad(40)), sin(deg_to_rad(40)))


const HEALTH_ORB_VALUES = [ 1, 2, 4 ]
const HEALTH_ORB_MULTS = [ 1.25, 0.6, 0.125 ]


static var is_menu_open:bool = false
static var is_in_boss_rush:bool = false
static var increment_boss_rush_timer:bool = false
static var is_random_game:bool = false


static var noclip_mode:bool = false
static var damage_mult:bool = false
static var show_entity_layer:bool = true
static var stack_shells:bool = true
static var stack_weapons:bool = false
static var stack_weapon_mods:bool = true

# Block of vars from musicParent to healthOrbPointer

static var palette := preload("res://Assets/Images/Palette.png")
static var missing := preload("res://Assets/Images/Missing.png")


static var disconnected_sound := preload("res://Scenes/internals/DisconnectedSound.tscn")


static var current_area:int = 0
static var current_subarea:int = 0


static var player:Player
static var cam_layer:Node2D
static var cam:Camera2D
static var active_room:Node2D
#endregion


#region Save info
static var profile:String
static var data_general:Dictionary
static var data_profile1:Dictionary
static var data_profile2:Dictionary
static var data_profile3:Dictionary
static var data_records:Dictionary
static var save_prefix:String = "snailyplus_saves"
static var current_profile:Dictionary
#endregion


#region Save functions
static func add_item(id:int, count:int) -> void:
	while id > len(current_profile["items"]):
		current_profile["items"].append(0)
	current_profile["items"][id] += count


static func remove_item(id:int, count:int) -> void:
	if id < len(current_profile["items"]):
		current_profile["items"][id] -= count


static func check_item(id:int) -> int:
	var output = 0
	if id < len(current_profile["items"]):
		output = current_profile["items"][id]
	return output


static func save_general():
	var file = FileAccess.open("user://" + save_prefix + "/GeneralData.json", FileAccess.WRITE_READ)
	file.store_string(JSON.stringify(data_general, "\t", false))


static func save_profile(iprofile:int):
	if iprofile < 1 or iprofile > 3:
		return
	var file = FileAccess.open("user://" + save_prefix + "/Profile" + str(iprofile) + ".json", FileAccess.WRITE_READ)
	match iprofile:
		1:
			file.store_string(JSON.stringify(data_profile1, "\t", false))
		2:
			file.store_string(JSON.stringify(data_profile2, "\t", false))
		3:
			file.store_string(JSON.stringify(data_profile3, "\t", false))


static func save_records():
	var file = FileAccess.open("user://" + save_prefix + "/Records.json", FileAccess.WRITE_READ)
	file.store_string(JSON.stringify(data_records, "\t", false))


static func save_all():
	save_general()
	save_profile(1)
	save_profile(2)
	save_profile(3)
	save_records()
#endregion


static func get_shell_level() -> int:
	if check_item(Item.ItemTypes.METAL_SHELL):
		return 3
	elif check_item(Item.ItemTypes.GRAVITY_SHELL):
		return 2
	elif check_item(Item.ItemTypes.ICE_SHELL):
		return 1
	else:
		return 0


static func is_number(value:Variant, consider_strings := false) -> bool:
	if value is int:
		return true
	if value is float:
		return true
	if consider_strings:
		if value is String:
			if value.is_valid_float():
				return true
			if value.is_valid_int():
				return true
			if value.is_valid_hex_number():
				return true
			if value.is_valid_hex_number(true):
				return true
	return false


static func integrate(num:float, target:float, speed:float, elapsed:float, threshold:float = 0.1) -> float:
	var scale = pow(0.1, speed)
	num = num * pow(scale, elapsed) + target * (1.0 - pow(scale, elapsed))
	if absf(num - target) < threshold:
		num = target
	return num


static func play_sfx_disconnected(sound:AudioStream) -> void:
	var new_discon_sound = disconnected_sound.instantiate()
	GameCore.instance.sfx_group.add_child(new_discon_sound)
	new_discon_sound.load_and_play(sound)


static func is_box_on_screen(box:CollisionShape2D) -> bool:
	var box_size:Vector2 = box.shape.size
	var cam_pos:Vector2 = UICore.instance.get_cam_center_pos()
	var cam_offset:Vector2 = UICore.instance.cam.offset * 0.5
	var pos_diff:Vector2 = box.position - cam_pos
	var within_x = absf(pos_diff.x) < cam_offset.x + box_size.x
	var within_y = absf(pos_diff.y) < cam_offset.y + box_size.y
	if within_x and within_y:
		return true
	else:
		return false
