# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name IntroCinematic
extends Node2D


#region Variables
const PATTERN_GRID:Vector2i = Vector2i(12, 8)
const PATTERN_SIZE:Vector2i = Vector2i(48, 48)
const PATTERN_PATH:String = "res://Assets/Images/Intro/IntroBGPatterns.json"
const PATTERN_DIR:Vector2i = Vector2i(-34, -34)
const CHAR_DELAY:float = 0.067
const SKIP_THRESHOLD:float = 1.0
const SCENE_THRESHOLD:float = 1.1

## List of translatable strings to be printed on screen during the intro
var story_strings:PackedStringArray = [
	tr(&" All was peaceful in Snail Town..."),
	tr(&" Until one day...     \n     Moon Snail left on a journey"),
	tr(&" Soon after...     \n     the snails began to disappear                   \n                   one by one"),
	tr(&" Will anyone help?  Can anyone \n        rescue the missing snails??"),
	tr(&" You can do it, %s!!        \n        It's up to you!!  Good luck!!")
]

## The amount of time in seconds that this node has been active
var elapsed:float = 0.0
## The amount of time in seconds since the fadeout was started
var fade_elapsed:float = 0.0
## List of all active background pattern sprites
var bg_patterns:Array[JsonSprite2D] = []
## The string to print on screen
var target_str:String = ""
## Pointer used to determine which character to add to the printed string next
var char_i:int = 0
## The time in seconds left until the next character can be printed
var char_timeout:float = 0.0
## The character currently being played as
var player_char:Player.Players = Player.Players.SNAILY
## Will be set if the fadeout has started
var started_fade:bool = false
## Reference to the menu scene
var menu:MainMenu

## The text node that displays the story strings
@export var text:SnailyText
## The parent node that holds all background pattern sprites as children
@export var pattern_group:Node2D
## The parent node for the fourth story page, which shows the player character
@export var story4_parent:Node2D
## A multiplier applied to the background pattern scroll speed
@export var pattern_speed_mult:float = 1.0
## The sound that plays when a new character is added to the visible string
@export var sfx_text:AudioStreamPlayer
## The sound that plays when the story art is changed to a new frame
@export var sfx_page:AudioStreamPlayer
## The color cover that fades the intro out
@export var cover:ColorCover
#endregion


func _ready() -> void:
	_create_bg()
	clear_text()


func setup_player(player:Player.Players) -> void:
	player_char = player
	var story4_anim:String = "res://Assets/Images/Intro/Intro4%s.json"
	match player:
		Player.Players.SNAILY:
			story4_anim = story4_anim % "A"
			story_strings[4] = story_strings[4] % GlobalText.characters[0][1]
		Player.Players.SLUGGY:
			story4_anim = story4_anim % "B"
			story_strings[4] = story_strings[4] % GlobalText.characters[1][1]
		Player.Players.UPSIDE:
			story4_anim = story4_anim % "C"
			story_strings[4] = story_strings[4] % GlobalText.characters[2][1]
		Player.Players.LEGGY:
			story4_anim = story4_anim % "D"
			story_strings[4] = story_strings[4] % GlobalText.characters[3][1]
		Player.Players.BLOBBY:
			story4_anim = story4_anim % "E"
			story_strings[4] = story_strings[4] % GlobalText.characters[4][1]
		Player.Players.LEECHY:
			story4_anim = story4_anim % "F"
			story_strings[4] = story_strings[4] % GlobalText.characters[5][1]
	var story4_spr:JsonSprite2D = JsonSprite2D.new()
	story4_spr.texture_path = story4_anim
	story4_parent.add_child(story4_spr)


func _process(delta: float) -> void:
	elapsed += delta
	if started_fade:
		fade_elapsed += delta
	
	for pattern in bg_patterns:
		pattern.position += PATTERN_DIR * delta * pattern_speed_mult
		if pattern.position.x < -PATTERN_SIZE.x:
			pattern.position.x += PATTERN_GRID.x * PATTERN_SIZE.x
		if pattern.position.y < -PATTERN_SIZE.y:
			pattern.position.y += PATTERN_GRID.y * PATTERN_SIZE.y
	
	if char_i < target_str.length():
		char_timeout -= delta
		if char_timeout <= 0.0:
			char_timeout += CHAR_DELAY
			var advance_from_newline:bool = false
			var printed_next:bool = false
			var play_sound:bool = false
			while not printed_next:
				var this_char:String = target_str[char_i]
				if this_char == "\n" or (this_char == " " and char_i == 0):
					advance_from_newline = true
				text.set_snaily_text(text.text + this_char)
				if not advance_from_newline or (
				advance_from_newline and (this_char != " " and this_char != "\n")):
					printed_next = true
				char_i += 1
				play_sound = this_char != " "
			if play_sound:
				sfx_text.play()
	
	if elapsed >= SKIP_THRESHOLD and not started_fade and SInput.check_input(SInput.Inputs.PAUSE, true):
		start_fade()
	
	if started_fade:
		sfx_text.volume_linear = clampf(sfx_text.volume_linear - delta, 0.0, 1.0)
		sfx_page.volume_linear = clampf(sfx_page.volume_linear - delta, 0.0, 1.0)
		if menu:
			menu.music.volume_linear = clampf(menu.music.volume_linear - delta, 0.0, 1.0)
		if fade_elapsed >= SCENE_THRESHOLD:
			get_tree().change_scene_to_file("uid://ltxtlsrku2k0")


## Automatically creates all background patterns
func _create_bg() -> void:
	var variant:bool = false
	for y in range(PATTERN_GRID.y):
		variant = y % 2 == 1
		for x in range(PATTERN_GRID.x):
			var new_spr:JsonSprite2D = JsonSprite2D.new()
			new_spr.texture_path = PATTERN_PATH
			new_spr.load_autoplay = [
				("2" if variant else "1") + "a",
				("2" if variant else "1") + "b",
				("2" if variant else "1") + "c",
				("2" if variant else "1") + "d",
			]
			pattern_group.add_child(new_spr)
			new_spr.position = Vector2i(x, y) * PATTERN_SIZE
			bg_patterns.append(new_spr)
			variant = not variant


## Sets a target string for the dialogue label to slowly print
func set_text(string_index:int) -> void:
	clear_text()
	char_i = 0
	target_str = story_strings[string_index]


## Clears all text from the story label and associated variables
func clear_text() -> void:
	text.set_snaily_text("")
	target_str = ""


## Begins the fadeout, the end of which triggers the change to the game scene
func start_fade() -> void:
	started_fade = true
	var end_color:Color = cover.start_color
	end_color.a = 1.0
	cover.set_new_fade(cover.start_color, end_color, 1.0)
