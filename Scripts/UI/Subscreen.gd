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

var group_start_x:float = 0.0
var elapsed:float = 0.0
var selection_depth:int = -1
var selectable_items:Array = [] # Formatting: [ JsonSprite2D, Int ]
var selection:int = -1
var list_focused:bool = false

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
	_init_item_slots()


func _process(delta: float) -> void:
	elapsed += delta
	selector_spr.position.x = abs(sin(elapsed * 8)) * -2
	
	if selection_depth == 0:
		if SInput.check_input(SInput.Inputs.MAP, true) or SInput.check_input(SInput.Inputs.PAUSE, true):
			queue_free()
			UICore.instance.pause_layer.unpause_fade_out()
	
	if selection_depth < 0:
		selection_depth = 0


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
	pass
