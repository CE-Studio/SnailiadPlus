# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name WeaponIcons
extends Node2D


#region Variables
const LERP_RATE:float = 10.0

var icon_states:Array[int] = []
var first_update:bool = true

@export var icons:Array[SnailySprite2D] = []
@export var sfx:Array[AudioStreamPlayer] = []
#endregion


func _ready() -> void:
	for icon in icons:
		icon_states.append(0)
		icon.position += Vector2(0, 8)


func _process(delta: float) -> void:
	for i in range(len(icons)):
		var target_y
		match icon_states[i]:
			0:
				target_y = 8
			1:
				target_y = -8
			2:
				target_y = -12
		var pos = icons[i].position
		pos = pos.lerp(Vector2(pos.x, target_y), LERP_RATE * delta)
		icons[i].position = pos


func update(play_sound:bool = true) -> void:
	var active_weapons:int = 0
	for i in range(len(icons)):
		var has:bool = false
		var equipped:bool = false
		match i:
			0:
				has = Statics.check_item(Item.ItemTypes.BROOM)
				equipped = Statics.player.selected_weapon & 1 > 0
			1:
				has = Statics.check_item(Item.ItemTypes.PEASHOOTER)
				equipped = Statics.player.selected_weapon & 2 > 0
			2:
				has = (Statics.check_item(Item.ItemTypes.BOOMERANG) or
				Statics.check_item(Item.ItemTypes.SECRET_BOOMERANG))
				equipped = Statics.player.selected_weapon & 4 > 0
			3:
				has = (Statics.check_item(Item.ItemTypes.RAINBOW_WAVE) or
				Statics.check_item(Item.ItemTypes.DEBUG_WAVE))
				equipped = Statics.player.selected_weapon & 8 > 0
		if equipped:
			if icon_states[i] != 2:
				icons[i].play("selected")
			icon_states[i] = 2
			active_weapons += 1
		else:
			if icon_states[i] == 2:
				icons[i].play("default")
			icon_states[i] = 1 if has else 0
		icons[i].visible = icon_states[i] != 0
	if play_sound:
		sfx[active_weapons].play()
	if first_update:
		_process(1.0 / LERP_RATE)
		first_update = false
