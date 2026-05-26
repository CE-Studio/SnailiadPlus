# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name DamageNumber
extends Node2D


const ANIM_ROLLBACK_THRESHOLD:float = 0.6
const HIT_ELAPSED_THRESHOLD:float = 0.45

var elapsed:float = 0.0
var elapsed_since_last_hit:float = 0.0
var vel:float = 0.0
var value:int = 0

@export var text:SnailyText
@export var anim:AnimationPlayer


func _ready() -> void:
	vel = (randf() - 0.5) * 32.0


func instance(num:int, color:Color):
	text.set_snaily_text(str(num))
	value = num
	text.modulate = color


func add(num:int) -> void:
	value += num
	text.set_snaily_text(str(value))
	if anim.current_animation_position > ANIM_ROLLBACK_THRESHOLD:
		anim.seek(ANIM_ROLLBACK_THRESHOLD)
	elapsed_since_last_hit = 0.0


func _process(delta: float) -> void:
	if elapsed <= ANIM_ROLLBACK_THRESHOLD:
		position.x += vel * delta
	elapsed += delta
	elapsed_since_last_hit += delta


func _despawn() -> void:
	queue_free()


func is_below_hit_threshold() -> bool:
	return elapsed_since_last_hit <= HIT_ELAPSED_THRESHOLD


func is_same_color(new_col:Color) -> bool:
	return new_col == text.modulate
