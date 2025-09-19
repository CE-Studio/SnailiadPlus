class_name PauseLayer
extends Node2D


#region Variables
var subscreen:Subscreen = null

@onready var cam:UICore = UICore.instance
@onready var menu_scene:PackedScene = preload("res://Scenes/IngameMenuScene.tscn")
@onready var subscreen_scene:PackedScene = preload("res://Scenes/UI/Subscreen.tscn")
#endregion


func _physics_process(_delta: float) -> void:
	if not cam:
		cam = UICore.instance
	if SInput.input_just_pressed(SInput.Inputs.PAUSE) and not get_tree().paused:
		pause_fade_in()
		add_child(menu_scene.instantiate())
		cam.minimap.process_mode = Node.PROCESS_MODE_INHERIT
	if SInput.input_just_pressed(SInput.Inputs.MAP) and not get_tree().paused:
		pause_fade_in(true)
		subscreen = subscreen_scene.instantiate()
		add_child(subscreen)
		subscreen.position = Vector2(0, 240)
		cam.minimap.subscreen_mode = true
		cam.minimap.process_mode = Node.PROCESS_MODE_ALWAYS
		if not cam.minimap.visible:
			cam.minimap.update_visible(1)


func pause_fade_in(bottom_cover:bool = false) -> void:
	get_tree().paused = true
	if bottom_cover:
		cam.color_cover_bottom.set_new_fade(cam.color_cover_bottom.modulate, Color(0.0, 0.0, 0.0, 0.6), 0.25)
	else:
		cam.color_cover_top.set_new_fade(cam.color_cover_top.modulate, Color(0.0, 0.0, 0.0, 0.6), 0.25)
	cam.popup_layer.visible = false
	if subscreen:
		subscreen = null


func unpause_fade_out() -> void:
	get_tree().paused = false
	cam.color_cover_top.set_new_fade(cam.color_cover_top.modulate, Color(0.0, 0.0, 0.0, 0.0), 0.25)
	cam.color_cover_bottom.set_new_fade(cam.color_cover_bottom.modulate, Color(0.0, 0.0, 0.0, 0.0), 0.25)
	cam.popup_layer.visible = true
	cam.set_all_visibility_from_settings()
	cam.minimap.subscreen_mode = false
	cam.minimap.modulate = Color.WHITE
