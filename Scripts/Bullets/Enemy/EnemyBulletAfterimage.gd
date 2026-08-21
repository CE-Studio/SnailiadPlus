# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name EnemyBulletAfterimage
extends EnemyBullet

const DAMAGE_MULT:float = 0.25
const MAX_LIFE_TIME:float = 0.3

## How long it takes the sprite to fade out
var fade_time:float = MAX_LIFE_TIME
## A boolean inferred from the game settings that controls how opaque the afterimage is upon spawning
var opaque:bool = ProjectSettings.get_setting("game/visuals/opaque_afterimages")


func _spawn_afterimage(_parent:EnemyBullet) -> void:
	damage = floori(_parent.damage * DAMAGE_MULT)
	box.shape.size = _parent.box.shape.size
	vis.rect = _parent.vis.rect
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
	sprite.modulate.a = inverse_lerp(0.0, MAX_LIFE_TIME, fade_time) - 0.25
	if not opaque:
		sprite.modulate.a *= 0.4
	else:
		sprite.modulate.a *= 0.8
	fade_time -= delta
	if fade_time <= 0.0:
		_despawn()
