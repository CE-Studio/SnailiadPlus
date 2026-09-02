# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name Subscreen
extends Node2D


#region Variables
const ITEMS_WEAPON:Array = [
	Item.ItemTypes.PEASHOOTER,
	Item.ItemTypes.BOOMERANG,
	Item.ItemTypes.RAINBOW_WAVE,
]
const ITEMS_SHELL:Array = [
	Item.ItemTypes.NONE,
	Item.ItemTypes.ICE_SHELL,
	Item.ItemTypes.GRAVITY_SHELL,
	Item.ItemTypes.METAL_SHELL
]
const ITEMS_ABILITY:Array = [
	Item.ItemTypes.SHELL_SHIELD,
	Item.ItemTypes.HIGH_JUMP,
	Item.ItemTypes.RAPID_FIRE,
	Item.ItemTypes.DEVASTATOR,
	Item.ItemTypes.GRAVITY_SHOCK
]
const SUB_ITEMS:Dictionary = {
	Item.ItemTypes.SECRET_BOOMERANG: Item.ItemTypes.BOOMERANG,
	Item.ItemTypes.DEBUG_WAVE: Item.ItemTypes.RAINBOW_WAVE
}
const GROUP_SELECTION_X_OFFSET:int = 4
const LIST_SPRITE_OFFSET:int = 10
const SELECTOR_LIST_OFFSET:Vector2i = Vector2(-20, -1)
const SELECTOR_SPEED:float = 20.0
const SUBSCREEN_ENTER_SPEED:float = 16.0
const SUBSCREEN_INACTIVE_ACCEL:float = 16.0
const SUBSCREEN_SWITCH_SPEED:float = 3.5
const DESC_PANEL_SPEED:float = 10.0

enum MoveMode {
	NONE = -1,
	LIST,
	NAME,
	MAP,
	GRID
}

var elapsed:float = 0.0
var selection_depth:int = 0
var selectable_items:Array[Array] = [ [], [], [] ] # Formatting: [ SnailySprite2D, Int ]
var selection:Vector2i = Vector2i(0, -1)
var map_focused:bool = true
var selector_target:Vector2i = Vector2.ZERO
var map_sel_origin:Vector2i
var map_selection:Vector2i = Vector2.ZERO
var desc_panel_origin:Vector2
var active:bool = true
var exit_speed:float = 1.0
var player_origin:Vector2
var player_shell:int = 0
var item_origins:Array[Vector2] = []
var selected_item:Sprite2D

var zoomed_map:Node2D = null
var map_zoomed:bool = false

@export var body_map:SnailySprite2D
@export var body_inv:SnailySprite2D
@export var player_icon:SnailySprite2D
@export var header_name:SnailyText
@export var name_box:HBoxContainer
@export var name_text:SnailyText
@export var header_weapon:SnailyText
@export var slist_weapon:Node2D
@export var tlist_weapon:VBoxContainer
@export var header_shell:SnailyText
@export var slist_shell:Node2D
@export var tlist_shell:VBoxContainer
@export var header_ability:SnailyText
@export var slist_ability:Node2D
@export var tlist_ability:VBoxContainer
@export var list_sprite_frames:SpriteFrames
@export var selector:Node2D
@export var selector_spr:SnailySprite2D
@export var sfx_move:AudioStreamPlayer
@export var sfx_select:AudioStreamPlayer
@export var sfx_open:AudioStreamPlayer
@export var sfx_close:AudioStreamPlayer
@export var sfx_switch:AudioStreamPlayer
@export var sel_target_name:Marker2D
@export var map:Minimap
@export var map_selector:SnailySprite2D
@export var desc_panel:PanelContainer
@export var desc_name:SnailyText
@export var desc_body:SnailyText
@export var prompts:Array[Node2D]
@export var prompt_icon_texts:Array[SnailyText]
@export var prompt_desc_texts:Array[SnailyText]
@export var info_text:SnailyText
@export var map_text:SnailyText
@export var time_text:SnailyText
@export var item_text:SnailyText
@export var helix_count:SnailyText
@export var radar:SnailyText
@export var anim:AnimationPlayer
@export var player_sprite:Sprite2D
@export var item_sprites:Array[Sprite2D]
@export var item_target:Marker2D

