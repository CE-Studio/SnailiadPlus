extends Particle


func _spawn(_data:Array) -> void:
	super(_data)
	sprite.action = _data[0]
