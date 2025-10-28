extends Sprite2D


#region Variables
const NAME_STR:String = "entity_%s_name"
const DESC_STR:String = "entity_%s_desc"
const ENEMY_PATH:String = "res://Scenes/Entities/Enemies/%s.tscn"
const BOSS_PATH:String = "res://Scenes/Entities/Enemies/Bosses/%s.tscn"
const FOCUS_COLOR_SPEED:float = 4.5
const SELECTOR_WIGGLE_SPEED:float = 3.8
const SELECTOR_EASE_RATE:float = 20.0
const SELECTOR_Y_MARGIN:int = 20
const ENTRIES:Array = [
	Enemy.EnemyTypes.SPIKEY_COMMON,
	Enemy.EnemyTypes.SPIKEY_TOUGH,
	#Enemy.EnemyTypes.SPIKEY_ABSURD,
	Enemy.EnemyTypes.BABYFISH,
	Enemy.EnemyTypes.FLOATSPIKE,
	Enemy.EnemyTypes.BLOB_COMMON,
	Enemy.EnemyTypes.BLOB_TOUGH,
	#Enemy.EnemyTypes.BLOB_ANGEL,
	Enemy.EnemyTypes.BLOB_DEVIL,
	Enemy.EnemyTypes.CHIRPY,
	Enemy.EnemyTypes.BATTYBAT,
	Enemy.EnemyTypes.FIREBALL,
	Enemy.EnemyTypes.ICEBALL,
	Enemy.EnemyTypes.GHOSTBALL,
	Enemy.EnemyTypes.SNELK,
	Enemy.EnemyTypes.KITTY,
	Enemy.EnemyTypes.CANON,
	#Enemy.EnemyTypes.NONCANON,
	#Enemy.EnemyTypes.FANON,
	#Enemy.EnemyTypes.SNAKEY_COMMON,
	#Enemy.EnemyTypes.SNAKEY_TOUGH,
	#Enemy.EnemyTypes.SKYVIPER,
	#Enemy.EnemyTypes.SPIDER_COMMON,
	#Enemy.EnemyTypes.SPIDER_TOUGH,
	#Enemy.EnemyTypes.TURTLE_COMMON,
	#Enemy.EnemyTypes.TURTLE_TOUGH,
	#Enemy.EnemyTypes.JELLYFISH,
	#Enemy.EnemyTypes.SEAHORSE,
	#Enemy.EnemyTypes.TALLFISH_COMMON,
	#Enemy.EnemyTypes.TALLFISH_TOUGH,
	#Enemy.EnemyTypes.WALLEYE,
	#Enemy.EnemyTypes.PINCER_FLOOR,
	#Enemy.EnemyTypes.PINCER_WALL,
	#Enemy.EnemyTypes.PINCER_CEILING,
	#Enemy.EnemyTypes.GEAR_COMMON,
	#Enemy.EnemyTypes.GEAR_TOUGH,
	#Enemy.EnemyTypes.DRONE,
	#Enemy.EnemyTypes.BALLOON,
	Enemy.EnemyTypes.SHELLBREAKER,
	Enemy.EnemyTypes.STOMPY,
	#Enemy.EnemyTypes.SPACEBOX,
	#Enemy.EnemyTypes.BABYBOX,
	#Enemy.EnemyTypes.MOONSNAIL,
	#Enemy.EnemyTypes.GIGASNAIL,
	#Enemy.EnemyTypes.COSMICSNAIL,
	#Enemy.EnemyTypes.SHELLBREAKER_RUSH,
	#Enemy.EnemyTypes.STOMPY_RUSH,
	#Enemy.EnemyTypes.SPACEBOX_RUSH,
	#Enemy.EnemyTypes.MOONSNAIL_RUSH,
	#Enemy.EnemyTypes.GIGASNAIL_RUSH,
]

var parent_layer:MenuLayer
var text_color:Color = Statics.get_color(Vector2i(0, 4))
var list_items:Array[SnailyText] = []
var entity_list:Array[String] = []
var entry_states:Array[bool] = []
var elapsed:float = 0.0
var focused_text:SnailyText = null
var selector_origin_x:float = 0.0
var selection:int = 0


@onready var text_scene:PackedScene = preload("res://Scenes/internals/SnailyText.tscn")
@onready var scroll_list:VBoxContainer = $"ScrollPanel/EntityList"
@onready var sfx_beep:AudioStreamPlayer = $"Beep"
@onready var selector:Node2D = $"Selector"
@onready var name_text:SnailyText = $"Name"
@onready var desc_text:SnailyText = $"Description"
@onready var display_enemies:Array[Enemy] = []
@onready var enemy_spawn:Node2D = $"EntityOrigin"
#endregion


