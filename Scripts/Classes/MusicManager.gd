@icon("res://Editor/ico/MusicManager.svg")
class_name MusicManager
extends Node


#region Variables
enum Loops {
	Menu,
	Town1, Town2,
	Carelia,
	Spiralis,
	Abyssus,
	Lirata,
	Shrine,
	Boss1, Boss2, Boss3, Boss4, FinalBoss,
	CreditsIntro, Credits, CreditsAlt,
	Snelk,
	BossRush,
	None = -1
}

const FILES:Dictionary = {
	Loops.Menu: "res://Assets/Sounds/Music/TitleSong.ogg",
	Loops.Town1: "res://Assets/Sounds/Music/SnailTown.ogg",
	Loops.Town2: "res://Assets/Sounds/Music/TestZone.ogg",
	Loops.Carelia: "res://Assets/Sounds/Music/MareCarelia.ogg",
	Loops.Spiralis: "res://Assets/Sounds/Music/SpiralisSilere.ogg",
	Loops.Abyssus: "res://Assets/Sounds/Music/AmastridaAbyssus.ogg",
	Loops.Lirata: "res://Assets/Sounds/Music/LuxLirata.ogg",
	Loops.Shrine: "res://Assets/Sounds/Music/ShrineOfIris.ogg",
	Loops.Boss1: "res://Assets/Sounds/Music/Boss1.ogg",
	Loops.Boss2: "res://Assets/Sounds/Music/Boss2.ogg",
	Loops.Boss3: "res://Assets/Sounds/Music/Boss3.ogg",
	Loops.Boss4: "res://Assets/Sounds/Music/Boss4.ogg",
	Loops.FinalBoss: "res://Assets/Sounds/Music/Boss4b.ogg",
	Loops.CreditsIntro: "res://Assets/Sounds/Music/EndingIntro.ogg",
	Loops.Credits: "res://Assets/Sounds/Music/EndingCredts.ogg",
	Loops.CreditsAlt: "res://Assets/Sounds/Music/EndingCreditsAlt.ogg",
	Loops.Snelk: "res://Assets/Sounds/Music/Snelk.ogg",
	Loops.BossRush: "res://Assets/Sounds/Music/BossRush.ogg",
	Loops.None: "res://Assets/Sounds/Sfx/BossHpBleep.ogg"
}

const GROUPS:Array = [
	[ Loops.Town1, Loops.Town2 ]
]

const GROUP_FADE_TIME_SECONDS:float = 1.0

var active_players:Array = [ ] # Stores AudioStreamPlayers
var active_loops:Array = [ ] # Stores corresponding Loops values
var current_song:Loops = Loops.None
var group_focus:int = 0
var is_group_song:bool = false
var awaiting_load:bool = false
var global_vol_mult:float = 1.0
var global_vol_fade:float = 1.0
var global_vol_fade_spd:float = 1.0
#endregion


func _process(delta: float) -> void:
	if global_vol_mult != global_vol_fade:
		var this_delta = delta * global_vol_fade_spd
		if abs(global_vol_fade - global_vol_mult) < this_delta:
			global_vol_mult = global_vol_fade
		else:
			global_vol_mult += this_delta if (global_vol_fade > global_vol_mult) else -this_delta
	
	if awaiting_load:
		var all_loaded = true
		for loop in active_loops:
			if not ResourceLoader.has_cached(FILES[loop]):
				all_loaded = false
		if all_loaded:
			for player in active_players:
				player.play()
			awaiting_load = false
	elif is_group_song:
		for i in range(active_players.size()):
			var player:AudioStreamPlayer = active_players[i]
			var vol_change:float = GROUP_FADE_TIME_SECONDS * delta
			if i != group_focus:
				vol_change *= -1
			player.volume_linear = clampf(player.volume_linear + vol_change, 0.0, global_vol_mult)
	else:
		for i in range(active_players.size()):
			var player:AudioStreamPlayer = active_players[i]
			player.volume_linear = global_vol_mult


func play_song(loop:Loops) -> void:
	# If new and current songs are the same then just return
	if loop == current_song:
		return
	# If new and current songs are in the same Group:
	#   Set fade out on current song and fade in on new song then return
	# Else if current song is not None
	#   Delete all sources of current song
	var new_song_group = find_song_in_groups(loop)
	if current_song != Loops.None:
		if new_song_group != -1 and new_song_group == find_song_in_groups(current_song):
			group_focus = find_song_id_in_group(loop, new_song_group)
			current_song = loop
			return
		else:
			stop_all(false)
	# If new song is not None
	#   Create sources for new song (and Group if part of one)
	if loop != Loops.None:
		if new_song_group != -1:
			var loop_count = GROUPS[new_song_group].size()
			var active_id = find_song_id_in_group(loop, new_song_group)
			var player_count:int = 0
			for i in GROUPS[new_song_group]:
				var new_player = create_new_player(i)
				active_players.append(new_player)
				active_loops.append(i)
				if i == loop:
					group_focus = player_count
				else:
					new_player.volume_linear = 0
				player_count += 1
			is_group_song = true
			awaiting_load = true
		else:
			var new_player = create_new_player(loop)
			active_players.append(new_player)
			active_loops.append(loop)
			is_group_song = false
			awaiting_load = true
	current_song = loop


func stop_all(reset_current:bool = true) -> void:
	for player in active_players:
		player.queue_free()
	active_players.clear()
	active_loops.clear()
	if reset_current:
		current_song = Loops.None


func find_song_in_groups(loop:Loops) -> int:
	var found_group:int = -1
	for i in range(GROUPS.size()):
		if GROUPS[i].has(loop):
			found_group = i
	return found_group


func find_song_id_in_group(loop:Loops, group:int) -> int:
	if not GROUPS[group].has(loop):
		return -1
	var song_id:int = 0
	for i in range(GROUPS[group].size()):
		if GROUPS[group][i] == loop:
			song_id = i
	return song_id


func create_new_player(loop:Loops) -> AudioStreamPlayer:
	var player = AudioStreamPlayer.new()
	player.stream = load(FILES[loop])
	player.bus = &"Music"
	player.name = str(loop)
	add_child(player)
	return player


func set_global_volume(vol:float) -> void:
	global_vol_mult = vol
	global_vol_fade = vol


func set_fade(fade:float, speed:float = 1.0) -> void:
	global_vol_fade = fade
	global_vol_fade_spd = speed