@onready var zoomed_scn:PackedScene = preload("uid://b1u1t0hvg2fob")
#endregion


func _ready() -> void:
	var this_char = int(Statics.current_profile["character"])
	body_map.play(str(this_char))
	body_inv.play(str(this_char))
	player_icon.play(str(this_char))
	name_text.set_snaily_text(GlobalText.get_player_name(this_char as Player.Players, true))
	player_icon.position.x = name_box.position.x + name_text.get_width()
	selector_target = sel_target_name.position
	selector.position = selector_target
	map_selector.visible = false
	map_sel_origin = Vector2i(map.position) + map.MARKER_ZERO
	map_selection = UICore.instance.minimap.last_player_pos
	_init_item_slots()
	if selectable_items[0].size() == 0: # If weapon list is empty,
		selection.x = 1 # force selection to shell list to select normal shell
	_set_map_prompts()
	_set_inv_prompts()
	_set_desc(-2)
	desc_name.set_snaily_text("")
	desc_body.set_snaily_text("")
	desc_panel_origin = desc_panel.position
	desc_panel.modulate.a = 0.0
	sfx_open.play()
	info_text.set_snaily_text(info_text.text % [
		GlobalText.get_player_name(this_char),
		GlobalText.difficulties[Statics.current_profile["difficulty"] as int]
		])
	map_text.set_snaily_text(map_text.text % Minimap.get_map_rate())
	item_text.set_snaily_text(item_text.text % Statics.get_item_percentage())
	time_text.set_snaily_text(time_text.text % Statics.get_igt_str())
	UICore.instance.heart_group.z_index = 30
	helix_count.set_snaily_text("x " + str(Statics.check_item(Item.ItemTypes.HELIX_FRAGMENT)))
	if Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS4):
		var ratio:Vector2i = Statics.get_area_item_ratio(GameCore.instance.current_area)
		radar.set_snaily_text(tr(&"Area items found: %d/%d") %  [ratio.x, ratio.y])
		if ratio.x == ratio.y and ratio.y != 0:
			radar.set_default_flashy(2)
			radar.enable_rainbow_scroll()
	else:
		radar.visible = false
	_init_item_sprites()


func _process(delta: float) -> void:
	elapsed += delta
	selector_spr.position.x = abs(sin(elapsed * 8)) * -2
	
	if active:
		if map_zoomed and (SInput.check_input(SInput.Inputs.UI_BACK, true)
		or SInput.check_input(SInput.Inputs.PAUSE, true)
		or SInput.check_input(SInput.Inputs.STRAFE, true)):
			sfx_select.play()
			zoomed_map.queue_free()
			map_zoomed = false
			return
		if (selection_depth == 0 and not map_zoomed
		and SInput.check_input(SInput.Inputs.STRAFE, true)):
			sfx_select.play()
			map_zoomed = true
			zoomed_map = zoomed_scn.instantiate()
			add_child(zoomed_map)
			zoomed_map.position = Statics.VECTOR_CENTER
			zoomed_map.init(map)
			return
		
		var close_flag:bool = false
		if selection_depth == 0 and elapsed >= 0.125:
			if (SInput.check_input(SInput.Inputs.MAP, true)
			or SInput.check_input(SInput.Inputs.PAUSE, true)
			or SInput.check_input(SInput.Inputs.UI_BACK, true)):
				close_flag = true
		elif selection_depth == 1:
			if SInput.check_input(SInput.Inputs.PAUSE, true):
				close_flag = true
		if close_flag:
			if zoomed_map:
				zoomed_map.queue_free()
			UICore.instance.pause_layer.unpause_fade_out()
			active = false
			UICore.instance.minimap.update_player()
			UICore.instance.minimap.update_p_marker_layer()
			UICore.instance.heart_group.z_index = 0
			sfx_close.play()
			return
		
		if selection_depth < 0:
			selection_depth = 0
		
		_test_for_move_selection()
		selector.position = selector.position.lerp(selector_target, SELECTOR_SPEED * delta)
		_test_for_selection_events()
		
		position = position.lerp(Vector2.ZERO, SUBSCREEN_ENTER_SPEED * delta)
		
		if selection_depth == 0 and SInput.check_input(SInput.Inputs.SPEAK, true):
			if map_focused:
				anim.play("swap", -1, SUBSCREEN_SWITCH_SPEED)
			else:
				anim.play("swap", -1, -SUBSCREEN_SWITCH_SPEED, true)
			map_focused = not map_focused
			sfx_switch.play()
		
		var panel_y:float = desc_panel_origin.y
		var panel_a:float = 0.0
		if selection_depth == 1 and not map_focused:
			panel_y -= desc_panel.size.y
			panel_a = 1.0
		desc_panel.position.y = lerpf(
			desc_panel.position.y,
			panel_y,
			DESC_PANEL_SPEED * delta
		)
		desc_panel.modulate.a = lerpf(
			desc_panel.modulate.a,
			panel_a,
			DESC_PANEL_SPEED * delta
		)
		
		_update_item_sprites(delta)
	else:
		position.y += exit_speed
		exit_speed *= 1.0 + (SUBSCREEN_INACTIVE_ACCEL * delta)
		if position.y >= 240.0 and not sfx_close.playing:
			queue_free()


