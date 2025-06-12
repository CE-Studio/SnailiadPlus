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

var name_str:String = ""

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
	var character = int(Statics.current_profile["character"])
	var species = Statics.get_character_species_string(character)
	match type:
		ItemTypes.PEASHOOTER:
			id_str = "Peashooter"
			name_str = Statics.get_text("item_peashooter")
		ItemTypes.BOOMERANG:
			id_str = "Boomerang"
			name_str = Statics.get_text("item_boomerang")
		ItemTypes.RAINBOW_WAVE:
			id_str = "RainbowWave"
			name_str = Statics.get_text("item_rainbowWave")
		ItemTypes.DEVASTATOR:
			id_str = "Devastator"
			name_str = Statics.get_text("item_devastator")
			box.shape.size = Vector2(44, 28)
		ItemTypes.HIGH_JUMP:
			id_str = "HighJump"
			name_str = Statics.get_text("item_highJump")
			if character == Player.Players.BLOBBY:
				id_str = "WallGrab"
				name_str = Statics.get_text("item_wallGrab")
		ItemTypes.SHELL_SHIELD:
			id_str = "ShellShield"
			name_str = Statics.get_text("item_shellShield")
			if character == Player.Players.BLOBBY:
				id_str = "Shelmet"
				name_str = Statics.get_text("item_shelmet")
		ItemTypes.RAPID_FIRE:
			id_str = "RapidFire"
			name_str = Statics.get_text("item_rapidFire")
			if character == Player.Players.LEECHY:
				id_str = "Backfire"
				name_str = Statics.get_text("item_backfire")
		ItemTypes.ICE_SHELL:
			id_str = "IceSnail"
			name_str = Statics.get_text("item_iceSnail") % species
		ItemTypes.GRAVITY_SHELL:
			match character:
				Player.Players.UPSIDE:
					id_str = "MagneticFoot"
					name_str = Statics.get_text("item_magneticFoot")
				Player.Players.LEGGY:
					id_str = "CorkscrewJump"
					name_str = Statics.get_text("item_corkscrewJump")
				Player.Players.BLOBBY:
					id_str = "AngelJump"
					name_str = Statics.get_text("item_angelJump")
				_:
					id_str = "GravitySnail"
					name_str = Statics.get_text("item_gravSnail") % species
		ItemTypes.METAL_SHELL:
			id_str = "FullMetalSnail"
			match character:
				Player.Players.SLUGGY or Player.Players.LEECHY:
					name_str = Statics.get_text("item_fullMetalSnail_noShell") % species
				Player.Players.BLOBBY:
					name_str = Statics.get_text("item_fullMetalSnail_blob") % species
				_:
					name_str = Statics.get_text("item_fullMetalSnail_generic") % species
		ItemTypes.GRAVITY_SHOCK:
			id_str = "GravityShock"
			name_str = Statics.get_text("item_gravityShock")
		ItemTypes.SECRET_BOOMERANG:
			id_str = "Boomerang"
			name_str = Statics.get_text("item_boomerang_secret")
		ItemTypes.DEBUG_WAVE:
			id_str = "RainbowWave"
			name_str = Statics.get_text("item_rainbowWave_secret")
		ItemTypes.HEART_CONTAINER:
			id_str = "HeartContainer"
		ItemTypes.HELIX_FRAGMENT:
			id_str = "HelixFragment"
			box.shape.size = Vector2(12, 12)
		#ItemTypes.RADAR_SHELL:
		ItemTypes.WEAPON_LOCK_TRAP:
			id_str = "TrapItem"
			name_str = Statics.get_text("item_trapWeapon")
		ItemTypes.GRAVITY_LOCK_TRAP:
			id_str = "TrapItem"
			name_str = Statics.get_text("item_trapGravity")
		ItemTypes.LULLABY_TRAP:
			id_str = "TrapItem"
			name_str = Statics.get_text("item_trapLullaby")
		ItemTypes.SPIDER_TRAP:
			id_str = "TrapItem"
			name_str = Statics.get_text("item_trapSpider")
		ItemTypes.WARP_TRAP:
			id_str = "TrapItem"
			name_str = Statics.get_text("item_trapWarp")
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
		Statics.current_profile["item_rate"] = Statics.get_item_percentage()
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
			ItemTypes.HEART_CONTAINER:
				if Statics.is_in_boss_rush:
					name_str = Statics.get_text("item_heartContainer_noNum")
				else:
					name_str = Statics.get_text("item_heartContainer") % Statics.check_item(ItemTypes.HEART_CONTAINER)
			ItemTypes.HELIX_FRAGMENT:
				if Statics.is_in_boss_rush:
					name_str = Statics.get_text("item_helixFragment_noNum")
				else:
					name_str = Statics.get_text("item_helixFragment") % Statics.check_item(ItemTypes.HELIX_FRAGMENT)
			#ItemTypes.RADAR_SHELL:
			#ItemTypes.WEAPON_LOCK_TRAP:
			#ItemTypes.GRAVITY_LOCK_TRAP:
			#ItemTypes.LULLABY_TRAP:
			#ItemTypes.SPIDER_TRAP:
			#ItemTypes.WARP_TRAP:
			#_:
		Statics.save_profile(Statics.current_profile_id)
		UICore.instance.play_save_anim()
		UICore.instance.show_item_collection_text(name_str)


func _on_collect_timer_timeout() -> void:
	queue_free()
