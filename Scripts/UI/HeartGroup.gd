# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name HeartGroup
extends Node2D


var hearts:Array[SnailySprite2D] = []
@export var heart_sprite_frames:SpriteFrames


func draw_new_hearts() -> void:
	for heart in hearts:
		heart.queue_free()
	hearts.clear()
	var max_hp:int = Player.instance.max_health
	var health_per_heart:int = Statics.HEALTH_PER_HEART[Statics.current_profile["difficulty"]]
	var running_total:int = 0
	var heart_count:int = 0
	var origin:Vector2i = Vector2i(8, 8)
	var spacing:Vector2i = Vector2i(8, 8)
	var hearts_per_row:int = 7
	while running_total < max_hp:
		var new_heart:SnailySprite2D = SnailySprite2D.new()
		new_heart.sprite_frames = heart_sprite_frames
		add_child(new_heart)
		hearts.append(new_heart)
		var pos:Vector2 = Vector2(origin.x + ((heart_count % hearts_per_row) * spacing.x),
		origin.y + floori((heart_count / hearts_per_row) * spacing.y))
		new_heart.position = pos
		heart_count += 1
		running_total += health_per_heart
	call_deferred("update_hearts")


func update_hearts() -> void:
	var health:int = Player.instance.health
	var health_per_heart:int = Statics.HEALTH_PER_HEART[Statics.current_profile["difficulty"]]
	var running_total:int = 0
	for heart in hearts:
		var this_heart_value:int = clampi(health - running_total, 0, health_per_heart)
		var anim_name:String
		match int(Statics.current_profile["difficulty"]):
			0: anim_name = "easy_"
			1: anim_name = "normal_"
			2: anim_name = "absurd_"
		heart.play(anim_name + str(this_heart_value))
		running_total += health_per_heart