## Adds all relevant list items for collected items to their respective categories
func _init_item_slots() -> void:
	var total:int = 0
	var spr_offset:int = 0
	
	for i in ITEMS_WEAPON:
		var count:int = _get_item_count(i)
		if count > 0:
			var spr = _add_list_spr(slist_weapon, i, "weapon", spr_offset)
			_add_list_text(tlist_weapon, GlobalText.get_item_name(i))
			spr_offset += LIST_SPRITE_OFFSET
			_add_item_selectable(spr, i, 0)
		total += count
	if total == 0:
		header_weapon.set_snaily_text(tr(&"?????"))
	
	total = 0
	spr_offset = 0
	for i in ITEMS_SHELL:
		var count:int = _get_item_count(i)
		if count > 0:
			var spr = _add_list_spr(slist_shell, i, "body", spr_offset)
			_add_list_text(tlist_shell, GlobalText.get_item_name(i, true))
			spr_offset += LIST_SPRITE_OFFSET
			_add_item_selectable(spr, i, 1)
		total += count
	if total == 0:
		header_shell.set_snaily_text(tr(&"?????"))
	
	total = 0
	spr_offset = 0
	for i in ITEMS_ABILITY:
		var count:int = _get_item_count(i)
		if count > 0:
			var spr = _add_list_spr(slist_ability, i, "ability", spr_offset)
			_add_list_text(tlist_ability, GlobalText.get_item_name(i))
			spr_offset += LIST_SPRITE_OFFSET
			_add_item_selectable(spr, i, 2)
		total += count
	if total == 0:
		header_ability.set_snaily_text(tr(&"?????"))


## Adds a new separator sprite to a category list
func _add_list_spr(_group:Node2D, _id:int, _action:String, _y:int) -> SnailySprite2D:
	var new_spr:SnailySprite2D = SnailySprite2D.new()
	new_spr.sprite_frames = list_sprite_frames
	_group.add_child(new_spr)
	new_spr.name = str(_id)
	new_spr.play(_action)
	new_spr.position = Vector2(0, _y)
	return new_spr


## Adds a new text entry to a category list
func _add_list_text(_group:VBoxContainer, _text:String) -> void:
	var new_text:SnailyText = SnailyText.new()
	new_text.shadow_scale = 1
	new_text.text_scale = 1
	_group.add_child(new_text)
	new_text.set_snaily_text(_text, true)


## Marks a certain displayed item as able to be selected and read about
func _add_item_selectable(_sprite:SnailySprite2D, _item_id:int, _column:int) -> void:
	selectable_items[_column].append( [ _sprite, _item_id ] )


## Properly registers all item sprites and sets their visibility depending on if you actually
## have the specified item or not
func _init_item_sprites() -> void:
	player_origin = player_sprite.position
	player_shell = Statics.get_shell_level(0 if Statics.stack_shells else 1)
	if Statics.stack_shells and player_shell == 3:
		player_shell = 4
	player_sprite.frame_coords.x = player_shell
	match Player.instance.who_i_is:
		Player.Players.BLOBBY:
			player_sprite.frame_coords.y = 5 if Statics.check_item(Item.ItemTypes.SHELL_SHIELD) else 4
		Player.Players.LEECHY:
			player_sprite.frame_coords.y = 6
		_:
			player_sprite.frame_coords.y = Player.instance.who_i_is as int
	
	for i in range(item_sprites.size()):
		if item_sprites[i] != null:
			item_origins.append(item_sprites[i].position)
			item_sprites[i].visible = _get_item_count(i)
		else:
			item_origins.append(Vector2.ZERO)