func _ready() -> void:
	parent_layer = get_parent()
	
	var enemy_enums := Enemy.EnemyTypes.keys()
	#for enemy in enemy_enums:
	#	if enemy is String:
	#		entity_list.append(enemy.to_camel_case())
	for enemy in ENTRIES:
		entity_list.append(enemy_enums[enemy].to_camel_case())
	
	for i in range(entity_list.size()):
		var entity := entity_list[i]
		if not Statics.check_bestiary_entry(ENTRIES[i]):
			entity = "none"
			entry_states.append(false)
		else:
			entry_states.append(true)
		entity = NAME_STR % entity
		var new_text:SnailyText = text_scene.instantiate()
		new_text.text_scale = 1
		new_text.max_width = 112
		new_text.name = str(i)
		new_text.modulate = text_color
		new_text.set_snaily_text(entity)
		scroll_list.add_child(new_text)
		list_items.append(new_text)
		#region Set focus
		new_text.focus_mode = Control.FOCUS_ALL
		var this_focus = "../" + str(i)
		var last_focus = "../" + str(i - 1)
		var next_focus = "../" + str(i + 1)
		if i == 0:
			last_focus = "../" + str(entity_list.size() - 1)
		if i == entity_list.size() - 1:
			next_focus = "../" + str(0)
		new_text.focus_neighbor_left = this_focus
		new_text.focus_neighbor_right = this_focus
		new_text.focus_neighbor_bottom = next_focus
		new_text.focus_next = next_focus
		new_text.focus_neighbor_top = last_focus
		new_text.focus_previous = last_focus
		#endregion
		new_text.focus_entered.connect(_on_text_focused)
	
	if list_items.size() > 0:
		list_items[0].grab_focus()
	
	selector_origin_x = selector.position.x
	
	name_text.modulate = text_color
	desc_text.modulate = text_color
	_update_entry_display()


func _process(delta: float) -> void:
	elapsed += delta
	for text in list_items:
		if text == focused_text:
			var elapsed_mod:float = abs(sin(elapsed * FOCUS_COLOR_SPEED))
			text.modulate = text_color.lerp(Color.WHITE, elapsed_mod)
		else:
			text.modulate = text_color
	
	selector.position.x = selector_origin_x + sin(elapsed * SELECTOR_WIGGLE_SPEED)
	selector.global_position.y = clampf(
		lerpf(
			selector.global_position.y,
			focused_text.global_position.y + 5,
			SELECTOR_EASE_RATE * delta
		),
		parent_layer.global_position.y + SELECTOR_Y_MARGIN,
		parent_layer.global_position.y + 240 - SELECTOR_Y_MARGIN
	)


func _on_text_focused() -> void:
	focused_text = get_viewport().gui_get_focus_owner()
	sfx_beep.play()
	selection = int(focused_text.name)
	_update_entry_display()


func _update_entry_display() -> void:
	if display_enemies.size() > 0:
		for enemy in display_enemies:
			enemy.queue_free()
		display_enemies.clear()
	var has_entry := entry_states[selection]
	var this_entity := entity_list[selection] if has_entry else "none"
	name_text.set_snaily_text(NAME_STR % this_entity)
	desc_text.set_snaily_text(DESC_STR % this_entity)
	if has_entry:
		spawn_display_entity(this_entity)


func spawn_display_entity(entity:String) -> void:
	match entity:
		"babyfish":
			var fish1 = spawn_entity("babyfish1")
			enemy_spawn.add_child(fish1)
			fish1.position = Vector2.LEFT * 12
			var fish2 = spawn_entity("babyfish2")
			enemy_spawn.add_child(fish2)
			fish2.position = Vector2.RIGHT * 12
		# floatspike
		"chirpy":
			var chirpy1 = spawn_entity("chirpy_common")
			enemy_spawn.add_child(chirpy1)
			chirpy1.position = Vector2.LEFT * 16
			var chirpy2 = spawn_entity("chirpy_tough")
			enemy_spawn.add_child(chirpy2)
			chirpy2.position = Vector2.RIGHT * 16
		# snakey
		"kitty":
			var kitty2 = spawn_entity("kitty_tough")
			enemy_spawn.add_child(kitty2)
		_:
			var general_enemy = spawn_entity(entity)
			enemy_spawn.add_child(general_enemy)
			general_enemy.position = Vector2.ZERO


func spawn_entity(entity:String) -> Enemy:
	entity = entity.to_pascal_case()
	var new_enemy:Enemy = null
	if ResourceLoader.exists(BOSS_PATH % entity):
		new_enemy = load(BOSS_PATH % entity).instantiate()
	else:
		new_enemy = load(ENEMY_PATH % entity).instantiate()
	new_enemy.display_mode = true
	display_enemies.append(new_enemy)
	new_enemy.z_index = -5
	return new_enemy
