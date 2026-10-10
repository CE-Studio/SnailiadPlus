# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name RushEnding
extends Node2D


#region Variables
const CENTER:Vector2 = Vector2(200, 120)
const SUN_GRAVITY:float = 1200.0
const PARTICLE_COUNT:int = 64
const PARTICLE_MAX_DIST:float = 256.0
const PARTICLE_DURATION:Vector2 = Vector2(2.5, 4.0)
const POLYGON_VERTICES:int = 12
const ITEM_SPRITESHEET:Texture2D = preload("uid://14gb1mahe3ip")
const ITEM_SPACING:float = 32.0 + 4.0
const ITEM_START_Y:float = 112.0
const ITEM_RAISE_TIME:float = 1.2
const ITEM_RAISE_EASE:float = 0.2
const ITEM_RAISE_DELAY:float = 0.15

var sun_pos:Vector2 = Vector2.ZERO
var tick_sun:bool = false
var sun_velocity:Vector2 = Vector2(-110.0, -300.0)
var item_sprites:Array[SnailySprite2D] = []
var item_elapsed:float = 0.0
var increment_elapsed:bool = false
var time_id:String = ""
var total_time:Array[float] = []
var is_new_best_time:bool = false
var old_lowest_inv:Array = []
var final_inv:Array = []
var is_new_lowest_items:bool = false
var is_tied_items:bool = false
var save_tied_items:bool = false
var elapsed:float = -PI

@export var anim:AnimationPlayer
@export var sun:SnailySprite2D
@export var poly_outer:Polygon2D
@export var poly_inner:Polygon2D
@export var header:SnailyText
@export var bullet_header:SnailyText
@export var bullet_stats:SnailyText
@export var time_header:SnailyText
@export var time_stats:SnailyText
@export var misc_header:SnailyText
@export var misc_stats:SnailyText
@export var item_group:Node2D
@export var new_best_time:SnailyText
@export var new_lowest_items:SnailyText
@export var continue_prompt:SnailyText
@export var sfx_continue:AudioStreamPlayer
@export var sfx_select:AudioStreamPlayer
#endregion


func _ready() -> void:
	var radius:Vector2 = Vector2(16.0, 0.0)
	var rot_amount:float = TAU / POLYGON_VERTICES
	var vertices:Array[Vector2] = []
	for i in range(POLYGON_VERTICES):
		vertices.append(radius)
		radius = radius.rotated(rot_amount)
	poly_outer.polygon = vertices
	poly_inner.polygon = vertices
	PlayerBullet.bullet_a = 1.0
	header.enable_rainbow_scroll()
	_setup_bullet_stats()
	_setup_time_stats()
	_setup_misc_stats()
	_setup_items()
	_tick_items()
	var char_name:String = GlobalText.characters[Player.instance.who_i_is][0]
	#region New best time
	new_best_time.set_snaily_text(tr(&"New best for %s!!") % char_name)
	new_best_time.enable_rainbow_scroll()
	time_id = Statics.infer_time_id()
	if Statics.has_time(time_id) and Statics.compare_times(Statics.get_time(time_id), total_time) > 0:
		is_new_best_time = true
	else:
		new_best_time.visible = false
	#endregion
	#region New lowest items
	new_lowest_items.set_snaily_text(tr(&"New lowest for %s!!") % char_name)
	new_lowest_items.enable_rainbow_scroll()
	var this_inv:Array = Statics.current_profile["inventory"].duplicate()
	var this_count:int = 0
	for item in this_inv:
		if item is int:
			this_count += item
	old_lowest_inv = Statics.data_records["lowest_rush_inv"][Player.instance.who_i_is].duplicate()
	if not old_lowest_inv.is_empty():
		var old_count:int = 0
		for item in old_lowest_inv:
			if item is int:
				old_count += item
		if this_count < old_count:
			is_new_lowest_items = true
		else:
			new_lowest_items.visible = false
			if this_count == old_count:
				is_tied_items = true
	#endregion


func _process(delta: float) -> void:
	if tick_sun:
		sun.position += sun_velocity * delta
		sun_velocity.y += SUN_GRAVITY * delta
	if increment_elapsed:
		item_elapsed += delta
		_tick_items()
	if not anim.is_playing():
		elapsed += delta * 4.0
		continue_prompt.modulate.a = cos(elapsed) + 1.0
		if SInput.check_any_button():
			sfx_continue.play()
			_try_save_from_any_press()


