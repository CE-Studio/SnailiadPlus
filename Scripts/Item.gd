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

@onready var jingle_minor:AudioStream = load("res://Assets/Sounds/Music/MinorItemJingle.ogg")
@onready var jingle_major:AudioStream = load("res://Assets/Sounds/Music/MajorItemJingle.ogg")
@onready var sprite:JsonSprite2D
@onready var box:CollisionShape2D = $"Area2D/CollisionShape2D"
@onready var timer:Timer = $"CollectTimer"

var collected:bool = false

const HOVER_DIS:int = 24
const HOVER_EASE:float = 12.5
#endregion


func _ready() -> void:
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
			box.shape.size = Vector2(44, 28)
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
			box.shape.size = Vector2(12, 12)
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
	sprite = JsonSprite2D.new()
	sprite.texture_path = "res://Assets/Images/Items/" + id_str + ".json"
	add_child.call_deferred(sprite)
	sprite.action = "item"


func _process(delta: float) -> void:
	if collected:
		var target_pos = GameCore.instance.player.position
		match GameCore.instance.player.gravity_dir:
			Statics.DirsSurface.FLOOR:
				target_pos += HOVER_DIS * Vector2.UP
			Statics.DirsSurface.LWALL:
				target_pos += HOVER_DIS * Vector2.RIGHT
			Statics.DirsSurface.RWALL:
				target_pos += HOVER_DIS * Vector2.LEFT
			Statics.DirsSurface.CEILING:
				target_pos += HOVER_DIS * Vector2.DOWN
		position = position.lerp(target_pos, HOVER_EASE * delta)


func _on_player_entered(body: Node2D) -> void:
	if not collected:
		collected = true
		timer.start()
		if is_super_unique:
			Statics.play_sfx_disconnected(jingle_major)
		else:
			Statics.play_sfx_disconnected(jingle_minor)
		Statics.add_item(type, 1)


func _on_collect_timer_timeout() -> void:
	queue_free()
