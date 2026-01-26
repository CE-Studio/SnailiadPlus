class_name EmberLayer
extends ParticleLayer


const EMBERS_PER_UNIT:int = 16

@export_range(0.1, 8.0, 0.1) var density:float = 1:
	set(value):
		density = value
		particle_count = roundi(EMBERS_PER_UNIT * density)


func spawn() -> void:
	var option:int = ProjectSettings.get_setting("game/world/particles")
	if not (option == Statics.ParticleOptions.ENVIRONMENTS
	or option == Statics.ParticleOptions.ALL):
		queue_free()
	else:
		super()


func _spawn_one() -> void:
	var this_particle:String = particles[randi_range(0, particles.size() - 1)]
	var center = Vector2(200, 120) if static_position else UICore.instance.get_cam_center_pos()
	var spawn_pos = center + Vector2(
		randf_range(-WRAP_BOUNDS.x, WRAP_BOUNDS.x),
		WRAP_BOUNDS.y
	)
	var target_layer:Room.Layers = (Room.Layers.BG2 + randi_range(0, 3)) as Room.Layers
	var new_particle = Statics.spawn_particle_cam_synced(this_particle, target_layer, spawn_pos)
	active_particles.append(new_particle)
