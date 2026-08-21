# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
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
@onready var sprite:SnailySprite2D = $"SnailySprite2D"
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
	or Statics.check_location_collected(location_id)
	or type == ItemTypes.NONE):
		queue_free()
		return

	var character = int(Statics.current_profile["character"])
	name_str = GlobalText.get_item_name(type)
	var anim:String = "unknown"
	match type:
		ItemTypes.PEASHOOTER:
			anim = "peashooter"
		ItemTypes.BOOMERANG:
			anim = "boomerang"
		ItemTypes.RAINBOW_WAVE:
			anim = "rainbow_wave"
		ItemTypes.DEVASTATOR:
			anim = "devastator"
			box.shape.size = Vector2(44, 28)
		ItemTypes.HIGH_JUMP:
			anim = "high_jump"
			if character == Player.Players.BLOBBY:
				anim = "wall_grab"
		ItemTypes.SHELL_SHIELD:
			anim = "shell_shield"
			if character == Player.Players.BLOBBY:
				anim = "shelmet"
		ItemTypes.RAPID_FIRE:
			anim = "rapid_fire"
			if character == Player.Players.LEECHY:
				anim = "backfire"
		ItemTypes.ICE_SHELL:
			anim = "ice_shell"
		ItemTypes.GRAVITY_SHELL:
			match character:
				Player.Players.UPSIDE:
					anim = "magnetic_foot"
				Player.Players.LEGGY:
					anim = "corkscrew_jump"
				Player.Players.BLOBBY:
					anim = "angel_jump"
				_:
					anim = "gravity_shell"
		ItemTypes.METAL_SHELL:
			anim = "full_metal_shell"
		ItemTypes.GRAVITY_SHOCK:
			anim = "gravity_shock"
		ItemTypes.SECRET_BOOMERANG:
			anim = "secret_boomerang"
		ItemTypes.DEBUG_WAVE:
			anim = "debug_rainbow_wave"
		ItemTypes.HEART_CONTAINER:
			anim = "heart_container"
		ItemTypes.HELIX_FRAGMENT:
			anim = "helix_fragment"
			box.shape.size = Vector2(12, 12)
		#ItemTypes.RADAR_SHELL:
		ItemTypes.WEAPON_LOCK_TRAP:
			anim = "trap"
		ItemTypes.GRAVITY_LOCK_TRAP:
			anim = "trap"
		ItemTypes.LULLABY_TRAP:
			anim = "trap"
		ItemTypes.SPIDER_TRAP:
			anim = "trap"
		ItemTypes.WARP_TRAP:
			anim = "trap"
	if not sprite.sprite_frames.has_animation(anim):
		anim = "unknown"
	sprite.play(anim)

	UICore.instance.darkness_layer.add_source(self, 48)


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
		var played_unique_dust:bool = false
		timer.start()
		if is_super_unique:
			Statics.play_sfx_disconnected(jingle_major)
			SInput.read_inputs = false
		else:
			Statics.play_sfx_disconnected(jingle_minor)

		Statics.add_item(type, 1)
		Statics.mark_item_location(location_id)
		Statics.current_profile["item_rate"] = Statics.get_item_percentage()
		match type:
			ItemTypes.PEASHOOTER:
				if Statics.stack_weapons or (GameCore.instance.player.selected_weapon < 2):
					if not Player.instance.is_equipped(1):
						GameCore.instance.player._toggle_weapon(1)
			ItemTypes.BOOMERANG:
				if Statics.stack_weapons or (GameCore.instance.player.selected_weapon < 4):
					if not Player.instance.is_equipped(2):
						GameCore.instance.player._toggle_weapon(2)
			ItemTypes.RAINBOW_WAVE:
				if Statics.stack_weapons or (GameCore.instance.player.selected_weapon < 8):
					if not Player.instance.is_equipped(3):
						GameCore.instance.player._toggle_weapon(3)
			#ItemTypes.DEVASTATOR:
			#ItemTypes.HIGH_JUMP:
			#ItemTypes.SHELL_SHIELD:
			#ItemTypes.RAPID_FIRE:
			ItemTypes.ICE_SHELL:
				if is_super_unique:
					played_unique_dust = true
					Statics.spawn_particle("ShellUpEffect", Room.Layers.GROUND, position, [1, true, true, 1])
			ItemTypes.GRAVITY_SHELL:
				if is_super_unique:
					played_unique_dust = true
					var anim_id:int = 0
					match int(Statics.current_profile["character"]):
						Player.Players.UPSIDE:
							anim_id = 4
						Player.Players.LEGGY:
							anim_id = 5
						Player.Players.BLOBBY:
							anim_id = 6
						_:
							anim_id = 2
					Statics.spawn_particle("ShellUpEffect", Room.Layers.GROUND, position, [anim_id, true, true, 2])
			ItemTypes.METAL_SHELL:
				if is_super_unique:
					played_unique_dust = true
					Statics.spawn_particle("ShellUpEffect", Room.Layers.GROUND, position, [3, true, true, 3])
			ItemTypes.GRAVITY_SHOCK:
				AchievementCore.instance.check_add(AchievementCore.Achievements.GRAVITY_SHOCK)
			ItemTypes.SECRET_BOOMERANG:
				if Statics.stack_weapons or (GameCore.instance.player.selected_weapon < 4):
					if not Player.instance.is_equipped(2):
						GameCore.instance.player._toggle_weapon(2)
				AchievementCore.instance.check_add(AchievementCore.Achievements.SECRET_BOOMERANG)
			ItemTypes.DEBUG_WAVE:
				if Statics.stack_weapons or (GameCore.instance.player.selected_weapon < 8):
					if not Player.instance.is_equipped(3):
						GameCore.instance.player._toggle_weapon(3)
			ItemTypes.HEART_CONTAINER:
				if not Statics.is_in_boss_rush:
					name_str = tr(&"Heart Container #%d") % Statics.check_item(Item.ItemTypes.HEART_CONTAINER)
				GameCore.instance.player.max_health += Statics.HEALTH_PER_HEART[Statics.current_profile["difficulty"]]
				GameCore.instance.player.health = GameCore.instance.player.max_health
				UICore.instance.heart_group.draw_new_hearts()
			ItemTypes.HELIX_FRAGMENT:
				if not Statics.is_in_boss_rush:
					name_str = tr(&"Helix Fragment #%d") % Statics.check_item(Item.ItemTypes.HELIX_FRAGMENT)
			#ItemTypes.RADAR_SHELL:
			#ItemTypes.WEAPON_LOCK_TRAP:
			#ItemTypes.GRAVITY_LOCK_TRAP:
			#ItemTypes.LULLABY_TRAP:
			#ItemTypes.SPIDER_TRAP:
			#ItemTypes.WARP_TRAP:
			#_:
		if is_super_unique and not played_unique_dust:
			Statics.spawn_particle("ShellUpEffect", Room.Layers.GROUND, position, [0, true, true])
		var is_100_rate:bool = Statics.get_item_percentage(0, true, true) == 100.0
		Statics.save_profile(Statics.current_profile_id)
		UICore.instance.play_save_anim()
		UICore.instance.show_item_collection_text(name_str, is_100_rate)
		UICore.instance.minimap.update_markers(UICore.instance.minimap.last_drawn_cells)
		UICore.instance.minimap.update_player()
		if is_100_rate:
			AchievementCore.instance.check_add(AchievementCore.Achievements.ITEMS_100)
			Statics.add_unlock_condition(Statics.Unlocks.ITEM_RANDO)


func _on_collect_timer_timeout() -> void:
	queue_free()
