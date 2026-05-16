# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
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
const VECTOR_CENTER:Vector2 = Vector2(200.0, 120.0)


const ASPECT_RATIOS:Array = [
	Vector2i(400, 240), # 5:3
	Vector2i(400, 320), # 5:4
	Vector2i(400, 300), # 4:3
	Vector2i(448, 252), # 16:9
	Vector2i(400, 250), # 16:10
	Vector2i(402, 268), # 3:2
]
const ASPECT_RATIO_OFFSETS:Array = [
	Vector2i(0, 0),
	Vector2i(0, 80),
	Vector2i(0, 60),
	Vector2i(48, 12),
	Vector2i(0, 10),
	Vector2i(2, 28),
]
const TARGET_FRAMERATES:Array = [
	0,
	30,
	60,
	120
]


const HEALTH_PER_HEART = [ 8, 4, 2 ]
const HEALTH_ORB_VALUES = [ 1, 2, 4 ]
const HEALTH_ORB_MULTS = [ 1.25, 0.6, 0.125 ]

const DAMAGE_MULT = 10


const ROOM_PATH:String = "res://Scenes/Rooms/%s.tscn"
const WORLD_SPAWN:Array = [
	[ "SnailTown/TownMain", 640, 600 ], # Snaily
	[ "SnailTown/TownMain", 640, 600 ], # Sluggy
	[ "SnailTown/TownMain", 928, 152 ], # Upside
	[ "SnailTown/TownMain", 640, 600 ], # Leggy
	[ "SnailTown/TownMain", 640, 600 ], # Blobby
	[ "SnailTown/TownMain", 640, 600 ], # Leechy
]


# Maximum counts of each item in Item.ItemTypes for a save to be considered 100% complete
const COUNTED_INVENTORY:Array = [ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 11, 30, 1, 0, 0, 0, 0, 0, 0 ]


static var main_menu_booted_once:bool = false
static var is_menu_open:bool = false
static var shortcut_load_game_scene:bool = false

static var is_in_boss_rush:bool = false
static var increment_igt:bool = true
static var increment_boss_rush_timer:bool = false
static var is_random_game:bool = false


static var noclip_mode:bool = false
static var damage_mult:bool = false
static var show_entity_layer:bool = false
static var show_invis_entites:bool = false
static var stack_shells:bool = true
static var stack_weapons:bool = true
static var stack_weapon_mods:bool = true

# Block of vars from musicParent to healthOrbPointer

static var palette:Image = preload("res://Assets/Images/Palette.png")
static var missing := preload("res://Assets/Images/Missing.png")


static var disconnected_sound := preload("res://Scenes/internals/DisconnectedSound.tscn")
static var damage_number := preload("res://Scenes/internals/DamageNumber.tscn")


static var current_area:int = 0
static var current_subarea:int = 0


static var player:Player
static var cam_layer:Node2D
static var cam:Camera2D
static var active_room:Room

static var text_lib:Dictionary

static var particle_cache:Array[PackedScene] = []
static var particle_table:Array[String] = []


#region Game scene load information
static var load_room:String
static var load_coords:Vector2i
#endregion
#endregion


#region Save info
static var profile:String
static var data_profile1:Dictionary[String, Variant]
static var data_profile2:Dictionary[String, Variant]
static var data_profile3:Dictionary[String, Variant]
static var data_records:Dictionary[String, Variant]
static var save_prefix:String = "snailyplus_saves"
static var current_profile:Dictionary[String, Variant]
static var current_profile_id:int
static var cutscene_persistent_vars:Dictionary[StringName, Dictionary] = {&"": {}}

enum Unlocks {
	BOSS_RUSH, # (Boss Rush; earned from beating the game)
	CHAR_SEL, # (Character unlock; earned from Boss Rush completion)
	ABSURD_DIFF, # (Absurd difficulty; earned from beating the game in 30 minutes or fewer)
	ITEM_RANDO, # (Item randomizer gamemode; earned from collecting 100% of counted items)
	OPEN_MAP, # (Fully revealed map on profile start; earned from filling 100% of the map)
	SIX_HUNDO, # (600% gamemode; earned from beating the game with any character aside from Snaily)
	CHAOS_MODE, # (Chaos gamemode; earned from beating the game on absurd difficulty)
}