func set_sun_pos(pos:Vector2) -> void:
	sun_pos = pos
	sun.position = pos


func fade_ui_out() -> void:
	UICore.instance.fade_ui(0.0, 2.5)


func kill_sun() -> void:
	tick_sun = true
	for i in range(PARTICLE_COUNT):
		var new_burst_dot:Particle = Statics.spawn_particle("BurstDot", Room.Layers.FG2,
			sun_pos, [ randf_range(PARTICLE_DURATION.x, PARTICLE_DURATION.y),
				randf() * PARTICLE_MAX_DIST, CENTER ])
		match randi() % 5:
			0: new_burst_dot.sprite.modulate = Statics.get_color(Vector2i(3, 4))
			1: new_burst_dot.sprite.modulate = Statics.get_color(Vector2i(3, 9))
			2: new_burst_dot.sprite.modulate = Statics.get_color(Vector2i(2, 7))
			3: new_burst_dot.sprite.modulate = Statics.get_color(Vector2i(0, 1))
			4: new_burst_dot.sprite.modulate = Statics.get_color(Vector2i(2, 2))


func _setup_bullet_stats() -> void:
	if not RushManager.instance:
		bullet_stats.set_snaily_text(tr(&"Not sure how, but no RushManager was present during this run. Stats have not been saved."))
		return
	var manager:RushManager = RushManager.instance
	var out_str:String = ""
	var spr_pos:Vector2 = Vector2(-8.0, 6.0)
	for i in range(manager.bullets.size()):
		if manager.bullets[i] > 0:
			out_str += str(manager.bullets[i]) + "\n"
			match i:
				0:
					_add_item_sprite(spr_pos, 0)
				1:
					_add_item_sprite(spr_pos, 1)
				2:
					_add_item_sprite(spr_pos + (Vector2.LEFT * 9.0), 0)
					_add_item_sprite(spr_pos, 1)
				3:
					_add_item_sprite(spr_pos, 2)
				4:
					_add_item_sprite(spr_pos + (Vector2.LEFT * 9.0), 0)
					_add_item_sprite(spr_pos, 2)
				5:
					_add_item_sprite(spr_pos + (Vector2.LEFT * 9.0), 1)
					_add_item_sprite(spr_pos, 2)
				6:
					_add_item_sprite(spr_pos + (Vector2.LEFT * 18.0), 0)
					_add_item_sprite(spr_pos + (Vector2.LEFT * 9.0), 1)
					_add_item_sprite(spr_pos, 2)
				7:
					_add_item_sprite(spr_pos, 10)
			spr_pos.y += 10.0
	bullet_stats.set_snaily_text(out_str)


func _add_item_sprite(pos:Vector2, id:int) -> void:
	var new_spr:Sprite2D = Sprite2D.new()
	bullet_stats.add_child(new_spr)
	new_spr.position = pos
	new_spr.texture = ITEM_SPRITESHEET
	new_spr.hframes = ITEM_SPRITESHEET.get_width() / 8
	new_spr.vframes = ITEM_SPRITESHEET.get_height() / 8
	new_spr.frame = id


func _setup_time_stats() -> void:
	if not RushManager.instance:
		time_stats.set_snaily_text(tr(&"Not sure how, but no RushManager was present during this run. Stats have not been saved."))
		return
	var manager:RushManager = RushManager.instance
	var out_str:String = ""
	var total_seconds:float = 0.0
	for i in range(manager.boss_time.size()):
		var boss_name:String = ""
		match i:
			0: boss_name = tr(&"Super Shellbreaker")
			1: boss_name = tr(&"Vis Vires")
			2: boss_name = tr(&"Time Cube")
			3: boss_name = tr(&"Sun Snail")
			4: boss_name = tr(&"Giga Sun Snail")
		var minutes:int = 0
		var seconds:float = manager.boss_time[i]
		total_seconds += seconds
		while seconds >= 60.0:
			seconds -= 60.0
			minutes += 1
		out_str += "%s: %02d:%05.2f\n" % [boss_name, minutes, seconds]
	var total_minutes:int = 0
	while total_seconds >= 60.0:
		total_seconds -= 60.0
		total_minutes += 1
	out_str += "%s: %02d:%05.2f" % [tr(&"Total"), total_minutes, total_seconds]
	total_time = [0.0, total_minutes, total_seconds]
	time_stats.set_snaily_text(out_str)


