@icon("res://Editor/ico/ParticleLayer.svg")
class_name ParticleLayer
extends Node2D


#region Variables
const WRAP_BOUNDS:Vector2 = Vector2(232, 152)
const WRAP_DIST:Vector2 = Vector2(464, 304)
const STATIC_POSITION:Vector2 = Statics.VECTOR_CENTER

@export var particles:Array[String] = []
@export var spawn_delay:float = 0.0
@export var spawn_all_at_once:bool = true
@export var particle_count:int = 0
@export var static_position:bool = false
@export var move_with_camera:bool = false
@export var wrap_at_edges:bool = true
@export var z_override:int = 0
@export var always_process:bool = false

var active_particles:Array[Particle] = []
var spawn_cooldown:float = 0.0
var initialized:bool = false
#endregion


func spawn() -> void:
	if not UICore.instance and not static_position:
		return
	
	if spawn_all_at_once:
		for i in range(particle_count):
			_spawn_one()
	
	if z_override != 0 or always_process:
		for particle in active_particles:
			if z_override != 0:
				particle.z_index = z_override
				particle.z_as_relative = false
			if always_process:
				particle.process_mode = Node.PROCESS_MODE_ALWAYS
	
	initialized = true


func _process(delta: float) -> void:
	if (not UICore.instance and not static_position) or not initialized:
		return
	
	for i in range(active_particles.size() - 1, -1, -1):
		if active_particles[i] == null:
			active_particles.remove_at(i)
	
	spawn_cooldown -= delta
	while spawn_cooldown <= 0.0 and active_particles.size() < particle_count:
		_spawn_one()
		spawn_cooldown = spawn_delay
	
	var cam_center:Vector2 = STATIC_POSITION
	var wrap_offset:Vector2 = Vector2.ZERO
	if UICore.instance:
		var center:Vector2 = UICore.instance.get_cam_center_pos()
		if move_with_camera:
			cam_center = center
		else:
			wrap_offset = center - STATIC_POSITION
	#if static_position:
	#	cam_center = STATIC_POSITION
	#elif move_with_camera:
	#	cam_center = STATIC_POSITION
	if wrap_at_edges:
		for particle in active_particles:
			while particle.global_position.x < cam_center.x - WRAP_BOUNDS.x + wrap_offset.x:
				particle.position.x += WRAP_DIST.x
			while particle.global_position.x > cam_center.x + WRAP_BOUNDS.x + wrap_offset.x:
				particle.position.x -= WRAP_DIST.x
			while particle.global_position.y < cam_center.y - WRAP_BOUNDS.y + wrap_offset.y:
				particle.position.y += WRAP_DIST.y
			while particle.global_position.y > cam_center.y + WRAP_BOUNDS.y + wrap_offset.y:
				particle.position.y -= WRAP_DIST.y


func _spawn_one() -> void:
	var this_particle:String = particles[randi_range(0, particles.size() - 1)]
	var center = STATIC_POSITION if static_position else UICore.instance.get_cam_center_pos()
	var spawn_pos = center + Vector2(
		randf_range(-WRAP_BOUNDS.x, WRAP_BOUNDS.x),
		randf_range(-WRAP_BOUNDS.y, WRAP_BOUNDS.y)
	)
	var new_particle:Particle
	if move_with_camera:
		new_particle = Statics.spawn_particle_cam_synced(this_particle, Room.Layers.GROUND, spawn_pos)
	else:
		new_particle = Statics.spawn_particle(this_particle, Room.Layers.GROUND, spawn_pos)
	active_particles.append(new_particle)


func despawn() -> void:
	for particle in active_particles:
		particle.queue_free()
	queue_free()


func force_process() -> void:
	always_process = true
	for particle in active_particles:
		particle.process_mode = Node.PROCESS_MODE_ALWAYS
