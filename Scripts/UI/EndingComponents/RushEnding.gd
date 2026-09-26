# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name RushEnding
extends Node2D


#region Variables
const CENTER:Vector2 = Vector2(200, 120)
const SUN_GRAVITY:float = 1200.0
const PARTICLE_COUNT:int = 64
const PARTICLE_MAX_DIST:float = 256.0
const PARTICLE_DURATION:float = 4.0

var sun_pos:Vector2 = Vector2.ZERO
var tick_sun:bool = false
var sun_velocity:Vector2 = Vector2(-110.0, -300.0)

@export var sun:SnailySprite2D
#endregion


func _process(delta: float) -> void:
	if tick_sun:
		sun.position += sun_velocity * delta
		sun_velocity.y += SUN_GRAVITY * delta


func set_sun_pos(pos:Vector2) -> void:
	sun_pos = pos
	sun.position = pos


func fade_ui_out() -> void:
	UICore.instance.fade_ui(0.0, 2.5)


func kill_sun() -> void:
	tick_sun = true
	for i in range(PARTICLE_COUNT):
		var new_burst_dot:Particle = Statics.spawn_particle("BurstDot", Room.Layers.FG2,
			sun_pos, [ PARTICLE_DURATION, randf() * PARTICLE_MAX_DIST, CENTER ])
		match randi() % 5:
			0: new_burst_dot.sprite.modulate = Statics.get_color(Vector2i(3, 4))
			1: new_burst_dot.sprite.modulate = Statics.get_color(Vector2i(3, 9))
			2: new_burst_dot.sprite.modulate = Statics.get_color(Vector2i(2, 7))
			3: new_burst_dot.sprite.modulate = Statics.get_color(Vector2i(0, 1))
			4: new_burst_dot.sprite.modulate = Statics.get_color(Vector2i(2, 2))
