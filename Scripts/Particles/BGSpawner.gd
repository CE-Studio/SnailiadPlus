@icon("res://Editor/ico/BGSpawner.svg")
class_name BGSpawner
extends Node2D


#region Variables
@export var spawn_count:int = 4
#@export_enum("Bird") var spawn_type:int = 0
@export var spawn_particle:String = "BGBird"

var box:CollisionShape2D
#endregion


func _ready() -> void:
	if GameCore.instance == null:
		return
	var particle_setting = ProjectSettings.get_setting("game/world/particles")
	if (particle_setting != Statics.ParticleOptions.ENVIRONMENTS
	and particle_setting != Statics.ParticleOptions.ALL):
		return
	
	var children = get_children()
	for child in children:
		if child is CollisionShape2D:
			box = child
	assert(box != null, "No CollisionShape2D found as a child of %s!" % name)
	assert(spawn_particle != null, "No particle set for %s to spawn!" % name)
	
	_spawn_particles.call_deferred()


func _spawn_particles() -> void:
	for i in spawn_count:
		var bounds:Vector2 = box.shape.size * 0.5
		var rand_position:Vector2 = Vector2(
			randf_range(-bounds.x, bounds.x),
			randf_range(-bounds.y, bounds.y)
		) + position
		Statics.spawn_particle(spawn_particle, Room.Layers.SKY, position, [bounds, rand_position])
