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

enum MoveMode {
	NONE = -1,
	LIST,
	NAME,
	MAP,
	GRID
}

var group_start_x:float = 0.0
var elapsed:float = 0.0
var selection_depth:int = -1
var selectable_items:Array = [] # Formatting: [ JsonSprite2D, Int ]
var selection:int = -1
var list_focused:bool = false
var map_focused:bool = true
var selector_target:Vector2i = Vector2.ZERO
var map_sel_origin:Vector2i
var map_selection:Vector2i = Vector2.ZERO
var active:bool = true
var exit_speed:float = 1.0

var zoomed_map:Node2D = null
var map_zoomed:bool = false

@export var separators:Array[JsonSprite2D] = []
@export var body:JsonSprite2D
@export var player_icon:JsonSprite2D
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
@export var selector:Node2D
@export var selector_spr:JsonSprite2D
@export_file("*.json") var list_spr_json:String
@export var sfx_move:AudioStreamPlayer
@export var sfx_select:AudioStreamPlayer
@export var sfx_open:AudioStreamPlayer
@export var sfx_close:AudioStreamPlayer
@export var sel_target_name:Marker2D
@export var sel_target_map:Marker2D
#@export var map_target:Marker2D
@export var map:Minimap
@export var map_selector:JsonSprite2D
@export var desc_name:SnailyText
@export var desc_body:SnailyText
@export var marker_text:SnailyText
@export var select_text:SnailyText
@export var map_text:SnailyText
@export var time_text:SnailyText
@export var item_text:SnailyText

@onready var text_scn:PackedScene = preload("res://Scenes/internals/SnailyText.tscn")
@onready var zoomed_scn:PackedScene = preload("res://Scenes/UI/MinimapZoomed.tscn")
#endregion


func _ready() -> void:
	group_start_x = separators[0].position.x
	var this_char = int(Statics.current_profile["character"])
	body.action = str(this_char)
	player_icon.action = str(this_char)
	name_text.set_snaily_text(GlobalText.get_player_name(this_char as Player.Players, true))
	player_icon.position.x = name_box.position.x + name_text.get_width()
	for sep in separators:
		sep.action = "anim"
	selector_spr.action = "anim"
	selector_target = sel_target_map.position
	selector.position = selector_target
	map_selector.visible = false
	map_sel_origin = Vector2i(map.position) + map.MARKER_ZERO
	map_selection = UICore.instance.minimap.last_player_pos
	_init_item_slots()
	desc_name.set_snaily_text("")
	desc_body.set_snaily_text("")
	sfx_open.play()
	map_text.set_snaily_text(map_text.text % Minimap.get_map_rate())
	item_text.set_snaily_text(item_text.text % Statics.get_item_percentage())
	time_text.set_snaily_text(time_text.text % Statics.get_igt_str())


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
		
		var close_flag:bool = false
		if selection_depth == 0:
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
			sfx_close.play()
			return
		
		if selection_depth < 0:
			selection_depth = 0
		
		_test_for_move_selection()
		selector.position = selector.position.lerp(selector_target, SELECTOR_SPEED * delta)
		_test_for_selection_events()
		
		position = position.lerp(Vector2.ZERO, SUBSCREEN_ENTER_SPEED * delta)
		
		if not map_zoomed and SInput.check_input(SInput.Inputs.STRAFE, true):
			sfx_select.play()
			map_zoomed = true
			zoomed_map = zoomed_scn.instantiate()
			add_child(zoomed_map)
			zoomed_map.position = Vector2(200, 120)
			zoomed_map.init(map)
	else:
		position.y += exit_speed
		exit_speed *= 1.0 + (SUBSCREEN_INACTIVE_ACCEL * delta)
		if position.y >= 240.0 and not sfx_close.playing:
			queue_free()


