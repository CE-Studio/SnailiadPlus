extends Particle


const CENTER_MIN:float = 8.0
const CENTER:Vector2 = Statics.VECTOR_CENTER
const EASE_MOD:float = 3.5
const CENTER_WEIGHT_THRESHOLD:float = 0.25
const WEIGHT_FADE_MULT:float = 1.8

var speed:float = randf() * 100.0 + 10.0
var mod:float = 1.0
var direction:Vector2 = Vector2.LEFT
var border_mode:int = 0 # 0 - no special mode, 1 - inward from border, 2 - outward to border
var fade_in:float = randf_range(1.5, 5.0)
var sleep_theta:float = randf() * TAU

var mode:String = "intro"
var weight_intro:float = 1.0
var weight_stomp:float = 0.0
var weight_smash:float = 0.0
var weight_strafe:float = 0.0
var weight_sleep:float = 0.0


func _spawn(_data:Array) -> void:
	var option:int = ProjectSettings.get_setting("game/world/particles")
	if not (option == Statics.ParticleOptions.ENVIRONMENTS
	or option == Statics.ParticleOptions.FLASH
	or option == Statics.ParticleOptions.ALL):
		queue_free()
	else:
		if _data.size() > 0 and _data[0] is float:
			mod = _data[0]
		super(_data)
	sprite.modulate.a = 0.0


func _process(delta: float) -> void:
	weight_intro = move_toward(weight_intro, 1.0 if mode == "intro" else 0.0, delta * WEIGHT_FADE_MULT)
	weight_stomp = move_toward(weight_stomp, 1.0 if mode == "stomp" else 0.0, delta * WEIGHT_FADE_MULT)
	weight_smash = move_toward(weight_smash, 1.0 if mode == "smash" else 0.0, delta * WEIGHT_FADE_MULT)
	weight_strafe = move_toward(weight_strafe, 1.0 if mode == "strafe" else 0.0, delta * WEIGHT_FADE_MULT)
	weight_sleep = move_toward(weight_sleep, 1.0 if mode == "sleep" else 0.0, delta * WEIGHT_FADE_MULT)
	
	if weight_intro > 0.0:
		_to_center(delta, ease(weight_intro, EASE_MOD))
	if weight_stomp > 0.0:
		position += Vector2.UP * speed * mod * delta * ease(weight_stomp, EASE_MOD)
	if weight_smash > 0.0:
		position += (Statics.VECTOR_DIAG * Vector2(-1, 1)) * speed * mod * delta * ease(weight_smash, EASE_MOD)
	if weight_strafe > 0.0:
		_from_center(delta, ease(weight_strafe, EASE_MOD))
	if weight_sleep > 0.0:
		sprite.position = Vector2(sin(sleep_theta), sin(sleep_theta * 2.0)) * 2.0 * ease(weight_sleep, EASE_MOD)
	sleep_theta += delta
	
	if fade_in > 0:
		fade_in -= delta
		sprite.modulate.a = clampf(1.0 - fade_in, 0.0, 1.0)


func _to_center(delta:float, curve:float) -> void:
	position = position.move_toward(CENTER, speed * mod * delta * curve)
	if position.distance_to(CENTER) <= CENTER_MIN and curve > CENTER_WEIGHT_THRESHOLD:
		var new_pos:Vector2 = Vector2(
			randf_range(-1.0, 1.0),
			-1 if randf() < 0.5 else 1
		)
		if randf() < 0.5:
			new_pos = Vector2(new_pos.y, new_pos.x)
		new_pos *= Vector2(216, 136)
		position = CENTER + new_pos


func _from_center(delta:float, curve:float) -> void:
	if absf(position.x - CENTER.x) > 216 or absf(position.y - CENTER.y) > 136 and curve > CENTER_WEIGHT_THRESHOLD:
		position = CENTER
		var theta:float = randf() * TAU
		direction = Vector2(cos(theta), sin(theta))
	else:
		direction = position.direction_to(CENTER) * -1
	position += direction * speed * mod * delta * curve
