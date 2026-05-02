# Copyright 2026 CE-Studio: AGPL-3.0-only
@icon("res://Editor/ico/LimitedSoundHandler.svg")
class_name LimitedSoundHandler
extends Node2D


#region Variables
const MAX_POLYPHONY:int = 2

## Array of the filenames of all active sounds being played through this handler
var active_list:Array[String] = []
## Array of all sources currently playing sounds
var sources:Array[AudioStreamPlayer] = []
#endregion


## Plays a given sound. If the sound is not yet logged under this handler, it will be created
func play_sound(sound:AudioStream, sound_name:String, vol:float = 1.0) -> void:
	if not active_list.has(sound_name):
		add_sound(sound, sound_name)
	var i:int = active_list.find(sound_name)
	sources[i].play()
	sources[i].volume_linear = vol


## Adds a new [AudioStreamPlayer] with a given sound to this handler
func add_sound(sound:AudioStream, sound_name:String) -> void:
	var new_player:AudioStreamPlayer = AudioStreamPlayer.new()
	new_player.stream = sound
	new_player.max_polyphony = MAX_POLYPHONY
	add_child(new_player)
	active_list.append(sound_name)
	sources.append(new_player)