enum ParticleOptions {
	NONE,
	ENVIRONMENTS,
	ENTITIES_FLASH,
	ENTITIES_ALL,
	FLASH,
	ALL,
}

enum WorldFlags {
	HAS_SEEN_CAVE_SNAIL,
	TALKED_TO_IRIS,
	DEFEATED_BOSS1,
	DEFEATED_BOSS2,
	DEFEATED_BOSS3,
	DEFEATED_BOSS4,
	SEEN_GRAVITY_CUTSCENE,
}

enum CutsceneFlags {
	PLACEHOLDER0,
	PLACEHOLDER1,
	PLACEHOLDER2,
	PLACEHOLDER3,
}
#endregion


#region Profile functions
static func format_game_time(time:Array) -> String:
	var time_string := "%d:%02d:%05.2f" % [ time[0], time[1], time[2] ]
	return time_string


static func get_item_percentage(_profile:int = 0, _snap:bool = false) -> float:
	var inventory:Array
	var _player:int
	var difficulty:int
	match _profile:
		1:
			inventory = data_profile1["items"]
			_player = data_profile1["character"]
			difficulty = data_profile1["difficulty"]
		2:
			inventory = data_profile2["items"]
			_player = data_profile2["character"]
			difficulty = data_profile2["difficulty"]
		3:
			inventory = data_profile3["items"]
			_player = data_profile3["character"]
			difficulty = data_profile3["difficulty"]
		_:
			inventory = current_profile["items"]
			_player = current_profile["character"]
			difficulty = current_profile["difficulty"]
	var collected_items:int = 0 # Counted items the player has collected and saved to ["items"]
	var max_items:int = 0 # Maximum item count for 100% as dictated by COUNTED_INVENTORY
	var total_items:int = 0 # Complete collection of items, counted or not, saved to ["items"]
	for i in inventory.size():
		total_items += inventory[i]
		match i:
			Item.ItemTypes.SHELL_SHIELD:
				if _player != Player.Players.SLUGGY and _player != Player.Players.LEECHY:
					collected_items += clampi(inventory[i], 0, COUNTED_INVENTORY[i])
					max_items += COUNTED_INVENTORY[i]
			Item.ItemTypes.ICE_SHELL:
				if difficulty != 2:
					collected_items += clampi(inventory[i], 0, COUNTED_INVENTORY[i])
					max_items += COUNTED_INVENTORY[i]
			_:
				if COUNTED_INVENTORY[i] > 0:
					collected_items += clampi(inventory[i], 0, COUNTED_INVENTORY[i])
					max_items += COUNTED_INVENTORY[i]
	var counted_percentage:float = (float(collected_items) / float(max_items)) * 100.0
	if counted_percentage == 100.0:
		var over_percentage:float = (float(total_items) / float(max_items)) * 100.0
		if _snap:
			over_percentage = snappedf(over_percentage, 0.1)
		return over_percentage
	if _snap:
		counted_percentage = snappedf(counted_percentage, 0.1)
	return counted_percentage


static func get_igt_str(_profile:int = 0) -> String:
	var time:Array = []
	match _profile:
		1:
			time = data_profile1["game_time"]
		2:
			time = data_profile2["game_time"]
		3:
			time = data_profile3["game_time"]
		_:
			time = current_profile["game_time"]
	var time_str:String = ""
	if time[0] > 0:
		time_str = "%d:%02d:%05.2f" % [ time[0], time[1], time[2] ]
	else:
		time_str = "%d:%05.2f" % [ time[1], time[2] ]
	time_str = time_str.strip_edges()
	return time_str
#endregion


#region Save functions
static func add_item(id:int, count:int) -> void:
	while id >= len(current_profile["items"]):
		current_profile["items"].append(0)
	current_profile["items"][id] += count


static func remove_item(id:int, count:int) -> void:
	if id < len(current_profile["items"]):
		current_profile["items"][id] -= count
		if current_profile["items"][id] < 0:
			current_profile["items"][id] = 0


static func check_item(id:int) -> int:
	var output:int = 0
	if id < len(current_profile["items"]):
		output = current_profile["items"][id]
	return output