## Updates the position and modulate of all item sprites based on if they're selected or not
func _update_item_sprites(delta:float) -> void:
	var weight:float = DESC_PANEL_SPEED * delta
	
	var player_target:Vector2 = player_origin
	var player_color:Color = Color.WHITE
	if selected_item == player_sprite:
		player_target = item_target.position
	elif selected_item != null:
		player_color = Color("3f3f3f")
	player_sprite.position = Vector2(
		lerpf(player_sprite.position.x, player_target.x, weight),
		lerpf(player_sprite.position.y, player_target.y, weight)
	)
	player_sprite.modulate = player_sprite.modulate.lerp(player_color, weight)
	
	for i in item_sprites.size():
		if item_sprites[i] != null and item_sprites[i] != player_sprite:
			var spr:Sprite2D = item_sprites[i]
			var this_target:Vector2 = item_origins[i]
			var item_color:Color = Color.WHITE
			if selected_item == spr:
				this_target = item_target.position
			elif selected_item != null:
				item_color = Color("3f3f3f")
			spr.position = Vector2(
				lerpf(spr.position.x, this_target.x, weight),
				lerpf(spr.position.y, this_target.y, weight)
			)
			spr.modulate = spr.modulate.lerp(item_color, weight)


## Returns the current held count of a given item, with consideration for items of different
## IDs that should be counted together as if they were of one type
func _get_item_count(id:Item.ItemTypes) -> int:
	if id == Item.ItemTypes.NONE:
		return 1
	var count:int = Statics.check_item(id)
	var keys:Array = SUB_ITEMS.keys()
	for i in keys.size():
		if SUB_ITEMS[keys[i]] == id:
			count += Statics.check_item(keys[i])
	return count


## Polls player input and shifts selection based on the current state of the subscreen
func _test_for_move_selection() -> void:
	if map_zoomed:
		return
	
	var move_mode:MoveMode = MoveMode.NONE
	var grid_move:Vector2i = Vector2i.ZERO
	
	if map_focused: # Map panel focused
		if selection_depth == 1:
			if SInput.check_input(SInput.Inputs.LEFT, true):
				grid_move.x -= 1
			if SInput.check_input(SInput.Inputs.RIGHT, true):
				grid_move.x += 1
			if SInput.check_input(SInput.Inputs.UP, true):
				grid_move.y -= 1
			if SInput.check_input(SInput.Inputs.DOWN, true):
				grid_move.y += 1
			if grid_move != Vector2i.ZERO:
				move_mode = MoveMode.GRID
	elif selection_depth == 0: # Inventory panel focused
		if SInput.check_input(SInput.Inputs.DOWN, true):
			selection.y += 1
			move_mode = MoveMode.LIST
			if selection.y >= selectable_items[selection.x].size():
				selection.y = -1
				move_mode = MoveMode.NAME
		if SInput.check_input(SInput.Inputs.UP, true):
			selection.y -= 1
			move_mode = MoveMode.NAME if selection.y == -1 else MoveMode.LIST
			if selection.y < -1:
				selection.y = selectable_items[selection.x].size() - 1
		if SInput.check_input(SInput.Inputs.LEFT, true) and selection.y != -1:
			selection.x -= 1
			if selection.x < 0:
				selection.x = selectable_items.size() - 1
			if selection.y >= selectable_items[selection.x].size():
				selection.y = selectable_items[selection.x].size() - 1
			move_mode = MoveMode.LIST
		if SInput.check_input(SInput.Inputs.RIGHT, true) and selection.y != -1:
			selection.x += 1
			if selection.x >= selectable_items.size():
				selection.x = 0
			if selection.y >= selectable_items[selection.x].size():
				selection.y = selectable_items[selection.x].size() - 1
			move_mode = MoveMode.LIST
	
	if move_mode != MoveMode.NONE:
		sfx_move.play()
	match move_mode:
		MoveMode.LIST:
			selector_target = selectable_items[selection.x][selection.y][0].global_position
			selector_target -= Vector2i(body_inv.global_position)
			selector_target += SELECTOR_LIST_OFFSET
		MoveMode.NAME:
			selector_target = sel_target_name.position
		MoveMode.GRID:
			map_selection += grid_move
			if map_selection.x < 0:
				map_selection.x += Minimap.MAP_SIZE.x
			elif map_selection.x >= Minimap.MAP_SIZE.x:
				map_selection.x -= Minimap.MAP_SIZE.x
			if map_selection.y < 0:
				map_selection.y += Minimap.MAP_SIZE.y
			elif map_selection.y >= Minimap.MAP_SIZE.y:
				map_selection.y -= Minimap.MAP_SIZE.y
			map_selector.position = map_sel_origin + (map_selection * 8)


