# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name PauseLayer
extends Node2D


#region Variables
var subscreen:Subscreen = null

## If [code]true[/code], will prevent any menu from being opened even when the player has control.
static var suppress_menuing:bool = false

@onready var cam:UICore = UICore.instance
@onready var game:GameCore = GameCore.instance
@onready var menu_scene:PackedScene = preload("uid://c84sydpddlb6c")
@onready var subscreen_scene:PackedScene = preload("uid://dpfe8sxp7e2kp")
@onready var debug_scene:PackedScene = preload("uid://c0jv4y1p8dri1")
#endregion


func _physics_process(_delta: float) -> void:
	if not cam and UICore.instance:
		cam = UICore.instance
	if not game and GameCore.instance:
		game = GameCore.instance
	if suppress_menuing:
		return
	if SInput.input_just_pressed(SInput.Inputs.PAUSE) and not get_tree().paused:
		pause_fade_in()
		add_child(menu_scene.instantiate())
		cam.minimap.process_mode = Node.PROCESS_MODE_INHERIT
	if SInput.input_just_pressed(SInput.Inputs.MAP) and not get_tree().paused:
		pause_fade_in()
		subscreen = subscreen_scene.instantiate()
		add_child(subscreen)
		subscreen.position = Vector2(0, 240)
	if SInput.input_just_pressed(SInput.Inputs.DEBUG) and not get_tree().paused:
		pause_no_fade()
		add_child(debug_scene.instantiate())


func pause_fade_in() -> void:
	get_tree().paused = true
	cam.color_cover.set_new_fade(cam.color_cover.modulate, Color(0.0, 0.0, 0.0, 0.6), 0.25)
	cam.popup_layer.visible = false
	if subscreen:
		subscreen = null


func pause_no_fade() -> void:
	get_tree().paused = true
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
	game.current_room.update_hint_layer_visibility()
