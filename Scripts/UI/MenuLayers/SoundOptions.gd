extends VBoxContainer


func _ready() -> void:
	$"Master".remote_set_option(ProjectSettings.get_setting("audio/volume/master"))
	$"Sound".remote_set_option(ProjectSettings.get_setting("audio/volume/sound"))
	$"Music".remote_set_option(ProjectSettings.get_setting("audio/volume/music"))


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
