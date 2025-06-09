@icon("res://Editor/ico/Item.svg")
class_name Item
extends Node2D


#region Variables
enum ItemTypes {
	PEASHOOTER,        #  0
	BOOMERANG,         #  1
	RAINBOW_WAVE,      #  2
	DEVASTATOR,        #  3
	HIGH_JUMP,         #  4 - Wall Grab
	SHELL_SHIELD,      #  5 - Shelmet
	RAPID_FIRE,        #  6 - Backfire
	ICE_SHELL,         #  7
	GRAVITY_SHELL,     #  8 - Magnetic Foot - Corkscrew Jump - Angel Jump
	METAL_SHELL,       #  9
	GRAVITY_SHOCK,     # 10
	SECRET_BOOMERANG,  # 11
	DEBUG_WAVE,        # 12
	HEART_CONTAINER,   # 13
	HELIX_FRAGMENT,    # 14
	RADAR_SHELL,       # 15
	BROOM,             # 16
	WEAPON_LOCK_TRAP,  # 17
	GRAVITY_LOCK_TRAP, # 18
	LULLABY_TRAP,      # 19
	SPIDER_TRAP,       # 20
	WARP_TRAP,         # 21
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
	if (difficulty_reqs & (1 << int(Statics.current_profile["difficulty"])) == 0
	or character_reqs & (1 << int(Statics.current_profile["character"])) == 0
	or Statics.check_location_collected(location_id)):
		queue_free()
		return
	
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
		_:
			id_str = "ItemBoundaryVisual"
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
		Statics.mark_item_location(location_id)
		match type:
			ItemTypes.PEASHOOTER:
				if Statics.stack_weapons or (GameCore.instance.player.selected_weapon < 1):
					GameCore.instance.player._toggle_weapon(1)
				UICore.instance.update_weapon_icons()
			ItemTypes.BOOMERANG:
				if Statics.stack_weapons or (GameCore.instance.player.selected_weapon < 2):
					GameCore.instance.player._toggle_weapon(2)
				UICore.instance.update_weapon_icons()
			ItemTypes.RAINBOW_WAVE:
				if Statics.stack_weapons or (GameCore.instance.player.selected_weapon < 3):
					GameCore.instance.player._toggle_weapon(3)
				UICore.instance.update_weapon_icons()
			#ItemTypes.DEVASTATOR:
			#ItemTypes.HIGH_JUMP:
			#ItemTypes.SHELL_SHIELD:
			#ItemTypes.RAPID_FIRE:
			#ItemTypes.ICE_SHELL:
			#ItemTypes.GRAVITY_SHELL:
			#ItemTypes.METAL_SHELL:
			#ItemTypes.GRAVITY_SHOCK:
			ItemTypes.SECRET_BOOMERANG:
				if Statics.stack_weapons or (GameCore.instance.player.selected_weapon < 2):
					GameCore.instance.player._toggle_weapon(2)
				UICore.instance.update_weapon_icons()
			ItemTypes.DEBUG_WAVE:
				if Statics.stack_weapons or (GameCore.instance.player.selected_weapon < 3):
					GameCore.instance.player._toggle_weapon(3)
				UICore.instance.update_weapon_icons()
			#ItemTypes.HEART_CONTAINER:
			#ItemTypes.HELIX_FRAGMENT:
			#ItemTypes.RADAR_SHELL:
			#ItemTypes.WEAPON_LOCK_TRAP:
			#ItemTypes.GRAVITY_LOCK_TRAP:
			#ItemTypes.LULLABY_TRAP:
			#ItemTypes.SPIDER_TRAP:
			#ItemTypes.WARP_TRAP:
			#_:
		Statics.save_profile(Statics.current_profile_id)
		UICore.instance.play_save_anim()


func _on_collect_timer_timeout() -> void:
	queue_free()
