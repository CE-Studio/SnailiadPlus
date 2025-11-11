class_name PauseLayer
extends Node2D


#region Variables
var subscreen:Subscreen = null

@onready var cam:UICore = UICore.instance
@onready var game:GameCore = GameCore.instance
@onready var menu_scene:PackedScene = preload("res://Scenes/IngameMenuScene.tscn")
@onready var subscreen_scene:PackedScene = preload("res://Scenes/UI/Subscreen.tscn")
#endregion


func _physics_process(_delta: float) -> void:
	if not cam and UICore.instance:
		cam = UICore.instance
	if not game and GameCore.instance:
		game = GameCore.instance
	if SInput.input_just_pressed(SInput.Inputs.PAUSE) and not get_tree().paused:
		pause_fade_in()
		add_child(menu_scene.instantiate())
		cam.minimap.process_mode = Node.PROCESS_MODE_INHERIT
	if SInput.input_just_pressed(SInput.Inputs.MAP) and not get_tree().paused:
		pause_fade_in()
		subscreen = subscreen_scene.instantiate()
		add_child(subscreen)
		subscreen.position = Vector2(0, 240)


func pause_fade_in() -> void:
	get_tree().paused = true
	cam.color_cover.set_new_fade(cam.color_cover.modulate, Color(0.0, 0.0, 0.0, 0.6), 0.25)
	cam.popup_layer.visible = false
	if subscreen:
		subscreen = null


func unpause_fade_out() -> void:
	get_tree().paused = false
	cam.color_cover.set_new_fade(cam.color_cover.modulate, Color(0.0, 0.0, 0.0, 0.0), 0.25)
	cam.popup_layer.visible = true
	cam.set_all_visibility_from_settings()
	cam.minimap.subscreen_mode = false
	cam.minimap.modulate = Color.WHITE
	cam.minimap.tick_minimap(0, false, true)
	cam.cam.set_cam_mode()
	cam.darkness_layer.update_col()
	cam.shake_setting = ProjectSettings.get_setting("game/visuals/screen_shake")
	game.current_room.set_environment_visibility()
