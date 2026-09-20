# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name ScrollingTilesLayer
extends ParticleLayer


const TILES_PER_UNIT:int = 12

@export_range(0.1, 16.0, 0.1) var density:float = 1.0:
	set(value):
		density = value
		particle_count = roundi(TILES_PER_UNIT * density)
@export_range(0.1, 8.0, 0.1) var speed_mod:float = 1.0
@export_enum("None", "Left", "Right", "Both") var horizontal:int = 0
@export_enum("None", "Up", "Down", "Both") var vertical:int = 3


func spawn() -> void:
	var option:int = ProjectSettings.get_setting("game/world/particles")
	if not (option == Statics.ParticleOptions.ENVIRONMENTS
	or option == Statics.ParticleOptions.ALL):
		queue_free()
	else:
		if horizontal == 0 and vertical == 0:
			vertical = 3
		super()


func _spawn_one() -> void:
	var this_particle:String = particles[randi_range(0, particles.size() - 1)]
	var center = STATIC_POSITION if static_position else UICore.instance.get_cam_center_pos()
	var spawn_pos = center + Vector2(
		randf_range(-WRAP_BOUNDS.x, WRAP_BOUNDS.x),
		randf_range(-WRAP_BOUNDS.y, WRAP_BOUNDS.y)
	) * 1.5
	var target_layer:Room.Layers = Room.Layers.BG1 if randf() < 0.5 else Room.Layers.BG2
	var new_particle:Particle
	
	var direction:Vector2 = Vector2.ZERO
	var axis:bool = false
	if horizontal != 0 and vertical != 0:
		axis = randf() > 0.5
	else:
		axis = vertical != 0
	if axis:
		match vertical:
			1: direction = Vector2.UP
			2: direction = Vector2.DOWN
			3: direction = Vector2.UP if randf() < 0.5 else Vector2.DOWN
	else:
		match horizontal:
			1: direction = Vector2.LEFT
			2: direction = Vector2.RIGHT
			3: direction = Vector2.LEFT if randf() < 0.5 else Vector2.RIGHT
	
	if move_with_camera:
		new_particle = Statics.spawn_particle_cam_synced(this_particle,
		target_layer, spawn_pos, [direction, speed_mod])
	else:
		new_particle = Statics.spawn_particle(this_particle,
		target_layer, spawn_pos, [direction, speed_mod])
	active_particles.append(new_particle)


func _tick_wrapped_particle(_particle:Particle, _difference:Vector2) -> void:
	super(_particle, _difference)
	_particle.sprite._set_random_frame()
