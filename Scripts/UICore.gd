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
		#TODO: add function to update weapon icons
