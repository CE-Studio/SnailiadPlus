extends Node2D
class_name UICore


#region Variables
var cam:CamControl
var weapon_icons:Array = [ ]
var weapon_icon_states:Array = [ ]
var heart_group:Node2D
var color_cover:ColorCover
var save_icon:JsonSprite2D

var flashy_popup_scene:PackedScene

static var instance:UICore
#endregion


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
	
	heart_group = $"Hearts"
	draw_new_hearts()
	
	color_cover = $"ColorCover"
	
	save_icon = $"SaveIcon"
	save_icon.visible = false
	
	flashy_popup_scene = preload("res://Scenes/UI/FlashyPopup.tscn")


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


func draw_new_hearts() -> void:
	for heart in heart_group.get_children():
		heart.queue_free()
	var max = GameCore.instance.player.max_health
	var health_per_heart = Statics.HEALTH_PER_HEART[Statics.current_profile["difficulty"]]
	var running_total = 0
	var heart_count = 0
	var origin = Vector2(8, 8)
	var spacing = Vector2(8, 8)
	var hearts_per_row = 7
	while running_total < max:
		var new_heart = JsonSprite2D.new()
		new_heart.texture_path = "res://Assets/Images/UI/Heart.json"
		heart_group.add_child(new_heart)
		var pos = Vector2(origin.x + ((heart_count % hearts_per_row) * spacing.x),
		origin.y + floori((heart_count / hearts_per_row) * spacing.y))
		new_heart.position = pos
		heart_count += 1
		running_total += health_per_heart
	update_hearts()


func update_hearts() -> void:
	var health = GameCore.instance.player.health
	var max = GameCore.instance.player.max_health
	var health_per_heart = Statics.HEALTH_PER_HEART[Statics.current_profile["difficulty"]]
	var running_total = 0
	for heart in heart_group.get_children():
		var this_heart_value = clampi(health - running_total, 0, health_per_heart)
		var anim_name:String
		match int(Statics.current_profile["difficulty"]):
			0: anim_name = "easy_"
			1: anim_name = "normal_"
			2: anim_name = "insane_"
		heart.action = anim_name + str(this_heart_value)
		running_total += health_per_heart


func get_cam_center_pos() -> Vector2:
	return position + cam.offset


func play_save_anim() -> void:
	save_icon.visible = true
	save_icon.action = "anim"


func show_item_collection_text(item_label:String) -> void:
	var header_label = flashy_popup_scene.instantiate()
	add_child(header_label)
	header_label.instance(item_label)
	header_label.position = Vector2i(200, 180)
	var percentage_label = flashy_popup_scene.instantiate()
	add_child(percentage_label)
	percentage_label.instance(Statics.get_text("hud_collectedItemPercentage") % Statics.current_profile["item_rate"], 3.0, 1, 0.2)
	percentage_label.position = Vector2i(200, 200)
