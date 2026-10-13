# Copyright 2026 CE-Studio: AGPL-3.0-only
extends VBoxContainer


var character:int = 0

@onready var layer:MenuLayer = get_parent()

@export var anim:AnimationPlayer
@export var sfx_select:AudioStreamPlayer
@export var sfx_start:AudioStreamPlayer
@export var char_scroller:ScrollingSnailyButton


func _ready() -> void:
	if not Statics.has_unlock(Statics.Unlocks.CHAR_SEL):
		char_scroller.free_safely.call_deferred()


func on_yes_pressed(_value) -> void:
	layer.can_focus = false
	layer.menu.read_inputs = false
	SInput.read_inputs = false
	var tween:Tween = get_tree().create_tween()
	tween.tween_property(layer.menu.music, "volume_linear", 0.0, 1.5)
	anim.play("fade")
	sfx_select.play()
	sfx_start.play()
	Statics.is_in_boss_rush = true
	Statics.setup_bossrush_profile(character)


func change_to_game_scene() -> void:
	Statics.load_room = Statics.ROOM_PATH % Statics.RUSH_SPAWN[0]
	Statics.load_coords = Vector2(Statics.RUSH_SPAWN[1], Statics.RUSH_SPAWN[2])
	get_tree().change_scene_to_file("uid://ltxtlsrku2k0")


func _on_char_cycled(value:int) -> void:
	character = value
	get_parent().menu.set_selector_anim(value)
