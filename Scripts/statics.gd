extends RefCounted
class_name Statics


#region Variables
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

const HEALTH_ORB_VALUES = [ 1, 2, 4 ]
const HEALTH_ORB_MULTS = [ 1.25, 0.6, 0.125 ]

static var is_menu_open:bool = false
static var is_in_boss_rush:bool = false
static var increment_boss_rush_timer:bool = false
static var is_random_game:bool = false

static var noclip_mode:bool = false
static var damage_mult:bool = false
static var show_entity_layer:bool = false

# Block of vars from musicParent to healthOrbPointer

static var palette := preload("res://Assets/Images/Palette.png")
static var missing := preload("res://Assets/Images/Missing.png")

static var current_area:int = 0
static var current_subarea:int = 0

static var player:Player
static var cam_layer:Node2D
static var cam:Camera2D
static var active_room:Node2D
# UI

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
#endregion


#region Save info
static var profile:String
static var data_general:Dictionary
static var data_profile1:Dictionary
static var data_profile2:Dictionary
static var data_profile3:Dictionary
static var data_records:Dictionary
static var SAVE_PREFIX:String = "snailyplus_saves"
#endregion


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


static func save_general():
	var file = FileAccess.open("user://" + SAVE_PREFIX + "/GeneralData.json", FileAccess.WRITE_READ)
	file.store_string(JSON.stringify(data_general, "\t", false))


static func save_profile(profile:int):
	if profile < 1 or profile > 3:
		return
	var file = FileAccess.open("user://" + SAVE_PREFIX + "/Profile" + str(profile) + ".json", FileAccess.WRITE_READ)
	match profile:
		1:
			file.store_string(JSON.stringify(data_profile1, "\t", false))
		2:
			file.store_string(JSON.stringify(data_profile2, "\t", false))
		3:
			file.store_string(JSON.stringify(data_profile3, "\t", false))


static func save_records():
	var file = FileAccess.open("user://" + SAVE_PREFIX + "/Records.json", FileAccess.WRITE_READ)
	file.store_string(JSON.stringify(data_records, "\t", false))


static func save_all():
	save_general()
	save_profile(1)
	save_profile(2)
	save_profile(3)
	save_records()
