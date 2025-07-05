extends Node2D


#region Variables
@onready var cam:UICore = UICore.instance
@onready var menu_scene:PackedScene = preload("res://Scenes/IngameMenuScene.tscn")
#endregion


func _physics_process(delta: float) -> void:
	if not cam:
		cam = UICore.instance
	if Input.is_action_just_pressed("Pause") and not get_tree().paused:
		pause_fade_in()
		add_child(menu_scene.instantiate())


func pause_fade_in() -> void:
	get_tree().paused = true
	cam.color_cover.set_new_fade(cam.color_cover.modulate, Color(0.0, 0.0, 0.0, 0.4), 0.25)


func unpause_fade_out() -> void:
	get_tree().paused = false
	cam.color_cover.set_new_fade(cam.color_cover.modulate, Color(0.0, 0.0, 0.0, 0.0), 0.25)
