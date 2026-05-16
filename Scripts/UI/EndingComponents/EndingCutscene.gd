# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name EndingCutscene
extends Node2D


const CHAR_TIMEOUT:float = 0.067
const DIALOGUE_FADE_RATE:float = 0.9
const SKIP_THRESHOLD:float = 1.0

## The dialogue to be displayed during the Moon Snail version of the cutscene
var dialogue_moon:PackedStringArray = [
	tr(&"    And so..."),
	tr(&"    Defeated, Moon Snail\n        lost his powers"),
	tr(&"    But could he ever become\n        Sun Snail once more?"),
]
## The dialogue to be displayed during the Sun Snail version of the cutscene
var dialogue_sun:PackedStringArray = [
	tr(&"    And so..."),
	tr(&"    Moon Snail once again\n        regained his light"),
	tr(&"    And became the\n        legendary Sun Snail"),
]

## The time in seconds since the cutscene was spawned
var elapsed:float = 0.0
## If [code]true[/code], the Sun Snail version of the cutscene is being played
var is_sun:bool = false
## The string that should be displayed
var target_str:String = ""
## The ID of the next character to be added to the string
var current_char:int = 0
## How long in seconds the script waits before adding a new character to the string
var char_timeout:float = 0.0
## Whether or not the dialogue string should be shown
var dialogue_visible:bool = false
## Will be set if the cutscene is currently being skipped
var skipping:bool = false


## The [AnimationPlayer] that drives most of the cutscene
@export var anim:AnimationPlayer
## The [StarLayer] shown between the background and everything else
@export var stars:StarLayer
## The fade that appears over everything except Moon Snail
@export var cover:Sprite2D
## The fade that appears over everything if the cutscene is skipped
@export var skip_cover:Sprite2D
## The background sprite
@export var bg:JsonSprite2D
## The first sprite instance of Moon Snail
@export var first_moon:JsonSprite2D
## The final sprite instance of Moon Snail, which can either be shelled or Sun Snail
@export var last_moon:JsonSprite2D
## The spotlight sprite
@export var spotlight:JsonSprite2D
## The dialogue node
@export var dialogue:SnailyText
## The music that plays during the cutscene
@export var music:AudioStreamPlayer
## The sound that plays when a character is added to the string
@export var sfx_dialogue:AudioStreamPlayer


func _ready() -> void:
	get_tree().paused = true
	UICore.instance.cam.set_to_static_pos()
	GameCore.instance.music_manager.stop_all()
	cover.modulate.a = 1.0
	first_moon.modulate.a = 0.0
	spotlight.modulate.a = 0.0
	stars.spawn()
	if Statics.get_item_percentage() >= 100:
		is_sun = true
		anim.play(&"Sun")
		last_moon.action = "sun"
	else:
		anim.play(&"Moon")
	dialogue.set_snaily_text("")


func _process(delta: float) -> void:
	elapsed += delta
	if not dialogue_visible and dialogue.modulate.a > 0.0:
		dialogue.modulate.a -= delta * DIALOGUE_FADE_RATE
	if current_char < target_str.length():
		if char_timeout <= 0.0:
			char_timeout = CHAR_TIMEOUT
			var this_char:String = " "
			while (this_char == " " or this_char == "\n") and current_char < target_str.length():
				this_char = target_str[current_char]
				dialogue.set_snaily_text(dialogue.text + this_char)
				current_char += 1
			sfx_dialogue.play()
		else:
			char_timeout -= delta
	if SInput.check_input(SInput.Inputs.PAUSE, true) and elapsed >= SKIP_THRESHOLD and not skipping:
		skipping = true
	if skipping:
		skip_cover.modulate.a += delta
		music.volume_linear = move_toward(music.volume_linear, 0.0, delta)
		if skip_cover.modulate.a >= 1.25:
			spawn_credits()


## Updates the background animation to the desired state
func update_bg(action:String) -> void:
	bg.action = action


## Sets the target string to a string out of the necessary dialogue array using the given index
func set_text(id:int) -> void:
	dialogue_visible = true
	dialogue.modulate.a = 1.0
	dialogue.set_snaily_text("")
	char_timeout = 0.0
	current_char = 0
	if is_sun:
		target_str = dialogue_sun[id]
	else:
		target_str = dialogue_moon[id]


## Fades the dialogue label
func fade_text() -> void:
	dialogue_visible = false


## Spawns and configures the fade for the credits
func spawn_credits() -> void:
	stars.despawn()
	var credits:EndingCredits = load("res://Scenes/UI/EndingComponents/EndingCredits.tscn").instantiate()
	GameCore.instance.add_child(credits)
	credits.setup(is_sun)
	queue_free()