func _setup_misc_stats() -> void:
	if not RushManager.instance:
		misc_stats.set_snaily_text(tr(&"Not sure how, but no RushManager was present during this run. Stats have not been saved."))
		return
	var manager:RushManager = RushManager.instance
	misc_stats.set_snaily_text(
		tr(&"Damage dealt: ") + str(manager.damage) + "\n" +
		tr(&"Times shielded: ") + str(manager.shields) + "\n" +
		tr(&"Times parried: ") + str(manager.parries) + "\n" +
		tr(&"Health lost: ") + str(manager.health_lost) + "\n" +
		tr(&"Health regained: ") + str(manager.health_gained) + "\n"
	)


func _setup_items() -> void:
	var item_start:Vector2 = Vector2.ZERO
	var children:Array = item_group.get_children()
	for i in range(children.size()):
		if children[i] and children[i] is SnailySprite2D:
			var spr:SnailySprite2D = children[i]
			var has:bool = false
			match i:
				0: has = Statics.check_item(Item.ItemTypes.PEASHOOTER)
				1: has = Statics.check_item(Item.ItemTypes.BOOMERANG)
				2: has = Statics.check_item(Item.ItemTypes.RAINBOW_WAVE)
				3: has = Statics.check_item(Item.ItemTypes.DEVASTATOR)
				4: has = Statics.check_item(Item.ItemTypes.RAPID_FIRE)
				5: has = Statics.check_item(Item.ItemTypes.SHELL_SHIELD)
				6: has = Statics.check_item(Item.ItemTypes.HIGH_JUMP)
				7: has = Statics.check_item(Item.ItemTypes.GRAVITY_SHELL)
				8: has = Statics.check_item(Item.ItemTypes.GRAVITY_SHOCK)
				9: has = Statics.check_item(Item.ItemTypes.HEART_CONTAINER) >= 1
				10: has = Statics.check_item(Item.ItemTypes.HEART_CONTAINER) >= 2
			if has:
				item_sprites.append(spr)
				spr.position = item_start
				item_start.x += ITEM_SPACING
				var player:Player.Players = Player.instance.who_i_is
				match i:
					4:
						if player == Player.Players.LEECHY: spr.play("backfire")
					5:
						if player == Player.Players.BLOBBY: spr.play("shelmet")
					6:
						if player == Player.Players.BLOBBY: spr.play("wall_grab")
					7:
						if player == Player.Players.UPSIDE: spr.play("magnetic_foot")
						if player == Player.Players.LEGGY: spr.play("corkscrew_jump")
						if player == Player.Players.BLOBBY: spr.play("angel_jump")
			else:
				spr.queue_free()
	item_group.position.x = 200.0 - ((item_start.x - ITEM_SPACING) * 0.5)


func _tick_items() -> void:
	for i in range(item_sprites.size()):
		var this_delay:float = ITEM_RAISE_DELAY * i
		var this_weight:float = inverse_lerp(0.0, ITEM_RAISE_TIME, item_elapsed - this_delay)
		this_weight = ease(this_weight, ITEM_RAISE_EASE)
		item_sprites[i].position.y = lerpf(ITEM_START_Y, 0.0, this_weight)


func start_ticking_items() -> void:
	increment_elapsed = true


func save_time() -> bool:
	if is_new_best_time or not Statics.has_time(time_id):
		Statics.save_time(time_id, total_time)
		return true
	return false


func save_inv() -> bool:
	if is_new_lowest_items or old_lowest_inv.is_empty() or (is_tied_items and save_tied_items):
		Statics.data_records["lowest_rush_inv"][Player.instance.who_i_is] = final_inv.duplicate()
		return true
	return false


func _try_save_from_any_press() -> void:
	if is_tied_items and not save_tied_items:
		pass
	else:
		if save_time() or save_inv():
			Statics.save_records()
