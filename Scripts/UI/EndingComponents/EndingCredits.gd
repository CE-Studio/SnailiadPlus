# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name EndingCredits
extends Node2D


#region Variables
const ENEMY_PATH:String = "res://Scenes/Entities/Enemies/%s.tscn"
const PIXEL_PEOPLE_PATH:String = "uid://dvgo6dlptr5d4"
const ENEMY_SPACING_Y:float = 40.0
const PIXEL_PEOPLE_SPACING_Y:float = 56.0
const SCROLL_DELAY:float = 3.2
const SCROLL_SPEED:float = -33.0
const SCROLL_SPEED_DOWN:float = -192.0
const SCROLL_SPEED_UP:float = 48.0
const FADE_DISTANCE:float = 24.0
const TILEMAP_SCROLL_MULT:float = 0.4125
const END_PAUSE_THRESHOLD:float = 4.5
const SKIP_THRESHOLD:float = 1.0

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
## Array of specialized labels that should fade in
var fade_labels:Array[SnailyText] = []
## Compantion array for fading labels that tracks at what Y positions they should start fading in at
var fade_heights:Array[float] = []
## Whether or not the end screen has started showing or not
var end_visible:bool = false
## How long the credits have sat at the bottom "the end" sprite
var scroll_end_time:float = 0.0
## The Y position the credits should stop scrolling at
var stop_y:float = 1024.0
## Debug tool to scroll credits up/down faster
var can_scroll_manually:bool = false

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
## The tilemap that scrolls behind the credits
@export var tilemap:TileMapLayer
## The stats screen
@export var stats:EndingStats

## Quick reference to the custom text scene
@onready var text:PackedScene = load("uid://dke6r2335reau")
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
	stats.credits = self


