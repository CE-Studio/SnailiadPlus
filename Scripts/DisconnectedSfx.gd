extends AudioStreamPlayer


func load_and_play(sound:AudioStream) -> void:
	stream = sound
	play()


func _on_sound_finished() -> void:
	queue_free()
