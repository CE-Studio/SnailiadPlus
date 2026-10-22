# Copyright 2026 CE-Studio: AGPL-3.0-only
extends VBoxContainer


@export var master:ScrollingSnailyButton
@export var sound:ScrollingSnailyButton
@export var music:ScrollingSnailyButton
@export var dialogue:ScrollingSnailyButton


func _ready() -> void:
	master.remote_set_option(ProjectSettings.get_setting("audio/volume/master"))
	sound.remote_set_option(ProjectSettings.get_setting("audio/volume/sound"))
	music.remote_set_option(ProjectSettings.get_setting("audio/volume/music"))
	dialogue.remote_set_option(ProjectSettings.get_setting("audio/volume/dialogue"))


func _on_master_cycled(value:int) -> void:
	var index = AudioServer.get_bus_index("Master")
	var vol = float(value) / 20.0
	AudioServer.set_bus_volume_db(index, linear_to_db(vol))
	ProjectSettings.set_setting("audio/volume/master", value)


func _on_sound_cycled(value:int) -> void:
	var index = AudioServer.get_bus_index("Sfx")
	var vol = float(value) / 20.0
	AudioServer.set_bus_volume_db(index, linear_to_db(vol))
	ProjectSettings.set_setting("audio/volume/sound", value)


func _on_music_cycled(value:int) -> void:
	var index = AudioServer.get_bus_index("Music")
	var vol = float(value) / 20.0
	AudioServer.set_bus_volume_db(index, linear_to_db(vol))
	ProjectSettings.set_setting("audio/volume/music", value)


func _on_dialogue_cycled(value:int) -> void:
	var index = AudioServer.get_bus_index("Dialogue")
	var vol = float(value) / 20.0
	AudioServer.set_bus_volume_db(index, linear_to_db(vol))
	ProjectSettings.set_setting("audio/volume/dialogue", value)
