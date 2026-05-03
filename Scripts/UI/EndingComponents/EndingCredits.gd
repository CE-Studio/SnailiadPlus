# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
extends Node2D


#region Variables
## Will be set to [code]true[/code] if the credits are currently fading out
var fading_out:bool = false

## The [StarLayer] shown behind the credits
@export var stars:StarLayer
## The fade that appears over everything
@export var cover:Sprite2D
## The main credits music
@export var music_main:AudioStreamPlayer
## The alternate credits music
@export var music_alt:AudioStreamPlayer
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


func _process(delta: float) -> void:
	if not fading_out:
		cover.modulate.a = move_toward(cover.modulate.a, 0.0, delta)
