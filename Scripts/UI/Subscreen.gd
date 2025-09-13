class_name Subscreen
extends Node2D


#region Variables
const ITEMS_WEAPON:Array = [
	Item.ItemTypes.PEASHOOTER,
	Item.ItemTypes.BOOMERANG,
	Item.ItemTypes.RAINBOW_WAVE,
]
const ITEMS_SHELL:Array = [
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

var group_start_x:float = 0.0
var elapsed:float = 0.0
var selection_depth:int = -1

@export var separators:Array[JsonSprite2D] = []
@export var body:JsonSprite2D
@export var player_icon:JsonSprite2D
@export var header_name:SnailyText
@export var header_weapon:SnailyText
@export var header_shell:SnailyText
@export var header_ability:SnailyText
@export var name_box:HBoxContainer
@export var name_text:SnailyText
@export var selector:Node2D
@export var selector_spr:JsonSprite2D
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
	for i in ITEMS_WEAPON:
		total += _get_item_count(i)
	if total == 0:
		header_weapon.set_snaily_text("subscreen_header_unknown")
	
	total = 0
	for i in ITEMS_SHELL:
		total += _get_item_count(i)
	if total == 0:
		header_shell.set_snaily_text("subscreen_header_unknown")
	
	total = 0
	for i in ITEMS_ABILITY:
		total += _get_item_count(i)
	if total == 0:
		header_ability.set_snaily_text("subscreen_header_unknown")


func _get_item_count(id:Item.ItemTypes) -> int:
	var count:int = Statics.check_item(id)
	var keys:Array = SUB_ITEMS.keys()
	for i in keys.size():
		if SUB_ITEMS[i] == id:
			count += Statics.check_item(keys[i])
	return count
