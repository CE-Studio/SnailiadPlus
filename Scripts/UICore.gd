extends Node2D
class_name UICore


#region Variables
var weapon_icons:Array = [ ]
var weapon_icon_states:Array = [ ]

var flashy_popup_scene:PackedScene
var color_popup_scene:PackedScene
var boss_bar:PackedScene

var active_area_label:Node
var active_boss_bar:BossHealthBar

static var instance:UICore

@onready var cam:CamControl = $"Camera2D"
@onready var heart_group:Node2D = $"TL/Hearts"
@onready var color_cover:ColorCover = $"ColorCover"
@onready var save_icon:JsonSprite2D = $"BR/SaveIcon"
@onready var bestiary_icon:JsonSprite2D = $"BR/BestiaryIcon"
@onready var minimap:Minimap = $"TR/Minimap"
@onready var border:JsonSprite2D = $"Border"
@onready var popup_layer:Node2D = $"PopupLayer"
@onready var pause_layer:Node2D = $"PauseLayer"
@onready var achievement_core:AchievementCore = $"TL/AchivementPanel"

@onready var tl:Node2D = $"TL"
@onready var tr:Node2D = $"TR"
@onready var bl:Node2D = $"BL"
@onready var br:Node2D = $"BR"
#endregion


func instantiate() -> void:
	instance = self
	cam.instantiate()
	
	var icon_id = 0
	for icon in $"BR/WeaponIcons".get_children():
		weapon_icons.append(icon)
		weapon_icon_states.append(0)
		icon.position += Vector2(0, 8)
		icon.action = str(icon_id) + "_off"
		icon_id += 1
	
	draw_new_hearts()
	
	save_icon.visible = false
	bestiary_icon.visible = false
	
	flashy_popup_scene = preload("res://Scenes/UI/FlashyPopup.tscn")
	color_popup_scene = preload("res://Scenes/UI/ColorPopup.tscn")
	boss_bar = preload("res://Scenes/UI/BossHealthBar.tscn")


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


func configure_for_aspect_ratio(ratio_id:int) -> void:
	var offset = Statics.ASPECT_RATIO_OFFSETS[ratio_id] * 0.5
	tl.position = -offset
	tr.position = Vector2(400 + offset.x, -offset.y)
	bl.position = Vector2(-offset.x, 240 + offset.y)
	br.position = Vector2(400 + offset.x, 240 + offset.y)
	set_border_anim(ratio_id)


func set_all_visibility_from_settings() -> void:
	pass # TODO figure out the bottom keys thing


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
		heart.reparent(instance)
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
	call_deferred("update_hearts")


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


func play_bestiary_anim() -> void:
	bestiary_icon.visible = true
	bestiary_icon.action = "anim"


func set_border_anim(anim_id:int) -> void:
	border.action = str(anim_id)
	border._process(0.0)


func show_item_collection_text(item_label:String) -> void:
	var header_label = flashy_popup_scene.instantiate()
	popup_layer.add_child(header_label)
	header_label.instance(item_label)
	header_label.position = Vector2i(200, 180)
	var percentage_label = flashy_popup_scene.instantiate()
	popup_layer.add_child(percentage_label)
	percentage_label.instance(Statics.get_text("hud_collectedItemPercentage") % Statics.current_profile["item_rate"], 3.0, 1, 0.2)
	percentage_label.position = Vector2i(200, 200)


func show_area_text(area_id:int) -> void:
	clear_area_text()
	var area_label = color_popup_scene.instantiate()
	popup_layer.add_child(area_label)
	var area_color:Color = Color.WHITE
	match area_id:
		0: area_color = Statics.get_color(Vector2i(2, 5))
		1: area_color = Statics.get_color(Vector2i(2, 8))
		2: area_color = Statics.get_color(Vector2i(3, 10))
		3: area_color = Statics.get_color(Vector2i(2, 3))
		4: area_color = Statics.get_color(Vector2i(0, 1))
		5: area_color = Statics.get_color(Vector2i(3, 11))
	var color_list:Array[Color] = [ Color.WHITE, area_color, Color.WHITE, area_color, Color.WHITE, area_color, Color.WHITE ]
	area_label.instance(Statics.get_text("area_%s" % Room.areas[area_id]), color_list)
	area_label.position = Vector2i(200, 100)
	active_area_label = area_label
	
	if area_id < 6:
		var text_width = area_label.text.get_width()
		for i in range(2):
			var border:JsonSprite2D = JsonSprite2D.new()
			border.texture_path = "res://Assets/Images/UI/AreaLabelBorders.json"
			area_label.add_child(border)
			border.action = ("%d_left" if (i == 0) else "%d_right") % area_id
			border.position = Vector2i((text_width * (-0.5 if (i == 0) else 0.5)) + 1, 3)


func clear_area_text() -> void:
	if active_area_label != null:
		active_area_label.queue_free()


func show_boss_bar(boss:Boss, hide_minimap:bool = true) -> BossHealthBar:
	clear_area_text()
	clear_boss_bar()
	active_boss_bar = boss_bar.instantiate()
	popup_layer.add_child(active_boss_bar)
	active_boss_bar.position = Vector2(200, tl.position.y)
	active_boss_bar.boss = boss
	minimap.update_visible(0.0, true)
	return active_boss_bar


func clear_boss_bar() -> void:
	if active_boss_bar != null:
		active_boss_bar.queue_free()
	minimap.update_visible(1.0)