static func mark_item_location(id:int, state:bool = true) -> void:
	while id >= len(current_profile["locations"]):
		current_profile["locations"].append(false)
	current_profile["locations"].set(id, state)


static func check_location_collected(id:int) -> bool:
	var output := false
	if id < len(current_profile["locations"]):
		output = current_profile["locations"][id]
	return output


static func save_general() -> void:
	ProjectSettings.save_custom("user://general_settings.godot")


static func save_profile(iprofile:int) -> void:
	if iprofile < 1 or iprofile > 3:
		return
	var file := FileAccess.open("user://" + save_prefix + "/Profile" + str(iprofile) + ".json", FileAccess.WRITE_READ)
	match iprofile:
		1:
			if iprofile == current_profile_id:
				data_profile1["cutscene_flags"] = CutsceneController.save_flags()
			file.store_string(JSON.stringify(data_profile1, "\t", false))
		2:
			if iprofile == current_profile_id:
				data_profile2["cutscene_flags"] = CutsceneController.save_flags()
			file.store_string(JSON.stringify(data_profile2, "\t", false))
		3:
			if iprofile == current_profile_id:
				data_profile3["cutscene_flags"] = CutsceneController.save_flags()
			file.store_string(JSON.stringify(data_profile3, "\t", false))
	file.close()


static func save_records() -> void:
	var file := FileAccess.open("user://" + save_prefix + "/Records.json", FileAccess.WRITE_READ)
	file.store_string(JSON.stringify(data_records, "\t", false))
	file.close()


static func save_all() -> void:
	save_general()
	save_profile(1)
	save_profile(2)
	save_profile(3)
	save_records()


static func delete_profile(iprofile:int) -> void:
	if iprofile < 1 or iprofile > 3:
		return
	var file := "user://" + save_prefix + "/Profile" + str(iprofile) + ".json"
	DirAccess.remove_absolute(file)


static func has_unlock(unlock:Unlocks) -> bool:
	return data_records["unlocks"].has(unlock)


static func add_achievement(id:int) -> void:
	if check_achievement(id):
		return
	while id >= len(data_records["achievements"]):
		data_records["achievements"].append(false)
	data_records["achievements"][id] = true


static func check_achievement(id:int) -> bool:
	var output := false
	if id < len(data_records["achievements"]):
		output = data_records["achievements"][id]
	return output


static func get_achievement_count() -> int:
	var earned:int = 0
	for i in AchievementCore.Achievements.keys().size():
		if check_achievement(i):
			earned += 1
	return earned


static func add_bestiary_entry(id:int) -> void:
	if check_bestiary_entry(id):
		return
	while id >= len(data_records["bestiary"]):
		data_records["bestiary"].append(false)
	data_records["bestiary"][id] = true
	UICore.instance.play_bestiary_anim()
	save_records()


static func check_bestiary_entry(id:int) -> bool:
	var output := false
	if id < len(data_records["bestiary"]):
		output = data_records["bestiary"][id]
	return output


static func set_world_flag(id:WorldFlags, value:Variant) -> void:
	while id >= len(current_profile["world_flags"]):
		current_profile["world_flags"].append(null)
	current_profile["world_flags"][id] = value


static func get_world_flag(id:WorldFlags) -> Variant:
	if id < len(current_profile["world_flags"]):
		if current_profile["world_flags"][id] == null:
			return false
		return current_profile["world_flags"][id]
	return false


static func get_time(id:String) -> Array:
	if data_records["times"].has(id):
		return data_records["times"][id]
	return [0.0, 0.0, 0.0]


static func has_time(id:String) -> bool:
	var time:Array = get_time(id)
	return time != [0.0, 0.0, 0.0]


static func infer_time_id() -> String:
	var character:String = "snaily"
	match Player.instance.who_i_is:
		Player.Players.SLUGGY: character = "sluggy"
		Player.Players.UPSIDE: character = "upside"
		Player.Players.LEGGY: character = "leggy"
		Player.Players.BLOBBY: character = "blobby"
		Player.Players.LEECHY: character = "leechy"
	var mode:String = "normal"
	if is_in_boss_rush: mode = "rush"
	elif current_profile["difficulty"] == 0: mode = "easy"
	elif current_profile["difficulty"] == 2: mode = "insane"
	return "_".join([character, mode])


