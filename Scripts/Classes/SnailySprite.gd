# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name SnailySprite2D
extends AnimatedSprite2D



@export var meta_info:Array = []
@export var start_on_random_frame:Array[String] = []
@export var start_all_on_random_frame:bool = false
@export var autoplay_any_on_spawn:bool = false
@export var autoplay_next:String = ""
@export var spawn_speed_variance:Vector2 = Vector2.ONE
@export var hide_on_finish:bool = false

@export_group("Infer Additional Animations")
@export var infer_new:bool = false
@export var add_count:int = 0
@export var rect_offset:Vector2i = Vector2i(0, 16)
@export var source:Texture2D


func _ready() -> void:
	connect("animation_changed", _on_anim_changed)
	connect("animation_finished", _on_anim_finished)
	set_speed(spawn_speed_variance.x, spawn_speed_variance.y)
	_infer_new()
	if autoplay != "" or autoplay_any_on_spawn:
		if autoplay_any_on_spawn:
			play_any_random()
		if start_all_on_random_frame or start_on_random_frame.has(autoplay):
			_set_random_frame()
		else:
			frame = 0


## Infers and creates additional sets of animations using the existing animations as a basis.
## This could be expensive and halt the game for a second if enough animations need to be made,
## so try to use sparingly if possible (e.g. player shell states)
func _infer_new() -> void:
	if not infer_new or add_count <= 0 or not source:
		return
	var anims:PackedStringArray = sprite_frames.get_animation_names()
	for anim in anims:
		if not anim.contains("00."):
			continue
		var frames:int = sprite_frames.get_frame_count(anim)
		var rects:Array[Rect2] = []
		for i in range(frames):
			var this_source:AtlasTexture = sprite_frames.get_frame_texture(anim, i)
			rects.append(this_source.region)
		for i in range(1, add_count + 1):
			var new_anim:String = anim.replace("00.", "%02d." % i)
			if anims.has(new_anim):
				continue
			sprite_frames.add_animation(new_anim)
			sprite_frames.set_animation_speed(new_anim, sprite_frames.get_animation_speed(anim))
			sprite_frames.set_animation_loop_mode(new_anim, sprite_frames.get_animation_loop_mode(anim))
			for j in rects.size():
				var new_tex:AtlasTexture = AtlasTexture.new()
				new_tex.atlas = source
				new_tex.region = rects[j]
				new_tex.region.position += (Vector2(rect_offset) * i)
				sprite_frames.add_frame(new_anim, new_tex)


## Takes an array of animations and selects one at random to play
func play_random(anims:PackedStringArray) -> void:
	var rand_i:int = randi_range(0, anims.size() - 1)
	var rand_anim:String = anims[rand_i]
	play(rand_anim)
	if start_on_random_frame.has(rand_anim):
		_set_random_frame()


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
	if start_all_on_random_frame or start_on_random_frame.has(animation):
		_set_random_frame()


## Called when the current animation finishes. Used here to check if the sprite should be hidden when
## the animation is finished, as well as to check for queued autoplay animations
func _on_anim_finished() -> void:
	if autoplay_next != "":
		if autoplay_next.to_lower() == "__any__":
			play_any_random()
		else:
			play(autoplay_next)
		autoplay_next = ""
	elif hide_on_finish: visible = false


## Sets the current frame of the active animation to a random frame
func _set_random_frame() -> void:
	var frame_count:int = sprite_frames.get_frame_count(animation)
	frame = randi_range(0, frame_count - 1)
