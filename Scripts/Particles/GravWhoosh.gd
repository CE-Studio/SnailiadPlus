# Copyright 2026 CE-Studio: AGPL-3.0-only
extends Particle


func _spawn(_data:Array) -> void:
	super(_data)
	
	var option:int = ProjectSettings.get_setting("game/world/particles")
	if not (option == Statics.ParticleOptions.ENTITIES_ALL
	or option == Statics.ParticleOptions.ALL):
		queue_free()
		return
	
	var direction:String = "v"
	if _data.size() > 0 and _data[0] is Statics.DirsSurface:
		if _data[0] == Statics.DirsSurface.LWALL or _data[0] == Statics.DirsSurface.RWALL:
			direction = "h"
		if _data[0] == Statics.DirsSurface.RWALL:
			sprite.flip_h = true
		if _data[0] == Statics.DirsSurface.FLOOR:
			sprite.flip_v = true
	sprite.play(direction + str(randi_range(0, 3)))