static func has_times_for_character(character:Player.Players) -> bool:
	var ch_str:String = "snaily"
	match character:
		Player.Players.SLUGGY: ch_str = "sluggy"
		Player.Players.UPSIDE: ch_str = "upside"
		Player.Players.LEGGY: ch_str = "leggy"
		Player.Players.BLOBBY: ch_str = "blobby"
		Player.Players.LEECHY: ch_str = "leechy"
	return (has_time(ch_str + "_easy") or has_time(ch_str + "_normal")
		or has_time(ch_str + "_insane") or has_time(ch_str + "_rush"))


static func has_times_for_mode(mode:String) -> bool:
	return (has_time("snaily_" + mode) or has_time("sluggy_" + mode) or has_time("upside_" + mode)
	or has_time("leggy_" + mode) or has_time("blobby_" + mode) or has_time("leechy_" + mode))


static func save_time(id:String, time:Array) -> void:
	if data_records["times"].keys().has(id):
		data_records["times"][id] = time.duplicate()


## Compares two game times, and returns a value equal to the result of the comparison.
## Will return -1 if [code]compare[/code] is less than [code]against[/code], 1 if more than, and 0 if equal
static func compare_times(compare:Array, against:Array) -> int:
	if compare[0] < against[0]: return -1
	if compare[0] > against[0]: return 1
	if compare[1] < against[1]: return -1
	if compare[1] > against[1]: return 1
	if compare[2] < against[2]: return -1
	if compare[2] > against[2]: return 1
	return 0


static func save_highest_percent(character:Player.Players, difficulty:int, rate:float) -> void:
	data_records["highest_percents"][character as int][difficulty] = rate


static func get_highest_percent(character:Player.Players, difficulty:int) -> float:
	return data_records["highest_percents"][character as int][difficulty]


static func save_lowest_percent(character:Player.Players, difficulty:int, rate:float) -> void:
	data_records["lowest_percents"][character as int][difficulty] = rate


static func get_lowest_percent(character:Player.Players, difficulty:int) -> float:
	return data_records["lowest_percents"][character as int][difficulty]
#endregion


#region Application functions
static func get_window_size() -> Vector2:
	return Vector2(ProjectSettings.get_setting("display/window/size/viewport_width"),
	ProjectSettings.get_setting("display/window/size/viewport_height"))


static func parse_version_to_text_string(version:String) -> String:
	var prefix := version.substr(0, 1)
	var number := version.substr(1)
	if is_number(prefix):
		number = version
	var output:String
	match prefix:
		"b": output = GlobalText.version_types[0] # Dev/beta
		"d": output = GlobalText.version_types[1] # Demo
		_: output = GlobalText.version_types[2]   # Release
	output = " ".join([output, number])
	return output


static func parse_version_to_array(version:String) -> Array:
	var prefix := version.substr(0, 1)
	if not is_number(prefix):
		version = version.substr(1)
	var version_parts := version.split(".")
	var version_numbers:Array
	for part in version_parts:
		version_numbers.append(int(part))
	return version_numbers


static func compare_versions(compare:Array, against:Array) -> int:
	if compare[0] < against[0]:
		return -1
	elif compare[0] > against[0]:
		return 1
	if compare[1] < against[1]:
		return -1
	elif compare[1] > against[1]:
		return 1
	if compare[2] < against[2]:
		return -1
	elif compare[2] > against[2]:
		return 1
	return 0
#endregion


#region Player state functions
# Modes -  0 - level (0-3)
#          1 - collective (0-7, accounts for all active shells)
static func get_shell_level(mode:int = 0) -> int:
	if mode == 1:
		var total:int = 0
		total += 1 if check_item(Item.ItemTypes.ICE_SHELL) else 0
		total += 2 if check_item(Item.ItemTypes.GRAVITY_SHELL) else 0
		total += 4 if check_item(Item.ItemTypes.METAL_SHELL) else 0
		return total
	if check_item(Item.ItemTypes.METAL_SHELL):
		return 3
	if check_item(Item.ItemTypes.GRAVITY_SHELL):
		return 2
	if check_item(Item.ItemTypes.ICE_SHELL):
		return 1
	return 0


