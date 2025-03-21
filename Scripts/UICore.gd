extends Node2D
class_name UICore


var cam:CamControl
var weapon_icons:Array = [ ]
var weapon_icon_states:Array = [ ]


static var instance:UICore


func instantiate() -> void:
	instance = self
	cam = $"Camera2D"
	cam.instantiate()
	
	var icon_id = 0
	for icon in $"WeaponIcons".get_children():
		weapon_icons.append(icon)
		weapon_icon_states.append(0)
		icon.position += Vector2(0, 8)
		icon.action = str(icon_id) + "_off"
		icon_id += 1


func _process(delta: float) -> void:
	# Weapon icons
	var equipped = GameCore.instance.player.selected_weapon
	for i in range(len(weapon_icons)):
		var target_y
		match weapon_icon_states[i]:
			0:
				target_y = 8
			1:
				target_y = -8
			2:
				target_y = -12
		var pos = weapon_icons[i].position
		pos = pos.lerp(Vector2(pos.x, target_y), 10.0 * delta)
		weapon_icons[i].position = pos


func update_weapon_icons() -> void:
	for i in range(len(weapon_icons)):
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
			if weapon_icon_states[i] != 2:
				weapon_icons[i].action = str(i) + "_on"
			weapon_icon_states[i] = 2
		else:
			if weapon_icon_states[i] == 2:
				weapon_icons[i].action = str(i) + "_off"
			weapon_icon_states[i] = 1 if has else 0


func get_cam_center_pos() -> Vector2:
	return position + cam.offset
