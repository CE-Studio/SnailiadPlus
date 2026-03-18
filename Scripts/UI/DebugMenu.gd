extends Node2D


#region Variables
const CELL_SIZE:Vector2i = Vector2i(16, 16)
const SELECTOR_OFFSET:Vector2i = Vector2i(8, 8)

var selected_cell:int = 0
var scrolling:bool = false
var buffer_frames:int = 4
var cell_desc:String = ""
var ctrl_prompt:String = ""
var select_type:SelectTypes = SelectTypes.NONE

enum SelectTypes {
	NONE,
	TOGGLE,
	SCROLL,
	ACTION,
}

@export var grid_size:Vector2i = Vector2i(10, 5)
@export var sprites:Array[Sprite2D] = []
@export var selector:JsonSprite2D
@export var desc:SnailyText
#endregion


func _ready() -> void:
	position = Statics.VECTOR_CENTER - (Vector2(CELL_SIZE * grid_size) * 0.5)
	for spr in sprites:
		if spr != null:
			_update_sprite(spr)
	if sprites[selected_cell] != null:
		_update_desc_action(sprites[selected_cell])


func _process(_delta: float) -> void:
	if ((SInput.check_input(SInput.Inputs.PAUSE, true) or SInput.check_input(SInput.Inputs.DEBUG, true))
	and not scrolling and buffer_frames <= 0):
		UICore.instance.pause_layer.unpause_fade_out()
		queue_free()
	
	var intended_dir:Vector2 = Vector2(
		int(SInput.check_input(SInput.Inputs.RIGHT, true)) - int(SInput.check_input(SInput.Inputs.LEFT, true)),
		int(SInput.check_input(SInput.Inputs.DOWN, true)) - int(SInput.check_input(SInput.Inputs.UP, true))
	)
	if intended_dir != Vector2.ZERO:
		if scrolling:
			pass
		else:
			_move_selection(intended_dir)
	if SInput.check_input(SInput.Inputs.UI_ACCEPT, true):
		_handle_select()
	
	if buffer_frames > 0:
		buffer_frames -= 1


func _move_selection(dir:Vector2i) -> void:
	var current_row:int = floori(selected_cell / grid_size.x)
	selected_cell += roundi(dir.x + (dir.y * grid_size.x))
	var new_row:int = floori(selected_cell / grid_size.x)
	if selected_cell < 0:
		new_row = -1
	if dir.y == 0:
		if new_row < current_row:
			selected_cell += grid_size.x
		elif new_row > current_row:
			selected_cell -= grid_size.x
	while selected_cell < 0:
		selected_cell += sprites.size()
	while selected_cell >= sprites.size():
		selected_cell -= sprites.size()
	selector.position = Vector2i(
		selected_cell % grid_size.x,
		floori(selected_cell / grid_size.x)
	) * CELL_SIZE + SELECTOR_OFFSET
	
	if sprites[selected_cell] != null:
		_update_desc_action(sprites[selected_cell])
	else:
		_clear_desc()


func _handle_select() -> void:
	match select_type:
		SelectTypes.NONE:
			pass
		SelectTypes.TOGGLE:
			_handle_toggle_cases(sprites[selected_cell])


func _handle_toggle_cases(spr:Sprite2D) -> void:
	match spr.name:
		"Boss1":
			Statics.set_world_flag(Statics.WorldFlags.DEFEATED_BOSS1,
			not Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS1))
		"Boss2":
			Statics.set_world_flag(Statics.WorldFlags.DEFEATED_BOSS2,
			not Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS2))
		"Boss3":
			Statics.set_world_flag(Statics.WorldFlags.DEFEATED_BOSS3,
			not Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS3))
		"Boss4":
			Statics.set_world_flag(Statics.WorldFlags.DEFEATED_BOSS4,
			not Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS4))
		"Noclip":
			Statics.noclip_mode = not Statics.noclip_mode
		"AttackMult":
			Statics.damage_mult = not Statics.damage_mult
		"ShowHidden":
			Statics.show_entity_layer = not Statics.show_entity_layer
			Statics.show_invis_entites = not Statics.show_invis_entites
	_update_sprite(spr)


