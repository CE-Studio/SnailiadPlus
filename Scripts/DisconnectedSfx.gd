extends AudioStreamPlayer


func load_and_play(sound:AudioStream, vol:float = 1.0) -> void:
	stream = sound
	volume_linear = vol
	play()


func _on_sound_finished() -> void:
	queue_free()
