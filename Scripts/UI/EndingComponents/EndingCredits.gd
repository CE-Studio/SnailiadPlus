# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
extends Node2D


#region Variables
const ENEMY_PATH:String = "res://Scenes/Entities/Enemies/%s.tscn"
const ENEMY_SPACING_Y:float = 24.0
const SCROLL_DELAY:float = 3.2
const SCROLL_SPEED:float = -33.0
const SCROLL_SPEED_DOWN:float = -192.0
const SCROLL_SPEED_UP:float = 48.0

## The amount of time in seconds since the credits were spawned
var elapsed:float = 0.0
## Will be set to [code]true[/code] if the credits are currently fading out
var fading_out:bool = false
## The Y position to spawn the next credits object at. Will be advanced when something is spawned
var spawn_y:float = -10.0
## The index of the last enemy spawned in a group
var group_i:int = 0
## What stage of credits creation the script is set to spawn next
var spawn_stage:int = 0
## Will be set if the credits have been fully spawned
var finished_spawning:bool = false

## The parent to all credits objects. Will scroll up slowly overtime
@export var credits_parent:Node2D
## The [StarLayer] shown behind the credits
@export var stars:StarLayer
## The fade that appears over everything
@export var cover:Sprite2D
## The main credits music
@export var music_main:AudioStreamPlayer
## The alternate credits music
@export var music_alt:AudioStreamPlayer

## Quick reference to the custom text scene
@onready var text:PackedScene = load("res://Scenes/internals/SnailyText.tscn")
#endregion


## Initializes the credits and properly sets the color of the fade-in
func setup(white:bool) -> void:
	stars.spawn()
	if white:
		cover.modulate = Color.WHITE
	else:
		cover.modulate = Color.BLACK
	if Player.instance.who_i_is == Player.Players.BLOBBY:
		music_alt.play()
	else:
		music_main.play()
	_create_credits()


func _process(delta: float) -> void:
	elapsed += delta
	if not fading_out:
		cover.modulate.a = move_toward(cover.modulate.a, 0.0, delta)
	if elapsed > SCROLL_DELAY:
		var this_scroll_speed:float = SCROLL_SPEED
		if SInput.vector_move().y > 0:
			this_scroll_speed = SCROLL_SPEED_DOWN
		elif SInput.vector_move().y < 0:
			this_scroll_speed = SCROLL_SPEED_UP
		credits_parent.position.y += this_scroll_speed * delta
	if not finished_spawning:
		_create_credits()