## Polls player input and determines how to "select" whatever item may be hovered over
func _test_for_selection_events() -> void:
	if map_zoomed:
		return
	
	if map_focused:
		match selection_depth:
			0:
				if SInput.check_input(SInput.Inputs.UI_ACCEPT, true):
					selection_depth += 1
					sfx_select.play()
					map_selector.visible = true
					map_selector.position = map_sel_origin + (map_selection * 8)
					_set_map_prompts()
			1:
				if SInput.check_input(SInput.Inputs.UI_ACCEPT, true):
					sfx_select.play()
					map.update_p_marker_at_cell(map_selection)
				elif SInput.check_input(SInput.Inputs.UI_BACK, true):
					sfx_select.play()
					map_selector.visible = false
					selection_depth -= 1
					_set_map_prompts()
	
	elif SInput.check_input(SInput.Inputs.UI_ACCEPT, true):
		match selection_depth:
			0:
				selection_depth += 1
				sfx_select.play()
				if selection.y == -1:
					_set_desc(-2)
					selected_item = player_sprite
				else:
					var id:int = selectable_items[selection.x][selection.y][1]
					_set_desc(id)
					selected_item = item_sprites[id]
					match id:
						Item.ItemTypes.ICE_SHELL: player_sprite.frame_coords.x = 1
						Item.ItemTypes.GRAVITY_SHELL: player_sprite.frame_coords.x = 2
						Item.ItemTypes.METAL_SHELL: player_sprite.frame_coords.x = 4
						Item.ItemTypes.NONE:
							player_sprite.frame_coords.x = 0
							selected_item = player_sprite
				desc_panel.size.y = 16
				selected_item.z_index = 1
			1:
				selection_depth -= 1
				sfx_select.play()
				selected_item.z_index = 0
				selected_item = null
				player_sprite.frame_coords.x = player_shell


## Updates the displayed icons and text for the prompts on the map panel based on the current
## selection depth
func _set_map_prompts() -> void:
	match selection_depth:
		0:
			prompt_icon_texts[0].set_snaily_text("bind__ui_accept")
			prompt_desc_texts[0].set_snaily_text(tr(&"Set markers"))
			prompt_icon_texts[1].set_snaily_text("bind__strafe")
			prompt_desc_texts[1].set_snaily_text(tr(&"Zoom in"))
			prompts[2].visible = true
			prompt_icon_texts[2].set_snaily_text("bind__speak")
			prompt_desc_texts[2].set_snaily_text(tr(&"Inventory panel"))
		1:
			prompt_icon_texts[0].set_snaily_text("bind__ui_accept")
			prompt_desc_texts[0].set_snaily_text(tr(&"Place/remove marker"))
			prompt_icon_texts[1].set_snaily_text("bind__ui_back")
			prompt_desc_texts[1].set_snaily_text(tr(&"Return"))
			prompts[2].visible = false


