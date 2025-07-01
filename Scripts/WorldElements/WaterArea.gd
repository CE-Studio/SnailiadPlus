class_name WaterArea
extends EnvironmentArea


func _physics_process(delta: float) -> void:
	super(delta)


func _on_body_enter(body) -> void:
	if spawn_grace_frames <= 0:
		var edge_data:Array = get_closest_point(body.position)
		call_splash(edge_data[0], edge_data[1], true)
	super(body)


func _on_body_exit(body) -> void:
	if spawn_grace_frames <= 0:
		var edge_data = get_closest_point(body.position)
		if body is not Enemy or body.environment == self:
			call_splash(edge_data[0], edge_data[1], false)
	super(body)


func call_splash(pos:Vector2, normal:Vector2, make_bubbles:bool) -> void:
	var particle_setting = Statics.data_general["particle_state"]
	if (particle_setting != Statics.ParticleOptions.ENVIRONMENTS
	and particle_setting != Statics.ParticleOptions.ALL):
		return
	
	Statics.spawn_particle("SplashTop", Room.Layers.GROUND, pos + (normal * 8))
	#TODO: shader option, cacophony of splashes on deload (bool read_interactions?)
