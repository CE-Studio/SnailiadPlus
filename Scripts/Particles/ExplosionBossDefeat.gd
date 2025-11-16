extends Particle


var life_time:float = 1.8
var play_sound:bool = true

@onready var boom1:AudioStreamPlayer = $"Boom1"
@onready var boom2:AudioStreamPlayer = $"Boom2"
@onready var boom3:AudioStreamPlayer = $"Boom3"
@onready var small_boom1:AudioStreamPlayer = $"SmallBoom1"
@onready var small_boom2:AudioStreamPlayer = $"SmallBoom2"
@onready var small_boom3:AudioStreamPlayer = $"SmallBoom3"
@onready var small_boom4:AudioStreamPlayer = $"SmallBoom4"


func _spawn(_data:Array) -> void:
	super(_data)
	
	if _data.size() > 0 and _data[0] is bool:
		play_sound = _data[0]


func _physics_process(delta: float) -> void:
	if life_time <= 0.0:
		_call_batched(true)
		_call_batched(true)
		_call_batched(true)
		queue_free()
	else:
		_call_batched(false)
	life_time -= delta


func _call_batched(front:bool = false) -> void:
	_create_radial("ExplosionSmall", randf() * 160, front)
	_create_radial("ExplosionSmall", randf() * 80, front)
	_create_radial("ExplosionBig", randf() * 120, front)
	_create_radial("ExplosionBig", randf() * 60, front)
	if randf() * 20.0 > 17.0:
		_create_radial("ExplosionHuge", randf() * 130, front)
	if randf() * 10.0 > 1.0:
		_random_boom()
	if randf() * 10.0 > 7.0:
		_random_small_boom()


func _random_boom() -> void:
	if not play_sound:
		return
	match floori(randf() * 3):
		0: boom1.play()
		1: boom2.play()
		2: boom3.play()


func _random_small_boom() -> void:
	if not play_sound:
		return
	match floori(randf() * 4):
		0: small_boom1.play()
		1: small_boom2.play()
		2: small_boom3.play()
		3: small_boom4.play()


func _create_radial(part_name:String, radius_max:float, front:bool = false) -> void:
	var this_radius = randf() * radius_max
	var this_angle = randf() * TAU
	var this_pos = Vector2(cos(this_angle), sin(this_angle)) * this_radius
	this_pos += position
	var new_part = Statics.spawn_particle(
		part_name, Room.Layers.FG1 if front else Room.Layers.BG1, this_pos)
	new_part.sound.stop()
