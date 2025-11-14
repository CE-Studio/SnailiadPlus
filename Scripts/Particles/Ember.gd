extends Particle


var theta:float = randf() * TAU
var speed:float = randf_range(-112, -48)
var deceleration:float = randf_range(-40, -8)
var origin_x:float
var amplitude:float = randf_range(0.5, 1.25)#(8, 20)


func _spawn(_data:Array) -> void:
	origin_x = position.x
	super._spawn(_data)


func _process(delta: float) -> void:
	position = Vector2(
		position.x + sin(theta) * amplitude,
		position.y + speed * delta
	)
	theta += delta * amplitude
	speed -= deceleration * delta
	super._process(delta)
	if sprite.action == "__NONE__":
		queue_free()
