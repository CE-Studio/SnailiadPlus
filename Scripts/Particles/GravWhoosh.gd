# Copyright 2026 CE-Studio: AGPL-3.0-only
extends Particle


func _spawn(_data:Array) -> void:
	super(_data)
	
	var option:int = ProjectSettings.get_setting("game/world/particles")
	if not (option == Statics.ParticleOptions.ENTITIES_ALL
	or option == Statics.ParticleOptions.ALL):
		queue_free()
		return
	
	var direction:String = "up"
	if _data.size() > 0 and _data[0] is Statics.DirsSurface:
		match _data[0]:
			Statics.DirsSurface.FLOOR: direction = "down"
			Statics.DirsSurface.LWALL: direction = "left"
			Statics.DirsSurface.RWALL: direction = "right"
			Statics.DirsSurface.CEILING: direction = "up"
	var type:int = randi_range(0, 3)
	var speed:String = "slow"
	match randi_range(0, 2):
		1: speed = "med"
		2: speed = "fast"
	sprite.action = "_".join([str(type), direction, speed])
