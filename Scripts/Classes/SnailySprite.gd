# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name SnailySprite2D
extends AnimatedSprite2D



## Fully customizable array meant to hold any externally adjustable information this sprite
## or its parent node can make use of.
@export var meta_info:Array = []
## Array containing the names of every animation that is allowed to start on a random frame.
@export var start_on_random_frame:Array[String] = []
## If [code]true[/code], every animation associated with this sprite will start on a random frame.
@export var start_all_on_random_frame:bool = false
## If [code]true[/code], this sprite will select any of its associated animations to autoplay when
## added to the tree.
@export var autoplay_any_on_spawn:bool = false
## When set, this value will be passed as an animation name that will automatically start playing
## as soon as the currently active animation finishes. This value is cleared as soon as it is read.
@export var autoplay_next:String = ""
## Will be used on creation to select a random multiplier that will be applied to the playback
## speed of all animations until [param set_speed()] is called again. The vector's X component
## is used as the minimum value, and the Y component is used as the maximum value.
@export var spawn_speed_variance:Vector2 = Vector2.ONE
## If [code]true[/code], this sprite will be hidden the instant a non-looping animation finishes playing.
@export var hide_on_finish:bool = false

@export_group("Infer Additional Animations")
## If [code]true[/code], this sprite, when added to the tree, will assess its existing animation set
## and generate new animations in accordance with the parameters below.
@export var infer_new:bool = false
## Marks the specific string key that is looked for and replaced in the existing animations' names.
@export var replace_key:String = "00."
## Denotes the number of additional sets of animations to create.
@export var add_count:int = 0
## The offset in pixels of the first generated animation set's frames relative to the base animation
## set's frames.
@export var rect_offset:Vector2i = Vector2i(0, 16)
## A direct reference to the texture that should be used to generate new animations.
@export var source:Texture2D
## An optional array containing any special keys that will replace [param replace_key] when
## an animation is created. If this array's size is less than the number of animation sets being
## generated, the remaining sets will be numbered as if this array was empty.
@export var specialized_keys:Array[String] = []


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
	if infer_new:
		assert(source, "A source Texture2D must be supplied for a SnailySprite2D to infer new animations.")
	if not infer_new or add_count <= 0 or not source or Engine.is_editor_hint():
		return
	var anims:PackedStringArray = sprite_frames.get_animation_names()
	for anim in anims:
		if not anim.contains(replace_key):
			continue
		var frames:int = sprite_frames.get_frame_count(anim)
		var rects:Array[Rect2] = []
		for i in range(frames):
			var this_source:AtlasTexture = sprite_frames.get_frame_texture(anim, i)
			rects.append(this_source.region)
		for i in range(1, add_count + 1):
			var new_anim:String = anim.replace(replace_key, "%02d." % i)
			if i <= specialized_keys.size():
				new_anim = anim.replace(replace_key, specialized_keys[i - 1])
			elif replace_key == "0":
				new_anim = anim.replace(replace_key, str(i))
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
			#print("Added " + new_anim)


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
