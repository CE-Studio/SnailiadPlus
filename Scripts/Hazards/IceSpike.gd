# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
@tool
class_name IceSpike
extends Hazard


const MAX_SHINE_COOLDOWN:float = 6.0

var shine_cooldown:float = 0.0
var dec_cooldown:bool = false

@export var direction:Statics.DirsSurface = Statics.DirsSurface.FLOOR:
	set(value):
		if value == Statics.DirsSurface.NONE:
			value = Statics.DirsSurface.FLOOR
		direction = value
		if Engine.is_editor_hint():
			var _sprite:MarkerSprite = $"SnailySprite2D/MarkerSprite"
			match direction:
				Statics.DirsSurface.FLOOR:
					_sprite.rotation_degrees = 0.0
				Statics.DirsSurface.LWALL:
					_sprite.rotation_degrees = 90.0
				Statics.DirsSurface.RWALL:
					_sprite.rotation_degrees = -90.0
				Statics.DirsSurface.CEILING:
					_sprite.rotation_degrees = 180.0
@export var sprite:SnailySprite2D


func _ready() -> void:
	super()
	match direction:
		Statics.DirsSurface.FLOOR:
			sprite.play("d")
		Statics.DirsSurface.LWALL:
			sprite.play("l")
		Statics.DirsSurface.RWALL:
			sprite.play("r")
			sprite.flip_h = true
		Statics.DirsSurface.CEILING:
			sprite.play("u")
			sprite.flip_v = true
	shine_cooldown = randf() * MAX_SHINE_COOLDOWN


func _process(delta:float) -> void:
	if dec_cooldown:
		shine_cooldown -= delta
	if shine_cooldown <= 0.0:
		shine_cooldown = randf() * MAX_SHINE_COOLDOWN
		sprite.autoplay_next = sprite.animation
		sprite.play(sprite.animation + "_shine")


func _on_sprite_anim_changed() -> void:
	dec_cooldown = not dec_cooldown
