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

var active_players:Array = [ ]
var current_song:Loops = Loops.None
var group_focus:int = 0
var is_group_song:bool = false
#endregion


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
			pass # Replace with fade code
		else:
			for player in active_players:
				player.queue_free()
	# If new song is not None
	#   Create sources for new song (and Group if part of one)
	if loop != Loops.None:
		if new_song_group != -1:
			pass
		else:
			var new_player = create_new_player(loop)
			active_players.append(new_player)
			new_player.play()


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
	var player = AudioStreamPlayer.new() #TODO get AudioStreamPlayers spawning
	player.stream = load(FILES[loop])
	player.bus = &"Music"
	player.name = str(loop)
	add_child(player)
	return player