## Standalone function for organizing and creating the credits. Spawning each enemy or enemy
## group is done in separate calls to reduce lag
func _create_credits() -> void:
	match spawn_stage:
		0:
			_add_label(tr(&"Credits"), 3)
			_add_y(180.0)
		1:
			_add_enemy_group(["SpikeyCommon", "SpikeyTough"], 60, 8)
			_add_label(tr(&"Spikey"))
			_add_y(ENEMY_SPACING_Y)
		2:
			_add_enemy_group(["Babyfish1", "Babyfish2"], 50, 8)
			_add_label(tr(&"Babyfish"))
			_add_y(ENEMY_SPACING_Y)
		3:
			_add_enemy_group(["FloatspikeCommon", "FloatspikeTough"], 26, 8)
			_add_label(tr(&"Floatspike"))
			_add_y(ENEMY_SPACING_Y)
		4:
			_add_enemy_group(["BlobCommon", "BlobTough", "BlobAngel", "BlobDevil"], 20, 8)
			_add_label(tr(&"Blob, Blub, Angelblob, and Devilblob"))
			_add_y(ENEMY_SPACING_Y)
		5:
			_add_enemy_group(["ChirpyCommon", "ChirpyTough"], 28, 24, 48)
			_add_label(tr(&"Chirpy"))
			_add_y(ENEMY_SPACING_Y)
		6:
			_add_enemy("Battybat", Vector2(0, 8))
			_add_label(tr(&"Batty Bat"))
			_add_y(ENEMY_SPACING_Y)
		7:
			_add_enemy_group(["Fireball", "Iceball"], 24, 8)
			_add_label(tr(&"Fireball and Iceball"))
			_add_y(ENEMY_SPACING_Y)
		8:
			_add_enemy("Snelk", Vector2(0, 12), 32)
			_add_label(tr(&"Secret Snelk"))
			_add_y(ENEMY_SPACING_Y)
		9:
			_add_enemy_group(["KittyTough"], 0, 8)
			_add_label(tr(&"Kitty!!"))
			_add_y(ENEMY_SPACING_Y)
		10:
			_add_enemy("Ghostball", Vector2(0, 8))
			_add_label(tr(&"Ghost Dandelion"))
			_add_y(ENEMY_SPACING_Y)
		11:
			_add_enemy_group(["Canon", "Noncanon"], 60, 24, 40)
			_add_label(tr(&"Canon and Non-canon"))
			_add_y(ENEMY_SPACING_Y)
		12:
			_add_enemy_group(["SnakeyCommon", "SnakeyTough"], 40, 8)
			_add_label(tr(&"Snakey"))
			_add_y(ENEMY_SPACING_Y)
		13:
			_add_enemy("Skyviper", Vector2(0, 8))
			_add_label(tr(&"Sky Viper"))
			_add_y(ENEMY_SPACING_Y)
		14:
			_add_enemy("SpiderCommon", Vector2(0, 8))
			_add_label(tr(&"Spider"))
			_add_y(ENEMY_SPACING_Y)
		15:
			_add_enemy("SpiderTough", Vector2(0, 8))
			_add_label(tr(&"Spider Mama"))
			_add_y(ENEMY_SPACING_Y)
		16:
			_add_enemy("TurtleCommon", Vector2(0, 16), 40)
			_add_label(tr(&"Gravity Turtle"))
			_add_y(ENEMY_SPACING_Y)
		17:
			_add_enemy("TurtleTough", Vector2(0, 16), 40)
			_add_label(tr(&"Gravity Turtle (Cherry Red Finish)"))
			_add_y(ENEMY_SPACING_Y)
		18:
			_add_enemy("Jellyfish", Vector2(0, 8))
			_add_label(tr(&"Jellyfish"))
			_add_y(ENEMY_SPACING_Y)
		19:
			_add_enemy("Seahorse", Vector2(0, 16), 40)
			_add_label(tr(&"Syngnathida"))
			_add_y(ENEMY_SPACING_Y)
		20:
			_add_enemy_group(["TallfishCommon", "TallfishTough"], 52, 24, 56)
			_add_label(tr(&"Tallfish and Angry Tallfish"))
			_add_y(ENEMY_SPACING_Y)
		21:
			_add_enemy("Walleye", Vector2(8, 8))
			_add_label(tr(&"Walleye"))
			_add_y(ENEMY_SPACING_Y)
		22:
			_add_enemy_group(["Pincer", "Pincer"], 34, 8)
			_add_label(tr(&"Pincer and Sky Pincer"))
			_add_y(ENEMY_SPACING_Y)
		23:
			_add_enemy("GearCommon", Vector2(0, 16), 40)
			_add_label(tr(&"Spinnygear"))
			_add_y(ENEMY_SPACING_Y)
		24:
			_add_enemy("Drone", Vector2(0, 14), 36)
			_add_label(tr(&"Federation Drone"))
			_add_y(ENEMY_SPACING_Y)
		25:
			_add_enemy("Balloon", Vector2(0, 16), 40)
			_add_label(tr(&"Balloon Buster"))
			_add_y(ENEMY_SPACING_Y)
		26:
			_add_y(52)
			_add_enemy("Bosses/Shellbreaker", Vector2(0, 24), 56)
			_add_label(tr(&"Shellbreaker"))
			_add_y(ENEMY_SPACING_Y)
		27:
			_add_enemy("Bosses/Stompy", Vector2(0, 56), 216)
			_add_label(tr(&"Stompy"))
			_add_y(ENEMY_SPACING_Y)
		_:
			finished_spawning = true
	spawn_stage += 1


## Adds the value given to the spawn Y position
func _add_y(space:float) -> void:
	spawn_y += space


## Adds the given text to 
func _add_label(_text:String, _size:int = 2) -> void:
	var new_text:SnailyText = text.instantiate()
	credits_parent.add_child(new_text)
	new_text.set_alignment(HORIZONTAL_ALIGNMENT_CENTER, VERTICAL_ALIGNMENT_TOP)
	new_text.max_width = 400
	new_text.text_scale = _size
	new_text.add_shadow(1)
	new_text.set_snaily_text(_text)
	new_text.position = Vector2((new_text.get_width() * -0.5) * 0.5 * _size, spawn_y)
	var line_count:int = new_text.get_line_count()
	_add_y((8 * _size) * line_count + 2)


## Adds the requested enemy at the given offset
func _add_enemy(_path:String, _offset:Vector2, _y_add:float = 24) -> void:
	var enemy:Enemy = load(ENEMY_PATH % _path).instantiate()
	enemy.display_mode = true
	enemy.z_index = -1
	credits_parent.add_child(enemy)
	enemy.position = Vector2(_offset.x, _offset.y + spawn_y)
	enemy.origin = enemy.position
	enemy.configure_display_mode(true, group_i)
	_add_y(_y_add)


## Adds a group of enemies on the same Y coordinate
func _add_enemy_group(_paths:PackedStringArray, _spacing:float, _y_offset:float, _y_add:float = 24) -> void:
	var this_offset:float = (_paths.size() - 1) * -0.5 * _spacing
	for path in _paths:
		_add_enemy(path, Vector2(this_offset, _y_offset), 0)
		this_offset += _spacing
		group_i += 1
	_add_y(_y_add)
	group_i = 0
