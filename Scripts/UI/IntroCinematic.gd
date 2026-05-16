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

## List of all active background pattern sprites
var bg_patterns:Array[JsonSprite2D] = []
## The string to print on screen
var target_str:String = ""
## Pointer used to determine which character to add to the printed string next
var char_i:int = 0
## The time in seconds left until the next character can be printed
var char_timeout:float = 0.0

## The text node that displays the story strings
@export var text:SnailyText
## The parent node that holds all background pattern sprites as children
@export var pattern_group:Node2D
## A multiplier applied to the background pattern scroll speed
@export var pattern_speed_mult:float = 1.0
## The sound that plays when a new character is added to the visible string
@export var sfx_text:AudioStreamPlayer
## The sound that plays when the story art is changed to a new frame
@export var sfx_page:AudioStreamPlayer
#endregion


func _ready() -> void:
	_create_bg()


func _process(delta: float) -> void:
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
			while not printed_next:
				printed_next = true


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
func set_text() -> void:
	pass


## Clears all text from the story label and associated variables
func clear_text() -> void:
	text.set_snaily_text("")
	target_str = ""