func _generic_set_sprite(spr:Sprite2D, condition:bool) -> void:
	spr.frame_coords.x = int(condition)


func _set_item_desc(_name:String, _count:int) -> void:
	cell_desc = "%s - %d in inventory" % [_name, _count]
	_set_desc_common()


func _set_boss_desc(_name:String, _alive:bool) -> void:
	cell_desc = "%s - currently %s" % [_name, "alive" if _alive else "dead"]
	_set_desc_common()


func _set_tool_desc(_name:String) -> void:
	cell_desc = _name
	_set_desc_common()


func _set_scroll_prompt() -> void:
	ctrl_prompt = "Select to change"
	_set_desc_common()
	select_type = SelectTypes.SCROLL


func _set_toggle_prompt() -> void:
	ctrl_prompt = "Select to toggle"
	_set_desc_common()
	select_type = SelectTypes.TOGGLE


func _clear_desc() -> void:
	cell_desc = ""
	ctrl_prompt = ""
	_set_desc_common()
	select_type = SelectTypes.NONE


func _set_desc_common() -> void:
	if desc:
		desc.set_snaily_text("\n".join([cell_desc, ctrl_prompt]))


func _update_sprite(spr:Sprite2D) -> void:
	match spr.name:
		"Peashooter":
			_generic_set_sprite(spr, Statics.check_item(Item.ItemTypes.PEASHOOTER))
		"Boomerang":
			_generic_set_sprite(spr, Statics.check_item(Item.ItemTypes.BOOMERANG))
		"RainbowWave":
			_generic_set_sprite(spr, Statics.check_item(Item.ItemTypes.RAINBOW_WAVE))
		"Devastator":
			_generic_set_sprite(spr, Statics.check_item(Item.ItemTypes.DEVASTATOR))
		"HighJump":
			_generic_set_sprite(spr, Statics.check_item(Item.ItemTypes.HIGH_JUMP))
		"ShellShield":
			_generic_set_sprite(spr, Statics.check_item(Item.ItemTypes.SHELL_SHIELD))
		"RapidFire":
			_generic_set_sprite(spr, Statics.check_item(Item.ItemTypes.RAPID_FIRE))
		"IceSnail":
			_generic_set_sprite(spr, Statics.check_item(Item.ItemTypes.ICE_SHELL))
		"GravitySnail":
			_generic_set_sprite(spr, Statics.check_item(Item.ItemTypes.GRAVITY_SHELL))
		"FullMetalSnail":
			_generic_set_sprite(spr, Statics.check_item(Item.ItemTypes.METAL_SHELL))
		"GravityShock":
			_generic_set_sprite(spr, Statics.check_item(Item.ItemTypes.GRAVITY_SHOCK))
		"Broom":
			_generic_set_sprite(spr, Statics.check_item(Item.ItemTypes.BROOM))
		"RadarShell":
			_generic_set_sprite(spr, Statics.check_item(Item.ItemTypes.RADAR_SHELL))
		"HeartContainer":
			_generic_set_sprite(spr, Statics.check_item(Item.ItemTypes.HEART_CONTAINER))
		"HelixFragment":
			_generic_set_sprite(spr, Statics.check_item(Item.ItemTypes.HELIX_FRAGMENT))
		"Boss1":
			_generic_set_sprite(spr, not Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS1))
		"Boss2":
			_generic_set_sprite(spr, not Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS2))
		"Boss3":
			_generic_set_sprite(spr, not Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS3))
		"Boss4":
			_generic_set_sprite(spr, not Statics.get_world_flag(Statics.WorldFlags.DEFEATED_BOSS4))
		"WeaponTrap":
			pass
		"GravityTrap":
			pass
		"LullabyTrap":
			pass
		"SpiderTrap":
			pass
		"WarpTrap":
			pass
		"Noclip":
			_generic_set_sprite(spr, Statics.noclip_mode)
		"AttackMult":
			_generic_set_sprite(spr, Statics.damage_mult)
		"ShowHidden":
			_generic_set_sprite(spr, Statics.show_entity_layer)


