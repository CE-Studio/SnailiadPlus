# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name MovingPlatform
extends Node2D

#region Variables
const NEUTRAL_BUFFER:float = 2.0

@export_range(1, 4, 1) var size:int = 2
@export_enum("Blue", "Pink") var type:int = 0
@export var path_anim:StringName = ""

var last_pos:Vector2 = Vector2.ZERO
var last_anim:String = ""

@export_group("Components")
@export var body:AnimatableBody2D
@export var col:CollisionShape2D
@export var sprite:SnailySprite2D
@export var anim:AnimationPlayer
#endregion


func _ready() -> void:
	col.shape = RectangleShape2D.new()
	col.shape.size = Vector2(16 * size, 16)
	if anim.has_animation(path_anim):
		anim.play(path_anim)


func _physics_process(_delta: float) -> void:
	var state:String = "neutral"
	if last_pos != body.position:
		var dir:Vector2 = last_pos.direction_to(body.position)
		if abs(dir.y) > abs(dir.x):
			state = "up" if dir.y < 0 else "down"
		else:
			state = "left" if dir.x < 0 else "right"
	var anim_name:String = "%d_%d_%s" % [ type, size, state ]
	if anim_name != last_anim:
		sprite.play(anim_name)
		last_anim = anim_name
	last_pos = body.position
