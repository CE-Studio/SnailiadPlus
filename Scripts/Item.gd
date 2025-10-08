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
	if GameCore.instance == null:
		return
	if (difficulty_reqs & (1 << int(Statics.current_profile["difficulty"])) == 0
	or character_reqs & (1 << int(Statics.current_profile["character"])) == 0
	or Statics.check_location_collected(location_id)):
		queue_free()
		return
	
	var id_str
	var character = int(Statics.current_profile["character"])
	name_str = get_name_str_from_id(type)
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
			if character == Player.Players.BLOBBY:
				id_str = "WallGrab"
		ItemTypes.SHELL_SHIELD:
			id_str = "ShellShield"
			if character == Player.Players.BLOBBY:
				id_str = "Shelmet"
		ItemTypes.RAPID_FIRE:
			id_str = "RapidFire"
			if character == Player.Players.LEECHY:
				id_str = "Backfire"
		ItemTypes.ICE_SHELL:
			id_str = "IceSnail"
		ItemTypes.GRAVITY_SHELL:
			match character:
				Player.Players.UPSIDE:
					id_str = "MagneticFoot"
				Player.Players.LEGGY:
					id_str = "CorkscrewJump"
				Player.Players.BLOBBY:
					id_str = "AngelJump"
				_:
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


static func get_name_str_from_id(id:ItemTypes, specify_shell:bool = false) -> String:
	var character = int(Statics.current_profile["character"])
	var species = Statics.get_character_species_string(character)
	var shell = Statics.get_text("subscreen_shell")
	match id:
		ItemTypes.PEASHOOTER:
			return Statics.get_text("item_peashooter")
		ItemTypes.BOOMERANG:
			return Statics.get_text("item_boomerang")
		ItemTypes.RAINBOW_WAVE:
			return Statics.get_text("item_rainbowWave")
		ItemTypes.DEVASTATOR:
			return Statics.get_text("item_devastator")
		ItemTypes.HIGH_JUMP:
			if character == Player.Players.BLOBBY:
				return Statics.get_text("item_wallGrab")
			return Statics.get_text("item_highJump")
		ItemTypes.SHELL_SHIELD:
			if character == Player.Players.BLOBBY:
				return Statics.get_text("item_shelmet")
			return Statics.get_text("item_shellShield")
		ItemTypes.RAPID_FIRE:
			if character == Player.Players.LEECHY:
				return Statics.get_text("item_backfire")
			return Statics.get_text("item_rapidFire")
		ItemTypes.ICE_SHELL:
			return Statics.get_text("item_iceSnail") % species
		ItemTypes.GRAVITY_SHELL:
			match character:
				Player.Players.UPSIDE:
					return Statics.get_text("item_magneticFoot")
				Player.Players.LEGGY:
					return Statics.get_text("item_corkscrewJump")
				Player.Players.BLOBBY:
					return Statics.get_text("item_angelJump")
				_:
					return Statics.get_text("item_gravSnail") % species
		ItemTypes.METAL_SHELL:
			match character:
				Player.Players.SLUGGY or Player.Players.LEECHY:
					return Statics.get_text("item_fullMetalSnail_noShell") % species
				Player.Players.BLOBBY:
					return Statics.get_text("item_fullMetalSnail_blob") % species
				_:
					if specify_shell:
						return Statics.get_text("item_fullMetalSnail_generic") % shell
					return Statics.get_text("item_fullMetalSnail_generic") % species
		ItemTypes.GRAVITY_SHOCK:
			return Statics.get_text("item_gravityShock")
		ItemTypes.SECRET_BOOMERANG:
			return Statics.get_text("item_boomerang_secret")
		ItemTypes.DEBUG_WAVE:
			return Statics.get_text("item_rainbowWave_secret")
		ItemTypes.HEART_CONTAINER:
			return Statics.get_text("item_heartContainer_noNum")
		ItemTypes.HELIX_FRAGMENT:
			return Statics.get_text("item_helixFragment_noNum")
		#ItemTypes.RADAR_SHELL:
		ItemTypes.WEAPON_LOCK_TRAP:
			return Statics.get_text("item_trapWeapon")
		ItemTypes.GRAVITY_LOCK_TRAP:
			return Statics.get_text("item_trapGravity")
		ItemTypes.LULLABY_TRAP:
			return Statics.get_text("item_trapLullaby")
		ItemTypes.SPIDER_TRAP:
			return Statics.get_text("item_trapSpider")
		ItemTypes.WARP_TRAP:
			return Statics.get_text("item_trapWarp")
	if specify_shell:
		var normal = Statics.get_text("subscreen_shellNormal")
		match character:
			Player.Players.SLUGGY or Player.Players.BLOBBY or Player.Players.LEECHY:
				return normal % species
			_:
				return normal % shell
	return "item_nothing"


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


func _on_player_entered(_body: Node2D) -> void:
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
				if Statics.stack_weapons or (GameCore.instance.player.selected_weapon < 2):
					GameCore.instance.player._toggle_weapon(1)
				UICore.instance.update_weapon_icons(false)
			ItemTypes.BOOMERANG:
				if Statics.stack_weapons or (GameCore.instance.player.selected_weapon < 4):
					GameCore.instance.player._toggle_weapon(2)
				UICore.instance.update_weapon_icons(false)
			ItemTypes.RAINBOW_WAVE:
				if Statics.stack_weapons or (GameCore.instance.player.selected_weapon < 8):
					GameCore.instance.player._toggle_weapon(3)
				UICore.instance.update_weapon_icons(false)
			#ItemTypes.DEVASTATOR:
			#ItemTypes.HIGH_JUMP:
			#ItemTypes.SHELL_SHIELD:
			#ItemTypes.RAPID_FIRE:
			#ItemTypes.ICE_SHELL:
			#ItemTypes.GRAVITY_SHELL:
			#ItemTypes.METAL_SHELL:
			#ItemTypes.GRAVITY_SHOCK:
			ItemTypes.SECRET_BOOMERANG:
				if Statics.stack_weapons or (GameCore.instance.player.selected_weapon < 4):
					GameCore.instance.player._toggle_weapon(2)
				UICore.instance.update_weapon_icons(false)
				UICore.instance.achievement_core.check_add(AchievementCore.Achievements.SECRET_BOOMERANG)
			ItemTypes.DEBUG_WAVE:
				if Statics.stack_weapons or (GameCore.instance.player.selected_weapon < 8):
					GameCore.instance.player._toggle_weapon(3)
				UICore.instance.update_weapon_icons(false)
			ItemTypes.HEART_CONTAINER:
				if Statics.is_in_boss_rush:
					name_str = Statics.get_text("item_heartContainer_noNum")
				else:
					name_str = Statics.get_text("item_heartContainer") % Statics.check_item(ItemTypes.HEART_CONTAINER)
				GameCore.instance.player.max_health += Statics.HEALTH_PER_HEART[Statics.current_profile["difficulty"]]
				GameCore.instance.player.health = GameCore.instance.player.max_health
				UICore.instance.draw_new_hearts()
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
		UICore.instance.minimap.update_markers(UICore.instance.minimap.last_drawn_cells)
		UICore.instance.minimap.update_player()


func _on_collect_timer_timeout() -> void:
	queue_free()
