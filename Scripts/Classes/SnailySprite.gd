# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name SnailySprite2D
extends AnimatedSprite2D


@export var meta_info:Array = []
@export var start_on_random_frame:Array[String] = []
@export var start_all_on_random:bool = false
@export var hide_on_finish:bool = false


func _ready() -> void:
	connect("animation_changed", _on_anim_changed)
	connect("animation_finished", _on_anim_finished)


## Takes an array of animations and selects one at random to play
func play_random(anims:PackedStringArray) -> void:
	var rand_i:int = randi_range(0, anims.size() - 1)
	var rand_anim:String = anims[rand_i]
	play(rand_anim)


## Selects a random animation to play out of all animations tied to this sprite
func play_any_random() -> void:
	var anims:PackedStringArray = sprite_frames.get_animation_names()
	play_random(anims)


## Called to set this sprite's speed scale to a random range, or a single value if only one is given
func set_speed(_min:float, _max:float = -1.0) -> void:
	if _max == -1.0: speed_scale = _min
	else: speed_scale = randf_range(_min, _max)


## Called when a new animation is set to play. Used here to check for a required random start
## and apply it if so
func _on_anim_changed() -> void:
	visible = true
	if start_all_on_random or start_on_random_frame.has(animation):
		var frame_count:int = sprite_frames.get_frame_count(animation)
		frame = randi_range(0, frame_count - 1)


## Called when the current animation finishes. Used here to check if the sprite should be hidden when
## the animation is finished
func _on_anim_finished() -> void:
	if hide_on_finish: visible = false
