class_name StarLayer
extends ParticleLayer


const STARS_PER_UNIT:int = 60

@export_range(0.1, 16.0, 0.1) var density:float = 1.0:
	set(value):
		density = value
		particle_count = roundi(STARS_PER_UNIT * density)
@export_range(0.1, 8.0, 0.1) var speed_mod:float = 1.0
@export_enum(
	"North", "Northeast", "East", "Southeast", "South",
	"Southwest", "West", "Northwest", "Inward", "Outward"
) var direction:int = 6


func spawn() -> void:
	var option:int = ProjectSettings.get_setting("game/world/particles")
	if not (option == Statics.ParticleOptions.ENVIRONMENTS
	or option == Statics.ParticleOptions.FLASH
	or option == Statics.ParticleOptions.ALL):
		queue_free()
	else:
		super()


func _spawn_one() -> void:
	var this_particle:String = particles[randi_range(0, particles.size() - 1)]
	var center = STATIC_POSITION if static_position else UICore.instance.get_cam_center_pos()
	var spawn_pos = center + Vector2(
		randf_range(-WRAP_BOUNDS.x, WRAP_BOUNDS.x),
		randf_range(-WRAP_BOUNDS.y, WRAP_BOUNDS.y)
	)
	var target_layer:Room.Layers = Room.Layers.SKY if randf() < 0.5 else Room.Layers.BG2
	var new_particle:Particle
	if move_with_camera:
		new_particle = Statics.spawn_particle_cam_synced(this_particle,
		target_layer, spawn_pos, [direction, speed_mod])
	else:
		new_particle = Statics.spawn_particle(this_particle,
		target_layer, spawn_pos, [direction, speed_mod])
	active_particles.append(new_particle)