## Updates the displayed icons and text for the prompts on the inventory panel based on
## the current selection depth
func _set_inv_prompts() -> void:
	match selection_depth:
		0:
			prompt_icon_texts[3].set_snaily_text("bind__ui_accept")
			prompt_desc_texts[3].set_snaily_text(tr(&"Read more"))
			prompts[4].visible = true
			prompt_icon_texts[4].set_snaily_text("bind__speak")
			prompt_desc_texts[4].set_snaily_text(tr(&"Map panel"))
		1:
			prompt_icon_texts[3].set_snaily_text("bind__ui_accept")
			prompt_desc_texts[3].set_snaily_text(tr(&"Read less"))
			prompts[4].visible = false


## Writes the appropriate item description to the description text node based on the given item ID
func _set_desc(id:int) -> void:
	desc_name.visible = true
	desc_name.set_snaily_text(GlobalText.get_item_name(id, true))
	var player = int(Statics.current_profile["character"])
	var stacked_shells:StringName = tr(&"")
	match id:
		Item.ItemTypes.PEASHOOTER:
			desc_body.set_snaily_text(tr(&"The first line of defense.  This little gun lets you fire small yet hardy peas at any wayward foe.  They're not the most effective projectile, as they crumble from contact with enemy and surface alike, but they'll do in a pinch.\n\n[color=#ffd48c]Hold the SHOOT button to fire"))
		Item.ItemTypes.BOOMERANG:
			desc_body.set_snaily_text(tr(&"These little things can pack quite the punch!  Their points and edges are refined to catch wind and foe alike.  When tossed at just the right distance, the turnaround can score some big damage!\n\n[color=#ffd48c]Hold the SHOOT button to fire"))
		Item.ItemTypes.RAINBOW_WAVE:
			desc_body.set_snaily_text(tr(&"It's said that those who do good in the eyes of Iris are offered a small piece of her power.  Lucky you!!  These sharpened shards of solid light can cut through just about any shell, wall, or particularly stubborn slime.\n\n[color=#ffd48c]Hold the SHOOT button to fire"))
		Item.ItemTypes.DEVASTATOR:
			desc_body.set_snaily_text(tr(&"An old relic of unknown origin, said to empower the bearer with strength and fury to rival even the highest of gods.  At least, I think so; I may have slept through that class.  Regardless, having this on you powers up all of your attacks!  Neat, huh?"))
		Item.ItemTypes.HIGH_JUMP:
			if player == Player.Players.BLOBBY:
				desc_body.set_snaily_text(tr(&"Contained inside is a ready-made meal crafted specifically to help make a blob's body stickier.  It's mostly hard candy.\n\n[color=#ffd48c]Hold toward a wall or ceiling while airborne to stick to it"))
			else:
				desc_body.set_snaily_text(tr(&"This badge is actually a container full of nothing but helium, allowing its wearer a little more air time when they jump!  Mind the fall, though; it doesn't cushion the landing very well.\n\n[color=#ffd48c]Hold the JUMP button to jump as high as possible"))
		Item.ItemTypes.SHELL_SHIELD:
			if player == Player.Players.BLOBBY:
				desc_body.set_snaily_text(tr(&"Now isn't this a nice find!!  This helmet looks just about sturdy enough to shrug off a hit or two and come out unscathed.  Plus, it's fashionable!!\n\n[color=#ffd48c]Press toward the ground to hide under it"))
			else:
				desc_body.set_snaily_text(tr(&"With a little polish and protective slime, your shell is now capable of withstanding damage!  From the lightest graze to the heaviest blow, nothing is too much to handle anymore! Just remember to buff out the scratches when you get home.\n\n[color=#ffd48c]Press toward the ground to hide in your shell"))
		Item.ItemTypes.RAPID_FIRE:
			if player == Player.Players.LEECHY:
				desc_body.set_snaily_text(tr(&"subscreen_desc_backfire"))
			else:
				desc_body.set_snaily_text(tr(&"An intriguing little gizmo.  Even just holding it makes you feel more nimble and energetic!  Throwing attacks at anything in your way should be much easier and quicker now."))
		Item.ItemTypes.ICE_SHELL:
			desc_body.set_snaily_text(tr(&"This frosty relic grants its bearer a close attunement with the cold, preventing harm from any icy adversaries and environmental hazards.  It also gives your body a lovely shine!"))
		Item.ItemTypes.GRAVITY_SHELL:
			match player:
				Player.Players.UPSIDE: desc_body.set_snaily_text(tr(&"subscreen_desc_magneticFoot"))
				Player.Players.LEGGY: desc_body.set_snaily_text(tr(&"subscreen_desc_corkscrewJump"))
				Player.Players.BLOBBY: desc_body.set_snaily_text(tr(&"subscreen_desc_angelJump"))
				_: desc_body.set_snaily_text(tr(&"This badge is crafted from discarded gravity turtle scutes that still carry some of that innate control over the force.  With a little concentration, wearing this badge allows the wearer to redirect which direction they get pulled.\n\n[color=#ffd48c]Hold a direction and press the GRAVITY button to flip gravity"))
			if Statics.stack_shells:
				desc_body.set_snaily_text(desc_body.text + stacked_shells)
		Item.ItemTypes.METAL_SHELL:
			desc_body.set_snaily_text(tr(&"A special shield that binds perfectly to anyone who wields it, protecting them from all manner of harm.  It even includes perfect thermal shielding, letting you finally step outside in the summer!"))
			if Statics.stack_shells:
				desc_body.set_snaily_text(desc_body.text + stacked_shells)
		Item.ItemTypes.GRAVITY_SHOCK:
			desc_body.set_snaily_text(tr(&"A relic long thought lost, this item allows anyone to build immense amounts of power and release it in a fireball of sheer destruction.  Use this power wisely, and you can bring down even the toughest of foes and walls with ease!\n\n[color=#ffd48c]Gravity jump toward your current gravity to activate.  Move while it's active to steer.  Jump while active to cancel early."))
		Item.ItemTypes.NONE:
			desc_body.set_snaily_text(tr(&"It's you!\nThis is your normal self; how you've always known yourself.  You can take a hit or two, but not much else.  But hey, that just means there's room to grow!"))
			if Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS4):
				desc_body.set_snaily_text(tr(&"Despite everything, it's still you.\nYou've come far.  You've changed.  Even through it all, though, it's never a bad idea to remember who you are at your core.  Self love is important, you know!"))
		-2:
			desc_name.visible = false
			match player:
				Player.Players.SNAILY:
					desc_body.set_snaily_text(tr(&"Snaily is just your average slow-going, grass-eating, fun-loving snail.  There isn't much to separate them from the rest of Snail Town, except for one thing: their drive.  Snail Town picked them for this adventure knowing Snaily wouldn't shy away and would get it done, no matter how hard it would be.\n\nSnaily has the strength to climb up and around walls and ceilings, and sports a shell to hide in when things get scary."))
				Player.Players.SLUGGY:
					desc_body.set_snaily_text(tr(&"Slugs are not an uncommon sight around these parts, but none are quite as daring and adventurous as Sluggy!  Sluggy doesn't let the threat of danger scare them away from the adventure of a lifetime, and believes Snail Town's faith is not misplaced with them.\n\nSluggy's lack of a shell makes them take a little more damage, but also move faster and jump higher than a snail."))
				Player.Players.UPSIDE:
					desc_body.set_snaily_text(tr(&"Upside-Down Snail is a quirky one.  They're part gravity snail, but never quite figured out how exactly to get ahold of this power.  As such, they're locked to always falling upward.  They've gotten used to it, though, as have all their friends in town.\n\nIn spite of their strange gravity, Upside can climb walls and floors like any other snail, and their shell is just as good for hiding."))
				Player.Players.LEGGY:
					desc_body.set_snaily_text(tr(&"Leggy hasn't always lived in Snail Town, but it didn't take very long at all for him to be accepted into it.  After all, he's slow and has a shell!  When he's not tracking down missing snails, Leggy likes to relax and write music.\n\nLeggy is slower and can't climb walls, but he can flip his gravity off the bat."))
				Player.Players.BLOBBY:
					desc_body.set_snaily_text(tr(&"Not every blob is out to get you!!  Blobby is one such blob who has made peace with Snail Town, and has set out on this adventure to help its friends however it can.  Even if that means fighting off other blobs.\n\nBlobby can't climb walls, instead leaning into its innate talent for jumping and the unique abilities that come with being a blob."))
				Player.Players.LEECHY:
					desc_body.set_snaily_text(tr(&"Leechy desc"))
		_:
			desc_name.set_snaily_text("")
			desc_body.set_snaily_text("")