static func has_shell(shell_id:int) -> bool:
	var query := 1 << (shell_id - 1)
	var level := 1 << (get_shell_level() - 1)
	if stack_shells:
		return query <= level
	return query & level > 0


static func get_character_name_string(character:Player.Players, full:bool = false) -> String:
	var char_i:int = character as int
	var full_i:int = 1 if full else 0
	return GlobalText.characters[char_i][full_i]


static func get_character_species_string(character:Player.Players, plural:bool = false) -> String:
	var char_i:int = character as int
	var plural_i:int = 1 if plural else 0
	return GlobalText.species[char_i][plural_i]
#endregion


#region World functions
static func solid_at_world_pos(pos:Vector2, enemy_collidable:bool = false) -> bool:
	var tile_pos := Vector2i(pos * FRAC_16)
	#print("Received world pos %s\nTranslating to tile pos %s" % [ str(pos), str(tile_pos) ])
	return solid_at_grid_pos(tile_pos, enemy_collidable)


static func solid_at_grid_pos(pos:Vector2i, enemy_collidable:bool = false) -> bool:
	if not active_room:
		return false
	if active_room.map_ground.get_cell_tile_data(pos):
		return true
	if (enemy_collidable and
	(active_room.map_entity1.get_cell_atlas_coords(pos) == Vector2i(2, 24)) or
	(active_room.map_entity2.get_cell_atlas_coords(pos) == Vector2i(2, 24))):
		return true
	return false


static func is_point_on_screen(pos:Vector2, buffer:Vector2 = Vector2.ZERO) -> bool:
	var aspect_buffer:Vector2 = ASPECT_RATIOS[ProjectSettings.get_setting("display/window/size/aspect_ratio")] * 0.5
	var cam_pos:Vector2 = UICore.instance.get_cam_center_pos()
	var within_x:bool = abs(pos.x - cam_pos.x) <= aspect_buffer.x + buffer.x
	var within_y:bool = abs(pos.y - cam_pos.y) <= aspect_buffer.y + buffer.y
	return within_x and within_y
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
	var scale := pow(0.1, speed)
	num = num * pow(scale, elapsed) + target * (1.0 - pow(scale, elapsed))
	if absf(num - target) < threshold:
		num = target
	return num


static func normalized_sigmoid(val:float, modifier:float = 12.0) -> float:
	return 1.0 / (1.0 + exp(-(val * modifier - (modifier * 0.5))))


static func play_sfx_disconnected(sound:AudioStream, vol:float = 1.0) -> void:
	var active_sounds_of_type:int = 0
	for sfx in GameCore.instance.sfx_group.get_children():
		if sfx.stream == sound:
			active_sounds_of_type += 1
	if active_sounds_of_type < 2:
		var new_discon_sound:AudioStreamPlayer = disconnected_sound.instantiate()
		GameCore.instance.sfx_group.add_child(new_discon_sound)
		new_discon_sound.load_and_play(sound, vol)


static func play_sfx_limited(sound:AudioStream, sound_name:String, vol:float = 1.0) -> void:
	GameCore.instance.lim_sfx_handler.play_sound(sound, sound_name, vol)


static func spawn_particle(name:String, layer:Room.Layers, pos:Vector2, data:Array = []) -> Particle:
	if active_room == null:
		return null
	if not particle_table.has(name):
		particle_table.append(name)
		particle_cache.append(load("res://Scenes/Particles/%s.tscn" % name))
	var new_particle:Particle = particle_cache[particle_table.find(name)].instantiate()
	match layer:
		Room.Layers.SKY: active_room.layer_sky.add_child(new_particle)
		Room.Layers.BG2: active_room.layer_bg2.add_child(new_particle)
		Room.Layers.BG1: active_room.layer_bg1.add_child(new_particle)
		Room.Layers.GROUND: active_room.layer_ground.add_child(new_particle)
		Room.Layers.FG1: active_room.layer_fg1.add_child(new_particle)
		Room.Layers.FG2: active_room.layer_fg2.add_child(new_particle)
	new_particle.position = pos
	new_particle._spawn(data)
	return new_particle


