# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name EmoteLayer
extends Node2D


enum Emotes {
	ZZZ,
	SURPRISE
}

var actor_size:Vector2 = Vector2(16, 8)
var flip_size:bool = false
var active_particles:Array[Particle] = []
var active_duration:float = 0.0
var clear_after_duration:bool = false


func _process(delta: float) -> void:
	if clear_after_duration:
		active_duration -= delta
		if active_duration <= 0.0:
			clear()
			clear_after_duration = false


func clear() -> void:
	for particle in active_particles:
		particle.queue_free()
	active_particles.clear()


func apply_offset(offset:Vector2) -> Vector2:
	if flip_size:
		offset = Vector2(
			abs(offset.y) * sign(offset.x),
			abs(offset.x) * sign(offset.y)
		)
	return position + offset


func set_duration(duration:float) -> void:
	if duration < 0.0:
		clear_after_duration = false
		return
	clear_after_duration = true
	active_duration = duration


func emote_from_enum(emote:Emotes, duration:float = -1.0) -> void:
	match emote:
		Emotes.ZZZ: zzz(duration)
		Emotes.SURPRISE: surprise(duration)


#region Emote funcs
func zzz(duration:float = -1.0) -> void:
	clear()
	var particle:Particle = Statics.spawn_particle("Zzz", Room.Layers.GROUND, position)
	particle.reparent(self)
	particle.position = apply_offset(actor_size * Vector2(1, -1) + Vector2(0, 4))
	active_particles.append(particle)
	set_duration(duration)


func surprise(duration:float = -1.0) -> void:
	clear()
	var particle:Particle = Statics.spawn_particle("Surprise", Room.Layers.GROUND, position)
	particle.reparent(self)
	particle.position = apply_offset(actor_size * Vector2(1, -1))
	active_particles.append(particle)
	set_duration(duration)
#endregion
