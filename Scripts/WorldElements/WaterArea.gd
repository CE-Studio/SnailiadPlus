class_name WaterArea
extends EnvironmentArea


@export_range(0.0, 256.0, 1.0) var bubble_count = 16
var active_bubbles:Array = []


func _ready() -> void:
	super()
	call_deferred("spawn_initial_bubbles")


func spawn_initial_bubbles() -> void:
	var particle_setting = Statics.data_general["particle_state"]
	if (particle_setting != Statics.ParticleOptions.ENVIRONMENTS
	and particle_setting != Statics.ParticleOptions.FLASH
	and particle_setting != Statics.ParticleOptions.ALL):
		return
	
	for i in range(bubble_count):
		var box:CollisionShape2D = boxes[randi_range(0, boxes.size() - 1)]
		var box_size = box.shape.size * 0.5
		var spawn_pos = Vector2(
			randf_range(-box_size.x, box_size.x),
			randf_range(-box_size.y, box_size.y)
		) + box.global_position
		var layer = randi_range(3, 5)
		var bubble = Statics.spawn_particle("Bubble", layer, spawn_pos)
		active_bubbles.append(bubble)
		bubble.home = box


func _physics_process(delta: float) -> void:
	super(delta)


func _on_body_enter(body) -> void:
	if spawn_grace_frames <= 0 and read_interactions:
		var edge_data:Array = get_closest_point(body.position)
		var speed:float
		if edge_data[1] == Vector2.UP or edge_data[1] == Vector2.DOWN:
			speed = abs(body.velocity.y)
		else:
			speed = abs(body.velocity.x)
		var lerped_speed = inverse_lerp(0.0, 512.0, speed)
		call_splash(edge_data[0], edge_data[1], lerped_speed, edge_data[2])
	super(body)


func _on_body_exit(body) -> void:
	if spawn_grace_frames <= 0 and read_interactions:
		var edge_data = get_closest_point(body.position)
		if body is not Enemy or body.environment == self:
			call_splash(edge_data[0], edge_data[1])
	super(body)


func call_splash(pos:Vector2, normal:Vector2, make_bubbles:float = 0, home_box:CollisionShape2D = null) -> void:
	var particle_setting = Statics.data_general["particle_state"]
	if (particle_setting != Statics.ParticleOptions.ENVIRONMENTS
	and particle_setting != Statics.ParticleOptions.ALL):
		return
	if not Statics.is_point_on_screen(pos, Vector2(8, 8)):
		return
	
	Statics.spawn_particle("SplashTop", Room.Layers.GROUND, pos + (normal * 8))
	if make_bubbles > 0:
		var bubble_count = floori(lerp(2, 10, make_bubbles))
		for i in range(bubble_count):
			match normal:
				Vector2.UP:
					var x_variance = randf_range(-4, 4)
					var bubble_pos = Vector2(pos.x + x_variance, pos.y + 6)
					var bubble_vel = Vector2(x_variance, make_bubbles * randf_range(3.0, 7.0)) * 0.25
					var bubble = Statics.spawn_particle("Bubble", Room.Layers.GROUND, bubble_pos, [ bubble_vel, true ])
					bubble.home = home_box
