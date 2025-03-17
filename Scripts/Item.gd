@icon("res://Editor/ico/Item.svg")
extends Node2D
class_name Item


#region Variables
enum ItemTypes {
	PEASHOOTER,
	BOOMERANG,
	RAINBOW_WAVE,
	DEVASTATOR,
	HIGH_JUMP,
	SHELL_SHIELD,
	RAPID_FIRE,
	ICE_SHELL,
	GRAVITY_SHELL,
	METAL_SHELL,
	GRAVITY_SHOCK,
	SECRET_BOOMERANG,
	DEBUG_WAVE,
	HEART_CONTAINER,
	HELIX_FRAGMENT,
	RADAR_SHELL,
	WEAPON_LOCK_TRAP,
	GRAVITY_LOCK_TRAP,
	LULLABY_TRAP,
	SPIDER_TRAP,
	WARP_TRAP,
	NONE = -1,
}

@export var counted_in_percentage:bool = true
@export var type:ItemTypes = ItemTypes.NONE
@export_range(0, 9999) var location_id:int
@export var is_super_unique:bool = false
@export_group("Spawn requirements")
@export_flags("Easy", "Normal", "Insane") var difficulty_reqs = 7
@export_flags("Snaily", "Sluggy", "Upside", "Leggy", "Blobby", "Leechy") var character_reqs = 63

@onready var jingle_minor:AudioStreamPlayer = $"AudioGroup/MinorJingle"
@onready var jingle_major:AudioStreamPlayer = $"AudioGroup/MajorJingle"

var collected:bool = false
#endregion
