class_name DamageNumber
extends Node2D


var elapsed:float = 0.0
var vel:float = 0.0


func _ready() -> void:
	vel = (randf() - 0.5) * 32.0


func instance(num:int, color:Color):
	var text:SnailyText = $"SnailyText"
	text.set_snaily_text_raw(str(num))
	text.modulate = color


func _process(delta: float) -> void:
	if elapsed <= 0.6:
		position.x += vel * delta
	elapsed += delta


func _despawn() -> void:
	queue_free()
