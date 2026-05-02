# Copyright 2026 CE-Studio: AGPL-3.0-only
@icon("res://Editor/ico/MusicManager.svg")
class_name MusicManager
extends Node


#region Variables
enum Loops {
	MENU,
	TOWN1, TOWN2,
	CARELIA,
	SPIRALIS,
	ABYSSUS,
	LIRATA,
	SHRINE,
	BOSS1, BOSS2, BOSS3, BOSS4, FINAL_BOSS,
	CREDITS_INTRO, CREDITS, CREDITS_ALT,
	SNELK,
	BOSS_RUSH,
	NONE = -1
}

const FILES:Dictionary = {
	Loops.MENU: "res://Assets/Sounds/Music/TitleSong.ogg",
	Loops.TOWN1: "res://Assets/Sounds/Music/SnailTown.ogg",
	Loops.TOWN2: "res://Assets/Sounds/Music/TestZone.ogg",
	Loops.CARELIA: "res://Assets/Sounds/Music/MareCarelia.ogg",
	Loops.SPIRALIS: "res://Assets/Sounds/Music/SpiralisSilere.ogg",
	Loops.ABYSSUS: "res://Assets/Sounds/Music/AmastridaAbyssus.ogg",
	Loops.LIRATA: "res://Assets/Sounds/Music/LuxLirata.ogg",
	Loops.SHRINE: "res://Assets/Sounds/Music/ShrineOfIris.ogg",
	Loops.BOSS1: "res://Assets/Sounds/Music/Boss1.ogg",
	Loops.BOSS2: "res://Assets/Sounds/Music/Boss2.ogg",
	Loops.BOSS3: "res://Assets/Sounds/Music/Boss3.ogg",
	Loops.BOSS4: "res://Assets/Sounds/Music/Boss4.ogg",
	Loops.FINAL_BOSS: "res://Assets/Sounds/Music/Boss4b.ogg",
	Loops.CREDITS_INTRO: "res://Assets/Sounds/Music/EndingIntro.ogg",
	Loops.CREDITS: "res://Assets/Sounds/Music/EndingCredts.ogg",
	Loops.CREDITS_ALT: "res://Assets/Sounds/Music/EndingCreditsAlt.ogg",
	Loops.SNELK: "res://Assets/Sounds/Music/Snelk.ogg",
	Loops.BOSS_RUSH: "res://Assets/Sounds/Music/BossRush.ogg",
	Loops.NONE: "res://Assets/Sounds/Sfx/BossHpBleep.ogg"
}

## Used to group certain loops together so that they can play simultaneously and fade in/out.
## Mainly used for area theme variations
const GROUPS:Array = [
	[ Loops.TOWN1, Loops.TOWN2 ]
]

## The time in seconds it takes to fade between loops in a group
const GROUP_FADE_TIME_SECONDS:float = 1.0

## Array storing the [AudioStreamPlayer] nodes that actually play music
var active_players:Array = [ ]
## Array storing the corresponding loop values for each [AudioStreamPlayer]
var active_loops:Array = [ ]
## Tracks which song is currently being played
var current_song:Loops = Loops.NONE
## Tracks which song out of a group is being played
var group_focus:int = 0
## Will be set if the currently active loop is part of a group
var is_group_song:bool = false
## Will be set if not all loops have finished loading into memory yet
var awaiting_load:bool = false
## Multiplier applied to the volume of all [AudioStreamPlayer] nodes
var global_vol_mult:float = 1.0
## Target value to fade the volume multiplier toward
var global_vol_fade:float = 1.0
## The rate at which the volume multiplier is faded
var global_vol_fade_spd:float = 1.0
## The time in seconds that the volume fade is set to delay
var global_vol_fade_delay:float = 0.0
#endregion


func _process(delta: float) -> void:
	if global_vol_mult != global_vol_fade:
		if global_vol_fade_delay > 0:
			global_vol_fade_delay -= delta
		elif global_vol_mult != global_vol_fade:
			var this_delta = delta * global_vol_fade_spd
			global_vol_mult = move_toward(global_vol_mult, global_vol_fade, this_delta)
	
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


## Plays the specified loop
func play_song(loop:Loops) -> void:
	# If new and current songs are the same then just return
	if loop == current_song:
		return
	# If new and current songs are in the same Group:
	#   Set fade out on current song and fade in on new song then return
	# Else if current song is not None
	#   Delete all sources of current song
	var new_song_group = find_song_in_groups(loop)
	if current_song != Loops.NONE:
		if new_song_group != -1 and new_song_group == find_song_in_groups(current_song):
			group_focus = find_song_id_in_group(loop, new_song_group)
			current_song = loop
			return
		stop_all(false)
	# If new song is not None
	#   Create sources for new song (and Group if part of one)
	if loop != Loops.NONE:
		if new_song_group != -1:
			#var loop_count = GROUPS[new_song_group].size()
			#var active_id = find_song_id_in_group(loop, new_song_group)
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


## Stops all active loops
func stop_all(reset_current:bool = true) -> void:
	for player in active_players:
		player.queue_free()
	active_players.clear()
	active_loops.clear()
	if reset_current:
		current_song = Loops.NONE


## Takes the specified loop and locates which group it happens to exist in
func find_song_in_groups(loop:Loops) -> int:
	var found_group:int = -1
	for i in range(GROUPS.size()):
		if GROUPS[i].has(loop):
			found_group = i
	return found_group


## Takes the specified loop and group and returns the indext at which the loop appears in the group
func find_song_id_in_group(loop:Loops, group:int) -> int:
	if not GROUPS[group].has(loop):
		return -1
	var song_id:int = 0
	for i in range(GROUPS[group].size()):
		if GROUPS[group][i] == loop:
			song_id = i
	return song_id


## Creates a new [AudioStreamPlayer] with the specified loop
func create_new_player(loop:Loops) -> AudioStreamPlayer:
	var player = AudioStreamPlayer.new()
	player.stream = load(FILES[loop])
	player.bus = &"Music"
	player.name = str(loop)
	add_child(player)
	return player


## Sets the volume multiplier with no fade
func set_global_volume(vol:float) -> void:
	global_vol_mult = vol
	global_vol_fade = vol


## Sets a target for the volume multiplier to fade toward and a speed at which to fade
func set_fade(fade:float, speed:float = 1.0, delay:float = 0.0) -> void:
	global_vol_fade = fade
	global_vol_fade_spd = speed
	global_vol_fade_delay = delay
