@icon("res://Editor/ico/LimitedSoundHandler.svg")
class_name LimitedSoundHandler
extends Node2D


#region Variables
const MAX_POLYPHONY:int = 2

var active_list:Array[String] = []
var sources:Array[AudioStreamPlayer] = []
#endregion


func play_sound(sound:AudioStream, sound_name:String, vol:float = 1.0) -> void:
	if not active_list.has(sound_name):
		add_sound(sound, sound_name)
	var i:int = active_list.find(sound_name)
	sources[i].play()
	sources[i].volume_linear = vol


func add_sound(sound:AudioStream, sound_name:String) -> void:
	var new_player:AudioStreamPlayer = AudioStreamPlayer.new()
	new_player.stream = sound
	new_player.max_polyphony = MAX_POLYPHONY
	add_child(new_player)
	active_list.append(sound_name)
	sources.append(new_player)
