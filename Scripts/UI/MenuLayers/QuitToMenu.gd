extends VBoxContainer


@onready var layer:MenuLayer = get_parent()


func _on_quit_save(_value) -> void:
	layer.menu.save_profile(Statics.current_profile_id)
	_on_quit_no_save(_value)


func _on_quit_no_save(_value) -> void:
	layer.can_focus = false
	layer.menu.read_inputs = false
	layer.menu.color_cover.set_new_fade(Color(0.0, 0.0, 0.0, 0.0), Color.BLACK, 1.0)
	GameCore.instance.music_manager.set_fade(0.0)
	$"Timer".start()

func _on_timer_timeout() -> void:
	get_tree().change_scene_to_file("res://Scenes/MenuScene.tscn")