func _init_item_slots() -> void:
	var total:int = 0
	var spr_offset:int = 0
	
	for i in ITEMS_WEAPON:
		var count:int = _get_item_count(i)
		if count > 0:
			var spr = _add_list_spr(slist_weapon, i, "weapon", spr_offset)
			_add_list_text(tlist_weapon, GlobalText.get_item_name(i))
			spr_offset += LIST_SPRITE_OFFSET
			_add_item_selectable(spr, i)
		total += count
	if total == 0:
		header_weapon.set_snaily_text(tr(&"?????"))
	
	total = 0
	spr_offset = 0
	for i in ITEMS_SHELL:
		var count:int = _get_item_count(i)
		if count > 0:
			var spr = _add_list_spr(slist_shell, i, "shell", spr_offset)
			_add_list_text(tlist_shell, GlobalText.get_item_name(i, true))
			spr_offset += LIST_SPRITE_OFFSET
			_add_item_selectable(spr, i)
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
			_add_item_selectable(spr, i)
		total += count
	if total == 0:
		header_ability.set_snaily_text(tr(&"?????"))


func _add_list_spr(_group:Node2D, _id:int, _action:String, _y:int) -> JsonSprite2D:
	var new_spr:JsonSprite2D = JsonSprite2D.new()
	new_spr.texture_path = list_spr_json
	_group.add_child(new_spr)
	new_spr.name = str(_id)
	new_spr.action = _action
	new_spr.position = Vector2(0, _y)
	return new_spr


func _add_list_text(_group:VBoxContainer, _text:String) -> void:
	var new_text = text_scn.instantiate()
	new_text.shadow_scale = 1
	new_text.text_scale = 1
	_group.add_child(new_text)
	new_text.set_snaily_text(_text)


func _add_item_selectable(_sprite:JsonSprite2D, _item_id:int) -> void:
	selectable_items.append( [ _sprite, _item_id ] )


func _get_item_count(id:Item.ItemTypes) -> int:
	if id == Item.ItemTypes.NONE:
		return 1
	var count:int = Statics.check_item(id)
	var keys:Array = SUB_ITEMS.keys()
	for i in keys.size():
		if SUB_ITEMS[keys[i]] == id:
			count += Statics.check_item(keys[i])
	return count


func _test_for_move_selection() -> void:
	if map_zoomed:
		return
	
	var move_mode:MoveMode = MoveMode.NONE
	var grid_move:Vector2i = Vector2i.ZERO
	
	if list_focused:
		if SInput.check_input(SInput.Inputs.RIGHT, true):
			list_focused = false
			map_focused = true
			move_mode = MoveMode.MAP
		elif SInput.check_input(SInput.Inputs.DOWN, true):
			selection += 1
			move_mode = MoveMode.LIST
			if selection >= selectable_items.size():
				selection = -1
				move_mode = MoveMode.NAME
		elif SInput.check_input(SInput.Inputs.UP, true):
			selection -= 1
			if selection == -1:
				move_mode = MoveMode.NAME
			else:
				if selection < -1:
					selection = selectable_items.size() - 1
				move_mode = MoveMode.LIST
	elif map_focused:
		if selection_depth == 0:
			if SInput.check_input(SInput.Inputs.LEFT, true):
				list_focused = true
				map_focused = false
				move_mode = MoveMode.NAME if selection == -1 else MoveMode.LIST
		elif selection_depth == 1:
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
	
	if move_mode != MoveMode.NONE:
		sfx_move.play()
	match move_mode:
		MoveMode.LIST:
			selector_target = selectable_items[selection][0].global_position
			selector_target -= Vector2i(UICore.instance.global_position)
			selector_target += SELECTOR_LIST_OFFSET
			_set_desc(selectable_items[selection][1])
			map.modulate = Color(0.3, 0.3, 0.3)
			marker_text.visible = false
			select_text.set_snaily_text(tr(&"Scroll selection - bind__UP bind__DOWN"))
		MoveMode.NAME:
			selector_target = sel_target_name.position
			_set_desc(-2)
			map.modulate = Color(0.3, 0.3, 0.3)
			marker_text.visible = false
			select_text.set_snaily_text(tr(&"Scroll selection - bind__UP bind__DOWN"))
		MoveMode.MAP:
			selector_target = sel_target_map.position
			desc_name.set_snaily_text("")
			desc_body.set_snaily_text("")
			map.modulate = Color.WHITE
			marker_text.visible = true
			select_text.set_snaily_text(tr(&"Swap selection - bind__LEFT bind__RIGHT"))
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


