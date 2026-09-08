# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
extends Node2D
class_name UICore


#region Variables
const BL_TEXT_ORIGIN:Vector2 = Vector2(3, -12)
const BL_TEXT_OFFSETS:Vector2 = Vector2(0, -8)
const BL_TEXT_OFFSETS_LARGE:Vector2 = Vector2(0, -20)
const MINIMAL_SHAKE_MOD:float = 0.4

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

@export var _tl:Node2D
@export var _tr:Node2D
@export var _bl:Node2D
@export var _br:Node2D

@export var cam:CamControl
@export var heart_group:HeartGroup
@export var color_cover:ColorCover
@export var save_icon:SnailySprite2D
@export var bestiary_icon:SnailySprite2D
@export var minimap:Minimap
@export var border:Sprite2D
@export var darkness_layer:DarknessLayer
@export var popup_layer:Node2D
@export var pause_layer:PauseLayer
@export var particle_layer:Node2D
@export var achievement_core:AchievementCore
@export var weapon_icons:WeaponIcons
@export var igt:HBoxContainer
@export var igt_text:SnailyText
@export var fps:HBoxContainer
@export var fps_text:SnailyText
@export var input_display:InputDisplay
@export var area_bookend_frames:SpriteFrames
#endregion


func instantiate() -> void:
	instance = self
	cam.instantiate()
	
	heart_group.draw_new_hearts()
	
	save_icon.visible = false
	bestiary_icon.visible = false
	
	flashy_popup_scene = load("uid://cst46kauimrf6")
	color_popup_scene = load("uid://b84helaw7ial0")
	boss_bar = load("uid://d0moh826bija1")
	
	set_all_visibility_from_settings.call_deferred()


func _process(delta: float) -> void:
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
	
	if Statics.invincibility:
		heart_group.modulate.a = 0.25
	else:
		heart_group.modulate.a = 1.0


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
	
	weapon_icons.visible = ProjectSettings.get_setting("game/ui/bottom_keys")


func get_cam_center_pos() -> Vector2:
	return position + cam.offset


func get_cam_movement_this_tick() -> Vector2:
	return cam.pos - cam.last_pos


func play_save_anim() -> void:
	save_icon.visible = true
	save_icon.play("default")


func play_bestiary_anim() -> void:
	bestiary_icon.visible = true
	bestiary_icon.play("default")


func set_border_anim(anim_id:int) -> void:
	border.frame = anim_id


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
	var area_label:ColorPopup = color_popup_scene.instantiate()
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
			var bookend:SnailySprite2D = SnailySprite2D.new()
			bookend.sprite_frames = area_bookend_frames
			area_label.add_child(bookend)
			bookend.play(("%d_left" if (i == 0) else "%d_right") % area_id)
			bookend.position = Vector2i(roundi(text_width * (-0.5 if (i == 0) else 0.5)) + 1, 3)
		
		if Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS4):
			var collection_label:ColorPopup = color_popup_scene.instantiate()
			area_label.add_child(collection_label)
			var ratio:Vector2i = Statics.get_area_item_ratio(area_id)
			var ratio_text:String = tr(&"Items found: %d/%d") % [ratio.x, ratio.y]
			collection_label.instance(ratio_text, [Color.WHITE], 4.5, 1, 0.4)
			collection_label.position.y += 20


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