func _update_desc_action(spr:Sprite2D) -> void:
	match spr.name:
		"Peashooter":
			_set_item_desc("Peashooter", Statics.check_item(Item.ItemTypes.PEASHOOTER))
			_set_scroll_prompt()
		"Boomerang":
			_set_item_desc("Boomerang", Statics.check_item(Item.ItemTypes.BOOMERANG))
			_set_scroll_prompt()
		"RainbowWave":
			_set_item_desc("Rainbow Wave", Statics.check_item(Item.ItemTypes.RAINBOW_WAVE))
			_set_scroll_prompt()
		"Devastator":
			_set_item_desc("Devastator", Statics.check_item(Item.ItemTypes.DEVASTATOR))
			_set_scroll_prompt()
		"HighJump":
			_set_item_desc("High Jump", Statics.check_item(Item.ItemTypes.HIGH_JUMP))
			_set_scroll_prompt()
		"ShellShield":
			_set_item_desc("Shell Shield", Statics.check_item(Item.ItemTypes.SHELL_SHIELD))
			_set_scroll_prompt()
		"RapidFire":
			_set_item_desc("Rapid Fire", Statics.check_item(Item.ItemTypes.RAPID_FIRE))
			_set_scroll_prompt()
		"IceSnail":
			_set_item_desc("Ice Snail", Statics.check_item(Item.ItemTypes.ICE_SHELL))
			_set_scroll_prompt()
		"GravitySnail":
			_set_item_desc("Gravity Snail", Statics.check_item(Item.ItemTypes.GRAVITY_SHELL))
			_set_scroll_prompt()
		"FullMetalSnail":
			_set_item_desc("Full Metal Snail", Statics.check_item(Item.ItemTypes.METAL_SHELL))
			_set_scroll_prompt()
		"GravityShock":
			_set_item_desc("Gravity Shock", Statics.check_item(Item.ItemTypes.GRAVITY_SHOCK))
			_set_scroll_prompt()
		"Broom":
			_set_item_desc("Broom", Statics.check_item(Item.ItemTypes.BROOM))
			_set_scroll_prompt()
		"RadarShell":
			_set_item_desc("Radar Shell", Statics.check_item(Item.ItemTypes.RADAR_SHELL))
			_set_scroll_prompt()
		"HeartContainer":
			_set_item_desc("Heart Container", Statics.check_item(Item.ItemTypes.HEART_CONTAINER))
			_set_scroll_prompt()
		"HelixFragment":
			_set_item_desc("Helix Fragment", Statics.check_item(Item.ItemTypes.HELIX_FRAGMENT))
			_set_scroll_prompt()
		"Boss1":
			_set_boss_desc("Shellbreaker", Statics.WorldFlags.DEFEATED_BOSS1)
			_set_toggle_prompt()
		"Boss2":
			_set_boss_desc("Stompy", Statics.WorldFlags.DEFEATED_BOSS2)
			_set_toggle_prompt()
		"Boss3":
			_set_boss_desc("Space Box", Statics.WorldFlags.DEFEATED_BOSS3)
			_set_toggle_prompt()
		"Boss4":
			_set_boss_desc("Moon Snail", Statics.WorldFlags.DEFEATED_BOSS4)
			_set_toggle_prompt()
		"WeaponTrap":
			pass
		"GravityTrap":
			pass
		"LullabyTrap":
			pass
		"SpiderTrap":
			pass
		"WarpTrap":
			pass
		"Noclip":
			_set_tool_desc("Noclip")
			_set_toggle_prompt()
		"AttackMult":
			_set_tool_desc("Damage multiplier")
			_set_toggle_prompt()
		"ShowHidden":
			_set_tool_desc("Show entity layer/invis entites")
			_set_toggle_prompt()
