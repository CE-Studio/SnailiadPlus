class_name PlayerBulletAfterimage
extends PlayerBullet


const DAMAGE_MULT:float = 0.25
const MAX_LIFE_TIME:float = 0.3

var fade_time:float = MAX_LIFE_TIME
var opaque:bool = ProjectSettings.get_setting("game/visuals/opaque_afterimages")

@onready var _sprite:Sprite2D = $"Sprite2D"


func _spawn_afterimage(parent:PlayerBullet) -> void:
	damage = floori(parent.damage * DAMAGE_MULT)
	box.shape.size = parent.box.shape.size
	vis.rect = parent.vis.rect
	_sprite.texture = parent.sprite.texture
	_sprite.hframes = parent.sprite.data["tiles"][0]
	_sprite.vframes = parent.sprite.data["tiles"][1]
	_sprite.flip_h = parent.sprite.flip_h
	_sprite.flip_v = parent.sprite.flip_v
	_sprite.frame_coords = Vector2i(
		parent.sprite.frame_coords.x + int(parent.sprite.meta["afterimage_offset_x"]),
		parent.sprite.frame_coords.y + int(parent.sprite.meta["afterimage_offset_y"])
	)


func _physics_process(delta: float) -> void:
	super(delta)
	_sprite.modulate.a = inverse_lerp(0.0, MAX_LIFE_TIME, fade_time)
	if not opaque:
		_sprite.modulate.a *= 0.5
	fade_time -= delta
	if fade_time <= 0.0:
		despawn()
