# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name PlayerBulletAfterimage
extends PlayerBullet


const DAMAGE_MULT:float = 0.25
const MAX_LIFE_TIME:float = 0.3

## How long it takes the sprite to fade out
var fade_time:float = MAX_LIFE_TIME
## A boolean inferred from the game settings that controls how opaque the afterimage is upon spawning
var opaque:bool = ProjectSettings.get_setting("game/visuals/opaque_afterimages")

@onready var _sprite:Sprite2D = $"Sprite2D"


func _spawn_afterimage(_parent:PlayerBullet) -> void:
	damage = floori(_parent.damage * DAMAGE_MULT)
	box_normal.shape.size = _parent.box_normal.shape.size
	box_normal.disabled = _parent.powered
	box_power.shape.size = _parent.box_power.shape.size
	box_power.disabled = not _parent.powered
	vis.rect = _parent.vis.rect
	#_sprite.texture = _parent.sprite.texture
	#_sprite.hframes = parent.sprite.data["tiles"][0]
	#_sprite.vframes = parent.sprite.data["tiles"][1]
	#_sprite.flip_h = _parent.sprite.flip_h
	#_sprite.flip_v = _parent.sprite.flip_v
	#_sprite.frame_coords = Vector2i(
	#	parent.sprite.frame_coords.x + int(parent.sprite.meta["afterimage_offset_x"]),
	#	parent.sprite.frame_coords.y + int(parent.sprite.meta["afterimage_offset_y"])
	#)
	sprite.sprite_frames = _parent.sprite.sprite_frames
	if _parent.sprite.sprite_frames.has_animation(_parent.sprite.animation + "_aftimg"):
		sprite.play(_parent.sprite.animation + "_aftimg")
	else:
		sprite.play(_parent.sprite.animation)
	sprite.frame = _parent.sprite.frame
	sprite.pause()
	sprite.flip_h = _parent.sprite.flip_h
	sprite.flip_v = _parent.sprite.flip_v


func _physics_process(delta: float) -> void:
	super(delta)
	modulate.a = inverse_lerp(0.0, MAX_LIFE_TIME, fade_time) - 0.25
	if not opaque:
		modulate.a *= 0.4
	else:
		modulate.a *= 0.8
	fade_time -= delta
	if fade_time <= 0.0:
		despawn()
