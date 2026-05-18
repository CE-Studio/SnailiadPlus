# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
extends Node2D
class_name UICore


#region Variables
const BL_TEXT_ORIGIN:Vector2 = Vector2(3, -12)
const BL_TEXT_OFFSETS:Vector2 = Vector2(0, -8)
const BL_TEXT_OFFSETS_LARGE:Vector2 = Vector2(0, -20)
const MINIMAL_SHAKE_MOD:float = 0.4

var weapon_icons:Array = [ ]
var weapon_icon_states:Array = [ ]

var flashy_popup_scene:PackedScene
var color_popup_scene:PackedScene
var boss_bar:PackedScene

var active_area_label:Node
var active_boss_bar:BossHealthBar

var active_shake_timeline:Array[float] = []
var shake_dir:Vector2 = Vector2.ZERO
var flip_shake_dir:bool = false
var shake_elapsed:float = 0.0
var current_shake_offset:Vector2 = Vector2.ZERO
enum ShakeCallMode {
	APPEND,
	OVERWRITE_ALL,
	OVERWRITE_AXIS
}
enum ShakeSetting {
	OFF,
	MINIMAL,
	ON,
	MIN_NOHUD,
	ON_NOHUD
}
var shake_setting:ShakeSetting = ShakeSetting.ON
var igt_flash_elapsed:float = 0.0
var igt_flash_col:Color = Statics.get_color(Vector2i(2, 4))

static var instance:UICore

@onready var cam:CamControl = $"Camera2D"
@onready var heart_group:Node2D = $"TL/Hearts"
@onready var color_cover:ColorCover = $"ColorCover"
@onready var save_icon:JsonSprite2D = $"BR/SaveIcon"
@onready var bestiary_icon:JsonSprite2D = $"BR/BestiaryIcon"
@onready var minimap:Minimap = $"TR/Minimap"
@onready var border:JsonSprite2D = $"Border"
@onready var darkness_layer:DarknessLayer = $"DarknessLayer"
@onready var popup_layer:Node2D = $"PopupLayer"
@onready var pause_layer:PauseLayer = $"PauseLayer"
@onready var particle_layer:Node2D = $"CamAlignedParticleLayer"
@onready var achievement_core:AchievementCore = $"TL/AchivementPanel"
@onready var weapon_icon_group:Node2D = $"BR/WeaponIcons"
@onready var igt:HBoxContainer = $"BL/InGameTime"
@onready var igt_text:SnailyText = $"BL/InGameTime/Text"
@onready var fps:HBoxContainer = $"BL/Framerate"
@onready var fps_text:SnailyText = $"BL/Framerate/Text"
@onready var input_display:InputDisplay = $"BL/InputDisplay"

@onready var _tl:Node2D = $"TL"
@onready var _tr:Node2D = $"TR"
@onready var _bl:Node2D = $"BL"
@onready var _br:Node2D = $"BR"

@onready var sfx_weapon_0:AudioStreamPlayer = $"BR/WeaponIcons/Sel0"
@onready var sfx_weapon_1:AudioStreamPlayer = $"BR/WeaponIcons/Sel1"
@onready var sfx_weapon_2:AudioStreamPlayer = $"BR/WeaponIcons/Sel2"
@onready var sfx_weapon_3:AudioStreamPlayer = $"BR/WeaponIcons/Sel3"
@onready var sfx_weapon_4:AudioStreamPlayer = $"BR/WeaponIcons/Sel4"
#endregion


func instantiate() -> void:
	instance = self
	cam.instantiate()
	
	var icon_id = 0
	for icon in weapon_icon_group.get_children():
		if icon is JsonSprite2D:
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
	
	set_all_visibility_from_settings.call_deferred()


func _process(delta: float) -> void:
	# Weapon icons
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
	
	# Framerate
	var fps_int = int(Engine.get_frames_per_second())
	var fps_setting = ProjectSettings.get_setting("game/visuals/frame_limit")
	if fps_setting == 0:
		fps_text.set_snaily_text(tr(&"%d FPS") % fps_int)
	else:
		var target_fps:int = 60
		match fps_setting:
			1: target_fps = 30
			2: target_fps = 60
			3: target_fps = 120
		fps_text.set_snaily_text(tr(&"%d/%d FPS") % [ fps_int, target_fps ])
	
	#IGT is counted up in GameCore.gd, being an aspect of the game/profile itself and not purely a HUD element
	if igt.visible:
		if Statics.increment_igt:
			igt_flash_elapsed = 0.0
			igt.modulate = Color.WHITE
		else:
			igt_flash_elapsed += delta
			if ceili(cos(igt_flash_elapsed * 16.0)) == 1:
				igt.modulate = igt_flash_col
			else:
				igt.modulate = Color.WHITE
	
	tick_screen_shake(delta)