func _test_for_selection_events() -> void:
	if map_zoomed:
		return
	
	match selection_depth:
		0:
			if map_focused and SInput.check_input(SInput.Inputs.UI_ACCEPT, true):
				selection_depth += 1
				sfx_select.play()
				map_selector.visible = true
				map_selector.action = "8"
				map_selector.position = map_sel_origin + (map_selection * 8)
				marker_text.set_snaily_text(tr(&"Place/remove marker - bind__UI_ACCEPT"))
				select_text.set_snaily_text(tr(&"Return - bind__UI_BACK"))
		1:
			if map_focused:
				if SInput.check_input(SInput.Inputs.UI_ACCEPT, true):
					sfx_select.play()
					map.update_p_marker_at_cell(map_selection)
				elif SInput.check_input(SInput.Inputs.UI_BACK, true):
					sfx_select.play()
					map_selector.action = "8_disable"
					selection_depth -= 1
					marker_text.set_snaily_text(tr(&"Set markers - bind__UI_ACCEPT   Zoom - bind__STRAFE"))
					select_text.set_snaily_text(tr(&"Swap selection - bind__LEFT bind__RIGHT"))


func _set_desc(id:int) -> void:
	desc_name.visible = true
	desc_name.set_snaily_text(GlobalText.get_item_name(id, true))
	var player = int(Statics.current_profile["character"])
	var stacked_shells:StringName = tr(&"")
	match id:
		Item.ItemTypes.PEASHOOTER:
			desc_body.set_snaily_text(tr(&"The first line of defense.  This little gun lets you fire small yet hardy peas at any wayward foe.  They're not the most effective projectile, as they crumble from contact with enemy and surface alike, but they'll do in a pinch.\n\n\n[color=#ffd48c]Hold the SHOOT button to fire"))
		Item.ItemTypes.BOOMERANG:
			desc_body.set_snaily_text(tr(&"These little things can pack quite the punch!  Their points and edges are refined to catch wind and foe alike.  When tossed at just the right distance, the turnaround can score some big damage!\n\n\n[color=#ffd48c]Hold the SHOOT button to fire"))
		Item.ItemTypes.RAINBOW_WAVE:
			desc_body.set_snaily_text(tr(&"It's said that those who do good in the eyes of Iris are offered a small piece of her power.  Lucky you!! These sharpened shards of solid light can cut through just about any shell, wall, or particularly stubborn slime.\n\n\n[color=#ffd48c]Hold the SHOOT button to fire"))
		Item.ItemTypes.DEVASTATOR:
			desc_body.set_snaily_text(tr(&"An old relic of unknown origin, said to empower the bearer with strength and fury to rival even the highest of gods.  At least, I think so; I may have slept through that class.  Regardless, having this on you powers up all of your attacks!  Neat, huh?"))
		Item.ItemTypes.HIGH_JUMP:
			if player == Player.Players.BLOBBY:
				desc_body.set_snaily_text(tr(&"Contained inside is a ready-made meal crafted specifically to help make a blob's body stickier.  It's mostly hard candy.\n\n\n[color=#ffd48c]Hold toward a wall or ceiling while airborne to stick to it"))
			else:
				desc_body.set_snaily_text(tr(&"This badge is actually a container full of nothing but helium, allowing its wearer a little more air time when they jump!  Mind the fall, though; it doesn't cushion the landing very well.\n\n\n[color=#ffd48c]Hold the JUMP button to jump as high as possible"))
		Item.ItemTypes.SHELL_SHIELD:
			if player == Player.Players.BLOBBY:
				desc_body.set_snaily_text(tr(&"Now isn't this a nice find!!  This helmet looks just about sturdy enough to shrug off a hit or two and come out unscathed.  Plus, it's fashionable!!\n\n\n[color=#ffd48c]Press toward the ground to hide under it"))
			else:
				desc_body.set_snaily_text(tr(&"With a little polish and protective slime, your shell is now capable of withstanding damage!  From the lightest graze to the heaviest blow, nothing is too much to handle anymore! Just remember to buff out the scratches when you get home.\n\n\n[color=#ffd48c]Press toward the ground to hide in your shell"))
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
				_: desc_body.set_snaily_text(tr(&"This badge is crafted from discarded gravity turtle scutes that still carry some of that innate control over the force.  With a little concentration, wearing this badge allows the wearer to redirect which direction they get pulled.\n\n\n[color=#ffd48c]Hold a direction and press the GRAVITY button to flip gravity"))
			if Statics.stack_shells:
				desc_body.set_snaily_text(desc_body.text + stacked_shells)
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
