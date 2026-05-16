# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name EndingFade
extends Node2D


const FADE_START_TIME:float = 2.0
const FADE_END_TIME:float = 6.5
const END_SPAWN_TIME:float = 10.0
const FADE_OUT_TIME:float = 4.0
const FADE_OUT_THRESHOLD:float = 6.6

static var instance:EndingFade

var elapsed:float = 0.0
var started_ui_fade:bool = false
var fade_in:bool = true
var do_giga_fadein:bool = true
var started_fade_out:bool = false
var explosion_point:Vector2 = Vector2.ZERO
var explosion:ExplosionBossDefeat


func _ready() -> void:
	instance = self
	modulate.a = 0.0


func _process(delta: float) -> void:
	if fade_in:
		modulate.a = inverse_lerp(FADE_START_TIME, FADE_END_TIME, elapsed)
		if elapsed >= FADE_START_TIME and not started_ui_fade:
			UICore.instance.fade_ui(0.0, FADE_END_TIME - FADE_START_TIME)
			started_ui_fade = true
		elapsed += delta
		if elapsed > END_SPAWN_TIME:
			var cutscene:EndingCutscene = load("res://Scenes/UI/EndingComponents/EndingCutscene.tscn").instantiate()
			GameCore.instance.add_child(cutscene)
			get_tree().paused = true
			fade_in = false
			elapsed = 0.0
			modulate.a = 1.0
	else:
		if not started_fade_out:
			started_fade_out = true
			UICore.instance.fade_ui(1.0, FADE_OUT_TIME)
			z_index += 100
			if do_giga_fadein and explosion_point != Vector2.ZERO:
				UICore.instance.call_screen_shake_radial([2.0, 4.5, 1.0, 0.5, 0.0], UICore.ShakeCallMode.OVERWRITE_ALL)
				explosion = Statics.spawn_particle(
					"ExplosionBossDefeat", Room.Layers.GROUND, explosion_point, [true, 5.0, false, true])
		modulate.a -= delta / FADE_OUT_TIME
		if explosion:
			explosion.set_all_vol_linear(clampf(elapsed * 0.25, 0.0, 1.0))
		if modulate.a < 0.0 and not do_giga_fadein:
			queue_free()
		if elapsed >= FADE_OUT_THRESHOLD:
			GameCore.instance.current_room.open_all_boss_doors()
			Statics.increment_igt = true
			queue_free()
		elapsed += delta