func _process(delta: float) -> void:
	elapsed += delta
	if not fading_out:
		cover.modulate.a = move_toward(cover.modulate.a, 0.0, delta)
	
	if scroll_end_time == 0.0 and elapsed > SCROLL_DELAY:
		var this_scroll_speed:float = SCROLL_SPEED
		if can_scroll_manually:
			if SInput.vector_move().y > 0:
				this_scroll_speed = SCROLL_SPEED_DOWN
			elif SInput.vector_move().y < 0:
				this_scroll_speed = SCROLL_SPEED_UP
		credits_parent.position.y += this_scroll_speed * delta
		tilemap.position.y += this_scroll_speed * delta * TILEMAP_SCROLL_MULT
		if credits_parent.position.y <= stop_y:
			credits_parent.position.y = stop_y
			scroll_end_time += delta
	elif scroll_end_time > 0.0 and not end_visible:
		scroll_end_time += delta
		if scroll_end_time >= END_PAUSE_THRESHOLD:
			end_visible = true
			stats.start_anim()
	if elapsed >= SKIP_THRESHOLD and not end_visible and SInput.check_input(SInput.Inputs.PAUSE, true):
		end_visible = true
		stats.start_anim()
	
	if not finished_spawning:
		_create_credits()
	for i in range(fade_labels.size()):
		var this_label:SnailyText = fade_labels[i]
		if this_label.modulate.a < 1.0:
			var this_a:float = inverse_lerp(fade_heights[i],
			fade_heights[i] - FADE_DISTANCE, this_label.global_position.y)
			this_a = clampf(this_a, 0.0, 1.0)
			if this_a > this_label.modulate.a:
				this_label.modulate.a = this_a


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
			_add_enemy_group(["ChirpyCommon", "ChirpyTough"], 28, 24, 40)
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
			_add_enemy("Snelk", Vector2(0, 12), 24)
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
			_add_enemy_group(["Canon", "Noncanon"], 60, 24, 32)
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
			_add_enemy("TurtleCommon", Vector2(0, 16), 32)
			_add_label(tr(&"Gravity Turtle"))
			_add_y(ENEMY_SPACING_Y)
		17:
			_add_enemy("TurtleTough", Vector2(0, 16), 32)
			_add_label(tr(&"Gravity Turtle (Cherry Red Finish)"))
			_add_y(ENEMY_SPACING_Y)
		18:
			_add_enemy("Jellyfish", Vector2(0, 8))
			_add_label(tr(&"Jellyfish"))
			_add_y(ENEMY_SPACING_Y)
		19:
			_add_enemy("Seahorse", Vector2(0, 16), 32)
			_add_label(tr(&"Syngnathida"))
			_add_y(ENEMY_SPACING_Y)
		20:
			_add_enemy_group(["TallfishCommon", "TallfishTough"], 52, 24, 48)
			_add_label(tr(&"Tallfish and Angry Tallfish"))
			_add_y(ENEMY_SPACING_Y)
		21:
			_add_enemy("Walleye", Vector2(8, 8))
			_add_label(tr(&"Walleye"))
			_add_y(ENEMY_SPACING_Y)
		22:
			_add_enemy("Angryblock", Vector2(0, 24), 56)
			_add_label(tr(&"This guy"))
			_add_y(ENEMY_SPACING_Y)
		23:
			_add_enemy_group(["Pincer", "Pincer"], 34, 8)
			_add_label(tr(&"Pincer and Sky Pincer"))
			_add_y(ENEMY_SPACING_Y)
		24:
			_add_enemy("GearCommon", Vector2(0, 16), 32)
			_add_label(tr(&"Spinnygear"))
			_add_y(ENEMY_SPACING_Y)
		25:
			_add_enemy("Drone", Vector2(0, 14), 28)
			_add_label(tr(&"Federation Drone"))
			_add_y(ENEMY_SPACING_Y)
		26:
			_add_enemy("Balloon", Vector2(0, 16), 32)
			_add_label(tr(&"Balloon Buster"))
			_add_y(ENEMY_SPACING_Y + 8)
		27:
			_add_y(52)
			_add_enemy("Bosses/Shellbreaker", Vector2(0, 24), 48)
			_add_label(tr(&"Shellbreaker"))
			_add_y(ENEMY_SPACING_Y + 8)
		28:
			_add_enemy("Bosses/Stompy", Vector2(0, 56), 208)
			_add_label(tr(&"Stompy"))
			_add_y(ENEMY_SPACING_Y + 8)
		29:
			_add_scene("uid://ddccj6od43xp1", 64, 128)
			_add_label(tr(&"Space Box"))
			_add_fade_label(tr(&"and Babybox"), 170)
			_add_y(ENEMY_SPACING_Y + 8)
		30:
			_add_enemy("Bosses/Moonsnail", Vector2(0, 8))
			_add_label(tr(&"Moon Snail"))
			_add_y(ENEMY_SPACING_Y + 8)
		31:
			_add_enemy("Bosses/Gigasnail", Vector2(0, 24), 48)
			_add_label(tr(&"Giga Snail"))
			_add_y(ENEMY_SPACING_Y + 8)
		32:
			match Player.instance.who_i_is:
				Player.Players.SNAILY:
					_add_sprite("uid://dduivh26hbk0y", "00.floor.right.idle",
					Vector2(0, 8))
					_add_label(tr(&"Snaily Snail"))
			_add_y(PIXEL_PEOPLE_SPACING_Y)
		33:
			_add_pixel_person("newstarshipsmell")
			_add_label(tr(&"Newstarshipsmell"))
			_add_label(tr(&"Tested Flash Snailiad extensively"), 1)
			_add_y(PIXEL_PEOPLE_SPACING_Y)
		34:
			_add_pixel_person("xdanond")
			_add_label(tr(&"xdanond"))
			_add_label(tr(&"Drew several of Flash Snailiad's sprites"), 1)
			_add_y(PIXEL_PEOPLE_SPACING_Y)
		35:
			_add_pixel_person("adamatomic")
			_add_label(tr(&"Adamatomic"))
			_add_label(tr(&"Created Flixel, without which snaily game would not exist!"), 1)
			_add_y(PIXEL_PEOPLE_SPACING_Y)
		36:
			_add_pixel_person("auriplane")
			_add_label(tr(&"Auriplane"))
			_add_label(tr(&"Author artist composer etc etc, allowed this project to exist"), 1)
			_add_y(PIXEL_PEOPLE_SPACING_Y)
		37:
			_add_pixel_person("epsilon")
			_add_label(tr(&"Epsilon"))
			_add_label(tr(&"Ported snaily game to Godot and piled a crap ton of stuff onto it"), 1)
			_add_y(PIXEL_PEOPLE_SPACING_Y)
		38:
			_add_pixel_person("clarence")
			_add_label(tr(&"clarence112"))
			_add_label(tr(&"Being infinitely better at coding than I am, and also very cute"), 1)
			_add_y(PIXEL_PEOPLE_SPACING_Y)
		39:
			_add_pixel_person("broomie")
			_add_label(tr(&"Broomietunes"))
			_add_label(tr(&"Wrote a bunch of new songs and sounds, and has excellent game design sense"), 1)
			_add_y(PIXEL_PEOPLE_SPACING_Y)
		40:
			_add_pixel_person("orange")
			_add_label(tr(&"ImpossibleOrange"))
			_add_label(tr(&"Helped draw some new sprites"), 1)
			_add_y(PIXEL_PEOPLE_SPACING_Y)
		41:
			_add_pixel_person("zettex")
			_add_label(tr(&"Zettex"))
			_add_label(tr(&"Originally designed two of the new playable characters"), 1)
			_add_y(PIXEL_PEOPLE_SPACING_Y)
		42:
			_add_pixel_person("minervo")
			_add_label(tr(&"Minervo Ionni"))
			_add_label(tr(&"Designed another character and let me bounce a bunch of ideas off it"), 1)
			_add_y(PIXEL_PEOPLE_SPACING_Y)
		43:
			_add_pixel_person("helispark")
			_add_label(tr(&"Helispark"))
			_add_label(tr(&"Playtesting and flavor text"), 1)
			_add_y(PIXEL_PEOPLE_SPACING_Y)
		44:
			_add_pixel_person("goldguy")
			_add_label(tr(&"Goldguy40"))
			_add_label(tr(&"Drew a handful of new tiles for me"), 1)
			_add_y(PIXEL_PEOPLE_SPACING_Y)
		45:
			_add_pixel_person("xander")
			_add_label(tr(&"They Call Me Xander"))
			_add_label(tr(&"Call him an entomologist the way he discovers those bugs"), 1)
			_add_y(PIXEL_PEOPLE_SPACING_Y)
		46:
			_add_pixel_person("ehseezed")
			_add_label(tr(&"Ehseezed"))
			_add_label(tr(&"Heading official Archipelago integration for me"), 1)
			_add_y(PIXEL_PEOPLE_SPACING_Y)
		47:
			_add_pixel_person("discord")
			_add_label(tr(&"The Snailiad Discord"))
			_add_label(tr(&"Testing, feedback, ideas, memes, encouragement, and being patient. Seriously, y'all are amazing and I'm happy to be a part of this community _@_V!!"), 1)
			_add_y(PIXEL_PEOPLE_SPACING_Y)
		48:
			if randf() <= 0.125:
				_add_sprite("uid://qt0amyc7q4m2", "default", Vector2(0, 16), 32)
				_add_y(PIXEL_PEOPLE_SPACING_Y)
		49:
			_add_label(tr(&"And you\n\n\nBecause seriously, why not\n\n\nAll the other games put \"And You\" in the credits, so I figure, \"And You\" must be someone pretty cool\n\n\nThanks, And You!!"))
			_add_y(PIXEL_PEOPLE_SPACING_Y + 32)
		50:
			_add_sprite("uid://m7h6tnoxysr3", "default", Vector2i(0, 61), 61)
			stop_y = -spawn_y + 120
		_:
			finished_spawning = true
	spawn_stage += 1