static func spawn_particle_cam_synced(name:String, layer:Room.Layers, pos:Vector2, data:Array = []) -> Particle:
	if not UICore.instance:
		return spawn_particle(name, layer, pos, data)
	if active_room == null:
		return null
	if not particle_table.has(name):
		particle_table.append(name)
		particle_cache.append(load("res://Scenes/Particles/%s.tscn" % name))
	var new_particle:Particle = particle_cache[particle_table.find(name)].instantiate()
	match layer:
		Room.Layers.SKY: new_particle.z_index -= 300
		Room.Layers.BG2: new_particle.z_index -= 200
		Room.Layers.BG1: new_particle.z_index -= 100
		Room.Layers.FG1: new_particle.z_index += 100
		Room.Layers.FG2: new_particle.z_index += 200
	new_particle.z_index -= UICore.instance.z_index
	UICore.instance.particle_layer.add_child(new_particle)
	new_particle.position = pos
	new_particle._spawn(data)
	return new_particle


static func clear_cam_synced_particles() -> void:
	if not UICore.instance:
		return
	for particle in UICore.instance.particle_layer.get_children():
		if particle is Particle:
			particle.queue_free()


static func colorize_sprite(spritesheet:Texture2D, _palette:Texture2D, row_id:int) -> Texture2D:
	var color_count := _palette.get_width()
	var palette_image := _palette.get_image()
	var sprite_image := spritesheet.get_image()
	var check_colors:Array = [ ]
	for i in color_count:
		check_colors.append(palette_image.get_pixel(i, 0))
	for y in spritesheet.get_height():
		for x in spritesheet.get_width():
			if spritesheet.is_pixel_opaque(x, y):
				var this_check_color = sprite_image.get_pixel(x, y)
				if check_colors.has(this_check_color):
					var _color_id = check_colors.find(this_check_color)
					var new_color = palette_image.get_pixel(this_check_color, row_id + 1)
					sprite_image.set_pixel(x, y, new_color)
	return ImageTexture.create_from_image(sprite_image)


static func get_color(coords:Vector2i) -> Color:
	return palette.get_pixelv(coords)


static func get_all_children(_node:Node) -> Array[Node]:
	var output:Array[Node] = []
	for child in _node.get_children():
		output.append(child)
		if child.get_child_count() > 0:
			output.append_array(get_all_children(child))
	return output


static func spin_vector2(vector:Vector2, ccw:bool) -> Vector2:
	match vector:
		Vector2.DOWN:
			return Vector2.RIGHT if ccw else Vector2.LEFT
		Vector2.LEFT:
			return Vector2.DOWN if ccw else Vector2.UP
		Vector2.UP:
			return Vector2.LEFT if ccw else Vector2.RIGHT
		Vector2.RIGHT:
			return Vector2.UP if ccw else Vector2.DOWN
	return Vector2.ZERO


static func spin_surface(surface:DirsSurface, ccw:bool) -> DirsSurface:
	match surface:
		DirsSurface.FLOOR:
			return DirsSurface.RWALL if ccw else DirsSurface.LWALL
		DirsSurface.LWALL:
			return DirsSurface.FLOOR if ccw else DirsSurface.CEILING
		DirsSurface.CEILING:
			return DirsSurface.LWALL if ccw else DirsSurface.RWALL
		DirsSurface.RWALL:
			return DirsSurface.CEILING if ccw else DirsSurface.FLOOR
	return DirsSurface.NONE


static func spin_cardinal(cardinal:DirsCardinal, ccw:bool) -> DirsCardinal:
	match cardinal:
		DirsCardinal.DOWN:
			return DirsCardinal.RIGHT if ccw else DirsCardinal.LEFT
		DirsCardinal.LEFT:
			return DirsCardinal.DOWN if ccw else DirsCardinal.UP
		DirsCardinal.UP:
			return DirsCardinal.LEFT if ccw else DirsCardinal.RIGHT
		DirsCardinal.RIGHT:
			return DirsCardinal.UP if ccw else DirsCardinal.DOWN
	return DirsCardinal.NONE