func configure_for_aspect_ratio(ratio_id:int) -> void:
	#ProjectSettings.set_setting("display/window/size/viewport_height", Statics.ASPECT_RATIOS[ratio_id].x)
	#ProjectSettings.set_setting("display/window/size/viewport_width", Statics.ASPECT_RATIOS[ratio_id].y)
	#DisplayServer.window_set_size(Statics.ASPECT_RATIOS[ratio_id])
	var offset = Statics.ASPECT_RATIO_OFFSETS[ratio_id] * 0.5
	_tl.position = -offset
	_tr.position = Vector2(400 + offset.x, -offset.y)
	_bl.position = Vector2(-offset.x, 240 + offset.y)
	_br.position = Vector2(400 + offset.x, 240 + offset.y)
	set_border_anim(ratio_id)


func set_all_visibility_from_settings() -> void:
	igt.position = BL_TEXT_ORIGIN
	fps.position = BL_TEXT_ORIGIN
	
	minimap.update_visible_from_settings(minimap.fade_override)
	
	input_display.visible = false
	if ProjectSettings.get_setting("game/ui/keymap"):
		input_display.visible = true
		igt.position.y += BL_TEXT_OFFSETS_LARGE.y
		fps.position.y += BL_TEXT_OFFSETS_LARGE.y
	
	igt.visible = false
	if ProjectSettings.get_setting("game/ui/in_game_time"):
		igt.visible = true
		fps.position.y += BL_TEXT_OFFSETS.y
	
	fps.visible = ProjectSettings.get_setting("game/ui/fps_counter")
	
	weapon_icon_group.visible = ProjectSettings.get_setting("game/ui/bottom_keys")


func update_weapon_icons(play_sound:bool = true) -> void:
	var active_weapons:int = 0
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
			active_weapons += 1
		else:
			if weapon_icon_states[i] == 2:
				weapon_icons[i].action = str(i) + "_off"
			weapon_icon_states[i] = 1 if has else 0
	if play_sound:
		match active_weapons:
			0: sfx_weapon_0.play()
			1: sfx_weapon_1.play()
			2: sfx_weapon_2.play()
			3: sfx_weapon_3.play()
			4: sfx_weapon_4.play()


func draw_new_hearts() -> void:
	for heart in heart_group.get_children():
		heart.reparent(instance)
		heart.queue_free()
	var max_hp = GameCore.instance.player.max_health
	var health_per_heart = Statics.HEALTH_PER_HEART[Statics.current_profile["difficulty"]]
	var running_total = 0
	var heart_count = 0
	var origin = Vector2(8, 8)
	var spacing = Vector2(8, 8)
	var hearts_per_row = 7
	while running_total < max_hp:
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
	#var max_hp = GameCore.instance.player.max_health
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


func get_cam_movement_this_tick() -> Vector2:
	return cam.pos - cam.last_pos


func play_save_anim() -> void:
	save_icon.visible = true
	save_icon.action = "anim"


func play_bestiary_anim() -> void:
	bestiary_icon.visible = true
	bestiary_icon.action = "anim"


func set_border_anim(anim_id:int) -> void:
	border.action = str(anim_id)
	border._process(0.0)


func show_flashy_popup(text:String) -> void:
	var popup_label = flashy_popup_scene.instantiate()
	popup_layer.add_child(popup_label)
	popup_label.instance(text)
	popup_label.position = Vector2i(200, 180)


func show_item_collection_text(item_label:String, is_100:bool) -> void:
	var header_label = flashy_popup_scene.instantiate()
	popup_layer.add_child(header_label)
	header_label.instance(item_label)
	header_label.position = Vector2i(200, 166)
	var percentage_label = flashy_popup_scene.instantiate()
	popup_layer.add_child(percentage_label)
	if is_100:
		percentage_label.instance(tr(&"Item collection 100% complete!!\nFind the Shrine of Iris!!"),
			5.0, 2, 0.2)
	else:
		percentage_label.instance(tr(&"Item collection %.1f%% complete!") %
			Statics.current_profile["item_rate"], 3.0, 1, 0.2)
	percentage_label.position = Vector2i(200, 192)


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
	area_label.instance(GlobalText.areas[Room.areas[area_id]], color_list)
	area_label.position = Vector2i(200, 100)
	active_area_label = area_label
	
	if area_id < 6:
		var text_width = area_label.text.get_width()
		for i in range(2):
			var bookend:JsonSprite2D = JsonSprite2D.new()
			bookend.texture_path = "res://Assets/Images/UI/AreaLabelBorders.json"
			area_label.add_child(bookend)
			bookend.action = ("%d_left" if (i == 0) else "%d_right") % area_id
			bookend.position = Vector2i((text_width * (-0.5 if (i == 0) else 0.5)) + 1, 3)


