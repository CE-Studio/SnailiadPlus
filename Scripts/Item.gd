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
@onready var sprite:JsonSprite2D = $"JsonSprite2D"

var collected:bool = false
#endregion


func _ready() -> void:
	pass
	var id_str
	match type:
		ItemTypes.PEASHOOTER:
			id_str = "Peashooter"
		ItemTypes.BOOMERANG:
			id_str = "Boomerang"
		ItemTypes.RAINBOW_WAVE:
			id_str = "RainbowWave"
		ItemTypes.DEVASTATOR:
			id_str = "Devastator"
		ItemTypes.HIGH_JUMP:
			id_str = "HighJump"
		ItemTypes.SHELL_SHIELD:
			id_str = "ShellShield"
		ItemTypes.RAPID_FIRE:
			id_str = "RapidFire"
		ItemTypes.ICE_SHELL:
			id_str = "IceSnail"
		ItemTypes.GRAVITY_SHELL:
			id_str = "GravitySnail"
		ItemTypes.METAL_SHELL:
			id_str = "FullMetalSnail"
		ItemTypes.GRAVITY_SHOCK:
			id_str = "GravityShock"
		ItemTypes.SECRET_BOOMERANG:
			id_str = "Boomerang"
		ItemTypes.DEBUG_WAVE:
			id_str = "RainbowWave"
		ItemTypes.HEART_CONTAINER:
			id_str = "HeartContainer"
		ItemTypes.HELIX_FRAGMENT:
			id_str = "HelixFragment"
		#ItemTypes.RADAR_SHELL:
		ItemTypes.WEAPON_LOCK_TRAP:
			id_str = "TrapItem"
		ItemTypes.GRAVITY_LOCK_TRAP:
			id_str = "TrapItem"
		ItemTypes.LULLABY_TRAP:
			id_str = "TrapItem"
		ItemTypes.SPIDER_TRAP:
			id_str = "TrapItem"
		ItemTypes.WARP_TRAP:
			id_str = "TrapItem"
	


func _on_player_entered(body: Node2D) -> void:
	pass # Replace with function body.
