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
const GROUP_SELECTION_X_OFFSET:float = 4.0
const LIST_SPRITE_OFFSET:float = 10.0
const SELECTOR_LIST_OFFSET:Vector2 = Vector2(-20.0, -1.0)
const SELECTOR_SPEED:float = 20.0
const SUBSCREEN_ENTER_SPEED:float = 16.0
const SUBSCREEN_INACTIVE_ACCEL:float = 16.0

enum MoveMode {
	NONE = -1,
	LIST,
	NAME,
	MAP
}

var group_start_x:float = 0.0
var elapsed:float = 0.0
var selection_depth:int = -1
var selectable_items:Array = [] # Formatting: [ JsonSprite2D, Int ]
var selection:int = -1
var list_focused:bool = false
var selector_target:Vector2 = Vector2.ZERO
var active:bool = true
var exit_speed:float = 1.0

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
@export var sel_target_name:Marker2D
@export var sel_target_map:Marker2D
@export var map_target:Marker2D
@export var desc_name:SnailyText
@export var desc_body:SnailyText

@onready var text_scn:PackedScene = preload("res://Scenes/internals/SnailyText.tscn")
#endregion


func _ready() -> void:
	group_start_x = separators[0].position.x
	var this_char = int(Statics.current_profile["character"])
	body.action = str(this_char)
	player_icon.action = str(this_char)
	name_text.set_snaily_text("char_full_%d" % this_char)
	player_icon.position.x = name_box.position.x + name_text.get_width()
	for sep in separators:
		sep.action = "anim"
	selector_spr.action = "anim"
	selector_target = sel_target_map.position
	selector.position = selector_target
	_init_item_slots()
	desc_name.set_snaily_text_raw("")
	desc_body.set_snaily_text_raw("")


func _process(delta: float) -> void:
	elapsed += delta
	selector_spr.position.x = abs(sin(elapsed * 8)) * -2
	
	if active:
		if selection_depth == 0:
			if SInput.check_input(SInput.Inputs.MAP, true) or SInput.check_input(SInput.Inputs.PAUSE, true):
				UICore.instance.pause_layer.unpause_fade_out()
				active = false
		
		if selection_depth < 0:
			selection_depth = 0
		
		_test_for_move_selection()
		selector.position = selector.position.lerp(selector_target, SELECTOR_SPEED * delta)
		
		position = position.lerp(Vector2.ZERO, SUBSCREEN_ENTER_SPEED * delta)
	else:
		position.y += exit_speed
		exit_speed *= 1.0 + (SUBSCREEN_INACTIVE_ACCEL * delta)
		if position.y >= 240.0:
			queue_free()


func _init_item_slots() -> void:
	var total:int = 0
	var spr_offset:int = 0
	
	for i in ITEMS_WEAPON:
		var count:int = _get_item_count(i)
		if count > 0:
			var spr = _add_list_spr(slist_weapon, i, "weapon", spr_offset)
			_add_list_text(tlist_weapon, Item.get_name_str_from_id(i))
			spr_offset += LIST_SPRITE_OFFSET
			_add_item_selectable(spr, i)
		total += count
	if total == 0:
		header_weapon.set_snaily_text("subscreen_header_unknown")
	
	total = 0
	spr_offset = 0
	for i in ITEMS_SHELL:
		var count:int = _get_item_count(i)
		if count > 0:
			var spr = _add_list_spr(slist_shell, i, "shell", spr_offset)
			_add_list_text(tlist_shell, Item.get_name_str_from_id(i, true))
			spr_offset += LIST_SPRITE_OFFSET
			_add_item_selectable(spr, i)
		total += count
	if total == 0:
		header_shell.set_snaily_text("subscreen_header_unknown")
	
	total = 0
	spr_offset = 0
	for i in ITEMS_ABILITY:
		var count:int = _get_item_count(i)
		if count > 0:
			var spr = _add_list_spr(slist_ability, i, "ability", spr_offset)
			_add_list_text(tlist_ability, Item.get_name_str_from_id(i))
			spr_offset += LIST_SPRITE_OFFSET
			_add_item_selectable(spr, i)
		total += count
	if total == 0:
		header_ability.set_snaily_text("subscreen_header_unknown")


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
	new_text.set_snaily_text_raw(_text)


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
	var move_mode:MoveMode = MoveMode.NONE
	if list_focused:
		if SInput.check_input(SInput.Inputs.RIGHT, true):
			list_focused = false
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
	else:
		if SInput.check_input(SInput.Inputs.LEFT, true):
			list_focused = true
			move_mode = MoveMode.NAME if selection == -1 else MoveMode.LIST
	
	if move_mode != MoveMode.NONE:
		sfx_move.play()
	match move_mode:
		MoveMode.LIST:
			selector_target = selectable_items[selection][0].global_position
			selector_target -= UICore.instance.global_position
			selector_target += SELECTOR_LIST_OFFSET
			_set_desc(selectable_items[selection][1])
		MoveMode.NAME:
			selector_target = sel_target_name.position
			_set_desc(-2)
		MoveMode.MAP:
			selector_target = sel_target_map.position
			desc_name.set_snaily_text_raw("")
			desc_body.set_snaily_text_raw("")


func _set_desc(id:int) -> void:
	desc_name.visible = true
	desc_name.set_snaily_text(Item.get_name_str_from_id(id, true))
	var player = int(Statics.current_profile["character"])
	match id:
		Item.ItemTypes.PEASHOOTER:
			desc_body.set_snaily_text("subscreen_desc_peashooter")
		Item.ItemTypes.BOOMERANG:
			desc_body.set_snaily_text("subscreen_desc_boomerang")
		Item.ItemTypes.RAINBOW_WAVE:
			desc_body.set_snaily_text("subscreen_desc_rainbowWave")
		Item.ItemTypes.DEVASTATOR:
			desc_body.set_snaily_text("subscreen_desc_devastator")
		Item.ItemTypes.HIGH_JUMP:
			if player == Player.Players.BLOBBY:
				desc_body.set_snaily_text("subscreen_desc_wallGrab")
			else:
				desc_body.set_snaily_text("subscreen_desc_highJump")
		Item.ItemTypes.SHELL_SHIELD:
			if player == Player.Players.BLOBBY:
				desc_body.set_snaily_text("subscreen_desc_shelmet")
			else:
				desc_body.set_snaily_text("subscreen_desc_shellShield")
		Item.ItemTypes.RAPID_FIRE:
			if player == Player.Players.LEECHY:
				desc_body.set_snaily_text("subscreen_desc_backfire")
			else:
				desc_body.set_snaily_text("subscreen_desc_rapidFire")
		Item.ItemTypes.NONE:
			desc_body.set_snaily_text("subscreen_desc_normalShell")
			if Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS4):
				desc_body.set_snaily_text("subscreen_desc_normalShell_afterWin")
		Item.ItemTypes.ICE_SHELL:
			desc_body.set_snaily_text("subscreen_desc_iceShell")
		-2:
			var pkeys = Player.Players.keys()
			desc_name.visible = false
			desc_body.set_snaily_text("subscreen_desc_" + pkeys[player].to_lower())
		_:
			desc_name.set_snaily_text_raw("")
			desc_body.set_snaily_text_raw("")