func clear_area_text() -> void:
	if active_area_label != null:
		active_area_label.queue_free()


func show_boss_bar(boss:Boss, hide_minimap:bool = true) -> BossHealthBar:
	clear_area_text()
	clear_boss_bar()
	active_boss_bar = boss_bar.instantiate()
	popup_layer.add_child(active_boss_bar)
	active_boss_bar.position = Vector2(200, _tl.position.y)
	active_boss_bar.instance(boss)
	if hide_minimap:
		minimap.update_visible_from_settings(0.0, true)
	return active_boss_bar


func clear_boss_bar() -> void:
	if active_boss_bar != null:
		active_boss_bar.queue_free()
	minimap.update_visible_from_settings(1.0)


func call_screen_shake_radial(timeline:Array[float], mode:ShakeCallMode) -> void:
	assert(timeline.size() >= 2, "Screen shake timeline must be at minimum two values in length!")
	shake_dir = Vector2.ZERO
	if mode != ShakeCallMode.OVERWRITE_ALL:
		active_shake_timeline.append_array(timeline)
	else:
		active_shake_timeline = timeline.duplicate()
		shake_elapsed = 0.0


func call_screen_shake_linear(timeline:Array[float], axis:Vector2, mode:ShakeCallMode) -> void:
	assert(timeline.size() >= 2, "Screen shake timeline must be at minimum two values in length!")
	if shake_dir == Vector2.ZERO or mode != ShakeCallMode.APPEND:
		shake_dir = axis
		current_shake_offset = axis
	if mode != ShakeCallMode.OVERWRITE_ALL:
		active_shake_timeline.append_array(timeline)
	else:
		active_shake_timeline = timeline.duplicate()
		shake_elapsed = 0.0


func tick_screen_shake(delta:float) -> void:
	if active_shake_timeline.size() > 0:
		var parse:bool = true
		var start_strength:float = 0.0
		var end_strength:float = 0.0
		var max_time:float = 0.0
		match active_shake_timeline.size():
			1:
				active_shake_timeline.clear()
				parse = false
			2:
				start_strength = active_shake_timeline[0]
				max_time = active_shake_timeline[1]
			_:
				start_strength = active_shake_timeline[0]
				max_time = active_shake_timeline[1]
				end_strength = active_shake_timeline[2]
		if parse:
			var this_strength:float = lerpf(start_strength, end_strength, shake_elapsed / max_time)
			if shake_dir == Vector2.ZERO:
				current_shake_offset = Vector2(
					randf_range(-1.0, 1.0),
					randf_range(-1.0, 1.0)
				).normalized() * this_strength
			else:
				current_shake_offset = shake_dir * this_strength
				if flip_shake_dir:
					shake_dir *= -1
				flip_shake_dir = not flip_shake_dir
			shake_elapsed += delta
			if shake_elapsed >= max_time:
				shake_elapsed -= max_time
				active_shake_timeline.remove_at(0)
				active_shake_timeline.remove_at(0)
				current_shake_offset = Vector2.ZERO
		
		if shake_setting == ShakeSetting.MINIMAL or shake_setting == ShakeSetting.MIN_NOHUD:
			current_shake_offset *= MINIMAL_SHAKE_MOD
		cam.shake_offset = current_shake_offset
		cam.do_screen_shake = shake_setting != ShakeSetting.OFF
		cam.shake_cam_only = shake_setting == ShakeSetting.MINIMAL or shake_setting == ShakeSetting.ON
	else:
		shake_elapsed = 0.0
		shake_dir = Vector2.ZERO
		flip_shake_dir = false


func fade_ui(to_a:float, duration:float) -> void:
	var fade_col:Color = modulate
	fade_col.a = to_a
	var fade:Tween = create_tween()
	fade.tween_property(self, "modulate", fade_col, duration)
