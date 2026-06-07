# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name DialogueSkipTrigger
extends Trigger


func _on_player_enter(_body:Node2D) -> void:
	if active and CutsceneController.running:
		CutsceneController.remote_skip_line()
		super(_body)