## Adds the value given to the spawn Y position
func _add_y(space:float) -> void:
	spawn_y += space


## Adds the given text
func _add_label(_text:String, _size:int = 2) -> SnailyText:
	var new_text:SnailyText = SnailyText.new()
	credits_parent.add_child(new_text)
	new_text.set_alignment(HORIZONTAL_ALIGNMENT_CENTER, VERTICAL_ALIGNMENT_TOP)
	new_text.size.x = 400
	new_text.text_scale = _size
	new_text.add_shadow(1)
	new_text.set_snaily_text(_text)
	new_text.position = Vector2(-200.0, spawn_y)
	var line_count:int = new_text.get_line_count()
	_add_y((8 * _size) * line_count + 2)
	return new_text


## Adds the given text as a fading label that only appears at a certain Y level
func _add_fade_label(_text:String, _threshold:float, _size:int = 2) -> void:
	var new_text = _add_label(_text, _size)
	fade_labels.append(new_text)
	fade_heights.append(_threshold)
	new_text.modulate.a = 0.0


## Adds the requested enemy at the given offset
func _add_enemy(_path:String, _offset:Vector2, _y_add:float = 16) -> void:
	var enemy:Enemy = load(ENEMY_PATH % _path).instantiate()
	enemy.display_mode = true
	enemy.z_index = -1
	credits_parent.add_child(enemy)
	enemy.position = Vector2(_offset.x, _offset.y + spawn_y)
	enemy.origin = enemy.position
	enemy.configure_display_mode(true, group_i)
	_add_y(_y_add)


## Adds a group of enemies on the same Y coordinate
func _add_enemy_group(_paths:PackedStringArray, _spacing:float, _y_offset:float, _y_add:float = 16) -> void:
	var this_offset:float = (_paths.size() - 1) * -0.5 * _spacing
	for path in _paths:
		_add_enemy(path, Vector2(this_offset, _y_offset), 0)
		this_offset += _spacing
		group_i += 1
	_add_y(_y_add)
	group_i = 0


## Adds a generic [SnailySprite2D] to the credits
func _add_sprite(_frames_path:String, _anim:String, _offset:Vector2, _y_add:float = 16) -> void:
	var new_spr:SnailySprite2D = SnailySprite2D.new()
	new_spr.sprite_frames = load(_frames_path)
	credits_parent.add_child(new_spr)
	new_spr.position = Vector2(_offset.x, spawn_y + _offset.y)
	new_spr.play(_anim)
	_add_y(_y_add)


## Adds a credits entry based on the credits-specific PixelPeople spritesheet
func _add_pixel_person(_anim:String) -> void:
	_add_sprite(PIXEL_PEOPLE_PATH, _anim, Vector2(0, 16), 32)


## Adds a non-enemy scene to the credits
func _add_scene(_path:String, _y_offset:float, _y_add:float = 16) -> void:
	var scn:Node2D = load(_path).instantiate()
	credits_parent.add_child(scn)
	scn.position = Vector2(0.0, spawn_y + _y_offset)
	_add_y(_y_add)


## Properly and safely frees the credits
func despawn() -> void:
	stars.despawn()
	UICore.instance.cam.reset_from_static_pos()
	get_tree().paused = false
	PauseLayer.suppress_menuing = false
	queue_free()
